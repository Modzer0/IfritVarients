// Decompiled with JetBrains decompiler
// Type: NuclearOption.Jobs.DetectorManager
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Profiling;
using UnityEngine;

#nullable disable
namespace NuclearOption.Jobs;

public class DetectorManager
{
  private static readonly ProfilerMarker scheduleMarker = new ProfilerMarker("DetectorManager Schedule");
  private static readonly ProfilerMarker finishMarker = new ProfilerMarker("DetectorManager Finish");
  private static readonly ProfilerMarker requestRadarMarker = new ProfilerMarker("Request RadarCheck");
  private static readonly ProfilerMarker requestLoSMarker = new ProfilerMarker("Request LoSCheck");
  public const int BATCH_SIZE = 16 /*0x10*/;
  public const float EARTH_RADIUS = 6371000f;
  private readonly JobPerf jobPerf;
  private List<DetectionRequest> LoSRequests = new List<DetectionRequest>();
  private NativeArray<RaycastHit> results;
  private NativeArray<RaycastCommand> commands;
  private int countInJob;
  private JobHandle handle;

  public DetectorManager(JobPerf jobPerf) => this.jobPerf = jobPerf;

  public static void RequestRadarCheck(
    TargetDetector detector,
    Unit target,
    IRadarReturn radarReturn)
  {
    using (DetectorManager.requestRadarMarker.Auto())
    {
      SceneSingleton<JobManager>.ThrowIfInstanceNull();
      GlobalPosition a = detector.GetScanPoint().GlobalPosition();
      GlobalPosition b = target.GlobalPosition();
      Vector3 vector3 = (b - a) with { y = 0.0f };
      float num1 = FastMath.Distance(a, b);
      float magnitude = vector3.magnitude;
      float num2 = Mathf.Sqrt(1.2742E+07f * a.y);
      float num3 = Mathf.Sqrt(1.2742E+07f * b.y);
      if ((double) num2 + (double) num3 < (double) magnitude)
        return;
      float num4 = 0.0f;
      if ((double) magnitude < (double) num2 && (double) b.y < (double) a.y * (1.0 - (double) magnitude / (double) num2))
      {
        float num5 = (float) ((double) num1 * (double) target.radarAlt / ((double) a.y - (double) b.y));
        num4 += Mathf.Min(num1, 1000f) / num5;
      }
      float clutterFactor = num4 + (float) ((double) target.maxRadius * (double) target.maxRadius * 2.0 / ((double) target.radarAlt * (double) target.radarAlt));
      SceneSingleton<JobManager>.i.detector.LoSRequests.Add(new DetectionRequest(detector, target, radarReturn, num1, clutterFactor));
    }
  }

  public static void RequestLoSCheck(TargetDetector detector, Unit target)
  {
    using (DetectorManager.requestLoSMarker.Auto())
    {
      SceneSingleton<JobManager>.ThrowIfInstanceNull();
      if (!detector.InVisualRange(target))
        return;
      SceneSingleton<JobManager>.i.detector.LoSRequests.Add(new DetectionRequest(detector, target, (IRadarReturn) null, 0.0f, 0.0f));
    }
  }

  private void DisposeNative(bool delayed)
  {
    this.results.DelayDispose<RaycastHit>(delayed);
    this.commands.DelayDispose<RaycastCommand>(delayed);
  }

  public void DisposeAll()
  {
    this.LoSRequests.Clear();
    this.DisposeNative(true);
  }

  private void CheckCapacity(int max)
  {
    if (this.results.Length >= max)
      return;
    int length = Mathf.Max(this.results.Length * 2, max) + 10;
    this.DisposeNative(false);
    this.results = new NativeArray<RaycastHit>(length, Allocator.Persistent);
    this.commands = new NativeArray<RaycastCommand>(length, Allocator.Persistent);
  }

  public void Schedule()
  {
    using (DetectorManager.scheduleMarker.Auto())
    {
      try
      {
        for (int index = this.LoSRequests.Count - 1; index >= 0; --index)
        {
          if ((UnityEngine.Object) this.LoSRequests[index].target == (UnityEngine.Object) null)
            this.LoSRequests.RemoveAt(index);
        }
        this.countInJob = this.LoSRequests.Count;
        if (this.countInJob == 0)
          return;
        this.CheckCapacity(this.countInJob);
        for (int index = 0; index < this.countInJob; ++index)
          this.commands[index] = this.LoSRequests[index].GetRaycastCommand();
        this.handle = RaycastCommand.ScheduleBatch(this.commands.GetSubArray(0, this.countInJob), this.results.GetSubArray(0, this.countInJob), 16 /*0x10*/, 1);
      }
      catch (Exception ex)
      {
        Debug.LogException(ex);
        this.LoSRequests.Clear();
      }
    }
  }

  public void FinishJob()
  {
    if (this.countInJob == 0)
      return;
    using (DetectorManager.finishMarker.Auto())
    {
      this.handle.Complete();
      try
      {
        for (int index = 0; index < this.countInJob; ++index)
          this.LoSRequests[index].ProcessResult(this.results[index]);
      }
      catch (Exception ex)
      {
        Debug.LogException(ex);
      }
      this.LoSRequests.RemoveRange(0, this.countInJob);
      this.countInJob = 0;
    }
  }
}

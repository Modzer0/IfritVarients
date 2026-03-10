// Decompiled with JetBrains decompiler
// Type: NuclearOption.Jobs.AeroJobSettings
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Jobs;

#nullable disable
namespace NuclearOption.Jobs;

public class AeroJobSettings
{
  private static readonly ProfilerMarker scheduleMarker = new ProfilerMarker("AeroJob Schedule");
  private static readonly ProfilerMarker setArgsMarker = new ProfilerMarker("AeroJob SetArgs");
  private static readonly ProfilerMarker setArgsFullMarker = new ProfilerMarker("AeroJob SetArgs_Full");
  private static readonly ProfilerMarker finishAeroMarker = new ProfilerMarker("AeroJob Finish");
  private readonly List<int> removeTransformIndexCache = new List<int>();
  public const int BATCH_SIZE = 64 /*0x40*/;
  private readonly JobPerf jobPerf;
  private readonly ControlJobSettings controlJob;
  public NativeArray<float> liftCharts;
  public NativeArray<float> dragCharts;
  public AeroPart[] partsInJob;
  public TransformAccessArray transformAccess;
  public NativeArray<Ptr<AeroPartFields>> fields;
  public NativeArray<JobTransformValues> transformValues;
  public NativeArray<PtrRefCounter<IndexLink>> transformLinks;
  public int countInJob;
  private JobHandle handleAccess;
  private JobHandle handleMath;

  public AeroJobSettings(JobPerf jobPerf, ControlJobSettings controlJob)
  {
    this.jobPerf = jobPerf;
    this.controlJob = controlJob;
  }

  public void CheckCapacity(int neededLength)
  {
    AeroPart[] partsInJob = this.partsInJob;
    int length = partsInJob != null ? partsInJob.Length : 0;
    if (length >= neededLength)
      return;
    int num = JobManager.IncreaseCapacity(length, neededLength, 128 /*0x80*/);
    this.fields.Resize<Ptr<AeroPartFields>>(num, true);
    this.transformValues.Resize<JobTransformValues>(num * 2, false);
    this.transformLinks.Resize<PtrRefCounter<IndexLink>>(num * 2, true, NativeArrayOptions.ClearMemory);
    this.transformAccess.Resize(num * 2);
    Array.Resize<AeroPart>(ref this.partsInJob, num);
  }

  public void DisposeAll()
  {
    this.partsInJob.SafeClear();
    this.fields.DelayDispose<Ptr<AeroPartFields>>();
    this.transformValues.DelayDispose<JobTransformValues>();
    this.transformLinks.ClearAndDispose<IndexLink>(this.transformAccess.isCreated ? this.transformAccess.length : 0);
    this.transformAccess.DelayDispose();
    this.countInJob = 0;
  }

  public void SetArgs_Added(
    JobUnitList<JobPart<AeroPart, AeroPartFields>> list)
  {
    using (AeroJobSettings.setArgsFullMarker.Auto())
    {
      JobPart<AeroPart, AeroPartFields> add;
      int addIndex;
      while (list.ProcessNextAdded(out add, out addIndex))
      {
        Transform liftTransform;
        Transform otherTransform;
        int num = add.Part.GetJobTransforms(out liftTransform, out otherTransform) ? 1 : 0;
        ref AeroPartFields local = ref add.Field.Ref();
        IndexLink.AddTransform(this.transformAccess, this.transformLinks, add.links, ref local.liftTransformIndex, liftTransform);
        if (num != 0)
          IndexLink.AddTransform(this.transformAccess, this.transformLinks, add.links, ref local.otherTransformIndex, otherTransform);
        this.partsInJob[addIndex] = add.Part;
        this.fields[addIndex] = add.Field;
        ++this.countInJob;
      }
    }
  }

  public void SetArgs_Removed(
    JobUnitList<JobPart<AeroPart, AeroPartFields>> list)
  {
    using (AeroJobSettings.setArgsFullMarker.Auto())
    {
      this.removeTransformIndexCache.Clear();
      int removeIndex;
      JobPart<AeroPart, AeroPartFields> removedItem;
      while (list.ProcessNextRemoved(out removeIndex, out removedItem))
      {
        IndexLink.QueueToRemove(this.removeTransformIndexCache, this.transformLinks, removedItem.links);
        int index = this.countInJob - 1;
        if (index != removeIndex)
        {
          this.partsInJob[removeIndex] = this.partsInJob[index];
          this.fields[removeIndex] = this.fields[index];
        }
        --this.countInJob;
      }
      IndexLink.RemoveLinks(this.transformAccess, this.transformLinks, this.removeTransformIndexCache);
    }
  }

  public void SetArgs_Update()
  {
    using (AeroJobSettings.setArgsMarker.Auto())
    {
      for (int index = 0; index < this.countInJob; ++index)
        this.partsInJob[index].UpdateJobFields();
    }
  }

  public void Schedule(
    JobUnitList<JobPart<AeroPart, AeroPartFields>> aeroParts,
    Ptr<JobSharedFields> shared)
  {
    using (AeroJobSettings.scheduleMarker.Auto())
    {
      if (aeroParts.PendingChangesRemove)
      {
        JobPerf.GetTimestamp();
        this.SetArgs_Removed(aeroParts);
      }
      if (aeroParts.PendingChangesAdd)
      {
        JobPerf.GetTimestamp();
        this.CheckCapacity(aeroParts.CountAfterPending);
        JobPerf.GetTimestamp();
        this.SetArgs_Added(aeroParts);
      }
      if (this.countInJob == 0)
        return;
      this.handleAccess = new ReadTransformJob(this.transformValues, shared.AeroAccessPtr()).ScheduleReadOnly<ReadTransformJob>(this.transformAccess, 64 /*0x40*/, this.controlJob.handleAccess);
      JobPerf.GetTimestamp();
      this.SetArgs_Update();
      AeroJob_Math jobData = new AeroJob_Math()
      {
        liftCharts = this.liftCharts,
        dragCharts = this.dragCharts,
        airDensityChart = LevelInfo.airDensityChart,
        windVelocity = NetworkSceneSingleton<LevelInfo>.i.GetWind(),
        windTurbulence = NetworkSceneSingleton<LevelInfo>.i.GetTurbulence(),
        fields = this.fields,
        transformValues = this.transformValues,
        shared = shared
      };
      this.handleMath = jobData.ScheduleByRef<AeroJob_Math>(this.countInJob, 64 /*0x40*/, this.handleAccess);
      JobHandle.ScheduleBatchedJobs();
    }
  }

  public void FinishJob()
  {
    if (this.countInJob == 0)
      return;
    JobPerf.GetTimestamp();
    using (AeroJobSettings.finishAeroMarker.Auto())
    {
      this.handleAccess.Complete();
      this.handleMath.Complete();
      try
      {
        for (int index = 0; index < this.countInJob; ++index)
          this.partsInJob[index].ApplyJobFields();
      }
      catch (Exception ex)
      {
        Debug.LogException(ex);
      }
    }
  }
}

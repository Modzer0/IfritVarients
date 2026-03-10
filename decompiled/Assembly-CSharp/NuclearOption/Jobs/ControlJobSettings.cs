// Decompiled with JetBrains decompiler
// Type: NuclearOption.Jobs.ControlJobSettings
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

public class ControlJobSettings
{
  private static readonly ProfilerMarker scheduleMarker = new ProfilerMarker("ControlSurfaceJob Schedule");
  private static readonly ProfilerMarker updateArgsMarker = new ProfilerMarker("ControlSurfaceJob UpdateArgs");
  private static readonly ProfilerMarker updateArgsFullMarker = new ProfilerMarker("ControlSurfaceJob UpdateArgs_Full");
  private static readonly ProfilerMarker finishAeroMarker = new ProfilerMarker("ControlSurfaceJob Finish");
  private readonly List<int> removeTransformIndexCache = new List<int>();
  public const int BATCH_SIZE = 32 /*0x20*/;
  private readonly JobPerf jobPerf;
  public ControlSurface[] partsInJob;
  public TransformAccessArray transformAccess;
  public NativeArray<Ptr<ControlSurfaceFields>> fields;
  public NativeArray<Quaternion> rotations;
  public NativeArray<PtrRefCounter<IndexLink>> transformLinks;
  public int countInJob;
  private JobHandle handleMath;
  public JobHandle handleAccess;

  public ControlJobSettings(JobPerf jobPerf) => this.jobPerf = jobPerf;

  public void CheckCapacity(int neededLength)
  {
    ControlSurface[] partsInJob = this.partsInJob;
    int length = partsInJob != null ? partsInJob.Length : 0;
    if (length >= neededLength)
      return;
    int num = JobManager.IncreaseCapacity(length, neededLength, 64 /*0x40*/);
    this.fields.Resize<Ptr<ControlSurfaceFields>>(num, true);
    this.rotations.Resize<Quaternion>(num * 3, false);
    this.transformLinks.Resize<PtrRefCounter<IndexLink>>(num * 3, true, NativeArrayOptions.ClearMemory);
    this.transformAccess.Resize(num * 3);
    Array.Resize<ControlSurface>(ref this.partsInJob, num);
  }

  public void DisposeAll()
  {
    this.partsInJob.SafeClear();
    this.fields.DelayDispose<Ptr<ControlSurfaceFields>>();
    this.rotations.DelayDispose<Quaternion>();
    this.transformLinks.ClearAndDispose<IndexLink>(this.transformAccess.isCreated ? this.transformAccess.length : 0);
    this.transformAccess.DelayDispose();
    this.countInJob = 0;
  }

  public void SetArgs_Add(
    JobUnitList<JobPart<ControlSurface, ControlSurfaceFields>> list)
  {
    using (ControlJobSettings.updateArgsFullMarker.Auto())
    {
      JobPart<ControlSurface, ControlSurfaceFields> add;
      int addIndex;
      while (list.ProcessNextAdded(out add, out addIndex))
      {
        Transform liftTransform;
        Transform upperTransform;
        Transform lowerTransform;
        int num = add.Part.GetJobTransforms(out liftTransform, out upperTransform, out lowerTransform) ? 1 : 0;
        ref ControlSurfaceFields local = ref add.Field.Ref();
        IndexLink.AddTransform(this.transformAccess, this.transformLinks, add.links, ref local.visibleTransformLink, liftTransform);
        if (num != 0)
        {
          IndexLink.AddTransform(this.transformAccess, this.transformLinks, add.links, ref local.upperTransformLink, upperTransform);
          IndexLink.AddTransform(this.transformAccess, this.transformLinks, add.links, ref local.lowerTransformLink, lowerTransform);
        }
        this.partsInJob[addIndex] = add.Part;
        this.fields[addIndex] = add.Field;
        ++this.countInJob;
      }
    }
  }

  public void SetArgs_Remove(
    JobUnitList<JobPart<ControlSurface, ControlSurfaceFields>> list)
  {
    using (ControlJobSettings.updateArgsFullMarker.Auto())
    {
      this.removeTransformIndexCache.Clear();
      int removeIndex;
      JobPart<ControlSurface, ControlSurfaceFields> removedItem;
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
    using (ControlJobSettings.updateArgsMarker.Auto())
    {
      for (int index = 0; index < this.countInJob; ++index)
        this.partsInJob[index].UpdateJobFields();
    }
  }

  public void Schedule(
    JobUnitList<JobPart<ControlSurface, ControlSurfaceFields>> controlSurfaces,
    Ptr<JobSharedFields> shared)
  {
    using (ControlJobSettings.scheduleMarker.Auto())
    {
      if (controlSurfaces.PendingChangesRemove)
      {
        JobPerf.GetTimestamp();
        this.SetArgs_Remove(controlSurfaces);
      }
      if (controlSurfaces.PendingChangesAdd)
      {
        JobPerf.GetTimestamp();
        this.CheckCapacity(controlSurfaces.CountAfterPending);
        JobPerf.GetTimestamp();
        this.SetArgs_Add(controlSurfaces);
      }
      if (this.countInJob == 0)
        return;
      JobPerf.GetTimestamp();
      this.SetArgs_Update();
      ControlSurfaceJob_Math jobData = new ControlSurfaceJob_Math()
      {
        fields = this.fields,
        rotations = this.rotations,
        shared = shared
      };
      this.handleMath = jobData.ScheduleByRef<ControlSurfaceJob_Math>(this.countInJob, 32 /*0x20*/);
      this.handleAccess = new SetLocalRotationJob(this.rotations, shared.ControlAccessPtr()).Schedule<SetLocalRotationJob>(this.transformAccess, this.handleMath);
    }
  }

  public void FinishJob()
  {
    if (this.countInJob == 0)
      return;
    JobPerf.GetTimestamp();
    using (ControlJobSettings.finishAeroMarker.Auto())
    {
      this.handleMath.Complete();
      this.handleAccess.Complete();
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

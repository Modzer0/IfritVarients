// Decompiled with JetBrains decompiler
// Type: NuclearOption.Jobs.WaterJobsSettings
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using Unity.Collections;
using Unity.Jobs;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Jobs;

#nullable disable
namespace NuclearOption.Jobs;

public class WaterJobsSettings
{
  private static readonly ProfilerMarker scheduleMarker = new ProfilerMarker("WaterJob Schedule");
  private static readonly ProfilerMarker finishMarker = new ProfilerMarker("WaterJob Finish");
  private static readonly ProfilerMarker setArgsMarker = new ProfilerMarker("WaterJob SetArgs");
  private static readonly ProfilerMarker setArgsFullMarker = new ProfilerMarker("WaterJob SetArgs_Full");
  public const int BATCH_SIZE = 32 /*0x20*/;
  private readonly JobPerf jobPerf;
  private ShipPart[] partsInJob;
  public TransformAccessArray transformAccess;
  public NativeArray<Ptr<ShipPartFields>> fields;
  public NativeArray<JobTransformValues> transformValues;
  private int countInJob;
  private JobHandle handleAccess;
  private JobHandle handleMath;

  public WaterJobsSettings(JobPerf jobPerf) => this.jobPerf = jobPerf;

  private void CheckCapacity(int neededLength)
  {
    ShipPart[] partsInJob = this.partsInJob;
    int length = partsInJob != null ? partsInJob.Length : 0;
    if (length >= neededLength)
      return;
    int num = JobManager.IncreaseCapacity(length, neededLength, 64 /*0x40*/);
    Array.Resize<ShipPart>(ref this.partsInJob, num);
    this.transformAccess.Resize(num);
    this.fields.Resize<Ptr<ShipPartFields>>(num, true);
    this.transformValues.Resize<JobTransformValues>(num, false);
  }

  public void DisposeAll()
  {
    this.partsInJob.SafeClear();
    this.transformAccess.DelayDispose();
    this.fields.DelayDispose<Ptr<ShipPartFields>>();
    this.transformValues.DelayDispose<JobTransformValues>();
    this.countInJob = 0;
  }

  public void SetArgs_Added(
    JobUnitList<JobPart<ShipPart, ShipPartFields>> list)
  {
    using (WaterJobsSettings.setArgsFullMarker.Auto())
    {
      JobPart<ShipPart, ShipPartFields> add;
      int addIndex;
      while (list.ProcessNextAdded(out add, out addIndex))
      {
        this.transformAccess.Add(add.Part.GetJobTransforms());
        this.partsInJob[addIndex] = add.Part;
        this.fields[addIndex] = add.Field;
        ++this.countInJob;
      }
    }
  }

  public void SetArgs_Removed(
    JobUnitList<JobPart<ShipPart, ShipPartFields>> list)
  {
    using (WaterJobsSettings.setArgsFullMarker.Auto())
    {
      int removeIndex;
      while (list.ProcessNextRemoved(out removeIndex, out JobPart<ShipPart, ShipPartFields> _))
      {
        this.transformAccess.RemoveAtSwapBack(removeIndex);
        int index = this.countInJob - 1;
        if (index != removeIndex)
        {
          this.partsInJob[removeIndex] = this.partsInJob[index];
          this.fields[removeIndex] = this.fields[index];
        }
        --this.countInJob;
      }
    }
  }

  public void SetArgs_Update()
  {
    using (WaterJobsSettings.setArgsMarker.Auto())
    {
      for (int index = 0; index < this.countInJob; ++index)
        this.partsInJob[index].UpdateJobFields(ref this.transformValues.GetReadOnlyRef(index));
    }
  }

  public void Schedule_1(
    JobUnitList<JobPart<ShipPart, ShipPartFields>> shipParts,
    Ptr<JobSharedFields> shared)
  {
    using (WaterJobsSettings.scheduleMarker.Auto())
    {
      if (shipParts.PendingChangesRemove)
      {
        JobPerf.GetTimestamp();
        this.SetArgs_Removed(shipParts);
      }
      if (shipParts.PendingChangesAdd)
      {
        JobPerf.GetTimestamp();
        this.CheckCapacity(shipParts.CountAfterPending);
        JobPerf.GetTimestamp();
        this.SetArgs_Added(shipParts);
      }
      if (this.countInJob == 0)
        return;
      this.handleAccess = new ReadTransformJob(this.transformValues, shared.WaterAccessPtr()).ScheduleReadOnly<ReadTransformJob>(this.transformAccess, 32 /*0x20*/);
      JobHandle.ScheduleBatchedJobs();
    }
  }

  public void Schedule_2(Ptr<JobSharedFields> shared)
  {
    if (this.countInJob == 0)
      return;
    this.handleAccess.Complete();
    JobPerf.GetTimestamp();
    this.SetArgs_Update();
    JobPerf.GetTimestamp();
    WaterJob_Math jobData = new WaterJob_Math()
    {
      fields = this.fields,
      transformValues = this.transformValues,
      shared = shared
    };
    this.handleMath = jobData.ScheduleByRef<WaterJob_Math>(this.countInJob, 32 /*0x20*/);
    JobHandle.ScheduleBatchedJobs();
  }

  public void FinishJob()
  {
    if (this.countInJob == 0)
      return;
    using (WaterJobsSettings.finishMarker.Auto())
    {
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

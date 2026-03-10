// Decompiled with JetBrains decompiler
// Type: NuclearOption.Jobs.GroundVehicleJobSettings
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Jobs;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Jobs;

#nullable disable
namespace NuclearOption.Jobs;

public class GroundVehicleJobSettings
{
  private static readonly ProfilerMarker schedule1Marker = new ProfilerMarker("GroundVehicle Schedule1");
  private static readonly ProfilerMarker scheduleQueue1Marker = new ProfilerMarker("GroundVehicle Schedule Queue 1");
  private static readonly ProfilerMarker schedule2Marker = new ProfilerMarker("GroundVehicle Schedule2");
  private static readonly ProfilerMarker schedule2Raycasts = new ProfilerMarker("GroundVehicle ProcessRaycasts");
  private static readonly ProfilerMarker scheduleQueue2Marker = new ProfilerMarker("GroundVehicle Schedule Queue 1");
  private static readonly ProfilerMarker setArgsMarker = new ProfilerMarker("GroundVehicle SetArgs");
  private static readonly ProfilerMarker setArgsFieldsMarker = new ProfilerMarker("GroundVehicle SetArgs_fields");
  private static readonly ProfilerMarker setArgsPathfinderMarker = new ProfilerMarker("GroundVehicle SetArgs_pathfinder");
  private static readonly ProfilerMarker setArgsObstacleMarker = new ProfilerMarker("GroundVehicle SetArgs_obstacle");
  private static readonly ProfilerMarker setArgsFullMarker = new ProfilerMarker("GroundVehicle SetArgs_Full");
  private static readonly ProfilerMarker finishMarker = new ProfilerMarker("GroundVehicle Finish");
  public const float TICK_FREQUENCY = 60f;
  public const int INPUTS_UPDATE_FREQUENCY_TICKS = 12;
  public const int SAMPLE_GROUND_FREQUENCY_TICKS = 6;
  public const float INPUTS_UPDATE_FREQUENCY = 0.2f;
  public const float SAMPLE_GROUND_FREQUENCY = 0.1f;
  public const int BATCH_SIZE = 32 /*0x20*/;
  private readonly JobPerf jobPerf;
  public GroundVehicle[] partsInJob;
  public TransformAccessArray transformAccess;
  public NativeArray<Ptr<GroundVehicleFields>> fields;
  public NativeArray<JobTransformValues> transformValues;
  public NativeArray<RaycastHit> rayResults;
  public NativeArray<RaycastCommand> rayCommands;
  public int countInJob;
  private JobHandle handleRaycast;
  private JobHandle handle_math2;
  private JobHandle handleAccess;
  private JobHandle handleMath1;

  public GroundVehicleJobSettings(JobPerf jobPerf) => this.jobPerf = jobPerf;

  public void CheckCapacity(int neededLength)
  {
    GroundVehicle[] partsInJob = this.partsInJob;
    int length = partsInJob != null ? partsInJob.Length : 0;
    if (length >= neededLength)
      return;
    int num = JobManager.IncreaseCapacity(length, neededLength, 64 /*0x40*/);
    Array.Resize<GroundVehicle>(ref this.partsInJob, num);
    this.transformAccess.Resize(num);
    this.fields.Resize<Ptr<GroundVehicleFields>>(num, true);
    this.transformValues.Resize<JobTransformValues>(num, false);
    int newLength = (num + 6 - 1) / 6;
    this.rayResults.Resize<RaycastHit>(newLength, false);
    this.rayCommands.Resize<RaycastCommand>(newLength, false);
  }

  public void DisposeAll()
  {
    this.partsInJob.SafeClear();
    this.transformAccess.DelayDispose();
    this.fields.DelayDispose<Ptr<GroundVehicleFields>>();
    this.transformValues.DelayDispose<JobTransformValues>();
    this.rayResults.DelayDispose<RaycastHit>();
    this.rayCommands.DelayDispose<RaycastCommand>();
    this.countInJob = 0;
  }

  public void SetArgs_ProcessAdded(
    JobUnitList<JobPart<GroundVehicle, GroundVehicleFields>> list)
  {
    using (GroundVehicleJobSettings.setArgsFullMarker.Auto())
    {
      JobPart<GroundVehicle, GroundVehicleFields> add;
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

  public void SetArgs_ProcessRemoved(
    JobUnitList<JobPart<GroundVehicle, GroundVehicleFields>> list)
  {
    using (GroundVehicleJobSettings.setArgsFullMarker.Auto())
    {
      int removeIndex;
      while (list.ProcessNextRemoved(out removeIndex, out JobPart<GroundVehicle, GroundVehicleFields> _))
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

  public void SetArgs_Update(Ptr<JobSharedFields> shared)
  {
    using (GroundVehicleJobSettings.setArgsMarker.Auto())
    {
      using (GroundVehicleJobSettings.setArgsFieldsMarker.Auto())
      {
        for (int index = 0; index < this.countInJob; ++index)
          this.partsInJob[index].UpdateJobFields();
      }
      int tickOffset = shared.Ref().tickOffset;
      using (GroundVehicleJobSettings.setArgsPathfinderMarker.Auto())
      {
        for (int i = 0; i < this.countInJob; ++i)
        {
          if (GroundVehicleJobSettings.ShouldRunInputs(i, tickOffset))
            this.partsInJob[i].UpdateJobFields_Pathfinder();
        }
      }
      using (GroundVehicleJobSettings.setArgsObstacleMarker.Auto())
      {
        for (int i = 0; i < this.countInJob; ++i)
        {
          if (GroundVehicleJobSettings.ShouldRunInputs(i, tickOffset))
            this.partsInJob[i].UpdateJobFields_Obstacles();
        }
      }
    }
  }

  public void Schedule_1(
    JobUnitList<JobPart<GroundVehicle, GroundVehicleFields>> vehicles,
    Ptr<JobSharedFields> shared)
  {
    using (GroundVehicleJobSettings.schedule1Marker.Auto())
    {
      Ptr<GroundVehicleFields>.JobRunningSafety = true;
      if (vehicles.PendingChangesRemove)
      {
        JobPerf.GetTimestamp();
        this.SetArgs_ProcessRemoved(vehicles);
      }
      if (vehicles.PendingChangesAdd)
      {
        JobPerf.GetTimestamp();
        this.CheckCapacity(vehicles.CountAfterPending);
        JobPerf.GetTimestamp();
        this.SetArgs_ProcessAdded(vehicles);
      }
      if (this.countInJob == 0)
        return;
      JobPerf.GetTimestamp();
      this.SetArgs_Update(shared);
      this.Schedule_1_inner(shared);
    }
  }

  private void Schedule_1_inner(Ptr<JobSharedFields> shared)
  {
    using (GroundVehicleJobSettings.scheduleQueue1Marker.Auto())
    {
      this.handleAccess = new ReadTransformJob(this.transformValues, shared.VehicleAccessPtr()).ScheduleReadOnly<ReadTransformJob>(this.transformAccess, 32 /*0x20*/);
      GroundVehicleJob_Math1 jobData = new GroundVehicleJob_Math1()
      {
        fields = this.fields,
        transformValues = this.transformValues,
        shared = shared,
        rayCommands = this.rayCommands
      };
      this.handleMath1 = jobData.ScheduleByRef<GroundVehicleJob_Math1>(this.countInJob, 32 /*0x20*/, this.handleAccess);
      int length = this.SampleGroundRayCount(shared.Ref().tickOffset);
      this.handleRaycast = RaycastCommand.ScheduleBatch(this.rayCommands.GetSubArray(0, length), this.rayResults.GetSubArray(0, length), 32 /*0x20*/, 1, this.handleMath1);
      JobHandle.ScheduleBatchedJobs();
    }
  }

  public void Schedule_2(Ptr<JobSharedFields> shared)
  {
    if (this.countInJob == 0)
      return;
    ProfilerMarker profilerMarker = GroundVehicleJobSettings.schedule2Marker;
    using (profilerMarker.Auto())
    {
      JobPerf.GetTimestamp();
      this.handleAccess.Complete();
      this.handleMath1.Complete();
      this.handleRaycast.Complete();
      JobPerf.GetTimestamp();
      this.ProcessRayCasts(shared.Ref().tickOffset);
      profilerMarker = GroundVehicleJobSettings.scheduleQueue2Marker;
      using (profilerMarker.Auto())
      {
        GroundVehicleJob_Math2 jobData = new GroundVehicleJob_Math2()
        {
          fields = this.fields,
          transformValues = this.transformValues,
          shared = shared,
          rayResults = this.rayResults
        };
        this.handle_math2 = jobData.ScheduleByRef<GroundVehicleJob_Math2>(this.countInJob, 32 /*0x20*/, this.handleRaycast);
      }
      JobHandle.ScheduleBatchedJobs();
    }
  }

  private void ProcessRayCasts(int tickOffset)
  {
    using (GroundVehicleJobSettings.schedule2Raycasts.Auto())
    {
      for (int index = 0; index < this.countInJob; ++index)
      {
        ref GroundVehicleFields local1 = ref this.fields[index].Ref();
        if (local1.monoBehaviourEnabled)
        {
          GroundVehicle groundVehicle = this.partsInJob[index];
          ref SampleGroundResult local2 = ref local1.sampleGroundResult;
          ref JobTransformValues.ReadOnly local3 = ref this.transformValues.GetReadOnlyRef(index);
          if (GroundVehicleJobSettings.ShouldRunSampleGround(index, tickOffset))
          {
            RaycastHit rayResult = this.rayResults[GroundVehicleJobSettings.SampleGroundIndex(index)];
            local2.didHit = rayResult.colliderInstanceID != 0;
            if (local2.didHit)
            {
              local2.hitNormal = rayResult.normal;
              local2.hitPoint = rayResult.point;
              if ((UnityEngine.Object) rayResult.collider.sharedMaterial == (UnityEngine.Object) GameAssets.i.terrainMaterial)
              {
                if (GameManager.ShowEffects)
                  groundVehicle.ContactDustParticles(rayResult.point);
                local2.onPaved = false;
              }
              else
                local2.onPaved = true;
            }
            Rigidbody attachedRigidbody = local2.didHit ? rayResult.collider.attachedRigidbody : (Rigidbody) null;
            if ((UnityEngine.Object) attachedRigidbody != (UnityEngine.Object) null)
            {
              local2.hasHitRB = true;
              groundVehicle.hitRB = attachedRigidbody;
            }
            else
              local2.hasHitRB = false;
          }
          if (local2.hasHitRB)
          {
            Rigidbody hitRb = groundVehicle.hitRB;
            if ((UnityEngine.Object) hitRb == (UnityEngine.Object) null)
            {
              local2.hasHitRB = false;
            }
            else
            {
              Vector3 angularVelocity = hitRb.angularVelocity;
              groundVehicle.rb.angularVelocity = angularVelocity;
              local1.angularVelocity = angularVelocity;
              local2.hitPointVelocity = hitRb.GetPointVelocity(local3.Position - local3.Up() * 100f);
            }
          }
        }
      }
    }
  }

  public void FinishJob()
  {
    if (this.countInJob == 0)
      return;
    JobPerf.GetTimestamp();
    using (GroundVehicleJobSettings.finishMarker.Auto())
    {
      this.handle_math2.Complete();
      Ptr<GroundVehicleFields>.JobRunningSafety = false;
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

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool ShouldRunInputs(int i, int tickOffset) => (i + tickOffset) % 12 == 0;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool ShouldRunSampleGround(int i, int tickOffset) => (i + tickOffset) % 6 == 0;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int SampleGroundIndex(int i) => i / 6;

  public int SampleGroundRayCount(int tickOffset)
  {
    int num = (6 - tickOffset % 6) % 6;
    return num < this.countInJob ? (this.countInJob - 1 - num) / 6 + 1 : 0;
  }
}

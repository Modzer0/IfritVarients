// Decompiled with JetBrains decompiler
// Type: NuclearOption.Jobs.JobManager
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using Unity.Collections;
using Unity.Profiling;
using UnityEngine;

#nullable disable
namespace NuclearOption.Jobs;

public class JobManager : SceneSingleton<JobManager>
{
  private static readonly ProfilerMarker fixedUpdateEarlyMarker = new ProfilerMarker("JobManager FixedUpdateEarly");
  private static readonly ProfilerMarker fixedUpdateLateMarker = new ProfilerMarker("JobManager FixedUpdateLate");
  private static readonly ProfilerMarker scheduleAllMarker = new ProfilerMarker("JobManager Schedule");
  private static readonly ProfilerMarker scheduleAeroMarker = new ProfilerMarker("AeroJob Schedule");
  private static readonly ProfilerMarker pilotAeroInputsMarker = new ProfilerMarker("JobManager Pilot Inputs");
  private readonly JobUnitList<JobPart<AeroPart, AeroPartFields>> aeroParts = new JobUnitList<JobPart<AeroPart, AeroPartFields>>();
  private readonly JobUnitList<JobPart<ControlSurface, ControlSurfaceFields>> controlSurfaces = new JobUnitList<JobPart<ControlSurface, ControlSurfaceFields>>();
  private readonly List<Ship> ships = new List<Ship>();
  private readonly JobUnitList<JobPart<ShipPart, ShipPartFields>> shipParts = new JobUnitList<JobPart<ShipPart, ShipPartFields>>();
  private readonly JobUnitList<JobPart<GroundVehicle, GroundVehicleFields>> vehicles = new JobUnitList<JobPart<GroundVehicle, GroundVehicleFields>>();
  private readonly List<Pilot> pilots = new List<Pilot>();
  private readonly AeroJobSettings aeroJob;
  private readonly ControlJobSettings controlJob;
  private readonly WaterJobsSettings waterJobs;
  private readonly GroundVehicleJobSettings vehicleJob;
  public readonly DetectorManager detector;
  private readonly JobPerf jobPerf;
  private PtrAllocation<JobSharedFields> shared;

  private JobManager()
  {
    this.jobPerf = new JobPerf();
    this.controlJob = new ControlJobSettings(this.jobPerf);
    this.aeroJob = new AeroJobSettings(this.jobPerf, this.controlJob);
    this.waterJobs = new WaterJobsSettings(this.jobPerf);
    this.vehicleJob = new GroundVehicleJobSettings(this.jobPerf);
    this.detector = new DetectorManager(this.jobPerf);
  }

  public static void Add(JobPart<AeroPart, AeroPartFields> aeroPart)
  {
    SceneSingleton<JobManager>.ThrowIfInstanceNull();
    SceneSingleton<JobManager>.i.aeroParts.Add(aeroPart);
  }

  public static void Add(
    JobPart<ControlSurface, ControlSurfaceFields> controlSurface)
  {
    SceneSingleton<JobManager>.ThrowIfInstanceNull();
    SceneSingleton<JobManager>.i.controlSurfaces.Add(controlSurface);
  }

  public static void Add(Ship ship)
  {
    SceneSingleton<JobManager>.ThrowIfInstanceNull();
    SceneSingleton<JobManager>.i.ships.Add(ship);
    foreach (ShipPart part in ship.parts)
      SceneSingleton<JobManager>.i.shipParts.Add(part.SetupJob());
  }

  public static void Add(JobPart<ShipPart, ShipPartFields> shipPart)
  {
    SceneSingleton<JobManager>.ThrowIfInstanceNull();
    SceneSingleton<JobManager>.i.shipParts.Add(shipPart);
  }

  public static void Add(
    JobPart<GroundVehicle, GroundVehicleFields> vehicle)
  {
    SceneSingleton<JobManager>.ThrowIfInstanceNull();
    SceneSingleton<JobManager>.i.vehicles.Add(vehicle);
  }

  public static void Add(Pilot pilot)
  {
    SceneSingleton<JobManager>.ThrowIfInstanceNull();
    SceneSingleton<JobManager>.i.pilots.Add(pilot);
  }

  public static void Remove(ref JobPart<AeroPart, AeroPartFields> part)
  {
    if (part == null)
      return;
    if ((UnityEngine.Object) SceneSingleton<JobManager>.i != (UnityEngine.Object) null)
      SceneSingleton<JobManager>.i.aeroParts.Remove(part);
    part = (JobPart<AeroPart, AeroPartFields>) null;
  }

  public static void Remove(
    ref JobPart<ControlSurface, ControlSurfaceFields> part)
  {
    if (part == null)
      return;
    if ((UnityEngine.Object) SceneSingleton<JobManager>.i != (UnityEngine.Object) null)
      SceneSingleton<JobManager>.i.controlSurfaces.Remove(part);
    part = (JobPart<ControlSurface, ControlSurfaceFields>) null;
  }

  public static void Remove(Ship ship)
  {
    if (!((UnityEngine.Object) SceneSingleton<JobManager>.i != (UnityEngine.Object) null))
      return;
    SceneSingleton<JobManager>.i.ships.Remove(ship);
  }

  public static void Remove(ref JobPart<ShipPart, ShipPartFields> part)
  {
    if (part == null)
      return;
    if ((UnityEngine.Object) SceneSingleton<JobManager>.i != (UnityEngine.Object) null)
      SceneSingleton<JobManager>.i.shipParts.Remove(part);
    part = (JobPart<ShipPart, ShipPartFields>) null;
  }

  public static void Remove(
    ref JobPart<GroundVehicle, GroundVehicleFields> part)
  {
    if (part == null)
      return;
    if ((UnityEngine.Object) SceneSingleton<JobManager>.i != (UnityEngine.Object) null)
      SceneSingleton<JobManager>.i.vehicles.Remove(part);
    part = (JobPart<GroundVehicle, GroundVehicleFields>) null;
  }

  public static void Remove(Pilot pilot)
  {
    if (!((UnityEngine.Object) SceneSingleton<JobManager>.i != (UnityEngine.Object) null))
      return;
    SceneSingleton<JobManager>.i.pilots.Remove(pilot);
  }

  public static int IncreaseCapacity(int currentLength, int neededLength, int min)
  {
    int num = currentLength;
    if (num < min)
      num = min;
    while (num < neededLength)
      num *= 2;
    return num;
  }

  protected override void Awake()
  {
    JobsAllocator<JobSharedFields>.Allocate(ref this.shared, 1);
    this.jobPerf.Open();
    base.Awake();
    this.GenerateCharts();
    this.FixedUpdateEarlyTask().Forget();
  }

  private void OnDestroy()
  {
    if (this.shared.IsCreated)
      this.shared.Ref().DebugMarkersArray.Dispose();
    this.shared.Dispose();
    this.aeroParts.FullClear();
    this.controlSurfaces.FullClear();
    this.shipParts.FullClear();
    this.vehicles.FullClear();
    this.ships.Clear();
    this.pilots.Clear();
    this.aeroJob.liftCharts.DelayDispose<float>();
    this.aeroJob.dragCharts.DelayDispose<float>();
    this.aeroJob.DisposeAll();
    this.controlJob.DisposeAll();
    this.waterJobs.DisposeAll();
    this.vehicleJob.DisposeAll();
    this.detector.DisposeAll();
    this.jobPerf.Dispose();
  }

  private void GenerateCharts()
  {
    List<Airfoil> airfoilsList = new List<Airfoil>();
    foreach (AircraftDefinition aircraftDefinition in Encyclopedia.i.aircraft)
      aircraftDefinition.aircraftParameters.AddAirfoils(ref airfoilsList);
    NativeArray<float> dst1 = new NativeArray<float>(airfoilsList.Count * 128 /*0x80*/, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
    NativeArray<float> dst2 = new NativeArray<float>(airfoilsList.Count * 128 /*0x80*/, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
    for (int index = 0; index < airfoilsList.Count; ++index)
    {
      airfoilsList[index].id = index;
      NativeArray<float>.Copy(airfoilsList[index].BuildLiftChart(), 0, dst1, index * 128 /*0x80*/, 128 /*0x80*/);
      NativeArray<float>.Copy(airfoilsList[index].BuildDragChart(), 0, dst2, index * 128 /*0x80*/, 128 /*0x80*/);
    }
    this.aeroJob.liftCharts = dst1;
    this.aeroJob.dragCharts = dst2;
  }

  private async UniTask FixedUpdateEarlyTask()
  {
    JobManager jobManager = this;
    CancellationToken cancel = jobManager.destroyCancellationToken;
    while (true)
    {
      await UniTask.Yield(PlayerLoopTiming.FixedUpdate);
      if (!cancel.IsCancellationRequested)
      {
        try
        {
          jobManager.FixedUpdateEarly();
        }
        catch (Exception ex)
        {
          Debug.LogException(ex);
        }
        try
        {
          jobManager.FixedUpdateLate();
        }
        catch (Exception ex)
        {
          Debug.LogException(ex);
        }
      }
      else
        break;
    }
    cancel = new CancellationToken();
  }

  private void FixedUpdateEarly()
  {
    using (JobManager.fixedUpdateEarlyMarker.Auto())
    {
      JobPerf.GetTimestamp();
      this.ScheduleJobs();
      JobPerf.GetTimestamp();
      this.PilotAeroInputs();
    }
  }

  private void PilotAeroInputs()
  {
    using (JobManager.pilotAeroInputsMarker.Auto())
    {
      for (int index = this.pilots.Count - 1; index >= 0; --index)
      {
        if (this.pilots[index].Pilot_OnAeroInputsApplied() == PartResult.Remove)
          this.pilots.RemoveAt(index);
      }
    }
  }

  private void FixedUpdateLate()
  {
    using (JobManager.fixedUpdateLateMarker.Auto())
    {
      this.FinishJobs();
      this.shared.Ref().LogAndReset(this.jobPerf);
      if (!PlayerSettings.debugVis)
        return;
      this.shared.Ref().DrawDebugMarkers();
    }
  }

  private void ScheduleJobs()
  {
    using (JobManager.scheduleAllMarker.Auto())
    {
      if (!LevelInfo.airDensityChart.IsCreated)
      {
        Debug.LogError((object) "AeroJob could not run because LevelInfo had no AirDensityChart");
      }
      else
      {
        ref JobSharedFields local = ref this.shared.Ref();
        ++local.tickOffset;
        local.datum = Datum.GetBurstDatum();
        local.fixedDeltaTime = Time.fixedDeltaTime;
        local.timeSinceLevelLoad = Time.timeSinceLevelLoad;
        this.waterJobs.Schedule_1(this.shipParts, (Ptr<JobSharedFields>) this.shared);
        this.vehicleJob.Schedule_1(this.vehicles, (Ptr<JobSharedFields>) this.shared);
        this.controlJob.Schedule(this.controlSurfaces, (Ptr<JobSharedFields>) this.shared);
        this.aeroJob.Schedule(this.aeroParts, (Ptr<JobSharedFields>) this.shared);
        this.detector.Schedule();
        this.waterJobs.Schedule_2((Ptr<JobSharedFields>) this.shared);
        this.vehicleJob.Schedule_2((Ptr<JobSharedFields>) this.shared);
      }
    }
  }

  private void FinishJobs()
  {
    JobPerf.GetTimestamp();
    this.controlJob.FinishJob();
    this.aeroJob.FinishJob();
    this.vehicleJob.FinishJob();
    JobPerf.GetTimestamp();
    this.waterJobs.FinishJob();
    JobPerf.GetTimestamp();
    try
    {
      foreach (Ship ship in this.ships)
        ship.ApplyJobResults();
    }
    catch (Exception ex)
    {
      Debug.LogException(ex);
    }
    this.detector.FinishJob();
  }
}

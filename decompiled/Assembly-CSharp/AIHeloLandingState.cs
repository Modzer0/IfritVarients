// Decompiled with JetBrains decompiler
// Type: AIHeloLandingState
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System.Collections.Generic;
using Unity.Profiling;
using UnityEngine;

#nullable disable
public class AIHeloLandingState : PilotBaseState
{
  private static readonly ProfilerMarker fixedUpdateStateMarker = new ProfilerMarker("AIHeloLandingState.FixedUpdateState");
  private RotorShaft rotorShaft;
  private float maxRPM;
  private AircraftParameters aircraftParameters;
  private float lastEjectionCheck;
  private float stuckTimer;
  private float descentAmount;
  private float lastLandingClearCheck;
  private float lastNearbyAircraftCheck;
  private float ejectionCheckTimer;
  private float targetHeight;
  private bool touchedDown;
  private bool landingClear;
  private Vector3 avoidAircraftVector;
  private Vector3 waitOffset;
  private Airbase.VerticalLandingPoint landingPoint;
  private bool reachedApproachPoint;
  private List<Aircraft> nearbyAircraft;
  private GameObject modeDebug;
  private GameObject targetDebug;
  private GameObject aimLeadDebug;
  private Renderer modeDebugRenderer;

  public override void EnterState(Pilot pilot)
  {
    this.touchedDown = false;
    this.stateDisplayName = "landing";
    this.pilot = pilot;
    this.aircraft = pilot.aircraft;
    this.ejectionCheckTimer = 0.0f;
    this.aircraftParameters = this.aircraft.GetAircraftParameters();
    this.controlInputs = this.aircraft.GetInputs();
    RunwayQuery runwayQuery = new RunwayQuery()
    {
      RunwayType = RunwayQueryType.Vertical,
      MinSize = this.aircraft.maxRadius
    };
    if (this.aircraft.NetworkHQ.TryGetNearestAirbase(this.aircraft.transform.position, out this.nearestAirbase, runwayQuery) && this.nearestAirbase.TryRequestVerticalLanding(this.aircraft, runwayQuery, out this.landingPoint))
    {
      this.destination = this.landingPoint.GetApproachPoint(this.aircraft);
      this.reachedApproachPoint = false;
    }
    foreach (UnitPart unitPart in this.aircraft.partLookup)
    {
      if ((Object) unitPart != (Object) null && unitPart.gameObject.GetComponent<IEngine>() is RotorShaft component)
      {
        this.rotorShaft = component;
        this.maxRPM = component.GetMaxRPM();
        break;
      }
    }
  }

  private bool LandingPointClear()
  {
    if ((double) Time.timeSinceLevelLoad - (double) this.lastLandingClearCheck < 1.0)
      return this.landingClear;
    this.landingClear = !this.landingPoint.IsOccupied(this.aircraft);
    return this.landingClear;
  }

  public override void FixedUpdateState(Pilot pilot)
  {
    using (AIHeloLandingState.fixedUpdateStateMarker.Auto())
    {
      if ((double) Time.timeSinceLevelLoad - (double) this.lastEjectionCheck > 1.0)
        this.EjectionCheck();
      if ((Object) this.nearestAirbase == (Object) null)
        this.aircraft.StartEjectionSequence();
      else if (this.landingPoint == null || !this.landingPoint.IsAvailable())
      {
        pilot.SwitchState((PilotBaseState) pilot.AIHeloCombatState);
      }
      else
      {
        if (!this.reachedApproachPoint)
        {
          this.targetHeight = 45f;
          GlobalPosition approachPoint = this.landingPoint.GetApproachPoint(this.aircraft);
          this.destination = approachPoint;
          if (FastMath.InRange(approachPoint, this.aircraft.GlobalPosition(), 200f))
          {
            if (pilot.aircraft.gearState == LandingGear.GearState.LockedRetracted)
              pilot.aircraft.SetGear(true);
            this.landingPoint.RegisterLanding(this.aircraft);
            this.reachedApproachPoint = true;
          }
        }
        else
        {
          if (pilot.aircraft.gearState == LandingGear.GearState.LockedRetracted)
            pilot.aircraft.SetGear(true);
          this.destination = this.landingPoint.point.position.ToGlobalPosition();
          if (!this.aircraft.IsAutoHoverEnabled() && FastMath.InRange(this.destination, this.aircraft.GlobalPosition(), 200f))
          {
            this.aircraft.GetControlsFilter().SetAutoHover(true);
            this.aircraft.SetFlightAssist(false);
          }
          if (!this.LandingPointClear())
          {
            this.destination = this.destination + this.waitOffset;
            this.targetHeight = 30f;
            this.stuckTimer = 0.0f;
            if ((double) Time.timeSinceLevelLoad - (double) this.lastNearbyAircraftCheck > 1.0)
            {
              this.waitOffset = 75f * this.landingPoint.point.forward;
              this.lastNearbyAircraftCheck = Time.timeSinceLevelLoad;
              this.avoidAircraftVector = Vector3.zero;
              foreach (Aircraft landing in this.landingPoint.GetLandingQueue())
              {
                if ((Object) landing != (Object) this.aircraft && (Object) landing != (Object) null)
                  this.avoidAircraftVector += FastMath.Direction(landing.GlobalPosition(), this.aircraft.GlobalPosition());
              }
              this.avoidAircraftVector.y = 0.0f;
            }
            this.stateDisplayName = "waiting to land";
            this.destination = this.destination + this.avoidAircraftVector.normalized * 50f;
          }
          else
            this.stateDisplayName = "touching down";
        }
        float magnitude1 = ((this.destination - this.aircraft.GlobalPosition()) with
        {
          y = 0.0f
        }).magnitude;
        Vector3 targetVelocity = this.landingPoint != null ? this.landingPoint.GetVelocity() : Vector3.zero;
        double magnitude2 = (double) (this.aircraft.rb.velocity - targetVelocity).magnitude;
        if (this.reachedApproachPoint)
          this.targetHeight = (double) magnitude1 < 15.0 ? this.targetHeight - 3f * Time.fixedDeltaTime : 30f;
        if (PlayerSettings.debugVis && (Object) this.aircraft == (Object) SceneSingleton<CameraStateManager>.i.followingUnit)
        {
          if ((Object) this.targetDebug == (Object) null)
          {
            this.targetDebug = Object.Instantiate<GameObject>(GameAssets.i.debugPoint, Datum.origin);
            this.targetDebug.transform.localScale = Vector3.one * 10f;
          }
          this.targetDebug.transform.position = this.destination.ToLocalPosition();
        }
        if ((double) this.aircraft.radarAlt < 0.20000000298023224 || this.touchedDown)
        {
          this.controlInputs.brake = 1f;
          this.controlInputs.throttle = 0.0f;
          this.controlInputs.pitch = 0.0f;
          this.controlInputs.yaw = 0.0f;
          this.controlInputs.roll = 0.0f;
          this.aircraft.FilterInputs();
        }
        else if (this.aircraft.IsAutoHoverEnabled())
        {
          Unit attachedUnit;
          this.aircraft.autopilot.Hover(this.destination, this.targetHeight, this.nearestAirbase.TryGetAttachedUnit(out attachedUnit) ? attachedUnit.transform.forward : -this.landingPoint.point.forward);
        }
        else
          this.aircraft.autopilot.AutoAim(this.destination, this.targetHeight, this.destination - this.aircraft.GlobalPosition(), targetVelocity, (double) magnitude1 > 200.0);
      }
    }
  }

  private void EjectionCheck()
  {
    this.lastEjectionCheck = Time.timeSinceLevelLoad;
    if ((double) this.aircraft.speed < 1.0)
    {
      ++this.stuckTimer;
      if ((double) this.aircraft.radarAlt < 1.0)
      {
        this.touchedDown = true;
        ++this.ejectionCheckTimer;
      }
    }
    if ((double) this.stuckTimer > 5.0 || (double) this.ejectionCheckTimer > 2.0)
    {
      this.aircraft.StartEjectionSequence();
      this.pilot.SwitchState((PilotBaseState) this.pilot.parkedState);
    }
    else
    {
      if (!((Object) this.rotorShaft != (Object) null) || (double) this.rotorShaft.GetRPM() >= (double) this.maxRPM * 0.10000000149011612)
        return;
      this.aircraft.StartEjectionSequence();
      this.pilot.SwitchState((PilotBaseState) this.pilot.parkedState);
    }
  }

  public override void UpdateState(Pilot pilot)
  {
  }

  public override void LeaveState()
  {
  }
}

// Decompiled with JetBrains decompiler
// Type: AIPilotLandingState
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;
using Unity.Profiling;
using UnityEngine;

#nullable disable
public class AIPilotLandingState : PilotBaseState
{
  private static readonly ProfilerMarker fixedUpdateStateMarker = new ProfilerMarker("AIPilotLandingState.FixedUpdateState");
  private Airbase airbase;
  private AircraftParameters aircraftParameters;
  private GameObject aimVis;
  private GameObject nextWaypointDebug;
  private Airbase.Runway.RunwayUsage runwayUsage;
  private Transform runwayExitPoint;
  private List<Missile> missileAlerts;
  private float timeOnGround;
  private float lastSlowCheck;
  private float lastExitPointCheck;
  private float lastAirbaseSearch;
  private float speedAdjustment;
  private float stuckTimer;
  private float lastEjectionCheck;
  private bool requestedLanding;
  private bool touchedDown;
  private bool reachedApproachPoint;
  private bool runwayExcursion;
  private bool abortingLanding;
  private bool hasTailHook;

  public override void EnterState(Pilot pilot)
  {
    pilot.flightInfo.HasTakenOff = true;
    this.aircraft = pilot.aircraft;
    this.aircraftParameters = this.aircraft.GetAircraftParameters();
    this.stateDisplayName = "landing";
    this.pilot = pilot;
    this.controlInputs = this.aircraft.GetInputs();
    this.missileAlerts = this.aircraft.GetMissileWarningSystem().knownMissiles;
    if (this.fuelChecker == null)
      this.fuelChecker = new PilotBaseState.FuelChecker(this.aircraft);
    this.timeOnGround = 0.0f;
    this.stuckTimer = 0.0f;
    this.requestedLanding = false;
    this.touchedDown = false;
    this.reachedApproachPoint = false;
    this.runwayExcursion = false;
    this.abortingLanding = false;
    this.hasTailHook = this.aircraft.weaponManager.HasTailHook();
    this.runwayExitPoint = (Transform) null;
    this.SearchBestAirbase();
    if (!PlayerSettings.debugVis)
      return;
    this.aimVis = NetworkSceneSingleton<Spawner>.i.SpawnLocal(GameAssets.i.debugPoint, Datum.origin);
    this.aimVis.transform.localScale = Vector3.one * 2f;
  }

  private void SearchBestAirbase()
  {
    if (this.reachedApproachPoint || this.abortingLanding || (double) Time.timeSinceLevelLoad - (double) this.lastAirbaseSearch < 10.0)
      return;
    this.lastAirbaseSearch = Time.timeSinceLevelLoad;
    this.speedAdjustment = Mathf.Sqrt(this.aircraft.GetMass() / this.aircraft.definition.aircraftInfo.maxWeight);
    float num = this.speedAdjustment * this.aircraftParameters.landingSpeed;
    RunwayQuery runwayQuery = new RunwayQuery()
    {
      RunwayType = RunwayQueryType.Landing,
      MinSize = this.aircraftParameters.takeoffDistance,
      TailHook = this.aircraft.weaponManager.HasTailHook(),
      LandingSpeed = num
    };
    this.airbase = this.aircraft.NetworkHQ.GetNearestAirbase(this.aircraft.transform.position, runwayQuery);
    Airbase.Runway.RunwayUsage? nullable = (UnityEngine.Object) this.airbase != (UnityEngine.Object) null ? this.airbase.RequestLanding(this.aircraft, runwayQuery) : new Airbase.Runway.RunwayUsage?();
    if (nullable.HasValue)
    {
      this.runwayUsage = nullable.Value;
    }
    else
    {
      this.aircraft.StartEjectionSequence();
      this.pilot.SwitchState((PilotBaseState) null);
    }
  }

  private void CheckApproachParameters()
  {
    if ((double) Time.timeSinceLevelLoad - (double) this.lastSlowCheck < 2.0)
      return;
    this.lastSlowCheck = Time.timeSinceLevelLoad;
    this.SearchBestAirbase();
    if ((double) this.timeOnGround > 2.0)
      this.runwayExcursion = !this.runwayUsage.Runway.AircraftOnRunway(this.aircraft) || (double) this.timeOnGround > 10.0 && (double) this.aircraft.speed < 1.0;
    if (this.runwayExcursion && (double) this.aircraft.speed > (double) this.aircraftParameters.landingSpeed * 0.5)
    {
      this.abortingLanding = true;
      this.touchedDown = false;
      this.runwayExcursion = false;
      this.reachedApproachPoint = false;
    }
    else
    {
      if ((double) this.aircraft.radarAlt < 10.0)
        return;
      float num = Mathf.Sqrt(this.aircraft.GetMass() / this.aircraft.definition.aircraftInfo.maxWeight) * this.aircraftParameters.landingSpeed;
      if (!this.runwayUsage.Runway.IsSuitable(new RunwayQuery()
      {
        RunwayType = RunwayQueryType.Landing,
        MinSize = this.aircraftParameters.takeoffDistance,
        LandingSpeed = num,
        TailHook = this.aircraft.weaponManager.HasTailHook()
      }))
      {
        this.abortingLanding = true;
      }
      else
      {
        foreach (Aircraft landing in this.runwayUsage.Runway.GetLandingList())
        {
          if (!((UnityEngine.Object) landing == (UnityEngine.Object) null) && !((UnityEngine.Object) landing == (UnityEngine.Object) this.aircraft))
          {
            Vector3 to = this.aircraft.rb.velocity - landing.rb.velocity;
            Vector3 vector3 = landing.transform.position - this.aircraft.transform.position;
            if ((double) Vector3.Dot(vector3, this.aircraft.transform.forward) >= 0.0 && (double) to.magnitude > ((double) vector3.magnitude - 200.0) * 0.10000000149011612 && (double) Vector3.Angle(vector3, to) < 10.0)
              this.abortingLanding = true;
          }
        }
      }
    }
  }

  public bool TrySetExitPoint(Airbase.Runway runway)
  {
    if ((double) Time.timeSinceLevelLoad - (double) this.lastExitPointCheck > 10.0)
    {
      this.lastExitPointCheck = Time.timeSinceLevelLoad;
      runway.TryGetExitTaxiPoint(this.aircraft.transform, this.aircraft.speed, out this.runwayExitPoint);
    }
    return (UnityEngine.Object) this.runwayExitPoint != (UnityEngine.Object) null;
  }

  public override void FixedUpdateState(Pilot pilot)
  {
    using (AIPilotLandingState.fixedUpdateStateMarker.Auto())
    {
      this.EjectionCheck();
      if (this.abortingLanding)
      {
        Vector3 velocity = this.aircraft.rb.velocity with
        {
          y = 0.0f
        };
        this.aircraft.autopilot.AutoAim(this.aircraft.GlobalPosition() + velocity.normalized * 1000f + Vector3.up * 8f, true, true, false, 0.95f, 135f, false, 100f, Vector3.zero);
        this.controlInputs.throttle = 1f;
        this.aircraft.SetFlightAssist(true);
        this.aircraft.SetGear(false);
        this.runwayUsage.Runway.DeregisterLanding(this.aircraft);
        if ((double) this.aircraft.speed <= (double) this.aircraftParameters.cornerSpeed)
          return;
        pilot.SwitchState((PilotBaseState) pilot.AICombatState);
      }
      else if ((double) this.aircraft.radarAlt > 30.0 && this.missileAlerts.Count > 0 && this.fuelChecker.HasEnoughFuel())
        this.abortingLanding = true;
      else if (this.runwayExcursion)
      {
        this.runwayUsage.Runway.DeregisterLanding(this.aircraft);
        if ((double) this.aircraft.speed >= 1.0)
          return;
        pilot.aircraft.StartEjectionSequence();
        pilot.SwitchState((PilotBaseState) pilot.parkedState);
      }
      else
      {
        this.CheckApproachParameters();
        if (this.runwayUsage.Runway == null)
        {
          this.SetAutopilot(this.aircraft.GlobalPosition() + this.aircraft.transform.forward * 10000f, this.aircraftParameters.turningRadius * 3f, false);
        }
        else
        {
          Vector3 normalized = this.runwayUsage.Runway.GetDirection(this.runwayUsage.Reverse).normalized;
          if (!this.reachedApproachPoint)
          {
            Vector3 glideslopeAimpoint = this.runwayUsage.Runway.GetGlideslopeAimpoint(this.aircraft, this.aircraftParameters.turningRadius * 3f, this.runwayUsage.Reverse, 30f);
            Vector3 from = glideslopeAimpoint - this.aircraft.transform.position;
            float num1 = (Mathf.Sin((float) (((double) Vector3.Angle(from, normalized) - 90.0) * (Math.PI / 180.0))) + 1f) * this.aircraftParameters.turningRadius;
            Vector3 position = glideslopeAimpoint + Vector3.RotateTowards(-normalized * num1, -from, 1.57079637f, 0.0f);
            float num2 = FastMath.Distance(position.ToGlobalPosition(), this.aircraft.GlobalPosition());
            if ((double) num2 < 500.0)
              this.reachedApproachPoint = true;
            this.SetAutopilot(position.ToGlobalPosition(), this.aircraftParameters.turningRadius * 3f, false);
            this.controlInputs.throttle = Mathf.Clamp((float) (0.5 - (double) (this.aircraft.speed - (this.aircraftParameters.cornerSpeed + num2 * 0.02f)) * 0.10000000149011612), 0.0f, this.aircraftParameters.cruiseThrottle);
          }
          else
          {
            Transform transform = this.runwayUsage.Reverse ? this.runwayUsage.Runway.End : this.runwayUsage.Runway.Start;
            Vector3 vector3 = transform.position - this.aircraft.transform.position;
            float magnitude = vector3.magnitude;
            Vector3 position1 = this.aircraft.transform.position;
            Vector3 position2 = transform.position;
            Vector3 spawnOffset = this.aircraft.definition.spawnOffset;
            float num3 = Vector3.Dot(this.aircraft.rb.velocity - this.runwayUsage.Runway.GetVelocity(), vector3.normalized);
            float timeToTouchdown = Mathf.Min(magnitude / num3, 30f);
            if (this.runwayUsage.Runway.GetVelocity() != Vector3.zero)
            {
              vector3 = transform.position + this.runwayUsage.Runway.GetVelocity() * timeToTouchdown - this.aircraft.transform.position;
              magnitude = vector3.magnitude;
            }
            float num4 = Vector3.Dot(new Vector3(vector3.x, 0.0f, vector3.z).normalized, normalized);
            float num5 = Vector3.Dot(new Vector3(this.aircraft.rb.velocity.x, 0.0f, this.aircraft.rb.velocity.z).normalized, normalized);
            if (((double) num4 < 0.5 || (double) num5 < 0.5) && this.aircraft.gearState == LandingGear.GearState.LockedExtended)
              magnitude *= -1f;
            float num6 = this.aircraftParameters.landingSpeed * 1.5f * this.speedAdjustment;
            if ((double) num5 > 0.949999988079071 && (double) num4 > 0.949999988079071)
            {
              num6 = (float) ((double) this.aircraftParameters.landingSpeed * (double) this.speedAdjustment + 0.014999999664723873 * (double) Mathf.Max(magnitude - 700f, 0.0f));
              if (this.aircraft.gearState == LandingGear.GearState.LockedRetracted)
                this.aircraft.SetGear(true);
              if (!this.requestedLanding && (double) magnitude < 2000.0)
              {
                this.requestedLanding = true;
                this.airbase.RpcRegisterUsage(this.aircraft, true, new byte?(this.runwayUsage.Runway.index));
              }
            }
            float num7 = Vector3.Dot(this.aircraft.transform.forward, this.aircraft.rb.velocity - NetworkSceneSingleton<LevelInfo>.i.GetWind(this.aircraft.GlobalPosition()));
            this.controlInputs.throttle = Mathf.Clamp((float) (((double) num6 + 5.0 - (double) num7) * 0.10000000149011612), 0.0f, this.aircraftParameters.cruiseThrottle);
            this.controlInputs.brake = this.touchedDown ? this.controlInputs.brake + 2f * Time.deltaTime : 0.0f;
            if ((double) this.aircraft.radarAlt < 0.5)
              this.touchedDown = true;
            GlobalPosition globalPosition = this.runwayUsage.Runway.GetGlideslopeAimpoint(this.aircraft, Mathf.Min(magnitude, this.aircraftParameters.turningRadius * 3f) - Mathf.Max(magnitude * 0.2f, 100f), this.runwayUsage.Reverse, timeToTouchdown).ToGlobalPosition();
            float glideslopeError = this.runwayUsage.Runway.GetGlideslopeError(this.aircraft, timeToTouchdown, this.runwayUsage.Reverse);
            if (this.aircraft.gearState == LandingGear.GearState.LockedExtended && (double) timeToTouchdown < 3.0 && (!this.hasTailHook || !this.runwayUsage.Runway.Arrestor))
            {
              this.controlInputs.throttle = 0.0f;
              globalPosition += Vector3.up * 2.5f;
            }
            if ((double) timeToTouchdown < 0.0 && (double) timeToTouchdown > -2.0 && this.hasTailHook && this.runwayUsage.Runway.Arrestor)
            {
              this.controlInputs.throttle = 0.99f;
              this.controlInputs.brake = 0.0f;
            }
            if (this.aircraftParameters.verticalLanding && this.runwayUsage.Runway.airbase.AttachedAirbase && (double) timeToTouchdown < 30.0)
              pilot.SwitchStateNew((PilotBaseState) new AIPilotShortLandingState());
            else if (this.touchedDown)
            {
              this.controlInputs.customAxis1 = 1f;
              this.timeOnGround += Time.deltaTime;
              Vector3 position3 = (this.runwayUsage.Runway.GetNearestPoint(this.aircraft.transform.position, false) + this.runwayUsage.Runway.GetDirection(this.runwayUsage.Reverse).normalized * 100f) with
              {
                y = globalPosition.ToLocalPosition().y
              };
              bool flag = this.TrySetExitPoint(this.runwayUsage.Runway);
              if (flag && (double) this.timeOnGround > 2.0)
              {
                float num8 = Vector3.Distance(this.runwayExitPoint.position, this.aircraft.transform.position);
                this.runwayExitPoint.position.ToGlobalPosition();
                float num9 = 15f + Mathf.Sqrt(Mathf.Max(num8 - 30f, 1f));
                if (this.runwayUsage.Runway.airbase.AttachedAirbase)
                  num9 *= 0.5f;
                this.controlInputs.throttle = (double) this.aircraft.speed < (double) num9 ? 0.5f : 0.0f;
                this.controlInputs.brake = (double) this.aircraft.speed > (double) num9 ? 1f : 0.0f;
                if ((double) num8 < (double) this.aircraft.maxRadius + 20.0)
                {
                  if (pilot.AITaxiState == null)
                    pilot.AITaxiState = new AIPilotTaxiState();
                  this.runwayUsage.Runway.DeregisterLanding(this.aircraft);
                  pilot.SwitchState((PilotBaseState) pilot.AITaxiState);
                  return;
                }
              }
              if (!flag && (double) this.aircraft.speed < 15.0)
              {
                if (pilot.AITaxiState == null)
                  pilot.AITaxiState = new AIPilotTaxiState();
                pilot.SwitchState((PilotBaseState) pilot.AITaxiState);
              }
              else
                this.SetAutopilot(position3.ToGlobalPosition(), magnitude, true);
            }
            else
            {
              this.timeOnGround = 0.0f;
              if ((double) num4 < 0.89999997615814209 || (double) num5 < 0.89999997615814209)
                this.SetAutopilot(globalPosition, magnitude, false);
              else
                this.SetAutoLand(globalPosition, glideslopeError, magnitude, (double) timeToTouchdown < 5.0);
            }
          }
        }
      }
    }
  }

  private void SetAutopilot(GlobalPosition aimpoint, float touchdownDist, bool alignWithRunway)
  {
    this.aircraft.autopilot.AutoAim(aimpoint, true, (double) touchdownDist < 500.0, alignWithRunway, 0.95f, 135f, true, 0.06f * touchdownDist, Vector3.zero);
    if (!((UnityEngine.Object) this.aimVis != (UnityEngine.Object) null))
      return;
    this.aimVis.transform.localPosition = aimpoint.AsVector3();
  }

  private void SetAutoLand(
    GlobalPosition aimpoint,
    float heightError,
    float touchdownDist,
    bool alignWithRunway)
  {
    this.aircraft.autopilot.AutoAim(aimpoint, true, (double) touchdownDist < 500.0, alignWithRunway, 0.95f, 135f, false, 0.05f * touchdownDist, Vector3.zero);
    if (!((UnityEngine.Object) this.aimVis != (UnityEngine.Object) null))
      return;
    this.aimVis.transform.localPosition = aimpoint.AsVector3();
  }

  private void EjectionCheck()
  {
    if ((double) Time.timeSinceLevelLoad - (double) this.lastEjectionCheck < 5.0)
      return;
    this.lastEjectionCheck = Time.timeSinceLevelLoad;
    bool flag = false;
    if ((double) this.aircraft.cockpit.xform.position.y < (double) Datum.LocalSeaY)
      flag = true;
    if ((double) this.aircraft.speed < 1.0 && (double) this.aircraft.radarAlt < 5.0)
      flag = true;
    if ((double) this.aircraft.radarAlt > 40.0 && ((double) Vector3.Dot(this.aircraft.cockpit.xform.forward, this.aircraft.rb.velocity) < 0.0 || (double) this.aircraft.partDamageTracker.GetDetachedRatio() > 0.11999999731779099))
      flag = true;
    if (!flag)
      return;
    this.pilot.aircraft.StartEjectionSequence();
  }

  public override void UpdateState(Pilot pilot)
  {
  }

  public override void LeaveState()
  {
  }
}

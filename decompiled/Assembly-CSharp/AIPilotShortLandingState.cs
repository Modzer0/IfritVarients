// Decompiled with JetBrains decompiler
// Type: AIPilotShortLandingState
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System.Collections.Generic;
using Unity.Profiling;
using UnityEngine;

#nullable disable
public class AIPilotShortLandingState : PilotBaseState
{
  private static readonly ProfilerMarker fixedUpdateStateMarker = new ProfilerMarker("AIPilotShortLandingState.FixedUpdateState");
  private Airbase airbase;
  private AircraftParameters aircraftParameters;
  private GameObject aimVis;
  private GameObject nextWaypointDebug;
  private Airbase.Runway.RunwayUsage runwayUsage;
  private ControlsFilter controlsFilter;
  private Transform runwayExitPoint;
  private List<Missile> missileAlerts;
  private float timeOnGround;
  private float lastSlowCheck;
  private float lastExitPointCheck;
  private float lastAirbaseSearch;
  private float speedAdjustment;
  private float stuckTimer;
  private float lastEjectionCheck;
  private float touchdownSpeed;
  private float approachSpeed;
  private float hoverTargetHeight;
  private float adjustedShortLandingSpeed;
  private bool requestedLanding;
  private bool startedRollingVerticalApproach;
  private bool touchedDown;
  private bool reachedApproachPoint;
  private bool runwayExcursion;
  private bool abortingLanding;
  private bool stationaryOnGround;

  public override void EnterState(Pilot pilot)
  {
    pilot.flightInfo.HasTakenOff = true;
    this.aircraft = pilot.aircraft;
    this.aircraftParameters = this.aircraft.GetAircraftParameters();
    this.controlsFilter = this.aircraft.GetControlsFilter();
    this.stateDisplayName = "Short Landing";
    this.pilot = pilot;
    this.controlInputs = this.aircraft.GetInputs();
    this.missileAlerts = this.aircraft.GetMissileWarningSystem().knownMissiles;
    this.timeOnGround = 0.0f;
    this.stuckTimer = 0.0f;
    this.requestedLanding = false;
    this.touchedDown = false;
    this.reachedApproachPoint = false;
    this.runwayExcursion = false;
    this.abortingLanding = false;
    this.runwayExitPoint = (Transform) null;
    this.approachSpeed = Mathf.Lerp(this.aircraftParameters.takeoffSpeed, this.aircraftParameters.cornerSpeed, 0.5f);
    this.touchdownSpeed = 25f;
    this.SearchBestAirbase();
    if (!PlayerSettings.debugVis)
      return;
    this.aimVis = NetworkSceneSingleton<Spawner>.i.SpawnLocal(GameAssets.i.debugPoint, Datum.origin);
    this.aimVis.transform.localScale = Vector3.one * 2f;
  }

  private void SearchBestAirbase()
  {
    this.lastAirbaseSearch = Time.timeSinceLevelLoad;
    this.speedAdjustment = Mathf.Sqrt(this.aircraft.GetMass() / this.aircraft.definition.aircraftInfo.maxWeight);
    float num = this.speedAdjustment * this.aircraftParameters.takeoffSpeed;
    RunwayQuery runwayQuery = new RunwayQuery()
    {
      RunwayType = RunwayQueryType.Landing,
      MinSize = this.aircraftParameters.takeoffDistance,
      LandingSpeed = num,
      TailHook = this.aircraft.weaponManager.HasTailHook()
    };
    this.airbase = this.aircraft.NetworkHQ.GetNearestAirbase(this.aircraft.transform.position, runwayQuery);
    Airbase.Runway.RunwayUsage? nullable = (Object) this.airbase != (Object) null ? this.airbase.RequestLanding(this.aircraft, runwayQuery) : new Airbase.Runway.RunwayUsage?();
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
    if ((double) this.timeOnGround > 2.0)
      this.runwayExcursion = !this.runwayUsage.Runway.AircraftOnRunway(this.aircraft) || (double) this.timeOnGround > 10.0 && (double) this.aircraft.speed < 1.0;
    if (this.runwayExcursion && (double) this.aircraft.speed > (double) this.aircraftParameters.takeoffSpeed * 0.5 && !this.aircraft.IsAutoHoverEnabled())
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
      if (!this.runwayUsage.Runway.IsSuitable(new RunwayQuery()))
      {
        this.abortingLanding = true;
      }
      else
      {
        this.adjustedShortLandingSpeed = Mathf.Pow(this.aircraft.GetMass() / this.aircraft.definition.aircraftInfo.maxWeight, 2f) * this.aircraftParameters.shortLandingSpeed;
        foreach (Aircraft landing in this.runwayUsage.Runway.GetLandingList())
        {
          if (!((Object) landing == (Object) null) && !((Object) landing == (Object) this.aircraft))
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
    return (Object) this.runwayExitPoint != (Object) null;
  }

  public override void FixedUpdateState(Pilot pilot)
  {
    using (AIPilotShortLandingState.fixedUpdateStateMarker.Auto())
    {
      if (this.abortingLanding)
      {
        Vector3 velocity = this.aircraft.rb.velocity with
        {
          y = 0.0f
        };
        this.aircraft.autopilot.AutoAim(this.aircraft.GlobalPosition() + velocity.normalized * 1000f + Vector3.up * 8f, true, false, false, 0.95f, 45f, false, 100f, Vector3.zero);
        this.controlInputs.throttle = 1f;
        this.controlInputs.customAxis1 = 1f;
        this.aircraft.SetFlightAssist(true);
        this.aircraft.GetControlsFilter().SetAutoHover(false);
        this.aircraft.SetGear(false);
        this.runwayUsage.Runway.DeregisterLanding(this.aircraft);
        if ((double) this.aircraft.speed <= (double) this.aircraftParameters.cornerSpeed)
          return;
        pilot.SwitchState((PilotBaseState) pilot.AICombatState);
      }
      else if (this.missileAlerts.Count > 0)
      {
        this.abortingLanding = true;
      }
      else
      {
        if ((double) this.aircraft.speed < 1.0)
        {
          this.stuckTimer += Time.deltaTime;
          if ((double) this.stuckTimer > 5.0)
          {
            pilot.aircraft.StartEjectionSequence();
            pilot.SwitchState((PilotBaseState) pilot.parkedState);
          }
        }
        if (this.runwayExcursion)
        {
          this.runwayUsage.Runway.DeregisterLanding(this.aircraft);
          this.controlInputs.throttle = 0.0f;
          this.controlInputs.brake = 1f;
          if ((double) this.aircraft.speed >= 1.0)
            return;
          pilot.aircraft.StartEjectionSequence();
          pilot.SwitchState((PilotBaseState) pilot.parkedState);
        }
        else
        {
          this.EjectionCheck();
          this.CheckApproachParameters();
          Vector3 vector3_1 = this.runwayUsage.Runway.GetDirection(this.runwayUsage.Reverse);
          Vector3 normalized = vector3_1.normalized;
          Transform transform = this.runwayUsage.Reverse ? this.runwayUsage.Runway.End : this.runwayUsage.Runway.Start;
          Vector3 vector3_2 = transform.position - this.aircraft.transform.position;
          float magnitude = vector3_2.magnitude;
          Vector3 position1 = this.aircraft.transform.position;
          Vector3 position2 = transform.position;
          Vector3 spawnOffset = this.aircraft.definition.spawnOffset;
          float num1 = Vector3.Dot(this.aircraft.rb.velocity - this.runwayUsage.Runway.GetVelocity(), vector3_2.normalized);
          float timeToTouchdown = Mathf.Min(magnitude / num1, 30f);
          if (this.runwayUsage.Runway.GetVelocity() != Vector3.zero)
          {
            vector3_2 = transform.position + this.runwayUsage.Runway.GetVelocity() * timeToTouchdown - this.aircraft.transform.position;
            magnitude = vector3_2.magnitude;
          }
          float num2 = Vector3.Dot(new Vector3(vector3_2.x, 0.0f, vector3_2.z).normalized, normalized);
          vector3_1 = new Vector3(this.aircraft.rb.velocity.x, 0.0f, this.aircraft.rb.velocity.z);
          float num3 = Vector3.Dot(vector3_1.normalized, normalized);
          if (((double) num2 < 0.5 || (double) num3 < 0.5) && this.aircraft.gearState == LandingGear.GearState.LockedExtended)
            magnitude *= -1f;
          double takeoffSpeed1 = (double) this.aircraftParameters.takeoffSpeed;
          double speedAdjustment1 = (double) this.speedAdjustment;
          if ((double) num3 > 0.949999988079071 && (double) num2 > 0.949999988079071)
          {
            double takeoffSpeed2 = (double) this.aircraftParameters.takeoffSpeed;
            double speedAdjustment2 = (double) this.speedAdjustment;
            double num4 = (double) Mathf.Max(magnitude - 700f, 0.0f);
            if (this.aircraft.gearState == LandingGear.GearState.LockedRetracted)
              this.aircraft.SetGear(true);
            if (!this.requestedLanding && (double) magnitude < 2000.0)
            {
              this.requestedLanding = true;
              this.airbase.RpcRegisterUsage(this.aircraft, true, new byte?(this.runwayUsage.Runway.index));
            }
          }
          this.controlInputs.brake = this.touchedDown ? this.controlInputs.brake + 2f * Time.deltaTime : 0.0f;
          if ((double) this.aircraft.radarAlt < 0.5)
            this.touchedDown = true;
          GlobalPosition globalPosition = this.runwayUsage.Runway.GetGlideslopeAimpoint(this.aircraft, Mathf.Min(magnitude, this.aircraftParameters.turningRadius * 3f) - Mathf.Max(magnitude * 0.2f, 100f), this.runwayUsage.Reverse, timeToTouchdown).ToGlobalPosition();
          float glideslopeError = this.runwayUsage.Runway.GetGlideslopeError(this.aircraft, timeToTouchdown, this.runwayUsage.Reverse);
          if (!this.touchedDown)
          {
            if (!this.aircraft.flightAssist)
              this.aircraft.SetFlightAssist(true);
            vector3_1 = this.runwayUsage.Runway.GetVelocity();
            double num5 = (double) vector3_1.magnitude + 2.0 * (double) Mathf.Sqrt(this.runwayUsage.Runway.Length);
            float num6 = this.aircraft.speed - Mathf.Max(Mathf.Sqrt((float) (num5 * num5 + 3.0 * (double) magnitude)), this.adjustedShortLandingSpeed);
            float num7 = (float) (0.34999999403953552 - (double) num6 * 0.10000000149011612);
            this.controlInputs.customAxis1 = (double) this.aircraft.speed > 130.0 ? 0.0f : num7;
            if (this.aircraft.IsAutoHoverEnabled())
            {
              Vector3 direction = this.runwayUsage.Runway.GetDirection(this.runwayUsage.Reverse);
              GlobalPosition destination = (double) this.aircraft.speed > 20.0 ? transform.GlobalPosition() : this.runwayUsage.Runway.GetNearestPoint(this.aircraft.transform, false).ToGlobalPosition();
              Vector3 vector3_3 = (this.aircraft.GlobalPosition() - destination) with
              {
                y = 0.0f
              };
              this.stateDisplayName = "Short Landing Touchdown";
              if ((double) vector3_3.sqrMagnitude < 400.0)
                this.hoverTargetHeight -= 2f * Time.fixedDeltaTime;
              else
                this.hoverTargetHeight = 10f;
              this.aircraft.autopilot.Hover(destination, this.hoverTargetHeight, direction);
              return;
            }
            this.stateDisplayName = "Short Landing Approach";
            globalPosition += Vector3.up * 10f;
            float a = num6 * -0.1f;
            if (this.controlsFilter.ReverseThrust)
              a = Mathf.Max(a, 0.0f);
            this.controlInputs.throttle = 0.8f + a;
            if ((double) magnitude < 500.0 && ((double) timeToTouchdown < 5.0 || (double) this.aircraft.GlobalPosition().y < (double) globalPosition.y))
            {
              this.hoverTargetHeight = 15f;
              this.aircraft.GetControlsFilter().SetAutoHover(true);
            }
          }
          if (this.touchedDown)
          {
            this.controlInputs.customAxis1 = 1f;
            this.controlInputs.throttle = 0.0f;
            this.timeOnGround += Time.deltaTime;
            Vector3 nearestPoint = this.runwayUsage.Runway.GetNearestPoint(this.aircraft.transform.position, false);
            vector3_1 = this.runwayUsage.Runway.GetDirection(this.runwayUsage.Reverse);
            Vector3 vector3_4 = vector3_1.normalized * 100f;
            Vector3 position3 = (nearestPoint + vector3_4) with
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
            this.SetAutoLand(globalPosition, glideslopeError, magnitude, (double) timeToTouchdown < 5.0);
        }
      }
    }
  }

  private void SetAutopilot(GlobalPosition aimpoint, float touchdownDist, bool alignWithRunway)
  {
    this.aircraft.autopilot.AutoAim(aimpoint, true, false, alignWithRunway, 0.95f, 135f, true, 0.06f * touchdownDist, Vector3.zero);
    if (!((Object) this.aimVis != (Object) null))
      return;
    this.aimVis.transform.localPosition = aimpoint.AsVector3();
  }

  private void SetAutoLand(
    GlobalPosition aimpoint,
    float heightError,
    float touchdownDist,
    bool alignWithRunway)
  {
    this.aircraft.autopilot.AutoAim(aimpoint, true, false, alignWithRunway, 0.95f, 135f, false, 0.05f * touchdownDist, Vector3.zero);
    if (!((Object) this.aimVis != (Object) null))
      return;
    this.aimVis.transform.localPosition = aimpoint.AsVector3();
  }

  private void EjectionCheck()
  {
    if ((double) Time.timeSinceLevelLoad - (double) this.lastEjectionCheck < 3.0)
      return;
    this.lastEjectionCheck = Time.timeSinceLevelLoad;
    bool flag = (double) this.aircraft.radarAlt < 1.0 && (double) this.aircraft.speed < 1.0;
    if ((double) this.aircraft.transform.position.y < (double) Datum.LocalSeaY || this.aircraft.cockpit.IsDetached() || this.stationaryOnGround & flag)
      this.pilot.aircraft.StartEjectionSequence();
    this.stationaryOnGround = flag;
  }

  public override void UpdateState(Pilot pilot)
  {
  }

  public override void LeaveState()
  {
  }
}

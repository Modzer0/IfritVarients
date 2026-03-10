// Decompiled with JetBrains decompiler
// Type: AIHeloTransportState
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;
using Unity.Profiling;
using UnityEngine;

#nullable disable
public class AIHeloTransportState : PilotBaseState
{
  private static readonly ProfilerMarker fixedUpdateStateMarker = new ProfilerMarker("AIHeloTransportState.FixedUpdateState");
  private AIHeloTransportState.TransportMode transportMode;
  private RotorShaft rotorShaft;
  private float maxRPM;
  private AIHeloTransportState.TransportDestination transportDestination;
  private AircraftParameters aircraftParameters;
  private float lastEjectionCheck;
  private float touchedDownTime;
  private float lastEnemyCheck;
  private float lastLandingSpotCheck;
  private float missileReactTime;
  private float countermeasuresLastSelected;
  private float targetDist;
  private float missileAimTime;
  private float lastFiredTime;
  private float lastTargetAssessTime;
  private float lastLoSCheck;
  private float lastAirbaseSearch;
  private float timeWithoutMission;
  private float targetHeight;
  private bool deployedCargo;
  private bool targetLoS;
  private List<Missile> missileAlerts = new List<Missile>();
  private Unit currentTarget;
  private TrackingInfo currentTargetTracking;
  private TrackingInfo nearestGroundEnemy;
  private CombatAI.TargetSearchResults targetSearchResults;
  private Vector3 approachDirection;
  private string countermeasureType;
  private GameObject targetDebug;

  public AIHeloTransportState(Aircraft aircraft)
  {
    this.aircraft = aircraft;
    this.missileAlerts = aircraft.GetMissileWarningSystem().knownMissiles;
    aircraft.GetMissileWarningSystem().onMissileWarning += new Action<MissileWarning.OnMissileWarning>(this.HeloTransport_OnMissileAlert);
  }

  public override void EnterState(Pilot pilot)
  {
    this.stateDisplayName = "transporting cargo";
    this.transportMode = AIHeloTransportState.TransportMode.Waiting;
    this.pilot = pilot;
    this.aircraft = pilot.aircraft;
    this.aircraftParameters = this.aircraft.GetAircraftParameters();
    this.touchedDownTime = 0.0f;
    this.deployedCargo = false;
    this.approachDirection = this.aircraft.transform.forward;
    this.timeWithoutMission = 0.0f;
    this.nearestAirbase = this.aircraft.NetworkHQ.GetNearestAirbase(this.aircraft.transform.position);
    this.aircraft.SetFlightAssistToDefault();
    this.controlInputs = this.aircraft.GetInputs();
    if (this.aircraft.GetControlsFilter() is HeloControlsFilter controlsFilter)
    {
      this.rotorShaft = controlsFilter.GetRotorShaft();
      if ((UnityEngine.Object) this.rotorShaft != (UnityEngine.Object) null)
        this.maxRPM = this.rotorShaft.GetMaxRPM();
    }
    TrackingInfo nearestUnit;
    if (!this.aircraft.NetworkHQ.GetNearestGroundEnemy(this.aircraft.GlobalPosition(), out nearestUnit))
      return;
    Vector3 vector3 = (nearestUnit.lastKnownPosition - this.aircraft.GlobalPosition()) with
    {
      y = 0.0f
    };
    this.transportDestination = new AIHeloTransportState.TransportDestination(nearestUnit.lastKnownPosition - vector3.normalized * 50f, nearestUnit.lastKnownPosition - vector3.normalized * 50f, 90f);
  }

  private void HeloTransport_OnMissileAlert(MissileWarning.OnMissileWarning e)
  {
    if ((double) this.missileReactTime > 0.0)
      return;
    this.missileReactTime = -0.5f / Mathf.Clamp(Vector3.Dot(FastMath.NormalizedDirection(this.aircraft.transform.position, e.missile.transform.position), this.aircraft.transform.forward), 0.2f, 1f);
  }

  private void ChooseCountermeasures()
  {
    this.countermeasuresLastSelected = Time.timeSinceLevelLoad;
    if (this.missileAlerts.Count > 1)
      this.missileAlerts.Sort((Comparison<Missile>) ((a, b) => Vector3.Distance(a.transform.position, this.aircraft.transform.position).CompareTo(Vector3.Distance(b.transform.position, this.aircraft.transform.position))));
    this.countermeasureType = this.aircraft.countermeasureManager.ChooseCountermeasure(this.missileAlerts[0]);
  }

  private void Countermeasures()
  {
    if (this.missileAlerts.Count == 0)
    {
      this.missileReactTime = 0.0f;
      if (!this.pilot.aircraft.countermeasureTrigger)
        return;
      this.aircraft.Countermeasures(false, this.aircraft.countermeasureManager.activeIndex);
    }
    else
    {
      if (PlayerSettings.debugVis && (UnityEngine.Object) this.aircraft == (UnityEngine.Object) SceneSingleton<CameraStateManager>.i.followingUnit)
        Debug.Log((object) $"[HeloTransportState] reacting to {this.missileAlerts.Count} incoming missiles");
      if ((double) Time.timeSinceLevelLoad - (double) this.countermeasuresLastSelected > 2.0)
        this.ChooseCountermeasures();
      Vector3 zero = Vector3.zero;
      for (int index = this.missileAlerts.Count - 1; index >= 0; --index)
        zero += this.aircraft.transform.position - this.missileAlerts[index].transform.position;
      this.missileReactTime += (double) this.missileReactTime < 0.0 ? Time.deltaTime : Time.deltaTime;
      if (!(this.countermeasureType == "IR"))
        return;
      if ((double) this.missileReactTime > 0.0 && (double) this.missileReactTime < 2.0)
      {
        if (this.pilot.aircraft.countermeasureTrigger)
          return;
        this.aircraft.Countermeasures(true, this.aircraft.countermeasureManager.activeIndex);
      }
      else
      {
        if (!this.pilot.aircraft.countermeasureTrigger)
          return;
        this.aircraft.Countermeasures(false, this.aircraft.countermeasureManager.activeIndex);
      }
    }
  }

  private void SearchForLandingSpot()
  {
    if ((double) Time.timeSinceLevelLoad - (double) this.lastLandingSpotCheck < 3.0)
      return;
    this.lastLandingSpotCheck = Time.timeSinceLevelLoad;
    foreach (WeaponStation weaponStation in this.aircraft.weaponStations)
    {
      if (weaponStation.WeaponInfo.cargo)
      {
        this.aircraft.weaponManager.currentWeaponStation = weaponStation;
        break;
      }
    }
    this.pilot.flightInfo.EnemyContact = true;
    bool rearmShip = this.aircraft.weaponManager.currentWeaponStation.WeaponInfo.rearmShip;
    bool rearmGround = this.aircraft.weaponManager.currentWeaponStation.WeaponInfo.rearmGround;
    if (rearmShip | rearmGround)
    {
      Unit lowestAmmoUnit;
      if (this.aircraft.NetworkHQ.GetUnitNeedingRearming(rearmShip, rearmGround, out lowestAmmoUnit))
      {
        this.transportDestination.validMission = true;
        this.transportDestination.UpdateLZ(this.aircraft, lowestAmmoUnit);
        if (rearmShip)
        {
          this.transportMode = AIHeloTransportState.TransportMode.NavalSupply;
          this.stateDisplayName = "Delivering Naval Supplies";
        }
        else
        {
          this.transportMode = AIHeloTransportState.TransportMode.LandSuppy;
          this.transportDestination.UpdateTouchdownPoint(100f, this.aircraft);
          this.stateDisplayName = "Delivering Supplies";
        }
      }
      else
      {
        this.transportMode = AIHeloTransportState.TransportMode.Waiting;
        this.transportDestination.validMission = false;
        this.OrbitAirbase();
        this.stateDisplayName = "Awaiting Cargo Mission";
      }
    }
    else
    {
      GlobalPosition? targetPosition = new GlobalPosition?();
      float range = float.MaxValue;
      float targetRadius = 0.0f;
      TrackingInfo nearestUnit;
      Unit unit;
      if (this.aircraft.NetworkHQ.GetNearestGroundEnemy(this.aircraft.GlobalPosition(), out nearestUnit) && nearestUnit.TryGetUnit(out unit))
      {
        this.stateDisplayName = "Transporting Vehicles (contact)";
        targetPosition = new GlobalPosition?(nearestUnit.lastKnownPosition);
        range = FastMath.Distance(targetPosition.Value, this.aircraft.GlobalPosition());
        targetRadius = unit.maxRadius * 2f;
      }
      MissionPosition.PositionResult result;
      if (MissionPosition.TryGetClosestObjectivePosition((Unit) this.aircraft, out result) && FastMath.InRange(this.aircraft.GlobalPosition(), result.Position, range))
      {
        targetPosition = new GlobalPosition?(result.Position);
        this.stateDisplayName = "Transporting Vehicles (objective)";
        targetRadius = 100f;
      }
      if (targetPosition.HasValue)
      {
        this.transportDestination.validMission = true;
        this.transportDestination.UpdateLZ(this.aircraft, targetPosition, targetRadius, ref this.approachDirection);
        this.transportDestination.UpdateTouchdownPoint(1000f, this.aircraft);
      }
      else
      {
        this.transportMode = AIHeloTransportState.TransportMode.Waiting;
        this.transportDestination.validMission = false;
        this.OrbitAirbase();
        this.stateDisplayName = "Awaiting Cargo Mission";
      }
    }
  }

  private Airbase GetNearestAirbase()
  {
    if ((double) Time.timeSinceLevelLoad - (double) this.lastAirbaseSearch > 3.0)
    {
      this.lastAirbaseSearch = Time.timeSinceLevelLoad;
      this.nearestAirbase = this.aircraft.NetworkHQ.GetNearestAirbase(this.aircraft.transform.position);
    }
    return this.nearestAirbase;
  }

  public void OrbitAirbase()
  {
    if ((UnityEngine.Object) this.nearestAirbase == (UnityEngine.Object) null)
    {
      this.transportDestination.touchdownPoint = this.aircraft.GlobalPosition();
    }
    else
    {
      this.timeWithoutMission += 3f;
      if ((double) this.timeWithoutMission > 45.0)
      {
        this.pilot.SwitchState((PilotBaseState) this.pilot.AIHeloLandingState);
      }
      else
      {
        int num = 2000;
        Vector3 vector3_1 = (this.nearestAirbase.center.GlobalPosition() - this.aircraft.GlobalPosition()) with
        {
          y = 0.0f
        };
        Vector3 current = Vector3.Cross(vector3_1, Vector3.up);
        if ((double) Vector3.Dot(vector3_1, this.aircraft.transform.right) < 0.0)
          current *= -1f;
        float f = vector3_1.magnitude / (float) num;
        float maxRadiansDelta = (double) Mathf.Abs(f) <= -0.40000000596046448 || (double) f >= 0.40000000596046448 ? f * 3f : f * 0.5f;
        Vector3 vector3_2 = Vector3.RotateTowards(current, vector3_1, maxRadiansDelta, 1f);
        this.transportDestination.touchdownPoint = this.aircraft.GlobalPosition() + vector3_2.normalized * 4000f;
      }
    }
  }

  private void TargetSearch()
  {
    if ((double) Time.timeSinceLevelLoad - (double) this.lastTargetAssessTime < 5.0 || this.aircraft.weaponManager.currentWeaponStation != null && this.aircraft.weaponManager.currentWeaponStation.SalvoInProgress)
      return;
    if ((double) this.aircraft.radarAlt < 2.0)
    {
      foreach (WeaponStation weaponStation in this.aircraft.weaponStations)
      {
        if (weaponStation.WeaponInfo.cargo)
        {
          this.aircraft.weaponManager.currentWeaponStation = weaponStation;
          break;
        }
      }
      this.aircraft.weaponManager.ClearTargetList();
    }
    else
    {
      this.lastTargetAssessTime = Time.timeSinceLevelLoad;
      Unit currentTarget = this.currentTarget;
      this.targetSearchResults = CombatAI.ChooseHQTarget((Unit) this.aircraft, 1f, this.aircraft.weaponStations);
      if ((UnityEngine.Object) this.targetSearchResults.target != (UnityEngine.Object) null)
        this.targetDist = FastMath.Distance(this.targetSearchResults.target.GlobalPosition(), this.aircraft.GlobalPosition());
      if (this.targetSearchResults.chosenWeaponStation != null)
        this.aircraft.weaponManager.currentWeaponStation = this.targetSearchResults.chosenWeaponStation;
      if (!((UnityEngine.Object) this.targetSearchResults.target != (UnityEngine.Object) currentTarget))
        return;
      this.currentTarget = this.targetSearchResults.target;
      this.aircraft.weaponManager.ClearTargetList();
      if (!((UnityEngine.Object) this.currentTarget != (UnityEngine.Object) null))
        return;
      this.pilot.flightInfo.EnemyContact = true;
      this.currentTargetTracking = this.aircraft.NetworkHQ.GetTrackingData(this.currentTarget.persistentID);
      this.aircraft.weaponManager.AddTargetList(this.currentTarget);
    }
  }

  private void LoSCheck()
  {
    if ((double) Time.timeSinceLevelLoad - (double) this.lastLoSCheck < 1.0)
      return;
    this.lastLoSCheck = Time.timeSinceLevelLoad;
    this.targetLoS = this.currentTarget.LineOfSight(this.aircraft.transform.position - Vector3.up * this.aircraft.definition.spawnOffset.y, 1000f);
  }

  private void DefendWithMissiles()
  {
    if ((UnityEngine.Object) this.currentTarget == (UnityEngine.Object) null || (double) this.aircraft.radarAlt < 10.0)
      return;
    WeaponStation currentWeaponStation = this.aircraft.weaponManager.currentWeaponStation;
    WeaponInfo weaponInfo = currentWeaponStation.WeaponInfo;
    if (weaponInfo.bomb || weaponInfo.gun || (double) this.targetDist < (double) weaponInfo.targetRequirements.minRange || (double) this.targetDist > (double) weaponInfo.targetRequirements.maxRange || currentWeaponStation.Ammo <= 0)
      return;
    this.LoSCheck();
    float num = Vector3.Angle(this.currentTarget.transform.position - this.aircraft.transform.position, this.aircraft.transform.forward);
    if (!this.targetLoS || (double) num > 30.0 || (double) this.currentTargetTracking.missileAttacks > (double) currentWeaponStation.WeaponInfo.CalcAttacksNeeded(this.currentTarget))
      return;
    this.missileAimTime = (double) num < (double) weaponInfo.targetRequirements.minAlignment ? this.missileAimTime + Time.deltaTime : 0.0f;
    GlobalPosition knownPosition;
    if (currentWeaponStation.SalvoInProgress || (double) num >= (double) weaponInfo.targetRequirements.minAlignment || (double) Time.timeSinceLevelLoad - (double) this.lastFiredTime <= 2.5 || !this.aircraft.NetworkHQ.TryGetKnownPosition(this.currentTarget, out knownPosition) || !FastMath.InRange(knownPosition, this.currentTarget.GlobalPosition(), 500f))
      return;
    this.aircraft.weaponManager.ClearTargetList();
    if (CombatAI.LookForMissileTargets(this.aircraft, this.currentTarget, currentWeaponStation) <= 0)
      return;
    this.pilot.Fire();
    this.lastTargetAssessTime = Time.timeSinceLevelLoad - 2f;
    this.lastFiredTime = Time.timeSinceLevelLoad;
  }

  public override void FixedUpdateState(Pilot pilot)
  {
    using (AIHeloTransportState.fixedUpdateStateMarker.Auto())
    {
      this.Countermeasures();
      float magnitude = ((this.destination - this.aircraft.GlobalPosition()) with
      {
        y = 0.0f
      }).magnitude;
      float num = this.aircraftParameters.minimumRadarAlt;
      if ((double) magnitude < 1500.0)
      {
        num *= 0.3f;
        if ((double) magnitude < 200.0)
          num = Mathf.Lerp(0.0f, num, (float) (((double) magnitude - 10.0) / 200.0));
      }
      bool deployed = (double) magnitude < 1000.0;
      if (this.aircraft.gearDeployed != deployed)
        this.aircraft.SetGear(deployed);
      this.aircraft.SetFlightAssist(true);
      this.SearchForLandingSpot();
      this.destination = this.transportDestination.touchdownPoint;
      if (PlayerSettings.debugVis && (UnityEngine.Object) this.aircraft == (UnityEngine.Object) SceneSingleton<CameraStateManager>.i.followingUnit)
      {
        if ((UnityEngine.Object) this.targetDebug == (UnityEngine.Object) null)
          this.targetDebug = UnityEngine.Object.Instantiate<GameObject>(GameAssets.i.debugArrowGreen, this.aircraft.transform);
        this.targetDebug.transform.rotation = Quaternion.LookRotation(this.destination - this.aircraft.GlobalPosition());
        this.targetDebug.transform.localScale = new Vector3(1f, 1f, magnitude);
      }
      bool followTerrain = (double) magnitude > 200.0;
      if (this.transportMode == AIHeloTransportState.TransportMode.NavalSupply && (double) magnitude < 100.0)
        num = 10f;
      if ((double) magnitude < 300.0)
      {
        if (!this.aircraft.IsAutoHoverEnabled())
        {
          this.aircraft.GetControlsFilter().SetAutoHover(true);
          this.aircraft.SetFlightAssist(false);
        }
        Vector3 aimDirection = (double) magnitude > 50.0 ? this.destination - this.aircraft.GlobalPosition() : this.aircraft.cockpit.xform.forward;
        this.targetHeight = (double) magnitude > 20.0 ? 20f : this.targetHeight - 2f * Time.fixedDeltaTime;
        this.aircraft.autopilot.Hover(this.destination, this.targetHeight, aimDirection);
      }
      else
        this.aircraft.autopilot.AutoAim(this.destination, num, Vector3.zero, Vector3.zero, followTerrain);
      this.EjectionCheck();
      if (this.transportMode == AIHeloTransportState.TransportMode.NavalSupply && this.transportDestination.dropConditionsMet)
      {
        this.touchedDownTime += Time.deltaTime;
        foreach (WeaponStation weaponStation in this.aircraft.weaponStations)
        {
          if (weaponStation.WeaponInfo.cargo)
            this.aircraft.weaponManager.currentWeaponStation = weaponStation;
        }
        if (!this.deployedCargo)
        {
          pilot.Fire();
          pilot.flightInfo.LastCargoDelivery = Time.timeSinceLevelLoad;
          this.deployedCargo = true;
          pilot.flightInfo.EnemyContact = true;
        }
        if ((double) this.touchedDownTime <= 1.0)
          return;
        pilot.SwitchState((PilotBaseState) pilot.AIHeloTakeoffState);
      }
      else
      {
        if ((double) this.aircraft.radarAlt < 2.0)
        {
          this.controlInputs.brake = 1f;
          this.controlInputs.throttle = 0.0f;
          this.controlInputs.pitch = 0.0f;
          this.controlInputs.yaw = 0.0f;
          this.controlInputs.roll = 0.0f;
          if ((double) this.aircraft.speed < 10.0)
          {
            pilot.flightInfo.EnemyContact = true;
            this.touchedDownTime += Time.deltaTime;
            if (!this.deployedCargo)
            {
              pilot.flightInfo.LastCargoDelivery = Time.timeSinceLevelLoad;
              pilot.Fire();
            }
            if (this.aircraft.weaponManager.currentWeaponStation.WeaponInfo.rearmGround)
              this.deployedCargo = true;
            pilot.flightInfo.DeliveredCargo = true;
            if ((double) this.touchedDownTime <= 7.0)
              return;
            pilot.SwitchState((PilotBaseState) pilot.AIHeloTakeoffState);
            return;
          }
        }
        this.TargetSearch();
        this.DefendWithMissiles();
      }
    }
  }

  private void EjectionCheck()
  {
    if ((double) Time.timeSinceLevelLoad - (double) this.lastEjectionCheck < 5.0)
      return;
    this.lastEjectionCheck = Time.timeSinceLevelLoad;
    bool flag1 = false;
    if ((double) this.aircraft.cockpit.xform.position.y < (double) Datum.LocalSeaY)
      flag1 = true;
    int num = (double) this.aircraft.speed >= 1.0 ? 0 : ((double) this.aircraft.radarAlt < 5.0 ? 1 : 0);
    bool flag2 = (double) this.aircraft.radarAlt > 40.0;
    if (num != 0 && ((UnityEngine.Object) this.GetNearestAirbase() == (UnityEngine.Object) null || (double) FastMath.SquareDistance(this.aircraft.GlobalPosition(), this.transportDestination.touchdownPoint) > 40000.0))
      flag1 = true;
    if ((num | (flag2 ? 1 : 0)) != 0)
    {
      if ((double) this.aircraft.partDamageTracker.GetDetachedRatio() > 0.11999999731779099)
        flag1 = true;
      if ((UnityEngine.Object) this.rotorShaft != (UnityEngine.Object) null && (double) this.rotorShaft.GetRPM() < (double) this.maxRPM * 0.10000000149011612)
        flag1 = true;
    }
    if (!flag1)
      return;
    this.pilot.aircraft.StartEjectionSequence();
  }

  public override void UpdateState(Pilot pilot)
  {
  }

  public override void LeaveState()
  {
    this.aircraft.NetworkHQ.DeregisterDropZone(this.transportDestination.touchdownPoint);
  }

  public enum TransportMode
  {
    CombatVehicle,
    LandSuppy,
    NavalSupply,
    Radar,
    Waiting,
  }

  private struct TransportDestination(
    GlobalPosition landingPosition,
    GlobalPosition enemyPosition,
    float levelAmount)
  {
    public bool validMission = false;
    public bool dropConditionsMet = false;
    public GlobalPosition touchdownPoint = landingPosition;
    public GlobalPosition enemyPosition = enemyPosition;
    public GlobalPosition LZ = enemyPosition;
    public TrackingInfo nearestEnemy = (TrackingInfo) null;
    public float slope = levelAmount;
    public int touchdownPointAttempts = 0;

    public void UpdateLZ(
      Aircraft aircraft,
      GlobalPosition? targetPosition,
      float targetRadius,
      ref Vector3 approachDirection)
    {
      if (!targetPosition.HasValue)
      {
        this.slope = 90f;
        this.touchdownPointAttempts = 0;
      }
      else if (FastMath.InRange(aircraft.GlobalPosition(), this.touchdownPoint, 3000f))
      {
        if (!PlayerSettings.debugVis || !((UnityEngine.Object) aircraft == (UnityEngine.Object) SceneSingleton<CameraStateManager>.i.followingUnit))
          return;
        Debug.Log((object) "[HeloTransportState] situation changed, but comitting to landing");
      }
      else
      {
        approachDirection = FastMath.NormalizedDirection(targetPosition.Value, aircraft.GlobalPosition());
        approachDirection.y = 0.0f;
        GlobalPosition fromPosition = targetPosition.Value + approachDirection * (60f + targetRadius);
        float num = Mathf.Min(CombatAI.GetSafeStandoffDist(fromPosition, aircraft.NetworkHQ), 10000f);
        GlobalPosition a = fromPosition + approachDirection * num;
        if (FastMath.InRange(a, this.LZ, 1000f))
          return;
        this.LZ = a;
        this.slope = 90f;
        this.touchdownPointAttempts = 0;
        if (!PlayerSettings.debugVis || !((UnityEngine.Object) aircraft == (UnityEngine.Object) SceneSingleton<CameraStateManager>.i.followingUnit))
          return;
        Debug.Log((object) $"[HeloTransportState] situation changed, generating new LZ at a distance of {(this.LZ - targetPosition.Value).magnitude}m");
        GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(GameAssets.i.debugArrowGreen, Datum.origin);
        gameObject.transform.localPosition = targetPosition.Value.AsVector3() + Vector3.up * 200f;
        gameObject.transform.rotation = Quaternion.LookRotation(-Vector3.up);
        gameObject.transform.localScale = new Vector3(50f, 50f, 200f);
        UnityEngine.Object.Destroy((UnityEngine.Object) gameObject, 3f);
      }
    }

    public void UpdateLZ(Aircraft aircraft, Unit unitToRearm)
    {
      Vector3 normalized = (aircraft.GlobalPosition() - unitToRearm.GlobalPosition()).normalized with
      {
        y = 0.0f
      };
      GlobalPosition a = unitToRearm.GlobalPosition() + normalized * (30f + unitToRearm.maxRadius);
      if (unitToRearm is Ship unit)
      {
        float num = Mathf.Min(FastMath.Distance(aircraft.GlobalPosition(), unitToRearm.GlobalPosition()) / Mathf.Max(aircraft.speed, 1f), 30f);
        this.slope = 0.0f;
        if ((double) unit.speed * 10.0 > (double) unit.maxRadius)
        {
          Vector3 velocity = unit.rb.velocity with
          {
            y = 0.0f
          };
          Vector3 onNormal = velocity.normalized * unit.maxRadius + velocity * (20f + num);
          this.touchdownPoint = unit.GlobalPosition() + onNormal;
          GlobalPosition b = unit.GlobalPosition() + Vector3.Project(aircraft.GlobalPosition() - unit.GlobalPosition(), onNormal);
          this.dropConditionsMet = FastMath.InRange(aircraft.GlobalPosition(), b, 50f);
        }
        else
        {
          this.touchdownPoint = a;
          this.dropConditionsMet = FastMath.InRange(aircraft.GlobalPosition(), this.touchdownPoint, 50f);
        }
      }
      else if ((double) FastMath.SquareDistance(aircraft.GlobalPosition(), this.touchdownPoint) < 4000000.0)
      {
        if (!PlayerSettings.debugVis || !((UnityEngine.Object) aircraft == (UnityEngine.Object) SceneSingleton<CameraStateManager>.i.followingUnit))
          return;
        Debug.Log((object) "[HeloTransportState] situation changed, but comitting to landing");
      }
      else
      {
        if ((double) FastMath.SquareDistance(a, this.LZ) <= 1000000.0)
          return;
        this.LZ = a;
        this.slope = 90f;
        this.touchdownPointAttempts = 0;
        if (!PlayerSettings.debugVis || !((UnityEngine.Object) aircraft == (UnityEngine.Object) SceneSingleton<CameraStateManager>.i.followingUnit))
          return;
        Debug.Log((object) ("[HeloTransportState] situation changed, generating new LZ at a distance of 50m from " + unitToRearm.unitName));
      }
    }

    public void UpdateTouchdownPoint(float maxRadius, Aircraft aircraft)
    {
      if ((double) this.slope < 3.0)
      {
        if (!PlayerSettings.debugVis || !((UnityEngine.Object) aircraft == (UnityEngine.Object) SceneSingleton<CameraStateManager>.i.followingUnit))
          return;
        Debug.Log((object) $"[HeloTransportState] Satisfied with current touchdown point slope: {this.slope}");
      }
      else if ((double) this.slope < 20.0 && (double) FastMath.SquareDistance(aircraft.GlobalPosition(), this.touchdownPoint) < 1000000.0)
      {
        if (!PlayerSettings.debugVis || !((UnityEngine.Object) aircraft == (UnityEngine.Object) SceneSingleton<CameraStateManager>.i.followingUnit))
          return;
        Debug.Log((object) $"[HeloTransportState] Too close to landing point to change, touchdown slope: {this.slope}");
      }
      else
      {
        Vector2 vector2 = UnityEngine.Random.insideUnitCircle * Mathf.Min((float) (50 * this.touchdownPointAttempts), maxRadius);
        GlobalPosition position = this.LZ + new Vector3(vector2.x, 0.0f, vector2.y);
        RaycastHit hitInfo;
        if (!Physics.Linecast(position.ToLocalPosition() + Vector3.up * 4000f, position.ToLocalPosition() - Vector3.up * 4000f, out hitInfo, 64 /*0x40*/))
          return;
        ++this.touchdownPointAttempts;
        float num = Vector3.Angle(hitInfo.normal, Vector3.up);
        if ((double) num >= 20.0 || (double) hitInfo.point.y <= (double) Datum.LocalSeaY || (double) num >= (double) this.slope)
          return;
        aircraft.NetworkHQ.DeregisterDropZone(this.touchdownPoint);
        if (!aircraft.NetworkHQ.IsDropZoneClear(hitInfo.point.ToGlobalPosition()))
        {
          if (!PlayerSettings.debugVis || !((UnityEngine.Object) aircraft == (UnityEngine.Object) SceneSingleton<CameraStateManager>.i.followingUnit))
            return;
          Debug.Log((object) $"[HeloTransportState] Drop zone not clear, continuing search {num}");
        }
        else
        {
          if (PlayerSettings.debugVis && (UnityEngine.Object) aircraft == (UnityEngine.Object) SceneSingleton<CameraStateManager>.i.followingUnit)
            Debug.Log((object) $"[HeloTransportState] Found touchdown point with slope of {num}, old point had a slope of {this.slope}");
          this.slope = num;
          this.touchdownPoint = hitInfo.point.ToGlobalPosition();
          aircraft.NetworkHQ.RegisterDropZone(this.touchdownPoint);
        }
      }
    }
  }
}

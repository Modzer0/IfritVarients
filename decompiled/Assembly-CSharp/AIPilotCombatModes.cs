// Decompiled with JetBrains decompiler
// Type: AIPilotCombatModes
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;
using Unity.Profiling;
using UnityEngine;

#nullable disable
public class AIPilotCombatModes : PilotBaseState
{
  private static readonly ProfilerMarker fixedUpdateStateMarker = new ProfilerMarker("AIPilotCombatState.FixedUpdateState");
  private GlobalPosition targetKnownPosition;
  private Vector3 targetVel;
  private Vector3 targetVelPrev;
  private Vector3 targetVector;
  private Vector3 aimTargetVel;
  private Vector3 missileEvadeVector;
  private Vector3 randomErrorVector;
  private AircraftParameters aircraftParameters;
  private Unit currentTarget;
  private TrackingInfo currentTargetTracking;
  private float lastTargetAssessTime = -10f;
  private float lastFiredTime;
  private float breakOffTimer;
  private float missileReactTime;
  private float missileImpactTime;
  private float targetDist;
  private float targetAngle;
  private float lastTargetAngle;
  private float lastGunFiredTime;
  private GameObject modeDebug;
  private GameObject targetDebug;
  private GameObject aimLeadDebug;
  private Renderer modeDebugRenderer;
  private List<Missile> missileAlerts;
  private float countermeasuresLastSelected;
  private float lastEjectionCheck;
  private float lastAirbaseCheck;
  private float lastLoSCheck;
  private float timeWithoutTarget;
  private float aimEffort;
  private float strafeTimer;
  private float notchingCooldown;
  private float lastBombTargetCheck;
  private float targetHeight;
  private float bombLastDropped;
  private float fireTimer;
  private float climbFactor;
  private bool aimVelocity;
  private bool followTerrain;
  private bool ignoreCollision;
  private bool climbing;
  private bool targetObscured;
  private string countermeasureType;
  private TerrainWarningSystem terrainWarning;
  private CombatAI.TargetSearchResults targetSearchResults;

  public AIPilotCombatModes(Aircraft aircraft)
  {
    this.aircraft = aircraft;
    this.terrainWarning = aircraft.autopilot.GetTerrainWarningSystem();
    this.missileAlerts = aircraft.GetMissileWarningSystem().knownMissiles;
    aircraft.GetMissileWarningSystem().onMissileWarning += new Action<MissileWarning.OnMissileWarning>(this.AICombat_OnMissileAlert);
    this.friendlyAircraftProximity = new FriendlyAircraftProximity();
  }

  public override void EnterState(Pilot pilot)
  {
    this.pilot = pilot;
    this.timeWithoutTarget = 0.0f;
    this.aircraft = pilot.aircraft;
    this.targetHeight = this.aircraft.radarAlt;
    pilot.flightInfo.HasTakenOff = true;
    this.aircraft.SetFlightAssist(true);
    this.bombLastDropped = -100f;
    this.lastGunFiredTime = -100f;
    if ((UnityEngine.Object) pilot.aircraft.NetworkHQ == (UnityEngine.Object) null)
      return;
    this.controlInputs = this.aircraft.GetInputs();
    if (this.threatVector == null)
      this.threatVector = new ThreatVector((Unit) this.aircraft);
    if (this.fuelChecker == null)
      this.fuelChecker = new PilotBaseState.FuelChecker(this.aircraft);
    if (pilot.aircraft.gearState == LandingGear.GearState.LockedExtended)
      pilot.aircraft.SetGear(false);
    this.aircraft.onRadarWarning += new Action<Aircraft.OnRadarWarning>(this.AICombat_OnRadarWarning);
    this.aircraftParameters = this.aircraft.definition.aircraftParameters;
    if (!PlayerSettings.debugVis || !((UnityEngine.Object) this.modeDebug == (UnityEngine.Object) null))
      return;
    this.targetDebug = NetworkSceneSingleton<Spawner>.i.SpawnLocal(GameAssets.i.debugArrowGreen, Datum.origin);
    this.targetDebug.transform.localScale = Vector3.one * 2f;
    this.aimLeadDebug = NetworkSceneSingleton<Spawner>.i.SpawnLocal(GameAssets.i.waypointDebug, Datum.origin);
    this.aimLeadDebug.transform.localScale = Vector3.one * 0.1f;
  }

  private void AICombat_OnMissileAlert(MissileWarning.OnMissileWarning e)
  {
    this.randomErrorVector = UnityEngine.Random.insideUnitSphere;
    if ((double) this.missileReactTime > 0.0)
      return;
    this.missileReactTime = (float) UnityEngine.Random.Range(-2, -5);
  }

  private void AICombat_OnRadarWarning(Aircraft.OnRadarWarning e)
  {
    if ((double) e.emitter.definition.roleIdentity.antiAir <= 0.40000000596046448)
      return;
    this.targetHeight -= 50f;
  }

  public override void LeaveState()
  {
    this.aircraft.onRadarWarning -= new Action<Aircraft.OnRadarWarning>(this.AICombat_OnRadarWarning);
  }

  private void AvoidThreats()
  {
    this.threatVector.CheckThreats(this.currentTarget);
    if (!((UnityEngine.Object) this.currentTarget != (UnityEngine.Object) null))
      return;
    float num = this.threatVector.threatVector.magnitude / Mathf.Max(this.targetSearchResults.opportunity, 0.01f) / (float) (2.0 * ((double) this.aircraft.bravery + (double) this.friendlyAircraftProximity.CheckMorale(this.aircraft)));
    Vector3 vector3 = Vector3.RotateTowards((this.destination - this.aircraft.GlobalPosition()) with
    {
      y = 0.0f
    }, -this.threatVector.threatVector, Mathf.Min(num * 0.25f, 1.39626336f), 0.0f);
    this.destination = this.aircraft.GlobalPosition() + vector3;
  }

  private bool NoTarget()
  {
    GlobalPosition knownPosition;
    if ((UnityEngine.Object) this.currentTarget != (UnityEngine.Object) null && !this.currentTarget.disabled && this.aircraft.NetworkHQ.TryGetKnownPosition(this.currentTarget, out knownPosition))
    {
      this.timeWithoutTarget = 0.0f;
      this.targetVector = knownPosition - this.aircraft.GlobalPosition();
      this.targetAngle = Vector3.Angle(this.aircraft.transform.forward, this.targetVector);
      this.targetDist = this.targetVector.magnitude;
      this.destination = knownPosition;
      this.targetKnownPosition = this.destination;
      this.targetVel = (UnityEngine.Object) this.currentTarget.rb != (UnityEngine.Object) null ? this.currentTarget.rb.velocity : Vector3.zero;
      this.destination = this.destination + this.targetVel * Mathf.Min(this.targetDist / this.aircraft.speed, 20f);
      if ((UnityEngine.Object) this.targetDebug != (UnityEngine.Object) null && (UnityEngine.Object) SceneSingleton<CameraStateManager>.i.followingUnit == (UnityEngine.Object) this.aircraft)
      {
        this.targetDebug.SetActive(true);
        this.targetDebug.transform.position = this.aircraft.transform.position;
        this.targetDebug.transform.LookAt(this.currentTarget.transform.position);
        this.targetDebug.transform.localScale = new Vector3(1f, 1f, this.targetDist);
      }
      return false;
    }
    this.stateDisplayName = "no target";
    this.targetVel = Vector3.zero;
    if ((UnityEngine.Object) this.nearestAirbase != (UnityEngine.Object) null)
      this.destination = this.nearestAirbase.center.position.ToGlobalPosition();
    if ((UnityEngine.Object) this.modeDebugRenderer != (UnityEngine.Object) null)
      this.modeDebugRenderer.material.SetColor("_EmissionColor", Color.white);
    GlobalPosition destination;
    if (!this.pilot.flightInfo.EnemyContact && !this.targetSearchResults.outOfAmmo && MissionPosition.TryGetClosestPosition((Unit) this.aircraft, out destination))
    {
      this.destination = destination;
      if ((UnityEngine.Object) this.modeDebugRenderer != (UnityEngine.Object) null)
        this.modeDebugRenderer.material.SetColor("_EmissionColor", Color.magenta);
      this.AvoidThreats();
      return true;
    }
    this.timeWithoutTarget += Time.deltaTime;
    if ((double) this.timeWithoutTarget > 10.0 && this.missileAlerts.Count == 0)
    {
      if (this.pilot.AILandingState == null)
        this.pilot.AILandingState = new AIPilotLandingState();
      this.pilot.SwitchState((PilotBaseState) this.pilot.AILandingState);
      if ((UnityEngine.Object) this.modeDebugRenderer != (UnityEngine.Object) null)
        this.modeDebugRenderer.material.SetColor("_EmissionColor", Color.black);
    }
    if ((UnityEngine.Object) this.targetDebug != (UnityEngine.Object) null)
      this.targetDebug.SetActive(false);
    this.AvoidThreats();
    return true;
  }

  private new void FindNearestAirbase()
  {
    if ((double) Time.timeSinceLevelLoad - (double) this.lastAirbaseCheck < 5.0)
      return;
    this.lastAirbaseCheck = Time.timeSinceLevelLoad;
    this.nearestAirbase = this.aircraft.NetworkHQ.GetNearestAirbase(this.aircraft.transform.position, new RunwayQuery()
    {
      RunwayType = RunwayQueryType.Landing,
      MinSize = this.aircraftParameters.takeoffDistance,
      LandingSpeed = this.aircraftParameters.takeoffSpeed,
      TailHook = this.aircraft.weaponManager.HasTailHook()
    });
  }

  private void AssessHQTargets()
  {
    if ((double) Time.timeSinceLevelLoad - (double) this.lastTargetAssessTime < 5.0 || this.aircraft.weaponManager.currentWeaponStation != null && this.aircraft.weaponManager.currentWeaponStation.SalvoInProgress)
      return;
    Unit currentTarget = this.currentTarget;
    this.targetSearchResults = CombatAI.ChooseHQTarget((Unit) this.aircraft, this.aircraft.bravery, this.aircraft.weaponStations);
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

  private bool BreakOffAttack()
  {
    if ((double) this.breakOffTimer <= 0.0)
      return false;
    this.breakOffTimer -= Time.deltaTime;
    if ((UnityEngine.Object) this.nearestAirbase == (UnityEngine.Object) null)
      return false;
    this.threatVector.CheckThreats((Unit) null);
    this.destination = this.nearestAirbase.center.position.ToGlobalPosition();
    this.destination = this.destination - this.threatVector.threatVector.normalized * 10000f;
    this.aimVelocity = true;
    this.followTerrain = true;
    this.controlInputs.throttle = 1f;
    if ((UnityEngine.Object) this.modeDebugRenderer != (UnityEngine.Object) null)
      this.modeDebugRenderer.material.SetColor("_EmissionColor", Color.green + Color.red * 0.5f);
    this.stateDisplayName = "breaking off attack";
    return true;
  }

  private bool RetreatStandoff(WeaponInfo weaponInfo)
  {
    float num1 = Mathf.Max(this.aircraftParameters.turningRadius * 2f, weaponInfo.targetRequirements.minRange * 2f);
    float num2 = 0.0f;
    if (this.currentTarget is Aircraft currentTarget)
      num2 = currentTarget.speed / this.aircraft.speed;
    if ((double) this.aircraft.transform.position.y - (double) this.currentTarget.transform.position.y > (double) this.aircraftParameters.turningRadius || (double) num2 > 0.60000002384185791 || (double) this.targetDist > (double) num1 || (double) this.targetAngle < 90.0 && (double) this.targetDist > (double) weaponInfo.targetRequirements.minRange)
      return false;
    Vector3 vector3 = (this.aircraft.GlobalPosition() - this.currentTarget.GlobalPosition()) with
    {
      y = 0.0f
    };
    this.destination = this.aircraft.GlobalPosition() + vector3.normalized * num1;
    this.followTerrain = true;
    this.aimVelocity = true;
    this.ignoreCollision = true;
    this.controlInputs.throttle = 1f;
    this.aimEffort = 0.5f;
    if ((UnityEngine.Object) this.modeDebugRenderer != (UnityEngine.Object) null)
      this.modeDebugRenderer.material.SetColor("_EmissionColor", Color.yellow);
    this.stateDisplayName = "retreating";
    return true;
  }

  private void ManageAltitude(WeaponInfo weaponInfo)
  {
    if ((double) Time.timeSinceLevelLoad - (double) this.lastLoSCheck < 1.0)
      return;
    this.lastLoSCheck = Time.timeSinceLevelLoad;
    if ((double) this.targetHeight < (double) this.aircraftParameters.minimumRadarAlt)
      this.targetHeight = Mathf.Lerp(this.targetHeight, this.aircraftParameters.minimumRadarAlt, 0.5f);
    if ((double) this.targetHeight < (double) this.aircraft.radarAlt + 200.0)
      this.targetHeight += 5f;
    if ((UnityEngine.Object) this.currentTarget == (UnityEngine.Object) null || (UnityEngine.Object) weaponInfo == (UnityEngine.Object) null)
      return;
    this.targetObscured = !this.currentTarget.LineOfSight(this.aircraft.transform.position - Vector3.up * this.aircraft.definition.spawnOffset.y, 1000f);
    bool flag1 = (double) this.aircraft.transform.position.GlobalY() < (double) this.targetKnownPosition.y;
    bool flag2 = (double) this.targetDist < (double) Mathf.Max(weaponInfo.targetRequirements.maxRange, 4000f);
    bool flag3 = false;
    if (this.targetObscured & flag2)
      flag3 = true;
    else if ((double) this.currentTarget.radarAlt > 10.0 && (double) this.currentTarget.transform.position.y > (double) this.aircraft.transform.position.y)
      flag3 = true;
    if ((double) this.targetDist < 5000.0 & flag1)
      flag3 = true;
    this.targetHeight += flag3 ? 20f : -10f;
  }

  private bool EvadingMissiles()
  {
    if ((double) this.missileReactTime < 0.0)
      return false;
    if (this.missileAlerts.Count == 0)
    {
      if ((double) this.notchingCooldown <= 0.0)
        return false;
    }
    else
    {
      this.targetHeight = 10f;
      Vector3 vector3 = this.missileAlerts[0].transform.position - this.aircraft.transform.position;
      this.missileImpactTime = vector3.magnitude / Mathf.Max(Vector3.Dot(-vector3.normalized, this.missileAlerts[0].rb.velocity - this.aircraft.rb.velocity), 1f);
      Vector3 rhs = Vector3.Cross(this.missileAlerts[0].GetEvasionPoint() - this.aircraft.GlobalPosition(), this.aircraft.rb.velocity);
      this.missileEvadeVector = Vector3.Cross((this.aircraft.GlobalPosition() - this.missileAlerts[0].GetEvasionPoint()).normalized, rhs);
      if ((double) Vector3.Dot(this.missileEvadeVector, this.aircraft.transform.forward) < 0.0)
        this.missileEvadeVector *= -1f;
    }
    this.aimVelocity = true;
    int num = this.countermeasureType == "SARH" ? 1 : (this.countermeasureType == "ARH" ? 1 : 0);
    this.stateDisplayName = this.aircraft.countermeasureTrigger ? "countermeasures" : "evading missiles";
    if (num != 0)
    {
      bool flag = (double) this.missileImpactTime < 8.0 && (double) Vector3.Angle(this.missileEvadeVector, this.aircraft.rb.velocity) < 20.0;
      if (!this.aircraft.countermeasureTrigger & flag)
        this.aircraft.Countermeasures(true, this.aircraft.countermeasureManager.activeIndex);
      if (this.aircraft.countermeasureTrigger && !flag)
        this.aircraft.Countermeasures(false, this.aircraft.countermeasureManager.activeIndex);
    }
    this.destination = this.aircraft.GlobalPosition() + this.missileEvadeVector * 1000f + this.randomErrorVector * 100f / this.aircraft.skill;
    if ((double) this.missileImpactTime < 2.0)
    {
      this.destination = this.destination + Vector3.up * 1000f;
      this.followTerrain = false;
    }
    this.controlInputs.throttle = 1f;
    return true;
  }

  private void CountermeasureMissiles()
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
      if ((double) Time.timeSinceLevelLoad - (double) this.countermeasuresLastSelected > 2.0)
        this.ChooseCountermeasures();
      Vector3 zero = Vector3.zero;
      for (int index = this.missileAlerts.Count - 1; index >= 0; --index)
        zero += this.aircraft.transform.position - this.missileAlerts[index].transform.position;
      this.missileReactTime += (double) this.missileReactTime < 0.0 ? Mathf.Max(Vector3.Dot(-zero.normalized, this.aircraft.transform.forward), 0.4f) * Time.deltaTime : Time.deltaTime;
      if (!(this.countermeasureType == "IR"))
        return;
      if ((double) this.missileReactTime > 0.0)
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

  private void ChooseCountermeasures()
  {
    this.countermeasuresLastSelected = Time.timeSinceLevelLoad;
    if (this.missileAlerts.Count > 1)
      this.missileAlerts.Sort((Comparison<Missile>) ((a, b) => Vector3.Distance(a.transform.position, this.aircraft.transform.position).CompareTo(Vector3.Distance(b.transform.position, this.aircraft.transform.position))));
    this.countermeasureType = this.aircraft.countermeasureManager.ChooseCountermeasure(this.missileAlerts[0]);
  }

  private bool Bombing(WeaponInfo weaponInfo)
  {
    if (!weaponInfo.bomb || (double) Time.timeSinceLevelLoad - (double) this.bombLastDropped < 10.0)
      return false;
    WeaponStation currentWeaponStation = this.aircraft.weaponManager.currentWeaponStation;
    if (currentWeaponStation == null || currentWeaponStation.Ammo == 0)
      return false;
    Vector3 targetVector = this.targetVector with
    {
      y = 0.0f
    };
    Vector3 forward = this.aircraft.transform.forward with
    {
      y = 0.0f
    };
    if ((double) this.targetDist < 2000.0 + (double) this.aircraft.radarAlt * 0.5 + (double) this.aircraft.speed * 3.5999999046325684 && (double) Vector3.Angle(forward.normalized, targetVector.normalized) > 20.0)
    {
      this.breakOffTimer = 15f;
      return false;
    }
    if (this.targetObscured)
      return false;
    this.lastTargetAssessTime = Time.timeSinceLevelLoad;
    float initialHeight = this.aircraft.transform.position.y - (this.currentTarget.transform.position.y + weaponInfo.airburstHeight);
    Vector3 vector3 = new Vector3(this.currentTarget.transform.position.x - this.aircraft.transform.position.x, 0.0f, this.currentTarget.transform.position.z - this.aircraft.transform.position.z);
    float magnitude = vector3.magnitude;
    float y = this.aircraft.rb.velocity.y;
    float num1 = Kinematics.FallTime(initialHeight, y);
    float num2 = Mathf.Max(Vector3.Dot(this.aircraft.rb.velocity, vector3.normalized), 1f);
    float num3 = magnitude / num2;
    if ((double) num3 < 10.0 && (double) num1 < 8.0)
    {
      this.breakOffTimer = 15f;
      return false;
    }
    if ((double) Time.timeSinceLevelLoad - (double) this.lastBombTargetCheck > 2.0)
    {
      this.lastBombTargetCheck = Time.timeSinceLevelLoad;
      this.aircraft.weaponManager.ClearTargetList();
      CombatAI.LookForBombingTargets(this.aircraft, this.currentTarget, this.aircraft.weaponManager.currentWeaponStation);
    }
    if ((double) this.currentTarget.transform.position.y + 100.0 > (double) this.aircraft.transform.position.y)
      this.targetHeight += 50f * Time.deltaTime;
    float f = num3 - num1;
    if ((double) Mathf.Abs(f) < 1.5)
    {
      this.bombLastDropped = Time.timeSinceLevelLoad;
      this.lastFiredTime = Time.timeSinceLevelLoad;
      this.breakOffTimer = 15f;
      this.pilot.Fire();
    }
    this.stateDisplayName = "bombing";
    float num4 = this.aircraft.transform.position.GlobalY();
    this.targetHeight = Mathf.Max(this.targetHeight, num4 - this.targetKnownPosition.y);
    this.targetHeight += 30f * Time.deltaTime;
    this.followTerrain = false;
    this.destination = this.currentTarget.GlobalPosition() + this.targetVel * Mathf.Clamp(f, 0.0f, 30f) + Vector3.up * this.targetHeight;
    if ((double) f < 15.0 && (double) num4 < 2000.0)
    {
      float num5 = Mathf.Max(magnitude * (Mathf.Min(this.aircraft.speed - f * 4f, 300f) / 600f) - this.aircraft.transform.position.y, 0.0f);
      this.destination.y = num4 + num5;
      this.followTerrain = false;
    }
    if ((UnityEngine.Object) this.modeDebugRenderer != (UnityEngine.Object) null)
      this.modeDebugRenderer.material.SetColor("_EmissionColor", Color.blue);
    return true;
  }

  private bool Strafing(WeaponInfo weaponInfo)
  {
    if (!weaponInfo.boresight || this.aircraft.weaponManager.currentWeaponStation.Ammo <= 0 || (double) this.targetAngle > 50.0 || (double) this.targetDist > (double) weaponInfo.targetRequirements.maxRange * 2.0 || (double) this.targetAngle > (double) this.targetDist * 0.10000000149011612)
      return false;
    float num1 = TargetCalc.TargetLeadTime(this.currentTarget, this.aircraft.gameObject, this.aircraft.rb, weaponInfo.muzzleVelocity, weaponInfo.dragCoef, 2);
    float num2 = this.targetDist / Mathf.Max(Vector3.Dot(-this.targetVector.normalized, (UnityEngine.Object) this.currentTarget.rb != (UnityEngine.Object) null ? this.currentTarget.rb.velocity - this.aircraft.rb.velocity : -this.aircraft.rb.velocity), 0.1f);
    if ((double) this.targetAngle < 45.0)
      this.lastTargetAssessTime = Time.timeSinceLevelLoad;
    this.ignoreCollision = (double) this.targetAngle > 5.0 || (double) num2 < 3.0;
    GlobalPosition a = new GlobalPosition();
    GlobalPosition knownPosition;
    if (this.aircraft.NetworkHQ.TryGetKnownPosition(this.currentTarget, out knownPosition))
      a = knownPosition;
    else
      Debug.LogError((object) "Failed to get known position");
    if (this.targetObscured)
    {
      this.climbing = true;
    }
    else
    {
      Vector3 vector3_1 = (this.targetVel - this.targetVelPrev) / Time.fixedDeltaTime;
      this.targetVelPrev = this.targetVel;
      if ((double) this.currentTarget.radarAlt < 1.0)
      {
        double num3 = ((double) this.aircraft.transform.position.y - (double) this.currentTarget.transform.position.y) / (double) Mathf.Max(this.targetDist, 10f);
        if (num3 < 0.10000000149011612)
          this.climbing = true;
        if (num3 > 0.20000000298023224)
          this.climbing = false;
      }
      else
        this.climbing = false;
      float num4 = (double) this.currentTarget.speed < 30.0 ? weaponInfo.targetRequirements.maxRange : weaponInfo.targetRequirements.maxRange * 0.7f;
      Vector3 vector3_2 = this.targetVel * num1 + 0.5f * num1 * num1 * (vector3_1 + Vector3.up * 9.81f);
      GlobalPosition globalPosition = this.currentTarget.GlobalPosition() + vector3_2;
      double num5 = (double) Vector3.Dot(globalPosition - this.aircraft.GlobalPosition(), this.aircraft.cockpit.xform.up);
      Vector3 zero = Vector3.zero;
      foreach (Weapon weapon in this.aircraft.weaponManager.currentWeaponStation.Weapons)
        zero += weapon.transform.forward;
      float num6 = Vector3.Angle(zero, globalPosition - this.aircraft.GlobalPosition());
      float num7 = Mathf.Clamp((num6 - this.lastTargetAngle) / Time.fixedDeltaTime, -20f, 0.0f);
      this.lastTargetAngle = num6;
      this.strafeTimer = (double) num6 > 2.0 ? 1f : Mathf.Max(this.strafeTimer - 1f * Time.fixedDeltaTime, 0.0f);
      float num8 = Mathf.Clamp(50f * this.currentTarget.maxRadius / this.targetDist, 0.5f, 3f);
      float num9 = Mathf.Min(num6 * 0.05f, 1f);
      a = globalPosition + (this.targetVel * num9 + 0.5f * num9 * (vector3_1 + Vector3.up * 9.81f));
      if ((double) this.targetDist < (double) num4 && (double) num6 + (double) Mathf.Min(num7 * 0.25f, 0.0f) < (double) num8)
      {
        this.pilot.Fire();
        this.lastGunFiredTime = Time.timeSinceLevelLoad;
      }
      this.targetHeight = this.aircraft.radarAlt;
    }
    this.climbFactor += this.climbing ? 100f * Time.deltaTime : -500f * Time.deltaTime;
    this.climbFactor = Mathf.Max(this.climbFactor, 0.0f);
    this.destination = a + Vector3.up * this.climbFactor;
    if ((double) num2 > 0.0 && (double) this.targetAngle < 15.0)
    {
      int num10 = (double) Vector3.Dot(this.currentTarget.transform.forward, this.aircraft.transform.forward) > 0.0 ? 2 : 1;
      if ((double) this.currentTarget.speed < 30.0)
        num10 = 2;
      if ((double) num2 < (double) num10)
        this.breakOffTimer = (double) this.currentTarget.speed < 30.0 ? 4f : 2f;
    }
    this.aimVelocity = false;
    this.aimEffort = 1f;
    this.followTerrain = false;
    this.aimTargetVel = this.targetVel;
    this.controlInputs.throttle = (double) this.aircraft.speed <= (double) this.aircraftParameters.cornerSpeed || (double) this.aircraft.speed <= (double) this.currentTarget.speed * 1.5 ? 1f : 0.0f;
    if ((double) this.currentTarget.speed > 30.0)
    {
      if ((double) this.targetDist < 500.0 && (double) this.targetAngle < 45.0 && (double) this.aircraft.speed - (double) this.currentTarget.speed > 60.0)
        this.controlInputs.throttle = 0.0f;
      this.ignoreCollision = true;
    }
    if ((UnityEngine.Object) this.aimLeadDebug != (UnityEngine.Object) null)
    {
      this.aimLeadDebug.transform.localPosition = a.AsVector3();
      this.aimLeadDebug.transform.localScale = Vector3.one * 0.0005f * FastMath.Distance(a, SceneSingleton<CameraStateManager>.i.transform.GlobalPosition());
      this.aimLeadDebug.transform.LookAt(this.aircraft.transform.position);
    }
    if ((UnityEngine.Object) this.modeDebugRenderer != (UnityEngine.Object) null)
      this.modeDebugRenderer.material.SetColor("_EmissionColor", Color.red);
    this.stateDisplayName = "strafing";
    return true;
  }

  private bool UsingEnergyWeapon(WeaponInfo weaponInfo)
  {
    WeaponStation currentWeaponStation = this.aircraft.weaponManager.currentWeaponStation;
    float maxRange = weaponInfo.targetRequirements.maxRange;
    if (!weaponInfo.energy || (double) this.targetDist < (double) weaponInfo.targetRequirements.minRange || (double) this.targetDist > (double) maxRange)
      return false;
    if (Physics.Linecast(this.aircraft.transform.position, this.aircraft.transform.position + this.aircraft.rb.velocity * 4f, 64 /*0x40*/))
    {
      this.breakOffTimer = 4f;
      return false;
    }
    this.followTerrain = false;
    GlobalPosition knownPosition;
    if ((double) this.targetAngle < (double) weaponInfo.targetRequirements.minAlignment && (double) Time.timeSinceLevelLoad - (double) this.lastFiredTime > (double) weaponInfo.fireInterval && this.aircraft.NetworkHQ.TryGetKnownPosition(this.currentTarget, out knownPosition) && FastMath.InRange(knownPosition, this.currentTarget.GlobalPosition(), 500f))
    {
      this.pilot.Fire();
      if (currentWeaponStation.Ammo <= 0 || (double) this.targetDist < (double) weaponInfo.targetRequirements.minRange * 2.0)
        this.breakOffTimer = 5f;
    }
    if ((UnityEngine.Object) this.modeDebugRenderer != (UnityEngine.Object) null)
      this.modeDebugRenderer.material.SetColor("_EmissionColor", Color.cyan);
    this.stateDisplayName = "using energy weapon";
    return true;
  }

  private bool UsingMissiles(WeaponInfo weaponInfo)
  {
    WeaponStation currentWeaponStation = this.aircraft.weaponManager.currentWeaponStation;
    if (weaponInfo.bomb || weaponInfo.boresight)
      return false;
    GlobalPosition knownPosition;
    if ((double) weaponInfo.muzzleVelocity > 0.0 && (UnityEngine.Object) this.currentTarget.rb != (UnityEngine.Object) null && this.aircraft.NetworkHQ.TryGetKnownPosition(this.currentTarget, out knownPosition))
    {
      float num = this.targetDist / weaponInfo.muzzleVelocity;
      GlobalPosition a = knownPosition + this.currentTarget.rb.velocity * num;
      this.destination = a;
      this.targetDist = FastMath.Distance(a, this.aircraft.GlobalPosition());
      this.targetAngle = Vector3.Angle(this.destination - this.aircraft.GlobalPosition(), this.aircraft.transform.forward);
    }
    if ((double) this.targetDist < (double) weaponInfo.targetRequirements.minRange || (double) this.targetDist > (double) weaponInfo.targetRequirements.maxRange || !weaponInfo.overHorizon && this.targetObscured || (double) this.aircraft.speed < (double) weaponInfo.targetRequirements.minOwnerSpeed || (double) this.currentTargetTracking.missileAttacks > (double) currentWeaponStation.WeaponInfo.CalcAttacksNeeded(this.currentTarget))
      return false;
    if (Physics.Linecast(this.aircraft.transform.position, this.aircraft.transform.position + this.aircraft.rb.velocity * 4f, 64 /*0x40*/))
    {
      this.breakOffTimer = 4f;
      return false;
    }
    this.followTerrain = (double) this.targetAngle > 20.0;
    this.stateDisplayName = "using missiles";
    if (currentWeaponStation.SalvoInProgress)
      return true;
    if ((double) this.targetAngle < (double) weaponInfo.targetRequirements.minAlignment && (double) Time.timeSinceLevelLoad - (double) this.lastFiredTime > 2.5 && this.aircraft.NetworkHQ.IsTargetPositionAccurate(this.currentTarget, 1000f))
    {
      this.aircraft.weaponManager.ClearTargetList();
      if (CombatAI.LookForMissileTargets(this.aircraft, this.currentTarget, currentWeaponStation) > 0)
      {
        this.pilot.Fire();
        this.lastTargetAssessTime = Time.timeSinceLevelLoad - 2f;
        this.lastFiredTime = Time.timeSinceLevelLoad;
        if (currentWeaponStation.Ammo <= 0 || (double) this.targetDist < (double) weaponInfo.targetRequirements.minRange * 2.0)
          this.breakOffTimer = 5f;
      }
    }
    if ((UnityEngine.Object) this.modeDebugRenderer != (UnityEngine.Object) null)
      this.modeDebugRenderer.material.SetColor("_EmissionColor", Color.cyan);
    return true;
  }

  private void CombatModes()
  {
    this.aimEffort = 0.5f;
    this.ignoreCollision = false;
    this.targetVel = Vector3.zero;
    this.aimTargetVel = Vector3.zero;
    this.aimVelocity = true;
    this.followTerrain = true;
    this.controlInputs.throttle = (double) this.targetAngle <= 45.0 || (double) this.targetDist >= (double) this.aircraftParameters.turningRadius * 2.0 || (double) this.aircraft.speed <= (double) this.aircraftParameters.cornerSpeed + 27.0 ? 1f : 0.0f;
    this.FindNearestAirbase();
    this.CountermeasureMissiles();
    this.AssessHQTargets();
    WeaponInfo weaponInfo = this.aircraft.weaponManager.currentWeaponStation != null ? this.aircraft.weaponManager.currentWeaponStation.WeaponInfo : (WeaponInfo) null;
    this.ManageAltitude(weaponInfo);
    if ((double) Time.timeSinceLevelLoad - (double) this.lastGunFiredTime < 0.5 && (UnityEngine.Object) weaponInfo != (UnityEngine.Object) null && weaponInfo.gun)
      this.pilot.Fire();
    if (this.EvadingMissiles() || this.NoTarget())
      return;
    GlobalPosition knownPosition;
    if (this.aircraft.NetworkHQ.TryGetKnownPosition(this.currentTarget, out knownPosition))
      this.destination = knownPosition;
    if (this.BreakOffAttack() || this.Bombing(weaponInfo) || this.RetreatStandoff(weaponInfo))
      return;
    if (this.Strafing(weaponInfo))
    {
      this.ignoreCollision = false;
    }
    else
    {
      if (this.UsingEnergyWeapon(weaponInfo) || this.UsingMissiles(weaponInfo))
        return;
      this.aimEffort = 0.9f;
      this.stateDisplayName = "flying to target";
      this.AvoidThreats();
      if (!((UnityEngine.Object) this.modeDebugRenderer != (UnityEngine.Object) null))
        return;
      this.modeDebugRenderer.material.SetColor("_EmissionColor", Color.green);
    }
  }

  private void EjectionCheck()
  {
    if ((double) Time.timeSinceLevelLoad - (double) this.lastEjectionCheck < 5.0)
      return;
    this.lastEjectionCheck = Time.timeSinceLevelLoad;
    bool flag = false;
    if ((double) this.aircraft.cockpit.xform.position.y < (double) Datum.LocalSeaY)
      flag = true;
    if ((double) this.aircraft.speed < 1.0 && (double) this.aircraft.radarAlt < 1.0)
      flag = true;
    if ((double) this.aircraft.radarAlt > 40.0 && ((double) Vector3.Dot(this.aircraft.cockpit.xform.forward, this.aircraft.rb.velocity) < 0.0 || (double) this.aircraft.partDamageTracker.GetDetachedRatio() > 0.11999999731779099))
      flag = true;
    if (!flag)
      return;
    this.pilot.aircraft.StartEjectionSequence();
  }

  public override void FixedUpdateState(Pilot pilot)
  {
    using (AIPilotCombatModes.fixedUpdateStateMarker.Auto())
    {
      if (!this.fuelChecker.HasEnoughFuel())
        pilot.SwitchState((PilotBaseState) pilot.AILandingState);
      this.CombatModes();
      if (pilot.aircraft.countermeasureTrigger && this.countermeasureType == "IR")
        this.controlInputs.throttle = 0.0f;
      if ((double) Vector3.Angle(this.aircraft.rb.velocity, Vector3.down) < 45.0 && (double) this.aircraft.speed > (double) this.aircraftParameters.cornerSpeed)
        this.controlInputs.throttle = 0.0f;
      if ((double) Time.timeSinceLevelLoad - (double) this.lastFiredTime < 1.0 || (double) Time.timeSinceLevelLoad - (double) this.bombLastDropped < 4.0)
      {
        this.destination = this.aircraft.GlobalPosition() + this.aircraft.rb.velocity + Vector3.up * 2f;
        this.followTerrain = false;
      }
      this.targetHeight = Mathf.Clamp(this.targetHeight, this.aircraft.maxRadius, 3000f);
      this.aircraft.autopilot.AutoAim(this.destination, this.aimVelocity, this.ignoreCollision, false, this.aimEffort, 180f, this.followTerrain, this.targetHeight, this.aimTargetVel);
      this.EjectionCheck();
    }
  }

  public override void UpdateState(Pilot pilot)
  {
  }
}

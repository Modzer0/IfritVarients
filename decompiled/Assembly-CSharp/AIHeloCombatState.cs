// Decompiled with JetBrains decompiler
// Type: AIHeloCombatState
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using Unity.Profiling;
using UnityEngine;

#nullable disable
public class AIHeloCombatState : PilotBaseState
{
  private static readonly ProfilerMarker fixedUpdateStateMarker = new ProfilerMarker("AIHeloCombatState.FixedUpdateState");
  private AIHeloCombatState.CombatMode combatMode;
  private RotorShaft rotorShaft;
  private Unit currentTarget;
  private GlobalPosition targetKnownPosition;
  private Vector3 targetVector;
  private Vector3 targetVel;
  private Vector3 missileEvadeVector;
  private Vector3 randomErrorVector;
  private bool lineOfSight;
  private bool tooClose;
  private bool aimBoresight;
  private AircraftParameters aircraftParameters;
  private WeaponInfo currentWeaponInfo;
  private float targetDist;
  private float targetAngle;
  private float timeWithoutTarget;
  private float lastFiredTime;
  private float missileAimTime;
  private float missileReactTime;
  private float notchingCooldown;
  private float breakOffTimer;
  private float desiredHeight;
  private float dangerLevel;
  private float gunshipTime;
  private float bombLastDropped;
  private float lastBombTargetCheck;
  private string countermeasureType;
  private GameObject targetDebug;
  private GameObject aimLeadDebug;

  public AIHeloCombatState(Pilot pilot) => this.Initialize(pilot);

  public override void EnterState(Pilot pilot)
  {
    this.pilot = pilot;
    this.controlInputs = this.aircraft.GetInputs();
    this.aircraft.weaponManager.SetGunsLinked(true);
    this.aircraftParameters = this.aircraft.GetAircraftParameters();
    this.nearestAirbase = this.aircraft.NetworkHQ.GetNearestAirbase(this.aircraft.transform.position);
    this.timeWithoutTarget = 0.0f;
    if (this.aircraft.GetControlsFilter() is HeloControlsFilter controlsFilter)
      this.rotorShaft = controlsFilter.GetRotorShaft();
    this.aircraft.SetFlightAssistToDefault();
    this.aircraft.SetGear(false);
    pilot.On10sCheck += new Action(this.AIHeloCombatState_On10sInterval);
    pilot.On5sCheck += new Action(this.AIHeloCombatState_On5sInterval);
    pilot.On1sCheck += new Action(this.AIHeloCombatState_On1sInterval);
    this.aircraft.GetMissileWarningSystem().onMissileWarning += new Action<MissileWarning.OnMissileWarning>(this.AIHeloCombatState_OnMissileAlert);
    if (!PlayerSettings.debugVis)
      return;
    this.aimLeadDebug = NetworkSceneSingleton<Spawner>.i.SpawnLocal(GameAssets.i.waypointDebug, Datum.origin);
    this.aimLeadDebug.transform.localScale = Vector3.one * 0.1f;
  }

  private void AIHeloCombatState_On10sInterval() => this.FindNearestAirbase();

  private void AIHeloCombatState_On5sInterval()
  {
    this.EjectionCheck();
    this.AssessHQTargets();
  }

  private void AIHeloCombatState_On1sInterval()
  {
    this.Countermeasures();
    this.ChooseCombatMode();
    this.CheckLoS();
    this.ManageAltitude();
  }

  private void AIHeloCombatState_OnMissileAlert(MissileWarning.OnMissileWarning e)
  {
    if ((double) this.missileReactTime > 0.0)
      return;
    float num = (float) ((1.0 + (double) Vector3.Dot(FastMath.NormalizedDirection(this.aircraft.GlobalPosition(), e.missile.GlobalPosition()), -this.aircraft.transform.forward)) * 0.5);
    this.missileReactTime = UnityEngine.Random.Range(0.25f, 2f) * (float) (1.0 + (double) num * 2.0);
  }

  private void NoTarget()
  {
    this.desiredHeight -= 20f * Time.fixedDeltaTime;
    this.timeWithoutTarget += Time.fixedDeltaTime;
    this.targetVel = Vector3.zero;
    GlobalPosition destination;
    if (!this.pilot.flightInfo.EnemyContact && MissionPosition.TryGetClosestPosition((Unit) this.aircraft, out destination))
    {
      this.timeWithoutTarget = 0.0f;
      this.destination = destination;
    }
    else
    {
      if (!((UnityEngine.Object) this.nearestAirbase != (UnityEngine.Object) null))
        return;
      this.destination = this.nearestAirbase.center.position.ToGlobalPosition();
      if ((double) this.timeWithoutTarget <= 15.0 || !FastMath.InRange(this.destination, this.aircraft.GlobalPosition(), 3000f))
        return;
      this.pilot.SwitchState((PilotBaseState) this.pilot.AIHeloLandingState);
    }
  }

  private void BreakOffAttack()
  {
    if ((UnityEngine.Object) this.nearestAirbase != (UnityEngine.Object) null)
    {
      this.destination = this.nearestAirbase.center.transform.position.ToGlobalPosition();
      if (FastMath.InRange(this.nearestAirbase.center.position, this.aircraft.transform.position, 2000f))
        this.tooClose = false;
    }
    this.breakOffTimer -= Time.deltaTime;
  }

  private void EvadeMissiles()
  {
    if ((UnityEngine.Object) this.evadingMissile != (UnityEngine.Object) null)
      this.missileEvadeVector = Vector3.Cross((this.aircraft.GlobalPosition() - this.evadingMissile.GetEvasionPoint()).normalized, Vector3.up);
    if ((double) Vector3.Dot(this.missileEvadeVector, this.aircraft.transform.forward) < 0.0)
      this.missileEvadeVector *= -1f;
    this.desiredHeight -= 40f * Time.fixedDeltaTime;
    if (this.missileWarningSystem.IsWarning() && (this.countermeasureType == "SARH" || this.countermeasureType == "ARH"))
      this.notchingCooldown = 2.5f;
    else
      this.notchingCooldown -= Time.fixedDeltaTime;
    if ((double) this.notchingCooldown > 0.0)
    {
      this.desiredHeight -= 25f * Time.deltaTime;
      bool flag = (double) Vector3.Dot(this.missileEvadeVector, this.aircraft.transform.forward) > 0.5;
      if (!this.aircraft.countermeasureTrigger & flag)
      {
        this.stateDisplayName = "defensive jamming";
        this.aircraft.Countermeasures(true, this.aircraft.countermeasureManager.activeIndex);
      }
      if (this.aircraft.countermeasureTrigger && !flag)
        this.aircraft.Countermeasures(false, this.aircraft.countermeasureManager.activeIndex);
    }
    this.destination = this.aircraft.GlobalPosition() + this.missileEvadeVector * 10000f + this.randomErrorVector * 1000f / this.aircraft.skill;
    if (this.missileWarningSystem.IsWarning() || (double) this.notchingCooldown > 0.0)
      return;
    this.evadingMissile = (Missile) null;
    this.missileReactTime = 0.0f;
    this.ChooseCombatMode();
  }

  private void Countermeasures()
  {
    if (!this.missileWarningSystem.IsWarning())
    {
      if (!this.pilot.aircraft.countermeasureTrigger)
        return;
      this.aircraft.Countermeasures(false, this.aircraft.countermeasureManager.activeIndex);
    }
    else
    {
      if ((double) this.missileReactTime > 0.0)
      {
        --this.missileReactTime;
        if ((double) this.missileReactTime <= 0.0 && this.missileWarningSystem.TryGetNearestIncoming(out this.evadingMissile))
        {
          this.countermeasureType = this.aircraft.countermeasureManager.ChooseCountermeasure(this.evadingMissile);
          this.SetCombatMode(AIHeloCombatState.CombatMode.EvadingMissiles);
        }
      }
      if (!(this.countermeasureType == "IR"))
        return;
      if ((double) this.missileReactTime <= 0.0)
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

  private void CheckLoS()
  {
    if ((UnityEngine.Object) this.currentTarget == (UnityEngine.Object) null)
      return;
    this.lineOfSight = this.currentTarget.LineOfSight(this.aircraft.transform.position - Vector3.up * 5f, 5f);
    this.targetDist = FastMath.Distance(this.targetKnownPosition, this.aircraft.GlobalPosition());
    this.targetVel = (UnityEngine.Object) this.currentTarget.rb != (UnityEngine.Object) null ? this.currentTarget.rb.velocity : Vector3.zero;
    this.targetAngle = Vector3.Angle(this.aircraft.transform.forward, this.targetKnownPosition - this.aircraft.GlobalPosition());
  }

  private void ManageAltitude()
  {
    this.dangerLevel = Mathf.Lerp(this.dangerLevel, 0.0f, 0.02f);
    if ((UnityEngine.Object) this.currentTarget == (UnityEngine.Object) null || (UnityEngine.Object) this.currentWeaponInfo == (UnityEngine.Object) null)
    {
      this.desiredHeight -= 20f;
    }
    else
    {
      float num = this.combatMode == AIHeloCombatState.CombatMode.Bombing || this.combatMode == AIHeloCombatState.CombatMode.GunshipMode ? 0.0f : this.currentWeaponInfo.targetRequirements.minRange + 500f;
      if (!this.tooClose && (double) this.targetDist < (double) num && this.lineOfSight && (UnityEngine.Object) this.nearestAirbase != (UnityEngine.Object) null)
        this.tooClose = true;
      if (this.tooClose && (double) this.targetDist > (double) num)
        this.tooClose = false;
      bool flag1 = (double) this.targetDist < (double) this.currentWeaponInfo.targetRequirements.maxRange + 500.0;
      bool flag2 = false;
      if (((this.tooClose ? 0 : (!this.lineOfSight ? 1 : 0)) & (flag1 ? 1 : 0)) != 0)
        flag2 = true;
      else if ((double) this.currentTarget.radarAlt > 10.0 && (double) this.currentTarget.transform.position.y > (double) this.aircraft.transform.position.y)
        flag2 = true;
      if (this.tooClose)
      {
        this.destination = this.aircraft.GlobalPosition() - this.targetVector.normalized * 10000f;
        this.breakOffTimer = 4f;
        this.SetCombatMode(AIHeloCombatState.CombatMode.BreakOffAttack);
      }
      this.desiredHeight += flag2 ? 10f : -20f;
    }
  }

  private void GunshipMode()
  {
    float num1 = this.currentWeaponInfo.targetRequirements.maxRange * 0.66f;
    this.gunshipTime += Time.fixedDeltaTime;
    float num2 = num1 - this.gunshipTime * 5f;
    Vector3 vector3_1 = (this.targetKnownPosition - this.aircraft.GlobalPosition()) with
    {
      y = 0.0f
    };
    Vector3 vector3_2 = Vector3.Cross(vector3_1, Vector3.up);
    Vector3 aimDirection;
    if ((double) this.aircraft.weaponManager.currentWeaponStation.TurretTraverseRange() < 60.0 && this.aircraft.weaponManager.currentWeaponStation.GetFiringConeDirection(out aimDirection, out float _))
    {
      if ((double) Vector3.Dot(aimDirection, this.aircraft.transform.right) < 0.0)
        vector3_2 *= -1f;
    }
    else if ((double) Vector3.Dot(vector3_2, this.aircraft.transform.forward) < (double) Vector3.Dot(-vector3_2, this.aircraft.transform.forward))
      vector3_2 *= -1f;
    float f = (this.targetDist - num2) / num2;
    float maxRadiansDelta;
    if ((double) Mathf.Abs(f) > -0.40000000596046448 && (double) f < 0.40000000596046448)
    {
      maxRadiansDelta = f * 0.5f;
      if (!this.lineOfSight)
        this.desiredHeight += 20f * Time.deltaTime;
    }
    else
      maxRadiansDelta = f * 3f;
    Vector3 vector3_3 = Vector3.RotateTowards(vector3_2, vector3_1, maxRadiansDelta, 1f);
    this.destination = this.aircraft.GlobalPosition() + vector3_3.normalized * 8000f;
  }

  private void Bombing(WeaponInfo weaponInfo)
  {
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
      this.SetCombatMode(AIHeloCombatState.CombatMode.BreakOffAttack);
    }
    else if (!this.lineOfSight)
    {
      this.SetCombatMode(AIHeloCombatState.CombatMode.FlyingToTarget);
    }
    else
    {
      float initialHeight = this.aircraft.transform.position.y - (this.currentTarget.transform.position.y + weaponInfo.airburstHeight);
      Vector3 vector3 = new Vector3(this.currentTarget.transform.position.x - this.aircraft.transform.position.x, 0.0f, this.currentTarget.transform.position.z - this.aircraft.transform.position.z);
      float magnitude = vector3.magnitude;
      float y = this.aircraft.rb.velocity.y;
      float num1 = Kinematics.FallTime(initialHeight, y);
      float num2 = Mathf.Max(Vector3.Dot(this.aircraft.rb.velocity, vector3.normalized), 1f);
      double num3 = (double) magnitude / (double) num2;
      if (num3 < 10.0 && (double) num1 < 8.0)
      {
        this.breakOffTimer = 15f;
        this.SetCombatMode(AIHeloCombatState.CombatMode.BreakOffAttack);
      }
      if ((double) Time.timeSinceLevelLoad - (double) this.lastBombTargetCheck > 2.0)
      {
        this.lastBombTargetCheck = Time.timeSinceLevelLoad;
        this.aircraft.weaponManager.ClearTargetList();
        CombatAI.LookForBombingTargets(this.aircraft, this.currentTarget, this.aircraft.weaponManager.currentWeaponStation);
      }
      if ((double) this.currentTarget.transform.position.y + 100.0 > (double) this.aircraft.transform.position.y)
        this.desiredHeight += 50f * Time.deltaTime;
      float f = (float) num3 - num1;
      if ((double) Mathf.Abs(f) < 1.5)
      {
        this.bombLastDropped = Time.timeSinceLevelLoad;
        this.lastFiredTime = Time.timeSinceLevelLoad;
        this.breakOffTimer = 15f;
        this.pilot.Fire();
        this.SetCombatMode(AIHeloCombatState.CombatMode.BreakOffAttack);
      }
      float num4 = this.aircraft.transform.position.GlobalY();
      this.desiredHeight = Mathf.Max(this.desiredHeight, num4 - this.targetKnownPosition.y);
      this.desiredHeight += 30f * Time.deltaTime;
      this.destination = this.currentTarget.GlobalPosition() + this.targetVel * Mathf.Clamp(f, 0.0f, 30f) + Vector3.up * this.desiredHeight;
      if ((double) f >= 15.0 || (double) num4 >= 2000.0)
        return;
      float num5 = Mathf.Max(magnitude * (Mathf.Min(this.aircraft.speed - f * 4f, 300f) / 600f) - this.aircraft.transform.position.y, 0.0f);
      this.destination.y = num4 + num5;
    }
  }

  private void UseMissiles()
  {
    this.destination = this.targetKnownPosition + Vector3.up * this.targetDist * 0.0005f * Mathf.Min(this.currentWeaponInfo.targetRequirements.minAlignment * 0.5f, 30f);
    this.missileAimTime = (double) this.targetAngle < (double) this.currentWeaponInfo.targetRequirements.minAlignment ? this.missileAimTime + Time.deltaTime : 0.0f;
    GlobalPosition knownPosition;
    if (this.aircraft.weaponManager.currentWeaponStation.SalvoInProgress || (double) this.targetAngle >= (double) this.currentWeaponInfo.targetRequirements.minAlignment || (double) Time.timeSinceLevelLoad - (double) this.lastFiredTime <= 2.5 || !this.aircraft.NetworkHQ.TryGetKnownPosition(this.currentTarget, out knownPosition) || !FastMath.InRange(knownPosition, this.currentTarget.GlobalPosition(), 500f))
      return;
    this.aircraft.weaponManager.ClearTargetList();
    if (CombatAI.LookForMissileTargets(this.aircraft, this.currentTarget, this.aircraft.weaponManager.currentWeaponStation) <= 0)
      return;
    this.pilot.Fire();
    this.lastFiredTime = Time.timeSinceLevelLoad;
  }

  private void UseBoresightWeapon()
  {
    float num = TargetCalc.TargetLeadTime(this.currentTarget, this.aircraft.gameObject, this.aircraft.rb, this.currentWeaponInfo.muzzleVelocity, this.currentWeaponInfo.dragCoef, 2);
    Vector3 vector3_1 = ((UnityEngine.Object) this.currentTarget.rb != (UnityEngine.Object) null ? this.currentTarget.rb.velocity : Vector3.zero) - this.aircraft.rb.velocity;
    Vector3 vector3_2 = Vector3.up * 9.81f;
    this.destination = this.currentTarget.GlobalPosition() + vector3_1 * num + vector3_2 * num * num * 0.5f;
    if ((double) Vector3.Angle(this.aircraft.transform.forward, this.destination - this.aircraft.GlobalPosition()) >= 150.0 * (double) this.currentTarget.maxRadius / (double) this.targetDist)
      return;
    this.pilot.Fire();
  }

  private void FlyToTarget()
  {
    this.timeWithoutTarget = 0.0f;
    this.destination = this.targetKnownPosition;
  }

  private void SetCombatMode(AIHeloCombatState.CombatMode newCombatMode)
  {
    if (this.combatMode == newCombatMode)
      return;
    this.combatMode = newCombatMode;
    switch (newCombatMode)
    {
      case AIHeloCombatState.CombatMode.FlyingToTarget:
        this.stateDisplayName = "Flying To Target";
        break;
      case AIHeloCombatState.CombatMode.EvadingMissiles:
        this.stateDisplayName = "Evading Missiles";
        break;
      case AIHeloCombatState.CombatMode.BreakOffAttack:
        this.stateDisplayName = "Breaking Off Attack";
        break;
      case AIHeloCombatState.CombatMode.NoTarget:
        this.stateDisplayName = "No Target";
        break;
      case AIHeloCombatState.CombatMode.Bombing:
        this.stateDisplayName = "Bombing";
        break;
      case AIHeloCombatState.CombatMode.GunshipMode:
        this.stateDisplayName = "Gunship";
        break;
      case AIHeloCombatState.CombatMode.UsingMissiles:
        this.stateDisplayName = "Using Missiles";
        break;
      case AIHeloCombatState.CombatMode.UsingBoresightWeapon:
        this.stateDisplayName = "Using Boresight Weapon";
        break;
    }
    this.aimBoresight = this.combatMode == AIHeloCombatState.CombatMode.UsingMissiles || this.combatMode == AIHeloCombatState.CombatMode.UsingBoresightWeapon;
    if (this.combatMode == AIHeloCombatState.CombatMode.GunshipMode)
      return;
    this.gunshipTime = 0.0f;
  }

  private void ChooseCombatMode()
  {
    if ((UnityEngine.Object) this.evadingMissile != (UnityEngine.Object) null && (double) this.missileReactTime <= 0.0)
      this.SetCombatMode(AIHeloCombatState.CombatMode.EvadingMissiles);
    else if ((double) this.breakOffTimer > 0.0 && (UnityEngine.Object) this.nearestAirbase != (UnityEngine.Object) null)
    {
      this.SetCombatMode(AIHeloCombatState.CombatMode.BreakOffAttack);
    }
    else
    {
      if (!this.fuelChecker.HasEnoughFuel())
        this.pilot.SwitchState((PilotBaseState) this.pilot.AIHeloLandingState);
      if (this.aircraft.weaponStations.Count == 0)
      {
        this.SetCombatMode(AIHeloCombatState.CombatMode.NoTarget);
      }
      else
      {
        bool flag1 = (UnityEngine.Object) this.currentTarget != (UnityEngine.Object) null && this.aircraft.NetworkHQ.TryGetKnownPosition(this.currentTarget, out this.targetKnownPosition);
        WeaponStation currentWeaponStation = this.aircraft.weaponManager.currentWeaponStation;
        this.currentWeaponInfo = currentWeaponStation.WeaponInfo;
        bool flag2 = flag1 && this.aircraft.NetworkHQ.IsTargetPositionAccurate(this.currentTarget, 20f);
        if ((UnityEngine.Object) this.currentTarget == (UnityEngine.Object) null || this.currentTarget.disabled || !flag1 || (UnityEngine.Object) this.currentWeaponInfo == (UnityEngine.Object) null || currentWeaponStation.Ammo == 0)
        {
          this.SetCombatMode(AIHeloCombatState.CombatMode.NoTarget);
        }
        else
        {
          this.currentWeaponInfo = this.aircraft.weaponManager.currentWeaponStation?.WeaponInfo;
          if (this.currentWeaponInfo.bomb && (double) Time.timeSinceLevelLoad - (double) this.bombLastDropped > 10.0)
            this.SetCombatMode(AIHeloCombatState.CombatMode.Bombing);
          else if (((!currentWeaponStation.HasTurret() ? 0 : ((double) currentWeaponStation.TurretTraverseRange() > 15.0 ? 1 : 0)) & (flag2 ? 1 : 0)) != 0 && (double) this.targetDist < (double) this.currentWeaponInfo.targetRequirements.maxRange * 1.5)
            this.SetCombatMode(AIHeloCombatState.CombatMode.GunshipMode);
          else if (((!this.currentWeaponInfo.gun ? 0 : ((double) currentWeaponStation.TurretTraverseRange() < 15.0 ? 1 : 0)) & (flag2 ? 1 : 0)) != 0 && (double) this.targetDist < (double) this.currentWeaponInfo.targetRequirements.maxRange * 1.2000000476837158 && (double) this.targetAngle < 20.0)
          {
            this.SetCombatMode(AIHeloCombatState.CombatMode.UsingBoresightWeapon);
          }
          else
          {
            WeaponInfo weaponInfo = currentWeaponStation.WeaponInfo;
            if (!weaponInfo.bomb && !weaponInfo.boresight && !weaponInfo.gun && (double) this.targetDist > (double) weaponInfo.targetRequirements.minRange && (double) this.targetDist < (double) weaponInfo.targetRequirements.maxRange && this.lineOfSight && (double) this.targetAngle < 25.0)
              this.SetCombatMode(AIHeloCombatState.CombatMode.UsingMissiles);
            else
              this.SetCombatMode(AIHeloCombatState.CombatMode.FlyingToTarget);
          }
        }
      }
    }
  }

  private void RunCombatModes()
  {
    switch (this.combatMode)
    {
      case AIHeloCombatState.CombatMode.FlyingToTarget:
        this.FlyToTarget();
        break;
      case AIHeloCombatState.CombatMode.EvadingMissiles:
        this.EvadeMissiles();
        break;
      case AIHeloCombatState.CombatMode.BreakOffAttack:
        this.BreakOffAttack();
        break;
      case AIHeloCombatState.CombatMode.NoTarget:
        this.NoTarget();
        break;
      case AIHeloCombatState.CombatMode.Bombing:
        this.Bombing(this.currentWeaponInfo);
        break;
      case AIHeloCombatState.CombatMode.GunshipMode:
        this.GunshipMode();
        break;
      case AIHeloCombatState.CombatMode.UsingMissiles:
        this.UseMissiles();
        break;
      case AIHeloCombatState.CombatMode.UsingBoresightWeapon:
        this.UseBoresightWeapon();
        break;
    }
  }

  public override void FixedUpdateState(Pilot pilot)
  {
    using (AIHeloCombatState.fixedUpdateStateMarker.Auto())
    {
      this.RunCombatModes();
      this.desiredHeight = Mathf.Clamp(this.desiredHeight, 0.0f, 1000f);
      float num = this.aircraftParameters.minimumRadarAlt + this.desiredHeight;
      if (this.aimBoresight)
        this.aircraft.autopilot.BoresightAim(this.destination, Mathf.Max(num, this.aircraft.radarAlt + 2f));
      else
        this.aircraft.autopilot.AutoAim(this.destination, num, Vector3.zero, Vector3.zero, true);
    }
  }

  private void AssessHQTargets()
  {
    if (this.aircraft.weaponManager.currentWeaponStation == null || this.aircraft.weaponManager.currentWeaponStation.SalvoInProgress)
      return;
    foreach (WeaponStation weaponStation in this.aircraft.weaponStations)
    {
      if (weaponStation.Cargo && weaponStation.Ammo > 0 && (double) Time.timeSinceLevelLoad - (double) this.pilot.flightInfo.LastCargoDelivery > 15.0)
      {
        if (this.pilot.AIHeloTransportState == null)
          this.pilot.AIHeloTransportState = new AIHeloTransportState(this.aircraft);
        this.pilot.SwitchState((PilotBaseState) this.pilot.AIHeloTransportState);
        return;
      }
    }
    CombatAI.TargetSearchResults targetSearchResults = CombatAI.ChooseHQTarget((Unit) this.aircraft, this.aircraft.bravery, this.aircraft.weaponStations);
    if (!((UnityEngine.Object) targetSearchResults.target != (UnityEngine.Object) this.currentTarget))
      return;
    this.currentTarget = targetSearchResults.target;
    if (this.aircraft.weaponManager.GetTargetList().Count > 0)
      this.aircraft.weaponManager.ClearTargetList();
    if ((UnityEngine.Object) this.currentTarget != (UnityEngine.Object) null)
    {
      this.pilot.flightInfo.EnemyContact = true;
      this.aircraft.weaponManager.currentWeaponStation = targetSearchResults.chosenWeaponStation;
      if (this.combatMode != AIHeloCombatState.CombatMode.GunshipMode)
        this.aircraft.weaponManager.AddTargetList(this.currentTarget);
      this.targetDist = float.MaxValue;
      this.targetAngle = 180f;
    }
    else
      this.SetCombatMode(AIHeloCombatState.CombatMode.NoTarget);
  }

  private void EjectionCheck()
  {
    bool flag = false;
    if ((double) this.aircraft.cockpit.xform.position.y < (double) Datum.LocalSeaY)
      flag = true;
    if ((double) this.aircraft.speed < 1.0 && (double) this.aircraft.radarAlt < 5.0)
      flag = true;
    if ((double) this.aircraft.radarAlt > 40.0 && ((double) this.aircraft.partDamageTracker.GetDetachedRatio() > 0.11999999731779099 || (UnityEngine.Object) this.rotorShaft != (UnityEngine.Object) null && (double) this.rotorShaft.GetRPM() < (double) this.rotorShaft.GetMaxRPM() * 0.30000001192092896))
      flag = true;
    if (!flag)
      return;
    this.aircraft.StartEjectionSequence();
  }

  public override void UpdateState(Pilot pilot)
  {
  }

  public override void LeaveState()
  {
    this.pilot.On10sCheck -= new Action(this.AIHeloCombatState_On10sInterval);
    this.pilot.On5sCheck -= new Action(this.AIHeloCombatState_On5sInterval);
    this.pilot.On1sCheck -= new Action(this.AIHeloCombatState_On1sInterval);
  }

  private enum CombatMode
  {
    FlyingToTarget,
    EvadingMissiles,
    BreakOffAttack,
    NoTarget,
    Bombing,
    GunshipMode,
    UsingMissiles,
    UsingBoresightWeapon,
  }
}

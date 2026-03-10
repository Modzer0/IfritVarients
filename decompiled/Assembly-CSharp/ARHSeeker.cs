// Decompiled with JetBrains decompiler
// Type: ARHSeeker
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;

#nullable disable
public class ARHSeeker : MissileSeeker
{
  [SerializeField]
  private float lockPerseverance = 2f;
  [SerializeField]
  private bool homeOnJam;
  [SerializeField]
  private float homingLockDelay;
  [SerializeField]
  private float minReacquireRange = 2000f;
  [SerializeField]
  private float datalinkPositionalError;
  [SerializeField]
  private float maxTrackingAngle;
  [SerializeField]
  private float armDelay = 1f;
  [SerializeField]
  private float guidanceDelay = 1f;
  [SerializeField]
  private float terminalRange = 12000f;
  [SerializeField]
  private float maxLead = 5f;
  [SerializeField]
  private float selfDestructAtSpeed = 200f;
  [SerializeField]
  private float loftAmount = 0.2f;
  [SerializeField]
  private RadarParams radarParameters;
  [Range(0.0f, 1f)]
  [SerializeField]
  private float jamTolerance;
  private GlobalPosition knownPos;
  private Vector3 knownVel;
  private Vector3 knownVelPrev;
  private Vector3 knownAccel;
  private float lastActiveTrackAttempt;
  private float lastDatalinkTrackAttempt;
  private float timeWithoutReturn;
  private float returnStrength;
  private float homingLockTime;
  private float jamAccumulation;
  private float topSpeed;
  private float targetDist;
  private float timeToTarget;
  private Vector3 positionalErrorVector;
  private bool armed;
  private bool guidance;
  private bool isJammed;
  private bool radarLockEstablished;
  private bool achievedLock;

  public override void Initialize(Unit target, GlobalPosition aimpoint)
  {
    this.missile.NetworkseekerMode = Missile.SeekerMode.passive;
    this.positionalErrorVector = UnityEngine.Random.insideUnitSphere * this.datalinkPositionalError;
    this.missile.onJam += new Action<Unit.JamEventArgs>(this.ARHSeeker_OnJam);
    this.lastActiveTrackAttempt = Time.timeSinceLevelLoad;
    this.topSpeed = this.missile.GetTopSpeed(0.0f, 0.0f);
    this.targetUnit = target;
    this.knownPos = this.missile.GlobalPosition() + this.missile.transform.forward * 100000f;
    this.missile.SetAimpoint(this.knownPos, Vector3.zero);
    this.StartSlowUpdateDelayed(1f, new Action(this.SlowChecks));
  }

  private void SlowChecks()
  {
    if (this.missile.disabled)
      return;
    if ((!this.missile.EngineOn() || (double) this.missile.timeSinceSpawn > 10.0) && (this.missile.LosingGround() || this.missile.MissedTarget() || (double) this.missile.speed < (double) this.selfDestructAtSpeed || (UnityEngine.Object) this.targetUnit == (UnityEngine.Object) null))
      this.missile.Detonate(Vector3.up, false, false);
    if ((double) this.loftAmount <= 0.0)
      return;
    Vector3 vector3 = this.knownPos - this.missile.GlobalPosition();
    float a = Vector3.Dot(vector3.normalized, this.missile.rb.velocity);
    GlobalPosition knownPosition;
    if ((UnityEngine.Object) this.targetUnit != (UnityEngine.Object) null && this.missile.NetworkHQ.TryGetKnownPosition(this.targetUnit, out knownPosition))
      vector3 = knownPosition - this.missile.GlobalPosition();
    this.targetDist = vector3.magnitude;
    this.timeToTarget = this.targetDist / Mathf.Max(a, 10f);
  }

  public override string GetSeekerType() => "ARH";

  private void ARHSeeker_OnJam(Unit.JamEventArgs e)
  {
    Vector3 vector3 = e.jammingUnit.GlobalPosition() - this.missile.GlobalPosition();
    this.jamAccumulation += e.jamAmount;
    this.missile.RecordDamage(e.jammingUnit.persistentID, 0.01f);
    if ((double) this.jamAccumulation < (double) this.jamTolerance || !this.homeOnJam)
      return;
    this.missile.SetTarget(e.jammingUnit);
    this.targetUnit = e.jammingUnit;
    this.knownPos = e.jammingUnit.GlobalPosition();
    this.knownVel = e.jammingUnit.rb.velocity;
    this.radarLockEstablished = false;
  }

  private float GetRadarReturn()
  {
    if ((double) Time.timeSinceLevelLoad - (double) this.lastActiveTrackAttempt < 0.5)
      return this.returnStrength;
    this.lastActiveTrackAttempt = Time.timeSinceLevelLoad;
    if (!(this.targetUnit is IRadarReturn targetUnit) || this.isJammed && !this.homeOnJam)
      return 0.0f;
    GlobalPosition a = this.missile.GlobalPosition();
    GlobalPosition b = this.targetUnit.GlobalPosition();
    Vector3 vector3 = (b - a) with { y = 0.0f };
    float num1 = FastMath.Distance(a, b);
    float magnitude = vector3.magnitude;
    float num2 = Mathf.Sqrt(1.2742E+07f * a.y);
    float num3 = Mathf.Sqrt(1.2742E+07f * b.y);
    if ((double) num2 + (double) num3 < (double) num1 || !TargetCalc.LineOfSight(this.transform, this.targetUnit.transform, 10f))
      return -1f;
    if ((double) num1 > (double) this.radarParameters.maxRange || (double) this.returnStrength < (double) this.radarParameters.minSignal && (double) num1 < (double) this.minReacquireRange || (double) Vector3.Angle(this.transform.forward, this.targetUnit.transform.position - this.transform.position) > (double) this.maxTrackingAngle)
      return 0.0f;
    float num4 = 0.0f;
    if ((double) magnitude < (double) num2 && (double) b.y < (double) a.y * (1.0 - (double) magnitude / (double) num2))
    {
      float num5 = (float) ((double) num1 * (double) this.targetUnit.radarAlt / ((double) a.y - (double) b.y));
      num4 += Mathf.Min(num1, 1000f) / num5;
    }
    float clutter = num4 + (float) ((double) this.targetUnit.maxRadius * (double) this.targetUnit.maxRadius * 2.0 / ((double) this.targetUnit.radarAlt * (double) this.targetUnit.radarAlt));
    return targetUnit.GetRadarReturn(this.missile.transform.position, (Radar) null, (Unit) this.missile, num1, clutter, this.radarParameters, true);
  }

  public override void Seek()
  {
    if (this.missile.targetID.NotValid)
    {
      this.knownPos += this.knownVel * Time.fixedDeltaTime;
      this.missile.SetAimpoint(this.knownPos, Vector3.zero);
    }
    else
    {
      if (!this.armed && (double) this.missile.timeSinceSpawn > (double) this.armDelay)
      {
        this.armed = true;
        this.missile.Arm();
        this.missile.SetTangible(true);
      }
      if (!this.guidance && (double) this.missile.timeSinceSpawn > (double) this.guidanceDelay)
      {
        this.guidance = true;
        this.missile.DeployFins();
      }
      this.jamAccumulation -= Mathf.Max(this.jamAccumulation, 0.2f) * Mathf.Max(this.jamTolerance, 0.1f) * Time.deltaTime;
      this.jamAccumulation = Mathf.Clamp01(this.jamAccumulation);
      this.isJammed = (double) this.jamAccumulation > (double) this.jamTolerance;
      if ((UnityEngine.Object) this.targetUnit == (UnityEngine.Object) null)
      {
        this.missile.SetTarget((Unit) null);
        this.missile.SetAimpoint(this.missile.GlobalPosition() + this.missile.transform.forward * 10000f, Vector3.zero);
      }
      else
      {
        if (!this.radarLockEstablished)
          this.DatalinkMode();
        else
          this.TerminalMode();
        if (!this.guidance)
          return;
        Vector3 platformVel = (double) this.missile.timeSinceSpawn < 3.0 ? this.missile.transform.forward * this.topSpeed : this.missile.rb.velocity;
        Vector3 leadVectorWithAccel = TargetCalc.GetLeadVectorWithAccel(this.knownPos, this.missile.GlobalPosition(), this.knownVel, platformVel, this.knownAccel, this.maxLead);
        if ((double) this.loftAmount > 0.0)
        {
          if ((double) this.missile.timeSinceSpawn < 3.0)
            this.timeToTarget = this.targetDist / this.topSpeed;
          float num = Mathf.Min((float) ((double) this.timeToTarget * (double) this.timeToTarget * 4.90500020980835) * this.loftAmount, this.targetDist * this.loftAmount);
          leadVectorWithAccel += num * Vector3.up;
          this.timeToTarget -= Time.fixedDeltaTime;
        }
        this.missile.SetAimpoint(this.knownPos + leadVectorWithAccel, this.knownVel);
      }
    }
  }

  private void DatalinkMode()
  {
    if ((double) Time.timeSinceLevelLoad - (double) this.lastDatalinkTrackAttempt < 1.0)
      return;
    this.lastDatalinkTrackAttempt = Time.timeSinceLevelLoad;
    if ((double) FastMath.Distance(this.knownPos, this.missile.GlobalPosition()) < (double) this.terminalRange)
    {
      this.returnStrength = this.GetRadarReturn();
      Missile.SeekerMode seekerMode = (double) this.returnStrength > (double) this.radarParameters.minSignal ? Missile.SeekerMode.activeLock : Missile.SeekerMode.activeSearch;
      if (this.missile.seekerMode != seekerMode)
        this.missile.NetworkseekerMode = seekerMode;
    }
    if ((double) this.returnStrength > (double) this.radarParameters.minSignal)
    {
      this.knownPos = this.targetUnit.GlobalPosition();
      this.knownVel = (UnityEngine.Object) this.targetUnit.rb != (UnityEngine.Object) null ? this.targetUnit.rb.velocity : Vector3.zero;
      this.radarLockEstablished = true;
      if (this.achievedLock || !(this.targetUnit is Aircraft targetUnit))
        return;
      targetUnit.RecordDamage(this.missile.ownerID, 1f / 1000f);
      this.achievedLock = true;
    }
    else
    {
      if (this.missile.NetworkHQ.IsTargetBeingTracked(this.targetUnit))
        this.knownVel = (UnityEngine.Object) this.targetUnit.rb != (UnityEngine.Object) null ? this.targetUnit.rb.velocity : Vector3.zero;
      if (this.missile.NetworkHQ.IsTargetPositionAccurate(this.targetUnit, 2000f))
        this.knownPos = this.missile.NetworkHQ.GetKnownPosition(this.targetUnit).Value + this.positionalErrorVector;
      else
        this.knownPos += this.knownVel;
      if (FastMath.InRange(this.knownPos, this.targetUnit.GlobalPosition(), 2000f))
        return;
      this.knownVel = Vector3.zero;
      this.knownPos = this.missile.GlobalPosition() + this.missile.transform.forward * 10000f;
      this.missile.SetTarget((Unit) null);
      this.targetUnit = (Unit) null;
    }
  }

  private void TerminalMode()
  {
    this.returnStrength = this.GetRadarReturn();
    Missile.SeekerMode seekerMode = (double) this.returnStrength > (double) this.radarParameters.minSignal ? Missile.SeekerMode.activeLock : Missile.SeekerMode.activeSearch;
    if (this.missile.seekerMode != seekerMode)
      this.missile.NetworkseekerMode = seekerMode;
    if ((double) this.returnStrength < (double) this.radarParameters.minSignal)
    {
      if ((double) this.returnStrength == -1.0)
        this.missile.SetTarget((Unit) null);
      this.homingLockTime = 0.0f;
      this.timeWithoutReturn += Time.deltaTime;
      GlobalPosition knownPosition;
      if (this.missile.NetworkHQ.TryGetKnownPosition(this.targetUnit, out knownPosition))
        this.knownPos = knownPosition + this.positionalErrorVector;
      if ((double) Vector3.Angle(this.knownPos - this.missile.GlobalPosition(), this.missile.transform.forward) > (double) this.maxTrackingAngle)
        this.knownPos = this.missile.GlobalPosition() + this.missile.transform.forward * 1000f;
      else
        this.knownPos += this.knownVel * Time.fixedDeltaTime;
      if ((double) this.timeWithoutReturn <= (double) this.lockPerseverance)
        return;
      this.missile.SetTarget((Unit) null);
    }
    else
    {
      this.homingLockTime += Time.fixedDeltaTime;
      this.timeWithoutReturn = 0.0f;
      if ((double) this.homingLockTime > (double) this.homingLockDelay)
      {
        this.knownPos = this.targetUnit.GlobalPosition();
        this.knownVel = (UnityEngine.Object) this.targetUnit.rb != (UnityEngine.Object) null ? this.targetUnit.rb.velocity : Vector3.zero;
        this.knownAccel = (this.knownVel - this.knownVelPrev) / Time.fixedDeltaTime;
        this.knownVelPrev = this.knownVel;
      }
      this.missile.SetTarget(this.targetUnit);
    }
  }
}

// Decompiled with JetBrains decompiler
// Type: LaserSeeker
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;
using UnityEngine;

#nullable disable
public class LaserSeeker : MissileSeeker
{
  [SerializeField]
  private float errorRate = 5f;
  [SerializeField]
  private float maxSeekerAngle = 10f;
  [SerializeField]
  private float maxTargetSpeed = 30f;
  [SerializeField]
  private float loftAmount = 0.05f;
  [SerializeField]
  private float selfDestructAtSpeed = 200f;
  [SerializeField]
  private float guidanceDelay = 0.5f;
  private GlobalPosition knownPos;
  private Vector3 knownVel;
  private Vector3 positionError;
  private Vector3 positionErrorChange;
  private bool hasLock;
  private float lastTrack;
  private float timeToTarget;
  private float targetDist;
  private float topSpeed;
  private LaserDesignator laserDesignator;
  private List<Unit> targetList;

  public override void Initialize(Unit target, GlobalPosition aimpoint)
  {
    if (this.missile.owner is Aircraft owner)
    {
      this.laserDesignator = owner.GetLaserDesignator();
      this.targetList = owner.weaponManager.GetTargetList();
    }
    this.topSpeed = this.missile.GetWeaponInfo().GetMaxSpeed();
    this.positionErrorChange = UnityEngine.Random.insideUnitSphere * this.errorRate;
    this.knownPos = this.missile.GlobalPosition() + this.missile.transform.forward * 10000f;
    if (UnitRegistry.TryGetUnit(new PersistentID?(this.missile.targetID), out target) && (UnityEngine.Object) this.missile.NetworkHQ != (UnityEngine.Object) null && this.missile.NetworkHQ.IsTargetLased(target))
    {
      this.lastTrack = -100f;
      this.targetUnit = target;
      this.missile.NetworkseekerMode = Missile.SeekerMode.passive;
    }
    else if ((UnityEngine.Object) this.missile.NetworkHQ != (UnityEngine.Object) null)
    {
      Unit lasedTarget;
      if (this.missile.NetworkHQ.TryGetLasedTargetInView(this.transform, this.maxSeekerAngle, 15000f, out lasedTarget))
        this.targetUnit = lasedTarget;
      this.lastTrack = -100f;
      this.missile.NetworkseekerMode = Missile.SeekerMode.passive;
    }
    this.missile.DeployFins();
    this.missile.SetAimpoint(this.knownPos, this.knownVel);
    this.StartSlowUpdateDelayed(1f, new Action(this.SlowChecks));
  }

  private void SlowChecks()
  {
    if (this.missile.disabled || this.missile.EngineOn() || !this.missile.LosingGround() && !this.missile.MissedTarget() && (double) this.missile.speed >= (double) this.selfDestructAtSpeed)
      return;
    this.missile.Detonate(Vector3.up, false, false);
  }

  public override string GetSeekerType() => "Laser";

  private bool TrackLaser()
  {
    this.lastTrack = Time.timeSinceLevelLoad;
    return !((UnityEngine.Object) this.missile.NetworkHQ == (UnityEngine.Object) null) && this.missile.NetworkHQ.IsTargetLased(this.targetUnit) && (double) Vector3.Angle(this.targetUnit.transform.position - this.transform.position, this.transform.forward) <= (double) this.maxSeekerAngle && this.targetUnit.LineOfSight(this.missile.transform.position, 1000f);
  }

  private void GetTargetParameters()
  {
    if ((UnityEngine.Object) this.targetUnit == (UnityEngine.Object) null)
    {
      this.positionError += this.positionErrorChange * Time.fixedDeltaTime;
    }
    else
    {
      if ((double) Time.timeSinceLevelLoad - (double) this.lastTrack > 0.10000000149011612)
        this.hasLock = this.TrackLaser();
      if (this.hasLock)
      {
        this.positionError = Vector3.zero;
        TargetCalc.GetLeadFromMaxTargetSpeed(this.targetUnit, this.targetUnit.transform, this.transform, this.knownPos, this.maxTargetSpeed, out this.knownPos, out this.knownVel);
      }
      else
      {
        this.knownPos += this.knownVel * Time.fixedDeltaTime;
        this.positionError += this.positionErrorChange * Time.fixedDeltaTime;
      }
    }
  }

  private void SendTargetInfo()
  {
    if (this.hasLock && this.missile.targetID != this.targetUnit.persistentID)
      this.missile.SetTarget(this.targetUnit);
    if (!this.hasLock && this.missile.targetID.IsValid)
      this.missile.SetTarget((Unit) null);
    if ((double) this.missile.timeSinceSpawn < (double) this.guidanceDelay)
      return;
    Vector3 knownVel = this.knownVel;
    Vector3 platformVel = this.missile.EngineOn() ? this.missile.transform.forward * this.topSpeed : this.missile.rb.velocity;
    Vector3 leadVector = TargetCalc.GetLeadVector(this.knownPos, this.missile.GlobalPosition(), knownVel, platformVel, 10f);
    if ((double) this.loftAmount > 0.0 && (UnityEngine.Object) this.targetUnit != (UnityEngine.Object) null)
    {
      this.targetDist = FastMath.Distance(this.knownPos, this.missile.GlobalPosition());
      this.timeToTarget = this.targetDist / Mathf.Max(this.missile.EngineOn() ? this.topSpeed : this.missile.speed, 10f);
      float t = (float) ((double) this.timeToTarget * 0.25 - 0.30000001192092896);
      float a = (float) ((double) this.timeToTarget * (double) this.timeToTarget * 4.90500020980835);
      float b = (float) ((double) this.timeToTarget * (double) this.timeToTarget * 4.90500020980835) * this.loftAmount;
      leadVector += Mathf.Lerp(a, b, t) * Vector3.up;
    }
    this.missile.SetAimpoint(this.knownPos + this.positionError + leadVector, this.knownVel);
  }

  public override void Seek()
  {
    this.GetTargetParameters();
    this.SendTargetInfo();
    if (this.missile.IsTangible() || !((UnityEngine.Object) this.missile.owner != (UnityEngine.Object) null) || FastMath.InRange(this.missile.GlobalPosition(), this.missile.owner.GlobalPosition(), 20f))
      return;
    this.missile.SetTangible(true);
  }
}

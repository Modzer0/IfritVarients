// Decompiled with JetBrains decompiler
// Type: IRSeeker
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;

#nullable disable
public class IRSeeker : MissileSeeker
{
  [SerializeField]
  private float flareRejection;
  private IRSource IRTarget;
  private GlobalPosition knownPos;
  private Vector3 knownVel;
  private Vector3 knownVelPrev;
  private Vector3 knownAccel;
  private Vector3 driftError;
  private Vector3 errorOffset;
  [SerializeField]
  private float positionalError;
  [SerializeField]
  private float driftRate;
  [SerializeField]
  private float guidanceDelay = 0.25f;
  [SerializeField]
  private float tangibleDelay = 0.25f;
  [SerializeField]
  private float selfDestructAtSpeed = 200f;
  [SerializeField]
  private float maxLead = 5f;
  [SerializeField]
  private AnimationCurve rangeFactor;
  private float dazzleAmount;
  private float lastEvaluated;
  private float topSpeed;
  private bool guidance;
  private bool achievedLock;
  private bool targetOnLaunch;

  public override void Initialize(Unit target, GlobalPosition aimpoint)
  {
    this.targetUnit = target;
    this.errorOffset = UnityEngine.Random.insideUnitSphere;
    this.missile.NetworkseekerMode = Missile.SeekerMode.passive;
    this.topSpeed = this.missile.GetWeaponInfo().GetMaxSpeed();
    GlobalPosition? nullable = (UnityEngine.Object) this.targetUnit != (UnityEngine.Object) null ? this.missile.NetworkHQ.GetKnownPosition(this.targetUnit) : new GlobalPosition?();
    this.knownPos = this.missile.GlobalPosition() + this.missile.transform.forward * 10000f;
    this.missile.SetAimpoint(this.knownPos, Vector3.zero);
    if ((UnityEngine.Object) this.targetUnit == (UnityEngine.Object) null || !this.targetUnit.HasIRSignature() || !nullable.HasValue || FastMath.OutOfRange(this.targetUnit.GlobalPosition(), nullable.Value, 500f) || !this.targetUnit.LineOfSight(this.transform.position, 1000f))
    {
      this.LoseLock();
    }
    else
    {
      this.targetOnLaunch = true;
      this.IRTarget = this.targetUnit.GetIRSource();
      if (this.IRTarget.flare)
        this.LoseLock();
      else
        this.targetUnit.onAddIRSource += new Action<IRSource>(this.IRSeeker_OnTargetFlare);
    }
    this.lastEvaluated = Time.timeSinceLevelLoad;
    this.missile.onDisableUnit += new Action<Unit>(this.IRSeeker_OnMissileDestroyed);
    this.StartSlowUpdateDelayed(0.5f, new Action(this.SlowChecks));
  }

  private void SlowChecks()
  {
    if (this.missile.disabled || this.missile.EngineOn() || !this.missile.LosingGround() && !this.missile.MissedTarget() && (double) this.missile.speed >= (double) this.selfDestructAtSpeed && (!this.targetOnLaunch || !((UnityEngine.Object) this.targetUnit == (UnityEngine.Object) null)))
      return;
    this.missile.Detonate(Vector3.up, false, false);
  }

  public override string GetSeekerType() => "IR";

  public override void Seek()
  {
    if (!this.missile.IsTangible() && (double) this.missile.timeSinceSpawn > (double) this.tangibleDelay)
    {
      if ((UnityEngine.Object) this.missile.owner != (UnityEngine.Object) null && (FastMath.InRange(this.missile.owner.GlobalPosition(), this.missile.GlobalPosition(), 50f) || (double) Vector3.Dot(this.missile.owner.GlobalPosition() - this.missile.GlobalPosition(), this.missile.rb.velocity) > 0.0))
        return;
      this.missile.SetTangible(true);
    }
    if (!this.guidance && (double) this.missile.timeSinceSpawn > (double) this.guidanceDelay)
    {
      this.missile.DeployFins();
      this.guidance = true;
    }
    if ((double) Time.timeSinceLevelLoad - (double) this.lastEvaluated > 0.25 && !this.IRLockCheck())
      this.IRTarget = (IRSource) null;
    if (this.guidance)
    {
      if (this.IRTarget != null && (UnityEngine.Object) this.IRTarget.transform != (UnityEngine.Object) null)
      {
        this.knownPos = this.IRTarget.transform.GlobalPosition();
        this.knownVel = !((UnityEngine.Object) this.targetUnit != (UnityEngine.Object) null) || !((UnityEngine.Object) this.targetUnit.rb != (UnityEngine.Object) null) ? Vector3.zero : this.targetUnit.rb.velocity;
        this.knownAccel = (this.knownVel - this.knownVelPrev) / Time.fixedDeltaTime;
        this.knownVelPrev = this.knownVel;
        this.driftError = Vector3.zero;
      }
      else
        this.driftError += UnityEngine.Random.insideUnitSphere * (float) ((double) this.driftRate * (double) Time.deltaTime / 2.0);
    }
    float maxLead = this.maxLead;
    Vector3 platformVel = (double) this.missile.timeSinceSpawn < 3.0 ? this.missile.transform.forward * this.topSpeed : this.missile.rb.velocity;
    this.missile.SetAimpoint(this.knownPos + (TargetCalc.GetLeadVectorWithAccel(this.knownPos, this.missile.GlobalPosition(), this.knownVel, platformVel, this.knownAccel, maxLead) + (this.driftError + this.errorOffset * (this.dazzleAmount + this.positionalError))), this.knownVel);
  }

  private bool IRLockCheck()
  {
    this.lastEvaluated = Time.timeSinceLevelLoad;
    if (this.IRTarget == null || (UnityEngine.Object) this.IRTarget.transform == (UnityEngine.Object) null)
      return false;
    if (Physics.Linecast(this.transform.position, this.IRTarget.transform.position, out RaycastHit _, 64 /*0x40*/))
    {
      this.LoseLock();
      this.IRTarget = (IRSource) null;
      return false;
    }
    if (!this.achievedLock && this.targetUnit is Aircraft targetUnit)
    {
      targetUnit.RecordDamage(this.missile.ownerID, 1f / 1000f);
      this.achievedLock = true;
    }
    return true;
  }

  private void IRSeeker_OnTargetFlare(IRSource source)
  {
    if (this.missile.targetID.NotValid)
      return;
    float num1 = this.RangeCoef(FastMath.Distance(this.transform.position, this.IRTarget.transform.position));
    Vector3 vector3 = FastMath.NormalizedDirection(this.transform.position, this.IRTarget.transform.position);
    Vector3 rhs = FastMath.NormalizedDirection(source.transform.position, this.IRTarget.transform.position);
    float num2 = Mathf.Clamp01(1f - Mathf.Abs(Vector3.Dot(vector3, rhs)));
    float num3 = this.AspectCoef(vector3);
    float num4 = Mathf.Clamp01(this.BackgroundBrightness(vector3)) * 2f;
    float num5 = (float) ((double) this.IRTarget.intensity * (1.0 + (double) num3) / ((double) num1 + (double) num4));
    this.dazzleAmount += (1f + num2) / this.flareRejection;
    if ((double) this.dazzleAmount <= (double) num5)
      return;
    this.LoseLock();
    this.IRTarget = source;
  }

  private float AspectCoef(Vector3 targetVector)
  {
    return (float) ((double) Mathf.Clamp01(Vector3.Dot(-this.IRTarget.transform.forward, targetVector)) * 0.5 + (double) Mathf.Clamp01(Vector3.Dot(this.IRTarget.transform.forward, targetVector)) * 2.0);
  }

  private float RangeCoef(float targetDistance)
  {
    float maxRange = this.missile.GetWeaponInfo().targetRequirements.maxRange;
    return this.rangeFactor.Evaluate(targetDistance / maxRange);
  }

  private float BackgroundBrightness(Vector3 targetVector)
  {
    float cloudOcclusion = NetworkSceneSingleton<LevelInfo>.i.GetCloudOcclusion(this.transform.position);
    float b = NetworkSceneSingleton<LevelInfo>.i.sun.color.b;
    return Mathf.Clamp01(Vector3.Dot(targetVector, -NetworkSceneSingleton<LevelInfo>.i.sun.transform.forward)) * (1f - cloudOcclusion) * b;
  }

  private void LoseLock()
  {
    if ((UnityEngine.Object) this.targetUnit != (UnityEngine.Object) null)
      this.targetUnit.onAddIRSource -= new Action<IRSource>(this.IRSeeker_OnTargetFlare);
    if (this.missile.disabled)
      return;
    this.missile.SetTarget((Unit) null);
  }

  private void IRSeeker_OnMissileDestroyed(Unit unit) => this.LoseLock();
}

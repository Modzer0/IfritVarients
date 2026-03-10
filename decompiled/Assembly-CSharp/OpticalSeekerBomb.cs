// Decompiled with JetBrains decompiler
// Type: OpticalSeekerBomb
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;

#nullable disable
public class OpticalSeekerBomb : MissileSeeker
{
  [SerializeField]
  private float searchRadius;
  [SerializeField]
  private float maxTargetSpeed = 30f;
  [SerializeField]
  private float tangibleDelay = 0.25f;
  [SerializeField]
  private float guidanceDelay = 0.5f;
  [SerializeField]
  private float finDelay = 0.2f;
  [SerializeField]
  private float altitudeFuseHeight;
  [SerializeField]
  private float armDelay;
  private GlobalPosition knownPos;
  private GlobalPosition aimPos;
  private Vector3 knownVel;
  private bool hasVisual;
  private bool guidance;
  private bool deployedFins;
  private bool armTriggered;
  private float lastVisualCheck;
  private float timeToTarget;
  private float trajectoryError;
  private float airburstHeight;
  private Transform targetTransform;
  private GameObject aimpointDebug;

  public override void Initialize(Unit target, GlobalPosition aimpoint)
  {
    float num = Kinematics.FallTime(this.missile.GlobalPosition().y, this.missile.rb.velocity.y);
    Vector3 vector3 = new Vector3(this.missile.rb.velocity.x, 0.0f, this.missile.rb.velocity.z);
    this.aimPos = this.missile.GlobalPosition() + vector3 * num;
    this.aimPos.y = 0.0f;
    this.knownPos = this.aimPos;
    this.missile.NetworkseekerMode = Missile.SeekerMode.passive;
    this.airburstHeight = this.missile.GetWeaponInfo().airburstHeight;
    if (UnitRegistry.TryGetUnit(new PersistentID?(this.missile.targetID), out this.targetUnit))
    {
      this.targetTransform = (double) this.targetUnit.maxRadius > 20.0 ? target.GetRandomPart() : this.targetUnit.transform;
      GlobalPosition knownPosition;
      if (this.missile.NetworkHQ.TryGetKnownPosition(this.targetUnit, out knownPosition))
        this.knownPos = knownPosition;
    }
    this.StartSlowUpdateDelayed(0.5f, new Action(this.SlowChecks));
  }

  private void SlowChecks()
  {
    if (this.missile.disabled)
      return;
    if (!this.missile.IsTangible() && (double) this.missile.timeSinceSpawn > (double) this.tangibleDelay)
      this.missile.SetTangible(true);
    if (this.missile.IsArmed() && (double) this.missile.speed < 30.0 && (double) this.missile.radarAlt < 30.0)
      this.missile.Detonate(Vector3.up, false, true);
    this.missile.UpdateRadarAlt();
    Vector3 vector3 = this.knownPos - this.missile.GlobalPosition();
    float num = Kinematics.FallTime(-vector3.y, this.missile.rb.velocity.y);
    vector3.y = 0.0f;
    float magnitude = vector3.magnitude;
    float a = Vector3.Dot(vector3.normalized, this.missile.rb.velocity);
    this.timeToTarget = Mathf.Max(magnitude, 10f) / Mathf.Max(a, 10f);
    this.trajectoryError = this.timeToTarget / num;
  }

  public override string GetSeekerType() => "Optical";

  private bool TrackVisual()
  {
    this.lastVisualCheck = Time.timeSinceLevelLoad;
    return FastMath.InRange(this.targetUnit.GlobalPosition(), this.knownPos, this.searchRadius + this.targetUnit.maxRadius) && this.targetUnit.LineOfSight(this.transform.position, 1000f);
  }

  private void GetTargetParameters()
  {
    if ((UnityEngine.Object) this.targetUnit == (UnityEngine.Object) null || (UnityEngine.Object) this.targetTransform == (UnityEngine.Object) null)
      return;
    if ((double) Time.timeSinceLevelLoad - (double) this.lastVisualCheck > 0.25)
      this.hasVisual = this.TrackVisual();
    if (this.hasVisual)
    {
      this.knownPos = this.targetTransform.GlobalPosition();
      this.knownVel = (UnityEngine.Object) this.targetUnit.rb != (UnityEngine.Object) null ? this.targetUnit.rb.velocity : Vector3.zero;
    }
    else
      this.knownPos += this.knownVel * Time.fixedDeltaTime;
  }

  private void SendTargetInfo()
  {
    if (this.hasVisual && this.missile.targetID.NotValid)
      this.missile.SetTarget(this.targetUnit);
    if (!this.hasVisual && this.missile.targetID.IsValid)
      this.missile.SetTarget((Unit) null);
    if (!this.guidance)
      return;
    GlobalPosition aimPoint = this.missile.GlobalPosition() + this.missile.rb.velocity * 10000f;
    if ((double) this.missile.rb.velocity.y < 0.0)
    {
      Vector3 targetVel = (double) this.maxTargetSpeed < 1000.0 ? Vector3.ClampMagnitude(this.knownVel, this.maxTargetSpeed) : this.knownVel;
      aimPoint = this.knownPos + TargetCalc.GetLeadVector(this.knownPos, this.missile.GlobalPosition(), targetVel, this.missile.rb.velocity, 10f) + (float) ((double) this.timeToTarget * (double) this.timeToTarget * 4.90500020980835) * this.trajectoryError * Vector3.up;
      if ((double) this.airburstHeight > 0.0 && (double) this.timeToTarget > (double) (this.armDelay - this.missile.timeSinceSpawn))
        aimPoint += Vector3.up * this.airburstHeight;
    }
    this.timeToTarget -= Time.fixedDeltaTime;
    if ((double) this.altitudeFuseHeight > 0.0)
      this.missile.UpdateRadarAlt();
    if (!this.armTriggered && (double) this.missile.timeSinceSpawn > (double) this.armDelay && ((double) this.altitudeFuseHeight == 0.0 || (double) this.missile.radarAlt < (double) this.altitudeFuseHeight))
    {
      this.missile.Arm();
      this.armTriggered = true;
    }
    if (this.armTriggered && (double) this.altitudeFuseHeight > 0.0 && (double) this.missile.radarAlt < (double) this.altitudeFuseHeight)
      this.missile.Detonate(Vector3.up, false, (double) this.missile.radarAlt < 2.0);
    this.missile.SetAimpoint(aimPoint, this.knownVel);
  }

  public override void Seek()
  {
    this.GetTargetParameters();
    this.SendTargetInfo();
    if (!this.deployedFins && (double) this.missile.timeSinceSpawn > (double) this.finDelay)
    {
      this.missile.DeployFins();
      this.deployedFins = true;
    }
    if (!this.missile.IsTangible() && (double) this.missile.timeSinceSpawn > (double) this.tangibleDelay)
      this.missile.SetTangible(true);
    if (this.guidance || (double) this.missile.timeSinceSpawn <= (double) this.guidanceDelay)
      return;
    this.guidance = true;
  }
}

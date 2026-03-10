// Decompiled with JetBrains decompiler
// Type: OpticalSeekerShell
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;

#nullable disable
public class OpticalSeekerShell : MissileSeeker
{
  [SerializeField]
  private float searchRadius = 50f;
  [SerializeField]
  private float maxTargetSpeed = 30f;
  private GlobalPosition knownPos;
  private GlobalPosition aimPos;
  private Vector3 knownVel;
  private bool hasVisual;
  private float lastVisualCheck;
  private float timeToTarget;
  private float trajectoryError;
  private Transform targetTransform;
  private GameObject aimpointDebug;

  public override void Initialize(Unit target, GlobalPosition aimpoint)
  {
    float num = Kinematics.FallTime(this.missile.GlobalPosition().y, this.missile.rb.velocity.y);
    Vector3 vector3 = new Vector3(this.missile.rb.velocity.x, 0.0f, this.missile.rb.velocity.z);
    this.aimPos = this.missile.GlobalPosition() + vector3 * num;
    this.aimPos.y = 0.0f;
    this.knownPos = this.aimPos;
    this.missile.DeployFins();
    this.missile.NetworkseekerMode = Missile.SeekerMode.passive;
    if (UnitRegistry.TryGetUnit(new PersistentID?(this.missile.targetID), out this.targetUnit))
    {
      this.targetTransform = target.GetRandomPart();
      GlobalPosition knownPosition;
      if (this.missile.NetworkHQ.TryGetKnownPosition(this.targetUnit, out knownPosition))
        this.knownPos = knownPosition;
    }
    if (PlayerSettings.debugVis)
    {
      this.aimpointDebug = UnityEngine.Object.Instantiate<GameObject>(GameAssets.i.debugPoint, Datum.origin);
      this.aimpointDebug.transform.localPosition = this.knownPos.AsVector3();
      this.aimpointDebug.transform.localScale = Vector3.one * 3f;
    }
    this.StartSlowUpdateDelayed(0.5f, new Action(this.SlowChecks));
  }

  private void SlowChecks()
  {
    if (this.missile.disabled)
      return;
    if ((double) this.missile.speed < 1.0 && this.missile.IsArmed())
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
    GlobalPosition aimPoint = this.missile.GlobalPosition() + this.missile.rb.velocity * 10000f;
    if ((double) this.missile.rb.velocity.y < 0.0)
    {
      Vector3 targetVel = (double) this.maxTargetSpeed < 1000.0 ? Vector3.ClampMagnitude(this.knownVel, this.maxTargetSpeed) : this.knownVel;
      aimPoint = this.knownPos + TargetCalc.GetLeadVector(this.knownPos, this.missile.GlobalPosition(), targetVel, this.missile.rb.velocity, 10f) + (float) ((double) this.timeToTarget * (double) this.timeToTarget * 4.90500020980835) * this.trajectoryError * Vector3.up;
    }
    if (PlayerSettings.debugVis && (UnityEngine.Object) this.aimpointDebug != (UnityEngine.Object) null)
      this.aimpointDebug.transform.localPosition = aimPoint.AsVector3();
    this.timeToTarget -= Time.fixedDeltaTime;
    if (!this.missile.IsTangible() && (UnityEngine.Object) this.missile.owner != (UnityEngine.Object) null && !FastMath.InRange(this.missile.owner.GlobalPosition(), this.missile.GlobalPosition(), 15f))
      this.missile.SetTangible(true);
    this.missile.SetAimpoint(aimPoint, this.knownVel);
  }

  public override void Seek()
  {
    this.GetTargetParameters();
    this.SendTargetInfo();
  }
}

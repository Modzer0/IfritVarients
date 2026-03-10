// Decompiled with JetBrains decompiler
// Type: SARHSeeker
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;

#nullable disable
public class SARHSeeker : MissileSeeker
{
  [SerializeField]
  private float lockPersistence = 2f;
  [SerializeField]
  private float tangibleDelay = 2f;
  [SerializeField]
  private float armDelay = 2f;
  [SerializeField]
  private float selfDestructAtSpeed = 200f;
  [SerializeField]
  private float seekerAngle = 45f;
  private RadarParams radarParams;
  [Range(0.0f, 1f)]
  [SerializeField]
  private float jamTolerance;
  private Radar radarSource;
  private Transform radarSourcePoint;
  private Transform targetTransform;
  private GlobalPosition knownPos;
  private Vector3 knownVel;
  private Vector3 knownVelPrev;
  private Vector3 knownAccel;
  private float lastTrackingCheck;
  private float timeWithoutTrack;
  private float topSpeed;
  private float trackingStrength;
  private float jamAccumulation;
  private bool isJammed;
  private Unit detectorUnit;

  public override void Initialize(Unit target, GlobalPosition aimpoint)
  {
    this.knownPos = aimpoint;
    Unit unit = (Unit) null;
    if ((UnityEngine.Object) this.missile.owner != (UnityEngine.Object) null)
      unit = this.missile.owner.radar.GetAttachedUnit();
    this.topSpeed = this.missile.GetWeaponInfo().maxSpeed;
    if (!((UnityEngine.Object) unit != (UnityEngine.Object) null))
      return;
    this.radarSource = unit.radar as Radar;
    this.missile.radar = (TargetDetector) this.radarSource;
    this.radarParams = this.radarSource.RadarParameters;
    this.detectorUnit = this.missile.owner;
    if ((UnityEngine.Object) this.radarSource != (UnityEngine.Object) null)
    {
      this.radarSourcePoint = this.radarSource.GetScanPoint();
      this.detectorUnit = this.radarSource.GetAttachedUnit();
    }
    this.missile.onJam += new Action<Unit.JamEventArgs>(this.SARHSeeker_OnJam);
    this.targetUnit = target;
    GlobalPosition knownPosition;
    if ((UnityEngine.Object) this.targetUnit != (UnityEngine.Object) null && (UnityEngine.Object) this.missile.NetworkHQ != (UnityEngine.Object) null && this.missile.NetworkHQ.TryGetKnownPosition(this.targetUnit, out knownPosition))
    {
      this.knownPos = knownPosition;
      this.missile.SetAimpoint(this.knownPos, Vector3.zero);
    }
    this.lastTrackingCheck = Time.timeSinceLevelLoad;
    this.targetTransform = (UnityEngine.Object) target != (UnityEngine.Object) null ? target.GetRandomPart() : (Transform) null;
    this.missile.DeployFins();
    this.StartSlowUpdateDelayed(1f, new Action(this.SlowChecks));
  }

  public override GlobalPosition GetEvasionPoint()
  {
    return (UnityEngine.Object) this.missile.radar != (UnityEngine.Object) null ? this.missile.radar.GetScanPoint().GlobalPosition() : this.missile.GlobalPosition();
  }

  public override string GetSeekerType() => "SARH";

  private void SlowChecks()
  {
    if (this.missile.disabled)
      return;
    if (!this.missile.IsTangible() && (double) this.missile.timeSinceSpawn > (double) this.tangibleDelay)
    {
      this.missile.SetTangible(true);
      if ((UnityEngine.Object) this.radarSource != (UnityEngine.Object) null)
        this.missile.RpcAssignRadar(this.radarSource.GetAttachedUnit());
    }
    if (this.missile.EngineOn() || !this.missile.IsArmed() || !this.missile.LosingGround() && !this.missile.MissedTarget() && (double) this.missile.speed >= (double) this.selfDestructAtSpeed)
      return;
    this.missile.Detonate(Vector3.up, false, false);
  }

  private void SARHSeeker_OnJam(Unit.JamEventArgs e)
  {
    if ((double) Vector3.Angle(e.jammingUnit.GlobalPosition() - this.missile.GlobalPosition(), this.missile.transform.forward) > (double) this.seekerAngle)
      return;
    this.jamAccumulation += e.jamAmount / Mathf.Max(this.jamTolerance, 0.1f);
    this.missile.RecordDamage(e.jammingUnit.persistentID, 0.01f);
  }

  private float GetTrackingStrength(Unit targetUnit)
  {
    if ((double) Time.timeSinceLevelLoad - (double) this.lastTrackingCheck < 0.20000000298023224)
      return this.trackingStrength;
    this.lastTrackingCheck = Time.timeSinceLevelLoad;
    if ((UnityEngine.Object) targetUnit == (UnityEngine.Object) null || !(targetUnit is IRadarReturn radarReturn))
    {
      this.trackingStrength = 0.0f;
      return 0.0f;
    }
    GlobalPosition a = this.radarSourcePoint.GlobalPosition();
    GlobalPosition b = targetUnit.GlobalPosition();
    Vector3 vector3 = (b - a) with { y = 0.0f };
    float num1 = FastMath.Distance(a, b);
    float magnitude = vector3.magnitude;
    float num2 = Mathf.Sqrt(1.2742E+07f * a.y);
    float num3 = Mathf.Sqrt(1.2742E+07f * b.y);
    if (((double) num2 + (double) num3 <= (double) num1 ? 0 : (TargetCalc.LineOfSight(this.radarSourcePoint, targetUnit.transform, 10f) ? 1 : 0)) == 0)
    {
      this.trackingStrength = 0.0f;
      this.missile.SetTarget((Unit) null);
      return 0.0f;
    }
    this.isJammed = (double) this.jamAccumulation > (double) this.jamTolerance || this.radarSource.IsJammed();
    if (this.isJammed || (UnityEngine.Object) this.radarSource == (UnityEngine.Object) null || (UnityEngine.Object) this.radarSourcePoint == (UnityEngine.Object) null || (UnityEngine.Object) this.detectorUnit == (UnityEngine.Object) null || this.detectorUnit.disabled)
    {
      this.trackingStrength = 0.0f;
      return 0.0f;
    }
    float num4 = 0.0f;
    if ((double) magnitude < (double) num2 && (double) b.y < (double) a.y * (1.0 - (double) magnitude / (double) num2))
    {
      float num5 = (float) ((double) num1 * (double) targetUnit.radarAlt / ((double) a.y - (double) b.y));
      num4 += Mathf.Min(num1, 1000f) / num5;
    }
    float clutter = num4 + (float) ((double) targetUnit.maxRadius * (double) targetUnit.maxRadius * 2.0 / ((double) targetUnit.radarAlt * (double) targetUnit.radarAlt));
    this.trackingStrength = radarReturn.GetRadarReturn(this.radarSourcePoint.position, (Radar) null, (Unit) this.missile, num1, clutter, this.radarParams, false);
    return this.trackingStrength;
  }

  private void SearchMode()
  {
    if (this.missile.seekerMode != Missile.SeekerMode.activeSearch)
      this.missile.NetworkseekerMode = Missile.SeekerMode.activeSearch;
    if ((double) this.timeWithoutTrack > (double) this.lockPersistence)
    {
      this.targetTransform = (Transform) null;
      this.missile.SetTarget((Unit) null);
      this.missile.onJam -= new Action<Unit.JamEventArgs>(this.SARHSeeker_OnJam);
    }
    else
    {
      this.timeWithoutTrack += Time.fixedDeltaTime;
      this.knownPos += this.knownVel * Time.fixedDeltaTime;
      this.knownAccel = Vector3.zero;
    }
  }

  private void LockedMode()
  {
    if (this.missile.seekerMode != Missile.SeekerMode.activeLock)
      this.missile.NetworkseekerMode = Missile.SeekerMode.activeLock;
    this.knownPos = this.targetTransform.GlobalPosition();
    this.knownVel = (UnityEngine.Object) this.targetUnit.rb != (UnityEngine.Object) null ? this.targetUnit.rb.velocity : Vector3.zero;
    this.knownAccel = (this.knownVel - this.knownVelPrev) / Time.fixedDeltaTime;
    this.knownVelPrev = this.knownVel;
  }

  private void SendTargetInfo()
  {
    Vector3 platformVel = (double) this.missile.timeSinceSpawn < 3.0 ? this.missile.transform.forward * this.topSpeed : this.missile.rb.velocity;
    this.missile.SetAimpoint(this.knownPos + (this.missile.IsArmed() ? TargetCalc.GetLeadVectorWithAccel(this.knownPos, this.missile.GlobalPosition(), this.knownVel, platformVel, this.knownAccel, 10f) : Vector3.zero), this.knownVel);
  }

  public override void Seek()
  {
    if (!this.missile.IsArmed() && (double) this.missile.timeSinceSpawn > (double) this.armDelay)
      this.missile.Arm();
    if ((UnityEngine.Object) this.targetTransform != (UnityEngine.Object) null && (UnityEngine.Object) this.targetUnit != (UnityEngine.Object) null)
    {
      this.jamAccumulation -= Mathf.Max(this.jamAccumulation, 0.2f) * Mathf.Max(this.jamTolerance, 0.1f) * Time.deltaTime;
      this.jamAccumulation = Mathf.Clamp01(this.jamAccumulation);
      if ((double) this.GetTrackingStrength(this.targetUnit) >= (double) this.radarParams.minSignal)
      {
        this.missile.SetTarget(this.targetUnit);
        this.timeWithoutTrack = 0.0f;
        this.LockedMode();
      }
      else
        this.SearchMode();
    }
    else
    {
      this.knownPos += this.knownVel * Time.fixedDeltaTime;
      if ((UnityEngine.Object) this.targetUnit == (UnityEngine.Object) null && this.missile.IsArmed())
        this.missile.Detonate(Vector3.up, false, false);
    }
    this.SendTargetInfo();
  }
}

// Decompiled with JetBrains decompiler
// Type: ARMSeeker
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;
using UnityEngine;

#nullable disable
public class ARMSeeker : MissileSeeker
{
  [SerializeField]
  private float maxTargetAngle;
  [SerializeField]
  private float inertialDrift;
  [SerializeField]
  private float guidanceDelay = 2f;
  [SerializeField]
  private float finDelay = 0.5f;
  [SerializeField]
  private float tangibleDelay = 2f;
  [SerializeField]
  private float maxTargetSpeed = 100f;
  [SerializeField]
  private float selfDestructAtSpeed = 200f;
  [SerializeField]
  private float loftAmount = 0.2f;
  [SerializeField]
  private JinkEvasion jinkEvasion;
  private GlobalPosition knownPos;
  private Vector3 knownVel;
  private Vector3 targetDrift;
  private List<Radar> recentReturns;
  private float lastEvaluation;
  private float lastLOSCheck;
  private float targetDist;
  private float timeToTarget;
  private Radar targetedRadar;
  private bool guidance;
  private bool finsDeployed;

  public override void Initialize(Unit target, GlobalPosition aimpoint)
  {
    this.recentReturns = new List<Radar>();
    this.lastEvaluation = Time.timeSinceLevelLoad;
    GlobalPosition knownPosition;
    this.knownPos = !((UnityEngine.Object) target != (UnityEngine.Object) null) || !this.missile.NetworkHQ.TryGetKnownPosition(target, out knownPosition) ? this.missile.GlobalPosition() + this.transform.forward * 100000f : knownPosition;
    this.knownVel = !((UnityEngine.Object) target != (UnityEngine.Object) null) || !((UnityEngine.Object) target.rb != (UnityEngine.Object) null) || !this.missile.NetworkHQ.IsTargetBeingTracked(target) ? Vector3.zero : target.rb.velocity;
    if ((UnityEngine.Object) target != (UnityEngine.Object) null)
      this.targetedRadar = target.radar as Radar;
    if ((UnityEngine.Object) this.missile.NetworkHQ != (UnityEngine.Object) null)
      this.missile.onRadarPing += new Action<Aircraft.OnRadarWarning>(this.ARMSeeker_OnRadarPing);
    this.missile.SetAimpoint(this.missile.GlobalPosition() + this.transform.forward * 100000f, Vector3.zero);
    this.StartSlowUpdateDelayed(1f, new Action(this.SlowChecks));
  }

  private void SlowChecks()
  {
    if (this.missile.disabled)
      return;
    if (!this.missile.EngineOn() && (this.missile.LosingGround() || this.missile.MissedTarget() || (double) this.missile.speed < (double) this.selfDestructAtSpeed))
      this.missile.Detonate(Vector3.up, false, false);
    if ((double) this.loftAmount <= 0.0)
      return;
    Vector3 vector3 = this.knownPos - this.missile.GlobalPosition();
    float a = Vector3.Dot(vector3.normalized, this.missile.rb.velocity);
    this.targetDist = vector3.magnitude;
    this.timeToTarget = this.targetDist / Mathf.Max(a, 10f);
  }

  public override string GetSeekerType() => "ARAD";

  public Radar TrackCurrentTarget()
  {
    if ((UnityEngine.Object) this.targetedRadar == (UnityEngine.Object) null || !this.targetedRadar.activated)
      return (Radar) null;
    if ((double) Time.timeSinceLevelLoad - (double) this.lastLOSCheck > 0.25)
    {
      this.lastLOSCheck = Time.timeSinceLevelLoad;
      if (!this.targetedRadar.GetAttachedUnit().LineOfSight(this.transform.position, 10000f))
        return (Radar) null;
    }
    return this.targetedRadar;
  }

  public override void Seek()
  {
    if (!this.missile.LocalSim)
      return;
    if (!this.finsDeployed && (double) this.missile.timeSinceSpawn > (double) this.finDelay)
    {
      this.finsDeployed = true;
      this.missile.DeployFins();
    }
    if (!this.missile.IsTangible() && (double) this.missile.timeSinceSpawn > (double) this.tangibleDelay)
      this.missile.SetTangible(true);
    if (!this.guidance && (double) this.missile.timeSinceSpawn > (double) this.guidanceDelay)
      this.guidance = true;
    Radar targetedRadar = this.targetedRadar;
    this.targetedRadar = this.TrackCurrentTarget();
    if ((double) Time.timeSinceLevelLoad - (double) this.lastEvaluation > 4.5 && (UnityEngine.Object) this.missile.NetworkHQ != (UnityEngine.Object) null)
    {
      if ((UnityEngine.Object) this.targetedRadar == (UnityEngine.Object) null)
        this.targetedRadar = this.EvaluateRadarSources();
      this.recentReturns.Clear();
    }
    if ((UnityEngine.Object) this.targetedRadar == (UnityEngine.Object) null)
    {
      this.targetDrift += UnityEngine.Random.insideUnitSphere * (float) ((double) this.inertialDrift * (double) Time.deltaTime / 2.0);
      this.missile.SetTarget((Unit) null);
    }
    else
    {
      if ((UnityEngine.Object) this.targetedRadar != (UnityEngine.Object) targetedRadar)
        this.missile.SetTarget((UnityEngine.Object) this.targetedRadar != (UnityEngine.Object) null ? this.targetedRadar.GetAttachedUnit() : (Unit) null);
      this.knownPos = this.targetedRadar.GetScanPoint().position.ToGlobalPosition();
      this.knownVel = this.targetedRadar.GetVelocity();
      this.targetDrift = Vector3.zero;
    }
    if (!this.guidance)
      return;
    Vector3 targetVel = (double) this.maxTargetSpeed < 1000.0 ? Vector3.ClampMagnitude(this.knownVel, this.maxTargetSpeed) : this.knownVel;
    Vector3 leadVector = TargetCalc.GetLeadVector(this.knownPos, this.missile.GlobalPosition(), targetVel, this.missile.rb.velocity, 10f);
    if ((double) this.loftAmount > 0.0 && (UnityEngine.Object) this.targetedRadar != (UnityEngine.Object) null)
    {
      float num = Mathf.Min((float) ((double) this.timeToTarget * (double) this.timeToTarget * 4.90500020980835) * this.loftAmount, this.targetDist * this.loftAmount);
      leadVector += num * Vector3.up;
      this.timeToTarget -= Time.fixedDeltaTime;
    }
    if ((double) this.jinkEvasion.amount > 0.0)
      leadVector += this.jinkEvasion.ApplyJink(this.transform.GlobalPosition(), this.knownPos);
    this.missile.SetAimpoint(this.knownPos + this.targetDrift + leadVector, this.knownVel);
  }

  public Radar EvaluateRadarSources()
  {
    this.lastEvaluation = Time.timeSinceLevelLoad;
    Radar radarSources = (Radar) null;
    float num1 = 0.0f;
    if (this.missile.targetID.IsValid)
    {
      TrackingInfo trackingData = this.missile.NetworkHQ.GetTrackingData(this.missile.targetID);
      if (trackingData != null)
        --trackingData.missileAttacks;
    }
    foreach (Radar recentReturn in this.recentReturns)
    {
      if (!((UnityEngine.Object) recentReturn == (UnityEngine.Object) null) && recentReturn.activated)
      {
        float num2 = Vector3.Distance(recentReturn.transform.position, this.missile.transform.position);
        float num3 = Vector3.Angle(recentReturn.transform.position - this.missile.transform.position, this.knownPos - this.missile.GlobalPosition());
        if ((double) num3 <= (double) this.maxTargetAngle)
        {
          float num4 = (float) (10000.0 / ((double) num2 * ((double) num3 + 1.0)));
          TrackingInfo trackingData = this.missile.NetworkHQ.GetTrackingData(recentReturn.GetAttachedUnit().persistentID);
          if (trackingData != null)
            num4 /= (float) ((int) trackingData.missileAttacks + 1);
          if ((double) num4 > (double) num1)
          {
            num1 = num4;
            radarSources = recentReturn;
          }
        }
      }
    }
    if (this.missile.targetID.IsValid)
    {
      TrackingInfo trackingData = this.missile.NetworkHQ.GetTrackingData(this.missile.targetID);
      if (trackingData != null)
        ++trackingData.missileAttacks;
    }
    return radarSources;
  }

  public void ARMSeeker_OnRadarPing(Aircraft.OnRadarWarning e)
  {
    Vector3 vector3 = (UnityEngine.Object) e.emitter.rb != (UnityEngine.Object) null ? e.emitter.rb.velocity : Vector3.zero;
    if ((double) Vector3.Angle(e.emitter.transform.position - this.transform.position, this.missile.rb.velocity - vector3) >= (double) this.maxTargetAngle || this.recentReturns.Contains(e.radar))
      return;
    this.recentReturns.Add(e.radar);
  }

  private struct RadarReturn
  {
    public readonly Unit emitter;
  }
}

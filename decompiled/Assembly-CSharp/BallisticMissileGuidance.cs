// Decompiled with JetBrains decompiler
// Type: BallisticMissileGuidance
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;

#nullable disable
public class BallisticMissileGuidance : MissileSeeker
{
  [SerializeField]
  private float airburstHeight;
  [SerializeField]
  private float armDelay = 2f;
  [SerializeField]
  private float tangibleDelay = 2f;
  [SerializeField]
  private float circularError;
  [SerializeField]
  private float maxTargetSpeed = 20f;
  [SerializeField]
  private BallisticMissileGuidance.RCS rcs;
  [SerializeField]
  private PIDFactors trajectoryPIDFactors;
  private GlobalPosition knownPos;
  private PID trajectoryPID;
  private Vector3 knownVel;
  private Vector3 errorOffset;
  private Vector3 desiredVector;
  private float lastTargetUpdate;
  private float launchTime;
  private float targetAngle;

  private void Awake()
  {
    this.launchTime = Time.timeSinceLevelLoad;
    this.errorOffset = UnityEngine.Random.insideUnitSphere * this.circularError;
    this.trajectoryPID = new PID(this.trajectoryPIDFactors);
  }

  public override void Initialize(Unit target, GlobalPosition aimpoint)
  {
    this.missile.NetworkseekerMode = Missile.SeekerMode.passive;
    this.targetAngle = 45f;
    if (UnitRegistry.TryGetUnit(new PersistentID?(this.missile.targetID), out target))
    {
      this.targetUnit = target;
      this.knownPos = this.missile.NetworkHQ.GetKnownPosition(target) ?? this.missile.GlobalPosition() + this.missile.transform.forward * 10000f;
      this.knownVel = (UnityEngine.Object) target.rb != (UnityEngine.Object) null ? Vector3.ClampMagnitude(target.rb.velocity, this.maxTargetSpeed) : Vector3.zero;
    }
    else
    {
      Vector3 forward = this.missile.transform.forward with
      {
        y = 0.0f
      };
      this.knownPos = this.missile.GlobalPosition() + forward.normalized * 100000f;
      this.knownPos.y = 0.0f;
    }
    double magnitude = (double) ((this.knownPos - this.missile.GlobalPosition()) with
    {
      y = 0.0f
    }).magnitude;
    float remainingDeltaV = this.missile.GetRemainingDeltaV();
    float num = (float) ((double) Mathf.Asin((float) (magnitude * 9.8100004196167 / ((double) remainingDeltaV * (double) remainingDeltaV))) * 0.5 * 57.295780181884766);
    this.targetAngle = (double) num > 45.0 ? num : 90f - num;
    if (float.IsNaN(this.targetAngle))
      this.targetAngle = 45f;
    this.missile.SetAimpoint(this.knownPos, this.knownVel);
    this.StartSlowUpdateDelayed(1f, new Action(this.SlowChecks));
    this.missile.DeployFins();
  }

  private void SlowChecks()
  {
    if (this.missile.disabled)
      return;
    if (!this.missile.IsArmed() && (double) this.missile.timeSinceSpawn > (double) this.armDelay)
      this.missile.Arm();
    if ((double) this.missile.timeSinceSpawn > 4.0 && (double) this.missile.speed < 50.0 && (double) this.missile.GlobalPosition().y < 3000.0)
      this.missile.Detonate(Vector3.up, false, false);
    if (this.missile.IsTangible() || (double) this.missile.timeSinceSpawn <= (double) this.tangibleDelay)
      return;
    this.missile.SetTangible(true);
  }

  public override string GetSeekerType() => "INS";

  private void SetTrajectory()
  {
    if ((double) Time.timeSinceLevelLoad - (double) this.lastTargetUpdate < 0.5)
      return;
    this.lastTargetUpdate = Time.timeSinceLevelLoad;
    if ((UnityEngine.Object) this.targetUnit != (UnityEngine.Object) null && !this.targetUnit.disabled && this.missile.NetworkHQ.TryGetKnownPosition(this.targetUnit, out this.knownPos) && this.missile.NetworkHQ.IsTargetBeingTracked(this.targetUnit))
      this.knownVel = (UnityEngine.Object) this.targetUnit.rb == (UnityEngine.Object) null ? Vector3.zero : this.targetUnit.rb.velocity;
    Vector3 to = this.knownPos - this.missile.GlobalPosition();
    Vector3 current = new Vector3(to.x, 0.0f, to.z);
    float num = current.magnitude / Mathf.Max(Vector3.Dot(this.missile.rb.velocity, current.normalized), 100f);
    GlobalPosition targetPos = this.knownPos + this.knownVel * Kinematics.FallTime(this.missile.GlobalPosition().y - this.knownPos.y, this.missile.rb.velocity.y) + (float) ((double) num * (double) num * 4.90500020980835) * Vector3.up;
    if (this.missile.EngineOn())
    {
      float overshoot;
      this.SimulateTrajectorySimple(out overshoot);
      this.targetAngle += this.trajectoryPID.GetOutput(overshoot * (1f / 1000f), 0.5f);
      this.targetAngle = Mathf.Clamp(this.targetAngle, 45f, 90f);
      if (float.IsNaN(this.targetAngle))
        this.targetAngle = 45f;
      this.desiredVector = Vector3.RotateTowards(current, Vector3.up, this.targetAngle * ((float) Math.PI / 180f), 0.0f);
      targetPos = this.transform.GlobalPosition() + this.desiredVector * 1000f;
    }
    else if ((double) Vector3.Angle(this.transform.forward, to) > 30.0 || (double) this.missile.airDensity < 0.10000000149011612)
      targetPos = this.transform.GlobalPosition() + this.missile.rb.velocity * 10f;
    Vector3 leadVector = TargetCalc.GetLeadVector(targetPos, this.missile.GlobalPosition(), this.knownVel, this.missile.rb.velocity, 30f);
    this.missile.SetAimpoint(targetPos + leadVector + this.errorOffset, this.knownVel);
  }

  private void SimulateTrajectorySimple(out float overshoot)
  {
    Vector3 vector3_1 = this.knownPos - this.missile.GlobalPosition();
    Vector3 vector3_2 = new Vector3(vector3_1.x, 0.0f, vector3_1.z);
    float magnitude1 = vector3_2.magnitude;
    float remainingDeltaV = this.missile.GetRemainingDeltaV();
    float remainingBurnTime = this.missile.GetRemainingBurnTime();
    Vector3 normalized = (vector3_2 + Mathf.Tan(this.targetAngle * ((float) Math.PI / 180f)) * magnitude1 * Vector3.up).normalized;
    if ((double) Vector3.Angle(normalized, this.missile.rb.velocity) > 5.0)
    {
      overshoot = 0.0f;
    }
    else
    {
      Vector3 lhs = this.missile.rb.velocity + remainingDeltaV * normalized;
      GlobalPosition globalPosition = this.missile.GlobalPosition() + remainingBurnTime * 0.5f * (this.missile.rb.velocity + lhs);
      float magnitude2 = new Vector3(this.knownPos.x - globalPosition.x, 0.0f, this.knownPos.z - globalPosition.z).magnitude;
      float num1 = Vector3.Dot(lhs, vector3_2.normalized);
      float initialVerticalVelocity = Vector3.Dot(lhs, Vector3.up);
      float num2 = Kinematics.FallTime(globalPosition.y, initialVerticalVelocity);
      float num3 = magnitude2 / num1;
      overshoot = (num2 - num3) * num1;
    }
  }

  private void Fusing()
  {
    if (!this.missile.IsTangible() || (double) this.airburstHeight <= 0.0 || (double) this.missile.timeSinceSpawn <= 30.0 || this.missile.IsArmed() || (double) this.missile.GlobalPosition().y - (double) this.knownPos.y >= (double) this.airburstHeight)
      return;
    this.missile.Arm();
    this.missile.Detonate(Vector3.up, false, false);
  }

  public override void Seek()
  {
    if (!this.missile.LocalSim)
      return;
    this.SetTrajectory();
    this.rcs.CorrectTrajectory(this.missile.airDensity, this.knownPos, this.knownVel, this.missile.rb);
    this.Fusing();
  }

  [Serializable]
  private class RCS
  {
    public bool enabled;
    private float lastFired;

    public void CorrectTrajectory(
      float airDensity,
      GlobalPosition targetPosition,
      Vector3 targetKnownVel,
      Rigidbody rb)
    {
      if ((double) airDensity > 0.10000000149011612 || (double) Time.timeSinceLevelLoad - (double) this.lastFired < 5.0)
        return;
      this.lastFired = Time.timeSinceLevelLoad;
      float num1 = Kinematics.FallTime(rb.transform.GlobalPosition().y - targetPosition.y, rb.velocity.y);
      targetPosition += num1 * targetKnownVel;
      Vector3 vector3_1 = targetPosition - rb.transform.GlobalPosition();
      Vector3 vector3_2 = new Vector3(vector3_1.x, 0.0f, vector3_1.z);
      Vector3 vector3_3 = new Vector3(rb.velocity.x, 0.0f, rb.velocity.z);
      double num2 = (double) num1;
      Vector3 force = vector3_2 / (float) num2 - vector3_3;
      force.x = Mathf.Clamp(force.x, -1f, 1f);
      force.z = Mathf.Clamp(force.z, -1f, 1f);
      rb.AddForce(force, ForceMode.VelocityChange);
    }
  }
}

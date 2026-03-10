// Decompiled with JetBrains decompiler
// Type: AimSolver
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;

#nullable disable
[Serializable]
public class AimSolver
{
  [SerializeField]
  private bool correctShots;
  [SerializeField]
  private float simulationInterval = 2f;
  [SerializeField]
  private float rakeAmount = 0.1f;
  [SerializeField]
  private float rakeFrequency = 1f;
  private float targetDist;
  private float lastSim;
  private Vector3 targetVelPrev;
  private Vector3 targetAccelSmoothed;
  private Vector3 targetAccelSmoothingVel;
  private Vector3 simCorrection;
  private Vector3 lastSimCorrection;
  private Vector3 simCorrectionRate;
  private Vector3 simCorrectionSmoothed;
  private Vector3 correctionSmoothingVel;
  private Vector3 aimCorrection;
  private Vector3 observedBulletVel;
  private Unit attachedUnit;
  private Unit currentTarget;
  private Transform firingTransform;
  private WeaponInfo weaponInfo;
  private BulletSim.Bullet observedBullet;

  public void SetTarget(
    Unit attachedUnit,
    Unit target,
    Transform firingTransform,
    WeaponInfo weaponInfo)
  {
    this.attachedUnit = attachedUnit;
    this.firingTransform = firingTransform;
    this.weaponInfo = weaponInfo;
    if ((UnityEngine.Object) target == (UnityEngine.Object) this.currentTarget)
      return;
    this.currentTarget = target;
    this.observedBullet = (BulletSim.Bullet) null;
    this.aimCorrection = Vector3.zero;
    this.simCorrection = Vector3.zero;
    this.simCorrectionSmoothed = Vector3.zero;
    this.targetAccelSmoothed = Vector3.zero;
    this.lastSim = 0.0f;
  }

  public void SetObservedBullet(BulletSim.Bullet bullet)
  {
    if (!this.correctShots)
      return;
    this.observedBullet = bullet;
  }

  public void ObserveBullet()
  {
    int num = this.observedBullet.impacted ? 0 : (this.observedBullet.active ? 1 : 0);
    if (num != 0)
      this.observedBulletVel = this.observedBullet.velocity;
    Vector3 vector = this.observedBullet.position - this.currentTarget.GlobalPosition();
    if (num != 0 && (double) Vector3.Dot(this.observedBullet.velocity, -vector) >= 0.0)
      return;
    if ((double) vector.magnitude < (double) Vector3.Distance(this.currentTarget.transform.position, this.firingTransform.position) * 0.5)
    {
      Vector3 forward = Vector3.ProjectOnPlane(vector, this.observedBulletVel);
      this.aimCorrection -= forward * 0.5f;
      if (PlayerSettings.debugVis)
      {
        GameObject gameObject = NetworkSceneSingleton<Spawner>.i.SpawnLocal(GameAssets.i.debugArrowGreen, this.currentTarget.transform);
        gameObject.transform.localPosition = Vector3.zero;
        gameObject.transform.rotation = Quaternion.LookRotation(forward);
        gameObject.transform.localScale = new Vector3(2f, 2f, forward.magnitude);
      }
    }
    else
      this.aimCorrection = Vector3.zero;
    this.observedBullet = (BulletSim.Bullet) null;
  }

  private void RunSim(
    GlobalPosition muzzlePosition,
    GlobalPosition targetPosition,
    Vector3 simpleLead,
    Vector3 targetVel,
    float estimatedTimeToTarget)
  {
    if ((double) Time.timeSinceLevelLoad - (double) this.lastSim < (double) this.simulationInterval)
      return;
    this.lastSim = Time.timeSinceLevelLoad;
    this.simCorrection = -Kinematics.TrajectorySim(this.weaponInfo, ((double) this.attachedUnit.speed > 1.0 ? this.attachedUnit.rb.GetPointVelocity(this.firingTransform.position) : Vector3.zero) + simpleLead.normalized * this.weaponInfo.muzzleVelocity, muzzlePosition, targetPosition, targetVel, this.targetAccelSmoothed, 0.1f, out float _);
    if (!(this.simCorrectionSmoothed == Vector3.zero))
      return;
    this.simCorrectionSmoothed = this.simCorrection;
  }

  public Vector3 GetAimVector(out float targetRange)
  {
    GlobalPosition globalPosition1 = this.firingTransform.GlobalPosition();
    GlobalPosition globalPosition2 = this.currentTarget.GlobalPosition();
    targetRange = FastMath.Distance(globalPosition2, globalPosition1);
    Vector3 rhs = (double) this.currentTarget.speed < 1.0 ? Vector3.zero : this.currentTarget.rb.velocity;
    Vector3 vector3_1 = (double) this.attachedUnit.speed < 1.0 ? Vector3.zero : this.attachedUnit.rb.velocity;
    float num = Vector3.Dot(-(globalPosition2 - globalPosition1).normalized, rhs);
    float estimatedTimeToTarget = targetRange / (this.weaponInfo.GetMaxSpeed() * 0.9f + num);
    if (this.correctShots && this.observedBullet != null)
      this.ObserveBullet();
    if ((double) this.weaponInfo.muzzleVelocity == 0.0)
      return globalPosition2 + estimatedTimeToTarget * rhs - globalPosition1;
    Vector3 target = (rhs - this.targetVelPrev) / Time.fixedDeltaTime;
    this.targetVelPrev = rhs;
    Vector3 targetVel = rhs * (float) (1.0 + (double) Mathf.Cos((float) ((double) Time.timeSinceLevelLoad * 3.1415927410125732 * 2.0) * this.rakeFrequency) * (double) this.rakeAmount);
    this.targetAccelSmoothed = Vector3.SmoothDamp(this.targetAccelSmoothed, target, ref this.targetAccelSmoothingVel, 0.5f);
    Vector3 vector3_2 = estimatedTimeToTarget * targetVel + 0.5f * estimatedTimeToTarget * estimatedTimeToTarget * this.targetAccelSmoothed - estimatedTimeToTarget * vector3_1;
    Vector3 vector3_3 = (float) ((double) estimatedTimeToTarget * (double) estimatedTimeToTarget * 4.90500020980835) * Vector3.up;
    Vector3 simpleLead = globalPosition2 + vector3_2 + vector3_3 - globalPosition1;
    this.RunSim(globalPosition1, globalPosition2, simpleLead, targetVel, estimatedTimeToTarget);
    this.simCorrectionSmoothed = Vector3.SmoothDamp(this.simCorrectionSmoothed, this.simCorrection, ref this.correctionSmoothingVel, 0.15f);
    return simpleLead + this.simCorrection + this.aimCorrection;
  }
}

// Decompiled with JetBrains decompiler
// Type: JammingPod
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.Networking;
using UnityEngine;

#nullable disable
public class JammingPod : Weapon
{
  [SerializeField]
  private float power;
  [SerializeField]
  private float effectiveness;
  [SerializeField]
  private AnimationCurve rangeFalloff;
  [SerializeField]
  private Transform directionTransform;
  private PowerSupply powerSupply;
  private float lastJammingTick;
  private float previousLastFired;
  private float rewardAmount;
  private float rewardCount;
  private float rewardThreshold = 60f;

  public override void AttachToUnit(Unit unit)
  {
    base.AttachToUnit(unit);
    this.lastJammingTick = Time.timeSinceLevelLoad;
    this.powerSupply = unit.GetPowerSupply();
    this.enabled = false;
  }

  public override void SetTarget(Unit target) => this.currentTarget = target;

  public override void Fire(
    Unit owner,
    Unit target,
    Vector3 inheritedVelocity,
    WeaponStation weaponStation,
    GlobalPosition aimpoint)
  {
    int num = this.enabled ? 1 : 0;
    this.enabled = true;
    this.lastFired = Time.timeSinceLevelLoad;
    weaponStation.LastFiredTime = Time.timeSinceLevelLoad;
  }

  private void LateUpdate()
  {
    if ((double) this.lastFired == (double) this.previousLastFired)
      this.enabled = false;
    this.previousLastFired = this.lastFired;
  }

  private void FixedUpdate()
  {
    Vector3 forward = (Object) this.currentTarget != (Object) null ? this.currentTarget.transform.position - this.transform.position : this.transform.forward;
    this.directionTransform.rotation = Quaternion.LookRotation(forward);
    float num1 = this.powerSupply.DrawPower(this.power) / this.power;
    if ((Object) this.currentTarget == (Object) null || !this.attachedUnit.NetworkHQ.IsTargetBeingTracked(this.currentTarget) || Physics.Linecast(this.directionTransform.position, ((Object) this.currentTarget.radar != (Object) null ? this.currentTarget.radar.GetScanPoint() : this.currentTarget.transform).position, out RaycastHit _, 64 /*0x40*/))
      return;
    float magnitude = forward.magnitude;
    if ((double) Time.timeSinceLevelLoad - (double) this.lastJammingTick <= 0.20000000298023224 || !NetworkManagerNuclearOption.i.Server.Active)
      return;
    float num2 = this.rangeFalloff.Evaluate(magnitude) * num1;
    this.lastJammingTick = Time.timeSinceLevelLoad;
    this.currentTarget.Jam(new Unit.JamEventArgs()
    {
      jamAmount = num2,
      jammingUnit = this.attachedUnit
    });
    if (!(this.attachedUnit is Aircraft attachedUnit) || !this.currentTarget.HasRadarEmission() || !((Object) attachedUnit.Player != (Object) null) || !((Object) this.currentTarget.NetworkHQ != (Object) null) || !((Object) this.currentTarget.NetworkHQ != (Object) attachedUnit.NetworkHQ))
      return;
    this.rewardCount += num2 * 0.2f;
    this.rewardAmount += 0.0001f * num2 * Mathf.Sqrt(this.currentTarget.definition.value);
    if ((double) this.rewardCount <= (double) this.rewardThreshold)
      return;
    attachedUnit.NetworkHQ.ReportJammingAction(attachedUnit.Player, this.currentTarget, this.rewardAmount);
    this.rewardAmount = 0.0f;
    this.rewardCount = 0.0f;
  }
}

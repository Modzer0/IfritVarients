// Decompiled with JetBrains decompiler
// Type: OpticalSeeker
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

#nullable disable
public class OpticalSeeker : MissileSeeker
{
  [SerializeField]
  private bool useDatalink;
  [SerializeField]
  private bool terrainAvoidance;
  [SerializeField]
  private bool aimVelocity;
  [SerializeField]
  private float searchRadius;
  [SerializeField]
  private float loftAmount;
  [SerializeField]
  private float topAttackAngle;
  [SerializeField]
  private float magnification = 1000f;
  [SerializeField]
  private float searchAngle = 90f;
  [SerializeField]
  private float maxTargetSpeed = 30f;
  [SerializeField]
  private float selfDestructAtSpeed = 200f;
  [SerializeField]
  private float tangibleDelay = 0.25f;
  [SerializeField]
  private float guidanceDelay = 0.5f;
  [SerializeField]
  private float finDelay = 0.2f;
  [SerializeField]
  private float armDelay;
  [SerializeField]
  private float timeFuse;
  [SerializeField]
  private float aimVelocitySpeed = 300f;
  [SerializeField]
  private JinkEvasion jinkEvasion;
  [SerializeField]
  private TopAttack topAttack;
  private GlobalPosition knownPos;
  private Vector3 knownVel;
  private bool hasVisual;
  private bool guidance;
  private bool deployedFins;
  private bool targetObstructed;
  private bool tooFast;
  private bool armTriggered;
  private float lastOpticalCheck;
  private float timeToTarget;
  private float targetDist;
  private float topSpeed;
  private Transform targetTransform;

  public override void Initialize(Unit target, GlobalPosition aimpoint)
  {
    this.knownPos = aimpoint;
    this.topSpeed = this.missile.GetTopSpeed(0.0f, 0.0f);
    this.timeToTarget = 100f;
    if (UnitRegistry.TryGetUnit(new PersistentID?(this.missile.targetID), out target))
    {
      this.lastOpticalCheck = -100f;
      this.targetUnit = target;
      this.targetTransform = (double) this.targetUnit.maxRadius > 20.0 ? target.GetRandomPart() : this.targetUnit.transform;
      this.knownPos = this.missile.NetworkHQ.GetKnownPosition(target) ?? this.missile.GlobalPosition() + this.missile.transform.forward * 10000f;
      this.knownVel = (UnityEngine.Object) target.rb != (UnityEngine.Object) null ? target.rb.velocity : Vector3.zero;
      if ((double) this.topAttack.Amount > 0.0 && ((double) target.definition.armorTier < 5.0 || (double) target.maxRadius > 10.0 || FastMath.InRange(this.knownPos, this.missile.GlobalPosition(), this.topAttack.TooCloseRange)))
        this.topAttack.Amount = 0.0f;
      this.missile.NetworkseekerMode = Missile.SeekerMode.passive;
    }
    else
    {
      this.knownPos = this.missile.GlobalPosition() + this.missile.transform.forward * 100000f;
      this.knownVel = Vector3.zero;
    }
    this.missile.SetAimVelocity(this.aimVelocity);
    if ((double) this.timeFuse > 0.0)
      this.TimeFuse().Forget();
    this.missile.SetAimpoint(this.knownPos, this.knownVel);
    this.StartSlowUpdateDelayed(1f, new Action(this.SlowChecks));
  }

  private async UniTask TimeFuse()
  {
    OpticalSeeker opticalSeeker = this;
    CancellationToken cancel = opticalSeeker.destroyCancellationToken;
    await UniTask.Delay((int) (1000.0 * (double) opticalSeeker.timeFuse));
    if (cancel.IsCancellationRequested)
      cancel = new CancellationToken();
    else if ((UnityEngine.Object) opticalSeeker.missile == (UnityEngine.Object) null)
      cancel = new CancellationToken();
    else if (opticalSeeker.missile.disabled)
    {
      cancel = new CancellationToken();
    }
    else
    {
      opticalSeeker.missile.Arm();
      opticalSeeker.missile.Detonate(Vector3.up, false, false);
      cancel = new CancellationToken();
    }
  }

  private void SlowChecks()
  {
    if (this.missile.disabled)
      return;
    if (!this.missile.IsTangible() && (double) this.missile.timeSinceSpawn > (double) this.tangibleDelay)
      this.missile.SetTangible(true);
    if (!this.missile.EngineOn())
    {
      if (this.missile.LosingGround() || this.missile.MissedTarget() || (double) this.missile.speed < (double) this.selfDestructAtSpeed)
        this.missile.Detonate(Vector3.up, false, false);
      if ((double) this.missile.speed < (double) this.aimVelocitySpeed && (double) this.timeToTarget < 5.0)
        this.loftAmount = 1f;
    }
    if ((double) this.loftAmount <= 0.0 || !((UnityEngine.Object) this.targetUnit != (UnityEngine.Object) null))
      return;
    Vector3 vector3 = (this.knownPos - this.missile.GlobalPosition()) with
    {
      y = 0.0f
    };
    this.targetDist = vector3.magnitude;
    this.timeToTarget = this.targetDist / Mathf.Max(Vector3.Dot(vector3.normalized, this.missile.rb.velocity), 10f);
  }

  public override string GetSeekerType() => "Optical";

  private void OpticalCheck()
  {
    this.lastOpticalCheck = Time.timeSinceLevelLoad;
    if (!this.hasVisual && this.useDatalink && this.missile.NetworkHQ.IsTargetBeingTracked(this.targetUnit))
      this.knownPos = this.targetUnit.GlobalPosition();
    bool flag1 = FastMath.InRange(this.targetUnit.GlobalPosition(), this.knownPos, this.searchRadius);
    if (!flag1)
    {
      this.targetObstructed = false;
      this.hasVisual = false;
    }
    else
    {
      bool flag2 = this.targetUnit.LineOfSight(this.missile.transform.position, 1000f);
      this.hasVisual = flag1 & flag2 && (double) Vector3.Angle(this.targetUnit.transform.position - this.transform.position, this.transform.forward) < (double) this.searchAngle;
      this.targetObstructed = !flag2;
    }
  }

  private void GetTargetParameters()
  {
    if ((UnityEngine.Object) this.targetUnit == (UnityEngine.Object) null || (UnityEngine.Object) this.targetTransform == (UnityEngine.Object) null)
      return;
    if ((double) Time.timeSinceLevelLoad - (double) this.lastOpticalCheck > 0.25)
    {
      this.OpticalCheck();
      this.missile.SetTarget(this.hasVisual ? this.targetUnit : (Unit) null);
    }
    if (this.hasVisual)
      TargetCalc.GetLeadFromMaxTargetSpeed(this.targetUnit, this.targetTransform, this.transform, this.knownPos, this.maxTargetSpeed, out this.knownPos, out this.knownVel);
    else
      this.knownPos += this.knownVel * Time.fixedDeltaTime;
  }

  private void SendTargetInfo()
  {
    if (!this.guidance)
      return;
    Vector3 platformVel = (double) this.missile.timeSinceSpawn < 3.0 ? this.missile.transform.forward * this.topSpeed : this.missile.rb.velocity;
    Vector3 leadVector = TargetCalc.GetLeadVector(this.knownPos, this.missile.GlobalPosition(), this.knownVel, platformVel, 10f);
    if ((double) this.loftAmount > 0.0 && (UnityEngine.Object) this.targetUnit != (UnityEngine.Object) null && !this.topAttack.Active)
    {
      float num = Mathf.Min((float) ((double) this.timeToTarget * (double) this.timeToTarget * 4.90500020980835) * this.loftAmount, this.targetDist * this.loftAmount);
      leadVector += num * Vector3.up;
      this.timeToTarget -= Time.fixedDeltaTime;
    }
    if (this.hasVisual && (double) this.jinkEvasion.amount > 0.0)
      leadVector += this.jinkEvasion.ApplyJink(this.transform.GlobalPosition(), this.knownPos);
    if ((double) this.topAttack.Amount > 0.0)
      leadVector += this.topAttack.ApplyTopAttack(this.missile.GlobalPosition(), this.knownPos, this.missile.speed);
    if (this.terrainAvoidance && this.targetObstructed)
    {
      Vector3 vector3 = FastMath.Direction(this.missile.GlobalPosition(), this.knownPos) with
      {
        y = 0.0f
      };
      this.missile.SetAimpoint(this.knownPos + leadVector + vector3.magnitude * (0.25f * Vector3.up), this.knownVel);
    }
    else
    {
      if (!this.armTriggered && (double) this.missile.timeSinceSpawn > (double) this.armDelay)
      {
        this.missile.Arm();
        this.armTriggered = true;
      }
      this.missile.SetAimpoint(this.knownPos + leadVector, this.knownVel);
    }
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

  public override float GetMinSpeed() => this.selfDestructAtSpeed;
}

// Decompiled with JetBrains decompiler
// Type: OpticalSeekerCruiseMissile
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;

#nullable disable
public class OpticalSeekerCruiseMissile : MissileSeeker
{
  [SerializeField]
  private float armRange;
  [SerializeField]
  private float altitudeTarget;
  [SerializeField]
  private float formationSpacing;
  [SerializeField]
  private float terminalRange = 2000f;
  [SerializeField]
  private float terminalSearchRadius;
  [SerializeField]
  private float finDelay = 1f;
  [SerializeField]
  private float tangibleDelay = 2f;
  [SerializeField]
  private float guidanceDelay = 1f;
  [SerializeField]
  private float maxTargetSpeed;
  [SerializeField]
  private VLSBooster booster;
  private Transform targetPart;
  private bool terminalMode;
  private bool guidance;
  private bool finsDeployed;
  private bool initialBoostMode;
  private GlobalPosition knownPos;
  private Vector3 knownVel;
  private Vector3 terrainClearVector;
  private float lastTerminalCheck;
  private FactionHQ targetHQAtLaunch;
  [SerializeField]
  private JinkEvasion jinkEvasion;
  [SerializeField]
  private TopAttack topAttack;
  [SerializeField]
  private TerminalBoost terminalBoost;

  public override void Initialize(Unit target, GlobalPosition aimpoint)
  {
    this.targetUnit = target;
    this.missile.onDisableUnit += new Action<Unit>(this.OpticalSeekerCruiseMissile_OnMissileDisabled);
    if ((UnityEngine.Object) this.missile.NetworkHQ != (UnityEngine.Object) null)
      this.missile.NetworkHQ.RegisterCruiseMissile(this.missile);
    this.terrainClearVector = this.transform.forward * 1000f;
    Vector3 forward = this.missile.transform.forward with
    {
      y = 0.0f
    };
    if ((UnityEngine.Object) target != (UnityEngine.Object) null && (UnityEngine.Object) this.missile.NetworkHQ != (UnityEngine.Object) null)
    {
      this.targetHQAtLaunch = target.NetworkHQ;
      this.missile.NetworkHQ.TryGetKnownPosition(target, out this.knownPos);
      if ((double) this.topAttack.Amount > 0.0)
      {
        float num = UnityEngine.Random.Range(0.0f, 1f);
        if ((double) target.maxRadius < 20.0 || FastMath.InRange(this.knownPos, this.missile.GlobalPosition(), this.topAttack.TooCloseRange) || (double) num > (double) this.topAttack.probability)
          this.topAttack.Amount = 0.0f;
      }
    }
    else
    {
      this.knownPos = this.missile.GlobalPosition() + forward.normalized * 100000f;
      this.knownVel = Vector3.zero;
    }
    this.missile.SetAimpoint(this.missile.GlobalPosition() + forward.normalized * 10000f, Vector3.zero);
    this.StartSlowUpdateDelayed(1f, new Action(this.SlowChecks));
  }

  private void SlowChecks()
  {
    if (this.missile.disabled)
      return;
    this.missile.UpdateRadarAlt();
    if ((double) this.missile.timeSinceSpawn > 10.0 && (this.missile.LosingGround() || this.missile.MissedTarget() || (UnityEngine.Object) this.targetUnit == (UnityEngine.Object) null || (double) this.missile.speed < 100.0))
      this.missile.Detonate(Vector3.up, false, false);
    if (this.missile.IsTangible() || (double) this.missile.timeSinceSpawn <= 2.0)
      return;
    this.missile.SetTangible(true);
  }

  public override string GetSeekerType() => "INS / Opt.";

  public GlobalPosition TerrainWaypoint(GlobalPosition destination)
  {
    destination.y = Mathf.Max(destination.y, Datum.LocalSeaY + this.altitudeTarget);
    Vector3 vector3_1 = Vector3.RotateTowards(this.missile.rb.velocity.normalized, destination - this.missile.GlobalPosition(), 0.17453292f, 0.0f);
    float num1 = 1f;
    if ((UnityEngine.Object) this.missile.NetworkHQ != (UnityEngine.Object) null)
    {
      foreach (Missile cruiseMissile in this.missile.NetworkHQ.GetCruiseMissiles())
      {
        if (FastMath.InRange(cruiseMissile.GlobalPosition(), this.transform.GlobalPosition(), 5000f))
        {
          Vector3 vector3_2 = this.transform.position - cruiseMissile.transform.position;
          Vector3 normalized = vector3_2.normalized;
          float num2 = Mathf.Min(this.formationSpacing * this.formationSpacing / vector3_2.sqrMagnitude, 0.03f);
          vector3_2.y = Mathf.Max(vector3_2.y, 0.0f);
          vector3_1 += vector3_2.normalized * num2;
          float num3 = Vector3.Dot(-normalized, this.transform.forward);
          num1 += num3 * 0.03f;
        }
      }
    }
    this.missile.SetThrottle(Mathf.Clamp(num1, 0.8f, 1f));
    float num4 = Mathf.Max(this.missile.speed, 100f) * 6f;
    vector3_1.y = 0.0f;
    Vector3 vector3_3 = this.transform.position + vector3_1.normalized * num4;
    RaycastHit hitInfo;
    if (Physics.Linecast(vector3_3 + Vector3.up * 5000f, vector3_3 - Vector3.up * 5000f, out hitInfo, 8256))
    {
      vector3_3 = hitInfo.point;
      vector3_3.y = Mathf.Max(vector3_3.y, Datum.LocalSeaY);
    }
    else
      vector3_3.y = Datum.LocalSeaY;
    vector3_3 += Vector3.up * this.altitudeTarget;
    if ((double) this.missile.radarAlt < (double) this.altitudeTarget)
      vector3_3 += (float) (((double) this.altitudeTarget - (double) this.missile.radarAlt) * 2.0) * Vector3.up;
    Vector3 forward = Vector3.Lerp(this.terrainClearVector, vector3_3 - this.transform.position, 0.2f);
    this.terrainClearVector = Physics.Linecast(this.transform.position - Vector3.up * 2f, this.transform.position + forward, 8256) ? Vector3.Lerp(this.terrainClearVector, Vector3.up * 1000f, 0.1f) : forward;
    this.terrainClearVector.y = Mathf.Max(this.terrainClearVector.y, (float) -((double) (this.transform.position.y - Datum.LocalSeaY) - (double) this.altitudeTarget));
    if (PlayerSettings.debugVis)
    {
      GameObject gameObject1 = UnityEngine.Object.Instantiate<GameObject>(GameAssets.i.waypointDebug, Datum.origin);
      gameObject1.transform.position = this.transform.position + this.terrainClearVector;
      gameObject1.transform.LookAt(this.transform);
      UnityEngine.Object.Destroy((UnityEngine.Object) gameObject1, 0.5f);
      GameObject gameObject2 = UnityEngine.Object.Instantiate<GameObject>(GameAssets.i.debugArrow, this.transform);
      gameObject2.transform.rotation = Quaternion.LookRotation(forward);
      gameObject2.transform.localScale = new Vector3(1f, 1f, forward.magnitude);
      UnityEngine.Object.Destroy((UnityEngine.Object) gameObject2, 0.5f);
      GameObject gameObject3 = UnityEngine.Object.Instantiate<GameObject>(GameAssets.i.debugArrowGreen, this.transform);
      gameObject3.transform.rotation = Quaternion.LookRotation(this.terrainClearVector);
      gameObject3.transform.localScale = new Vector3(1f, 1f, this.terrainClearVector.magnitude);
      UnityEngine.Object.Destroy((UnityEngine.Object) gameObject3, 0.5f);
    }
    return this.missile.GlobalPosition() + this.terrainClearVector;
  }

  private void OpticalSeekerCruiseMissile_OnMissileDisabled(Unit unit)
  {
    this.missile.onDisableUnit -= new Action<Unit>(this.OpticalSeekerCruiseMissile_OnMissileDisabled);
    if (!((UnityEngine.Object) this.missile.NetworkHQ != (UnityEngine.Object) null))
      return;
    this.missile.NetworkHQ.DeregisterCruiseMissile(this.missile);
  }

  public void PreTerminalMode()
  {
    if ((double) Time.timeSinceLevelLoad - (double) this.lastTerminalCheck < 0.5)
      return;
    this.lastTerminalCheck = Time.timeSinceLevelLoad;
    GlobalPosition aimPoint = this.TerrainWaypoint(this.knownPos);
    if (!this.initialBoostMode)
      this.missile.SetAimpoint(aimPoint, Vector3.zero);
    if ((double) this.missile.timeSinceSpawn <= 3.0 || !FastMath.InRange(this.transform.GlobalPosition(), this.knownPos, this.terminalRange))
      return;
    if ((UnityEngine.Object) this.targetUnit != (UnityEngine.Object) null && !this.targetUnit.disabled)
    {
      this.targetPart = this.targetUnit.GetRandomPart();
      this.terminalMode = true;
      this.missile.Arm();
    }
    else
      this.missile.Detonate(Vector3.up, false, false);
  }

  public void TerminalMode()
  {
    if ((UnityEngine.Object) this.targetUnit == (UnityEngine.Object) null)
    {
      this.missile.Detonate(Vector3.up, false, false);
    }
    else
    {
      float magnitude = ((this.knownPos - this.missile.GlobalPosition()) with
      {
        y = 0.0f
      }).magnitude;
      if (this.targetUnit.LineOfSight(this.transform.position, 1000f))
      {
        if ((double) magnitude < (double) this.armRange)
          this.missile.Arm();
        TargetCalc.GetLeadFromMaxTargetSpeed(this.targetUnit, this.targetPart, this.transform, this.knownPos, this.maxTargetSpeed, out this.knownPos, out this.knownVel);
        GlobalPosition knownPos = this.knownPos;
        if ((double) this.jinkEvasion.amount > 0.0)
        {
          Vector3 vector3 = this.jinkEvasion.ApplyJink(this.transform.GlobalPosition(), this.targetUnit.GlobalPosition());
          vector3.y = Mathf.Max(vector3.y, 0.0f);
          knownPos += vector3;
        }
        if ((double) this.topAttack.Amount > 0.0)
          knownPos += this.topAttack.ApplyTopAttack(this.missile.GlobalPosition(), this.knownPos, this.missile.speed);
        if ((double) this.terminalBoost.Amount > 0.0)
          this.terminalBoost.ApplyTerminalBoost(this.missile, this.missile.GlobalPosition(), this.knownPos);
        this.missile.SetAimpoint(knownPos, this.knownVel);
      }
      else
        this.missile.SetAimpoint(this.knownPos + Vector3.up * magnitude * 0.5f, this.knownVel);
    }
  }

  public override void Seek()
  {
    this.initialBoostMode = false;
    if ((double) this.missile.timeSinceSpawn < 10.0)
    {
      if (!this.finsDeployed && (double) this.missile.timeSinceSpawn > (double) this.finDelay)
      {
        this.missile.DeployFins();
        this.finsDeployed = true;
      }
      if ((UnityEngine.Object) this.booster != (UnityEngine.Object) null && (UnityEngine.Object) this.targetUnit != (UnityEngine.Object) null)
      {
        this.guidance = true;
        this.missile.SetAimpoint(this.knownPos, Vector3.zero);
        this.initialBoostMode = true;
      }
    }
    if ((UnityEngine.Object) this.targetUnit != (UnityEngine.Object) null && !this.targetUnit.disabled)
    {
      if ((UnityEngine.Object) this.targetUnit.NetworkHQ != (UnityEngine.Object) this.targetHQAtLaunch && (UnityEngine.Object) this.targetUnit.NetworkHQ == (UnityEngine.Object) this.missile.NetworkHQ && (double) this.missile.timeSinceSpawn > 10.0)
        this.missile.Detonate(Vector3.up, false, false);
      GlobalPosition? knownPosition = this.missile.NetworkHQ.GetKnownPosition(this.targetUnit);
      if (knownPosition.HasValue)
        this.knownPos = knownPosition.Value;
    }
    if (!this.guidance)
    {
      this.missile.SetAimpoint(this.missile.GlobalPosition() + this.missile.rb.velocity.normalized * 100000f, Vector3.zero);
      this.guidance = (double) this.missile.timeSinceSpawn > (double) this.guidanceDelay;
    }
    else if (!this.terminalMode)
      this.PreTerminalMode();
    else
      this.TerminalMode();
  }
}

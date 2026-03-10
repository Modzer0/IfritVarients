// Decompiled with JetBrains decompiler
// Type: TargetDetector
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Cysharp.Threading.Tasks;
using NuclearOption.Jobs;
using NuclearOption.Networking;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

#nullable disable
public class TargetDetector : MonoBehaviour
{
  [SerializeField]
  protected Unit attachedUnit;
  [SerializeField]
  protected Transform scanner;
  [SerializeField]
  protected UnitPart part;
  [Tooltip("in seconds")]
  [SerializeField]
  private float checkInterval;
  [Tooltip("in seconds")]
  [SerializeField]
  private float alertCheckInterval;
  [SerializeField]
  protected float visualRange;
  [SerializeField]
  protected float magnification;
  [SerializeField]
  protected float maxSpeed;
  [SerializeField]
  protected TargetDetector.Rotator[] rotators;
  [SerializeField]
  protected bool shared;
  [HideInInspector]
  public bool activated;
  protected List<Unit> unitsInRange = new List<Unit>(100);
  public List<Unit> detectedTargets = new List<Unit>();
  protected bool disabled;
  private float rewardAmount;
  private float rewardCount;
  private float rewardThreshold = 1f;

  public event Action onScan;

  public event Action<Unit> onDetectTarget;

  public event Action<TargetDetector> onDisabled;

  protected virtual void OnDestroy()
  {
    this.onDetectTarget = (Action<Unit>) null;
    this.onDisabled = (Action<TargetDetector>) null;
    this.unitsInRange.Clear();
    this.detectedTargets.Clear();
  }

  public Transform GetScanPoint() => this.scanner;

  public virtual float GetRadarRange() => 0.0f;

  public float GetVisualRange() => this.visualRange;

  public float GetVisualMagnification() => this.magnification;

  public Unit GetAttachedUnit() => this.attachedUnit;

  public bool IsOperational() => !this.disabled;

  public Vector3 GetVelocity()
  {
    return (UnityEngine.Object) this.part == (UnityEngine.Object) null || !((UnityEngine.Object) this.part.rb != (UnityEngine.Object) null) ? Vector3.zero : this.part.rb.velocity;
  }

  public void SetAttachedUnit(Unit unit) => this.attachedUnit = unit;

  protected virtual void Awake()
  {
    this.enabled = false;
    this.attachedUnit.onInitialize += new Action(this.TargetDetector_OnInitialize);
    if ((UnityEngine.Object) this.part != (UnityEngine.Object) null)
      this.part.onApplyDamage += new Action<UnitPart.OnApplyDamage>(this.TargetDetector_OnApplyDamage);
    this.attachedUnit.onDisableUnit += new Action<Unit>(this.TargetDetector_OnUnitDisabled);
  }

  protected void TargetDetector_OnInitialize()
  {
    this.activated = true;
    if (this.attachedUnit.IsServer || GameManager.IsLocalAircraft(this.attachedUnit))
      this.RepeatSearch().Forget();
    if (!this.shared || !((UnityEngine.Object) this.attachedUnit.NetworkHQ != (UnityEngine.Object) null) || !(this is Radar radar))
      return;
    this.attachedUnit.NetworkHQ.RegisterRadar(radar);
  }

  protected virtual void TargetDetector_OnApplyDamage(UnitPart.OnApplyDamage e)
  {
  }

  protected void DisableTargetDetector()
  {
    this.disabled = true;
    Action<TargetDetector> onDisabled = this.onDisabled;
    if (onDisabled != null)
      onDisabled(this);
    UnityEngine.Object.Destroy((UnityEngine.Object) this);
  }

  protected virtual void TargetDetector_OnUnitDisabled(Unit unit)
  {
    this.DisableTargetDetector();
    this.attachedUnit.onDisableUnit -= new Action<Unit>(this.TargetDetector_OnUnitDisabled);
    UnityEngine.Object.Destroy((UnityEngine.Object) this);
  }

  private async UniTask RepeatSearch()
  {
    TargetDetector targetDetector = this;
    CancellationToken cancel = targetDetector.destroyCancellationToken;
    await UniTask.Delay((int) ((double) UnityEngine.Random.value * (double) targetDetector.checkInterval * 1000.0));
    while (!cancel.IsCancellationRequested)
    {
      if (targetDetector.activated && (UnityEngine.Object) targetDetector.scanner != (UnityEngine.Object) null && !targetDetector.attachedUnit.disabled && (UnityEngine.Object) targetDetector.attachedUnit.NetworkHQ != (UnityEngine.Object) null)
      {
        targetDetector.detectedTargets.Clear();
        targetDetector.TargetSearch();
        Action onScan = targetDetector.onScan;
        if (onScan != null)
          onScan();
      }
      await UniTask.Delay((int) (((double) targetDetector.alertCheckInterval == 0.0 || targetDetector.detectedTargets.Count == 0 ? (double) targetDetector.checkInterval : (double) targetDetector.alertCheckInterval) * 1000.0));
    }
    cancel = new CancellationToken();
  }

  protected virtual void TargetSearch() => this.VisualCheck();

  public void DetectTarget(Unit target)
  {
    this.detectedTargets.Add(target);
    Action<Unit> onDetectTarget = this.onDetectTarget;
    if (onDetectTarget != null)
      onDetectTarget(target);
    if (!NetworkManagerNuclearOption.i.Server.Active)
      return;
    if (this.attachedUnit is Aircraft attachedUnit && (UnityEngine.Object) attachedUnit.Player != (UnityEngine.Object) null && (UnityEngine.Object) target.NetworkHQ != (UnityEngine.Object) null && (UnityEngine.Object) target.NetworkHQ != (UnityEngine.Object) attachedUnit.NetworkHQ)
    {
      float num = 0.0f;
      if (!attachedUnit.NetworkHQ.trackingDatabase.ContainsKey(target.persistentID))
        num = 0.05f;
      else if (!attachedUnit.NetworkHQ.IsTargetPositionAccurate(target, 500f))
        num = 0.01f;
      if (target is Missile missile && (double) missile.definition.value < 25.0)
        num = 0.0f;
      this.rewardCount += num * Mathf.Sqrt(target.definition.value);
      this.rewardAmount += num * Mathf.Sqrt(target.definition.value);
      if ((double) this.rewardCount > (double) this.rewardThreshold)
      {
        attachedUnit.NetworkHQ.ReportReconAction(attachedUnit.Player, this.rewardAmount);
        this.rewardAmount = 0.0f;
        this.rewardCount = 0.0f;
      }
    }
    this.attachedUnit.NetworkHQ.RpcUpdateTrackingInfo(target.persistentID);
  }

  public bool InVisualRange(Unit target)
  {
    return FastMath.InRange(this.scanner.GlobalPosition(), target.GlobalPosition(), target.GetVisibility() * this.magnification);
  }

  protected void VisualCheck()
  {
    BattlefieldGrid.GetUnitsInRangeNonAlloc(this.scanner.GlobalPosition(), this.visualRange, this.unitsInRange);
    foreach (Unit target in this.unitsInRange)
    {
      if (!((UnityEngine.Object) target.NetworkHQ == (UnityEngine.Object) this.attachedUnit.NetworkHQ) && !target.disabled && (double) target.speed <= (double) this.maxSpeed && !this.detectedTargets.Contains(target))
        DetectorManager.RequestLoSCheck(this, target);
    }
  }

  [Serializable]
  protected class Rotator
  {
    public Transform transform;
    public Vector3 axis;

    public void Reset() => this.transform.localRotation = Quaternion.identity;
  }
}

// Decompiled with JetBrains decompiler
// Type: FireControl
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

#nullable disable
public class FireControl : MonoBehaviour
{
  [SerializeField]
  private FireControl.TargetAcquisitionMode targetAcquisitionMode;
  private List<WeaponStation> subscribedWeaponStations = new List<WeaponStation>();
  [SerializeField]
  private float targetAssessmentInterval;
  [SerializeField]
  private float salvoInterval;
  [SerializeField]
  private float planningTimePerFire;
  [SerializeField]
  private Unit attachedUnit;
  private float maxRange;
  private float minRange;
  private List<TrackingInfo> allTargets = new List<TrackingInfo>();
  private List<FireControl.FireControlTarget> salvoTargets = new List<FireControl.FireControlTarget>();
  private List<FireControl.ScoredTarget> scoredTargets = new List<FireControl.ScoredTarget>();
  private List<FireControl.QueuedAttack> queuedAttacks = new List<FireControl.QueuedAttack>();
  [SerializeField]
  private Radar radar;
  [SerializeField]
  private List<Turret> turrets = new List<Turret>();
  [SerializeField]
  private List<Turret> availableTurrets = new List<Turret>();
  [SerializeField]
  private FireControl.Deployable[] deployables;
  private WeaponStation weaponStation;
  private float lastTargetSort;
  private bool deployed;
  private List<Missile> currentMissiles = new List<Missile>();
  private bool planningSalvo;
  private Dictionary<WeaponStation, int> stationAmmo = new Dictionary<WeaponStation, int>();

  private void Awake()
  {
    this.attachedUnit.onInitialize += new Action(this.FireControl_OnInitialize);
    this.attachedUnit.onDisableUnit += new Action<Unit>(this.FireControl_OnUnitDisabled);
    if (GameManager.gameState != GameState.Encyclopedia)
      return;
    this.DeployAll().Forget();
  }

  private void FireControl_OnInitialize()
  {
    if (this.attachedUnit is Ship attachedUnit)
      this.planningTimePerFire /= Mathf.Max(attachedUnit.skill, 0.1f);
    if (!this.attachedUnit.IsServer)
      return;
    if (this.targetAcquisitionMode == FireControl.TargetAcquisitionMode.datalink)
    {
      this.StartSlowUpdate(this.targetAssessmentInterval, new Action(this.HQTargetAssessment));
      this.attachedUnit.onRegisterMissile += new Action<Missile>(this.FireControl_OnRegisterMissile);
      this.attachedUnit.onDeregisterMissile += new Action<Missile>(this.FireControl_OnDeregisterMissile);
    }
    if (this.targetAcquisitionMode == FireControl.TargetAcquisitionMode.searchForRadar && (UnityEngine.Object) this.attachedUnit.NetworkHQ != (UnityEngine.Object) null)
    {
      this.attachedUnit.NetworkHQ.RegisterFireControl(this);
      this.SearchForRadar().Forget();
      this.StartSlowUpdateDelayed(1f, new Action(this.DistributeTurretTargets));
    }
    if (this.targetAcquisitionMode != FireControl.TargetAcquisitionMode.strategicStrike || !((UnityEngine.Object) this.attachedUnit.NetworkHQ != (UnityEngine.Object) null))
      return;
    this.StrategicTargetCheck().Forget();
  }

  private void FireControl_OnUnitDisabled(Unit attachedUnit)
  {
    if (this.targetAcquisitionMode == FireControl.TargetAcquisitionMode.searchForRadar && (UnityEngine.Object) attachedUnit.NetworkHQ != (UnityEngine.Object) null)
      attachedUnit.NetworkHQ.DeregisterFireControl(this);
    foreach (Turret turret in this.turrets)
      turret.FireControlDisabled();
  }

  private void FireControl_OnTurretDisabled(Unit turretUnit)
  {
    for (int index = this.turrets.Count - 1; index >= 0; --index)
    {
      Unit attachedUnit = this.turrets[index].GetAttachedUnit();
      if (attachedUnit.disabled)
      {
        attachedUnit.onDisableUnit += new Action<Unit>(this.FireControl_OnTurretDisabled);
        this.turrets.RemoveAt(index);
      }
    }
  }

  private async UniTask SearchForRadar()
  {
    FireControl fireControl = this;
    CancellationToken cancel;
    if ((UnityEngine.Object) fireControl == (UnityEngine.Object) null)
    {
      cancel = new CancellationToken();
    }
    else
    {
      cancel = fireControl.destroyCancellationToken;
      await UniTask.Delay(250);
      if (cancel.IsCancellationRequested)
      {
        cancel = new CancellationToken();
      }
      else
      {
        while ((UnityEngine.Object) fireControl.radar == (UnityEngine.Object) null)
        {
          Radar nearestRadar;
          if (fireControl.attachedUnit.NetworkHQ.TryGetRadar(fireControl.attachedUnit.GlobalPosition(), 300f, out nearestRadar))
            fireControl.RegisterRadar(nearestRadar);
          await UniTask.Delay(4000);
          if (cancel.IsCancellationRequested)
          {
            cancel = new CancellationToken();
            return;
          }
        }
        cancel = new CancellationToken();
      }
    }
  }

  private async UniTask StrategicTargetCheck()
  {
    FireControl fireControl = this;
    CancellationToken cancel = fireControl.destroyCancellationToken;
    Weapon weapon = fireControl.turrets[0].GetWeapon();
    UniTask uniTask = UniTask.Delay(UnityEngine.Random.Range(5000, 10000));
    await uniTask;
    while (!cancel.IsCancellationRequested)
    {
      float currentEscalation = NetworkSceneSingleton<MissionManager>.i.currentEscalation;
      float strategicThreshold = NetworkSceneSingleton<MissionManager>.i.strategicThreshold;
      if ((double) fireControl.attachedUnit.speed < 1.0 && weapon.ammo > 0 && (double) currentEscalation > (double) strategicThreshold && !fireControl.planningSalvo)
        fireControl.HQTargetAssessment();
      uniTask = UniTask.Delay(10000);
      await uniTask;
    }
    cancel = new CancellationToken();
    weapon = (Weapon) null;
  }

  private async UniTask DeployAll()
  {
    FireControl fireControl = this;
    CancellationToken cancel = fireControl.destroyCancellationToken;
    bool finishedDeploying = fireControl.deployables.Length == 0;
    while (!finishedDeploying)
    {
      if (cancel.IsCancellationRequested)
      {
        cancel = new CancellationToken();
        return;
      }
      finishedDeploying = true;
      foreach (FireControl.Deployable deployable in fireControl.deployables)
        finishedDeploying = finishedDeploying && deployable.TryDeploy();
      await UniTask.Yield();
    }
    cancel = new CancellationToken();
  }

  private void RegisterRadar(Radar radar)
  {
    this.radar = radar;
    radar.GetAttachedUnit().onDisableUnit += new Action<Unit>(this.FireControl_OnRadarDisabled);
    foreach (Turret turret in this.turrets)
      turret.GetAttachedUnit().radar = (TargetDetector) radar;
  }

  public bool TryGetRadar(out Radar radar)
  {
    radar = this.radar;
    return (UnityEngine.Object) radar != (UnityEngine.Object) null;
  }

  public void DeregisterTurret(Turret turret)
  {
    this.turrets.Remove(turret);
    turret.GetAttachedUnit().onDisableUnit -= new Action<Unit>(this.FireControl_OnTurretDisabled);
  }

  public void RegisterTurret(Turret turret)
  {
    this.turrets.Add(turret);
    this.weaponStation = turret.GetWeaponStation();
    turret.GetAttachedUnit().onDisableUnit += new Action<Unit>(this.FireControl_OnTurretDisabled);
  }

  private void DistributeTurretTargets()
  {
    if ((UnityEngine.Object) this.radar == (UnityEngine.Object) null)
      return;
    GlobalPosition a1 = this.attachedUnit.GlobalPosition();
    List<Unit> detectedTargets = this.radar.detectedTargets;
    FactionHQ networkHq = this.attachedUnit.NetworkHQ;
    this.availableTurrets.Clear();
    foreach (Turret turret in this.turrets)
    {
      if (turret.HasAmmo())
        this.availableTurrets.Add(turret);
    }
    if (this.availableTurrets.Count == 0)
      return;
    this.scoredTargets.Clear();
    foreach (Unit unit in detectedTargets)
    {
      if (!((UnityEngine.Object) unit == (UnityEngine.Object) null) && !unit.disabled)
      {
        float targetDistance = FastMath.Distance(a1, unit.GlobalPosition());
        OpportunityThreat opportunityThreat = CombatAI.AnalyzeTarget(this.weaponStation, this.attachedUnit, networkHq.GetTrackingData(unit.persistentID), targetDistance: targetDistance);
        float score = opportunityThreat.opportunity * (1f + opportunityThreat.threat) / targetDistance;
        if ((double) score > 0.0)
          this.scoredTargets.Add(new FireControl.ScoredTarget(unit, score));
      }
    }
    this.scoredTargets.Sort((Comparison<FireControl.ScoredTarget>) ((a, b) => a.score.CompareTo(b.score)));
    for (int index = 0; index < this.availableTurrets.Count; ++index)
    {
      Unit target = index < this.scoredTargets.Count ? this.scoredTargets[index].target : (Unit) null;
      this.availableTurrets[index].SetTargetFromController(target);
    }
  }

  private void FireControl_OnRadarDisabled(Unit unit)
  {
    unit.onDisableUnit -= new Action<Unit>(this.FireControl_OnRadarDisabled);
    foreach (Turret turret in this.turrets)
      turret.SetTargetFromController((Unit) null);
    this.SearchForRadar().Forget();
  }

  private void FireControl_OnRegisterMissile(Missile missile) => this.currentMissiles.Add(missile);

  private void FireControl_OnDeregisterMissile(Missile missile)
  {
    this.currentMissiles.Remove(missile);
  }

  public void SubscribeWeaponStation(WeaponStation weaponStation)
  {
    this.maxRange = Mathf.Max(this.maxRange, weaponStation.Weapons[0].info.targetRequirements.maxRange);
    this.minRange = Mathf.Min(this.minRange, weaponStation.Weapons[0].info.targetRequirements.minRange);
    this.subscribedWeaponStations.Add(weaponStation);
  }

  private void HQTargetAssessment()
  {
    if (this.attachedUnit.disabled || this.subscribedWeaponStations.Count == 0 || (UnityEngine.Object) this.attachedUnit.NetworkHQ == (UnityEngine.Object) null)
      return;
    bool flag = false;
    this.allTargets = this.attachedUnit.NetworkHQ.GetTargetsWithinRange(this.allTargets, this.transform, this.maxRange, false);
    WeaponStation subscribedWeaponStation = this.subscribedWeaponStations[0];
    foreach (TrackingInfo allTarget in this.allTargets)
    {
      allTarget.GetUnit();
      OpportunityThreat opportunityThreat = CombatAI.AnalyzeTarget(subscribedWeaponStation, this.attachedUnit, allTarget);
      Unit unit;
      if ((double) opportunityThreat.GetCombinedScore() > 0.0 && !this.SalvoListContainsTarget(allTarget) && allTarget.TryGetUnit(out unit) && (double) subscribedWeaponStation.WeaponInfo.CalcAttacksNeeded(unit) - (double) allTarget.missileAttacks > 0.0 && !this.TargetHasInboundMissiles(unit))
      {
        this.salvoTargets.Add(new FireControl.FireControlTarget(opportunityThreat, allTarget, subscribedWeaponStation));
        flag = true;
      }
    }
    if (flag)
      this.salvoTargets.Sort((Comparison<FireControl.FireControlTarget>) ((a, b) => a.GetCombinedScore().CompareTo(b.GetCombinedScore())));
    if (this.planningSalvo || this.salvoTargets.Count <= 0)
      return;
    this.PlanSalvo().Forget();
  }

  private bool SalvoListContainsTarget(TrackingInfo trackedTarget)
  {
    foreach (FireControl.FireControlTarget salvoTarget in this.salvoTargets)
    {
      if (salvoTarget.trackingInfo == trackedTarget)
        return true;
    }
    return false;
  }

  private bool TargetHasInboundMissiles(Unit target)
  {
    foreach (Missile currentMissile in this.currentMissiles)
    {
      if (currentMissile.targetID == target.persistentID)
        return true;
    }
    return false;
  }

  private async UniTask PlanSalvo()
  {
    FireControl fireControl = this;
    fireControl.planningSalvo = true;
    fireControl.queuedAttacks.Clear();
    fireControl.stationAmmo.Clear();
    int num1 = 0;
    foreach (WeaponStation subscribedWeaponStation in fireControl.subscribedWeaponStations)
    {
      fireControl.stationAmmo.Add(subscribedWeaponStation, subscribedWeaponStation.Ammo);
      num1 += subscribedWeaponStation.Ammo;
    }
    CancellationToken cancel;
    if (num1 == 0 || fireControl.salvoTargets.Count == 0)
    {
      fireControl.planningSalvo = false;
      cancel = new CancellationToken();
    }
    else
    {
      int index1 = 0;
      int num2 = 0;
      int count = fireControl.salvoTargets.Count;
      int index2 = fireControl.salvoTargets.Count - 1;
      while (num1 > 0 && index2 >= 0 && count > 0 && num2 < 100)
      {
        ++num2;
        if (num2 <= 100)
        {
          if (index1 >= fireControl.subscribedWeaponStations.Count)
            index1 = 0;
          WeaponStation subscribedWeaponStation = fireControl.subscribedWeaponStations[index1];
          if (fireControl.stationAmmo[subscribedWeaponStation] <= 0)
          {
            ++index1;
          }
          else
          {
            bool finished;
            fireControl.salvoTargets[index2].QueueAttack(fireControl.queuedAttacks, subscribedWeaponStation, out finished);
            fireControl.stationAmmo[subscribedWeaponStation]--;
            --num1;
            ++index1;
            if (finished)
            {
              --count;
              --index2;
            }
          }
        }
        else
          break;
      }
      cancel = fireControl.destroyCancellationToken;
      await UniTask.Delay((int) ((double) fireControl.planningTimePerFire * 1000.0 * (double) num2));
      if (cancel.IsCancellationRequested)
      {
        cancel = new CancellationToken();
      }
      else
      {
        fireControl.LaunchSalvo().Forget();
        cancel = new CancellationToken();
      }
    }
  }

  public void DeployOrStowLaunchers(bool deploying) => this.DeployOrStow(deploying).Forget();

  private async UniTask DeployOrStow(bool deploying)
  {
    FireControl fireControl = this;
    fireControl.deployed = !deploying;
    CancellationToken cancel = fireControl.destroyCancellationToken;
    bool finished = fireControl.deployables.Length == 0;
    while (!finished)
    {
      finished = true;
      foreach (FireControl.Deployable deployable in fireControl.deployables)
        finished = finished && (deploying ? deployable.TryDeploy() : deployable.TryStow());
      await UniTask.Yield();
      if (cancel.IsCancellationRequested)
      {
        cancel = new CancellationToken();
        return;
      }
    }
    fireControl.deployed = deploying;
    cancel = new CancellationToken();
  }

  private async UniTask LaunchSalvo()
  {
    FireControl fireControl = this;
    CancellationToken cancel = fireControl.destroyCancellationToken;
    if (fireControl.deployables.Length != 0)
    {
      fireControl.deployed = false;
      if (fireControl.attachedUnit is GroundVehicle attachedUnit)
        attachedUnit.RpcDeployFireControl(true);
      while (!fireControl.deployed)
      {
        await UniTask.Delay(1000);
        if (cancel.IsCancellationRequested)
        {
          cancel = new CancellationToken();
          return;
        }
      }
      await UniTask.Delay(2000);
      if (cancel.IsCancellationRequested)
      {
        cancel = new CancellationToken();
        return;
      }
    }
    int attackNum = 1;
    foreach (FireControl.QueuedAttack queuedAttack in fireControl.queuedAttacks)
    {
      if (!fireControl.attachedUnit.disabled && queuedAttack.StillValid(fireControl.attachedUnit.NetworkHQ))
      {
        queuedAttack.Fire(fireControl.attachedUnit);
        ++attackNum;
        await UniTask.Delay((int) ((double) fireControl.salvoInterval * 1000.0));
        if (cancel.IsCancellationRequested)
        {
          cancel = new CancellationToken();
          return;
        }
      }
      else
        queuedAttack.Cancel(fireControl.attachedUnit);
    }
    fireControl.planningSalvo = false;
    fireControl.salvoTargets.Clear();
    if (fireControl.deployables.Length == 0)
    {
      cancel = new CancellationToken();
    }
    else
    {
      fireControl.deployed = true;
      if (fireControl.attachedUnit is GroundVehicle attachedUnit)
        attachedUnit.RpcDeployFireControl(false);
      while (fireControl.deployed)
      {
        await UniTask.Delay(1000);
        if (cancel.IsCancellationRequested)
        {
          cancel = new CancellationToken();
          return;
        }
      }
      await UniTask.Delay(2000);
      int num = cancel.IsCancellationRequested ? 1 : 0;
      cancel = new CancellationToken();
    }
  }

  [Serializable]
  private class Deployable
  {
    [SerializeField]
    private Transform transform;
    [SerializeField]
    private Vector3 stowedAngle;
    [SerializeField]
    private Vector3 deployedAngle;
    [SerializeField]
    private float deployRate;
    private float deployAmount;

    public bool TryDeploy()
    {
      this.deployAmount += this.deployRate * Time.deltaTime;
      this.transform.localEulerAngles = Vector3.Lerp(this.stowedAngle, this.deployedAngle, this.deployAmount);
      return (double) this.deployAmount >= 1.0;
    }

    public bool TryStow()
    {
      this.deployAmount -= this.deployRate * Time.deltaTime;
      this.transform.localEulerAngles = Vector3.Lerp(this.stowedAngle, this.deployedAngle, this.deployAmount);
      return (double) this.deployAmount <= 0.0;
    }
  }

  private enum TargetAcquisitionMode
  {
    parentUnitTargetDetector,
    assignedTargetDetectors,
    searchForRadar,
    datalink,
    strategicStrike,
  }

  private readonly struct ScoredTarget(Unit target, float score)
  {
    public readonly Unit target = target;
    public readonly float score = score;
  }

  private class FireControlTarget
  {
    private readonly OpportunityThreat opportunityThreat;
    private float shotsRequired;
    public readonly TrackingInfo trackingInfo;

    public FireControlTarget(
      OpportunityThreat opportunityThreat,
      TrackingInfo trackingInfo,
      WeaponStation weaponStation)
    {
      this.opportunityThreat = opportunityThreat;
      this.trackingInfo = trackingInfo;
      Unit unit;
      this.shotsRequired = trackingInfo.TryGetUnit(out unit) ? weaponStation.WeaponInfo.CalcAttacksNeeded(unit) - (float) ((int) trackingInfo.missileAttacks + (int) trackingInfo.attackers) : 0.0f;
    }

    public bool StillRelevant(FactionHQ hq)
    {
      Unit unit;
      return this.trackingInfo.TryGetUnit(out unit) && !unit.disabled && (UnityEngine.Object) unit.NetworkHQ != (UnityEngine.Object) null && (UnityEngine.Object) unit.NetworkHQ != (UnityEngine.Object) hq;
    }

    public float GetCombinedScore() => this.opportunityThreat.GetCombinedScore();

    public void QueueAttack(
      List<FireControl.QueuedAttack> queuedAttacks,
      WeaponStation weaponStation,
      out bool finished)
    {
      --this.shotsRequired;
      Unit unit;
      if (this.trackingInfo.TryGetUnit(out unit))
        queuedAttacks.Add(new FireControl.QueuedAttack(unit, this.trackingInfo, weaponStation));
      finished = (double) this.shotsRequired <= 0.0;
    }
  }

  private struct QueuedAttack
  {
    private readonly Unit target;
    private readonly WeaponStation weaponStation;
    private readonly TrackingInfo trackingInfo;

    public QueuedAttack(Unit target, TrackingInfo trackingInfo, WeaponStation weaponStation)
    {
      this.target = target;
      this.trackingInfo = trackingInfo;
      this.weaponStation = weaponStation;
      ++trackingInfo.attackers;
    }

    public bool StillValid(FactionHQ hq)
    {
      return (UnityEngine.Object) this.target != (UnityEngine.Object) null && !this.target.disabled && (UnityEngine.Object) this.target.NetworkHQ != (UnityEngine.Object) null && (UnityEngine.Object) this.target.NetworkHQ != (UnityEngine.Object) hq;
    }

    public void Fire(Unit attachedUnit)
    {
      this.weaponStation.Fire(attachedUnit, this.target);
      --this.trackingInfo.attackers;
    }

    public void Cancel(Unit attachedUnit) => --this.trackingInfo.attackers;
  }
}

// Decompiled with JetBrains decompiler
// Type: Turret
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

#nullable disable
public class Turret : MonoBehaviour
{
  [SerializeField]
  private Unit attachedUnit;
  [SerializeField]
  private Turret.TargetAcquisitionMode targetAcquisitionMode = Turret.TargetAcquisitionMode.assignedTargetDetectors;
  [SerializeField]
  private FireControl fireControl;
  [SerializeField]
  private bool firesWithoutAiming;
  [SerializeField]
  private List<TargetDetector> targetDetectors;
  [SerializeField]
  private WeaponStation[] weaponStations;
  [SerializeField]
  private Weapon aimSafetyWeapon;
  [SerializeField]
  private float traverseRate;
  [SerializeField]
  private float traverseRange;
  [SerializeField]
  private float elevationRate = 30f;
  [SerializeField]
  private float minElevation;
  [SerializeField]
  private float maxElevation;
  [SerializeField]
  private Transform elevationTransform;
  [SerializeField]
  private FiringCone[] firingCones;
  [SerializeField]
  private float targetAssessmentInterval = 2f;
  [SerializeField]
  private AimSolver aimSolver;
  [SerializeField]
  private float lockTime = 0.5f;
  [SerializeField]
  private bool newTargetSearchAfterFire;
  [SerializeField]
  private UnitPart[] criticalParts;
  private Aircraft aircraft;
  private bool disabled;
  [SerializeField]
  private bool manual;
  [SerializeField]
  private bool ready;
  [SerializeField]
  private bool onTarget;
  [SerializeField]
  private bool stowed;
  private float lastVectorSent;
  private byte turretIndex;
  private Vector3 manualVector;
  private float traverseAngle;
  private float elevationAngle;
  private float traverseError;
  private float elevationError;
  private float targetRange;
  private float timeOnTarget;
  private float lastTargetAssessment;
  private Vector3 aimVector;
  private float maxRange;
  private List<Unit> potentialTargets = new List<Unit>();
  private Unit target;
  private RaycastHit hit;
  private WeaponStation currentWeaponStation;
  private GameObject debugArrow;

  private void Awake()
  {
    if ((UnityEngine.Object) this.attachedUnit == (UnityEngine.Object) null)
      this.attachedUnit = this.GetComponentInParent<UnitPart>().parentUnit;
    if (this.weaponStations.Length != 0)
    {
      foreach (WeaponStation weaponStation in this.weaponStations)
        weaponStation.WeaponInfo = weaponStation.Weapons[0].info;
      this.currentWeaponStation = this.weaponStations[0];
      if ((UnityEngine.Object) this.elevationTransform == (UnityEngine.Object) null)
        this.elevationTransform = this.weaponStations[0].Weapons[0].transform;
    }
    this.elevationAngle = this.weaponStations[0].Weapons[0].transform.localRotation.x;
    this.attachedUnit.onInitialize += new Action(this.Turret_OnInitialize);
    this.attachedUnit.onDisableUnit += new Action<Unit>(this.Turret_OnUnitDisabled);
    if (this.attachedUnit is Aircraft attachedUnit)
      this.aircraft = attachedUnit;
    foreach (UnitPart criticalPart in this.criticalParts)
      this.RegisterPart(criticalPart);
  }

  private void OnDestroy()
  {
    this.potentialTargets.Clear();
    this.target = (Unit) null;
    foreach (TargetDetector targetDetector in this.targetDetectors)
      this.DeregisterTargetDetector(targetDetector);
  }

  private void Turret_OnInitialize()
  {
    if ((UnityEngine.Object) this.aircraft == (UnityEngine.Object) null)
    {
      foreach (WeaponStation weaponStation in this.weaponStations)
      {
        this.currentWeaponStation = weaponStation;
        weaponStation.Number = (byte) this.attachedUnit.weaponStations.Count;
        weaponStation.WeaponInfo = weaponStation.Weapons[0].info;
        weaponStation.Rearm();
        int num = (int) weaponStation.AssignTurret(this);
        this.attachedUnit.RegisterWeaponStation(weaponStation);
      }
      this.AssessWeapons();
    }
    if (this.attachedUnit is IRearmable attachedUnit)
    {
      float skill = 0.7f;
      if (this.attachedUnit is GroundVehicle attachedUnit2)
        skill = attachedUnit2.skill;
      else if (this.attachedUnit is Ship attachedUnit1)
        skill = attachedUnit1.skill;
      this.RegisterRearm(attachedUnit, skill);
    }
    if (!this.attachedUnit.IsServer)
      return;
    if (this.targetAcquisitionMode == Turret.TargetAcquisitionMode.datalink && (UnityEngine.Object) this.attachedUnit.NetworkHQ != (UnityEngine.Object) null)
      this.StartSlowUpdateDelayed(this.targetAssessmentInterval, new Action(this.DatalinkTargetSearch));
    else if (this.targetAcquisitionMode == Turret.TargetAcquisitionMode.fireControl && (UnityEngine.Object) this.fireControl != (UnityEngine.Object) null && this.weaponStations.Length != 0)
    {
      foreach (WeaponStation weaponStation in this.weaponStations)
        this.fireControl.SubscribeWeaponStation(weaponStation);
    }
    else if (this.targetAcquisitionMode == Turret.TargetAcquisitionMode.assignedTargetDetectors)
    {
      foreach (TargetDetector targetDetector in this.targetDetectors)
        this.RegisterTargetDetector(targetDetector);
    }
    else
    {
      if (this.stowed || this.targetAcquisitionMode != Turret.TargetAcquisitionMode.searchForRadar || !((UnityEngine.Object) this.attachedUnit.NetworkHQ != (UnityEngine.Object) null))
        return;
      this.SearchForRadar().Forget();
    }
  }

  private void RegisterRearm(IRearmable rearmable, float skill)
  {
    rearmable.OnRearm += new Action<RearmEventArgs>(this.Turret_OnRearm);
    this.lockTime /= Mathf.Max(skill, 0.1f);
    this.traverseRate *= Mathf.Max(skill, 0.33f);
  }

  public void AttachToWeaponManager(Aircraft aircraft)
  {
    this.currentWeaponStation = this.weaponStations[0];
    this.attachedUnit = (Unit) aircraft;
    bool flag = false;
    foreach (WeaponStation weaponStation in aircraft.weaponStations)
    {
      if ((UnityEngine.Object) weaponStation.WeaponInfo == (UnityEngine.Object) this.currentWeaponStation.WeaponInfo)
      {
        this.weaponStations = new WeaponStation[1];
        this.weaponStations[0] = weaponStation;
        this.currentWeaponStation = weaponStation;
        this.turretIndex = weaponStation.AssignTurret(this);
        flag = true;
        break;
      }
    }
    if (!flag)
      aircraft.RegisterWeaponStation(this.currentWeaponStation);
    if (this.targetAcquisitionMode == Turret.TargetAcquisitionMode.parentUnitTargetDetector && (UnityEngine.Object) this.attachedUnit.NetworkHQ != (UnityEngine.Object) null)
      this.RegisterTargetDetector((this.attachedUnit as Aircraft).EOTS);
    if (this.targetAcquisitionMode != Turret.TargetAcquisitionMode.datalink || !((UnityEngine.Object) this.attachedUnit.NetworkHQ != (UnityEngine.Object) null))
      return;
    this.StartSlowUpdateDelayed(this.targetAssessmentInterval, new Action(this.DatalinkTargetSearch));
  }

  private void RegisterTargetDetector(TargetDetector targetDetector)
  {
    if (!this.targetDetectors.Contains(targetDetector))
      this.targetDetectors.Add(targetDetector);
    if (targetDetector is Radar)
      this.attachedUnit.radar = (TargetDetector) (targetDetector as Radar);
    targetDetector.onDetectTarget += new Action<Unit>(this.Turret_OnDetectTarget);
    targetDetector.onScan += new Action(this.Turret_OnCompleteScan);
    targetDetector.onDisabled += new Action<TargetDetector>(this.Turret_OnTargetDetectorDisabled);
  }

  private void DeregisterTargetDetector(TargetDetector targetDetector)
  {
    if ((UnityEngine.Object) targetDetector == (UnityEngine.Object) null)
      return;
    targetDetector.onDetectTarget -= new Action<Unit>(this.Turret_OnDetectTarget);
    targetDetector.onScan -= new Action(this.Turret_OnCompleteScan);
    targetDetector.onDisabled -= new Action<TargetDetector>(this.Turret_OnTargetDetectorDisabled);
  }

  private void RegisterPart(UnitPart part)
  {
    part.onApplyDamage += new Action<UnitPart.OnApplyDamage>(this.Turret_OnPartDamage);
    part.onPartDetached += new Action<UnitPart>(this.Turret_OnDetachedFromUnit);
  }

  private void DeregisterPart(UnitPart part)
  {
    if (!((UnityEngine.Object) part != (UnityEngine.Object) null))
      return;
    part.onApplyDamage -= new Action<UnitPart.OnApplyDamage>(this.Turret_OnPartDamage);
    part.onPartDetached -= new Action<UnitPart>(this.Turret_OnDetachedFromUnit);
  }

  public void FireControlDisabled()
  {
    this.fireControl = (FireControl) null;
    this.SearchForRadar().Forget();
  }

  private async UniTask SearchForRadar()
  {
    Turret turret = this;
    CancellationToken cancel;
    if ((UnityEngine.Object) turret == (UnityEngine.Object) null)
    {
      cancel = new CancellationToken();
    }
    else
    {
      cancel = turret.destroyCancellationToken;
      turret.attachedUnit.radar = (TargetDetector) null;
      turret.targetDetectors.Clear();
      if ((UnityEngine.Object) turret.fireControl != (UnityEngine.Object) null)
        turret.fireControl.DeregisterTurret(turret);
      turret.fireControl = (FireControl) null;
      await UniTask.Delay(1000);
      if (cancel.IsCancellationRequested)
      {
        cancel = new CancellationToken();
      }
      else
      {
        while ((UnityEngine.Object) turret.attachedUnit != (UnityEngine.Object) null)
        {
          if (!turret.attachedUnit.disabled)
          {
            Radar radar;
            if (turret.attachedUnit.NetworkHQ.TryGetFireControl(turret.attachedUnit.GlobalPosition(), 300f, out turret.fireControl) && turret.fireControl.TryGetRadar(out radar))
            {
              turret.attachedUnit.radar = (TargetDetector) radar;
              turret.fireControl.RegisterTurret(turret);
              cancel = new CancellationToken();
              return;
            }
            Radar nearestRadar;
            if (turret.attachedUnit.NetworkHQ.TryGetRadar(turret.attachedUnit.GlobalPosition(), 300f, out nearestRadar))
            {
              turret.RegisterTargetDetector((TargetDetector) nearestRadar);
              cancel = new CancellationToken();
              return;
            }
            await UniTask.Delay(10000);
            if (cancel.IsCancellationRequested)
            {
              cancel = new CancellationToken();
              return;
            }
          }
          else
          {
            cancel = new CancellationToken();
            return;
          }
        }
        cancel = new CancellationToken();
      }
    }
  }

  private void DatalinkTargetSearch()
  {
    if (this.manual || this.currentWeaponStation.Ammo <= 0)
      return;
    this.potentialTargets = this.attachedUnit.NetworkHQ.GetTargetsWithinCone(this.potentialTargets, this.elevationTransform, this.firingCones, this.currentWeaponStation.WeaponInfo.targetRequirements.lineOfSight);
    this.ChooseTarget(true);
  }

  private void Turret_OnDetectTarget(Unit unit)
  {
    if (this.manual || this.potentialTargets.Contains(unit))
      return;
    this.potentialTargets.Add(unit);
  }

  private void Turret_OnCompleteScan()
  {
    if (this.manual || (double) Time.timeSinceLevelLoad - (double) this.lastTargetAssessment <= (double) this.targetAssessmentInterval)
      return;
    this.ChooseTarget(true);
  }

  public Unit GetAttachedUnit() => this.attachedUnit;

  public bool HasAmmo() => this.weaponStations[0].Ammo > 0;

  private unsafe void ChooseTarget(bool clearAfterSearch)
  {
    if (this.disabled || this.attachedUnit.disabled)
      return;
    this.lastTargetAssessment = Time.timeSinceLevelLoad;
    if ((UnityEngine.Object) this.target != (UnityEngine.Object) null)
    {
      this.target.onDisableUnit -= new Action<Unit>(this.Turret_OnTargetDisabled);
      if (this.attachedUnit.NetworkHQ.trackingDatabase.ContainsKey(this.target.persistentID))
        --this.attachedUnit.NetworkHQ.trackingDatabase[this.target.persistentID].attackers;
    }
    Unit target = this.target;
    this.target = (Unit) null;
    float priorityThreshold = 0.0f;
    foreach (Unit potentialTarget in this.potentialTargets)
      this.AssessTargetPriority(potentialTarget, ref priorityThreshold);
    if ((UnityEngine.Object) this.target != (UnityEngine.Object) null)
    {
      this.target.onDisableUnit += new Action<Unit>(this.Turret_OnTargetDisabled);
      ++this.attachedUnit.NetworkHQ.trackingDatabase[this.target.persistentID].attackers;
    }
    ; // Unable to render the statement
    Span<PersistentID> span = new Span<PersistentID>((void*) local, 1);
    span[0] = (UnityEngine.Object) this.target != (UnityEngine.Object) null ? this.target.persistentID : PersistentID.None;
    if ((UnityEngine.Object) this.target != (UnityEngine.Object) target)
    {
      if (this.attachedUnit is Aircraft attachedUnit)
      {
        if (this.currentWeaponStation.TurretCount() > 1)
          attachedUnit.SetStationTurretTarget(this.currentWeaponStation.Number, this.turretIndex, span[0]);
        else
          attachedUnit.SetStationTargets(this.currentWeaponStation.Number, Span<PersistentID>.op_Implicit(span));
      }
      else
        this.attachedUnit.RpcSetStationTargets(this.currentWeaponStation.Number, Span<PersistentID>.op_Implicit(span));
    }
    if (!clearAfterSearch)
      return;
    this.potentialTargets.Clear();
  }

  private void AssessTargetPriority(Unit targetCandidate, ref float priorityThreshold)
  {
    TrackingInfo trackingData = this.attachedUnit.NetworkHQ.GetTrackingData(targetCandidate.persistentID);
    if (trackingData == null || (UnityEngine.Object) targetCandidate == (UnityEngine.Object) null || targetCandidate.disabled || (UnityEngine.Object) targetCandidate.NetworkHQ == (UnityEngine.Object) null)
      return;
    Vector3 targetVector = targetCandidate.transform.position - this.transform.position;
    if (!FiringConeChecker.VectorWithinFiringCones(this.firingCones, targetVector, out Vector3 _))
      return;
    float magnitude = targetVector.magnitude;
    foreach (WeaponStation weaponStation in this.weaponStations)
    {
      float num1 = weaponStation.WeaponInfo.targetRequirements.maxRange / magnitude;
      if ((double) num1 >= 0.699999988079071)
      {
        OpportunityThreat opportunityThreat = CombatAI.AnalyzeTarget(weaponStation, this.attachedUnit, trackingData, 2f, magnitude);
        float num2 = opportunityThreat.opportunity * (1f + opportunityThreat.threat) * num1;
        if ((double) num2 != 0.0)
        {
          if (weaponStation.Reloading || weaponStation.Ammo <= 0)
            num2 = 0.01f;
          if ((double) num1 < 1.0 || (double) magnitude < (double) weaponStation.WeaponInfo.targetRequirements.minRange)
            num2 *= 0.01f;
          if ((double) targetCandidate.definition.armorTier > (double) weaponStation.WeaponInfo.armorTierEffectiveness)
            num2 *= 0.2f;
          if (this.targetAcquisitionMode == Turret.TargetAcquisitionMode.datalink && (double) targetCandidate.speed > 0.0 && (double) num1 < 2.0)
            num2 *= 0.6f;
          if ((double) num2 > (double) priorityThreshold)
          {
            this.target = targetCandidate;
            this.currentWeaponStation = weaponStation;
            priorityThreshold = num2;
          }
        }
      }
    }
  }

  public Unit GetTarget() => this.target;

  public WeaponStation GetWeaponStation() => this.currentWeaponStation;

  public bool HasFiringCone(out Vector3 firingConeForward, out float angle)
  {
    firingConeForward = this.transform.forward;
    angle = 0.0f;
    if (this.firingCones.Length == 0)
      return false;
    firingConeForward = this.firingCones[0].GetDirection();
    angle = this.firingCones[0].GetAngle();
    return true;
  }

  public bool IsOnTarget() => this.onTarget;

  public float GetTraverseRange() => this.traverseRange;

  public Weapon GetWeapon()
  {
    return (UnityEngine.Object) this.aimSafetyWeapon != (UnityEngine.Object) null ? this.aimSafetyWeapon : this.weaponStations[0].Weapons[0];
  }

  public WeaponStation[] GetWeaponStations() => this.weaponStations;

  private void Turret_OnPartDamage(UnitPart.OnApplyDamage e)
  {
    if ((double) e.hitPoints >= 0.0)
      return;
    this.disabled = true;
    this.enabled = false;
  }

  private void Turret_OnDetachedFromUnit(UnitPart unitPart)
  {
    this.disabled = true;
    this.enabled = false;
  }

  public Vector3 GetDirection()
  {
    return this.elevationTransform.position + this.elevationTransform.forward * 10000f;
  }

  private void Turret_OnTargetDetectorDisabled(TargetDetector targetDetector)
  {
    if (this.attachedUnit.disabled)
      return;
    this.targetDetectors.Remove(targetDetector);
    if (this.stowed || this.targetDetectors.Count != 0 || this.targetAcquisitionMode != Turret.TargetAcquisitionMode.searchForRadar || !((UnityEngine.Object) this.attachedUnit.NetworkHQ != (UnityEngine.Object) null))
      return;
    this.SearchForRadar().Forget();
  }

  private void Turret_OnUnitDisabled(Unit unit) => this.DestroyTurret();

  public void DestroyTurret()
  {
    this.attachedUnit.onDisableUnit -= new Action<Unit>(this.Turret_OnUnitDisabled);
    TrackingInfo trackingInfo;
    if ((UnityEngine.Object) this.target != (UnityEngine.Object) null && (UnityEngine.Object) this.attachedUnit.NetworkHQ != (UnityEngine.Object) null && this.attachedUnit.NetworkHQ.trackingDatabase.TryGetValue(this.target.persistentID, out trackingInfo))
      --trackingInfo.attackers;
    foreach (TargetDetector targetDetector in this.targetDetectors)
      this.DeregisterTargetDetector(targetDetector);
    foreach (UnitPart criticalPart in this.criticalParts)
      this.DeregisterPart(criticalPart);
    UnityEngine.Object.Destroy((UnityEngine.Object) this);
  }

  public void SetTarget(PersistentID id, byte stationIndex)
  {
    if (this.attachedUnit.disabled)
      return;
    UnitRegistry.TryGetUnit(new PersistentID?(id), out this.target);
    this.currentWeaponStation = this.attachedUnit.weaponStations[(int) stationIndex];
    this.maxRange = this.currentWeaponStation.WeaponInfo.targetRequirements.maxRange;
    this.enabled = (UnityEngine.Object) this.target != (UnityEngine.Object) null || this.manual;
    this.aimSolver.SetTarget(this.attachedUnit, this.target, this.currentWeaponStation.Weapons[0].transform, this.currentWeaponStation.WeaponInfo);
  }

  public unsafe void SetTargetFromController(Unit target)
  {
    if (!((UnityEngine.Object) this.target != (UnityEngine.Object) target))
      return;
    ; // Unable to render the statement
    Span<PersistentID> span = new Span<PersistentID>((void*) local, 1);
    span[0] = (UnityEngine.Object) target != (UnityEngine.Object) null ? target.persistentID : PersistentID.None;
    this.attachedUnit.RpcSetStationTargets((byte) 0, Span<PersistentID>.op_Implicit(span));
  }

  public void SetStowed(bool stowed)
  {
    if (this.stowed == stowed)
      return;
    this.stowed = stowed;
    this.enabled = !stowed;
    if (this.targetAcquisitionMode != Turret.TargetAcquisitionMode.searchForRadar || !((UnityEngine.Object) this.attachedUnit.NetworkHQ != (UnityEngine.Object) null))
      return;
    if (stowed)
    {
      this.attachedUnit.radar = (TargetDetector) null;
      foreach (TargetDetector targetDetector in this.targetDetectors)
        this.DeregisterTargetDetector(targetDetector);
      if (!((UnityEngine.Object) this.fireControl != (UnityEngine.Object) null))
        return;
      this.fireControl.DeregisterTurret(this);
    }
    else
      this.SearchForRadar().Forget();
  }

  public void SetManual(bool enabled)
  {
    this.manual = enabled;
    this.enabled = true;
  }

  public void SetVector(Vector3 vector) => this.manualVector = vector;

  public void AssessWeapons()
  {
    foreach (WeaponStation weaponStation in this.attachedUnit.weaponStations)
    {
      weaponStation.AssessAmmo();
      if (weaponStation.Ammo <= 0 && this.attachedUnit is IRearmable attachedUnit && weaponStation.Weapons[0].Rearmable)
        attachedUnit.RequestRearm();
    }
  }

  private void Turret_OnTargetDisabled(Unit unit) => this.ChooseTarget(false);

  private void AimTurret(Vector3 aimVector)
  {
    this.targetRange = aimVector.magnitude;
    Vector3 vector3 = aimVector;
    Vector3 nearestAllowableVector;
    if (!FiringConeChecker.VectorWithinFiringCones(this.firingCones, vector3, out nearestAllowableVector))
      vector3 = nearestAllowableVector;
    float num = Vector3.Angle(aimVector, vector3);
    this.traverseError = TargetCalc.GetAngleOnAxis(this.transform.forward, vector3, this.transform.up);
    this.traverseAngle += Mathf.Clamp(this.traverseError, -this.traverseRate * Time.fixedDeltaTime, this.traverseRate * Time.fixedDeltaTime);
    if ((double) this.traverseRange < 360.0)
      this.traverseAngle = Mathf.Clamp(this.traverseAngle, -this.traverseRange, this.traverseRange);
    this.elevationError = TargetCalc.GetAngleOnAxis(this.elevationTransform.forward, vector3, this.transform.right);
    this.elevationAngle += Mathf.Clamp(this.elevationError, -30f * Time.fixedDeltaTime, 30f * Time.fixedDeltaTime);
    this.elevationAngle = Mathf.Clamp(this.elevationAngle, -this.maxElevation, -this.minElevation);
    this.elevationTransform.localEulerAngles = new Vector3(this.elevationAngle, 0.0f, 0.0f);
    this.transform.localEulerAngles = new Vector3(0.0f, this.traverseAngle, 0.0f);
    this.onTarget = (double) num + (double) Mathf.Abs(this.traverseError) + (double) Mathf.Abs(this.elevationError) < 1.0;
    if (!((UnityEngine.Object) this.aimSafetyWeapon != (UnityEngine.Object) null))
      return;
    this.aimSafetyWeapon.Safety = !this.onTarget;
  }

  private bool AimTurret(WeaponStation weaponStation)
  {
    if (this.currentWeaponStation.Ammo == 0)
      return false;
    this.aimVector = this.aimSolver.GetAimVector(out this.targetRange);
    Vector3 vector3 = this.aimVector;
    Vector3 nearestAllowableVector;
    if (!FiringConeChecker.VectorWithinFiringCones(this.firingCones, vector3, out nearestAllowableVector))
      vector3 = nearestAllowableVector;
    float num = Vector3.Angle(this.aimVector, vector3);
    this.traverseError = TargetCalc.GetAngleOnAxis(this.transform.forward, vector3, this.transform.up);
    this.traverseAngle += Mathf.Clamp(this.traverseError, -this.traverseRate * Time.fixedDeltaTime, this.traverseRate * Time.fixedDeltaTime);
    if ((double) Mathf.Abs(this.traverseError) < 45.0 && (double) this.minElevation != (double) this.maxElevation)
    {
      this.elevationError = TargetCalc.GetAngleOnAxis(this.elevationTransform.forward, vector3, this.transform.right);
      this.elevationAngle += Mathf.Clamp(this.elevationError, -this.elevationRate * Time.fixedDeltaTime, this.elevationRate * Time.fixedDeltaTime);
      this.elevationAngle = Mathf.Clamp(this.elevationAngle, -this.maxElevation, -this.minElevation);
      this.elevationTransform.localEulerAngles = new Vector3(this.elevationAngle, 0.0f, 0.0f);
    }
    else
      this.elevationError = 0.0f;
    if (!weaponStation.WeaponInfo.boresight && !weaponStation.WeaponInfo.gun)
      this.elevationError = 0.0f;
    if ((double) this.traverseRange < 360.0)
      this.traverseAngle = Mathf.Clamp(this.traverseAngle, -this.traverseRange, this.traverseRange);
    this.transform.localEulerAngles = new Vector3(0.0f, this.traverseAngle, 0.0f);
    this.onTarget = (double) num + (double) Mathf.Abs(this.traverseError) + (double) Mathf.Abs(this.elevationError) < 1.0;
    if ((UnityEngine.Object) this.aimSafetyWeapon != (UnityEngine.Object) null)
      this.aimSafetyWeapon.Safety = !this.onTarget;
    return this.onTarget;
  }

  public Vector2 GetTurretAimError()
  {
    return new Vector2(TargetCalc.GetAngleOnAxis(this.transform.forward, this.aimVector, this.transform.up), TargetCalc.GetAngleOnAxis(this.transform.forward, this.aimVector, this.transform.right));
  }

  private void Turret_OnRearm(RearmEventArgs _)
  {
    for (int index = 0; index < this.weaponStations.Length; ++index)
      this.weaponStations[index].Rearm();
  }

  public void SetObservedBullet(BulletSim.Bullet bullet)
  {
    if ((UnityEngine.Object) this.target == (UnityEngine.Object) null || (double) this.target.speed > 100.0)
      return;
    this.aimSolver.SetObservedBullet(bullet);
  }

  private void FixedUpdate()
  {
    bool flag = !this.manual && this.targetAcquisitionMode != Turret.TargetAcquisitionMode.datalink && this.targetDetectors.Count == 0 && (UnityEngine.Object) this.fireControl == (UnityEngine.Object) null;
    if ((((UnityEngine.Object) this.attachedUnit == (UnityEngine.Object) null || this.attachedUnit.disabled ? 1 : (this.disabled ? 1 : 0)) | (flag ? 1 : 0)) != 0 || this.stowed)
    {
      this.enabled = false;
    }
    else
    {
      if (this.manual)
      {
        if ((UnityEngine.Object) this.target == (UnityEngine.Object) null)
        {
          if (this.aircraft.LocalSim && SceneSingleton<CameraStateManager>.i.currentState == SceneSingleton<CameraStateManager>.i.cockpitState)
          {
            this.SetVector(SceneSingleton<CameraStateManager>.i.transform.forward);
            if ((double) Time.timeSinceLevelLoad - (double) this.lastVectorSent > 0.20000000298023224)
            {
              this.aircraft.SetTurretVector(this.currentWeaponStation.Number, this.manualVector);
              this.lastVectorSent = Time.timeSinceLevelLoad;
            }
          }
          this.AimTurret(this.manualVector);
          return;
        }
      }
      else if ((UnityEngine.Object) this.target == (UnityEngine.Object) null || this.target.disabled)
      {
        this.enabled = false;
        return;
      }
      if (this.firesWithoutAiming || this.AimTurret(this.currentWeaponStation))
        this.timeOnTarget += Time.deltaTime;
      else
        this.timeOnTarget = 0.0f;
      if (this.manual || !this.attachedUnit.LocalSim || (double) this.targetRange >= (double) this.maxRange || (double) this.timeOnTarget <= (double) this.lockTime || !this.currentWeaponStation.Ready() || !((UnityEngine.Object) this.target.NetworkHQ != (UnityEngine.Object) this.attachedUnit.NetworkHQ))
        return;
      Transform elevationTransform = this.elevationTransform;
      if ((UnityEngine.Object) this.attachedUnit == (UnityEngine.Object) SceneSingleton<CombatHUD>.i.aircraft && !SceneSingleton<CombatHUD>.i.turretAutoControl || !this.firesWithoutAiming && Physics.Linecast(elevationTransform.position, elevationTransform.position + elevationTransform.forward * 200f, out this.hit, -8193) && (double) this.hit.distance < (double) this.targetRange - ((double) this.target.maxRadius + 50.0))
        return;
      this.currentWeaponStation.Fire(this.attachedUnit, this.target);
      if (this.attachedUnit is IRearmable attachedUnit && this.currentWeaponStation.Weapons[0].Rearmable)
        attachedUnit.RequestRearm();
      if (!this.attachedUnit.IsServer || this.currentWeaponStation.WeaponInfo.gun || !this.newTargetSearchAfterFire || !((UnityEngine.Object) this.fireControl == (UnityEngine.Object) null))
        return;
      this.ChooseTarget(false);
    }
  }

  private enum TargetAcquisitionMode
  {
    parentUnitTargetDetector,
    assignedTargetDetectors,
    searchForRadar,
    datalink,
    fireControl,
  }
}

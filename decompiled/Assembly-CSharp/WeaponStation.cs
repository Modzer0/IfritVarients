// Decompiled with JetBrains decompiler
// Type: WeaponStation
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.Networking;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

#nullable disable
[Serializable]
public class WeaponStation
{
  [FormerlySerializedAs("weapons")]
  public List<Weapon> Weapons = new List<Weapon>();
  public List<Turret> Turrets = new List<Turret>();
  [FormerlySerializedAs("weaponInfo")]
  [HideInInspector]
  public WeaponInfo WeaponInfo;
  [HideInInspector]
  public byte Number;
  [HideInInspector]
  public bool Cargo;
  [HideInInspector]
  public bool Reloading;
  [HideInInspector]
  public bool Full;
  [HideInInspector]
  public bool WeaponActive;
  [HideInInspector]
  public bool SalvoInProgress;
  [HideInInspector]
  public float LastFiredTime;
  private readonly bool gearSafety;
  private readonly bool groundSafety;
  private readonly bool sortWeapons;
  public int Ammo;
  public int FullAmmo;
  private int weaponIndex;
  public Dictionary<UnitDefinition, OpportunityThreat> TypeLookup = new Dictionary<UnitDefinition, OpportunityThreat>();

  public event Action OnUpdated;

  public WeaponStation(
    Unit unit,
    bool cargo,
    bool gearSafety,
    bool groundSafety,
    bool sortWeapons)
  {
    this.Number = (byte) unit.weaponStations.Count;
    this.Cargo = cargo;
    this.gearSafety = gearSafety;
    this.groundSafety = groundSafety;
    this.sortWeapons = sortWeapons;
  }

  public void SetStationActive(Aircraft aircraft, bool isActive)
  {
    if (!this.HasTurret())
      return;
    foreach (Turret turret in this.Turrets)
      turret.SetManual(isActive && (UnityEngine.Object) aircraft.Player != (UnityEngine.Object) null);
    if (!isActive || !((UnityEngine.Object) aircraft == (UnityEngine.Object) SceneSingleton<CombatHUD>.i.aircraft))
      return;
    SceneSingleton<AircraftActionsReport>.i.ReportText(this.WeaponInfo.weaponName + " Turret under pilot control", 4f);
  }

  public void SetTurretVector(Vector3 vector)
  {
    if (this.Turrets == null)
      return;
    foreach (Turret turret in this.Turrets)
      turret.SetVector(vector);
  }

  public void RegisterWeapon(
    Weapon weapon,
    Aircraft aircraft,
    WeaponMount weaponMount,
    Hardpoint hardpoint)
  {
    if (!(weapon is Gun) || !this.Weapons.Contains(weapon))
      this.Weapons.Add(weapon);
    weapon.AttachToHardpoint(aircraft, hardpoint, weaponMount);
    weapon.SetWeaponStation(this);
    this.WeaponInfo = weapon.info;
  }

  public bool SafetyIsOn(Aircraft aircraft)
  {
    if (aircraft.gearState != LandingGear.GearState.LockedRetracted && this.gearSafety)
      return true;
    return (double) aircraft.radarAlt < 0.5 && this.groundSafety;
  }

  public bool Ready()
  {
    return (double) Time.timeSinceLevelLoad - (double) this.LastFiredTime > (double) this.WeaponInfo.fireInterval && this.Ammo > 0 && (double) this.GetReloadStatusMin() <= 0.0;
  }

  public float GetReloadStatusMax()
  {
    if (!this.Reloading)
      return 0.0f;
    float a = 0.0f;
    foreach (Weapon weapon in this.Weapons)
      a = Mathf.Max(a, weapon.GetReloadProgress());
    return a;
  }

  public float GetReloadStatusMin()
  {
    if (!this.Reloading)
      return 0.0f;
    float a = 1f;
    foreach (Weapon weapon in this.Weapons)
      a = Mathf.Min(a, weapon.GetReloadProgress());
    return a;
  }

  public string GetAmmoReadout()
  {
    int num1 = 0;
    int num2 = 0;
    foreach (Weapon weapon in this.Weapons)
    {
      num1 += weapon.GetAmmoLoaded();
      num2 += weapon.GetAmmoTotal();
    }
    if (num2 != num1)
      return $"{num1} / {this.Ammo}";
    return NetworkManagerNuclearOption.i.Server.Active ? $"{num1}" : $"{this.Ammo}";
  }

  public byte AssignTurret(Turret turret)
  {
    if (this.Turrets == null)
      this.Turrets = new List<Turret>();
    this.Turrets.Add(turret);
    return (byte) (this.Turrets.Count - 1);
  }

  public bool HasTurret() => this.Turrets != null && this.Turrets.Count > 0;

  public float TurretTraverseRange()
  {
    return this.Turrets.Count == 0 ? 0.0f : this.Turrets[0].GetTraverseRange();
  }

  public int TurretCount() => this.Turrets.Count;

  public bool GetFiringConeDirection(out Vector3 aimDirection, out float coneAngle)
  {
    aimDirection = Vector3.zero;
    coneAngle = 0.0f;
    Vector3 firingConeForward;
    float angle;
    if (this.Turrets == null || this.Turrets.Count == 0 || !this.Turrets[0].HasFiringCone(out firingConeForward, out angle))
      return false;
    aimDirection = firingConeForward;
    coneAngle = angle;
    return true;
  }

  public Turret GetTurret()
  {
    return this.Turrets == null || this.Turrets.Count == 0 ? (Turret) null : this.Turrets[0];
  }

  public Vector2 GetTurretRelativeAim()
  {
    Vector2 turretRelativeAim = Vector2.zero;
    if (this.Turrets != null || this.Turrets.Count > 0)
      turretRelativeAim = this.Turrets[0].GetTurretAimError();
    return turretRelativeAim;
  }

  public void Updated()
  {
    Action onUpdated = this.OnUpdated;
    if (onUpdated == null)
      return;
    onUpdated();
  }

  public void Rearm()
  {
    foreach (Weapon weapon in this.Weapons)
      weapon.Rearm();
    this.AccountAmmo();
    this.Reloading = false;
    this.weaponIndex = 0;
    this.Updated();
  }

  public float GetAmmoLevel() => (float) (this.Ammo / Mathf.Max(this.FullAmmo, 0));

  public void SetStationTargets(ReadOnlySpan<PersistentID> targetIDs)
  {
    if (targetIDs.Length == 0)
    {
      foreach (Weapon weapon in this.Weapons)
        weapon.SetTarget((Unit) null);
      foreach (Turret turret in this.Turrets)
        turret.SetTarget(PersistentID.None, this.Number);
    }
    else
    {
      int num1 = 0;
      foreach (Turret turret in this.Turrets)
      {
        if (num1 >= targetIDs.Length)
          num1 = 0;
        PersistentID id = targetIDs[num1];
        int number = (int) this.Number;
        turret.SetTarget(id, (byte) number);
        ++num1;
      }
      int num2 = 0;
      for (int index = 0; index < this.Weapons.Count; ++index)
      {
        if (num2 >= targetIDs.Length)
          num2 = 0;
        Unit unit;
        if (UnitRegistry.TryGetUnit(new PersistentID?(targetIDs[num2]), out unit))
          this.Weapons[index].SetTarget(unit);
        ++num2;
      }
    }
  }

  public void SetStationTurretTarget(byte turretIndex, PersistentID targetID)
  {
    if (!this.HasTurret() || (int) turretIndex >= this.Turrets.Count)
      return;
    this.Turrets[(int) turretIndex].SetTarget(targetID, this.Number);
  }

  public Unit GetStationTarget()
  {
    Unit stationTarget = (Unit) null;
    foreach (Weapon weapon in this.Weapons)
      stationTarget = weapon.GetTarget();
    foreach (Turret turret in this.Turrets)
      stationTarget = turret.GetTarget();
    return stationTarget;
  }

  public float PrioritizeByPosition(Transform transform, Aircraft aircraft)
  {
    Vector3 vector3 = (transform.position - aircraft.transform.position) with
    {
      y = 0.0f
    };
    return vector3.sqrMagnitude + 0.1f * Vector3.Dot(vector3.normalized, aircraft.transform.right);
  }

  public float GenerateCargoPriority(Weapon mount, Aircraft aircraft)
  {
    return Vector3.Dot((mount.transform.position - aircraft.transform.position) with
    {
      y = 0.0f
    }, aircraft.transform.forward);
  }

  public void SortWeapons(Aircraft aircraft)
  {
    for (int index = this.Weapons.Count - 1; index >= 0; --index)
    {
      if ((UnityEngine.Object) this.Weapons[index] == (UnityEngine.Object) null)
        this.Weapons.RemoveAt(index);
    }
    if (this.Cargo)
      this.Weapons.Sort((Comparison<Weapon>) ((a, b) => this.GenerateCargoPriority(a, aircraft).CompareTo(this.GenerateCargoPriority(b, aircraft))));
    if (this.Weapons.Count < 2 || !this.sortWeapons)
      return;
    this.Weapons.Sort((Comparison<Weapon>) ((a, b) => this.PrioritizeByPosition(b.transform, aircraft).CompareTo(this.PrioritizeByPosition(a.transform, aircraft))));
  }

  public void AssessAmmo()
  {
    if (this.Weapons.Count > 0 && (UnityEngine.Object) this.Weapons[0] != (UnityEngine.Object) null)
      this.WeaponInfo = this.Weapons[0].info;
    foreach (Weapon weapon in this.Weapons)
      weapon.SetWeaponStation(this);
  }

  public OpportunityThreat CalcOpportunityThreat(UnitDefinition definition, Unit attachedUnit)
  {
    if (this.TypeLookup == null)
      this.TypeLookup = new Dictionary<UnitDefinition, OpportunityThreat>();
    if (!this.TypeLookup.ContainsKey(definition))
      this.TypeLookup.Add(definition, new OpportunityThreat(definition.GetOpportunity(this.WeaponInfo.effectiveness) * definition.GetOpportunity(attachedUnit.definition.roleIdentity), attachedUnit.definition.ThreatPosedBy(definition.roleIdentity)));
    return this.TypeLookup[definition];
  }

  public void RemoteFireAuto(Unit owner)
  {
    for (int index = 0; index < this.Weapons.Count; ++index)
    {
      if ((UnityEngine.Object) this.Weapons[index] != (UnityEngine.Object) null)
      {
        Vector3 inheritedVelocity = (UnityEngine.Object) owner.rb != (UnityEngine.Object) null ? owner.rb.velocity : new Vector3();
        this.Weapons[index].Fire(owner, (Unit) null, inheritedVelocity, this, new GlobalPosition());
      }
    }
  }

  public void RemoteFireSingle(Unit owner)
  {
    for (int index = 0; index < this.Weapons.Count; ++index)
    {
      if ((UnityEngine.Object) this.Weapons[index] != (UnityEngine.Object) null)
      {
        Vector3 inheritedVelocity = (UnityEngine.Object) owner.rb != (UnityEngine.Object) null ? owner.rb.velocity : new Vector3();
        this.Weapons[index].RemoteSingleFire(owner, (Unit) null, inheritedVelocity, this, new GlobalPosition());
      }
    }
  }

  public void LaunchMount(Unit owner, Unit target, GlobalPosition aimpoint)
  {
    if (this.weaponIndex >= this.Weapons.Count)
      return;
    Weapon weapon = this.Weapons[this.weaponIndex];
    int roundsFired = 0;
    if ((UnityEngine.Object) weapon != (UnityEngine.Object) null && weapon.IsAttached())
      weapon.Fire(owner, target, owner.rb.velocity, this, aimpoint);
    if (!this.WeaponInfo.troops && !this.WeaponInfo.sling)
    {
      roundsFired = 1;
      ++this.weaponIndex;
    }
    if (this.Cargo && this.weaponIndex < this.Weapons.Count)
      this.WeaponInfo = this.Weapons[this.weaponIndex].info;
    this.UpdateLastFired(roundsFired);
    this.Updated();
  }

  public void Fire(Unit owner, Unit target)
  {
    if (owner is Aircraft aircraft && this.SafetyIsOn(aircraft))
      return;
    for (int index = 0; index < this.Weapons.Count; ++index)
    {
      Weapon weapon = this.Weapons[index];
      Vector3 inheritedVelocity = (UnityEngine.Object) owner.rb != (UnityEngine.Object) null ? owner.rb.velocity : new Vector3();
      if ((UnityEngine.Object) weapon != (UnityEngine.Object) null && weapon.IsAttached())
        weapon.Fire(owner, target, inheritedVelocity, this, new GlobalPosition());
    }
    if ((double) this.WeaponInfo.fireInterval != 0.0)
      return;
    owner.SetFiringState((int) this.Number, true);
  }

  public void AccountAmmo()
  {
    this.Ammo = 0;
    this.FullAmmo = 0;
    foreach (Weapon weapon in this.Weapons)
    {
      this.Ammo += weapon.GetAmmoTotal();
      this.FullAmmo += weapon.GetFullAmmo();
    }
    this.Full = this.Ammo == this.FullAmmo;
  }

  public void UpdateLastFired(int roundsFired)
  {
    if (NetworkManagerNuclearOption.i.Server.Active)
      this.AccountAmmo();
    else
      this.Ammo -= roundsFired;
    this.LastFiredTime = Time.timeSinceLevelLoad;
  }
}

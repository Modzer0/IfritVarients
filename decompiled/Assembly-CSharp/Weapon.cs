// Decompiled with JetBrains decompiler
// Type: Weapon
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

#nullable disable
[Serializable]
public class Weapon : MonoBehaviour
{
  public Unit attachedUnit;
  public WeaponInfo info;
  public int ammo;
  protected WeaponStation weaponStation;
  protected Hardpoint hardpoint;
  protected WeaponMount mount;
  protected float lastFired;
  protected Unit currentTarget;
  public bool Rearmable = true;
  [HideInInspector]
  public bool Safety;

  public virtual void Fire(
    Unit owner,
    Unit target,
    Vector3 inheritedVelocity,
    WeaponStation weaponStation,
    GlobalPosition aimpoint)
  {
  }

  public virtual void RemoteSingleFire(
    Unit owner,
    Unit target,
    Vector3 inheritedVelocity,
    WeaponStation weaponStation,
    GlobalPosition aimpoint)
  {
  }

  public virtual void AttachToUnit(Unit unit) => this.attachedUnit = unit;

  public virtual void SetTarget(Unit unit)
  {
  }

  public async UniTask TrackFiringVisibility()
  {
    Weapon weapon = this;
    CancellationToken cancel;
    if ((double) Time.timeSinceLevelLoad - (double) weapon.lastFired < 4.0)
    {
      cancel = new CancellationToken();
    }
    else
    {
      weapon.lastFired = Time.timeSinceLevelLoad;
      weapon.attachedUnit.ModifyVisibility(weapon.info.visibilityWhenFired);
      cancel = weapon.destroyCancellationToken;
      while ((double) Time.timeSinceLevelLoad - (double) weapon.lastFired < 4.0)
      {
        await UniTask.Delay(1000);
        if (cancel.IsCancellationRequested)
        {
          cancel = new CancellationToken();
          return;
        }
      }
      weapon.attachedUnit.ModifyVisibility(-weapon.info.visibilityWhenFired);
      cancel = new CancellationToken();
    }
  }

  public virtual void Rearm()
  {
  }

  public virtual int GetAmmoLoaded() => 1;

  public virtual int GetAmmoTotal() => 1;

  public virtual int GetFullAmmo() => 1;

  public virtual float GetReloadProgress() => 0.0f;

  public virtual bool HasMagazines() => false;

  public bool IsAttached() => this.hardpoint == null || !this.hardpoint.part.IsDetached();

  public virtual void AttachToHardpoint(
    Aircraft aircraft,
    Hardpoint hardpoint,
    WeaponMount weaponMount)
  {
    this.AttachToUnit((Unit) aircraft);
    this.hardpoint = hardpoint;
    this.mount = weaponMount;
  }

  public void SetWeaponStation(WeaponStation weaponStation) => this.weaponStation = weaponStation;

  public void ReportReloading(bool reloading)
  {
    if (this.weaponStation == null)
      return;
    this.weaponStation.Reloading = reloading;
  }

  public virtual float GetMass() => this.info.massPerRound * (float) this.GetAmmoTotal();

  public Unit GetTarget() => this.currentTarget;
}

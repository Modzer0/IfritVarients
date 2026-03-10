// Decompiled with JetBrains decompiler
// Type: Hardpoint
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;

#nullable disable
[Serializable]
public class Hardpoint
{
  public Transform transform;
  public UnitPart part;
  public BayDoor[] bayDoors;
  public float doorOpenDuration;
  private WeaponMount mount;
  [SerializeField]
  private Hardpoint.HardpointPylon[] pylonOptions;
  public Renderer Pylon;
  public Renderer Plug;
  public Weapon[] BuiltInWeapons;
  public Turret[] BuiltInTurrets;
  private GameObject spawnedPrefab;

  public GameObject SpawnMount(Aircraft aircraft, WeaponMount weaponMount)
  {
    if ((UnityEngine.Object) this.transform == (UnityEngine.Object) null || this.part.IsDetached())
      return (GameObject) null;
    if ((UnityEngine.Object) this.spawnedPrefab != (UnityEngine.Object) null)
      Debug.LogError((object) $"attempting to spawn {weaponMount.mountName} on pylon which is already occupied!");
    foreach (Turret builtInTurret in this.BuiltInTurrets)
      builtInTurret.AttachToWeaponManager(aircraft);
    foreach (Gun builtInWeapon in this.BuiltInWeapons)
    {
      builtInWeapon.LoadAmmunition(weaponMount);
      aircraft.weaponManager.RegisterWeapon((Weapon) builtInWeapon, weaponMount, this);
    }
    this.mount = weaponMount;
    this.ModifyMass(this.mount.emptyMass);
    this.ModifyDrag(this.mount.emptyDrag);
    this.ModifyRCS(this.mount.emptyRCS);
    this.spawnedPrefab = UnityEngine.Object.Instantiate<GameObject>(weaponMount.prefab, this.transform);
    ColorableMount component1;
    if (this.spawnedPrefab.TryGetComponent<ColorableMount>(out component1))
      component1.AttachToAircraft(aircraft);
    if (weaponMount.radar)
    {
      this.spawnedPrefab.GetComponentInChildren<Radar>().AttachToUnit((Unit) aircraft);
      AeroPart component2 = this.spawnedPrefab.GetComponent<AeroPart>();
      if ((UnityEngine.Object) component2 != (UnityEngine.Object) null && aircraft.LocalSim)
      {
        component2.CreateRB(aircraft.rb.GetPointVelocity(this.transform.position), this.transform.position);
        component2.CreateJoints();
      }
    }
    if (weaponMount.countermeasure)
      this.spawnedPrefab.GetComponentInChildren<Countermeasure>().AttachToUnit(aircraft);
    foreach (Weapon componentsInChild in this.spawnedPrefab.GetComponentsInChildren<Weapon>())
      aircraft.weaponManager.RegisterWeapon(componentsInChild, weaponMount, this);
    if (weaponMount.turret)
      this.spawnedPrefab.GetComponentInChildren<Turret>().AttachToWeaponManager(aircraft);
    return this.spawnedPrefab;
  }

  public void SpringOpenBayDoors()
  {
    foreach (BayDoor bayDoor in this.bayDoors)
    {
      if ((UnityEngine.Object) bayDoor != (UnityEngine.Object) null)
        bayDoor.OpenDoor(this.doorOpenDuration);
    }
  }

  public void ModifyMass(float change)
  {
    if ((double) change == 0.0 || !((UnityEngine.Object) this.part != (UnityEngine.Object) null) || !((UnityEngine.Object) this.part.rb != (UnityEngine.Object) null))
      return;
    this.part.ModifyMass(change);
  }

  public BayDoor GetCargoDoor() => this.bayDoors.Length == 0 ? (BayDoor) null : this.bayDoors[0];

  public void ModifyDrag(float change) => this.part.ModifyDrag(change);

  public void ModifyRCS(float change) => this.part.parentUnit.ModifyRCS(change);

  public void RemoveMount()
  {
    if ((UnityEngine.Object) this.mount == (UnityEngine.Object) null)
      return;
    this.ModifyRCS(-this.mount.emptyRCS);
    this.ModifyDrag(-this.mount.emptyDrag);
    this.ModifyMass(-this.mount.emptyMass);
    foreach (Gun builtInWeapon in this.BuiltInWeapons)
      builtInWeapon.LoadAmmunition((WeaponMount) null);
    this.mount = (WeaponMount) null;
    if ((UnityEngine.Object) this.spawnedPrefab == (UnityEngine.Object) null)
      return;
    UnitPart component;
    if (this.spawnedPrefab.TryGetComponent<UnitPart>(out component))
      component.RemoveFromUnit();
    UnityEngine.Object.Destroy((UnityEngine.Object) this.spawnedPrefab);
    this.spawnedPrefab = (GameObject) null;
  }

  public void ShowPylon(bool weaponLoaded)
  {
    if (weaponLoaded)
    {
      if ((UnityEngine.Object) this.Plug != (UnityEngine.Object) null)
        this.Plug.enabled = false;
      bool visible = false;
      foreach (Hardpoint.HardpointPylon pylonOption in this.pylonOptions)
      {
        if (visible)
        {
          pylonOption.ShowPylon(false);
        }
        else
        {
          visible = pylonOption.MatchesMount(this.mount);
          pylonOption.ShowPylon(visible);
        }
      }
    }
    else
    {
      foreach (Hardpoint.HardpointPylon pylonOption in this.pylonOptions)
        pylonOption.ShowPylon(false);
      if (!((UnityEngine.Object) this.Plug != (UnityEngine.Object) null))
        return;
      this.Plug.enabled = true;
    }
  }

  [Serializable]
  private class HardpointPylon
  {
    [SerializeField]
    private bool cargo;
    [SerializeField]
    private WeaponMount mount;
    [SerializeField]
    private Renderer renderer;

    public bool MatchesMount(WeaponMount mount)
    {
      if ((UnityEngine.Object) mount == (UnityEngine.Object) null)
        return false;
      return this.cargo ? mount.Cargo || mount.Troops : (UnityEngine.Object) this.mount == (UnityEngine.Object) mount || (UnityEngine.Object) this.mount == (UnityEngine.Object) null;
    }

    public void ShowPylon(bool visible) => this.renderer.enabled = visible;
  }
}

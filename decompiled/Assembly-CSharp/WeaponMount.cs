// Decompiled with JetBrains decompiler
// Type: WeaponMount
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;

#nullable disable
[CreateAssetMenu(fileName = "New Weapon Mount", menuName = "ScriptableObjects/WeaponMount", order = 4)]
public class WeaponMount : ScriptableObject, INetworkDefinition, IHasJsonKey
{
  public GameObject prefab;
  [Tooltip("name used it look up to find unit, saved in mission json")]
  public string jsonKey;
  public WeaponInfo info;
  public string mountName;
  public int ammo;
  public bool turret;
  public bool missileBay;
  public bool radar;
  public bool tailHook;
  public bool slingloadHook;
  public bool countermeasure;
  public bool colorable;
  public bool Cargo;
  public bool Troops;
  public bool sortWeapons = true;
  public bool GearSafety = true;
  public bool GroundSafety = true;
  public bool GunAmmo;
  public float emptyCost;
  public float emptyMass;
  [HideInInspector]
  public float mass;
  public float drag;
  public float emptyDrag;
  public float RCS;
  public float emptyRCS;
  public bool disabled;
  public bool dontAutomaticallyAddToEncyclopedia;

  [field: NonSerialized]
  int? INetworkDefinition.LookupIndex { get; set; }

  string IHasJsonKey.JsonKey
  {
    get => this.jsonKey;
    set
    {
      if (!Application.isEditor)
        throw new Exception("JsonKey should only be set in UnityEditor");
      this.jsonKey = value;
    }
  }

  public void Initialize()
  {
    if (this.Cargo)
    {
      foreach (Unit componentsInChild in this.prefab.GetComponentsInChildren<Unit>())
        this.mass += componentsInChild.GetMass();
    }
    if ((UnityEngine.Object) this.info != (UnityEngine.Object) null)
    {
      if ((UnityEngine.Object) this.info.weaponPrefab != (UnityEngine.Object) null)
      {
        Missile component = this.info.weaponPrefab.GetComponent<Missile>();
        this.info.SetMassPerRound(component.definition.mass);
        this.info.SetCostPerRound(component.definition.value);
        this.mass = this.emptyMass + this.info.massPerRound * (float) this.ammo;
        Weapon[] componentsInChildren = this.prefab.GetComponentsInChildren<Weapon>();
        if (componentsInChildren.Length == 0)
          return;
        this.ammo = componentsInChildren.Length;
        this.mountName = this.ammo > 1 ? $"{this.info.weaponName} x{this.ammo}" : this.info.weaponName ?? "";
        if (!((UnityEngine.Object) componentsInChildren[0].info != (UnityEngine.Object) null))
          return;
        this.info = componentsInChildren[0].info;
      }
      else
      {
        Gun componentInChildren = this.prefab.GetComponentInChildren<Gun>();
        if ((UnityEngine.Object) componentInChildren != (UnityEngine.Object) null)
        {
          this.ammo = componentInChildren.GetFullAmmo();
          this.mountName = this.ammo > 1 ? $"{this.info.weaponName} ({this.ammo} rounds)" : this.info.weaponName ?? "";
          this.info = componentInChildren.info;
        }
        if (!this.Cargo)
          return;
        Weapon[] componentsInChildren = this.prefab.GetComponentsInChildren<Weapon>();
        if (componentsInChildren.Length == 0)
          return;
        this.ammo = componentsInChildren.Length;
        if (!((UnityEngine.Object) componentsInChildren[0].info != (UnityEngine.Object) null))
          return;
        this.info = componentsInChildren[0].info;
      }
    }
    else
    {
      TailHook componentInChildren = this.prefab.GetComponentInChildren<TailHook>();
      if (!((UnityEngine.Object) componentInChildren != (UnityEngine.Object) null))
        return;
      this.mass = componentInChildren.GetMass();
    }
  }

  public float GetDragPerRound() => (this.drag - this.emptyDrag) / (float) this.ammo;

  public float GetRCSPerRound() => (this.RCS - this.emptyRCS) / (float) this.ammo;
}

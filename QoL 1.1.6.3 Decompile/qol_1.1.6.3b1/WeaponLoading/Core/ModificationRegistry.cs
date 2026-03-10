// Decompiled with JetBrains decompiler
// Type: qol.WeaponLoading.Core.ModificationRegistry
// Assembly: qol, Version=1.1.6.3, Culture=neutral, PublicKeyToken=null
// MVID: 3B6539EA-E38C-45EE-8FFF-FC58CC42BADC
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\BepInEx\plugins\qol_1.1.6.3b1.dll

using System.Collections.Generic;
using UnityEngine;

#nullable disable
namespace qol.WeaponLoading.Core;

public class ModificationRegistry
{
  private readonly Dictionary<string, GameObject> _gameObjects = new Dictionary<string, GameObject>();
  private readonly Dictionary<string, WeaponMount> _weaponMounts = new Dictionary<string, WeaponMount>();
  private readonly Dictionary<string, WeaponInfo> _weaponInfos = new Dictionary<string, WeaponInfo>();
  private readonly Dictionary<string, MissileDefinition> _missileDefinitions = new Dictionary<string, MissileDefinition>();
  private readonly Dictionary<string, Material> _materials = new Dictionary<string, Material>();

  public void RegisterGameObject(string key, GameObject obj) => this._gameObjects[key] = obj;

  public GameObject GetGameObject(string key)
  {
    GameObject gameObject;
    return !this._gameObjects.TryGetValue(key, out gameObject) ? (GameObject) null : gameObject;
  }

  public void RegisterWeaponMount(string key, WeaponMount mount) => this._weaponMounts[key] = mount;

  public WeaponMount GetWeaponMount(string key)
  {
    WeaponMount weaponMount;
    return !this._weaponMounts.TryGetValue(key, out weaponMount) ? (WeaponMount) null : weaponMount;
  }

  public void RegisterWeaponInfo(string key, WeaponInfo info) => this._weaponInfos[key] = info;

  public WeaponInfo GetWeaponInfo(string key)
  {
    WeaponInfo weaponInfo;
    return !this._weaponInfos.TryGetValue(key, out weaponInfo) ? (WeaponInfo) null : weaponInfo;
  }

  public void RegisterMissileDefinition(string key, MissileDefinition def)
  {
    this._missileDefinitions[key] = def;
  }

  public MissileDefinition GetMissileDefinition(string key)
  {
    MissileDefinition missileDefinition;
    return !this._missileDefinitions.TryGetValue(key, out missileDefinition) ? (MissileDefinition) null : missileDefinition;
  }

  public void RegisterMaterial(string key, Material mat) => this._materials[key] = mat;

  public Material GetMaterial(string key)
  {
    Material material;
    return !this._materials.TryGetValue(key, out material) ? (Material) null : material;
  }

  public IEnumerable<KeyValuePair<string, GameObject>> GetAllGameObjects()
  {
    return (IEnumerable<KeyValuePair<string, GameObject>>) this._gameObjects;
  }

  public IEnumerable<KeyValuePair<string, WeaponMount>> GetAllWeaponMounts()
  {
    return (IEnumerable<KeyValuePair<string, WeaponMount>>) this._weaponMounts;
  }
}

// Decompiled with JetBrains decompiler
// Type: Encyclopedia
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

#nullable disable
[CreateAssetMenu(fileName = "Encyclopedia", menuName = "ScriptableObjects/Encyclopedia", order = 999)]
public class Encyclopedia : ScriptableObject
{
  private static readonly ResourcesAsyncLoader<Encyclopedia> loader = ResourcesAsyncLoader.Create<Encyclopedia>(nameof (Encyclopedia), new Action<Encyclopedia>(Encyclopedia.AfterLoad));
  public List<AircraftDefinition> aircraft;
  public List<VehicleDefinition> vehicles;
  public List<MissileDefinition> missiles;
  public List<BuildingDefinition> buildings;
  public List<ShipDefinition> ships;
  public List<SceneryDefinition> scenery;
  public List<UnitDefinition> otherUnits;
  public List<WeaponMount> weaponMounts;
  public static Dictionary<string, UnitDefinition> Lookup;
  public static Dictionary<string, WeaponMount> WeaponLookup;
  private static Dictionary<UnitDefinition, float> massLookup;
  public List<INetworkDefinition> IndexLookup;

  public static Encyclopedia i => Encyclopedia.loader.Get();

  public static async UniTask Preload(CancellationToken cancel)
  {
    await Encyclopedia.loader.Load(cancel);
  }

  private static void AfterLoad(Encyclopedia instance) => instance.AfterLoad();

  public void SortByValue()
  {
    this.aircraft.Sort((Comparison<AircraftDefinition>) ((a, b) => a.value.CompareTo(b.value)));
    this.vehicles.Sort((Comparison<VehicleDefinition>) ((a, b) => a.value.CompareTo(b.value)));
  }

  private void AfterLoad()
  {
    if (Encyclopedia.Lookup == null)
      Encyclopedia.Lookup = new Dictionary<string, UnitDefinition>();
    Encyclopedia.Lookup.Clear();
    if (Encyclopedia.WeaponLookup == null)
      Encyclopedia.WeaponLookup = new Dictionary<string, WeaponMount>();
    Encyclopedia.WeaponLookup.Clear();
    if (this.IndexLookup == null)
      this.IndexLookup = new List<INetworkDefinition>();
    this.IndexLookup.Clear();
    if (Encyclopedia.massLookup == null)
      Encyclopedia.massLookup = new Dictionary<UnitDefinition, float>();
    Encyclopedia.massLookup.Clear();
    foreach (UnitDefinition unitDefinition in this.aircraft)
      unitDefinition.CacheMass();
    foreach (UnitDefinition vehicle in this.vehicles)
      vehicle.CacheMass();
    foreach (UnitDefinition missile in this.missiles)
      missile.CacheMass();
    foreach (UnitDefinition ship in this.ships)
      ship.CacheMass();
    foreach (UnitDefinition otherUnit in this.otherUnits)
      otherUnit.CacheMass();
    foreach (WeaponMount weaponMount in this.weaponMounts)
      weaponMount.Initialize();
    this.SortByValue();
    AddList<AircraftDefinition, UnitDefinition>(Encyclopedia.Lookup, this.aircraft);
    AddList<VehicleDefinition, UnitDefinition>(Encyclopedia.Lookup, this.vehicles);
    AddList<MissileDefinition, UnitDefinition>(Encyclopedia.Lookup, this.missiles);
    AddList<BuildingDefinition, UnitDefinition>(Encyclopedia.Lookup, this.buildings);
    AddList<ShipDefinition, UnitDefinition>(Encyclopedia.Lookup, this.ships);
    AddList<SceneryDefinition, UnitDefinition>(Encyclopedia.Lookup, this.scenery);
    AddList<UnitDefinition, UnitDefinition>(Encyclopedia.Lookup, this.otherUnits);
    AddList<WeaponMount, WeaponMount>(Encyclopedia.WeaponLookup, this.weaponMounts);
    AddWithIndex<AircraftDefinition>(this.IndexLookup, this.aircraft);
    AddWithIndex<VehicleDefinition>(this.IndexLookup, this.vehicles);
    AddWithIndex<MissileDefinition>(this.IndexLookup, this.missiles);
    AddWithIndex<BuildingDefinition>(this.IndexLookup, this.buildings);
    AddWithIndex<ShipDefinition>(this.IndexLookup, this.ships);
    AddWithIndex<SceneryDefinition>(this.IndexLookup, this.scenery);
    AddWithIndex<UnitDefinition>(this.IndexLookup, this.otherUnits);
    AddWithIndex<WeaponMount>(this.IndexLookup, this.weaponMounts);

    static void AddList<TItem, TBase>(Dictionary<string, TBase> lookup, List<TItem> items)
      where TItem : TBase
      where TBase : ScriptableObject, IHasJsonKey
    {
      foreach (TItem obj in items)
      {
        if ((UnityEngine.Object) obj == (UnityEngine.Object) null)
          Debug.LogError((object) $"Null found in Encyclopedia list for, {typeof (TItem)}");
        else
          lookup.Add(obj.JsonKey, (TBase) obj);
      }
    }

    static void AddWithIndex<T>(List<INetworkDefinition> lookup, List<T> items) where T : ScriptableObject, INetworkDefinition
    {
      foreach (T obj in items)
      {
        if (!((UnityEngine.Object) obj == (UnityEngine.Object) null))
        {
          obj.LookupIndex = new int?(lookup.Count);
          lookup.Add((INetworkDefinition) obj);
        }
      }
    }
  }

  public bool TryGetPrefab(string key, out GameObject prefab)
  {
    UnitDefinition unitDefinition;
    if (Encyclopedia.Lookup.TryGetValue(key, out unitDefinition))
    {
      prefab = unitDefinition.unitPrefab;
      return true;
    }
    Debug.LogError((object) $"Prefab with key '{key}' not found in Encyclopedia");
    prefab = (GameObject) null;
    return false;
  }

  public IEnumerable<UnitDefinition> GetAircraftAndVehicles()
  {
    return ((IEnumerable<UnitDefinition>) this.aircraft).Concat<UnitDefinition>((IEnumerable<UnitDefinition>) this.vehicles);
  }

  private void OnValidate()
  {
    Dictionary<string, IHasJsonKey> allKeys = new Dictionary<string, IHasJsonKey>(200);
    Validate<AircraftDefinition>(this.aircraft);
    Validate<VehicleDefinition>(this.vehicles);
    Validate<MissileDefinition>(this.missiles);
    Validate<BuildingDefinition>(this.buildings);
    Validate<ShipDefinition>(this.ships);
    Validate<SceneryDefinition>(this.scenery);
    Validate<UnitDefinition>(this.otherUnits);
    Validate<WeaponMount>(this.weaponMounts);

    void Validate<T>(List<T> assets) where T : UnityEngine.Object, IHasJsonKey
    {
      foreach (T asset in assets)
      {
        if ((UnityEngine.Object) asset == (UnityEngine.Object) null)
          Debug.LogWarning((object) "Null item count in Encyclopedia lists");
        else if (string.IsNullOrEmpty(asset.JsonKey))
        {
          asset.JsonKey = asset.name;
          Debug.LogWarning((object) $"Asset in Encyclopedia has no JsonKey, setting key to be asset name: {asset}", (UnityEngine.Object) asset);
        }
        else if (!allKeys.TryAdd(asset.JsonKey, (IHasJsonKey) asset))
          Debug.LogError((object) $"Two or more assets had the same JsonKey, old:{allKeys[asset.JsonKey]}, new:{asset}");
      }
    }
  }
}

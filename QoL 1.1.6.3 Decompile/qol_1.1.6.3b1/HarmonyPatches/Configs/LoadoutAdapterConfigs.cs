// Decompiled with JetBrains decompiler
// Type: qol.HarmonyPatches.Configs.LoadoutAdapterConfigs
// Assembly: qol, Version=1.1.6.3, Culture=neutral, PublicKeyToken=null
// MVID: 3B6539EA-E38C-45EE-8FFF-FC58CC42BADC
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\BepInEx\plugins\qol_1.1.6.3b1.dll

using System;
using System.Collections.Generic;

#nullable disable
namespace qol.HarmonyPatches.Configs;

public static class LoadoutAdapterConfigs
{
  public static readonly IReadOnlyDictionary<string, LoadoutAdapterConfigs.AircraftLoadoutAdapter> Adapters = (IReadOnlyDictionary<string, LoadoutAdapterConfigs.AircraftLoadoutAdapter>) new Dictionary<string, LoadoutAdapterConfigs.AircraftLoadoutAdapter>()
  {
    {
      "COIN",
      new LoadoutAdapterConfigs.AircraftLoadoutAdapter("COIN", 4, 5, weaponSpecificRemapping: (IReadOnlyDictionary<(int, string), int>) new Dictionary<(int, string), int>()
      {
        {
          (3, "ECMPod1"),
          4
        }
      }, vacatedSlotDefaults: (IReadOnlyDictionary<int, string>) new Dictionary<int, string>()
      {
        {
          3,
          "P_FlarePod1"
        }
      })
    },
    {
      "SmallFighter1",
      new LoadoutAdapterConfigs.AircraftLoadoutAdapter("SmallFighter1", 5, 6)
    },
    {
      "CAS1",
      new LoadoutAdapterConfigs.AircraftLoadoutAdapter("CAS1", 7, 8)
    },
    {
      "Fighter1",
      new LoadoutAdapterConfigs.AircraftLoadoutAdapter("Fighter1", 4, 5)
    },
    {
      "Darkreach",
      new LoadoutAdapterConfigs.AircraftLoadoutAdapter("Darkreach", 4, 5)
    }
  };

  public static bool TryGetAdapter(
    string aircraftName,
    out LoadoutAdapterConfigs.AircraftLoadoutAdapter adapter)
  {
    return LoadoutAdapterConfigs.Adapters.TryGetValue(aircraftName, out adapter);
  }

  public static bool IsVanillaLoadout(string aircraftName, int weaponCount)
  {
    LoadoutAdapterConfigs.AircraftLoadoutAdapter aircraftLoadoutAdapter;
    return LoadoutAdapterConfigs.Adapters.TryGetValue(aircraftName, out aircraftLoadoutAdapter) && weaponCount == aircraftLoadoutAdapter.VanillaHardpointCount;
  }

  public static List<WeaponMount> AdaptLoadout(
    string aircraftName,
    List<WeaponMount> vanillaWeapons,
    Func<string, WeaponMount> lookupWeapon)
  {
    LoadoutAdapterConfigs.AircraftLoadoutAdapter adapter;
    if (!LoadoutAdapterConfigs.Adapters.TryGetValue(aircraftName, out adapter))
      return (List<WeaponMount>) null;
    if (vanillaWeapons.Count != adapter.VanillaHardpointCount)
      return vanillaWeapons.Count < adapter.ModdedHardpointCount ? LoadoutAdapterConfigs.PadLoadout(vanillaWeapons, adapter, lookupWeapon) : (List<WeaponMount>) null;
    List<WeaponMount> weaponMountList = new List<WeaponMount>((IEnumerable<WeaponMount>) new WeaponMount[adapter.ModdedHardpointCount]);
    HashSet<int> intSet1 = new HashSet<int>();
    HashSet<int> intSet2 = new HashSet<int>();
    for (int index1 = 0; index1 < vanillaWeapons.Count; ++index1)
    {
      WeaponMount vanillaWeapon = vanillaWeapons[index1];
      string name = vanillaWeapon?.name;
      int index2;
      if (!string.IsNullOrEmpty(name) && adapter.WeaponSpecificRemapping.TryGetValue((index1, name), out index2) && index2 >= 0 && index2 < adapter.ModdedHardpointCount)
      {
        weaponMountList[index2] = vanillaWeapon;
        intSet1.Add(index2);
        intSet2.Add(index1);
      }
    }
    for (int index3 = 0; index3 < vanillaWeapons.Count; ++index3)
    {
      if (!intSet2.Contains(index3))
      {
        int num;
        int index4;
        if (adapter.SlotRemapping.TryGetValue(index3, out num))
        {
          if (num != -1)
            index4 = num;
          else
            continue;
        }
        else
          index4 = index3;
        if (index4 >= 0 && index4 < adapter.ModdedHardpointCount && !intSet1.Contains(index4))
        {
          weaponMountList[index4] = vanillaWeapons[index3];
          intSet1.Add(index4);
        }
      }
    }
    foreach (int num in intSet2)
    {
      string str;
      if (!intSet1.Contains(num) && adapter.VacatedSlotDefaults.TryGetValue(num, out str) && !string.IsNullOrEmpty(str))
      {
        weaponMountList[num] = lookupWeapon(str);
        intSet1.Add(num);
      }
    }
    for (int index = 0; index < adapter.ModdedHardpointCount; ++index)
    {
      string str;
      if (!intSet1.Contains(index) && adapter.NewSlotDefaults.TryGetValue(index, out str) && !string.IsNullOrEmpty(str))
        weaponMountList[index] = lookupWeapon(str);
    }
    return weaponMountList;
  }

  private static List<WeaponMount> PadLoadout(
    List<WeaponMount> weapons,
    LoadoutAdapterConfigs.AircraftLoadoutAdapter adapter,
    Func<string, WeaponMount> lookupWeapon)
  {
    List<WeaponMount> weaponMountList = new List<WeaponMount>((IEnumerable<WeaponMount>) weapons);
    while (weaponMountList.Count < adapter.ModdedHardpointCount)
    {
      int count = weaponMountList.Count;
      string str;
      if (adapter.NewSlotDefaults.TryGetValue(count, out str) && !string.IsNullOrEmpty(str))
        weaponMountList.Add(lookupWeapon(str));
      else
        weaponMountList.Add((WeaponMount) null);
    }
    return weaponMountList;
  }

  public struct AircraftLoadoutAdapter(
    string aircraftName,
    int vanillaCount,
    int moddedCount,
    IReadOnlyDictionary<int, int> slotRemapping = null,
    IReadOnlyDictionary<int, string> newSlotDefaults = null,
    IReadOnlyDictionary<(int, string), int> weaponSpecificRemapping = null,
    IReadOnlyDictionary<int, string> vacatedSlotDefaults = null)
  {
    public string AircraftName = aircraftName;
    public int VanillaHardpointCount = vanillaCount;
    public int ModdedHardpointCount = moddedCount;
    public IReadOnlyDictionary<int, int> SlotRemapping = slotRemapping ?? (IReadOnlyDictionary<int, int>) new Dictionary<int, int>();
    public IReadOnlyDictionary<int, string> NewSlotDefaults = newSlotDefaults ?? (IReadOnlyDictionary<int, string>) new Dictionary<int, string>();
    public IReadOnlyDictionary<(int, string), int> WeaponSpecificRemapping = weaponSpecificRemapping ?? (IReadOnlyDictionary<(int, string), int>) new Dictionary<(int, string), int>();
    public IReadOnlyDictionary<int, string> VacatedSlotDefaults = vacatedSlotDefaults ?? (IReadOnlyDictionary<int, string>) new Dictionary<int, string>();
  }
}

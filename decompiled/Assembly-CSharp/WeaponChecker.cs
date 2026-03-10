// Decompiled with JetBrains decompiler
// Type: WeaponChecker
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.Networking;
using NuclearOption.SavedMission;
using System.Collections.Generic;
using UnityEngine;

#nullable disable
public static class WeaponChecker
{
  public static bool CanAffordRearm(Player player, WeaponManager weaponManager, bool includeCargo)
  {
    float valueInMillions = WeaponChecker.GetLoadoutFullValue(weaponManager, includeCargo) - weaponManager.GetCurrentValue(includeCargo);
    int num = WeaponChecker.GetLoadoutFullWarheads(weaponManager) - weaponManager.GetCurrentWarheads();
    Debug.Log((object) $"Checking rearm affordability: current value: {UnitConverter.ValueReading(weaponManager.GetCurrentValue(includeCargo))}, rearmed value: {UnitConverter.ValueReading(WeaponChecker.GetLoadoutFullValue(weaponManager, includeCargo))}, difference: {UnitConverter.ValueReading(valueInMillions)}, player funds: {UnitConverter.ValueReading(player.Allocation)}");
    return (double) valueInMillions <= (double) player.Allocation && num <= player.HQ.GetWarheadStockpile();
  }

  public static float GetLoadoutFullValue(WeaponManager weaponManager, bool includeCargo)
  {
    float loadoutFullValue = 0.0f;
    Loadout currentLoadout = weaponManager.GetCurrentLoadout();
    for (int index = 0; index < currentLoadout.weapons.Count; ++index)
    {
      int count = weaponManager.hardpointSets[index].hardpoints.Count;
      WeaponMount weapon = currentLoadout.weapons[index];
      if (!((Object) weapon == (Object) null) && !((Object) weapon.info == (Object) null) && (includeCargo || !weapon.info.cargo))
        loadoutFullValue += (float) count * (weapon.emptyCost + weapon.info.costPerRound * (float) weapon.ammo);
    }
    return loadoutFullValue;
  }

  public static int GetLoadoutFullWarheads(WeaponManager weaponManager)
  {
    int loadoutFullWarheads = 0;
    Loadout currentLoadout = weaponManager.GetCurrentLoadout();
    for (int index = 0; index < currentLoadout.weapons.Count; ++index)
    {
      int count = weaponManager.hardpointSets[index].hardpoints.Count;
      WeaponMount weapon = currentLoadout.weapons[index];
      if (!((Object) weapon == (Object) null) && !((Object) weapon.info == (Object) null) && weapon.info.nuclear)
        loadoutFullWarheads += count * weapon.ammo;
    }
    return loadoutFullWarheads;
  }

  public static void VetLoadout(
    AircraftDefinition definition,
    Loadout loadout,
    Player player,
    FactionHQ hq)
  {
    WeaponManager weaponManager = definition.unitPrefab.GetComponent<Aircraft>().weaponManager;
    float allocation = player.Allocation;
    for (int index = 0; index < loadout.weapons.Count; ++index)
    {
      HardpointSet hardpointSet = weaponManager.hardpointSets[index];
      if (hardpointSet.BlockedByOtherHardpoint(loadout))
        loadout.weapons[index] = (WeaponMount) null;
      int count = hardpointSet.hardpoints.Count;
      WeaponMount weapon = loadout.weapons[index];
      if ((Object) weapon != (Object) null && !hardpointSet.weaponOptions.Contains(weapon))
      {
        Debug.LogWarning((object) "WeaponMount was not in hardpointSet's options, removing it from loadout");
        loadout.weapons[index] = (WeaponMount) null;
      }
      else if (!((Object) weapon == (Object) null) && !((Object) weapon.info == (Object) null))
      {
        float num = (float) count * (weapon.emptyCost + ((Object) weapon.info != (Object) null ? weapon.info.costPerRound : 0.0f) * (float) weapon.ammo);
        if ((double) num > (double) allocation || !WeaponChecker.VetWeapon(weapon, hq, count))
        {
          if ((double) num > (double) allocation)
            Debug.LogError((object) $"{weapon} (value: {num}, hardpoints: {count}, ammo: {weapon.ammo}) exceeds remaining loadout budget for {player.PlayerName}");
          num = 0.0f;
          loadout.weapons[index] = (WeaponMount) null;
        }
        allocation -= num;
      }
    }
  }

  public static bool VetWeapon(WeaponMount weaponMount, FactionHQ hq, int hardpoints)
  {
    return !hq.restrictedWeapons.Contains(weaponMount.mountName) && (!((Object) weaponMount.info != (Object) null) || !weaponMount.info.nuclear || hq.GetWarheadStockpile() >= weaponMount.ammo * hardpoints && MissionManager.AllowTactical() && (!weaponMount.info.strategic || MissionManager.AllowStrategic()));
  }

  public static void GetAvailableWeaponsNonAlloc(
    Airbase airbase,
    int? playerRank,
    HardpointSet hardpointSet,
    FactionHQ HQ,
    int? warheadsAvailable,
    bool includeEmpty,
    List<WeaponMount> outAvailable)
  {
    outAvailable.Clear();
    for (int index = 1; index < hardpointSet.weaponOptions.Count; ++index)
    {
      WeaponMount weaponOption = hardpointSet.weaponOptions[index];
      if ((Object) weaponOption == (Object) null)
      {
        if (includeEmpty)
          outAvailable.Add(weaponOption);
      }
      else if (!weaponOption.disabled)
      {
        if ((Object) weaponOption.info != (Object) null && (Object) HQ != (Object) null)
        {
          if (!HQ.restrictedWeapons.Contains(weaponOption.name) && (!weaponOption.info.nuclear || MissionManager.AllowTactical() && (!weaponOption.info.strategic || MissionManager.AllowStrategic()) && (!playerRank.HasValue || (double) playerRank.Value >= (double) NetworkSceneSingleton<MissionManager>.i.tacticalMinRank && (!weaponOption.info.strategic || (double) playerRank.Value >= (double) NetworkSceneSingleton<MissionManager>.i.strategicMinRank)) && (!warheadsAvailable.HasValue || hardpointSet.hardpoints.Count * weaponOption.ammo <= warheadsAvailable.Value)))
          {
            if (weaponOption.info.cargo)
            {
              if (airbase.TryGetAttachedUnit(out Unit _))
              {
                if (weaponOption.info.rearmShip)
                  continue;
              }
              else if (!airbase.HasStorage() && (weaponOption.info.rearmGround || weaponOption.info.rearmShip))
                continue;
            }
          }
          else
            continue;
        }
        outAvailable.Add(weaponOption);
      }
    }
    if (!includeEmpty || outAvailable.Count != 0)
      return;
    outAvailable.Add((WeaponMount) null);
  }

  public static void PreferNukesFilter(
    int warheadsAvailable,
    HardpointSet hardpointSet,
    List<WeaponMount> listToFilter)
  {
    if (warheadsAvailable <= 0 || !MissionManager.AllowTactical())
      return;
    bool flag1 = false;
    bool flag2 = false;
    foreach (WeaponMount weaponMount in listToFilter)
    {
      if (!((Object) weaponMount == (Object) null) && !((Object) weaponMount.info == (Object) null) && weaponMount.info.nuclear && hardpointSet.hardpoints.Count * weaponMount.ammo <= warheadsAvailable)
      {
        if (weaponMount.info.strategic)
          flag2 = true;
        else
          flag1 = true;
      }
    }
    if (!flag1 && !flag2)
      return;
    for (int index = listToFilter.Count - 1; index >= 0; --index)
    {
      WeaponMount weaponMount = listToFilter[index];
      if ((Object) weaponMount == (Object) null || (Object) weaponMount.info == (Object) null)
        listToFilter.RemoveAt(index);
      else if (!weaponMount.info.nuclear)
        listToFilter.RemoveAt(index);
      else if (flag2 && !weaponMount.info.strategic)
        listToFilter.RemoveAt(index);
    }
    if (flag2)
    {
      foreach (WeaponMount weaponMount in listToFilter)
        ;
    }
    else
    {
      if (!flag1)
        return;
      foreach (WeaponMount weaponMount in listToFilter)
        ;
    }
  }
}

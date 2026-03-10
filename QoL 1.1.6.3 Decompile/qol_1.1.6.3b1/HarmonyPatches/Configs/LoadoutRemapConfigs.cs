// Decompiled with JetBrains decompiler
// Type: qol.HarmonyPatches.Configs.LoadoutRemapConfigs
// Assembly: qol, Version=1.1.6.3, Culture=neutral, PublicKeyToken=null
// MVID: 3B6539EA-E38C-45EE-8FFF-FC58CC42BADC
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\BepInEx\plugins\qol_1.1.6.3b1.dll

using System.Collections.Generic;

#nullable disable
namespace qol.HarmonyPatches.Configs;

public static class LoadoutRemapConfigs
{
  public static readonly IReadOnlyDictionary<(string AircraftName, int HardpointIndex, string OriginalKey), string> WeaponRemappings = (IReadOnlyDictionary<(string, int, string), string>) new Dictionary<(string, int, string), string>()
  {
    {
      ("AttackHelo1", 3, "ECMPod1"),
      "P_FlarePod1"
    },
    {
      ("QuadVTOL1", 4, "ECMPod1"),
      "P_FlarePod1"
    }
  };
  public static readonly IReadOnlyDictionary<string, string> GlobalWeaponRemappings = (IReadOnlyDictionary<string, string>) new Dictionary<string, string>()
  {
    {
      "turret_30mmHE_750",
      "turret_30mm_rotary"
    },
    {
      "gun_30mmHE_750_turret",
      "turret_30mm_rotary"
    }
  };

  public static bool TryGetRemappedKey(
    string aircraftName,
    int hardpointIndex,
    string originalKey,
    out string remappedKey)
  {
    if (LoadoutRemapConfigs.WeaponRemappings.TryGetValue((aircraftName, hardpointIndex, originalKey), out remappedKey) || LoadoutRemapConfigs.GlobalWeaponRemappings.TryGetValue(originalKey, out remappedKey))
      return true;
    remappedKey = (string) null;
    return false;
  }
}

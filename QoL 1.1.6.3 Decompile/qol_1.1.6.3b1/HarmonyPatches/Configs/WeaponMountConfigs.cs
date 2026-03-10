// Decompiled with JetBrains decompiler
// Type: qol.HarmonyPatches.Configs.WeaponMountConfigs
// Assembly: qol, Version=1.1.6.3, Culture=neutral, PublicKeyToken=null
// MVID: 3B6539EA-E38C-45EE-8FFF-FC58CC42BADC
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\BepInEx\plugins\qol_1.1.6.3b1.dll

using System.Collections.Generic;

#nullable disable
namespace qol.HarmonyPatches.Configs;

public static class WeaponMountConfigs
{
  public static readonly IReadOnlyList<string> ActiveTurretMounts = (IReadOnlyList<string>) new List<string>()
  {
    "turret_20mm_rotary",
    "turret_30mm_chaingun",
    "turret_30mm_rotary"
  };
  public static readonly IReadOnlyList<string> CustomTrailerMounts = (IReadOnlyList<string>) new List<string>()
  {
    "SAMTrailer1x1(Clone)",
    "HLT-Mx1(Clone)",
    "HLT-FTx1(Clone)",
    "LaserTrailer1x1(Clone)",
    "CRAMTrailer1x1(Clone)",
    "HLT-FCx1(Clone)"
  };
}

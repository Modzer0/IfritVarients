// Decompiled with JetBrains decompiler
// Type: qol.WeaponLoading.Configs.HorseMeshConfigs
// Assembly: qol, Version=1.1.6.3, Culture=neutral, PublicKeyToken=null
// MVID: 3B6539EA-E38C-45EE-8FFF-FC58CC42BADC
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\BepInEx\plugins\qol_1.1.6.3b1.dll

using System.Collections.Generic;

#nullable disable
namespace qol.WeaponLoading.Configs;

public static class HorseMeshConfigs
{
  public static readonly IReadOnlyList<string> DisabledMeshPaths = (IReadOnlyList<string>) new List<string>()
  {
    "Horse1/MBT_tread",
    "Horse1/MBT_chassis_LOD1",
    "Horse1/MBT_turret",
    "Horse1/MBT_turret/MBT_turret_LOD1",
    "Horse1/MBT_turret/MBT_gun/barrel",
    "Horse1/MBT_turret/ATGM",
    "Horse1/MBT_turret/laser_turret",
    "Horse1/MBT_turret/laser_turret/laser_barrel"
  };
}

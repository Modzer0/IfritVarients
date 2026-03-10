// Decompiled with JetBrains decompiler
// Type: qol.WeaponLoading.Configs.MBTEjectionConfigs
// Assembly: qol, Version=1.1.6.3, Culture=neutral, PublicKeyToken=null
// MVID: 3B6539EA-E38C-45EE-8FFF-FC58CC42BADC
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\BepInEx\plugins\qol_1.1.6.3b1.dll

using System.Collections.Generic;

#nullable disable
namespace qol.WeaponLoading.Configs;

public static class MBTEjectionConfigs
{
  public static readonly IReadOnlyList<(string GunPath, string EjectionTransformPath)> EjectionConfigs = (IReadOnlyList<(string, string)>) new List<(string, string)>()
  {
    ("MBT/MBT_turret/MBT_gun", "MBT/MBT_turret"),
    ("MBT1/MBT_turret/MBT_gun", "MBT1/MBT_turret")
  };
}

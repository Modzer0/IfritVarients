// Decompiled with JetBrains decompiler
// Type: qol.WeaponLoading.Configs.ExplosionConfigs
// Assembly: qol, Version=1.1.6.3, Culture=neutral, PublicKeyToken=null
// MVID: 3B6539EA-E38C-45EE-8FFF-FC58CC42BADC
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\BepInEx\plugins\qol_1.1.6.3b1.dll

using System.Collections.Generic;

#nullable disable
namespace qol.WeaponLoading.Configs;

public static class ExplosionConfigs
{
  public static readonly IReadOnlyList<string> ExplosionPrefabs = (IReadOnlyList<string>) new List<string>()
  {
    "explosion_1000kg",
    "explosion_100kg",
    "explosion_10kg",
    "explosion_1000kg_dusty",
    "explosion_100kg_dusty",
    "explosion_10kg_dusty"
  };
  public static readonly IReadOnlyList<string> CannonHitPrefabs = (IReadOnlyList<string>) new List<string>()
  {
    "cannonHit_50mm_armor",
    "cannonHit_50mm_dusty",
    "cannonHit_100mm_armor",
    "cannonHit_100mm_dusty"
  };
}

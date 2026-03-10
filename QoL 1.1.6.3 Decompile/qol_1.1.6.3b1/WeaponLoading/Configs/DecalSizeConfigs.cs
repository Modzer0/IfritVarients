// Decompiled with JetBrains decompiler
// Type: qol.WeaponLoading.Configs.DecalSizeConfigs
// Assembly: qol, Version=1.1.6.3, Culture=neutral, PublicKeyToken=null
// MVID: 3B6539EA-E38C-45EE-8FFF-FC58CC42BADC
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\BepInEx\plugins\qol_1.1.6.3b1.dll

using System.Collections.Generic;

#nullable disable
namespace qol.WeaponLoading.Configs;

public static class DecalSizeConfigs
{
  public static readonly IReadOnlyList<(string Path, float DecalSize, float FadeInTime)> ExplicitDecalSettings = (IReadOnlyList<(string, float, float)>) new List<(string, float, float)>()
  {
    ("fireball_large", 240f, 3f),
    ("fireball_medium", 80f, 80f)
  };
  public static readonly IReadOnlyList<string> ExplosionDecalPaths = (IReadOnlyList<string>) new List<string>()
  {
    "explosion_1kg",
    "explosion_1kg_dusty",
    "explosion_1kg_armor",
    "explosion_10kg",
    "explosion_10kg_dusty",
    "explosion_10kg_armor",
    "explosion_100kg",
    "explosion_100kg_dusty",
    "explosion_1000kg",
    "explosion_1000kg_dusty",
    "explosion_10000kg",
    "explosion_1kt",
    "explosion_20kt",
    "explosion_250kt"
  };
  public const float ExplosionDecalSizeMultiplier = 2f;
}

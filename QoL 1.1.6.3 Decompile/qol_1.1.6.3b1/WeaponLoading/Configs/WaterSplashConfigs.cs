// Decompiled with JetBrains decompiler
// Type: qol.WeaponLoading.Configs.WaterSplashConfigs
// Assembly: qol, Version=1.1.6.3, Culture=neutral, PublicKeyToken=null
// MVID: 3B6539EA-E38C-45EE-8FFF-FC58CC42BADC
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\BepInEx\plugins\qol_1.1.6.3b1.dll

using System.Collections.Generic;

#nullable disable
namespace qol.WeaponLoading.Configs;

public static class WaterSplashConfigs
{
  public const float LifetimeMultiplier = 2f;
  public const float SizeMultiplier = 2f;
  public static readonly IReadOnlyList<string> WaterEffectPaths = (IReadOnlyList<string>) new List<string>()
  {
    "WaterSurfaceExplosion_1kg",
    "WaterSurfaceExplosion_10kg",
    "WaterSurfaceExplosion_100kg",
    "WaterSurfaceExplosion_1000kg",
    "ShellSplash_10kg",
    "ShellSplash_1kg",
    "waterSplashSmall",
    "waterSplashMed",
    "waterImpactSmall"
  };
}

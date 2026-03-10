// Decompiled with JetBrains decompiler
// Type: qol.WeaponLoading.Configs.RailgunParticleConfigs
// Assembly: qol, Version=1.1.6.3, Culture=neutral, PublicKeyToken=null
// MVID: 3B6539EA-E38C-45EE-8FFF-FC58CC42BADC
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\BepInEx\plugins\qol_1.1.6.3b1.dll

using System.Collections.Generic;

#nullable disable
namespace qol.WeaponLoading.Configs;

public static class RailgunParticleConfigs
{
  public const string KEMFirePath = "P_KEM1/FireParticles";
  public const float KEMFireDuration = 14f;
  public const string RailgunHitArmorPath = "railgunHit_armor";
  public const float RailgunArmorGravityModifier = 0.1f;
  public const float RailgunArmorSizeMultiplier = 3f;
  public const string RailgunHitDustyPath = "railgunHit_dusty";
  public const float RailgunDustyGravityModifier = 0.1f;
  public const float RailgunDustySizeMultiplier = 2f;
  public const float RailgunDustyLifetimeMultiplier = 2f;
  public static readonly IReadOnlyList<string> SmokeLingerPaths = (IReadOnlyList<string>) new List<string>()
  {
    "AShM1/VLS_Booster/smokeParticles/smokeLinger",
    "P_SAMRadar1/VLS_Booster/smokeParticles/smokeLinger"
  };
  public const float SmokeLingerEmissionRate = 30f;
}

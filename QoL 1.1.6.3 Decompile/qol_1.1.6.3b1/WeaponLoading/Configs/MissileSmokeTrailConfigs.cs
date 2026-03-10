// Decompiled with JetBrains decompiler
// Type: qol.WeaponLoading.Configs.MissileSmokeTrailConfigs
// Assembly: qol, Version=1.1.6.3, Culture=neutral, PublicKeyToken=null
// MVID: 3B6539EA-E38C-45EE-8FFF-FC58CC42BADC
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\BepInEx\plugins\qol_1.1.6.3b1.dll

using System.Collections.Generic;

#nullable disable
namespace qol.WeaponLoading.Configs;

public static class MissileSmokeTrailConfigs
{
  public static readonly IReadOnlyList<(string Path, float Duration)> ParticleDurations = (IReadOnlyList<(string, float)>) new List<(string, float)>()
  {
    ("AAM1/smokeParticles", 8f),
    ("AAM2/smokeParticles", 12f),
    ("AAM3/smokeParticles", 6f),
    ("AAM4/smokeParticles", 64f),
    ("AAM4/FireParticlesBooster/fireParticlesSustainer", 92f),
    ("SAM_Radar1/smokeParticles", 24f),
    ("SAM_Radar2/smokeParticles", 32f)
  };
  public static readonly IReadOnlyList<(string Path, float Opacity)> TrailOpacities = (IReadOnlyList<(string, float)>) new List<(string, float)>()
  {
    ("AAM1/smokeParticles/smokeTrail", 0.2f),
    ("AAM2/smokeParticles/smokeTrailBooster", 0.1f),
    ("AAM3/smokeParticles/smokeTrail", 0.2f),
    ("AAM4/smokeParticles/smokeTrailBooster", 0.2f),
    ("SAM_Radar1/smokeParticles/smokeTrail", 0.4f),
    ("SAM_Radar2/smokeParticles/smokeTrail", 0.5f)
  };
}

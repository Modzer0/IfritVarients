// Decompiled with JetBrains decompiler
// Type: qol.HarmonyPatches.Configs.NightVisionConfigs
// Assembly: qol, Version=1.1.6.3, Culture=neutral, PublicKeyToken=null
// MVID: 3B6539EA-E38C-45EE-8FFF-FC58CC42BADC
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\BepInEx\plugins\qol_1.1.6.3b1.dll

#nullable disable
namespace qol.HarmonyPatches.Configs;

public static class NightVisionConfigs
{
  public const float GainMin = -3f;
  public const float GainMax = 3f;
  public const float BloomThresholdMin = 0.0f;
  public const float BloomThresholdMax = 2f;
  public const float Contrast = 25f;
  public const float FilmGrainIntensity = 2f;
  public const float FilmGrainResponse = 2f;
  public const float ChromaticAberrationIntensity = 0.5f;
  public const float VignetteIntensity = 0.3f;
}

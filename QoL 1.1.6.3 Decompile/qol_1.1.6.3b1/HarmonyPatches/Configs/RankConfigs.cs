// Decompiled with JetBrains decompiler
// Type: qol.HarmonyPatches.Configs.RankConfigs
// Assembly: qol, Version=1.1.6.3, Culture=neutral, PublicKeyToken=null
// MVID: 3B6539EA-E38C-45EE-8FFF-FC58CC42BADC
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\BepInEx\plugins\qol_1.1.6.3b1.dll

#nullable disable
namespace qol.HarmonyPatches.Configs;

public static class RankConfigs
{
  public static readonly float[] RankThresholds = new float[7]
  {
    0.0f,
    10f,
    20f,
    40f,
    80f,
    160f,
    320f
  };
}

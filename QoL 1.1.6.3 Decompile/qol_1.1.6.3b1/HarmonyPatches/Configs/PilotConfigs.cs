// Decompiled with JetBrains decompiler
// Type: qol.HarmonyPatches.Configs.PilotConfigs
// Assembly: qol, Version=1.1.6.3, Culture=neutral, PublicKeyToken=null
// MVID: 3B6539EA-E38C-45EE-8FFF-FC58CC42BADC
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\BepInEx\plugins\qol_1.1.6.3b1.dll

#nullable disable
namespace qol.HarmonyPatches.Configs;

public static class PilotConfigs
{
  public const float AIPilotSkillMin = 5f;
  public const float AIPilotSkillMax = 15f;
  public const float AIPilotBraveryMin = 0.5f;
  public const float AIPilotBraveryMax = 1.5f;
  public const float DefaultAISkill = 20f;
  public const float BloodPumpRate = 0.32f;
  public const float StaminaRecoveryRate = 0.19f;
  public const float RedoutMultiplier = 2f;
  public const float AircraftRemovalDelayMultiplier = 2f;
}

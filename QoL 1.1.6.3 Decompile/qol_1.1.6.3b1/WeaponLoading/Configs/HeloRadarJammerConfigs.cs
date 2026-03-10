// Decompiled with JetBrains decompiler
// Type: qol.WeaponLoading.Configs.HeloRadarJammerConfigs
// Assembly: qol, Version=1.1.6.3, Culture=neutral, PublicKeyToken=null
// MVID: 3B6539EA-E38C-45EE-8FFF-FC58CC42BADC
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\BepInEx\plugins\qol_1.1.6.3b1.dll

using System.Collections.Generic;

#nullable disable
namespace qol.WeaponLoading.Configs;

public static class HeloRadarJammerConfigs
{
  public static readonly IReadOnlyList<string> HeloJammerPaths = (IReadOnlyList<string>) new List<string>()
  {
    "AttackHelo1",
    "QuadVTOL1"
  };
  public const string RadarJammerSpriteName = "weaponicon_radarJammer";
}

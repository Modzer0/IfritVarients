// Decompiled with JetBrains decompiler
// Type: qol.WeaponLoading.Configs.WeaponIconConfigs
// Assembly: qol, Version=1.1.6.3, Culture=neutral, PublicKeyToken=null
// MVID: 3B6539EA-E38C-45EE-8FFF-FC58CC42BADC
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\BepInEx\plugins\qol_1.1.6.3b1.dll

#nullable disable
namespace qol.WeaponLoading.Configs;

public static class WeaponIconConfigs
{
  public static readonly (string ResourceSuffix, string WeaponInfoName)[] IconMappings = new (string, string)[4]
  {
    ("weaponIcon_P_GLB1.png", "P_GLB1_info"),
    ("weaponIcon_P_HASM1.png", "P_HAsM1_info"),
    ("weaponIcon_P_KEM1.png", "P_KEM1_info"),
    ("weaponIcon_P_LRSAM1.png", "P_SAMRadar1_info")
  };
  public const string ResourcePrefix = "Resources.P_Missiles1.";
}

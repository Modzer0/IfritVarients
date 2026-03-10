// Decompiled with JetBrains decompiler
// Type: qol.WeaponLoading.Configs.AnnexArrestorConfigs
// Assembly: qol, Version=1.1.6.3, Culture=neutral, PublicKeyToken=null
// MVID: 3B6539EA-E38C-45EE-8FFF-FC58CC42BADC
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\BepInEx\plugins\qol_1.1.6.3b1.dll

using UnityEngine;

#nullable disable
namespace qol.WeaponLoading.Configs;

public static class AnnexArrestorConfigs
{
  public const string SourceCablePath = "FleetCarrier1/hull_R/hull_R2/arrestorCable1";
  public const string TargetParentPath = "AssaultCarrier1/hull_RL/deck_R";
  public static readonly Vector3 CableLocalPosition = new Vector3(0.1f, 1.6f, -20f);
  public const string AirbasePath = "AssaultCarrier1";
  public const int RunwayIndex = 2;
}

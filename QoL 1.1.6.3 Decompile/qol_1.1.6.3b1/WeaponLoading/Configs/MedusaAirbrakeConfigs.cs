// Decompiled with JetBrains decompiler
// Type: qol.WeaponLoading.Configs.MedusaAirbrakeConfigs
// Assembly: qol, Version=1.1.6.3, Culture=neutral, PublicKeyToken=null
// MVID: 3B6539EA-E38C-45EE-8FFF-FC58CC42BADC
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\BepInEx\plugins\qol_1.1.6.3b1.dll

using System.Collections.Generic;

#nullable disable
namespace qol.WeaponLoading.Configs;

public static class MedusaAirbrakeConfigs
{
  public const string AircraftPath = "EW1";
  public static readonly IReadOnlyList<(string RudderPath, string HingePath)> AirbrakeConfigs = (IReadOnlyList<(string, string)>) new List<(string, string)>()
  {
    ("EW1/fuselage_R/tail/hstab_L/vstab_L/rudder_L", "EW1/fuselage_R/tail/hstab_L/vstab_L/rudder_L_hinge"),
    ("EW1/fuselage_R/tail/hstab_R/vstab_R/rudder_R", "EW1/fuselage_R/tail/hstab_R/vstab_R/rudder_R_hinge")
  };
}

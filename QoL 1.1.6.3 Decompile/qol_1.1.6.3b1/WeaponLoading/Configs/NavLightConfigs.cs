// Decompiled with JetBrains decompiler
// Type: qol.WeaponLoading.Configs.NavLightConfigs
// Assembly: qol, Version=1.1.6.3, Culture=neutral, PublicKeyToken=null
// MVID: 3B6539EA-E38C-45EE-8FFF-FC58CC42BADC
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\BepInEx\plugins\qol_1.1.6.3b1.dll

using System.Collections.Generic;
using UnityEngine;

#nullable disable
namespace qol.WeaponLoading.Configs;

public static class NavLightConfigs
{
  public static readonly IReadOnlyList<(string MaterialName, Color Color)> NavLightColors = (IReadOnlyList<(string, Color)>) new List<(string, Color)>()
  {
    ("slimelight_on", new Color(0.0f, 2f, 2f, 0.0f)),
    ("navlight_L", new Color(2f, 0.0f, 0.0f, 0.0f)),
    ("navlight_R", new Color(0.0f, 2f, 0.0f, 0.0f))
  };
  public static readonly (string MaterialName, Color Color, float Smoothness) MatteBlack = ("Matte_black", new Color(0.2f, 0.2f, 0.2f, 1f), 0.0f);
  public static readonly IReadOnlyList<string> DeactivatePaths = (IReadOnlyList<string>) new List<string>()
  {
    "Fighter1/vstab1/vstab2/slimelights_tail_L"
  };
}

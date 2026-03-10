// Decompiled with JetBrains decompiler
// Type: qol.WeaponLoading.Configs.MaterialConfigs
// Assembly: qol, Version=1.1.6.3, Culture=neutral, PublicKeyToken=null
// MVID: 3B6539EA-E38C-45EE-8FFF-FC58CC42BADC
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\BepInEx\plugins\qol_1.1.6.3b1.dll

using System.Collections.Generic;
using UnityEngine;

#nullable disable
namespace qol.WeaponLoading.Configs;

public static class MaterialConfigs
{
  public static readonly IReadOnlyList<(string MaterialName, Color Color)> GlassColors = (IReadOnlyList<(string, Color)>) new List<(string, Color)>()
  {
    ("CAS1_glass", new Color(0.8f, 1f, 0.9f, 0.1f)),
    ("UtilityHelo1_glass", new Color(0.8f, 1f, 0.9f, 0.1f)),
    ("QuadVTOL1_canopy", new Color(0.8f, 1f, 0.9f, 0.1f)),
    ("COIN_glass", new Color(0.4f, 0.5f, 0.45f, 0.1f)),
    ("trainer_glass", new Color(0.4f, 0.5f, 0.45f, 0.1f)),
    ("AttackHelo1_glass", new Color(0.6f, 0.5f, 0.4f, 0.5f)),
    ("fighter1_canopy", new Color(1f, 1f, 1f, 0.1f)),
    ("smallFighter1_canopy", new Color(1f, 0.9f, 1f, 0.5f)),
    ("multirole1_canopy", new Color(1f, 0.5f, 0.5f, 0.1f)),
    ("EW1_glass", new Color(1f, 1f, 1f, 1f)),
    ("SFB_glass", new Color(1f, 1f, 1f, 1f))
  };
}

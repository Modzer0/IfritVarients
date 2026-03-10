// Decompiled with JetBrains decompiler
// Type: qol.WeaponLoading.Configs.PropFanEfficiencyConfigs
// Assembly: qol, Version=1.1.6.3, Culture=neutral, PublicKeyToken=null
// MVID: 3B6539EA-E38C-45EE-8FFF-FC58CC42BADC
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\BepInEx\plugins\qol_1.1.6.3b1.dll

using System.Collections.Generic;
using UnityEngine;

#nullable disable
namespace qol.WeaponLoading.Configs;

public static class PropFanEfficiencyConfigs
{
  public static readonly IReadOnlyList<string> BrawlerPropFanPaths = (IReadOnlyList<string>) new List<string>()
  {
    "CAS1/wing1_L/engineMount_L/engine_L/hub_L",
    "CAS1/wing1_R/engineMount_R/engine_R/hub_R"
  };
  public static readonly IReadOnlyList<Keyframe> BrawlerEfficiencyKeyframes = (IReadOnlyList<Keyframe>) new List<Keyframe>()
  {
    new Keyframe(0.0f, 1.1f),
    new Keyframe(50f, 1.25f),
    new Keyframe(100f, 1f),
    new Keyframe(200f, 0.5f),
    new Keyframe(400f, 0.0f)
  };
}

// Decompiled with JetBrains decompiler
// Type: qol.WeaponLoading.Configs.FireDamageConfigs
// Assembly: qol, Version=1.1.6.3, Culture=neutral, PublicKeyToken=null
// MVID: 3B6539EA-E38C-45EE-8FFF-FC58CC42BADC
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\BepInEx\plugins\qol_1.1.6.3b1.dll

using System.Collections.Generic;

#nullable disable
namespace qol.WeaponLoading.Configs;

public static class FireDamageConfigs
{
  public static readonly IReadOnlyList<(string Path, float Damage, float Range, float Duration)> FireDamageSettings = (IReadOnlyList<(string, float, float, float)>) new List<(string, float, float, float)>()
  {
    ("fireball_large", 10f, 25f, 2f),
    ("fireball_medium", 5f, 10f, 2f),
    ("fire_large", 20f, 20f, 120f),
    ("fire_med", 5f, 10f, 60f),
    ("fire_small", 1f, 2.5f, 30f)
  };
}

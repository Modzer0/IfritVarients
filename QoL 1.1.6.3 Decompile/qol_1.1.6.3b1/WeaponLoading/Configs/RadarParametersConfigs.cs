// Decompiled with JetBrains decompiler
// Type: qol.WeaponLoading.Configs.RadarParametersConfigs
// Assembly: qol, Version=1.1.6.3, Culture=neutral, PublicKeyToken=null
// MVID: 3B6539EA-E38C-45EE-8FFF-FC58CC42BADC
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\BepInEx\plugins\qol_1.1.6.3b1.dll

using System.Collections.Generic;

#nullable disable
namespace qol.WeaponLoading.Configs;

public static class RadarParametersConfigs
{
  public static readonly IReadOnlyList<(string Path, float? MaxRange, float? MaxSignal, float? MinSignal, float? ClutterFactor, float? DopplerFactor)> RadarModifications = (IReadOnlyList<(string, float?, float?, float?, float?, float?)>) new List<(string, float?, float?, float?, float?, float?)>()
  {
    ("kestrel/fuselage_F/nosecone", new float?(30000f), new float?(4f), new float?(0.5f), new float?(0.25f), new float?(0.1f)),
    ("Destroyer1/Hull_Bridge/Hull_Radar", new float?(75000f), new float?(), new float?(), new float?(), new float?()),
    ("RadarSAM1/turret", new float?(20000f), new float?(), new float?(), new float?(0.7f), new float?())
  };
}

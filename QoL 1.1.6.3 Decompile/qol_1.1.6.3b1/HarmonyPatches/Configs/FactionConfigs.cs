// Decompiled with JetBrains decompiler
// Type: qol.HarmonyPatches.Configs.FactionConfigs
// Assembly: qol, Version=1.1.6.3, Culture=neutral, PublicKeyToken=null
// MVID: 3B6539EA-E38C-45EE-8FFF-FC58CC42BADC
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\BepInEx\plugins\qol_1.1.6.3b1.dll

using System.Collections.Generic;

#nullable disable
namespace qol.HarmonyPatches.Configs;

public static class FactionConfigs
{
  public static readonly IReadOnlyDictionary<string, string> ColorToFactionName = (IReadOnlyDictionary<string, string>) new Dictionary<string, string>()
  {
    {
      "A76BFFFF",
      "BDF"
    },
    {
      "FFB800FF",
      "PALA"
    },
    {
      "008FFFFF",
      "BLU"
    },
    {
      "FF140AFF",
      "RED"
    }
  };

  public static string GetFactionName(string colorCode)
  {
    string str;
    return !FactionConfigs.ColorToFactionName.TryGetValue(colorCode, out str) ? "" : str;
  }
}

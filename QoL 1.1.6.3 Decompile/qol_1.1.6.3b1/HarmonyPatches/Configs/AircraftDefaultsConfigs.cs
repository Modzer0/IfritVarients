// Decompiled with JetBrains decompiler
// Type: qol.HarmonyPatches.Configs.AircraftDefaultsConfigs
// Assembly: qol, Version=1.1.6.3, Culture=neutral, PublicKeyToken=null
// MVID: 3B6539EA-E38C-45EE-8FFF-FC58CC42BADC
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\BepInEx\plugins\qol_1.1.6.3b1.dll

using System.Collections.Generic;

#nullable disable
namespace qol.HarmonyPatches.Configs;

public static class AircraftDefaultsConfigs
{
  public static readonly IReadOnlyDictionary<string, float> FuelLevels = (IReadOnlyDictionary<string, float>) new Dictionary<string, float>()
  {
    {
      "COIN",
      0.1f
    },
    {
      "trainer",
      0.35f
    },
    {
      "CAS1",
      0.4f
    },
    {
      "UtilityHelo1",
      0.45f
    },
    {
      "AttackHelo1",
      0.2f
    },
    {
      "Fighter1",
      0.7f
    },
    {
      "SmallFighter1",
      0.75f
    },
    {
      "Multirole1",
      0.6f
    },
    {
      "QuadVTOL1",
      0.15f
    },
    {
      "EW1",
      0.4f
    },
    {
      "Darkreach",
      0.2f
    }
  };
  public static readonly IReadOnlyDictionary<string, int> LiveryIndexBDF = (IReadOnlyDictionary<string, int>) new Dictionary<string, int>()
  {
    {
      "COIN",
      0
    },
    {
      "trainer",
      0
    },
    {
      "CAS1",
      0
    },
    {
      "UtilityHelo1",
      0
    },
    {
      "AttackHelo1",
      0
    },
    {
      "Fighter1",
      0
    },
    {
      "SmallFighter1",
      0
    },
    {
      "Multirole1",
      0
    },
    {
      "QuadVTOL1",
      0
    },
    {
      "EW1",
      0
    },
    {
      "Darkreach",
      0
    }
  };
  public static readonly IReadOnlyDictionary<string, int> LiveryIndexPrimeva = (IReadOnlyDictionary<string, int>) new Dictionary<string, int>()
  {
    {
      "COIN",
      1
    },
    {
      "trainer",
      1
    },
    {
      "CAS1",
      0
    },
    {
      "UtilityHelo1",
      1
    },
    {
      "AttackHelo1",
      0
    },
    {
      "Fighter1",
      0
    },
    {
      "SmallFighter1",
      1
    },
    {
      "Multirole1",
      0
    },
    {
      "QuadVTOL1",
      0
    },
    {
      "EW1",
      0
    },
    {
      "Darkreach",
      0
    }
  };
}

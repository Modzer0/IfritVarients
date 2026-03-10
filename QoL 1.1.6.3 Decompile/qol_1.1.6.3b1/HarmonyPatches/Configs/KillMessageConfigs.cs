// Decompiled with JetBrains decompiler
// Type: qol.HarmonyPatches.Configs.KillMessageConfigs
// Assembly: qol, Version=1.1.6.3, Culture=neutral, PublicKeyToken=null
// MVID: 3B6539EA-E38C-45EE-8FFF-FC58CC42BADC
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\BepInEx\plugins\qol_1.1.6.3b1.dll

using System.Collections.Generic;

#nullable disable
namespace qol.HarmonyPatches.Configs;

public static class KillMessageConfigs
{
  public static readonly IReadOnlyDictionary<KillType, (string[] common, string[] rare)> KillerMessages = (IReadOnlyDictionary<KillType, (string[], string[])>) new Dictionary<KillType, (string[], string[])>()
  {
    [KillType.Aircraft] = (new string[3]
    {
      "shot down",
      "killed",
      "downed"
    }, new string[4]
    {
      "obliterated",
      "smited",
      "vaporized",
      "nuked"
    }),
    [KillType.Vehicle] = (new string[3]
    {
      "destroyed",
      "wrecked",
      "disabled"
    }, new string[3]
    {
      "decommissioned",
      "atomized",
      "scrapped"
    }),
    [KillType.Building] = (new string[4]
    {
      "demolished",
      "leveled",
      "flattened",
      "razed"
    }, new string[3]
    {
      "eviscerated",
      "dismantled",
      "deleted"
    }),
    [KillType.Missile] = (new string[1]{ "intercepted" }, new string[1]
    {
      "parried"
    }),
    [KillType.Ship] = (new string[3]
    {
      "sank",
      "scuttled",
      "blew up"
    }, new string[3]
    {
      "capsized",
      "decompartmentalized",
      "submarine-ified"
    })
  };
  public static readonly IReadOnlyDictionary<KillType, (string[] common, string[] rare)> NoKillerMessages = (IReadOnlyDictionary<KillType, (string[], string[])>) new Dictionary<KillType, (string[], string[])>()
  {
    [KillType.Aircraft] = (new string[4]
    {
      "crashed",
      "died",
      "self destructed",
      "exploded"
    }, new string[3]
    {
      "experienced kinetic energy",
      "entered the danger zone",
      "rapidly disassembled"
    }),
    [KillType.Vehicle] = (new string[3]
    {
      "was destroyed",
      "was wrecked",
      "was abandoned"
    }, new string[3]
    {
      "hit a pothole",
      "experienced radiation",
      "left this world"
    }),
    [KillType.Building] = (new string[3]
    {
      "collapsed",
      "imploded",
      "was demolished"
    }, new string[2]
    {
      "was converted to a parking lot",
      "experienced structural failure"
    }),
    [KillType.Missile] = (new string[0], new string[2]
    {
      "hit a civilian building",
      "returned to sender"
    }),
    [KillType.Ship] = (new string[3]
    {
      "sank",
      "sank itself",
      "took on water"
    }, new string[2]
    {
      "turned into a reef",
      "met a watery end"
    })
  };
}

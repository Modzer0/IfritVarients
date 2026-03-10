// Decompiled with JetBrains decompiler
// Type: FactionHelper
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.SavedMission;
using NuclearOption.SavedMission.ObjectiveV2;
using System.Collections.Generic;
using UnityEngine;

#nullable disable
public static class FactionHelper
{
  public const string NO_FACTION = "None";
  public const string NEUTRAL_FACTION = "Neutral";
  public const string Boscali = "Boscali";
  public const string Primeva = "Primeva";

  public static bool EmptyOrNoFaction(string name) => string.IsNullOrEmpty(name) || name == "None";

  public static bool EmptyOrNoFactionOrNeutral(string name)
  {
    return string.IsNullOrEmpty(name) || name == "None" || name == "Neutral";
  }

  public static bool BelongsToFaction(this IHasFaction hasFaction, string faction)
  {
    string factionName = hasFaction.FactionName;
    if (faction == factionName)
      return true;
    return FactionHelper.EmptyOrNoFaction(faction) && FactionHelper.EmptyOrNoFaction(factionName);
  }

  public static FactionHQ FindHQ(this IHasFaction hasFaction)
  {
    return FactionRegistry.HqFromName(hasFaction.FactionName);
  }

  public static Color GetColorOrGray(this FactionHQ hq)
  {
    return !((Object) hq != (Object) null) ? Color.gray : hq.faction.color;
  }

  public static List<string> GetFactionsAndNeutral()
  {
    List<MissionFaction> factions = MissionManager.CurrentMission.factions;
    List<string> factionsAndNeutral = new List<string>(factions.Count + 1);
    factionsAndNeutral.Add("Neutral");
    foreach (MissionFaction missionFaction in factions)
      factionsAndNeutral.Add(missionFaction.factionName);
    return factionsAndNeutral;
  }

  public static string ToUIString(FactionHQ hq)
  {
    if ((Object) hq == (Object) null)
      return "None";
    Faction faction = hq.faction;
    return faction.factionName.AddColor(faction.color);
  }

  public static string ToUIString(string factionName)
  {
    if (FactionHelper.EmptyOrNoFaction(factionName))
      return "None";
    if (factionName == "Neutral")
      return "Neutral";
    Faction faction = FactionRegistry.FactionFromName(factionName);
    return faction.factionName.AddColor(faction.color);
  }
}

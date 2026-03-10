// Decompiled with JetBrains decompiler
// Type: FactionRegistry
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;
using UnityEngine;

#nullable disable
public static class FactionRegistry
{
  public static readonly List<Faction> factions = new List<Faction>();
  public static readonly Dictionary<string, Faction> factionLookup = new Dictionary<string, Faction>();
  public static readonly Dictionary<Faction, FactionHQ> HQLookup = new Dictionary<Faction, FactionHQ>();
  public static readonly Dictionary<string, Airbase> airbaseLookup = new Dictionary<string, Airbase>();

  public static void RegisterAirbase(string key, Airbase airbase)
  {
    if (string.IsNullOrEmpty(key))
      throw new ArgumentNullException(nameof (key), "airbase name was null");
    ColorLog<Airbase>.Info("RegisterAirbase " + key);
    FactionRegistry.airbaseLookup.Add(key, airbase);
  }

  public static void UnregisterAirbase(Airbase airbase)
  {
    string uniqueName = airbase.SavedAirbase.UniqueName;
    if (string.IsNullOrEmpty(uniqueName))
      Debug.LogError((object) "Airbase name empty when UnregisterAirbase was called");
    ColorLog<Airbase>.Info("UnregisterAirbase " + uniqueName);
    FactionRegistry.airbaseLookup.Remove(uniqueName);
  }

  public static void ChangeAirbaseName(Airbase airbase, string newName)
  {
    FactionRegistry.airbaseLookup.Remove(airbase.SavedAirbase.UniqueName);
    FactionRegistry.airbaseLookup.Add(newName, airbase);
  }

  public static void RegisterFaction(Faction faction, FactionHQ HQ)
  {
    if (!FactionRegistry.factions.Contains(faction))
      FactionRegistry.factions.Add(faction);
    if (!FactionRegistry.factionLookup.TryGetValue(faction.factionName, out Faction _))
      FactionRegistry.factionLookup.Add(faction.factionName, faction);
    if (FactionRegistry.HQLookup.TryGetValue(faction, out FactionHQ _))
      return;
    FactionRegistry.HQLookup.Add(faction, HQ);
  }

  public static Dictionary<Faction, FactionHQ>.ValueCollection GetAllHQs()
  {
    return FactionRegistry.HQLookup.Values;
  }

  public static void Clear()
  {
    FactionRegistry.factions.Clear();
    FactionRegistry.factionLookup.Clear();
    FactionRegistry.airbaseLookup.Clear();
    FactionRegistry.HQLookup.Clear();
  }

  public static FactionHQ HQFromFaction(Faction faction)
  {
    if ((UnityEngine.Object) faction == (UnityEngine.Object) null)
      return (FactionHQ) null;
    return FactionRegistry.HQLookup.ContainsKey(faction) ? FactionRegistry.HQLookup[faction] : (FactionHQ) null;
  }

  public static FactionHQ HqFromName(string factionName)
  {
    Faction key = FactionRegistry.FactionFromName(factionName);
    return (UnityEngine.Object) key != (UnityEngine.Object) null ? FactionRegistry.HQLookup[key] : (FactionHQ) null;
  }

  public static Faction FactionFromName(string factionName)
  {
    if (string.IsNullOrEmpty(factionName))
      return (Faction) null;
    Faction faction;
    return FactionRegistry.factionLookup.TryGetValue(factionName, out faction) ? faction : (Faction) null;
  }
}

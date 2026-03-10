// Decompiled with JetBrains decompiler
// Type: NuclearOption.SavedMission.Mission
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Mirage;
using NuclearOption.SavedMission.ConvertVersions;
using NuclearOption.SavedMission.ObjectiveV2;
using NuclearOption.SceneLoading;
using System;
using System.Collections.Generic;
using UnityEngine;

#nullable disable
namespace NuclearOption.SavedMission;

[NetworkMessage]
[Serializable]
public class Mission
{
  public static readonly Mission NullMission = new Mission("NULL", true);
  public int JsonVersion;
  [Obsolete("WorkshopId has been moved to workshop.json", true)]
  public ulong WorkshopId;
  public MapKey MapKey;
  public MissionSettings missionSettings = new MissionSettings();
  public MissionEnvironment environment = new MissionEnvironment();
  public List<SavedAircraft> aircraft = new List<SavedAircraft>();
  public List<SavedVehicle> vehicles = new List<SavedVehicle>();
  public List<SavedShip> ships = new List<SavedShip>();
  public List<SavedBuilding> buildings = new List<SavedBuilding>();
  public List<SavedScenery> scenery = new List<SavedScenery>();
  public List<SavedContainer> containers = new List<SavedContainer>();
  public List<SavedMissile> missiles = new List<SavedMissile>();
  public List<SavedPilot> pilots = new List<SavedPilot>();
  public List<MissionFaction> factions = new List<MissionFaction>();
  public List<SavedAirbase> airbases = new List<SavedAirbase>();
  public List<UnitInventory> unitInventories = new List<UnitInventory>();
  public SavedMissionObjectives objectives;
  [NonSerialized]
  public string Name;
  [NonSerialized]
  public MissionObjectives Objectives;
  [NonSerialized]
  public MissionKey? LoadKey;

  public bool HasFullLoaded => this.Objectives != null;

  public Mission()
  {
  }

  public Mission(string name, bool skipAfterLoad = false)
  {
    this.JsonVersion = MissionVersionUpgrade.LatestVersion;
    this.Name = name;
    Mission.SetupNewMission(ref this.objectives);
    if (skipAfterLoad)
      return;
    this.AfterLoad(name);
  }

  private static void SetupNewMission(ref SavedMissionObjectives objectives)
  {
    ref List<SavedObjective> local1 = ref objectives.Objectives;
    if (local1 == null)
      local1 = new List<SavedObjective>();
    ref List<SavedOutcome> local2 = ref objectives.Outcomes;
    if (local2 == null)
      local2 = new List<SavedOutcome>();
    if (objectives.Objectives.Count != 0)
      return;
    objectives.Objectives.Add(new SavedObjective()
    {
      UniqueName = MissionObjectivesFactory.MissionStartName,
      Hidden = true
    });
  }

  public void AfterLoad(string name)
  {
    this.LoadKey = new MissionKey?();
    this.Name = name;
    MissionVersionUpgrade.Upgrade(this);
    ColorLog<MissionManager>.Info("AfterLoad for mission " + (this.Name ?? "NULL"));
  }

  public void AfterLoad(MissionKey key)
  {
    this.LoadKey = new MissionKey?(key);
    this.Name = key.Name;
    MissionVersionUpgrade.Upgrade(this);
    ColorLog<MissionManager>.Info("AfterLoad for mission " + (this.Name ?? "NULL"));
  }

  public void OnSceneLoaded(MissionManager manager)
  {
    ColorLog<MissionManager>.Info("OnSceneLoaded for mission " + (this.Name ?? "NULL"));
    foreach (SavedUnit allSavedUnit in (IEnumerable<SavedUnit>) MissionManager.GetAllSavedUnits(this, false))
    {
      Unit unit;
      if (UnitRegistry.customIDLookup.TryGetValue(allSavedUnit.UniqueName, out unit))
      {
        allSavedUnit.PlacementType = !unit.BuiltIn ? PlacementType.Custom : PlacementType.Override;
        unit.LinkSavedUnit(allSavedUnit);
      }
      else
        allSavedUnit.PlacementType = PlacementType.Custom;
    }
    if (GameManager.gameState == GameState.Editor)
    {
      foreach (Unit allUnit in UnitRegistry.allUnits)
      {
        if (allUnit.SavedUnit == null)
          allUnit.EditorMapLoaded();
      }
      foreach (Airbase buildInAirbase in FactionRegistry.airbaseLookup.Values)
        buildInAirbase.SavedAirbase.AfterLoadEditor(buildInAirbase, (IReadOnlyList<SavedBuilding>) null);
      if (this.airbases.Count > 0)
      {
        IReadOnlyList<SavedBuilding> allSavedBuildings = MissionManager.GetAllSavedBuildings(this, true);
        foreach (SavedAirbase airbase in this.airbases)
          airbase.AfterLoadEditor((Airbase) null, allSavedBuildings);
      }
    }
    this.Objectives = MissionObjectivesFactory.Load(this, this.objectives);
    foreach (Objective allObjective in this.Objectives.AllObjectives)
      allObjective.MissionManager = manager;
    foreach (FactionHQ allHq in FactionRegistry.GetAllHQs())
    {
      string factionName = allHq.faction.factionName;
      foreach (Objective allObjective in this.Objectives.AllObjectives)
      {
        if (allObjective.SavedObjective.Faction == factionName)
          allObjective.FactionHQ = allHq;
      }
    }
    this.SetupAirbase(manager.ServerObjectManager);
    if (manager.IsServer)
    {
      foreach (SavedBuilding building in this.buildings)
        building.LoadAirbaseRef();
      foreach (Unit allUnit in UnitRegistry.allUnits)
        this.ServerSetupBuiltInBuildings(allUnit);
    }
    else
    {
      if (!manager.IsClient)
        return;
      Mission.ClientAddBuildingsToAirbase();
    }
  }

  public static void ClientAddBuildingsToAirbase()
  {
    foreach (Unit allUnit in UnitRegistry.allUnits)
    {
      if (allUnit is Building building)
        building.ClientAddBuildingToAirbase();
    }
  }

  private void ServerSetupBuiltInBuildings(Unit unit)
  {
    if (!unit.BuiltIn)
    {
      Debug.LogError((object) "Only OnSceneLoaded units should exist in OnSceneLoaded");
    }
    else
    {
      if (!(unit is Building building))
        return;
      if (!(unit.SavedUnit is SavedBuilding savedUnit) || savedUnit.PlacementType == PlacementType.BuiltIn)
      {
        if ((UnityEngine.Object) building.MapAirbase != (UnityEngine.Object) null)
        {
          building.SetAirbase(building.MapAirbase);
        }
        else
        {
          if (!((UnityEngine.Object) building.MapHQ != (UnityEngine.Object) null))
            return;
          building.NetworkHQ = building.MapHQ;
        }
      }
      else if (savedUnit.PlacementType == PlacementType.Override)
      {
        if (savedUnit.AirbaseRef != null)
          building.SetAirbase(savedUnit.AirbaseRef.Airbase);
        else
          building.NetworkHQ = savedUnit.FindHQ();
        building.capturable = savedUnit.capturable;
        Factory component;
        if (savedUnit.factoryOptions == null || !building.TryGetComponent<Factory>(out component))
          return;
        component.SetFactory(savedUnit.factoryOptions);
      }
      else
        Debug.LogError((object) $"Building should not have placement type {savedUnit.PlacementType} before units have spawned");
    }
  }

  public void BeforeSave()
  {
    this.objectives = MissionObjectivesFactory.Save(this.Objectives);
    foreach (SavedBuilding building in this.buildings)
      building.SaveAirbaseString();
    foreach (SavedAirbase airbase in this.airbases)
      airbase.BeforeSave();
  }

  public void EnsureFactionExists(Faction faction, out MissionFaction missionFaction)
  {
    if (this.TryGetFaction(faction.factionName, out missionFaction))
      return;
    missionFaction = new MissionFaction(faction.factionName);
    this.factions.Add(missionFaction);
  }

  public void GetFactionFromHq(FactionHQ factionHQ, out MissionFaction missionFaction)
  {
    this.EnsureFactionExists(factionHQ.faction, out missionFaction);
    missionFaction.FactionHQ = factionHQ;
  }

  public bool TryGetFaction(string name, out MissionFaction faction)
  {
    foreach (MissionFaction faction1 in this.factions)
    {
      if (faction1.factionName == name)
      {
        faction = faction1;
        return true;
      }
    }
    faction = (MissionFaction) null;
    return false;
  }

  private void SetupAirbase(ServerObjectManager som)
  {
    Dictionary<string, Airbase> airbaseLookup = FactionRegistry.airbaseLookup;
    bool flag = (UnityEngine.Object) som != (UnityEngine.Object) null;
    List<SavedAirbase> airbaseToSpawn = new List<SavedAirbase>();
    foreach (Airbase airbase in airbaseLookup.Values)
      airbase.UnlinkSavedAirbase();
    foreach (SavedAirbase airbase1 in this.airbases)
    {
      airbase1.SavedInMission = true;
      Airbase airbase2;
      if (airbaseLookup.TryGetValue(airbase1.UniqueName, out airbase2))
      {
        ColorLog<Mission>.Info($"Found {airbase1.UniqueName} in scene");
        airbase2.LinkSavedAirbase(airbase1, airbase2.IsCustom);
        foreach (SavedBuilding allSavedBuilding in (IEnumerable<SavedBuilding>) MissionManager.GetAllSavedBuildings(this, true))
        {
          if (allSavedBuilding.Airbase == airbase1.UniqueName)
            allSavedBuilding.SetAirbase(airbase1);
        }
      }
      else
      {
        ColorLog<Mission>.Info($"Did not find {airbase1.UniqueName} in scene");
        if (!airbase1.IsOverride & flag)
          airbaseToSpawn.Add(airbase1);
      }
    }
    foreach (Airbase airbase in airbaseLookup.Values)
    {
      if (!airbase.SavedAirbase.SavedInMission)
        airbase.LinkSavedAirbase(airbase.SavedAirbase, false);
    }
    if (!flag)
      return;
    this.SpawnAllCustomAirbase(airbaseToSpawn, som);
  }

  private void SpawnAllCustomAirbase(List<SavedAirbase> airbaseToSpawn, ServerObjectManager som)
  {
    foreach (SavedAirbase saved in airbaseToSpawn)
      this.SpawnCustomAirbase(saved, som);
  }

  public Airbase SpawnCustomAirbase(SavedAirbase saved, ServerObjectManager som)
  {
    ColorLog<Airbase>.Info("Spawn custom airbase " + saved.UniqueName);
    GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(GameAssets.i.airbasePrefab, Datum.origin);
    gameObject.name = saved.UniqueName;
    Airbase component = gameObject.GetComponent<Airbase>();
    component.SetupCustomAirbase(saved);
    som.Spawn(component.Identity);
    return component;
  }
}

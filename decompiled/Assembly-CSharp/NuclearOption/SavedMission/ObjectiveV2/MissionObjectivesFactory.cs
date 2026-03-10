// Decompiled with JetBrains decompiler
// Type: NuclearOption.SavedMission.ObjectiveV2.MissionObjectivesFactory
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.SavedMission.ObjectiveV2.Outcomes;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#nullable disable
namespace NuclearOption.SavedMission.ObjectiveV2;

public class MissionObjectivesFactory
{
  public static string MissionStartName => "Mission Start";

  public static string MissionStartSpawnUnitName => "Mission Start Default Spawn Units";

  public static void AssertSceneAirbaseRegistered()
  {
  }

  public static MissionObjectives Load(Mission mission, SavedMissionObjectives missionObjectives)
  {
    LoadErrors loadErrors = new LoadErrors();
    MissionLookups lookups = new MissionLookups(loadErrors);
    Objective[] source1 = new Objective[missionObjectives.Objectives.Count];
    Outcome[] source2 = new Outcome[missionObjectives.Outcomes.Count];
    IReadOnlyList<SavedUnit> allSavedUnits = MissionManager.GetAllSavedUnits(mission, true);
    List<Exception> errors = (List<Exception>) null;
    foreach (SavedUnit savedUnit in (IEnumerable<SavedUnit>) allSavedUnits)
    {
      if (!string.IsNullOrEmpty(savedUnit.UniqueName))
        lookups.SavedUnits.Add(savedUnit.UniqueName, savedUnit);
    }
    foreach (SavedAirbase airbase in mission.airbases)
    {
      if (!string.IsNullOrEmpty(airbase.UniqueName))
        lookups.Airbases.Add(airbase.UniqueName, airbase);
    }
    MissionObjectivesFactory.AssertSceneAirbaseRegistered();
    foreach (KeyValuePair<string, Airbase> keyValuePair in FactionRegistry.airbaseLookup)
    {
      string key = keyValuePair.Key;
      Airbase airbase = keyValuePair.Value;
      lookups.Airbases.TryAdd(key, airbase.SavedAirbase);
    }
    for (int index = 0; index < source1.Length; ++index)
    {
      source1[index] = SavedObjective.Create(missionObjectives.Objectives[index].Type);
      source1[index].SavedObjective = missionObjectives.Objectives[index];
      lookups.Objectives.Add(missionObjectives.Objectives[index].UniqueName, source1[index]);
    }
    for (int index = 0; index < source2.Length; ++index)
    {
      source2[index] = SavedOutcome.Create(missionObjectives.Outcomes[index].Type);
      source2[index].SavedOutcome = missionObjectives.Outcomes[index];
      lookups.Outcomes.Add(missionObjectives.Outcomes[index].UniqueName, source2[index]);
    }
    for (int index = 0; index < source1.Length; ++index)
    {
      try
      {
        source1[index].Load(lookups);
      }
      catch (Exception ex)
      {
        if (errors == null)
          errors = new List<Exception>();
        errors.Add(ex);
      }
    }
    for (int index = 0; index < source2.Length; ++index)
    {
      try
      {
        source2[index].Load(lookups);
      }
      catch (Exception ex)
      {
        if (errors == null)
          errors = new List<Exception>();
        errors.Add(ex);
      }
    }
    Objective startObjective;
    if (!lookups.Objectives.TryGetValue(MissionObjectivesFactory.MissionStartName, out startObjective))
    {
      if (errors == null)
        errors = new List<Exception>();
      errors.Add((Exception) new KeyNotFoundException("Mission must have objective with name " + MissionObjectivesFactory.MissionStartName));
    }
    if (errors != null)
    {
      loadErrors.AddExceptions(errors);
      throw new MissionLoadException(loadErrors);
    }
    return new MissionObjectives(((IEnumerable<Objective>) source1).ToList<Objective>(), ((IEnumerable<Outcome>) source2).ToList<Outcome>(), startObjective, loadErrors);
  }

  public static void AddStartingUnits(
    MissionObjectives missionObjectives,
    IReadOnlyList<SavedUnit> allUnits)
  {
    MissionObjectivesFactory.AddStartingUnits(missionObjectives.StartObjective, (IReadOnlyList<Outcome>) missionObjectives.AllOutcomes, allUnits);
  }

  private static void AddStartingUnits(
    Objective startObjective,
    IReadOnlyList<Outcome> outcomes,
    IReadOnlyList<SavedUnit> allUnits)
  {
    HashSet<SavedUnit> unspawnedUnits = MissionObjectivesFactory.GetUnspawnedUnits(outcomes, allUnits);
    if (unspawnedUnits.Count == 0)
      return;
    SpawnUnitOutcome spawnUnitOutcome1 = new SpawnUnitOutcome();
    spawnUnitOutcome1.SavedOutcome = new SavedOutcome()
    {
      UniqueName = MissionObjectivesFactory.MissionStartSpawnUnitName
    };
    spawnUnitOutcome1.UnitsToSpawn = new List<SavedUnit>((IEnumerable<SavedUnit>) unspawnedUnits);
    SpawnUnitOutcome spawnUnitOutcome2 = spawnUnitOutcome1;
    spawnUnitOutcome2.UnitsToSpawn.Sort((Comparison<SavedUnit>) ((x, y) =>
    {
      if (!(x is SavedAircraft x2))
        return 1;
      return !(y is SavedAircraft y2) ? -1 : Spawner.SortControlledAircraft(x2, y2);
    }));
    startObjective.Outcomes.Add((Outcome) spawnUnitOutcome2);
  }

  private static HashSet<SavedUnit> GetUnspawnedUnits(
    IReadOnlyList<Outcome> outcomes,
    IReadOnlyList<SavedUnit> allUnits)
  {
    HashSet<SavedUnit> unspawnedUnits = new HashSet<SavedUnit>(allUnits.Where<SavedUnit>((Func<SavedUnit, bool>) (x => x.PlacementType == PlacementType.Custom)));
    foreach (Outcome outcome in (IEnumerable<Outcome>) outcomes)
    {
      if (outcome is SpawnUnitOutcome spawnUnitOutcome)
      {
        foreach (SavedUnit savedUnit in spawnUnitOutcome.UnitsToSpawn)
          unspawnedUnits.Remove(savedUnit);
      }
    }
    return unspawnedUnits;
  }

  public static SavedMissionObjectives Save(MissionObjectives runtime)
  {
    Dictionary<string, Objective> dictionary1 = new Dictionary<string, Objective>();
    Dictionary<string, Outcome> dictionary2 = new Dictionary<string, Outcome>();
    foreach (Outcome allOutcome in runtime.AllOutcomes)
      dictionary2.Add(allOutcome.SavedOutcome.UniqueName, allOutcome);
    foreach (Objective allObjective in runtime.AllObjectives)
    {
      dictionary1.Add(allObjective.SavedObjective.UniqueName, allObjective);
      foreach (Outcome outcome1 in allObjective.Outcomes)
      {
        Outcome outcome2;
        if (dictionary2.TryGetValue(outcome1.SavedOutcome.UniqueName, out outcome2))
        {
          if (outcome2 != outcome1)
            throw new ArgumentException("Multiple Outcomes with same UniqueName");
        }
        else
          Debug.LogError((object) "Outcome was in AllObjectives list but not in AllOutcomes");
      }
    }
    foreach (Objective objective in dictionary1.Values)
      objective.Save();
    foreach (Outcome outcome in dictionary2.Values)
      outcome.Save();
    SavedMissionObjectives missionObjectives = new SavedMissionObjectives()
    {
      Objectives = new List<SavedObjective>(),
      Outcomes = new List<SavedOutcome>()
    };
    dictionary1[MissionObjectivesFactory.MissionStartName].SavedObjective.Outcomes.Remove(MissionObjectivesFactory.MissionStartSpawnUnitName);
    dictionary2.Remove(MissionObjectivesFactory.MissionStartSpawnUnitName);
    missionObjectives.Objectives.AddRange(dictionary1.Values.Select<Objective, SavedObjective>((Func<Objective, SavedObjective>) (x => x.SavedObjective)));
    missionObjectives.Outcomes.AddRange(dictionary2.Values.Select<Outcome, SavedOutcome>((Func<Outcome, SavedOutcome>) (x => x.SavedOutcome)));
    return missionObjectives;
  }
}

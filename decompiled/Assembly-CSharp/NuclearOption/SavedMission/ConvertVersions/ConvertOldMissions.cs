// Decompiled with JetBrains decompiler
// Type: NuclearOption.SavedMission.ConvertVersions.ConvertOldMissions
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.SavedMission.ObjectiveV2;
using NuclearOption.SavedMission.ObjectiveV2.Outcomes;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#nullable disable
namespace NuclearOption.SavedMission.ConvertVersions;

[Obsolete("")]
public static class ConvertOldMissions
{
  public static SavedMissionObjectives ConvertObjective(
    List<MissionFaction> factions,
    IReadOnlyList<SavedUnit> units)
  {
    ConvertOldMissions.AllObjectives allObjectives = new ConvertOldMissions.AllObjectives();
    SavedObjective start = new SavedObjective()
    {
      UniqueName = "Mission Start",
      Type = ObjectiveType.None,
      Faction = "",
      Outcomes = new List<string>(),
      Hidden = true
    };
    allObjectives.SetStartMission(start);
    ConvertOldMissions.FixUnitNames(units);
    foreach (MissionFaction faction in factions)
      ConvertOldMissions.Convert(faction.factionName, faction.objectives, allObjectives, units);
    return allObjectives.ToMissionObjectives();
  }

  private static void FixUnitNames(IReadOnlyList<SavedUnit> units)
  {
    HashSet<string> others = new HashSet<string>();
    foreach (SavedUnit unit in (IEnumerable<SavedUnit>) units)
    {
      if (!string.IsNullOrEmpty(unit.UniqueName))
      {
        SaveHelper.MakeUnique(ref unit.UniqueName, others);
        others.Add(unit.UniqueName);
      }
      else if (!string.IsNullOrEmpty(unit.unitCustomID))
      {
        unit.UniqueName = unit.unitCustomID;
        SaveHelper.MakeUnique(ref unit.UniqueName, others);
        others.Add(unit.UniqueName);
      }
    }
    foreach (SavedUnit unit in (IEnumerable<SavedUnit>) units)
    {
      if (string.IsNullOrEmpty(unit.UniqueName))
      {
        unit.UniqueName = unit.type;
        SaveHelper.MakeUnique(ref unit.UniqueName, others, false);
        others.Add(unit.UniqueName);
      }
    }
  }

  private static void Convert(
    string faction,
    List<MissionObjective> old,
    ConvertOldMissions.AllObjectives allObjectives,
    IReadOnlyList<SavedUnit> units)
  {
    SavedOutcome outcome1 = new SavedOutcome();
    SavedOutcome savedOutcome;
    if (old.Any<MissionObjective>((Func<MissionObjective, bool>) (x => x.victoryObjective)))
    {
      savedOutcome = new SavedOutcome();
      savedOutcome.UniqueName = "Victory Outcome";
      savedOutcome.Type = OutcomeType.EndGame;
      outcome1 = savedOutcome;
      allObjectives.Add(ref outcome1);
    }
    List<string>[] stringListArray = new List<string>[old.Count - 1];
    for (int index = 0; index < stringListArray.Length; ++index)
      stringListArray[index] = new List<string>();
    for (int index = 0; index < old.Count; ++index)
    {
      MissionObjective old1 = old[index];
      SavedObjective newObjective = ConvertOldMissions.CreateFromOld(old1, faction);
      if (!string.IsNullOrEmpty(old1.message))
      {
        SavedOutcome outcome2 = ConvertOldMissions.AddMessageOutcome(old1);
        allObjectives.Add(ref outcome2);
        newObjective.Outcomes.Add(outcome2.UniqueName);
      }
      SavedUnit[] array = units.Where<SavedUnit>((Func<SavedUnit, bool>) (x => x.spawnTiming == newObjective.UniqueName)).ToArray<SavedUnit>();
      if (array.Length != 0)
      {
        SavedOutcome outcome3 = ConvertOldMissions.AddSpawnUnitOutcome(old1, array);
        allObjectives.Add(ref outcome3);
        newObjective.Outcomes.Add(outcome3.UniqueName);
      }
      if (old1.victoryObjective)
        newObjective.Outcomes.Add(outcome1.UniqueName);
      ObjectiveData objectiveData1;
      if (index < old.Count - 1)
      {
        savedOutcome = new SavedOutcome();
        savedOutcome.UniqueName = "StartNext_" + old[index + 1].objectiveName;
        savedOutcome.Type = OutcomeType.StartObjective;
        ref SavedOutcome local = ref savedOutcome;
        List<ObjectiveData> objectiveDataList = new List<ObjectiveData>();
        objectiveData1 = new ObjectiveData();
        objectiveData1.StringValue = "NEXT_OUTCOME_PLACEHOLDER";
        objectiveData1.FloatValue = (float) index;
        objectiveDataList.Add(objectiveData1);
        local.Data = objectiveDataList;
        SavedOutcome outcome4 = savedOutcome;
        allObjectives.Add(ref outcome4);
        newObjective.Outcomes.Add(outcome4.UniqueName);
      }
      bool flag1 = old1.positionTrigger && (double) old1.triggerRange > 0.0;
      bool flag2 = old1.targetUnits.Count > 0;
      if (flag1 & flag2)
      {
        newObjective.Type = ObjectiveType.CompleteOtherObjective;
        newObjective.Hidden = true;
        SavedObjective fromOld1 = ConvertOldMissions.CreateFromOld(old1, faction);
        ConvertOldMissions.ReachWaypoint(ref fromOld1, old1, true);
        allObjectives.Add(ref fromOld1);
        if (index > 0)
          stringListArray[index - 1].Add(fromOld1.UniqueName);
        List<ObjectiveData> data1 = newObjective.Data;
        objectiveData1 = new ObjectiveData();
        objectiveData1.StringValue = fromOld1.UniqueName;
        ObjectiveData objectiveData2 = objectiveData1;
        data1.Add(objectiveData2);
        savedOutcome = new SavedOutcome();
        savedOutcome.UniqueName = "Complete_" + fromOld1.UniqueName;
        savedOutcome.Type = OutcomeType.StopOrCompleteObjective;
        ref SavedOutcome local1 = ref savedOutcome;
        List<ObjectiveData> objectiveDataList1 = new List<ObjectiveData>();
        objectiveData1 = new ObjectiveData();
        objectiveData1.FloatValue = 0.0f;
        objectiveDataList1.Add(objectiveData1);
        objectiveData1 = new ObjectiveData();
        objectiveData1.StringValue = fromOld1.UniqueName;
        objectiveDataList1.Add(objectiveData1);
        local1.Data = objectiveDataList1;
        SavedOutcome outcome5 = savedOutcome;
        allObjectives.Add(ref outcome5);
        newObjective.Outcomes.Add(outcome5.UniqueName);
        SavedObjective fromOld2 = ConvertOldMissions.CreateFromOld(old1, faction);
        ConvertOldMissions.DestroyUnits(ref fromOld2, old1, true);
        allObjectives.Add(ref fromOld2);
        if (index > 0)
          stringListArray[index - 1].Add(fromOld2.UniqueName);
        List<ObjectiveData> data2 = newObjective.Data;
        objectiveData1 = new ObjectiveData();
        objectiveData1.StringValue = fromOld2.UniqueName;
        ObjectiveData objectiveData3 = objectiveData1;
        data2.Add(objectiveData3);
        savedOutcome = new SavedOutcome();
        savedOutcome.UniqueName = "Complete_" + fromOld2.UniqueName;
        savedOutcome.Type = OutcomeType.StopOrCompleteObjective;
        ref SavedOutcome local2 = ref savedOutcome;
        List<ObjectiveData> objectiveDataList2 = new List<ObjectiveData>();
        objectiveData1 = new ObjectiveData();
        objectiveData1.FloatValue = 0.0f;
        objectiveDataList2.Add(objectiveData1);
        objectiveData1 = new ObjectiveData();
        objectiveData1.StringValue = fromOld2.UniqueName;
        objectiveDataList2.Add(objectiveData1);
        local2.Data = objectiveDataList2;
        SavedOutcome outcome6 = savedOutcome;
        allObjectives.Add(ref outcome6);
        newObjective.Outcomes.Add(outcome6.UniqueName);
        List<ObjectiveData> data3 = newObjective.Data;
        objectiveData1 = new ObjectiveData();
        objectiveData1.FloatValue = 0.0f;
        ObjectiveData objectiveData4 = objectiveData1;
        data3.Insert(0, objectiveData4);
      }
      else if (flag1)
        ConvertOldMissions.ReachWaypoint(ref newObjective, old1, false);
      else if (flag2)
        ConvertOldMissions.DestroyUnits(ref newObjective, old1, false);
      else if (old1.objectiveName != "Mission Start")
        Debug.LogWarning((object) $"Objective '{old1.objectiveName}' could not be converted and will be left as type None");
      allObjectives.Add(ref newObjective);
      if (index > 0)
        stringListArray[index - 1].Add(newObjective.UniqueName);
    }
    for (int index = 0; index < allObjectives.outcomes.Count; ++index)
    {
      if (allObjectives.outcomes[index].UniqueName.StartsWith("StartNext_"))
      {
        SavedOutcome outcome7 = allObjectives.outcomes[index];
        if (!(outcome7.Data[0].StringValue != "NEXT_OUTCOME_PLACEHOLDER"))
        {
          int floatValue = (int) outcome7.Data[0].FloatValue;
          List<string> source = stringListArray[floatValue];
          outcome7.Data = source.Select<string, ObjectiveData>((Func<string, ObjectiveData>) (x => new ObjectiveData()
          {
            StringValue = x
          })).ToList<ObjectiveData>();
          allObjectives.outcomes[index] = outcome7;
        }
      }
    }
  }

  private static SavedObjective CreateFromOld(MissionObjective old, string faction)
  {
    return new SavedObjective()
    {
      UniqueName = old.objectiveName,
      DisplayName = old.objectiveName,
      Hidden = false,
      Data = new List<ObjectiveData>(),
      Outcomes = new List<string>(),
      Type = ObjectiveType.None,
      Faction = faction
    };
  }

  private static void DestroyUnits(
    ref SavedObjective objective,
    MissionObjective old,
    bool setName)
  {
    if (setName)
    {
      // ISSUE: explicit reference operation
      ^ref objective.UniqueName += "_DestroyUnit";
    }
    objective.Type = ObjectiveType.DestroyUnits;
    objective.Hidden = false;
    objective.Data = old.targetUnits.Select<string, ObjectiveData>((Func<string, ObjectiveData>) (x => new ObjectiveData()
    {
      StringValue = x
    })).ToList<ObjectiveData>();
    objective.Data.Insert(0, new ObjectiveData()
    {
      FloatValue = 1f
    });
  }

  private static void ReachWaypoint(
    ref SavedObjective objective,
    MissionObjective old,
    bool setName)
  {
    if (setName)
    {
      // ISSUE: explicit reference operation
      ^ref objective.UniqueName += "_ReachWaypoints";
    }
    objective.Type = ObjectiveType.ReachWaypoints;
    objective.Hidden = false;
    ref SavedObjective local = ref objective;
    List<ObjectiveData> objectiveDataList = new List<ObjectiveData>();
    ObjectiveData objectiveData1 = new ObjectiveData();
    objectiveData1.VectorValue = old.position;
    objectiveData1.FloatValue = old.triggerRange;
    objectiveDataList.Add(objectiveData1);
    local.Data = objectiveDataList;
    List<ObjectiveData> data = objective.Data;
    objectiveData1 = new ObjectiveData();
    objectiveData1.FloatValue = 0.0f;
    ObjectiveData objectiveData2 = objectiveData1;
    data.Insert(0, objectiveData2);
  }

  private static SavedOutcome AddMessageOutcome(MissionObjective old)
  {
    ShowMessageOutcome source = new ShowMessageOutcome();
    source.LoadNew(new SavedOutcome()
    {
      UniqueName = "Message_" + old.objectiveName,
      Type = OutcomeType.ShowMessage
    });
    source.Message = old.message;
    source.PlaySound.SetValue(true, (object) source, true);
    source.ObjectiveFactionOnly.SetValue(true, (object) source, true);
    source.Save();
    return source.SavedOutcome;
  }

  private static SavedOutcome AddSpawnUnitOutcome(MissionObjective old, SavedUnit[] units)
  {
    SpawnUnitOutcome spawnUnitOutcome1 = new SpawnUnitOutcome();
    spawnUnitOutcome1.SavedOutcome = new SavedOutcome()
    {
      UniqueName = "SpawnUnits_" + old.objectiveName,
      Type = OutcomeType.SpawnUnit
    };
    SpawnUnitOutcome spawnUnitOutcome2 = spawnUnitOutcome1;
    if (spawnUnitOutcome2.UnitsToSpawn == null)
      spawnUnitOutcome2.UnitsToSpawn = new List<SavedUnit>();
    spawnUnitOutcome1.UnitsToSpawn.AddRange((IEnumerable<SavedUnit>) units);
    spawnUnitOutcome1.Save();
    return spawnUnitOutcome1.SavedOutcome;
  }

  private class AllObjectives
  {
    public List<SavedObjective> objectives = new List<SavedObjective>();
    public List<SavedOutcome> outcomes = new List<SavedOutcome>();
    private bool hasStart;

    public SavedMissionObjectives ToMissionObjectives()
    {
      return new SavedMissionObjectives()
      {
        Objectives = this.objectives,
        Outcomes = this.outcomes
      };
    }

    public void Add(ref SavedObjective objective)
    {
      int num = !this.hasStart ? 0 : (objective.UniqueName == MissionObjectivesFactory.MissionStartName ? 1 : 0);
      SaveHelper.MakeUnique(ref objective.UniqueName, this.objectives.Select<SavedObjective, string>((Func<SavedObjective, string>) (x => x.UniqueName)));
      this.objectives.Add(objective);
      if (num == 0)
        return;
      SavedOutcome outcome = new SavedOutcome()
      {
        UniqueName = "Start_" + objective.UniqueName,
        Type = OutcomeType.StartObjective,
        Data = new List<ObjectiveData>()
        {
          new ObjectiveData() { StringValue = objective.UniqueName }
        }
      };
      this.Add(ref outcome);
      this.objectives[0].Outcomes.Add(outcome.UniqueName);
    }

    public void Add(ref SavedOutcome outcome)
    {
      SaveHelper.MakeUnique(ref outcome.UniqueName, this.outcomes.Select<SavedOutcome, string>((Func<SavedOutcome, string>) (x => x.UniqueName)));
      this.outcomes.Add(outcome);
    }

    internal void SetStartMission(SavedObjective start)
    {
      this.Add(ref start);
      this.hasStart = true;
    }
  }
}

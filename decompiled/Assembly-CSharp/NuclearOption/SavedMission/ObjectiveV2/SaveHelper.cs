// Decompiled with JetBrains decompiler
// Type: NuclearOption.SavedMission.ObjectiveV2.SaveHelper
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

public static class SaveHelper
{
  public static string GetNameSavedCheckDestroyed<T>(this T reference) where T : ISaveableReference
  {
    reference.ThrowIfDestroyed();
    return reference.UniqueName;
  }

  public static void ThrowIfDestroyed(this ISaveableReference reference)
  {
    if (reference.Destroyed)
      throw new InvalidOperationException($"Object Destroyed, ${reference}");
  }

  public static void ValidateEnum<T>(T value) where T : Enum
  {
    if (Enum.IsDefined(typeof (T), (object) value))
      return;
    Debug.LogError((object) $"Value '{value}' was not a valid {typeof (T).FullName}");
  }

  public static int CountStartedBy(MissionObjectives missionObjectives, Objective objective)
  {
    int num = 0;
    foreach (Outcome allOutcome in missionObjectives.AllOutcomes)
    {
      if (allOutcome is StartObjectiveOutcome objectiveOutcome && objectiveOutcome.objectivesToStart.Contains(objective))
        ++num;
    }
    return num;
  }

  public static int CountUsedBy(MissionObjectives missionObjectives, Outcome outcome)
  {
    int num = 0;
    foreach (Objective allObjective in missionObjectives.AllObjectives)
    {
      if (allObjective.Outcomes.Contains(outcome))
        ++num;
    }
    return num;
  }

  public static int CountSpawnedBy(MissionObjectives missionObjectives, SavedUnit savedUnit)
  {
    int num = 0;
    foreach (Outcome allOutcome in missionObjectives.AllOutcomes)
    {
      if (allOutcome is SpawnUnitOutcome spawnUnitOutcome && spawnUnitOutcome.UnitsToSpawn.Contains(savedUnit))
        ++num;
    }
    return num;
  }

  public static bool MakeUnique(this MissionObjectives mission, Objective objective)
  {
    return mission.MakeUnique(ref objective.SavedObjective.UniqueName, objective);
  }

  public static bool MakeUnique(
    this MissionObjectives mission,
    ref string name,
    Objective objective)
  {
    IEnumerable<string> others = mission.AllObjectives.Where<Objective>((Func<Objective, bool>) (x => x != objective)).Select<Objective, string>((Func<Objective, string>) (x => x.SavedObjective.UniqueName));
    return SaveHelper.MakeUnique(ref name, others);
  }

  public static bool MakeUnique(this MissionObjectives mission, Outcome outcome)
  {
    return mission.MakeUnique(ref outcome.SavedOutcome.UniqueName, outcome);
  }

  public static bool MakeUnique(this MissionObjectives mission, ref string name, Outcome outcome)
  {
    IEnumerable<string> others = mission.AllOutcomes.Where<Outcome>((Func<Outcome, bool>) (x => x != outcome)).Select<Outcome, string>((Func<Outcome, string>) (x => x.SavedOutcome.UniqueName));
    return SaveHelper.MakeUnique(ref name, others);
  }

  public static bool MakeUnique(
    ref string name,
    ISaveableReference current,
    IEnumerable<ISaveableReference> others,
    bool warn = true)
  {
    IEnumerable<string> others1 = others.Where<ISaveableReference>((Func<ISaveableReference, bool>) (x => x != current)).Select<ISaveableReference, string>((Func<ISaveableReference, string>) (x => x.UniqueName));
    return SaveHelper.MakeUnique(ref name, others1, warn);
  }

  public static bool MakeUnique(ref string name, IEnumerable<ISaveableReference> others, bool warn = true)
  {
    IEnumerable<string> others1 = others.Select<ISaveableReference, string>((Func<ISaveableReference, string>) (x => x.UniqueName));
    return SaveHelper.MakeUnique(ref name, others1, warn);
  }

  public static bool MakeUnique<T>(ref string name, Dictionary<string, T> others, bool warn = true)
  {
    return SaveHelper.MakeUnique(ref name, new Func<string, bool>(others.ContainsKey), warn);
  }

  public static bool MakeUnique(ref string name, HashSet<string> others, bool warn = true)
  {
    return SaveHelper.MakeUnique(ref name, new Func<string, bool>(others.Contains), warn);
  }

  public static bool MakeUnique(ref string name, IEnumerable<string> others, bool warn = true)
  {
    return SaveHelper.MakeUnique(ref name, (Func<string, bool>) (checkName => others.Any<string>((Func<string, bool>) (x => x == checkName))), warn);
  }

  private static bool MakeUnique(ref string name, Func<string, bool> contains, bool warn = true)
  {
    if (string.IsNullOrEmpty(name))
      name = "empty";
    int num = 0;
    string str1 = "";
    while (true)
    {
      string str2 = name + str1;
      if (contains(str2))
      {
        if (warn)
        {
          Debug.LogWarning((object) ("non-unique name found " + name));
          warn = false;
        }
        ++num;
        str1 = $"_{num}";
      }
      else
        break;
    }
    if (string.IsNullOrEmpty(str1))
      return false;
    name += str1;
    return true;
  }
}

// Decompiled with JetBrains decompiler
// Type: NuclearOption.SavedMission.ObjectiveV2.MissionObjectives
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;

#nullable disable
namespace NuclearOption.SavedMission.ObjectiveV2;

public class MissionObjectives
{
  public Objective StartObjective;
  public readonly List<Objective> AllObjectives;
  public readonly List<Outcome> AllOutcomes;
  public readonly LoadErrors LoadErrors;

  public MissionObjectives(
    List<Objective> objectives,
    List<Outcome> outcomes,
    Objective startObjective,
    LoadErrors loadErrors)
  {
    this.AllObjectives = objectives;
    this.AllOutcomes = outcomes;
    this.StartObjective = startObjective;
    this.LoadErrors = loadErrors;
  }

  public void AddNewObjective(Objective obj)
  {
    obj.ThrowIfDestroyed();
    if (this.AllObjectives.Contains(obj))
      throw new ArgumentException($"Objective already in list: {obj}");
    this.MakeUnique(obj);
    this.AllObjectives.Add(obj);
  }

  public void AddNewOutcome(Outcome outcome, Objective parent = null)
  {
    outcome.ThrowIfDestroyed();
    if (this.AllOutcomes.Contains(outcome))
      throw new ArgumentException($"Outcome already in list: {outcome}");
    this.MakeUnique(outcome);
    this.AllOutcomes.Add(outcome);
    parent?.Outcomes.Add(outcome);
  }

  public void AddExistingOutcome(Outcome outcome, Objective parent)
  {
    outcome.ThrowIfDestroyed();
    if (!this.AllOutcomes.Contains(outcome))
      throw new ArgumentException($"Outcome was NOT in list: {outcome}");
    parent.Outcomes.Add(outcome);
  }

  public void RemoveObjectiveAt(int index)
  {
    Objective allObjective = this.AllObjectives[index];
    this.AllObjectives.RemoveAt(index);
    this.ReferenceDestroyed((ISaveableReference) allObjective);
  }

  public void RemoveOutcome(Outcome outcome)
  {
    int index = this.AllOutcomes.IndexOf(outcome);
    if (index < 0)
      return;
    this.RemoveOutcomeAt(index);
  }

  public void RemoveOutcomeAt(int index)
  {
    Outcome allOutcome = this.AllOutcomes[index];
    this.AllOutcomes.RemoveAt(index);
    foreach (Objective allObjective in this.AllObjectives)
      allObjective.Outcomes.Remove(allOutcome);
    this.ReferenceDestroyed((ISaveableReference) allOutcome);
  }

  public void ReferenceDestroyed(ISaveableReference reference)
  {
    reference.Destroyed = true;
    foreach (Objective allObjective in this.AllObjectives)
      allObjective.ReferenceDestroyed(reference);
    foreach (Outcome allOutcome in this.AllOutcomes)
      allOutcome.ReferenceDestroyed(reference);
  }

  public Objective GetObjective(string name)
  {
    foreach (Objective allObjective in this.AllObjectives)
    {
      if (allObjective.SavedObjective.UniqueName == name)
        return allObjective;
    }
    throw new KeyNotFoundException("Could not find mission with name " + name);
  }
}

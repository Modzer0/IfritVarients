// Decompiled with JetBrains decompiler
// Type: MissionRunner
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.SavedMission.ObjectiveV2;
using System;
using System.Collections.Generic;
using UnityEngine;

#nullable disable
public class MissionRunner
{
  private readonly MissionObjectives objectives;
  public readonly List<Objective> ActiveObjectives = new List<Objective>();
  public readonly Dictionary<FactionHQ, List<Objective>> activeByFaction = new Dictionary<FactionHQ, List<Objective>>();
  private readonly List<Objective> completeTemp = new List<Objective>();

  public event Action<Objective> OnObjectiveStart;

  public event Action<Objective> OnObjectiveCompleted;

  public MissionRunner(MissionObjectives objectives) => this.objectives = objectives;

  public void OnMissionStart() => this.StartObjective(this.objectives.StartObjective);

  public void Update()
  {
    this.completeTemp.Clear();
    foreach (Objective activeObjective in this.ActiveObjectives)
    {
      if (activeObjective.UpdateAndCheck())
        this.completeTemp.Add(activeObjective);
    }
    foreach (Objective objective in this.completeTemp)
      this.CompleteObjective(objective);
  }

  public void ClientOnlyUpdate()
  {
    foreach (Objective activeObjective in this.ActiveObjectives)
      activeObjective.ClientOnlyUpdate();
  }

  internal void StartObjective(Objective obj, bool addToActive = true)
  {
    if (obj.Status != ObjectiveStatus.NotStarted)
      return;
    obj.Status = ObjectiveStatus.Running;
    if (addToActive)
      this.AddActiveObjective(obj);
    Action<Objective> onObjectiveStart = this.OnObjectiveStart;
    if (onObjectiveStart == null)
      return;
    onObjectiveStart(obj);
  }

  public void StopObjective(Objective obj)
  {
    if (obj.Status == ObjectiveStatus.Complete)
      return;
    obj.Status = ObjectiveStatus.Complete;
    this.RemoveActiveObjective(obj);
  }

  public void CompleteObjective(Objective obj)
  {
    if (obj.Status == ObjectiveStatus.Complete)
      return;
    obj.Status = ObjectiveStatus.Complete;
    obj.Complete();
    obj.Cleanup();
    this.RemoveActiveObjective(obj);
    Action<Objective> objectiveCompleted = this.OnObjectiveCompleted;
    if (objectiveCompleted == null)
      return;
    objectiveCompleted(obj);
  }

  private void AddActiveObjective(Objective obj)
  {
    FactionHQ factionHq = obj.FactionHQ;
    if (obj.NeedsFaction && (UnityEngine.Object) factionHq == (UnityEngine.Object) null)
    {
      Debug.LogError((object) $"{obj.SavedObjective.Type} needs a faction");
    }
    else
    {
      this.ActiveObjectives.Add(obj);
      if ((UnityEngine.Object) factionHq != (UnityEngine.Object) null)
      {
        List<Objective> objectiveList;
        if (!this.activeByFaction.TryGetValue(factionHq, out objectiveList))
        {
          objectiveList = new List<Objective>();
          this.activeByFaction[factionHq] = objectiveList;
        }
        objectiveList.Add(obj);
      }
      obj.OnStart();
    }
  }

  private void RemoveActiveObjective(Objective obj)
  {
    this.ActiveObjectives.Remove(obj);
    if (!((UnityEngine.Object) obj.FactionHQ != (UnityEngine.Object) null))
      return;
    this.activeByFaction[obj.FactionHQ].Remove(obj);
  }

  public void SetAllRemoteActiveObjectives(
    IReadOnlyList<string> names,
    IReadOnlyDictionary<string, List<int>> dataLookup)
  {
    this.ActiveObjectives.Clear();
    this.activeByFaction.Clear();
    foreach (string name in (IEnumerable<string>) names)
    {
      Objective objective = this.objectives.GetObjective(name);
      this.AddActiveObjective(objective);
      List<int> data;
      if (dataLookup.TryGetValue(name, out data))
        objective.ReceiveNetworkData(data);
    }
  }

  public void ClearRemoteActiveObjectives()
  {
    this.ActiveObjectives.Clear();
    this.activeByFaction.Clear();
  }

  public Objective AddRemoteActiveObjective(string name)
  {
    Objective objective = this.objectives.GetObjective(name);
    this.AddActiveObjective(objective);
    return objective;
  }

  public Objective RemoveRemoteActiveObjective(string name)
  {
    Objective objective = this.objectives.GetObjective(name);
    this.RemoveActiveObjective(objective);
    return objective;
  }

  public void Cleanup()
  {
    foreach (Objective activeObjective in this.ActiveObjectives)
      activeObjective.Cleanup();
  }
}

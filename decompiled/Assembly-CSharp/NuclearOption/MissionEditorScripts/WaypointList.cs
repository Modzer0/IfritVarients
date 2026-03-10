// Decompiled with JetBrains decompiler
// Type: NuclearOption.MissionEditorScripts.WaypointList
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.MissionEditorScripts.MultiSelect;
using NuclearOption.SavedMission;
using NuclearOption.SavedMission.ObjectiveV2;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

#nullable disable
namespace NuclearOption.MissionEditorScripts;

public class WaypointList : MonoBehaviour
{
  [SerializeField]
  private Button addWaypointButton;
  [SerializeField]
  private WaypointEntry waypointEntryPrefab;
  [SerializeField]
  private GameObject waypointsNotSameOverlay;
  [SerializeField]
  private Transform waypointsPanel;
  private List<WaypointEntry> entries = new List<WaypointEntry>();
  private List<string> objectives;
  private NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<SavedUnit> targets;

  private void Awake()
  {
    this.addWaypointButton.onClick.AddListener(new UnityAction(this.AddWaypoint));
  }

  public void Setup(NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<SavedUnit> targets)
  {
    this.targets = targets;
    bool flag = targets.AllTheSame<VehicleWaypoint>(new NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<SavedUnit>.GetField<IReadOnlyList<VehicleWaypoint>>(this.GetWaypoints));
    this.waypointsNotSameOverlay.SetActive(!flag);
    this.Setup(flag ? (IReadOnlyList<VehicleWaypoint>) this.GetWaypoints(targets.Targets[0]) : (IReadOnlyList<VehicleWaypoint>) Array.Empty<VehicleWaypoint>());
  }

  private List<VehicleWaypoint> GetWaypoints(SavedUnit unit)
  {
    switch (unit)
    {
      case SavedVehicle savedVehicle:
        return savedVehicle.waypoints;
      case SavedShip savedShip:
        return savedShip.waypoints;
      default:
        throw new NotSupportedException($"{unit.GetType()} is not supported for GetWaypoints");
    }
  }

  public void Setup(IReadOnlyList<VehicleWaypoint> waypoints)
  {
    foreach (Component entry in this.entries)
      UnityEngine.Object.Destroy((UnityEngine.Object) entry.gameObject);
    this.entries.Clear();
    this.objectives = this.GetObjectives();
    for (int index = 0; index < waypoints.Count; ++index)
      this.entries.Add(UnityEngine.Object.Instantiate<WaypointEntry>(this.waypointEntryPrefab, this.waypointsPanel));
    this.SetupAllWithIndex();
  }

  public List<string> GetObjectives()
  {
    List<string> objectives = new List<string>();
    objectives.Add("Unit Spawn");
    foreach (Objective allObjective in MissionManager.Objectives.AllObjectives)
    {
      string savedCheckDestroyed = allObjective.GetNameSavedCheckDestroyed<Objective>();
      objectives.Add(savedCheckDestroyed);
    }
    return objectives;
  }

  private void SetupAllWithIndex()
  {
    List<VehicleWaypoint> waypoints = this.GetWaypoints(this.targets.Targets[0]);
    for (int index = 0; index < this.entries.Count; ++index)
    {
      int i = index;
      VehicleWaypoint waypoint = waypoints[i];
      MultiField<GlobalPosition> positionField = new MultiField<GlobalPosition>((Func<GlobalPosition>) (() => waypoint.position), (Action<GlobalPosition>) (v => this.targets.SetSameValue<GlobalPosition>((NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<SavedUnit>.GetFieldRef<GlobalPosition>) (x => ref this.GetWaypoints(x)[i].position), v)));
      MultiField<string> objectiveField = new MultiField<string>((Func<string>) (() => waypoint.objective), (Action<string>) (v => this.targets.SetSameValue<string>((NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<SavedUnit>.GetFieldRef<string>) (x => ref this.GetWaypoints(x)[i].objective), v)));
      this.entries[i].SetEntry(this, this.objectives, i, positionField, objectiveField);
    }
  }

  public void AddWaypoint()
  {
    foreach (SavedUnit target in (IEnumerable<SavedUnit>) this.targets.Targets)
      this.GetWaypoints(target).Add(new VehicleWaypoint()
      {
        position = SceneSingleton<CameraStateManager>.i.transform.position.ToGlobalPosition(),
        objective = "Unit Spawn"
      });
    this.entries.Add(UnityEngine.Object.Instantiate<WaypointEntry>(this.waypointEntryPrefab, this.waypointsPanel));
    this.GetComponentInParent<ILayerRebuildRoot>()?.Rebuild();
    this.SetupAllWithIndex();
  }

  public void RemoveWayPoint(WaypointEntry entry)
  {
    foreach (SavedUnit target in (IEnumerable<SavedUnit>) this.targets.Targets)
      this.GetWaypoints(target).RemoveAt(entry.Index);
    this.entries.RemoveAt(entry.Index);
    UnityEngine.Object.Destroy((UnityEngine.Object) entry.gameObject);
    this.GetComponentInParent<ILayerRebuildRoot>()?.RebuildEndOfFrame();
    this.SetupAllWithIndex();
  }
}

// Decompiled with JetBrains decompiler
// Type: NuclearOption.SavedMission.ObjectiveV2.Objectives.WaypointEditor
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.MissionEditorScripts;
using System.Collections.Generic;
using UnityEngine;

#nullable disable
namespace NuclearOption.SavedMission.ObjectiveV2.Objectives;

public class WaypointEditor : IObjectiveEditorUpdate
{
  private readonly UIPrefabs prefabs;
  private readonly ReachWaypointsObjective objective;
  private readonly List<Waypoint> waypoints;
  private readonly List<WaypointObjectiveHandle> waypointHandles = new List<WaypointObjectiveHandle>();
  private GameObject parent;
  private int listHash;

  public WaypointEditor(
    Canvas canvas,
    UIPrefabs prefabs,
    List<Waypoint> waypoints,
    ReachWaypointsObjective objective)
  {
    this.prefabs = prefabs;
    this.objective = objective;
    this.waypoints = waypoints;
    this.parent = new GameObject("Waypoint_Editor", new System.Type[1]
    {
      typeof (RectTransform)
    });
    this.parent.transform.SetParent(canvas.transform);
    this.parent.transform.SetAsFirstSibling();
  }

  public void Destroy()
  {
    UnityEngine.Object.Destroy((UnityEngine.Object) this.parent);
    this.waypointHandles.Clear();
    this.listHash = 0;
  }

  public void DeleteWaypoint(int index)
  {
    this.waypoints.RemoveAt(index);
    this.Update();
    if (!((UnityEngine.Object) this.objective.DataList != (UnityEngine.Object) null))
      return;
    this.objective.DataList.RefreshList();
  }

  public void Update()
  {
    int num = WaypointEditor.HashList(this.waypoints);
    if (num == this.listHash)
      return;
    this.listHash = num;
    while (this.waypointHandles.Count < this.waypoints.Count)
      this.waypointHandles.Add(UnityEngine.Object.Instantiate<WaypointObjectiveHandle>(this.prefabs.WaypointEditor, this.parent.transform));
    for (int index = 0; index < this.waypointHandles.Count; ++index)
    {
      WaypointObjectiveHandle waypointHandle = this.waypointHandles[index];
      if (index < this.waypoints.Count)
        waypointHandle.SetWaypoint(this, index, this.waypoints[index], (Objective) this.objective);
      else
        waypointHandle.Hide();
    }
  }

  private static int HashList(List<Waypoint> waypoints)
  {
    int num = 7;
    foreach (Waypoint waypoint in waypoints)
      num = num * 31 /*0x1F*/ + waypoint.GetHashCode();
    return num;
  }
}

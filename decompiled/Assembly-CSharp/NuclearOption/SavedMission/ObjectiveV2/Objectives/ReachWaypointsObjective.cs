// Decompiled with JetBrains decompiler
// Type: NuclearOption.SavedMission.ObjectiveV2.Objectives.ReachWaypointsObjective
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.MissionEditorScripts;
using NuclearOption.Networking;
using System;
using System.Collections.Generic;
using UnityEngine;

#nullable disable
namespace NuclearOption.SavedMission.ObjectiveV2.Objectives;

public class ReachWaypointsObjective : 
  CompleteOrderObjectiveWithPositions<Waypoint>,
  IObjectiveWithPosition
{
  public EmptyDataList DataList { get; private set; }

  public override bool NeedsFaction => true;

  protected override void WriteObjective(ReadWriteObjective writer)
  {
    writer.Enum<CompleteOrder>(ref this.completeOrder, CompleteOrder.InOrder);
    writer.DataList<Waypoint>(ref this.allItems);
  }

  protected override void DataReferenceDestroyed(ISaveableReference reference)
  {
  }

  protected override bool CheckComplete(Waypoint item)
  {
    List<Player> players = this.FactionHQ.GetPlayers(false);
    if (players.Count == 0)
      return false;
    foreach (Player player in players)
    {
      Aircraft aircraft = player.Aircraft;
      if (!((UnityEngine.Object) aircraft == (UnityEngine.Object) null) && FastMath.InRange(aircraft.GlobalPosition(), (GlobalPosition) (ValueWrapper<GlobalPosition>) item.GlobalPosition, (float) (ValueWrapper<float>) item.Range) && (double) Vector3.Dot(aircraft.rb.velocity, (GlobalPosition) (ValueWrapper<GlobalPosition>) item.GlobalPosition - aircraft.GlobalPosition()) < 0.0)
        return true;
    }
    return false;
  }

  protected override bool TryGetPosition(Waypoint item, out ObjectivePosition position)
  {
    position = item.ToObjectivePosition();
    return true;
  }

  public override void DrawData(DataDrawer drawer)
  {
    if (this.allItems == null)
      this.allItems = new List<Waypoint>();
    drawer.DrawEnum<CompleteOrder>("Complete Order", (int) this.completeOrder, (Action<int>) (v => this.completeOrder = (CompleteOrder) v));
    drawer.Space(10);
    this.DataList = drawer.DrawList<Waypoint>(300, this.allItems, new DrawInnerData<Waypoint>(ReachWaypointsObjective.DrawWaypoint), new Func<Waypoint>(ReachWaypointsObjective.CreateNewWaypoint));
  }

  private static Waypoint CreateNewWaypoint()
  {
    RaycastHit hitInfo;
    return new Waypoint((!Physics.Raycast(Camera.main.ScreenPointToRay(new Vector3((float) (Screen.width / 2), (float) (Screen.height / 2))), out hitInfo, 10000f, 64 /*0x40*/) ? Vector3.zero : hitInfo.point).ToGlobalPosition());
  }

  private static void DrawWaypoint(int _, Waypoint waypoint, DataDrawer drawer)
  {
    drawer.InstantiateWithParent<FloatDataField>(drawer.Prefabs.FloatFieldPrefab).Setup("Range", (IValueWrapper<float>) waypoint.Range);
    drawer.InstantiateWithParent<Vector3DataField>(drawer.Prefabs.VectorFieldPrefab).Setup("Position", (IValueWrapper<Vector3>) waypoint.GlobalPosition);
  }

  public override IObjectiveEditorUpdate CreateEditorUpdate(Canvas canvas, UIPrefabs prefabs)
  {
    return (IObjectiveEditorUpdate) new WaypointEditor(canvas, prefabs, this.allItems, this);
  }
}

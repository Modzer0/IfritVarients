// Decompiled with JetBrains decompiler
// Type: NuclearOption.MissionEditorScripts.WaypointSelectionDetails
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.SavedMission.ObjectiveV2;
using NuclearOption.SavedMission.ObjectiveV2.Objectives;
using UnityEngine;

#nullable disable
namespace NuclearOption.MissionEditorScripts;

public class WaypointSelectionDetails : SingleSelectionDetails
{
  public readonly WaypointObjectiveHandle Handle;

  public WaypointSelectionDetails(WaypointObjectiveHandle handle)
    : base((IEditorSelectable) handle, (IValueWrapper<GlobalPosition>) handle.Waypoint.GlobalPosition, (IValueWrapper<Quaternion>) null)
  {
    this.Handle = handle;
  }

  public override string DisplayName => this.Handle.GetDisplayName();

  public override bool IsDestroyed => (Object) this.Handle == (Object) null;

  public override bool AutoUnhover => false;

  public override void Focus()
  {
    Vector3 localPosition = this.PositionWrapper.Value.ToLocalPosition();
    float distance = (float) (ValueWrapper<float>) this.Handle.Waypoint.Range * 1.3f;
    SceneSingleton<CameraStateManager>.i.FocusPosition(localPosition, new Quaternion?(), distance);
  }

  public override bool Delete()
  {
    this.Handle.DeleteWaypoint();
    return true;
  }

  public override string ToString() => $"Waypoint({this.PositionWrapper.Value})";
}

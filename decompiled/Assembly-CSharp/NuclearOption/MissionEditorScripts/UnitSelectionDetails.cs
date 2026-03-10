// Decompiled with JetBrains decompiler
// Type: NuclearOption.MissionEditorScripts.UnitSelectionDetails
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.SavedMission;
using NuclearOption.SavedMission.ObjectiveV2;
using RuntimeHandle;
using System;
using UnityEngine;

#nullable disable
namespace NuclearOption.MissionEditorScripts;

public class UnitSelectionDetails : SingleSelectionDetails
{
  public readonly Unit Unit;
  public readonly SavedUnit SavedUnit;

  public override string DisplayName
  {
    get => this.SavedUnit == null ? this.Unit.unitName : this.SavedUnit.UniqueName;
  }

  public override bool IsDestroyed => (UnityEngine.Object) this.Unit == (UnityEngine.Object) null;

  public Faction Faction
  {
    get
    {
      return !((UnityEngine.Object) this.Unit.NetworkHQ != (UnityEngine.Object) null) ? (Faction) null : this.Unit.NetworkHQ.faction;
    }
  }

  public override bool TryGetFaction(out Faction faction)
  {
    faction = this.Faction;
    return true;
  }

  public UnitSelectionDetails(Unit unit)
    : base((IEditorSelectable) unit, unit.SavedUnit.PlacementType == PlacementType.Custom ? (IValueWrapper<GlobalPosition>) unit.SavedUnit.PositionWrapper : (IValueWrapper<GlobalPosition>) null, unit.SavedUnit.PlacementType == PlacementType.Custom ? (IValueWrapper<Quaternion>) unit.SavedUnit.RotationWrapper : (IValueWrapper<Quaternion>) null)
  {
    this.Unit = !((UnityEngine.Object) unit == (UnityEngine.Object) null) ? unit : throw new ArgumentNullException(nameof (Unit));
    this.SavedUnit = unit.SavedUnit;
  }

  public override HandleAxes AllowedPositionAxes
  {
    get
    {
      UnitDefinition definition = this.Unit.definition;
      return (double) definition.maxEditorHeight != (double) definition.minEditorHeight ? HandleAxes.XYZ : HandleAxes.XZ;
    }
  }

  public override void Focus() => SceneSingleton<CameraStateManager>.i.SetFollowingUnit(this.Unit);

  public override bool Delete()
  {
    SceneSingleton<MissionEditor>.i.RemoveUnit(this.Unit);
    return true;
  }

  public GlobalPosition ClampPosition(GlobalPosition position)
  {
    UnitDefinition definition = this.Unit.definition;
    double num = (double) this.GetTerrainPosition(position).y + (double) definition.spawnOffset.y;
    float min = (float) num + definition.minEditorHeight;
    float max = (float) num + definition.maxEditorHeight;
    position.y = Mathf.Clamp(position.y, min, max);
    return position;
  }

  private GlobalPosition GetTerrainPosition(GlobalPosition position)
  {
    GlobalPosition? nullable = UnitSelectionDetails.FindHighestTerrainPoint(position);
    if (!nullable.HasValue)
      nullable = new GlobalPosition?(position with
      {
        y = -100f
      });
    return nullable.Value;
  }

  private static GlobalPosition? FindHighestTerrainPoint(GlobalPosition position)
  {
    int num = Physics.RaycastNonAlloc(position.ToLocalPosition() + Vector3.up * 10000f, Vector3.down, UnitSelection.hitCache, 20000f, 64 /*0x40*/);
    if (num == 0)
      return new GlobalPosition?();
    Vector3? nullable = new Vector3?();
    for (int index = 0; index < num; ++index)
    {
      if (!((UnityEngine.Object) UnitSelection.hitCache[index].collider.GetComponentInParent<Unit>() != (UnityEngine.Object) null))
      {
        Vector3 point = UnitSelection.hitCache[index].point;
        if (!nullable.HasValue || (double) nullable.Value.y < (double) point.y)
          nullable = new Vector3?(point);
      }
    }
    return !nullable.HasValue ? new GlobalPosition?() : new GlobalPosition?(nullable.Value.ToGlobalPosition());
  }

  public override string ToString() => $"Selection({this.Unit.UniqueName})";
}

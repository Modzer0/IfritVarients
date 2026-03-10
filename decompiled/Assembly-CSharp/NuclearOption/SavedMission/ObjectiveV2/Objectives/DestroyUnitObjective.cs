// Decompiled with JetBrains decompiler
// Type: NuclearOption.SavedMission.ObjectiveV2.Objectives.DestroyUnitObjective
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.MissionEditorScripts;
using System;
using System.Collections.Generic;

#nullable disable
namespace NuclearOption.SavedMission.ObjectiveV2.Objectives;

public class DestroyUnitObjective : 
  CompleteOrderObjectiveWithPositions<SavedUnit>,
  IObjectiveWithPosition
{
  protected override void WriteObjective(ReadWriteObjective writer)
  {
    writer.Enum<CompleteOrder>(ref this.completeOrder, CompleteOrder.CompleteAll);
    writer.ReferenceList<SavedUnit>(ref this.allItems);
  }

  protected override void DataReferenceDestroyed(ISaveableReference reference)
  {
    if (!(reference is SavedUnit savedUnit))
      return;
    this.allItems.Remove(savedUnit);
  }

  protected override bool CheckComplete(SavedUnit item)
  {
    Unit unit;
    return UnitRegistry.customIDLookup.TryGetValue(item.UniqueName, out unit) && ((UnityEngine.Object) unit == (UnityEngine.Object) null || unit.disabled);
  }

  protected override bool TryGetPosition(SavedUnit item, out ObjectivePosition position)
  {
    GlobalPosition pos;
    int num = UnitRegistry.TryGetPosition(item, out pos) ? 1 : 0;
    position = new ObjectivePosition(pos, new float?());
    return num != 0;
  }

  public override void DrawData(DataDrawer drawer)
  {
    if (this.allItems == null)
      this.allItems = new List<SavedUnit>();
    drawer.DrawEnum<CompleteOrder>("Complete Order", (int) this.completeOrder, (Action<int>) (v => this.completeOrder = (CompleteOrder) v));
    drawer.Space(10);
    drawer.DrawList(300, this.allItems, true);
  }
}

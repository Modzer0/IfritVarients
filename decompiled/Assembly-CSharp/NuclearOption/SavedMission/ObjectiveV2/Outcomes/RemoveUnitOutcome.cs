// Decompiled with JetBrains decompiler
// Type: NuclearOption.SavedMission.ObjectiveV2.Outcomes.RemoveUnitOutcome
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.MissionEditorScripts;
using System.Collections.Generic;
using UnityEngine;

#nullable disable
namespace NuclearOption.SavedMission.ObjectiveV2.Outcomes;

internal class RemoveUnitOutcome : Outcome
{
  public List<SavedUnit> UnitsToRemove;

  protected override void WriteOutcome(ReadWriteObjective writer)
  {
    writer.ReferenceList<SavedUnit>(ref this.UnitsToRemove);
  }

  public override void ReferenceDestroyed(ISaveableReference reference)
  {
    if (!(reference is SavedUnit savedUnit))
      return;
    this.UnitsToRemove.Remove(savedUnit);
  }

  public override void Complete(Objective completedObjective)
  {
    foreach (SavedUnit savedUnit in this.UnitsToRemove)
    {
      if (savedUnit.HasSpawned)
        this.RemoveUnit(savedUnit);
    }
  }

  private void RemoveUnit(SavedUnit savedUnit)
  {
    Unit unit;
    if (!UnitRegistry.customIDLookup.TryGetValue(savedUnit.UniqueName, out unit) || !((Object) unit != (Object) null))
      return;
    Object.Destroy((Object) unit.gameObject);
  }

  public override void DrawData(DataDrawer drawer)
  {
    if (this.UnitsToRemove == null)
      this.UnitsToRemove = new List<SavedUnit>();
    drawer.DrawList(300, this.UnitsToRemove, true);
  }
}

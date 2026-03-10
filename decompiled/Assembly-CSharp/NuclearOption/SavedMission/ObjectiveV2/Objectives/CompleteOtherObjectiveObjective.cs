// Decompiled with JetBrains decompiler
// Type: NuclearOption.SavedMission.ObjectiveV2.Objectives.CompleteOtherObjectiveObjective
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.MissionEditorScripts;
using System;
using System.Collections.Generic;

#nullable disable
namespace NuclearOption.SavedMission.ObjectiveV2.Objectives;

public class CompleteOtherObjectiveObjective : CompleteOrderObjective<Objective>
{
  private static readonly List<string> completeOrderOptions = new List<string>()
  {
    CompleteOrder.CompleteAny.ToNicifyString<CompleteOrder>(),
    CompleteOrder.CompleteAll.ToNicifyString<CompleteOrder>()
  };

  protected override void WriteObjective(ReadWriteObjective writer)
  {
    writer.Enum<CompleteOrder>(ref this.completeOrder, CompleteOrder.CompleteAll);
    writer.ReferenceList<Objective>(ref this.allItems);
  }

  protected override void DataReferenceDestroyed(ISaveableReference reference)
  {
    if (!(reference is Objective objective))
      return;
    this.allItems.Remove(objective);
  }

  public override void ClientOnlyUpdate()
  {
  }

  protected override bool CheckComplete(Objective item) => item.Status == ObjectiveStatus.Complete;

  public override void DrawData(DataDrawer drawer)
  {
    if (this.allItems == null)
      this.allItems = new List<Objective>();
    drawer.DrawDropdown("Complete Order", CompleteOtherObjectiveObjective.completeOrderOptions, (int) this.completeOrder, (Action<int>) (v => this.completeOrder = (CompleteOrder) v));
    drawer.Space(10);
    drawer.DrawList(300, this.allItems);
  }
}

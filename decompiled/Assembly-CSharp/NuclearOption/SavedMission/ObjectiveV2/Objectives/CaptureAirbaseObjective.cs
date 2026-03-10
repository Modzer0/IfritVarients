// Decompiled with JetBrains decompiler
// Type: NuclearOption.SavedMission.ObjectiveV2.Objectives.CaptureAirbaseObjective
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.MissionEditorScripts;
using System;
using System.Collections.Generic;
using UnityEngine;

#nullable disable
namespace NuclearOption.SavedMission.ObjectiveV2.Objectives;

public class CaptureAirbaseObjective : 
  CompleteOrderObjectiveWithPositions<SavedAirbase>,
  IObjectiveWithPosition
{
  public override bool NeedsFaction => true;

  protected override bool AllCompleteAtOnce => true;

  protected override void WriteObjective(ReadWriteObjective writer)
  {
    writer.Enum<CompleteOrder>(ref this.completeOrder, CompleteOrder.CompleteAll);
    writer.ReferenceList<SavedAirbase>(ref this.allItems);
  }

  protected override void DataReferenceDestroyed(ISaveableReference reference)
  {
    if (!(reference is SavedAirbase savedAirbase))
      return;
    this.allItems.Remove(savedAirbase);
  }

  protected override bool CheckComplete(SavedAirbase item)
  {
    Airbase airbase;
    if (FactionRegistry.airbaseLookup.TryGetValue(item.UniqueName, out airbase))
      return (UnityEngine.Object) airbase.CurrentHQ == (UnityEngine.Object) this.FactionHQ;
    Debug.LogWarning((object) ("Could not find airbase with name " + item.UniqueName));
    return true;
  }

  protected override bool TryGetPosition(SavedAirbase item, out ObjectivePosition position)
  {
    position = new ObjectivePosition(item.Center, new float?());
    return true;
  }

  public override void DrawData(DataDrawer drawer)
  {
    if (this.allItems == null)
      this.allItems = new List<SavedAirbase>();
    drawer.DrawEnum<CompleteOrder>("Complete Order", (int) this.completeOrder, (Action<int>) (v => this.completeOrder = (CompleteOrder) v));
    drawer.Space(10);
    drawer.DrawList(300, this.allItems).SelectExistingDropdown.FilterSet.Apply((object) "NotAttached", new FilterSet.Filter(CaptureAirbaseObjective.FilterNotAttached));
  }

  private static bool FilterNotAttached(object obj)
  {
    Airbase airbase = ((SavedAirbase) obj).Airbase;
    return !((UnityEngine.Object) airbase != (UnityEngine.Object) null) || !airbase.AttachedAirbase;
  }
}

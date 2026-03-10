// Decompiled with JetBrains decompiler
// Type: NuclearOption.SavedMission.ObjectiveV2.Objectives.SpotUnitObjective
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.MissionEditorScripts;
using System;
using System.Collections.Generic;

#nullable disable
namespace NuclearOption.SavedMission.ObjectiveV2.Objectives;

public class SpotUnitObjective : CompleteOrderObjective<SavedUnit>
{
  private static readonly List<string> completeOrderOptions = new List<string>()
  {
    CompleteOrder.CompleteAny.ToNicifyString<CompleteOrder>(),
    CompleteOrder.CompleteAll.ToNicifyString<CompleteOrder>()
  };

  public override bool NeedsFaction => true;

  private CompleteOrderList<SavedUnit> completeList
  {
    get => (CompleteOrderList<SavedUnit>) this.completeList;
  }

  protected override void WriteObjective(ReadWriteObjective writer)
  {
    writer.Enum<CompleteOrder>(ref this.completeOrder, CompleteOrder.InOrder);
    writer.ReferenceList<SavedUnit>(ref this.allItems);
  }

  protected override void DataReferenceDestroyed(ISaveableReference reference)
  {
    if (!(reference is SavedUnit savedUnit))
      return;
    this.allItems.Remove(savedUnit);
  }

  public override void OnStart()
  {
    base.OnStart();
    this.FactionHQ.onDiscoverUnit += new Action<PersistentID>(this.FactionHQ_onDiscoverUnit);
    this.CheckExistingTracking();
  }

  public override void Cleanup()
  {
    this.FactionHQ.onDiscoverUnit -= new Action<PersistentID>(this.FactionHQ_onDiscoverUnit);
  }

  private void CheckExistingTracking()
  {
    foreach (CompleteOrderList<SavedUnit>.CheckItem toCheck in this.completeList.ToCheck.ToArray())
    {
      string uniqueName = toCheck.Item.UniqueName;
      Unit unit;
      if (UnitRegistry.customIDLookup.TryGetValue(uniqueName, out unit) && this.FactionHQ.trackingDatabase.TryGetValue(unit.persistentID, out TrackingInfo _))
        this.completeList.MarkCompleted(toCheck);
    }
  }

  protected override bool CheckComplete(SavedUnit toCheck) => false;

  private void FactionHQ_onDiscoverUnit(PersistentID id)
  {
    Unit unit;
    if (!UnitRegistry.TryGetUnit(new PersistentID?(id), out unit))
      return;
    foreach (CompleteOrderList<SavedUnit>.CheckItem toCheck in this.completeList.ToCheck)
    {
      if (toCheck.Item.UniqueName == unit.UniqueName)
      {
        this.completeList.MarkCompleted(toCheck);
        break;
      }
    }
  }

  public override void ClientOnlyUpdate()
  {
  }

  public override void DrawData(DataDrawer drawer)
  {
    if (this.allItems == null)
      this.allItems = new List<SavedUnit>();
    drawer.DrawDropdown("Complete Order", SpotUnitObjective.completeOrderOptions, (int) this.completeOrder, (Action<int>) (v => this.completeOrder = (CompleteOrder) v));
    drawer.Space(10);
    drawer.DrawList(300, this.allItems, true);
  }
}

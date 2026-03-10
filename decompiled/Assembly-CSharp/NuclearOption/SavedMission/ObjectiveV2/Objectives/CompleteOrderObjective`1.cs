// Decompiled with JetBrains decompiler
// Type: NuclearOption.SavedMission.ObjectiveV2.Objectives.CompleteOrderObjective`1
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;
using Unity.Profiling;

#nullable disable
namespace NuclearOption.SavedMission.ObjectiveV2.Objectives;

public abstract class CompleteOrderObjective<T> : Objective
{
  private static readonly ProfilerMarker updateAndCheckMarker = new ProfilerMarker("CompleteOrderObjectiveUpdateAndCheck");
  protected IObjectiveList<T> completeList;
  protected List<T> allItems;
  protected CompleteOrder completeOrder;
  private readonly CheckCallback<T> checkCallbackCached;

  public CompleteOrderObjective()
  {
    this.checkCallbackCached = new CheckCallback<T>(this.CheckComplete);
  }

  protected virtual bool AllCompleteAtOnce { get; }

  public override float CompletePercent => this.completeList.GetCompletePercent();

  public override void ReceiveNetworkData(List<int> data)
  {
    this.completeList.ReadNetworkData(data);
  }

  public override void OnStart()
  {
    this.completeList = this.completeOrder != CompleteOrder.CompleteAll || !this.AllCompleteAtOnce ? (IObjectiveList<T>) new CompleteOrderList<T>(this.allItems, this.completeOrder, new Action(this.UpdateNetworkData)) : (IObjectiveList<T>) new CompleteAllList<T>(this.allItems, new Action(this.UpdateNetworkData));
    this.UpdateNetworkData();
  }

  private void UpdateNetworkData()
  {
    using (CompleteOrderObjective<T>.updateAndCheckMarker.Auto())
    {
      if (!this.MissionManager.IsServer)
        return;
      this.MissionManager.UpdateNetworkData((Objective) this, this.completeList.UpdateNetworkList());
    }
  }

  public override bool UpdateAndCheck()
  {
    return this.completeList.UpdateAndCheck(this.checkCallbackCached);
  }

  protected abstract bool CheckComplete(T toCheck);
}

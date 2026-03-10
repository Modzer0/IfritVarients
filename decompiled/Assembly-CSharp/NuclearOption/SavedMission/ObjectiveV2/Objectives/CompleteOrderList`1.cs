// Decompiled with JetBrains decompiler
// Type: NuclearOption.SavedMission.ObjectiveV2.Objectives.CompleteOrderList`1
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;

#nullable disable
namespace NuclearOption.SavedMission.ObjectiveV2.Objectives;

public class CompleteOrderList<T> : IObjectiveList<T>
{
  private readonly List<T> allItems;
  private readonly CompleteOrder completeOrder;
  private readonly Action itemCompleted;
  public readonly List<CompleteOrderList<T>.CheckItem> ToCheck = new List<CompleteOrderList<T>.CheckItem>();
  private readonly List<int> networkList = new List<int>();

  public bool MarkedComplete { get; private set; }

  public CompleteOrderList(List<T> allItems, CompleteOrder completeOrder, Action itemCompleted)
  {
    this.allItems = allItems;
    this.completeOrder = completeOrder;
    this.itemCompleted = itemCompleted;
    this.MarkedComplete = allItems.Count == 0;
    if (allItems.Count <= 0)
      return;
    switch (this.completeOrder)
    {
      case CompleteOrder.InOrder:
        this.ToCheck.Add(new CompleteOrderList<T>.CheckItem(0, allItems[0]));
        break;
      default:
        for (int index = 0; index < this.allItems.Count; ++index)
          this.ToCheck.Add(new CompleteOrderList<T>.CheckItem(index, allItems[index]));
        break;
    }
  }

  public float GetCompletePercent()
  {
    switch (this.completeOrder)
    {
      case CompleteOrder.CompleteAll:
        return (float) (1.0 - (double) this.ToCheck.Count / (double) this.allItems.Count);
      case CompleteOrder.InOrder:
        return (float) this.ToCheck[0].Index / (float) this.allItems.Count;
      default:
        return 0.0f;
    }
  }

  public List<int> UpdateNetworkList()
  {
    this.WriteNetworkData(this.networkList);
    return this.networkList;
  }

  public void WriteNetworkData(List<int> data)
  {
    data.Clear();
    foreach (CompleteOrderList<T>.CheckItem checkItem in this.ToCheck)
      data.Add(checkItem.Index);
  }

  public void ReadNetworkData(List<int> data)
  {
    this.ToCheck.Clear();
    foreach (int index in data)
      this.ToCheck.Add(new CompleteOrderList<T>.CheckItem(index, this.allItems[index]));
  }

  public bool UpdateAndCheck(CheckCallback<T> check)
  {
    if (this.MarkedComplete)
      return true;
    foreach (CompleteOrderList<T>.CheckItem toCheck in this.ToCheck)
    {
      if (check(toCheck.Item))
      {
        this.MarkedComplete = this.MarkCompletedInternal(toCheck);
        return this.MarkedComplete;
      }
    }
    return false;
  }

  public void MarkCompleted(CompleteOrderList<T>.CheckItem toCheck)
  {
    this.MarkedComplete = this.MarkCompletedInternal(toCheck);
  }

  private bool MarkCompletedInternal(CompleteOrderList<T>.CheckItem toCheck)
  {
    if (this.completeOrder == CompleteOrder.CompleteAny)
      return true;
    if (this.completeOrder == CompleteOrder.CompleteAll)
    {
      this.ToCheck.Remove(toCheck);
      if (this.ToCheck.Count == 0)
        return true;
    }
    if (this.completeOrder == CompleteOrder.InOrder)
    {
      int index = toCheck.Index + 1;
      if (index >= this.allItems.Count)
        return true;
      this.ToCheck[0] = new CompleteOrderList<T>.CheckItem(index, this.allItems[index]);
    }
    Action itemCompleted = this.itemCompleted;
    if (itemCompleted != null)
      itemCompleted();
    return false;
  }

  public void ForeachNotComplete(Action<T> callback)
  {
    foreach (CompleteOrderList<T>.CheckItem checkItem in this.ToCheck)
      callback(checkItem.Item);
  }

  public readonly struct CheckItem(int index, T item) : IEquatable<CompleteOrderList<T>.CheckItem>
  {
    public readonly int Index = index;
    public readonly T Item = item;

    bool IEquatable<CompleteOrderList<T>.CheckItem>.Equals(CompleteOrderList<T>.CheckItem other)
    {
      return this.Index == other.Index;
    }
  }
}

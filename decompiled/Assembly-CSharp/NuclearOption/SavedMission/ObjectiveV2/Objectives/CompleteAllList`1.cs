// Decompiled with JetBrains decompiler
// Type: NuclearOption.SavedMission.ObjectiveV2.Objectives.CompleteAllList`1
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;

#nullable disable
namespace NuclearOption.SavedMission.ObjectiveV2.Objectives;

public class CompleteAllList<T> : IObjectiveList<T>
{
  private readonly List<T> allItems;
  private readonly Action itemCompleted;
  private readonly bool[] complete;
  private readonly List<int> networkList = new List<int>();

  public CompleteAllList(List<T> allItems, Action itemCompleted)
  {
    this.allItems = allItems;
    this.itemCompleted = itemCompleted;
    this.complete = new bool[allItems.Count];
  }

  public float GetCompletePercent()
  {
    int num = 0;
    for (int index = 0; index < this.complete.Length; ++index)
    {
      if (this.complete[index])
        ++num;
    }
    return (float) num / (float) this.allItems.Count;
  }

  public List<int> UpdateNetworkList()
  {
    this.WriteNetworkData(this.networkList);
    return this.networkList;
  }

  public void WriteNetworkData(List<int> data)
  {
    data.Clear();
    for (int index1 = 0; index1 < this.complete.Length; ++index1)
    {
      int num1 = index1 % 32 /*0x20*/;
      if (num1 == 0)
        data.Add(this.complete[index1] ? 1 : 0);
      else if (this.complete[index1])
      {
        int index2 = index1 / 32 /*0x20*/;
        uint num2 = (uint) data[index2] | (uint) (1 << num1);
        data[index2] = (int) num2;
      }
    }
  }

  public void ReadNetworkData(List<int> data)
  {
    for (int index1 = 0; index1 < data.Count; ++index1)
    {
      uint num1 = (uint) data[index1];
      for (int index2 = 0; index2 < 32 /*0x20*/; ++index2)
      {
        int index3 = index1 * 32 /*0x20*/ + index2;
        if (index3 >= this.complete.Length)
          return;
        uint num2 = num1 >> index2 & 1U;
        this.complete[index3] = num2 == 1U;
      }
    }
  }

  public bool UpdateAndCheck(CheckCallback<T> check)
  {
    if (this.allItems.Count == 0)
      return true;
    bool flag1 = true;
    for (int index = 0; index < this.allItems.Count; ++index)
    {
      T allItem = this.allItems[index];
      int num1 = this.complete[index] ? 1 : 0;
      bool flag2 = check(allItem);
      if (!flag2)
        flag1 = false;
      int num2 = flag2 ? 1 : 0;
      if (num1 != num2)
      {
        this.complete[index] = flag2;
        Action itemCompleted = this.itemCompleted;
        if (itemCompleted != null)
          itemCompleted();
      }
    }
    return flag1;
  }

  public void ForeachNotComplete(Action<T> callback)
  {
    for (int index = 0; index < this.complete.Length; ++index)
    {
      if (!this.complete[index])
        callback(this.allItems[index]);
    }
  }
}

// Decompiled with JetBrains decompiler
// Type: NuclearOption.MissionEditorScripts.EmptyDataList
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#nullable disable
namespace NuclearOption.MissionEditorScripts;

public class EmptyDataList : ListControllerWithButtonsBase<EmptyDataItemWrapper>
{
  public bool AllowSwapItems = true;
  private EmptyDataList.DrawInnerData drawContent;
  private IList dataList;

  protected override IList GetDataList() => this.dataList;

  public void UpdateList<T>(List<T> list, EmptyDataList.DrawInnerData<T> drawContent)
  {
    this.dataList = (IList) list;
    this.drawContent = (EmptyDataList.DrawInnerData) ((i, v, c) => drawContent(i, (T) v, c));
    this.RefreshList();
  }

  public override void RefreshList()
  {
    this.wrappers.Clear();
    for (int index = 0; index < this.dataList.Count; ++index)
      this.wrappers.Add(new EmptyDataItemWrapper(this.drawContent, this.dataList[index], index, this.dataList.Count, (Action<int>) null, this.deleteClicked, this.AllowSwapItems ? new MoveAction(((ListControllerWithButtonsBase<EmptyDataItemWrapper>) this).SwapItems) : (MoveAction) null));
    this.UpdateListFromWrapper();
  }

  public delegate void DrawInnerData(int index, object value, RectTransform parent);

  public delegate void DrawInnerData<T>(int index, T value, RectTransform parent);
}

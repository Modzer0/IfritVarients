// Decompiled with JetBrains decompiler
// Type: NuclearOption.MissionEditorScripts.EmptyDataItemWrapper
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;

#nullable disable
namespace NuclearOption.MissionEditorScripts;

public readonly struct EmptyDataItemWrapper(
  EmptyDataList.DrawInnerData drawContent,
  object value,
  int index,
  int count,
  Action<int> editClicked,
  Action<int> deleteClicked,
  MoveAction moveClicked)
{
  public readonly EmptyDataList.DrawInnerData DrawContent = drawContent;
  public readonly object Value = value;
  public readonly int Index = index;
  public readonly int Count = count;
  public readonly Action<int> EditClicked = editClicked;
  public readonly Action<int> DeleteClicked = deleteClicked;
  public readonly MoveAction MoveClicked = moveClicked;
}

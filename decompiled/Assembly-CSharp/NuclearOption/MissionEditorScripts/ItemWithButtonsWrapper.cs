// Decompiled with JetBrains decompiler
// Type: NuclearOption.MissionEditorScripts.ItemWithButtonsWrapper
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;

#nullable disable
namespace NuclearOption.MissionEditorScripts;

public struct ItemWithButtonsWrapper(
  bool enabled,
  string text,
  int index,
  int count,
  Action<int> editClicked,
  Action<int> deleteClicked,
  MoveAction moveClicked,
  string editText,
  string deleteText)
{
  public readonly bool Enabled = enabled;
  public readonly string Text = text;
  public readonly int Index = index;
  public readonly int Count = count;
  public readonly Action<int> EditClicked = editClicked;
  public readonly Action<int> DeleteClicked = deleteClicked;
  public readonly MoveAction MoveClicked = moveClicked;
  public readonly string EditButtonText = editText;
  public readonly string DeleteButtonText = deleteText;
}

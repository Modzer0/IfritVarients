// Decompiled with JetBrains decompiler
// Type: NuclearOption.Networking.Lobbies.ListSortButton
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

#nullable disable
namespace NuclearOption.Networking.Lobbies;

[Serializable]
public class ListSortButton
{
  [SerializeField]
  private Button button;
  [SerializeField]
  private TextMeshProUGUI label;
  private string defaultLabelText;
  private ListSortButton.SortState currentState = ListSortButton.SortState.None;

  public void Init(UnityAction onClick, Color color)
  {
    this.defaultLabelText = this.label.text;
    this.button.onClick.AddListener(onClick);
    this.UpdateLabel(ListSortButton.SortState.None, color, true);
  }

  public void UpdateLabel(ListSortButton.SortState newState, Color color, bool force = false)
  {
    if (!force && this.currentState == newState)
      return;
    this.currentState = newState;
    string str = "";
    switch (this.currentState)
    {
      case ListSortButton.SortState.ascending:
        str = GoogleIconFont.FontString("\uE5C7").AddSize(0.8f);
        break;
      case ListSortButton.SortState.descending:
        str = GoogleIconFont.FontString("\uE5C5").AddSize(0.8f);
        break;
    }
    this.label.text = this.defaultLabelText + str;
    this.label.color = color;
  }

  public enum SortState
  {
    ascending,
    descending,
    None,
  }
}

// Decompiled with JetBrains decompiler
// Type: NuclearOption.MissionEditorScripts.EmptyDataListItem
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using JamesFrowen.ScriptableVariables.UI;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

#nullable disable
namespace NuclearOption.MissionEditorScripts;

public class EmptyDataListItem : ListItem<EmptyDataItemWrapper>
{
  [Header("Right buttons")]
  [SerializeField]
  private Button removeButton;
  [Header("Left buttons")]
  [SerializeField]
  private int buttonLeftPad;
  [SerializeField]
  private Button moveUpButton;
  [SerializeField]
  private Button moveDownButton;
  [SerializeField]
  private GameObject buttonGroup;
  [SerializeField]
  private VerticalLayoutGroup layout;
  [SerializeField]
  private RectTransform content;
  private static readonly List<Transform> tmp = new List<Transform>();

  protected override void Awake()
  {
    this.removeButton.onClick.AddListener(new UnityAction(this.RemoveClicked));
    this.moveUpButton.onClick.AddListener(new UnityAction(this.MoveUpClicked));
    this.moveDownButton.onClick.AddListener(new UnityAction(this.MoveDownClicked));
  }

  private void RemoveClicked()
  {
    Action<int> deleteClicked = this.Value.DeleteClicked;
    if (deleteClicked == null)
      return;
    deleteClicked(this.Value.Index);
  }

  private void MoveUpClicked()
  {
    MoveAction moveClicked = this.Value.MoveClicked;
    if (moveClicked == null)
      return;
    moveClicked(this.Value.Index, this.Value.Index - 1);
  }

  private void MoveDownClicked()
  {
    MoveAction moveClicked = this.Value.MoveClicked;
    if (moveClicked == null)
      return;
    moveClicked(this.Value.Index, this.Value.Index + 1);
  }

  protected sealed override void SetValue(EmptyDataItemWrapper value)
  {
    this.removeButton.gameObject.SetActive(this.Value.DeleteClicked != null);
    this.buttonGroup.SetActive(value.MoveClicked != null);
    this.moveUpButton.gameObject.SetActive(this.Value.Index > 0);
    this.moveDownButton.gameObject.SetActive(this.Value.Index < this.Value.Count - 1);
    this.layout.padding.left = value.MoveClicked != null ? this.buttonLeftPad : 0;
    float x = ((RectTransform) this.transform).sizeDelta.x;
    this.content.sizeDelta = this.content.sizeDelta with
    {
      x = x - (float) this.layout.padding.left - (float) this.layout.padding.right
    };
    foreach (object obj in (Transform) this.content)
      EmptyDataListItem.tmp.Add((Transform) obj);
    foreach (Component component in EmptyDataListItem.tmp)
      UnityEngine.Object.Destroy((UnityEngine.Object) component.gameObject);
    EmptyDataListItem.tmp.Clear();
    value.DrawContent(value.Index, value.Value, this.content);
  }
}

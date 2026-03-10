// Decompiled with JetBrains decompiler
// Type: NuclearOption.MissionEditorScripts.ListItemWithButtons
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using JamesFrowen.ScriptableVariables.UI;
using System;
using TMPro;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

#nullable disable
namespace NuclearOption.MissionEditorScripts;

public class ListItemWithButtons : ListItem<ItemWithButtonsWrapper>
{
  private static readonly ProfilerMarker setValueMarker = new ProfilerMarker("ListItemWithButtons SetValue");
  [Header("Main Text")]
  [SerializeField]
  protected TextMeshProUGUI text;
  [Header("Right buttons")]
  [SerializeField]
  private Button editButton;
  [Space]
  [SerializeField]
  private Button removeButton;
  [SerializeField]
  protected TextMeshProUGUI remoteText;
  [Header("Left buttons")]
  [SerializeField]
  private Button moveUpButton;
  [SerializeField]
  private Button moveDownButton;
  [SerializeField]
  private GameObject buttonGroup;
  [Header("Layout")]
  [SerializeField]
  private int heightPerLine;
  [SerializeField]
  private int padding;
  [SerializeField]
  private int minHeight;

  private RectTransform RectTransform => (RectTransform) this.transform;

  protected override void Awake()
  {
    base.Awake();
    this.editButton.onClick.AddListener(new UnityAction(this.EditClicked));
    this.removeButton.onClick.AddListener(new UnityAction(this.RemoveClicked));
    this.moveUpButton.onClick.AddListener(new UnityAction(this.MoveUpClicked));
    this.moveDownButton.onClick.AddListener(new UnityAction(this.MoveDownClicked));
  }

  private void EditClicked()
  {
    Action<int> editClicked = this.Value.EditClicked;
    if (editClicked == null)
      return;
    editClicked(this.Value.Index);
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

  protected sealed override void SetValue(ItemWithButtonsWrapper value)
  {
    using (ListItemWithButtons.setValueMarker.Auto())
    {
      if (!value.Enabled)
      {
        this.SetActive(false);
      }
      else
      {
        if ((UnityEngine.Object) this.editButton.gameObject != (UnityEngine.Object) this.gameObject)
          this.editButton.gameObject.SetActive(value.EditClicked != null);
        this.removeButton.gameObject.SetActive(value.DeleteClicked != null);
        this.buttonGroup.SetActive(value.MoveClicked != null);
        this.moveUpButton.gameObject.SetActive(value.Index > 0);
        this.moveDownButton.gameObject.SetActive(value.Index < value.Count - 1);
        if (!string.IsNullOrEmpty(value.DeleteButtonText))
          this.remoteText.text = value.DeleteButtonText;
        this.RectTransform.sizeDelta = this.RectTransform.sizeDelta with
        {
          y = this.CalculateHeight(value)
        };
        if (!((UnityEngine.Object) this.text != (UnityEngine.Object) null))
          return;
        this.text.text = value.Text;
      }
    }
  }

  public override float CalculateHeight(ItemWithButtonsWrapper value)
  {
    return (float) Mathf.Max(this.padding + value.Text.CountLines() * this.heightPerLine, this.minHeight);
  }
}

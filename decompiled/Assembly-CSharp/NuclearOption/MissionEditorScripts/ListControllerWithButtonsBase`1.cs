// Decompiled with JetBrains decompiler
// Type: NuclearOption.MissionEditorScripts.ListControllerWithButtonsBase`1
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using JamesFrowen.ScriptableVariables.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

#nullable disable
namespace NuclearOption.MissionEditorScripts;

public abstract class ListControllerWithButtonsBase<TWrapper> : ListController<TWrapper>
{
  [Header("Extra layout")]
  [Tooltip("Use _layoutRoot if _parent is not the root layout for content")]
  [SerializeField]
  protected RectTransform _layoutRoot;
  [Header("Create New")]
  [SerializeField]
  private Button createNewButton;
  [SerializeField]
  private ListControllerWithButtonsBase<TWrapper>.ButtonInList createPlacement;
  [Header("Add Existing")]
  [SerializeField]
  private Button addExistingButton;
  [SerializeField]
  private ListControllerWithButtonsBase<TWrapper>.ButtonInList addPlacement;
  [Header("Title")]
  [SerializeField]
  public TextMeshProUGUI TitleText;
  [SerializeField]
  private RectTransform listParent;
  protected Action newClicked;
  protected Action addClicked;
  protected Action<int> editClicked;
  protected Action<int> deleteClicked;
  protected List<TWrapper> wrappers = new List<TWrapper>();

  protected abstract IList GetDataList();

  public void SetHeight(int listPanelHeight)
  {
    this.listParent.sizeDelta = this.listParent.sizeDelta with
    {
      y = (float) listPanelHeight
    };
  }

  protected override void Awake()
  {
    base.Awake();
    if ((UnityEngine.Object) this._layoutRoot == (UnityEngine.Object) null)
      this._layoutRoot = this._parent;
    if ((UnityEngine.Object) this.createNewButton != (UnityEngine.Object) null)
    {
      this.createNewButton.onClick.AddListener(new UnityAction(this.CallNewClicked));
      this.createNewButton.gameObject.SetActive(false);
    }
    if (!((UnityEngine.Object) this.addExistingButton != (UnityEngine.Object) null))
      return;
    this.addExistingButton.onClick.AddListener(new UnityAction(this.CallAddClicked));
    this.addExistingButton.gameObject.SetActive(false);
  }

  private void CallNewClicked()
  {
    Action newClicked = this.newClicked;
    if (newClicked == null)
      return;
    newClicked();
  }

  private void CallAddClicked()
  {
    Action addClicked = this.addClicked;
    if (addClicked == null)
      return;
    addClicked();
  }

  public virtual void Setup(
    Action newClicked,
    Action addClicked,
    Action<int> editClicked,
    Action<int> deleteClicked)
  {
    this.newClicked = newClicked;
    this.addClicked = addClicked;
    this.editClicked = editClicked;
    this.deleteClicked = deleteClicked;
    if ((UnityEngine.Object) this.createNewButton != (UnityEngine.Object) null)
      this.createNewButton.gameObject.SetActive(newClicked != null);
    if (!((UnityEngine.Object) this.addExistingButton != (UnityEngine.Object) null))
      return;
    this.addExistingButton.gameObject.SetActive(addClicked != null);
  }

  protected void UpdateListFromWrapper()
  {
    this.UpdateList((IReadOnlyList<TWrapper>) this.wrappers);
    this.RebuildLayout();
  }

  protected override void RebuildLayout()
  {
    if ((UnityEngine.Object) this.createNewButton != (UnityEngine.Object) null)
      ListControllerWithButtonsBase<TWrapper>.MoveButton(this.createNewButton, this.createPlacement);
    if ((UnityEngine.Object) this.addExistingButton != (UnityEngine.Object) null)
      ListControllerWithButtonsBase<TWrapper>.MoveButton(this.addExistingButton, this.addPlacement);
    FixLayout.ForceRebuildAtEndOfFrame(this._layoutRoot);
  }

  private static void MoveButton(
    Button button,
    ListControllerWithButtonsBase<TWrapper>.ButtonInList placement)
  {
    if (placement != ListControllerWithButtonsBase<TWrapper>.ButtonInList.AtStart)
    {
      if (placement != ListControllerWithButtonsBase<TWrapper>.ButtonInList.AtEnd)
        return;
      button.transform.SetAsLastSibling();
    }
    else
      button.transform.SetAsFirstSibling();
  }

  protected virtual void SwapItems(int from, int to)
  {
    IList dataList;
    IList list1 = dataList = this.GetDataList();
    int index1 = to;
    IList list2 = dataList;
    int index2 = from;
    object obj1 = dataList[from];
    object obj2 = dataList[to];
    object obj3;
    list1[index1] = obj3 = obj1;
    list2[index2] = obj3 = obj2;
    this.RefreshList();
  }

  public abstract void RefreshList();

  private enum ButtonInList
  {
    None,
    AtStart,
    AtEnd,
  }
}

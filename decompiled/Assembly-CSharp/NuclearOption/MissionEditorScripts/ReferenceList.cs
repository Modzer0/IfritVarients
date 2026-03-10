// Decompiled with JetBrains decompiler
// Type: NuclearOption.MissionEditorScripts.ReferenceList
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.SavedMission.ObjectiveV2;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#nullable disable
namespace NuclearOption.MissionEditorScripts;

public class ReferenceList : ListControllerWithButtonsBase<ItemWithButtonsWrapper>
{
  [Header("Select Dropdown")]
  public ReferencePopup SelectExistingDropdown;
  private ReferenceToString toDropdownString;
  private ReferenceToString toListString;
  private GetAllOptions getAllOptions;
  public readonly FilterSet FilterSet = new FilterSet();
  public string EditButtonText;
  public string DeleteButtonText;
  private bool dropdownOpen;
  private ReferenceList.ListWrapper dataList;

  protected override IList GetDataList() => this.dataList?.Inner;

  public event Action<ISaveableReference> ItemAdded;

  public event Action<ISaveableReference> ItemRemoved;

  protected override void Awake()
  {
    base.Awake();
    this.FilterSet.OnFilterChanged += new Action(((ListControllerWithButtonsBase<ItemWithButtonsWrapper>) this).RefreshList);
    this.SelectExistingDropdown.Hide();
    this.SelectExistingDropdown.FilterSet.Apply((object) "InRefList", (FilterSet.Filter) (obj => !this.dataList.Contains((ISaveableReference) obj)));
  }

  public void SetupList<T>(List<T> data, Func<T, string> toListString) where T : ISaveableReference
  {
    this.SetupListInternal((IList) data, (ReferenceToString) null, (ReferenceToString) (o => toListString((T) o)), (GetAllOptions) null, ReferenceList.ButtonsEvents.DontAdd);
  }

  public void SetupList<T>(
    List<T> data,
    Func<T, string> toListString,
    ReferenceList.ButtonsEvents addButtonEvents)
    where T : ISaveableReference
  {
    this.SetupList<T>(data, (Func<T, string>) null, toListString, (Func<IEnumerable<T>>) null, addButtonEvents);
  }

  public void SetupList<T>(
    List<T> data,
    Func<T, string> toDropdownString,
    Func<T, string> toListString,
    Func<IEnumerable<T>> getAllOptions,
    ReferenceList.ButtonsEvents addButtonEvents)
    where T : ISaveableReference
  {
    this.SetupListInternal((IList) data, (ReferenceToString) (o => toDropdownString((T) o)), (ReferenceToString) (o => toListString((T) o)), (GetAllOptions) (() => getAllOptions().Cast<ISaveableReference>()), addButtonEvents);
  }

  private void SetupListInternal(
    IList data,
    ReferenceToString toDropdownString,
    ReferenceToString toListString,
    GetAllOptions getAllOptions,
    ReferenceList.ButtonsEvents addButtonEvents)
  {
    this.dataList = new ReferenceList.ListWrapper(data);
    this.dataList.ItemAdded += (Action<ISaveableReference>) (i =>
    {
      Action<ISaveableReference> itemAdded = this.ItemAdded;
      if (itemAdded == null)
        return;
      itemAdded(i);
    });
    this.dataList.ItemRemoved += (Action<ISaveableReference>) (i =>
    {
      Action<ISaveableReference> itemRemoved = this.ItemRemoved;
      if (itemRemoved == null)
        return;
      itemRemoved(i);
    });
    this.toListString = toListString;
    this.toDropdownString = toDropdownString;
    this.getAllOptions = getAllOptions;
    switch (addButtonEvents)
    {
      case ReferenceList.ButtonsEvents.OverrideNullOnly:
        this.Setup(this.newClicked ?? (Action) null, this.addClicked ?? new Action(this.AddExisting), this.editClicked ?? new Action<int>(this.EditItem), this.deleteClicked ?? new Action<int>(this.RemoveItem));
        break;
      case ReferenceList.ButtonsEvents.Override:
        this.Setup((Action) null, new Action(this.AddExisting), new Action<int>(this.EditItem), new Action<int>(this.RemoveItem));
        break;
    }
    this.RefreshList();
  }

  public void Clear()
  {
    this.dataList = (ReferenceList.ListWrapper) null;
    this.toListString = (ReferenceToString) null;
    this.toDropdownString = (ReferenceToString) null;
    this.getAllOptions = (GetAllOptions) null;
    this.Setup((Action) null, (Action) null, (Action<int>) null, (Action<int>) null);
    this.UpdateListEmpty();
  }

  public override void RefreshList()
  {
    if (this.dataList == null)
      return;
    this.UpdateList(this.dataList, this.toListString);
  }

  public void UpdateList<T>(List<T> list, ReferenceToString toString) where T : ISaveableReference
  {
    this.UpdateList(new ReferenceList.ListWrapper((IList) list), toString);
  }

  public void UpdateList(ReferenceList.ListWrapper listWrapper, ReferenceToString toString)
  {
    this.wrappers.Clear();
    bool flag = this.FilterSet.Count == 0 || listWrapper.Where<ISaveableReference>((Func<ISaveableReference, bool>) (item => item.CanBeSorted)).All<ISaveableReference>((Func<ISaveableReference, bool>) (item => this.FilterSet.FilterItem((object) item)));
    int count = listWrapper.Count;
    for (int index = 0; index < count; ++index)
    {
      ISaveableReference saveableReference = listWrapper[index];
      bool enabled = this.FilterSet.FilterItem((object) saveableReference);
      string text = (string) null;
      if (enabled)
        text = toString(saveableReference);
      Action<int> deleteClicked = saveableReference.CanBeReference ? this.deleteClicked : (Action<int>) null;
      MoveAction moveClicked = !flag || !saveableReference.CanBeSorted ? (MoveAction) null : new MoveAction(((ListControllerWithButtonsBase<ItemWithButtonsWrapper>) this).SwapItems);
      this.wrappers.Add(new ItemWithButtonsWrapper(enabled, text, index, count, this.editClicked, deleteClicked, moveClicked, this.EditButtonText, this.DeleteButtonText));
    }
    this.UpdateListFromWrapper();
  }

  protected override void SwapItems(int from, int to)
  {
    if (!this.dataList[from].CanBeSorted || !this.dataList[to].CanBeSorted)
      return;
    base.SwapItems(from, to);
  }

  public void UpdateListEmpty()
  {
    this.wrappers.Clear();
    this.UpdateListFromWrapper();
  }

  private void AddExisting()
  {
    if (this.dropdownOpen)
      Debug.LogWarning((object) "Dropdown already open");
    else
      this.PickOption((ISaveableReference) null, (Action<ISaveableReference>) (item => this.dataList.Add(item)));
  }

  private void EditItem(int i)
  {
    if (this.dropdownOpen)
      Debug.LogWarning((object) "Dropdown already open");
    else
      this.PickOption(this.dataList[i], (Action<ISaveableReference>) (item => this.dataList[i] = item));
  }

  private void RemoveItem(int i)
  {
    this.dataList.RemoveAt(i);
    this.RefreshList();
    SceneSingleton<MissionEditor>.i.CheckAutoSave();
  }

  private void PickOption(ISaveableReference startingOption, Action<ISaveableReference> onPick)
  {
    this.dropdownOpen = true;
    this.SelectExistingDropdown.ShowPickOption(startingOption, false, this.getAllOptions, this.toDropdownString, (PickOrCancel) ((pick, obj) =>
    {
      this.dropdownOpen = false;
      if (!pick)
        return;
      onPick(obj);
      SceneSingleton<MissionEditor>.i.CheckAutoSave();
      this.RefreshList();
    }));
  }

  public enum ButtonsEvents
  {
    OverrideNullOnly,
    DontAdd,
    Override,
  }

  public class ListWrapper : IEnumerable<ISaveableReference>, IEnumerable
  {
    public readonly IList Inner;

    public event Action<ISaveableReference> ItemAdded;

    public event Action<ISaveableReference> ItemRemoved;

    public ListWrapper(IList inner) => this.Inner = inner;

    public ISaveableReference this[int index]
    {
      get => (ISaveableReference) this.Inner[index];
      set
      {
        ISaveableReference saveableReference = this.Inner[index] as ISaveableReference;
        this.Inner[index] = (object) value;
        if (saveableReference != null)
        {
          Action<ISaveableReference> itemRemoved = this.ItemRemoved;
          if (itemRemoved != null)
            itemRemoved(saveableReference);
        }
        if (value == null)
          return;
        Action<ISaveableReference> itemAdded = this.ItemAdded;
        if (itemAdded == null)
          return;
        itemAdded(value);
      }
    }

    public int Count => this.Inner.Count;

    public bool Contains(ISaveableReference obj)
    {
      for (int index = 0; index < this.Inner.Count; ++index)
      {
        if (((ISaveableReference) this.Inner[index]).UniqueName == obj.UniqueName)
          return true;
      }
      return false;
    }

    public void Add(ISaveableReference item)
    {
      this.Inner.Add((object) item);
      if (item == null)
        return;
      Action<ISaveableReference> itemAdded = this.ItemAdded;
      if (itemAdded == null)
        return;
      itemAdded(item);
    }

    public void RemoveAt(int i)
    {
      ISaveableReference saveableReference = this.Inner[i] as ISaveableReference;
      this.Inner.RemoveAt(i);
      if (saveableReference == null)
        return;
      Action<ISaveableReference> itemRemoved = this.ItemRemoved;
      if (itemRemoved == null)
        return;
      itemRemoved(saveableReference);
    }

    IEnumerator IEnumerable.GetEnumerator() => this.Inner.GetEnumerator();

    public IEnumerator<ISaveableReference> GetEnumerator()
    {
      return this.Inner.Cast<ISaveableReference>().GetEnumerator();
    }
  }
}

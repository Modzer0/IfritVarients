// Decompiled with JetBrains decompiler
// Type: NuclearOption.MissionEditorScripts.DataDrawer
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.SavedMission.ObjectiveV2;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

#nullable disable
namespace NuclearOption.MissionEditorScripts;

public class DataDrawer
{
  private readonly RectTransform mainParent;
  private readonly UIPrefabs prefabs;
  private bool addedGroupBox;
  public bool Success;
  private readonly List<IValueWrapper> TrackedChanges = new List<IValueWrapper>();
  private Stack<RectTransform> parentStack = new Stack<RectTransform>();
  public float? Width;

  public event Action PanelAfterEdit;

  public event Action TrackedValueChanged;

  public RectTransform Parent => this.parentStack.Peek();

  public UIPrefabs Prefabs => this.prefabs;

  public DataDrawer(RectTransform parent, UIPrefabs prefabs)
  {
    this.parentStack.Push(parent);
    this.mainParent = parent;
    this.prefabs = prefabs;
  }

  public void Cleanup()
  {
    this.PanelAfterEdit = (Action) null;
    this.TrackedValueChanged = (Action) null;
    foreach (IValueWrapper trackedChange in this.TrackedChanges)
      trackedChange.UnregisterOnChange((object) this);
    this.TrackedChanges.Clear();
  }

  public void Reset()
  {
    this.Cleanup();
    while ((UnityEngine.Object) this.parentStack.Peek() != (UnityEngine.Object) this.mainParent)
      UnityEngine.Object.Destroy((UnityEngine.Object) this.parentStack.Pop().gameObject);
    this.Success = false;
    this.addedGroupBox = false;
  }

  public void InvokeAfterEdit()
  {
    Action panelAfterEdit = this.PanelAfterEdit;
    if (panelAfterEdit == null)
      return;
    panelAfterEdit();
  }

  public void CheckGroupBox()
  {
    if (this.addedGroupBox)
      return;
    this.addedGroupBox = true;
    this.Success = true;
    this.parentStack.Push((RectTransform) UnityEngine.Object.Instantiate<RectTransform>(this.prefabs.GroupBoxPrefab, (Transform) this.Parent).transform);
    if (!this.Width.HasValue)
      return;
    float? width = this.Width;
    float groupBoxPadding = this.prefabs.GroupBoxPadding;
    this.Width = width.HasValue ? new float?(width.GetValueOrDefault() - groupBoxPadding) : new float?();
  }

  private void GroupInternal<T>(T prefab, Action<T> inner) where T : Component
  {
    this.CheckGroupBox();
    T obj = UnityEngine.Object.Instantiate<T>(prefab, (Transform) this.Parent);
    this.parentStack.Push((RectTransform) obj.transform);
    inner(obj);
    this.parentStack.Pop();
  }

  public void HorizontalGroup(Action<HorizontalLayoutGroup> inner)
  {
    this.GroupInternal<HorizontalLayoutGroup>(this.prefabs.HorizontalGroupPrefab, inner);
  }

  public void VerticalGroup(Action<VerticalLayoutGroup> inner)
  {
    this.GroupInternal<VerticalLayoutGroup>(this.prefabs.VerticalGroupPrefab, inner);
  }

  public void Nothing() => this.Success = true;

  public void Space(int height)
  {
    RectTransform transform = (RectTransform) new GameObject("spacer", new System.Type[1]
    {
      typeof (RectTransform)
    }).transform;
    transform.SetParent((Transform) this.Parent);
    transform.sizeDelta = new Vector2(10f, (float) height);
  }

  public ReferenceList DrawList<T>(
    int height,
    string listTitle,
    string dropdownTitle,
    List<T> list,
    Func<T, string> toDropdownString,
    Func<T, string> toListString,
    Func<IEnumerable<T>> getAllOptions)
    where T : ISaveableReference
  {
    ReferenceList referenceList = this.InstantiateWithParent<ReferenceList>(this.prefabs.ReferenceListPrefab);
    referenceList.TitleText.text = listTitle;
    referenceList.SelectExistingDropdown.SetTitle(dropdownTitle);
    referenceList.SetHeight(height);
    referenceList.SetupList<T>(list, toDropdownString, toListString, getAllOptions, ReferenceList.ButtonsEvents.OverrideNullOnly);
    this.Success = true;
    return referenceList;
  }

  public EmptyDataList DrawList<T>(int height, List<T> list, DrawInnerData<T> drawContent) where T : class, new()
  {
    return this.DrawList<T>(height, list, drawContent, (Func<T>) (() => new T()));
  }

  public EmptyDataList DrawList<T>(
    int height,
    List<T> list,
    DrawInnerData<T> drawContent,
    Func<T> createNew,
    Action<T> onDelete = null)
  {
    EmptyDataList controller = this.InstantiateWithParent<EmptyDataList>(this.prefabs.DataListPrefab);
    controller.SetHeight(height);
    controller.Setup(new Action(CreateNew), (Action) null, (Action<int>) null, new Action<int>(Delete));
    Refresh();
    return controller;

    void CreateNew()
    {
      list.Add(createNew());
      Refresh();
    }

    void Delete(int index)
    {
      Action<T> action = onDelete;
      if (action != null)
        action(list[index]);
      list.RemoveAt(index);
      Refresh();
    }

    void Refresh() => controller.UpdateList<T>(list, new EmptyDataList.DrawInnerData<T>(DrawItem));

    void DrawItem(int index, T value, RectTransform parent)
    {
      this.parentStack.Push(parent);
      drawContent(index, value, this);
      this.parentStack.Pop();
    }
  }

  public DropdownDataField DrawEnum<T>(string label, int value, Action<int> setValue) where T : struct, Enum
  {
    DropdownDataField dropdownDataField = this.DrawDropdown(label, EnumNames<T>.GetNames(), value, setValue);
    dropdownDataField.LabelWidth(160 /*0xA0*/);
    return dropdownDataField;
  }

  public DropdownDataField DrawDropdown(
    string label,
    List<string> options,
    string value,
    Action<string> setValue)
  {
    DropdownDataField dropdownDataField = this.InstantiateWithParent<DropdownDataField>(this.prefabs.Dropdown);
    dropdownDataField.Setup(label, options, value, setValue);
    return dropdownDataField;
  }

  public DropdownDataField DrawDropdown(
    string label,
    List<string> options,
    int value,
    Action<int> setValue)
  {
    DropdownDataField dropdownDataField = this.InstantiateWithParent<DropdownDataField>(this.prefabs.Dropdown);
    dropdownDataField.Setup(label, options, value, setValue);
    return dropdownDataField;
  }

  public (OverrideDataField, TDataField) DrawOverride<T, TDataField>(
    string label,
    ValueWrapperOverride<T> wrapper,
    TDataField dataFieldPrefab)
    where T : IEquatable<T>
    where TDataField : DataField, IDataField<T>
  {
    OverrideDataField overrideDataField = this.InstantiateWithParent<OverrideDataField>(this.Prefabs.OverrideField);
    TDataField dataField = UnityEngine.Object.Instantiate<TDataField>(dataFieldPrefab, (Transform) overrideDataField.innerHolder);
    dataField.Setup(label, (IValueWrapper<T>) wrapper);
    overrideDataField.Setup<T>((ValueWrapper<Override<T>>) wrapper, (OverrideDataField.InnerField) (DataField) dataField);
    return (overrideDataField, dataField);
  }

  public T InstantiateWithParent<T>(T prefab) where T : Component
  {
    this.CheckGroupBox();
    T comp = UnityEngine.Object.Instantiate<T>(prefab, (Transform) this.Parent);
    if (this.Width.HasValue)
      comp.SetRectWidth(this.Width.Value);
    return comp;
  }

  public TextMeshProUGUI DrawHeader(string text, int spaceBefore = 16 /*0x10*/, int spaceAfter = 10)
  {
    this.Space(spaceBefore);
    TextMeshProUGUI textMeshProUgui = this.InstantiateWithParent<TextMeshProUGUI>(this.Prefabs.TextPrefab);
    textMeshProUgui.text = text;
    textMeshProUgui.alignment = TextAlignmentOptions.Left;
    textMeshProUgui.fontWeight = FontWeight.Bold;
    textMeshProUgui.fontSize *= 1.3f;
    this.Space(spaceAfter);
    return textMeshProUgui;
  }

  public TextMeshProUGUI DrawLabel(string text)
  {
    TextMeshProUGUI textMeshProUgui = this.InstantiateWithParent<TextMeshProUGUI>(this.Prefabs.TextPrefab);
    textMeshProUgui.text = text;
    textMeshProUgui.alignment = TextAlignmentOptions.Left;
    return textMeshProUgui;
  }

  public TextMeshProUGUI DrawLabelWarning(string text)
  {
    TextMeshProUGUI textMeshProUgui = this.InstantiateWithParent<TextMeshProUGUI>(this.Prefabs.TextPrefab);
    textMeshProUgui.text = text;
    textMeshProUgui.alignment = TextAlignmentOptions.Left;
    textMeshProUgui.color = Color.yellow;
    return textMeshProUgui;
  }

  public TextMeshProUGUI DrawLabelError(string text)
  {
    TextMeshProUGUI textMeshProUgui = this.InstantiateWithParent<TextMeshProUGUI>(this.Prefabs.TextPrefab);
    textMeshProUgui.text = text;
    textMeshProUgui.alignment = TextAlignmentOptions.Left;
    textMeshProUgui.color = Color.red;
    return textMeshProUgui;
  }

  public void TrackChanges(IValueWrapper valueWrapper)
  {
    this.TrackedChanges.Add(valueWrapper);
    valueWrapper.RegisterOnChange((object) this, (Action) (() =>
    {
      Action trackedValueChanged = this.TrackedValueChanged;
      if (trackedValueChanged == null)
        return;
      trackedValueChanged();
    }));
  }
}

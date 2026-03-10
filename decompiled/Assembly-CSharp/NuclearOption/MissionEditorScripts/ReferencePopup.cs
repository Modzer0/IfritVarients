// Decompiled with JetBrains decompiler
// Type: NuclearOption.MissionEditorScripts.ReferencePopup
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.SavedMission;
using NuclearOption.SavedMission.ObjectiveV2;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

#nullable disable
namespace NuclearOption.MissionEditorScripts;

public class ReferencePopup : MonoBehaviour
{
  [SerializeField]
  private GameObject holder;
  [SerializeField]
  private TextMeshProUGUI titleText;
  [SerializeField]
  private TextMeshProUGUI noOptionsText;
  [SerializeField]
  private Button cancelDropdownButton;
  [SerializeField]
  private TMP_Dropdown allUnitsDropdown;
  [SerializeField]
  public GameObject filterHolder;
  [SerializeField]
  private DropdownDataField factionFilter;
  [SerializeField]
  private DropdownDataField unitTypeFilter;
  [SerializeField]
  private DropdownDataField placementTypeFilter;
  public readonly FilterSet FilterSet = new FilterSet();
  private ISaveableReference startingOption;
  private GetAllOptions getAllOptions;
  private ReferenceToString toDropdownString;
  private PickOrCancel pickOrCancelCallback;
  private bool allowNone;
  private bool filtersSetup;
  private int optionOffset;
  private bool placementFilterIncludeAttached;
  private bool hideUnitTypeFilter;
  private bool hideFactionFilter;
  private bool hidePlacementTypeFilter;
  private readonly List<string> optionNames = new List<string>();
  private readonly List<ISaveableReference> optionValues = new List<ISaveableReference>();

  public GameObject Holder => this.holder;

  private void CheckFilterSetup()
  {
    if (this.filtersSetup)
      return;
    FilterSet.SetupFilterUnitType(this.unitTypeFilter, (string) null, this.FilterSet);
    FilterSet.SetupFilterFaction(this.factionFilter, (string) null, this.FilterSet);
    FilterSet.SetupFilterPlacement(this.placementTypeFilter, (string) null, this.FilterSet, this.placementFilterIncludeAttached);
    this.filtersSetup = true;
  }

  public void HideFactionFilter(bool hide = true)
  {
    this.HideFilter(hide, ref this.hideFactionFilter, this.factionFilter);
  }

  public void HideUnitTypeFilter(bool hide = true)
  {
    this.HideFilter(hide, ref this.hideUnitTypeFilter, this.unitTypeFilter);
  }

  public void HidePlacementTypeFilter(bool hide = true)
  {
    this.HideFilter(hide, ref this.hidePlacementTypeFilter, this.placementTypeFilter);
  }

  private void HideFilter(bool hide, ref bool hideField, DropdownDataField filter)
  {
    hideField = hide;
    if (hide)
      filter.gameObject.SetActive(false);
    else
      this.ApplyFilter();
  }

  public void SetTitle(string title) => this.titleText.text = title;

  private void Awake()
  {
    if ((UnityEngine.Object) this.holder == (UnityEngine.Object) null)
      this.holder = this.gameObject;
    this.FilterSet.OnFilterChanged += new Action(this.Refresh);
    this.cancelDropdownButton.onClick.AddListener(new UnityAction(this.CancelPressed));
    this.allUnitsDropdown.onValueChanged.AddListener(new UnityAction<int>(this.DropdownValueChanged));
  }

  public void ShowPickOption(
    ISaveableReference startingOption,
    bool allowNone,
    GetAllOptions getAllOptions,
    ReferenceToString toDropdownString,
    PickOrCancel pickOrCancelCallback)
  {
    this.startingOption = startingOption;
    this.getAllOptions = getAllOptions;
    this.toDropdownString = toDropdownString;
    this.pickOrCancelCallback = pickOrCancelCallback;
    this.allowNone = allowNone;
    this.placementFilterIncludeAttached = getAllOptions().All<ISaveableReference>((Func<ISaveableReference, bool>) (x => x is IHasPlacementType hasPlacementType && hasPlacementType.CanBeAttached));
    this.Refresh();
    this.Show();
  }

  private void Show()
  {
    this.holder.SetActive(true);
    this.unitTypeFilter.Interactable = true;
    this.factionFilter.Interactable = true;
    LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform) this.transform);
  }

  public void Hide() => this.holder.SetActive(false);

  private void CancelPressed()
  {
    this.pickOrCancelCallback(false, (ISaveableReference) null);
    this.Hide();
  }

  private void DropdownValueChanged(int index)
  {
    int index1 = index - this.optionOffset;
    this.pickOrCancelCallback(true, index1 >= 0 ? this.optionValues[index1] : (ISaveableReference) null);
    this.Hide();
  }

  private void Refresh()
  {
    if (this.getAllOptions == null)
      return;
    this.ApplyFilter();
    bool flag = this.optionNames.Count > 0;
    this.noOptionsText.gameObject.SetActive(!flag);
    this.allUnitsDropdown.gameObject.SetActive(flag);
    if (!flag)
      return;
    this.optionOffset = this.SetOptions(this.startingOption);
  }

  private void ApplyFilter()
  {
    this.CheckFilterSetup();
    IEnumerable<ISaveableReference> source = this.getAllOptions();
    int num = source.Count<ISaveableReference>();
    this.optionNames.Clear();
    this.optionValues.Clear();
    bool flag1 = num > 0;
    bool flag2 = num > 0;
    bool flag3 = num > 0;
    foreach (ISaveableReference saveableReference in source)
    {
      if (!(saveableReference is SavedUnit))
        flag1 = false;
      if (!(saveableReference is IHasFaction))
        flag2 = false;
      if (!(saveableReference is IHasPlacementType))
        flag3 = false;
      if ((saveableReference == null || saveableReference.CanBeReference) && this.FilterSet.FilterItem((object) saveableReference))
      {
        this.optionValues.Add(saveableReference);
        this.optionNames.Add(this.toDropdownString(saveableReference));
      }
    }
    this.factionFilter.gameObject.SetActive(!this.hideFactionFilter & flag2);
    this.unitTypeFilter.gameObject.SetActive(!this.hideUnitTypeFilter & flag1);
    this.placementTypeFilter.gameObject.SetActive(!this.hidePlacementTypeFilter & flag3);
  }

  private int SetOptions(ISaveableReference startingOption)
  {
    this.allUnitsDropdown.ClearOptions();
    int num = 0;
    if (this.allowNone || startingOption == null)
    {
      this.allUnitsDropdown.options.Add(new TMP_Dropdown.OptionData((this.allowNone ? "<none>" : "<select>").AddColor(new Color(0.5f, 0.5f, 0.5f))));
      ++num;
    }
    if (startingOption != null && this.optionValues.IndexOf(startingOption) != -1)
    {
      this.allUnitsDropdown.options.Add(new TMP_Dropdown.OptionData(this.toDropdownString(startingOption)));
      ++num;
    }
    int input = startingOption != null ? num + this.optionValues.IndexOf(startingOption) : 0;
    this.allUnitsDropdown.AddOptions(this.optionNames);
    this.allUnitsDropdown.SetValueWithoutNotify(input);
    return num;
  }
}

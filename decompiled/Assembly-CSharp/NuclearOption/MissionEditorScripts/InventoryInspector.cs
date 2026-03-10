// Decompiled with JetBrains decompiler
// Type: NuclearOption.MissionEditorScripts.InventoryInspector
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.SavedMission;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

#nullable disable
namespace NuclearOption.MissionEditorScripts;

public class InventoryInspector : MonoBehaviour
{
  [SerializeField]
  private TextMeshProUGUI title;
  [SerializeField]
  private TMP_Dropdown unitDropdown;
  [SerializeField]
  private Button addButton;
  [SerializeField]
  private Button closeButton;
  [SerializeField]
  private InventoryInspectorItem itemPrefab;
  [SerializeField]
  private GameObject inventoryDifferentOverlay;
  private List<UnitDefinition> unitDropdownOptions = new List<UnitDefinition>();
  private NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<(SavedUnit saved, UnitStorage storage)> targets = new NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<(SavedUnit, UnitStorage)>();
  private List<InventoryInspectorItem> entries = new List<InventoryInspectorItem>();

  private void Awake()
  {
    this.addButton.onClick.AddListener(new UnityAction(this.AddUnit));
    this.closeButton.onClick.AddListener((UnityAction) (() => this.gameObject.SetActive(false)));
  }

  public void UnitPanelTargetsChanged(
    List<(SavedUnit saved, UnitStorage storage)> storageTargets)
  {
    if (storageTargets.Count == 0)
    {
      Debug.LogError((object) "InventoryPanel should not be opened if there are no units with UnitStorage");
    }
    else
    {
      this.targets.ReplaceTargets((IReadOnlyList<(SavedUnit, UnitStorage)>) storageTargets);
      this.DestroyItems();
      bool flag = this.targets.AllTheSame<StoredUnitCount>((NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<(SavedUnit, UnitStorage)>.GetField<IReadOnlyList<StoredUnitCount>>) (x => (IReadOnlyList<StoredUnitCount>) x.storage.GetStoredList())) && this.targets.AllTheSame<UnitDefinition>((NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<(SavedUnit, UnitStorage)>.GetField<IReadOnlyList<UnitDefinition>>) (x => (IReadOnlyList<UnitDefinition>) InventoryInspector.CreatePossibleSupplyList(x.storage)), (NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<(SavedUnit, UnitStorage)>.EqualityChecker<UnitDefinition>) ((a, b) => (UnityEngine.Object) a == (UnityEngine.Object) b));
      this.inventoryDifferentOverlay.SetActive(!flag);
      if (flag)
      {
        this.title.text = this.targets.Targets.Count == 1 ? $"[{this.targets.Targets[0].saved.UniqueName}] inventory" : $"[{this.targets.Targets.Count} units] inventory";
        this.RefreshUI();
      }
      else
      {
        this.title.text = "no inventory";
        this.unitDropdown.ClearOptions();
        this.unitDropdownOptions.Clear();
      }
    }
  }

  private void DestroyItems()
  {
    foreach (InventoryInspectorItem entry in this.entries)
    {
      if ((UnityEngine.Object) entry != (UnityEngine.Object) null)
      {
        entry.gameObject.SetActive(false);
        UnityEngine.Object.Destroy((UnityEngine.Object) entry.gameObject);
      }
    }
    this.entries.Clear();
  }

  private void RefreshUI()
  {
    UnitStorage storage = this.targets.Targets[0].storage;
    List<StoredUnitCount> storedList = storage.GetStoredList();
    List<UnitDefinition> possibleSupplyList = InventoryInspector.CreatePossibleSupplyList(storage);
    int? count1 = storedList?.Count;
    this.PopulateDropdown((IReadOnlyList<StoredUnitCount>) storedList, possibleSupplyList);
    bool flag = false;
    while (true)
    {
      int count2 = this.entries.Count;
      int? nullable = count1;
      int valueOrDefault = nullable.GetValueOrDefault();
      if (count2 > valueOrDefault & nullable.HasValue)
      {
        UnityEngine.Object.Destroy((UnityEngine.Object) this.entries.Last<InventoryInspectorItem>());
        this.entries.RemoveAt(this.entries.Count - 1);
        flag = true;
      }
      else
        break;
    }
    while (true)
    {
      int count3 = this.entries.Count;
      int? nullable = count1;
      int valueOrDefault = nullable.GetValueOrDefault();
      if (count3 < valueOrDefault & nullable.HasValue)
      {
        this.entries.Add(UnityEngine.Object.Instantiate<InventoryInspectorItem>(this.itemPrefab, this.transform));
        flag = true;
      }
      else
        break;
    }
    if (flag)
    {
      ILayerRebuildRoot componentInParent = this.GetComponentInParent<ILayerRebuildRoot>();
      componentInParent?.Rebuild();
      componentInParent?.RebuildEndOfFrame();
    }
    for (int index = 0; index < storedList.Count; ++index)
    {
      StoredUnitCount supply = storedList[index];
      this.entries[index].SetEntry(this, supply);
    }
  }

  private static List<UnitDefinition> CreatePossibleSupplyList(UnitStorage storage)
  {
    List<UnitDefinition> possibleSupplyList = new List<UnitDefinition>();
    foreach (UnitDefinition unitDefinition in Encyclopedia.Lookup.Values)
    {
      if (!unitDefinition.disabled && storage.CanFit(unitDefinition))
        possibleSupplyList.Add(unitDefinition);
    }
    return possibleSupplyList;
  }

  private void PopulateDropdown(
    IReadOnlyList<StoredUnitCount> currentSupply,
    List<UnitDefinition> possibleUnits)
  {
    this.unitDropdown.ClearOptions();
    this.unitDropdownOptions.Clear();
    foreach (UnitDefinition possibleUnit in possibleUnits)
    {
      UnitDefinition def = possibleUnit;
      if (!currentSupply.Any<StoredUnitCount>((Func<StoredUnitCount, bool>) (x => x.UnitType == def.jsonKey)))
      {
        this.unitDropdownOptions.Add(def);
        this.unitDropdown.options.Add(new TMP_Dropdown.OptionData(def.unitName, def.friendlyIcon));
      }
    }
    this.addButton.interactable = this.unitDropdown.options.Count > 0;
    this.unitDropdown.SetValueWithoutNotify(0);
    this.unitDropdown.RefreshShownValue();
  }

  public void AddUnit()
  {
    if (this.unitDropdownOptions.Count == 0)
    {
      Debug.LogError((object) "Add clicked when there are no options in the dropdown");
    }
    else
    {
      UnitDefinition unitDropdownOption = this.unitDropdownOptions[this.unitDropdown.value];
      foreach ((SavedUnit saved, UnitStorage storage) target in (IEnumerable<(SavedUnit saved, UnitStorage storage)>) this.targets.Targets)
        target.storage.AddOrRemoveUnitEditor(MissionManager.CurrentMission, target.saved, unitDropdownOption, 1);
      this.RefreshUI();
    }
  }

  public void AddOne(InventoryInspectorItem entry)
  {
    foreach ((SavedUnit saved, UnitStorage storage) target in (IEnumerable<(SavedUnit saved, UnitStorage storage)>) this.targets.Targets)
      target.storage.AddOrRemoveUnitEditor(MissionManager.CurrentMission, target.saved, entry.UnitDefinition, 1);
    this.RefreshUI();
  }

  public void RemoveOne(InventoryInspectorItem entry)
  {
    foreach ((SavedUnit saved, UnitStorage storage) target in (IEnumerable<(SavedUnit saved, UnitStorage storage)>) this.targets.Targets)
      target.storage.AddOrRemoveUnitEditor(MissionManager.CurrentMission, target.saved, entry.UnitDefinition, -1);
    this.RefreshUI();
  }
}

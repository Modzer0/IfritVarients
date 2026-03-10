// Decompiled with JetBrains decompiler
// Type: NuclearOption.MissionEditorScripts.BuildingOptions
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.SavedMission;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

#nullable disable
namespace NuclearOption.MissionEditorScripts;

public class BuildingOptions : UnitPanelOptions
{
  private const string WARHEAD_PRODUCTION_TYPE = "Nuclear Warhead";
  [Header("Capture")]
  [SerializeField]
  private Toggle captureToggle;
  [SerializeField]
  private GameObject captureToggleDifferentValue;
  [Header("Factory")]
  [SerializeField]
  private GameObject factoryOptionsPanel;
  [SerializeField]
  private TextMeshProUGUI factionHeader;
  [SerializeField]
  private GameObject productionTypeDifferentWarning;
  [SerializeField]
  private TextMeshProUGUI productionTimeLabel;
  [SerializeField]
  private Slider productionTimeSlider;
  [SerializeField]
  private TMP_Dropdown productionUnitDropdown;
  private List<(SavedUnit saved, SavedBuilding.FactoryOptions options, Factory factory)> factoryList = new List<(SavedUnit, SavedBuilding.FactoryOptions, Factory)>();

  public override void Cleanup() => this.targets.RemoveChanged((object) this.captureToggle);

  protected override void SetupInner()
  {
    this.productionUnitDropdown.onValueChanged.AddListener(new UnityAction<int>(this.ProductionUnitDropdownChanged));
    this.productionTimeSlider.onValueChanged.AddListener(new UnityAction<float>(this.ProductionTimeSliderChanged));
    this.targets.SetupToggle(this.captureToggle, this.captureToggleDifferentValue, (NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<SavedUnit>.GetFieldRef<bool>) (x => ref ((SavedBuilding) x).capturable));
    this.captureToggle.onValueChanged.AddListener((UnityAction<bool>) (_ => this.unitMenu.CheckAllOverrides()));
  }

  public override void OnTargetsChanged()
  {
    this.factoryList.Clear();
    foreach (SavedUnit target in (IEnumerable<SavedUnit>) this.targets.Targets)
    {
      Factory component;
      if (target.Unit.TryGetComponent<Factory>(out component))
      {
        SavedBuilding savedBuilding1 = (SavedBuilding) target;
        SavedBuilding savedBuilding2 = savedBuilding1;
        if (savedBuilding2.factoryOptions == null)
          savedBuilding2.factoryOptions = new SavedBuilding.FactoryOptions();
        this.factoryList.Add(((SavedUnit) savedBuilding1, savedBuilding1.factoryOptions, component));
      }
    }
    this.factoryOptionsPanel.SetActive(this.factoryList.Count > 0);
    if (this.factoryList.Count <= 0)
      return;
    this.factionHeader.text = this.targets.Targets.Count == 1 ? "Factory" : (this.factoryList.Count == 1 ? "1 Factory" : $"{this.factoryList.Count} Factories");
    bool flag1 = true;
    bool flag2 = true;
    float productionTime = this.factoryList[0].options.productionTime;
    string productionType = this.factoryList[0].options.productionType;
    Factory.FactoryType factoryType = this.factoryList[0].factory.factoryType;
    for (int index = 1; index < this.factoryList.Count; ++index)
    {
      (SavedUnit _, SavedBuilding.FactoryOptions options, Factory factory) = this.factoryList[index];
      if (factoryType != factory.factoryType)
      {
        flag2 = false;
        break;
      }
      if ((double) productionTime != (double) options.productionTime || productionType != options.productionType)
      {
        flag1 = false;
        break;
      }
    }
    this.productionTypeDifferentWarning.SetActive(!flag2);
    if (flag2)
    {
      int input = -1;
      if (factoryType == Factory.FactoryType.Nukes)
      {
        this.GenerateWeaponList();
        if (flag1)
          input = productionType == "Nuclear Warhead" ? 1 : 0;
      }
      else
      {
        this.GenerateUnitList();
        if (flag1)
          input = this.GetUnitIndex(productionType);
      }
      this.productionUnitDropdown.SetValueWithoutNotify(input);
      this.productionUnitDropdown.RefreshShownValue();
      this.SetProductionTimeSlider(flag1 ? new float?(productionTime) : new float?());
    }
    else
    {
      this.productionUnitDropdown.SetValueWithoutNotify(-1);
      this.productionUnitDropdown.RefreshShownValue();
      this.SetProductionTimeSlider(new float?());
    }
  }

  private void GenerateUnitList()
  {
    this.productionUnitDropdown.options.Clear();
    this.productionUnitDropdown.options.Add(new TMP_Dropdown.OptionData("none"));
    foreach (UnitDefinition aircraftAndVehicle in Encyclopedia.i.GetAircraftAndVehicles())
    {
      if (!aircraftAndVehicle.disabled)
        this.productionUnitDropdown.options.Add(new TMP_Dropdown.OptionData(aircraftAndVehicle.unitName));
    }
  }

  private void GenerateWeaponList()
  {
    this.productionUnitDropdown.options.Clear();
    this.productionUnitDropdown.options.Add(new TMP_Dropdown.OptionData("none"));
    this.productionUnitDropdown.options.Add(new TMP_Dropdown.OptionData("Nuclear Warhead"));
  }

  private int GetUnitIndex(string productionType)
  {
    UnitDefinition unitDefinition;
    if (!Encyclopedia.Lookup.TryGetValue(productionType, out unitDefinition))
      return 0;
    for (int index = 0; index < this.productionUnitDropdown.options.Count; ++index)
    {
      if (this.productionUnitDropdown.options[index].text == unitDefinition.unitName)
        return index;
    }
    return 0;
  }

  private void ProductionUnitDropdownChanged(int index)
  {
    foreach ((SavedUnit saved, SavedBuilding.FactoryOptions options, Factory factory) factory in this.factoryList)
      this.unitMenu.CheckOverride(factory.saved);
    string productionType = GetProductionType(index);
    foreach ((SavedUnit saved, SavedBuilding.FactoryOptions options, Factory factory) factory in this.factoryList)
      factory.options.productionType = productionType;

    string GetProductionType(int index)
    {
      if (this.productionUnitDropdown.options[index].text == "Nuclear Warhead")
        return "Nuclear Warhead";
      foreach (UnitDefinition aircraftAndVehicle in Encyclopedia.i.GetAircraftAndVehicles())
      {
        if (this.productionUnitDropdown.options[index].text == aircraftAndVehicle.unitName)
          return aircraftAndVehicle.unitPrefab.name;
      }
      return "";
    }
  }

  private void SetProductionTimeSlider(float? productionTime)
  {
    if (productionTime.HasValue)
    {
      this.productionTimeSlider.SetValueWithoutNotify(productionTime.Value);
      this.productionTimeLabel.text = $"{productionTime.Value:F0}s";
    }
    else
    {
      this.productionTimeSlider.SetValueWithoutNotify(0.0f);
      this.productionTimeLabel.text = "-";
    }
  }

  private void ProductionTimeSliderChanged(float value)
  {
    foreach ((SavedUnit saved, SavedBuilding.FactoryOptions options, Factory factory) factory in this.factoryList)
      this.unitMenu.CheckOverride(factory.saved);
    foreach ((SavedUnit saved, SavedBuilding.FactoryOptions options, Factory factory) factory in this.factoryList)
      factory.options.productionTime = value;
    this.SetProductionTimeSlider(new float?(value));
  }
}

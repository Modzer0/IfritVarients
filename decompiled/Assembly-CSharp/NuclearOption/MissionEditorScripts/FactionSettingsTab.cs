// Decompiled with JetBrains decompiler
// Type: NuclearOption.MissionEditorScripts.FactionSettingsTab
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.SavedMission;
using NuclearOption.SavedMission.ObjectiveV2;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

#nullable disable
namespace NuclearOption.MissionEditorScripts;

public class FactionSettingsTab : MonoBehaviour, IMissionTab
{
  [SerializeField]
  private TMP_Dropdown factionSelect;
  [SerializeField]
  private TMP_Dropdown unitSelect;
  [SerializeField]
  private Button addUnitButton;
  [SerializeField]
  private Slider factionStartingBalanceSlider;
  [SerializeField]
  private Slider factionRegularIncomeSlider;
  [SerializeField]
  private Slider playerJoinAllowanceSlider;
  [SerializeField]
  private Slider killRewardSlider;
  [SerializeField]
  private Slider playerTaxRateSlider;
  [SerializeField]
  private Slider excessFundsDistributeSlider;
  [SerializeField]
  private TextMeshProUGUI factionStartingBalanceLabel;
  [SerializeField]
  private TextMeshProUGUI factionRegularIncomeLabel;
  [SerializeField]
  private TextMeshProUGUI playerJoinAllowanceLabel;
  [SerializeField]
  private TextMeshProUGUI killRewardLabel;
  [SerializeField]
  private TextMeshProUGUI playerTaxRateLabel;
  [SerializeField]
  private TextMeshProUGUI excessFundsDistributeLabel;
  [SerializeField]
  private Slider reserveAirframesSlider;
  [SerializeField]
  private Slider extraReservesPerPlayerSlider;
  [SerializeField]
  private Slider AIAircraftLimitSlider;
  [SerializeField]
  private Slider reduceAIPerFriendlyPlayerSlider;
  [SerializeField]
  private Slider addAIPerEnemyPlayerSlider;
  [SerializeField]
  private TextMeshProUGUI reserveAirframesLabel;
  [SerializeField]
  private TextMeshProUGUI extraReservesPerPlayerLabel;
  [SerializeField]
  private TextMeshProUGUI AIAircraftLimitLabel;
  [SerializeField]
  private TextMeshProUGUI reduceAIPerFriendlyPlayerLabel;
  [SerializeField]
  private TextMeshProUGUI addAIPerEnemyPlayerLabel;
  [SerializeField]
  private Slider startingWarheadsSlider;
  [SerializeField]
  private Slider reserveWarheadsSlider;
  [SerializeField]
  private TextMeshProUGUI startingWarheadsLabel;
  [SerializeField]
  private TextMeshProUGUI reserveWarheadsLabel;
  [SerializeField]
  private Toggle preventJoinToggle;
  [SerializeField]
  private GameObject preventJoinToggleDifferentValue;
  [SerializeField]
  private Transform inventoryPanel;
  [SerializeField]
  private GameObject supplyItemPrefab;
  [SerializeField]
  private GameObject supplyNotSameOverlay;
  private NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<MissionFaction> selectedFactions = new NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<MissionFaction>();
  private List<InventoryItem> inventoryItems = new List<InventoryItem>();
  private UnitDefinition unitSelection;
  private Mission mission;
  private const string ALL_FACTIONS = "Both";
  [SerializeField]
  private Toggle preventDonationToggle;
  [SerializeField]
  private GameObject preventDonationToggleDifferentValue;
  [Header("Camera Start Position")]
  [SerializeField]
  private OverrideDataField cameraPositionOverrideField;
  [SerializeField]
  private Vector3DataField cameraPositionField;
  [SerializeField]
  private Vector3DataField cameraRotationField;
  [SerializeField]
  private Button setCameraFromCurrentButton;
  [SerializeField]
  private Button moveCameraToSavedButton;
  [SerializeField]
  private GameObject cameraPositionDifferentOverlay;
  private ValueWrapperOverride<PositionRotation> cameraOverrideWrapper;
  private ValueWrapperGlobalPosition cameraPositionWrapper;
  private ValueWrapperQuaternion cameraRotationWrapper;

  private void Start()
  {
    this.factionSelect.onValueChanged.AddListener((UnityAction<int>) (_ => this.SelectFaction()));
    this.unitSelect.onValueChanged.AddListener((UnityAction<int>) (_ => this.SelectUnitType()));
    this.selectedFactions.SetupSlider<float>(this.factionStartingBalanceSlider, this.factionStartingBalanceLabel, (NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<MissionFaction>.GetFieldRef<float>) (f => ref f.startingBalance), (NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<MissionFaction>.ModifyValue<float, float>) (v => Mathf.Sqrt(v)), (NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<MissionFaction>.ModifyValue<float, float>) (v => v * v), (NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<MissionFaction>.GetLabelString<float>) (v => UnitConverter.ValueReading(v) ?? ""));
    this.selectedFactions.SetupSlider<float>(this.factionRegularIncomeSlider, this.factionRegularIncomeLabel, (NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<MissionFaction>.GetFieldRef<float>) (f => ref f.regularIncome), (NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<MissionFaction>.ModifyValue<float, float>) (v => Mathf.Sqrt(v)), (NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<MissionFaction>.ModifyValue<float, float>) (v => v * v), (NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<MissionFaction>.GetLabelString<float>) (v => UnitConverter.ValueReading(v) ?? ""));
    this.selectedFactions.SetupSlider(this.excessFundsDistributeSlider, this.excessFundsDistributeLabel, (NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<MissionFaction>.GetFieldRef<float>) (f => ref f.excessFundsDistributePercent), (NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<MissionFaction>.GetLabelString<float>) (v => $"{(ValueType) (v * 100f):0}%"));
    this.selectedFactions.SetupSlider<float>(this.playerJoinAllowanceSlider, this.playerJoinAllowanceLabel, (NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<MissionFaction>.GetFieldRef<float>) (f => ref f.playerJoinAllowance), (NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<MissionFaction>.ModifyValue<float, float>) (v => Mathf.Sqrt(v)), (NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<MissionFaction>.ModifyValue<float, float>) (v => v * v), (NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<MissionFaction>.GetLabelString<float>) (v => UnitConverter.ValueReading(v) ?? ""));
    this.selectedFactions.SetupSlider(this.killRewardSlider, this.killRewardLabel, (NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<MissionFaction>.GetFieldRef<float>) (f => ref f.killReward), (NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<MissionFaction>.GetLabelString<float>) (v => $"{(ValueType) (v * 100f):F0}%"));
    this.selectedFactions.SetupSlider(this.playerTaxRateSlider, this.playerTaxRateLabel, (NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<MissionFaction>.GetFieldRef<float>) (f => ref f.playerTaxRate), (NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<MissionFaction>.GetLabelString<float>) (v => $"{(ValueType) (v * 100f):0}%"));
    this.selectedFactions.SetupSlider<int>(this.startingWarheadsSlider, this.startingWarheadsLabel, (NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<MissionFaction>.GetFieldRef<int>) (f => ref f.startingWarheads), (NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<MissionFaction>.ModifyValue<int, float>) (v => (float) v), (NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<MissionFaction>.ModifyValue<float, int>) (v => (int) v), (NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<MissionFaction>.GetLabelString<int>) (v => $"{v:0}"));
    this.selectedFactions.SetupSlider<int>(this.reserveWarheadsSlider, this.reserveWarheadsLabel, (NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<MissionFaction>.GetFieldRef<int>) (f => ref f.reserveWarheads), (NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<MissionFaction>.ModifyValue<int, float>) (v => (float) v), (NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<MissionFaction>.ModifyValue<float, int>) (v => (int) v), (NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<MissionFaction>.GetLabelString<int>) (v => $"{v:0}"));
    this.selectedFactions.SetupSlider<int>(this.reserveAirframesSlider, this.reserveAirframesLabel, (NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<MissionFaction>.GetFieldRef<int>) (f => ref f.reserveAirframes), (NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<MissionFaction>.ModifyValue<int, float>) (v => (float) v), (NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<MissionFaction>.ModifyValue<float, int>) (v => (int) v), (NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<MissionFaction>.GetLabelString<int>) (v => $"{v:0}"));
    this.selectedFactions.SetupSlider<int>(this.extraReservesPerPlayerSlider, this.extraReservesPerPlayerLabel, (NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<MissionFaction>.GetFieldRef<int>) (f => ref f.extraReservesPerPlayer), (NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<MissionFaction>.ModifyValue<int, float>) (v => (float) v), (NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<MissionFaction>.ModifyValue<float, int>) (v => (int) v), (NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<MissionFaction>.GetLabelString<int>) (v => $"{v:0}"));
    this.selectedFactions.SetupSlider<int>(this.AIAircraftLimitSlider, this.AIAircraftLimitLabel, (NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<MissionFaction>.GetFieldRef<int>) (f => ref f.AIAircraftLimit), (NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<MissionFaction>.ModifyValue<int, float>) (v => (float) v), (NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<MissionFaction>.ModifyValue<float, int>) (v => (int) v), (NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<MissionFaction>.GetLabelString<int>) (v => $"{v:0}"));
    this.selectedFactions.SetupSlider(this.reduceAIPerFriendlyPlayerSlider, this.reduceAIPerFriendlyPlayerLabel, (NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<MissionFaction>.GetFieldRef<float>) (f => ref f.reduceAIPerFriendlyPlayer), (NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<MissionFaction>.GetLabelString<float>) (v => $"{v:0.0}"));
    this.selectedFactions.SetupSlider(this.addAIPerEnemyPlayerSlider, this.addAIPerEnemyPlayerLabel, (NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<MissionFaction>.GetFieldRef<float>) (f => ref f.addAIPerEnemyPlayer), (NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<MissionFaction>.GetLabelString<float>) (v => $"{v:0.0}"));
    this.selectedFactions.SetupToggle(this.preventJoinToggle, this.preventJoinToggleDifferentValue, (NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<MissionFaction>.GetFieldRef<bool>) (f => ref f.preventJoin));
    this.selectedFactions.SetupToggle(this.preventDonationToggle, this.preventDonationToggleDifferentValue, (NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<MissionFaction>.GetFieldRef<bool>) (f => ref f.preventDonation));
    this.SetupCameraPositionFields();
    this.selectedFactions.AddAndInvokeChanged((object) this, new Action(this.OnFactionsChanged));
  }

  public void SetMission(Mission mission)
  {
    this.mission = mission;
    this.factionSelect.options.Clear();
    this.factionSelect.options.Add(new TMP_Dropdown.OptionData("Both"));
    foreach (MissionFaction faction in mission.factions)
      this.factionSelect.options.Add(new TMP_Dropdown.OptionData(faction.factionName));
    this.factionSelect.SetValueWithoutNotify(0);
    this.SelectFaction();
  }

  private void SetupCameraPositionFields()
  {
    if (GameManager.gameState == GameState.Editor)
    {
      this.cameraPositionOverrideField.gameObject.SetActive(true);
      this.setCameraFromCurrentButton.onClick.AddListener(new UnityAction(this.SetCameraPositionFromCurrent));
      this.moveCameraToSavedButton.onClick.AddListener(new UnityAction(this.MoveCameraToSaved));
      this.cameraOverrideWrapper = ValueWrapper.FromCallback<ValueWrapperOverride<PositionRotation>, Override<PositionRotation>>((object) this, new Override<PositionRotation>(), this.selectedFactions.SetSameValueAction<Override<PositionRotation>>((NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<MissionFaction>.GetFieldRef<Override<PositionRotation>>) (f => ref f.cameraStartPosition)));
      this.cameraPositionWrapper = ValueWrapper.FromCallback<ValueWrapperGlobalPosition, GlobalPosition>((object) this, new GlobalPosition(), this.selectedFactions.SetSameValueAction<GlobalPosition>((NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<MissionFaction>.GetFieldRef<GlobalPosition>) (f => ref f.cameraStartPosition.Value.Position)));
      this.cameraRotationWrapper = ValueWrapper.FromCallback<ValueWrapperQuaternion, Quaternion>((object) this, new Quaternion(), this.selectedFactions.SetSameValueAction<Quaternion>((NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<MissionFaction>.GetFieldRef<Quaternion>) (f => ref f.cameraStartPosition.Value.Rotation)));
      this.cameraPositionOverrideField.Setup<PositionRotation>((ValueWrapper<Override<PositionRotation>>) this.cameraOverrideWrapper, (OverrideDataField.InnerField) (DataField) this.cameraPositionField, (OverrideDataField.InnerField) (DataField) this.cameraRotationField, (OverrideDataField.InnerField) this.moveCameraToSavedButton, (OverrideDataField.InnerField) this.setCameraFromCurrentButton);
      this.cameraPositionField.Setup("Position", (IValueWrapper<Vector3>) this.cameraPositionWrapper);
      this.cameraRotationField.Setup("Rotation", (IValueWrapper<Vector3>) this.cameraRotationWrapper);
    }
    else
      this.cameraPositionOverrideField.gameObject.SetActive(false);
  }

  private void SetCameraPositionFromCurrent()
  {
    PositionRotation positionRotation;
    SceneSingleton<CameraStateManager>.i.GetCameraPosition(out positionRotation);
    this.cameraPositionWrapper.SetValue(positionRotation.Position, (object) null, true);
    this.cameraRotationWrapper.SetValue(positionRotation.Rotation, (object) null, true);
  }

  private void MoveCameraToSaved()
  {
    SceneSingleton<CameraStateManager>.i.SetCameraPosition(this.cameraOverrideWrapper.Value.Value);
  }

  public void SelectFaction()
  {
    string text = this.factionSelect.options[this.factionSelect.value].text;
    if (text == "Both")
    {
      this.selectedFactions.ReplaceTargets((IReadOnlyList<MissionFaction>) this.mission.factions);
    }
    else
    {
      MissionFaction faction;
      if (this.mission.TryGetFaction(text, out faction))
        this.selectedFactions.ReplaceTargets(faction);
      else
        Debug.LogWarning((object) ("Failed to find faction with name: " + text));
    }
  }

  private void OnFactionsChanged()
  {
    if (this.cameraOverrideWrapper != null)
    {
      Override<PositionRotation> sameValue1;
      bool sameValue2 = this.selectedFactions.TryGetSameValue<Override<PositionRotation>>((NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<MissionFaction>.GetField<Override<PositionRotation>>) (f => f.cameraStartPosition), out sameValue1);
      this.cameraPositionDifferentOverlay.SetActive(!sameValue2);
      if (sameValue2)
      {
        this.cameraOverrideWrapper.SetValue(sameValue1, (object) this, true);
        this.cameraPositionWrapper.SetValue(sameValue1.Value.Position, (object) this, true);
        this.cameraRotationWrapper.SetValue(sameValue1.Value.Rotation, (object) this, true);
      }
      else
      {
        this.cameraOverrideWrapper.SetValue(new Override<PositionRotation>(), (object) this, true);
        this.cameraPositionWrapper.SetValue(new GlobalPosition(), (object) this, true);
        this.cameraRotationWrapper.SetValue(new Quaternion(), (object) this, true);
      }
    }
    this.GenerateUnitList();
  }

  private void GenerateUnitList()
  {
    List<string> stringList = new List<string>();
    foreach (Component inventoryItem in this.inventoryItems)
      UnityEngine.Object.Destroy((UnityEngine.Object) inventoryItem.gameObject);
    this.inventoryItems.Clear();
    bool flag = this.selectedFactions.AllTheSame<FactionSupply>((NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<MissionFaction>.GetField<IReadOnlyList<FactionSupply>>) (x => (IReadOnlyList<FactionSupply>) x.supplies));
    this.supplyNotSameOverlay.SetActive(!flag);
    if (!flag)
      return;
    IReadOnlyList<MissionFaction> targets = this.selectedFactions.Targets;
    for (int index = 0; index < targets[0].supplies.Count; ++index)
    {
      FactionSupply supply = targets[0].supplies[index];
      stringList.Add(supply.unitType);
      InventoryItem component = UnityEngine.Object.Instantiate<GameObject>(this.supplyItemPrefab, this.inventoryPanel).GetComponent<InventoryItem>();
      component.SetInventoryItem(index, supply, targets, this);
      this.inventoryItems.Add(component);
    }
    this.unitSelect.options.Clear();
    foreach (UnitDefinition aircraftAndVehicle in Encyclopedia.i.GetAircraftAndVehicles())
    {
      if (!stringList.Contains(aircraftAndVehicle.unitPrefab.name))
        this.unitSelect.options.Add(new TMP_Dropdown.OptionData(aircraftAndVehicle.unitName));
    }
    this.SelectUnitType();
    this.unitSelect.SetValueWithoutNotify(0);
    this.unitSelect.RefreshShownValue();
  }

  public void SelectUnitType()
  {
    if (this.unitSelect.options.Count == 0)
    {
      this.unitSelection = (UnitDefinition) null;
      this.addUnitButton.interactable = false;
    }
    else
    {
      foreach (UnitDefinition aircraftAndVehicle in Encyclopedia.i.GetAircraftAndVehicles())
      {
        if (aircraftAndVehicle.unitName == this.unitSelect.options[this.unitSelect.value].text)
        {
          this.unitSelection = aircraftAndVehicle;
          break;
        }
      }
      this.addUnitButton.interactable = true;
    }
  }

  public void RemoveSupplyEntry(InventoryItem inventoryItem)
  {
    this.inventoryItems.Remove(inventoryItem);
    this.GenerateUnitList();
  }

  public void AddSupplyUnit()
  {
    if (string.IsNullOrEmpty(this.unitSelect.options[this.unitSelect.value].text) || (UnityEngine.Object) this.unitSelection == (UnityEngine.Object) null)
      return;
    foreach (MissionFaction target in (IEnumerable<MissionFaction>) this.selectedFactions.Targets)
    {
      MissionFaction missionFaction1;
      MissionFaction missionFaction2 = missionFaction1 = target;
      if (missionFaction2.supplies == null)
        missionFaction2.supplies = new List<FactionSupply>();
      missionFaction1.supplies.Add(new FactionSupply(this.unitSelection.unitPrefab.name, 1));
    }
    this.GenerateUnitList();
  }
}

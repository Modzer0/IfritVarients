// Decompiled with JetBrains decompiler
// Type: NuclearOption.MissionEditorScripts.MissionSettingsTab
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.SavedMission;
using NuclearOption.SavedMission.ObjectiveV2;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

#nullable disable
namespace NuclearOption.MissionEditorScripts;

public class MissionSettingsTab : MonoBehaviour, IMissionTab
{
  [SerializeField]
  private TMP_InputField summaryInput;
  [SerializeField]
  private TMP_Dropdown playerModeDropdown;
  [SerializeField]
  private Slider startingRankSlider;
  [SerializeField]
  private Slider rankMultiplierSlider;
  [SerializeField]
  private Slider successfulSortieBonusSlider;
  [SerializeField]
  private Slider nuclearEscalationThresholdSlider;
  [SerializeField]
  private Slider strategicEscalationThresholdSlider;
  [SerializeField]
  private Slider minRankTacticalSlider;
  [SerializeField]
  private Slider minRankStrategicSlider;
  [SerializeField]
  private Toggle respawnToggle;
  [SerializeField]
  private TextMeshProUGUI startingRankLabel;
  [SerializeField]
  private TextMeshProUGUI rankMultiplierLabel;
  [SerializeField]
  private TextMeshProUGUI successfulSortieBonusLabel;
  [SerializeField]
  private TextMeshProUGUI nuclearEscalationThresholdLabel;
  [SerializeField]
  private TextMeshProUGUI strategicEscalationThresholdLabel;
  [SerializeField]
  private TextMeshProUGUI minRankTacticalLabel;
  [SerializeField]
  private TextMeshProUGUI minRankStrategicLabel;
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
  [Space]
  [SerializeField]
  private Slider wrecksMaxNumberSlider;
  [SerializeField]
  private Slider wrecksDecayTimeSlider;
  [SerializeField]
  private TextMeshProUGUI wrecksMaxNumberLabel;
  [SerializeField]
  private TextMeshProUGUI wrecksDecayTimeLabel;
  private MissionSettings missionSettings;
  private ValueWrapperOverride<PositionRotation> overrideWrapper;
  private ValueWrapperGlobalPosition cameraPositionWrapper;
  private ValueWrapperQuaternion cameraRotationWrapper;

  private void Awake()
  {
    this.playerModeDropdown.ClearOptions();
    this.playerModeDropdown.AddOptions(EnumNames<PlayerMode>.GetNames());
    this.playerModeDropdown.onValueChanged.AddListener(new UnityAction<int>(this.ApplySettings));
    this.summaryInput.onEndEdit.AddListener(new UnityAction<string>(this.ApplySettings));
    this.startingRankSlider.onValueChanged.AddListener(new UnityAction<float>(this.ApplySettings));
    this.rankMultiplierSlider.onValueChanged.AddListener(new UnityAction<float>(this.ApplySettings));
    this.successfulSortieBonusSlider.onValueChanged.AddListener(new UnityAction<float>(this.ApplySettings));
    this.nuclearEscalationThresholdSlider.onValueChanged.AddListener(new UnityAction<float>(this.ApplySettings));
    this.strategicEscalationThresholdSlider.onValueChanged.AddListener(new UnityAction<float>(this.ApplySettings));
    this.respawnToggle.onValueChanged.AddListener(new UnityAction<bool>(this.ApplySettings));
    this.wrecksMaxNumberSlider.onValueChanged.AddListener(new UnityAction<float>(this.ApplySettings));
    this.wrecksDecayTimeSlider.onValueChanged.AddListener(new UnityAction<float>(this.ApplySettings));
    this.SetupCameraPositionFields();
  }

  public void SetMission(Mission mission)
  {
    this.missionSettings = mission.missionSettings;
    this.summaryInput.SetTextWithoutNotify(this.missionSettings.description);
    this.playerModeDropdown.SetValueWithoutNotify((int) this.missionSettings.playerMode);
    this.startingRankSlider.SetValueWithoutNotify((float) this.missionSettings.playerStartingRank);
    this.rankMultiplierSlider.SetValueWithoutNotify(this.missionSettings.rankMultiplier);
    this.respawnToggle.SetIsOnWithoutNotify(this.missionSettings.allowRespawn);
    this.successfulSortieBonusSlider.SetValueWithoutNotify(this.missionSettings.successfulSortieBonus);
    this.nuclearEscalationThresholdSlider.SetValueWithoutNotify(Mathf.Sqrt(this.missionSettings.nuclearEscalationThreshold));
    this.strategicEscalationThresholdSlider.SetValueWithoutNotify(Mathf.Sqrt(this.missionSettings.strategicEscalationThreshold));
    this.minRankTacticalSlider.SetValueWithoutNotify(Mathf.Sqrt((float) this.missionSettings.minRankTacticalWarhead));
    this.minRankStrategicSlider.SetValueWithoutNotify(Mathf.Sqrt((float) this.missionSettings.minRankStrategicWarhead));
    this.wrecksMaxNumberSlider.SetValueWithoutNotify((float) this.missionSettings.wrecksMaxNumber * 0.1f);
    this.wrecksDecayTimeSlider.SetValueWithoutNotify(this.missionSettings.wrecksDecayTime);
    if (this.overrideWrapper != null)
    {
      this.overrideWrapper.SetValue(this.missionSettings.cameraStartPosition, (object) this, true);
      this.cameraPositionWrapper.SetValue(this.missionSettings.cameraStartPosition.Value.Position, (object) this, true);
      this.cameraRotationWrapper.SetValue(this.missionSettings.cameraStartPosition.Value.Rotation, (object) this, true);
    }
    this.UpdateLabels();
  }

  private void SetupCameraPositionFields()
  {
    if (GameManager.gameState == GameState.Editor)
    {
      this.cameraPositionOverrideField.gameObject.SetActive(true);
      this.setCameraFromCurrentButton.onClick.AddListener(new UnityAction(this.SetCameraPositionFromCurrent));
      this.moveCameraToSavedButton.onClick.AddListener(new UnityAction(this.MoveCameraToSaved));
      this.overrideWrapper = ValueWrapper.FromCallback<ValueWrapperOverride<PositionRotation>, Override<PositionRotation>>((object) this, new Override<PositionRotation>(), (Action<Override<PositionRotation>>) (newValue => this.missionSettings.cameraStartPosition = newValue));
      this.cameraPositionWrapper = ValueWrapper.FromCallback<ValueWrapperGlobalPosition, GlobalPosition>((object) this, new GlobalPosition(), (Action<GlobalPosition>) (newValue => this.missionSettings.cameraStartPosition.Value.Position = newValue));
      this.cameraRotationWrapper = ValueWrapper.FromCallback<ValueWrapperQuaternion, Quaternion>((object) this, new Quaternion(), (Action<Quaternion>) (newValue => this.missionSettings.cameraStartPosition.Value.Rotation = newValue));
      this.cameraPositionOverrideField.Setup<PositionRotation>((ValueWrapper<Override<PositionRotation>>) this.overrideWrapper, (OverrideDataField.InnerField) (DataField) this.cameraPositionField, (OverrideDataField.InnerField) (DataField) this.cameraRotationField, (OverrideDataField.InnerField) this.moveCameraToSavedButton, (OverrideDataField.InnerField) this.setCameraFromCurrentButton);
      this.cameraPositionField.Setup("Position", (IValueWrapper<Vector3>) this.cameraPositionWrapper);
      this.cameraRotationField.Setup("Rotation", (IValueWrapper<Vector3>) this.cameraRotationWrapper);
    }
    else
      this.cameraPositionOverrideField.gameObject.SetActive(false);
  }

  private void ApplySettings(string arg0) => this.ApplySettings();

  private void ApplySettings(int arg0) => this.ApplySettings();

  private void ApplySettings(float arg0) => this.ApplySettings();

  private void ApplySettings(bool arg0) => this.ApplySettings();

  public void ApplySettings()
  {
    this.missionSettings.description = this.summaryInput.text;
    this.missionSettings.playerMode = (PlayerMode) this.playerModeDropdown.value;
    this.missionSettings.playerStartingRank = (int) this.startingRankSlider.value;
    this.missionSettings.rankMultiplier = this.rankMultiplierSlider.value;
    this.missionSettings.allowRespawn = this.respawnToggle.isOn;
    this.missionSettings.successfulSortieBonus = this.successfulSortieBonusSlider.value;
    if ((double) this.strategicEscalationThresholdSlider.value < (double) this.nuclearEscalationThresholdSlider.value)
      this.strategicEscalationThresholdSlider.SetValueWithoutNotify(this.nuclearEscalationThresholdSlider.value);
    this.missionSettings.nuclearEscalationThreshold = this.nuclearEscalationThresholdSlider.value * this.nuclearEscalationThresholdSlider.value;
    this.missionSettings.strategicEscalationThreshold = this.strategicEscalationThresholdSlider.value * this.strategicEscalationThresholdSlider.value;
    if ((double) this.minRankStrategicSlider.value < (double) this.minRankTacticalSlider.value)
      this.minRankStrategicSlider.SetValueWithoutNotify(this.minRankTacticalSlider.value);
    this.missionSettings.minRankTacticalWarhead = (int) this.minRankTacticalSlider.value;
    this.missionSettings.minRankStrategicWarhead = (int) this.minRankStrategicSlider.value;
    this.missionSettings.wrecksMaxNumber = (int) this.wrecksMaxNumberSlider.value * 10;
    this.missionSettings.wrecksDecayTime = this.wrecksDecayTimeSlider.value;
    this.UpdateLabels();
  }

  private void UpdateLabels()
  {
    this.startingRankLabel.text = this.startingRankSlider.value.ToString("F0");
    this.rankMultiplierLabel.text = (this.rankMultiplierSlider.value * 100f).ToString("F0") + "%";
    this.successfulSortieBonusLabel.text = (this.successfulSortieBonusSlider.value * 100f).ToString("F0") + "%";
    this.nuclearEscalationThresholdLabel.text = $"{(ValueType) (float) ((double) this.nuclearEscalationThresholdSlider.value * (double) this.nuclearEscalationThresholdSlider.value)}";
    this.strategicEscalationThresholdLabel.text = $"{(ValueType) (float) ((double) this.strategicEscalationThresholdSlider.value * (double) this.strategicEscalationThresholdSlider.value)}";
    this.minRankTacticalLabel.text = $"{this.minRankTacticalSlider.value}";
    this.minRankStrategicLabel.text = $"{this.minRankStrategicSlider.value}";
    this.wrecksMaxNumberLabel.text = (double) this.wrecksMaxNumberSlider.value > 0.0 ? $"{(ValueType) (float) (10.0 * (double) this.wrecksMaxNumberSlider.value)}" : "No despawn";
    this.wrecksDecayTimeLabel.text = (double) this.wrecksDecayTimeSlider.value > 0.0 ? $"{this.wrecksDecayTimeSlider.value} minutes" : "No decay";
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
    SceneSingleton<CameraStateManager>.i.SetCameraPosition(this.overrideWrapper.Value.Value);
  }
}

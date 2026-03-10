// Decompiled with JetBrains decompiler
// Type: GameplayMenu
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;
using UnityEngine.UI;

#nullable disable
public class GameplayMenu : MonoBehaviour
{
  [SerializeField]
  private Dropdown unitSystemDropdown;
  [SerializeField]
  private Slider cockpitCameraInertiaSlider;
  [SerializeField]
  private Slider FoVSlider;
  [SerializeField]
  private Slider externalFoVSlider;
  [SerializeField]
  private Slider LandingCamSlider;
  [SerializeField]
  private Toggle zoomOnBoresightToggle;
  [SerializeField]
  private Text cockpitCameraInertiaLabel;
  [SerializeField]
  private Text FoVSliderLabel;
  [SerializeField]
  private Text externalFoVSliderLabel;
  [SerializeField]
  private Slider radialMenuControl;
  [SerializeField]
  private Toggle padlockToggle;
  [SerializeField]
  private Toggle tacScreenIRToggle;
  [SerializeField]
  private Toggle cameraAutoNVGToggle;
  [SerializeField]
  private Slider killFeedMunitionSlider;
  [SerializeField]
  private Slider killFeedAircraftSlider;
  [SerializeField]
  private Slider killFeedVehicleSlider;
  [SerializeField]
  private Slider killFeedBuildingSlider;
  [SerializeField]
  private Slider killFeedShipSlider;
  [SerializeField]
  private Slider killFeedMinValueSlider;
  [SerializeField]
  private Slider killFeedNbLinesSlider;
  [SerializeField]
  private Text killFeedMinValueText;
  [SerializeField]
  private Text killFeedNbLinesText;
  [SerializeField]
  private Toggle hitMarkersToggle;

  private void Start() => this.UpdateLabels();

  private void UpdateLabels()
  {
    this.unitSystemDropdown.SetValueWithoutNotify((int) PlayerSettings.unitSystem);
    this.cockpitCameraInertiaSlider.SetValueWithoutNotify(PlayerSettings.cockpitCamInertia);
    this.FoVSlider.SetValueWithoutNotify(PlayerSettings.defaultFoV);
    this.externalFoVSlider.SetValueWithoutNotify(PlayerSettings.defaultExternalFoV);
    this.zoomOnBoresightToggle.SetIsOnWithoutNotify(PlayerSettings.zoomOnBoresight);
    this.padlockToggle.SetIsOnWithoutNotify(PlayerSettings.padLockTarget);
    this.tacScreenIRToggle.SetIsOnWithoutNotify(PlayerSettings.tacScreenIR);
    this.cameraAutoNVGToggle.SetIsOnWithoutNotify(PlayerSettings.cameraAutoNVG);
    this.cockpitCameraInertiaLabel.text = $"{(ValueType) (float) ((double) this.cockpitCameraInertiaSlider.value * 100.0):F0}%";
    this.FoVSliderLabel.text = $"{this.FoVSlider.value:F0}°";
    this.externalFoVSliderLabel.text = $"{this.externalFoVSlider.value:F0}°";
    this.LandingCamSlider.SetValueWithoutNotify((float) PlayerSettings.landingCam);
    this.radialMenuControl.SetValueWithoutNotify((float) PlayerSettings.radialControl);
    this.killFeedNbLinesSlider.SetValueWithoutNotify((float) PlayerSettings.killFeedNbLines);
    this.killFeedMinValueSlider.SetValueWithoutNotify((float) (int) PlayerSettings.killFeedMinValue);
    this.killFeedMunitionSlider.SetValueWithoutNotify((float) PlayerSettings.killFeedMunition);
    this.killFeedAircraftSlider.SetValueWithoutNotify((float) PlayerSettings.killFeedAircraft);
    this.killFeedShipSlider.SetValueWithoutNotify((float) PlayerSettings.killFeedShip);
    this.killFeedBuildingSlider.SetValueWithoutNotify((float) PlayerSettings.killFeedBuilding);
    this.killFeedVehicleSlider.SetValueWithoutNotify((float) PlayerSettings.killFeedVehicle);
    this.killFeedMinValueText.text = UnitConverter.ValueReading(this.killFeedMinValueSlider.value);
    this.killFeedNbLinesText.text = $"{(ValueType) (float) (5.0 * (double) this.killFeedNbLinesSlider.value)}";
    this.hitMarkersToggle.SetIsOnWithoutNotify(PlayerSettings.showHitMarkers);
  }

  private void OnDestroy() => PlayerSettings.LoadPrefs();

  public void ApplySettings()
  {
    PlayerPrefs.SetInt("UnitSystem", this.unitSystemDropdown.value);
    PlayerPrefs.SetFloat("CockpitCamInertia", this.cockpitCameraInertiaSlider.value);
    this.cockpitCameraInertiaLabel.text = $"{(ValueType) (float) ((double) this.cockpitCameraInertiaSlider.value * 100.0):F0}%";
    PlayerPrefs.SetFloat("DefaultFoV", this.FoVSlider.value);
    this.FoVSliderLabel.text = $"{this.FoVSlider.value:F0}°";
    PlayerPrefs.SetFloat("DefaultExternalFoV", this.externalFoVSlider.value);
    this.externalFoVSliderLabel.text = $"{this.externalFoVSlider.value:F0}°";
    PlayerPrefs.SetInt("ZoomOnBoresight", this.zoomOnBoresightToggle.isOn ? 1 : 0);
    PlayerPrefs.SetInt("PadLockTarget", this.padlockToggle.isOn ? 1 : 0);
    PlayerPrefs.SetInt("TacScreenIR", this.tacScreenIRToggle.isOn ? 1 : 0);
    PlayerPrefs.SetInt("CameraAutoNVG", this.cameraAutoNVGToggle.isOn ? 1 : 0);
    PlayerPrefs.SetInt("LandingCam", (int) this.LandingCamSlider.value);
    PlayerPrefs.SetInt("RadialControl", (int) this.radialMenuControl.value);
    PlayerPrefs.SetInt("KillFeedMunition", (int) this.killFeedMunitionSlider.value);
    PlayerPrefs.SetInt("KillFeedAircraft", (int) this.killFeedAircraftSlider.value);
    PlayerPrefs.SetInt("KillFeedBuilding", (int) this.killFeedBuildingSlider.value);
    PlayerPrefs.SetInt("KillFeedShip", (int) this.killFeedShipSlider.value);
    PlayerPrefs.SetInt("KillFeedVehicle", (int) this.killFeedVehicleSlider.value);
    PlayerPrefs.SetFloat("KillFeedMinValue", this.killFeedMinValueSlider.value);
    this.killFeedMinValueText.text = UnitConverter.ValueReading(this.killFeedMinValueSlider.value);
    PlayerPrefs.SetInt("KillFeedNbLines", (int) this.killFeedNbLinesSlider.value);
    this.killFeedNbLinesText.text = $"{(ValueType) (float) (5.0 * (double) this.killFeedNbLinesSlider.value)}";
    PlayerPrefs.SetInt("ShowHitMarkers", this.hitMarkersToggle.isOn ? 1 : 0);
    if ((UnityEngine.Object) SceneSingleton<CameraStateManager>.i != (UnityEngine.Object) null)
    {
      CameraStateManager i = SceneSingleton<CameraStateManager>.i;
      float FOVTarget = i.currentState == i.cockpitState ? this.FoVSlider.value : this.externalFoVSlider.value;
      i.SetDesiredFoV(FOVTarget, 0.0f);
    }
    PlayerSettings.ApplyPrefs();
  }
}

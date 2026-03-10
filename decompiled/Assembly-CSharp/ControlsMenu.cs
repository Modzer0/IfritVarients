// Decompiled with JetBrains decompiler
// Type: ControlsMenu
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using UnityEngine;
using UnityEngine.UI;

#nullable disable
public class ControlsMenu : MonoBehaviour
{
  private GameObject parentMenu;
  public static ControlsMenu.BindType bindType;
  [SerializeField]
  private Toggle useVirtualJoystick;
  [SerializeField]
  private Toggle invertPitch;
  [SerializeField]
  private Toggle viewInvertPitch;
  [SerializeField]
  private Toggle throttleUseNegative;
  [SerializeField]
  private Toggle throttleUseRelative;
  [SerializeField]
  private Toggle controllerMenuNavigation;
  [SerializeField]
  private Toggle menuWeaponSafety;
  [SerializeField]
  private Toggle invertCollective;
  [SerializeField]
  private Slider sensitivitySlider;
  [SerializeField]
  private Slider centeringSlider;
  [SerializeField]
  private Text sensitivityLabel;
  [SerializeField]
  private Text centeringLabel;
  [SerializeField]
  private Slider viewSensitivitySlider;
  [SerializeField]
  private Text viewSensitivityLabel;
  [SerializeField]
  private Slider viewSmoothingSlider;
  [SerializeField]
  private Text viewSmoothingLabel;
  [SerializeField]
  private Toggle useTrackIR;
  [SerializeField]
  private Slider clickDelaySlider;
  [SerializeField]
  private Slider pressDelaySlider;
  [SerializeField]
  private Text clickDelayLabel;
  [SerializeField]
  private Text pressDelayLabel;
  private Button selectedTab;

  private void OnEnable()
  {
  }

  public void EditBindings()
  {
    if ((Object) SceneSingleton<GameplayUI>.i != (Object) null)
      SceneSingleton<GameplayUI>.i.menuCanvas.enabled = false;
    GameManager.controlMapper.Open();
  }

  private void Start()
  {
    PlayerSettings.LoadPrefs();
    this.InitializeStates();
  }

  public void SavePlayerSettings()
  {
    PlayerPrefs.SetInt("VirtualJoystickEnabled", this.useVirtualJoystick.isOn ? 1 : 0);
    PlayerPrefs.SetInt("VirtualJoystickInvertPitch", this.invertPitch.isOn ? 1 : 0);
    PlayerPrefs.SetInt("ViewInvertPitch", this.viewInvertPitch.isOn ? 1 : 0);
    PlayerPrefs.SetInt("ThrottleUseNegative", this.throttleUseNegative.isOn ? 1 : 0);
    PlayerPrefs.SetInt("ThrottleUseRelative", this.throttleUseRelative.isOn ? 1 : 0);
    PlayerPrefs.SetInt("ControllerMenuNavigation", this.controllerMenuNavigation.isOn ? 1 : 0);
    PlayerPrefs.SetInt("MenuWeaponSafety", this.menuWeaponSafety.isOn ? 1 : 0);
    PlayerPrefs.SetInt("InvertCollective", this.invertCollective.isOn ? 1 : 0);
    PlayerPrefs.SetFloat("VirtualJoystickSensitivity", this.sensitivitySlider.value);
    PlayerPrefs.SetFloat("VirtualJoystickCentering", this.centeringSlider.value);
    PlayerPrefs.SetFloat("ViewSensitivity", this.viewSensitivitySlider.value);
    PlayerPrefs.SetFloat("ViewSmoothing", this.viewSmoothingSlider.value);
    PlayerPrefs.SetFloat("PressDelay", this.pressDelaySlider.value);
    PlayerPrefs.SetFloat("ClickDelay", this.clickDelaySlider.value);
    PlayerPrefs.SetInt("UseTrackIR", this.useTrackIR.isOn ? 1 : 0);
    PlayerSettings.LoadPrefs();
    PlayerSettings.ApplyPrefs();
  }

  public void ResetSlidersToggles()
  {
    PlayerPrefs.SetInt("VirtualJoystickEnabled", 0);
    PlayerPrefs.SetInt("VirtualJoystickInvertPitch", 0);
    PlayerPrefs.SetInt("ViewInvertPitch", 0);
    PlayerPrefs.SetInt("ThrottleUseNegative", 1);
    PlayerPrefs.SetInt("ThrottleUseRelative", 0);
    PlayerPrefs.SetInt("ControllerMenuNavigation", 1);
    PlayerPrefs.SetInt("MenuWeaponSafety", 1);
    PlayerPrefs.SetInt("InvertCollective", 0);
    PlayerPrefs.SetFloat("VirtualJoystickSensitivity", 0.25f);
    PlayerPrefs.SetFloat("VirtualJoystickCentering", 0.0f);
    PlayerPrefs.SetFloat("ViewSensitivity", 0.5f);
    PlayerPrefs.SetFloat("ViewSmoothing", 0.5f);
    PlayerPrefs.SetFloat("PressDelay", 0.2f);
    PlayerPrefs.SetFloat("ClickDelay", 0.1f);
    PlayerSettings.LoadPrefs();
    this.InitializeStates();
  }

  private void InitializeStates()
  {
    this.useVirtualJoystick.SetIsOnWithoutNotify(PlayerSettings.virtualJoystickEnabled);
    this.invertPitch.SetIsOnWithoutNotify(PlayerSettings.virtualJoystickInvertPitch);
    this.viewInvertPitch.SetIsOnWithoutNotify(PlayerSettings.viewInvertPitch);
    this.throttleUseNegative.SetIsOnWithoutNotify(PlayerSettings.throttleUseNegative);
    this.throttleUseRelative.SetIsOnWithoutNotify(PlayerSettings.throttleUseRelative);
    this.controllerMenuNavigation.SetIsOnWithoutNotify(PlayerSettings.controllerMenuNavigation);
    this.menuWeaponSafety.SetIsOnWithoutNotify(PlayerSettings.menuWeaponSafety);
    this.invertCollective.SetIsOnWithoutNotify(PlayerSettings.invertCollective);
    this.sensitivitySlider.SetValueWithoutNotify(PlayerSettings.virtualJoystickSensitivity);
    this.centeringSlider.SetValueWithoutNotify(PlayerSettings.virtualJoystickCentering);
    this.viewSensitivitySlider.SetValueWithoutNotify(PlayerSettings.viewSensitivity);
    this.viewSmoothingSlider.SetValueWithoutNotify(PlayerSettings.viewSmoothing);
    this.pressDelaySlider.SetValueWithoutNotify(PlayerSettings.pressDelay);
    this.clickDelaySlider.SetValueWithoutNotify(PlayerSettings.clickDelay);
    this.useTrackIR.SetIsOnWithoutNotify(PlayerSettings.useTrackIR);
    this.UpdateLabels();
  }

  public void UpdateLabels()
  {
    this.sensitivityLabel.text = $"Virtual Joystick Sensitivity ({this.sensitivitySlider.value.ToString("F1")})";
    this.centeringLabel.text = $"Virtual Joystick Centering Force ({this.centeringSlider.value.ToString("F1")})";
    this.viewSensitivityLabel.text = $"View Motion Sensitivity ({this.viewSensitivitySlider.value.ToString("F1")})";
    this.viewSmoothingLabel.text = $"View Smoothing ({this.viewSmoothingSlider.value.ToString("F1")})";
    this.clickDelayLabel.text = $"Button Click Delay ({this.clickDelaySlider.value.ToString("F2")})";
    this.pressDelayLabel.text = $"Button Hold Delay ({this.pressDelaySlider.value.ToString("F2")})";
  }

  public void OnDestroy() => PlayerSettings.LoadPrefs();

  public void CloseControlsMenu()
  {
    this.parentMenu.SetActive(true);
    Object.Destroy((Object) this.gameObject);
    SceneSingleton<GameplayUI>.i.gameplayCanvas.enabled = true;
  }

  private void Update()
  {
  }

  public enum BindType
  {
    Keyboard,
    Button,
    Axis,
  }
}

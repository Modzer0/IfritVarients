// Decompiled with JetBrains decompiler
// Type: HUDSettings
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;
using UnityEngine.UI;

#nullable disable
public class HUDSettings : MonoBehaviour
{
  [SerializeField]
  private Toggle lagPipToggle;
  [SerializeField]
  private Text lagPipLabel;
  [SerializeField]
  private Toggle rangeCircleToggle;
  [SerializeField]
  private Text rangeCircleLabel;
  [SerializeField]
  private Slider HUDTimeSlider;
  [SerializeField]
  private Toggle gaugesToggle;
  [SerializeField]
  private Text gaugesLabel;
  [SerializeField]
  private Toggle HUDWeaponsToggle;
  [SerializeField]
  private Text HUDWeaponsLabel;
  [SerializeField]
  private Slider HMDWidthSlider;
  [SerializeField]
  private Text HMDWidthLabel;
  [SerializeField]
  private Slider HMDHeightSlider;
  [SerializeField]
  private Text HMDHeightLabel;
  [SerializeField]
  private Slider HMDSideDistSlider;
  [SerializeField]
  private Text HMDSideDistLabel;
  [SerializeField]
  private Slider HMDSideAngleSlider;
  [SerializeField]
  private Text HMDSideAngleLabel;
  [SerializeField]
  private Slider HMDTopHeightSlider;
  [SerializeField]
  private Text HMDTopHeightLabel;
  [SerializeField]
  private Slider HMDHideDistSlider;
  [SerializeField]
  private Text HMDHideDistLabel;
  [SerializeField]
  private Slider HMDIconSizeSlider;
  [SerializeField]
  private Text HMDIconSizeLabel;
  [SerializeField]
  private Slider HUDTextSizeSlider;
  [SerializeField]
  private Text HUDTextSizeLabel;
  [SerializeField]
  private Slider HMDTextSizeSlider;
  [SerializeField]
  private Text HMDTextSizeLabel;
  [SerializeField]
  private Slider OverlayTextSizeSlider;
  [SerializeField]
  private Text OverlayTextSizeLabel;
  [SerializeField]
  private Slider ColorRSlider;
  [SerializeField]
  private Text ColorRLabel;
  [SerializeField]
  private Slider ColorGSlider;
  [SerializeField]
  private Text ColorGLabel;
  [SerializeField]
  private Slider ColorBSlider;
  [SerializeField]
  private Text ColorBLabel;
  [SerializeField]
  private Image colorExample;

  private void Start()
  {
    this.HMDWidthSlider.maxValue = (float) Screen.width / (float) Screen.height * 1080f;
    this.HMDHeightSlider.maxValue = 1080f;
    this.UpdateLabels();
  }

  private void UpdateLabels()
  {
    this.lagPipToggle.SetIsOnWithoutNotify(PlayerSettings.lagPip);
    this.lagPipLabel.text = this.lagPipToggle.isOn ? "Lag" : "Lead";
    this.rangeCircleToggle.SetIsOnWithoutNotify(PlayerSettings.rangeCircle);
    this.rangeCircleLabel.text = this.rangeCircleToggle.isOn ? "Circle" : "Ladder";
    this.HUDTimeSlider.SetValueWithoutNotify((float) PlayerSettings.hudTime);
    this.gaugesToggle.SetIsOnWithoutNotify(PlayerSettings.gauges);
    this.gaugesLabel.text = this.gaugesToggle.isOn ? "Show" : "Hide";
    this.HUDWeaponsToggle.SetIsOnWithoutNotify(PlayerSettings.hudWeapons);
    this.HUDWeaponsLabel.text = this.HUDWeaponsToggle.isOn ? "Show" : "Hide";
    this.HMDWidthSlider.SetValueWithoutNotify(PlayerSettings.hmdWidth);
    this.HMDWidthLabel.text = $"{this.HMDWidthSlider.value:F0}px";
    this.HMDHeightSlider.SetValueWithoutNotify(PlayerSettings.hmdHeight);
    this.HMDHeightLabel.text = $"{this.HMDHeightSlider.value:F0}px";
    this.HMDSideDistSlider.SetValueWithoutNotify(PlayerSettings.hmdSideDist);
    this.HMDSideDistLabel.text = $"{this.HMDSideDistSlider.value:F0}px";
    this.HMDSideAngleSlider.SetValueWithoutNotify(PlayerSettings.hmdSideAngle);
    this.HMDSideAngleLabel.text = $"{this.HMDSideAngleSlider.value:F0}°";
    this.HMDTopHeightSlider.SetValueWithoutNotify(PlayerSettings.hmdTopHeight);
    this.HMDTopHeightLabel.text = $"{this.HMDTopHeightSlider.value:F0}px";
    this.HMDHideDistSlider.SetValueWithoutNotify(PlayerSettings.hmdHideDist);
    this.HMDHideDistLabel.text = $"{(ValueType) (float) (100.0 * (double) this.HMDHideDistSlider.value):F0}%";
    this.HMDIconSizeSlider.SetValueWithoutNotify(PlayerSettings.hmdIconSize);
    this.HMDIconSizeLabel.text = $"{this.HMDIconSizeSlider.value:F0}";
    this.HUDTextSizeSlider.SetValueWithoutNotify(PlayerSettings.hudTextSize);
    this.HUDTextSizeLabel.text = $"{this.HUDTextSizeSlider.value:F0}";
    this.HMDTextSizeSlider.SetValueWithoutNotify(PlayerSettings.hmdTextSize);
    this.HMDTextSizeLabel.text = $"{this.HMDTextSizeSlider.value:F0}";
    this.ColorRSlider.SetValueWithoutNotify((float) PlayerSettings.hudColorR);
    this.ColorRLabel.text = $"R {this.ColorRSlider.value:F0}";
    this.ColorGSlider.SetValueWithoutNotify((float) PlayerSettings.hudColorG);
    this.ColorGLabel.text = $"R {this.ColorGSlider.value:F0}";
    this.ColorBSlider.SetValueWithoutNotify((float) PlayerSettings.hudColorB);
    this.ColorBLabel.text = $"R {this.ColorBSlider.value:F0}";
    this.colorExample.color = new Color(this.ColorRSlider.value, this.ColorGSlider.value, this.ColorBSlider.value);
    this.OverlayTextSizeSlider.SetValueWithoutNotify(PlayerSettings.overlayTextSize);
    this.OverlayTextSizeLabel.text = $"{this.OverlayTextSizeSlider.value:F0}";
  }

  private void OnDestroy()
  {
    PlayerSettings.ApplyPrefs();
    if (!((UnityEngine.Object) SceneSingleton<HUDOptions>.i != (UnityEngine.Object) null))
      return;
    SceneSingleton<HUDOptions>.i.ApplyHUDSettings();
  }

  public void ApplySettings()
  {
    PlayerPrefs.SetInt("LagPip", this.lagPipToggle.isOn ? 1 : 0);
    this.lagPipLabel.text = this.lagPipToggle.isOn ? "Lag" : "Lead";
    PlayerPrefs.SetInt("RangeCircle", this.rangeCircleToggle.isOn ? 1 : 0);
    this.rangeCircleLabel.text = this.rangeCircleToggle.isOn ? "Circle" : "Ladder";
    PlayerPrefs.SetInt("HUDTime", (int) this.HUDTimeSlider.value);
    PlayerPrefs.SetInt("Gauges", this.gaugesToggle.isOn ? 1 : 0);
    this.gaugesLabel.text = this.gaugesToggle.isOn ? "Show" : "Hide";
    PlayerPrefs.SetInt("HUDWeapons", this.HUDWeaponsToggle.isOn ? 1 : 0);
    this.HUDWeaponsLabel.text = this.HUDWeaponsToggle.isOn ? "Show" : "Hide";
    PlayerPrefs.SetFloat("HMDWidth", 10f * (float) Mathf.RoundToInt(this.HMDWidthSlider.value / 10f));
    this.HMDWidthLabel.text = $"{(ValueType) (float) (10.0 * (double) Mathf.RoundToInt(this.HMDWidthSlider.value / 10f)):F0}px";
    PlayerPrefs.SetFloat("HMDHeight", 10f * (float) Mathf.RoundToInt(this.HMDHeightSlider.value / 10f));
    this.HMDHeightLabel.text = $"{(ValueType) (float) (10.0 * (double) Mathf.RoundToInt(this.HMDHeightSlider.value / 10f)):F0}px";
    this.HMDSideDistSlider.value = Mathf.Clamp(this.HMDSideDistSlider.value, 100f, 5f * (float) Mathf.RoundToInt(this.HMDWidthSlider.value / 10f));
    this.HMDSideDistSlider.value = 5f * (float) Mathf.RoundToInt(this.HMDSideDistSlider.value / 5f);
    PlayerPrefs.SetFloat("HMDSideDist", this.HMDSideDistSlider.value);
    this.HMDSideDistLabel.text = $"{this.HMDSideDistSlider.value:F0}px";
    PlayerPrefs.SetFloat("HMDSideAngle", this.HMDSideAngleSlider.value);
    this.HMDSideAngleLabel.text = $"{this.HMDSideAngleSlider.value:F0}°";
    this.HMDTopHeightSlider.value = Mathf.Clamp(this.HMDTopHeightSlider.value, -5f * (float) Mathf.RoundToInt(this.HMDHeightSlider.value / 10f), 5f * (float) Mathf.RoundToInt(this.HMDHeightSlider.value / 10f));
    this.HMDTopHeightSlider.value = 5f * (float) Mathf.RoundToInt(this.HMDTopHeightSlider.value / 5f);
    PlayerPrefs.SetFloat("HMDTopHeight", this.HMDTopHeightSlider.value);
    this.HMDTopHeightLabel.text = $"{this.HMDTopHeightSlider.value:F0}px";
    this.HMDHideDistSlider.value = Mathf.Max(this.HMDHideDistSlider.value, 0.2f);
    PlayerPrefs.SetFloat("HMDHideDist", this.HMDHideDistSlider.value);
    this.HMDHideDistLabel.text = $"{(ValueType) (float) (100.0 * (double) this.HMDHideDistSlider.value):F0}%";
    this.HMDIconSizeSlider.value = 5f * (float) Mathf.RoundToInt(this.HMDIconSizeSlider.value / 5f);
    PlayerPrefs.SetFloat("HMDIconSize", this.HMDIconSizeSlider.value);
    this.HMDIconSizeLabel.text = $"{this.HMDIconSizeSlider.value:F0}";
    this.HUDTextSizeSlider.value = 2f * (float) Mathf.RoundToInt(this.HUDTextSizeSlider.value / 2f);
    PlayerPrefs.SetFloat("HUDTextSize", this.HUDTextSizeSlider.value);
    this.HUDTextSizeLabel.text = $"{this.HUDTextSizeSlider.value:F0}";
    this.HMDTextSizeSlider.value = 2f * (float) Mathf.RoundToInt(this.HMDTextSizeSlider.value / 2f);
    PlayerPrefs.SetFloat("HMDTextSize", this.HMDTextSizeSlider.value);
    this.HMDTextSizeLabel.text = $"{this.HMDTextSizeSlider.value:F0}";
    this.OverlayTextSizeSlider.value = 2f * (float) Mathf.RoundToInt(this.OverlayTextSizeSlider.value / 2f);
    PlayerPrefs.SetFloat("OverlayTextSize", this.OverlayTextSizeSlider.value);
    this.OverlayTextSizeLabel.text = $"{this.OverlayTextSizeSlider.value:F0}";
    PlayerPrefs.SetInt("HUDColorR", 0);
    PlayerPrefs.SetInt("HUDColorG", (int) byte.MaxValue);
    PlayerPrefs.SetInt("HUDColorB", 0);
    PlayerSettings.LoadPrefs();
  }
}

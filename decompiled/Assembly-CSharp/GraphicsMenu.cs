// Decompiled with JetBrains decompiler
// Type: GraphicsMenu
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

#nullable disable
public class GraphicsMenu : MonoBehaviour
{
  [SerializeField]
  private Toggle windowedToggle;
  [SerializeField]
  private Toggle debugVisToggle;
  [SerializeField]
  private Toggle cinematicModeToggle;
  [SerializeField]
  private Toggle vsyncToggle;
  [SerializeField]
  private Slider cloudsDetail;
  [SerializeField]
  private Text cloudsDetailValue;
  [SerializeField]
  private Dropdown mipmapLevelDropdown;
  [SerializeField]
  private Dropdown shadowDetailDropdown;
  [SerializeField]
  private Dropdown antiAliasingDropdown;

  private void Start()
  {
    this.windowedToggle.SetIsOnWithoutNotify(Screen.fullScreenMode == FullScreenMode.Windowed);
    this.debugVisToggle.SetIsOnWithoutNotify(PlayerPrefs.HasKey("DebugVis") && PlayerPrefs.GetInt("DebugVis") == 1);
    this.cloudsDetail.SetValueWithoutNotify(PlayerPrefs.HasKey("CloudDetail") ? PlayerPrefs.GetFloat("CloudDetail") : 1f);
    this.cloudsDetailValue.text = $"{(ValueType) (float) ((double) this.cloudsDetail.value * 100.0):F0}%";
    this.cinematicModeToggle.SetIsOnWithoutNotify(PlayerSettings.cinematicMode);
    this.mipmapLevelDropdown.SetValueWithoutNotify(PlayerSettings.mipmapLevel);
    this.mipmapLevelDropdown.onValueChanged.AddListener(new UnityAction<int>(this.OnMipmapLevelChanged));
    this.shadowDetailDropdown.SetValueWithoutNotify(PlayerSettings.shadowQuality);
    this.shadowDetailDropdown.onValueChanged.AddListener(new UnityAction<int>(this.OnShadowDetailChanged));
    this.antiAliasingDropdown.SetValueWithoutNotify(PlayerSettings.antiAliasing);
    this.antiAliasingDropdown.onValueChanged.AddListener(new UnityAction<int>(this.OnAntiAliasingChanged));
    this.vsyncToggle.SetIsOnWithoutNotify(PlayerSettings.vsync);
    this.vsyncToggle.onValueChanged.AddListener(new UnityAction<bool>(this.OnVsyncChanged));
  }

  private void OnVsyncChanged(bool isOn)
  {
    QualitySettings.vSyncCount = isOn ? 1 : 0;
    PlayerPrefs.SetInt("Vsync", isOn ? 1 : 0);
  }

  private void OnMipmapLevelChanged(int value)
  {
    QualitySettings.globalTextureMipmapLimit = this.mipmapLevelDropdown.value;
    PlayerPrefs.SetInt("MipmapLevel", this.mipmapLevelDropdown.value);
  }

  private void OnShadowDetailChanged(int value)
  {
    PlayerPrefs.SetInt("ShadowQuality", Mathf.Max(this.shadowDetailDropdown.value, 0));
    UnityEngine.Rendering.Universal.ShadowResolution shadowResolution = UnityEngine.Rendering.Universal.ShadowResolution._512;
    if (this.shadowDetailDropdown.value == 2)
      shadowResolution = UnityEngine.Rendering.Universal.ShadowResolution._1024;
    if (this.shadowDetailDropdown.value == 3)
      shadowResolution = UnityEngine.Rendering.Universal.ShadowResolution._2048;
    if (this.shadowDetailDropdown.value == 4)
      shadowResolution = UnityEngine.Rendering.Universal.ShadowResolution._4096;
    UnityGraphicsBullshit.MainLightCastShadows = this.shadowDetailDropdown.value > 0;
    UnityGraphicsBullshit.MainLightShadowResolution = shadowResolution;
  }

  private void OnAntiAliasingChanged(int value)
  {
    QualitySettings.antiAliasing = this.antiAliasingDropdown.value;
    PlayerPrefs.SetInt("AntiAliasing", this.antiAliasingDropdown.value);
  }

  public void SetWindowed()
  {
    if (this.windowedToggle.isOn)
      Screen.fullScreenMode = FullScreenMode.Windowed;
    if (this.windowedToggle.isOn)
      return;
    Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
    Screen.fullScreen = true;
  }

  public void UpdateToggles()
  {
    PlayerPrefs.SetInt("DebugVis", this.debugVisToggle.isOn ? 1 : 0);
    PlayerSettings.debugVis = this.debugVisToggle.isOn;
    PlayerPrefs.SetFloat("CloudDetail", this.cloudsDetail.value);
    PlayerSettings.cloudDetail = this.cloudsDetail.value;
    PlayerPrefs.SetInt("MipmapLevel", this.mipmapLevelDropdown.value);
    this.cloudsDetailValue.text = $"{(ValueType) (float) ((double) this.cloudsDetail.value * 100.0):F0}%";
    PlayerSettings.cinematicMode = this.cinematicModeToggle.isOn;
    PlayerSettings.LoadPrefs();
    PlayerSettings.ApplyPrefs();
  }
}

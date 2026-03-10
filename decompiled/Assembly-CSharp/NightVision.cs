// Decompiled with JetBrains decompiler
// Type: NightVision
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.MissionEditorScripts;
using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

#nullable disable
public class NightVision : MonoBehaviour
{
  [SerializeField]
  private Volume postProcessing;
  public static NightVision i;
  private bool nightVisSelected;
  private bool nightVisActive;
  [SerializeField]
  private float gainMin;
  [SerializeField]
  private float gainMax;
  [SerializeField]
  private float bloomThresholdMin;
  [SerializeField]
  private float bloomThresholdMax;
  private ColorAdjustments colorAdjustments;
  private Bloom bloom;
  private float gainLastUpdated;

  private void Awake() => NightVision.i = this;

  private void Start()
  {
    SceneSingleton<CameraStateManager>.i.onSwitchCamera += new Action(this.NightVis_OnSwitchCam);
    if (!this.postProcessing.profile.TryGet<ColorAdjustments>(out this.colorAdjustments))
      throw new NullReferenceException("colorAdjustments");
    if (!this.postProcessing.profile.TryGet<Bloom>(out this.bloom))
      throw new NullReferenceException("bloom");
  }

  private void NightVis_OnSwitchCam()
  {
    if (!PlayerSettings.cameraAutoNVG || (double) NetworkSceneSingleton<LevelInfo>.i.timeOfDay >= 5.5999999046325684 && (double) NetworkSceneSingleton<LevelInfo>.i.timeOfDay <= 18.399999618530273)
      return;
    this.nightVisSelected = true;
  }

  private void UpdateGain()
  {
    if ((double) Time.unscaledTime - (double) this.gainLastUpdated < 1.0)
      return;
    this.gainLastUpdated = Time.unscaledTime;
    float t = Mathf.InverseLerp(0.01f, 0.4f, NetworkSceneSingleton<LevelInfo>.i.GetAmbientLight());
    float num1 = Mathf.Lerp(this.gainMax, this.gainMin, t);
    float num2 = Mathf.Lerp(this.bloomThresholdMin, this.bloomThresholdMax, t);
    this.colorAdjustments.postExposure.value = num1;
    this.bloom.threshold.value = num2;
  }

  private bool BlockToggle()
  {
    if (InputFieldChecker.InsideInputField)
      return true;
    CursorFlags flags = CursorManager.GetFlags();
    return GameManager.gameState == GameState.Editor ? CursorManager.GetFlags() != CursorFlags.NotInGame : flags != 0;
  }

  public static void Toggle()
  {
    if ((UnityEngine.Object) NightVision.i == (UnityEngine.Object) null || NightVision.i.BlockToggle())
      return;
    NightVision.i.nightVisSelected = !NightVision.i.nightVisSelected;
  }

  private void Update()
  {
    if (this.BlockToggle())
      return;
    if (this.nightVisSelected)
    {
      if (!this.nightVisActive)
      {
        this.nightVisActive = true;
        this.postProcessing.enabled = true;
        NetworkSceneSingleton<LevelInfo>.i.PostProcessing.enabled = false;
      }
    }
    else if (this.nightVisActive)
    {
      this.nightVisActive = false;
      this.postProcessing.enabled = false;
      NetworkSceneSingleton<LevelInfo>.i.PostProcessing.enabled = true;
    }
    if (this.nightVisActive)
      this.UpdateGain();
    if (!GameManager.playerInput.GetButtonDown("Night Vis"))
      return;
    NightVision.Toggle();
  }
}

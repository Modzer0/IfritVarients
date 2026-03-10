// Decompiled with JetBrains decompiler
// Type: FlyByWireTuner
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

#nullable disable
public class FlyByWireTuner : MonoBehaviour
{
  private Aircraft aircraft;
  private ControlsFilter controlsFilter;
  [SerializeField]
  private Toggle fbwEnabled;
  [SerializeField]
  private TMP_InputField directControlFactor;
  [SerializeField]
  private TMP_InputField angVel;
  [SerializeField]
  private TMP_InputField cornerSpeed;
  [SerializeField]
  private TMP_InputField psmSpeed;
  [SerializeField]
  private TMP_InputField slowFast;
  [SerializeField]
  private TMP_InputField pitchAdjustLimitSlow;
  [SerializeField]
  private TMP_InputField pFactorSlow;
  [SerializeField]
  private TMP_InputField dFactorSlow;
  [SerializeField]
  private TMP_InputField pitchAdjustLimitFast;
  [SerializeField]
  private TMP_InputField pFactorFast;
  [SerializeField]
  private TMP_InputField dFactorFast;
  [SerializeField]
  private TMP_InputField rollTrimRate;
  [SerializeField]
  private TMP_InputField rollTrimLimit;
  [SerializeField]
  private TMP_InputField yawTightness;
  [SerializeField]
  private TMP_InputField rollTightness;

  private void Awake()
  {
    this.gameObject.SetActive(false);
    if (!PlayerSettings.debugVis)
      return;
    this.StartSlowUpdateDelayed(1f, new Action(this.CheckAircraft));
  }

  private void CheckAircraft()
  {
    if (!((UnityEngine.Object) SceneSingleton<CombatHUD>.i.aircraft != (UnityEngine.Object) null) || !((UnityEngine.Object) SceneSingleton<CombatHUD>.i.aircraft != (UnityEngine.Object) this.aircraft))
      return;
    this.aircraft = SceneSingleton<CombatHUD>.i.aircraft;
    this.controlsFilter = this.aircraft.GetControlsFilter();
    if (this.controlsFilter is HeloControlsFilter)
    {
      this.gameObject.SetActive(false);
    }
    else
    {
      this.gameObject.SetActive(true);
      this.GetParameters();
    }
  }

  public void GetParameters()
  {
    if ((UnityEngine.Object) this.controlsFilter == (UnityEngine.Object) null)
      return;
    (bool, float[]) byWireParameters = this.controlsFilter.GetFlyByWireParameters();
    this.fbwEnabled.SetIsOnWithoutNotify(byWireParameters.Item1);
    float[] numArray = byWireParameters.Item2;
    this.directControlFactor.text = $"{numArray[0]:F2}";
    this.angVel.text = $"{numArray[1]:F2}";
    this.cornerSpeed.text = $"{numArray[2]:F0}";
    this.psmSpeed.text = $"{numArray[3]:F0}";
    this.slowFast.text = $"{numArray[4]:F0}";
    this.pitchAdjustLimitSlow.text = $"{numArray[5]:F2}";
    this.pFactorSlow.text = $"{numArray[6]:F3}";
    this.dFactorSlow.text = $"{numArray[7]:F3}";
    this.pitchAdjustLimitFast.text = $"{numArray[8]:F2}";
    this.pFactorFast.text = $"{numArray[9]:F3}";
    this.dFactorFast.text = $"{numArray[10]:F3}";
    this.rollTrimRate.text = $"{numArray[11]:F2}";
    this.rollTrimLimit.text = $"{numArray[12]:F2}";
    this.yawTightness.text = $"{numArray[13]:F2}";
    this.rollTightness.text = $"{numArray[14]:F2}";
  }

  public void SetParameters()
  {
    if ((UnityEngine.Object) this.controlsFilter == (UnityEngine.Object) null)
      return;
    this.controlsFilter.SetFlyByWireParameters(this.fbwEnabled.isOn, new float[15]
    {
      float.Parse(this.directControlFactor.text),
      float.Parse(this.angVel.text),
      float.Parse(this.cornerSpeed.text),
      float.Parse(this.psmSpeed.text),
      float.Parse(this.slowFast.text),
      float.Parse(this.pitchAdjustLimitSlow.text),
      float.Parse(this.pFactorSlow.text),
      float.Parse(this.dFactorSlow.text),
      float.Parse(this.pitchAdjustLimitFast.text),
      float.Parse(this.pFactorFast.text),
      float.Parse(this.dFactorFast.text),
      float.Parse(this.rollTrimRate.text),
      float.Parse(this.rollTrimLimit.text),
      float.Parse(this.yawTightness.text),
      float.Parse(this.rollTightness.text)
    });
  }
}

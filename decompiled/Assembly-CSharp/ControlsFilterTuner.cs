// Decompiled with JetBrains decompiler
// Type: ControlsFilterTuner
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using UnityEngine;
using UnityEngine.UI;

#nullable disable
public class ControlsFilterTuner : MonoBehaviour
{
  [SerializeField]
  private Slider basePitchDampingSlider;
  [SerializeField]
  private Slider velocityPitchDampingSlider;
  [SerializeField]
  private Slider maxResponseAirspeedSlider;
  [SerializeField]
  private Slider maxResponseRateSlider;
  [SerializeField]
  private Slider pitchTrimRateSlider;
  [SerializeField]
  private Slider pitchTrimLimitSlider;
  [SerializeField]
  private Slider gearTrimSlider;
  [SerializeField]
  private Text basePitchDampingValue;
  [SerializeField]
  private Text velocityPitchDampingValue;
  [SerializeField]
  private Text maxResponseAirspeedValue;
  [SerializeField]
  private Text maxResponseRateValue;
  [SerializeField]
  private Text pitchTrimRateValue;
  [SerializeField]
  private Text pitchTrimLimitValue;
  [SerializeField]
  private Text gearTrimValue;
  private ControlsFilter controlsFilter;

  public void SetControlsFilter(
    ControlsFilter filter,
    float basePitchDamping,
    float velocityPitchDamping,
    float maxResponseAirspeed,
    float maxResponseRate,
    float pitchTrimRate,
    float pitchTrimLimit,
    float gearTrim)
  {
    this.controlsFilter = filter;
    this.basePitchDampingSlider.SetValueWithoutNotify(basePitchDamping);
    this.velocityPitchDampingSlider.SetValueWithoutNotify(velocityPitchDamping);
    this.maxResponseAirspeedSlider.SetValueWithoutNotify(maxResponseAirspeed);
    this.maxResponseRateSlider.SetValueWithoutNotify(maxResponseRate);
    this.pitchTrimRateSlider.SetValueWithoutNotify(pitchTrimRate);
    this.pitchTrimLimitSlider.SetValueWithoutNotify(pitchTrimLimit);
    this.gearTrimSlider.SetValueWithoutNotify(gearTrim);
    this.ApplyValues();
  }

  public void ApplyValues()
  {
    int num = (Object) this.controlsFilter != (Object) null ? 1 : 0;
    this.basePitchDampingValue.text = this.basePitchDampingSlider.value.ToString("F2");
    this.velocityPitchDampingValue.text = this.velocityPitchDampingSlider.value.ToString("F3");
    this.maxResponseAirspeedValue.text = this.maxResponseAirspeedSlider.value.ToString("F0");
    this.maxResponseRateValue.text = this.maxResponseRateSlider.value.ToString("F2");
    this.pitchTrimRateValue.text = this.pitchTrimRateSlider.value.ToString("F2");
    this.pitchTrimLimitValue.text = this.pitchTrimLimitSlider.value.ToString("F2");
    this.gearTrimValue.text = this.gearTrimSlider.value.ToString("F2");
  }
}

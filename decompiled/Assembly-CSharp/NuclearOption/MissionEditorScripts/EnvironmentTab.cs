// Decompiled with JetBrains decompiler
// Type: NuclearOption.MissionEditorScripts.EnvironmentTab
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.SavedMission;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

#nullable disable
namespace NuclearOption.MissionEditorScripts;

public class EnvironmentTab : MonoBehaviour, IMissionTab
{
  [SerializeField]
  private Slider timeSlider;
  [SerializeField]
  private Slider timeFactorSlider;
  [SerializeField]
  private Slider conditionsSlider;
  [SerializeField]
  private Slider cloudHeightSlider;
  [SerializeField]
  private Slider windSpeedSlider;
  [SerializeField]
  private Slider windTurbulenceSlider;
  [SerializeField]
  private Slider windRandomDirectionSlider;
  [SerializeField]
  private Slider moonPhaseSlider;
  [SerializeField]
  private RadialSlider windDirectionRadialSlider;
  [SerializeField]
  private TextMeshProUGUI timeLabel;
  [SerializeField]
  private TextMeshProUGUI timeFactorLabel;
  [SerializeField]
  private TextMeshProUGUI conditionsLabel;
  [SerializeField]
  private TextMeshProUGUI cloudHeightLabel;
  [SerializeField]
  private TextMeshProUGUI windSpeedLabel;
  [SerializeField]
  private TextMeshProUGUI windTurbulenceLabel;
  [SerializeField]
  private TextMeshProUGUI windHeadingLabel;
  [SerializeField]
  private TextMeshProUGUI windRandomHeadingLabel;
  [SerializeField]
  private TextMeshProUGUI moonPhaseLabel;
  [SerializeField]
  private WeatherSet[] weatherSets;
  [SerializeField]
  private Transform windArrow;
  [SerializeField]
  private Image windRandomArc;
  [SerializeField]
  private Image moonPhaseImage;
  private MissionEnvironment environment;

  public void SetMission(Mission mission)
  {
    this.environment = mission.environment;
    this.timeSlider.SetValueWithoutNotify(this.environment.timeOfDay / 24f);
    this.timeFactorSlider.SetValueWithoutNotify(this.environment.timeFactor);
    this.conditionsSlider.SetValueWithoutNotify(this.environment.weatherIntensity);
    this.cloudHeightSlider.SetValueWithoutNotify(Mathf.Max(this.environment.cloudAltitude, 500f));
    this.windSpeedSlider.SetValueWithoutNotify(this.environment.windSpeed);
    this.windTurbulenceSlider.SetValueWithoutNotify(this.environment.windTurbulence);
    this.windDirectionRadialSlider.SetValue(this.environment.windHeading);
    this.windRandomDirectionSlider.SetValueWithoutNotify(this.environment.windRandomHeading);
    this.moonPhaseSlider.SetValueWithoutNotify(this.environment.moonPhase);
    this.UpdateLabels();
  }

  public void ValuesChanged()
  {
    this.environment.timeOfDay = this.timeSlider.value * 24f;
    this.environment.timeFactor = this.timeFactorSlider.value;
    this.environment.weatherIntensity = this.conditionsSlider.value;
    this.environment.cloudAltitude = this.cloudHeightSlider.value;
    this.environment.windSpeed = this.windSpeedSlider.value;
    this.environment.windTurbulence = this.windTurbulenceSlider.value;
    this.environment.windHeading = this.windDirectionRadialSlider.value;
    this.environment.windRandomHeading = this.windRandomDirectionSlider.value;
    this.environment.moonPhase = this.moonPhaseSlider.value;
    if ((UnityEngine.Object) NetworkSceneSingleton<LevelInfo>.i != (UnityEngine.Object) null)
      NetworkSceneSingleton<LevelInfo>.i.LoadEnvironment(this.environment);
    this.UpdateLabels();
  }

  public void UpdateLabels()
  {
    this.timeLabel.text = UnitConverter.TimeOfDay(this.timeSlider.value * 24f, false);
    float num1 = 1f;
    float num2 = this.timeFactorSlider.value;
    if ((double) num2 <= 0.0)
    {
      if ((double) num2 != -2.0)
      {
        if ((double) num2 != -1.0)
        {
          if ((double) num2 == 0.0)
            num1 = 1f;
        }
        else
          num1 = 0.5f;
      }
      else
        num1 = 0.0f;
    }
    else if ((double) num2 != 1.0)
    {
      if ((double) num2 != 2.0)
      {
        if ((double) num2 == 3.0)
          num1 = 60f;
      }
      else
        num1 = 30f;
    }
    else
      num1 = 10f;
    this.timeFactorLabel.text = (double) this.timeFactorSlider.value == -1.0 ? $"{num1:F1}x" : $"{num1:F0}x";
    this.conditionsLabel.text = this.weatherSets[Mathf.Clamp(Mathf.FloorToInt(this.environment.weatherIntensity * (float) this.weatherSets.Length), 0, this.weatherSets.Length - 1)].displayName;
    this.cloudHeightLabel.text = UnitConverter.AltitudeReading(this.environment.cloudAltitude);
    this.windSpeedLabel.text = UnitConverter.SpeedReading(this.environment.windSpeed);
    this.windTurbulenceLabel.text = $"{(ValueType) (float) ((double) this.environment.windTurbulence * 100.0):F0}%";
    this.windHeadingLabel.text = $"{this.environment.windHeading:F0}";
    this.windRandomHeadingLabel.text = $"{this.environment.windRandomHeading:F0}°";
    this.windRandomArc.fillAmount = (float) (2.0 * (double) this.environment.windRandomHeading / 360.0);
    this.windRandomArc.transform.localEulerAngles = new Vector3(0.0f, 0.0f, this.environment.windRandomHeading);
    this.moonPhaseImage.enabled = true;
    if ((double) this.moonPhaseSlider.value < 0.0)
    {
      this.moonPhaseLabel.text = "No Moon";
      this.moonPhaseImage.enabled = false;
    }
    else if ((double) this.moonPhaseSlider.value >= 0.0 && (double) this.moonPhaseSlider.value <= 2.0 || (double) this.moonPhaseSlider.value > 26.0 && (double) this.moonPhaseSlider.value <= 28.0)
      this.moonPhaseLabel.text = "New Moon";
    else if ((double) this.moonPhaseSlider.value > 2.0 && (double) this.moonPhaseSlider.value <= 6.0)
      this.moonPhaseLabel.text = "First Crescent";
    else if ((double) this.moonPhaseSlider.value > 6.0 && (double) this.moonPhaseSlider.value <= 11.0)
      this.moonPhaseLabel.text = "First Quarter";
    else if ((double) this.moonPhaseSlider.value > 11.0 && (double) this.moonPhaseSlider.value <= 17.0)
      this.moonPhaseLabel.text = "Full Moon";
    else if ((double) this.moonPhaseSlider.value > 17.0 && (double) this.moonPhaseSlider.value <= 22.0)
    {
      this.moonPhaseLabel.text = "Last Quarter";
    }
    else
    {
      if ((double) this.moonPhaseSlider.value <= 22.0 || (double) this.moonPhaseSlider.value > 26.0)
        return;
      this.moonPhaseLabel.text = "Last Crescent";
    }
  }
}

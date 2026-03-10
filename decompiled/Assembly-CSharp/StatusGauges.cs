// Decompiled with JetBrains decompiler
// Type: StatusGauges
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;
using UnityEngine.UI;

#nullable disable
public class StatusGauges : HUDApp
{
  private Aircraft aircraft;
  [SerializeField]
  private Image fuelLevelDisplay;
  [SerializeField]
  private Image throttleLevelDisplay;
  [SerializeField]
  private StatusGauges.Gauge irLevelGauge;
  [SerializeField]
  private Text massValue;
  [SerializeField]
  private Text twrValue;
  private IRSource irSource;
  private ControlInputs inputs;
  private float lastRefresh;
  private float refreshDelay = 10f;
  private float gaugeThickness = 25f;

  public override void Initialize(Aircraft aircraft)
  {
    if ((UnityEngine.Object) aircraft == (UnityEngine.Object) null)
      return;
    this.aircraft = aircraft;
    this.irSource = aircraft.GetIRSource();
    this.inputs = aircraft.GetInputs();
    float fuelLevel = aircraft.GetFuelLevel();
    this.fuelLevelDisplay.rectTransform.sizeDelta = new Vector2(this.gaugeThickness, 200f * fuelLevel);
    this.fuelLevelDisplay.color = GameAssets.i.redGreenGradient.Evaluate(fuelLevel);
    this.massValue.text = UnitConverter.WeightReading(aircraft.GetMass());
  }

  public override void Refresh()
  {
    if ((UnityEngine.Object) this.aircraft == (UnityEngine.Object) null)
      return;
    this.irLevelGauge.Update(Mathf.Clamp(this.irSource.intensity, 0.0f, 12f));
    this.throttleLevelDisplay.rectTransform.sizeDelta = new Vector2(this.gaugeThickness, 200f * this.inputs.throttle);
    if ((double) Time.timeSinceLevelLoad <= (double) this.lastRefresh + (double) this.refreshDelay)
      return;
    float fuelLevel = this.aircraft.GetFuelLevel();
    this.fuelLevelDisplay.rectTransform.sizeDelta = new Vector2(this.gaugeThickness, 200f * fuelLevel);
    this.fuelLevelDisplay.color = GameAssets.i.redGreenGradient.Evaluate(fuelLevel);
    float mass = this.aircraft.GetMass();
    this.massValue.text = UnitConverter.WeightReading(mass);
    float maxPower;
    if (this.aircraft.GetMaxPower(out maxPower))
    {
      this.twrValue.text = UnitConverter.PowerToWeightReading(maxPower * (1f / 1000f) / mass);
    }
    else
    {
      float maxThrust;
      if (this.aircraft.GetMaxThrust(out maxThrust))
        this.twrValue.text = $"{(ValueType) (float) ((double) maxThrust / ((double) mass * 9.8100004196167)):F2}";
    }
    this.lastRefresh = Time.timeSinceLevelLoad;
  }

  [Serializable]
  private class Gauge
  {
    [SerializeField]
    private Text title;
    [SerializeField]
    private Image image;
    [SerializeField]
    private Image circle;
    [SerializeField]
    private Text reading;
    [SerializeField]
    private Transform needle;
    [SerializeField]
    private float maxValue;

    public void Update(float value)
    {
      if ((UnityEngine.Object) this.reading == (UnityEngine.Object) null)
        return;
      this.reading.text = value.ToString("F1");
      this.needle.transform.localEulerAngles = Vector3.forward * (value / this.maxValue) * -300f;
      this.circle.fillAmount = 0.84f * value / this.maxValue;
      this.circle.color = GameAssets.i.redGreenGradient.Evaluate((float) (1.0 - (double) value / (double) this.maxValue));
    }
  }
}

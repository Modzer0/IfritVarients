// Decompiled with JetBrains decompiler
// Type: EngineTelemetry
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;
using UnityEngine.UI;

#nullable disable
public class EngineTelemetry : MonoBehaviour
{
  private Aircraft aircraft;
  [SerializeField]
  private string engineName;
  [SerializeField]
  private EngineTelemetry.Gauge thrustGauge;
  [SerializeField]
  private EngineTelemetry.Gauge rpmGauge;
  [SerializeField]
  private EngineTelemetry.Gauge throttleGauge;
  [SerializeField]
  private EngineTelemetry.Gauge pitchGauge;
  [SerializeField]
  private Text statusDisplay;
  [SerializeField]
  private Text[] colorableTexts;
  [SerializeField]
  private Image[] colorableImages;
  private IEngine engineInterface;
  private bool displayPower;
  private bool displayPitch;
  private float lastUpdate;
  private ControlInputs controlInputs;

  private void Start()
  {
    this.aircraft = SceneSingleton<CombatHUD>.i.aircraft;
    foreach (UnitPart unitPart in this.aircraft.partLookup)
    {
      if ((UnityEngine.Object) unitPart != (UnityEngine.Object) null && unitPart.gameObject.name == this.engineName)
      {
        this.engineInterface = unitPart.gameObject.GetComponentInChildren<IEngine>();
        this.engineInterface.OnEngineDisable += new Action(this.EngineTelemetry_OnEngineFailure);
        this.engineInterface.OnEngineDamage += new Action(this.EngineTelemetry_OnEngineDamage);
        break;
      }
    }
    if (this.engineInterface == null)
      Debug.LogWarning((object) ("Couldn't find engine interface for part " + this.engineName));
    if (this.engineInterface is IPowerSource)
      this.displayPower = true;
    if (this.engineInterface is IPitchTelemetry)
      this.displayPitch = true;
    this.controlInputs = this.aircraft.GetInputs();
  }

  private void OnDestroy()
  {
    if (this.engineInterface == null)
      return;
    this.engineInterface.OnEngineDisable -= new Action(this.EngineTelemetry_OnEngineFailure);
    this.engineInterface.OnEngineDamage -= new Action(this.EngineTelemetry_OnEngineDamage);
  }

  private void EngineTelemetry_OnEngineFailure()
  {
    this.ApplyColor(Color.red);
    this.statusDisplay.text = "INOPERABLE";
  }

  private void EngineTelemetry_OnEngineDamage()
  {
    this.ApplyColor(Color.yellow);
    this.statusDisplay.text = "FAULT DETECTED";
  }

  private void ApplyColor(Color color)
  {
    foreach (Graphic colorableText in this.colorableTexts)
      colorableText.color = color;
    foreach (Graphic colorableImage in this.colorableImages)
      colorableImage.color = color;
    this.statusDisplay.color = color;
  }

  private void Update()
  {
    if ((double) Time.timeSinceLevelLoad - (double) this.lastUpdate < 0.05000000074505806)
      return;
    this.lastUpdate = Time.timeSinceLevelLoad;
    if (!(bool) (this.engineInterface as UnityEngine.Object))
    {
      this.enabled = false;
    }
    else
    {
      if (this.displayPower)
      {
        if (this.engineInterface is IPowerSource engineInterface1)
          this.thrustGauge.UpdatePower(engineInterface1.GetPower());
      }
      else if (this.engineInterface is IThrustSource engineInterface2)
        this.thrustGauge.UpdateThrust(engineInterface2.GetThrust());
      if (this.displayPitch)
        this.pitchGauge.UpdateNumber((this.engineInterface as IPitchTelemetry).GetPitch());
      this.rpmGauge.UpdateRPM(this.engineInterface.GetRPMRatio());
      this.throttleGauge.UpdateNumber(this.controlInputs.throttle * 100f);
    }
  }

  [Serializable]
  private class Gauge
  {
    [SerializeField]
    private Text title;
    [SerializeField]
    private Text reading;
    [SerializeField]
    private Transform needle;
    [SerializeField]
    private float maxValue;

    public void UpdatePower(float power)
    {
      if ((UnityEngine.Object) this.reading == (UnityEngine.Object) null)
        return;
      this.reading.text = (power * (1f / 1000f)).ToString("F1");
      this.needle.transform.localEulerAngles = Vector3.forward * (power * (1f / 1000f) / this.maxValue) * -300f;
    }

    public void UpdateNumber(float number)
    {
      if ((UnityEngine.Object) this.reading == (UnityEngine.Object) null)
        return;
      this.reading.text = number.ToString("F0");
      this.needle.transform.localEulerAngles = Vector3.forward * (number / this.maxValue) * -300f;
    }

    public void UpdateRPM(float rpmRatio)
    {
      if ((UnityEngine.Object) this.reading == (UnityEngine.Object) null)
        return;
      this.reading.text = $"{(ValueType) (float) ((double) rpmRatio * 100.0):F0}%";
      this.needle.transform.localEulerAngles = Vector3.forward * rpmRatio * -300f;
    }

    public void UpdateThrust(float thrust)
    {
      if ((UnityEngine.Object) this.reading == (UnityEngine.Object) null)
        return;
      this.reading.text = (thrust * (1f / 1000f)).ToString("F1");
      this.needle.transform.localEulerAngles = Vector3.forward * (thrust / this.maxValue) * -300f;
    }
  }
}

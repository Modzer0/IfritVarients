// Decompiled with JetBrains decompiler
// Type: SpeedGauge
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using UnityEngine;
using UnityEngine.UI;

#nullable disable
public class SpeedGauge : HUDApp
{
  private Aircraft aircraft;
  private AircraftParameters aircraftParameters;
  [SerializeField]
  private AudioClip overspeedVoice;
  [SerializeField]
  private Gradient speedGradient;
  [SerializeField]
  private Text airspeedDisplay;
  [SerializeField]
  private Text overspeedDisplay;
  [SerializeField]
  private Image border;
  [SerializeField]
  private float overspeedThreshold = float.MaxValue;
  private float lastOverspeed = -100f;

  private void Awake() => this.overspeedDisplay.enabled = false;

  public override void Initialize(Aircraft aircraft)
  {
    if ((Object) aircraft == (Object) null)
      return;
    this.aircraft = aircraft;
    this.aircraftParameters = aircraft.definition.aircraftParameters;
  }

  public override void RefreshSettings()
  {
    base.RefreshSettings();
    this.airspeedDisplay.fontSize = this.fontSize;
  }

  public override void Refresh()
  {
    if ((Object) this.aircraft == (Object) null)
      return;
    float speed = this.aircraft.speed;
    this.airspeedDisplay.text = UnitConverter.SpeedReading(speed);
    if ((double) speed <= (double) this.overspeedThreshold)
    {
      if (this.overspeedDisplay.enabled)
        this.overspeedDisplay.enabled = false;
      this.airspeedDisplay.color = this.aircraft.gearState == LandingGear.GearState.LockedExtended ? Color.green : this.speedGradient.Evaluate(speed * 0.5f / Mathf.Max(this.aircraftParameters.takeoffSpeed, 1f));
    }
    else
    {
      this.overspeedDisplay.enabled = (double) Mathf.Sin(Time.timeSinceLevelLoad * 16f) > 0.0;
      if ((double) Time.timeSinceLevelLoad - (double) this.lastOverspeed > 20.0)
        SoundManager.PlayInterfaceOneShot(this.overspeedVoice);
      this.lastOverspeed = Time.timeSinceLevelLoad;
      this.airspeedDisplay.color = Color.red;
    }
  }
}

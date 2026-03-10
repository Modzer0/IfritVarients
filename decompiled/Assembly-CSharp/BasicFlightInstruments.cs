// Decompiled with JetBrains decompiler
// Type: BasicFlightInstruments
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using UnityEngine;
using UnityEngine.UI;

#nullable disable
public class BasicFlightInstruments : HUDApp
{
  private Aircraft aircraft;
  [SerializeField]
  private Text airspeedDisplay;
  [SerializeField]
  private Text altitudeDisplay;
  [SerializeField]
  private Text climbRateDisplay;
  [SerializeField]
  private Text headingDisplay;
  [SerializeField]
  private Image horizonDisplay;
  [SerializeField]
  private Image skyDisplay;
  [SerializeField]
  private Image verticalSpeedIndicator;
  [SerializeField]
  private Image AoAIndicator;
  private float lastUpdate;

  public override void Initialize(Aircraft aircraft)
  {
    if ((Object) aircraft == (Object) null)
      return;
    this.aircraft = aircraft;
  }

  public override void RefreshSettings()
  {
    base.RefreshSettings();
    this.airspeedDisplay.fontSize = this.fontSize;
    this.altitudeDisplay.fontSize = this.fontSize;
    this.climbRateDisplay.fontSize = this.fontSize;
    this.headingDisplay.fontSize = this.fontSize;
  }

  public override void Refresh()
  {
    if ((Object) this.aircraft == (Object) null || (double) Time.timeSinceLevelLoad - (double) this.lastUpdate < 0.05000000074505806)
      return;
    this.lastUpdate = Time.timeSinceLevelLoad;
    this.airspeedDisplay.text = UnitConverter.SpeedReading(this.aircraft.speed);
    this.altitudeDisplay.text = UnitConverter.AltitudeReading(this.aircraft.radarAlt) ?? "";
    float speed = Vector3.Dot(this.aircraft.CockpitRB().velocity, Vector3.up);
    this.climbRateDisplay.text = UnitConverter.ClimbRateReading(speed);
    this.verticalSpeedIndicator.transform.localPosition = new Vector3(0.0f, 1f * Mathf.Clamp(speed, -50f, 50f), 0.0f);
    this.headingDisplay.text = $"{this.aircraft.transform.eulerAngles.y:F0}°";
    this.horizonDisplay.transform.localEulerAngles = new Vector3(0.0f, 0.0f, -this.aircraft.transform.eulerAngles.z);
    float x = this.aircraft.transform.eulerAngles.x;
    if ((double) x > 180.0)
      x -= 360f;
    this.horizonDisplay.fillAmount = Mathf.Clamp((float) (0.5 + (double) x / 180.0), 0.0f, 1f);
    this.skyDisplay.fillAmount = Mathf.Clamp((float) (0.5 - (double) x / 180.0), 0.0f, 1f);
    if ((double) this.aircraft.speed > 10.0)
    {
      Vector3 vector3 = this.aircraft.cockpit.transform.InverseTransformDirection(this.aircraft.cockpit.rb.velocity);
      this.AoAIndicator.transform.localPosition = new Vector3(0.0f, 1.6f * Mathf.Clamp(Mathf.Atan2(vector3.y, vector3.z) * -57.29578f, -30f, 30f), 0.0f);
    }
    else
      this.AoAIndicator.transform.localPosition = Vector3.zero;
  }
}

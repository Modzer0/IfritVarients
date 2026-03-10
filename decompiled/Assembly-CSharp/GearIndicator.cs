// Decompiled with JetBrains decompiler
// Type: GearIndicator
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using UnityEngine;
using UnityEngine.UI;

#nullable disable
public class GearIndicator : HUDApp
{
  private Aircraft aircraft;
  [SerializeField]
  private float speedLimit;
  [SerializeField]
  private Image brakeIcon;
  [SerializeField]
  private Image gearIcon;
  [SerializeField]
  private Sprite brakeSprite;
  [SerializeField]
  private Sprite parkingBrakeSprite;
  [SerializeField]
  private AudioClip raiseGearVoice;
  [SerializeField]
  private AudioClip lowerGearVoice;
  [SerializeField]
  private bool lowerGearWarning;
  [SerializeField]
  private Text lowerGearText;
  private float lowerGearLastPlayed = -45f;
  private bool raiseGearPlayed;
  private AircraftParameters aircraftParameters;
  private ControlInputs inputs;

  private void Awake() => this.lowerGearText.enabled = false;

  public override void RefreshSettings()
  {
  }

  public override void Initialize(Aircraft aircraft)
  {
    this.speedLimit /= 3.6f;
    this.aircraft = aircraft;
    this.inputs = aircraft.GetInputs();
    this.aircraftParameters = aircraft.definition.aircraftParameters;
  }

  public override void Refresh()
  {
    if (this.aircraft.gearState == LandingGear.GearState.LockedExtended)
    {
      if (!this.gearIcon.enabled)
        this.gearIcon.enabled = true;
      if ((double) this.aircraft.speed > (double) this.speedLimit)
      {
        this.gearIcon.color = Color.red;
        if (!this.raiseGearPlayed)
        {
          SoundManager.PlayInterfaceOneShot(this.raiseGearVoice);
          this.raiseGearPlayed = true;
        }
      }
      else
      {
        this.raiseGearPlayed = false;
        this.gearIcon.color = Color.green;
      }
      bool flag = (double) this.aircraft.speed < 1.0 && (double) this.inputs.throttle < 0.10000000149011612;
      this.lowerGearText.enabled = false;
      this.brakeIcon.enabled = flag || (double) this.inputs.brake > 0.019999999552965164;
    }
    else
    {
      this.raiseGearPlayed = false;
      this.gearIcon.color = Color.green;
      if (this.aircraft.gearState == LandingGear.GearState.LockedRetracted && this.gearIcon.enabled)
      {
        this.gearIcon.enabled = false;
        this.brakeIcon.enabled = false;
      }
      if (this.aircraft.gearState == LandingGear.GearState.Retracting || this.aircraft.gearState == LandingGear.GearState.Extending)
        this.gearIcon.enabled = this.enabled = (double) Mathf.Sin(Time.timeSinceLevelLoad * 16f) > 0.0;
    }
    if (!this.lowerGearWarning || this.aircraft.gearState != LandingGear.GearState.LockedRetracted)
      return;
    if (this.lowerGearText.enabled && (double) Time.timeSinceLevelLoad - (double) this.lowerGearLastPlayed > 4.0)
      this.lowerGearText.enabled = false;
    if ((double) this.inputs.throttle >= 0.5 || (double) this.aircraft.radarAlt >= 30.0 || (double) this.aircraft.rb.velocity.y >= -0.5 || (double) this.aircraft.speed >= (double) this.aircraftParameters.takeoffSpeed * 1.5 || (double) Time.timeSinceLevelLoad - (double) this.lowerGearLastPlayed <= 60.0)
      return;
    this.lowerGearLastPlayed = Time.timeSinceLevelLoad;
    SoundManager.PlayInterfaceOneShot(this.lowerGearVoice);
    this.lowerGearText.enabled = true;
  }
}

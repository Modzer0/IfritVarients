// Decompiled with JetBrains decompiler
// Type: AoAIndexer
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using UnityEngine;
using UnityEngine.UI;

#nullable disable
public class AoAIndexer : HUDApp
{
  [SerializeField]
  private Image outline;
  [SerializeField]
  private Image slowArrow;
  [SerializeField]
  private Image fastArrow;
  [SerializeField]
  private Image onSpeedCircle;
  [SerializeField]
  private float targetAoA;
  [SerializeField]
  private float AoARange;
  [SerializeField]
  private GameObject[] hideWhenActive;
  private ControlInputs inputs;
  private Aircraft aircraft;

  public override void Initialize(Aircraft aircraft)
  {
    this.aircraft = aircraft;
    this.inputs = aircraft.GetInputs();
  }

  public override void Refresh()
  {
    if ((Object) this.aircraft == (Object) null)
      return;
    if (this.aircraft.gearState == LandingGear.GearState.LockedRetracted || (double) this.aircraft.radarAlt < 1.0 || (double) this.inputs.throttle > 0.699999988079071)
    {
      if (!this.outline.enabled)
        return;
      this.outline.enabled = false;
      this.slowArrow.enabled = false;
      this.fastArrow.enabled = false;
      this.onSpeedCircle.enabled = false;
      foreach (GameObject gameObject in this.hideWhenActive)
        gameObject.SetActive(true);
    }
    else
    {
      if (!this.outline.enabled)
      {
        foreach (GameObject gameObject in this.hideWhenActive)
          gameObject.SetActive(false);
        this.outline.enabled = true;
      }
      Vector3 vector3 = this.aircraft.cockpit.transform.InverseTransformDirection(this.aircraft.cockpit.rb.velocity);
      float f = (Mathf.Atan2(vector3.y, vector3.z) * -57.29578f - this.targetAoA) / this.AoARange;
      this.slowArrow.enabled = (double) f > 0.33329999446868896;
      this.fastArrow.enabled = (double) f < -0.33329999446868896;
      this.onSpeedCircle.enabled = (double) Mathf.Abs(f) < 0.66666698455810547;
    }
  }
}

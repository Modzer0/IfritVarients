// Decompiled with JetBrains decompiler
// Type: ShipPropulsion
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;

#nullable disable
public class ShipPropulsion : MonoBehaviour
{
  [SerializeField]
  private ShipPart part;
  [SerializeField]
  private ShipPart[] criticalParts;
  [SerializeField]
  private float thrust;
  [SerializeField]
  private float steeringThrust;
  [SerializeField]
  private float momentumFactor = 0.05f;
  [SerializeField]
  private float damageThreshold;
  [SerializeField]
  private float inputSmoothing;
  [SerializeField]
  private ParticleSystem[] particles;
  [SerializeField]
  private AudioSource thrustSound;
  [SerializeField]
  private AudioSource engineSound;
  [SerializeField]
  private Transform thrustTransform;
  [SerializeField]
  private bool underwater = true;
  [SerializeField]
  private ShipPropulsion.Rotator[] rotators;
  private float thrustInputSmoothed;
  private float steeringInputSmoothed;
  private float thrustSmoothSpeed;
  private float steeringSmoothSpeed;
  private ShipInputs inputs;
  private Ship ship;

  protected void Awake()
  {
    this.part.onDetachFromParent += new Action<ShipPart>(this.ShipPropulsion_OnPartDetach);
    foreach (UnitPart criticalPart in this.criticalParts)
      criticalPart.onApplyDamage += new Action<UnitPart.OnApplyDamage>(this.ShipPropulsion_OnApplyDamage);
    this.part.parentUnit.onDisableUnit += new Action<Unit>(this.ShipPropulsion_OnDisabled);
    this.ship = this.part.parentUnit as Ship;
    this.inputs = this.ship.GetInputs();
  }

  private void ShipPropulsion_OnPartDetach(ShipPart shipPart) => this.DisablePropulsion();

  private void ShipPropulsion_OnApplyDamage(UnitPart.OnApplyDamage e)
  {
    if ((double) e.hitPoints >= (double) this.damageThreshold)
      return;
    this.DisablePropulsion();
  }

  private void ShipPropulsion_OnDisabled(Unit unit) => this.DisablePropulsion();

  private void DisablePropulsion()
  {
    this.thrust = 0.0f;
    foreach (ParticleSystem particle in this.particles)
      particle.Stop();
    if ((UnityEngine.Object) this.thrustSound != (UnityEngine.Object) null)
      this.thrustSound.Stop();
    if ((UnityEngine.Object) this.engineSound != (UnityEngine.Object) null)
      this.engineSound.Stop();
    this.enabled = false;
  }

  private void FixedUpdate()
  {
    if ((double) this.thrust == 0.0)
    {
      this.enabled = false;
    }
    else
    {
      foreach (ShipPropulsion.Rotator rotator in this.rotators)
        rotator.Animate(this.inputs.throttle);
      if (!this.ship.LocalSim)
        return;
      int num = !this.underwater || (double) this.thrustTransform.position.y < (double) Datum.LocalSeaY ? 1 : 0;
      float throttle = this.inputs.throttle;
      float target = (float) (1.0 + (double) this.ship.speed * (double) this.momentumFactor) * this.inputs.steering;
      this.thrustInputSmoothed = FastMath.SmoothDamp(this.thrustInputSmoothed, throttle, ref this.thrustSmoothSpeed, this.inputSmoothing);
      this.steeringInputSmoothed = FastMath.SmoothDamp(this.steeringInputSmoothed, target, ref this.steeringSmoothSpeed, this.inputSmoothing);
      this.steeringInputSmoothed *= this.ship.AllowedSteerRate;
      this.part.rb.AddForceAtPosition((float) num * this.thrustInputSmoothed * this.thrust * this.transform.forward + (float) num * this.steeringInputSmoothed * this.steeringThrust * this.transform.right, this.thrustTransform.position);
      if ((UnityEngine.Object) this.engineSound != (UnityEngine.Object) null)
      {
        this.engineSound.pitch = Mathf.Lerp(this.engineSound.pitch, Mathf.Min((float) (0.5 + (double) this.ship.speed * 0.004999999888241291 + (double) Mathf.Abs(this.thrustInputSmoothed) * 0.34999999403953552 + (double) Mathf.Abs(this.steeringInputSmoothed) * 0.25), 1.5f), Time.deltaTime);
        this.engineSound.volume = this.engineSound.pitch;
      }
      if ((double) throttle > 0.10000000149011612)
      {
        if (this.particles.Length != 0 && !this.particles[0].isPlaying)
        {
          foreach (ParticleSystem particle in this.particles)
            particle.Play();
        }
        if ((UnityEngine.Object) this.thrustSound != (UnityEngine.Object) null)
        {
          if (!this.thrustSound.isPlaying)
            this.thrustSound.Play();
          if ((double) this.thrustSound.volume < 1.0)
            this.thrustSound.volume += Time.deltaTime;
        }
      }
      if ((double) throttle >= 0.10000000149011612)
        return;
      if (this.particles.Length != 0 && this.particles[0].isPlaying)
      {
        foreach (ParticleSystem particle in this.particles)
          particle.Stop();
      }
      if (!((UnityEngine.Object) this.thrustSound != (UnityEngine.Object) null) || !this.thrustSound.isPlaying)
        return;
      if ((double) this.thrustSound.volume > 0.0)
        this.thrustSound.volume -= Time.deltaTime;
      else
        this.thrustSound.Stop();
    }
  }

  [Serializable]
  private class Rotator
  {
    [SerializeField]
    private Transform transform;
    [SerializeField]
    private float minSpeed;
    [SerializeField]
    private float maxSpeed;

    public void Animate(float power)
    {
      this.transform.localEulerAngles += Vector3.forward * Mathf.Lerp(this.minSpeed, this.maxSpeed, power) * Time.deltaTime;
    }
  }
}

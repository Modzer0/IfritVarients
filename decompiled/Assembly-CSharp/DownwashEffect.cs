// Decompiled with JetBrains decompiler
// Type: DownwashEffect
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;

#nullable disable
public class DownwashEffect : MonoBehaviour
{
  [SerializeField]
  private Aircraft aircraft;
  [SerializeField]
  private ParticleSystem[] landEffects;
  [SerializeField]
  private ParticleSystem[] waterEffects;
  [SerializeField]
  private float range;
  [SerializeField]
  private float minSpeed;
  private ParticleSystem.MainModule[] waterEffectMains;
  private ParticleSystem.MainModule[] landEffectMains;
  private float lastCheck;
  private GlobalPosition effectPosition;
  private Vector3 velAlongSlope;

  private void Start()
  {
    this.waterEffectMains = new ParticleSystem.MainModule[this.waterEffects.Length];
    this.landEffectMains = new ParticleSystem.MainModule[this.landEffects.Length];
    for (int index = 0; index < this.waterEffects.Length; ++index)
      this.waterEffectMains[index] = this.waterEffects[index].main;
    for (int index = 0; index < this.landEffects.Length; ++index)
      this.landEffectMains[index] = this.landEffects[index].main;
    this.StartSlowUpdateDelayed(1f, new Action(this.SlowUpdate));
  }

  private void SetEffects(ParticleSystem[] systems, bool enabled)
  {
    if (enabled)
    {
      foreach (ParticleSystem system in systems)
        system.Play();
    }
    else
    {
      foreach (ParticleSystem system in systems)
        system.Stop();
    }
  }

  private void SlowUpdate()
  {
    if (this.enabled)
    {
      if ((double) this.aircraft.radarAlt <= (double) this.range)
        return;
      this.enabled = false;
      this.StopEffects();
    }
    else
    {
      if ((double) this.aircraft.radarAlt >= (double) this.range)
        return;
      this.enabled = true;
    }
  }

  private void StopEffects()
  {
    foreach (ParticleSystem waterEffect in this.waterEffects)
      waterEffect.Stop();
    foreach (ParticleSystem landEffect in this.landEffects)
      landEffect.Stop();
  }

  private void Update()
  {
    if ((double) Time.timeSinceLevelLoad - (double) this.lastCheck > 0.10000000149011612)
    {
      if ((double) this.aircraft.displayDetail < 1.0)
      {
        this.StopEffects();
        return;
      }
      this.velAlongSlope = new Vector3(this.aircraft.rb.velocity.x, 0.0f, this.aircraft.rb.velocity.z);
      this.lastCheck = Time.timeSinceLevelLoad;
      float num = this.range;
      bool enabled1 = false;
      bool enabled2 = false;
      if ((double) this.aircraft.speed >= (double) this.minSpeed)
      {
        Ray ray = new Ray(this.aircraft.transform.position, Vector3.up * -340f - this.aircraft.rb.velocity);
        RaycastHit hitInfo;
        if (Physics.Raycast(ray, out hitInfo, this.range, 64 /*0x40*/))
        {
          num = hitInfo.distance;
          this.velAlongSlope = Vector3.ProjectOnPlane(this.aircraft.rb.velocity, hitInfo.normal);
          this.transform.rotation = Quaternion.LookRotation(this.velAlongSlope);
          this.effectPosition = hitInfo.point.ToGlobalPosition();
          Color color = new Color(1f, 1f, 1f, (float) (1.0 - (double) num / (double) this.range));
          for (int index = 0; index < this.landEffectMains.Length; ++index)
          {
            this.landEffectMains[index].emitterVelocity = this.velAlongSlope;
            this.landEffectMains[index].startColor = (ParticleSystem.MinMaxGradient) color;
          }
          enabled1 = true;
        }
        float enter;
        if (Datum.WaterPlane().Raycast(ray, out enter) && (double) enter < (double) num)
        {
          this.transform.rotation = Quaternion.identity;
          this.effectPosition = this.aircraft.transform.GlobalPosition() + ray.direction * enter;
          Color color = new Color(1f, 1f, 1f, (float) (1.0 - (double) enter / (double) this.range));
          for (int index = 0; index < this.waterEffectMains.Length; ++index)
          {
            this.waterEffectMains[index].emitterVelocity = new Vector3(this.aircraft.rb.velocity.x, 0.0f, this.aircraft.rb.velocity.z);
            this.waterEffectMains[index].startColor = (ParticleSystem.MinMaxGradient) color;
          }
          enabled2 = true;
          enabled1 = false;
        }
      }
      this.SetEffects(this.waterEffects, enabled2);
      this.SetEffects(this.landEffects, enabled1);
    }
    this.transform.position = this.effectPosition.ToLocalPosition() + this.velAlongSlope * (Time.timeSinceLevelLoad - this.lastCheck);
  }
}

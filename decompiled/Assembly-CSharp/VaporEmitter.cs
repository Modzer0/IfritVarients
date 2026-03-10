// Decompiled with JetBrains decompiler
// Type: VaporEmitter
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;

#nullable disable
[Serializable]
public class VaporEmitter
{
  public ParticleSystem particles;
  private ParticleSystem.MainModule main;
  public Transform[] emitTransforms;
  public float emitFrequency;
  public float minSpeed = 20f;
  public float opacity = 1f;
  public AnimationCurve alphaThresholdVelocity;
  [SerializeField]
  private bool localSpace;
  [SerializeField]
  private bool transonic;
  [SerializeField]
  private bool contrail;
  private float emitCounter;
  private ParticleSystem.EmitParams emitParams;
  private float minAltitude = 7500f;
  private float maxAltitude = 12500f;

  public void Initialize()
  {
    this.main = this.particles.main;
    if (this.localSpace)
      return;
    this.main.simulationSpace = ParticleSystemSimulationSpace.Custom;
    this.main.customSimulationSpace = Datum.origin;
  }

  public void Emit(float alpha, float airspeed, Vector3 velocity, float altitude)
  {
    if (this.transonic)
    {
      float speedOfSound = LevelInfo.GetSpeedOfSound(altitude);
      Keyframe[] keys = this.alphaThresholdVelocity.keys;
      keys[3].time = speedOfSound;
      keys[2].time = speedOfSound - 40f;
      keys[4].time = speedOfSound + 40f;
      this.alphaThresholdVelocity.keys = keys;
    }
    bool flag;
    if (this.contrail)
    {
      flag = (double) altitude > (double) this.minAltitude && (double) altitude < (double) this.maxAltitude;
      if (flag)
        this.main.startLifetime = (ParticleSystem.MinMaxCurve) (float) (1.0 + 29.0 * (double) Mathf.Clamp01((float) (1.0 - 2.0 * (double) Mathf.Abs((float) (((double) altitude - 0.5 * ((double) this.minAltitude + (double) this.maxAltitude)) / ((double) this.maxAltitude - (double) this.minAltitude))))));
    }
    else
      flag = (double) Mathf.Abs(alpha) > (double) this.alphaThresholdVelocity.Evaluate(airspeed);
    if (!flag)
      return;
    this.emitCounter += Time.deltaTime * Mathf.Min(this.emitFrequency, airspeed / this.minSpeed * this.emitFrequency);
    if ((double) this.emitCounter <= 1.0)
      return;
    this.emitCounter = 0.0f;
    for (int index = 0; index < this.emitTransforms.Length; ++index)
    {
      if ((UnityEngine.Object) this.emitTransforms[index] != (UnityEngine.Object) null)
      {
        if (!this.localSpace)
        {
          this.emitParams.position = this.emitTransforms[index].position.ToGlobalPosition().AsVector3();
          this.emitParams.velocity = velocity + this.main.startSpeed.constant * new Vector3((float) UnityEngine.Random.Range(-1, 1), (float) UnityEngine.Random.Range(-1, 1), (float) UnityEngine.Random.Range(-1, 1));
          this.particles.Emit(this.emitParams, 1);
        }
        else
        {
          this.emitTransforms[index].transform.rotation = Quaternion.LookRotation(-velocity);
          this.particles.Play();
        }
      }
    }
  }
}

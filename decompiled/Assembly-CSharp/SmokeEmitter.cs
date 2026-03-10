// Decompiled with JetBrains decompiler
// Type: SmokeEmitter
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;

#nullable disable
[Serializable]
public class SmokeEmitter
{
  public ParticleSystem particles;
  private ParticleSystem.MainModule main;
  public Transform[] emitTransforms;
  public float emitFrequency;
  public float minSpeed = 20f;
  public float opacity = 1f;
  public Color color = Color.red;
  [SerializeField]
  private bool localSpace;
  private float emitCounter;
  private ParticleSystem.EmitParams emitParams;

  public void Initialize()
  {
    this.main = this.particles.main;
    if (this.localSpace)
      return;
    this.main.simulationSpace = ParticleSystemSimulationSpace.Custom;
    this.main.customSimulationSpace = Datum.origin;
  }

  public void Emit(bool active, float airspeed, Vector3 velocity)
  {
    if (!active)
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

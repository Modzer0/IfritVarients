// Decompiled with JetBrains decompiler
// Type: ParticleEffectManager
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;
using UnityEngine;

#nullable disable
public class ParticleEffectManager : SceneSingleton<ParticleEffectManager>
{
  private Dictionary<string, ParticleEffectManager.GlobalParticleSystem> systemLookup = new Dictionary<string, ParticleEffectManager.GlobalParticleSystem>();
  [SerializeField]
  private ParticleEffectManager.GlobalParticleSystem[] globalParticleSystems;
  private ParticleSystem.EmitParams emitParams;

  protected override void Awake()
  {
    base.Awake();
    foreach (ParticleEffectManager.GlobalParticleSystem globalParticleSystem in this.globalParticleSystems)
    {
      globalParticleSystem.Initialize();
      this.systemLookup.Add(globalParticleSystem.name, globalParticleSystem);
    }
  }

  public void EmitParticles(
    string name,
    int number,
    GlobalPosition origin,
    Vector3 startVelocity,
    float positionVariation,
    float lifetime,
    float lifetimeVariation,
    float startSize,
    float sizeVariation,
    float velocityVariation,
    float opacity,
    float opacityVariation)
  {
    ParticleEffectManager.GlobalParticleSystem globalParticleSystem = this.systemLookup[name];
    for (int index = 0; index < number; ++index)
    {
      this.emitParams.position = origin.AsVector3() + startVelocity * Time.deltaTime * (float) index / (float) number + ((double) positionVariation == 0.0 ? Vector3.zero : UnityEngine.Random.insideUnitSphere * positionVariation);
      this.emitParams.velocity = startVelocity + ((double) velocityVariation == 0.0 ? Vector3.zero : UnityEngine.Random.insideUnitSphere * velocityVariation);
      globalParticleSystem.main.startLifetime = (ParticleSystem.MinMaxCurve) (lifetime + ((double) lifetimeVariation == 0.0 ? 0.0f : UnityEngine.Random.value * lifetime * lifetimeVariation));
      globalParticleSystem.main.startSize = (ParticleSystem.MinMaxCurve) (startSize + ((double) sizeVariation == 0.0 ? 0.0f : UnityEngine.Random.value * startSize * sizeVariation));
      Color mainColor = globalParticleSystem.mainColor with
      {
        a = opacity + ((double) opacityVariation == 0.0 ? 0.0f : UnityEngine.Random.value * opacity * opacityVariation)
      };
      globalParticleSystem.main.startColor = (ParticleSystem.MinMaxGradient) mainColor;
      globalParticleSystem.system.Emit(this.emitParams, 1);
    }
  }

  [Serializable]
  private class GlobalParticleSystem
  {
    public string name;
    public ParticleSystem system;
    public Color mainColor;
    [NonSerialized]
    public ParticleSystem.EmissionModule emission;
    [NonSerialized]
    public ParticleSystem.MainModule main;

    public void Initialize()
    {
      this.emission = this.system.emission;
      this.main = this.system.main;
      this.main.simulationSpace = ParticleSystemSimulationSpace.Custom;
      this.main.customSimulationSpace = Datum.origin;
      this.mainColor = this.main.startColor.color;
    }
  }
}

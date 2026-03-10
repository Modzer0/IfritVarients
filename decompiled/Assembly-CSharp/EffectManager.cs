// Decompiled with JetBrains decompiler
// Type: EffectManager
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System.Collections.Generic;
using UnityEngine;

#nullable disable
public class EffectManager : SceneSingleton<EffectManager>
{
  public int maxEffects = 20;
  private Queue<GameObject> effectQueue = new Queue<GameObject>();
  private List<EffectManager.ImpactSet> groundImpacts = new List<EffectManager.ImpactSet>();

  private void Start()
  {
    foreach (GameObject groundImpact in GameAssets.i.groundImpacts)
    {
      GameObject gameObject = Object.Instantiate<GameObject>(groundImpact, Datum.origin);
      ParticleSystem[] componentsInChildren = gameObject.GetComponentsInChildren<ParticleSystem>();
      this.groundImpacts.Add(new EffectManager.ImpactSet(gameObject, componentsInChildren));
    }
  }

  public void ImpactDust(float impactForce, GlobalPosition globalPosition, Quaternion rotation)
  {
    this.groundImpacts[Mathf.FloorToInt((float) ((double) Mathf.Clamp01(impactForce / 50000f) * (double) this.groundImpacts.Count - 1.0 / 1000.0))].Play(globalPosition, rotation);
  }

  public void AddEffect(GameObject effect)
  {
    this.effectQueue.Enqueue(effect);
    if (this.effectQueue.Count <= this.maxEffects)
      return;
    Object.Destroy((Object) this.effectQueue.Dequeue());
  }

  private class ImpactSet
  {
    private GameObject gameObject;
    private ParticleSystem[] systems;

    public ImpactSet(GameObject gameObject, ParticleSystem[] systems)
    {
      this.gameObject = gameObject;
      this.systems = systems;
    }

    public void Play(GlobalPosition position, Quaternion rotation)
    {
      this.gameObject.transform.position = position.ToLocalPosition();
      this.gameObject.transform.rotation = rotation;
      for (int index = 0; index < this.systems.Length; ++index)
        this.systems[index].Play();
    }
  }
}

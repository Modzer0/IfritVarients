// Decompiled with JetBrains decompiler
// Type: ExplosionAudioManager
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;
using UnityEngine;

#nullable disable
public class ExplosionAudioManager : SceneSingleton<ExplosionAudioManager>
{
  [SerializeField]
  private List<ExplosionAudioManager.ManagedExplosion> sources = new List<ExplosionAudioManager.ManagedExplosion>();

  public void AddExplosionAudio(AudioSource audioSource, AudioLowPassFilter filter, float yield)
  {
    this.sources.Add(new ExplosionAudioManager.ManagedExplosion(audioSource, filter, yield));
    this.enabled = true;
  }

  private void Update()
  {
    Vector3 position = SceneSingleton<CameraStateManager>.i.transform.position;
    for (int index = this.sources.Count - 1; index >= 0; --index)
    {
      ExplosionAudioManager.ManagedExplosion source = this.sources[index];
      if (source.AudioSourceNull())
        this.sources.RemoveAt(index);
      else if (source.InRange(position))
        this.sources.RemoveAt(index);
    }
    if (this.sources.Count != 0)
      return;
    this.enabled = false;
  }

  [Serializable]
  public class ManagedExplosion
  {
    private readonly AudioSource audioSource;
    private readonly Transform xform;
    private readonly AudioLowPassFilter filter;
    private readonly float startTime;
    private readonly float yield;
    private float propagation;

    public ManagedExplosion(AudioSource audioSource, AudioLowPassFilter filter, float yield)
    {
      this.audioSource = audioSource;
      this.xform = audioSource.transform;
      this.filter = filter;
      this.startTime = Time.timeSinceLevelLoad;
      this.yield = yield;
      this.propagation = Mathf.Pow(yield, 0.3333f) * 0.5f;
    }

    public bool AudioSourceNull() => (UnityEngine.Object) this.audioSource == (UnityEngine.Object) null;

    public bool InRange(Vector3 listenerPosition)
    {
      this.propagation += 340f * Time.deltaTime;
      if (!FastMath.InRange(listenerPosition, this.xform.position, this.propagation))
        return false;
      this.Play();
      return true;
    }

    public void Play()
    {
      float num1 = Time.timeSinceLevelLoad - this.startTime;
      this.audioSource.bypassListenerEffects = (double) num1 > 0.10000000149011612;
      this.filter.cutoffFrequency = Mathf.Clamp(22000f / num1, 1000f, 22000f);
      this.audioSource.Play();
      if ((double) this.yield <= 0.0)
        return;
      float num2 = this.yield * 100f / Vector3.SqrMagnitude(this.xform.position - SceneSingleton<CameraStateManager>.i.transform.position);
      SceneSingleton<CameraStateManager>.i.ShakeCamera(Mathf.Clamp01(num2), 0.0f);
    }
  }
}

// Decompiled with JetBrains decompiler
// Type: ExplosionAudio
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;

#nullable disable
public class ExplosionAudio : MonoBehaviour
{
  [SerializeField]
  private ExplosionAudio.ExplosionSound[] explosionSounds;
  private AudioLowPassFilter lowPassFilter;

  private void Start()
  {
    this.lowPassFilter = this.gameObject.AddComponent<AudioLowPassFilter>();
    float num = FastMath.SquareDistance(SceneSingleton<CameraStateManager>.i.transform.position, this.transform.position);
    foreach (ExplosionAudio.ExplosionSound explosionSound in this.explosionSounds)
    {
      float maxDistance = explosionSound.source.maxDistance;
      if ((double) num < (double) maxDistance * (double) maxDistance)
      {
        explosionSound.source.clip = explosionSound.clips[Mathf.FloorToInt((float) UnityEngine.Random.Range(0, explosionSound.clips.Length))];
        explosionSound.source.pitch += UnityEngine.Random.Range(-0.2f, 0.2f);
        explosionSound.source.dopplerLevel = 0.0f;
        explosionSound.source.spatialBlend = 1f;
        SceneSingleton<ExplosionAudioManager>.i.AddExplosionAudio(explosionSound.source, this.lowPassFilter, explosionSound.yield);
      }
    }
    UnityEngine.Object.Destroy((UnityEngine.Object) this);
  }

  [Serializable]
  public class ExplosionSound
  {
    public AudioSource source;
    public AudioClip[] clips;
    public float yield;
  }
}

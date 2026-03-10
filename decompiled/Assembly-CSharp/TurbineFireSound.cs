// Decompiled with JetBrains decompiler
// Type: TurbineFireSound
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

#nullable disable
public class TurbineFireSound : MonoBehaviour
{
  public AudioSource source;
  public AudioClip[] clips;
  public float pitchVariation = 0.05f;
  public float volumeVariation = 0.05f;
  public Rigidbody rb;

  private void Start()
  {
    if (this.clips.Length != 0)
      this.source.clip = this.clips[Random.Range(0, this.clips.Length)];
    this.source.pitch += Random.Range(-this.pitchVariation, this.pitchVariation);
    this.source.volume += Random.Range(-this.volumeVariation, this.volumeVariation);
    this.source.Play();
    if (this.source.loop)
      this.source.time = Random.Range(0.0f, this.source.clip.length);
    this.rb = this.transform.GetComponentInParent<Rigidbody>();
  }

  private void Update()
  {
    if ((Object) this.rb != (Object) null)
      this.source.volume = Mathf.Lerp(this.source.volume, Mathf.Min(this.source.volume, Mathf.Clamp01(this.rb.velocity.magnitude * 0.1f)), 0.1f);
    if (!((Object) this.transform.parent == (Object) Datum.origin))
      return;
    this.source.volume = Mathf.Lerp(this.source.volume, 0.0f, 0.1f);
  }
}

// Decompiled with JetBrains decompiler
// Type: AoADisplay
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using UnityEngine;
using UnityEngine.UI;

#nullable disable
public class AoADisplay : HUDApp
{
  [SerializeField]
  private Text AoAText;
  [SerializeField]
  private Text stallText;
  [SerializeField]
  private Gradient colorAtAngle;
  [SerializeField]
  private AudioClip stallHorn;
  [SerializeField]
  private AudioClip stallVoice;
  [SerializeField]
  private float hornVolume;
  [SerializeField]
  private float hornThreshold;
  [SerializeField]
  private float velocityThreshold = 10f;
  [SerializeField]
  private float gradientMax;
  private Aircraft aircraft;
  private AudioSource hornSource;
  private float hornLastPlayed;

  private void Awake() => this.stallText.enabled = false;

  public override void Initialize(Aircraft aircraft)
  {
    this.aircraft = aircraft;
    this.hornSource = aircraft.gameObject.AddComponent<AudioSource>();
    this.hornSource.outputAudioMixerGroup = SoundManager.i.InterfaceMixer;
    this.hornSource.clip = this.stallHorn;
    this.hornSource.loop = true;
    this.hornSource.volume = this.hornVolume;
    this.hornSource.spatialBlend = 0.0f;
    this.hornSource.dopplerLevel = 0.0f;
    this.hornSource.bypassEffects = true;
  }

  private void OnDestroy()
  {
    if (!((Object) this.hornSource != (Object) null))
      return;
    this.hornSource.Stop();
  }

  public override void RefreshSettings()
  {
    base.RefreshSettings();
    this.AoAText.fontSize = this.fontSize;
  }

  public override void Refresh()
  {
    if ((Object) this.aircraft == (Object) null)
      return;
    Vector3 vector3 = this.aircraft.cockpit.transform.InverseTransformDirection(this.aircraft.cockpit.rb.velocity);
    float num = Mathf.Atan2(vector3.y, vector3.z) * -57.29578f;
    this.AoAText.enabled = (double) this.aircraft.speed > (double) this.velocityThreshold;
    if (this.AoAText.enabled)
    {
      this.AoAText.color = this.colorAtAngle.Evaluate(num / this.gradientMax);
      this.AoAText.text = $"{num:F1}°";
    }
    if ((double) num > (double) this.hornThreshold && this.AoAText.enabled)
    {
      this.stallText.enabled = (double) Mathf.Sin(Time.timeSinceLevelLoad * 16f) > 0.0;
      this.stallText.color = this.AoAText.color;
      if (!this.hornSource.isPlaying)
      {
        this.hornSource.Play();
        if ((double) Time.timeSinceLevelLoad - (double) this.hornLastPlayed > 5.0)
          SoundManager.PlayInterfaceOneShot(this.stallVoice);
      }
      this.hornLastPlayed = Time.timeSinceLevelLoad;
    }
    else
    {
      this.stallText.enabled = false;
      if (!this.hornSource.isPlaying)
        return;
      this.hornSource.Stop();
    }
  }
}

// Decompiled with JetBrains decompiler
// Type: AudioMixerVolume
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using UnityEngine;
using UnityEngine.Audio;

#nullable disable
public class AudioMixerVolume : MonoBehaviour
{
  public static readonly string Master = "MasterVolume";
  public static readonly string Effects = "EffectsVolume";
  public static readonly string Interface = "InterfaceVolume";
  public static readonly string Menu = "MenuVolume";
  public static readonly string Music = "MusicVolume";
  public static readonly string RadarWarning = "RadarWarningVolume";
  public static readonly string MissileAlert = "MissileAlertVolume";
  public static readonly string JammedNoise = "JammedNoiseVolume";
  public static readonly string EffectLowPassCutoff = nameof (EffectLowPassCutoff);
  public static readonly string EffectChorusDryMix = nameof (EffectChorusDryMix);
  public static readonly string MasterLowPassCutoff = nameof (MasterLowPassCutoff);
  [SerializeField]
  private AudioMixer _mixer;

  public static void SetValue(string channel, float value)
  {
    PlayerPrefs.SetFloat(channel, value);
    SoundManager.i.Volumes.ChangeMixerVolume(channel, value);
  }

  public static float GetPref(string channel) => PlayerPrefs.GetFloat(channel, 0.75f);

  private void Start()
  {
    this.LoadPref(AudioMixerVolume.Master);
    this.LoadPref(AudioMixerVolume.Effects);
    this.LoadPref(AudioMixerVolume.Interface);
    this.LoadPref(AudioMixerVolume.Menu);
    this.LoadPref(AudioMixerVolume.Music);
    this.LoadPref(AudioMixerVolume.RadarWarning);
    this.LoadPref(AudioMixerVolume.MissileAlert);
    this.LoadPref(AudioMixerVolume.JammedNoise);
  }

  private void LoadPref(string channel)
  {
    float pref = AudioMixerVolume.GetPref(channel);
    this.ChangeMixerVolume(channel, pref);
  }

  private void ChangeMixerVolume(string channel, float volume)
  {
    if (!Application.isPlaying)
      return;
    float decibel = AudioHelper.LinearToDecibel(volume);
    this._mixer.SetFloat(channel, decibel);
  }

  public static void SetEffectsAudioFilterStrength(float cutoff, float volume)
  {
    AudioMixerVolume volumes = SoundManager.i.Volumes;
    volumes._mixer.SetFloat(AudioMixerVolume.EffectLowPassCutoff, cutoff);
    volumes._mixer.SetFloat(AudioMixerVolume.EffectChorusDryMix, volume);
  }

  public static void SetMasterAudioFilterStrength(float cutoff)
  {
    SoundManager.i.Volumes._mixer.SetFloat(AudioMixerVolume.MasterLowPassCutoff, cutoff);
  }
}

// Decompiled with JetBrains decompiler
// Type: SoundManager
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.Audio;

#nullable disable
public class SoundManager : MonoBehaviour
{
  private static readonly ResourcesAsyncLoader<SoundManager> loader = ResourcesAsyncLoader.Create<SoundManager>("MusicAndVolume", (Func<GameObject, SoundManager>) null);
  [Header("References")]
  [SerializeField]
  private MusicManager musicManager;
  [SerializeField]
  private AudioMixerVolume mixerVolume;
  [Header("Oneshot Sources")]
  [SerializeField]
  private AudioSource effectsSource;
  [SerializeField]
  private AudioSource interfaceSource;
  [SerializeField]
  private AudioSource radarWarningSource;
  [SerializeField]
  private AudioSource menuSource;
  [Header("Mixers")]
  [SerializeField]
  public AudioMixerGroup EffectsMixer;
  [SerializeField]
  public AudioMixerGroup HeavyEffectsMixer;
  [SerializeField]
  public AudioMixerGroup InterfaceMixer;
  [SerializeField]
  public AudioMixerGroup RadarWarningMixer;
  [SerializeField]
  public AudioMixerGroup MissileAlertMixer;
  [SerializeField]
  public AudioMixerGroup JammedNoiseMixer;
  [SerializeField]
  public AudioMixerGroup MenuMixer;
  [SerializeField]
  public AudioMixerGroup MusicMixer;

  public static SoundManager i => SoundManager.loader.Get();

  public static async UniTask Preload(CancellationToken cancel)
  {
    await SoundManager.loader.Load(cancel);
  }

  public MusicManager Music => this.musicManager;

  public AudioMixerVolume Volumes => this.mixerVolume;

  public static void PlayEffectOneShot(AudioClip audioClip)
  {
    SoundManager.i.effectsSource.PlayOneShot(audioClip);
  }

  public static void PlayInterfaceOneShot(AudioClip audioClip)
  {
    SoundManager.i.interfaceSource.PlayOneShot(audioClip);
  }

  public static void PlayRadarWarningOneShot(AudioClip audioClip)
  {
    SoundManager.i.radarWarningSource.PlayOneShot(audioClip);
  }

  public static void PlayMenuOneShot(AudioClip audioClip)
  {
    SoundManager.i.menuSource.PlayOneShot(audioClip);
  }

  public static void PlayEffect(AudioClip audioClip)
  {
    SoundManager.PlaySound(SoundManager.i.effectsSource, audioClip);
  }

  public static void PlayInterface(AudioClip audioClip)
  {
    SoundManager.PlaySound(SoundManager.i.interfaceSource, audioClip);
  }

  public static void PlayMenu(AudioClip audioClip)
  {
    SoundManager.PlaySound(SoundManager.i.menuSource, audioClip);
  }

  public static void PlaySound(AudioSource source, AudioClip audioClip)
  {
    if ((UnityEngine.Object) source == (UnityEngine.Object) SoundManager.i.interfaceSource && PlayerSettings.cinematicMode)
      return;
    if (source.isPlaying)
      source.Stop();
    source.time = 0.0f;
    source.clip = audioClip;
    source.Play();
  }
}

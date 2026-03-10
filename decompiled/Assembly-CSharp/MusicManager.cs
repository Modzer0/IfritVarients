// Decompiled with JetBrains decompiler
// Type: MusicManager
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

#nullable disable
public class MusicManager : MonoBehaviour
{
  [SerializeField]
  private AudioSource currentSource;
  [SerializeField]
  private AudioSource fadeSource;
  private bool isFading;
  private float currentClipPriority;
  private List<AudioClip> clipsPlayed = new List<AudioClip>();
  [SerializeField]
  private AudioClip menuMusic;

  public static MusicManager i => SoundManager.i.Music;

  private void Awake()
  {
    this.currentSource.ignoreListenerPause = true;
    this.fadeSource.ignoreListenerPause = true;
  }

  public bool IsPlaying() => this.currentSource.isPlaying;

  public void PlayMenuMusic()
  {
    if (this.IsPlaying())
      return;
    this.PlayMusic(this.menuMusic, false);
  }

  public void PlayMusic(AudioClip audioClip, bool repeat)
  {
    if (GameManager.IsHeadless)
      return;
    this.currentSource.time = 0.0f;
    this.currentSource.clip = audioClip;
    this.currentSource.loop = repeat;
    this.currentSource.volume = 1f;
    this.currentSource.Play();
  }

  public static void ResetPlayedMusic() => MusicManager.i.clipsPlayed.Clear();

  public void CrossFadeMusic(
    AudioClip audioClip,
    float fadeOutTime,
    float fadeInTime,
    bool repeat,
    bool allowReplay,
    bool replacePlaying,
    float priority = 0.0f)
  {
    if (GameManager.IsHeadless || (Object) audioClip == (Object) null || !replacePlaying && this.IsPlaying() || this.IsPlaying() && (double) priority < (double) this.currentClipPriority)
      return;
    if (this.isFading)
    {
      Debug.LogWarning((object) "Already CrossFading");
    }
    else
    {
      if (!allowReplay)
      {
        if (this.HasPlayedMusic(audioClip))
          return;
        this.clipsPlayed.Add(audioClip);
      }
      this.fadeSource.time = 0.0f;
      this.fadeSource.clip = audioClip;
      this.fadeSource.loop = repeat;
      this.fadeSource.volume = 0.0f;
      if ((Object) audioClip != (Object) null)
        this.fadeSource.Play();
      this.CrossFade(fadeOutTime, fadeInTime).Forget();
    }
  }

  public bool HasPlayedMusic(AudioClip clip) => this.clipsPlayed.Contains(clip);

  public void QueueMusicClip(AudioClip audioClip, float clipPriority)
  {
    if (GameManager.IsHeadless)
      return;
    Debug.Log((object) ("Queuing audioClip " + audioClip.name));
    this.QueueMusic(audioClip, clipPriority).Forget();
  }

  private async UniTask QueueMusic(AudioClip audioClip, float priority)
  {
    while (this.IsPlaying())
      await UniTask.Delay(1000);
    if (GameManager.gameResolution != GameResolution.Ongoing)
      return;
    this.currentClipPriority = priority;
    this.PlayMusic(audioClip, false);
  }

  private async UniTask CrossFade(float outTime, float inTime)
  {
    MusicManager musicManager = this;
    musicManager.isFading = true;
    try
    {
      CancellationToken cancel = musicManager.destroyCancellationToken;
      float outVolume = 1f;
      float inVolume = 0.0f;
      while ((double) outVolume > 0.0 || (double) inVolume < 1.0)
      {
        await UniTask.Yield();
        if (cancel.IsCancellationRequested)
          return;
        outVolume = (double) outTime == 0.0 ? 0.0f : outVolume - Time.deltaTime / outTime;
        inVolume = (double) inTime == 0.0 ? 1f : inVolume + Time.deltaTime / inTime;
        musicManager.fadeSource.volume = inVolume;
        musicManager.currentSource.volume = outVolume;
      }
      AudioSource currentSource = musicManager.currentSource;
      AudioSource fadeSource = musicManager.fadeSource;
      musicManager.fadeSource = currentSource;
      musicManager.currentSource = fadeSource;
      musicManager.currentSource.volume = 1f;
      musicManager.fadeSource.volume = 0.0f;
      musicManager.fadeSource.Stop();
      cancel = new CancellationToken();
    }
    finally
    {
      musicManager.isFading = false;
    }
  }

  public void FadeOut(float time)
  {
    if (GameManager.IsHeadless || (Object) this.currentSource == (Object) null)
      return;
    this.FadeoutSource(this.currentSource, time).Forget();
  }

  private async UniTask FadeoutSource(AudioSource source, float time)
  {
    CancellationToken cancel = this.destroyCancellationToken;
    for (; (double) source.volume > 0.0; source.volume -= Time.deltaTime / time)
    {
      await UniTask.Yield();
      if (cancel.IsCancellationRequested)
      {
        cancel = new CancellationToken();
        return;
      }
    }
    source.Stop();
    cancel = new CancellationToken();
  }

  public void StopMusic()
  {
    this.currentClipPriority = 0.0f;
    this.currentSource.Stop();
  }
}

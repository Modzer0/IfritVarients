// Decompiled with JetBrains decompiler
// Type: KillDisplay
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

#nullable disable
public class KillDisplay : SceneSingleton<KillDisplay>
{
  [SerializeField]
  private Text killText;
  [SerializeField]
  private Text rankText;
  [SerializeField]
  private Image rankBackground;
  [SerializeField]
  private AudioClip[] killAudioEffects;
  [SerializeField]
  private AudioClip rankIncreaseClip;
  [SerializeField]
  private float maxKillHeat;
  [SerializeField]
  private AudioClip killsong;
  private float killHeat;
  private float killDisplayTimer;
  private float rankDisplayTimer;
  private float killsongLastPlayed = -60f;

  private void OnEnable()
  {
    this.rankText.enabled = false;
    this.rankBackground.enabled = false;
    this.killsongLastPlayed = -60f;
  }

  public void DisplayKill(
    PersistentUnit killedUnit,
    float creditGiven,
    FactionHQ.RewardType actionType)
  {
    this.killDisplayTimer = 5f;
    this.killText.enabled = true;
    if (actionType == FactionHQ.RewardType.Kill && killedUnit != null)
    {
      this.killHeat += creditGiven;
      this.killText.text = $"{killedUnit.unitName} +{creditGiven.ToString("F1")}";
      SoundManager.PlayInterfaceOneShot(this.killAudioEffects[Mathf.FloorToInt(Mathf.Clamp(this.killHeat / this.maxKillHeat, 0.0f, 0.99f) * (float) this.killAudioEffects.Length)]);
      this.enabled = true;
      if ((double) this.killHeat < (double) this.maxKillHeat || (double) Time.timeSinceLevelLoad - (double) this.killsongLastPlayed <= 60.0)
        return;
      this.killsongLastPlayed = Time.timeSinceLevelLoad;
      MusicManager.i.CrossFadeMusic(this.killsong, 2f, 0.0f, false, true, false);
    }
    else
    {
      switch (actionType)
      {
        case FactionHQ.RewardType.Recon:
          this.killText.text = "Detected units +" + creditGiven.ToString("F1");
          this.enabled = true;
          break;
        case FactionHQ.RewardType.Jamming:
          this.killText.text = "Jamming radar +" + creditGiven.ToString("F1");
          this.enabled = true;
          break;
        case FactionHQ.RewardType.Supply:
          this.killText.text = "Resupplied units +" + creditGiven.ToString("F1");
          this.enabled = true;
          break;
        case FactionHQ.RewardType.Refuel:
          this.killText.text = "Resupplied units +" + creditGiven.ToString("F1");
          this.enabled = true;
          break;
        case FactionHQ.RewardType.Repair:
          this.killText.text = "Repaired unit +" + creditGiven.ToString("F1");
          this.enabled = true;
          break;
        case FactionHQ.RewardType.RescuePilots:
          this.killText.text = "Rescued pilots +" + creditGiven.ToString("F1");
          this.enabled = true;
          break;
        case FactionHQ.RewardType.CapturePilots:
          this.killText.text = "Captured pilots +" + creditGiven.ToString("F1");
          this.enabled = true;
          break;
        case FactionHQ.RewardType.CaptureLocation:
          this.killText.text = "Captured location +" + creditGiven.ToString("F1");
          this.enabled = true;
          break;
      }
    }
  }

  public void DisplayBonus(float scoreAwarded)
  {
    if ((double) scoreAwarded == 0.0)
      return;
    this.killDisplayTimer = 5f;
    this.killText.enabled = true;
    this.killText.text = $"Successful Sortie + {scoreAwarded:F1}";
    this.enabled = true;
  }

  public static void FlashRank(int rank)
  {
    if ((Object) SceneSingleton<KillDisplay>.i == (Object) null)
      Object.Instantiate<GameObject>(GameAssets.i.killDisplay, SceneSingleton<GameplayUI>.i.gameplayCanvas.transform);
    SceneSingleton<KillDisplay>.i.DelayFlashRank(rank).Forget();
  }

  public static void FlashNewAircraft(AircraftDefinition aircraftDefinition)
  {
    if ((Object) SceneSingleton<KillDisplay>.i == (Object) null)
      Object.Instantiate<GameObject>(GameAssets.i.killDisplay, SceneSingleton<GameplayUI>.i.gameplayCanvas.transform);
    SceneSingleton<KillDisplay>.i.DelayFlashNewAircraft(aircraftDefinition).Forget();
  }

  private async UniTask DelayFlashRank(int rank)
  {
    KillDisplay killDisplay = this;
    CancellationToken cancel = killDisplay.destroyCancellationToken;
    await UniTask.Delay(2000);
    if (cancel.IsCancellationRequested)
    {
      cancel = new CancellationToken();
    }
    else
    {
      killDisplay.enabled = true;
      killDisplay.rankDisplayTimer = 5f;
      killDisplay.rankText.enabled = true;
      killDisplay.rankBackground.enabled = true;
      SoundManager.PlayInterfaceOneShot(killDisplay.rankIncreaseClip);
      killDisplay.rankText.text = $"RANK {rank}";
      cancel = new CancellationToken();
    }
  }

  private async UniTask DelayFlashNewAircraft(AircraftDefinition aircraftDefinition)
  {
    KillDisplay killDisplay = this;
    if (killDisplay.destroyCancellationToken.IsCancellationRequested)
      return;
    killDisplay.enabled = true;
    killDisplay.rankDisplayTimer = 5f;
    killDisplay.rankText.enabled = true;
    killDisplay.rankBackground.enabled = true;
    SoundManager.PlayInterfaceOneShot(killDisplay.rankIncreaseClip);
    killDisplay.rankText.text = "+1 " + aircraftDefinition.unitName;
  }

  private void Update()
  {
    if ((double) this.killDisplayTimer <= 0.0 && (double) this.rankDisplayTimer <= 0.0 && (double) this.killHeat <= 0.0)
    {
      this.enabled = false;
      this.killHeat = 0.0f;
    }
    else
    {
      this.killHeat -= Time.deltaTime;
      this.killDisplayTimer -= Time.deltaTime;
      if ((double) this.killDisplayTimer <= 0.0)
        this.killText.enabled = false;
      this.rankDisplayTimer -= Time.deltaTime;
      if ((double) this.rankDisplayTimer > 0.0)
        return;
      this.rankText.enabled = false;
      this.rankBackground.enabled = false;
    }
  }

  public enum MessageType
  {
    Kill,
    Recon,
    Jamming,
    Supply,
    Rescue,
    CapturePilots,
    CaptureLocation,
  }
}

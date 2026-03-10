// Decompiled with JetBrains decompiler
// Type: ChatSettings
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

#nullable disable
public class ChatSettings : MonoBehaviour
{
  [SerializeField]
  private Toggle chatToggle;
  [SerializeField]
  private Toggle filterToggle;
  [SerializeField]
  private Toggle ttsToggle;
  [SerializeField]
  private Slider ttsSpeed;
  [SerializeField]
  private Slider ttsVolume;
  [SerializeField]
  private Button testTts;

  public void Awake()
  {
    this.chatToggle.onValueChanged.AddListener(new UnityAction<bool>(this.ChatToggleChanged));
    this.filterToggle.onValueChanged.AddListener(new UnityAction<bool>(this.FilterToggleChanged));
    this.ttsToggle.onValueChanged.AddListener(new UnityAction<bool>(this.TtsToggleChanged));
    this.ttsSpeed.minValue = -10f;
    this.ttsSpeed.maxValue = 10f;
    this.ttsSpeed.wholeNumbers = true;
    this.ttsSpeed.onValueChanged.AddListener(new UnityAction<float>(this.TtsSpeedChanged));
    this.ttsVolume.minValue = 0.0f;
    this.ttsVolume.maxValue = 100f;
    this.ttsVolume.wholeNumbers = true;
    this.ttsVolume.onValueChanged.AddListener(new UnityAction<float>(this.TtsVolumeChanged));
    this.testTts.onClick.AddListener(new UnityAction(this.TestTts));
    this.chatToggle.SetIsOnWithoutNotify(PlayerSettings.chatEnabled);
    this.filterToggle.SetIsOnWithoutNotify(PlayerSettings.chatFilter);
    this.ttsToggle.SetIsOnWithoutNotify(PlayerSettings.chatTts);
    this.ttsSpeed.SetValueWithoutNotify((float) PlayerSettings.chatTtsSpeed);
    this.ttsVolume.SetValueWithoutNotify((float) PlayerSettings.chatTtsVolume);
  }

  private void ChatToggleChanged(bool on)
  {
    PlayerSettings.chatEnabled = on;
    PlayerPrefs.SetInt("ChatEnabled", on ? 1 : 0);
  }

  private void FilterToggleChanged(bool on)
  {
    PlayerSettings.chatFilter = on;
    PlayerPrefs.SetInt("ChatFilter", on ? 1 : 0);
  }

  private void TtsToggleChanged(bool on)
  {
    PlayerSettings.chatTts = on;
    PlayerPrefs.SetInt("ChatTts", on ? 1 : 0);
  }

  private void TtsSpeedChanged(float speed)
  {
    PlayerSettings.chatTtsSpeed = (int) speed;
    PlayerPrefs.SetInt("ChatTtsSpeed", (int) speed);
  }

  private void TtsVolumeChanged(float volume)
  {
    PlayerSettings.chatTtsVolume = (int) volume;
    PlayerPrefs.SetInt("ChatTtsVolume", (int) volume);
  }

  private void TestTts()
  {
    UniTask.Void((Func<UniTaskVoid>) (async () => await WindowsTTS.SpeakAsync(PlayerSettings.chatTtsSpeed, PlayerSettings.chatTtsVolume, "Player said: Welcome to Nuclear Option!", PlayerSettings.chatFilter)));
  }
}

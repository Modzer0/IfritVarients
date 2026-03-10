// Decompiled with JetBrains decompiler
// Type: AudioMenu
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

#nullable disable
public class AudioMenu : MonoBehaviour
{
  [SerializeField]
  private Slider masterVolumeSlider;
  [SerializeField]
  private Text masterVolumeValue;
  [SerializeField]
  private Slider musicVolumeSlider;
  [SerializeField]
  private Text musicVolumeValue;
  [SerializeField]
  private Slider interfaceVolumeSlider;
  [SerializeField]
  private Text interfaceVolumeValue;
  [SerializeField]
  private Slider effectsVolumeSlider;
  [SerializeField]
  private Text effectsVolumeValue;
  [SerializeField]
  private Slider menuVolumeSlider;
  [SerializeField]
  private Text menuVolumeValue;
  [SerializeField]
  private Slider radarWarningVolumeSlider;
  [SerializeField]
  private Text radarWarningVolumeValue;
  [SerializeField]
  private Slider missileAlertVolumeSlider;
  [SerializeField]
  private Text missileAlertVolumeValue;
  [SerializeField]
  private Slider jammedNoiseVolumeSlider;
  [SerializeField]
  private Text jammedNoiseVolumeValue;

  public void Start()
  {
    AudioMenu.SetupSlider(this.masterVolumeSlider, this.masterVolumeValue, AudioMixerVolume.Master);
    AudioMenu.SetupSlider(this.musicVolumeSlider, this.musicVolumeValue, AudioMixerVolume.Music);
    AudioMenu.SetupSlider(this.interfaceVolumeSlider, this.interfaceVolumeValue, AudioMixerVolume.Interface);
    AudioMenu.SetupSlider(this.effectsVolumeSlider, this.effectsVolumeValue, AudioMixerVolume.Effects);
    AudioMenu.SetupSlider(this.menuVolumeSlider, this.menuVolumeValue, AudioMixerVolume.Menu);
    AudioMenu.SetupSlider(this.radarWarningVolumeSlider, this.radarWarningVolumeValue, AudioMixerVolume.RadarWarning);
    AudioMenu.SetupSlider(this.missileAlertVolumeSlider, this.missileAlertVolumeValue, AudioMixerVolume.MissileAlert);
    AudioMenu.SetupSlider(this.jammedNoiseVolumeSlider, this.jammedNoiseVolumeValue, AudioMixerVolume.JammedNoise);
  }

  private static void SetupSlider(Slider slider, Text text, string channel)
  {
    float pref = AudioMixerVolume.GetPref(channel);
    slider.SetValueWithoutNotify(pref);
    AudioMenu.SetFormattedText(text, pref);
    slider.onValueChanged.AddListener((UnityAction<float>) (newValue =>
    {
      AudioMixerVolume.SetValue(channel, newValue);
      AudioMenu.SetFormattedText(text, newValue);
    }));
  }

  private static void SetFormattedText(Text text, float value)
  {
    text.text = (value * 100f).ToString("F0") + "%";
  }
}

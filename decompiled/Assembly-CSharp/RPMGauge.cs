// Decompiled with JetBrains decompiler
// Type: RPMGauge
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#nullable disable
public class RPMGauge : HUDApp
{
  [SerializeField]
  private string[] sourceNames;
  [SerializeField]
  private Text rpmText;
  [SerializeField]
  private Image textBox;
  [SerializeField]
  private Gradient rpmColor;
  [SerializeField]
  private AudioClip warningSound;
  [SerializeField]
  private float warningVolume;
  [SerializeField]
  private float warningThreshold;
  [SerializeField]
  private float maxRPM;
  private Aircraft aircraft;
  private List<IEngine> sources;
  private AudioSource warningSource;
  private float displayedRPM;

  public override void Initialize(Aircraft aircraft)
  {
    this.aircraft = aircraft;
    this.sources = new List<IEngine>();
    foreach (string sourceName in this.sourceNames)
    {
      foreach (UnitPart unitPart in aircraft.partLookup)
      {
        if ((Object) unitPart != (Object) null && unitPart.gameObject.name == sourceName)
        {
          this.sources.Add(unitPart.gameObject.GetComponent<IEngine>());
          break;
        }
      }
    }
    this.warningSource = aircraft.gameObject.AddComponent<AudioSource>();
    this.warningSource.outputAudioMixerGroup = SoundManager.i.InterfaceMixer;
    this.warningSource.clip = this.warningSound;
    this.warningSource.loop = true;
    this.warningSource.volume = this.warningVolume;
    this.warningSource.spatialBlend = 0.0f;
    this.warningSource.dopplerLevel = 0.0f;
    this.warningSource.bypassEffects = true;
  }

  private void OnDestroy()
  {
    if (!((Object) this.warningSource != (Object) null))
      return;
    this.warningSource.Stop();
  }

  public override void Refresh()
  {
    this.displayedRPM = 0.0f;
    foreach (IEngine source in this.sources)
      this.displayedRPM += source.GetRPM();
    this.displayedRPM /= (float) this.sources.Count;
    this.rpmText.text = $"RPM {this.displayedRPM:F0}";
    if ((Object) this.warningSource != (Object) null)
    {
      if ((double) this.displayedRPM < (double) this.warningThreshold && (double) this.aircraft.radarAlt > (double) this.aircraft.definition.spawnOffset.y + 1.0)
      {
        if (!this.warningSource.isPlaying)
          this.warningSource.Play();
        this.warningSource.pitch = this.displayedRPM / this.warningThreshold;
      }
      else if (this.warningSource.isPlaying)
        this.warningSource.Stop();
    }
    Color color = this.rpmColor.Evaluate((float) (((double) this.displayedRPM - (double) this.warningThreshold) / ((double) this.maxRPM - (double) this.warningThreshold)));
    this.rpmText.color = color;
    this.textBox.color = color;
  }
}

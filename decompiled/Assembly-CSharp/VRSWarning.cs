// Decompiled with JetBrains decompiler
// Type: VRSWarning
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System.Collections.Generic;
using UnityEngine;

#nullable disable
public class VRSWarning : HUDApp
{
  [SerializeField]
  private string[] sourceNames;
  [SerializeField]
  private GameObject warningObject;
  [SerializeField]
  private AudioClip warningSound;
  [SerializeField]
  private float warningVolume;
  [SerializeField]
  private float warningThreshold;
  private Aircraft aircraft;
  private List<RotorShaft> sources;
  private float VRSFactor;
  private bool inVRS;
  private AudioSource warningSource;

  public override void Initialize(Aircraft aircraft)
  {
    this.aircraft = aircraft;
    this.sources = new List<RotorShaft>();
    foreach (string sourceName in this.sourceNames)
    {
      foreach (UnitPart unitPart in aircraft.partLookup)
      {
        if ((Object) unitPart != (Object) null && unitPart.gameObject.name == sourceName)
        {
          this.sources.Add(unitPart.gameObject.GetComponent<RotorShaft>());
          break;
        }
      }
    }
    this.warningObject.SetActive(false);
    this.warningSource = aircraft.gameObject.AddComponent<AudioSource>();
    this.warningSource.outputAudioMixerGroup = SoundManager.i.InterfaceMixer;
    this.warningSource.clip = this.warningSound;
    this.warningSource.loop = false;
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
    this.VRSFactor = 0.0f;
    foreach (RotorShaft source in this.sources)
      this.VRSFactor += source.GetVRSFactor();
    this.VRSFactor /= (float) this.sources.Count;
    if ((double) this.VRSFactor > (double) this.warningThreshold && (double) this.aircraft.radarAlt > (double) this.aircraft.definition.spawnOffset.y + 1.0)
    {
      if (this.inVRS)
        return;
      this.warningObject.SetActive(true);
      if (!this.warningSource.isPlaying)
        this.warningSource.Play();
      this.inVRS = true;
    }
    else
    {
      if (!this.inVRS)
        return;
      this.warningObject.SetActive(false);
      this.inVRS = false;
    }
  }
}

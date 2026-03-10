// Decompiled with JetBrains decompiler
// Type: RadarJammer
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System.Collections.Generic;
using UnityEngine;

#nullable disable
public class RadarJammer : Countermeasure
{
  [SerializeField]
  private float powerUsage;
  [SerializeField]
  private float capacitance = 100f;
  [SerializeField]
  private float jammingIntensity;
  [SerializeField]
  private float dischargeVolume;
  [SerializeField]
  private AudioClip dischargeSound;
  [SerializeField]
  [Range(0.0f, 1f)]
  private float volumeMultiplier;
  private float lastActivated;
  private float jamIntensityPrev;
  private float jamIntensityCurrent;
  private AudioSource dischargeSource;
  private PowerSupply powerSupply;

  protected override void Awake()
  {
    this.ammo = 1;
    if ((Object) this.aircraft != (Object) null)
    {
      this.powerSupply = this.aircraft.GetPowerSupply();
      this.powerSupply.AddUser();
      this.powerSupply.ModifyCapacitance(this.capacitance);
    }
    base.Awake();
  }

  public override List<string> GetThreatTypes()
  {
    if (this.threatTypes == null)
      this.threatTypes = new List<string>()
      {
        "ARH",
        "SARH"
      };
    return this.threatTypes;
  }

  public override void AttachToUnit(Aircraft aircraft)
  {
    this.powerSupply = aircraft.GetPowerSupply();
    base.AttachToUnit(aircraft);
    this.powerSupply.AddUser();
  }

  public override void Fire()
  {
    this.enabled = true;
    this.lastActivated = Time.timeSinceLevelLoad;
    float num = this.powerSupply.DrawPower(this.powerUsage);
    this.jamIntensityCurrent = this.jammingIntensity * num / this.powerUsage;
    this.aircraft.AddJammingIntensity(this.jamIntensityCurrent - this.jamIntensityPrev);
    this.jamIntensityPrev = this.jamIntensityCurrent;
    if ((Object) this.dischargeSource == (Object) null)
    {
      this.dischargeSource = this.gameObject.AddComponent<AudioSource>();
      this.dischargeSource.outputAudioMixerGroup = SoundManager.i.InterfaceMixer;
      this.dischargeSource.clip = this.dischargeSound;
      this.dischargeSource.spatialBlend = 1f;
      this.dischargeSource.dopplerLevel = 0.0f;
      this.dischargeSource.spread = 5f;
      this.dischargeSource.maxDistance = 40f;
      this.dischargeSource.minDistance = 5f;
    }
    this.dischargeSource.pitch = num / this.powerUsage;
    this.dischargeSource.volume = num / this.powerUsage * this.volumeMultiplier;
    if (this.dischargeSource.isPlaying)
      return;
    this.dischargeSource.Play();
  }

  private void Update()
  {
    if ((double) Time.timeSinceLevelLoad - (double) this.lastActivated <= 0.10000000149011612 || !((Object) this.aircraft != (Object) null))
      return;
    this.aircraft.AddJammingIntensity(-this.jamIntensityPrev);
    this.jamIntensityPrev = 0.0f;
    this.enabled = false;
    if (!((Object) SceneSingleton<CombatHUD>.i != (Object) null) || !((Object) this.aircraft == (Object) SceneSingleton<CombatHUD>.i.aircraft))
      return;
    this.UpdateHUD();
  }

  public override void UpdateHUD()
  {
    SceneSingleton<CombatHUD>.i.DisplayCountermeasures(this.displayName, this.displayImage, (double) Time.timeSinceLevelLoad - (double) this.lastActivated < 0.05000000074505806);
    if (!((Object) this.dischargeSource != (Object) null) || !this.dischargeSource.isPlaying || (double) Time.timeSinceLevelLoad - (double) this.lastActivated <= 0.05000000074505806)
      return;
    this.dischargeSource.Stop();
  }

  public float GetMaxJammingIntensity() => this.jammingIntensity;

  protected override void OnDestroy()
  {
    base.OnDestroy();
    if (!((Object) this.powerSupply != (Object) null))
      return;
    this.powerSupply.RemoveUser();
    this.powerSupply.ModifyCapacitance(-this.capacitance);
  }
}

// Decompiled with JetBrains decompiler
// Type: WeaponIndicator
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;
using UnityEngine.UI;

#nullable disable
public class WeaponIndicator : HUDApp
{
  [SerializeField]
  private Image weaponImage;
  [SerializeField]
  private Text weaponName;
  [SerializeField]
  private Text weaponAmmo;
  private bool safetyOn;
  private Aircraft aircraft;

  public override void Initialize(Aircraft aircraft) => this.aircraft = aircraft;

  public override void RefreshSettings()
  {
    base.RefreshSettings();
    this.weaponName.fontSize = this.fontSize;
    this.weaponAmmo.fontSize = this.fontSize;
    this.Show(PlayerSettings.hudWeapons);
    this.Refresh();
  }

  public override void Refresh()
  {
    if ((UnityEngine.Object) this.aircraft == (UnityEngine.Object) null || !PlayerSettings.hudWeapons)
      return;
    if (this.aircraft.weaponManager.currentWeaponStation != null)
    {
      this.weaponImage.color = Color.green;
      this.weaponName.color = Color.green;
      this.weaponAmmo.color = Color.green;
      if (this.weaponName.text != this.aircraft.weaponManager.currentWeaponStation.WeaponInfo.shortName)
      {
        if (!this.weaponImage.enabled)
          this.weaponImage.enabled = true;
        this.weaponImage.sprite = this.aircraft.weaponManager.currentWeaponStation.WeaponInfo.weaponIcon;
        this.weaponName.text = this.aircraft.weaponManager.currentWeaponStation.WeaponInfo.shortName;
      }
      if (!this.aircraft.weaponManager.currentWeaponStation.WeaponInfo.energy)
      {
        int ammo = this.aircraft.weaponManager.currentWeaponStation.Ammo;
        this.weaponAmmo.text = $"{this.aircraft.weaponManager.currentWeaponStation.Ammo}";
        if ((double) ammo == 0.0)
        {
          this.weaponImage.color = Color.red;
          this.weaponName.color = Color.red;
          this.weaponAmmo.color = Color.red;
        }
      }
      else
      {
        float charge = this.aircraft.GetPowerSupply().GetCharge();
        this.weaponAmmo.text = $"{(ValueType) (float) (100.0 * (double) charge):F0}";
        if ((double) charge == 0.0)
          this.weaponAmmo.color = Color.red;
      }
      if (this.aircraft.weaponManager.currentWeaponStation.SafetyIsOn(this.aircraft))
      {
        this.weaponImage.color = Color.grey;
        this.weaponName.color = Color.grey;
        this.weaponAmmo.color = Color.grey;
        if (!this.safetyOn)
          SceneSingleton<AircraftActionsReport>.i.ReportText("Weapon safety lock <b>Enabled</b>", 5f);
        this.safetyOn = true;
      }
      else
      {
        if (!this.safetyOn)
          return;
        SceneSingleton<AircraftActionsReport>.i.ReportText("Weapon safety lock <b>Disabled</b>", 5f);
        this.safetyOn = false;
      }
    }
    else
    {
      this.weaponImage.enabled = false;
      this.weaponName.text = "";
      this.weaponAmmo.text = "";
    }
  }

  public void Show(bool arg)
  {
    this.weaponImage.enabled = arg;
    this.weaponName.enabled = arg;
    this.weaponAmmo.enabled = arg;
  }
}

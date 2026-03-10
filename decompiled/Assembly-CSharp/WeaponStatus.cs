// Decompiled with JetBrains decompiler
// Type: WeaponStatus
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;
using UnityEngine.UI;

#nullable disable
public class WeaponStatus : MonoBehaviour
{
  [SerializeField]
  private Image weaponImage;
  [SerializeField]
  private Image reloadProgressImage;
  [SerializeField]
  private Image safetyImage;
  [SerializeField]
  private GameObject weaponPanel;
  [SerializeField]
  private Text nameText;
  [SerializeField]
  private Text ammoText;
  [SerializeField]
  private RectTransform reloadProgress;
  [SerializeField]
  private RectTransform panel;
  private WeaponStation weaponStation;
  private Aircraft aircraft;
  private Color weaponBaseColor;
  private Color weaponFlashColor;
  private float flashAmount;

  private void Awake()
  {
    this.enabled = false;
    this.weaponFlashColor = this.weaponBaseColor;
  }

  public void SetCurrentStation(Aircraft aircraft, WeaponStation weaponStation)
  {
    if (this.weaponStation != null)
      this.weaponStation.OnUpdated -= new Action(this.WeaponStatus_OnStationUpdated);
    if ((UnityEngine.Object) this.aircraft != (UnityEngine.Object) null)
      this.aircraft.onSetGear -= new Action<Aircraft.OnSetGear>(this.WeaponStatus_OnSetGear);
    if (weaponStation == null)
      return;
    this.aircraft = aircraft;
    this.weaponStation = weaponStation;
    aircraft.onSetGear += new Action<Aircraft.OnSetGear>(this.WeaponStatus_OnSetGear);
    weaponStation.OnUpdated += new Action(this.WeaponStatus_OnStationUpdated);
    this.UpdateDisplay(weaponStation);
    this.UpdateSafety();
  }

  private void WeaponStatus_OnStationUpdated()
  {
    this.flashAmount = 1f;
    this.UpdateDisplay(this.weaponStation);
    this.enabled = true;
  }

  private void WeaponStatus_OnSetGear(Aircraft.OnSetGear e) => this.UpdateSafety();

  private void UpdateSafety()
  {
    if (this.weaponStation.SafetyIsOn(this.aircraft))
    {
      if (this.safetyImage.enabled)
        return;
      this.safetyImage.enabled = true;
      SceneSingleton<AircraftActionsReport>.i.ReportText("Weapon safety lock <b>Enabled</b>", 5f);
    }
    else
    {
      if (!this.safetyImage.enabled)
        return;
      this.safetyImage.enabled = false;
      SceneSingleton<AircraftActionsReport>.i.ReportText("Weapon safety lock <b>Disabled</b>", 5f);
    }
  }

  public void SetVisible(bool visibility) => this.weaponPanel.SetActive(visibility);

  public void SetSafety(bool safety) => this.safetyImage.enabled = safety;

  public void UpdateDisplay(WeaponStation weaponStation)
  {
    if (weaponStation == null)
      return;
    this.weaponImage.sprite = weaponStation.WeaponInfo.weaponIcon;
    this.ammoText.text = weaponStation.GetAmmoReadout();
    this.nameText.text = weaponStation.Cargo ? $"Cargo ({weaponStation.WeaponInfo.weaponName})" : weaponStation.WeaponInfo.weaponName;
    this.weaponBaseColor = weaponStation.Ammo > 0 ? Color.green : Color.red;
    this.weaponImage.color = Color.Lerp(this.weaponBaseColor, Color.green * 0.5f + Color.red, this.flashAmount);
    this.ammoText.color = this.weaponImage.color;
    this.nameText.color = this.weaponImage.color;
    if (weaponStation.Reloading)
    {
      this.enabled = true;
      Vector2 vector2 = new Vector2(this.panel.sizeDelta.x - 10f, this.reloadProgress.sizeDelta.y);
      this.reloadProgressImage.enabled = true;
      this.weaponImage.color = Color.green * 0.5f + Color.red;
      this.reloadProgress.sizeDelta = new Vector2(vector2.x, vector2.y);
    }
    else
      this.reloadProgressImage.enabled = false;
  }

  private void Update()
  {
    if ((double) this.flashAmount > 0.0)
      this.flashAmount -= 5f * Time.deltaTime;
    this.UpdateDisplay(this.weaponStation);
    if (!this.weaponStation.Reloading)
    {
      if ((double) this.flashAmount <= 0.0)
        this.enabled = false;
      this.reloadProgressImage.enabled = false;
    }
    else
    {
      float reloadStatusMax = this.weaponStation.GetReloadStatusMax();
      Vector2 vector2 = new Vector2(this.panel.sizeDelta.x - 10f, this.reloadProgress.sizeDelta.y);
      this.reloadProgressImage.enabled = true;
      this.reloadProgress.sizeDelta = new Vector2(vector2.x * reloadStatusMax, vector2.y);
    }
  }
}

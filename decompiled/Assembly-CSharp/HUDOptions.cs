// Decompiled with JetBrains decompiler
// Type: HUDOptions
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;
using UnityEngine;

#nullable disable
public class HUDOptions : SceneSingleton<HUDOptions>
{
  public HUDOptions.HUDMode currentMode;
  public MFDScreen screen;
  public List<HUDOptions_ToggleButton> listModes = new List<HUDOptions_ToggleButton>();
  public List<HUDOptions_ToggleButton> listVehicleTypes = new List<HUDOptions_ToggleButton>();
  public List<HUDOptions_ToggleButton> listBuildingTypes = new List<HUDOptions_ToggleButton>();
  public List<HUDOptions_Item> listOptionItems = new List<HUDOptions_Item>();
  private float lastRefresh;
  private float refreshDelay = 1f;
  private bool needUpdateIcon;

  public event Action OnApplyOptions;

  private void Start()
  {
    foreach (HUDOptions_Item listOptionItem in this.listOptionItems)
      listOptionItem.Initialize();
    this.needUpdateIcon = true;
  }

  public void ApplyHUDSettings()
  {
    Action onApplyOptions = this.OnApplyOptions;
    if (onApplyOptions == null)
      return;
    onApplyOptions();
  }

  public void AutomaticToggle(WeaponStation newWeaponStation, bool safety)
  {
    if (newWeaponStation == null)
    {
      this.ToggleButtons(this.listModes[0]);
    }
    else
    {
      WeaponInfo weaponInfo = newWeaponStation.WeaponInfo;
      if (safety)
      {
        this.ToggleButtons(this.listModes[0]);
        this.currentMode = HUDOptions.HUDMode.NAV;
      }
      else if ((double) weaponInfo.effectiveness.antiAir == 0.0 && (double) weaponInfo.effectiveness.antiSurface == 0.0 && (double) weaponInfo.effectiveness.antiMissile == 0.0 && (double) weaponInfo.effectiveness.antiRadar == 0.0)
      {
        this.ToggleButtons(this.listModes[5]);
        this.currentMode = HUDOptions.HUDMode.LOG;
      }
      else if (weaponInfo.gun)
      {
        this.ToggleButtons(this.listModes[1]);
        this.currentMode = HUDOptions.HUDMode.GUN;
      }
      else if (weaponInfo.gun || (double) weaponInfo.effectiveness.antiAir > (double) weaponInfo.effectiveness.antiSurface)
      {
        this.ToggleButtons(this.listModes[2]);
        this.currentMode = HUDOptions.HUDMode.A2A;
      }
      else if ((double) weaponInfo.effectiveness.antiSurface > (double) weaponInfo.effectiveness.antiAir)
      {
        this.ToggleButtons(this.listModes[3]);
        this.currentMode = HUDOptions.HUDMode.A2G;
      }
      else if ((double) weaponInfo.effectiveness.antiRadar > (double) weaponInfo.effectiveness.antiSurface)
      {
        this.ToggleButtons(this.listModes[4]);
        this.currentMode = HUDOptions.HUDMode.EW;
      }
      else
      {
        this.ToggleButtons(this.listModes[0]);
        this.currentMode = HUDOptions.HUDMode.NAV;
      }
    }
  }

  public void ToggleButtons(HUDOptions_ToggleButton button)
  {
    if (this.listModes.Contains(button))
    {
      for (int index = 0; index < this.listModes.Count; ++index)
      {
        if ((UnityEngine.Object) this.listModes[index] != (UnityEngine.Object) button)
          this.listModes[index].Set(false);
        else
          this.listModes[index].Set(true);
      }
    }
    else if (this.listVehicleTypes.Contains(button))
    {
      for (int index = 0; index < this.listVehicleTypes.Count; ++index)
      {
        if ((UnityEngine.Object) this.listVehicleTypes[index] != (UnityEngine.Object) button)
          this.listVehicleTypes[index].Set(false);
        else
          this.listVehicleTypes[index].Set(true);
      }
    }
    else if (this.listBuildingTypes.Contains(button))
    {
      for (int index = 0; index < this.listBuildingTypes.Count; ++index)
      {
        if ((UnityEngine.Object) this.listBuildingTypes[index] != (UnityEngine.Object) button)
          this.listBuildingTypes[index].Set(false);
        else
          this.listBuildingTypes[index].Set(true);
      }
    }
    this.NeedUpdateIcons();
  }

  public void ApplyToggleSettings(HUDOptions_ToggleButton button)
  {
    if (button.prioritySettings.Count > 0)
    {
      for (int index = 0; index < this.listOptionItems.Count; ++index)
        this.listOptionItems[index].SetPriority(button.prioritySettings[index]);
    }
    if (button.vehiclesSettings.Count > 0)
    {
      for (int index = 0; index < this.listVehicleTypes.Count; ++index)
        this.listVehicleTypes[index].Set(button.vehiclesSettings[index]);
    }
    if (button.buildingsSettings.Count > 0)
    {
      for (int index = 0; index < this.listBuildingTypes.Count; ++index)
        this.listBuildingTypes[index].Set(button.buildingsSettings[index]);
    }
    this.NeedUpdateIcons();
  }

  private void Update()
  {
    if (!this.needUpdateIcon || (double) Time.timeSinceLevelLoad <= (double) this.lastRefresh + (double) this.refreshDelay)
      return;
    this.lastRefresh = Time.timeSinceLevelLoad;
    Action onApplyOptions = this.OnApplyOptions;
    if (onApplyOptions != null)
      onApplyOptions();
    this.needUpdateIcon = false;
  }

  public void NeedUpdateIcons() => this.needUpdateIcon = true;

  public void SaveValues()
  {
    foreach (HUDOptions_ToggleButton listMode in this.listModes)
      listMode.SaveValues();
  }

  public enum HUDMode
  {
    NAV,
    GUN,
    A2A,
    A2G,
    EW,
    LOG,
  }
}

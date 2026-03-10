// Decompiled with JetBrains decompiler
// Type: SystemStatusDisplay
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;
using UnityEngine.UI;

#nullable disable
public class SystemStatusDisplay : HUDApp
{
  private Aircraft aircraft;
  [SerializeField]
  private Image systemEngineImg;
  [SerializeField]
  private Image systemFlightControlImg;
  [SerializeField]
  private Image systemRadarImg;
  [SerializeField]
  private Image systemGearImg;
  [SerializeField]
  private Image systemModeImg;
  [SerializeField]
  private Image systemFuelImg;
  [SerializeField]
  private Text systemEngineTxt;
  [SerializeField]
  private Text systemFlightControlTxt;
  [SerializeField]
  private Text systemRadarTxt;
  [SerializeField]
  private Text systemGearTxt;
  [SerializeField]
  private Text systemModeTxt;
  [SerializeField]
  private Text systemFuelTxt;

  public override void Initialize(Aircraft aircraft)
  {
    if ((UnityEngine.Object) aircraft == (UnityEngine.Object) null)
      return;
    this.aircraft = aircraft;
    aircraft.onSetGear += new Action<Aircraft.OnSetGear>(this.OnSetGear);
    if (aircraft.gearDeployed)
    {
      this.systemGearImg.color = Color.green;
      this.systemGearTxt.color = Color.green;
    }
    else
    {
      this.systemGearImg.color = Color.grey;
      this.systemGearTxt.color = Color.grey;
    }
    aircraft.onSetFlightAssist += new Action<Aircraft.OnFlightAssistToggle>(this.OnSetFlightAssist);
    if ((UnityEngine.Object) aircraft.radar != (UnityEngine.Object) null)
    {
      this.systemRadarTxt.text = "RADAR";
      this.OnSetRadar();
    }
    else
      this.systemRadarTxt.text = "OPTICAL";
    this.OnSetFuel();
    SceneSingleton<HUDOptions>.i.OnApplyOptions += new Action(this.OnHUDMode);
    foreach (Component component1 in aircraft.partLookup)
    {
      IEngine component2;
      if (component1.TryGetComponent<IEngine>(out component2))
      {
        component2.OnEngineDisable += new Action(this.OnEngineDisable);
        component2.OnEngineDamage += new Action(this.OnEngineDamage);
      }
    }
  }

  public override void RefreshSettings()
  {
    base.RefreshSettings();
    this.systemEngineTxt.fontSize = this.fontSize;
    this.systemFlightControlTxt.fontSize = this.fontSize;
    this.systemRadarTxt.fontSize = this.fontSize;
    this.systemGearTxt.fontSize = this.fontSize;
    this.systemModeTxt.fontSize = this.fontSize;
    this.systemFuelTxt.fontSize = this.fontSize;
  }

  public override void Refresh()
  {
    if ((UnityEngine.Object) this.aircraft == (UnityEngine.Object) null)
      return;
    if ((UnityEngine.Object) this.aircraft.radar != (UnityEngine.Object) null)
      this.OnSetRadar();
    this.OnSetFuel();
    this.OnHUDMode();
  }

  public void OnSetGear(Aircraft.OnSetGear onSetGear)
  {
    if (onSetGear.gearState == LandingGear.GearState.LockedRetracted)
    {
      this.systemGearImg.color = Color.grey;
      this.systemGearTxt.color = Color.grey;
    }
    else if (onSetGear.gearState == LandingGear.GearState.Extending || onSetGear.gearState == LandingGear.GearState.Retracting)
    {
      this.systemGearImg.color = Color.yellow;
      this.systemGearTxt.color = Color.yellow;
    }
    else
    {
      this.systemGearImg.color = Color.green;
      this.systemGearTxt.color = Color.green;
    }
  }

  public void OnSetFlightAssist(Aircraft.OnFlightAssistToggle onFlightAssistToggle)
  {
    if (onFlightAssistToggle.enabled)
    {
      this.systemFlightControlImg.color = Color.green;
      this.systemFlightControlTxt.color = Color.green;
    }
    else
    {
      this.systemFlightControlImg.color = Color.grey;
      this.systemFlightControlTxt.color = Color.grey;
    }
  }

  public void OnSetRadar()
  {
    if (this.aircraft.radar.activated)
    {
      this.systemRadarImg.color = Color.green;
      this.systemRadarTxt.color = Color.green;
    }
    else
    {
      this.systemRadarImg.color = Color.grey;
      this.systemRadarTxt.color = Color.grey;
    }
  }

  public void OnSetFuel()
  {
    if ((double) this.aircraft.GetFuelLevel() < 0.5)
    {
      this.systemFuelImg.color = Color.yellow;
      this.systemFuelTxt.color = Color.yellow;
    }
    else if ((double) this.aircraft.GetFuelLevel() < 0.20000000298023224)
    {
      this.systemFuelImg.color = new Color(1f, 0.5f, 0.0f);
      this.systemFuelTxt.color = new Color(1f, 0.5f, 0.0f);
    }
    if ((double) this.aircraft.GetFuelLevel() < 0.10000000149011612)
    {
      this.systemFuelImg.color = Color.red;
      this.systemFuelTxt.color = Color.red;
    }
    else
    {
      this.systemFuelImg.color = Color.green;
      this.systemFuelTxt.color = Color.green;
    }
  }

  public void OnHUDMode()
  {
    this.systemModeTxt.text = $"MODE : {SceneSingleton<HUDOptions>.i.currentMode}";
    if (SceneSingleton<HUDOptions>.i.currentMode == HUDOptions.HUDMode.NAV)
    {
      this.systemModeImg.color = Color.white;
      this.systemModeTxt.color = Color.white;
    }
    else
    {
      this.systemModeImg.color = Color.green;
      this.systemModeTxt.color = Color.green;
    }
  }

  public void OnEngineDamage()
  {
    this.systemEngineImg.color = Color.yellow;
    this.systemEngineTxt.color = Color.yellow;
  }

  public void OnEngineDisable()
  {
    this.systemEngineImg.color = Color.red;
    this.systemEngineTxt.color = Color.red;
    foreach (UnitPart unitPart in this.aircraft.partLookup)
    {
      IEngine component;
      if ((UnityEngine.Object) unitPart != (UnityEngine.Object) null && unitPart.TryGetComponent<IEngine>(out component))
      {
        component.OnEngineDisable -= new Action(this.OnEngineDisable);
        component.OnEngineDamage -= new Action(this.OnEngineDamage);
      }
    }
  }

  private void OnDestroy()
  {
    if ((UnityEngine.Object) SceneSingleton<HUDOptions>.i != (UnityEngine.Object) null)
      SceneSingleton<HUDOptions>.i.OnApplyOptions -= new Action(this.OnHUDMode);
    if (!((UnityEngine.Object) this.aircraft != (UnityEngine.Object) null))
      return;
    this.aircraft.onSetGear -= new Action<Aircraft.OnSetGear>(this.OnSetGear);
    this.aircraft.onSetFlightAssist -= new Action<Aircraft.OnFlightAssistToggle>(this.OnSetFlightAssist);
    foreach (UnitPart unitPart in this.aircraft.partLookup)
    {
      IEngine component;
      if ((UnityEngine.Object) unitPart != (UnityEngine.Object) null && unitPart.TryGetComponent<IEngine>(out component))
      {
        component.OnEngineDisable -= new Action(this.OnEngineDisable);
        component.OnEngineDamage -= new Action(this.OnEngineDamage);
      }
    }
  }
}

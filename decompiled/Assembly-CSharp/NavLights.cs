// Decompiled with JetBrains decompiler
// Type: NavLights
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;

#nullable disable
public class NavLights : MonoBehaviour
{
  [SerializeField]
  private NavLights.NavLight[] navLights;
  [SerializeField]
  private Aircraft aircraft;
  private bool isOn;

  private void Awake()
  {
    foreach (NavLights.NavLight navLight in this.navLights)
      navLight.Initialize(this.aircraft, this);
  }

  public void ToggleNavLights()
  {
    foreach (NavLights.NavLight navLight in this.navLights)
      navLight.ToggleState();
  }

  public void SetState(bool newState)
  {
    if (SceneSingleton<CameraStateManager>.i.currentState == SceneSingleton<CameraStateManager>.i.selectionState || SceneSingleton<CameraStateManager>.i.currentState == SceneSingleton<CameraStateManager>.i.encyclopediaState || newState == this.isOn)
      return;
    this.isOn = newState;
    if (!((UnityEngine.Object) SceneSingleton<CombatHUD>.i != (UnityEngine.Object) null) || !((UnityEngine.Object) this.aircraft == (UnityEngine.Object) SceneSingleton<CombatHUD>.i.aircraft))
      return;
    string report = this.isOn ? "Nav Lights <b>On</b>" : "Nav Lights <b>Off</b>";
    SceneSingleton<AircraftActionsReport>.i.ReportText(report, 5f);
  }

  private enum ControlledState
  {
    Never,
    Auto,
    ForceOn,
  }

  [Serializable]
  private class NavLight
  {
    [SerializeField]
    private Renderer renderer;
    [SerializeField]
    private GameObject[] objects;
    [SerializeField]
    private Material litMaterial;
    [SerializeField]
    private UnitPart part;
    [SerializeField]
    private LandingGear gear;
    [SerializeField]
    private NavLights.ControlledState controlState;
    private Material baseMaterial;
    private Aircraft aircraft;
    private NavLights manager;
    private bool isOn;

    public void Initialize(Aircraft aircraft, NavLights navLights)
    {
      this.aircraft = aircraft;
      this.manager = navLights;
      this.baseMaterial = this.renderer.material;
      aircraft.onSetGear += new Action<Aircraft.OnSetGear>(this.NavLight_OnSetGear);
      if ((UnityEngine.Object) this.part != (UnityEngine.Object) null)
        this.part.onParentDetached += new Action<UnitPart>(this.NavLight_OnDetachedFromUnit);
      if ((UnityEngine.Object) this.gear != (UnityEngine.Object) null)
        this.gear.onGearBreak += new Action<LandingGear>(this.NavLight_OnGearBreak);
      this.isOn = false;
      this.Toggle(this.isOn);
    }

    private void NavLight_OnSetGear(Aircraft.OnSetGear e)
    {
      this.Toggle(e.gearState == LandingGear.GearState.LockedExtended || e.gearState == LandingGear.GearState.Extending);
    }

    private void Toggle(bool enabled)
    {
      this.isOn = enabled || this.controlState == NavLights.ControlledState.ForceOn;
      this.renderer.material = this.isOn ? this.litMaterial : this.baseMaterial;
      foreach (GameObject gameObject in this.objects)
        gameObject.SetActive(this.isOn);
      this.manager.SetState(this.isOn);
    }

    private void NavLight_OnGearBreak(LandingGear landingGear)
    {
      this.controlState = NavLights.ControlledState.Auto;
      this.Toggle(false);
      this.gear.onGearBreak -= new Action<LandingGear>(this.NavLight_OnGearBreak);
      if ((UnityEngine.Object) this.part != (UnityEngine.Object) null)
        this.part.onParentDetached -= new Action<UnitPart>(this.NavLight_OnDetachedFromUnit);
      this.aircraft.onSetGear -= new Action<Aircraft.OnSetGear>(this.NavLight_OnSetGear);
    }

    private void NavLight_OnDetachedFromUnit(UnitPart parentPart)
    {
      this.controlState = NavLights.ControlledState.Auto;
      this.Toggle(false);
      this.aircraft.onSetGear -= new Action<Aircraft.OnSetGear>(this.NavLight_OnSetGear);
      this.part.onParentDetached -= new Action<UnitPart>(this.NavLight_OnDetachedFromUnit);
      if (!((UnityEngine.Object) this.gear != (UnityEngine.Object) null))
        return;
      this.gear.onGearBreak -= new Action<LandingGear>(this.NavLight_OnGearBreak);
    }

    public void ToggleState()
    {
      if (this.controlState == NavLights.ControlledState.Never)
        return;
      this.controlState = this.controlState != NavLights.ControlledState.Auto ? NavLights.ControlledState.Auto : NavLights.ControlledState.ForceOn;
      if (this.aircraft.gearState != LandingGear.GearState.LockedRetracted)
        return;
      this.Toggle(!this.isOn);
    }
  }
}

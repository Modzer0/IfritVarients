// Decompiled with JetBrains decompiler
// Type: AutopilotIndicator
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;
using UnityEngine.UI;

#nullable disable
public class AutopilotIndicator : HUDApp
{
  [SerializeField]
  private Image icon;
  [SerializeField]
  private float maxSpeed = 27f;
  private Aircraft aircraft;

  public override void Initialize(Aircraft aircraft)
  {
    aircraft.GetControlsFilter().OnSetAutoHover += new Action(this.AutopilotIndicator_OnSetAutopilot);
    this.icon.enabled = aircraft.GetControlsFilter().IsAutoHoverEnabled();
    this.aircraft = aircraft;
  }

  public new void RefreshSettings()
  {
  }

  public override void Refresh()
  {
    if (!this.icon.enabled)
      return;
    this.icon.color = (double) this.aircraft.speed > (double) this.maxSpeed ? Color.green * 0.5f + Color.red : Color.green;
  }

  private void OnDestroy()
  {
    if (!((UnityEngine.Object) this.aircraft != (UnityEngine.Object) null))
      return;
    this.aircraft.GetControlsFilter().OnSetAutoHover -= new Action(this.AutopilotIndicator_OnSetAutopilot);
  }

  private void AutopilotIndicator_OnSetAutopilot()
  {
    this.icon.enabled = this.aircraft.GetControlsFilter().IsAutoHoverEnabled();
  }
}

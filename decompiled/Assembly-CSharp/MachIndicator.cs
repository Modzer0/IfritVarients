// Decompiled with JetBrains decompiler
// Type: MachIndicator
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;
using UnityEngine.UI;

#nullable disable
public class MachIndicator : HUDApp
{
  private Aircraft aircraft;
  private AircraftParameters aircraftParameters;
  [SerializeField]
  private Text machDisplay;

  public override void Initialize(Aircraft aircraft)
  {
    if ((UnityEngine.Object) aircraft == (UnityEngine.Object) null)
      return;
    this.aircraft = aircraft;
    this.aircraftParameters = aircraft.definition.aircraftParameters;
  }

  public override void RefreshSettings()
  {
    base.RefreshSettings();
    this.machDisplay.fontSize = this.fontSize;
  }

  public override void Refresh()
  {
    if ((UnityEngine.Object) this.aircraft == (UnityEngine.Object) null)
      return;
    this.machDisplay.text = $"{(ValueType) (float) ((double) this.aircraft.speed / (double) LevelInfo.GetSpeedOfSound(this.aircraft.GlobalPosition().y)):F2}";
  }
}

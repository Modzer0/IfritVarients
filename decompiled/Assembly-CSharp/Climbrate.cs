// Decompiled with JetBrains decompiler
// Type: Climbrate
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using UnityEngine;
using UnityEngine.UI;

#nullable disable
public class Climbrate : HUDApp
{
  [SerializeField]
  private Text climbRate;
  [SerializeField]
  private Aircraft aircraft;

  public override void Initialize(Aircraft aircraft) => this.aircraft = aircraft;

  public override void RefreshSettings()
  {
    base.RefreshSettings();
    this.climbRate.fontSize = this.fontSize;
  }

  public override void Refresh()
  {
    if ((Object) this.aircraft == (Object) null)
      return;
    this.climbRate.text = UnitConverter.ClimbRateReading(Vector3.Dot(this.aircraft.CockpitRB().velocity, Vector3.up));
  }
}

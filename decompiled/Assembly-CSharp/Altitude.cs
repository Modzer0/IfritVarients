// Decompiled with JetBrains decompiler
// Type: Altitude
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using UnityEngine;
using UnityEngine.UI;

#nullable disable
public class Altitude : HUDApp
{
  [SerializeField]
  private Text radarAlt;
  [SerializeField]
  private Text absAlt;
  [SerializeField]
  private Aircraft aircraft;
  [SerializeField]
  private Image border;

  public override void Initialize(Aircraft aircraft) => this.aircraft = aircraft;

  public override void RefreshSettings()
  {
    base.RefreshSettings();
    this.radarAlt.fontSize = this.fontSize;
    this.absAlt.fontSize = this.fontSize;
  }

  public override void Refresh()
  {
    if ((Object) this.aircraft == (Object) null)
      return;
    this.radarAlt.text = $"R[{UnitConverter.AltitudeReading(this.aircraft.radarAlt)}]";
    this.absAlt.text = UnitConverter.AltitudeReading(this.aircraft.transform.position.GlobalY());
  }
}

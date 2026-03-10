// Decompiled with JetBrains decompiler
// Type: GIndicators
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using UnityEngine;
using UnityEngine.UI;

#nullable disable
public class GIndicators : HUDApp
{
  [SerializeField]
  private Aircraft aircraft;
  [SerializeField]
  private Text gForceLabel;
  [SerializeField]
  private Text gMaxForceLabel;
  private float maxGNumber;

  public override void Initialize(Aircraft aircraft) => this.aircraft = aircraft;

  public override void RefreshSettings()
  {
    base.RefreshSettings();
    this.gForceLabel.fontSize = this.fontSize;
    this.gMaxForceLabel.fontSize = this.fontSize;
  }

  public override void Refresh()
  {
    if ((Object) this.aircraft == (Object) null)
      return;
    float b = Vector3.Dot(this.aircraft.pilots[0].GetAccel() + Vector3.up, this.aircraft.transform.up);
    this.maxGNumber = Mathf.Max(this.maxGNumber, b);
    this.gForceLabel.text = $"{b:F1}";
    this.gMaxForceLabel.text = $"[{this.maxGNumber:F1}]";
  }
}

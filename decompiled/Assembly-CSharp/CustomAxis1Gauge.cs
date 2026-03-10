// Decompiled with JetBrains decompiler
// Type: CustomAxis1Gauge
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using UnityEngine;
using UnityEngine.UI;

#nullable disable
public class CustomAxis1Gauge : HUDApp
{
  [SerializeField]
  private Image negativeBar;
  [SerializeField]
  private Image positiveBar;
  [SerializeField]
  private float axisSplitPosition = 0.4f;
  [SerializeField]
  private float idleZone = 0.03f;
  [SerializeField]
  private Text label;
  private ControlInputs inputs;
  private Aircraft aircraft;
  private float axisPrev;

  public override void Initialize(Aircraft aircraft)
  {
    this.inputs = aircraft.GetInputs();
    this.aircraft = aircraft;
  }

  public override void Refresh()
  {
    if ((Object) this.aircraft == (Object) null || (double) this.axisPrev == (double) this.inputs.customAxis1)
      return;
    this.axisPrev = this.inputs.customAxis1;
    float num1 = this.axisSplitPosition + this.idleZone;
    float num2 = this.axisSplitPosition - this.idleZone;
    float num3 = (float) (((double) this.inputs.customAxis1 - (double) num1) / (1.0 - (double) num1));
    float num4 = (num2 - this.inputs.customAxis1) / num2;
    this.positiveBar.fillAmount = num3;
    this.negativeBar.fillAmount = num4;
  }

  public override void RefreshSettings()
  {
    base.RefreshSettings();
    this.label.fontSize = this.fontSize - 4;
  }
}

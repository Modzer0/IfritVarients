// Decompiled with JetBrains decompiler
// Type: PropGauge
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;
using UnityEngine.UI;

#nullable disable
public class PropGauge : HUDApp
{
  [SerializeField]
  private string sourceName;
  [SerializeField]
  private Text rpmText;
  [SerializeField]
  private Text aoaText;
  [SerializeField]
  private Text powerText;
  [SerializeField]
  private Image propCircle;
  [SerializeField]
  private Gradient rpmColor;
  [SerializeField]
  private float warningThreshold;
  [SerializeField]
  private float maxRPM;
  private Aircraft aircraft;
  private ConstantSpeedProp source;

  public override void Initialize(Aircraft aircraft)
  {
    this.aircraft = aircraft;
    foreach (UnitPart unitPart in aircraft.partLookup)
    {
      if ((UnityEngine.Object) unitPart != (UnityEngine.Object) null && unitPart.gameObject.name == this.sourceName)
      {
        this.source = unitPart.gameObject.GetComponent<ConstantSpeedProp>();
        break;
      }
    }
  }

  public override void Refresh()
  {
    if ((UnityEngine.Object) this.source == (UnityEngine.Object) null)
      return;
    float rpmRatio = this.source.GetRPMRatio();
    this.rpmText.text = $"{(ValueType) (float) ((double) rpmRatio * 100.0):F1}%";
    this.aoaText.text = $"{this.source.GetAoA():F1}°";
    this.powerText.text = UnitConverter.PowerReading(this.source.GetPowerAvailable() * (1f / 1000f));
    this.rpmText.color = this.rpmColor.Evaluate((float) (((double) rpmRatio - (double) this.warningThreshold) / (1.0 - (double) this.warningThreshold)));
    this.propCircle.fillAmount = rpmRatio;
  }
}

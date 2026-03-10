// Decompiled with JetBrains decompiler
// Type: HoverIndicator
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using UnityEngine;
using UnityEngine.UI;

#nullable disable
public class HoverIndicator : HUDApp
{
  [SerializeField]
  private float maxSpeed;
  [SerializeField]
  private float maxLineLength;
  [SerializeField]
  private Transform indicatorLine;
  [SerializeField]
  private Transform indicatorLineAnchor;
  [SerializeField]
  private Transform indicatorLineTip;
  [SerializeField]
  private Image indicatorImage;
  private Aircraft aircraft;

  public override void Initialize(Aircraft aircraft) => this.aircraft = aircraft;

  public override void Refresh()
  {
    if ((Object) this.aircraft == (Object) null)
      return;
    if ((double) this.aircraft.speed > (double) this.maxSpeed * 0.27777779102325439)
    {
      if (!this.indicatorImage.enabled)
        return;
      this.indicatorImage.enabled = false;
    }
    else
    {
      if (!this.indicatorImage.enabled)
        this.indicatorImage.enabled = true;
      float num1 = Vector3.Dot(this.aircraft.rb.velocity, -this.aircraft.transform.forward) * 3.6f;
      float num2 = Vector3.Dot(this.aircraft.rb.velocity, -this.aircraft.transform.right) * 3.6f;
      float num3 = num1 * (this.maxLineLength / this.maxSpeed);
      float num4 = num2 * (this.maxLineLength / this.maxSpeed);
      this.indicatorLineTip.localPosition = new Vector3(num4, num3, 0.0f);
      this.indicatorLine.transform.eulerAngles = new Vector3(0.0f, 0.0f, (float) (-(double) Mathf.Atan2(num4, num3) * 57.295780181884766 + 180.0));
      this.indicatorLine.transform.localScale = new Vector3(1f, this.indicatorLineTip.localPosition.magnitude, 1f);
    }
  }
}

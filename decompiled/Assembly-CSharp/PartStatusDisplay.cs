// Decompiled with JetBrains decompiler
// Type: PartStatusDisplay
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#nullable disable
[Serializable]
public class PartStatusDisplay
{
  public Image partImage;
  private UnitPart unitPart;
  public float redStatusThreshold;
  [NonSerialized]
  public float displayCondition = 1f;
  private StatusDisplay statusDisplay;

  public void DamageSubscribe(Aircraft aircraft, StatusDisplay statusDisplay)
  {
    this.unitPart = this.FindPart(aircraft.partLookup);
    if ((UnityEngine.Object) this.unitPart != (UnityEngine.Object) null)
    {
      this.unitPart.onApplyDamage += new Action<UnitPart.OnApplyDamage>(this.StatusDisplay_OnDamage);
      this.unitPart.onParentDetached += new Action<UnitPart>(this.StatusDisplay_OnDetach);
    }
    this.statusDisplay = statusDisplay;
  }

  private UnitPart FindPart(List<UnitPart> partLookup)
  {
    foreach (UnitPart part in partLookup)
    {
      if ((UnityEngine.Object) part != (UnityEngine.Object) null && part.gameObject.name == this.partImage.gameObject.name)
        return part;
    }
    return (UnitPart) null;
  }

  public void DamageUnsubscribe()
  {
    if (!((UnityEngine.Object) this.unitPart != (UnityEngine.Object) null))
      return;
    this.unitPart.onApplyDamage -= new Action<UnitPart.OnApplyDamage>(this.StatusDisplay_OnDamage);
    this.unitPart.onParentDetached -= new Action<UnitPart>(this.StatusDisplay_OnDetach);
  }

  private void StatusDisplay_OnDamage(UnitPart.OnApplyDamage e)
  {
    this.displayCondition = Mathf.Max((float) (((double) e.hitPoints - (double) this.redStatusThreshold) / (100.0 - (double) this.redStatusThreshold)), 0.0f);
    if (e.detached)
      this.displayCondition = 0.0f;
    this.partImage.color = this.partImage.color with
    {
      g = Mathf.Min(this.displayCondition * 2f, 1f),
      a = 1f - this.displayCondition
    };
    this.statusDisplay.DisplayDamage();
  }

  private void StatusDisplay_OnDetach(UnitPart part)
  {
    this.DamageUnsubscribe();
    this.partImage.color = new Color(0.7f, 0.0f, 0.25f);
    this.statusDisplay.DisplayDamage();
  }
}

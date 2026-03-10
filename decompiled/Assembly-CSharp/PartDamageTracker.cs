// Decompiled with JetBrains decompiler
// Type: PartDamageTracker
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;

#nullable disable
public class PartDamageTracker
{
  private float detachedRatio;
  private float lastCheck;
  private bool needsCheck;
  private readonly Aircraft aircraft;

  private void PartDamageTracker_OnPartDetached(UnitPart part) => this.needsCheck = true;

  public PartDamageTracker(Aircraft aircraft)
  {
    this.aircraft = aircraft;
    this.needsCheck = false;
    int num = 0;
    foreach (UnitPart allPart in aircraft.GetAllParts())
    {
      ++num;
      Action<UnitPart> action = new Action<UnitPart>(this.PartDamageTracker_OnPartDetached);
      allPart.onPartDetached += action;
    }
  }

  public float GetDetachedRatio()
  {
    if ((double) Time.timeSinceLevelLoad - (double) this.lastCheck < 1.0 || !this.needsCheck)
      return this.detachedRatio;
    this.lastCheck = Time.timeSinceLevelLoad;
    float num1 = 0.0f;
    float num2 = 0.0f;
    foreach (UnitPart allPart in this.aircraft.GetAllParts())
    {
      ++num1;
      if (allPart.IsDetached())
        ++num2;
    }
    this.needsCheck = false;
    this.detachedRatio = num2 / num1;
    return this.detachedRatio;
  }
}

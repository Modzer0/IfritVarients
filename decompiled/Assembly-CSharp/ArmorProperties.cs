// Decompiled with JetBrains decompiler
// Type: ArmorProperties
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;

#nullable disable
[Serializable]
public class ArmorProperties
{
  public float pierceArmor;
  public float blastArmor;
  public float fireArmor;
  public float pierceTolerance = 1f;
  public float blastTolerance = 1f;
  public float fireTolerance = 1f;
  public float overpressureLimit = 5f;

  public float CalcNetDamage(
    float pierceDamage,
    float blastDamage,
    float fireDamage,
    float impactDamage)
  {
    double num1 = (double) Mathf.Max(pierceDamage - this.pierceArmor, 0.0f) / (double) this.pierceTolerance;
    float num2 = Mathf.Max(blastDamage - this.blastArmor, 0.0f) / this.blastTolerance;
    float num3 = Mathf.Max(fireDamage - this.fireArmor, 0.0f) / this.fireTolerance;
    double num4 = (double) num2;
    return (float) (num1 + num4) + num3 + impactDamage;
  }
}

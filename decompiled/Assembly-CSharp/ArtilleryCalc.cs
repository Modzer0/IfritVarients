// Decompiled with JetBrains decompiler
// Type: ArtilleryCalc
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;

#nullable disable
public static class ArtilleryCalc
{
  public static float GetElevation(
    float displacementH,
    float displacementV,
    float muzzleVelocity,
    int maxIterations)
  {
    float f1 = 0.3926991f;
    Vector2 zero = Vector2.zero;
    float f2 = 0.0f;
    float num1 = 0.0001f;
    int num2 = 0;
    while ((double) Mathf.Abs(f2) > 10.0 && num2 <= maxIterations)
    {
      zero.x = muzzleVelocity * Mathf.Cos(f1);
      zero.y = muzzleVelocity * Mathf.Sin(f1);
      float num3 = zero.y / 9.81f + Mathf.Sqrt((float) (2.0 * (double) (zero.y / 19.62f - displacementV) / 9.8100004196167));
      f2 = zero.x * num3 - displacementH;
      f1 -= num1 * f2;
    }
    Debug.Log((object) $"[TargetCacl] Calculated artillery firing solution in {num2} iterations");
    return f1 * ((float) Math.PI / 180f);
  }
}

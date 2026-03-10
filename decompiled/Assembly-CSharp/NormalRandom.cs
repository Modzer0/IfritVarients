// Decompiled with JetBrains decompiler
// Type: NormalRandom
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

#nullable disable
public static class NormalRandom
{
  public static float GetNormalizedRandom(float minValue = 0.01f, float maxValue = 1f)
  {
    float num1;
    float f;
    do
    {
      num1 = (float) (2.0 * (double) Random.value - 1.0);
      float num2 = (float) (2.0 * (double) Random.value - 1.0);
      f = (float) ((double) num1 * (double) num1 + (double) num2 * (double) num2);
    }
    while ((double) f >= 1.0);
    double num3 = (double) num1 * (double) Mathf.Sqrt(-2f * Mathf.Log(f) / f);
    float num4 = (float) (((double) minValue + (double) maxValue) / 2.0);
    double num5 = ((double) maxValue - (double) num4) / 3.0;
    return Mathf.Clamp((float) (num3 * num5) + num4, minValue, maxValue);
  }
}

// Decompiled with JetBrains decompiler
// Type: NuclearOption.Jobs.ChartHelper
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using Unity.Burst;
using UnityEngine;

#nullable disable
namespace NuclearOption.Jobs;

public static class ChartHelper
{
  [BurstDiscard]
  public static void LogErrorDiscard(string msg) => Debug.LogError((object) msg);

  public static float SafeRead(float index, ReadOnlySpan<float> chart)
  {
    if (float.IsNaN(index))
    {
      ChartHelper.LogErrorDiscard("Index was IsNaN");
      ref ReadOnlySpan<float> local = ref chart;
      return local[local.Length - 1];
    }
    if (float.IsInfinity(index))
    {
      ChartHelper.LogErrorDiscard("Index was Infinity");
      ref ReadOnlySpan<float> local = ref chart;
      return local[local.Length - 1];
    }
    if ((double) index <= 0.0)
      return chart[0];
    if ((double) index >= (double) (chart.Length - 1))
    {
      ref ReadOnlySpan<float> local = ref chart;
      return local[local.Length - 1];
    }
    int num1 = Mathf.FloorToInt(index);
    int num2 = Mathf.CeilToInt(index);
    double a = (double) chart[num1];
    float num3 = chart[num2];
    float num4 = index % 1f;
    double b = (double) num3;
    double t = (double) num4;
    return Mathf.Lerp((float) a, (float) b, (float) t);
  }
}

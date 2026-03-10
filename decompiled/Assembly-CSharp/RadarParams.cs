// Decompiled with JetBrains decompiler
// Type: RadarParams
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;

#nullable disable
[Serializable]
public struct RadarParams(
  float maxRange,
  float maxSignal,
  float minSignal,
  float clutterFactor,
  float dopplerFactor)
{
  public float maxRange = maxRange;
  public float maxSignal = maxSignal;
  public float minSignal = minSignal;
  public float clutterFactor = clutterFactor;
  public float dopplerFactor = dopplerFactor;

  public float GetSignalStrength(
    Vector3 direction,
    float dist,
    Rigidbody rb,
    float RCS,
    float clutter,
    float jamming)
  {
    float a = Mathf.Min(this.maxRange / dist * Mathf.Pow(RCS, 0.25f), this.maxSignal) - clutter * this.clutterFactor;
    if ((double) a <= (double) this.minSignal)
      return a;
    float num1 = Mathf.Lerp(a, this.maxSignal, 0.5f);
    if ((UnityEngine.Object) rb != (UnityEngine.Object) null)
    {
      float num2 = Mathf.Min(Mathf.Abs(Vector3.Dot(direction, rb.velocity)), 150f) * this.dopplerFactor;
      num1 *= 1f + num2;
    }
    return num1 - jamming;
  }
}

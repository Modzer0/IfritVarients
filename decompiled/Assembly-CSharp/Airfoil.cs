// Decompiled with JetBrains decompiler
// Type: Airfoil
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using Unity.Collections;
using UnityEngine;

#nullable disable
[Serializable]
public class Airfoil
{
  public string name;
  [NonSerialized]
  public int id;
  public AnimationCurve liftCoef;
  public AnimationCurve dragCoef;

  public NativeArray<float> BuildLiftChart()
  {
    NativeArray<float> nativeArray = new NativeArray<float>(128 /*0x80*/, Allocator.Temp);
    for (int index = 0; index < 128 /*0x80*/; ++index)
    {
      float time = (float) (index - 64 /*0x40*/) * 0.04908734f;
      nativeArray[index] = this.liftCoef.Evaluate(time);
    }
    return nativeArray;
  }

  public NativeArray<float> BuildDragChart()
  {
    NativeArray<float> nativeArray = new NativeArray<float>(128 /*0x80*/, Allocator.Temp);
    for (int index = 0; index < 128 /*0x80*/; ++index)
    {
      float time = (float) (index - 64 /*0x40*/) * 0.04908734f;
      nativeArray[index] = this.dragCoef.Evaluate(time);
    }
    return nativeArray;
  }
}

// Decompiled with JetBrains decompiler
// Type: NuclearOption.NetworkTransforms.SendTransformBatcherInterpolationDeltaTimeSource
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using JamesFrowen.Graphy;
using UnityEngine;

#nullable disable
namespace NuclearOption.NetworkTransforms;

public class SendTransformBatcherInterpolationDeltaTimeSource : GraphDataSource
{
  public SendTransformBatcher SendTransformBatcher;
  private double previous;

  public override float GetNewValue()
  {
    if ((Object) this.SendTransformBatcher == (Object) null)
    {
      this.SendTransformBatcher = Object.FindObjectOfType<SendTransformBatcher>();
      if ((Object) this.SendTransformBatcher == (Object) null)
        return 0.0f;
    }
    double serverSmoothTime = this.SendTransformBatcher.ServerSmoothTime;
    double num1 = serverSmoothTime - this.previous;
    this.previous = serverSmoothTime;
    float num2 = (float) (num1 * 1000.0);
    return (double) num2 < 0.0 ? 0.0f : num2;
  }
}

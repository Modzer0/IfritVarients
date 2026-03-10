// Decompiled with JetBrains decompiler
// Type: NuclearOption.NetworkTransforms.SendTransformBatcherExtrapolationTimeSource
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using JamesFrowen.Graphy;
using UnityEngine;

#nullable disable
namespace NuclearOption.NetworkTransforms;

public class SendTransformBatcherExtrapolationTimeSource : GraphDataSource
{
  public SendTransformBatcher SendTransformBatcher;

  public override float GetNewValue()
  {
    if ((Object) this.SendTransformBatcher == (Object) null)
    {
      this.SendTransformBatcher = Object.FindObjectOfType<SendTransformBatcher>();
      if ((Object) this.SendTransformBatcher == (Object) null)
        return 0.0f;
    }
    float num = (float) (this.SendTransformBatcher.Debug_extrapolationOffset * 1000.0);
    return (double) num < 0.0 ? 0.0f : num;
  }
}

// Decompiled with JetBrains decompiler
// Type: NuclearOption.Jobs.ReadTransformJob
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine.Jobs;

#nullable disable
namespace NuclearOption.Jobs;

[BurstCompile]
public struct ReadTransformJob(NativeArray<JobTransformValues> values, Ptr<long> timer) : 
  IJobParallelForTransform
{
  [WriteOnly]
  public NativeArray<JobTransformValues> values = values;
  [NativeDisableUnsafePtrRestriction]
  public Ptr<long> timer = timer;

  public void Execute(int i, TransformAccess access)
  {
    JobPerf.GetTimestampBurst();
    if (!access.isValid)
      return;
    ref JobTransformValues local = ref this.values.AsSpan()[i];
    access.GetPositionAndRotation(out local.Position, out local.Rotation);
  }
}

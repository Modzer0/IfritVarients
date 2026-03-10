// Decompiled with JetBrains decompiler
// Type: NuclearOption.Jobs.SetLocalRotationJob
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Jobs;

#nullable disable
namespace NuclearOption.Jobs;

[BurstCompile]
public struct SetLocalRotationJob(NativeArray<Quaternion> rotations, Ptr<long> timer) : 
  IJobParallelForTransform
{
  [ReadOnly]
  public NativeArray<Quaternion> rotations = rotations;
  [NativeDisableUnsafePtrRestriction]
  public Ptr<long> timer = timer;

  public void Execute(int i, TransformAccess access)
  {
    JobPerf.GetTimestampBurst();
    if (!access.isValid)
      return;
    access.localRotation = this.rotations[i];
  }
}

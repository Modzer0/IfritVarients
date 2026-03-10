// Decompiled with JetBrains decompiler
// Type: NuclearOption.Jobs.ControlSurfaceJob_Math
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;

#nullable disable
namespace NuclearOption.Jobs;

[BurstCompile]
public struct ControlSurfaceJob_Math : IJobParallelFor
{
  [ReadOnly]
  public NativeArray<Ptr<ControlSurfaceFields>> fields;
  [WriteOnly]
  public NativeArray<Quaternion> rotations;
  [ReadOnly]
  [NativeDisableUnsafePtrRestriction]
  public Ptr<JobSharedFields> shared;

  private ref Quaternion GetRotation(int index) => ref this.rotations.AsSpan()[index];

  public void Execute(int i)
  {
    JobPerf.GetTimestampBurst();
    ref ControlSurfaceFields local = ref this.fields[i].Ref();
    int index1;
    if (!IndexLink.BurstGetTransformIndex(local.visibleTransformLink, out index1))
      return;
    int index2 = 0;
    int index3 = 0;
    if ((double) local.maxSplit > 0.0 && (!IndexLink.BurstGetTransformIndex(local.upperTransformLink, out index2) || !IndexLink.BurstGetTransformIndex(local.lowerTransformLink, out index3)))
      return;
    this.Execute(ref local, ref this.GetRotation(index1), index2, index3);
  }

  public void Execute(
    ref ControlSurfaceFields fields,
    ref Quaternion mainRotation,
    int upperIndex,
    int lowerIndex)
  {
    if (fields.IsDetached)
      return;
    float fixedDeltaTime = this.shared.Ref().fixedDeltaTime;
    ref ControlInputsBurst local = ref fields.controlInputs;
    if (!fields.flap)
    {
      float num1 = local.pitch * fields.pitchRange - fields.currentPitch;
      fields.currentPitch += Mathf.Clamp(num1, -fields.servoSpeed * fixedDeltaTime, fields.servoSpeed * fixedDeltaTime);
      fields.currentPitch = Mathf.Clamp(fields.currentPitch, -Mathf.Abs(fields.pitchRange), Mathf.Abs(fields.pitchRange));
      float num2 = local.roll * fields.rollRange - fields.currentRoll;
      fields.currentRoll += Mathf.Clamp(num2, -fields.servoSpeed * fixedDeltaTime, fields.servoSpeed * fixedDeltaTime);
      fields.currentRoll = Mathf.Clamp(fields.currentRoll, -Mathf.Abs(fields.rollRange), Mathf.Abs(fields.rollRange));
      float num3 = local.yaw * fields.yawRange - fields.currentYaw;
      fields.currentYaw += Mathf.Clamp(num3, -fields.servoSpeed * fixedDeltaTime, fields.servoSpeed * fixedDeltaTime);
      fields.currentYaw = Mathf.Clamp(fields.currentYaw, -Mathf.Abs(fields.yawRange), Mathf.Abs(fields.yawRange));
    }
    else
    {
      if (fields.gearState == LandingGear.GearState.Extending || fields.gearState == LandingGear.GearState.LockedExtended)
        fields.currentPitch -= fields.servoSpeed * fixedDeltaTime;
      else
        fields.currentPitch += fields.servoSpeed * fixedDeltaTime;
      fields.currentPitch = Mathf.Clamp(fields.currentPitch, -fields.pitchRange, 0.0f);
    }
    float angle = fields.currentPitch + fields.currentRoll + fields.currentYaw;
    mainRotation = fields.restingRotation * Quaternion.AngleAxis(angle, Vector3.right);
    if ((double) fields.maxSplit <= 0.0)
      return;
    float num = Mathf.Clamp(((double) local.throttle == 0.0 ? fields.maxSplit : 0.0f) + fields.yawSplitFactor * local.yaw * fields.maxSplit, 0.0f, fields.maxSplit);
    fields.splitAmount += Mathf.Clamp(num - fields.splitAmount, -fields.servoSpeed * fixedDeltaTime, fields.servoSpeed * fixedDeltaTime);
    this.GetRotation(upperIndex) = fields.restingSplitRotation * Quaternion.AngleAxis(fields.splitAmount, Vector3.right);
    this.GetRotation(lowerIndex) = fields.restingSplitRotation * Quaternion.AngleAxis(-fields.splitAmount, Vector3.right);
  }
}

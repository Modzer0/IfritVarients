// Decompiled with JetBrains decompiler
// Type: NuclearOption.Jobs.WaterJob_Math
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
public struct WaterJob_Math : IJobParallelFor
{
  [ReadOnly]
  public NativeArray<Ptr<ShipPartFields>> fields;
  [ReadOnly]
  public NativeArray<JobTransformValues> transformValues;
  [ReadOnly]
  [NativeDisableUnsafePtrRestriction]
  public Ptr<JobSharedFields> shared;

  public void Execute(int i)
  {
    JobPerf.GetTimestampBurst();
    this.Execute(ref this.fields[i].Ref(), ref this.transformValues.GetReadOnlyRef(i));
  }

  private void Execute(ref ShipPartFields fields, ref JobTransformValues.ReadOnly transform)
  {
    float num = this.shared.Ref().datum.GlobalY(transform.Position);
    float submergedAmount = Mathf.Clamp01((fields.partHeight * 0.5f - num) / fields.partHeight);
    Vector3 vector3 = this.Buoyancy(submergedAmount, fields.displacement) * Vector3.up + this.Drag(submergedAmount, ref fields, ref transform);
    if (!float.IsFinite(vector3.x) || !float.IsFinite(vector3.y) || !float.IsFinite(vector3.z))
    {
      Debug.LogError((object) "Non-finite force from WaterJob");
      vector3 = Vector3.zero;
    }
    fields.forcePosition = transform.Position;
    fields.force = vector3;
    fields.submergedAmount = submergedAmount;
  }

  private float Buoyancy(float submergedAmount, float displacement)
  {
    return Mathf.Lerp(1.2f, 1000f, submergedAmount) * 9.81f * displacement;
  }

  private Vector3 Drag(
    float submergedAmount,
    ref ShipPartFields fields,
    ref JobTransformValues.ReadOnly transform)
  {
    Vector3 rhs1 = transform.Forward();
    Vector3 rhs2 = transform.Right();
    Vector3 rhs3 = transform.Up();
    double f1 = (double) Vector3.Dot(fields.velocity, rhs1);
    float f2 = Vector3.Dot(fields.velocity, rhs2);
    float f3 = Vector3.Dot(fields.velocity, rhs3);
    float num1 = (float) f1 * Mathf.Abs((float) f1) * fields.directionalDrag.z * fields.mass * submergedAmount;
    float num2 = f2 * Mathf.Abs(f2) * fields.directionalDrag.x * fields.mass * submergedAmount;
    float num3 = f3 * Mathf.Abs(f3) * fields.directionalDrag.y * fields.mass * submergedAmount;
    return Vector3.ClampMagnitude(rhs1 * -num1 + rhs2 * -num2 + rhs3 * -num3, fields.mass * 500f);
  }
}

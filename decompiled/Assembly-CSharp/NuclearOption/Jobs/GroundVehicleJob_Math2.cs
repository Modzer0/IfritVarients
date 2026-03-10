// Decompiled with JetBrains decompiler
// Type: NuclearOption.Jobs.GroundVehicleJob_Math2
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Profiling;
using UnityEngine;

#nullable disable
namespace NuclearOption.Jobs;

[BurstCompile]
public struct GroundVehicleJob_Math2 : IJobParallelFor
{
  private static readonly ProfilerMarker executeMarker = new ProfilerMarker("GroundVehicleJob_Math2 Execute");
  private static readonly ProfilerMarker processSampleGroundMarker = new ProfilerMarker("GroundVehicleJob_Math2 ProcessSampleGround");
  private static readonly ProfilerMarker suspensionPhysicsMarker = new ProfilerMarker("GroundVehicleJob_Math2 SuspensionPhysics");
  private const float NO_SURFACE_ALT = 100f;
  [ReadOnly]
  public NativeArray<JobTransformValues> transformValues;
  [ReadOnly]
  public NativeArray<Ptr<GroundVehicleFields>> fields;
  [ReadOnly]
  [NativeDisableUnsafePtrRestriction]
  public Ptr<JobSharedFields> shared;
  internal NativeArray<RaycastHit> rayResults;

  public void Execute(int i)
  {
    JobPerf.GetTimestampBurst();
    this.Execute(i, ref this.fields[i].Ref(), ref this.transformValues.GetReadOnlyRef(i));
  }

  private void Execute(
    int i,
    ref GroundVehicleFields fields,
    ref JobTransformValues.ReadOnly transform)
  {
    if (!fields.monoBehaviourEnabled)
      return;
    if (GroundVehicleJobSettings.ShouldRunSampleGround(i, this.shared.Ref().tickOffset))
      this.ProcessSampleGround(ref fields);
    this.SuspensionPhysics(ref fields, ref transform);
    if (fields.disabled || (double) Mathf.Abs(fields.speed) >= 1.0 || (double) Vector3.Dot(transform.Up(), Vector3.up) >= 0.5)
      return;
    float num = Mathf.Clamp(TargetCalc.GetAngleOnAxis(transform.Up(), Vector3.up, transform.Forward()), -10f, 10f);
    fields.AddTorque(num * 5f * fields.mass * transform.Forward());
  }

  private void ProcessSampleGround(ref GroundVehicleFields fields)
  {
    if (fields.sampleGroundResult.didHit)
    {
      Vector3 hitNormal = fields.sampleGroundResult.hitNormal;
      Vector3 globalPosition = this.shared.Ref().datum.ToGlobalPosition(fields.sampleGroundResult.hitPoint);
      fields.surfacePlane.SetNormalAndPosition(hitNormal, globalPosition);
    }
    else
      fields.surfacePlane.SetNormalAndPosition(Vector3.up, new Vector3(0.0f, -100f, 0.0f));
  }

  private void SuspensionPhysics(
    ref GroundVehicleFields fields,
    ref JobTransformValues.ReadOnly transform)
  {
    Vector3 vector3_1;
    if (fields.sampleGroundResult.hasHitRB)
    {
      vector3_1 = fields.velocity - fields.sampleGroundResult.hitPointVelocity;
      fields.surfacePlane.Translate(vector3_1 * this.shared.Ref().fixedDeltaTime);
      fields.speed = Vector3.Dot(vector3_1, transform.Forward());
    }
    else
      vector3_1 = fields.velocity;
    float enter;
    fields.radarAlt = !fields.surfacePlane.Raycast(new Ray(this.shared.Ref().datum.ToGlobalPosition(transform.Position), transform.Down()), out enter) ? 100f : Mathf.Max(enter, 0.0f);
    if ((double) fields.radarAlt > (double) fields.suspensionTravel * 2.0)
      return;
    float num1 = fields.springRate * Mathf.Max(fields.suspensionTravel - fields.radarAlt, 0.0f);
    float num2 = (double) num1 > 0.0 ? -fields.dampingRate * Mathf.Min(Vector3.Dot(fields.surfacePlane.normal, vector3_1), 0.0f) : 0.0f;
    Vector3 vector3_2 = fields.surfacePlane.normal * (num1 + num2);
    Vector3 vector3_3 = 12f * fields.inertiaTensor * -Vector3.Cross(fields.surfacePlane.normal, transform.Up()) - 4f * fields.inertiaTensor * fields.angularVelocity;
    Vector3 onNormal = Vector3.Cross(transform.Forward(), fields.surfacePlane.normal);
    Vector3 vector3_4 = new Vector3(-vector3_2.x, 0.0f, -vector3_2.z) - vector3_1 * fields.mass * 10f;
    Vector3 vector3_5 = Vector3.Project(-vector3_1 * 10f + vector3_4, onNormal);
    Vector3 vector3_6 = (-vector3_1 * 10f + vector3_4) * fields.inputs.brake;
    float num3 = fields.sampleGroundResult.onPaved ? fields.topSpeedOnroad : fields.topSpeedOffroad;
    Vector3 vector3_7;
    if ((double) Mathf.Abs(fields.speed) < (double) num3 * 0.27777698636054993)
    {
      float num4 = Mathf.Clamp(fields.engineOutput * 40f / Mathf.Max(Mathf.Abs(fields.speed), 1f), (float) (-(double) fields.acceleration * 5.0), fields.acceleration * 5f);
      vector3_7 = transform.Forward() * fields.mass * num4;
    }
    else
      vector3_7 = Vector3.zero;
    Vector3 vector3_8 = new Vector3(Mathf.Sign(-vector3_1.x) - vector3_1.x * 0.05f, Mathf.Sign(-vector3_1.y) - vector3_1.y * 0.05f, Mathf.Sign(-vector3_1.z) - vector3_1.z * 0.05f) * (float) (0.05000000074505806 * ((double) num1 + (double) num2));
    Vector3 vector3_9 = vector3_6;
    Vector3 vector3_10 = Vector3.ClampMagnitude(vector3_5 + vector3_9 + vector3_8, (num1 + num2) * fields.frictionCoef) + vector3_7;
    fields.AddForce(vector3_2 + vector3_10);
    fields.AddTorque(fields.inputs.steering * 0.2f * fields.inertiaTensor * transform.Up() + vector3_3);
  }
}

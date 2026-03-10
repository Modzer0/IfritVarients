// Decompiled with JetBrains decompiler
// Type: NuclearOption.Jobs.AeroJob_Math
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

#nullable disable
namespace NuclearOption.Jobs;

[BurstCompile]
public struct AeroJob_Math : IJobParallelFor
{
  [ReadOnly]
  public NativeArray<float> liftCharts;
  [ReadOnly]
  public NativeArray<float> dragCharts;
  [ReadOnly]
  public NativeArray<float> airDensityChart;
  public Vector3 windVelocity;
  public float windTurbulence;
  [ReadOnly]
  public NativeArray<Ptr<AeroPartFields>> fields;
  [ReadOnly]
  public NativeArray<JobTransformValues> transformValues;
  [ReadOnly]
  [NativeDisableUnsafePtrRestriction]
  public Ptr<JobSharedFields> shared;

  public void Execute(int i)
  {
    JobPerf.GetTimestampBurst();
    ref AeroPartFields local = ref this.fields[i].Ref();
    int index1;
    if (!IndexLink.BurstGetTransformIndex(local.liftTransformIndex, out index1))
      return;
    int index2 = 0;
    if ((double) local.airflowChanneling > 0.0 && !IndexLink.BurstGetTransformIndex(local.otherTransformIndex, out index2))
      return;
    this.Execute(ref local, ref this.transformValues.GetReadOnlyRef(index1), index2);
  }

  private void Execute(
    ref AeroPartFields fields,
    ref JobTransformValues.ReadOnly liftTransform,
    int otherTransformIndex)
  {
    Vector3 velocity = fields.velocity;
    float a1 = fields.wingEffectiveness;
    float num1 = 0.0f;
    float num2 = fields.buoyancy;
    Vector3 zero = Vector3.zero;
    bool flag1 = false;
    Vector3 globalPosition = this.shared.Ref().datum.ToGlobalPosition(liftTransform.Position);
    float a2 = this.GetAirDensity(globalPosition.y);
    double num3 = (double) Mathf.Max(fields.submergedAmount, (float) -((double) globalPosition.y - (double) fields.collisionSize.y));
    float t = Mathf.Clamp01((float) (num3 / ((double) fields.collisionSize.y * 2.0)));
    if (num3 > 0.0)
    {
      num1 = t;
      Vector3 vector3_1 = new Vector3(fields.velocity.x, 0.0f, fields.velocity.z);
      if ((double) fields.velocity.y > -4.0)
        zero += 3f * t * fields.mass * vector3_1.magnitude * Vector3.up;
      num2 = Mathf.Max(num2 - 0.2f * t * this.shared.Ref().fixedDeltaTime, 0.0f);
      Vector3 vector3_2 = t * -fields.velocity * fields.mass * 5f;
      zero += new Vector3(vector3_2.x, vector3_2.y, vector3_2.z) + 9.81f * fields.buoyancy * fields.mass * t * Vector3.up;
      if ((double) this.shared.Ref().timeSinceLevelLoad - (double) fields.lastSplashTime > 2.0 && (double) math.lengthsq((float3) velocity) > 400.0)
      {
        flag1 = true;
        fields.lastSplashTime = this.shared.Ref().timeSinceLevelLoad;
      }
      a1 = Mathf.Lerp(a1, 0.01f, t);
      a2 = Mathf.Lerp(a2, 1000f, t);
    }
    Vector3 vector3_3 = this.CalcWind(globalPosition);
    Vector3 vector3_4 = velocity - new Vector3(vector3_3.x, vector3_3.y, vector3_3.z) * (1f - t);
    float num4 = math.lengthsq((float3) vector3_4);
    if ((double) fields.airflowChanneling > 0.0)
    {
      Vector3 target = this.transformValues.GetReadOnlyRef(otherTransformIndex).Forward();
      vector3_4 = Vector3.RotateTowards(vector3_4, target, fields.airflowChanneling, 0.0f);
    }
    Vector3 vector3_5 = Quaternion.Inverse(liftTransform.Rotation) * vector3_4;
    Vector3 normalized = Vector3.Cross(vector3_4, liftTransform.Right()).normalized;
    float alpha = Mathf.Atan2(vector3_5.y, vector3_5.z);
    float liftCoef;
    float dragCoef;
    this.GetLiftDragCoef(fields.airfoilID, alpha, out liftCoef, out dragCoef);
    float num5 = (float) (((double) fields.dragArea + (double) fields.wingArea * 0.10000000149011612) * (1.0 - (double) a1));
    float num6 = (float) (0.5 * (double) a2 * (double) num4 * 0.5 * ((double) fields.dragArea + (double) num5));
    float num7 = (float) ((double) dragCoef * (double) a2 * (double) num4 * 0.5) * fields.wingArea * a1;
    float3 float3 = -math.normalize((float3) vector3_4) * (num7 + num6);
    float num8 = math.length((float3) vector3_4);
    float speedOfSound = LevelInfo.GetSpeedOfSound(globalPosition.y);
    if ((double) num8 > (double) speedOfSound * 0.800000011920929 && (double) num8 < (double) speedOfSound * 1.2000000476837158)
    {
      float b = 0.2f;
      float num9 = 0.15f;
      float num10 = Mathf.Min(Mathf.Abs((speedOfSound - num8) / speedOfSound), b);
      float num11 = (b - num10) / b;
      float3 *= (float) (1.0 + (double) num11 * (double) num11 * (double) num11 * (double) num9);
    }
    Vector3 vector3_6 = -normalized * ((float) ((double) liftCoef * (double) a2 * (double) num4 * 0.5) * fields.wingArea * a1);
    Vector3 vector3_7 = zero + (vector3_6 + new Vector3(float3.x, float3.y, float3.z));
    if (num3 > 0.0)
      vector3_7 = Vector3.ClampMagnitude(vector3_7, (float) ((double) math.length((float3) fields.velocity) * 0.5 * (double) fields.mass * 60.0));
    if ((double) globalPosition.y < -100.0)
      vector3_7 += Vector3.up * fields.mass * 20f;
    if (!float.IsFinite(vector3_7.x) || !float.IsFinite(vector3_7.y) || !float.IsFinite(vector3_7.z))
      vector3_7 = Vector3.zero;
    JobForceType jobForceType;
    if (vector3_7 != Vector3.zero)
    {
      fields.force = vector3_7;
      if (fields.centerOfLift != Vector3.zero)
      {
        jobForceType = JobForceType.ForceAndTorque;
        Vector3 vector3_8 = liftTransform.Rotation * fields.centerOfLift;
        fields.torque = Vector3.Cross(vector3_7, -vector3_8);
      }
      else
        jobForceType = JobForceType.Force;
    }
    else
      jobForceType = JobForceType.NoForce;
    fields.hasForce = jobForceType;
    fields.splashed = flag1;
    fields.buoyancy = num2;
    bool flag2 = (double) fields.angularDrag != (double) num1;
    if (flag2)
      fields.angularDrag = num1;
    fields.angularDragChanged = flag2;
  }

  private void GetLiftDragCoef(int airfoilID, float alpha, out float liftCoef, out float dragCoef)
  {
    if (airfoilID < 0)
    {
      liftCoef = 1.8f * Mathf.Sin(5f * alpha);
      dragCoef = (float) (1.5 * (1.0 - (double) Mathf.Cos(2f * alpha)) + 0.019999999552965164);
    }
    else
    {
      ReadOnlySpan<float> chart1 = this.liftCharts.AsReadOnlySpan().Slice(airfoilID * 128 /*0x80*/, 128 /*0x80*/);
      ReadOnlySpan<float> chart2 = this.dragCharts.AsReadOnlySpan().Slice(airfoilID * 128 /*0x80*/, 128 /*0x80*/);
      float index = (float) ((double) alpha * 20.371849060058594 + 64.0);
      liftCoef = ChartHelper.SafeRead(index, chart1);
      dragCoef = ChartHelper.SafeRead(index, chart2);
    }
  }

  public float GetAirDensity(float altitude)
  {
    return ChartHelper.SafeRead(altitude * 0.0021f, this.airDensityChart.AsReadOnlySpan());
  }

  private Vector3 CalcWind(Vector3 globalPosition)
  {
    Vector3 vector3_1 = this.shared.Ref().timeSinceLevelLoad * Vector3.one * 10f;
    Vector3 normalized = this.windVelocity.normalized;
    globalPosition += vector3_1;
    Vector3 vector3_2 = (float) ((double) Mathf.PerlinNoise1D(globalPosition.z * 0.02f) - 0.75 + 0.5 * (double) Mathf.PerlinNoise1D(globalPosition.z * 0.1f)) * normalized + (float) (0.5 * ((double) Mathf.PerlinNoise1D(globalPosition.x * 0.02f) - 0.75 + 0.5 * (double) Mathf.PerlinNoise1D(globalPosition.x * 0.1f))) * Vector3.Cross(normalized, Vector3.up) + (float) (0.30000001192092896 * ((double) Mathf.PerlinNoise1D(globalPosition.y * 0.02f) - 0.75 + 0.5 * (double) Mathf.PerlinNoise1D(globalPosition.y * 0.1f))) * Vector3.up;
    return this.windVelocity + this.windTurbulence * Mathf.Max(this.windVelocity.magnitude, 10f) * vector3_2;
  }
}

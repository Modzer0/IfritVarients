// Decompiled with JetBrains decompiler
// Type: NuclearOption.Jobs.GroundVehicleJob_Math1
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
public struct GroundVehicleJob_Math1 : IJobParallelFor
{
  private static readonly ProfilerMarker executeMarker = new ProfilerMarker("GroundVehicleJob_Math1 Execute");
  private static readonly ProfilerMarker inputsMarker = new ProfilerMarker("GroundVehicleJob_Math1 Inputs");
  private static readonly ProfilerMarker avoidObstaclesMarker = new ProfilerMarker("GroundVehicleJob_Math1 AvoidObstacles");
  private static readonly QueryParameters queryParam = new QueryParameters(-8193);
  [ReadOnly]
  public NativeArray<JobTransformValues> transformValues;
  [ReadOnly]
  public NativeArray<Ptr<GroundVehicleFields>> fields;
  [NativeDisableParallelForRestriction]
  [WriteOnly]
  public NativeArray<RaycastCommand> rayCommands;
  [NativeDisableUnsafePtrRestriction]
  public Ptr<JobSharedFields> shared;

  public void Execute(int i)
  {
    JobPerf.GetTimestampBurst();
    ref GroundVehicleFields local1 = ref this.fields[i].Ref();
    ref JobTransformValues.ReadOnly local2 = ref this.transformValues.GetReadOnlyRef(i);
    this.Execute(i, ref local1, ref local2);
    this.UpdateRayCommands(i, ref local1, ref local2);
  }

  private void Execute(
    int i,
    ref GroundVehicleFields fields,
    ref JobTransformValues.ReadOnly transform)
  {
    if (!fields.monoBehaviourEnabled)
      return;
    fields.ResetForceResults();
    fields.speed = Vector3.Dot(fields.velocity, transform.Forward());
    if (fields.unitDisabled && !fields.unitWasDisabled)
    {
      fields.unitWasDisabled = true;
      fields.acceleration = 0.0f;
      int num = (i + this.shared.Ref().tickOffset) % 3 - 1;
      fields.inputs.steering = (float) num;
      fields.inputs.brake = 0.0f;
      fields.stationary = false;
      fields.stationaryTime = 0.0f;
    }
    else if (!fields.unitDisabled && fields.unitWasDisabled)
      fields.unitWasDisabled = false;
    if ((double) Mathf.Abs(fields.speed) < 0.30000001192092896 && fields.sampleGroundResult.didHit && !fields.sampleGroundResult.hasHitRB)
      fields.stationaryTime += this.shared.Ref().fixedDeltaTime;
    else
      fields.stationaryTime = 0.0f;
    fields.stationary = (double) fields.stationaryTime > 5.0 && (double) fields.inputs.throttle == 0.0;
    if (GroundVehicleJobSettings.ShouldRunInputs(i, this.shared.Ref().tickOffset))
    {
      this.Inputs(ref fields, ref transform);
      if (!fields.disabled)
      {
        fields.inputs.throttle = Mathf.Clamp(fields.inputs.throttle, -0.7f, 1f);
        fields.inputs.brake = Mathf.Clamp01(1f - Mathf.Abs(fields.inputs.throttle));
      }
    }
    fields.engineOutput = fields.inputs.throttle * fields.acceleration;
  }

  private void UpdateRayCommands(
    int i,
    ref GroundVehicleFields fields,
    ref JobTransformValues.ReadOnly transform)
  {
    if (!GroundVehicleJobSettings.ShouldRunSampleGround(i, this.shared.Ref().tickOffset))
      return;
    if (fields.monoBehaviourEnabled)
    {
      Vector3 position = transform.Position;
      Vector3 direction = transform.Down();
      float distance = fields.suspensionTravel * 3f;
      this.rayCommands[GroundVehicleJobSettings.SampleGroundIndex(i)] = new RaycastCommand(position, direction, GroundVehicleJob_Math1.queryParam, distance);
    }
    else
      this.rayCommands[GroundVehicleJobSettings.SampleGroundIndex(i)] = new RaycastCommand();
  }

  private void Inputs(ref GroundVehicleFields fields, ref JobTransformValues.ReadOnly transform)
  {
    if (!fields.underwater && (double) this.shared.Ref().datum.GlobalY(transform.Position) < -2.0)
      fields.underwater = true;
    if (!fields.mobile)
      return;
    fields.inputs.throttle = 0.0f;
    float y = fields.angularVelocity.y;
    if (fields.disabled)
    {
      fields.AddTorque((float) (((double) fields.inputs.steering * 1.0 - (double) y) * (double) fields.speed * 1.0) * fields.mass * transform.Up());
      fields.inputs.brake += 0.004f;
    }
    else
    {
      if (!fields.steeringInfoNullable.HasValue)
        return;
      SteeringInfo steeringInfo = fields.steeringInfoNullable.Value;
      float num1 = Mathf.Max((float) (((double) steeringInfo.nextWaypointAngle - 10.0) * 0.10000000149011612), 0.1f);
      Vector3 normalized = steeringInfo.steerVector.normalized;
      float num2 = Mathf.Clamp(80f / num1, 20f, fields.topSpeedOnroad);
      if ((double) Vector3.Dot(normalized, transform.Forward()) < 0.5)
        num2 = 6f;
      if ((double) fields.speed > (double) num2 * 0.27777698636054993)
      {
        fields.inputs.brake = 1f;
        fields.inputs.throttle = 0.0f;
      }
      else
        fields.inputs.throttle = 1f;
      float throttleInhibit;
      this.AvoidObstacles(ref fields, ref transform, ref normalized, out throttleInhibit);
      fields.bulldozeTimer += throttleInhibit * 0.2f;
      fields.bulldozeTimer -= 0.0200000014f;
      fields.bulldozeTimer = Mathf.Clamp(fields.bulldozeTimer, 0.0f, 3f);
      if ((double) fields.bulldozeTimer < 1.0)
        fields.inputs.throttle -= Mathf.Min(throttleInhibit, 0.5f);
      if ((double) fields.reverseTimer > 0.0)
      {
        fields.reverseTimer -= 0.2f;
        fields.inputs.throttle = -0.7f;
      }
      if ((double) fields.speed < 1.0 && (double) fields.inputs.throttle > 0.0)
        fields.stuckTimer += 0.2f;
      if ((double) fields.stuckTimer > 2.0)
      {
        fields.reverseTimer = 3f;
        fields.stuckTimer = 0.0f;
      }
      fields.inputs.steering = Mathf.Clamp(TargetCalc.GetAngleOnAxis(transform.Forward(), normalized, transform.Up()), -10f, 10f);
      fields.inputs.steering -= (float) ((double) y * ((double) fields.speed + 10.0) * 0.40000000596046448);
    }
  }

  private void AvoidObstacles(
    ref GroundVehicleFields fields,
    ref JobTransformValues.ReadOnly transform,
    ref Vector3 steerDirection,
    out float throttleInhibit)
  {
    throttleInhibit = 0.0f;
    int num1 = 0;
    Vector3 zero = Vector3.zero;
    Vector3 vector3_1 = Vector3.zero;
    for (int index = 0; index < fields.ObstaclesArray.Length; ++index)
    {
      ObstaclePosition obstaclePosition = fields.ObstaclesArray[index];
      if ((double) obstaclePosition.Radius != 0.0)
      {
        Vector3 vector3_2 = FastMath.Direction(transform.Position, obstaclePosition.Position);
        float sqrMagnitude = vector3_2.sqrMagnitude;
        float range = fields.maxRadius + obstaclePosition.Radius;
        if ((double) sqrMagnitude <= ((double) range + 50.0) * ((double) range + 50.0) && (double) Vector3.Dot(vector3_2, steerDirection) >= -1.0)
        {
          if ((double) obstaclePosition.Radius < 8.0)
          {
            Vector3 vector3_3 = -vector3_2.normalized;
            float num2 = Mathf.Max(Vector3.Dot(-vector3_3, transform.Forward()), 0.0f);
            vector3_1 = vector3_3 * Mathf.Min((float) (100.0 * (1.0 + (double) num2)) / sqrMagnitude, 0.5f);
            throttleInhibit += num2 / (sqrMagnitude * 0.1f);
          }
          else
          {
            Vector3 vector3_4 = transform.Position + Vector3.Project(vector3_2, steerDirection);
            Ptr<DebugVisJobMarker> marker1;
            DebugVisJobMarker debugVisJobMarker1;
            if (fields.DEBUG_VIS && this.shared.Ref().NextDebugIndex(out marker1))
            {
              ref DebugVisJobMarker local = ref marker1.Ref();
              debugVisJobMarker1 = new DebugVisJobMarker();
              debugVisJobMarker1.Type = DebugVisJobMarkerType.DebugPoint;
              debugVisJobMarker1.Position = vector3_4 + Vector3.up * 15f;
              DebugVisJobMarker debugVisJobMarker2 = debugVisJobMarker1;
              local = debugVisJobMarker2;
            }
            float num3 = obstaclePosition.Position.y + obstaclePosition.Top;
            if ((double) vector3_4.y < (double) num3 && FastMath.InRange(vector3_4, obstaclePosition.Position, range))
            {
              Vector3 to = obstaclePosition.Position + FastMath.NormalizedDirection(obstaclePosition.Position, vector3_4) * range;
              zero += FastMath.NormalizedDirection(transform.Position, to);
              ++num1;
              Ptr<DebugVisJobMarker> marker2;
              if (fields.DEBUG_VIS && this.shared.Ref().NextDebugIndex(out marker2))
              {
                ref DebugVisJobMarker local = ref marker2.Ref();
                debugVisJobMarker1 = new DebugVisJobMarker();
                debugVisJobMarker1.Type = DebugVisJobMarkerType.DebugArrowGreen;
                debugVisJobMarker1.Position = vector3_4 + Vector3.up * 15f;
                debugVisJobMarker1.Rotation = Quaternion.LookRotation(to - vector3_4);
                debugVisJobMarker1.Scale = new Vector3(2f, 2f, (to - vector3_4).magnitude);
                DebugVisJobMarker debugVisJobMarker3 = debugVisJobMarker1;
                local = debugVisJobMarker3;
              }
            }
          }
        }
      }
    }
    if (num1 > 0)
      steerDirection = zero;
    steerDirection += vector3_1;
  }
}

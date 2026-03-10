// Decompiled with JetBrains decompiler
// Type: NuclearOption.NetworkTransforms.ClientAuthChecks
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using Unity.Profiling;
using UnityEngine;

#nullable disable
namespace NuclearOption.NetworkTransforms;

public class ClientAuthChecks
{
  private static readonly ProfilerMarker validateV1Marker = new ProfilerMarker("ValidateInternal_v1");
  private GlobalPosition previousPosition;
  private Quaternion previousRotation;
  private Vector3 previousVelocity;
  private double previousTimestamp;
  private readonly float joinTime;

  public int TotalCount { get; private set; }

  public int RejectedCount { get; private set; }

  public ClientAuthChecks(GlobalPosition position, Vector3 velocity, double initialTimestamp)
  {
    this.previousPosition = position;
    this.previousVelocity = velocity;
    this.previousTimestamp = initialTimestamp;
    this.joinTime = Time.timeSinceLevelLoad;
  }

  public bool Run(
    ref NetworkTransformBase.NetworkSnapshot snapshot,
    out ClientAuthChecks.RejectMask rejectMask)
  {
    rejectMask = ClientAuthChecks.RejectMask.Accepted;
    ClientAuthChecks.RejectMask rejectMask1;
    int num = this.ValidateInternal_v1(ref snapshot, out rejectMask1) ? 1 : 0;
    rejectMask |= rejectMask1;
    ++this.TotalCount;
    if (num != 0)
      return num != 0;
    ++this.RejectedCount;
    return num != 0;
  }

  private bool ValidateInternal_v1(
    ref NetworkTransformBase.NetworkSnapshot snapshot,
    out ClientAuthChecks.RejectMask rejectMask)
  {
    using (ClientAuthChecks.validateV1Marker.Auto())
    {
      double num1 = snapshot.timestamp.Value;
      float num2 = (float) (num1 - this.previousTimestamp);
      Vector3 vector3_1 = snapshot.velocity.Decompress();
      Vector3 vector3_2 = (vector3_1 - this.previousVelocity) / num2;
      float sqrMagnitude = vector3_2.sqrMagnitude;
      if ((double) ((snapshot.globalPos - this.previousPosition) / num2).sqrMagnitude > 2500.0 && (double) sqrMagnitude > 3600.0)
      {
        float num3 = Vector3.Dot(this.previousVelocity.normalized, vector3_2.normalized);
        if ((double) num3 > 0.3)
        {
          if ((double) sqrMagnitude > 10000.0)
          {
            rejectMask = ClientAuthChecks.RejectMask.AccelerationForward;
            return false;
          }
        }
        else if ((double) num3 > -0.6)
        {
          if ((double) sqrMagnitude > 3600.0)
          {
            rejectMask = ClientAuthChecks.RejectMask.AccelerationPerpendicular;
            return false;
          }
        }
        else
        {
          float num4 = (Mathf.Sqrt(this.previousVelocity.sqrMagnitude) + 10f) / num2;
          if ((double) sqrMagnitude > (double) num4 * (double) num4)
          {
            rejectMask = ClientAuthChecks.RejectMask.AccelerationBackwards;
            return false;
          }
        }
      }
      GlobalPosition globalPos = snapshot.globalPos;
      Vector3 vector3_3 = (this.previousVelocity + vector3_1) / 2f;
      GlobalPosition b = this.previousPosition + vector3_3 * num2;
      float range = (float) ((double) vector3_3.magnitude * (double) num2 + 10.0);
      if (FastMath.OutOfRange(globalPos, b, range))
      {
        rejectMask = ClientAuthChecks.RejectMask.Position;
        return false;
      }
      this.previousPosition = globalPos;
      this.previousVelocity = vector3_1;
      this.previousTimestamp = num1;
      this.previousRotation = snapshot.rotation;
      rejectMask = ClientAuthChecks.RejectMask.Accepted;
      return true;
    }
  }

  public NetworkTransformBase.NetworkSnapshot CreateServerSnapshot()
  {
    return new NetworkTransformBase.NetworkSnapshot()
    {
      globalPos = this.previousPosition,
      rotation = this.previousRotation,
      velocity = this.previousVelocity.Compress()
    };
  }

  public static ClientAuthChecks.CheckMode Mode { get; private set; }

  public static void SetRunChecks(ClientAuthChecks.CheckMode? mode)
  {
    ClientAuthChecks.Mode = mode.GetValueOrDefault();
  }

  public enum CheckMode
  {
    None,
    LogOnly,
    Ignore,
  }

  [Flags]
  public enum RejectMask : uint
  {
    Accepted = 0,
    AccelerationForward = 1,
    AccelerationPerpendicular = 2,
    AccelerationBackwards = 4,
    Position = 8,
  }
}

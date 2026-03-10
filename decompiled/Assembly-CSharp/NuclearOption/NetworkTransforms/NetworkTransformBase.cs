// Decompiled with JetBrains decompiler
// Type: NuclearOption.NetworkTransforms.NetworkTransformBase
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Mirage;
using Mirage.Serialization;
using System;
using Unity.Profiling;
using UnityEngine;

#nullable disable
namespace NuclearOption.NetworkTransforms;

public abstract class NetworkTransformBase : NetworkBehaviour
{
  private static readonly ProfilerMarker getSnapshotForTimeMarker = new ProfilerMarker("GetSnapshotForTime");
  private static readonly ProfilerMarker extrapolateMarker = new ProfilerMarker("Extrapolate");
  [Tooltip("How much to multiply extrapolation offset by. 0 = no extrapolation, 1 = full extrapolation")]
  [Range(0.0f, 1f)]
  public float extrapolationFactor = 1f;
  private double nextUpdate;
  private bool forceSync;
  public readonly NetworkTransformBase.SnapshotBufferLocalSnapshot SnapshotBuffer = new NetworkTransformBase.SnapshotBufferLocalSnapshot();
  [NonSerialized]
  public bool debugActive;
  [NonSerialized]
  public SendTransformBatcherDebugger.Gui debugGui;
  [NonSerialized]
  private const int SYNC_VAR_COUNT = 0;
  [NonSerialized]
  private const int RPC_COUNT = 0;

  public SendTransformBatcher SendBatcher { get; private set; }

  public virtual void Setup(SendTransformBatcher sendBatcher) => this.SendBatcher = sendBatcher;

  public void ForceSync() => this.forceSync = true;

  public void ResetUpdateTime(double time)
  {
    this.nextUpdate = time + (double) this.SyncSettings.Interval;
  }

  public bool TimeToUpdate(double time)
  {
    if (!this.forceSync && time <= this.nextUpdate)
      return false;
    this.nextUpdate = time + (double) this.SyncSettings.Interval;
    this.forceSync = false;
    return true;
  }

  public abstract bool ShouldSend();

  public abstract void Write(NetworkWriter writer);

  public abstract void Receive(double timestamp, NetworkReader writer);

  public abstract void VisualUpdate(ref VisualUpdateTime visualTime);

  protected bool TryGetSnapshot(
    ref VisualUpdateTime visualTime,
    out NetworkTransformBase.ViewSnapshot snapshot)
  {
    if (this.SnapshotBuffer.Count < 1)
    {
      snapshot = new NetworkTransformBase.ViewSnapshot();
      return false;
    }
    using (NetworkTransformBase.getSnapshotForTimeMarker.Auto())
    {
      float num = this.SyncSettings.Interval * 2.5f;
      double snapshotTime = visualTime.interpolationTime - (double) num;
      snapshot = this.SnapshotBuffer.GetSnapshotForTime(snapshotTime, this.debugActive, ref this.debugGui);
    }
    double extrapolationTime = visualTime.interpolationTime + visualTime.extrapolationOffset * (double) this.extrapolationFactor;
    if (this.debugActive)
    {
      float num = snapshot.SnapshotAge(extrapolationTime);
      this.debugGui.snapshot_velocity = num * snapshot.Velocity;
      this.debugGui.snapshot_acceleration = 0.5f * num * num * snapshot.Acceleration;
    }
    using (NetworkTransformBase.extrapolateMarker.Auto())
      snapshot = snapshot.Extrapolate(extrapolationTime, visualTime.maxExtrapolateAge);
    return true;
  }

  private void MirageProcessed()
  {
  }

  protected override int GetRpcCount() => 0;

  [NetworkMessage]
  public struct NetworkSnapshot(
    CompressedInputs? clientInputs,
    GlobalPosition globalPos,
    Quaternion rotation,
    Vector3 velocity)
  {
    public CompressedInputs? ClientInputs = clientInputs;
    public double? timestamp = new double?();
    public float? extraExtrapolation = new float?();
    public GlobalPosition globalPos = globalPos;
    public Vector3Compressed velocity = velocity.Compress();
    [QuaternionPack(12)]
    public Quaternion rotation = rotation;
    internal static QuaternionPacker rotation__Packer = new QuaternionPacker(12);

    public NetworkTransformBase.LocalSnapshot ToLocal()
    {
      return new NetworkTransformBase.LocalSnapshot()
      {
        globalPos = this.globalPos,
        rotation = new Quaternion?(this.rotation),
        velocity = new Vector3?(this.velocity.Decompress()),
        extraExtrapolation = this.extraExtrapolation.GetValueOrDefault()
      };
    }

    public bool Valid(bool logErrors)
    {
      bool flag = true;
      if (this.ClientInputs.HasValue)
        flag &= this.ClientInputs.Value.Valid(logErrors);
      if (this.timestamp.HasValue)
        flag &= NetworkFloatHelper.Validate(this.timestamp.Value, logErrors, "timestamp");
      if (this.extraExtrapolation.HasValue)
        flag &= NetworkFloatHelper.Validate(this.extraExtrapolation.Value, logErrors, "extraExtrapolation");
      return flag & NetworkFloatHelper.Validate(this.globalPos, logErrors, "globalPos") & NetworkFloatHelper.Validate(this.velocity, logErrors, "velocity") & NetworkFloatHelper.Validate(this.rotation, logErrors, "rotation");
    }
  }

  public struct LocalSnapshot
  {
    public GlobalPosition globalPos;
    public float extraExtrapolation;
    public Vector3? velocity;
    public Quaternion? rotation;
  }

  public class SnapshotBufferLocalSnapshot : NuclearOption.NetworkTransforms.SnapshotBuffer<NetworkTransformBase.LocalSnapshot>
  {
    public NetworkTransformBase.ViewSnapshot GetSnapshotForTime(double snapshotTime)
    {
      SendTransformBatcherDebugger.Gui debugGui = new SendTransformBatcherDebugger.Gui();
      return this.GetSnapshotForTime(snapshotTime, false, ref debugGui);
    }

    public NetworkTransformBase.ViewSnapshot GetSnapshotForTime(
      double snapshotTime,
      bool debugActive,
      ref SendTransformBatcherDebugger.Gui debugGui)
    {
      NetworkTransformBase.SnapshotBufferLocalSnapshot bufferLocalSnapshot = this;
      for (int index = bufferLocalSnapshot.Count - 1; index >= 0; --index)
      {
        NuclearOption.NetworkTransforms.SnapshotBuffer<NetworkTransformBase.LocalSnapshot>.TimedSnapshot timedSnapshot = bufferLocalSnapshot.Get(index);
        if (snapshotTime > timedSnapshot.Timestamp)
        {
          if (index == bufferLocalSnapshot.Count - 1)
          {
            if (index > 0)
            {
              if (debugActive)
                debugGui.snapType = $"After Last ({bufferLocalSnapshot.Count})";
              return this.AfterCurrent(index);
            }
            if (debugActive)
              debugGui.snapType = $"After Last ({bufferLocalSnapshot.Count})";
            return this.FromOne(index);
          }
          if (debugActive)
            debugGui.snapType = $"Between {index - 1}->{index} ({bufferLocalSnapshot.Count})";
          return this.Between(index + 1, snapshotTime);
        }
      }
      if (debugActive)
        debugGui.snapType = $"before first ({bufferLocalSnapshot.Count})";
      return this.FromOne(0);
    }

    internal NetworkTransformBase.ViewSnapshot FromOne(int currentIndex)
    {
      NuclearOption.NetworkTransforms.SnapshotBuffer<NetworkTransformBase.LocalSnapshot>.TimedSnapshot timedSnapshot = this.buffer[currentIndex];
      return new NetworkTransformBase.ViewSnapshot()
      {
        ExtraExtrapolation = timedSnapshot.Snapshot.extraExtrapolation,
        Timestamp = timedSnapshot.Timestamp,
        Position = timedSnapshot.Snapshot.globalPos.ToLocalPosition(),
        Rotation = timedSnapshot.Snapshot.rotation.GetValueOrDefault(),
        Velocity = timedSnapshot.Snapshot.velocity ?? Vector3.zero,
        Acceleration = Vector3.zero
      };
    }

    internal NetworkTransformBase.ViewSnapshot Between(int currentIndex, double snapshotTime)
    {
      NuclearOption.NetworkTransforms.SnapshotBuffer<NetworkTransformBase.LocalSnapshot>.TimedSnapshot timedSnapshot1 = this.buffer[currentIndex - 1];
      NuclearOption.NetworkTransforms.SnapshotBuffer<NetworkTransformBase.LocalSnapshot>.TimedSnapshot timedSnapshot2 = this.buffer[currentIndex];
      float num1 = FastMath.InverseLerp(timedSnapshot1.Timestamp, timedSnapshot2.Timestamp, snapshotTime);
      float num2 = Mathf.Lerp(timedSnapshot1.Snapshot.extraExtrapolation, timedSnapshot2.Snapshot.extraExtrapolation, num1);
      Vector3 localPosition = FastMath.LerpUnclamped(timedSnapshot1.Snapshot.globalPos, timedSnapshot2.Snapshot.globalPos, num1).ToLocalPosition();
      NetworkTransformBase.ViewSnapshot viewSnapshot = new NetworkTransformBase.ViewSnapshot()
      {
        ExtraExtrapolation = num2,
        Timestamp = snapshotTime,
        Position = localPosition
      };
      if (timedSnapshot2.Snapshot.rotation.HasValue)
        viewSnapshot.Rotation = Quaternion.SlerpUnclamped(timedSnapshot1.Snapshot.rotation.Value, timedSnapshot2.Snapshot.rotation.Value, num1);
      if (timedSnapshot2.Snapshot.velocity.HasValue)
      {
        viewSnapshot.Velocity = Vector3.LerpUnclamped(timedSnapshot1.Snapshot.velocity.Value, timedSnapshot2.Snapshot.velocity.Value, num1);
        Vector3 a = this.AccelerationOfMiddle(currentIndex - 1);
        Vector3 b = this.AccelerationOfMiddle(currentIndex);
        viewSnapshot.Acceleration = Vector3.LerpUnclamped(a, b, num1);
      }
      else
      {
        Vector3 a = this.VelocityOfMiddle(currentIndex - 1);
        Vector3 b = this.VelocityOfMiddle(currentIndex);
        viewSnapshot.Velocity = Vector3.LerpUnclamped(a, b, Mathf.Sqrt(num1));
        viewSnapshot.Acceleration = Vector3.zero;
      }
      return viewSnapshot;
    }

    internal NetworkTransformBase.ViewSnapshot AfterCurrent(int currentIndex)
    {
      NuclearOption.NetworkTransforms.SnapshotBuffer<NetworkTransformBase.LocalSnapshot>.TimedSnapshot timedSnapshot1 = this.buffer[currentIndex - 1];
      NuclearOption.NetworkTransforms.SnapshotBuffer<NetworkTransformBase.LocalSnapshot>.TimedSnapshot timedSnapshot2 = this.buffer[currentIndex];
      NetworkTransformBase.ViewSnapshot viewSnapshot = new NetworkTransformBase.ViewSnapshot()
      {
        ExtraExtrapolation = timedSnapshot2.Snapshot.extraExtrapolation,
        Timestamp = timedSnapshot2.Timestamp,
        Position = timedSnapshot2.Snapshot.globalPos.ToLocalPosition(),
        Rotation = timedSnapshot2.Snapshot.rotation.GetValueOrDefault()
      };
      float num = (float) (timedSnapshot2.Timestamp - timedSnapshot1.Timestamp);
      if (timedSnapshot2.Snapshot.velocity.HasValue)
      {
        viewSnapshot.Velocity = timedSnapshot2.Snapshot.velocity.Value;
        Vector3 vector3 = timedSnapshot2.Snapshot.velocity.Value - timedSnapshot1.Snapshot.velocity.Value;
        viewSnapshot.Acceleration = vector3 / num;
      }
      else
      {
        Vector3 vector3 = timedSnapshot2.Snapshot.globalPos - timedSnapshot1.Snapshot.globalPos;
        viewSnapshot.Velocity = vector3 / num / 2f;
        viewSnapshot.Acceleration = Vector3.zero;
      }
      return viewSnapshot;
    }

    private Vector3 VelocityOfMiddle(int middleIndex)
    {
      NuclearOption.NetworkTransforms.SnapshotBuffer<NetworkTransformBase.LocalSnapshot>.TimedSnapshot before;
      NuclearOption.NetworkTransforms.SnapshotBuffer<NetworkTransformBase.LocalSnapshot>.TimedSnapshot after;
      this.GetEitherSide(middleIndex, out before, out after);
      double timestamp1 = before.Timestamp;
      GlobalPosition globalPos = before.Snapshot.globalPos;
      double timestamp2 = after.Timestamp;
      return (after.Snapshot.globalPos - globalPos) / (float) (timestamp2 - timestamp1);
    }

    private Vector3 AccelerationOfMiddle(int middleIndex)
    {
      NuclearOption.NetworkTransforms.SnapshotBuffer<NetworkTransformBase.LocalSnapshot>.TimedSnapshot before;
      NuclearOption.NetworkTransforms.SnapshotBuffer<NetworkTransformBase.LocalSnapshot>.TimedSnapshot after;
      this.GetEitherSide(middleIndex, out before, out after);
      double timestamp1 = before.Timestamp;
      Vector3? velocity = before.Snapshot.velocity;
      Vector3 vector3_1 = velocity ?? Vector3.zero;
      double timestamp2 = after.Timestamp;
      velocity = after.Snapshot.velocity;
      Vector3 vector3_2 = (velocity ?? Vector3.zero) - vector3_1;
      double num1 = timestamp1;
      double num2 = timestamp2 - num1;
      return new Vector3(vector3_2.x / (float) num2, vector3_2.y / (float) num2, vector3_2.z / (float) num2);
    }

    private void GetEitherSide(
      int middleIndex,
      out NuclearOption.NetworkTransforms.SnapshotBuffer<NetworkTransformBase.LocalSnapshot>.TimedSnapshot before,
      out NuclearOption.NetworkTransforms.SnapshotBuffer<NetworkTransformBase.LocalSnapshot>.TimedSnapshot after)
    {
      int index1 = middleIndex - 1;
      if (index1 < 0)
        index1 = middleIndex;
      int index2 = middleIndex + 1;
      if (index2 >= this.Count)
        index2 = middleIndex;
      before = this.buffer[index1];
      after = this.buffer[index2];
    }
  }

  public struct ViewSnapshot
  {
    public float ExtraExtrapolation;
    public double Timestamp;
    public Vector3 Position;
    public Quaternion Rotation;
    public Vector3 Velocity;
    public Vector3 Acceleration;

    public void Deconstruct(
      out double timestamp,
      out Vector3 pos,
      out Quaternion rot,
      out Vector3 vel,
      out Vector3 acc)
    {
      timestamp = this.Timestamp;
      pos = this.Position;
      rot = this.Rotation;
      vel = this.Velocity;
      acc = this.Acceleration;
    }

    public readonly float SnapshotAge(double extrapolationTime)
    {
      return (float) (extrapolationTime + (double) this.ExtraExtrapolation - this.Timestamp);
    }

    public NetworkTransformBase.ViewSnapshot Extrapolate(
      double extrapolationTime,
      float maxExtrapolateAge)
    {
      (double _, Vector3 pos, Quaternion rot, Vector3 vel, Vector3 acc) = this;
      float num = this.SnapshotAge(extrapolationTime);
      if ((double) num > (double) maxExtrapolateAge)
        num = maxExtrapolateAge;
      Vector3 vector3_1 = vel * num;
      Vector3 vector3_2 = pos + vector3_1 + 0.5f * num * num * acc;
      Vector3 vector3_3 = vel + num * acc;
      return new NetworkTransformBase.ViewSnapshot()
      {
        Position = vector3_2,
        Rotation = rot,
        Velocity = vector3_3
      };
    }
  }
}

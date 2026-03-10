// Decompiled with JetBrains decompiler
// Type: NuclearOption.NetworkTransforms.GroundVehicleNetworkTransform
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Mirage.Serialization;
using System;
using Unity.Profiling;
using UnityEngine;

#nullable disable
namespace NuclearOption.NetworkTransforms;

public class GroundVehicleNetworkTransform : NetworkTransformBase
{
  private static readonly ProfilerMarker applySnapshotMarker = new ProfilerMarker("ApplySnapshot");
  private static readonly ProfilerMarker visualUpdateMarker = new ProfilerMarker("GroundVehicle.VisualUpdate");
  public GroundVehicle GroundVehicle;
  private Vector3 smoothingVel;
  private Vector3 rotationSmoothingVel;
  [NonSerialized]
  private const int SYNC_VAR_COUNT = 0;
  [NonSerialized]
  private const int RPC_COUNT = 0;

  public override bool ShouldSend() => !this.GroundVehicle.rb.isKinematic;

  public override void Write(NetworkWriter writer)
  {
    Vector3 position;
    Quaternion rotation;
    this.transform.GetPositionAndRotation(out position, out rotation);
    writer.Write<GlobalPosition>(position.ToGlobalPosition());
    writer.Write<Quaternion>(rotation);
  }

  public override void Receive(double timestamp, NetworkReader reader)
  {
    GlobalPosition globalPosition = reader.Read<GlobalPosition>();
    Quaternion quaternion = reader.Read<Quaternion>();
    if (NetworkFloatHelper.Validate(globalPosition, true, "GroundVehicle.Position") && NetworkFloatHelper.Validate(quaternion, true, "GroundVehicle.Rotation"))
      this.SnapshotBuffer.Insert(timestamp, new NetworkTransformBase.LocalSnapshot()
      {
        globalPos = globalPosition,
        rotation = new Quaternion?(quaternion)
      });
    else
      Debug.LogError((object) "Ignoring invalid GroundVehicle snapshot from server");
  }

  public override void VisualUpdate(ref VisualUpdateTime visualTime)
  {
    using (GroundVehicleNetworkTransform.visualUpdateMarker.Auto())
    {
      NetworkTransformBase.ViewSnapshot snapshot;
      if (this.GroundVehicle.rb.isKinematic || this.GroundVehicle.LocalSim || !this.TryGetSnapshot(ref visualTime, out snapshot))
        return;
      this.ApplySnapshot(snapshot);
    }
  }

  private void ApplySnapshot(NetworkTransformBase.ViewSnapshot snapshot)
  {
    using (GroundVehicleNetworkTransform.applySnapshotMarker.Auto())
    {
      Vector3 position1 = snapshot.Position;
      Vector3 velocity = snapshot.Velocity;
      Quaternion rotation = snapshot.Rotation;
      Rigidbody rb = this.GroundVehicle.rb;
      Vector3 position2 = this.transform.position;
      if (this.debugActive)
        this.debugGui.CalculateInfluence(snapshot, position2);
      this.transform.SetPositionAndRotation(position1, rotation);
      rb.velocity = velocity;
      rb.angularVelocity = Vector3.zero;
      rb.Move(position1, rotation);
    }
  }

  private void MirageProcessed()
  {
  }

  protected override int GetRpcCount() => 0;
}

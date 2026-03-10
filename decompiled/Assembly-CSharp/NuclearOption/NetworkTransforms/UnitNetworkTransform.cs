// Decompiled with JetBrains decompiler
// Type: NuclearOption.NetworkTransforms.UnitNetworkTransform
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Mirage.Serialization;
using System;
using Unity.Profiling;
using UnityEngine;

#nullable disable
namespace NuclearOption.NetworkTransforms;

public class UnitNetworkTransform : NetworkTransformBase
{
  private static readonly ProfilerMarker applySnapshotMarker = new ProfilerMarker("ApplySnapshot");
  private static readonly ProfilerMarker visualUpdateMarker = new ProfilerMarker("Unit.VisualUpdate");
  public Unit Unit;
  private Vector3 smoothingVel;
  private Vector3 rotationSmoothingVel;
  [NonSerialized]
  private const int SYNC_VAR_COUNT = 0;
  [NonSerialized]
  private const int RPC_COUNT = 0;

  public override bool ShouldSend() => !this.Unit.rb.isKinematic;

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
    if (NetworkFloatHelper.Validate(globalPosition, true, "Unit.Position") && NetworkFloatHelper.Validate(quaternion, true, "Unit.Rotation"))
      this.SnapshotBuffer.Insert(timestamp, new NetworkTransformBase.LocalSnapshot()
      {
        globalPos = globalPosition,
        rotation = new Quaternion?(quaternion)
      });
    else
      Debug.LogError((object) "Ignoring invalid Unit snapshot from server");
  }

  public override void VisualUpdate(ref VisualUpdateTime visualTime)
  {
    using (UnitNetworkTransform.visualUpdateMarker.Auto())
    {
      NetworkTransformBase.ViewSnapshot snapshot;
      if (this.Unit.rb.isKinematic || this.Unit.LocalSim || !this.TryGetSnapshot(ref visualTime, out snapshot))
        return;
      this.ApplySnapshot(snapshot, visualTime.snap);
    }
  }

  private void ApplySnapshot(NetworkTransformBase.ViewSnapshot snapshot, bool snap)
  {
    using (UnitNetworkTransform.applySnapshotMarker.Auto())
    {
      Vector3 position1 = snapshot.Position;
      Vector3 velocity = snapshot.Velocity;
      Quaternion rotation = snapshot.Rotation;
      Rigidbody rb = this.Unit.rb;
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

// Decompiled with JetBrains decompiler
// Type: NuclearOption.NetworkTransforms.ShipNetworkTransform
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Mirage.Serialization;
using System;
using Unity.Profiling;
using UnityEngine;

#nullable disable
namespace NuclearOption.NetworkTransforms;

public class ShipNetworkTransform : NetworkTransformBase
{
  private static readonly ProfilerMarker visualUpdateMarker = new ProfilerMarker("GroundVehicle.VisualUpdate");
  public Ship Ship;
  public NetworkPIDSmoother networkSmoother;
  [NonSerialized]
  private const int SYNC_VAR_COUNT = 0;
  [NonSerialized]
  private const int RPC_COUNT = 0;

  private void Awake() => this.Ship.onInitialize += new Action(this.Ship_onInitialize);

  private void Ship_onInitialize()
  {
    if (this.IsServer)
      return;
    this.networkSmoother.Initialize(this.Ship.rb);
  }

  public override bool ShouldSend() => !this.Ship.rb.isKinematic;

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
    if (NetworkFloatHelper.Validate(globalPosition, true, "Ship.Position") && NetworkFloatHelper.Validate(quaternion, true, "Ship.Rotation"))
      this.SnapshotBuffer.Insert(timestamp, new NetworkTransformBase.LocalSnapshot()
      {
        globalPos = globalPosition,
        rotation = new Quaternion?(quaternion)
      });
    else
      Debug.LogError((object) "Ignoring invalid Ship snapshot from server");
  }

  public override void VisualUpdate(ref VisualUpdateTime visualTime)
  {
    using (ShipNetworkTransform.visualUpdateMarker.Auto())
    {
      Rigidbody rb = this.Ship.rb;
      NetworkTransformBase.ViewSnapshot snapshot;
      if (rb.isKinematic || !this.TryGetSnapshot(ref visualTime, out snapshot))
        return;
      this.networkSmoother.SmoothRB(rb, snapshot);
    }
  }

  private void MirageProcessed()
  {
  }

  protected override int GetRpcCount() => 0;
}

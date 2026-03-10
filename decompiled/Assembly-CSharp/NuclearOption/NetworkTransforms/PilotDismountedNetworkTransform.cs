// Decompiled with JetBrains decompiler
// Type: NuclearOption.NetworkTransforms.PilotDismountedNetworkTransform
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Mirage.Serialization;
using System;
using UnityEngine;

#nullable disable
namespace NuclearOption.NetworkTransforms;

public class PilotDismountedNetworkTransform : NetworkTransformBase
{
  public PilotDismounted Pilot;
  private Vector3 smoothingVel;
  private Vector3 rotationSmoothingVel;
  private bool spawnLerpTimeStarted;
  private float spawnLerpTime;
  private const float SPAWN_SMOOTH_TIME = 12f;
  [NonSerialized]
  private const int SYNC_VAR_COUNT = 0;
  [NonSerialized]
  private const int RPC_COUNT = 0;

  public override bool ShouldSend() => !this.Pilot.rb.isKinematic;

  public override void Write(NetworkWriter writer)
  {
    Rigidbody rb = this.Pilot.rb;
    Vector3 position = rb.position;
    Quaternion rotation = rb.rotation;
    Vector3 velocity = rb.velocity;
    writer.Write<GlobalPosition>(position.ToGlobalPosition());
    writer.Write<Quaternion>(rotation);
    writer.Write<Vector3Compressed>(velocity.Compress());
  }

  public override void Receive(double timestamp, NetworkReader reader)
  {
    GlobalPosition globalPosition = reader.Read<GlobalPosition>();
    Quaternion quaternion = reader.Read<Quaternion>();
    Vector3Compressed vector3Compressed = reader.Read<Vector3Compressed>();
    if (NetworkFloatHelper.Validate(globalPosition, true, "Pilot.Position") && NetworkFloatHelper.Validate(quaternion, true, "Pilot.Rotation") && NetworkFloatHelper.Validate(vector3Compressed, true, "Pilot.Velocity"))
      this.SnapshotBuffer.Insert(timestamp, new NetworkTransformBase.LocalSnapshot()
      {
        globalPos = globalPosition,
        rotation = new Quaternion?(quaternion),
        velocity = new Vector3?(vector3Compressed.Decompress())
      });
    else
      Debug.LogError((object) "Ignoring invalid Pilot snapshot from server");
  }

  public override void VisualUpdate(ref VisualUpdateTime visualTime)
  {
    NetworkTransformBase.ViewSnapshot snapshot;
    if (this.Pilot.rb.isKinematic || this.Pilot.LocalSim || this.Pilot.IsOnEjectionRail || !this.TryGetSnapshot(ref visualTime, out snapshot))
      return;
    this.ApplySnapshot(snapshot);
  }

  private void ApplySnapshot(NetworkTransformBase.ViewSnapshot snapshot)
  {
    if (!this.spawnLerpTimeStarted)
    {
      this.spawnLerpTimeStarted = true;
      this.spawnLerpTime = !((UnityEngine.Object) this.Pilot.cockpitPart != (UnityEngine.Object) null) ? -12f : this.Pilot.timeSinceSpawn;
    }
    Vector3 vector3 = snapshot.Position;
    Quaternion rotation = snapshot.Rotation;
    Vector3 b = snapshot.Velocity;
    Rigidbody rb = this.Pilot.rb;
    float num = this.Pilot.timeSinceSpawn - this.spawnLerpTime;
    if ((double) num < 12.0 && (UnityEngine.Object) this.Pilot.cockpitPart != (UnityEngine.Object) null)
    {
      vector3 = Vector3.Lerp(rb.position, vector3, num / 12f);
      b = Vector3.Lerp(rb.velocity, b, num / 12f);
    }
    this.transform.SetPositionAndRotation(vector3, rotation);
    rb.velocity = b;
    rb.angularVelocity = Vector3.zero;
    rb.Move(vector3, rotation);
  }

  private void MirageProcessed()
  {
  }

  protected override int GetRpcCount() => 0;
}

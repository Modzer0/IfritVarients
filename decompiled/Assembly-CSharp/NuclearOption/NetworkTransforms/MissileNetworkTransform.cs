// Decompiled with JetBrains decompiler
// Type: NuclearOption.NetworkTransforms.MissileNetworkTransform
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Mirage.Serialization;
using System;
using UnityEngine;

#nullable disable
namespace NuclearOption.NetworkTransforms;

public class MissileNetworkTransform : NetworkTransformBase
{
  public Missile Missile;
  private Vector3 smoothingVel;
  private Vector3 rotationSmoothingVel;
  private bool spawnLerpTimeStarted;
  private float spawnLerpTime;
  private const float SPAWN_SMOOTH_TIME = 4f;
  private float smoothing = 0.05f;
  [NonSerialized]
  private const int SYNC_VAR_COUNT = 0;
  [NonSerialized]
  private const int RPC_COUNT = 0;

  private void Awake() => this.smoothing = this.GetSmoothing();

  private float GetSmoothing() => 0.05f;

  public override bool ShouldSend() => true;

  public override void Write(NetworkWriter writer)
  {
    Rigidbody rb = this.Missile.rb;
    Vector3 position = rb.position;
    Vector3 velocity = rb.velocity;
    writer.Write<GlobalPosition>(position.ToGlobalPosition());
    writer.Write<Vector3Compressed>(velocity.Compress());
  }

  public override void Receive(double timestamp, NetworkReader reader)
  {
    GlobalPosition globalPosition = reader.Read<GlobalPosition>();
    Vector3Compressed vector3Compressed = reader.Read<Vector3Compressed>();
    if (NetworkFloatHelper.Validate(globalPosition, true, "Missile.Position") && NetworkFloatHelper.Validate(vector3Compressed, true, "Missile.Velocity"))
      this.SnapshotBuffer.Insert(timestamp, new NetworkTransformBase.LocalSnapshot()
      {
        globalPos = globalPosition,
        velocity = new Vector3?(vector3Compressed.Decompress())
      });
    else
      Debug.LogError((object) "Ignoring invalid Missile snapshot from server");
  }

  public override void VisualUpdate(ref VisualUpdateTime visualTime)
  {
    NetworkTransformBase.ViewSnapshot snapshot;
    if (this.Missile.disabled || this.Missile.rb.isKinematic || !this.TryGetSnapshot(ref visualTime, out snapshot))
      return;
    this.ApplySnapshot(snapshot);
  }

  private void ApplySnapshot(NetworkTransformBase.ViewSnapshot snapshot)
  {
    Vector3 vector3_1 = snapshot.Position;
    Vector3 vector3_2 = snapshot.Velocity;
    Quaternion rotation = FastMath.LookRotation(vector3_2);
    Rigidbody rb = this.Missile.rb;
    if (!this.spawnLerpTimeStarted)
    {
      this.spawnLerpTimeStarted = true;
      this.spawnLerpTime = !((UnityEngine.Object) this.Missile.owner != (UnityEngine.Object) null) ? -4f : this.Missile.timeSinceSpawn;
    }
    float num = this.Missile.timeSinceSpawn - this.spawnLerpTime;
    if ((double) num < 4.0 && (UnityEngine.Object) this.Missile.owner != (UnityEngine.Object) null)
    {
      vector3_1 = Vector3.Lerp(rb.position, vector3_1, num / 4f);
      vector3_2 = Vector3.Lerp(rb.velocity, vector3_2, num / 4f);
    }
    this.transform.SetPositionAndRotation(vector3_1, rotation);
    rb.velocity = vector3_2;
    rb.angularVelocity = Vector3.zero;
    rb.Move(vector3_1, rotation);
  }

  private void MirageProcessed()
  {
  }

  protected override int GetRpcCount() => 0;
}

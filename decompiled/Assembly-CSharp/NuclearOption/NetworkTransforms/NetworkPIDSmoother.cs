// Decompiled with JetBrains decompiler
// Type: NuclearOption.NetworkTransforms.NetworkPIDSmoother
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;

#nullable disable
namespace NuclearOption.NetworkTransforms;

[Serializable]
public class NetworkPIDSmoother
{
  [SerializeField]
  private Vector3 posPID;
  [SerializeField]
  private Vector3 rotPID;
  [SerializeField]
  private float positionTeleportThreshold = 50f;
  [SerializeField]
  private float rotationTeleportThreshold = 20f;
  private PID xPositionPID;
  private PID yPositionPID;
  private PID zPositionPID;
  private PID xRotationPID;
  private PID yRotationPID;
  private PID zRotationPID;

  public void Initialize(Rigidbody rb)
  {
    rb.useGravity = false;
    this.xPositionPID = new PID(this.posPID);
    this.yPositionPID = new PID(this.posPID);
    this.zPositionPID = new PID(this.posPID);
    this.xRotationPID = new PID(this.rotPID);
    this.yRotationPID = new PID(this.rotPID);
    this.zRotationPID = new PID(this.rotPID);
  }

  public void SmoothRB(Rigidbody rb, NetworkTransformBase.ViewSnapshot snapshot)
  {
    float deltaTime = Mathf.Clamp(Time.deltaTime, 1f / 1000f, 0.2f);
    Vector3 vector3_1 = snapshot.Position - rb.position;
    if ((double) vector3_1.sqrMagnitude > (double) this.positionTeleportThreshold * (double) this.positionTeleportThreshold)
    {
      rb.MovePosition(snapshot.Position);
      rb.velocity = snapshot.Velocity;
    }
    else
      rb.AddForce(new Vector3()
      {
        x = this.xPositionPID.GetOutput(vector3_1.x, deltaTime),
        y = this.yPositionPID.GetOutput(vector3_1.y, deltaTime),
        z = this.zPositionPID.GetOutput(vector3_1.z, deltaTime)
      }, ForceMode.Acceleration);
    Vector3 other1 = snapshot.Rotation * Vector3.forward;
    Vector3 other2 = snapshot.Rotation * Vector3.up;
    Vector3 vector3_2 = new Vector3();
    vector3_2.x = TargetCalc.GetAngleOnAxis(rb.transform.forward, other1, rb.transform.right);
    vector3_2.y = TargetCalc.GetAngleOnAxis(rb.transform.forward, other1, rb.transform.up);
    vector3_2.z = TargetCalc.GetAngleOnAxis(rb.transform.up, other2, rb.transform.forward);
    if ((double) vector3_2.sqrMagnitude > (double) this.rotationTeleportThreshold * (double) this.rotationTeleportThreshold)
    {
      rb.MoveRotation(snapshot.Rotation);
      rb.angularVelocity = Vector3.zero;
    }
    else
      rb.AddRelativeTorque(new Vector3()
      {
        x = this.xRotationPID.GetOutput(vector3_2.x, deltaTime),
        y = this.yRotationPID.GetOutput(vector3_2.y, deltaTime),
        z = this.zRotationPID.GetOutput(vector3_2.z, deltaTime)
      } * rb.mass);
  }
}

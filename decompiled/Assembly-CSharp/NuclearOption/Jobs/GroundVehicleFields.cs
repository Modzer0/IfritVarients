// Decompiled with JetBrains decompiler
// Type: NuclearOption.Jobs.GroundVehicleFields
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System.Runtime.CompilerServices;
using UnityEngine;

#nullable disable
namespace NuclearOption.Jobs;

public struct GroundVehicleFields
{
  public float maxRadius;
  public float acceleration;
  public float mass;
  public bool mobile;
  public float topSpeedOnroad;
  public float topSpeedOffroad;
  public float suspensionTravel;
  public float dampingRate;
  public float springRate;
  public float frictionCoef;
  public float inertiaTensor;
  public float speed;
  public float stationaryTime;
  public Vector3 velocity;
  public Vector3 angularVelocity;
  public bool monoBehaviourEnabled;
  public bool unitDisabled;
  public bool unitWasDisabled;
  public bool underwater;
  public SteeringInfo? steeringInfoNullable;
  public PtrList<ObstaclePosition> ObstaclesArray;
  public bool DEBUG_VIS;
  public GroundVehicle.VehicleInputs inputs;
  public SampleGroundResult sampleGroundResult;
  public float engineOutput;
  public float bulldozeTimer;
  public float reverseTimer;
  public float stuckTimer;
  public Plane surfacePlane;
  private bool addForce;
  private bool addTorque;
  private Vector3 force;
  private Vector3 torque;
  public bool stationary;
  public float radarAlt;

  public bool disabled => this.unitDisabled || this.underwater;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void ResetForceResults()
  {
    this.addForce = false;
    this.addTorque = false;
    this.force = new Vector3();
    this.torque = new Vector3();
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void AddForce(Vector3 force)
  {
    this.addForce = true;
    this.force += force;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void AddTorque(Vector3 torque)
  {
    this.addTorque = true;
    this.torque += torque;
  }

  public void ApplyForce(Rigidbody rigidbody)
  {
    if (this.addForce)
      rigidbody.AddForce(this.force);
    if (!this.addTorque)
      return;
    rigidbody.AddTorque(this.torque);
  }
}

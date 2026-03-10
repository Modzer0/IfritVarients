// Decompiled with JetBrains decompiler
// Type: NuclearOption.Jobs.AeroPartFields
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

#nullable disable
namespace NuclearOption.Jobs;

public struct AeroPartFields
{
  public PtrRefCounter<IndexLink> liftTransformIndex;
  public PtrRefCounter<IndexLink> otherTransformIndex;
  public Vector3 centerOfLift;
  public int airfoilID;
  public float wingEffectiveness;
  public float buoyancy;
  public float angularDrag;
  public Vector3 collisionSize;
  public float airflowChanneling;
  public float lastSplashTime;
  public Vector3 velocity;
  public float mass;
  public float submergedAmount;
  public float wingArea;
  public float dragArea;
  public Vector3 force;
  public Vector3 torque;
  public JobForceType hasForce;
  public bool angularDragChanged;
  public bool splashed;
}

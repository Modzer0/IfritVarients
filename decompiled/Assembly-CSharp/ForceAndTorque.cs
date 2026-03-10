// Decompiled with JetBrains decompiler
// Type: ForceAndTorque
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

#nullable disable
public struct ForceAndTorque
{
  public Vector3 force;
  public Vector3 torque;

  public ForceAndTorque(Vector3 force, Vector3 offset)
  {
    this.force = force;
    this.torque = Vector3.Cross(force, -offset);
  }

  public ForceAndTorque(Vector3 force, Vector3 torque, bool useTorque)
  {
    this.force = force;
    this.torque = torque;
  }

  public void Add(ForceAndTorque additive)
  {
    this.force += additive.force;
    this.torque += additive.torque;
  }

  public void AddTorque(Vector3 torque) => this.torque += torque;

  public void Clear()
  {
    this.force = Vector3.zero;
    this.torque = Vector3.zero;
  }
}

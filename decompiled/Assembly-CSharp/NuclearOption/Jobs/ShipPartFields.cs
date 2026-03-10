// Decompiled with JetBrains decompiler
// Type: NuclearOption.Jobs.ShipPartFields
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

#nullable disable
namespace NuclearOption.Jobs;

public struct ShipPartFields
{
  public Vector3 velocity;
  public Vector3 directionalDrag;
  public float partHeight;
  public float displacement;
  public float mass;
  public Vector3 forcePosition;
  public Vector3 force;
  public float submergedAmount;
  internal Vector3 centerOfMassWorld;
}

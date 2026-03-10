// Decompiled with JetBrains decompiler
// Type: SteeringInfo
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

#nullable disable
public readonly struct SteeringInfo(Vector3 steerVector, float nextWaypointAngle)
{
  public readonly Vector3 steerVector = steerVector;
  public readonly float nextWaypointAngle = nextWaypointAngle;
  public static readonly SteeringInfo? None;
}

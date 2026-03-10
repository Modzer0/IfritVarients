// Decompiled with JetBrains decompiler
// Type: GlobalPosition
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using System.Runtime.InteropServices;
using UnityEngine;

#nullable disable
[Serializable]
[StructLayout(LayoutKind.Explicit, Size = 12)]
public struct GlobalPosition : IEquatable<GlobalPosition>
{
  [FieldOffset(0)]
  public float x;
  [FieldOffset(4)]
  public float y;
  [FieldOffset(8)]
  public float z;

  public GlobalPosition(float x, float y, float z)
  {
    this.x = x;
    this.y = y;
    this.z = z;
  }

  public GlobalPosition(Vector3 v)
  {
    this.x = v.x;
    this.y = v.y;
    this.z = v.z;
  }

  public override readonly int GetHashCode() => this.AsVector3().GetHashCode();

  public readonly Vector3 AsVector3() => new Vector3(this.x, this.y, this.z);

  public static GlobalPosition operator +(GlobalPosition pos, Vector3 move)
  {
    return new GlobalPosition(pos.x + move.x, pos.y + move.y, pos.z + move.z);
  }

  public static GlobalPosition operator -(GlobalPosition pos, Vector3 move)
  {
    return new GlobalPosition(pos.x - move.x, pos.y - move.y, pos.z - move.z);
  }

  public static Vector3 operator -(GlobalPosition to, GlobalPosition from)
  {
    return FastMath.Direction(from, to);
  }

  public static bool operator ==(GlobalPosition a, GlobalPosition b) => a.Equals(b);

  public static bool operator !=(GlobalPosition a, GlobalPosition b) => a.NotEqual(b);

  public override readonly bool Equals(object obj)
  {
    return obj is GlobalPosition other && this.Equals(other);
  }

  public readonly bool Equals(GlobalPosition other)
  {
    return (double) this.x == (double) other.x && (double) this.y == (double) other.y && (double) this.z == (double) other.z;
  }

  public readonly bool NotEqual(GlobalPosition other) => !this.Equals(other);

  public override readonly string ToString() => $"Global({this.x:0.0},{this.y:0.0},{this.z:0.0})";
}

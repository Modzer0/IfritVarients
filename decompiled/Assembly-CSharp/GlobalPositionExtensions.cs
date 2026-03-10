// Decompiled with JetBrains decompiler
// Type: GlobalPositionExtensions
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Mirage.Serialization;
using System.Runtime.CompilerServices;
using UnityEngine;

#nullable disable
public static class GlobalPositionExtensions
{
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static global::GlobalPosition ToGlobalPosition(this Vector3 position)
  {
    Vector3 vector3 = position - Datum.originPosition;
    return new global::GlobalPosition(vector3.x, vector3.y, vector3.z);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float GlobalX(this Vector3 position) => position.x - Datum.originPosition.x;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float GlobalY(this Vector3 position) => position.y - Datum.originPosition.y;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float GlobalZ(this Vector3 position) => position.z - Datum.originPosition.z;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector3 ToLocalPosition(this global::GlobalPosition position)
  {
    return position.AsVector3() + Datum.originPosition;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float LocalX(this global::GlobalPosition position)
  {
    return position.x + Datum.originPosition.x;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float LocalY(this global::GlobalPosition position)
  {
    return position.y + Datum.originPosition.y;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float LocalZ(this global::GlobalPosition position)
  {
    return position.z + Datum.originPosition.z;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static global::GlobalPosition GlobalPosition(this Unit unit)
  {
    return unit.transform.position.ToGlobalPosition();
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static global::GlobalPosition GlobalPosition(this Transform transform)
  {
    return transform.position.ToGlobalPosition();
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void WriteGlobalPosition(this NetworkWriter writer, global::GlobalPosition value)
  {
    writer.WriteVector3(value.AsVector3());
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static global::GlobalPosition ReadGlobalPosition(this NetworkReader reader)
  {
    return new global::GlobalPosition(reader.ReadVector3());
  }
}

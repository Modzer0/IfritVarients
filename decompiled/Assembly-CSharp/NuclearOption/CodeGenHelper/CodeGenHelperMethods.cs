// Decompiled with JetBrains decompiler
// Type: NuclearOption.CodeGenHelper.CodeGenHelperMethods
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System.Runtime.CompilerServices;
using UnityEngine;

#nullable disable
namespace NuclearOption.CodeGenHelper;

public static class CodeGenHelperMethods
{
  private static void Throw<T>(string from, T value) where T : struct
  {
    throw new FloatException($"Invalid float from {from}. Value={value}");
  }

  private static void Throw<T1, T2>(string from, T1 value1, T2 value2)
    where T1 : struct
    where T2 : struct
  {
    throw new FloatException($"Invalid float from {from}. Value1={value1} Value2={value2}");
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static bool PosCheck(Vector3 pos)
  {
    return float.IsFinite(pos.x) && float.IsFinite(pos.y) && float.IsFinite(pos.z);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static bool RotCheck(Quaternion rot)
  {
    return float.IsFinite(rot.x) && float.IsFinite(rot.y) && float.IsFinite(rot.z) && float.IsFinite(rot.w);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void Check_Transform_set_position(Transform transform, Vector3 pos)
  {
    if (CodeGenHelperMethods.PosCheck(pos))
      transform.position = pos;
    else
      CodeGenHelperMethods.Throw<Vector3>("Transform.set_position", pos);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void Check_Transform_set_rotation(Transform transform, Quaternion rot)
  {
    if (CodeGenHelperMethods.RotCheck(rot))
      transform.rotation = rot;
    else
      CodeGenHelperMethods.Throw<Quaternion>("Transform.set_rotation", rot);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void Check_Transform_SetPositionAndRotation(
    Transform transform,
    Vector3 pos,
    Quaternion rot)
  {
    if (CodeGenHelperMethods.PosCheck(pos) && CodeGenHelperMethods.RotCheck(rot))
      transform.SetPositionAndRotation(pos, rot);
    else
      CodeGenHelperMethods.Throw<Vector3, Quaternion>("Transform.SetPositionAndRotation", pos, rot);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void Check_Transform_set_localPosition(Transform transform, Vector3 pos)
  {
    if (CodeGenHelperMethods.PosCheck(pos))
      transform.localPosition = pos;
    else
      CodeGenHelperMethods.Throw<Vector3>("Transform.set_localPosition", pos);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void Check_Transform_set_localRotation(Transform transform, Quaternion rot)
  {
    if (CodeGenHelperMethods.RotCheck(rot))
      transform.localRotation = rot;
    else
      CodeGenHelperMethods.Throw<Quaternion>("Transform.set_localRotation", rot);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void Check_Transform_SetLocalPositionAndRotation(
    Transform transform,
    Vector3 pos,
    Quaternion rot)
  {
    if (CodeGenHelperMethods.PosCheck(pos) && CodeGenHelperMethods.RotCheck(rot))
      transform.SetLocalPositionAndRotation(pos, rot);
    else
      CodeGenHelperMethods.Throw<Vector3, Quaternion>("Transform.SetLocalPositionAndRotation", pos, rot);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void Check_Rigidbody_set_position(Rigidbody rb, Vector3 pos)
  {
    if (CodeGenHelperMethods.PosCheck(pos))
      rb.position = pos;
    else
      CodeGenHelperMethods.Throw<Vector3>("Rigidbody.set_position", pos);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void Check_Rigidbody_MovePosition(Rigidbody rb, Vector3 pos)
  {
    if (CodeGenHelperMethods.PosCheck(pos))
      rb.MovePosition(pos);
    else
      CodeGenHelperMethods.Throw<Vector3>("Rigidbody.MovePosition", pos);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void Check_Rigidbody_set_velocity(Rigidbody rb, Vector3 vel)
  {
    if (CodeGenHelperMethods.PosCheck(vel))
      rb.velocity = vel;
    else
      CodeGenHelperMethods.Throw<Vector3>("Rigidbody.set_velocity", vel);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void Check_Rigidbody_set_angularVelocity(Rigidbody rb, Vector3 vel)
  {
    if (CodeGenHelperMethods.PosCheck(vel))
      rb.angularVelocity = vel;
    else
      CodeGenHelperMethods.Throw<Vector3>("Rigidbody.set_angularVelocity", vel);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void Check_Rigidbody_set_rotation(Rigidbody rb, Quaternion rot)
  {
    if (CodeGenHelperMethods.RotCheck(rot))
      rb.rotation = rot;
    else
      CodeGenHelperMethods.Throw<Quaternion>("Rigidbody.set_rotation", rot);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void Check_Rigidbody_MoveRotation(Rigidbody rb, Quaternion rot)
  {
    if (CodeGenHelperMethods.RotCheck(rot))
      rb.MoveRotation(rot);
    else
      CodeGenHelperMethods.Throw<Quaternion>("Rigidbody.MoveRotation", rot);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void Check_Rigidbody_Move(Rigidbody rb, Vector3 pos, Quaternion rot)
  {
    if (CodeGenHelperMethods.RotCheck(rot))
      rb.Move(pos, rot);
    else
      CodeGenHelperMethods.Throw<Quaternion>("Rigidbody.Move", rot);
  }
}

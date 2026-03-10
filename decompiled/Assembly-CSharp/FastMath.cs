// Decompiled with JetBrains decompiler
// Type: FastMath
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using System.Runtime.CompilerServices;
using UnityEngine;

#nullable disable
public static class FastMath
{
  public static double Lerp(double a, double b, float t)
  {
    t = Mathf.Clamp01(t);
    return a + (b - a) * (double) t;
  }

  public static double LerpUnclamped(double a, double b, float t) => a + (b - a) * (double) t;

  public static float InverseLerp(double a, double b, double v)
  {
    return Mathf.Clamp01((float) ((v - a) / (b - a)));
  }

  public static float InverseLerpUnclamped(double a, double b, double v)
  {
    return (float) ((v - a) / (b - a));
  }

  public static float Map(float value, float aIn, float bIn, float aOut, float bOut)
  {
    if ((double) bIn == (double) aIn)
      return bOut;
    float num = Mathf.Clamp01((float) (((double) value - (double) aIn) / ((double) bIn - (double) aIn)));
    return aOut + (bOut - aOut) * num;
  }

  public static float MapUnclamped(float value, float aIn, float bIn, float aOut, float bOut)
  {
    if ((double) bIn == (double) aIn)
      return bOut;
    float num = (float) (((double) value - (double) aIn) / ((double) bIn - (double) aIn));
    return aOut + (bOut - aOut) * num;
  }

  public static GlobalPosition Lerp(GlobalPosition a, GlobalPosition b, float t)
  {
    t = Mathf.Clamp01(t);
    return new GlobalPosition(a.x + (b.x - a.x) * t, a.y + (b.y - a.y) * t, a.z + (b.z - a.z) * t);
  }

  public static GlobalPosition LerpUnclamped(GlobalPosition a, GlobalPosition b, float t)
  {
    return new GlobalPosition(a.x + (b.x - a.x) * t, a.y + (b.y - a.y) * t, a.z + (b.z - a.z) * t);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector3 LerpXZ(Vector3 a, Vector3 b, float t)
  {
    t = Mathf.Clamp01(t);
    return new Vector3(a.x + (b.x - a.x) * t, 0.0f, a.z + (b.z - a.z) * t);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector3 NormalizedDirection(GlobalPosition from, GlobalPosition to)
  {
    return FastMath.Direction(from, to).normalized;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector3 Direction(GlobalPosition from, GlobalPosition to)
  {
    return new Vector3(to.x - from.x, to.y - from.y, to.z - from.z);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float Distance(GlobalPosition a, GlobalPosition b)
  {
    return (float) Math.Sqrt((double) FastMath.SquareDistance(a, b));
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float SquareDistance(GlobalPosition a, GlobalPosition b)
  {
    double num1 = (double) a.x - (double) b.x;
    float num2 = a.y - b.y;
    float num3 = a.z - b.z;
    return (float) (num1 * num1 + (double) num2 * (double) num2 + (double) num3 * (double) num3);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool OutOfRange(GlobalPosition a, GlobalPosition b, float range)
  {
    return !FastMath.InRange(a, b, range);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool InRange(GlobalPosition a, GlobalPosition b, float range)
  {
    double num1 = (double) a.x - (double) b.x;
    float num2 = a.z - b.z;
    float num3 = a.y - b.y;
    return num1 * num1 + (double) num3 * (double) num3 + (double) num2 * (double) num2 < (double) range * (double) range;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector3 NormalizedDirection(Vector3 from, Vector3 to)
  {
    return FastMath.Direction(from, to).normalized;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector3 Direction(Vector3 from, Vector3 to)
  {
    return new Vector3(to.x - from.x, to.y - from.y, to.z - from.z);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float Distance(Vector3 a, Vector3 b)
  {
    return (float) Math.Sqrt((double) FastMath.SquareDistance(a, b));
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float SquareDistance(Vector3 a, Vector3 b)
  {
    double num1 = (double) a.x - (double) b.x;
    float num2 = a.y - b.y;
    float num3 = a.z - b.z;
    return (float) (num1 * num1 + (double) num2 * (double) num2 + (double) num3 * (double) num3);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool OutOfRange(Vector3 a, Vector3 b, float range)
  {
    return !FastMath.InRange(a, b, range);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool InRange(Vector3 a, Vector3 b, float range)
  {
    double num1 = (double) a.x - (double) b.x;
    float num2 = a.z - b.z;
    float num3 = a.y - b.y;
    return num1 * num1 + (double) num3 * (double) num3 + (double) num2 * (double) num2 < (double) range * (double) range;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float SmoothDamp(
    float current,
    float target,
    ref float currentVelocity,
    float smoothTime,
    bool useUnscaledTime = false)
  {
    float deltaTime = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
    return (double) deltaTime == 0.0 ? current : Mathf.SmoothDamp(current, target, ref currentVelocity, smoothTime, float.PositiveInfinity, deltaTime);
  }

  public static Quaternion SmoothDampQuaternion(
    Quaternion current,
    Quaternion target,
    ref Vector3 currentVelocity,
    float smoothTime)
  {
    float deltaTime = Time.deltaTime;
    if ((double) deltaTime == 0.0)
      return current;
    Vector3 eulerAngles1 = current.eulerAngles;
    Vector3 eulerAngles2 = target.eulerAngles;
    return Quaternion.Euler(Mathf.SmoothDampAngle(eulerAngles1.x, eulerAngles2.x, ref currentVelocity.x, smoothTime, float.PositiveInfinity, deltaTime), Mathf.SmoothDampAngle(eulerAngles1.y, eulerAngles2.y, ref currentVelocity.y, smoothTime, float.PositiveInfinity, deltaTime), Mathf.SmoothDampAngle(eulerAngles1.z, eulerAngles2.z, ref currentVelocity.z, smoothTime, float.PositiveInfinity, deltaTime));
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Quaternion LookRotation(Vector3 directionOrZero)
  {
    return (double) directionOrZero.x == 0.0 && (double) directionOrZero.y == 0.0 && (double) directionOrZero.z == 0.0 ? Quaternion.identity : Quaternion.LookRotation(directionOrZero);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Quaternion LookRotation(Vector3 directionOrZero, Vector3 upDirection)
  {
    return (double) directionOrZero.x == 0.0 && (double) directionOrZero.y == 0.0 && (double) directionOrZero.z == 0.0 ? Quaternion.identity : Quaternion.LookRotation(directionOrZero, upDirection);
  }
}

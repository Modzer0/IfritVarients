// Decompiled with JetBrains decompiler
// Type: NetworkFloatHelper
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System.Runtime.CompilerServices;
using UnityEngine;

#nullable disable
public static class NetworkFloatHelper
{
  public static bool Validate(float value, bool logErrors, string variableName)
  {
    if (float.IsFinite(value))
      return true;
    if (logErrors)
      NetworkFloatHelper.LogNaN(value, variableName);
    return false;
  }

  public static bool Validate(double value, bool logErrors, string variableName)
  {
    if (double.IsFinite(value))
      return true;
    if (logErrors)
      NetworkFloatHelper.LogNaN(value, variableName);
    return false;
  }

  public static bool Validate(Vector3 value, bool logErrors, string variableName)
  {
    bool flag = true;
    if (!float.IsFinite(value.x))
    {
      flag = false;
      if (logErrors)
        NetworkFloatHelper.LogNaN(value.x, variableName + ".x");
    }
    if (!float.IsFinite(value.y))
    {
      flag = false;
      if (logErrors)
        NetworkFloatHelper.LogNaN(value.y, variableName + ".y");
    }
    if (!float.IsFinite(value.z))
    {
      flag = false;
      if (logErrors)
        NetworkFloatHelper.LogNaN(value.z, variableName + ".z");
    }
    return flag;
  }

  public static bool Validate(Quaternion value, bool logErrors, string variableName)
  {
    bool flag = true;
    if (!float.IsFinite(value.x))
    {
      flag = false;
      if (logErrors)
        NetworkFloatHelper.LogNaN(value.x, variableName + ".x");
    }
    if (!float.IsFinite(value.y))
    {
      flag = false;
      if (logErrors)
        NetworkFloatHelper.LogNaN(value.y, variableName + ".y");
    }
    if (!float.IsFinite(value.z))
    {
      flag = false;
      if (logErrors)
        NetworkFloatHelper.LogNaN(value.z, variableName + ".z");
    }
    if (!float.IsFinite(value.w))
    {
      flag = false;
      if (logErrors)
        NetworkFloatHelper.LogNaN(value.w, variableName + ".w");
    }
    return flag;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool Validate(GlobalPosition value, bool logErrors, string variableName)
  {
    return NetworkFloatHelper.Validate(value.AsVector3(), logErrors, variableName);
  }

  public static bool Validate(Vector3Compressed value, bool logErrors, string variableName)
  {
    float num1 = Mathf.HalfToFloat(value.x.Value);
    float num2 = Mathf.HalfToFloat(value.y.Value);
    float num3 = Mathf.HalfToFloat(value.z.Value);
    bool flag = true;
    if (!float.IsFinite(num1))
    {
      flag = false;
      if (logErrors)
        NetworkFloatHelper.LogNaN(num1, variableName + ".x");
    }
    if (!float.IsFinite(num2))
    {
      flag = false;
      if (logErrors)
        NetworkFloatHelper.LogNaN(num2, variableName + ".y");
    }
    if (!float.IsFinite(num3))
    {
      flag = false;
      if (logErrors)
        NetworkFloatHelper.LogNaN(num3, variableName + ".z");
    }
    return flag;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool Validate(CompressedFloat compressed, bool logErrors, string variableName)
  {
    return NetworkFloatHelper.Validate(Mathf.HalfToFloat(compressed.Value), logErrors, variableName);
  }

  public static void LogNaN(double value, string variableName)
  {
    if (double.IsNaN(value))
      Debug.LogError((object) (variableName + " is NaN"));
    if (!double.IsInfinity(value))
      return;
    Debug.LogError((object) (variableName + " is infinity"));
  }

  public static void LogNaN(float value, string variableName)
  {
    if (float.IsNaN(value))
      Debug.LogError((object) (variableName + " is NaN"));
    if (!float.IsInfinity(value))
      return;
    Debug.LogError((object) (variableName + " is infinity"));
  }

  public static void LogNaN(Vector3 value, string variableName)
  {
    if (float.IsNaN(value.x))
      Debug.LogError((object) (variableName + ".x is NaN"));
    if (float.IsInfinity(value.x))
      Debug.LogError((object) (variableName + ".x is infinity"));
    if (float.IsNaN(value.y))
      Debug.LogError((object) (variableName + ".y is NaN"));
    if (float.IsInfinity(value.y))
      Debug.LogError((object) (variableName + ".y is infinity"));
    if (float.IsNaN(value.z))
      Debug.LogError((object) (variableName + ".z is NaN"));
    if (!float.IsInfinity(value.z))
      return;
    Debug.LogError((object) (variableName + ".z is infinity"));
  }

  public static Vector3Compressed CompressIfValid(
    Vector3 value,
    bool logErrors,
    string variableName,
    Vector3 defaultValue = default (Vector3))
  {
    if (!NetworkFloatHelper.Validate(value, logErrors, variableName))
      value = defaultValue;
    return value.Compress();
  }

  public static Vector3 DecompressIfValid(
    Vector3Compressed value,
    bool logErrors,
    string variableName,
    Vector3 defaultValue = default (Vector3))
  {
    return !NetworkFloatHelper.Validate(value, logErrors, variableName) ? defaultValue : value.Decompress();
  }

  public static bool TryDecompress(
    Vector3Compressed value,
    out Vector3 outValue,
    bool logErrors,
    string variableName)
  {
    outValue = value.Decompress();
    return NetworkFloatHelper.Validate(outValue, logErrors, variableName);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector3Compressed Compress(this Vector3 value)
  {
    Vector3Compressed vector3Compressed;
    vector3Compressed.x = value.x.Compress();
    vector3Compressed.y = value.y.Compress();
    vector3Compressed.z = value.z.Compress();
    return vector3Compressed;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector3 Decompress(this Vector3Compressed value)
  {
    Vector3 vector3;
    vector3.x = value.x.Decompress();
    vector3.y = value.y.Decompress();
    vector3.z = value.z.Decompress();
    return vector3;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static CompressedFloat Compress(this float value)
  {
    ushort half = Mathf.FloatToHalf(Mathf.Clamp(value, -65504f, 65504f));
    return new CompressedFloat() { Value = half };
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float Decompress(this CompressedFloat value)
  {
    float f = Mathf.HalfToFloat(value.Value);
    if (!float.IsFinite(f))
    {
      if (float.IsPositiveInfinity(f))
        return 65504f;
      if (float.IsNegativeInfinity(f))
        return -65504f;
    }
    return f;
  }
}

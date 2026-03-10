// Decompiled with JetBrains decompiler
// Type: NuclearOption.Jobs.UnsafeJobExtensions
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

#nullable disable
namespace NuclearOption.Jobs;

public static class UnsafeJobExtensions
{
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe ref JobTransformValues.ReadOnly GetReadOnlyRef(
    this NativeArray<JobTransformValues> array,
    int index)
  {
    UnsafeJobExtensions.LengthCheck(index, array.Length);
    // ISSUE: explicit reference operation
    return @((JobTransformValues.ReadOnly*) array.GetUnsafeReadOnlyPtr<JobTransformValues>())[index];
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void LengthCheck(int index, int length)
  {
    if (index >= 0 && index < length)
      return;
    UnsafeJobExtensions.FailOutOfRangeError(index, length);
  }

  public static void FailOutOfRangeError(int index, int length)
  {
    throw new IndexOutOfRangeException($"Index {index} is out of range of '{length}' Length.");
  }
}

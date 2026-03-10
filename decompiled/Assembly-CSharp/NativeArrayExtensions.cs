// Decompiled with JetBrains decompiler
// Type: NativeArrayExtensions
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Cysharp.Threading.Tasks;
using NuclearOption.Jobs;
using System;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Jobs;

#nullable disable
public static class NativeArrayExtensions
{
  public static void SafeClear(this Array array)
  {
    if (array == null)
      return;
    Array.Clear(array, 0, array.Length);
  }

  public static void DelayDispose(ref this TransformAccessArray array, bool delayed = true)
  {
    if (!array.isCreated)
      return;
    if (delayed)
    {
      TransformAccessArray copy = array;
      UniTask.Delay(1000).ContinueWith((Action) (() => copy.Dispose())).Forget();
    }
    else
      array.Dispose();
    array = new TransformAccessArray();
  }

  public static void DelayDispose<T>(ref this NativeArray<T> array, bool delayed = true) where T : unmanaged
  {
    if (!array.IsCreated)
      return;
    if (delayed)
    {
      NativeArray<T> copy = array;
      UniTask.Delay(1000).ContinueWith((Action) (() => copy.SafeDispose<T>())).Forget();
    }
    else
      array.Dispose();
    array = new NativeArray<T>();
  }

  public static void SafeDispose<T>(ref this NativeArray<T> array) where T : struct
  {
    try
    {
      array.Dispose();
      array = new NativeArray<T>();
    }
    catch (ObjectDisposedException ex)
    {
      Debug.Log((object) "NativeArray already disposed");
    }
  }

  public static void ClearAndDispose<T>(ref this NativeArray<PtrRefCounter<T>> array, int length) where T : unmanaged
  {
    if (array.IsCreated)
    {
      for (int index = 0; index < length; ++index)
      {
        PtrRefCounter<T> ptrRefCounter = array[index];
        if (ptrRefCounter.IsCreated)
        {
          ptrRefCounter = array[index];
          ptrRefCounter.RemoveRef();
        }
      }
    }
    array.Dispose();
  }

  public static void Resize<T>(
    ref this NativeArray<T> array,
    int newLength,
    bool copyItems,
    NativeArrayOptions options = NativeArrayOptions.UninitializedMemory)
    where T : unmanaged
  {
    NativeArray<T> dst = new NativeArray<T>(newLength, Allocator.Persistent, options);
    NativeArray<T> src = array;
    if (src.IsCreated)
    {
      if (copyItems)
        NativeArray<T>.Copy(src, 0, dst, 0, src.Length);
      src.Dispose();
    }
    array = dst;
  }

  public static void Resize(ref this TransformAccessArray array, int newLength)
  {
    if (array.isCreated)
      array.capacity = newLength * 2;
    else
      array = new TransformAccessArray(newLength * 2);
  }
}

// Decompiled with JetBrains decompiler
// Type: NuclearOption.Jobs.Ptr`1
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using System.Runtime.CompilerServices;
using UnityEngine;

#nullable disable
namespace NuclearOption.Jobs;

public struct Ptr<T>(T* ptr) where T : unmanaged
{
  public static bool JobRunningSafety;
  public unsafe T* ptr = ptr;

  public readonly unsafe bool IsCreated => (IntPtr) this.ptr != IntPtr.Zero;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public readonly unsafe ref T Ref()
  {
    if ((IntPtr) this.ptr == IntPtr.Zero)
      PtrNullException<Ptr<T>>.Throw();
    return ref (*this.ptr);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public readonly ref T RefCheckSafety()
  {
    if (Ptr<T>.JobRunningSafety)
      Debug.LogError((object) $"Getting Ptr<{typeof (T)}> but JobRunningSafety is true");
    return ref this.Ref();
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public readonly unsafe T Value()
  {
    if ((IntPtr) this.ptr == IntPtr.Zero)
      PtrNullException<Ptr<T>>.Throw();
    return *this.ptr;
  }

  public static unsafe implicit operator Ptr<T>(T* ptr) => new Ptr<T>(ptr);

  public static unsafe implicit operator T*(Ptr<T> ptr) => ptr.ptr;

  public static unsafe bool PtrEqual(Ptr<T> a, Ptr<T> b) => a.ptr == b.ptr;

  public override string ToString()
  {
    return !this.IsCreated ? "Ptr(NULL)" : $"Ptr({this.Ref().ToString()})";
  }
}

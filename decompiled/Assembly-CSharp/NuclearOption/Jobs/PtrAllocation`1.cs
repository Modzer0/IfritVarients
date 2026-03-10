// Decompiled with JetBrains decompiler
// Type: NuclearOption.Jobs.PtrAllocation`1
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using System.Runtime.CompilerServices;
using UnityEngine;

#nullable disable
namespace NuclearOption.Jobs;

public struct PtrAllocation<T>(T* ptr) : IDisposable where T : unmanaged
{
  public readonly unsafe T* ptr = ptr;

  public readonly unsafe bool IsCreated
  {
    [MethodImpl(MethodImplOptions.AggressiveInlining)] get => (IntPtr) this.ptr != IntPtr.Zero;
  }

  public void Dispose()
  {
    if (!this.IsCreated)
      return;
    JobsAllocator<T>.Free(ref this);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public readonly unsafe ref T Ref()
  {
    if ((IntPtr) this.ptr == IntPtr.Zero)
      PtrNullException<PtrAllocation<T>>.Throw();
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

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public readonly unsafe Ptr<T> AsPtr() => new Ptr<T>(this.ptr);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static implicit operator Ptr<T>(PtrAllocation<T> ptr) => ptr.AsPtr();

  public override string ToString()
  {
    return !this.IsCreated ? "PtrAllocation(NULL)" : $"PtrAllocation({this.Ref().ToString()})";
  }
}

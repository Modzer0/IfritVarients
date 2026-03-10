// Decompiled with JetBrains decompiler
// Type: NuclearOption.Jobs.PtrRefCounter`1
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using System.Runtime.CompilerServices;
using UnityEngine;

#nullable disable
namespace NuclearOption.Jobs;

public struct PtrRefCounter<T>(PtrAllocation<T> field, PtrAllocation<int> refCounter) : IDisposable where T : unmanaged
{
  public readonly PtrAllocation<T> field = field;
  public readonly PtrAllocation<int> refCounter = refCounter;

  public readonly void AddRef() => ++this.refCounter.Ref();

  public unsafe void RemoveRef()
  {
    if (!this.refCounter.IsCreated)
    {
      Debug.LogWarning((object) "RemoveRef called when PtrRefCounter was not created");
    }
    else
    {
      if (!MainMenu.ApplicationIsQuitting)
      {
        int num;
        try
        {
          ref int local = ref this.refCounter.Ref();
          --local;
          num = local;
        }
        catch (Exception ex)
        {
          Debug.LogWarning((object) $"Exception Removing Ref field:{(ValueType) (ulong) this.refCounter.ptr:X} {ex}");
          return;
        }
        if (num < 0)
          Debug.LogWarning((object) $"RemoveRef set count to {num} field:{(ValueType) (ulong) this.refCounter.ptr:X}");
        if (num <= 0)
          this.Dispose();
      }
      *(PtrRefCounter<T>*) ref this = new PtrRefCounter<T>();
    }
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public readonly int GetRefCount() => this.refCounter.Value();

  public readonly unsafe bool IsCreated
  {
    [MethodImpl(MethodImplOptions.AggressiveInlining)] get
    {
      return (IntPtr) this.field.ptr != IntPtr.Zero;
    }
  }

  public void Dispose()
  {
    if (!this.IsCreated)
      return;
    JobsAllocator<T>.Free(ref this);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public readonly ref T Ref() => ref this.field.Ref();

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public readonly ref T RefCheckSafety() => ref this.field.RefCheckSafety();

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public readonly T Value() => this.field.Value();

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public readonly unsafe Ptr<T> AsPtr() => new Ptr<T>(this.field.ptr);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static implicit operator Ptr<T>(PtrRefCounter<T> ptr) => ptr.AsPtr();

  public override string ToString()
  {
    return !this.IsCreated ? "PtrRefCounter(NULL)" : $"PtrRefCounter({this.Ref().ToString()})";
  }
}

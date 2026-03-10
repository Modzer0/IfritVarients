// Decompiled with JetBrains decompiler
// Type: NuclearOption.Jobs.PtrList`1
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#nullable disable
namespace NuclearOption.Jobs;

public struct PtrList<T> : IDisposable where T : unmanaged
{
  private unsafe T* ptr;
  public int Length;
  public int Capacity;

  public unsafe void EnsureCapacity(int needLength)
  {
    int capacity = this.Capacity;
    if (capacity >= needLength)
      return;
    int newCapacity = JobManager.IncreaseCapacity(capacity, needLength, 4);
    int cb = sizeof (T) * newCapacity;
    IntPtr num;
    if ((IntPtr) this.ptr == IntPtr.Zero)
    {
      JobsAllocatorShared.RecordAlloc<T>(this, newCapacity);
      num = Marshal.AllocHGlobal(cb);
    }
    else
    {
      JobsAllocatorShared.RecordReAlloc<T>(this, newCapacity);
      num = Marshal.ReAllocHGlobal(new IntPtr((void*) this.ptr), (IntPtr) cb);
    }
    this.ptr = (T*) num.ToPointer();
    this.Capacity = newCapacity;
  }

  public void Dispose() => PtrList<T>.Dispose(ref this);

  public static unsafe void Dispose(ref PtrList<T> list)
  {
    if ((IntPtr) list.ptr != IntPtr.Zero)
    {
      JobsAllocatorShared.RecordFree<T>(list);
      Marshal.FreeHGlobal(new IntPtr((void*) list.ptr));
    }
    list = new PtrList<T>();
  }

  public unsafe ref T this[int index]
  {
    [MethodImpl(MethodImplOptions.AggressiveInlining)] get
    {
      if ((IntPtr) this.ptr == IntPtr.Zero)
        PtrNullException<PtrList<T>>.Throw();
      UnsafeJobExtensions.LengthCheck(index, this.Length);
      return @this.ptr[index];
    }
  }

  public void Add(T item)
  {
    this.EnsureCapacity(this.Length + 1);
    this[this.Length] = item;
    ++this.Length;
  }

  public unsafe Ptr<T> GetPtr(int index)
  {
    if ((IntPtr) this.ptr == IntPtr.Zero)
      PtrNullException<PtrList<T>>.Throw();
    UnsafeJobExtensions.LengthCheck(index, this.Length);
    return new Ptr<T>(this.ptr + index);
  }
}

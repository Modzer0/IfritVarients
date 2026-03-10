// Decompiled with JetBrains decompiler
// Type: NuclearOption.Jobs.PtrArray`1
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#nullable disable
namespace NuclearOption.Jobs;

public struct PtrArray<T> : IDisposable where T : unmanaged
{
  private readonly unsafe T* ptr;
  public readonly int Length;

  public PtrArray(int length)
  {
    // ISSUE: unable to decompile the method.
  }

  public void Dispose() => PtrArray<T>.Dispose(ref this);

  public static unsafe void Dispose(ref PtrArray<T> array)
  {
    if ((IntPtr) array.ptr != IntPtr.Zero)
    {
      JobsAllocatorShared.RecordFree<T>(array);
      Marshal.FreeHGlobal(new IntPtr((void*) array.ptr));
    }
    array = new PtrArray<T>();
  }

  public unsafe ref T this[int index]
  {
    [MethodImpl(MethodImplOptions.AggressiveInlining)] get
    {
      if ((IntPtr) this.ptr == IntPtr.Zero)
        PtrNullException<PtrArray<T>>.Throw();
      UnsafeJobExtensions.LengthCheck(index, this.Length);
      return @this.ptr[index];
    }
  }

  public unsafe Ptr<T> GetPtr(int index)
  {
    if ((IntPtr) this.ptr == IntPtr.Zero)
      PtrNullException<PtrArray<T>>.Throw();
    UnsafeJobExtensions.LengthCheck(index, this.Length);
    return new Ptr<T>(this.ptr + index);
  }
}

// Decompiled with JetBrains decompiler
// Type: NuclearOption.Jobs.JobsAllocator`1
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using System.Runtime.InteropServices;
using UnityEngine;

#nullable disable
namespace NuclearOption.Jobs;

public class JobsAllocator<T> : JobsAllocator where T : unmanaged
{
  private const int DEFAULT_CHUNK_SIZE = 256 /*0x0100*/;
  private static readonly JobsAllocator<T> i = new JobsAllocator<T>();

  private JobsAllocator() => JobsAllocatorShared.Allocators.Add((JobsAllocator) this);

  private T* Allocate(int newChunkSize)
  {
    // ISSUE: unable to decompile the method.
  }

  private void AllocateNewChunk(int newChunkSize)
  {
    int cb = sizeof (T) * newChunkSize;
    this.RecordChunk(newChunkSize);
    IntPtr num = Marshal.AllocHGlobal(cb);
    this.chunks.Add(num);
    for (int index = 0; index < newChunkSize; ++index)
      this.freeItems.Enqueue(num + index * sizeof (T));
  }

  private void RecordChunk(int items)
  {
    this.TotalBytes += (long) (sizeof (T) * items);
    this.TotalItems += items;
    ColorLog<T>.Info($"Allocating new Chunk for {typeof (T).Name}, total KB:{this.TotalBytes / 1024L /*0x0400*/}");
  }

  public static unsafe void Allocate(ref PtrAllocation<T> ptr, int newChunkSize = 256 /*0x0100*/)
  {
    if ((IntPtr) ptr.ptr != IntPtr.Zero)
      Debug.LogError((object) "JobsAllocHelper.Allocate was given a pointer that already had a value");
    else
      ptr = new PtrAllocation<T>(JobsAllocator<T>.i.Allocate(newChunkSize));
  }

  public static unsafe void AllocateRefCounter(ref PtrRefCounter<T> ptrRefCounter, int newChunkSize = 256 /*0x0100*/)
  {
    if ((IntPtr) ptrRefCounter.field.ptr != IntPtr.Zero)
    {
      Debug.LogError((object) "JobsAllocHelper.Allocate was given a pointer that already had a value");
    }
    else
    {
      PtrAllocation<T> field = new PtrAllocation<T>(JobsAllocator<T>.i.Allocate(newChunkSize));
      PtrAllocation<int> refCounter = new PtrAllocation<int>(JobsAllocator<int>.i.Allocate(newChunkSize));
      refCounter.Ref() = 1;
      ptrRefCounter = new PtrRefCounter<T>(field, refCounter);
    }
  }

  public static void Free(ref PtrAllocation<T> ptr)
  {
    // ISSUE: unable to decompile the method.
  }

  public static void Free(ref PtrRefCounter<T> ptrRefCounter)
  {
    PtrAllocation<T> field = ptrRefCounter.field;
    PtrAllocation<int> refCounter = ptrRefCounter.refCounter;
    JobsAllocator<T>.Free(ref field);
    JobsAllocator<int>.Free(ref refCounter);
    ptrRefCounter = new PtrRefCounter<T>();
  }
}

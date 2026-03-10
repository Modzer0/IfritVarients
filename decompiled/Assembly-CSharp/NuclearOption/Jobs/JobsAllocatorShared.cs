// Decompiled with JetBrains decompiler
// Type: NuclearOption.Jobs.JobsAllocatorShared
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;
using UnityEngine;

#nullable disable
namespace NuclearOption.Jobs;

public static class JobsAllocatorShared
{
  public static List<JobsAllocator> Allocators = new List<JobsAllocator>();
  private static int arrayCount;
  private static long arrayBytes;
  private static int listCount;
  private static long listBytes;

  public static long TotalBytes()
  {
    long num = 0;
    foreach (JobsAllocator allocator in JobsAllocatorShared.Allocators)
      num += allocator.TotalBytes;
    return num;
  }

  public static int TotalItems()
  {
    int num = 0;
    foreach (JobsAllocator allocator in JobsAllocatorShared.Allocators)
      num += allocator.TotalItems;
    return num;
  }

  public static int AllocatedItems()
  {
    int num = 0;
    foreach (JobsAllocator allocator in JobsAllocatorShared.Allocators)
      num += allocator.AllocatedItems;
    return num;
  }

  public static int ItemsInPool()
  {
    int num1 = 0;
    foreach (JobsAllocator allocator in JobsAllocatorShared.Allocators)
    {
      int num2 = allocator.TotalItems - allocator.AllocatedItems;
      num1 += num2;
    }
    return num1;
  }

  public static void SortAll()
  {
    if (GameManager.gameState != GameState.Encyclopedia)
    {
      if (JobsAllocatorShared.arrayCount > 0)
        Debug.LogError((object) $"Leak warning: PtrArray still allocated when sorting: count={JobsAllocatorShared.arrayCount} byte={JobsAllocatorShared.arrayBytes}");
      if (JobsAllocatorShared.listCount > 0)
        Debug.LogError((object) $"Leak warning: PtrList still allocated when sorting: count={JobsAllocatorShared.listCount} byte={JobsAllocatorShared.listBytes}");
      foreach (JobsAllocator allocator in JobsAllocatorShared.Allocators)
      {
        if (allocator.AllocatedItems > 0)
          Debug.LogError((object) $"Leak warning: {allocator.GetType()} still allocated when sorting: {allocator.AllocatedItems} / {allocator.TotalItems}");
      }
    }
    Debug.Log((object) "Sorting all Allocations");
    foreach (JobsAllocator allocator in JobsAllocatorShared.Allocators)
      JobsAllocatorShared.SortQueue(allocator.freeItems);
  }

  private static void DebugAssertSorted(Queue<IntPtr> queue)
  {
    IntPtr num1 = new IntPtr();
    foreach (IntPtr message in queue)
    {
      Debug.Log((object) message);
      int num2 = num1 != IntPtr.Zero ? 1 : 0;
      num1 = message;
    }
  }

  public static void SortQueue(Queue<IntPtr> queue)
  {
    int count = queue.Count;
    IntPtr[] array = new IntPtr[count];
    for (int index = 0; index < count; ++index)
      array[index] = queue.Dequeue();
    Array.Sort<IntPtr>(array, (IComparer<IntPtr>) JobsAllocatorShared.IntPtrCompare.Instance);
    for (int index = 0; index < count; ++index)
      queue.Enqueue(array[index]);
  }

  private static int Size<T>() where T : unmanaged => sizeof (T);

  public static void RecordAlloc<T>(PtrArray<T> array) where T : unmanaged
  {
    ++JobsAllocatorShared.arrayCount;
    JobsAllocatorShared.arrayBytes += (long) (JobsAllocatorShared.Size<T>() * array.Length);
  }

  public static void RecordFree<T>(PtrArray<T> array) where T : unmanaged
  {
    --JobsAllocatorShared.arrayCount;
    JobsAllocatorShared.arrayBytes -= (long) (JobsAllocatorShared.Size<T>() * array.Length);
  }

  public static void RecordAlloc<T>(PtrList<T> _, int newCapacity) where T : unmanaged
  {
    ++JobsAllocatorShared.listCount;
    JobsAllocatorShared.listBytes += (long) (JobsAllocatorShared.Size<T>() * newCapacity);
  }

  public static void RecordReAlloc<T>(PtrList<T> list, int newCapacity) where T : unmanaged
  {
    JobsAllocatorShared.listBytes += (long) (JobsAllocatorShared.Size<T>() * (newCapacity - list.Capacity));
  }

  public static void RecordFree<T>(PtrList<T> list) where T : unmanaged
  {
    --JobsAllocatorShared.listCount;
    JobsAllocatorShared.listBytes -= (long) (JobsAllocatorShared.Size<T>() * list.Capacity);
  }

  private class IntPtrCompare : IComparer<IntPtr>
  {
    public static JobsAllocatorShared.IntPtrCompare Instance = new JobsAllocatorShared.IntPtrCompare();

    public int Compare(IntPtr x, IntPtr y) => x.ToInt64().CompareTo(y.ToInt64());
  }
}

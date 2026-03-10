// Decompiled with JetBrains decompiler
// Type: NuclearOption.Jobs.JobsAllocator
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

#nullable disable
namespace NuclearOption.Jobs;

public class JobsAllocator : IDisposable
{
  public long TotalBytes;
  public int TotalItems;
  public int AllocatedItems;
  public readonly List<IntPtr> chunks = new List<IntPtr>();
  public readonly Queue<IntPtr> freeItems = new Queue<IntPtr>();

  ~JobsAllocator() => this.Dispose();

  public void Dispose()
  {
    List<IntPtr> chunks = this.chunks;
    // ISSUE: explicit non-virtual call
    if ((chunks != null ? (__nonvirtual (chunks.Count) > 0 ? 1 : 0) : 0) == 0)
      return;
    foreach (IntPtr chunk in this.chunks)
    {
      if (chunk != IntPtr.Zero)
        Marshal.FreeHGlobal(chunk);
    }
    this.chunks.Clear();
  }
}

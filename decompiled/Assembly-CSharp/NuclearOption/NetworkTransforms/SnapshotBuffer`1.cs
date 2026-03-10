// Decompiled with JetBrains decompiler
// Type: NuclearOption.NetworkTransforms.SnapshotBuffer`1
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System.Collections.Generic;
using UnityEngine;

#nullable disable
namespace NuclearOption.NetworkTransforms;

public class SnapshotBuffer<T> : ISnapshotBuffer where T : struct
{
  protected readonly List<SnapshotBuffer<T>.TimedSnapshot> buffer = new List<SnapshotBuffer<T>.TimedSnapshot>(4);

  public int Count => this.buffer.Count;

  public SnapshotBuffer<T>.TimedSnapshot Get(int i) => this.buffer[i];

  public SnapshotBuffer<T>.TimedSnapshot? GetNullable(int i)
  {
    return 0 > i || i >= this.buffer.Count ? new SnapshotBuffer<T>.TimedSnapshot?() : new SnapshotBuffer<T>.TimedSnapshot?(this.buffer[i]);
  }

  public void GetLastValues(
    out SnapshotBuffer<T>.TimedSnapshot previous,
    out SnapshotBuffer<T>.TimedSnapshot current)
  {
    ref SnapshotBuffer<T>.TimedSnapshot local1 = ref previous;
    List<SnapshotBuffer<T>.TimedSnapshot> buffer1 = this.buffer;
    SnapshotBuffer<T>.TimedSnapshot timedSnapshot1 = buffer1[buffer1.Count - 2];
    local1 = timedSnapshot1;
    ref SnapshotBuffer<T>.TimedSnapshot local2 = ref current;
    List<SnapshotBuffer<T>.TimedSnapshot> buffer2 = this.buffer;
    SnapshotBuffer<T>.TimedSnapshot timedSnapshot2 = buffer2[buffer2.Count - 1];
    local2 = timedSnapshot2;
  }

  public void Insert(double timestamp, T snapshot, bool ignoreWarning = false)
  {
    if (this.buffer.Count >= 1)
    {
      double num = timestamp;
      List<SnapshotBuffer<T>.TimedSnapshot> buffer = this.buffer;
      double timestamp1 = buffer[buffer.Count - 1].Timestamp;
      if (num <= timestamp1)
      {
        if (ignoreWarning)
          return;
        Debug.LogWarning((object) "older snapshot inserted into buffer");
        return;
      }
    }
    this.buffer.Add(new SnapshotBuffer<T>.TimedSnapshot(timestamp, snapshot));
  }

  public void RemoveOld(double timestamp)
  {
    int num1 = 0;
    int num2 = this.buffer.Count - 4;
    while (num1 < num2 && this.buffer[num1].Timestamp < timestamp)
      ++num1;
    if (num1 <= 0)
      return;
    this.buffer.RemoveRange(0, num1);
  }

  public readonly struct TimedSnapshot(double timestamp, T snapshot)
  {
    public readonly double Timestamp = timestamp;
    public readonly T Snapshot = snapshot;
  }
}

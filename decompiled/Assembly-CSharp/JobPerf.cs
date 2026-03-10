// Decompiled with JetBrains decompiler
// Type: JobPerf
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using Unity.Profiling;

#nullable disable
public class JobPerf : IDisposable
{
  private static readonly ProfilerMarker flushMarker = new ProfilerMarker("JobPerf.Flush");
  private StreamWriter writer;
  private readonly List<JobPerf.Entry> entries = new List<JobPerf.Entry>();
  public bool Enabled;

  [Conditional("ENABLE_JOB_PERF")]
  public void Flush()
  {
    using (JobPerf.flushMarker.Auto())
    {
      if (this.Enabled)
      {
        foreach (JobPerf.Entry entry in this.entries)
          this.writer.WriteLine($"{entry.Label},{entry.Time:0.000},");
      }
      this.entries.Clear();
    }
  }

  public void Open(string fileName = "JobPerf.csv") => UnityEngine.Debug.Log((object) "Skipping JobPerf");

  public void Dispose()
  {
    this.Enabled = false;
    this.writer?.Close();
    this.writer = (StreamWriter) null;
  }

  [Conditional("ENABLE_JOB_PERF")]
  public void WriteStart(string label, long start)
  {
    if (!this.Enabled)
      return;
    this.entries.Add(new JobPerf.Entry()
    {
      Label = label,
      Time = BenchmarkScope.MillisecondsSince(start)
    });
  }

  [Conditional("ENABLE_JOB_PERF")]
  public void WriteTicks(string label, long ticks)
  {
    if (!this.Enabled)
      return;
    this.entries.Add(new JobPerf.Entry()
    {
      Label = label,
      Time = BenchmarkScope.TicksToMilliseconds(ticks)
    });
  }

  [Conditional("ENABLE_JOB_PERF")]
  public void WriteTicksBurst(string label, long ticks)
  {
    if (!this.Enabled)
      return;
    this.entries.Add(new JobPerf.Entry()
    {
      Label = label,
      Time = BenchmarkBurst.TicksToMilliseconds(ticks)
    });
  }

  [Conditional("ENABLE_JOB_PERF")]
  public void WriteOther(string label, double value)
  {
    if (!this.Enabled)
      return;
    this.entries.Add(new JobPerf.Entry()
    {
      Label = label,
      Time = value
    });
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static long GetTimestamp() => 0;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static long GetTimestampBurst() => 0;

  [Conditional("ENABLE_JOB_PERF")]
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  internal static void AddBurst(ref long timer, long start)
  {
  }

  private struct Entry
  {
    public string Label;
    public double Time;
  }
}

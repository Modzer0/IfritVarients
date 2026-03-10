// Decompiled with JetBrains decompiler
// Type: BenchmarkScope
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

#nullable disable
public struct BenchmarkScope : IDisposable
{
  private static readonly double msPerTick = 1000.0 / (double) Stopwatch.Frequency;
  public static bool ShowInRelease;
  private readonly string label;
  private readonly long startTick;

  public BenchmarkScope(string label, long startTick)
    : this()
  {
    this.label = label;
    this.startTick = startTick;
  }

  public void Dispose()
  {
    double num = BenchmarkScope.MillisecondsSince(this.startTick);
    if (!BenchmarkScope.ShowInRelease)
      return;
    ColorLog<BenchmarkScope>.Info($"{this.label} {num:0.000}ms");
  }

  public static BenchmarkScope Create(string label)
  {
    return new BenchmarkScope(label, Stopwatch.GetTimestamp());
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static double MillisecondsSince(long startTick)
  {
    return (double) (Stopwatch.GetTimestamp() - startTick) * BenchmarkScope.msPerTick;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static double MillisecondsBetween(long startTick, long endTick)
  {
    return (double) (endTick - startTick) * BenchmarkScope.msPerTick;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static long GetTimestamp() => Stopwatch.GetTimestamp();

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static long TicksSince(long startTick) => Stopwatch.GetTimestamp() - startTick;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static double TicksToMilliseconds(long ticks) => (double) ticks * BenchmarkScope.msPerTick;
}

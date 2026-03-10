// Decompiled with JetBrains decompiler
// Type: BenchmarkBurst
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System.Runtime.CompilerServices;
using Unity.Profiling.LowLevel.Unsafe;

#nullable disable
public static class BenchmarkBurst
{
  private static readonly double msPerTick;

  static BenchmarkBurst()
  {
    ProfilerUnsafeUtility.TimestampConversionRatio nanosecondsConversionRatio = ProfilerUnsafeUtility.TimestampToNanosecondsConversionRatio;
    BenchmarkBurst.msPerTick = (double) nanosecondsConversionRatio.Numerator / (double) nanosecondsConversionRatio.Denominator / 1000000.0;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static long GetTimestamp() => ProfilerUnsafeUtility.Timestamp;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static double MillisecondsSince(long startTicks)
  {
    return (double) (ProfilerUnsafeUtility.Timestamp - startTicks) * BenchmarkBurst.msPerTick;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static long TicksSince(long startTicks) => ProfilerUnsafeUtility.Timestamp - startTicks;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static double TicksToMilliseconds(long ticks) => (double) ticks * BenchmarkBurst.msPerTick;
}

// Decompiled with JetBrains decompiler
// Type: NuclearOption.NetworkTransforms.SmoothNetworkTime
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Mirage;
using System;
using UnityEngine;

#nullable disable
namespace NuclearOption.NetworkTransforms;

public class SmoothNetworkTime
{
  private readonly SmoothNetworkTime.Timer extrapolation = new SmoothNetworkTime.Timer();
  private readonly SmoothNetworkTime.Timer interpolation = new SmoothNetworkTime.Timer();

  public double ExtrapolationOffsetUnclamped => this.extrapolation.Time - this.interpolation.Time;

  public double InterpolationTime => this.interpolation.Time;

  public double GetExtrapolationOffset(float max = 3.40282347E+38f)
  {
    return Math.Clamp(this.extrapolation.Time - this.interpolation.Time, 0.0, (double) max);
  }

  public SmoothNetworkTime()
  {
    this.extrapolation.Time = 0.0;
    this.interpolation.Time = 0.0;
  }

  public void Update(float deltaTime)
  {
    this.interpolation.Time += (double) deltaTime * (double) this.interpolation.Timescale;
    this.extrapolation.Time += (double) deltaTime * (double) this.extrapolation.Timescale;
  }

  public void ResetValues(double targetTime, double extrapolationOffset)
  {
    this.interpolation.Time = targetTime;
    this.interpolation.Timescale = 1f;
    this.interpolation.AvgDiff.Reset();
    this.extrapolation.Time = targetTime + extrapolationOffset;
    this.extrapolation.Timescale = 1f;
    this.extrapolation.AvgDiff.Reset();
  }

  public void OnMessage(
    double targetTime,
    double extrapolationOffset,
    float snapThreshold,
    out bool snap)
  {
    SmoothNetworkTime.AdjustTimeScale(targetTime, ref this.interpolation.Time, this.interpolation.AvgDiff, snapThreshold, out this.interpolation.Timescale, out snap);
    double target = this.interpolation.Time + extrapolationOffset;
    if (snap)
    {
      this.extrapolation.Timescale = 1f;
      this.extrapolation.Time = target;
      this.extrapolation.AvgDiff.Reset();
    }
    else
      SmoothNetworkTime.AdjustTimeScale(target, ref this.extrapolation.Time, this.extrapolation.AvgDiff, snapThreshold, out this.extrapolation.Timescale, out bool _);
  }

  public static void AdjustTimeScale(
    double target,
    ref double current,
    ExponentialMovingAverage diffAvg,
    float snapThreshold,
    out float timescale,
    out bool snap)
  {
    if (current == 0.0)
    {
      snap = true;
      timescale = 1f;
      current = target;
    }
    else
    {
      float newValue = (float) (target - current);
      diffAvg.Add((double) newValue);
      float num = (float) diffAvg.Value;
      if ((double) Mathf.Abs(num) > (double) snapThreshold)
      {
        Debug.LogWarning((object) $"{Time.unscaledTime:0.000}: Smooth Time Snap: diff={num}");
        snap = true;
        timescale = 1f;
        current = target;
        diffAvg.Reset();
      }
      else
      {
        timescale = SmoothNetworkTime.CalculateTimeScale((double) num);
        snap = false;
      }
    }
  }

  private static float CalculateTimeScale(double diff)
  {
    if (diff > 0.25)
      return 1.08f;
    if (diff > 0.02500000037252903)
      return 1.01f;
    if (diff < -0.024999998509883881)
      return 0.8f;
    if (diff < -1.0 / 400.0)
      return 0.96f;
    if (diff > 1.0 / 400.0)
      return 1.0025f;
    return diff < -0.00024999998277053237 ? 0.9975f : 1f;
  }

  private class Timer
  {
    public readonly ExponentialMovingAverage AvgDiff = new ExponentialMovingAverage(20);
    public double Time;
    public float Timescale = 1f;
  }
}

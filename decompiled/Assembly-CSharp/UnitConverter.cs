// Decompiled with JetBrains decompiler
// Type: UnitConverter
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;

#nullable disable
public static class UnitConverter
{
  public static string AltitudeReading(float altitude)
  {
    if (PlayerSettings.unitSystem != PlayerSettings.UnitSystem.Metric)
      return $"{(ValueType) (float) ((double) altitude * 3.2808399200439453):F0}ft";
    return (double) Mathf.Abs(altitude) >= 10.0 ? $"{altitude:F0}m" : $"{altitude:F1}m";
  }

  public static string DistanceReading(float distance)
  {
    if (PlayerSettings.unitSystem == PlayerSettings.UnitSystem.Metric)
    {
      if ((double) distance > 10000.0)
        return $"{(ValueType) (float) ((double) distance * (1.0 / 1000.0)):F0}km";
      return (double) distance > 1000.0 ? $"{(ValueType) (float) ((double) distance * (1.0 / 1000.0)):F1}km" : $"{distance:F0}m";
    }
    float num = distance * 1.09361f;
    return (double) num < 1000.0 ? $"{num:F0}yd" : $"{(ValueType) (float) ((double) distance * 0.00053995702182874084):F1}nm";
  }

  public static string SpeedReading(float speed)
  {
    return PlayerSettings.unitSystem == PlayerSettings.UnitSystem.Metric ? $"{(double) speed * 3.6:F0}km/h" : $"{(ValueType) (float) ((double) speed * 1.9438400268554688):F0}kt";
  }

  public static string SpeedReadingGround(float speed)
  {
    return PlayerSettings.unitSystem == PlayerSettings.UnitSystem.Metric ? $"{(double) speed * 3.6:F0}km/h" : $"{(double) speed * 2.23694:F0}mph";
  }

  public static string ClimbRateReading(float speed)
  {
    string str = "";
    if ((double) speed > 0.5)
      str = "+";
    if (PlayerSettings.unitSystem != PlayerSettings.UnitSystem.Metric)
      return $"{str}{(ValueType) (float) ((double) speed * 60.0 * 3.2808399200439453):F0}fpm";
    return (double) Mathf.Abs(speed) >= 10.0 ? $"{str}{speed:F0}m/s" : $"{str}{speed:F1}m/s";
  }

  public static string DimensionReading(float length)
  {
    return PlayerSettings.unitSystem == PlayerSettings.UnitSystem.Metric ? $"{length:F1}m" : $"{(double) length * 3.28084:F1}ft";
  }

  public static string WeightReading(float weight)
  {
    if ((double) weight > 10000.0)
      return $"{(ValueType) (float) ((double) weight * (1.0 / 1000.0)):F2}t";
    return PlayerSettings.unitSystem == PlayerSettings.UnitSystem.Metric ? $"{weight:F0}kg" : $"{(double) weight * 2.20462:F0}lb";
  }

  public static string YieldReading(float yield)
  {
    if ((double) yield > 10000000000.0)
      return $"{(ValueType) (float) ((double) yield * 9.9999997171806854E-10):F0}Mt";
    if ((double) yield > 100000000.0)
      return $"{(ValueType) (float) ((double) yield * 9.9999999747524271E-07):F0}kt";
    if ((double) yield > 10000000.0)
      return $"{(ValueType) (float) ((double) yield * 9.9999999747524271E-07):F1}kt";
    return PlayerSettings.unitSystem == PlayerSettings.UnitSystem.Metric ? $"{yield:F0}kg" : $"{(double) yield * 2.20462:F0}lb";
  }

  public static string PowerReading(float kW)
  {
    if (PlayerSettings.unitSystem != PlayerSettings.UnitSystem.Metric)
      return $"{kW * 1.34102f:F1}hp";
    if ((double) kW < 1.0)
      return $"{(ValueType) (float) ((double) kW * 1000.0):F0}W";
    return (double) kW < 1000.0 ? $"{kW:F1}kW" : $"{(ValueType) (float) ((double) kW * (1.0 / 1000.0)):F2}MW";
  }

  public static string PowerToWeightReading(float kWPerKg)
  {
    if (PlayerSettings.unitSystem == PlayerSettings.UnitSystem.Metric)
      return $"{kWPerKg:F2}kW/kg";
    kWPerKg *= 1.341f;
    kWPerKg *= 0.4536f;
    return $"{kWPerKg:F2}hp/lb";
  }

  public static string ValueReading(float valueInMillions)
  {
    float num = valueInMillions * 1000000f;
    if ((double) num * (double) num < 100000000.0)
      return $"${num:F0}";
    if ((double) valueInMillions * (double) valueInMillions < 1.0)
      return $"${(ValueType) (float) ((double) valueInMillions * 1000.0):F1}k";
    if ((double) valueInMillions * (double) valueInMillions < 100.0)
      return $"${(ValueType) (float) ((double) valueInMillions * 1.0):F2}m";
    if ((double) valueInMillions * (double) valueInMillions < 1000000.0)
      return $"${valueInMillions:F1}m";
    return (double) valueInMillions * (double) valueInMillions < 999999995904.0 ? $"${(ValueType) (float) ((double) valueInMillions * (1.0 / 1000.0)):F2}b" : $"${(ValueType) (float) ((double) valueInMillions * 9.9999999747524271E-07):F3}t";
  }

  public static string TimeOfDay(float totalHours, bool includeSeconds)
  {
    int num1 = (int) totalHours;
    float num2 = (float) ((double) totalHours % 1.0 * 3600.0);
    int num3 = (int) ((double) num2 / 60.0);
    if (!includeSeconds)
      return $"{num1:D2}:{num3:D2}";
    int num4 = (int) ((double) num2 % 60.0);
    return $"{num1:D2}:{num3:D2}:{num4:D2}";
  }
}

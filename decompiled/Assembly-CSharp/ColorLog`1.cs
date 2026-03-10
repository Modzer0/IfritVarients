// Decompiled with JetBrains decompiler
// Type: ColorLog`1
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using System.Diagnostics;
using UnityEngine;

#nullable disable
public class ColorLog<T> : ColorLog
{
  private static string prefix = ColorLog.CreatePrefixForType<T>(Application.isEditor);

  private ColorLog()
  {
  }

  [Conditional("UNITY_EDITOR")]
  public static void Trace(string extra = null)
  {
    Console.WriteLine($"{LogTimeUpdater.unscaledTime:0.000}: {ColorLog<T>.prefix}{(extra == null ? (object) "" : (object) (" " + extra))} TraceStack:{Environment.StackTrace}");
  }

  [Conditional("UNITY_EDITOR")]
  public static void Log(string message)
  {
    UnityEngine.Debug.Log((object) $"{LogTimeUpdater.unscaledTime:0.000}: {ColorLog<T>.prefix} {message}");
  }

  [Conditional("UNITY_EDITOR")]
  public static void LogWarning(string message)
  {
    UnityEngine.Debug.LogWarning((object) $"{LogTimeUpdater.unscaledTime:0.000}: {ColorLog<T>.prefix} [Warn] {message}");
  }

  public static void LogError(string message)
  {
    UnityEngine.Debug.LogError((object) $"{LogTimeUpdater.unscaledTime:0.000}: {ColorLog<T>.prefix} [Error] {message}");
  }

  [Conditional("UNITY_EDITOR")]
  public static void EditorAssert(bool condition, string message)
  {
    if (condition)
      return;
    UnityEngine.Debug.LogError((object) $"{LogTimeUpdater.unscaledTime:0.000}: {ColorLog<T>.prefix} [Assert] {message}");
  }

  public static void Info(string message)
  {
    UnityEngine.Debug.Log((object) $"{LogTimeUpdater.unscaledTime:0.000}: {ColorLog<T>.prefix} {message}");
  }

  public static void InfoWarn(string message)
  {
    UnityEngine.Debug.LogWarning((object) $"{LogTimeUpdater.unscaledTime:0.000}: {ColorLog<T>.prefix} [Warn] {message}");
  }

  public static void InfoAssert(bool condition, string message)
  {
    if (condition)
      return;
    UnityEngine.Debug.LogError((object) $"{LogTimeUpdater.unscaledTime:0.000}: {ColorLog<T>.prefix} [Assert] {message}");
  }
}

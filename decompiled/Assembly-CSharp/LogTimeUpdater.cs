// Decompiled with JetBrains decompiler
// Type: LogTimeUpdater
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Cysharp.Threading.Tasks;
using UnityEngine;

#nullable disable
public static class LogTimeUpdater
{
  public static float unscaledTime;
  public static bool IsRunning;

  public static async UniTask RunForever()
  {
    if (LogTimeUpdater.IsRunning)
    {
      Debug.LogError((object) "LogTimeUpdater running");
    }
    else
    {
      LogTimeUpdater.IsRunning = true;
      try
      {
        while (true)
        {
          LogTimeUpdater.unscaledTime = Time.unscaledTime;
          await UniTask.Yield(PlayerLoopTiming.EarlyUpdate);
        }
      }
      finally
      {
        LogTimeUpdater.IsRunning = false;
      }
    }
  }
}

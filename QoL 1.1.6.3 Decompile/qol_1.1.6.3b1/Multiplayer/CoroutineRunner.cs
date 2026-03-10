// Decompiled with JetBrains decompiler
// Type: qol.Multiplayer.CoroutineRunner
// Assembly: qol, Version=1.1.6.3, Culture=neutral, PublicKeyToken=null
// MVID: 3B6539EA-E38C-45EE-8FFF-FC58CC42BADC
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\BepInEx\plugins\qol_1.1.6.3b1.dll

using BepInEx.Logging;
using UnityEngine;

#nullable disable
namespace qol.Multiplayer;

public class CoroutineRunner : MonoBehaviour
{
  private static CoroutineRunner _instance;
  private static readonly object _lock = new object();
  private static bool _isShuttingDown = false;
  private static ManualLogSource _logger;

  public static CoroutineRunner Instance
  {
    get
    {
      if (CoroutineRunner._isShuttingDown)
      {
        CoroutineRunner._logger?.LogWarning((object) "[CoroutineRunner] Instance requested during shutdown, returning null");
        return (CoroutineRunner) null;
      }
      lock (CoroutineRunner._lock)
      {
        if ((Object) CoroutineRunner._instance == (Object) null)
        {
          CoroutineRunner._logger?.LogInfo((object) "[CoroutineRunner] Searching for existing instance...");
          CoroutineRunner._instance = Object.FindObjectOfType<CoroutineRunner>();
          if ((Object) CoroutineRunner._instance == (Object) null)
          {
            CoroutineRunner._logger?.LogWarning((object) "[CoroutineRunner] Creating new CoroutineRunner instance");
            GameObject target = new GameObject("QOLMod_CoroutineRunner");
            CoroutineRunner._instance = target.AddComponent<CoroutineRunner>();
            Object.DontDestroyOnLoad((Object) target);
            CoroutineRunner._logger?.LogWarning((object) "[CoroutineRunner] New instance created and marked as DontDestroyOnLoad");
          }
          else
            CoroutineRunner._logger?.LogWarning((object) "[CoroutineRunner] Using existing CoroutineRunner instance found in scene");
        }
        return CoroutineRunner._instance;
      }
    }
  }

  public static void Initialize(ManualLogSource logger) => CoroutineRunner._logger = logger;

  private void OnApplicationQuit()
  {
    CoroutineRunner._logger?.LogWarning((object) "[CoroutineRunner] Application quitting, setting shutdown flag");
    CoroutineRunner._isShuttingDown = true;
  }

  private void OnDestroy()
  {
    if (!((Object) CoroutineRunner._instance == (Object) this))
      return;
    CoroutineRunner._logger?.LogWarning((object) "[CoroutineRunner] Instance destroyed");
    CoroutineRunner._instance = (CoroutineRunner) null;
  }
}

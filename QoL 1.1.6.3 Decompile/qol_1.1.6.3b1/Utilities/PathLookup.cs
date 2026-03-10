// Decompiled with JetBrains decompiler
// Type: qol.Utilities.PathLookup
// Assembly: qol, Version=1.1.6.3, Culture=neutral, PublicKeyToken=null
// MVID: 3B6539EA-E38C-45EE-8FFF-FC58CC42BADC
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\BepInEx\plugins\qol_1.1.6.3b1.dll

using BepInEx.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#nullable disable
namespace qol.Utilities;

public static class PathLookup
{
  private static readonly Dictionary<string, GameObject> _pathCache = new Dictionary<string, GameObject>();
  private static ManualLogSource _logger;

  public static void Initialize(ManualLogSource logger) => PathLookup._logger = logger;

  public static GameObject Find(string path, bool checkCache = true)
  {
    GameObject gameObject1;
    if (checkCache && PathLookup._pathCache.TryGetValue(path, out gameObject1))
    {
      if ((UnityEngine.Object) gameObject1 != (UnityEngine.Object) null)
        return gameObject1;
      PathLookup._pathCache.Remove(path);
    }
    if (string.IsNullOrEmpty(path))
    {
      PathLookup._logger?.LogDebug((object) "PathLookup.Find: empty path provided.");
      return (GameObject) null;
    }
    string[] parts = path.Split('/', StringSplitOptions.None);
    List<GameObject> list = ((IEnumerable<GameObject>) Resources.FindObjectsOfTypeAll<GameObject>()).Where<GameObject>((Func<GameObject, bool>) (go => (UnityEngine.Object) go.transform.parent == (UnityEngine.Object) null && go.name == parts[0])).ToList<GameObject>();
    if (list.Count == 0)
    {
      PathLookup._logger?.LogDebug((object) $"PathLookup.Find: no root match for '{path}'.");
      PathLookup._pathCache[path] = (GameObject) null;
      return (GameObject) null;
    }
    foreach (GameObject gameObject2 in list)
    {
      GameObject gameObject3 = gameObject2;
      bool flag = true;
      for (int index = 1; index < parts.Length; ++index)
      {
        Transform transform = gameObject3.transform.Find(parts[index]);
        if ((UnityEngine.Object) transform == (UnityEngine.Object) null)
        {
          flag = false;
          break;
        }
        gameObject3 = transform.gameObject;
      }
      if (flag)
      {
        PathLookup._pathCache[path] = gameObject3;
        return gameObject3;
      }
    }
    PathLookup._logger?.LogDebug((object) $"PathLookup.Find: path '{path}' not found.");
    PathLookup._pathCache[path] = (GameObject) null;
    return (GameObject) null;
  }

  public static void ClearCache() => PathLookup._pathCache.Clear();
}

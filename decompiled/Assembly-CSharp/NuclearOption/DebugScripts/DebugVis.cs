// Decompiled with JetBrains decompiler
// Type: NuclearOption.DebugScripts.DebugVis
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System.Collections.Generic;
using UnityEngine;

#nullable disable
namespace NuclearOption.DebugScripts;

public static class DebugVis
{
  private static DebugVis.DebugVisTracker tracker;
  private static readonly List<GameObject> Markers = new List<GameObject>();

  public static bool Enabled => PlayerSettings.debugVis;

  public static bool Create<T>(ref T field, T prefab, Transform parent = null) where T : Object
  {
    if ((Object) field != (Object) null)
      return false;
    field = Object.Instantiate<T>(prefab, parent);
    if ((Object) DebugVis.tracker == (Object) null)
      DebugVis.tracker = new GameObject("DebugVisTracker").AddComponent<DebugVis.DebugVisTracker>();
    if (field is GameObject gameObject)
      DebugVis.Markers.Add(gameObject);
    else if (field is Component component)
      DebugVis.Markers.Add(component.gameObject);
    return true;
  }

  public static void AddMarker(GameObject go) => DebugVis.Markers.Add(go);

  public static void CleanupMarkers()
  {
    Object.Destroy((Object) DebugVis.tracker.gameObject);
    foreach (Object marker in DebugVis.Markers)
      Object.Destroy(marker);
  }

  public class DebugVisTracker : MonoBehaviour
  {
    private void Update()
    {
      if (DebugVis.Enabled)
        return;
      DebugVis.CleanupMarkers();
    }
  }
}

// Decompiled with JetBrains decompiler
// Type: NuclearOption.MissionEditorScripts.FixLayout
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#nullable disable
namespace NuclearOption.MissionEditorScripts;

public class FixLayout
{
  private static bool requestRebuild;
  private static readonly List<RectTransform> toRebuild = new List<RectTransform>();
  private static List<LayoutGroup> cache = new List<LayoutGroup>();
  private static Queue<RectTransform> forceRebuildQueue = new Queue<RectTransform>();
  private static int rebuildCount = 0;
  private static int totalRebuilds = 0;
  private const int MAX_REBUILDS = 10;

  private FixLayout()
  {
  }

  public static void ForceRebuildAtEndOfFrame(RectTransform target)
  {
    if (FixLayout.toRebuild.Contains(target))
      return;
    FixLayout.toRebuild.Add(target);
    if (FixLayout.requestRebuild)
      return;
    FixLayout.requestRebuild = true;
    FixLayout.DelayRebuild().Forget();
  }

  private static async UniTask DelayRebuild()
  {
    await UniTask.Yield(PlayerLoopTiming.PostLateUpdate);
    FixLayout.requestRebuild = false;
    foreach (RectTransform target in FixLayout.toRebuild)
    {
      if (!((Object) target == (Object) null))
        FixLayout.ForceRebuildRecursive(target);
    }
    FixLayout.toRebuild.Clear();
  }

  public static void ForceRebuildRecursive(RectTransform target)
  {
    FixLayout.forceRebuildQueue.Enqueue(target);
    if (FixLayout.rebuildCount != 0)
      return;
    FixLayout.RebuildNow();
  }

  private static void RebuildNow()
  {
    try
    {
      while (FixLayout.forceRebuildQueue.Count > 0)
      {
        ++FixLayout.rebuildCount;
        if (FixLayout.rebuildCount > 10)
        {
          Debug.LogError((object) "FixLayout reached max rebuilds");
          break;
        }
        FixLayout.RebuildNext();
      }
    }
    finally
    {
      FixLayout.rebuildCount = 0;
      FixLayout.totalRebuilds = 0;
      FixLayout.forceRebuildQueue.Clear();
    }
  }

  private static void RebuildNext()
  {
    FixLayout.forceRebuildQueue.Dequeue().GetComponentsInChildren<LayoutGroup>(FixLayout.cache);
    FixLayout.cache.Reverse();
    foreach (LayoutGroup layoutGroup in FixLayout.cache)
    {
      ++FixLayout.totalRebuilds;
      LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform) layoutGroup.transform);
    }
  }
}

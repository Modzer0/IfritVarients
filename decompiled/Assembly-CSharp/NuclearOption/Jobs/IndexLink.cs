// Decompiled with JetBrains decompiler
// Type: NuclearOption.Jobs.IndexLink
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Jobs;

#nullable disable
namespace NuclearOption.Jobs;

public struct IndexLink
{
  public NullableIndex TransformIndex;

  public override string ToString() => this.TransformIndex.ToString();

  public static void AddTransform(
    TransformAccessArray transformAccess,
    NativeArray<PtrRefCounter<IndexLink>> transformLinks,
    List<PtrRefCounter<IndexLink>> partLinks,
    ref PtrRefCounter<IndexLink> field,
    Transform transform)
  {
    JobsAllocator<IndexLink>.AllocateRefCounter(ref field);
    IndexLink.AddLinked(transformAccess, transformLinks, field, transform);
    partLinks.Add(field);
    field.AddRef();
  }

  private static void AddLinked(
    TransformAccessArray transformAccess,
    NativeArray<PtrRefCounter<IndexLink>> transformLinks,
    PtrRefCounter<IndexLink> link,
    Transform toAdd)
  {
    int length = transformAccess.length;
    transformAccess.Add(toAdd);
    transformLinks[length] = link;
    link.AddRef();
    link.Ref().TransformIndex = new NullableIndex(length);
  }

  public static void QueueToRemove(
    List<int> outToRemove,
    NativeArray<PtrRefCounter<IndexLink>> transformLinks,
    List<PtrRefCounter<IndexLink>> linksToRemove)
  {
    foreach (PtrRefCounter<IndexLink> ptrRefCounter in linksToRemove)
    {
      NullableIndex transformIndex = ptrRefCounter.Value().TransformIndex;
      if (transformIndex.HasValue)
        outToRemove.Add(transformIndex.Index);
      ptrRefCounter.RemoveRef();
    }
    linksToRemove.Clear();
  }

  public static void RemoveLinks(
    TransformAccessArray transformAccess,
    NativeArray<PtrRefCounter<IndexLink>> transformLinks,
    List<int> toRemove)
  {
    toRemove.Sort();
    for (int index = toRemove.Count - 1; index >= 0; --index)
    {
      int removeIndex = toRemove[index];
      IndexLink.RemoveLinked(transformAccess, transformLinks, removeIndex);
    }
    toRemove.Clear();
  }

  private static void RemoveLinked(
    TransformAccessArray transformAccess,
    NativeArray<PtrRefCounter<IndexLink>> linkArray,
    int removeIndex)
  {
    int index = transformAccess.length - 1;
    transformAccess.RemoveAtSwapBack(removeIndex);
    PtrRefCounter<IndexLink> link1 = linkArray[removeIndex];
    link1.Ref().TransformIndex = new NullableIndex();
    link1.RemoveRef();
    if (index == removeIndex)
      return;
    PtrRefCounter<IndexLink> link2 = linkArray[index];
    linkArray[removeIndex] = link2;
    link2.Ref().TransformIndex = new NullableIndex(removeIndex);
  }

  public static bool BurstGetTransformIndex(PtrRefCounter<IndexLink> link, out int index)
  {
    if (!link.IsCreated)
    {
      Debug.LogError((object) "Link was not allocated");
      index = 0;
      return false;
    }
    NullableIndex transformIndex = link.Value().TransformIndex;
    if (!transformIndex.HasValue)
    {
      Debug.LogError((object) "Not Transform index");
      index = 0;
      return false;
    }
    index = transformIndex.Index;
    return true;
  }
}

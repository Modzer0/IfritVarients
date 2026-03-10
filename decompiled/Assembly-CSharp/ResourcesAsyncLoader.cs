// Decompiled with JetBrains decompiler
// Type: ResourcesAsyncLoader
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

#nullable disable
public class ResourcesAsyncLoader
{
  public static async UniTask LoadPrefab(
    string path,
    CancellationToken cancel,
    Action<GameObject> afterLoad)
  {
    long start = BenchmarkScope.GetTimestamp();
    GameObject target = await ResourcesAsyncLoader.InstantiateOne((GameObject) await UnityAsyncExtensions.ToUniTask(Resources.LoadAsync<GameObject>(path)), cancel);
    if (cancel.IsCancellationRequested)
      return;
    UnityEngine.Object.DontDestroyOnLoad((UnityEngine.Object) target);
    long timestamp1 = BenchmarkScope.GetTimestamp();
    Action<GameObject> action = afterLoad;
    if (action != null)
      action(target);
    long timestamp2 = BenchmarkScope.GetTimestamp();
    ColorLog<ResourcesAsyncLoader>.Info($"Loaded {typeof (GameObject)}, load:{BenchmarkScope.MillisecondsBetween(start, timestamp1):0}, setup:{BenchmarkScope.MillisecondsBetween(timestamp1, timestamp2),0}");
  }

  public static ResourcesAsyncLoader<T> Create<T>(string path, Action<T> afterLoad = null) where T : ScriptableObject
  {
    return new ResourcesAsyncLoader<T>(path, (Func<UnityEngine.Object, CancellationToken, UniTask<T>>) ((scriptableObject, cancel) =>
    {
      Action<T> action = afterLoad;
      if (action != null)
        action((T) scriptableObject);
      return UniTask.FromResult<T>((T) scriptableObject);
    }));
  }

  public static ResourcesAsyncLoader<T> Create<T>(string path, Func<GameObject, T> afterLoad = null) where T : MonoBehaviour
  {
    return new ResourcesAsyncLoader<T>(path, (Func<UnityEngine.Object, CancellationToken, UniTask<T>>) (async (prefab, cancel) =>
    {
      GameObject target = await ResourcesAsyncLoader.InstantiateOne((GameObject) prefab, cancel);
      if (cancel.IsCancellationRequested)
        return default (T);
      UnityEngine.Object.DontDestroyOnLoad((UnityEngine.Object) target);
      if (afterLoad == null)
        afterLoad = (Func<GameObject, T>) (c => c.GetComponent<T>());
      return afterLoad(target);
    }));
  }

  private static async UniTask<GameObject> InstantiateOne(
    GameObject prefab,
    CancellationToken cancel)
  {
    (bool flag, GameObject[] gameObjectArray) = await UnityEngine.Object.InstantiateAsync<GameObject>(prefab).ToUniTask<GameObject>(cancellationToken: cancel).SuppressCancellationThrow();
    return !flag ? gameObjectArray[0] : (GameObject) null;
  }
}

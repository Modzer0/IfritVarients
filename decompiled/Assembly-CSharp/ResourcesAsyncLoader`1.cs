// Decompiled with JetBrains decompiler
// Type: ResourcesAsyncLoader`1
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

#nullable disable
public class ResourcesAsyncLoader<T> : ResourcesAsyncLoader where T : UnityEngine.Object
{
  private readonly string path;
  private readonly Func<UnityEngine.Object, CancellationToken, UniTask<T>> createInstance;
  private T instance;

  public ResourcesAsyncLoader(
    string path,
    Func<UnityEngine.Object, CancellationToken, UniTask<T>> createInstance)
  {
    this.path = path;
    this.createInstance = createInstance;
  }

  public bool IsLoaded => (UnityEngine.Object) this.instance != (UnityEngine.Object) null;

  public void AssetNotLoaded()
  {
    if (!((UnityEngine.Object) this.instance != (UnityEngine.Object) null))
      return;
    Debug.LogError((object) $"{typeof (T)} Should not have been loaded multiple time");
  }

  public T Get()
  {
    if (!MainMenu.ApplicationIsQuitting && (UnityEngine.Object) this.instance == (UnityEngine.Object) null)
      Debug.LogError((object) $"{typeof (T)} was not preloaded by menu");
    return this.instance;
  }

  public async UniTask Load(CancellationToken cancel)
  {
    if ((UnityEngine.Object) this.instance != (UnityEngine.Object) null)
    {
      Debug.LogWarning((object) $"{typeof (T)} already loaded");
    }
    else
    {
      long start = BenchmarkScope.GetTimestamp();
      UnityEngine.Object uniTask = await UnityAsyncExtensions.ToUniTask(Resources.LoadAsync<UnityEngine.Object>(this.path));
      if (cancel.IsCancellationRequested)
        return;
      if ((UnityEngine.Object) this.instance != (UnityEngine.Object) null)
      {
        Debug.LogError((object) $"{typeof (T)} was loaded sync while preloading");
      }
      else
      {
        long load = BenchmarkScope.GetTimestamp();
        this.instance = await this.createInstance(uniTask, cancel);
        if (cancel.IsCancellationRequested)
          return;
        long timestamp = BenchmarkScope.GetTimestamp();
        ColorLog<ResourcesAsyncLoader>.Info($"Loaded {typeof (T)}, load:{BenchmarkScope.MillisecondsBetween(start, load):0}, setup:{BenchmarkScope.MillisecondsBetween(load, timestamp):0}");
      }
    }
  }
}

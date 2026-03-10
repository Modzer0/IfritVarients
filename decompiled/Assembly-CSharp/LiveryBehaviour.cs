// Decompiled with JetBrains decompiler
// Type: LiveryBehaviour
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

#nullable disable
public class LiveryBehaviour : MonoBehaviour
{
  private Aircraft aircraft;
  private LiveryKey? fallback;
  private LiveryKey key;
  private bool hasLoaded;
  private AsyncOperationHandle<LiveryData> handle;
  private MaterialCleanup materialCleanup;

  public bool IsLoading { get; private set; }

  private void OnDestroy()
  {
    LiveryBehaviour.ReleaseHandle(ref this.handle);
    this.materialCleanup?.CleanupAll();
  }

  private static void ReleaseHandle(ref AsyncOperationHandle<LiveryData> handle)
  {
    if (!handle.IsValid())
      return;
    Addressables.Release<LiveryData>(handle);
    handle = new AsyncOperationHandle<LiveryData>();
  }

  public void Setup(Aircraft aircraft, LiveryKey? fallback = null)
  {
    this.aircraft = aircraft;
    this.fallback = fallback;
  }

  public void SetKey(LiveryKey key)
  {
    if (this.hasLoaded && this.key.Equals(key))
      return;
    if (this.IsLoading)
      Debug.LogError((object) "LiveryBehaviour.Load called multiple times");
    else
      this.Load(key, this.fallback).Forget();
  }

  private async UniTask Load(LiveryKey key, LiveryKey? fallback, CancellationToken cancel = default (CancellationToken))
  {
    LiveryBehaviour liveryBehaviour = this;
    if (cancel == CancellationToken.None)
      cancel = liveryBehaviour.destroyCancellationToken;
    liveryBehaviour.key = key;
    liveryBehaviour.hasLoaded = true;
    bool success = false;
    try
    {
      liveryBehaviour.IsLoading = true;
      AsyncOperationHandle<LiveryData> handle1;
      (success, handle1) = await key.Load(liveryBehaviour.aircraft);
      if (cancel.IsCancellationRequested && success)
      {
        LiveryBehaviour.ReleaseHandle(ref handle1);
        return;
      }
      if (success)
      {
        liveryBehaviour.SetLivery(handle1.Result);
        AsyncOperationHandle<LiveryData> handle2 = liveryBehaviour.handle;
        liveryBehaviour.handle = handle1;
        LiveryBehaviour.ReleaseHandle(ref handle2);
      }
    }
    catch (Exception ex)
    {
      Debug.LogWarning((object) $"Exception when loading: {ex}");
    }
    finally
    {
      liveryBehaviour.IsLoading = false;
    }
    if (success)
      return;
    Debug.LogWarning((object) $"Failed to load livery for {(liveryBehaviour.aircraft.HasAuthority ? (object) "local" : (object) "remote")} player. Key:{key}");
    if (!fallback.HasValue)
      return;
    Debug.LogWarning((object) "Loading fallback livery");
    await liveryBehaviour.Load(fallback.Value, new LiveryKey?(), cancel);
  }

  private void SetLivery(LiveryData livery)
  {
    if ((UnityEngine.Object) this.aircraft.weaponManager != (UnityEngine.Object) null)
      this.aircraft.weaponManager.UpdateColorables(livery);
    if (this.materialCleanup == null)
      this.materialCleanup = new MaterialCleanup();
    foreach (UnitPart unitPart in this.aircraft.partLookup)
      unitPart.SetLivery(livery, this.materialCleanup);
  }
}

// Decompiled with JetBrains decompiler
// Type: NuclearOption.AddressableScripts.ModLoader
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Cysharp.Threading.Tasks;
using NuclearOption.Workshop;
using Steamworks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

#nullable disable
namespace NuclearOption.AddressableScripts;

public abstract class ModLoader
{
  private static bool hasInit;

  private static void CheckInit()
  {
    if (ModLoader.hasInit)
      return;
    ModLoader.hasInit = true;
    Addressables.AddResourceLocator((IResourceLocator) new ModLoader.LocationLocator());
  }

  public abstract string Label { get; }

  public abstract System.Type AssetType { get; }

  private static string GetMetaPath(string folderName) => Path.Combine(folderName, "meta.json");

  public static void WriteMetaData<TMeta>(string folder, TMeta meta) where TMeta : struct, IMetaData
  {
    File.WriteAllText(ModLoader.GetMetaPath(folder), JsonUtility.ToJson((object) meta));
  }

  public static TMeta? ReadMetaData<TMeta>(SubscribedItem steamItem) where TMeta : struct, IMetaData
  {
    return ModLoader.ReadMetaData<TMeta>(steamItem.Id, steamItem.Folder);
  }

  public static TMeta? ReadMetaData<TMeta>(string folder) where TMeta : struct, IMetaData
  {
    return ModLoader.ReadMetaData<TMeta>(PublishedFileId_t.Invalid, folder);
  }

  public static TMeta? ReadMetaData<TMeta>(PublishedFileId_t id, string folder) where TMeta : struct, IMetaData
  {
    string metaPath = ModLoader.GetMetaPath(folder);
    if (!File.Exists(metaPath))
    {
      Debug.LogWarning((object) (metaPath + " does not exist"));
      return new TMeta?();
    }
    string json = File.ReadAllText(metaPath);
    try
    {
      TMeta meta = JsonUtility.FromJson<TMeta>(json);
      meta.FolderFullPath = folder;
      meta.Id = id;
      return new TMeta?(meta);
    }
    catch (Exception ex)
    {
      Debug.LogException(ex);
      return new TMeta?();
    }
  }

  public abstract UniTask<string> GetCatalogPath(string folderName);

  protected async UniTask<IResourceLocation> GetAddressableKey(string catalogFolder)
  {
    ModLoadCache.LocationCache locationCache;
    if (ModLoadCache.CacheLocations.TryGetValue(catalogFolder, out locationCache))
    {
      bool pending = locationCache.TaskPending;
      if (pending)
        ColorLog<ModLoader>.Info("Waiting on pending task for " + catalogFolder);
      while (locationCache.TaskPending)
        await UniTask.Yield();
      if (pending)
        ColorLog<ModLoader>.Info($"Pending task finished for {catalogFolder} success={locationCache.Location != null}");
      return locationCache.Location;
    }
    ColorLog<ModLoader>.Info("First load for " + catalogFolder);
    locationCache = new ModLoadCache.LocationCache();
    locationCache.TaskPending = true;
    ModLoadCache.CacheLocations.Add(catalogFolder, locationCache);
    try
    {
      IResourceLocation addressableKeyFullLoad = await this.GetAddressableKeyFullLoad(catalogFolder);
      locationCache.Location = addressableKeyFullLoad;
      return addressableKeyFullLoad;
    }
    finally
    {
      locationCache.TaskPending = false;
    }
  }

  protected async UniTask<IResourceLocation> GetAddressableKeyFullLoad(string catalogFolder)
  {
    string catalogPath = await this.GetCatalogPath(catalogFolder);
    if (!File.Exists(catalogPath))
    {
      Debug.LogError((object) (catalogPath + " does not exist"));
      return (IResourceLocation) null;
    }
    AsyncOperationHandle<IResourceLocator> handle = Addressables.LoadContentCatalogAsync(catalogPath);
    IList<IResourceLocation> locations;
    (await handle).Locate((object) this.Label, this.AssetType, out locations);
    IResourceLocation addressableKeyFullLoad = locations[0];
    Addressables.Release<IResourceLocator>(handle);
    return addressableKeyFullLoad;
  }

  public async UniTask<AsyncOperationHandle<TData>> LoadAsset<TData>(string folder)
  {
    IResourceLocation addressableKey = await this.GetAddressableKey(folder);
    return addressableKey == null ? new AsyncOperationHandle<TData>() : await ModLoader.LoadAssetInternal<TData>((object) addressableKey);
  }

  public static UniTask<AsyncOperationHandle<TData>> LoadAsset<TData>(AssetReferenceT<TData> obj) where TData : UnityEngine.Object
  {
    return ModLoader.LoadAssetInternal<TData>((object) obj);
  }

  public static async UniTask<AsyncOperationHandle<TData>> LoadAssetInternal<TData>(object obj)
  {
    ModLoader.CheckInit();
    AsyncOperationHandle<TData> handle = Addressables.LoadAssetAsync<TData>(obj);
    TData data = await handle;
    AsyncOperationHandle<TData> asyncOperationHandle = handle;
    handle = new AsyncOperationHandle<TData>();
    return asyncOperationHandle;
  }

  private class LocationLocator : IResourceLocator
  {
    public string LocatorId => "LocationToLocationLocator";

    public IEnumerable<object> Keys => Enumerable.Empty<object>();

    public bool Locate(object key, System.Type type, out IList<IResourceLocation> locations)
    {
      if (key is IResourceLocation resourceLocation && type.IsAssignableFrom(resourceLocation.ResourceType))
      {
        locations = (IList<IResourceLocation>) new List<IResourceLocation>()
        {
          resourceLocation
        };
        return true;
      }
      locations = (IList<IResourceLocation>) null;
      return false;
    }
  }
}

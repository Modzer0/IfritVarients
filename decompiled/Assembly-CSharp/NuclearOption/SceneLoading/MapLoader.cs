// Decompiled with JetBrains decompiler
// Type: NuclearOption.SceneLoading.MapLoader
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Cysharp.Threading.Tasks;
using Mirage;
using NuclearOption.Networking;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

#nullable disable
namespace NuclearOption.SceneLoading;

[CreateAssetMenu(fileName = "MapLoader", menuName = "ScriptableObjects/MapLoader", order = 998)]
public class MapLoader : ScriptableObject
{
  public static readonly string Empty = "EMPTY";
  public static readonly string MainMenu = "Assets/Scenes/MainMenu/MainMenu.unity";
  public static readonly string MultiplayerMenu = "Assets/Scenes/MultiplayerMenu/MultiplayerMenu.unity";
  public static readonly string MissionsMenu = "Assets/Scenes/MissionsMenu/MissionsMenu.unity";
  public static readonly string Encyclopedia = "Assets/Scenes/Encyclopedia/Encyclopedia.unity";
  [SerializeField]
  private GameObject menuCameraPrefab;
  [FormerlySerializedAs("DefaultScene")]
  public MapKey DefaultMap;
  [Scene]
  public string[] GameScenes;
  [Header("Game World")]
  [Scene]
  public string GameWorldScene;
  public MapDetails[] Maps;

  public MapKey CurrentMap { get; private set; }

  public MapLoader.SceneKey CurrentScene { get; private set; }

  public void ClearMap()
  {
    this.CurrentMap = new MapKey();
    this.CurrentScene = new MapLoader.SceneKey();
  }

  public async UniTask<MapLoader.LoadResult> Load(MapKey mapKey, IProgress<float> progress = null)
  {
    if (mapKey.Type == MapKey.KeyType.None)
    {
      mapKey = this.DefaultMap;
      ColorLog<MapLoader>.Info($"Using Default scene {mapKey}");
    }
    else
      ColorLog<MapLoader>.Info($"Try load {mapKey}");
    if (mapKey.Equals(this.CurrentMap))
    {
      ColorLog<MapLoader>.Info("Already loaded");
      return MapLoader.LoadResult.AlreadyLoaded;
    }
    if (!this.CanLoad(mapKey))
      return MapLoader.LoadResult.InvalidKey;
    MapLoader.SceneKey sceneKey = this.SceneFromMap(mapKey);
    long startSceneTime = Stopwatch.GetTimestamp();
    MapLoader.LoadResult loadResult = await this.LoadScene(sceneKey, progress);
    ColorLog<MapLoader>.Info($"Scene Load duration {BenchmarkScope.MillisecondsSince(startSceneTime)}ms");
    if (loadResult == MapLoader.LoadResult.Failed)
      return MapLoader.LoadResult.Failed;
    this.CurrentScene = sceneKey;
    if (loadResult == MapLoader.LoadResult.ChangedScene)
    {
      ColorLog<MapLoader>.Info("Scene changed, settings up network objects");
      if (NetworkManagerNuclearOption.i.Server.Active)
        MapLoader.ServerSceneSetup();
      else if (NetworkManagerNuclearOption.i.Client.Active)
        MapLoader.ClientSceneSetup();
    }
    if (mapKey.Type == MapKey.KeyType.GameWorldPrefab)
    {
      long timestamp = Stopwatch.GetTimestamp();
      int num = SceneSingleton<MapSettingsManager>.i.EnableMap(mapKey.Path) ? 1 : 0;
      ColorLog<MapLoader>.Info($"Prefab Load duration {BenchmarkScope.MillisecondsSince(timestamp)}ms");
      if (num == 0)
        return MapLoader.LoadResult.Failed;
      if (loadResult == MapLoader.LoadResult.AlreadyLoaded)
        loadResult = MapLoader.LoadResult.ChangedWorldPrefab;
    }
    this.CurrentMap = mapKey;
    return loadResult;
  }

  public bool CanLoad(MapKey key)
  {
    switch (key.Type)
    {
      case MapKey.KeyType.None:
        ColorLog<MapLoader>.InfoWarn("Can't load key None");
        return false;
      case MapKey.KeyType.GameWorldPrefab:
        int num = ((IEnumerable<MapDetails>) this.Maps).Any<MapDetails>((Func<MapDetails, bool>) (x => x.PrefabName == key.Path)) ? 1 : 0;
        if (num != 0)
        {
          ColorLog<MapLoader>.Info("Found GameWorldPrefab " + key.Path);
          return num != 0;
        }
        ColorLog<MapLoader>.InfoWarn("Could not find GameWorldPrefab " + key.Path);
        return num != 0;
      case MapKey.KeyType.BuiltinScene:
        if (key.Path == this.GameWorldScene)
        {
          ColorLog<MapLoader>.InfoWarn("BuiltinScene should not be loading GameWorld, Use GameWorldPrefab instead");
          return false;
        }
        foreach (string gameScene in this.GameScenes)
        {
          if (gameScene == key.Path)
          {
            ColorLog<MapLoader>.Info("Found Scene full path " + key.Path);
            return true;
          }
          if (Path.GetFileNameWithoutExtension(gameScene) == key.Path)
          {
            ColorLog<MapLoader>.Info("Found Scene name " + key.Path);
            return true;
          }
        }
        ColorLog<MapLoader>.InfoWarn($"Could not find {key.Path} in Allowed scene list");
        return false;
      default:
        throw new InvalidEnumArgumentException("Type", (int) key.Type, typeof (MapKey.KeyType));
    }
  }

  private async UniTask<MapLoader.LoadResult> LoadScene(
    MapLoader.SceneKey key,
    IProgress<float> progress)
  {
    if (key.Equals(this.CurrentScene))
    {
      ColorLog<MapLoader>.Info($"Scene already loaded {key}");
      return MapLoader.LoadResult.AlreadyLoaded;
    }
    ColorLog<MapLoader>.Info($"Trying to load {key}");
    if (key.Type != MapLoader.SceneKey.KeyType.BuiltinScene)
      throw new InvalidEnumArgumentException("Type", (int) key.Type, typeof (MapKey.KeyType));
    try
    {
      ColorLog<MapLoader>.Info("Start Load " + key.Path);
      await SceneManager.LoadSceneAsync(key.Path).ToUniTask(progress);
      ColorLog<MapLoader>.Info("Load Finished, active scene = " + SceneManager.GetSceneAt(SceneManager.sceneCount - 1).path);
      return MapLoader.LoadResult.ChangedScene;
    }
    catch (Exception ex)
    {
      UnityEngine.Debug.LogError((object) $"Loading scene threw {ex.GetType()}");
      UnityEngine.Debug.LogException(ex);
      return MapLoader.LoadResult.Failed;
    }
  }

  private static void ClientSceneSetup()
  {
    ClientObjectManager clientObjectManager = NetworkManagerNuclearOption.i.ClientObjectManager;
    clientObjectManager.spawnableObjects.Clear();
    foreach (NetworkIdentity identity in Resources.FindObjectsOfTypeAll<NetworkIdentity>())
    {
      if (!identity.IsSpawned && MapLoader.GetObjectType(identity) == ObjectType.SceneObject)
        clientObjectManager.spawnableObjects.Add(identity.SceneId, identity);
    }
  }

  private static void ServerSceneSetup()
  {
    ServerObjectManager serverObjectManager = NetworkManagerNuclearOption.i.ServerObjectManager;
    NetworkIdentity[] objectsOfTypeAll = Resources.FindObjectsOfTypeAll<NetworkIdentity>();
    List<NetworkIdentity> networkIdentityList = new List<NetworkIdentity>();
    foreach (NetworkIdentity identity in objectsOfTypeAll)
    {
      if (identity.IsSpawned || MapLoader.GetObjectType(identity) == ObjectType.SceneObject)
        networkIdentityList.Add(identity);
    }
    networkIdentityList.Sort((Comparison<NetworkIdentity>) ((x, y) => x.NetId == 0U && y.NetId == 0U ? x.SceneId.CompareTo(y.SceneId) : x.NetId.CompareTo(y.NetId)));
    foreach (NetworkIdentity identity in networkIdentityList)
      serverObjectManager.Spawn(identity);
  }

  public MapLoader.SceneKey SceneFromMap(MapKey key)
  {
    switch (key.Type)
    {
      case MapKey.KeyType.GameWorldPrefab:
        ColorLog<MapLoader>.Info("Loading GameWorld=" + this.GameWorldScene);
        return new MapLoader.SceneKey(MapLoader.SceneKey.KeyType.BuiltinScene, this.GameWorldScene);
      case MapKey.KeyType.BuiltinScene:
        return new MapLoader.SceneKey(MapLoader.SceneKey.KeyType.BuiltinScene, key.Path);
      default:
        throw new InvalidEnumArgumentException("Type", (int) key.Type, typeof (MapKey.KeyType));
    }
  }

  public static ObjectType GetObjectType(NetworkIdentity identity)
  {
    if ((UnityEngine.Object) identity.gameObject.GetComponentInParent<NetworkMap>(true) != (UnityEngine.Object) null)
      return ObjectType.MapObject;
    return identity.IsSceneObject ? ObjectType.SceneObject : ObjectType.Prefab;
  }

  public UnityEngine.AsyncOperation LoadEmpty()
  {
    ColorLog<MapLoader>.Info("Creating empty scene");
    Scene activeScene = SceneManager.GetActiveScene();
    try
    {
      Scene scene = SceneManager.CreateScene("empty");
      if (GameManager.ShowEffects)
        SceneManager.MoveGameObjectToScene(UnityEngine.Object.Instantiate<GameObject>(this.menuCameraPrefab), scene);
    }
    catch (Exception ex)
    {
      UnityEngine.Debug.LogError((object) $"Failed to create empty: {ex}");
    }
    if (activeScene.IsValid() && activeScene.isLoaded)
    {
      UnityEngine.AsyncOperation asyncOperation = SceneManager.UnloadSceneAsync(activeScene);
      if (asyncOperation != null)
        return asyncOperation;
      UnityEngine.Debug.LogWarning((object) "UnloadSceneAsync returned null");
      return asyncOperation;
    }
    UnityEngine.Debug.LogWarning((object) "Active scene was not loaded");
    return (UnityEngine.AsyncOperation) null;
  }

  public bool TryGetMapName(MapKey key, out string mapName)
  {
    mapName = (string) null;
    if (key.Type == MapKey.KeyType.None)
      key = this.DefaultMap;
    MapDetails mapDetails = ((IEnumerable<MapDetails>) this.Maps).FirstOrDefault<MapDetails>((Func<MapDetails, bool>) (x => x.PrefabName == key.Path));
    if ((UnityEngine.Object) mapDetails != (UnityEngine.Object) null)
      mapName = mapDetails.MapName;
    return !string.IsNullOrEmpty(mapName);
  }

  public enum LoadResult
  {
    None,
    InvalidKey,
    Failed,
    ChangedScene,
    ChangedWorldPrefab,
    AlreadyLoaded,
  }

  [Serializable]
  public struct SceneKey(MapLoader.SceneKey.KeyType type, string path) : 
    IEquatable<MapLoader.SceneKey>
  {
    public MapLoader.SceneKey.KeyType Type = type;
    public string Path = path;

    public override string ToString() => $"({this.Type},{this.Path})";

    public readonly bool Equals(MapLoader.SceneKey other)
    {
      return this.Type == other.Type && this.Path == other.Path;
    }

    public readonly bool IsDefault() => new MapLoader.SceneKey().Equals(this);

    [Serializable]
    public enum KeyType : byte
    {
      BuiltinScene = 1,
      Addressables = 2,
    }
  }
}

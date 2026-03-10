// Decompiled with JetBrains decompiler
// Type: MapSettingsManager
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.Networking;
using NuclearOption.SceneLoading;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#nullable disable
public class MapSettingsManager : SceneSingleton<MapSettingsManager>
{
  public MapLoader MapLoader;
  public MapSettingsManager.Map[] Maps;
  private MapSettingsManager.Map currentMap;
  private MapSettings mapInScene;

  private void OnValidate()
  {
    if (this.Maps.Length != this.MapLoader.Maps.Length)
      Debug.LogError((object) "Map lists had different counts");
    else if (this.Maps.Length != ((IEnumerable<MapSettingsManager.Map>) this.Maps).Select<MapSettingsManager.Map, MapDetails>((Func<MapSettingsManager.Map, MapDetails>) (x => x.Details)).Distinct<MapDetails>().Count<MapDetails>())
    {
      Debug.LogError((object) "Duplicate maps found in list");
    }
    else
    {
      foreach (MapSettingsManager.Map map1 in this.Maps)
      {
        bool flag = false;
        foreach (MapDetails map2 in this.MapLoader.Maps)
        {
          if ((UnityEngine.Object) map1.Details == (UnityEngine.Object) map2)
          {
            flag = true;
            break;
          }
        }
        if (!flag)
        {
          Debug.LogError((object) $"Map {map1.Details.PrefabName} now found in MapLoader");
          break;
        }
      }
    }
  }

  public bool EnableMap(string mapName)
  {
    if (mapName == this.currentMap?.Details.PrefabName)
    {
      Debug.LogWarning((object) $"Map {mapName} was already enabled");
      return true;
    }
    ColorLog<MapSettingsManager>.Info("Enabling Map=" + mapName);
    MapSettingsManager.Map mapPrefab = ((IEnumerable<MapSettingsManager.Map>) this.Maps).FirstOrDefault<MapSettingsManager.Map>((Func<MapSettingsManager.Map, bool>) (x => x.Details.PrefabName == mapName));
    if (mapPrefab == null)
    {
      Debug.LogError((object) ("Could not find map with name " + mapName));
      return false;
    }
    if ((UnityEngine.Object) this.mapInScene != (UnityEngine.Object) null)
      MapSettingsManager.UnloadMap(this.mapInScene);
    this.mapInScene = MapSettingsManager.LoadMap(mapPrefab);
    this.currentMap = mapPrefab;
    return true;
  }

  private static MapSettings LoadMap(MapSettingsManager.Map mapPrefab)
  {
    MapSettings mapSettings = UnityEngine.Object.Instantiate<MapSettings>(mapPrefab.Prefab, Datum.origin.position, Quaternion.identity, Datum.origin);
    NetworkMap networkMap = mapSettings.NetworkMap;
    mapSettings.gameObject.SetActive(true);
    if (NetworkManagerNuclearOption.i.Server.Active)
      networkMap.ServerSpawn(NetworkManagerNuclearOption.i.ServerObjectManager);
    else if (NetworkManagerNuclearOption.i.Client.Active)
      networkMap.ClientRegister(NetworkManagerNuclearOption.i.ClientObjectManager);
    NetworkSceneSingleton<LevelInfo>.i.ApplyMapSettings(mapSettings);
    return mapSettings;
  }

  private static void UnloadMap(MapSettings map)
  {
    NetworkMap networkMap = map.NetworkMap;
    if (NetworkManagerNuclearOption.i.Server.Active)
      networkMap.ServerUnspawn(NetworkManagerNuclearOption.i.ServerObjectManager);
    else if (NetworkManagerNuclearOption.i.Client.Active)
      networkMap.ClientUnregister(NetworkManagerNuclearOption.i.ClientObjectManager);
    UnitRegistry.Clear();
    if (!((UnityEngine.Object) map != (UnityEngine.Object) null))
      return;
    map.gameObject.SetActive(false);
    UnityEngine.Object.Destroy((UnityEngine.Object) map.gameObject);
  }

  [Serializable]
  public class Map
  {
    public MapDetails Details;
    public MapSettings Prefab;
  }
}

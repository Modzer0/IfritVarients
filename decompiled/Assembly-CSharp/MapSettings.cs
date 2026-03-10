// Decompiled with JetBrains decompiler
// Type: MapSettings
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.SavedMission;
using NuclearOption.SceneLoading;
using RoadPathfinding;
using System;
using UnityEngine;

#nullable disable
public class MapSettings : MonoBehaviour
{
  [Header("Prefab setup")]
  public NetworkMap NetworkMap;
  [Header("Map Settings")]
  public Vector2 MapSize;
  public int GridSizeX;
  public int GridSizeY;
  public int OffsetX;
  public int OffsetY;
  public float Latitude;
  public RoadNetwork RoadNetwork;
  public RoadNetwork SeaLanes;
  public Texture2D OceanBasecolor;
  public Texture2D OceanDepthmap;
  public Texture2D TerrainColorMap;
  public Sprite MapImage;
  public Transform ReflectionProbePoint;
  public PositionRotation CameraPositionRotation;
  [SerializeField]
  private MapSettings.MapMusic[] factionMusic;

  public event Action BeforeDestroy;

  public AudioClip GetTacticalMusic(Faction faction)
  {
    foreach (MapSettings.MapMusic mapMusic in this.factionMusic)
    {
      if ((UnityEngine.Object) mapMusic.Faction == (UnityEngine.Object) faction)
        return mapMusic.GetTacticalMusic();
    }
    return (AudioClip) null;
  }

  public AudioClip GetStrategicMusic(Faction faction)
  {
    foreach (MapSettings.MapMusic mapMusic in this.factionMusic)
    {
      if ((UnityEngine.Object) mapMusic.Faction == (UnityEngine.Object) faction)
        return mapMusic.GetStrategicMusic();
    }
    return (AudioClip) null;
  }

  public AudioClip GetStartMusic(Faction faction)
  {
    foreach (MapSettings.MapMusic mapMusic in this.factionMusic)
    {
      if ((UnityEngine.Object) mapMusic.Faction == (UnityEngine.Object) faction)
        return mapMusic.GetStartMusic();
    }
    return (AudioClip) null;
  }

  public Color GetTerrainColorAtCoordinate(GlobalPosition globalPosition)
  {
    Vector2 vector2_1 = new Vector2(globalPosition.x + this.MapSize.x * 0.5f, globalPosition.z + this.MapSize.y * 0.5f);
    Vector2 vector2_2 = new Vector2(vector2_1.x / this.MapSize.x, vector2_1.y / this.MapSize.y);
    Vector2 vector2_3 = new Vector2(vector2_2.x * (float) this.TerrainColorMap.width, vector2_2.y * (float) this.TerrainColorMap.height);
    return this.TerrainColorMap.GetPixel((int) vector2_3.x, (int) vector2_3.y);
  }

  private void Start() => NetworkSceneSingleton<LevelInfo>.i.LoadedMapSettings = this;

  private void OnDestroy()
  {
    Action beforeDestroy = this.BeforeDestroy;
    if (beforeDestroy == null)
      return;
    beforeDestroy();
  }

  [Serializable]
  private class MapMusic
  {
    public Faction Faction;
    [SerializeField]
    private AudioClip startMusic;
    [SerializeField]
    private AudioClip tacticalMusic;
    [SerializeField]
    private AudioClip strategicMusic;

    public AudioClip GetTacticalMusic() => this.tacticalMusic;

    public AudioClip GetStrategicMusic() => this.strategicMusic;

    public AudioClip GetStartMusic() => this.startMusic;
  }
}

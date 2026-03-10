// Decompiled with JetBrains decompiler
// Type: LevelInfo
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Mirage;
using Mirage.Serialization;
using NuclearOption.Jobs;
using NuclearOption.Networking;
using NuclearOption.SavedMission;
using RoadPathfinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

#nullable disable
public class LevelInfo : NetworkSceneSingleton<LevelInfo>
{
  private static readonly int id_EmissionColor = Shader.PropertyToID("_EmissionColor");
  private static readonly int id_macro_basecolor = Shader.PropertyToID("_macro_basecolor");
  private static readonly int id_macro_depth = Shader.PropertyToID("_macro_depth");
  private static readonly int id_size = Shader.PropertyToID("_size");
  private static readonly int id_AmbientIntensity = Shader.PropertyToID("_AmbientIntensity");
  private static readonly int id_SunColor = Shader.PropertyToID("_SunColor");
  private static readonly int id_FogColor = Shader.PropertyToID("_FogColor");
  private static readonly int id_ScatterIntensity = Shader.PropertyToID("_ScatterIntensity");
  private static readonly int id_SunDirection = Shader.PropertyToID("_SunDirection");
  private static readonly int id_CloudOcclusion = Shader.PropertyToID("_CloudOcclusion");
  private static readonly int id_Altitude = Shader.PropertyToID("_Altitude");
  private static readonly int id_CameraPosition = Shader.PropertyToID("_CameraPosition");
  private static readonly int id_OriginOffset = Shader.PropertyToID("_OriginOffset");
  private static readonly int id_Conditions = Shader.PropertyToID("_Conditions");
  public MapSettings LoadedMapSettings;
  public float mapSize;
  public float hashGridSize;
  public GameObject worldAxis;
  public GameObject starfield;
  public GameObject moonAxis;
  public GameObject ocean;
  public GameObject sunObject;
  public GameObject moonObject;
  public Light sun;
  public Light moon;
  public Light nightVisionIlluminator;
  [SyncVar]
  public float timeOfDay;
  [SyncVar]
  public float conditions;
  [SyncVar]
  public float cloudHeight = 1800f;
  [SyncVar]
  public Vector3 windVelocity;
  [SyncVar]
  public float windTurbulence;
  [SyncVar]
  public float windSpeed;
  [SyncVar]
  public float moonPhase;
  private float ambientIntensity;
  [SerializeField]
  private WindZone windZone;
  [SerializeField]
  private CloudLayer cloudLayer;
  [SerializeField]
  private ReflectionProbe reflectionProbe;
  [SerializeField]
  private LevelInfo.SkyAnimData skyAnimData;
  [SerializeField]
  private Material[] lightScatteringParticles;
  [SerializeField]
  private Material waterMaterial;
  private Color sunColor;
  [NonSerialized]
  private Color[] lightScatteringParticlesStartingColor;
  private Material sunMat;
  private Material moonMat;
  public Volume PostProcessing;
  public Bloom bloom;
  [SerializeField]
  private Transform waterPlane;
  private Vector3 windOffset;
  public static NativeArray<float> airDensityChart;
  private float reflectionLastUpdated;
  private float skyLastUpdated;
  private float lightingLastUpdated;
  [SerializeField]
  private GameObject pathfindingSegmentVis;
  [SerializeField]
  private List<GameObject> pathfindingVisualizations;
  public UniversalAdditionalLightData SunURPLightData;
  public UniversalAdditionalLightData MoonURPLightData;
  private float timeFactor;
  private float windMainHeading;
  private float windRandomArc;
  private float lastWindChange;
  private float windChangeDelay = 120f;
  private MapSettings currentMapSettings;
  public bool isDayLight;
  private float daylightLastUpdate;
  private float daylightUpdateRate = 1f;
  [NonSerialized]
  private const int SYNC_VAR_COUNT = 7;
  [NonSerialized]
  private const int RPC_COUNT = 0;

  public RoadNetwork roadNetwork { get; private set; }

  public RoadNetwork seaLanes { get; private set; }

  public event Action onDaylightChange;

  protected override void Awake()
  {
    base.Awake();
    BattlefieldGrid.GenerateGrid(this.mapSize, this.hashGridSize);
    LevelInfo.GenerateAirDensityChart();
    ColorAdjustments component;
    if (this.PostProcessing.profile.TryGet<ColorAdjustments>(out component))
      ExposureController.SetColorAdjustments(component);
    this.PostProcessing.profile.TryGet<Bloom>(out this.bloom);
    this.sunMat = MaterialHelper.CloneMaterial((UnityEngine.Renderer) this.sunObject.GetComponent<MeshRenderer>());
    this.moonMat = MaterialHelper.CloneMaterial((UnityEngine.Renderer) this.moonObject.GetComponent<MeshRenderer>());
    this.lightScatteringParticlesStartingColor = ((IEnumerable<Material>) this.lightScatteringParticles).Select<Material, Color>((Func<Material, Color>) (x => x.GetColor(LevelInfo.id_EmissionColor))).ToArray<Color>();
    ExposureController.RegisterBrightLight(this.sun);
  }

  public void ApplyMapSettings(MapSettings mapSettings)
  {
    ColorLog<LevelInfo>.Info("ApplyMapSettings: " + mapSettings.name);
    this.currentMapSettings = mapSettings;
    this.mapSize = mapSettings.MapSize.x;
    this.worldAxis.transform.localEulerAngles = new Vector3(-mapSettings.Latitude, this.worldAxis.transform.localEulerAngles.y, this.worldAxis.transform.localEulerAngles.z);
    this.roadNetwork = mapSettings.RoadNetwork;
    this.seaLanes = mapSettings.SeaLanes;
    this.roadNetwork.RegenerateNetwork();
    this.seaLanes.RegenerateNetwork();
    this.waterMaterial.SetTexture(LevelInfo.id_macro_basecolor, (Texture) mapSettings.OceanBasecolor);
    this.waterMaterial.SetTexture(LevelInfo.id_macro_depth, (Texture) mapSettings.OceanDepthmap);
    this.waterMaterial.SetVector(LevelInfo.id_size, (Vector4) new Vector2(mapSettings.MapSize.x, mapSettings.MapSize.y));
    if ((UnityEngine.Object) mapSettings.ReflectionProbePoint != (UnityEngine.Object) null)
    {
      this.reflectionProbe.transform.SetParent(mapSettings.ReflectionProbePoint);
      this.reflectionProbe.transform.localPosition = Vector3.zero;
      mapSettings.BeforeDestroy += (Action) (() =>
      {
        if (!this.reflectionProbe.transform.IsChildOf(mapSettings.transform))
          return;
        this.reflectionProbe.transform.SetParent((Transform) null);
        this.reflectionProbe.transform.localPosition = Vector3.zero;
      });
    }
    DynamicMap.LoadMapImage(mapSettings);
    if (GameManager.gameState == GameState.Encyclopedia)
      return;
    this.LoadFromMission(MissionManager.CurrentMission);
  }

  private Camera GetCurrentCam()
  {
    return GameManager.gameState != GameState.Encyclopedia ? SceneSingleton<CameraStateManager>.i.mainCamera : Camera.main;
  }

  private static void GenerateAirDensityChart()
  {
    ColorLog<LevelInfo>.Info(nameof (GenerateAirDensityChart));
    if (LevelInfo.airDensityChart.IsCreated)
      LevelInfo.airDensityChart.Dispose();
    LevelInfo.airDensityChart = new NativeArray<float>(64 /*0x40*/, Allocator.Persistent);
    for (int index = 0; index < 64 /*0x40*/; ++index)
      LevelInfo.airDensityChart[index] = GameAssets.i.airDensityAltitude.Evaluate((float) index * 0.47619f);
  }

  public static float GetAirDensity(float altitude)
  {
    return ChartHelper.SafeRead(altitude * 0.0021f, LevelInfo.airDensityChart.AsReadOnlySpan());
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float GetSpeedOfSound(float altitude)
  {
    return Mathf.Max((float) (-0.004999999888241291 * (double) altitude + 340.0), 290f);
  }

  private void OnDestroy()
  {
    BattlefieldGrid.Clear();
    for (int index = 0; index < this.lightScatteringParticles.Length; ++index)
    {
      Material scatteringParticle = this.lightScatteringParticles[index];
      Color color1 = this.lightScatteringParticlesStartingColor[index];
      int idEmissionColor = LevelInfo.id_EmissionColor;
      Color color2 = color1;
      scatteringParticle.SetColor(idEmissionColor, color2);
    }
  }

  public float GetAmbientLight()
  {
    return (float) (((double) this.ambientIntensity + (double) this.moon.intensity * 0.10000000149011612) * (1.0 - (double) this.conditions * 0.5));
  }

  public Vector3 GetWind() => this.windVelocity;

  public float GetTurbulence() => this.windTurbulence;

  public Vector3 GetWind(GlobalPosition globalPosition)
  {
    globalPosition += this.windOffset;
    Vector3 vector3 = (float) ((double) Mathf.PerlinNoise1D(globalPosition.z * 0.02f) - 0.75 + 0.5 * (double) Mathf.PerlinNoise1D(globalPosition.z * 0.1f)) * this.windZone.transform.forward + (float) (0.5 * ((double) Mathf.PerlinNoise1D(globalPosition.x * 0.02f) - 0.75 + 0.5 * (double) Mathf.PerlinNoise1D(globalPosition.x * 0.1f))) * this.windZone.transform.right + (float) (0.30000001192092896 * ((double) Mathf.PerlinNoise1D(globalPosition.y * 0.02f) - 0.75 + 0.5 * (double) Mathf.PerlinNoise1D(globalPosition.y * 0.1f))) * Vector3.up;
    return this.windVelocity + this.windTurbulence * Mathf.Max(this.windSpeed, 10f) * vector3;
  }

  private void UpdateTimeOfDayLighting(bool forceImmediate)
  {
    if (!forceImmediate && (double) Time.realtimeSinceStartup - (double) this.lightingLastUpdated < 1.0)
      return;
    Camera currentCam = this.GetCurrentCam();
    float num1 = currentCam.transform.position.GlobalY();
    if ((double) num1 < 0.0)
      return;
    this.lightingLastUpdated = Time.realtimeSinceStartup;
    this.worldAxis.transform.eulerAngles = new Vector3(this.worldAxis.transform.eulerAngles.x, this.worldAxis.transform.eulerAngles.y, (float) ((double) this.timeOfDay * 15.0 + 180.0));
    float t = GameAssets.i.airDensityAltitude.Evaluate(num1 * (1f / 1000f));
    float num2 = 0.0f;
    this.ambientIntensity = this.skyAnimData.ambientIntensity.Evaluate(this.timeOfDay);
    Color color1 = Color.Lerp(this.skyAnimData.fogColorGround.Evaluate(this.timeOfDay / 24f), this.skyAnimData.fogColorAltitude.Evaluate(this.timeOfDay / 24f), (float) (((double) currentCam.transform.position.y - (double) this.cloudLayer.transform.position.y) * 0.00019999999494757503)) * Mathf.Lerp(1.2f, 1.6f, t * 4f);
    Color a1 = GameAssets.i.skyColor.Evaluate(this.timeOfDay / 24f);
    RenderSettings.reflectionIntensity = Mathf.Clamp01(this.ambientIntensity);
    float num3 = 0.0f;
    if ((double) this.conditions > 0.60000002384185791)
    {
      num2 = Mathf.Clamp((float) (((double) this.cloudLayer.transform.position.y + 200.0 - (double) currentCam.transform.position.y) * 0.004999999888241291), 0.0f, 1f);
      num3 = num2 * Mathf.InverseLerp(0.8f, 1f, this.conditions);
      Color a2 = color1 * (float) (1.0 - (double) num2 * 0.25);
      float num4 = Mathf.Max(a2.r, 0.07f);
      color1 = Color.Lerp(a2, new Color(num4, num4, num4), num2 * this.conditions + num3);
      a1 = Color.Lerp(a1, new Color(a1.r * 0.75f, a1.r * 0.75f, a1.r * 0.75f), num2 * this.conditions + num3);
    }
    RenderSettings.ambientEquatorColor = GameAssets.i.horizonColor.Evaluate(this.timeOfDay / 24f) * (1f + this.moon.intensity);
    RenderSettings.ambientGroundColor = GameAssets.i.groundColor.Evaluate(this.timeOfDay / 24f) * (1f + this.moon.intensity);
    RenderSettings.ambientSkyColor = a1 * 1.3f + this.moon.color * 1.5f * this.moon.intensity * Mathf.Clamp01((float) ((0.05000000074505806 - (double) this.ambientIntensity) * 40.0));
    RenderSettings.fogColor = color1;
    RenderSettings.ambientIntensity = this.ambientIntensity + 0.5f * this.moon.intensity;
    RenderSettings.skybox.SetFloat(LevelInfo.id_AmbientIntensity, this.ambientIntensity * t);
    this.sunColor = this.skyAnimData.sunColor.Evaluate(this.timeOfDay / 24f) * 300f;
    RenderSettings.skybox.SetColor(LevelInfo.id_SunColor, this.sunColor);
    RenderSettings.fogDensity = Mathf.Max((float) ((double) t * 0.00014000000373926014 * (double) this.skyAnimData.hazeAmount + (double) num3 * (1.0 / 1000.0)), 3E-05f);
    Color color2 = color1 * Mathf.Lerp(0.92f, 1f, t);
    RenderSettings.skybox.SetFloat(LevelInfo.id_Conditions, this.conditions);
    RenderSettings.skybox.SetColor(LevelInfo.id_FogColor, color2);
    RenderSettings.skybox.SetFloat(LevelInfo.id_ScatterIntensity, (float) ((double) this.skyAnimData.scattering.Evaluate(this.timeOfDay) * (double) t * (1.0 + (double) this.moon.intensity)));
    RenderSettings.skybox.SetVector(LevelInfo.id_SunDirection, new Vector4(this.sun.transform.forward.x, this.sun.transform.forward.y, this.sun.transform.forward.z, 0.0f));
    RenderSettings.skybox.SetFloat(LevelInfo.id_CloudOcclusion, num2 * num2);
    RenderSettings.skybox.SetFloat(LevelInfo.id_Altitude, num1 * 7E-06f);
    this.sun.color = this.skyAnimData.sunColor.Evaluate(this.timeOfDay / 24f) * this.skyAnimData.directSunlightIntensity.Evaluate(this.timeOfDay / 24f);
    this.sun.gameObject.SetActive((double) this.sun.color.r > 0.0 && (double) num2 < 0.89999997615814209);
    this.sunObject.SetActive(this.sun.gameObject.activeSelf);
    float num5 = 1f - Mathf.Abs(Vector3.Dot(this.sun.transform.forward, Vector3.up));
    this.sunMat.SetColor(LevelInfo.id_EmissionColor, this.sun.color * 1000000f * Mathf.Clamp01((float) (1.0 - (double) num2 * 2.0) - num5));
    if ((double) this.ambientIntensity > 0.029999999329447746)
    {
      this.bloom.threshold.value = 1.3f;
      this.bloom.intensity.value = 0.5f;
    }
    else
    {
      this.bloom.threshold.value = 0.3f;
      this.bloom.intensity.value = 3f;
    }
    if ((double) this.moonPhase <= -1.0)
      return;
    bool flag = (double) Vector3.Dot(this.moon.transform.position - currentCam.transform.position, Vector3.up) > 0.0;
    this.moon.intensity = (float) (0.5 * (double) Mathf.Clamp01(1f - num2) * (1.0 - (double) Mathf.Abs(this.moonPhase - 14f) / 14.0) * (flag ? 1.0 : 0.0));
    this.moonMat.SetVector("_SunVector", (Vector4) -this.sun.transform.forward);
  }

  public void UpdateFogDensity(float cloudFog)
  {
  }

  public bool TryGetTerrainColorAtCoordinate(GlobalPosition globalPosition, out Color sampledColor)
  {
    sampledColor = Color.clear;
    if ((UnityEngine.Object) this.currentMapSettings == (UnityEngine.Object) null)
      return false;
    sampledColor = this.currentMapSettings.GetTerrainColorAtCoordinate(globalPosition);
    return true;
  }

  public Color GetSunColor() => this.sunColor;

  public float GetDaylightFactor(Vector3 position)
  {
    float num1 = 1f;
    if ((double) this.timeOfDay > 18.0)
      num1 -= (float) (((double) this.timeOfDay - 18.0) * 2.0);
    else if ((double) this.timeOfDay < 6.0)
      num1 -= (float) ((6.0 - (double) this.timeOfDay) * 2.0);
    float num2 = Mathf.Clamp01(1f - this.GetCloudOcclusion(position));
    return Mathf.Clamp01(num1) * num2;
  }

  public float GetCloudOcclusion(Vector3 position)
  {
    return Mathf.InverseLerp(0.5f, 0.7f, this.conditions) * Mathf.Clamp01((float) (((double) this.cloudLayer.transform.position.y + 200.0 - (double) position.y) * 0.004999999888241291));
  }

  public void SetCookie(Texture2D texture, float scale, Vector2 windDisplacement)
  {
    this.sun.transform.rotation = Quaternion.LookRotation(this.sun.transform.forward, Vector3.up);
    this.sun.cookie = (Texture) texture;
    this.moon.cookie = (Texture) texture;
    this.SunURPLightData.lightCookieOffset = windDisplacement;
    this.SunURPLightData.lightCookieSize = new Vector2(scale, scale);
    this.MoonURPLightData.lightCookieOffset = windDisplacement;
    this.MoonURPLightData.lightCookieSize = new Vector2(scale, scale);
  }

  public void VisualizeWaypoints(List<GlobalPosition> waypoints, Unit unit)
  {
    foreach (UnityEngine.Object pathfindingVisualization in this.pathfindingVisualizations)
      UnityEngine.Object.Destroy(pathfindingVisualization);
    this.pathfindingVisualizations.Clear();
    float distance = 0.0f;
    for (int index = 0; index < waypoints.Count - 1; ++index)
    {
      GameObject gameObject1 = UnityEngine.Object.Instantiate<GameObject>(this.pathfindingSegmentVis, Datum.origin);
      gameObject1.transform.localPosition = waypoints[index].AsVector3();
      gameObject1.transform.rotation = Quaternion.LookRotation(waypoints[index + 1] - waypoints[index]);
      float num = FastMath.Distance(waypoints[index + 1], waypoints[index]);
      distance += num;
      gameObject1.transform.localScale = new Vector3(unit.maxRadius * 2f, 1f, num - 1f);
      this.pathfindingVisualizations.Add(gameObject1);
      if ((double) num < 0.10000000149011612)
      {
        GameObject gameObject2 = UnityEngine.Object.Instantiate<GameObject>(GameAssets.i.debugArrow, Datum.origin);
        gameObject2.transform.position = gameObject1.transform.position;
        gameObject2.transform.localScale = new Vector3(50f, 50f, 100f);
        gameObject2.transform.rotation = Quaternion.LookRotation(Vector3.up);
        this.pathfindingVisualizations.Add(gameObject2);
      }
    }
    Debug.Log((object) ("Road Distance: " + UnitConverter.DistanceReading(distance)));
  }

  public void SetTimeOfDay(float timeOfDay)
  {
    this.NetworktimeOfDay = timeOfDay;
    this.UpdateTimeOfDayLighting(true);
    this.UpdateSkybox(true);
    this.reflectionProbe.RenderProbe();
  }

  public void LoadFromMission(Mission mission)
  {
    this.LoadEnvironment(mission?.environment ?? new MissionEnvironment());
    this.SetStartingCamera(mission);
    if (mission == null || GameManager.gameState == GameState.Editor || mission.missionSettings.missionRoads.roads.Count <= 0)
      return;
    this.roadNetwork.roads.AddRange((IEnumerable<Road>) mission.missionSettings.missionRoads.roads);
    this.roadNetwork.RegenerateNetwork();
  }

  public void SetStartingCamera(Mission mission)
  {
    if (GameManager.IsPlayingFromEditor || !(SceneSingleton<CameraStateManager>.i.currentState is CameraFreeState))
      return;
    PositionRotation positionRotation = mission == null || !mission.missionSettings.cameraStartPosition.IsOverride ? this.currentMapSettings.CameraPositionRotation : mission.missionSettings.cameraStartPosition.Value;
    SceneSingleton<CameraStateManager>.i.SetCameraPosition(positionRotation);
    SceneSingleton<CameraStateManager>.i.CheckOriginShift();
  }

  public void LoadEnvironment(MissionEnvironment environment)
  {
    this.SetTimeOfDay(environment.timeOfDay);
    this.Networkconditions = environment.weatherIntensity;
    this.NetworkcloudHeight = environment.cloudAltitude;
    this.NetworkwindSpeed = environment.windSpeed;
    this.NetworkwindTurbulence = environment.windTurbulence;
    this.windMainHeading = environment.windHeading;
    this.SetWindHeading(this.windMainHeading);
    this.windZone.windPulseMagnitude = this.windTurbulence;
    this.windZone.windPulseFrequency = 0.2f;
    if ((double) this.windSpeed == 0.0)
      this.NetworkwindSpeed = 0.1f;
    this.NetworkwindVelocity = this.windZone.transform.forward * this.windSpeed;
    this.windZone.windMain = this.windVelocity.magnitude;
    float timeFactor = environment.timeFactor;
    if ((double) timeFactor <= 0.0)
    {
      if ((double) timeFactor != -2.0)
      {
        if ((double) timeFactor != -1.0)
        {
          if ((double) timeFactor == 0.0)
          {
            this.timeFactor = 1f;
            goto label_16;
          }
        }
        else
        {
          this.timeFactor = 0.5f;
          goto label_16;
        }
      }
      else
      {
        this.timeFactor = 0.0f;
        goto label_16;
      }
    }
    else if ((double) timeFactor != 1.0)
    {
      if ((double) timeFactor != 2.0)
      {
        if ((double) timeFactor == 3.0)
        {
          this.timeFactor = 60f;
          goto label_16;
        }
      }
      else
      {
        this.timeFactor = 30f;
        goto label_16;
      }
    }
    else
    {
      this.timeFactor = 10f;
      goto label_16;
    }
    this.timeFactor = 1f;
label_16:
    this.windRandomArc = environment.windRandomHeading;
    this.NetworkmoonPhase = environment.moonPhase;
    this.SetMoonPhase(this.moonPhase);
  }

  public void SetMoonPhase(float value)
  {
    this.moonAxis.transform.localEulerAngles = new Vector3(-5f, 0.0f, (float) (180.0 - (double) value * 12.859999656677246));
    if (GameManager.gameState != GameState.Editor)
      return;
    this.UpdateTimeOfDayLighting(true);
  }

  public void SetWindHeading(float heading)
  {
    this.windZone.transform.localEulerAngles = new Vector3(0.0f, heading, 0.0f);
  }

  public float GetWindHeading() => this.windZone.transform.localEulerAngles.y;

  public void UpdateSkybox(bool forceImmediate)
  {
    if (!forceImmediate && (double) Time.realtimeSinceStartup - (double) this.skyLastUpdated < 1.0)
      return;
    this.skyLastUpdated = Time.realtimeSinceStartup;
  }

  private void UpdateWind()
  {
    this.windMainHeading += UnityEngine.Random.Range(-this.windRandomArc, this.windRandomArc);
  }

  private void UpdateWaterPlane(Camera mainCamera)
  {
    if ((UnityEngine.Object) this.waterPlane == (UnityEngine.Object) null)
      return;
    Vector3 vector3 = Datum.origin.position - mainCamera.transform.position;
    this.waterMaterial.SetVector(LevelInfo.id_OriginOffset, (Vector4) new Vector2(vector3.x, vector3.z));
    this.waterPlane.transform.position = new Vector3(mainCamera.transform.position.x, Datum.LocalSeaY, mainCamera.transform.position.z);
  }

  public void UpdateReflectionProbe(bool forceImmediate)
  {
    if (!forceImmediate && (double) Time.realtimeSinceStartup - (double) this.reflectionLastUpdated < 5.0)
      return;
    this.reflectionLastUpdated = Time.realtimeSinceStartup;
    if ((double) this.GetCurrentCam().transform.position.y > (double) Datum.LocalSeaY && !ExposureController.BrightLightsExist())
      this.reflectionProbe.RenderProbe();
    foreach (Material scatteringParticle in this.lightScatteringParticles)
      scatteringParticle.SetColor(LevelInfo.id_EmissionColor, Color.white * 2f * this.GetAmbientLight());
  }

  private void DaylightCheck()
  {
    if ((double) Time.realtimeSinceStartup - (double) this.daylightLastUpdate <= (double) this.daylightUpdateRate)
      return;
    this.daylightLastUpdate = Time.realtimeSinceStartup;
    bool flag = ((double) this.timeOfDay < 12.0 ? (double) this.timeOfDay - 6.0 : 18.0 - (double) this.timeOfDay) - (double) this.conditions * 3.0 > 0.0;
    if (flag == this.isDayLight)
      return;
    this.isDayLight = flag;
    Action onDaylightChange = this.onDaylightChange;
    if (onDaylightChange == null)
      return;
    onDaylightChange();
  }

  private void Update()
  {
    this.windOffset = 10f * Time.timeSinceLevelLoad * Vector3.one;
    if (!GameManager.ShowEffects)
      return;
    Camera main = Camera.main;
    int num = (UnityEngine.Object) main != (UnityEngine.Object) null ? 1 : 0;
    Vector3 position = num != 0 ? main.transform.position : new Vector3(0.0f, Datum.LocalSeaY, 0.0f);
    GlobalPosition globalPosition = position.ToGlobalPosition();
    Material skybox = RenderSettings.skybox;
    if ((UnityEngine.Object) skybox != (UnityEngine.Object) null)
      skybox.SetVector(LevelInfo.id_CameraPosition, new Vector4(globalPosition.x, globalPosition.y, globalPosition.z, 0.0f));
    this.UpdateSkybox(false);
    if (num != 0)
      this.UpdateWaterPlane(main);
    this.starfield.transform.position = position;
    if ((double) this.moonPhase >= 0.0)
    {
      this.NetworkmoonPhase = this.moonPhase + (float) ((double) this.timeFactor * (double) Time.deltaTime * 1.1574074051168282E-05);
      if ((double) this.moonPhase > 28.0)
        this.NetworkmoonPhase = this.moonPhase - 28f;
      this.SetMoonPhase(this.moonPhase);
    }
    this.UpdateTimeOfDayLighting(false);
    this.UpdateReflectionProbe(false);
    this.DaylightCheck();
    if (!NetworkManagerNuclearOption.i.Server.Active)
      return;
    this.NetworktimeOfDay = this.timeOfDay + (float) ((double) this.timeFactor * (double) Time.deltaTime * 0.00027777778450399637);
    if ((double) this.timeOfDay > 24.0)
      this.NetworktimeOfDay = this.timeOfDay - 24f;
    if ((double) this.windRandomArc <= 0.0)
      return;
    if ((double) Time.realtimeSinceStartup > (double) this.lastWindChange + (double) this.windChangeDelay)
    {
      this.lastWindChange = Time.realtimeSinceStartup;
      this.UpdateWind();
    }
    this.SetWindHeading(Mathf.LerpAngle(this.GetWindHeading(), this.windMainHeading, Time.deltaTime));
  }

  private void MirageProcessed()
  {
  }

  public float NetworktimeOfDay
  {
    get => this.timeOfDay;
    set
    {
      if (this.SyncVarEqual<float>(value, this.timeOfDay))
        return;
      float timeOfDay = this.timeOfDay;
      this.timeOfDay = value;
      this.SetDirtyBit(1UL);
    }
  }

  public float Networkconditions
  {
    get => this.conditions;
    set
    {
      if (this.SyncVarEqual<float>(value, this.conditions))
        return;
      float conditions = this.conditions;
      this.conditions = value;
      this.SetDirtyBit(2UL);
    }
  }

  public float NetworkcloudHeight
  {
    get => this.cloudHeight;
    set
    {
      if (this.SyncVarEqual<float>(value, this.cloudHeight))
        return;
      float cloudHeight = this.cloudHeight;
      this.cloudHeight = value;
      this.SetDirtyBit(4UL);
    }
  }

  public Vector3 NetworkwindVelocity
  {
    get => this.windVelocity;
    set
    {
      if (this.SyncVarEqual<Vector3>(value, this.windVelocity))
        return;
      Vector3 windVelocity = this.windVelocity;
      this.windVelocity = value;
      this.SetDirtyBit(8UL);
    }
  }

  public float NetworkwindTurbulence
  {
    get => this.windTurbulence;
    set
    {
      if (this.SyncVarEqual<float>(value, this.windTurbulence))
        return;
      float windTurbulence = this.windTurbulence;
      this.windTurbulence = value;
      this.SetDirtyBit(16UL /*0x10*/);
    }
  }

  public float NetworkwindSpeed
  {
    get => this.windSpeed;
    set
    {
      if (this.SyncVarEqual<float>(value, this.windSpeed))
        return;
      float windSpeed = this.windSpeed;
      this.windSpeed = value;
      this.SetDirtyBit(32UL /*0x20*/);
    }
  }

  public float NetworkmoonPhase
  {
    get => this.moonPhase;
    set
    {
      if (this.SyncVarEqual<float>(value, this.moonPhase))
        return;
      float moonPhase = this.moonPhase;
      this.moonPhase = value;
      this.SetDirtyBit(64UL /*0x40*/);
    }
  }

  public override bool SerializeSyncVars(NetworkWriter writer, bool initialize)
  {
    ulong syncVarDirtyBits = this.SyncVarDirtyBits;
    bool flag = base.SerializeSyncVars(writer, initialize);
    if (initialize)
    {
      writer.WriteSingleConverter(this.timeOfDay);
      writer.WriteSingleConverter(this.conditions);
      writer.WriteSingleConverter(this.cloudHeight);
      writer.WriteVector3(this.windVelocity);
      writer.WriteSingleConverter(this.windTurbulence);
      writer.WriteSingleConverter(this.windSpeed);
      writer.WriteSingleConverter(this.moonPhase);
      return true;
    }
    writer.Write(syncVarDirtyBits, 7);
    if (((long) syncVarDirtyBits & 1L) != 0L)
    {
      writer.WriteSingleConverter(this.timeOfDay);
      flag = true;
    }
    if (((long) syncVarDirtyBits & 2L) != 0L)
    {
      writer.WriteSingleConverter(this.conditions);
      flag = true;
    }
    if (((long) syncVarDirtyBits & 4L) != 0L)
    {
      writer.WriteSingleConverter(this.cloudHeight);
      flag = true;
    }
    if (((long) syncVarDirtyBits & 8L) != 0L)
    {
      writer.WriteVector3(this.windVelocity);
      flag = true;
    }
    if (((long) syncVarDirtyBits & 16L /*0x10*/) != 0L)
    {
      writer.WriteSingleConverter(this.windTurbulence);
      flag = true;
    }
    if (((long) syncVarDirtyBits & 32L /*0x20*/) != 0L)
    {
      writer.WriteSingleConverter(this.windSpeed);
      flag = true;
    }
    if (((long) syncVarDirtyBits & 64L /*0x40*/) != 0L)
    {
      writer.WriteSingleConverter(this.moonPhase);
      flag = true;
    }
    return flag;
  }

  public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
  {
    base.DeserializeSyncVars(reader, initialState);
    if (initialState)
    {
      this.timeOfDay = reader.ReadSingleConverter();
      this.conditions = reader.ReadSingleConverter();
      this.cloudHeight = reader.ReadSingleConverter();
      this.windVelocity = reader.ReadVector3();
      this.windTurbulence = reader.ReadSingleConverter();
      this.windSpeed = reader.ReadSingleConverter();
      this.moonPhase = reader.ReadSingleConverter();
    }
    else
    {
      ulong dirtyBit = reader.Read(7);
      this.SetDeserializeMask(dirtyBit, 0);
      if (((long) dirtyBit & 1L) != 0L)
        this.timeOfDay = reader.ReadSingleConverter();
      if (((long) dirtyBit & 2L) != 0L)
        this.conditions = reader.ReadSingleConverter();
      if (((long) dirtyBit & 4L) != 0L)
        this.cloudHeight = reader.ReadSingleConverter();
      if (((long) dirtyBit & 8L) != 0L)
        this.windVelocity = reader.ReadVector3();
      if (((long) dirtyBit & 16L /*0x10*/) != 0L)
        this.windTurbulence = reader.ReadSingleConverter();
      if (((long) dirtyBit & 32L /*0x20*/) != 0L)
        this.windSpeed = reader.ReadSingleConverter();
      if (((long) dirtyBit & 64L /*0x40*/) == 0L)
        return;
      this.moonPhase = reader.ReadSingleConverter();
    }
  }

  protected override int GetRpcCount() => 0;

  [Serializable]
  public class SkyAnimData
  {
    public AnimationCurve ambientIntensity;
    public AnimationCurve directSunlightIntensity;
    public AnimationCurve scattering;
    public Gradient sunColor;
    public Gradient fogColorGround;
    public Gradient fogColorAltitude;
    public float hazeAmount;
  }
}

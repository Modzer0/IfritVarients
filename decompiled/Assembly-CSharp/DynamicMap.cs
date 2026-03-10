// Decompiled with JetBrains decompiler
// Type: DynamicMap
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Mirage;
using NuclearOption;
using NuclearOption.Networking;
using Rewired;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

#nullable disable
public class DynamicMap : SceneSingleton<DynamicMap>
{
  public FactionHQ HQ;
  public GameObject viewIndicator;
  public GameObject mapImage;
  public Image mapBackground;
  public float mapScaleMinimized;
  public float mapScaleMaximized;
  public float mapScaleCurrent;
  public float mapLastUpdated;
  private Vector2 mapDimensions;
  public Transform mapScaleCenter;
  public Transform mapScaleProxy;
  public RectTransform mapTransform;
  public GameObject hudMapAnchor;
  private RectTransform mapRectTransform;
  private RectTransform backgroundRectTransform;
  public Vector2 mapCenter = Vector2.zero;
  private Vector2 positionOffset;
  private Vector2 stationaryOffset;
  private bool followingCamera;
  public GameObject iconLayer;
  public GameObject infoLayer;
  [SerializeField]
  private GameObject airbaseLayer;
  [SerializeField]
  private GameObject radarVisPrefab;
  [SerializeField]
  private GameObject notchLinePrefab;
  [SerializeField]
  private MapToolTip toolTip;
  private MapIcon infoTextTarget;
  private List<DynamicMap.RadarMapVis> radarVisualizations = new List<DynamicMap.RadarMapVis>();
  [SerializeField]
  private Canvas mapCanvas;
  public UnitMapIcon unitMapIconPrefab;
  public AirbaseMapIcon airbaseMapIconPrefab;
  public GameObject targetMarker;
  public GameObject jammedMarker;
  private Dictionary<Unit, UnitMapIcon> iconLookup = new Dictionary<Unit, UnitMapIcon>();
  private Dictionary<Airbase, AirbaseMapIcon> airbaseIconLookup = new Dictionary<Airbase, AirbaseMapIcon>();
  [SerializeField]
  private Color backgroundMinimized;
  [SerializeField]
  private Color backgroundMaximized;
  [SerializeField]
  private Transform topRight;
  [SerializeField]
  private Transform bottomLeft;
  public List<MapIcon> selectedIcons = new List<MapIcon>();
  public GameObject mapWaypoint;
  public GameObject mapWaypointVector;
  public List<MapWaypoint> waypoints = new List<MapWaypoint>();
  public List<GlobalPosition> constructWaypoints = new List<GlobalPosition>();
  private Rewired.Player player;
  private float clickTime;
  private int iconIndex;
  public float mapDimension;
  public float mapDisplayFactor;
  private List<PersistentID> queuedIcons = new List<PersistentID>();
  public GridLabels gridLabels;
  private bool isJumping;
  private Vector3 mapTarget;
  [SerializeField]
  private ObjectiveMarkerManager objectiveMarkerManager;
  [Tooltip("When jumping on the map, how fast should the map follow the click")]
  [SerializeField]
  private float mapMoveMaxJumpSpeed = 10f;
  [SerializeField]
  private float jumpCameraFocusDistance = 300f;
  [SerializeField]
  private float jumpCameraFocusHeight = 300f;
  [SerializeField]
  private float jumpCameraFocusAngleDown = 30f;

  public static bool AllowedToOpen { get; set; }

  public List<MapIcon> mapIcons { get; private set; } = new List<MapIcon>();

  public List<MapMarker> mapMarkers { get; private set; } = new List<MapMarker>();

  public static bool mapMaximized { get; private set; }

  public static event Action onMapChanged;

  public event Action onMapGenerated;

  public event Action onMapMaximized;

  public event Action onMapMinimized;

  public event Action onAllDeselected;

  public event Action<Unit> onUnitSelected;

  public event Action<Unit> onUnitDeselected;

  public static event Action onShowTypesChanged;

  public static void EnableCanvas(bool enable)
  {
    SceneSingleton<DynamicMap>.i.mapCanvas.gameObject.SetActive(enable);
  }

  public static void LoadMapImage(MapSettings mapSettings)
  {
    if ((UnityEngine.Object) SceneSingleton<DynamicMap>.i == (UnityEngine.Object) null)
      return;
    Image component = SceneSingleton<DynamicMap>.i.mapImage.GetComponent<Image>();
    SceneSingleton<DynamicMap>.i.mapImage.GetComponent<RectTransform>().sizeDelta = Vector2.one * (mapSettings.MapSize / 81920f) * 900f;
    Sprite mapImage = mapSettings.MapImage;
    component.sprite = mapImage;
    SceneSingleton<DynamicMap>.i.mapDimensions = mapSettings.MapSize;
    SceneSingleton<DynamicMap>.i.gridLabels.SetupGrid(mapSettings.GridSizeX, mapSettings.GridSizeY, mapSettings.OffsetX, mapSettings.OffsetY);
  }

  public static void ClampToMapEdge(Transform clampedTransform)
  {
    UnitMapIcon mapIcon;
    if ((UnityEngine.Object) SceneSingleton<CombatHUD>.i.aircraft == (UnityEngine.Object) null || !DynamicMap.TryGetMapIcon((Unit) SceneSingleton<CombatHUD>.i.aircraft, out mapIcon))
      return;
    Transform transform = mapIcon.transform;
    float num1 = SceneSingleton<DynamicMap>.i.mapScaleCurrent * 0.48f * (float) (Screen.height / 1080);
    Vector3 vector3_1 = clampedTransform.position - SceneSingleton<DynamicMap>.i.mapBackground.transform.position;
    Vector3 vector3_2 = clampedTransform.position - transform.position;
    Vector3 vector3_3 = transform.position - SceneSingleton<DynamicMap>.i.mapBackground.transform.position;
    float num2 = num1 + vector3_3.x;
    float x = num1 - vector3_3.x;
    float y = num1 - vector3_3.y;
    float num3 = num1 + vector3_3.y;
    if ((double) Mathf.Abs(vector3_1.x) < (double) num1 && (double) Mathf.Abs(vector3_1.y) < (double) num1)
      return;
    float num4 = (double) Mathf.Abs(vector3_2.x) > 0.0 ? vector3_2.y / vector3_2.x : 1000000f;
    if ((double) num4 == 0.0)
      num4 = 1E-06f;
    vector3_2 = (double) vector3_2.x < 0.0 ? ((double) num4 >= (double) num3 / (double) num2 || (double) num4 <= -(double) y / (double) num2 ? ((double) vector3_2.y > 0.0 ? new Vector3(y / num4, y, 0.0f) : new Vector3((float) -((double) num3 / (double) num4), -num3, 0.0f)) : new Vector3(-num2, -num2 * num4, 0.0f)) : ((double) num4 >= (double) y / (double) x || (double) num4 <= -((double) num3 / (double) x) ? ((double) vector3_2.y > 0.0 ? new Vector3(y / num4, y, 0.0f) : new Vector3((float) -((double) num3 / (double) num4), -num3, 0.0f)) : new Vector3(x, x * num4, 0.0f));
    clampedTransform.position = transform.position + vector3_2;
  }

  public static bool TryGetMapIcon(Unit unit, out UnitMapIcon mapIcon)
  {
    return SceneSingleton<DynamicMap>.i.iconLookup.TryGetValue(unit, out mapIcon);
  }

  protected override void Awake()
  {
    base.Awake();
    DynamicMap.AllowedToOpen = true;
    this.player = ReInput.players.GetPlayer(0);
    DynamicMap.EnableCanvas(false);
    DynamicMap.mapMaximized = false;
    this.mapRectTransform = this.GetComponent<RectTransform>();
    this.backgroundRectTransform = this.mapBackground.GetComponent<RectTransform>();
    this.mapBackground.color = this.backgroundMaximized;
    this.HideTooltip();
  }

  private void OnEnable()
  {
    NetworkManagerNuclearOption.i.Client.Disconnected.AddListener(new UnityAction<ClientStoppedReason>(this.DynamicMap_OnStopClient));
  }

  public GameObject ShowNotchLine()
  {
    return !DynamicMap.TryGetMapIcon((Unit) SceneSingleton<CombatHUD>.i.aircraft, out UnitMapIcon _) ? (GameObject) null : UnityEngine.Object.Instantiate<GameObject>(this.notchLinePrefab, this.iconLayer.transform);
  }

  public void UnselectAll()
  {
    this.selectedIcons.Clear();
    this.ClearWaypoints();
    foreach (MapIcon mapIcon in this.mapIcons)
    {
      if ((UnityEngine.Object) mapIcon != (UnityEngine.Object) null)
        mapIcon.DeselectIcon();
    }
    foreach (KeyValuePair<Airbase, AirbaseMapIcon> keyValuePair in this.airbaseIconLookup)
    {
      if ((UnityEngine.Object) keyValuePair.Value != (UnityEngine.Object) null)
        keyValuePair.Value.DeselectIcon();
    }
    Action onAllDeselected = this.onAllDeselected;
    if (onAllDeselected == null)
      return;
    onAllDeselected();
  }

  public void Maximize()
  {
    if (!DynamicMap.AllowedToOpen)
      return;
    DynamicMap.EnableCanvas(true);
    if (!DynamicMap.mapMaximized)
    {
      this.followingCamera = true;
      this.positionOffset = Vector2.zero;
      Vector3 vector3 = SceneSingleton<CameraStateManager>.i.transform.position.ToGlobalPosition().AsVector3() * this.mapDisplayFactor;
      this.stationaryOffset = new Vector2(vector3.x, vector3.z);
      this.transform.SetParent(SceneSingleton<GameplayUI>.i.transform);
      this.mapCanvas.transform.localScale = Vector3.one;
      this.transform.position = SceneSingleton<GameplayUI>.i.transform.position;
      this.mapRectTransform.sizeDelta = Vector2.one * this.mapScaleMaximized;
      this.backgroundRectTransform.sizeDelta = this.mapRectTransform.sizeDelta + new Vector2(20f, 20f);
      DynamicMap.mapMaximized = true;
      this.mapBackground.enabled = true;
      this.mapImage.transform.SetParent(this.mapBackground.transform);
      if (SceneSingleton<CameraStateManager>.i.currentState == SceneSingleton<CameraStateManager>.i.cockpitState)
        FlightHud.EnableCanvas(false);
      if (this.ShouldShowAirbase())
        this.ShowAirbases();
      else
        this.HideAirbases();
      CursorManager.SetFlag(CursorFlags.Map, true);
      if (GameManager.gameState != GameState.Editor)
      {
        SceneSingleton<GameplayUI>.i.ShowSpectatorPanel();
        if (GameManager.GetLocalHQ(out FactionHQ _))
          SceneSingleton<GameplayUI>.i.ShowSelectAirbase();
      }
      if (SceneSingleton<MapOptions>.i.showGridLabels && !this.gridLabels.LabelShown)
        this.gridLabels.Maximize(true);
      Action onMapMaximized = this.onMapMaximized;
      if (onMapMaximized != null)
        onMapMaximized();
    }
    this.mapScaleCurrent = this.mapScaleMaximized;
    this.mapDisplayFactor = this.mapScaleMaximized / this.mapDimension;
    this.CenterMinimizedMap();
  }

  private bool ShouldShowAirbase()
  {
    NuclearOption.Networking.Player localPlayer;
    if (!GameManager.GetLocalPlayer<NuclearOption.Networking.Player>(out localPlayer))
      return false;
    Aircraft aircraft = localPlayer.Aircraft;
    return (!((UnityEngine.Object) aircraft != (UnityEngine.Object) null) || aircraft.disabled) && !localPlayer.AircraftSpawnPending;
  }

  public void Minimize()
  {
    if (DynamicMap.mapMaximized)
    {
      this.ClearWaypoints();
      SceneSingleton<GameplayUI>.i.HideSpectatorPanel();
      this.transform.SetParent(this.hudMapAnchor.transform);
      this.mapCanvas.transform.localScale = Vector3.one;
      this.transform.localPosition = Vector3.zero;
      this.mapImage.transform.SetParent(this.mapScaleCenter.transform);
      this.mapScaleCenter.transform.localScale = new Vector3(2f, 2f, 2f);
      this.mapImage.transform.SetParent(this.mapBackground.transform);
      this.mapRectTransform.sizeDelta = Vector2.one * this.mapScaleMinimized;
      this.backgroundRectTransform.sizeDelta = this.mapRectTransform.sizeDelta + new Vector2(20f, 20f);
      this.mapBackground.enabled = false;
      DynamicMap.mapMaximized = false;
      this.HideTooltip();
      if (SceneSingleton<CameraStateManager>.i.currentState == SceneSingleton<CameraStateManager>.i.cockpitState)
        FlightHud.EnableCanvas(true);
      CursorManager.SetFlag(CursorFlags.Map, false);
      if (this.gridLabels.LabelShown)
        this.gridLabels.Maximize(false);
      Action onMapMinimized = this.onMapMinimized;
      if (onMapMinimized != null)
        onMapMinimized();
    }
    SceneSingleton<GameplayUI>.i.HideSelectAirbase();
    DynamicMap.EnableCanvas((UnityEngine.Object) SceneSingleton<CombatHUD>.i.aircraft != (UnityEngine.Object) null && CameraStateManager.cameraMode == CameraMode.cockpit);
    this.mapBackground.color = this.backgroundMinimized;
    this.mapScaleCurrent = this.mapScaleMinimized;
    this.mapDisplayFactor = this.mapScaleMaximized / this.mapDimension;
  }

  private void OnDestroy()
  {
    DynamicMap.mapMaximized = false;
    CursorManager.SetFlag(CursorFlags.Map, false);
  }

  public void CenterMap()
  {
    Vector3 vector3 = SceneSingleton<CameraStateManager>.i.transform.GlobalPosition().AsVector3() * this.mapDisplayFactor;
    this.mapImage.transform.localPosition = (this.mapImage.transform.right * -vector3.x + this.mapImage.transform.up * -vector3.z) * this.mapImage.transform.localScale.x * this.mapBackground.transform.localScale.x;
  }

  public void ClearMap()
  {
    this.UnselectAll();
    foreach (MapIcon mapIcon in this.mapIcons)
      mapIcon.RemoveIcon();
    foreach (KeyValuePair<Airbase, AirbaseMapIcon> keyValuePair in this.airbaseIconLookup)
      keyValuePair.Value.RemoveIcon();
    foreach (MapMarker mapMarker in this.mapMarkers)
      mapMarker.Remove();
    this.mapIcons.Clear();
    this.airbaseIconLookup.Clear();
    this.iconLookup.Clear();
    this.queuedIcons.Clear();
    this.mapMarkers.Clear();
  }

  public void GenerateMap(FactionHQ HQ, bool showAirbases)
  {
    if ((UnityEngine.Object) HQ == (UnityEngine.Object) null)
    {
      foreach (Unit allUnit in UnitRegistry.allUnits)
        this.AddIcon(allUnit.persistentID);
    }
    else
    {
      foreach (KeyValuePair<PersistentID, TrackingInfo> keyValuePair in HQ.trackingDatabase)
        this.AddIcon(keyValuePair.Key);
      foreach (PersistentID factionUnit in HQ.factionUnits)
        this.AddIcon(factionUnit);
      if (!showAirbases)
        return;
      foreach (Airbase airbase in HQ.GetAirbases())
        this.AddIcon(airbase);
    }
  }

  public void RefreshAirbases()
  {
    foreach (KeyValuePair<Airbase, AirbaseMapIcon> keyValuePair in this.airbaseIconLookup)
      UnityEngine.Object.Destroy((UnityEngine.Object) keyValuePair.Value.gameObject);
    this.airbaseIconLookup.Clear();
    if ((UnityEngine.Object) this.HQ == (UnityEngine.Object) null)
      return;
    foreach (Airbase airbase in this.HQ.GetAirbases())
      this.AddIcon(airbase);
  }

  public void ShowAirbases()
  {
    foreach (KeyValuePair<Airbase, AirbaseMapIcon> keyValuePair in this.airbaseIconLookup)
      keyValuePair.Value.iconImage.enabled = true;
  }

  public void HideAirbases()
  {
    foreach (KeyValuePair<Airbase, AirbaseMapIcon> keyValuePair in this.airbaseIconLookup)
      keyValuePair.Value.iconImage.enabled = false;
  }

  public void ShowRadarPing(Aircraft.OnRadarWarning source)
  {
    Image component = UnityEngine.Object.Instantiate<GameObject>(this.radarVisPrefab, this.iconLayer.transform).GetComponent<Image>();
    Color color1 = new Color(1f, 0.0f, 0.0f, 0.5f);
    Color color2 = new Color(1f, 1f, 0.0f, 0.25f);
    Color color3 = new Color(1f, 1f, 1f, 0.125f);
    Color color4 = source.detected ? color2 : color3;
    if (source.isTarget)
      color4 = color1;
    component.color = color4;
    this.radarVisualizations.Add(new DynamicMap.RadarMapVis(component, source));
  }

  public GlobalPosition GetCursorCoordinates()
  {
    Vector3 vector3 = (Input.mousePosition - this.mapImage.transform.position) * (this.mapDimension / (900f * this.mapImage.transform.lossyScale.x));
    return new GlobalPosition(vector3.x, 0.0f, vector3.y);
  }

  public bool TryGetCursorCoordinates(out GlobalPosition position)
  {
    bool cursorCoordinates = RectTransformUtility.RectangleContainsScreenPoint(this.mapBackground.rectTransform, (Vector2) Input.mousePosition, (Camera) null);
    position = cursorCoordinates ? this.GetCursorCoordinates() : new GlobalPosition();
    return cursorCoordinates;
  }

  public void SetFaction(FactionHQ HQ)
  {
    Action onMapGenerated = this.onMapGenerated;
    if (onMapGenerated != null)
      onMapGenerated();
    if ((UnityEngine.Object) this.HQ == (UnityEngine.Object) HQ)
      return;
    this.HQ = HQ;
    this.ClearMap();
    if ((UnityEngine.Object) HQ != (UnityEngine.Object) null)
    {
      this.GenerateMap(HQ, true);
      this.objectiveMarkerManager.Initialize(this.iconLayer.transform);
    }
    else
    {
      foreach (FactionHQ allHq in FactionRegistry.GetAllHQs())
        this.GenerateMap(allHq, false);
      this.GenerateMap((FactionHQ) null, false);
    }
  }

  private void DynamicMap_OnFactionChanged(Unit unit)
  {
    UnitMapIcon unitMapIcon;
    if (!this.iconLookup.TryGetValue(unit, out unitMapIcon))
      return;
    unitMapIcon.UpdateColor();
  }

  public void UpdateUnitFaction(Unit unit)
  {
    UnitMapIcon unitMapIcon;
    if (!this.iconLookup.TryGetValue(unit, out unitMapIcon))
      return;
    unitMapIcon.UpdateColor();
  }

  public float MetersToPixels() => this.mapTransform.rect.width / this.mapDimension;

  public void FlagIncomingMissile(Unit missileUnit)
  {
    this.GetOrAddIcon(missileUnit).SetMissileWarning();
  }

  public void ClearIncomingMissile(Unit missileUnit)
  {
    if (!((UnityEngine.Object) missileUnit != (UnityEngine.Object) null) || !this.iconLookup.ContainsKey(missileUnit))
      return;
    this.iconLookup[missileUnit].ClearMissileWarning();
  }

  public UnitMapIcon GetOrAddIcon(Unit unit)
  {
    UnitMapIcon orAddIcon;
    if (this.iconLookup.TryGetValue(unit, out orAddIcon))
      return orAddIcon;
    this.AddIcon(unit.persistentID);
    return this.iconLookup[unit];
  }

  public void AddIcon(PersistentID id)
  {
    this.queuedIcons.Add(id);
    this.SpawnQueuedIcons();
  }

  public void AddIcon(Airbase airbase)
  {
    AirbaseMapIcon airbaseMapIcon = UnityEngine.Object.Instantiate<AirbaseMapIcon>(this.airbaseMapIconPrefab);
    airbaseMapIcon.transform.SetParent(this.airbaseLayer.transform);
    airbaseMapIcon.SetIcon(airbase);
    this.airbaseIconLookup.Add(airbase, airbaseMapIcon);
  }

  public void SelectIcon(Unit unit)
  {
    if (!this.iconLookup.ContainsKey(unit) || this.selectedIcons.Contains((MapIcon) this.iconLookup[unit]) || SceneSingleton<TargetListSelector>.i.CheckExclusions(unit))
      return;
    this.iconLookup[unit].SelectIcon();
    this.selectedIcons.Add((MapIcon) this.iconLookup[unit]);
    Action<Unit> onUnitSelected = this.onUnitSelected;
    if (onUnitSelected == null)
      return;
    onUnitSelected(unit);
  }

  public void DeselectIcon(Unit unit)
  {
    if (!this.iconLookup.ContainsKey(unit))
      return;
    this.iconLookup[unit].DeselectIcon();
    this.selectedIcons.Remove((MapIcon) this.iconLookup[unit]);
    Action<Unit> onUnitDeselected = this.onUnitDeselected;
    if (onUnitDeselected == null)
      return;
    onUnitDeselected(unit);
  }

  public void SelectIcon(Airbase airbase)
  {
    AirbaseMapIcon airbaseMapIcon;
    if (!this.airbaseIconLookup.TryGetValue(airbase, out airbaseMapIcon))
      return;
    airbaseMapIcon.SelectIcon();
    this.selectedIcons.Add((MapIcon) airbaseMapIcon);
  }

  public void DeselectAllIcons()
  {
    foreach (KeyValuePair<Unit, UnitMapIcon> keyValuePair in this.iconLookup)
      keyValuePair.Value.DeselectIcon();
    foreach (KeyValuePair<Airbase, AirbaseMapIcon> keyValuePair in this.airbaseIconLookup)
      keyValuePair.Value.DeselectIcon();
    foreach (MapMarker mapMarker in this.mapMarkers)
    {
      if (mapMarker is TargetMarker)
        mapMarker.Remove();
    }
    this.selectedIcons.Clear();
  }

  public void HighlightIcon(Unit unit)
  {
    if (!this.iconLookup.ContainsKey(unit))
      return;
    this.iconLookup[unit].HighlightIcon();
  }

  public void RemoveIcon(MapIcon mapIcon)
  {
    if (this.selectedIcons.Contains(mapIcon))
      this.selectedIcons.Remove(mapIcon);
    this.mapIcons.Remove(mapIcon);
    if (!(mapIcon is UnitMapIcon unitMapIcon))
      return;
    this.iconLookup.Remove(unitMapIcon.unit);
  }

  public void DisplayExclusionZone(ExclusionZone exclusionZone)
  {
    GameObject icon = UnityEngine.Object.Instantiate<GameObject>(GameAssets.i.exclusionZoneDisplay, this.iconLayer.transform);
    icon.transform.localPosition = new Vector3(exclusionZone.position.x, exclusionZone.position.z, 0.0f) * SceneSingleton<DynamicMap>.i.mapDisplayFactor;
    icon.transform.localScale = Vector3.one * (exclusionZone.radius * SceneSingleton<DynamicMap>.i.mapDisplayFactor);
    Unit unit;
    if (UnitRegistry.TryGetUnit(new PersistentID?(exclusionZone.sourceId), out unit))
      unit.onDisableUnit += (Action<Unit>) (_ => UnityEngine.Object.Destroy((UnityEngine.Object) icon));
    else
      UnityEngine.Object.Destroy((UnityEngine.Object) icon, 30f);
  }

  private void SpawnQueuedIcons()
  {
    for (int index = this.queuedIcons.Count - 1; index >= 0; --index)
    {
      Unit unit;
      if (UnitRegistry.TryGetUnit(new PersistentID?(this.queuedIcons[index]), out unit))
      {
        if (!unit.disabled && (double) unit.definition.mapIconSize > 0.0 && !this.iconLookup.ContainsKey(unit))
          this.SpawnIconForUnit(unit);
        this.queuedIcons.RemoveAt(index);
      }
    }
  }

  private UnitMapIcon SpawnIconForUnit(Unit unit)
  {
    UnitMapIcon unitMapIcon = UnityEngine.Object.Instantiate<UnitMapIcon>(this.unitMapIconPrefab);
    unitMapIcon.transform.SetParent(this.iconLayer.transform);
    unitMapIcon.transform.localScale = Vector3.one;
    unitMapIcon.SetIcon(unit);
    this.mapIcons.Add((MapIcon) unitMapIcon);
    this.iconLookup.Add(unit, unitMapIcon);
    SceneSingleton<CombatHUD>.i.CreateMarker(unit.persistentID);
    return unitMapIcon;
  }

  private void DynamicMap_OnStopClient(ClientStoppedReason _)
  {
    NetworkManagerNuclearOption.i.Client.Disconnected.RemoveListener(new UnityAction<ClientStoppedReason>(this.DynamicMap_OnStopClient));
    if (!((UnityEngine.Object) this != (UnityEngine.Object) null))
      return;
    this.enabled = false;
  }

  public static void ClearJammingEffects()
  {
    SceneSingleton<CombatHUD>.i.jamAccumulation = 0.0f;
    foreach (MapIcon mapIcon in SceneSingleton<DynamicMap>.i.mapIcons)
    {
      if (mapIcon is UnitMapIcon unitMapIcon)
        unitMapIcon.ClearJammingDistortion();
    }
  }

  private void UpdateIcons()
  {
    float mapInverseScale = 1f / this.mapImage.transform.localScale.x;
    int num = Mathf.Min(this.iconIndex + Mathf.Max(1, (int) ((double) this.mapIcons.Count * 0.20000000298023224)), this.mapIcons.Count);
    for (int iconIndex = this.iconIndex; iconIndex < num; ++iconIndex)
      this.mapIcons[iconIndex].UpdateIcon(this.mapDisplayFactor, mapInverseScale, this.mapImage.transform, DynamicMap.mapMaximized);
    this.iconIndex = num < this.mapIcons.Count ? num : 0;
    if (!((UnityEngine.Object) SceneSingleton<CombatHUD>.i.aircraft != (UnityEngine.Object) null) || SceneSingleton<CombatHUD>.i.aircraft.disabled || (double) SceneSingleton<CombatHUD>.i.jamAccumulation <= 0.0)
      return;
    float jamAccumulation = SceneSingleton<CombatHUD>.i.jamAccumulation;
    for (int index = 0; index < this.mapIcons.Count; ++index)
    {
      if (this.mapIcons[index] is UnitMapIcon mapIcon)
        mapIcon.JammingDistortion(jamAccumulation);
    }
  }

  private void UpdateMap()
  {
    this.mapLastUpdated = Time.realtimeSinceStartup;
    float mapInverseScale = 1f / this.mapImage.transform.localScale.x;
    foreach (KeyValuePair<Airbase, AirbaseMapIcon> keyValuePair in this.airbaseIconLookup)
      keyValuePair.Value.UpdateIcon(this.mapDisplayFactor, mapInverseScale, this.mapImage.transform, DynamicMap.mapMaximized);
    for (int index = this.radarVisualizations.Count - 1; index >= 0; --index)
      this.radarVisualizations[index].Refresh();
    if (SceneSingleton<MapOptions>.i.showGridLabels != this.gridLabels.LabelEnabled)
      this.gridLabels.ShowLabels(SceneSingleton<MapOptions>.i.showGridLabels);
    if (this.waypoints.Count > 0)
    {
      foreach (MapWaypoint waypoint in this.waypoints)
        waypoint.UpdateMarker();
    }
    Action onMapChanged = DynamicMap.onMapChanged;
    if (onMapChanged == null)
      return;
    onMapChanged();
  }

  public void DisplayTooltip(MapIcon icon)
  {
    this.infoTextTarget = icon;
    this.toolTip.gameObject.SetActive(true);
    this.toolTip.ShowTooltip(icon);
  }

  public void HideTooltip()
  {
    this.infoTextTarget = (MapIcon) null;
    this.toolTip.gameObject.SetActive(false);
  }

  public void ClearWaypoints()
  {
    foreach (MapWaypoint waypoint in this.waypoints)
    {
      UnityEngine.Object.Destroy((UnityEngine.Object) waypoint.marker);
      UnityEngine.Object.Destroy((UnityEngine.Object) waypoint.vector);
    }
    this.waypoints.Clear();
    this.constructWaypoints.Clear();
  }

  private void Update()
  {
    this.SpawnQueuedIcons();
    this.UpdateIcons();
    if ((double) Time.realtimeSinceStartup - (double) this.mapLastUpdated >= 0.10000000149011612)
      this.UpdateMap();
    if (this.isJumping)
      this.JumptoTarget();
    else if (DynamicMap.mapMaximized)
      this.MapControls();
    else
      this.CenterMinimizedMap();
  }

  private void CenterMinimizedMap()
  {
    Vector3 vector3_1 = SceneSingleton<CameraStateManager>.i.transform.position.ToGlobalPosition().AsVector3() * this.mapDisplayFactor;
    if (!((UnityEngine.Object) SceneSingleton<CombatHUD>.i != (UnityEngine.Object) null) || !((UnityEngine.Object) SceneSingleton<CombatHUD>.i.aircraft != (UnityEngine.Object) null))
      return;
    Vector3 forward = SceneSingleton<CombatHUD>.i.aircraft.transform.forward with
    {
      y = 0.0f
    };
    Vector3 vector3_2 = vector3_1 + forward.normalized * this.mapDisplayFactor * 4000f;
    this.mapImage.transform.eulerAngles = new Vector3(0.0f, 0.0f, SceneSingleton<CombatHUD>.i.aircraft.transform.eulerAngles.y);
    this.mapImage.transform.localPosition = (this.mapImage.transform.right * -vector3_2.x + this.mapImage.transform.up * -vector3_2.z) * this.mapImage.transform.localScale.x * this.mapBackground.transform.localScale.x;
    this.viewIndicator.transform.localPosition = new Vector3(vector3_1.x, vector3_1.z, 0.0f);
    this.viewIndicator.transform.eulerAngles = new Vector3(0.0f, 0.0f, this.mapImage.transform.eulerAngles.z - SceneSingleton<CameraStateManager>.i.transform.eulerAngles.y);
  }

  private void SelectFromMap()
  {
    float num1 = 10000f;
    MapIcon mapIcon = (MapIcon) null;
    Vector3 mousePosition = Input.mousePosition;
    foreach (UnitMapIcon unitMapIcon in this.iconLookup.Values)
    {
      float num2 = FastMath.SquareDistance(mousePosition, unitMapIcon.transform.position);
      if ((double) num2 <= (double) num1 && unitMapIcon.iconImage.raycastTarget && !SceneSingleton<TargetListSelector>.i.CheckExclusions(unitMapIcon.unit) && (double) num2 < (double) num1 && unitMapIcon.iconImage.raycastTarget)
      {
        num1 = num2;
        mapIcon = (MapIcon) unitMapIcon;
      }
    }
    if (!((UnityEngine.Object) mapIcon != (UnityEngine.Object) null) || !mapIcon.gameObject.activeSelf)
      return;
    mapIcon.ClickIcon(MapIcon.ClickSource.Controller);
  }

  private void MapControls()
  {
    float num1 = this.player.GetAxis("Zoom View") * 0.05f;
    if ((double) num1 != 0.0)
      this.SetZoomLevel(Mathf.Clamp(this.mapScaleCenter.transform.localScale.x * (num1 + 1f), 1f, 20f));
    float axis1 = this.player.GetAxis("Move Map Horizontal");
    float axis2 = this.player.GetAxis("Move Map Vertical");
    if ((double) axis1 != 0.0 || (double) axis2 != 0.0)
    {
      float num2 = 300f * Time.unscaledDeltaTime / this.mapScaleCenter.localScale.x;
      this.positionOffset += new Vector2(axis1 * num2, axis2 * num2);
      Action onMapChanged = DynamicMap.onMapChanged;
      if (onMapChanged != null)
        onMapChanged();
    }
    if (Input.GetMouseButton(0))
    {
      this.clickTime += Time.deltaTime;
      float num3 = this.player.GetAxis("Pan View") * -1f;
      float axis3 = this.player.GetAxis("Tilt View");
      float num4 = 150f * Mathf.Min(Time.unscaledDeltaTime, 0.03f) / this.mapScaleCenter.localScale.x;
      this.positionOffset += new Vector2(num3 * num4, axis3 * num4);
      if ((double) this.positionOffset.magnitude > 0.0)
      {
        Action onMapChanged = DynamicMap.onMapChanged;
        if (onMapChanged != null)
          onMapChanged();
      }
    }
    else
      this.clickTime = 0.0f;
    if (this.player.GetButtonDown("Select"))
      this.SelectFromMap();
    GlobalPosition position;
    if (DynamicMap.mapMaximized && this.player.GetButtonDown("Jump Map") && !GameManager.GetLocalAircraft(out Aircraft _) && this.TryGetCursorCoordinates(out position))
      this.JumpCameraTo(position);
    this.followingCamera = this.positionOffset == Vector2.zero;
    if (DynamicMap.mapMaximized && this.selectedIcons.Count > 0 && Input.GetMouseButtonDown(1) && GameManager.gameState != GameState.Editor)
    {
      if (!Input.GetKey(KeyCode.LeftShift))
        this.ClearWaypoints();
      MapIcon selectedIcon1 = this.selectedIcons[0];
      if (selectedIcon1 is UnitMapIcon unitMapIcon1 && DynamicMap.GetFactionMode(unitMapIcon1.unit.NetworkHQ) == FactionMode.Friendly && !(unitMapIcon1.unit is Building))
      {
        Vector3 previousWaypoint = this.waypoints.Count > 0 ? this.waypoints[this.waypoints.Count - 1].marker.transform.localPosition : selectedIcon1.transform.localPosition;
        this.constructWaypoints.Add(this.GetCursorCoordinates());
        GameObject marker = UnityEngine.Object.Instantiate<GameObject>(this.mapWaypoint, this.iconLayer.transform);
        GameObject vector = UnityEngine.Object.Instantiate<GameObject>(this.mapWaypointVector, this.iconLayer.transform);
        MapWaypoint mapWaypoint = new MapWaypoint(previousWaypoint, marker, vector);
        if (this.waypoints.Count > 0)
          UnityEngine.Object.Destroy((UnityEngine.Object) this.waypoints[this.waypoints.Count - 1].marker);
        this.waypoints.Add(mapWaypoint);
        foreach (MapIcon selectedIcon2 in this.selectedIcons)
        {
          if (selectedIcon2 is UnitMapIcon unitMapIcon)
          {
            Unit unit = unitMapIcon.unit;
            if ((UnityEngine.Object) unit != (UnityEngine.Object) null && unit is ICommandable commandable && DynamicMap.GetFactionMode(unit.NetworkHQ) == FactionMode.Friendly)
            {
              UnitCommand unitCommand = commandable.UnitCommand;
              List<GlobalPosition> constructWaypoints = this.constructWaypoints;
              GlobalPosition waypoint = constructWaypoints[constructWaypoints.Count - 1];
              unitCommand.SetDestination(waypoint, true);
            }
          }
        }
      }
    }
    if (!((UnityEngine.Object) SceneSingleton<CameraStateManager>.i != (UnityEngine.Object) null))
      return;
    Vector3 vector3 = SceneSingleton<CameraStateManager>.i.transform.position.ToGlobalPosition().AsVector3() * this.mapDisplayFactor;
    float x = this.mapImage.transform.localScale.x;
    if (this.followingCamera)
    {
      Vector2 target = new Vector2(vector3.x, vector3.z);
      if ((UnityEngine.Object) SceneSingleton<CombatHUD>.i.aircraft != (UnityEngine.Object) null && !SceneSingleton<CombatHUD>.i.aircraft.disabled)
      {
        Vector3 forward = SceneSingleton<CombatHUD>.i.aircraft.transform.forward;
        Vector2 vector2 = new Vector2(forward.x, forward.z);
        this.stationaryOffset = target + 5000f * this.mapDisplayFactor * vector2;
      }
      else
        this.stationaryOffset = Vector2.MoveTowards(this.stationaryOffset, target, this.mapMoveMaxJumpSpeed);
    }
    this.mapImage.transform.localEulerAngles = Vector3.zero;
    this.mapScaleCenter.transform.localEulerAngles = Vector3.zero;
    Vector2 pos = -this.stationaryOffset - this.positionOffset;
    ((RectTransform) this.mapBackground.transform).rect.ClampPos(ref pos, 2f);
    this.positionOffset = -this.stationaryOffset - pos;
    this.mapImage.transform.localPosition = (Vector3) (pos * x);
    this.viewIndicator.transform.localPosition = new Vector3(vector3.x, vector3.z, 0.0f);
    this.viewIndicator.transform.eulerAngles = new Vector3(0.0f, 0.0f, this.mapImage.transform.eulerAngles.z - SceneSingleton<CameraStateManager>.i.transform.eulerAngles.y);
  }

  private void JumpCameraTo(GlobalPosition pos)
  {
    Vector3 localPosition = pos.ToLocalPosition();
    float num = Datum.LocalSeaY;
    RaycastHit hitInfo;
    if (Physics.Raycast(localPosition + new Vector3(0.0f, 10000f, 0.0f), Vector3.down, out hitInfo, 10000f, -1))
      num = hitInfo.point.y;
    float y = num + this.jumpCameraFocusHeight;
    Quaternion quaternion = Quaternion.Euler(this.jumpCameraFocusAngleDown, SceneSingleton<CameraStateManager>.i.transform.rotation.eulerAngles.y, 0.0f);
    Vector3 position = new Vector3(localPosition.x, y, localPosition.z);
    SceneSingleton<CameraStateManager>.i.FocusPosition(position, new Quaternion?(quaternion), this.jumpCameraFocusDistance);
  }

  public void SetMapTarget(GlobalPosition newTarget)
  {
    this.mapTarget = newTarget.AsVector3() * this.mapDisplayFactor;
    this.isJumping = true;
  }

  private void JumptoTarget()
  {
    Vector3 localPosition = this.mapImage.transform.localPosition;
    Vector3 b = (this.mapImage.transform.right * -this.mapTarget.x + this.mapImage.transform.up * -this.mapTarget.z) * this.mapImage.transform.localScale.x * this.mapBackground.transform.localScale.x;
    this.mapImage.transform.localPosition = Vector3.Lerp(localPosition, b, 0.05f);
    if ((double) Vector3.Distance(localPosition, b) >= 0.10000000149011612)
      return;
    this.mapTarget = Vector3.zero;
    this.isJumping = false;
  }

  public void SetZoomLevel(float zoomLevel)
  {
    this.mapScaleCenter.position = Input.mousePosition;
    this.mapScaleProxy.localScale = this.mapScaleCenter.localScale;
    this.mapScaleProxy.position = this.mapImage.transform.position;
    this.mapScaleProxy.transform.SetParent(this.mapScaleCenter);
    this.mapScaleCenter.localScale = Vector3.one * zoomLevel;
    this.mapScaleProxy.SetParent(this.mapBackground.transform);
    this.mapImage.transform.localScale = this.mapScaleProxy.localScale;
    this.mapImage.transform.position = this.mapScaleProxy.position;
    Action onMapChanged = DynamicMap.onMapChanged;
    if (onMapChanged == null)
      return;
    onMapChanged();
  }

  public float GetZoomLevel() => this.mapScaleCenter.localScale.x;

  public void ShowTypeChanged()
  {
    Action showTypesChanged = DynamicMap.onShowTypesChanged;
    if (showTypesChanged == null)
      return;
    showTypesChanged();
  }

  public static FactionMode GetFactionMode(FactionHQ hq = null, bool checkNoFactionBeforeSpectator = false)
  {
    if (checkNoFactionBeforeSpectator)
    {
      if ((UnityEngine.Object) hq == (UnityEngine.Object) null)
        return FactionMode.NoFaction;
      if ((UnityEngine.Object) SceneSingleton<DynamicMap>.i.HQ == (UnityEngine.Object) null)
        return FactionMode.Spectator;
    }
    else
    {
      if ((UnityEngine.Object) SceneSingleton<DynamicMap>.i.HQ == (UnityEngine.Object) null)
        return FactionMode.Spectator;
      if ((UnityEngine.Object) hq == (UnityEngine.Object) null)
        return FactionMode.NoFaction;
    }
    return !((UnityEngine.Object) SceneSingleton<DynamicMap>.i.HQ == (UnityEngine.Object) hq) ? FactionMode.Enemy : FactionMode.Friendly;
  }

  public static bool IsFactionMode(FactionHQ hq, FactionMode flags)
  {
    return (DynamicMap.GetFactionMode(hq) & flags) != 0;
  }

  private class RadarMapVis
  {
    public Image vectorImage;
    public Unit emitter;
    public float pingTime;
    public float delay = 1f;

    public RadarMapVis(Image vectorImage, Aircraft.OnRadarWarning source)
    {
      this.vectorImage = vectorImage;
      this.pingTime = Time.timeSinceLevelLoad;
      this.emitter = source.emitter;
      this.delay = 1f;
      if (source.detected)
        this.delay = 2f;
      if (!source.isTarget)
        return;
      this.delay = 4f;
    }

    public void Refresh()
    {
      float num = Time.timeSinceLevelLoad - this.pingTime;
      if ((UnityEngine.Object) this.emitter == (UnityEngine.Object) null || (double) num >= (double) this.delay || this.emitter.disabled || (UnityEngine.Object) SceneSingleton<CombatHUD>.i.aircraft == (UnityEngine.Object) null || SceneSingleton<CombatHUD>.i.aircraft.disabled)
      {
        UnityEngine.Object.Destroy((UnityEngine.Object) this.vectorImage.gameObject);
        SceneSingleton<DynamicMap>.i.radarVisualizations.Remove(this);
      }
      else
      {
        Vector3 vector3_1 = this.emitter.GlobalPosition().AsVector3() * SceneSingleton<DynamicMap>.i.mapDisplayFactor;
        this.vectorImage.transform.localPosition = new Vector3(vector3_1.x, vector3_1.z, 0.0f);
        Vector3 vector3_2 = SceneSingleton<DynamicMap>.i.iconLookup[(Unit) SceneSingleton<CombatHUD>.i.aircraft].transform.position - this.vectorImage.transform.position;
        this.vectorImage.transform.eulerAngles = new Vector3(0.0f, 0.0f, (float) (-(double) Mathf.Atan2(vector3_2.x, vector3_2.y) * 57.295780181884766));
        this.vectorImage.transform.localScale = new Vector3(1f, vector3_2.magnitude, 1f) / SceneSingleton<DynamicMap>.i.iconLayer.transform.lossyScale.x;
        Color color = this.vectorImage.color;
        color.a = Mathf.Lerp(color.a, 0.0f, num * 0.05f);
        this.vectorImage.color = color;
      }
    }
  }
}

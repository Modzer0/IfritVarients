// Decompiled with JetBrains decompiler
// Type: CombatHUD
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#nullable disable
public class CombatHUD : SceneSingleton<CombatHUD>
{
  public Transform iconLayer;
  public Aircraft aircraft;
  [SerializeField]
  private GameObject unitMarker;
  public Image targetDesignator;
  [SerializeField]
  private GameObject MissileUI;
  [SerializeField]
  private GameObject BoresightUI;
  [SerializeField]
  private GameObject BombingUI;
  [SerializeField]
  private GameObject TurretUI;
  [SerializeField]
  private GameObject CargoUI;
  [SerializeField]
  private GameObject LaserGuidedUI;
  [SerializeField]
  private GameObject SlingUI;
  [SerializeField]
  private GameObject NoWeaponUI;
  [SerializeField]
  private GameObject topRightPanel;
  [SerializeField]
  private WeaponStatus weaponStatus;
  [SerializeField]
  private GameObject countermeasureBackground;
  [SerializeField]
  private Image countermeasureImage;
  [SerializeField]
  private Text countermeasureName;
  [SerializeField]
  private Text countermeasureAmmo;
  [SerializeField]
  private ThreatList threatList;
  [SerializeField]
  private Image targetArrow;
  public Sprite minimizedFriendly;
  public Sprite minimizedHostile;
  [SerializeField]
  private Transform targetArrowTail;
  [SerializeField]
  private Text targetText;
  [SerializeField]
  private Text targetInfo;
  [SerializeField]
  private Transform aircraftActionsReportAnchor;
  [SerializeField]
  private AudioClip selectSound;
  [SerializeField]
  private AudioClip deselectSound;
  [SerializeField]
  private AudioClip deselectAllSound;
  [SerializeField]
  private AudioClip weaponSwitchSound;
  [SerializeField]
  private AudioClip jammedSound;
  [SerializeField]
  private ObjectiveOverlayManager objectiveOverlay;
  [SerializeField]
  private AudioSource jammedSource;
  [SerializeField]
  [Range(0.0f, 1f)]
  private float jammedVolumeMultiplier;
  private List<HUDUnitMarker> markers = new List<HUDUnitMarker>();
  private Dictionary<Unit, HUDUnitMarker> markerLookup = new Dictionary<Unit, HUDUnitMarker>();
  private List<Unit> targetList;
  private float smoothVel;
  [SerializeField]
  private GameObject hitMarker;
  private List<CombatHUD.HitMarker> hitMarkers = new List<CombatHUD.HitMarker>();
  private float targetSelectTimer;
  private int iconIndex;
  private float lastHitMarker;
  [HideInInspector]
  public float jamAccumulation;
  private HUDWeaponState weaponState;
  private WeaponStation currentWeaponStation;
  public GameObject notchIndicatorPrefab;

  public bool landingMode { get; private set; }

  public static event Action onSetTurretAuto;

  public static event Action<CombatHUD> onSetAircraft;

  public bool HasTargets
  {
    get
    {
      List<Unit> targetList = this.targetList;
      return targetList != null && __nonvirtual (targetList.Count) > 0;
    }
  }

  public bool turretAutoControl { get; private set; }

  protected override void Awake()
  {
    base.Awake();
    this.hitMarkers.Add(new CombatHUD.HitMarker(0.0f, this.hitMarker, (Unit) null, Vector3.zero));
    for (int index = 0; index < 19; ++index)
      this.hitMarkers.Add(new CombatHUD.HitMarker(0.0f, UnityEngine.Object.Instantiate<GameObject>(this.hitMarker, this.iconLayer), (Unit) null, Vector3.zero));
    PlayerSettings.OnApplyOptions += new Action(this.RefreshSettings);
  }

  private void OnDestroy()
  {
    PlayerSettings.OnApplyOptions -= new Action(this.RefreshSettings);
    this.ClearIcons();
  }

  public List<Unit> GetTargetList() => this.targetList;

  public void RefreshSettings()
  {
    this.targetText.fontSize = (int) PlayerSettings.overlayTextSize;
    this.targetInfo.fontSize = (int) PlayerSettings.hmdTextSize;
  }

  public void CreateMarker(PersistentID id)
  {
    Unit unit;
    if ((UnityEngine.Object) this.aircraft == (UnityEngine.Object) null || !UnitRegistry.TryGetUnit(new PersistentID?(id), out unit) || !((UnityEngine.Object) this.aircraft != (UnityEngine.Object) unit) || unit.disabled || this.markerLookup.ContainsKey(unit) || unit is Scenery)
      return;
    Image component = UnityEngine.Object.Instantiate<GameObject>(this.unitMarker, this.iconLayer).GetComponent<Image>();
    HUDUnitMarker hudUnitMarker = new HUDUnitMarker(unit, component);
    this.markers.Add(hudUnitMarker);
    this.markerLookup.Add(unit, hudUnitMarker);
    hudUnitMarker.AssessThreat((Unit) this.aircraft);
  }

  public void RemoveMarker(HUDUnitMarker marker)
  {
    if ((UnityEngine.Object) this.aircraft != (UnityEngine.Object) null && !this.aircraft.disabled)
      this.aircraft.weaponManager.RemoveTargetList(marker.unit);
    this.markers.Remove(marker);
    this.markerLookup.Remove(marker.unit);
  }

  public void DisplayHit(GlobalPosition hitPosition, Unit hitUnit)
  {
    if (!PlayerSettings.showHitMarkers || SceneSingleton<CameraStateManager>.i.currentState != SceneSingleton<CameraStateManager>.i.cockpitState || !(hitUnit is GroundVehicle) && !(hitUnit is Aircraft) && (double) hitUnit.maxRadius > 8.0)
      return;
    CombatHUD.HitMarker hitMarker = this.hitMarkers[0];
    this.hitMarkers.RemoveAt(0);
    hitMarker.SetMarker(hitPosition, hitUnit);
    this.hitMarkers.Add(hitMarker);
    if ((double) Time.timeSinceLevelLoad - (double) this.lastHitMarker < 0.05)
      return;
    this.lastHitMarker = Time.timeSinceLevelLoad;
    SoundManager.PlayInterfaceOneShot(GameAssets.i.hitMarkerSound);
  }

  public void ClearIcons()
  {
    foreach (HUDUnitMarker marker in this.markers)
      marker.RemoveIcon();
    this.markerLookup.Clear();
    this.markers.Clear();
    this.targetArrow.enabled = false;
    this.targetText.enabled = false;
    this.jamAccumulation = 0.0f;
  }

  public void SetAircraft(Aircraft aircraft)
  {
    this.ClearIcons();
    this.aircraft = aircraft;
    this.landingMode = false;
    this.threatList.SetAircraft(aircraft);
    this.targetList = (UnityEngine.Object) aircraft.weaponManager != (UnityEngine.Object) null ? aircraft.weaponManager.GetTargetList() : new List<Unit>();
    for (int i = 0; i < aircraft.NetworkHQ.factionUnits.Count; ++i)
      this.CreateMarker(aircraft.NetworkHQ.factionUnits[i]);
    foreach (KeyValuePair<PersistentID, TrackingInfo> keyValuePair in aircraft.NetworkHQ.trackingDatabase)
      this.CreateMarker(keyValuePair.Key);
    this.SetPlayerFaction();
    UnityEngine.Object.Instantiate<GameObject>(GameAssets.i.aircraftActionsReport, this.aircraftActionsReportAnchor).GetComponent<AircraftActionsReport>().Initialize(aircraft);
    this.objectiveOverlay.Initialize(aircraft, this.iconLayer);
    Action<CombatHUD> onSetAircraft = CombatHUD.onSetAircraft;
    if (onSetAircraft != null)
      onSetAircraft(this);
    this.jamAccumulation = 0.0f;
    aircraft.onJam += new Action<Unit.JamEventArgs>(this.CombatHUD_OnJam);
    this.turretAutoControl = true;
    FlightHud.ResetAircraft();
    PlayerSettings.OnApplyOptions += new Action(this.RefreshSettings);
  }

  public void RemoveAircraft()
  {
    if ((UnityEngine.Object) this.aircraft != (UnityEngine.Object) null)
      this.aircraft.onJam -= new Action<Unit.JamEventArgs>(this.CombatHUD_OnJam);
    this.jamAccumulation = 0.0f;
    DynamicMap.ClearJammingEffects();
    this.aircraft = (Aircraft) null;
    this.DeselectAll();
    PlayerSettings.OnApplyOptions -= new Action(this.RefreshSettings);
  }

  private void CombatHUD_OnJam(Unit.JamEventArgs e) => this.jamAccumulation += e.jamAmount * 0.5f;

  public void SetPlayerFaction()
  {
    foreach (HUDUnitMarker marker in this.markers)
      marker.AssessThreat((Unit) this.aircraft);
  }

  public bool MarkerExists(Unit unit) => this.markerLookup.ContainsKey(unit);

  public void HighlightMarker(Unit unit) => this.markerLookup[unit].SetNew();

  public void FlashMarker(Unit unit, bool flash)
  {
    if (this.landingMode || (UnityEngine.Object) unit == (UnityEngine.Object) null || (UnityEngine.Object) this.aircraft == (UnityEngine.Object) null)
      return;
    if (flash)
      this.CreateMarker(unit.persistentID);
    HUDUnitMarker hudUnitMarker;
    if (!this.markerLookup.TryGetValue(unit, out hudUnitMarker))
      return;
    hudUnitMarker.SetFlashing(flash);
  }

  public void DisplayWeaponSafety(bool safetyEnabled)
  {
    SceneSingleton<HUDOptions>.i.AutomaticToggle(this.currentWeaponStation, safetyEnabled);
  }

  public void ToggleAutoControl()
  {
    if (this.aircraft.weaponManager.StationsWithTurrets() == 0)
      return;
    this.turretAutoControl = !this.turretAutoControl;
    Action onSetTurretAuto = CombatHUD.onSetTurretAuto;
    if (onSetTurretAuto != null)
      onSetTurretAuto();
    string str = this.turretAutoControl ? "engage at will" : "hold fire";
    SceneSingleton<AircraftActionsReport>.i.ReportText("Turrets set to " + str, 4f);
  }

  public void ShowWeaponStation(WeaponStation weaponStation)
  {
    this.weaponStatus.SetCurrentStation(this.aircraft, weaponStation);
    if ((UnityEngine.Object) this.weaponState != (UnityEngine.Object) null)
      UnityEngine.Object.Destroy((UnityEngine.Object) this.weaponState.gameObject);
    this.weaponStatus.SetVisible(weaponStation != null);
    this.currentWeaponStation = weaponStation;
    if (weaponStation == null)
    {
      this.weaponState = UnityEngine.Object.Instantiate<GameObject>(this.NoWeaponUI, SceneSingleton<FlightHud>.i.GetHUDCenter()).GetComponent<HUDWeaponState>();
      this.weaponState.SetHUDWeaponState(this.targetDesignator, this.aircraft, weaponStation);
    }
    else
    {
      this.weaponState = (!weaponStation.HasTurret() ? (!weaponStation.WeaponInfo.boresight ? (!weaponStation.WeaponInfo.bomb ? (weaponStation.WeaponInfo.cargo || weaponStation.WeaponInfo.troops ? UnityEngine.Object.Instantiate<GameObject>(this.CargoUI, SceneSingleton<FlightHud>.i.HMDCenter) : (!weaponStation.WeaponInfo.sling ? (!weaponStation.WeaponInfo.laserGuided ? UnityEngine.Object.Instantiate<GameObject>(this.MissileUI, SceneSingleton<FlightHud>.i.GetHUDCenter()) : UnityEngine.Object.Instantiate<GameObject>(this.LaserGuidedUI, SceneSingleton<FlightHud>.i.GetHUDCenter())) : UnityEngine.Object.Instantiate<GameObject>(this.SlingUI, SceneSingleton<FlightHud>.i.GetHUDCenter()))) : UnityEngine.Object.Instantiate<GameObject>(this.BombingUI, SceneSingleton<FlightHud>.i.GetHUDCenter())) : UnityEngine.Object.Instantiate<GameObject>(this.BoresightUI, SceneSingleton<FlightHud>.i.GetHUDCenter())) : UnityEngine.Object.Instantiate<GameObject>(this.TurretUI, SceneSingleton<FlightHud>.i.GetHUDCenter())).GetComponent<HUDWeaponState>();
      this.weaponState.SetHUDWeaponState(this.targetDesignator, this.aircraft, weaponStation);
      SoundManager.PlayInterfaceOneShot(this.weaponSwitchSound);
      SceneSingleton<HUDOptions>.i.AutomaticToggle(this.currentWeaponStation, this.currentWeaponStation.SafetyIsOn(this.aircraft));
    }
  }

  public WeaponStation GetWeaponStation() => this.currentWeaponStation;

  public void DisplayCountermeasureAmmo(int ammo)
  {
    this.countermeasureAmmo.text = ammo.ToString();
    this.countermeasureAmmo.enabled = ammo > 1;
  }

  public void DisplayCountermeasures(string countermeasureName, Sprite displayImage, bool inUse)
  {
    Color color = inUse ? Color.red + Color.green * 0.5f : Color.green;
    this.countermeasureName.color = color;
    if (this.countermeasureName.text != countermeasureName)
      this.countermeasureName.text = countermeasureName;
    this.countermeasureImage.color = color;
    if (!((UnityEngine.Object) this.countermeasureImage.sprite != (UnityEngine.Object) displayImage))
      return;
    this.countermeasureImage.sprite = displayImage;
  }

  public void DisplayCountermeasures(string countermeasureName, Sprite displayImage, int ammo)
  {
    Color color = ammo > 0 ? Color.green : Color.red;
    if (this.countermeasureName.text != countermeasureName)
      this.countermeasureName.text = countermeasureName;
    if ((UnityEngine.Object) this.countermeasureImage.sprite != (UnityEngine.Object) displayImage)
      this.countermeasureImage.sprite = displayImage;
    this.countermeasureImage.color = color;
    this.countermeasureAmmo.color = color;
    this.countermeasureName.color = color;
    this.countermeasureAmmo.text = ammo.ToString();
    if (this.countermeasureAmmo.enabled)
      return;
    this.countermeasureAmmo.enabled = true;
  }

  public void SelectUnit(Unit unit)
  {
    if (this.markerLookup.ContainsKey(unit))
    {
      this.markerLookup[unit].SelectMarker();
      this.aircraft.weaponManager.AddTargetList(unit);
    }
    SoundManager.PlayInterfaceOneShot(this.selectSound);
  }

  public void DeSelectUnit(Unit unit)
  {
    if (this.targetList.Count == 0)
      return;
    if (this.markerLookup.ContainsKey(unit))
      this.markerLookup[unit]?.DeselectMarker();
    SoundManager.PlayInterfaceOneShot(this.deselectSound);
    SceneSingleton<DynamicMap>.i.DeselectIcon(unit);
    this.targetList.Remove(unit);
    this.aircraft.weaponManager.TargetListChanged();
  }

  private void TargetSelect(bool paint)
  {
    bool flag = false;
    List<HUDUnitMarker> hudUnitMarkerList = new List<HUDUnitMarker>();
    for (int index = 0; index < this.markers.Count; ++index)
    {
      if (FastMath.InRange(this.targetDesignator.gameObject.transform.position, this.markers[index].image.transform.position, 100f) && (paint || !this.markers[index].selected) && this.markers[index].image.enabled && !SceneSingleton<TargetListSelector>.i.CheckExclusions(this.markers[index].unit))
        hudUnitMarkerList.Add(this.markers[index]);
    }
    if (paint)
    {
      for (int index = 0; index < hudUnitMarkerList.Count; ++index)
      {
        if ((UnityEngine.Object) hudUnitMarkerList[index].unit != (UnityEngine.Object) null && (UnityEngine.Object) hudUnitMarkerList[index].unit.NetworkHQ != (UnityEngine.Object) null && (UnityEngine.Object) hudUnitMarkerList[index].unit.NetworkHQ != (UnityEngine.Object) this.aircraft.NetworkHQ && !this.targetList.Contains(hudUnitMarkerList[index].unit) && this.currentWeaponStation != null && (double) CombatAI.AnalyzeTarget(this.currentWeaponStation, (Unit) this.aircraft, this.aircraft.NetworkHQ.GetTrackingData(hudUnitMarkerList[index].unit.persistentID), mobile: true).opportunity != 0.0)
        {
          this.targetList.Insert(0, hudUnitMarkerList[index].unit);
          flag = true;
          hudUnitMarkerList[index].SelectMarker();
        }
      }
    }
    else if (hudUnitMarkerList.Count > 0)
    {
      hudUnitMarkerList.Sort((Comparison<HUDUnitMarker>) ((a, b) => b.AssessPriority(this.aircraft, this.targetDesignator.gameObject, this.currentWeaponStation).CompareTo(a.AssessPriority(this.aircraft, this.targetDesignator.gameObject, this.currentWeaponStation))));
      if (!this.targetList.Contains(hudUnitMarkerList[0].unit))
      {
        this.targetList.Insert(0, hudUnitMarkerList[0].unit);
        flag = true;
        hudUnitMarkerList[0].SelectMarker();
      }
    }
    if (!flag)
      return;
    SoundManager.PlayInterfaceOneShot(this.selectSound);
    this.aircraft.weaponManager.TargetListChanged();
  }

  public void DeselectAll(bool withAudio = false)
  {
    if (this.targetList.Count == 0)
      return;
    foreach (Unit target in this.targetList)
    {
      if (this.markerLookup.ContainsKey(target))
        this.markerLookup[target]?.DeselectMarker();
    }
    SceneSingleton<DynamicMap>.i.DeselectAllIcons();
    this.targetList.Clear();
    this.targetArrow.enabled = false;
    this.targetText.enabled = false;
    if ((UnityEngine.Object) this.aircraft != (UnityEngine.Object) null)
      this.aircraft.weaponManager.TargetListChanged();
    if (!withAudio)
      return;
    SoundManager.PlayInterfaceOneShot(this.deselectAllSound);
  }

  public void DeselectLast()
  {
    if (this.targetList.Count == 0)
      return;
    Unit target = this.targetList[0];
    if (this.markerLookup.ContainsKey(target))
      this.markerLookup[target]?.DeselectMarker();
    SoundManager.PlayInterfaceOneShot(this.deselectSound);
    SceneSingleton<DynamicMap>.i.DeselectIcon(target);
    this.targetList.Remove(target);
    this.aircraft.weaponManager.TargetListChanged();
  }

  private bool ShowTargetInfo()
  {
    HUDUnitMarker hudUnitMarker;
    GlobalPosition knownPosition;
    if (this.targetList.Count == 0 || this.targetArrow.enabled || !this.markerLookup.TryGetValue(this.targetList[0], out hudUnitMarker) || !this.aircraft.NetworkHQ.TryGetKnownPosition(hudUnitMarker.unit, out knownPosition))
      return false;
    this.targetInfo.transform.position = hudUnitMarker.image.transform.position;
    float distance = FastMath.Distance(knownPosition, this.aircraft.GlobalPosition());
    this.targetInfo.text = "";
    if (hudUnitMarker.unit is Aircraft)
    {
      Aircraft unit = hudUnitMarker.unit as Aircraft;
      if ((UnityEngine.Object) unit.pilots[0] != (UnityEngine.Object) null && (UnityEngine.Object) unit.Player != (UnityEngine.Object) null)
      {
        Text targetInfo = this.targetInfo;
        targetInfo.text = $"{targetInfo.text}{unit.Player.PlayerName}\n";
      }
    }
    Text targetInfo1 = this.targetInfo;
    targetInfo1.text = $"{targetInfo1.text}{hudUnitMarker.unit.definition.code}\n\n\n{UnitConverter.DistanceReading(distance)}";
    hudUnitMarker.image.color = Color.green;
    return true;
  }

  private void UpdateMarkers()
  {
    FactionHQ networkHq = this.aircraft.NetworkHQ;
    GlobalPosition viewPosition = SceneSingleton<CameraStateManager>.i.transform.GlobalPosition();
    if ((UnityEngine.Object) this.jammedSource == (UnityEngine.Object) null)
    {
      if ((double) this.jamAccumulation > 0.0)
      {
        this.jammedSource = this.gameObject.AddComponent<AudioSource>();
        this.jammedSource.spatialBlend = 0.0f;
        this.jammedSource.loop = true;
        this.jammedSource.dopplerLevel = 0.0f;
        this.jammedSource.outputAudioMixerGroup = SoundManager.i.JammedNoiseMixer;
        this.jammedSource.clip = this.jammedSound;
        this.jammedSource.volume = 0.0f;
        this.jammedSource.Play();
      }
    }
    else
    {
      if ((double) this.jamAccumulation == 0.0)
        this.jammedSource.Stop();
      if ((double) this.jamAccumulation > 0.0)
      {
        this.jammedSource.volume = FastMath.SmoothDamp(this.jammedSource.volume, this.jamAccumulation, ref this.smoothVel, 0.5f) * this.jammedVolumeMultiplier;
        if (!this.jammedSource.isPlaying)
        {
          this.jammedSource.Play();
          this.jammedSource.time = UnityEngine.Random.Range(0.0f, this.jammedSound.length);
        }
      }
    }
    if ((UnityEngine.Object) this.aircraft != (UnityEngine.Object) null && this.markers.Count > 0)
    {
      Vector3 forward = SceneSingleton<CameraStateManager>.i.transform.forward;
      foreach (HUDUnitMarker marker in this.markers)
      {
        marker.UpdatePosition(networkHq, viewPosition, forward);
        if ((double) this.jamAccumulation > 0.0)
          marker.JammingDistortion(this.jamAccumulation);
      }
      if (this.iconIndex >= this.markers.Count)
        this.iconIndex = 0;
      this.markers[this.iconIndex].UpdateVisibility(networkHq, viewPosition);
      ++this.iconIndex;
    }
    this.jamAccumulation = Mathf.Clamp01(this.jamAccumulation - Mathf.Max(this.jamAccumulation, 0.25f) * Time.deltaTime);
  }

  private void UpdateHitMarkers()
  {
    for (int index = 0; index < this.hitMarkers.Count; ++index)
      this.hitMarkers[index].Position();
  }

  public void SetTargetArrow(bool enabled, Vector3 position, Vector3 angles)
  {
    this.targetArrow.enabled = enabled;
    this.targetText.enabled = enabled;
    this.targetText.transform.position = this.targetArrowTail.position;
    if (!enabled)
      return;
    this.targetArrow.transform.position = position;
    this.targetArrow.transform.localEulerAngles = angles;
  }

  private void FixedUpdate()
  {
    if (!((UnityEngine.Object) this.weaponState != (UnityEngine.Object) null))
      return;
    this.weaponState.HUDFixedUpdate(this.aircraft, this.targetList);
  }

  private void LateUpdate()
  {
    if ((UnityEngine.Object) this.aircraft == (UnityEngine.Object) null)
      return;
    this.UpdateMarkers();
    this.UpdateHitMarkers();
    if (GameManager.playerInput.GetButtonTimedPressUp("Select", 0.0f, PlayerSettings.clickDelay))
      this.TargetSelect(false);
    else if (GameManager.playerInput.GetButtonTimedPressDown("Select", PlayerSettings.pressDelay) && !DynamicMap.mapMaximized)
      this.TargetSelect(true);
    if (this.targetList.Count > 0)
    {
      if ((UnityEngine.Object) this.aircraft.targetCam != (UnityEngine.Object) null)
        this.aircraft.targetCam.SetTargetCam();
    }
    else if (this.targetArrow.enabled)
    {
      this.targetArrow.enabled = false;
      this.targetText.enabled = false;
    }
    int num = this.ShowTargetInfo() ? 1 : 0;
    if (num != 0 && !this.targetInfo.enabled)
      this.targetInfo.enabled = true;
    if (num == 0 && this.targetInfo.enabled)
      this.targetInfo.enabled = false;
    if (!((UnityEngine.Object) this.weaponState != (UnityEngine.Object) null))
      return;
    this.weaponState.UpdateWeaponDisplay(this.aircraft, this.targetList);
  }

  public GameObject ShowNotchIndicator()
  {
    return UnityEngine.Object.Instantiate<GameObject>(this.notchIndicatorPrefab, this.iconLayer.transform);
  }

  private class HitMarker
  {
    public float hitTime;
    public GameObject marker;
    public Unit hitUnit;
    public Vector3 hitOffset;
    private Vector3 scaleVector = new Vector3(1f, 1f, 0.0f);

    public HitMarker(float hitTime, GameObject marker, Unit hitUnit, Vector3 hitOffset)
    {
      this.hitTime = hitTime;
      this.marker = marker;
      this.hitUnit = hitUnit;
      this.hitOffset = hitOffset;
      marker.SetActive(false);
    }

    public void SetMarker(GlobalPosition globalPosition, Unit hitUnit)
    {
      this.hitTime = Time.timeSinceLevelLoad;
      this.hitUnit = hitUnit;
      this.hitOffset = globalPosition - hitUnit.GlobalPosition();
      this.Position();
    }

    public void Position()
    {
      if ((UnityEngine.Object) this.hitUnit == (UnityEngine.Object) null || (double) Time.timeSinceLevelLoad - (double) this.hitTime > 0.20000000298023224)
      {
        this.marker.SetActive(false);
      }
      else
      {
        this.marker.SetActive((double) Vector3.Dot(this.hitUnit.transform.position + this.hitOffset - SceneSingleton<CameraStateManager>.i.transform.position, SceneSingleton<CameraStateManager>.i.transform.forward) > 0.0);
        if (!this.marker.activeSelf)
          return;
        this.marker.transform.position = Vector3.Scale(SceneSingleton<CameraStateManager>.i.mainCamera.WorldToScreenPoint(this.hitUnit.transform.position + this.hitOffset), this.scaleVector);
      }
    }
  }
}

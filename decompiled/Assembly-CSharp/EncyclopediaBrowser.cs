// Decompiled with JetBrains decompiler
// Type: EncyclopediaBrowser
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Cysharp.Threading.Tasks;
using NuclearOption.Networking;
using NuclearOption.SavedMission;
using NuclearOption.SceneLoading;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

#nullable disable
public class EncyclopediaBrowser : SceneSingleton<EncyclopediaBrowser>
{
  public EncyclopediaBrowser.EncyclopediaMode mode = EncyclopediaBrowser.EncyclopediaMode.Vehicle;
  private List<UnitDefinition> browseList = new List<UnitDefinition>();
  [SerializeField]
  private MapLoader mapLoader;
  [SerializeField]
  private Button vehiclesTab;
  [SerializeField]
  private Button aircraftTab;
  [SerializeField]
  private Button shipsTab;
  [SerializeField]
  private Button buildingsTab;
  [SerializeField]
  private Button missilesTab;
  [SerializeField]
  private Transform[] spawnTransforms;
  private Transform spawnTransform;
  [SerializeField]
  private TMP_Text unitName;
  [SerializeField]
  private TMP_Text unitDescription;
  [SerializeField]
  private GameObject massPanel;
  [SerializeField]
  private GameObject topSpeedPanel;
  [SerializeField]
  private GameObject emptyWeightPanel;
  [SerializeField]
  private GameObject stallSpeedPanel;
  [SerializeField]
  private GameObject maneuverabilityPanel;
  [SerializeField]
  private GameObject guidancePanel;
  [SerializeField]
  private GameObject yieldPanel;
  [SerializeField]
  private GameObject burnTimePanel;
  [SerializeField]
  private GameObject deltaVPanel;
  [SerializeField]
  private GameObject rangePanel;
  [SerializeField]
  private GameObject rcsPanel;
  [SerializeField]
  private GameObject weaponPanel;
  [SerializeField]
  private TMP_Text length;
  [SerializeField]
  private TMP_Text width;
  [SerializeField]
  private TMP_Text height;
  [SerializeField]
  private TMP_Text cost;
  [SerializeField]
  private TMP_Text mass;
  [SerializeField]
  private TMP_Text topSpeed;
  [SerializeField]
  private TMP_Text emptyWeight;
  [SerializeField]
  private TMP_Text stallSpeed;
  [SerializeField]
  private TMP_Text maneuverability;
  [SerializeField]
  private TMP_Text guidance;
  [SerializeField]
  private TMP_Text yield;
  [SerializeField]
  private TMP_Text burnTime;
  [SerializeField]
  private TMP_Text deltaV;
  [SerializeField]
  private TMP_Text range;
  [SerializeField]
  private TMP_Text rcs;
  [SerializeField]
  private GameObject backdropGround;
  [SerializeField]
  private GameObject backdropWater;
  [SerializeField]
  private RectTransform descriptionPanel;
  [SerializeField]
  private Material waterMaterial;
  [SerializeField]
  private MapKey mapKey;
  [SerializeField]
  private MapSettings mapSettings;
  [SerializeField]
  private EncyclopediaBrowser.WeaponStationDisplay[] weaponStationDisplays;
  private List<WeaponInfo> weaponInfoToShow = new List<WeaponInfo>();
  public GameObject spawnedUnitObject;
  private Unit spawnedUnit;
  private Transform[] spawnedParts;
  private float cameraDistance = 5f;
  private float cameraBaseHeight;
  private float targetDistance;
  private float cameraSmoothingVel;
  private bool initialized;
  private int index;

  private void SpawnUnit(UnitDefinition definition)
  {
    if ((UnityEngine.Object) NetworkSceneSingleton<Spawner>.i == (UnityEngine.Object) null)
      Debug.LogError((object) "Error: Spawner was not initialized");
    GlobalPosition globalPosition = this.spawnTransform.GlobalPosition() + Vector3.up * definition.spawnOffset.y;
    Debug.Log((object) $"Spawning {definition.unitName} at global position {globalPosition.x:F0}, {globalPosition.y:F0}, {globalPosition.z:F0}");
    if (this.mode == EncyclopediaBrowser.EncyclopediaMode.Vehicle)
      this.spawnedUnitObject = NetworkSceneSingleton<Spawner>.i.SpawnVehicle(definition.unitPrefab, globalPosition, this.spawnTransform.rotation, Vector3.zero, (FactionHQ) null, definition.unitName, 0.0f, true, (Player) null).gameObject;
    if (this.mode == EncyclopediaBrowser.EncyclopediaMode.Aircraft)
      this.spawnedUnitObject = NetworkSceneSingleton<Spawner>.i.SpawnAircraft((Player) null, definition.unitPrefab, (Loadout) null, 0.0f, new LiveryKey(), globalPosition, this.spawnTransform.rotation, Vector3.zero, (Hangar) null, (FactionHQ) null, definition.unitName, 0.0f, 0.0f).gameObject;
    if (this.mode == EncyclopediaBrowser.EncyclopediaMode.Missile)
      this.spawnedUnitObject = NetworkSceneSingleton<Spawner>.i.SpawnMissileEncyclopedia(definition as MissileDefinition, this.spawnTransform).gameObject;
    if (this.mode == EncyclopediaBrowser.EncyclopediaMode.Ship)
      this.spawnedUnitObject = NetworkSceneSingleton<Spawner>.i.SpawnShip(definition.unitPrefab, globalPosition, this.spawnTransform.rotation, (FactionHQ) null, definition.unitName, 0.0f, true).gameObject;
    if (this.mode == EncyclopediaBrowser.EncyclopediaMode.Building)
      this.spawnedUnitObject = NetworkSceneSingleton<Spawner>.i.SpawnBuilding(definition.unitPrefab, globalPosition, this.spawnTransform.rotation, (FactionHQ) null, (Airbase) null, definition.unitName, false, (SavedBuilding.FactoryOptions) null).gameObject;
    this.cameraBaseHeight = definition.spawnOffset.y;
    this.spawnedParts = this.spawnedUnitObject.GetComponentsInChildren<Transform>();
    this.spawnedUnit = this.spawnedUnitObject.GetComponent<Unit>();
    this.unitName.text = definition.unitName;
    this.targetDistance = (float) (((double) Mathf.Max(definition.length, definition.width * 0.7f) + (double) Mathf.Max(definition.width, definition.length * 0.7f)) * 0.5);
    if ((double) definition.height > (double) definition.length && (double) definition.height > (double) definition.width)
      this.targetDistance = definition.height * 1.2f;
    this.unitDescription.text = definition.description;
    this.targetDistance /= Mathf.Clamp(Mathf.Pow(this.targetDistance * 0.1f, 0.2f), 0.6f, 1.5f);
    this.DisplayUnitInfo(definition);
    this.UpdateWeaponDisplay(this.spawnedUnit);
    LayoutRebuilder.ForceRebuildLayoutImmediate(this.descriptionPanel);
  }

  private void DisplayUnitInfo(UnitDefinition definition)
  {
    this.length.text = UnitConverter.DimensionReading(definition.length);
    this.width.text = UnitConverter.DimensionReading(definition.width);
    this.height.text = UnitConverter.DimensionReading(definition.height);
    this.cost.text = UnitConverter.ValueReading(definition.value);
    if (definition is BuildingDefinition)
    {
      this.massPanel.SetActive(false);
      this.topSpeedPanel.SetActive(false);
      this.emptyWeightPanel.SetActive(false);
      this.stallSpeedPanel.SetActive(false);
      this.maneuverabilityPanel.SetActive(false);
      this.deltaVPanel.SetActive(false);
      this.rangePanel.SetActive(false);
      this.guidancePanel.SetActive(false);
      this.yieldPanel.SetActive(false);
      this.burnTimePanel.SetActive(false);
      this.rcsPanel.SetActive(false);
    }
    if (definition is VehicleDefinition)
    {
      this.massPanel.SetActive(true);
      this.topSpeedPanel.SetActive(true);
      this.emptyWeightPanel.SetActive(false);
      this.stallSpeedPanel.SetActive(false);
      this.maneuverabilityPanel.SetActive(false);
      this.deltaVPanel.SetActive(false);
      this.rangePanel.SetActive(false);
      this.guidancePanel.SetActive(false);
      this.yieldPanel.SetActive(false);
      this.burnTimePanel.SetActive(false);
      this.rcsPanel.SetActive(false);
      this.mass.text = UnitConverter.WeightReading((definition as VehicleDefinition).mass);
      this.topSpeed.text = UnitConverter.SpeedReadingGround(definition.unitPrefab.GetComponent<GroundVehicle>().GetTopSpeed() / 3.6f);
    }
    if (definition is ShipDefinition)
    {
      this.massPanel.SetActive(true);
      this.topSpeedPanel.SetActive(true);
      this.emptyWeightPanel.SetActive(false);
      this.stallSpeedPanel.SetActive(false);
      this.maneuverabilityPanel.SetActive(false);
      this.deltaVPanel.SetActive(false);
      this.rangePanel.SetActive(false);
      this.guidancePanel.SetActive(false);
      this.yieldPanel.SetActive(false);
      this.burnTimePanel.SetActive(false);
      this.rcsPanel.SetActive(false);
      ShipDefinition shipDefinition = definition as ShipDefinition;
      this.mass.text = UnitConverter.WeightReading(shipDefinition.mass);
      this.topSpeed.text = UnitConverter.SpeedReading(shipDefinition.shipInfo.topSpeed / 3.6f);
    }
    if (definition is AircraftDefinition)
    {
      this.massPanel.SetActive(false);
      this.emptyWeightPanel.SetActive(true);
      this.stallSpeedPanel.SetActive(true);
      this.maneuverabilityPanel.SetActive(true);
      this.deltaVPanel.SetActive(false);
      this.rangePanel.SetActive(false);
      this.guidancePanel.SetActive(false);
      this.yieldPanel.SetActive(false);
      this.burnTimePanel.SetActive(false);
      this.rcsPanel.SetActive(true);
      AircraftDefinition aircraftDefinition = definition as AircraftDefinition;
      Aircraft component = this.spawnedUnitObject.GetComponent<Aircraft>();
      float weight = 0.0f;
      foreach (UnitPart allPart in component.GetAllParts())
        weight += allPart.mass;
      this.emptyWeight.text = UnitConverter.WeightReading(weight);
      this.topSpeed.text = UnitConverter.SpeedReading(aircraftDefinition.aircraftInfo.maxSpeed / 3.6f);
      this.stallSpeed.text = UnitConverter.SpeedReading(aircraftDefinition.aircraftInfo.stallSpeed / 3.6f);
      this.maneuverability.text = aircraftDefinition.aircraftInfo.maneuverability.ToString("F1") + "g";
      this.rcs.text = $"{aircraftDefinition.radarSize}";
    }
    if (!(definition is MissileDefinition))
      return;
    this.massPanel.SetActive(true);
    this.emptyWeightPanel.SetActive(false);
    this.stallSpeedPanel.SetActive(false);
    this.maneuverabilityPanel.SetActive(false);
    this.guidancePanel.SetActive(true);
    this.yieldPanel.SetActive(true);
    this.rcsPanel.SetActive(true);
    MissileDefinition missileDefinition = definition as MissileDefinition;
    Missile componentInChildren = definition.unitPrefab.GetComponentInChildren<Missile>();
    WeaponInfo weaponInfo = componentInChildren.GetWeaponInfo();
    this.cost.text = UnitConverter.ValueReading(weaponInfo.costPerRound);
    this.unitDescription.text = weaponInfo.description;
    float speed = componentInChildren.CalcDeltaV();
    float topSpeed = componentInChildren.GetTopSpeed(0.0f, 0.0f);
    Debug.Log((object) $"Calculated deltaV: {this.deltaV}, topSpeed: {this.topSpeed}");
    double num = (double) componentInChildren.CalcRange(0.0f, 0.0f, 0.0f, 10000f, 0.0f, out float _);
    this.mass.text = UnitConverter.WeightReading(missileDefinition.GetMass());
    if ((double) speed > 0.0)
    {
      this.range.text = UnitConverter.DistanceReading(weaponInfo.targetRequirements.maxRange);
      this.burnTime.text = $"{componentInChildren.GetTotalBurnTime()}s";
      this.rangePanel.SetActive(true);
      this.burnTimePanel.SetActive(true);
    }
    else
    {
      this.rangePanel.SetActive(false);
      this.burnTimePanel.SetActive(false);
    }
    if ((double) topSpeed >= (double) speed)
    {
      this.deltaVPanel.SetActive(true);
      this.topSpeedPanel.SetActive(false);
      this.deltaV.text = UnitConverter.SpeedReading(speed);
    }
    else
    {
      this.deltaVPanel.SetActive(false);
      this.topSpeedPanel.SetActive(true);
      this.topSpeed.text = UnitConverter.SpeedReading(topSpeed);
    }
    this.guidance.text = componentInChildren.GetComponent<MissileSeeker>().GetSeekerType() ?? "";
    this.yield.text = UnitConverter.YieldReading(componentInChildren.GetYield()) + " TNT";
    this.rcs.text = $"{missileDefinition.radarSize}";
  }

  private void UpdateWeaponDisplay(Unit unit)
  {
    this.weaponPanel.SetActive(false);
    foreach (EncyclopediaBrowser.WeaponStationDisplay weaponStationDisplay in this.weaponStationDisplays)
      weaponStationDisplay.Hide();
    this.weaponInfoToShow.Clear();
    foreach (WeaponStation weaponStation in unit.weaponStations)
    {
      if (!this.weaponInfoToShow.Contains(weaponStation.WeaponInfo))
        this.weaponInfoToShow.Add(weaponStation.WeaponInfo);
    }
    this.weaponPanel.SetActive(this.weaponInfoToShow.Count > 0);
    this.weaponInfoToShow.Sort((Comparison<WeaponInfo>) ((a, b) => a.costPerRound.CompareTo(b.costPerRound)));
    for (int index = 0; index < this.weaponInfoToShow.Count; ++index)
      this.weaponStationDisplays[index].Show(unit, this.weaponInfoToShow[index]);
  }

  private void RemoveUnit()
  {
    if (this.spawnedParts == null)
      return;
    foreach (Transform spawnedPart in this.spawnedParts)
    {
      if ((UnityEngine.Object) spawnedPart != (UnityEngine.Object) null)
        UnityEngine.Object.Destroy((UnityEngine.Object) spawnedPart.gameObject);
    }
  }

  public void BackToMenu()
  {
    Time.timeScale = 1f;
    NetworkManagerNuclearOption.i.Stop(true);
  }

  protected override void Awake()
  {
    base.Awake();
    GameManager.SetGameState(GameState.Encyclopedia);
  }

  private void Start() => NetworkSceneSingleton<LevelInfo>.i.ApplyMapSettings(this.mapSettings);

  public Unit GetSpawnedUnit() => this.spawnedUnit;

  private async UniTask Initialize()
  {
    await UniTask.DelayFrame(1);
    this.SelectVehicles();
  }

  public void SelectAircraft()
  {
    this.spawnTransform = this.spawnTransforms[0];
    this.mode = EncyclopediaBrowser.EncyclopediaMode.Aircraft;
    this.RemoveUnit();
    this.index = 0;
    this.browseList.Clear();
    foreach (AircraftDefinition aircraftDefinition in Encyclopedia.i.aircraft)
    {
      if (!aircraftDefinition.disabled)
        this.browseList.Add((UnitDefinition) aircraftDefinition);
    }
    this.SpawnAircraft(this.browseList[this.index]);
  }

  public void SelectVehicles()
  {
    this.spawnTransform = this.spawnTransforms[1];
    this.mode = EncyclopediaBrowser.EncyclopediaMode.Vehicle;
    this.RemoveUnit();
    this.index = 0;
    this.browseList.Clear();
    foreach (VehicleDefinition vehicle in Encyclopedia.i.vehicles)
    {
      if (!vehicle.disabled)
        this.browseList.Add((UnitDefinition) vehicle);
    }
    this.SpawnUnit(this.browseList[this.index]);
  }

  public void SelectShips()
  {
    this.spawnTransform = this.spawnTransforms[2];
    this.mode = EncyclopediaBrowser.EncyclopediaMode.Ship;
    this.RemoveUnit();
    this.index = 0;
    this.browseList.Clear();
    this.browseList.AddRange((IEnumerable<UnitDefinition>) Encyclopedia.i.ships);
    this.waterMaterial.SetVector("_OriginOffset", (Vector4) Vector2.zero);
    this.SpawnUnit(this.browseList[this.index]);
  }

  public void SelectBuildings()
  {
    this.spawnTransform = this.spawnTransforms[3];
    this.mode = EncyclopediaBrowser.EncyclopediaMode.Building;
    this.RemoveUnit();
    this.index = 0;
    this.browseList.Clear();
    this.browseList.AddRange((IEnumerable<UnitDefinition>) Encyclopedia.i.buildings);
    this.SpawnUnit(this.browseList[this.index]);
  }

  public void SelectMissiles()
  {
    this.spawnTransform = this.spawnTransforms[4];
    this.mode = EncyclopediaBrowser.EncyclopediaMode.Missile;
    this.RemoveUnit();
    this.index = 0;
    this.browseList.Clear();
    foreach (MissileDefinition missile in Encyclopedia.i.missiles)
    {
      if (!missile.disabled)
        this.browseList.Add((UnitDefinition) missile);
    }
    this.browseList.Sort((Comparison<UnitDefinition>) ((a, b) => a.value.CompareTo(b.value)));
    this.SpawnUnit(this.browseList[this.index]);
  }

  public void SpawnAircraft(UnitDefinition definition)
  {
    this.SpawnUnit(definition);
    Aircraft componentInChildren = this.spawnedUnitObject.GetComponentInChildren<Aircraft>();
    componentInChildren.networked = false;
    componentInChildren.SetGear(LandingGear.GearState.LockedExtended);
  }

  private void Update()
  {
    if (!this.initialized && (UnityEngine.Object) NetworkSceneSingleton<Spawner>.i != (UnityEngine.Object) null && NetworkSceneSingleton<Spawner>.i.NetId != 0U)
    {
      this.initialized = true;
      PlayerSettings.LoadPrefs();
      this.aircraftTab.Select();
      this.SelectAircraft();
    }
    int num = (UnityEngine.Object) this.spawnedUnitObject == (UnityEngine.Object) null ? 1 : 0;
  }

  public void NextUnit(bool backwards)
  {
    int index = this.index;
    this.index += backwards ? -1 : 1;
    if (this.index < 0)
      this.index = this.browseList.Count - 1;
    if (this.index >= this.browseList.Count)
      this.index = 0;
    while (this.browseList[this.index].disabled)
    {
      this.index += backwards ? -1 : 1;
      if (this.index < 0)
        this.index = this.browseList.Count - 1;
      if (this.index >= this.browseList.Count)
        this.index = 0;
    }
    if (this.index == index)
      return;
    this.RemoveUnit();
    if (this.mode == EncyclopediaBrowser.EncyclopediaMode.Vehicle)
      this.SpawnUnit(this.browseList[this.index]);
    if (this.mode == EncyclopediaBrowser.EncyclopediaMode.Aircraft)
      this.SpawnAircraft(this.browseList[this.index]);
    if (this.mode == EncyclopediaBrowser.EncyclopediaMode.Ship)
      this.SpawnUnit(this.browseList[this.index]);
    if (this.mode == EncyclopediaBrowser.EncyclopediaMode.Missile)
      this.SpawnUnit(this.browseList[this.index]);
    if (this.mode != EncyclopediaBrowser.EncyclopediaMode.Building)
      return;
    this.SpawnUnit(this.browseList[this.index]);
  }

  [Serializable]
  private class WeaponStationDisplay
  {
    [SerializeField]
    private GameObject panel;
    [SerializeField]
    private TMP_Text nameText;
    [SerializeField]
    private TMP_Text ammoText;
    private List<WeaponStation> weaponStations;
    private WeaponInfo weaponInfo;

    public void Hide()
    {
      this.panel.SetActive(false);
      if (this.weaponStations != null)
        this.weaponStations.Clear();
      this.weaponInfo = (WeaponInfo) null;
    }

    public void Show(Unit unit, WeaponInfo weaponInfo)
    {
      this.weaponInfo = weaponInfo;
      if (this.weaponStations == null)
        this.weaponStations = new List<WeaponStation>();
      this.weaponStations.Clear();
      foreach (WeaponStation weaponStation in unit.weaponStations)
      {
        if ((UnityEngine.Object) weaponStation.WeaponInfo == (UnityEngine.Object) weaponInfo)
          this.weaponStations.Add(weaponStation);
      }
      this.panel.SetActive(true);
      this.UpdateText();
    }

    private void UpdateText()
    {
      int num = 0;
      foreach (WeaponStation weaponStation in this.weaponStations)
        num += weaponStation.FullAmmo;
      this.nameText.text = this.weaponInfo.weaponName ?? "";
      this.ammoText.text = $"x {num}";
    }
  }

  public enum EncyclopediaMode
  {
    Aircraft,
    Vehicle,
    Ship,
    Building,
    Missile,
  }

  private enum BrowserMode
  {
    Aircraft,
    Vehicle,
    Building,
    Ship,
    Missile,
  }
}

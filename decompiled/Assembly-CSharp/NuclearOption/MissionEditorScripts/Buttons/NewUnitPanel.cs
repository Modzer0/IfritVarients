// Decompiled with JetBrains decompiler
// Type: NuclearOption.MissionEditorScripts.Buttons.NewUnitPanel
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.Networking;
using NuclearOption.SavedMission;
using NuclearOption.SavedMission.ObjectiveV2;
using NuclearOption.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

#nullable disable
namespace NuclearOption.MissionEditorScripts.Buttons;

public class NewUnitPanel : MonoBehaviour, IPlacingMenu
{
  private static readonly Dictionary<string, NewUnitPanel.UnitOptionProvider> unitProviders = new Dictionary<string, NewUnitPanel.UnitOptionProvider>();
  private static string stickyTab;
  private static bool stickyPlaceMore;
  [SerializeField]
  private Button aircraft;
  [SerializeField]
  private Button vehicles;
  [SerializeField]
  private Button buildings;
  [SerializeField]
  private Button ships;
  [SerializeField]
  private Button scenery;
  [SerializeField]
  private Button missiles;
  [SerializeField]
  private Button otherUnits;
  [SerializeField]
  private Button airbase;
  [SerializeField]
  private Color highlight;
  [SerializeField]
  private Color normal;
  [SerializeField]
  private TMP_Dropdown typeDropdown;
  [SerializeField]
  private TMP_Dropdown factionDropdown;
  [SerializeField]
  private Vector3DataField positionField;
  [SerializeField]
  private SliderToggle placeMoreToggle;
  [SerializeField]
  private TextMeshProUGUI placeTextHint;
  [SerializeField]
  private float placeFlagSize = 5f;
  private Button activeButton;
  private List<string> factionOptions;
  private NewUnitPanel.PlaceType placeType;
  private NewUnitPanel.UnitOptionProvider activeProvider;
  private UnitDefinition placingDefinition;
  private GameObject placingObject;
  private AirbaseEditorFlag placingFlag;
  private ValueWrapperGlobalPosition positionWrapper;
  private LiveryKey aircraftLiver;

  private void Awake()
  {
    if (NewUnitPanel.unitProviders.Count == 0)
    {
      NewUnitPanel.unitProviders.Add("aircraft", NewUnitPanel.UnitOptionProvider.Create<AircraftDefinition>(Encyclopedia.i.aircraft));
      NewUnitPanel.unitProviders.Add("vehicles", NewUnitPanel.UnitOptionProvider.Create<VehicleDefinition>(Encyclopedia.i.vehicles));
      NewUnitPanel.unitProviders.Add("buildings", NewUnitPanel.UnitOptionProvider.Create<BuildingDefinition>(Encyclopedia.i.buildings));
      NewUnitPanel.unitProviders.Add("ships", NewUnitPanel.UnitOptionProvider.Create<ShipDefinition>(Encyclopedia.i.ships));
      NewUnitPanel.unitProviders.Add("scenery", NewUnitPanel.UnitOptionProvider.Create<SceneryDefinition>(Encyclopedia.i.scenery));
      NewUnitPanel.unitProviders.Add("missiles", NewUnitPanel.UnitOptionProvider.Create<MissileDefinition>(Encyclopedia.i.missiles));
      NewUnitPanel.unitProviders.Add("otherUnits", NewUnitPanel.UnitOptionProvider.Create<UnitDefinition>(Encyclopedia.i.otherUnits));
    }
    this.aircraft.onClick.AddListener((UnityAction) (() => this.TabClickedUnit(this.aircraft, "aircraft")));
    this.vehicles.onClick.AddListener((UnityAction) (() => this.TabClickedUnit(this.vehicles, "vehicles")));
    this.buildings.onClick.AddListener((UnityAction) (() => this.TabClickedUnit(this.buildings, "buildings")));
    this.ships.onClick.AddListener((UnityAction) (() => this.TabClickedUnit(this.ships, "ships")));
    this.scenery.onClick.AddListener((UnityAction) (() => this.TabClickedUnit(this.scenery, "scenery")));
    this.missiles.onClick.AddListener((UnityAction) (() => this.TabClickedUnit(this.missiles, "missiles")));
    this.otherUnits.onClick.AddListener((UnityAction) (() => this.TabClickedUnit(this.otherUnits, "otherUnits")));
    this.airbase.onClick.AddListener((UnityAction) (() => this.TabClickedAirbase(this.airbase)));
    this.typeDropdown.onValueChanged.AddListener(new UnityAction<int>(this.UnitTypeChanged));
    this.factionDropdown.onValueChanged.AddListener(new UnityAction<int>(this.FactionChanged));
    this.factionOptions = FactionHelper.GetFactionsAndNeutral();
    this.factionDropdown.ClearOptions();
    this.factionDropdown.AddOptions(this.factionOptions);
    int input = 0;
    if (SceneSingleton<MissionEditor>.i.stickyFaction != null)
      input = FactionHelper.EmptyOrNoFactionOrNeutral(SceneSingleton<MissionEditor>.i.stickyFaction) ? 0 : this.factionOptions.IndexOf(SceneSingleton<MissionEditor>.i.stickyFaction);
    this.factionDropdown.SetValueWithoutNotify(input);
    this.placeMoreToggle.onValueChanged.AddListener(new UnityAction<bool>(this.PlaceMoreChanged));
    this.positionWrapper = new ValueWrapperGlobalPosition();
    this.positionField.Setup("Position", (IValueWrapper<Vector3>) this.positionWrapper);
    this.positionField.Interactable = false;
    this.positionWrapper.SetValue(new GlobalPosition(), (object) this, true);
    this.ClearTypeDropDown();
    Button button = this.ButtonFromSticky();
    if ((UnityEngine.Object) button != (UnityEngine.Object) null)
      button.onClick.Invoke();
    this.placeMoreToggle.isOn = NewUnitPanel.stickyPlaceMore;
  }

  private Button ButtonFromSticky()
  {
    Button button;
    switch (NewUnitPanel.stickyTab)
    {
      case "airbase":
        button = this.airbase;
        break;
      case "aircraft":
        button = this.aircraft;
        break;
      case "buildings":
        button = this.buildings;
        break;
      case "missiles":
        button = this.missiles;
        break;
      case "otherUnits":
        button = this.otherUnits;
        break;
      case "scenery":
        button = this.scenery;
        break;
      case "ships":
        button = this.ships;
        break;
      case "vehicles":
        button = this.vehicles;
        break;
      default:
        button = (Button) null;
        break;
    }
    return button;
  }

  private void ClearTypeDropDown()
  {
    this.typeDropdown.options.Clear();
    this.typeDropdown.options.Add(new TMP_Dropdown.OptionData("N/A"));
    this.typeDropdown.interactable = false;
    this.typeDropdown.SetValueWithoutNotify(0);
    this.typeDropdown.RefreshShownValue();
  }

  private void SetActiveButton(Button newButton)
  {
    if ((UnityEngine.Object) this.activeButton != (UnityEngine.Object) null)
      this.activeButton.image.color = this.normal;
    this.activeButton = newButton;
    if (!((UnityEngine.Object) this.activeButton != (UnityEngine.Object) null))
      return;
    this.activeButton.image.color = this.highlight;
  }

  private void UnitTypeChanged(int index)
  {
    this.activeProvider.StickyOption = index;
    this.SpawnUnit(this.activeProvider.UnitDefinitions[index]);
  }

  private void FactionChanged(int index)
  {
    SceneSingleton<MissionEditor>.i.stickyFaction = this.factionOptions[index];
    if ((UnityEngine.Object) this.placingFlag != (UnityEngine.Object) null)
      this.placingFlag.SetColor(this.FactionColor());
    this.SetLiveryIfPlacingAircraft();
  }

  private void PlaceMoreChanged(bool toggle)
  {
    NewUnitPanel.stickyPlaceMore = this.placeMoreToggle.isOn;
    this.placeTextHint.text = toggle ? "Place multiple units" : "Select unit after placing (shift click to place more)";
  }

  private void TabClickedUnit(Button button, string unitType)
  {
    this.SetActiveButton(button);
    this.activeProvider = NewUnitPanel.unitProviders[unitType];
    NewUnitPanel.stickyTab = unitType;
    this.typeDropdown.options.Clear();
    foreach (UnitDefinition unitDefinition in this.activeProvider.UnitDefinitions)
      this.typeDropdown.options.Add(new TMP_Dropdown.OptionData(unitDefinition.unitName));
    this.typeDropdown.interactable = true;
    this.typeDropdown.SetValueWithoutNotify(this.activeProvider.StickyOption);
    this.typeDropdown.RefreshShownValue();
    this.SpawnUnit(this.activeProvider.UnitDefinitions[this.activeProvider.StickyOption]);
  }

  private void SetLiveryIfPlacingAircraft()
  {
    Aircraft component;
    if (!this.placingObject.TryGetComponent<Aircraft>(out component))
      return;
    List<(LiveryKey, string)> valueTupleList = new List<(LiveryKey, string)>();
    AircraftSelectionMenu.GetLiveryOptions(valueTupleList, component.definition, SceneSingleton<MissionEditor>.i.stickyFaction, true);
    this.aircraftLiver = valueTupleList.FirstOrDefault<(LiveryKey, string)>().Item1;
    component.SetLiveryKey(this.aircraftLiver, true);
  }

  private void SpawnUnit(UnitDefinition unitDefinition)
  {
    this.CancelPlace();
    this.placeType = NewUnitPanel.PlaceType.Unit;
    this.placingDefinition = unitDefinition;
    this.placingObject = UnityEngine.Object.Instantiate<GameObject>(unitDefinition.unitPrefab);
    this.SetLiveryIfPlacingAircraft();
    Vector3 placementUpAxis = SceneSingleton<UnitSelection>.i.GetPlacementUpAxis(unitDefinition);
    this.placingObject.transform.rotation = Quaternion.AngleAxis(SceneSingleton<CameraStateManager>.i.transform.eulerAngles.y, placementUpAxis);
    foreach (Collider componentsInChild in this.placingObject.GetComponentsInChildren<Collider>())
      componentsInChild.enabled = false;
    SceneSingleton<UnitSelection>.i.StartPlaceUnit((IPlacingMenu) this);
  }

  private void TabClickedAirbase(Button button)
  {
    this.SetActiveButton(button);
    this.CancelPlace();
    this.ClearTypeDropDown();
    this.placeType = NewUnitPanel.PlaceType.Airbase;
    this.placingFlag = MissionEditor.CreateFlag(this.FactionColor(), this.placeFlagSize);
    this.placingObject = this.placingFlag.gameObject;
    SceneSingleton<UnitSelection>.i.StartPlaceUnit((IPlacingMenu) this);
  }

  private Color FactionColor()
  {
    Faction faction = FactionRegistry.FactionFromName(SceneSingleton<MissionEditor>.i.stickyFaction);
    return !((UnityEngine.Object) faction != (UnityEngine.Object) null) ? Color.white : faction.color;
  }

  private void OnDestroy() => this.CancelPlace();

  public void CancelPlace()
  {
    if ((UnityEngine.Object) this.placingObject != (UnityEngine.Object) null)
      UnityEngine.Object.Destroy((UnityEngine.Object) this.placingObject);
    if (this.placeType != NewUnitPanel.PlaceType.None)
      SceneSingleton<UnitSelection>.i.StopPlacingUnit((IPlacingMenu) this);
    this.placingObject = (GameObject) null;
    this.placingFlag = (AirbaseEditorFlag) null;
    this.placeType = NewUnitPanel.PlaceType.None;
  }

  void IPlacingMenu.CancelPlace()
  {
    this.CancelPlace();
    this.ClearTypeDropDown();
    this.positionWrapper.SetValue(new GlobalPosition(), (object) this, true);
    this.SetActiveButton((Button) null);
  }

  (bool placeMore, IEditorSelectable placedObject) IPlacingMenu.Place(bool shift)
  {
    switch (this.placeType)
    {
      case NewUnitPanel.PlaceType.Unit:
        return this.PlaceUnit(shift);
      case NewUnitPanel.PlaceType.Airbase:
        return this.PlaceAirbase(shift);
      default:
        throw new Exception("Place should not be called when no object is being placed");
    }
  }

  private (bool placeMore, IEditorSelectable placedObject) PlaceUnit(bool shift)
  {
    Vector3 position;
    Quaternion rotation;
    this.placingObject.transform.GetPositionAndRotation(out position, out rotation);
    GlobalPosition globalPosition = position.ToGlobalPosition();
    FactionHQ factionHq = FactionRegistry.HqFromName(SceneSingleton<MissionEditor>.i.stickyFaction);
    string name = this.placingDefinition.unitPrefab.name;
    SaveHelper.MakeUnique(ref name, (IEnumerable<ISaveableReference>) MissionManager.GetAllSavedUnits(true), false);
    Unit unit = NetworkSceneSingleton<Spawner>.i.SpawnFromUnitDefinitionInEditor(this.placingDefinition, globalPosition, rotation, factionHq, name);
    Physics.SyncTransforms();
    SavedUnit savedUnit = SceneSingleton<MissionEditor>.i.RegisterNewUnit(unit, name);
    Aircraft component1;
    if (unit.TryGetComponent<Aircraft>(out component1))
    {
      ((SavedAircraft) savedUnit).liveryKey = this.aircraftLiver;
      component1.SetLiveryKey(this.aircraftLiver, true);
    }
    Airbase component2;
    if (unit.TryGetComponent<Airbase>(out component2))
      MissionEditor.CreateFlagForAirbase(component2);
    int num = shift ? 1 : (this.placeMoreToggle.isOn ? 1 : 0);
    if (num == 0)
      this.CancelPlace();
    return (num != 0, (IEditorSelectable) unit);
  }

  private (bool placeMore, IEditorSelectable placedObject) PlaceAirbase(bool shift)
  {
    Mission currentMission = MissionManager.CurrentMission;
    SavedAirbase saved = new SavedAirbase()
    {
      SavedInMission = true,
      UniqueName = AirbasePanel.NewAirbaseUniqueName((string) null)
    };
    saved.DisplayName = saved.UniqueName;
    saved.faction = SceneSingleton<MissionEditor>.i.stickyFaction;
    currentMission.airbases.Add(saved);
    Airbase airbase = currentMission.SpawnCustomAirbase(saved, NetworkManagerNuclearOption.i.ServerObjectManager);
    MissionEditor.CreateFlagForAirbase(airbase);
    GlobalPosition globalPosition = SceneSingleton<UnitSelection>.i.placementTransform.GlobalPosition();
    saved.CenterWrapper.SetValue(globalPosition, (object) this, true);
    saved.SelectionPositionWrapper.SetValue(globalPosition, (object) this, true);
    int num = shift ? 1 : (this.placeMoreToggle.isOn ? 1 : 0);
    if (num == 0)
      this.CancelPlace();
    return (num != 0, (IEditorSelectable) airbase);
  }

  void IPlacingMenu.MoveCursor(Transform placementTransform)
  {
    switch (this.placeType)
    {
      case NewUnitPanel.PlaceType.Unit:
        Vector3 position1 = placementTransform.position + placementTransform.up * this.placingDefinition.spawnOffset.y;
        Quaternion rotation = Quaternion.AngleAxis(SceneSingleton<CameraStateManager>.i.transform.eulerAngles.y, placementTransform.up);
        if (this.placingDefinition is BuildingDefinition)
          rotation = Quaternion.LookRotation(new Vector3(SceneSingleton<CameraStateManager>.i.transform.forward.x, 0.0f, SceneSingleton<CameraStateManager>.i.transform.forward.z), Vector3.up);
        this.placingObject.transform.SetPositionAndRotation(position1, rotation);
        this.positionWrapper.SetValue(position1.ToGlobalPosition(), (object) this, true);
        break;
      case NewUnitPanel.PlaceType.Airbase:
        Vector3 position2 = placementTransform.position;
        this.placingObject.transform.position = position2;
        this.positionWrapper.SetValue(position2.ToGlobalPosition(), (object) this, true);
        break;
    }
  }

  private enum PlaceType
  {
    None,
    Unit,
    Airbase,
  }

  private class UnitOptionProvider
  {
    public readonly UnitDefinition[] UnitDefinitions;
    public int StickyOption;

    private UnitOptionProvider(UnitDefinition[] unitDefinitions)
    {
      this.UnitDefinitions = unitDefinitions;
      this.StickyOption = 0;
    }

    public static NewUnitPanel.UnitOptionProvider Create<T>(List<T> definitionOptions) where T : UnitDefinition
    {
      List<UnitDefinition> unitDefinitionList = new List<UnitDefinition>(definitionOptions.Count);
      foreach (T definitionOption in definitionOptions)
      {
        if (!definitionOption.disabled)
          unitDefinitionList.Add((UnitDefinition) definitionOption);
      }
      unitDefinitionList.Sort((Comparison<UnitDefinition>) ((x, y) => x.unitName.CompareTo(y.unitName)));
      return new NewUnitPanel.UnitOptionProvider(unitDefinitionList.ToArray());
    }
  }
}

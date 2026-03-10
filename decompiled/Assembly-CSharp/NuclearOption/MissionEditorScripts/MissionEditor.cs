// Decompiled with JetBrains decompiler
// Type: NuclearOption.MissionEditorScripts.MissionEditor
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Cysharp.Threading.Tasks;
using NuclearOption.Networking;
using NuclearOption.SavedMission;
using NuclearOption.SavedMission.ObjectiveV2;
using NuclearOption.SceneLoading;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

#nullable disable
namespace NuclearOption.MissionEditorScripts;

public class MissionEditor : SceneSingleton<MissionEditor>
{
  private static PositionRotation playFromEditorCameraPosition;
  [NonSerialized]
  public string stickyFaction;
  [Header("References")]
  [SerializeField]
  private MapLoader mapLoader;
  [SerializeField]
  private GameObject cameraNavigator;
  [SerializeField]
  private EditorTabs tabs;
  [SerializeField]
  private UnitSelection unitSelection;
  [SerializeField]
  private UnitCopyPaste copyPaste;
  [SerializeField]
  private MissionEditorPopupMenus popupMenus;
  [SerializeField]
  private MissionLoadErrorPanel loadErrorPanel;
  [SerializeField]
  private GameObject airbaseFlagPrefab;
  [SerializeField]
  private GameObject airbaseRadiusPrefab;
  [Header("Camera Clipping")]
  public bool allowCameraClip;
  [SerializeField]
  private Button cameraClipToggleButton;
  [SerializeField]
  private Image cameraClipImage;
  [SerializeField]
  private Sprite cameraClip;
  [SerializeField]
  private Sprite cameraNoClip;
  [Header("Input Fields")]
  [SerializeField]
  private InputField missionNameInput;
  [SerializeField]
  private Toggle autoSaveToggle;
  private readonly HashSet<Unit> editorUnits = new HashSet<Unit>();

  protected override void Awake()
  {
    base.Awake();
    this.gameObject.AddComponent<InputFieldChecker>();
    this.autoSaveToggle.isOn = PlayerPrefs.GetInt("EditorAutoSave", 1) == 1;
    this.autoSaveToggle.onValueChanged.AddListener((UnityAction<bool>) (_ => PlayerPrefs.SetInt("EditorAutoSave", this.autoSaveToggle.isOn ? 1 : 0)));
    this.missionNameInput.onEndEdit.AddListener((UnityAction<string>) (_ => this.CheckAutoSave()));
    this.cameraClipToggleButton.onClick.AddListener(new UnityAction(this.ToggleCameraClip));
    this.allowCameraClip = PlayerPrefs.GetInt("EditorAllowCameraClip", this.allowCameraClip ? 1 : 0) == 1;
    this.SetCameraClip(this.allowCameraClip);
  }

  private void Start() => Time.timeScale = 0.0f;

  public void RemoveUnit(Unit unit)
  {
    if ((UnityEngine.Object) unit == (UnityEngine.Object) null || !this.editorUnits.Contains(unit))
      return;
    SavedUnit savedUnit = unit.SavedUnit;
    Mission currentMission = MissionManager.CurrentMission;
    if (unit is Aircraft)
      currentMission.aircraft.Remove((SavedAircraft) savedUnit);
    if (unit is GroundVehicle)
      currentMission.vehicles.Remove((SavedVehicle) savedUnit);
    if (unit is Ship)
      currentMission.ships.Remove((SavedShip) savedUnit);
    if (unit is Building)
      currentMission.buildings.Remove((SavedBuilding) savedUnit);
    if (unit is Scenery)
      currentMission.scenery.Remove((SavedScenery) savedUnit);
    if (unit is Container)
      currentMission.containers.Remove((SavedContainer) savedUnit);
    if (unit is Missile)
      currentMission.missiles.Remove((SavedMissile) savedUnit);
    if (unit is PilotDismounted)
      currentMission.pilots.Remove((SavedPilot) savedUnit);
    Airbase component;
    if (unit.TryGetComponent<Airbase>(out component))
    {
      string uniqueName = component.SavedAirbase.UniqueName;
      foreach (SavedAirbase airbase in currentMission.airbases)
      {
        if (airbase.UniqueName == uniqueName)
        {
          currentMission.airbases.Remove(airbase);
          break;
        }
      }
    }
    if (savedUnit.PlacementType == PlacementType.Custom)
      currentMission.Objectives.ReferenceDestroyed((ISaveableReference) savedUnit);
    this.editorUnits.Remove(unit);
    if (savedUnit.PlacementType == PlacementType.Custom)
    {
      unit.DisableUnit();
      UnityEngine.Object.Destroy((UnityEngine.Object) unit.gameObject);
      this.unitSelection.ClearIfSelected((IEditorSelectable) unit);
    }
    if (savedUnit.PlacementType == PlacementType.Override)
      unit.RemoveSavedUnitOverride(true);
    for (int index = 0; index < currentMission.unitInventories.Count; ++index)
    {
      if (currentMission.unitInventories[index].AttachedUnitUniqueName == savedUnit.UniqueName)
      {
        currentMission.unitInventories.RemoveAt(index);
        break;
      }
    }
    this.CheckAutoSave();
  }

  public void RemoveUnit(SavedUnit saved) => this.RemoveUnit(saved?.Unit);

  public static bool RemoveAirbase(Airbase airbase)
  {
    if (!airbase.SavedAirbaseOverride)
      return false;
    SavedAirbase savedAirbase = airbase.SavedAirbase;
    MissionManager.CurrentMission.airbases.Remove(savedAirbase);
    airbase.UnlinkSavedAirbase();
    if (savedAirbase != null)
    {
      foreach (SavedBuilding savedBuilding in savedAirbase.BuildingsRef.ToList<SavedBuilding>())
        savedBuilding.RemoveAirbase();
    }
    if (!airbase.BuiltIn && !airbase.AttachedAirbase)
      UnityEngine.Object.Destroy((UnityEngine.Object) airbase.gameObject);
    return true;
  }

  public void ToggleCameraClip() => this.SetCameraClip(!this.allowCameraClip);

  public void SetCameraClip(bool value)
  {
    if (this.allowCameraClip != value)
      PlayerPrefs.SetInt("EditorAllowCameraClip", value ? 1 : 0);
    this.allowCameraClip = value;
    this.cameraClipImage.sprite = this.allowCameraClip ? this.cameraClip : this.cameraNoClip;
  }

  public static async UniTask LoadEditor(NewMissionConfig config)
  {
    MissionManager.NewMission(config);
    await MissionEditor.LoadEditor(MissionManager.CurrentMission);
  }

  public static async UniTask LoadEditor(Mission mission)
  {
    if (GameManager.gameState == GameState.Menu)
    {
      await NetworkManagerNuclearOption.i.StartHostAsync(new HostOptions(SocketType.Offline, GameState.Editor, mission.MapKey));
      SceneSingleton<MissionEditor>.i.OnLoadMission(mission);
    }
    else
      await MissionEditor.LoadMissionAsync(mission);
  }

  public static async UniTask PlayFromEditor()
  {
    Mission mission;
    LoadingScreen loadingScreen;
    string loadErrors;
    if (!MissionSaveLoad.SaveMissionTemp(MissionManager.CurrentMission, true, out mission, out loadErrors))
    {
      Debug.LogError((object) ("Failed to reload mission " + loadErrors));
      mission = (Mission) null;
      loadingScreen = (LoadingScreen) null;
    }
    else
    {
      SceneSingleton<CameraStateManager>.i.GetCameraPosition(out MissionEditor.playFromEditorCameraPosition);
      loadingScreen = LoadingScreen.GetLoadingScreen();
      loadingScreen.ShowLoadingScreen();
      loadingScreen.SetProgressRange(0.0f, 0.3f);
      await MissionEditor.ExitEditor();
      await UniTask.Yield();
      MissionManager.SetMission(mission, false);
      UniTask uniTask = NetworkManagerNuclearOption.i.StartHostAsync(new HostOptions(SocketType.Offline, GameState.SinglePlayer, mission.MapKey));
      GameManager.IsPlayingFromEditor = true;
      loadingScreen.SetProgressRange(0.3f, 1f);
      await uniTask;
      if (SceneSingleton<CameraStateManager>.i.currentState is CameraFreeState)
        SceneSingleton<CameraStateManager>.i.SetCameraPosition(MissionEditor.playFromEditorCameraPosition);
      loadingScreen.HideLoadingScreen();
      mission = (Mission) null;
      loadingScreen = (LoadingScreen) null;
    }
  }

  public static async UniTask ReturnToEditor()
  {
    Mission mission;
    LoadingScreen loadingScreen;
    string loadErrors;
    if (!MissionSaveLoad.LoadMissionTemp(MissionManager.CurrentMission, out mission, out loadErrors))
    {
      Debug.LogError((object) ("Failed to reload mission " + loadErrors));
      mission = (Mission) null;
      loadingScreen = (LoadingScreen) null;
    }
    else
    {
      loadingScreen = LoadingScreen.GetLoadingScreen();
      loadingScreen.ShowLoadingScreen();
      loadingScreen.SetProgressRange(0.0f, 0.3f);
      Time.timeScale = 0.0f;
      UniTask uniTask = NetworkManagerNuclearOption.i.StopAsync(true);
      await uniTask;
      MissionManager.SetMission(mission, false);
      loadingScreen.SetProgressRange(0.3f, 1f);
      uniTask = MissionEditor.LoadEditor(mission);
      await uniTask;
      GameManager.ResetGameResolution();
      if (SceneSingleton<CameraStateManager>.i.currentState is CameraFreeState)
        SceneSingleton<CameraStateManager>.i.SetCameraPosition(MissionEditor.playFromEditorCameraPosition);
      loadingScreen.HideLoadingScreen();
      mission = (Mission) null;
      loadingScreen = (LoadingScreen) null;
    }
  }

  public static async UniTask ExitEditor()
  {
    Time.timeScale = 1f;
    await NetworkManagerNuclearOption.i.StopAsync(true);
  }

  private static async UniTask LoadMissionAsync(Mission missionToLoad)
  {
    UniTask uniTask;
    switch (await SceneSingleton<MissionEditor>.i.mapLoader.Load(missionToLoad.MapKey))
    {
      case MapLoader.LoadResult.ChangedScene:
        ColorLog<MissionEditor>.Info("Setting up new MissionEditor after scene change");
        NetworkManagerNuclearOption.i.Host_SceneLoaded();
        break;
      case MapLoader.LoadResult.ChangedWorldPrefab:
      case MapLoader.LoadResult.AlreadyLoaded:
        ColorLog<MissionEditor>.Info("Cleaning up existing scene");
        uniTask = SceneSingleton<MissionEditor>.i.CleanupCurrentMission();
        await uniTask;
        break;
      default:
        Debug.LogError((object) "Failed to load mission");
        return;
    }
    uniTask = UniTask.DelayFrame(2);
    await uniTask;
    SceneSingleton<MissionEditor>.i.OnLoadMission(missionToLoad);
  }

  private void OnLoadMission(Mission missionToLoad)
  {
    MissionObjectivesFactory.AssertSceneAirbaseRegistered();
    this.missionNameInput.SetTextWithoutNotify(missionToLoad.Name);
    MissionManager.SetMission(missionToLoad, true);
    LoadErrors loadErrors;
    try
    {
      missionToLoad.OnSceneLoaded(NetworkSceneSingleton<MissionManager>.i);
      loadErrors = missionToLoad.Objectives.LoadErrors;
    }
    catch (MissionLoadException ex)
    {
      Debug.LogError((object) "Failed to load mission");
      loadErrors = ex.LoadErrors;
    }
    loadErrors.LogAllErrors();
    this.loadErrorPanel.SetErrors(loadErrors);
    NetworkSceneSingleton<Spawner>.i.SpawnFromMissionInEditor(missionToLoad, new Action<Unit, SavedUnit>(this.RegisterUnit));
    NetworkSceneSingleton<LevelInfo>.i.LoadFromMission(missionToLoad);
    this.tabs.HideTab(true);
    this.popupMenus.CloseMenu();
    foreach (Airbase airbase in FactionRegistry.airbaseLookup.Values)
      MissionEditor.CreateFlagForAirbase(airbase);
  }

  public static AirbaseEditorFlag CreateFlag(Color color, float scale)
  {
    return AirbaseEditorFlag.Create((Transform) null, SceneSingleton<MissionEditor>.i.airbaseFlagPrefab, color, scale);
  }

  public static void CreateFlagForAirbase(Airbase airbase)
  {
    Color colorOrGray = airbase.CurrentHQ.GetColorOrGray();
    AirbaseEditorFlag componentInChildren1 = airbase.center.GetComponentInChildren<AirbaseEditorFlag>();
    if ((UnityEngine.Object) componentInChildren1 != (UnityEngine.Object) null)
    {
      componentInChildren1.SetColor(colorOrGray);
    }
    else
    {
      AirbaseEditorFlag airbaseEditorFlag = AirbaseEditorFlag.Create(airbase.center, SceneSingleton<MissionEditor>.i.airbaseFlagPrefab, colorOrGray, 5f);
      EditorSelectableProxy.Add((IEditorSelectable) airbase, airbaseEditorFlag.gameObject);
    }
    AirbaseEditorRadius componentInChildren2 = airbase.center.GetComponentInChildren<AirbaseEditorRadius>();
    if ((UnityEngine.Object) componentInChildren2 == (UnityEngine.Object) null)
      componentInChildren2 = AirbaseEditorRadius.Create(airbase.center, SceneSingleton<MissionEditor>.i.airbaseRadiusPrefab);
    componentInChildren2.Setup(colorOrGray, airbase.GetRadius());
  }

  private async UniTask CleanupCurrentMission()
  {
    bool flag = false;
    foreach (Unit editorUnit in this.editorUnits)
    {
      if (!editorUnit.BuiltIn)
      {
        flag = true;
        NetworkManagerNuclearOption.i.ServerObjectManager.Destroy(editorUnit.Identity, !editorUnit.Identity.IsSceneObject);
      }
    }
    this.editorUnits.Clear();
    foreach (Airbase airbase in FactionRegistry.airbaseLookup.Values.ToArray<Airbase>())
    {
      if (!((UnityEngine.Object) airbase == (UnityEngine.Object) null))
      {
        if ((airbase.BuiltIn ? 0 : (!airbase.AttachedAirbase ? 1 : 0)) != 0)
        {
          flag = true;
          NetworkManagerNuclearOption.i.ServerObjectManager.Destroy(airbase.Identity, !airbase.Identity.IsSceneObject);
        }
        else if (airbase.SavedAirbaseOverride)
          airbase.UnlinkSavedAirbase();
      }
    }
    if (flag)
      await UniTask.Yield();
    foreach (Unit allUnit in UnitRegistry.allUnits)
      allUnit.EditorMapCleanup();
    MissionObjectivesFactory.AssertSceneAirbaseRegistered();
  }

  private void RegisterUnit(Unit unit, SavedUnit savedUnit)
  {
    this.editorUnits.Add(unit);
    if (unit.SavedUnit != savedUnit)
      unit.LinkSavedUnit(savedUnit);
    savedUnit.AfterLoadEditor();
  }

  public SavedUnit RegisterNewUnit(Unit unit, string uniqueName)
  {
    SavedUnit savedUnit = this.CreateSavedUnit(unit);
    savedUnit.AfterCreate(unit, uniqueName);
    this.RegisterUnit(unit, savedUnit);
    return savedUnit;
  }

  public void AddUnitOverride(Unit unit, SavedUnit savedUnit)
  {
    this.AddSavedUnit(savedUnit);
    this.editorUnits.Add(unit);
    savedUnit.AfterAddOverride(unit);
  }

  private SavedUnit CreateSavedUnit(Unit unit)
  {
    switch (unit)
    {
      case Aircraft _:
        SavedAircraft savedUnit1 = new SavedAircraft();
        MissionManager.CurrentMission.aircraft.Add(savedUnit1);
        return (SavedUnit) savedUnit1;
      case GroundVehicle _:
        SavedVehicle savedUnit2 = new SavedVehicle();
        MissionManager.CurrentMission.vehicles.Add(savedUnit2);
        return (SavedUnit) savedUnit2;
      case Ship _:
        SavedShip savedUnit3 = new SavedShip();
        MissionManager.CurrentMission.ships.Add(savedUnit3);
        return (SavedUnit) savedUnit3;
      case Building _:
        SavedBuilding savedUnit4 = new SavedBuilding();
        MissionManager.CurrentMission.buildings.Add(savedUnit4);
        return (SavedUnit) savedUnit4;
      case Scenery _:
        SavedScenery savedUnit5 = new SavedScenery();
        MissionManager.CurrentMission.scenery.Add(savedUnit5);
        return (SavedUnit) savedUnit5;
      case Container _:
        SavedContainer savedUnit6 = new SavedContainer();
        MissionManager.CurrentMission.containers.Add(savedUnit6);
        return (SavedUnit) savedUnit6;
      case Missile _:
        SavedMissile savedUnit7 = new SavedMissile();
        MissionManager.CurrentMission.missiles.Add(savedUnit7);
        return (SavedUnit) savedUnit7;
      case PilotDismounted _:
        SavedPilot savedUnit8 = new SavedPilot();
        MissionManager.CurrentMission.pilots.Add(savedUnit8);
        return (SavedUnit) savedUnit8;
      default:
        throw new ArgumentException($"Can't create saved unit for unit type: {unit?.GetType()}");
    }
  }

  private void AddSavedUnit(SavedUnit saved)
  {
    switch (saved)
    {
      case SavedAircraft savedAircraft:
        MissionManager.CurrentMission.aircraft.Add(savedAircraft);
        break;
      case SavedVehicle savedVehicle:
        MissionManager.CurrentMission.vehicles.Add(savedVehicle);
        break;
      case SavedShip savedShip:
        MissionManager.CurrentMission.ships.Add(savedShip);
        break;
      case SavedBuilding savedBuilding:
        MissionManager.CurrentMission.buildings.Add(savedBuilding);
        break;
      case SavedScenery savedScenery:
        MissionManager.CurrentMission.scenery.Add(savedScenery);
        break;
      case SavedContainer savedContainer:
        MissionManager.CurrentMission.containers.Add(savedContainer);
        break;
      case SavedMissile savedMissile:
        MissionManager.CurrentMission.missiles.Add(savedMissile);
        break;
      case SavedPilot savedPilot:
        MissionManager.CurrentMission.pilots.Add(savedPilot);
        break;
      default:
        throw new ArgumentException($"Can't add SavedUnit of type {saved?.GetType()}");
    }
  }

  public void CheckAutoSave()
  {
  }
}

// Decompiled with JetBrains decompiler
// Type: MissionManager
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Cysharp.Threading.Tasks;
using Mirage;
using Mirage.Collections;
using Mirage.Serialization;
using NuclearOption.MissionEditorScripts;
using NuclearOption.Networking;
using NuclearOption.SavedMission;
using NuclearOption.SavedMission.ObjectiveV2;
using NuclearOption.SceneLoading;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;

#nullable disable
public class MissionManager : NetworkSceneSingleton<MissionManager>
{
  private static readonly List<SavedUnit> unitCache = new List<SavedUnit>();
  private static readonly List<SavedBuilding> buildingCache = new List<SavedBuilding>();
  private static readonly List<SavedUnit> unitCache2 = new List<SavedUnit>();
  private static readonly List<SavedAirbase> airbaseCache = new List<SavedAirbase>();
  [SyncVar]
  [NonSerialized]
  public float tacticalThreshold;
  [SyncVar]
  [NonSerialized]
  public float tacticalMinRank;
  [SyncVar]
  [NonSerialized]
  public float strategicThreshold;
  [SyncVar]
  [NonSerialized]
  public float strategicMinRank;
  [SyncVar]
  [NonSerialized]
  public float currentEscalation;
  private readonly SyncList<string> activeObjectiveNames = new SyncList<string>();
  private readonly SyncDictionary<string, List<int>> activeObjectiveData = new SyncDictionary<string, List<int>>();
  private static bool clientEventsAdded;
  private bool OnSyncMissionChangedRunningFromOnStartClient;
  [SyncVar]
  [NonSerialized]
  public double multiplayerStartTime;
  [SyncVar]
  [NonSerialized]
  public int wrecksMaxNumber = -1;
  [SyncVar]
  [NonSerialized]
  public float wrecksDecayTime = -1f;
  public List<Wreckage> listWrecks = new List<Wreckage>();
  [NonSerialized]
  private const int SYNC_VAR_COUNT = 8;
  [NonSerialized]
  private const int RPC_COUNT = 0;

  public static bool IsRunning { get; private set; }

  public static Mission CurrentMission { get; private set; }

  public static MissionObjectives Objectives => MissionManager.CurrentMission.Objectives;

  public static MissionRunner Runner { get; private set; }

  public static event Action<Mission> onMissionLoad;

  public static event Action<Objective> onObjectiveStarted;

  public static event Action<Objective> onObjectiveComplete;

  public float MissionTime
  {
    get
    {
      return GameManager.gameState == GameState.Multiplayer ? (float) (this.NetworkTime.Time - this.multiplayerStartTime) : Time.timeSinceLevelLoad;
    }
  }

  public static event Action<Mission> OnEditorOrPickerMissionChanged
  {
    add
    {
      if ((UnityEngine.Object) SceneSingleton<MissionEditor>.i != (UnityEngine.Object) null)
      {
        MissionManager.onMissionLoad += value;
        value(MissionManager.CurrentMission);
      }
      else
      {
        if (!((UnityEngine.Object) SceneSingleton<MissionsPicker>.i != (UnityEngine.Object) null))
          return;
        SceneSingleton<MissionsPicker>.i.OnMissionSelect += value;
        value(SceneSingleton<MissionsPicker>.i.Mission);
      }
    }
    remove
    {
      MissionManager.onMissionLoad -= value;
      if (!((UnityEngine.Object) SceneSingleton<MissionsPicker>.i != (UnityEngine.Object) null))
        return;
      SceneSingleton<MissionsPicker>.i.OnMissionSelect -= value;
    }
  }

  public static bool AllowTactical()
  {
    return (double) NetworkSceneSingleton<MissionManager>.i.currentEscalation >= (double) NetworkSceneSingleton<MissionManager>.i.tacticalThreshold;
  }

  public static bool AllowStrategic()
  {
    return (double) NetworkSceneSingleton<MissionManager>.i.currentEscalation >= (double) NetworkSceneSingleton<MissionManager>.i.strategicThreshold;
  }

  private void ClientDisconnected(ClientStoppedReason _) => this.StopMission();

  private void StopMission()
  {
    MissionManager.IsRunning = false;
    NetworkManagerNuclearOption.i.Client.Disconnected.RemoveListener(new UnityAction<ClientStoppedReason>(this.ClientDisconnected));
  }

  public static void NewMission(NewMissionConfig config)
  {
    Mission mission = new Mission(config.Name);
    mission.missionSettings.playerMode = config.PlayerMode;
    mission.MapKey = config.Map;
    if (mission.factions.Count == 0)
    {
      foreach (Faction defaultFaction in GameAssets.i.defaultFactions)
        mission.EnsureFactionExists(defaultFaction, out MissionFaction _);
    }
    MissionManager.SetMission(mission, false);
    foreach (MissionFaction faction in mission.factions)
    {
      bool flag = config.CanJoinAllFactions || config.JoinableFactions.Contains(faction.factionName);
      faction.preventJoin = !flag;
    }
  }

  public static void SetNullMission() => MissionManager.SetMission(Mission.NullMission, false);

  public static void SetMission(Mission mission, bool checkIfSame)
  {
    if (checkIfSame && MissionManager.CurrentMission == mission)
    {
      ColorLog<MissionManager>.Info("Skipping set mission because missions are the same");
    }
    else
    {
      MissionManager.IsRunning = false;
      if (MissionManager.CurrentMission != null)
        MissionManager.UnloadMission();
      ColorLog<MissionManager>.Info($"Setting CurrentMission, old:{MissionManager.CurrentMission?.Name} new:{mission?.Name}");
      MissionManager.CurrentMission = mission;
      Action<Mission> onMissionLoad = MissionManager.onMissionLoad;
      if (onMissionLoad != null)
        onMissionLoad(MissionManager.CurrentMission);
      if (NetworkManagerNuclearOption.i.Server.Active)
        NetworkManagerNuclearOption.i.SetNetworkMission(MissionManager.CurrentMission, NetworkMission.State.Loaded);
      if (!((UnityEngine.Object) NetworkSceneSingleton<MissionManager>.i != (UnityEngine.Object) null) || !NetworkSceneSingleton<MissionManager>.i.IsServer)
        return;
      NetworkSceneSingleton<MissionManager>.i.SetupInstanceValues();
    }
  }

  private static void UnloadMission()
  {
    ColorLog<MissionManager>.Info(nameof (UnloadMission));
    if (MissionManager.Runner != null)
    {
      MissionManager.Runner.OnObjectiveStart -= new Action<Objective>(NetworkSceneSingleton<MissionManager>.i.ServerObjectiveStarted);
      MissionManager.Runner.OnObjectiveCompleted -= new Action<Objective>(NetworkSceneSingleton<MissionManager>.i.ServerObjectiveComplete);
    }
    MissionManager.Runner = (MissionRunner) null;
    if (!((UnityEngine.Object) NetworkSceneSingleton<MissionManager>.i != (UnityEngine.Object) null))
      return;
    NetworkManagerNuclearOption.i.NetworkMission.Clear();
    if (NetworkSceneSingleton<MissionManager>.i.IsServer)
    {
      NetworkSceneSingleton<MissionManager>.i.activeObjectiveNames.Clear();
      NetworkSceneSingleton<MissionManager>.i.activeObjectiveData.Clear();
    }
    else
    {
      NetworkSceneSingleton<MissionManager>.i.activeObjectiveNames.Reset();
      NetworkSceneSingleton<MissionManager>.i.activeObjectiveData.Reset();
    }
  }

  public static void StartMission()
  {
    ColorLog<MissionManager>.Info(nameof (StartMission));
    try
    {
      MissionManager.CurrentMission.OnSceneLoaded(NetworkSceneSingleton<MissionManager>.i);
      MissionManager.CurrentMission.Objectives.LoadErrors.LogAllErrors();
    }
    catch (MissionLoadException ex)
    {
      Debug.LogError((object) "Failed to load mission");
      LoadErrors loadErrors = ex.LoadErrors;
      loadErrors.LogAllErrors();
      GameManager.SetDisconnectReason(new DisconnectInfo($"Mission '{MissionManager.CurrentMission.Name}' failed to load because of {loadErrors.Exceptions.Count} error(s)."));
      NetworkManagerNuclearOption.i.Stop(false);
      return;
    }
    NetworkSceneSingleton<LevelInfo>.i.SetStartingCamera(MissionManager.CurrentMission);
    MissionObjectivesFactory.AddStartingUnits(MissionManager.CurrentMission.Objectives, MissionManager.GetAllSavedUnits(MissionManager.CurrentMission, false));
    MissionManager.IsRunning = true;
    MissionManager.Runner = new MissionRunner(MissionManager.CurrentMission.Objectives);
    if (!NetworkSceneSingleton<MissionManager>.i.IsServer)
      return;
    NetworkSceneSingleton<MissionManager>.i.Client.Disconnected.AddListener(new UnityAction<ClientStoppedReason>(NetworkSceneSingleton<MissionManager>.i.ClientDisconnected));
    MissionManager.Runner.OnObjectiveStart += new Action<Objective>(NetworkSceneSingleton<MissionManager>.i.ServerObjectiveStarted);
    MissionManager.Runner.OnObjectiveCompleted += new Action<Objective>(NetworkSceneSingleton<MissionManager>.i.ServerObjectiveComplete);
    MissionManager.Runner.OnMissionStart();
    NetworkSceneSingleton<MissionManager>.i.NetworktacticalThreshold = MissionManager.CurrentMission.missionSettings.nuclearEscalationThreshold;
    NetworkSceneSingleton<MissionManager>.i.NetworktacticalMinRank = (float) MissionManager.CurrentMission.missionSettings.minRankTacticalWarhead;
    NetworkSceneSingleton<MissionManager>.i.NetworkstrategicThreshold = MissionManager.CurrentMission.missionSettings.strategicEscalationThreshold;
    NetworkSceneSingleton<MissionManager>.i.NetworkstrategicMinRank = (float) MissionManager.CurrentMission.missionSettings.minRankStrategicWarhead;
    NetworkSceneSingleton<MissionManager>.i.NetworkcurrentEscalation = 0.0f;
    NetworkSceneSingleton<MissionManager>.i.NetworkmultiplayerStartTime = NetworkSceneSingleton<MissionManager>.i.NetworkTime.Time;
    NetworkSceneSingleton<MissionManager>.i.NetworkwrecksMaxNumber = MissionManager.CurrentMission.missionSettings.wrecksMaxNumber;
    NetworkSceneSingleton<MissionManager>.i.NetworkwrecksDecayTime = MissionManager.CurrentMission.missionSettings.wrecksDecayTime;
    MissionManager.Runner.Update();
    NetworkManagerNuclearOption.i.ServerMissionStart(MissionManager.CurrentMission);
  }

  public static async UniTask RestartMission()
  {
    Mission mission = (Mission) null;
    LoadingScreen loadingScreen;
    if (MissionManager.CurrentMission.LoadKey.HasValue)
    {
      ColorLog<MissionManager>.Info($"Restarting mission, key={MissionManager.CurrentMission.LoadKey.Value}");
      string error;
      if (!MissionSaveLoad.TryLoad(MissionManager.CurrentMission.LoadKey.Value, out mission, out error))
      {
        Debug.LogError((object) ("Failed to reload mission " + error));
        mission = (Mission) null;
        loadingScreen = (LoadingScreen) null;
        return;
      }
    }
    else
      ColorLog<MissionManager>.Info("Restarting NULL mission");
    loadingScreen = LoadingScreen.GetLoadingScreen();
    loadingScreen.ShowLoadingScreen();
    loadingScreen.SetProgressRange(0.0f, 0.3f);
    bool isPlayingFromEditor = GameManager.IsPlayingFromEditor;
    Time.timeScale = 0.0f;
    await NetworkManagerNuclearOption.i.StopAsync(true);
    await UniTask.Yield();
    if (mission != null)
      MissionManager.SetMission(mission, false);
    else
      MissionManager.SetNullMission();
    NetworkManagerNuclearOption i = NetworkManagerNuclearOption.i;
    Mission mission1 = mission;
    HostOptions options = new HostOptions(SocketType.Offline, GameState.SinglePlayer, mission1 != null ? mission1.MapKey : new MapKey());
    UniTask uniTask = i.StartHostAsync(options);
    loadingScreen.SetProgressRange(0.3f, 1f);
    await uniTask;
    GameManager.IsPlayingFromEditor = isPlayingFromEditor;
    loadingScreen.HideLoadingScreen();
    mission = (Mission) null;
    loadingScreen = (LoadingScreen) null;
  }

  protected override void Awake()
  {
    base.Awake();
    this.Identity.OnStartClient.AddListener(new Action(this.OnStartClient));
    this.Identity.OnStopClient.AddListener(new Action(this.OnStopClient));
    this.Identity.OnStartServer.AddListener(new Action(this.OnStartServer));
    if (NetworkSceneSingleton<MissionManager>.i.wrecksMaxNumber <= 0)
      return;
    NetworkSceneSingleton<MissionManager>.i.StartSlowUpdate(60f, new Action(NetworkSceneSingleton<MissionManager>.i.WrecksManagement));
  }

  private void OnDestroy()
  {
    ColorLog<MissionManager>.Info(nameof (OnDestroy));
    this.OnStopClient();
    MissionManager.IsRunning = false;
    MissionManager.Runner?.Cleanup();
    MissionManager.Runner = (MissionRunner) null;
    MissionManager.SetNullMission();
    MissionManager.onMissionLoad = (Action<Mission>) null;
    MissionManager.onObjectiveStarted = (Action<Objective>) null;
    MissionManager.onObjectiveComplete = (Action<Objective>) null;
    this.listWrecks.Clear();
  }

  private void Update()
  {
    if (!MissionManager.IsRunning)
      return;
    if (this.IsServer)
    {
      MissionManager.Runner.Update();
    }
    else
    {
      if (!this.IsClient)
        return;
      MissionManager.Runner.ClientOnlyUpdate();
    }
  }

  private void WrecksManagement()
  {
    if (this.listWrecks.Count <= this.wrecksMaxNumber)
      return;
    int num = this.listWrecks.Count - this.wrecksMaxNumber;
    for (int index = 0; index < num; ++index)
      this.listWrecks[index].Deactivate();
  }

  private void OnStartServer()
  {
    if (GameManager.gameState == GameState.Editor)
      return;
    this.SetupInstanceValues();
    UniTask.Void((Func<UniTaskVoid>) (async () =>
    {
      YieldAwaitable yieldAwaitable = UniTask.Yield();
      await yieldAwaitable;
      yieldAwaitable = UniTask.Yield();
      await yieldAwaitable;
      MissionManager.StartMission();
    }));
  }

  private void SetupInstanceValues()
  {
    if (MissionManager.CurrentMission == null)
      return;
    this.activeObjectiveNames.Clear();
    this.activeObjectiveData.Clear();
  }

  private void OnStartClient()
  {
    if (this.IsServer)
      return;
    ColorLog<MissionManager>.Info("Client Start");
    this.OnSyncMissionChangedRunningFromOnStartClient = true;
    try
    {
      NetworkManagerNuclearOption.i.NetworkMission.Changed.AddListener(new Action<(NetworkMission.SyncMission, NetworkMission.State, bool)>(this.OnSyncMissionChanged));
      MissionManager.clientEventsAdded = true;
    }
    finally
    {
      this.OnSyncMissionChangedRunningFromOnStartClient = false;
    }
    this.activeObjectiveNames.OnInsert += new Action<int, string>(this.ActiveObjectiveNames_OnAdd);
    this.activeObjectiveNames.OnRemove += new Action<int, string>(this.ActiveObjectiveNames_OnRemove);
    this.activeObjectiveNames.OnClear += new Action(this.ActiveObjectiveNames_OnClear);
    this.activeObjectiveData.OnInsert += new Action<string, List<int>>(this.ActiveObjectiveData_OnAdd);
  }

  private void OnStopClient()
  {
    ColorLog<MissionManager>.Info("Client stop");
    if (!MissionManager.clientEventsAdded)
      return;
    MissionManager.clientEventsAdded = false;
    NetworkManagerNuclearOption.i.NetworkMission.Changed.RemoveListener(new Action<(NetworkMission.SyncMission, NetworkMission.State, bool)>(this.OnSyncMissionChanged));
  }

  private void OnSyncMissionChanged(
    (NetworkMission.SyncMission syncMission, NetworkMission.State state, bool stateChangeOnly) tuple)
  {
    (NetworkMission.SyncMission syncMission, NetworkMission.State state, bool stateChangeOnly) = tuple;
    if (this.OnSyncMissionChangedRunningFromOnStartClient & stateChangeOnly)
    {
      stateChangeOnly = false;
      ColorLog<MissionManager>.Info("ignoring stateChangeOnly because calling from OnStartClient");
    }
    if (stateChangeOnly)
      ColorLog<MissionManager>.Info($"OnSyncMissionChanged StateOnly {state}");
    else
      ColorLog<MissionManager>.Info($"OnSyncMissionChanged: ({state}, {syncMission.Name ?? "NULL"})");
    if (!stateChangeOnly)
    {
      MissionManager.CurrentMission = syncMission.Mission;
      MissionManager.CurrentMission.AfterLoad(syncMission.Name);
    }
    if (state == NetworkMission.State.Running)
    {
      ColorLog<MissionManager>.Info("Will start mission next frame");
      UniTask.Void((Func<UniTaskVoid>) (async () =>
      {
        MissionManager missionManager = this;
        CancellationToken cancel = missionManager.destroyCancellationToken;
        while (!missionManager.Client.Player.HasCharacter)
        {
          ColorLog<MissionManager>.Info("Waiting for Local player it spawn");
          await UniTask.Yield();
          if (cancel.IsCancellationRequested)
          {
            cancel = new CancellationToken();
            return;
          }
        }
        ColorLog<MissionManager>.Info("Waiting extra frame before start mission");
        await UniTask.Yield();
        if (cancel.IsCancellationRequested)
        {
          cancel = new CancellationToken();
        }
        else
        {
          foreach (FactionHQ allHq in FactionRegistry.GetAllHQs())
            MissionManager.CurrentMission.GetFactionFromHq(allHq, out MissionFaction _);
          MissionManager.StartMission();
          MissionManager.Runner.SetAllRemoteActiveObjectives((IReadOnlyList<string>) missionManager.activeObjectiveNames, (IReadOnlyDictionary<string, List<int>>) missionManager.activeObjectiveData);
          cancel = new CancellationToken();
        }
      }));
    }
    else
      ColorLog<MissionManager>.Info("Mission not running, wating for server to start it");
  }

  private void ActiveObjectiveNames_OnAdd(int index, string value)
  {
    if (MissionManager.Runner == null)
      return;
    Objective objective = MissionManager.Runner.AddRemoteActiveObjective(value);
    Action<Objective> objectiveStarted = MissionManager.onObjectiveStarted;
    if (objectiveStarted == null)
      return;
    objectiveStarted(objective);
  }

  private void ActiveObjectiveNames_OnRemove(int index, string value)
  {
    if (MissionManager.Runner == null)
      return;
    Objective objective = MissionManager.Runner.RemoveRemoteActiveObjective(value);
    Action<Objective> objectiveComplete = MissionManager.onObjectiveComplete;
    if (objectiveComplete == null)
      return;
    objectiveComplete(objective);
  }

  private void ActiveObjectiveNames_OnClear()
  {
    MissionManager.Runner?.ClearRemoteActiveObjectives();
  }

  private void ActiveObjectiveData_OnAdd(string key, List<int> item)
  {
    if (this.IsServer)
      return;
    this.UpdateNetworkObjectiveData(key, item);
  }

  private void UpdateNetworkObjectiveData(string uniqueName, List<int> data)
  {
    MissionManager.CurrentMission?.Objectives?.GetObjective(uniqueName)?.ReceiveNetworkData(data);
  }

  public void UpdateNetworkData(Objective obj, List<int> data)
  {
    this.activeObjectiveData[obj.SavedObjective.UniqueName] = data;
  }

  private void ServerObjectiveStarted(Objective obj)
  {
    this.activeObjectiveNames.Add(obj.SavedObjective.UniqueName);
    Action<Objective> objectiveStarted = MissionManager.onObjectiveStarted;
    if (objectiveStarted == null)
      return;
    objectiveStarted(obj);
  }

  private void ServerObjectiveComplete(Objective obj)
  {
    this.activeObjectiveNames.Remove(obj.SavedObjective.UniqueName);
    this.activeObjectiveData.Remove(obj.SavedObjective.UniqueName);
    Action<Objective> objectiveComplete = MissionManager.onObjectiveComplete;
    if (objectiveComplete == null)
      return;
    objectiveComplete(obj);
  }

  public static Dictionary<string, Airbase> GetAllAirbase() => FactionRegistry.airbaseLookup;

  public static IReadOnlyList<SavedAirbase> GetAllSavedAirbase()
  {
    MissionManager.airbaseCache.Clear();
    foreach (Airbase airbase in MissionManager.GetAllAirbase().Values)
      MissionManager.airbaseCache.Add(airbase.SavedAirbase);
    return (IReadOnlyList<SavedAirbase>) MissionManager.airbaseCache;
  }

  public static bool TryFindSavedAirbase(string uniqueName, out SavedAirbase saved)
  {
    IReadOnlyList<SavedAirbase> allSavedAirbase = MissionManager.GetAllSavedAirbase();
    int count = allSavedAirbase.Count;
    for (int index = 0; index < count; ++index)
    {
      SavedAirbase savedAirbase = allSavedAirbase[index];
      if (savedAirbase.UniqueName == uniqueName)
      {
        saved = savedAirbase;
        return true;
      }
    }
    saved = (SavedAirbase) null;
    return false;
  }

  public static IReadOnlyList<SavedUnit> GetAllSavedUnits(bool includeBuiltIn)
  {
    Mission currentMission = MissionManager.CurrentMission;
    return currentMission != null ? MissionManager.GetAllSavedUnits(currentMission, includeBuiltIn) : (IReadOnlyList<SavedUnit>) Array.Empty<SavedUnit>();
  }

  public static IReadOnlyList<SavedUnit> GetAllSavedUnits(Mission mission, bool includeBuiltIn)
  {
    MissionManager.unitCache.Clear();
    MissionManager.unitCache.AddRange((IEnumerable<SavedUnit>) mission.aircraft);
    MissionManager.unitCache.AddRange((IEnumerable<SavedUnit>) mission.vehicles);
    MissionManager.unitCache.AddRange((IEnumerable<SavedUnit>) mission.ships);
    MissionManager.unitCache.AddRange((IEnumerable<SavedUnit>) mission.buildings);
    MissionManager.unitCache.AddRange((IEnumerable<SavedUnit>) mission.scenery);
    MissionManager.unitCache.AddRange((IEnumerable<SavedUnit>) mission.containers);
    MissionManager.unitCache.AddRange((IEnumerable<SavedUnit>) mission.missiles);
    MissionManager.unitCache.AddRange((IEnumerable<SavedUnit>) mission.pilots);
    if (includeBuiltIn)
    {
      MissionManager.unitCache2.Clear();
      foreach (Unit allUnit in UnitRegistry.allUnits)
      {
        SavedUnit savedUnit = allUnit.SavedUnit;
        if (savedUnit != null && !MissionManager.unitCache.Contains(savedUnit))
          MissionManager.unitCache2.Add(savedUnit);
      }
      MissionManager.unitCache.AddRange((IEnumerable<SavedUnit>) MissionManager.unitCache2);
      MissionManager.unitCache2.Clear();
    }
    return (IReadOnlyList<SavedUnit>) MissionManager.unitCache;
  }

  public static IReadOnlyList<SavedBuilding> GetAllSavedBuildings(bool includeBuiltIn)
  {
    Mission currentMission = MissionManager.CurrentMission;
    return currentMission != null ? MissionManager.GetAllSavedBuildings(currentMission, includeBuiltIn) : (IReadOnlyList<SavedBuilding>) Array.Empty<SavedBuilding>();
  }

  public static IReadOnlyList<SavedBuilding> GetAllSavedBuildings(
    Mission mission,
    bool includeBuiltIn)
  {
    MissionManager.buildingCache.Clear();
    MissionManager.buildingCache.AddRange((IEnumerable<SavedBuilding>) mission.buildings);
    if (includeBuiltIn)
    {
      MissionManager.unitCache2.Clear();
      foreach (Unit allUnit in UnitRegistry.allUnits)
      {
        if (allUnit is Building)
        {
          SavedUnit savedUnit = allUnit.SavedUnit;
          if (savedUnit != null && !MissionManager.buildingCache.Contains((SavedBuilding) savedUnit))
            MissionManager.unitCache2.Add(savedUnit);
        }
      }
      foreach (SavedUnit savedUnit in MissionManager.unitCache2)
        MissionManager.buildingCache.Add((SavedBuilding) savedUnit);
      MissionManager.unitCache2.Clear();
    }
    return (IReadOnlyList<SavedBuilding>) MissionManager.buildingCache;
  }

  public MissionManager()
  {
    this.InitSyncObject((ISyncObject) this.activeObjectiveNames);
    this.InitSyncObject((ISyncObject) this.activeObjectiveData);
  }

  private void MirageProcessed()
  {
  }

  public float NetworktacticalThreshold
  {
    get => this.tacticalThreshold;
    set
    {
      if (this.SyncVarEqual<float>(value, this.tacticalThreshold))
        return;
      float tacticalThreshold = this.tacticalThreshold;
      this.tacticalThreshold = value;
      this.SetDirtyBit(1UL);
    }
  }

  public float NetworktacticalMinRank
  {
    get => this.tacticalMinRank;
    set
    {
      if (this.SyncVarEqual<float>(value, this.tacticalMinRank))
        return;
      float tacticalMinRank = this.tacticalMinRank;
      this.tacticalMinRank = value;
      this.SetDirtyBit(2UL);
    }
  }

  public float NetworkstrategicThreshold
  {
    get => this.strategicThreshold;
    set
    {
      if (this.SyncVarEqual<float>(value, this.strategicThreshold))
        return;
      float strategicThreshold = this.strategicThreshold;
      this.strategicThreshold = value;
      this.SetDirtyBit(4UL);
    }
  }

  public float NetworkstrategicMinRank
  {
    get => this.strategicMinRank;
    set
    {
      if (this.SyncVarEqual<float>(value, this.strategicMinRank))
        return;
      float strategicMinRank = this.strategicMinRank;
      this.strategicMinRank = value;
      this.SetDirtyBit(8UL);
    }
  }

  public float NetworkcurrentEscalation
  {
    get => this.currentEscalation;
    set
    {
      if (this.SyncVarEqual<float>(value, this.currentEscalation))
        return;
      float currentEscalation = this.currentEscalation;
      this.currentEscalation = value;
      this.SetDirtyBit(16UL /*0x10*/);
    }
  }

  public double NetworkmultiplayerStartTime
  {
    get => this.multiplayerStartTime;
    set
    {
      if (this.SyncVarEqual<double>(value, this.multiplayerStartTime))
        return;
      double multiplayerStartTime = this.multiplayerStartTime;
      this.multiplayerStartTime = value;
      this.SetDirtyBit(32UL /*0x20*/);
    }
  }

  public int NetworkwrecksMaxNumber
  {
    get => this.wrecksMaxNumber;
    set
    {
      if (this.SyncVarEqual<int>(value, this.wrecksMaxNumber))
        return;
      int wrecksMaxNumber = this.wrecksMaxNumber;
      this.wrecksMaxNumber = value;
      this.SetDirtyBit(64UL /*0x40*/);
    }
  }

  public float NetworkwrecksDecayTime
  {
    get => this.wrecksDecayTime;
    set
    {
      if (this.SyncVarEqual<float>(value, this.wrecksDecayTime))
        return;
      float wrecksDecayTime = this.wrecksDecayTime;
      this.wrecksDecayTime = value;
      this.SetDirtyBit(128UL /*0x80*/);
    }
  }

  public override bool SerializeSyncVars(NetworkWriter writer, bool initialize)
  {
    ulong syncVarDirtyBits = this.SyncVarDirtyBits;
    bool flag = base.SerializeSyncVars(writer, initialize);
    if (initialize)
    {
      writer.WriteSingleConverter(this.tacticalThreshold);
      writer.WriteSingleConverter(this.tacticalMinRank);
      writer.WriteSingleConverter(this.strategicThreshold);
      writer.WriteSingleConverter(this.strategicMinRank);
      writer.WriteSingleConverter(this.currentEscalation);
      writer.WriteDoubleConverter(this.multiplayerStartTime);
      writer.WritePackedInt32(this.wrecksMaxNumber);
      writer.WriteSingleConverter(this.wrecksDecayTime);
      return true;
    }
    writer.Write(syncVarDirtyBits, 8);
    if (((long) syncVarDirtyBits & 1L) != 0L)
    {
      writer.WriteSingleConverter(this.tacticalThreshold);
      flag = true;
    }
    if (((long) syncVarDirtyBits & 2L) != 0L)
    {
      writer.WriteSingleConverter(this.tacticalMinRank);
      flag = true;
    }
    if (((long) syncVarDirtyBits & 4L) != 0L)
    {
      writer.WriteSingleConverter(this.strategicThreshold);
      flag = true;
    }
    if (((long) syncVarDirtyBits & 8L) != 0L)
    {
      writer.WriteSingleConverter(this.strategicMinRank);
      flag = true;
    }
    if (((long) syncVarDirtyBits & 16L /*0x10*/) != 0L)
    {
      writer.WriteSingleConverter(this.currentEscalation);
      flag = true;
    }
    if (((long) syncVarDirtyBits & 32L /*0x20*/) != 0L)
    {
      writer.WriteDoubleConverter(this.multiplayerStartTime);
      flag = true;
    }
    if (((long) syncVarDirtyBits & 64L /*0x40*/) != 0L)
    {
      writer.WritePackedInt32(this.wrecksMaxNumber);
      flag = true;
    }
    if (((long) syncVarDirtyBits & 128L /*0x80*/) != 0L)
    {
      writer.WriteSingleConverter(this.wrecksDecayTime);
      flag = true;
    }
    return flag;
  }

  public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
  {
    base.DeserializeSyncVars(reader, initialState);
    if (initialState)
    {
      this.tacticalThreshold = reader.ReadSingleConverter();
      this.tacticalMinRank = reader.ReadSingleConverter();
      this.strategicThreshold = reader.ReadSingleConverter();
      this.strategicMinRank = reader.ReadSingleConverter();
      this.currentEscalation = reader.ReadSingleConverter();
      this.multiplayerStartTime = reader.ReadDoubleConverter();
      this.wrecksMaxNumber = reader.ReadPackedInt32();
      this.wrecksDecayTime = reader.ReadSingleConverter();
    }
    else
    {
      ulong dirtyBit = reader.Read(8);
      this.SetDeserializeMask(dirtyBit, 0);
      if (((long) dirtyBit & 1L) != 0L)
        this.tacticalThreshold = reader.ReadSingleConverter();
      if (((long) dirtyBit & 2L) != 0L)
        this.tacticalMinRank = reader.ReadSingleConverter();
      if (((long) dirtyBit & 4L) != 0L)
        this.strategicThreshold = reader.ReadSingleConverter();
      if (((long) dirtyBit & 8L) != 0L)
        this.strategicMinRank = reader.ReadSingleConverter();
      if (((long) dirtyBit & 16L /*0x10*/) != 0L)
        this.currentEscalation = reader.ReadSingleConverter();
      if (((long) dirtyBit & 32L /*0x20*/) != 0L)
        this.multiplayerStartTime = reader.ReadDoubleConverter();
      if (((long) dirtyBit & 64L /*0x40*/) != 0L)
        this.wrecksMaxNumber = reader.ReadPackedInt32();
      if (((long) dirtyBit & 128L /*0x80*/) == 0L)
        return;
      this.wrecksDecayTime = reader.ReadSingleConverter();
    }
  }

  protected override int GetRpcCount() => 0;
}

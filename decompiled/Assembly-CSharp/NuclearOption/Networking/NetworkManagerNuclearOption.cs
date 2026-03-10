// Decompiled with JetBrains decompiler
// Type: NuclearOption.Networking.NetworkManagerNuclearOption
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Cysharp.Threading.Tasks;
using JamesFrowen.Mirage.DebugScripts;
using Mirage;
using Mirage.Logging;
using Mirage.SocketLayer;
using Mirage.Sockets.Udp;
using Mirage.SteamworksSocket;
using NuclearOption.DedicatedServer;
using NuclearOption.Networking.Authentication;
using NuclearOption.Networking.Lobbies;
using NuclearOption.SavedMission;
using NuclearOption.SceneLoading;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Profiling;
using UnityEngine.SceneManagement;

#nullable disable
namespace NuclearOption.Networking;

public class NetworkManagerNuclearOption : NetworkManager
{
  private static readonly ILogger logger = LogFactory.GetLogger<NetworkManagerNuclearOption>();
  private const int DEFAULT_MAX_CONNECTIONS = 16 /*0x10*/;
  private static readonly ResourcesAsyncLoader<NetworkManagerNuclearOption> loader = ResourcesAsyncLoader.Create<NetworkManagerNuclearOption>("networkManager", (Func<GameObject, NetworkManagerNuclearOption>) null);
  [Header("References")]
  [SerializeField]
  private SteamManager steamManager;
  [SerializeField]
  private DedicatedServerManager dedicatedServerManager;
  [Header("Transports")]
  [SerializeField]
  private SteamworksSocketFactory steamTransport;
  [SerializeField]
  private UdpSocketFactory udpTransport;
  [SerializeField]
  private LagSocketFactory lagTransport;
  [SerializeField]
  private NetworkAuthenticatorNuclearOption authenticator;
  [Header("Prefabs and Scenes")]
  [SerializeField]
  private Player gamePlayerPrefab;
  [SerializeField]
  private MapLoader mapLoader;
  [SerializeField]
  private WaitingForDedicatedServerMenu waitingPrefab;
  private WaitingForDedicatedServerMenu waitingPanel;
  private ServerLoadingProgressMessage latestWaitingMessage;
  private bool loadingScene;
  private bool stopping;

  public static NetworkManagerNuclearOption i => NetworkManagerNuclearOption.loader.Get();

  public static async UniTask Preload(CancellationToken cancel)
  {
    await NetworkManagerNuclearOption.loader.Load(cancel);
  }

  public static bool? ModdedServer { get; private set; }

  public void SetModdedServer(bool value)
  {
    NetworkManagerNuclearOption.ModdedServer = new bool?(value);
  }

  public NetworkMission NetworkMission { get; private set; }

  public NuclearOption.SceneLoading.MapKey? MapKey { get; private set; }

  public static bool IsLoadingScene => NetworkManagerNuclearOption.i.loadingScene;

  public List<Player> GamePlayers { get; } = new List<Player>();

  public DedicatedServerManager DedicatedServerManager => this.dedicatedServerManager;

  public NetworkAuthenticatorNuclearOption Authenticator => this.authenticator;

  public SteamManager SteamManager => this.steamManager;

  public void Awake()
  {
    NetworkManagerNuclearOption.loader.AssetNotLoaded();
    this.mapLoader.ClearMap();
    this.Server.ErrorRateLimitEnabled = true;
    this.Client.DisconnectOnException = Application.isEditor;
    this.Server.RethrowException = Application.isEditor;
    this.Client.RethrowException = Application.isEditor;
    this.Server.Started.AddListener(new UnityAction(this.OnServerStarted));
    this.Server.Authenticated.AddListener(new UnityAction<INetworkPlayer>(this.OnServerAuthenticated));
    this.Server.Disconnected.AddListener(new UnityAction<INetworkPlayer>(this.OnServerDisconnect));
    this.Server.Stopped.AddListener(new UnityAction(this.ServerStopped));
    this.Client.Started.AddListener(new UnityAction(this.ClientStarted));
    this.Client.Authenticated.AddListener(new UnityAction<INetworkPlayer>(this.OnClientAuthenticated));
    this.Client.Disconnected.AddListener(new UnityAction<ClientStoppedReason>(this.ClientDisconnected));
    this.Server.Authenticated.AddListener(new UnityAction<INetworkPlayer>(this.LogServerConnected));
    this.Client.Authenticated.AddListener(new UnityAction<INetworkPlayer>(this.LogClientConnected));
    this.Server.Disconnected.AddListener(new UnityAction<INetworkPlayer>(this.LogServerDisconnected));
    this.Client.Disconnected.AddListener(new UnityAction<ClientStoppedReason>(this.LogClientDisconnected));
    this.NetworkMission = new NetworkMission(this.Server, this.Client);
  }

  private void LogServerConnected(INetworkPlayer player)
  {
    if (!NetworkManagerNuclearOption.logger.LogEnabled())
      return;
    NetworkManagerNuclearOption.logger.Log((object) $"Player connected: {player}");
  }

  private void LogClientConnected(INetworkPlayer player)
  {
    if (!NetworkManagerNuclearOption.logger.LogEnabled())
      return;
    NetworkManagerNuclearOption.logger.Log((object) $"Client connected: {player}");
  }

  private void LogServerDisconnected(INetworkPlayer player)
  {
    if (!NetworkManagerNuclearOption.logger.LogEnabled())
      return;
    NetworkManagerNuclearOption.logger.Log((object) $"Player disconnected: {player}");
  }

  private void LogClientDisconnected(ClientStoppedReason reason)
  {
    if (!NetworkManagerNuclearOption.logger.LogEnabled())
      return;
    NetworkManagerNuclearOption.logger.Log((object) $"Client disconnected {reason}");
  }

  public void ServerMissionStart(Mission mission)
  {
    this.NetworkMission.SetRunning();
    foreach (INetworkPlayer authenticatedPlayer in (IEnumerable<INetworkPlayer>) this.Server.AuthenticatedPlayers)
    {
      Player player;
      if (authenticatedPlayer.SceneIsReady && authenticatedPlayer.TryGetPlayer<Player>(out player))
        this.ServerMissionStartPlayer(mission, player);
    }
  }

  private void ServerMissionStartPlayer(Mission mission, Player player)
  {
    SavedPlayerData saveData = player.GetAuthData().SaveData;
    if (!saveData.Rejoined)
    {
      ColorLog<NetworkManagerNuclearOption>.Info($"setting playerStartingRank for {player} to {mission.missionSettings.playerStartingRank}");
      player.SetRank(mission.missionSettings.playerStartingRank, true);
    }
    else
      ColorLog<NetworkManagerNuclearOption>.InfoWarn($"skipping playerStartingRank for {player} because they are rejoining. rejoin rank:{saveData.Rank}");
    if (!((UnityEngine.Object) player.Aircraft == (UnityEngine.Object) null))
      return;
    NetworkSceneSingleton<Spawner>.i.TrySpawnPlayerControlled(player, out Aircraft _);
  }

  private void OnServerStarted()
  {
    if (MissionManager.CurrentMission != null)
      this.SetNetworkMission(MissionManager.CurrentMission, NetworkMission.State.Loaded);
    else
      this.NetworkMission.Clear();
  }

  public void SetNetworkMission(Mission mission, NetworkMission.State state)
  {
    if (!this.Server.Listening)
      return;
    this.NetworkMission.Set(mission, state);
  }

  private void OnServerAuthenticated(INetworkPlayer player)
  {
    if (this.MapKey.HasValue)
      this.SendLoadMapMessageOne(player);
    this.UpdateSteamPlayerCount();
  }

  private void UpdateSteamPlayerCount()
  {
    int count = this.Server.AuthenticatedPlayers.Count;
    if (DedicatedServerManager.IsRunning)
      --count;
    SteamLobby.instance.SetServerPlayerCount(count);
  }

  public void SendLoadMapMessageOne(INetworkPlayer player)
  {
    if (player.IsHost)
      return;
    player.SceneIsReady = false;
    player.Send<LoadMapMessage>(new LoadMapMessage()
    {
      Key = this.MapKey.Value
    });
  }

  public void SendLoadMapMessageAll()
  {
    this.MarkAllPlayersNotReady();
    this.Server.SendToAll<LoadMapMessage>(new LoadMapMessage()
    {
      Key = this.MapKey.Value
    }, true, true);
  }

  public void SendLoadWaitingMessage()
  {
    this.MarkAllPlayersNotReady();
    this.Server.SendToAll<LoadWaitingSceneMessage>(new LoadWaitingSceneMessage(), true, true);
  }

  private void MarkAllPlayersNotReady()
  {
    foreach (INetworkPlayer authenticatedPlayer in (IEnumerable<INetworkPlayer>) this.Server.AuthenticatedPlayers)
    {
      if (!authenticatedPlayer.IsHost)
        authenticatedPlayer.SceneIsReady = false;
    }
  }

  public void SendServerLoadingMessage(string message)
  {
    this.Server.SendToAll<ServerLoadingProgressMessage>(new ServerLoadingProgressMessage()
    {
      LoadingMessage = message
    }, true, true);
  }

  private void HandleSceneReadyMessage(INetworkPlayer player, SceneReadyMessage msg)
  {
    player.SceneIsReady = true;
    if (this.loadingScene)
    {
      if (!NetworkManagerNuclearOption.logger.LogEnabled())
        return;
      NetworkManagerNuclearOption.logger.Log((object) $"{player} finished loading scene, but server is still loading");
    }
    else if (player.HasCharacter)
    {
      player.Disconnect();
      Debug.LogError((object) "Player already had character");
    }
    else
      this.SpawnCharacter(player);
  }

  private void SpawnCharacter(INetworkPlayer networkPlayer)
  {
    switch (networkPlayer.GetAuthData().JoinAs)
    {
      case PlayerType.Normal:
        if (NetworkManagerNuclearOption.logger.LogEnabled())
          NetworkManagerNuclearOption.logger.Log((object) $"Spawning Normal Player for {networkPlayer}");
        Player player = UnityEngine.Object.Instantiate<Player>(this.gamePlayerPrefab);
        this.ServerObjectManager.AddCharacter(networkPlayer, player.Identity);
        if (!MissionManager.IsRunning)
          break;
        this.ServerMissionStartPlayer(MissionManager.CurrentMission, player);
        break;
      case PlayerType.Spectator:
        if (NetworkManagerNuclearOption.logger.LogEnabled())
          NetworkManagerNuclearOption.logger.Log((object) $"Spawning Spectator Player for {networkPlayer}");
        SpectatorPlayer prefab1 = PlayerHelper.CreatePrefab<SpectatorPlayer>();
        this.ServerObjectManager.AddCharacter(networkPlayer, prefab1.Identity);
        break;
      case PlayerType.DedicatedServer:
        if (NetworkManagerNuclearOption.logger.LogEnabled())
          NetworkManagerNuclearOption.logger.Log((object) $"Spawning Dedicated Server Player for {networkPlayer}");
        DedicatedServerPlayer prefab2 = PlayerHelper.CreatePrefab<DedicatedServerPlayer>();
        this.ServerObjectManager.AddCharacter(networkPlayer, prefab2.Identity);
        break;
      default:
        throw new ArgumentException("Can't spawn player of type Unknown");
    }
  }

  private void OnServerDisconnect(INetworkPlayer networkPlayer)
  {
    Player player;
    if (networkPlayer.TryGetPlayer<Player>(out player))
    {
      Unit unit;
      if (UnitRegistry.TryGetUnit(player.UnitID, out unit))
        unit.Networkdisabled = true;
      if ((UnityEngine.Object) player.HQ != (UnityEngine.Object) null)
        player.HQ.RemovePlayer(player);
      networkPlayer.RemoveAllOwnedObject(false);
      this.ServerObjectManager.Destroy(player.Identity);
    }
    this.UpdateSteamPlayerCount();
  }

  private void ServerStopped()
  {
    if (this.stopping)
      return;
    this.HandleStopAsync(new ClientStoppedReason?()).Forget();
  }

  private void ClientStarted()
  {
    this.Client.MessageHandler.RegisterHandler<LoadMapMessage>(new MessageDelegateWithPlayer<LoadMapMessage>(this.HandleSceneMessage));
    this.Client.MessageHandler.RegisterHandler<LoadWaitingSceneMessage>(new MessageDelegateWithPlayer<LoadWaitingSceneMessage>(this.HandleLoadWaitingMessage));
    this.Client.MessageHandler.RegisterHandler<HostEndedMessage>(new MessageDelegate<HostEndedMessage>(NetworkManagerNuclearOption.HandleHostEndedMessage));
    this.Client.MessageHandler.RegisterHandler<ServerLoadingProgressMessage>(new MessageDelegate<ServerLoadingProgressMessage>(this.HandleWaitingSceneProgressMessage));
    this.RegisterPrefabs();
  }

  private void OnClientAuthenticated(INetworkPlayer player)
  {
    if (!player.IsHost)
      JoinLobbyOverlay.Open(JoinProgress.Joining("Connecting complete, waiting for map"));
    NetworkTime time = this.Client.World.Time;
    time.PingInterval = 0.2f;
    time.PingWindowSize = 12;
    if (player.IsHost)
      return;
    CursorManager.SetFlag(CursorFlags.Loading, true);
    GameManager.SetGameState(GameState.Multiplayer);
  }

  private void HandleSceneMessage(INetworkPlayer player, LoadMapMessage msg)
  {
    JoinLobbyOverlay.Close();
    this.ClientLoadScene(msg).Forget();
  }

  private async UniTask ClientLoadScene(LoadMapMessage msg)
  {
    NetworkManagerNuclearOption managerNuclearOption = this;
    managerNuclearOption.SetLoading(true);
    LoadingScreen loadingScreen = LoadingScreen.GetLoadingScreen();
    loadingScreen.ShowLoadingScreen();
    if (NetworkManagerNuclearOption.logger.logEnabled)
      NetworkManagerNuclearOption.logger.Log((object) $"Client starting to load {msg.Key}");
    MapLoader.LoadResult loadResult;
    try
    {
      loadResult = await managerNuclearOption.mapLoader.Load(msg.Key, (IProgress<float>) loadingScreen);
    }
    catch (Exception ex)
    {
      NetworkManagerNuclearOption.logger.LogError((object) $"Exception in Scene load: {ex}");
      loadResult = MapLoader.LoadResult.Failed;
    }
    managerNuclearOption.SetLoading(false);
    GameManager.ResetGameResolution();
    try
    {
      switch (loadResult)
      {
        case MapLoader.LoadResult.ChangedScene:
        case MapLoader.LoadResult.ChangedWorldPrefab:
        case MapLoader.LoadResult.AlreadyLoaded:
          if (NetworkManagerNuclearOption.logger.LogEnabled())
            NetworkManagerNuclearOption.logger.Log((object) "Client finished loading, telling server we have finished");
          managerNuclearOption.Client.Send<SceneReadyMessage>(new SceneReadyMessage(), Mirage.Channel.Reliable);
          break;
        default:
          NetworkManagerNuclearOption.logger.LogError((object) $"Load {msg.Key} failed with result={loadResult}");
          managerNuclearOption.Client.Disconnect();
          break;
      }
    }
    catch (Exception ex)
    {
      NetworkManagerNuclearOption.logger.LogError((object) $"Exception handling LoadResult: {ex}");
    }
    UniTask.Void((Func<UniTaskVoid>) (async () =>
    {
      YieldAwaitable yieldAwaitable = UniTask.Yield();
      await yieldAwaitable;
      yieldAwaitable = UniTask.Yield();
      await yieldAwaitable;
      if (!((UnityEngine.Object) loadingScreen != (UnityEngine.Object) null))
        return;
      loadingScreen.HideLoadingScreen();
    }));
  }

  private void HandleLoadWaitingMessage(INetworkPlayer player, LoadWaitingSceneMessage _)
  {
    JoinLobbyOverlay.Close();
    this.ClientLoadEmpty().Forget();
  }

  private async UniTask ClientLoadEmpty()
  {
    int num = await this.LoadSystemScene(MapLoader.Empty, true) ? 1 : 0;
    await UniTask.Yield();
    this.waitingPanel = UnityEngine.Object.Instantiate<WaitingForDedicatedServerMenu>(this.waitingPrefab);
    this.waitingPanel.SetLoadingMessage(this.latestWaitingMessage.LoadingMessage);
  }

  private void ClientDisconnected(ClientStoppedReason disconnectReason)
  {
    if (this.Server.Active)
      return;
    if (GameManager.disconnectInfo == null)
    {
      switch (disconnectReason)
      {
        case ClientStoppedReason.Timeout:
          GameManager.SetDisconnectReason(new DisconnectInfo("Disconnected: Timeout"));
          break;
        case ClientStoppedReason.ServerFull:
          GameManager.SetDisconnectReason(new DisconnectInfo("Failed to join: Server full"));
          break;
        case ClientStoppedReason.ConnectingTimeout:
          GameManager.SetDisconnectReason(new DisconnectInfo("Failed to join: Timeout"));
          break;
        case ClientStoppedReason.KeyInvalid:
          GameManager.SetDisconnectReason(new DisconnectInfo("Failed to join: Invalid version"));
          break;
        default:
          GameManager.SetDisconnectReason(new DisconnectInfo("Disconnected: Unknown"));
          break;
      }
    }
    if (this.stopping)
      return;
    this.HandleStopAsync(new ClientStoppedReason?(disconnectReason)).Forget();
  }

  private void SetLoading(bool isLoading)
  {
    int num = isLoading ? 1 : 0;
    this.loadingScene = isLoading;
    CursorManager.SetFlag(CursorFlags.Loading, isLoading);
    if (!isLoading)
      return;
    CursorManager.SetFlag(CursorFlags.EmptyScene, false);
  }

  public async UniTask<bool> LoadSystemScene(string scene, bool warnIfAlreadyLoaded)
  {
    NetworkManagerNuclearOption managerNuclearOption = this;
    Scene activeScene = SceneManager.GetActiveScene();
    managerNuclearOption.MapKey = new NuclearOption.SceneLoading.MapKey?();
    if (NetworkManagerNuclearOption.logger.LogEnabled())
      NetworkManagerNuclearOption.logger.Log((object) $"Trying to load '{scene}'");
    if (activeScene.name == scene || activeScene.path == scene)
    {
      if (warnIfAlreadyLoaded)
        Debug.LogWarning((object) ("Already loaded " + scene));
      else if (NetworkManagerNuclearOption.logger.LogEnabled())
        NetworkManagerNuclearOption.logger.Log((object) ("Already loaded " + scene));
      return false;
    }
    if (NetworkManagerNuclearOption.logger.LogEnabled())
      NetworkManagerNuclearOption.logger.Log((object) $"Start load scene '{scene}'. Old Scene = {activeScene.name}");
    managerNuclearOption.SetLoading(true);
    try
    {
      managerNuclearOption.mapLoader.ClearMap();
      MissionManager.SetNullMission();
      SlowUpdateExtensions.DestroyRunner();
      LoadingScreen loadingScreen = LoadingScreen.GetLoadingScreen();
      loadingScreen.ShowLoadingScreen();
      AsyncOperation asyncOperation;
      if (scene == MapLoader.Empty)
      {
        CursorManager.SetFlag(CursorFlags.EmptyScene, true);
        asyncOperation = managerNuclearOption.mapLoader.LoadEmpty();
        if (asyncOperation == null)
          return false;
      }
      else
      {
        asyncOperation = SceneManager.LoadSceneAsync(scene);
        if (asyncOperation == null)
        {
          NetworkManagerNuclearOption.logger.LogWarning((object) "LoadSceneAsync returned null");
          return false;
        }
      }
      UniTask uniTask = asyncOperation.ToUniTask((IProgress<float>) loadingScreen);
      await uniTask;
      GameManager.ResetGame();
      uniTask = NetworkManagerNuclearOption.MemoryCleanup();
      await uniTask;
      loadingScreen.HideLoadingScreen();
      loadingScreen = (LoadingScreen) null;
    }
    finally
    {
      managerNuclearOption.SetLoading(false);
    }
    if (managerNuclearOption.Client.Active)
    {
      if (NetworkManagerNuclearOption.logger.LogEnabled())
        NetworkManagerNuclearOption.logger.Log((object) "Client preparing scene objects");
      managerNuclearOption.ClientObjectManager.PrepareToSpawnSceneObjects();
    }
    if (managerNuclearOption.Server.Active)
    {
      if (NetworkManagerNuclearOption.logger.LogEnabled())
        NetworkManagerNuclearOption.logger.Log((object) "Server spawning scene Objects");
      managerNuclearOption.ServerObjectManager.SpawnSceneObjects();
    }
    if (NetworkManagerNuclearOption.logger.LogEnabled())
      NetworkManagerNuclearOption.logger.Log((object) $"Finished load scene '{scene}'");
    return true;
  }

  public static async UniTask MemoryCleanup()
  {
    await Resources.UnloadUnusedAssets();
    NetworkManagerNuclearOption.LogMonoHeap("MonoHeap: {0} (BEFORE CLEANUP)");
    GC.Collect(2, GCCollectionMode.Forced, true);
    GC.WaitForPendingFinalizers();
    NetworkManagerNuclearOption.LogMonoHeap("MonoHeap: {0} (After CLEANUP)");
  }

  public static void LogMonoHeap(string format)
  {
    int num1 = 1048576 /*0x100000*/;
    long num2 = Profiler.GetMonoUsedSizeLong() / (long) num1;
    long num3 = Profiler.GetMonoHeapSizeLong() / (long) num1;
    Debug.Log((object) string.Format(format, (object) $"{num2} / {num3} MB"));
  }

  private void RegisterPrefabs()
  {
    this.ClientObjectManager.RegisterPrefab(this.gamePlayerPrefab.Identity);
    PlayerHelper.RegisterPrefabs(this.ClientObjectManager);
    this.ClientObjectManager.RegisterPrefab(GameAssets.i.airbasePrefab.GetNetworkIdentity());
    this.RegisterAll<AircraftDefinition>(Encyclopedia.i.aircraft);
    this.RegisterAll<VehicleDefinition>(Encyclopedia.i.vehicles);
    this.RegisterAll<MissileDefinition>(Encyclopedia.i.missiles);
    this.RegisterAll<BuildingDefinition>(Encyclopedia.i.buildings);
    this.RegisterAll<ShipDefinition>(Encyclopedia.i.ships);
    this.RegisterAll<SceneryDefinition>(Encyclopedia.i.scenery);
    this.RegisterAll<UnitDefinition>(Encyclopedia.i.otherUnits);
  }

  private void RegisterAll<T>(List<T> list) where T : UnitDefinition
  {
    foreach (T obj in list)
      this.ClientObjectManager.RegisterPrefab(obj.unitPrefab.GetNetworkIdentity());
  }

  public async UniTaskVoid KickPlayerAsync(Player player)
  {
    NetworkManagerNuclearOption managerNuclearOption = this;
    if (!managerNuclearOption.Server.Active)
      throw new MethodInvocationException("KickPlayerAsync called when server is not active");
    INetworkPlayer conn = player.Owner;
    managerNuclearOption.authenticator.OnKick(conn);
    Player localPlayer;
    string hostName = GameManager.GetLocalPlayer<Player>(out localPlayer) ? localPlayer.PlayerName : "server";
    player.KickReason(string.Empty, hostName);
    await UniTask.Delay(100);
    conn.Disconnect();
    conn = (INetworkPlayer) null;
  }

  public void KickPlayer(INetworkPlayer conn)
  {
    if (!this.Server.Active)
      throw new MethodInvocationException("KickPlayer called when server is not active");
    this.authenticator.OnKick(conn);
    conn.Disconnect();
  }

  public void StartHost(HostOptions options) => this.StartHostAsync(options).Forget();

  public async UniTask StartHostAsync(HostOptions options)
  {
    NetworkManagerNuclearOption managerNuclearOption = this;
    if (managerNuclearOption.Server.Active)
      throw new InvalidOperationException("Can't start server because it is already running");
    if (managerNuclearOption.Client.Active)
      throw new InvalidOperationException("Can't start server because client is running");
    managerNuclearOption.ConfigureNetwork(options.SocketType, options.MaxConnections ?? 16 /*0x10*/);
    if (options.UdpPort.HasValue)
      managerNuclearOption.udpTransport.Port = (ushort) options.UdpPort.Value;
    if (string.IsNullOrEmpty(options.Password))
      managerNuclearOption.authenticator.ClearServerPassword();
    else
      managerNuclearOption.authenticator.SetServerPassword(options.Password);
    managerNuclearOption.Server.StartServer(managerNuclearOption.Client);
    managerNuclearOption.Server.MessageHandler.RegisterHandler<SceneReadyMessage>(new MessageDelegateWithPlayer<SceneReadyMessage>(managerNuclearOption.HandleSceneReadyMessage));
    CursorManager.SetFlag(CursorFlags.Loading, true);
    GameManager.SetGameState(options.GameState);
    GameManager.SetupGame();
    if (!string.IsNullOrEmpty(options.SystemScene))
    {
      if (await managerNuclearOption.LoadSystemScene(options.SystemScene, true))
        managerNuclearOption.Host_SceneLoaded();
      else
        NetworkManagerNuclearOption.logger.LogError((object) ("Failed to load system scene " + options.SystemScene));
    }
    else
    {
      (bool, MapLoader.LoadResult) valueTuple = await managerNuclearOption.ServerLoadMapScene(options.Map, (IProgress<float>) null);
      if (valueTuple.Item1)
      {
        managerNuclearOption.Host_SceneLoaded();
      }
      else
      {
        NetworkManagerNuclearOption.logger.LogError((object) $"Load {options.Map} failed with result={valueTuple.Item2}");
        managerNuclearOption.Server.Stop();
      }
    }
  }

  public async UniTask<(bool success, MapLoader.LoadResult result)> ServerLoadMapScene(
    NuclearOption.SceneLoading.MapKey mapKey,
    IProgress<float> progress)
  {
    NetworkManagerNuclearOption managerNuclearOption = this;
    managerNuclearOption.Server.LocalPlayer.SceneIsReady = false;
    managerNuclearOption.SetLoading(true);
    LoadingScreen loading = (LoadingScreen) null;
    bool withLoadingScreen = !GameManager.IsHeadless;
    if (withLoadingScreen)
    {
      loading = LoadingScreen.GetLoadingScreen();
      loading.ShowLoadingScreen();
    }
    MapLoader.LoadResult loadResult;
    try
    {
      if (mapKey.IsDefault())
      {
        ColorLog<NetworkManagerNuclearOption>.Info($"Replacing empty key with default: {mapKey} -> {managerNuclearOption.mapLoader.DefaultMap}");
        mapKey = managerNuclearOption.mapLoader.DefaultMap;
      }
      managerNuclearOption.MapKey = new NuclearOption.SceneLoading.MapKey?(mapKey);
      IProgress<float> progress1 = (IProgress<float>) null;
      if (progress != null | withLoadingScreen)
        progress1 = (IProgress<float>) new Progress<float>((Action<float>) (value =>
        {
          progress?.Report(value);
          if (!withLoadingScreen)
            return;
          ((IProgress<float>) loading).Report(value);
        }));
      loadResult = await managerNuclearOption.mapLoader.Load(mapKey, progress1);
    }
    finally
    {
      managerNuclearOption.SetLoading(false);
      if (withLoadingScreen)
        loading.HideLoadingScreen();
    }
    switch (loadResult)
    {
      case MapLoader.LoadResult.ChangedScene:
      case MapLoader.LoadResult.ChangedWorldPrefab:
      case MapLoader.LoadResult.AlreadyLoaded:
        return (true, loadResult);
      default:
        return (false, loadResult);
    }
  }

  public void Host_SceneLoaded()
  {
    if (NetworkManagerNuclearOption.logger.LogEnabled())
      NetworkManagerNuclearOption.logger.Log((object) "Server finished loading");
    this.Server.LocalPlayer.SceneIsReady = true;
    if (NetworkManagerNuclearOption.logger.LogEnabled())
      NetworkManagerNuclearOption.logger.Log((object) "Spawning Player object for host and connections");
    foreach (INetworkPlayer authenticatedPlayer in (IEnumerable<INetworkPlayer>) this.Server.AuthenticatedPlayers)
    {
      if (authenticatedPlayer.SceneIsReady)
        this.SpawnCharacter(authenticatedPlayer);
    }
    if (GameManager.gameState != GameState.Editor)
      return;
    UnityEngine.Object.Instantiate<GameObject>(GameAssets.i.missionEditor, SceneSingleton<GameplayUI>.i.menuCanvas.transform);
  }

  public void StartClient(ConnectOptions options)
  {
    if (this.Server.Active)
      throw new InvalidOperationException("Can't start client because server is running");
    if (this.Client.Active)
      throw new InvalidOperationException("Can't start client because it is already running");
    this.ConfigureNetwork(options.SocketType, 1);
    MissionManager.SetNullMission();
    if (string.IsNullOrEmpty(options.Password))
      this.authenticator.ClearClientPassword();
    else
      this.authenticator.SetClientPassword(options.Password);
    switch (options.SocketType)
    {
      case SocketType.UDP:
      case SocketType.LagUdp:
        this.Client.Connect(options.UdpHost, options.UdpPort.HasValue ? new ushort?((ushort) options.UdpPort.Value) : new ushort?());
        break;
      case SocketType.Steam:
      case SocketType.SteamGameServer:
      case SocketType.LagSteam:
        this.Client.Connect(options.SteamLobbyIDString);
        break;
      default:
        throw new ArgumentException("can not use SocketType.Offline for client");
    }
  }

  private void ConfigureNetwork(SocketType type, int maxConnections)
  {
    Config config = new Config()
    {
      MaxConnections = maxConnections,
      ConnectAttemptInterval = 0.25f,
      MaxConnectAttempts = 40,
      TimeoutDuration = 30f,
      MaxReliableFragments = 100,
      MaxReliablePacketsInSendBufferPerConnection = 3000
    };
    this.Server.PeerConfig = config;
    this.Client.PeerConfig = config;
    this.Server.Listening = type != 0;
    switch (type)
    {
      case SocketType.Offline:
      case SocketType.UDP:
        Debug.Log((object) "Setting UDP socket");
        this.Server.SocketFactory = (SocketFactory) this.udpTransport;
        this.Client.SocketFactory = (SocketFactory) this.udpTransport;
        break;
      case SocketType.Steam:
      case SocketType.SteamGameServer:
        Debug.Log((object) "Setting Steam socket");
        this.steamTransport.GameServer = type == SocketType.SteamGameServer;
        this.Server.SocketFactory = (SocketFactory) this.steamTransport;
        this.Client.SocketFactory = (SocketFactory) this.steamTransport;
        break;
      case SocketType.LagUdp:
        this.Server.SocketFactory = (SocketFactory) this.lagTransport;
        this.Client.SocketFactory = (SocketFactory) this.lagTransport;
        this.lagTransport.inner = (SocketFactory) this.udpTransport;
        break;
      case SocketType.LagSteam:
        this.Server.SocketFactory = (SocketFactory) this.lagTransport;
        this.Client.SocketFactory = (SocketFactory) this.lagTransport;
        this.lagTransport.inner = (SocketFactory) this.steamTransport;
        break;
    }
  }

  public void Stop(bool setDisconnectReason) => this.StopAsync(setDisconnectReason).Forget();

  public async UniTask StopAsync(bool setDisconnectReason)
  {
    NetworkManagerNuclearOption managerNuclearOption = this;
    if (setDisconnectReason)
      GameManager.SetDisconnectReason(DisconnectInfo.NoReason());
    UniTask uniTask = managerNuclearOption.HandleStopAsync(new ClientStoppedReason?());
    if (managerNuclearOption.Server.Active)
      managerNuclearOption.Server.Stop();
    else if (managerNuclearOption.Client.Active)
      managerNuclearOption.Client.Disconnect();
    else
      Debug.LogWarning((object) "Stop called when network was not active");
    await uniTask;
  }

  private async UniTask HandleStopAsync(ClientStoppedReason? disconnectReason)
  {
    if (this.stopping)
    {
      Debug.LogError((object) "Already Stopping network");
    }
    else
    {
      this.stopping = true;
      try
      {
        if (SceneManager.GetActiveScene().path == MapLoader.MultiplayerMenu)
        {
          JoinLobbyOverlay.Open(JoinProgress.GetFailReason(disconnectReason));
        }
        else
        {
          MusicManager.i.StopMusic();
          GameManager.SetGameState(GameState.Menu);
          int num = await this.LoadSystemScene(MapLoader.MainMenu, false) ? 1 : 0;
        }
      }
      finally
      {
        this.stopping = false;
      }
    }
  }

  public static void HandleHostEndedMessage(HostEndedMessage msg)
  {
    GameManager.SetDisconnectReason(new DisconnectInfo($"Host {msg.HostName} ended the game"));
  }

  public void HandleWaitingSceneProgressMessage(ServerLoadingProgressMessage msg)
  {
    this.latestWaitingMessage = msg;
    if (!((UnityEngine.Object) this.waitingPanel != (UnityEngine.Object) null))
      return;
    this.waitingPanel.SetLoadingMessage(msg.LoadingMessage);
  }
}

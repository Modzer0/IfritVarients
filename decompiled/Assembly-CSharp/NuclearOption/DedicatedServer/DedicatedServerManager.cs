// Decompiled with JetBrains decompiler
// Type: NuclearOption.DedicatedServer.DedicatedServerManager
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Cysharp.Threading.Tasks;
using Mirage;
using NuclearOption.BuildScripts;
using NuclearOption.Networking;
using NuclearOption.Networking.Lobbies;
using NuclearOption.NetworkTransforms;
using NuclearOption.SavedMission;
using NuclearOption.SavedMission.ObjectiveV2;
using NuclearOption.SceneLoading;
using Steamworks;
using System;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;

#nullable disable
namespace NuclearOption.DedicatedServer;

public class DedicatedServerManager : MonoBehaviour
{
  [SerializeField]
  private NetworkServer server;
  [SerializeField]
  private NetworkManagerNuclearOption networkManager;
  [SerializeField]
  private MapLoader mapLoader;
  [SerializeField]
  private int millisecondsInterval = 5000;
  private MissionRotation missionRotation;
  private MissionOptions currentMissionOption;
  private Mission currentMission;
  private double noPlayerStopTime;
  public DedicatedServerKeyValues keyValues = new DedicatedServerKeyValues();
  public static bool AutoRun;
  public static (DedicatedServerConfig config, string path) AutoRunConfig;
  public static bool UpdateReady;

  public static DedicatedServerManager Instance
  {
    get => NetworkManagerNuclearOption.i.DedicatedServerManager;
  }

  public static bool IsRunning { get; private set; }

  public DedicatedServerConfig Config { get; private set; }

  public string ConfigPath { get; private set; }

  public double NoPlayerStopTime => this.noPlayerStopTime;

  public MissionOptions CurrentMissionOption => this.currentMissionOption;

  public MissionOptions NextMissionOption => this.missionRotation.PeakNext();

  public static void SetAutoRunConfig(DedicatedServerConfig config, string path)
  {
    DedicatedServerManager.AutoRunConfig = (config, path);
  }

  public static (DedicatedServerConfig config, string path) GetConfig()
  {
    if (DedicatedServerManager.AutoRunConfig.config == null)
      DedicatedServerManager.AutoRunConfig = DedicatedServerConfig.AutoFindOrCreate();
    return DedicatedServerManager.AutoRunConfig;
  }

  private void Awake()
  {
    if (!GameManager.IsHeadless || CommandLineArgParser.IsAutoStart)
      return;
    ColorLog<DedicatedServerManager>.Info("Setting AutoRun");
    DedicatedServerManager.AutoRun = true;
  }

  private void Start()
  {
    if (!DedicatedServerManager.AutoRun)
      return;
    this.StartAsync().Forget();
  }

  private async UniTaskVoid StartAsync()
  {
    DedicatedServerManager dedicatedServerManager = this;
    await MainMenu.WaitForLoaded(dedicatedServerManager.destroyCancellationToken);
    if (!SteamManager.ServerInitialized)
    {
      ColorLog<DedicatedServerManager>.InfoWarn("Could not start DedicatedServerManager because Steam failed to init");
      Application.Quit();
    }
    else
    {
      (DedicatedServerConfig config, string path) config = DedicatedServerManager.GetConfig();
      dedicatedServerManager.Run(config.config, config.path);
    }
  }

  public void Run(DedicatedServerConfig config, string configPath)
  {
    this.Config = config;
    this.ConfigPath = configPath;
    this.missionRotation = new MissionRotation(config.MissionRotation, config.RotationType);
    this.server.ErrorRateLimitEnabled = !this.Config.DisableErrorKick;
    this.Runner().Forget();
  }

  private async UniTask Runner()
  {
    DedicatedServerManager.IsRunning = !DedicatedServerManager.IsRunning ? true : throw new InvalidOperationException("Already running");
    try
    {
      await this.RunnerInner();
    }
    finally
    {
      DedicatedServerManager.IsRunning = false;
    }
  }

  private async UniTask RunnerInner()
  {
    DedicatedServerManager dedicatedServerManager = this;
    CancellationToken cancel = dedicatedServerManager.destroyCancellationToken;
    ColorLog<DedicatedServerManager>.Info("Starting Server");
    await dedicatedServerManager.StartServer();
    if (!string.IsNullOrEmpty(dedicatedServerManager.Config.MissionDirectory))
      MissionGroup.UserGroup.SetDirectory(dedicatedServerManager.Config.MissionDirectory);
    ColorLog<DedicatedServerManager>.Info("Starting run loop");
    while (!cancel.IsCancellationRequested)
    {
      try
      {
        dedicatedServerManager.currentMission = (Mission) null;
        if (dedicatedServerManager.PreloadAndUpdateLobby(dedicatedServerManager.missionRotation.PeakNext(), false, out Mission _))
        {
          dedicatedServerManager.networkManager.Authenticator.ClearRejoinSaveData();
          if (!dedicatedServerManager.HasPlayers())
          {
            if (DedicatedServerManager.UpdateReady)
            {
              ColorLog<DedicatedServerManager>.Info("Closing for update");
              Application.Quit();
              cancel = new CancellationToken();
              return;
            }
            if (GameManager.gameState != GameState.ServerWaiting)
            {
              ColorLog<DedicatedServerManager>.Info("No players, unloading scene");
              await dedicatedServerManager.LoadEmptyScene();
            }
            ColorLog<DedicatedServerManager>.Info("Waiting for Players before loading next map");
            while (!dedicatedServerManager.HasPlayers())
            {
              await UniTask.Delay(dedicatedServerManager.millisecondsInterval, true);
              if (cancel.IsCancellationRequested)
              {
                cancel = new CancellationToken();
                return;
              }
              if (DedicatedServerManager.UpdateReady)
              {
                ColorLog<DedicatedServerManager>.Info("Closing for update");
                Application.Quit();
                cancel = new CancellationToken();
                return;
              }
            }
          }
          dedicatedServerManager.currentMissionOption = dedicatedServerManager.missionRotation.GetNext();
          if (dedicatedServerManager.PreloadAndUpdateLobby(dedicatedServerManager.currentMissionOption, true, out dedicatedServerManager.currentMission))
          {
            if (!await dedicatedServerManager.LoadNext(dedicatedServerManager.currentMission))
            {
              dedicatedServerManager.missionRotation.RemoveBrokenMap(dedicatedServerManager.currentMissionOption.Key);
              continue;
            }
            SendTransformBatcher component;
            if (dedicatedServerManager.networkManager.TryGetComponent<SendTransformBatcher>(out component))
              component.clientAuthDebugStream?.LogBlank();
            dedicatedServerManager.noPlayerStopTime = Time.unscaledTimeAsDouble + (double) dedicatedServerManager.Config.NoPlayerStopTime;
            while (!dedicatedServerManager.GameShouldStop())
            {
              await UniTask.Delay(dedicatedServerManager.millisecondsInterval, true);
              if (cancel.IsCancellationRequested)
              {
                cancel = new CancellationToken();
                return;
              }
            }
          }
          else
            continue;
        }
        else
          continue;
      }
      catch (Exception ex)
      {
        Debug.LogException(ex);
      }
      await UniTask.Yield();
    }
    cancel = new CancellationToken();
  }

  private async UniTask StartServer()
  {
    if (!this.SteamLogOn())
    {
      Debug.LogError((object) "Failed to host lobby");
      CommandLineArgParser.Quit();
    }
    else
    {
      ColorLog<DedicatedServerManager>.Info("Starting Network Server");
      HostOptions options = new HostOptions((SocketType) ((int) CommandLineArgParser.SocketType ?? 3), GameState.ServerWaiting, MapLoader.Empty);
      options.MaxConnections = new int?(this.Config.MaxPlayers);
      ushort? nullable = this.Config.Port.AsNullable<ushort>();
      options.UdpPort = nullable.HasValue ? new int?((int) nullable.GetValueOrDefault()) : new int?();
      options.Password = this.Config.Password;
      await NetworkManagerNuclearOption.i.StartHostAsync(options);
      ColorLog<DedicatedServerManager>.Info($"Server ID: {SteamGameServer.GetSteamID()}");
      this.LoadAllowBanList();
    }
  }

  public void LoadAllowBanList()
  {
    DedicatedServerManager.LoadAllowBanList(this.Config.BanListPaths, NetworkManagerNuclearOption.i.Authenticator.BanList);
    DedicatedServerManager.LoadAllowBanList(this.Config.ErrorKickImmuneListPaths, NetworkManagerNuclearOption.i.Authenticator.ErrorKickImmuneList);
  }

  private static void LoadAllowBanList(string[] list, AllowBanList authList)
  {
    if (list == null || list.Length == 0)
      return;
    AllowBanList allowBanList = authList;
    foreach (string path in list)
      allowBanList.Load(path);
  }

  private bool SteamLogOn()
  {
    ColorLog<DedicatedServerManager>.Info("Advertising dedicated server with name " + this.Config.ServerName);
    SteamGameServer.SetModDir("NuclearOption");
    SteamGameServer.SetProduct(SteamManager.SteamAppId);
    SteamGameServer.SetGameDescription("Nuclear Option Server");
    SteamGameServer.SetServerName(this.Config.ServerName ?? "Nuclear Option Server");
    SteamGameServer.SetMapName("Loading");
    SteamGameServer.SetDedicatedServer(true);
    this.keyValues.SetKeyValue("version", Application.version ?? "");
    this.keyValues.SetKeyValue("modded_server", LobbyInstance.BoolToTag(((int) NetworkManagerNuclearOption.ModdedServer ?? (this.Config.ModdedServer ? 1 : 0)) != 0));
    if (this.Config.HasPassword)
      this.keyValues.SetKeyValue("short_password", LobbyPassword.GetShortPassword(this.Config.Password));
    else
      this.keyValues.SetKeyValue("short_password", (string) null);
    this.keyValues.ApplyValuesToSteam();
    SteamGameServer.SetMaxPlayerCount(this.Config.MaxPlayers);
    SteamGameServer.SetBotPlayerCount(0);
    ColorLog<DedicatedServerManager>.Info("SteamGameServer.LogOnAnonymous");
    SteamGameServer.LogOnAnonymous();
    ColorLog<DedicatedServerManager>.Info($"Set Advertise Server: {!this.Config.Hidden}");
    SteamGameServer.SetAdvertiseServerActive(!this.Config.Hidden);
    return true;
  }

  private bool PreloadAndUpdateLobby(MissionOptions option, bool setStartTime, out Mission mission)
  {
    if (!DedicatedServerManager.PreLoadMission(option, out mission))
    {
      ColorLog<DedicatedServerManager>.Info("Failed to load mission");
      this.missionRotation.RemoveBrokenMap(option.Key);
      return false;
    }
    this.UpdateLobby(mission, setStartTime);
    return true;
  }

  public void UpdateLobby(Mission mission, bool setStartTime)
  {
    try
    {
      ColorLog<DedicatedServerManager>.Info("Updating server data for Mission=" + mission.Name);
      string str1 = mission.Name ?? "";
      string mapName;
      string str2 = this.mapLoader.TryGetMapName(mission.MapKey, out mapName) ? mapName : "";
      this.keyValues.SetKeyValue("mission_name", str1);
      this.keyValues.SetKeyValue("mission_description", mission.missionSettings.description ?? "");
      this.keyValues.SetKeyValue("mission_pvp_type", MissionTag.GetPvpTypeLobbyString(mission));
      this.keyValues.SetKeyValue("map_name", str2);
      ref MissionKey? local1 = ref mission.LoadKey;
      ulong? nullable;
      if (!local1.HasValue)
      {
        nullable = new ulong?();
      }
      else
      {
        ref readonly PublishedFileId_t? local2 = ref local1.GetValueOrDefault().WorkshopId;
        nullable = local2.HasValue ? new ulong?(local2.GetValueOrDefault().m_PublishedFileId) : new ulong?();
      }
      this.keyValues.SetKeyValue("mission_workshop_id", nullable.GetValueOrDefault().ToString("X"));
      if (setStartTime)
      {
        this.keyValues.SetKeyValue("start_time", LobbyInstance.CreateStartTime());
        this.keyValues.ApplyValuesToSteam();
      }
      else
      {
        this.keyValues.SetKeyValue("start_time", "no-game");
        this.keyValues.ApplyValuesToSteam();
      }
      this.keyValues.ApplyValuesToSteam();
    }
    catch (Exception ex)
    {
      Debug.LogException(ex);
    }
  }

  private static bool PreLoadMission(MissionOptions missionOptions, out Mission mission)
  {
    ColorLog<DedicatedServerManager>.Info("Loading next Mission " + missionOptions.Key.Name);
    MissionKey missionKey;
    if (!missionOptions.Key.TryGetKey(out missionKey))
    {
      mission = (Mission) null;
      return false;
    }
    string error;
    if (MissionSaveLoad.TryLoad(missionKey, out mission, out error))
      return true;
    Debug.LogError((object) error);
    return false;
  }

  private async UniTask<bool> LoadNext(Mission mission)
  {
    this.networkManager.SendLoadWaitingMessage();
    this.networkManager.SendServerLoadingMessage(mission.Name);
    if (GameManager.gameState != GameState.ServerWaiting)
      await this.LoadEmptyScene();
    if (!await this.LoadMissionMap(mission))
      return false;
    this.networkManager.SendLoadMapMessageAll();
    return true;
  }

  private async UniTask<bool> LoadMissionMap(Mission mission)
  {
    MissionManager.SetMission(mission, false);
    GameManager.SetGameState(GameState.Multiplayer);
    GameManager.SetupGame();
    Progress<float> progress = new Progress<float>((Action<float>) (value => NetworkManagerNuclearOption.i.SendServerLoadingMessage($"{mission.Name}\nLoading Scene: {(int) ((double) value * 100.0)}%")));
    (bool, MapLoader.LoadResult) valueTuple = await this.networkManager.ServerLoadMapScene(mission.MapKey, (IProgress<float>) progress);
    if (!valueTuple.Item1)
    {
      Debug.LogError((object) $"Load {mission.MapKey} failed with result={valueTuple.Item2}");
      return false;
    }
    NetworkManagerNuclearOption.i.SendServerLoadingMessage(mission.Name + "\nInitializing Mission");
    this.networkManager.Host_SceneLoaded();
    return true;
  }

  private async UniTask LoadEmptyScene()
  {
    GameManager.SetGameState(GameState.ServerWaiting);
    MissionManager.SetNullMission();
    int num = await NetworkManagerNuclearOption.i.LoadSystemScene(MapLoader.Empty, false) ? 1 : 0;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public bool HasPlayers() => this.RealPlayerCount() > 0;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public int RealPlayerCount() => this.server.AuthenticatedPlayers.Count - 1;

  private bool GameShouldStop()
  {
    float timeSinceLevelLoad = Time.timeSinceLevelLoad;
    double unscaledTimeAsDouble = Time.unscaledTimeAsDouble;
    if (GameManager.gameResolution != GameResolution.Ongoing)
    {
      ColorLog<DedicatedServerManager>.Info($"Stopping mission, Game Resolution: {GameManager.gameResolution}");
      return true;
    }
    if ((double) timeSinceLevelLoad > (double) this.CurrentMissionOption.MaxTime)
    {
      ColorLog<DedicatedServerManager>.Info($"Stopping mission, Reached max time of {this.CurrentMissionOption.MaxTime} seconds");
      return true;
    }
    int num = this.RealPlayerCount();
    if (num > 0)
    {
      this.noPlayerStopTime = unscaledTimeAsDouble + (double) this.Config.NoPlayerStopTime;
    }
    else
    {
      ColorLog<DedicatedServerManager>.Info("No players");
      if (unscaledTimeAsDouble > this.noPlayerStopTime)
      {
        ColorLog<DedicatedServerManager>.Info($"Stopping mission, No players for {this.Config.NoPlayerStopTime} seconds");
        return true;
      }
    }
    ColorLog<DedicatedServerManager>.Info($"Running Mission Players:{num}, GameTime:{timeSinceLevelLoad:0} RealTime:{unscaledTimeAsDouble:0}");
    return false;
  }

  public void ReloadConfig(DedicatedServerConfig optionalNewConfig, string newConfigPath)
  {
    if (optionalNewConfig != null)
    {
      this.Config = optionalNewConfig;
      this.ConfigPath = newConfigPath;
    }
    else
    {
      DedicatedServerConfig config;
      if (DedicatedServerConfig.TryLoad(this.ConfigPath, out config))
        this.Config = config;
      else
        Debug.LogError((object) "Failed to reload config");
    }
    this.server.ErrorRateLimitEnabled = !this.Config.DisableErrorKick;
    SteamGameServer.SetServerName(this.Config.ServerName ?? "Nuclear Option Server");
    this.keyValues.SetKeyValue("modded_server", LobbyInstance.BoolToTag(((int) NetworkManagerNuclearOption.ModdedServer ?? (this.Config.ModdedServer ? 1 : 0)) != 0));
    if (this.Config.HasPassword)
      this.keyValues.SetKeyValue("short_password", LobbyPassword.GetShortPassword(this.Config.Password));
    else
      this.keyValues.SetKeyValue("short_password", (string) null);
    this.keyValues.ApplyValuesToSteam();
    SteamGameServer.SetMaxPlayerCount(this.Config.MaxPlayers);
    this.LoadAllowBanList();
    NetworkManagerNuclearOption.i.Server.PeerConfig.MaxConnections = this.Config.MaxPlayers;
    if (string.IsNullOrEmpty(this.Config.Password))
      NetworkManagerNuclearOption.i.Authenticator.ClearServerPassword();
    else
      NetworkManagerNuclearOption.i.Authenticator.SetServerPassword(this.Config.Password);
    this.missionRotation = new MissionRotation(this.Config.MissionRotation, this.Config.RotationType);
    if (!string.IsNullOrEmpty(this.Config.MissionDirectory))
      MissionGroup.UserGroup.SetDirectory(this.Config.MissionDirectory);
    ColorLog<DedicatedServerManager>.Info($"Set Advertise Server: {!this.Config.Hidden}");
    SteamGameServer.SetAdvertiseServerActive(!this.Config.Hidden);
  }

  public void SetTimeRemaining(float timeRemaining)
  {
    this.currentMissionOption.MaxTime = Time.timeSinceLevelLoad + timeRemaining;
  }

  public bool SetNextMission(MissionOptions option)
  {
    Mission mission;
    if (!DedicatedServerManager.PreLoadMission(option, out mission))
    {
      ColorLog<DedicatedServerManager>.Info("Failed to load mission, skipping OverrideNext");
      return false;
    }
    this.missionRotation.OverrideNext(option);
    if (!this.HasPlayers())
      this.PreloadAndUpdateLobby(this.missionRotation.PeakNext(), false, out mission);
    return true;
  }
}

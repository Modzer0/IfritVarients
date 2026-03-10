// Decompiled with JetBrains decompiler
// Type: NuclearOption.Networking.Lobbies.SteamLobby
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Cysharp.Threading.Tasks;
using Mirage;
using NuclearOption.Chat;
using NuclearOption.SavedMission;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;

#nullable disable
namespace NuclearOption.Networking.Lobbies;

public class SteamLobby : MonoBehaviour
{
  public static SteamLobby instance;
  public NetworkManagerNuclearOption Manager;
  private Callback<LobbyCreated_t> _lobbyCreated;
  private Callback<GameLobbyJoinRequested_t> _joinRequest;
  private Callback<LobbyMatchList_t> _lobbyList;
  private Callback<LobbyDataUpdate_t> _lobbyDataUpdated;
  private readonly Dictionary<CSteamID, LobbyInstance> _lobbyData = new Dictionary<CSteamID, LobbyInstance>();
  private HostedLobbyInstance _hostedLobby;
  private int _currentLobbyMaxPlayers;
  private LobbyInstance _joinedLobby;
  private bool _initialized;
  private bool hasLocalLocation;
  private UniTaskCompletionSource<LobbyCreated_t> HostLobbyCompletion;
  public string CurrentLobbyName;
  private LobbyInstance joinPendingLobby;
  private SteamLobby.ServerListRequest serverListRequest;
  private bool locationTaskRunning;
  private bool playerLobbyRefreshInProgress;
  private LobbySearchFilter? pendingSearch;

  public event Action OnLobbyListCleared;

  public event Action<LobbyInstance> OnLobbyDataUpdated;

  public event Action<ServerLobbyInstance> OnLobbyPingUpdated;

  public event Action OnLobbyRefreshFinished;

  public event Action<string> OnLocationUpdated;

  private bool serverLobbiesRefreshInProgress => this.serverListRequest.InProgress;

  private void Awake()
  {
    this.serverListRequest = new SteamLobby.ServerListRequest(this);
    if ((UnityEngine.Object) SteamLobby.instance == (UnityEngine.Object) null)
      SteamLobby.instance = this;
    UnityEngine.Object.DontDestroyOnLoad((UnityEngine.Object) this.gameObject);
    CancellationToken cancellationToken = this.destroyCancellationToken;
  }

  private void Start() => this.StartAsync().Forget();

  private async UniTaskVoid StartAsync()
  {
    SteamLobby steamLobby = this;
    await MainMenu.WaitForLoaded(steamLobby.destroyCancellationToken);
    if (!SteamManager.ClientInitialized && !SteamManager.ServerInitialized)
      Debug.LogError((object) "Steam was not Initialized");
    if (SteamManager.ClientInitialized)
    {
      steamLobby._lobbyCreated = Callback<LobbyCreated_t>.Create(new Callback<LobbyCreated_t>.DispatchDelegate(steamLobby.OnLobbyCreated));
      steamLobby._joinRequest = Callback<GameLobbyJoinRequested_t>.Create(new Callback<GameLobbyJoinRequested_t>.DispatchDelegate(steamLobby.OnJoinRequest));
      steamLobby._lobbyList = Callback<LobbyMatchList_t>.Create(new Callback<LobbyMatchList_t>.DispatchDelegate(steamLobby.RequestLobbyListCallback));
      steamLobby._lobbyDataUpdated = Callback<LobbyDataUpdate_t>.Create(new Callback<LobbyDataUpdate_t>.DispatchDelegate(steamLobby.LobbyDataUpdated));
      steamLobby._initialized = true;
      steamLobby.Manager.Client.Disconnected.AddListener(new UnityAction<ClientStoppedReason>(steamLobby.ClientDisconnected));
      steamLobby.Manager.Server.Stopped.AddListener(new UnityAction(steamLobby.SetLobbyEnded));
    }
    int num = SteamManager.ServerInitialized ? 1 : 0;
  }

  private void OnDestroy()
  {
    if (!this._initialized)
      return;
    this._lobbyCreated.Dispose();
    this._joinRequest.Dispose();
    this._lobbyList.Dispose();
    this._lobbyDataUpdated.Dispose();
    this.Manager.Client.Disconnected.RemoveListener(new UnityAction<ClientStoppedReason>(this.ClientDisconnected));
  }

  private void ClientDisconnected(ClientStoppedReason _)
  {
    if (this.Manager.Server.IsHost)
      return;
    this.LeaveLobby();
  }

  private void SetLobbyEnded()
  {
    this._hostedLobby.SetLobbyEnded();
    this.LeaveLobby();
  }

  private void OnApplicationQuit() => this.LeaveLobby();

  public async UniTask<HostedLobbyInstance?> HostLobby(int maxPlayers, ELobbyType lobbyType)
  {
    (bool flag, LobbyCreated_t lobbyCreatedT) = await this.Steam_CreateLobbyAsync(lobbyType);
    if (flag)
    {
      JoinLobbyOverlay.Open(JoinProgress.Fail("Timeout while creating lobby"));
      return new HostedLobbyInstance?();
    }
    if (lobbyCreatedT.m_eResult != EResult.k_EResultOK)
    {
      Debug.LogError((object) $"Fail to create lobby: {lobbyCreatedT.m_eResult}");
      JoinLobbyOverlay.Open(JoinProgress.Fail("Failed to create new lobby"));
      return new HostedLobbyInstance?();
    }
    ColorLog<SteamLobby>.Info($"Lobby created successfully {lobbyCreatedT.m_ulSteamIDLobby}");
    this._hostedLobby = new HostedLobbyInstance(new CSteamID(lobbyCreatedT.m_ulSteamIDLobby));
    this._hostedLobby.SetData("HostAddress", SteamUser.GetSteamID().ToString());
    this._hostedLobby.SetData("version", Application.version);
    this._hostedLobby.SetStartTime();
    this._hostedLobby.SetData("max_members", maxPlayers.ToString());
    this._currentLobbyMaxPlayers = maxPlayers;
    return new HostedLobbyInstance?(this._hostedLobby);
  }

  private async UniTask<(bool IsTimeout, LobbyCreated_t Result)> Steam_CreateLobbyAsync(
    ELobbyType lobbyType)
  {
    if (this.HostLobbyCompletion != null)
    {
      Debug.LogError((object) "Steam_CreateLobbyAsync already running");
      return ();
    }
    ColorLog<SteamLobby>.Info("Hosting Lobby");
    this.HostLobbyCompletion = new UniTaskCompletionSource<LobbyCreated_t>();
    SteamMatchmaking.CreateLobby(lobbyType, 250);
    (bool flag, LobbyCreated_t lobbyCreatedT) = await this.HostLobbyCompletion.Task.TimeoutWithoutException<LobbyCreated_t>(TimeSpan.FromSeconds(20.0));
    this.HostLobbyCompletion = (UniTaskCompletionSource<LobbyCreated_t>) null;
    if (flag)
      Debug.LogError((object) "Create lobby timeout");
    return (flag, lobbyCreatedT);
  }

  private void OnLobbyCreated(LobbyCreated_t result)
  {
    ColorLog<SteamLobby>.Info("LobbyCreated_t callback");
    if (this.HostLobbyCompletion != null)
      this.HostLobbyCompletion.TrySetResult(result);
    else
      ColorLog<SteamLobby>.LogError("LobbyCreated_t no HostLobbyCompletion event, CreateLobby may have timed out before steam returned result");
  }

  public void CheckRelayLocationTask()
  {
    if (this.locationTaskRunning)
      return;
    this.GetLocationLoop().Forget();
  }

  private async UniTaskVoid GetLocationLoop()
  {
    SteamLobby steamLobby = this;
    steamLobby.locationTaskRunning = true;
    try
    {
      await steamLobby.WaitForLocalLocation();
      CancellationToken cancel = steamLobby.destroyCancellationToken;
      while (!cancel.IsCancellationRequested)
      {
        string location;
        if (SteamLobby.GetLocalLocation(out location))
          steamLobby.LocationUpdated(location);
        else
          Debug.LogError((object) "Failed to get host location");
        await UniTask.Delay(TimeSpan.FromMinutes(5.0));
      }
      cancel = new CancellationToken();
    }
    finally
    {
      steamLobby.locationTaskRunning = false;
    }
  }

  public void CheckPingServers()
  {
    float time = Time.time;
    foreach (LobbyInstance lobbyInstance in this._lobbyData.Values)
    {
      if (lobbyInstance is ServerLobbyInstance lobby && lobby.InList && (double) time > (double) lobby.NextPingTime)
      {
        lobby.NextPingTime = time + intervalFromCount(lobby.PingCount);
        new SteamLobby.PingQuery(this, this.serverListRequest.RunningQueries, lobby).Run();
      }
    }

    static float intervalFromCount(int count) => count < 5 ? (float) count * 2f : 30f;
  }

  public void ClearLobbyCache() => this._lobbyData.Clear();

  private async UniTask WaitForLocalLocation()
  {
    SteamLobby steamLobby = this;
    CancellationToken cancel;
    if (steamLobby.hasLocalLocation)
    {
      cancel = new CancellationToken();
    }
    else
    {
      SteamNetworkingUtils.InitRelayNetworkAccess();
      cancel = steamLobby.destroyCancellationToken;
      int attempt = 0;
      float totalWait = 0.0f;
      while (!cancel.IsCancellationRequested)
      {
        if (SteamLobby.GetLocalLocation(out string _))
        {
          Debug.Log((object) "GetLocalPingLocation Success");
          steamLobby.hasLocalLocation = true;
          cancel = new CancellationToken();
          return;
        }
        ++attempt;
        int millisecondsDelay = Mathf.Clamp(attempt < 8 ? 100 * (int) Mathf.Pow(2f, (float) attempt) : 20000, 200, 20000);
        totalWait += (float) millisecondsDelay;
        Debug.Log((object) $"GetLocalPingLocation Failed, attempt={attempt}, waiting {millisecondsDelay}ms, totalWait={(ValueType) (float) ((double) totalWait / 1000.0):0.0}s");
        await UniTask.Delay(millisecondsDelay);
      }
      cancel = new CancellationToken();
    }
  }

  private void LocationUpdated(string loc)
  {
    Action<string> onLocationUpdated = this.OnLocationUpdated;
    if (onLocationUpdated != null)
      onLocationUpdated(loc);
    if (!this._hostedLobby.IsValid)
      return;
    this._hostedLobby.SetData("HostPing", loc);
  }

  public static bool EstimatePing(string hostLocation, out int ping)
  {
    SteamNetworkPingLocation_t result;
    if (!SteamNetworkingUtils.ParsePingLocationString(hostLocation, out result))
    {
      ping = 0;
      return false;
    }
    ping = SteamNetworkingUtils.EstimatePingTimeFromLocalHost(ref result);
    return ping != -1;
  }

  private void OnJoinRequest(GameLobbyJoinRequested_t callback)
  {
    if (!SteamMatchmaking.RequestLobbyData(callback.m_steamIDLobby))
      Debug.LogError((object) "Failed to get lobby data");
    else
      this.joinPendingLobby = (LobbyInstance) new PlayerLobbyInstance(callback.m_steamIDLobby);
  }

  public void TryJoinLobby(LobbyInstance lobby, string password, bool promptIfPasswordNeeded)
  {
    if (lobby.HostVersion != Application.version)
    {
      Debug.LogError((object) "Version mismatch. Unable to join lobby.");
      JoinLobbyOverlay.Open(JoinProgress.Fail("Incorrect version"));
    }
    else if (promptIfPasswordNeeded && string.IsNullOrEmpty(password) && lobby.IsPasswordProtected(out string _))
    {
      Debug.LogWarning((object) "Showing password prompt to user because lobby has password");
      LobbyPasswordPromptSingleton.ShowPrompt(lobby);
    }
    else
    {
      JoinLobbyOverlay.Open(JoinProgress.Joining("Connecting via Steam"));
      if (!LobbyPassword.TestShortPassword(lobby, password))
      {
        UniTask.Void((Func<UniTaskVoid>) (async () =>
        {
          await UniTask.Delay(UnityEngine.Random.Range(300, 800));
          JoinLobbyOverlay.Open(JoinProgress.Fail("Password incorrect"));
          LobbyPasswordPromptSingleton.ShowPrompt(lobby);
        }));
        Debug.LogError((object) "Password does not match lobby data");
      }
      else
      {
        ColorLog<SteamLobby>.Info($"Connecting to '{lobby.LobbyNameSanitized}' Dedicated={lobby.DedicatedServer} Modded={lobby.ModdedServer}");
        this._joinedLobby = lobby;
        this.CurrentLobbyName = lobby.LobbyNameSanitized;
        string hostAddress = lobby.HostAddress;
        string udpAddress = lobby.UdpAddress;
        string udpPort = lobby.UdpPort;
        ConnectOptions options;
        if (string.IsNullOrEmpty(udpAddress) || string.IsNullOrEmpty(udpPort))
        {
          if (ulong.TryParse(hostAddress, out ulong _))
          {
            ColorLog<SteamLobby>.Info("Connecting via steam to " + hostAddress);
            options = new ConnectOptions(SocketType.Steam, hostAddress);
          }
          else
          {
            ColorLog<SteamLobby>.InfoWarn("Steam Socket invalid id:" + hostAddress);
            JoinLobbyOverlay.Open(JoinProgress.Fail("Did not have valid SteamId for host"));
            return;
          }
        }
        else
        {
          ushort result;
          if (ushort.TryParse(udpPort, out result))
          {
            ColorLog<SteamLobby>.Info($"Connecting via UDP to {udpAddress}:{result}");
            options = new ConnectOptions(SocketType.UDP, udpAddress, new int?((int) result));
          }
          else
          {
            Debug.LogError((object) "Failed to part udp port");
            this.LeaveLobby();
            return;
          }
        }
        options.Password = password;
        try
        {
          this.Manager.StartClient(options);
        }
        catch (Exception ex)
        {
          Debug.LogException(ex);
          JoinLobbyOverlay.Open(JoinProgress.Fail("Client Error"));
        }
      }
    }
  }

  public void GetLobbiesList(LobbySearchFilter lobbyFilter)
  {
    ColorLog<SteamLobby>.Info("GetLobbiesList clearing lobbies");
    this.RequestLobbies(lobbyFilter);
  }

  private void RequestLobbies(LobbySearchFilter filter)
  {
    if (!SteamUser.BLoggedOn())
    {
      ColorLog<SteamLobby>.Info("SteamUser not logged on");
      JoinLobbyOverlay.Open(JoinProgress.Fail("Not logged in or connected to steam"));
    }
    else if (this.serverLobbiesRefreshInProgress || this.playerLobbyRefreshInProgress)
    {
      ColorLog<SteamLobby>.InfoWarn($"Refresh in progress, saving pending search. server:{this.serverLobbiesRefreshInProgress} player:{this.playerLobbyRefreshInProgress}");
      this.serverListRequest.Cancel();
      this.pendingSearch = new LobbySearchFilter?(filter);
    }
    else
    {
      foreach (LobbyInstance lobbyInstance in this._lobbyData.Values)
        lobbyInstance.InList = false;
      Action lobbyListCleared = this.OnLobbyListCleared;
      if (lobbyListCleared != null)
        lobbyListCleared();
      switch (filter.ServerType)
      {
        case FilterServerType.All:
          this.RequestPlayerHostedLobbies(filter);
          this.serverListRequest.RequestServerLobbies(filter);
          break;
        case FilterServerType.DedicatedServerOnly:
          this.serverListRequest.RequestServerLobbies(filter);
          break;
        case FilterServerType.PlayerHostedOnly:
          this.RequestPlayerHostedLobbies(filter);
          break;
      }
    }
  }

  private bool CheckPendingRefresh()
  {
    if (this.destroyCancellationToken.IsCancellationRequested || !this.pendingSearch.HasValue)
      return false;
    ColorLog<SteamLobby>.Info($"Waiting for pending search server:{this.serverLobbiesRefreshInProgress} player:{this.playerLobbyRefreshInProgress}");
    if (this.serverLobbiesRefreshInProgress || this.playerLobbyRefreshInProgress)
      return false;
    ColorLog<SteamLobby>.Info("Running pending refresh now");
    LobbySearchFilter filter = this.pendingSearch.Value;
    this.pendingSearch = new LobbySearchFilter?();
    this.RequestLobbies(filter);
    return true;
  }

  private static bool GetLocalLocation(out string location)
  {
    SteamNetworkPingLocation_t result;
    if ((double) SteamNetworkingUtils.GetLocalPingLocation(out result) == -1.0)
    {
      location = (string) null;
      return false;
    }
    SteamNetworkingUtils.ConvertPingLocationToString(ref result, out location, 1024 /*0x0400*/);
    return true;
  }

  private void RequestPlayerHostedLobbies(LobbySearchFilter filter)
  {
    this.playerLobbyRefreshInProgress = true;
    ColorLog<SteamLobby>.Info(nameof (RequestPlayerHostedLobbies));
    SteamMatchmaking.AddRequestLobbyListDistanceFilter((ELobbyDistanceFilter) ((int) filter.distanceFilter ?? 3));
    if (filter.HideFull)
      SteamMatchmaking.AddRequestLobbyListNumericalFilter("open_member_spots", 0, ELobbyComparison.k_ELobbyComparisonNotEqual);
    if (!filter.ignoreVersionFilter)
      SteamMatchmaking.AddRequestLobbyListStringFilter("version", Application.version, ELobbyComparison.k_ELobbyComparisonEqual);
    if (filter.HidePasswordProtected)
      SteamMatchmaking.AddRequestLobbyListStringFilter("short_password", "", ELobbyComparison.k_ELobbyComparisonEqual);
    switch (filter.MissionPvpType)
    {
      case MissionPvpType.Pvp:
      case MissionPvpType.Pve:
        SteamMatchmaking.AddRequestLobbyListStringFilter("mission_pvp_type", MissionTag.GetPvpTypeLobbyString(filter.MissionPvpType), ELobbyComparison.k_ELobbyComparisonEqual);
        break;
    }
    SteamMatchmaking.RequestLobbyList();
  }

  private void RequestLobbyListCallback(LobbyMatchList_t result)
  {
    ColorLog<SteamLobby>.Info($"Got List of Lobbies, Count={result.m_nLobbiesMatching}");
    this.playerLobbyRefreshInProgress = false;
    if (this.CheckPendingRefresh())
    {
      ColorLog<SteamLobby>.Info("Skipping RequestLobbyData because of pending refresh");
    }
    else
    {
      if (!this.serverLobbiesRefreshInProgress)
      {
        Action lobbyRefreshFinished = this.OnLobbyRefreshFinished;
        if (lobbyRefreshFinished != null)
          lobbyRefreshFinished();
      }
      for (int iLobby = 0; (long) iLobby < (long) result.m_nLobbiesMatching; ++iLobby)
      {
        CSteamID lobbyByIndex = SteamMatchmaking.GetLobbyByIndex(iLobby);
        if (!this._lobbyData.ContainsKey(lobbyByIndex))
          this._lobbyData.Add(lobbyByIndex, (LobbyInstance) new PlayerLobbyInstance(lobbyByIndex));
        SteamMatchmaking.RequestLobbyData(lobbyByIndex);
      }
    }
  }

  private void LobbyDataUpdated(LobbyDataUpdate_t result)
  {
    ColorLog<SteamLobby>.Info($"Got Lobby data for {result.m_ulSteamIDLobby}");
    CSteamID key = new CSteamID(result.m_ulSteamIDLobby);
    if (this.joinPendingLobby is PlayerLobbyInstance joinPendingLobby && joinPendingLobby.LobbyId == key)
    {
      ColorLog<SteamLobby>.Info("Join pending, starting to join");
      this.joinPendingLobby = (LobbyInstance) null;
      this.TryJoinLobby((LobbyInstance) joinPendingLobby, (string) null, true);
    }
    else
    {
      LobbyInstance lobbyInstance;
      if (this._lobbyData.TryGetValue(key, out lobbyInstance))
      {
        ulong result1;
        if (!ulong.TryParse(lobbyInstance.HostAddress, out result1))
        {
          ColorLog<SteamLobby>.InfoWarn($"lobby {key} has no address");
          this._lobbyData.Remove(key);
        }
        else if (BlockList.IsBlocked(result1))
        {
          ColorLog<SteamLobby>.InfoWarn($"lobby {key} (owner {result1}) is blocked");
          this._lobbyData.Remove(key);
        }
        else
        {
          ColorLog<SteamLobby>.Info($"Found lobby {key} (owner {result1}) in dictionary, updating");
          lobbyInstance.InList = true;
          Action<LobbyInstance> lobbyDataUpdated = this.OnLobbyDataUpdated;
          if (lobbyDataUpdated == null)
            return;
          lobbyDataUpdated(lobbyInstance);
        }
      }
      else
        ColorLog<SteamLobby>.InfoWarn("Could not find lobby in dictionary");
    }
  }

  public void LeaveLobby()
  {
    if (!this._initialized)
      return;
    if (this._hostedLobby.IsValid)
      SteamMatchmaking.LeaveLobby(this._hostedLobby.Id);
    this._hostedLobby = new HostedLobbyInstance();
    this._joinedLobby = (LobbyInstance) null;
  }

  public void SetServerPlayerCount(int playerCount)
  {
    if (!this._hostedLobby.IsValid)
      return;
    this._hostedLobby.SetCurrentPlayers(playerCount, this._currentLobbyMaxPlayers);
  }

  private class ServerListRequest
  {
    public readonly List<SteamLobby.ISteamQuery> RunningQueries = new List<SteamLobby.ISteamQuery>();
    private readonly SteamLobby manager;
    private readonly ISteamMatchmakingServerListResponse response;
    private LobbySearchFilter filter;
    private HServerListRequest? request;

    public int searchNumber { get; private set; }

    public bool InProgress { get; private set; }

    public ServerListRequest(SteamLobby manager)
    {
      this.manager = manager;
      this.response = new ISteamMatchmakingServerListResponse(new ISteamMatchmakingServerListResponse.ServerResponded(this.OnServerResponded), new ISteamMatchmakingServerListResponse.ServerFailedToRespond(this.OnServerFailedToRespond), new ISteamMatchmakingServerListResponse.RefreshComplete(this.OnRefreshComplete));
    }

    public void RequestServerLobbies(LobbySearchFilter filter)
    {
      if (this.request.HasValue)
      {
        ColorLog<SteamLobby>.InfoWarn("RequestServerLobbies was already refreshing, ignoring 2nd request");
      }
      else
      {
        ++this.searchNumber;
        ColorLog<SteamLobby>.Info($"RequestServerLobbies searchNumber={this.searchNumber}");
        this.InProgress = true;
        MatchMakingKeyValuePair_t[] ppchFilters = SteamLobby.ServerListRequest.BuildServerRequestFilters(filter);
        this.request = new HServerListRequest?(SteamMatchmakingServers.RequestInternetServerList(SteamUtils.GetAppID(), ppchFilters, (uint) ppchFilters.Length, this.response));
        UniTask.Void((Func<UniTaskVoid>) (async () =>
        {
          while (SteamMatchmakingServers.IsRefreshing(this.request.Value))
          {
            await UniTask.Yield();
            if (this.manager.destroyCancellationToken.IsCancellationRequested)
            {
              ColorLog<SteamLobby>.Info("Request cancelled from destroyCancellationToken");
              SteamMatchmakingServers.CancelQuery(this.request.Value);
              break;
            }
          }
          ColorLog<SteamLobby>.Info("Request finished (or cancelled)");
          SteamMatchmakingServers.ReleaseRequest(this.request.Value);
          this.InProgress = false;
          this.request = new HServerListRequest?();
          this.manager.CheckPendingRefresh();
        }));
      }
    }

    public void Cancel()
    {
      ColorLog<SteamLobby>.Info("Request cancelled because refresh pressed again");
      if (this.request.HasValue)
        SteamMatchmakingServers.CancelQuery(this.request.Value);
      foreach (SteamLobby.ISteamQuery runningQuery in this.RunningQueries)
        runningQuery.Cancel(false);
      this.RunningQueries.Clear();
    }

    private static MatchMakingKeyValuePair_t[] BuildServerRequestFilters(LobbySearchFilter filter)
    {
      List<MatchMakingKeyValuePair_t> makingKeyValuePairTList1 = new List<MatchMakingKeyValuePair_t>();
      List<string> values = new List<string>();
      MatchMakingKeyValuePair_t makingKeyValuePairT1;
      if (filter.HideFull)
      {
        List<MatchMakingKeyValuePair_t> makingKeyValuePairTList2 = makingKeyValuePairTList1;
        makingKeyValuePairT1 = new MatchMakingKeyValuePair_t();
        makingKeyValuePairT1.m_szKey = "notfull";
        MatchMakingKeyValuePair_t makingKeyValuePairT2 = makingKeyValuePairT1;
        makingKeyValuePairTList2.Add(makingKeyValuePairT2);
      }
      if (filter.HideEmpty)
      {
        List<MatchMakingKeyValuePair_t> makingKeyValuePairTList3 = makingKeyValuePairTList1;
        makingKeyValuePairT1 = new MatchMakingKeyValuePair_t();
        makingKeyValuePairT1.m_szKey = "hasplayers";
        MatchMakingKeyValuePair_t makingKeyValuePairT3 = makingKeyValuePairT1;
        makingKeyValuePairTList3.Add(makingKeyValuePairT3);
      }
      if (!filter.ignoreVersionFilter)
        values.Add("v=" + Application.version);
      if (filter.HidePasswordProtected)
        values.Add("p=0");
      switch (filter.MissionPvpType)
      {
        case MissionPvpType.Pvp:
        case MissionPvpType.Pve:
          values.Add("t=" + MissionTag.GetPvpTypeLobbyString(filter.MissionPvpType));
          break;
      }
      if (values.Count > 0)
      {
        List<MatchMakingKeyValuePair_t> makingKeyValuePairTList4 = makingKeyValuePairTList1;
        makingKeyValuePairT1 = new MatchMakingKeyValuePair_t();
        makingKeyValuePairT1.m_szKey = "gametagsand";
        makingKeyValuePairT1.m_szValue = string.Join(",", (IEnumerable<string>) values);
        MatchMakingKeyValuePair_t makingKeyValuePairT4 = makingKeyValuePairT1;
        makingKeyValuePairTList4.Add(makingKeyValuePairT4);
      }
      return makingKeyValuePairTList1.ToArray();
    }

    private void OnServerResponded(HServerListRequest hRequest, int iServer)
    {
      ColorLog<SteamLobby>.Info($"Request Server List, success index:{iServer}");
      this.OnServerUpdated(SteamMatchmakingServers.GetServerDetails(hRequest, iServer), this.filter);
    }

    private void OnServerFailedToRespond(HServerListRequest hRequest, int iServer)
    {
      gameserveritem_t serverDetails = SteamMatchmakingServers.GetServerDetails(hRequest, iServer);
      ColorLog<SteamLobby>.InfoWarn($"Request Server List, fail index:{iServer}, Query Address:{serverDetails.m_NetAdr.GetQueryAddressString()}");
    }

    private void OnRefreshComplete(HServerListRequest hRequest, EMatchMakingServerResponse response)
    {
      ColorLog<SteamLobby>.Info($"Request Server List, Refresh complete: {response}");
      if (this.manager.playerLobbyRefreshInProgress)
        return;
      Action lobbyRefreshFinished = this.manager.OnLobbyRefreshFinished;
      if (lobbyRefreshFinished == null)
        return;
      lobbyRefreshFinished();
    }

    private void OnServerUpdated(gameserveritem_t details, LobbySearchFilter filter)
    {
      if (this.manager.pendingSearch.HasValue)
        ColorLog<SteamLobby>.Info("Skipping OnServerUpdated because of pending refresh");
      else if (details == null)
      {
        Debug.LogError((object) "No server details");
      }
      else
      {
        CSteamID steamId = details.m_steamID;
        ColorLog<SteamLobby>.Info($"{steamId} Game Tags:{details.GetGameTags()}");
        LobbyInstance lobbyInstance;
        ServerLobbyInstance lobby;
        if (this.manager._lobbyData.TryGetValue(steamId, out lobbyInstance))
        {
          if (lobbyInstance is ServerLobbyInstance serverLobbyInstance)
          {
            lobby = serverLobbyInstance;
          }
          else
          {
            Debug.LogError((object) $"Server with id {steamId} was not a Server");
            return;
          }
        }
        else
        {
          lobby = new ServerLobbyInstance();
          this.manager._lobbyData.Add(steamId, (LobbyInstance) lobby);
        }
        lobby.SetDetails(details);
        new SteamLobby.SearchQuery(this.manager, this, lobby, filter).Run();
      }
    }
  }

  private interface ISteamQuery
  {
    void Cancel(bool remove);
  }

  private class SearchQuery : SteamLobby.ISteamQuery
  {
    private readonly SteamLobby manager;
    private readonly ServerLobbyInstance lobby;
    private readonly SteamLobby.ServerListRequest listRequest;
    private readonly LobbySearchFilter filter;
    private readonly ISteamMatchmakingRulesResponse response;
    private HServerQuery query;
    private int searchNumber;

    public SearchQuery(
      SteamLobby manager,
      SteamLobby.ServerListRequest listRequest,
      ServerLobbyInstance lobby,
      LobbySearchFilter filter)
    {
      this.manager = manager;
      this.listRequest = listRequest;
      this.lobby = lobby;
      this.filter = filter;
      this.response = new ISteamMatchmakingRulesResponse(new ISteamMatchmakingRulesResponse.RulesResponded(this.RulesResponded), new ISteamMatchmakingRulesResponse.RulesFailedToRespond(this.RulesFailedToRespond), new ISteamMatchmakingRulesResponse.RulesRefreshComplete(this.RulesRefreshComplete));
    }

    public void Run()
    {
      this.searchNumber = this.listRequest.searchNumber;
      this.listRequest.RunningQueries.Add((SteamLobby.ISteamQuery) this);
      ColorLog<SteamLobby>.Info($"Server[{this.lobby.details.m_steamID}] SearchQuery for search {this.searchNumber}");
      servernetadr_t netAdr = this.lobby.details.m_NetAdr;
      this.query = SteamMatchmakingServers.ServerRules(netAdr.GetIP(), netAdr.GetQueryPort(), this.response);
    }

    public void Cancel(bool remove = true)
    {
      SteamMatchmakingServers.CancelServerQuery(this.query);
      if (!remove)
        return;
      this.listRequest.RunningQueries.Remove((SteamLobby.ISteamQuery) this);
    }

    private void RulesResponded(string key, string value)
    {
      if (string.IsNullOrEmpty(key))
        return;
      this.lobby.SetRule(key, value ?? string.Empty);
    }

    private void RulesFailedToRespond()
    {
      ColorLog<SteamLobby>.InfoWarn($"Server[{this.lobby.details.m_steamID}] Rule failed to respond");
      this.listRequest.RunningQueries.Remove((SteamLobby.ISteamQuery) this);
    }

    private void RulesRefreshComplete()
    {
      ColorLog<SteamLobby>.Info($"Server[{this.lobby.details.m_steamID}] Rule refresh complete");
      this.listRequest.RunningQueries.Remove((SteamLobby.ISteamQuery) this);
      int current;
      int max;
      if (this.filter.HidePasswordProtected && this.lobby.IsPasswordProtected(out string _) || this.filter.HideFull && this.lobby.GetPlayerCounts(out current, out max) && current >= max || !this.filter.PingDistanceAllowed(this.lobby.details.m_nPing))
        return;
      if (this.listRequest.searchNumber != this.searchNumber)
      {
        ColorLog<SteamLobby>.InfoWarn($"Server[{this.lobby.details.m_steamID}] searchNumber ({this.searchNumber} -> {this.listRequest.searchNumber}) changed before rule refresh completed, Skipping OnLobbyDataUpdated");
      }
      else
      {
        this.lobby.InList = true;
        Action<LobbyInstance> lobbyDataUpdated = this.manager.OnLobbyDataUpdated;
        if (lobbyDataUpdated == null)
          return;
        lobbyDataUpdated((LobbyInstance) this.lobby);
      }
    }
  }

  private class PingQuery : SteamLobby.ISteamQuery
  {
    private readonly SteamLobby manager;
    private readonly ServerLobbyInstance lobby;
    private readonly List<SteamLobby.ISteamQuery> runningQueries;
    private readonly ISteamMatchmakingPingResponse response;
    private HServerQuery query;

    public PingQuery(
      SteamLobby manager,
      List<SteamLobby.ISteamQuery> runningQueries,
      ServerLobbyInstance lobby)
    {
      this.manager = manager;
      this.runningQueries = runningQueries;
      this.lobby = lobby;
      this.response = new ISteamMatchmakingPingResponse(new ISteamMatchmakingPingResponse.ServerResponded(this.ServerResponded), new ISteamMatchmakingPingResponse.ServerFailedToRespond(this.ServerFailedToRespond));
    }

    public void Run()
    {
      this.runningQueries.Add((SteamLobby.ISteamQuery) this);
      servernetadr_t netAdr = this.lobby.details.m_NetAdr;
      this.query = SteamMatchmakingServers.PingServer(netAdr.GetIP(), netAdr.GetQueryPort(), this.response);
      ColorLog<SteamLobby>.Info($"Server[{this.lobby.details.m_steamID}] Run ping query");
    }

    public void Cancel(bool remove = true)
    {
      SteamMatchmakingServers.CancelServerQuery(this.query);
      if (!remove)
        return;
      this.runningQueries.Remove((SteamLobby.ISteamQuery) this);
    }

    private void ServerResponded(gameserveritem_t server)
    {
      this.runningQueries.Remove((SteamLobby.ISteamQuery) this);
      this.lobby.SetPingResult(server.m_nPing);
      if (!this.lobby.InList)
        return;
      Action<ServerLobbyInstance> lobbyPingUpdated = this.manager.OnLobbyPingUpdated;
      if (lobbyPingUpdated == null)
        return;
      lobbyPingUpdated(this.lobby);
    }

    private void ServerFailedToRespond()
    {
      this.runningQueries.Remove((SteamLobby.ISteamQuery) this);
    }
  }
}

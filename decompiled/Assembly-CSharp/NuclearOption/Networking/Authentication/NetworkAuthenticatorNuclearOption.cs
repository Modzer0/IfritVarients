// Decompiled with JetBrains decompiler
// Type: NuclearOption.Networking.Authentication.NetworkAuthenticatorNuclearOption
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Cysharp.Threading.Tasks;
using Mirage;
using Mirage.Authentication;
using Mirage.SocketLayer;
using Mirage.SteamworksSocket;
using NuclearOption.DedicatedServer;
using NuclearOption.Networking.Lobbies;
using Steamworks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;

#nullable disable
namespace NuclearOption.Networking.Authentication;

public class NetworkAuthenticatorNuclearOption : 
  NetworkAuthenticator<NetworkAuthenticatorNuclearOption.AuthMessage>
{
  private const int MAX_AUTH_TICKET_SIZE = 1024 /*0x0400*/;
  private const string BuildHashFilePath = "build-hash.txt";
  private static uint? buildHash;
  public SteamworksSocketFactory SteamTransport;
  [SerializeField]
  private NetworkServer server;
  [SerializeField]
  private NetworkClient client;
  [SerializeField]
  private TimeoutConfig timeoutConfig;
  private TimeoutManager timeoutManager;
  private readonly HashSet<CSteamID> kickedPlayers = new HashSet<CSteamID>();
  public readonly AllowBanList BanList = new AllowBanList();
  public readonly AllowBanList ErrorKickImmuneList = new AllowBanList();
  private readonly Dictionary<CSteamID, INetworkPlayer> steamPlayerLookup = new Dictionary<CSteamID, INetworkPlayer>();
  private LobbyPassword lobbyPassword;
  private readonly Dictionary<INetworkPlayer, NetworkAuthenticatorNuclearOption.Challenge> challenges = new Dictionary<INetworkPlayer, NetworkAuthenticatorNuclearOption.Challenge>();
  private readonly Dictionary<CSteamID, UniTaskCompletionSource<NetworkAuthenticatorNuclearOption.AuthTicketResult>> steamAuthTokenLookup = new Dictionary<CSteamID, UniTaskCompletionSource<NetworkAuthenticatorNuclearOption.AuthTicketResult>>();
  private string clientPassword;
  private HAuthTicket clientAuthTicket;
  private Callback<ValidateAuthTicketResponse_t> serverAuthCallback;
  private Callback<GetAuthSessionTicketResponse_t> clientAuthCallback;
  private UniTaskCompletionSource<GetAuthSessionTicketResponse_t> getTicketResult;
  private CancellationTokenSource clientDisconnectCancel;
  private CancellationTokenSource serverStopCancel;
  private Pool<byte[]> bufferPool = new Pool<byte[]>((Pool<byte[]>.CreateNewItemNoCount) (p => new byte[1024 /*0x0400*/]), 1, 10);

  private void Awake()
  {
    if (this.server.Active)
      Debug.LogError((object) "Server should not be active before NetworkAuthenticatorNuclearOption.Awake is called");
    this.timeoutManager = new TimeoutManager(this.timeoutConfig);
    this.SteamTransport.AcceptCallback = new Server.AcceptConnectionCallback(this.SteamNetAcceptCallback);
    this.server.SetErrorRateLimitReachedCallback(new NetworkServer.RateLimitCallback(this.ErrorRateLimitReached));
    this.server.SetAuthenticationFailedCallback(new NetworkServer.AuthFailCallback(this.ServerAuthFailed));
    this.server.Started.AddListener(new UnityAction(this.OnStartServer));
    this.server.Stopped.AddListener(new UnityAction(this.OnStopServer));
    this.server.Disconnected.AddListener(new UnityAction<INetworkPlayer>(this.OnServerDisconnected));
    this.client.Connected.AddListener((UnityAction<INetworkPlayer>) (p => this.OnClientConnected(p).Forget()));
    this.client.Disconnected.AddListener(new UnityAction<ClientStoppedReason>(this.OnClientDisconnected));
  }

  private void ErrorRateLimitReached(INetworkPlayer player)
  {
    ColorLog<NetworkAuthenticatorNuclearOption>.Info($"ErrorKick called because of errors from {player}, error flags:{player.ErrorFlags}");
    try
    {
      if (player.ConnectionHandle is SteamConnection connectionHandle)
      {
        CSteamID steamId = connectionHandle.SteamID;
        if (this.ErrorKickImmuneList.Contains(steamId))
        {
          ColorLog<NetworkAuthenticatorNuclearOption>.Info($"Skipping ErrorKick because {steamId} is in ErrorKickImmuneList");
          return;
        }
        if (this.timeoutManager.OnKickFromError(steamId, player.ErrorFlags))
        {
          ColorLog<NetworkAuthenticatorNuclearOption>.Info($"ErrorKick limit reached banning {steamId}");
          if (DedicatedServerManager.IsRunning)
            AllowBanList.BanAndAppendId(this.BanList, DedicatedServerManager.Instance.Config.BanListPaths[0], steamId, "Error Auto Ban");
        }
      }
    }
    catch (Exception ex)
    {
      Debug.LogException(ex);
    }
    player.Disconnect();
  }

  private bool SteamNetAcceptCallback(SteamNetConnectionStatusChangedCallback_t param)
  {
    CSteamID csteamId = new CSteamID(param.m_info.m_identityRemote.GetSteamID64());
    return !this.IsKickedOrBanned(csteamId) && !this.timeoutManager.HasTimeout(csteamId);
  }

  private void OnDestroy()
  {
    this.DisposeServerCallbacks();
    this.DisposeClientCallbacks();
  }

  private void DisposeServerCallbacks()
  {
    if (this.serverAuthCallback == null)
      return;
    this.serverAuthCallback.Dispose();
    this.serverAuthCallback = (Callback<ValidateAuthTicketResponse_t>) null;
  }

  private void DisposeClientCallbacks()
  {
    if (this.clientAuthCallback == null)
      return;
    this.clientAuthCallback.Dispose();
    this.clientAuthCallback = (Callback<GetAuthSessionTicketResponse_t>) null;
  }

  private void ServerAuthFailed(INetworkPlayer player, AuthenticationResult result)
  {
    ColorLog<NetworkAuthenticatorNuclearOption>.Info($"{player} failed to authenticated: {result.Reason}");
    player.Disconnect();
  }

  private async UniTask SendFailReason(INetworkPlayer player, string failReason)
  {
    ColorLog<NetworkAuthenticatorNuclearOption>.Info($"Sending fail reason to {player}, reason={failReason}");
    player.Send<NetworkAuthenticatorNuclearOption.AuthFailReason>(new NetworkAuthenticatorNuclearOption.AuthFailReason()
    {
      Reason = failReason
    });
    await UniTask.Delay(500);
  }

  private void HandleAuthFailReason(
    INetworkPlayer player,
    NetworkAuthenticatorNuclearOption.AuthFailReason message)
  {
    ColorLog<NetworkAuthenticatorNuclearOption>.Info("Received fail reason, reason=" + message.Reason);
    GameManager.SetDisconnectReason(new DisconnectInfo(message.Reason));
  }

  private void HandleBuildHashMismatch(
    INetworkPlayer player,
    NetworkAuthenticatorNuclearOption.BuildHashMismatch message)
  {
    if (!NetworkAuthenticatorNuclearOption.BuildHashDifferent(message.BuildHash))
      return;
    ColorLog<NetworkAuthenticatorNuclearOption>.Info($"Client ({NetworkAuthenticatorNuclearOption.GetBuildHash():X}) had different build hash than Server ({message.BuildHash:X})");
    FlashErrorMessageSingleton.ShowError("Build Number did not match server!", new float?(20f));
  }

  private void OnStartServer()
  {
    this.serverStopCancel?.Cancel();
    this.serverStopCancel = new CancellationTokenSource();
    this.server.MessageHandler.RegisterHandler<NetworkAuthenticatorNuclearOption.PasswordResponse>(new MessageDelegateWithPlayer<NetworkAuthenticatorNuclearOption.PasswordResponse>(this.HandlePasswordResponse), true);
    if ((UnityEngine.Object) this.server.SocketFactory == (UnityEngine.Object) this.SteamTransport)
      this.serverAuthCallback = !SteamManager.ServerInitialized ? Callback<ValidateAuthTicketResponse_t>.Create(new Callback<ValidateAuthTicketResponse_t>.DispatchDelegate(this.ValidateAuthTicketResponse)) : Callback<ValidateAuthTicketResponse_t>.CreateGameServer(new Callback<ValidateAuthTicketResponse_t>.DispatchDelegate(this.ValidateAuthTicketResponse));
    this.CheckSteamSessionDisconnectsLoop(this.serverStopCancel.Token).Forget();
  }

  private void OnStopServer()
  {
    this.serverStopCancel?.Cancel();
    this.serverStopCancel = (CancellationTokenSource) null;
    this.kickedPlayers.Clear();
    this.challenges.Clear();
    this.steamAuthTokenLookup.Clear();
    this.lobbyPassword = (LobbyPassword) null;
    this.DisposeServerCallbacks();
  }

  private void OnServerDisconnected(INetworkPlayer player)
  {
    if (!(player.ConnectionHandle is SteamConnection connectionHandle))
      return;
    CSteamID steamId = connectionHandle.SteamID;
    if (player.IsAuthenticated)
    {
      this.timeoutManager.OnDisconnect(steamId);
      ColorLog<NetworkAuthenticatorNuclearOption>.Info($"Ending auth session for {steamId}");
      if (SteamManager.ServerInitialized)
        SteamGameServer.EndAuthSession(steamId);
      else
        SteamUser.EndAuthSession(steamId);
    }
    else
    {
      UniTaskCompletionSource<NetworkAuthenticatorNuclearOption.AuthTicketResult> completionSource;
      if (!this.steamAuthTokenLookup.Remove(steamId, ref completionSource))
        return;
      completionSource.TrySetResult(NetworkAuthenticatorNuclearOption.AuthTicketResult.Fail("player disconnected before auth finished"));
    }
  }

  private async UniTaskVoid OnClientConnected(INetworkPlayer player)
  {
    NetworkAuthenticatorNuclearOption authenticatorNuclearOption = this;
    if (!string.IsNullOrEmpty(authenticatorNuclearOption.clientPassword))
      authenticatorNuclearOption.client.MessageHandler.RegisterHandler<NetworkAuthenticatorNuclearOption.PasswordChallenge>(new MessageDelegateWithPlayer<NetworkAuthenticatorNuclearOption.PasswordChallenge>(authenticatorNuclearOption.HandlePasswordChallenge), true);
    authenticatorNuclearOption.client.MessageHandler.RegisterHandler<NetworkAuthenticatorNuclearOption.AuthFailReason>(new MessageDelegateWithPlayer<NetworkAuthenticatorNuclearOption.AuthFailReason>(authenticatorNuclearOption.HandleAuthFailReason), true);
    authenticatorNuclearOption.client.MessageHandler.RegisterHandler<NetworkAuthenticatorNuclearOption.BuildHashMismatch>(new MessageDelegateWithPlayer<NetworkAuthenticatorNuclearOption.BuildHashMismatch>(authenticatorNuclearOption.HandleBuildHashMismatch), true);
    authenticatorNuclearOption.clientAuthCallback = Callback<GetAuthSessionTicketResponse_t>.Create(new Callback<GetAuthSessionTicketResponse_t>.DispatchDelegate(authenticatorNuclearOption.GetAuthSessionTicketResponse));
    PlayerType joinAs = !DedicatedServerManager.IsRunning || !player.IsHost ? PlayerType.Normal : PlayerType.DedicatedServer;
    byte[] buffer = authenticatorNuclearOption.bufferPool.Take();
    try
    {
      ArraySegment<byte> arraySegment = new ArraySegment<byte>();
      if (player.ConnectionHandle is SteamConnection connectionHandle)
      {
        JoinLobbyOverlay.Open(JoinProgress.Joining("Getting AuthTicket"));
        authenticatorNuclearOption.clientDisconnectCancel?.Cancel();
        authenticatorNuclearOption.clientDisconnectCancel = new CancellationTokenSource();
        ArraySegment<byte>? steamAuthTicket = await authenticatorNuclearOption.GetSteamAuthTicket(connectionHandle, buffer, authenticatorNuclearOption.clientDisconnectCancel.Token);
        if (steamAuthTicket.HasValue)
        {
          arraySegment = steamAuthTicket.Value;
        }
        else
        {
          ColorLog<NetworkAuthenticatorNuclearOption>.InfoWarn("GetSteamAuthTicket cancelled, aborting OnClientConnected task");
          player.Disconnect();
          buffer = (byte[]) null;
          return;
        }
      }
      if (!player.IsHost)
        JoinLobbyOverlay.Open(JoinProgress.Joining("Authenticating with Server"));
      NetworkAuthenticatorNuclearOption.AuthMessage message = new NetworkAuthenticatorNuclearOption.AuthMessage()
      {
        BuildHash = NetworkAuthenticatorNuclearOption.GetBuildHash(),
        JoinAs = joinAs,
        SteamAuthToken = arraySegment
      };
      NetworkAuthenticatorNuclearOption.AuthMessage.LogMessage(authenticatorNuclearOption.client.Player, message);
      authenticatorNuclearOption.SendAuthentication(authenticatorNuclearOption.client, message);
      buffer = (byte[]) null;
    }
    finally
    {
      authenticatorNuclearOption.bufferPool.Put(buffer);
    }
  }

  private void GetAuthSessionTicketResponse(GetAuthSessionTicketResponse_t param)
  {
    ColorLog<NetworkAuthenticatorNuclearOption>.Info($"GetAuthSessionTicketResponse handle:{param.m_hAuthTicket}, result:{param.m_eResult}");
    if (param.m_hAuthTicket == this.clientAuthTicket)
    {
      if (this.getTicketResult != null)
        this.getTicketResult.TrySetResult(param);
      else
        ColorLog<NetworkAuthenticatorNuclearOption>.InfoWarn("No pending Ticket Result");
    }
    else
      ColorLog<NetworkAuthenticatorNuclearOption>.InfoWarn($"GetAuthSessionTicketResponse did not match current ticket:{this.clientAuthTicket}");
  }

  private async UniTask<ArraySegment<byte>?> GetSteamAuthTicket(
    SteamConnection handle,
    byte[] buffer,
    CancellationToken cancel,
    int attempt = 0)
  {
    if (this.getTicketResult != null)
    {
      this.getTicketResult.TrySetCanceled(new CancellationToken());
      this.getTicketResult = (UniTaskCompletionSource<GetAuthSessionTicketResponse_t>) null;
    }
    if (this.CancelAuthTicket())
    {
      await UniTask.Delay(500);
      if (cancel.IsCancellationRequested)
        return new ArraySegment<byte>?();
      if (this.getTicketResult != null)
      {
        Debug.LogError((object) "New getTicketResult assigned while waiting delay after CancelAuthTicket");
        return new ArraySegment<byte>?();
      }
    }
    this.getTicketResult = new UniTaskCompletionSource<GetAuthSessionTicketResponse_t>();
    SteamNetworkingIdentity networkingIdentity = handle.SteamNetworkingIdentity;
    uint pcbTicket;
    HAuthTicket ticket = SteamUser.GetAuthSessionTicket(buffer, buffer.Length, out pcbTicket, ref networkingIdentity);
    this.clientAuthTicket = ticket;
    ArraySegment<byte> segment = new ArraySegment<byte>(buffer, 0, (int) pcbTicket);
    string buf;
    networkingIdentity.ToString(out buf);
    ColorLog<NetworkAuthenticatorNuclearOption>.Info($"GetAuthSessionTicket handle:{this.clientAuthTicket}, connect-to:{buf}, size:{pcbTicket}, hash:{NetworkAuthenticatorNuclearOption.HashArray(segment):X}");
    (bool, (bool, GetAuthSessionTicketResponse_t)) valueTuple1 = await this.getTicketResult.Task.AttachExternalCancellation<GetAuthSessionTicketResponse_t>(cancel).TimeoutWithoutException<GetAuthSessionTicketResponse_t>(TimeSpan.FromSeconds(5.0), DelayType.UnscaledDeltaTime).SuppressCancellationThrow();
    (bool, GetAuthSessionTicketResponse_t) valueTuple2 = valueTuple1.Item2;
    int num = valueTuple1.Item1 ? 1 : 0;
    bool flag = valueTuple2.Item1;
    GetAuthSessionTicketResponse_t sessionTicketResponseT = valueTuple2.Item2;
    this.getTicketResult = (UniTaskCompletionSource<GetAuthSessionTicketResponse_t>) null;
    if (num != 0)
      return new ArraySegment<byte>?();
    if (flag)
    {
      ColorLog<NetworkAuthenticatorNuclearOption>.InfoWarn("Reached timeout waiting for GetAuthSessionTicketResponse");
      return new ArraySegment<byte>?(segment);
    }
    if (sessionTicketResponseT.m_hAuthTicket != ticket)
    {
      Debug.LogError((object) "Ticket number returned from task did not match, cancelling");
      return new ArraySegment<byte>?();
    }
    if (sessionTicketResponseT.m_eResult == EResult.k_EResultOK)
      return new ArraySegment<byte>?(segment);
    if (attempt < 3)
    {
      ColorLog<NetworkAuthenticatorNuclearOption>.InfoWarn("Reached timeout waiting for GetAuthSessionTicketResponse");
      await UniTask.Delay(500 * (attempt + 1));
      return cancel.IsCancellationRequested ? new ArraySegment<byte>?() : await this.GetSteamAuthTicket(handle, buffer, cancel, attempt + 1);
    }
    ColorLog<NetworkAuthenticatorNuclearOption>.LogError("Max retries reached, aborting auth");
    return new ArraySegment<byte>?();
  }

  private void OnApplicationQuit() => this.CancelAuthTicket();

  private void OnClientDisconnected(ClientStoppedReason arg0)
  {
    ColorLog<NetworkAuthenticatorNuclearOption>.Info($"Handling client disconnect, CancelSource:{this.clientDisconnectCancel != null}, HasTicket:{this.clientAuthTicket != HAuthTicket.Invalid}");
    this.clientDisconnectCancel?.Cancel();
    this.clientDisconnectCancel = (CancellationTokenSource) null;
    this.CancelAuthTicket();
    this.DisposeClientCallbacks();
  }

  private bool CancelAuthTicket()
  {
    if (!(this.clientAuthTicket != HAuthTicket.Invalid))
      return false;
    ColorLog<NetworkAuthenticatorNuclearOption>.Info($"CancelAuthTicket handle:{this.clientAuthTicket}");
    SteamUser.CancelAuthTicket(this.clientAuthTicket);
    this.clientAuthTicket = HAuthTicket.Invalid;
    return true;
  }

  public void OnKick(INetworkPlayer conn)
  {
    ColorLog<NetworkAuthenticatorNuclearOption>.Info($"Kicking {conn}");
    this.kickedPlayers.Add(this.GetSteamId(conn));
  }

  public void AddKicked(CSteamID id)
  {
    if (this.kickedPlayers.Add(id))
      ColorLog<NetworkAuthenticatorNuclearOption>.Info($"Adding {id} to kick list");
    else
      ColorLog<NetworkAuthenticatorNuclearOption>.Info($"{id} already in kick list");
  }

  public void RemoveKicked(CSteamID id)
  {
    if (this.kickedPlayers.Remove(id))
      ColorLog<NetworkAuthenticatorNuclearOption>.Info($"Removed {id} from kick list");
    else
      ColorLog<NetworkAuthenticatorNuclearOption>.Info($"{id} not in kick list");
  }

  public void ClearKickList()
  {
    ColorLog<NetworkAuthenticatorNuclearOption>.Info("Clearing kicked list");
    this.kickedPlayers.Clear();
  }

  protected override async UniTask<AuthenticationResult> AuthenticateAsync(
    INetworkPlayer player,
    NetworkAuthenticatorNuclearOption.AuthMessage message,
    CancellationToken cancellationToken)
  {
    AuthenticationResult authenticationResult = await this.AuthenticateAsyncInternal(player, message, cancellationToken);
    if (authenticationResult.Success)
      ColorLog<NetworkAuthenticatorNuclearOption>.Info($"Accepting {player}, reason:{authenticationResult.Reason}");
    return authenticationResult;
  }

  protected async UniTask<AuthenticationResult> AuthenticateAsyncInternal(
    INetworkPlayer player,
    NetworkAuthenticatorNuclearOption.AuthMessage message,
    CancellationToken cancellationToken)
  {
    NetworkAuthenticatorNuclearOption.AuthMessage.LogMessage(player, message);
    if (NetworkAuthenticatorNuclearOption.BuildHashDifferent(message.BuildHash))
    {
      ColorLog<NetworkAuthenticatorNuclearOption>.Info($"Client ({message.BuildHash:X}) had different build hash than Server ({NetworkAuthenticatorNuclearOption.GetBuildHash():X})");
      player.Send<NetworkAuthenticatorNuclearOption.BuildHashMismatch>(new NetworkAuthenticatorNuclearOption.BuildHashMismatch()
      {
        BuildHash = NetworkAuthenticatorNuclearOption.GetBuildHash()
      });
    }
    AuthenticationResult? nullable = this.ValidateJoinAs(player, message);
    if (nullable.HasValue)
      return nullable.Value;
    return (UnityEngine.Object) this.server.SocketFactory == (UnityEngine.Object) this.SteamTransport ? await this.SteamAuthenticate(player, message, cancellationToken) : await this.UDPAuthenticate(player, message, cancellationToken);
  }

  private AuthenticationResult? ValidateJoinAs(
    INetworkPlayer player,
    NetworkAuthenticatorNuclearOption.AuthMessage message)
  {
    if (message.JoinAs == PlayerType.Unknown)
      return new AuthenticationResult?(AuthenticationResult.CreateFail("Auth message did not give a player type", (INetworkAuthenticator) this));
    return message.JoinAs == PlayerType.DedicatedServer ? (player.IsHost ? new AuthenticationResult?(AuthenticationResult.CreateSuccess("Dedicated Server", (INetworkAuthenticator) this, (UnityEngine.Object) this.server.SocketFactory == (UnityEngine.Object) this.SteamTransport ? (object) NetworkAuthenticatorNuclearOption.AuthData.FromSteamHost(message.JoinAs, this.GetSteamId(player), (SavedPlayerData) null) : (object) NetworkAuthenticatorNuclearOption.AuthData.FromUdp(message.JoinAs))) : new AuthenticationResult?(AuthenticationResult.CreateFail("Auth message did not give a player type", (INetworkAuthenticator) this))) : (message.JoinAs == PlayerType.Spectator ? new AuthenticationResult?(AuthenticationResult.CreateFail("Not Implemented", (INetworkAuthenticator) this)) : new AuthenticationResult?());
  }

  private async UniTask<AuthenticationResult> SteamAuthenticate(
    INetworkPlayer player,
    NetworkAuthenticatorNuclearOption.AuthMessage message,
    CancellationToken cancellationToken)
  {
    NetworkAuthenticatorNuclearOption authenticator = this;
    CSteamID id = authenticator.GetSteamId(player);
    if (player.IsHost)
    {
      ColorLog<NetworkAuthenticatorNuclearOption>.Info($"Accepting host {player}");
      return AuthenticationResult.CreateSuccess("Host player", (INetworkAuthenticator) authenticator, (object) NetworkAuthenticatorNuclearOption.AuthData.FromSteamHost(message.JoinAs, id, (SavedPlayerData) null));
    }
    AuthenticationResult result1;
    if (authenticator.IsKickedOrBanned(player, id, out result1))
      return result1;
    if (authenticator.timeoutManager.HasTimeout(id))
    {
      ColorLog<NetworkAuthenticatorNuclearOption>.Info($"[{player}, {id}] on timed out, Rejecting connection");
      return AuthenticationResult.CreateFail("Player in timeout list", (INetworkAuthenticator) authenticator);
    }
    if (message.SteamAuthToken.Array == null || message.SteamAuthToken.Count == 0)
      return AuthenticationResult.CreateFail("No Auth token given", (INetworkAuthenticator) authenticator);
    byte[] buffer = authenticator.bufferPool.Take();
    try
    {
      Buffer.BlockCopy((Array) message.SteamAuthToken.Array, message.SteamAuthToken.Offset, (Array) buffer, 0, message.SteamAuthToken.Count);
      ArraySegment<byte> authToken = new ArraySegment<byte>(buffer, 0, message.SteamAuthToken.Count);
      if (authenticator.lobbyPassword != null)
      {
        (bool, string) valueTuple = await authenticator.RunPasswordCheck(player, cancellationToken);
        int num = valueTuple.Item1 ? 1 : 0;
        string failReason = valueTuple.Item2;
        if (num == 0)
        {
          ColorLog<NetworkAuthenticatorNuclearOption>.Info($"[{player}, {id}] password wrong, adding 2 second timeout");
          authenticator.timeoutManager.AddCustomTimeout(id, 0, 2f);
          if (!cancellationToken.IsCancellationRequested)
            await authenticator.SendFailReason(player, "Password incorrect");
          return AuthenticationResult.CreateFail(failReason, (INetworkAuthenticator) authenticator);
        }
        failReason = (string) null;
      }
      NetworkAuthenticatorNuclearOption.AuthTicketResult authTicketResult = await authenticator.CheckSteamAuthToken(id, authToken, cancellationToken);
      if (!authTicketResult.success)
      {
        if (!cancellationToken.IsCancellationRequested)
          await authenticator.SendFailReason(player, "Steam Authentication failed");
        return AuthenticationResult.CreateFail(authTicketResult.failReason, (INetworkAuthenticator) authenticator);
      }
      AuthenticationResult result2;
      if (authTicketResult.m_SteamID != authTicketResult.m_OwnerSteamID && authenticator.IsKickedOrBanned(player, authTicketResult.m_OwnerSteamID, out result2))
      {
        ColorLog<NetworkAuthenticatorNuclearOption>.Info("Kicking because m_OwnerSteamID is on kick or ban list");
        return result2;
      }
      SavedPlayerData saveData = authenticator.CheckOldSaveData(player, id);
      authenticator.steamPlayerLookup[id] = player;
      return AuthenticationResult.CreateSuccess("Auth successful", (INetworkAuthenticator) authenticator, (object) NetworkAuthenticatorNuclearOption.AuthData.FromSteam(message.JoinAs, id, authTicketResult.m_OwnerSteamID, saveData));
    }
    finally
    {
      authenticator.bufferPool.Put(buffer);
    }
  }

  private bool IsKickedOrBanned(CSteamID id)
  {
    return this.kickedPlayers.Contains(id) || this.BanList.Contains(id);
  }

  private bool IsKickedOrBanned(
    INetworkPlayer player,
    CSteamID id,
    out AuthenticationResult result)
  {
    if (this.kickedPlayers.Contains(id))
    {
      ColorLog<NetworkAuthenticatorNuclearOption>.Info($"[{player}, {id}] on kick list, Rejecting connection");
      result = AuthenticationResult.CreateFail("Player in kick list", (INetworkAuthenticator) this);
      return true;
    }
    if (this.BanList.Contains(id))
    {
      ColorLog<NetworkAuthenticatorNuclearOption>.Info($"[{player}, {id}] on ban list, Rejecting connection");
      result = AuthenticationResult.CreateFail("Player in ban list", (INetworkAuthenticator) this);
      return true;
    }
    result = new AuthenticationResult();
    return false;
  }

  private async UniTask<NetworkAuthenticatorNuclearOption.AuthTicketResult> CheckSteamAuthToken(
    CSteamID id,
    ArraySegment<byte> steamAuthToken,
    CancellationToken cancellationToken)
  {
    ColorLog<NetworkAuthenticatorNuclearOption>.Info($"Begin Auth session for {id}");
    EBeginAuthSessionResult authSessionResult = SteamManager.ServerInitialized ? SteamGameServer.BeginAuthSession(steamAuthToken.Array, steamAuthToken.Count, id) : SteamUser.BeginAuthSession(steamAuthToken.Array, steamAuthToken.Count, id);
    ColorLog<NetworkAuthenticatorNuclearOption>.Info($"BeginAuthSession result={authSessionResult}");
    if (authSessionResult != EBeginAuthSessionResult.k_EBeginAuthSessionResultOK)
      return NetworkAuthenticatorNuclearOption.AuthTicketResult.Fail($"Failed to begin auth session: {authSessionResult}");
    UniTaskCompletionSource<NetworkAuthenticatorNuclearOption.AuthTicketResult> completionSource = new UniTaskCompletionSource<NetworkAuthenticatorNuclearOption.AuthTicketResult>();
    this.steamAuthTokenLookup.Add(id, completionSource);
    (bool flag, NetworkAuthenticatorNuclearOption.AuthTicketResult authTicketResult) = await completionSource.Task.AttachExternalCancellation<NetworkAuthenticatorNuclearOption.AuthTicketResult>(cancellationToken).SuppressCancellationThrow();
    return !flag ? authTicketResult : NetworkAuthenticatorNuclearOption.AuthTicketResult.Fail("Cancelled");
  }

  private void ValidateAuthTicketResponse(ValidateAuthTicketResponse_t param)
  {
    UniTaskCompletionSource<NetworkAuthenticatorNuclearOption.AuthTicketResult> completionSource;
    bool flag = this.steamAuthTokenLookup.Remove(param.m_SteamID, ref completionSource);
    ColorLog<NetworkAuthenticatorNuclearOption>.Info($"ValidateAuthTicketResponse_t hasPending:{flag}, id:{param.m_SteamID}, ownerId:{param.m_OwnerSteamID}, {param.m_eAuthSessionResponse}");
    if (flag)
    {
      completionSource.TrySetResult(param.m_eAuthSessionResponse == EAuthSessionResponse.k_EAuthSessionResponseOK ? NetworkAuthenticatorNuclearOption.AuthTicketResult.Success(param.m_SteamID, param.m_OwnerSteamID) : NetworkAuthenticatorNuclearOption.AuthTicketResult.Fail($"Steam Auth failed: {param.m_eAuthSessionResponse}"));
    }
    else
    {
      INetworkPlayer networkPlayer;
      if (this.steamPlayerLookup.TryGetValue(param.m_SteamID, out networkPlayer))
      {
        switch (param.m_eAuthSessionResponse)
        {
          case EAuthSessionResponse.k_EAuthSessionResponseOK:
            networkPlayer.GetAuthData().SteamSessionOk = true;
            break;
          case EAuthSessionResponse.k_EAuthSessionResponseUserNotConnectedToSteam:
            ColorLog<NetworkAuthenticatorNuclearOption>.Info($"Player {param.m_SteamID} disconnected from steam waiting 60 seconds before disconnecting them");
            NetworkAuthenticatorNuclearOption.AuthData authData = networkPlayer.GetAuthData();
            authData.SteamSessionOk = false;
            authData.SteamSessionDisconnectTime = UnityEngine.Time.unscaledTimeAsDouble;
            break;
          default:
            ColorLog<NetworkAuthenticatorNuclearOption>.Info("Disconnecting player because auth session failed");
            if (!networkPlayer.IsConnected)
              break;
            networkPlayer.Disconnect();
            break;
        }
      }
      else
        ColorLog<NetworkAuthenticatorNuclearOption>.InfoWarn($"Could not find player {param.m_SteamID} in lookup");
    }
  }

  private async UniTaskVoid CheckSteamSessionDisconnectsLoop(CancellationToken cancellation)
  {
    while (!cancellation.IsCancellationRequested)
    {
      this.CheckSteamSessionDisconnects();
      await UniTask.Delay(1000, true);
    }
  }

  private void CheckSteamSessionDisconnects()
  {
    double unscaledTimeAsDouble = UnityEngine.Time.unscaledTimeAsDouble;
    foreach (INetworkPlayer networkPlayer in this.steamPlayerLookup.Values)
    {
      if (networkPlayer.IsConnected)
      {
        NetworkAuthenticatorNuclearOption.AuthData authData = networkPlayer.GetAuthData();
        if (!authData.SteamSessionOk)
        {
          double num = authData.SteamSessionDisconnectTime + 60.0;
          if (unscaledTimeAsDouble > num)
          {
            ColorLog<NetworkAuthenticatorNuclearOption>.Info("Disconnecting player because they were disconnected from steam for 60 seconds");
            networkPlayer.Disconnect();
          }
        }
      }
    }
  }

  private async UniTask<AuthenticationResult> UDPAuthenticate(
    INetworkPlayer player,
    NetworkAuthenticatorNuclearOption.AuthMessage message,
    CancellationToken cancellationToken)
  {
    NetworkAuthenticatorNuclearOption authenticator = this;
    if (authenticator.lobbyPassword != null)
    {
      (bool flag, string reason) = await authenticator.RunPasswordCheck(player, cancellationToken);
      if (!flag)
        return AuthenticationResult.CreateFail(reason, (INetworkAuthenticator) authenticator);
    }
    return AuthenticationResult.CreateSuccess("UDP auth success", (INetworkAuthenticator) authenticator, (object) NetworkAuthenticatorNuclearOption.AuthData.FromUdp(message.JoinAs));
  }

  private SavedPlayerData CheckOldSaveData(INetworkPlayer player, CSteamID id)
  {
    SavedPlayerData savedPlayerData = (SavedPlayerData) null;
    INetworkPlayer networkPlayer;
    if (this.steamPlayerLookup.TryGetValue(id, out networkPlayer))
    {
      savedPlayerData = networkPlayer.Authentication.GetData<NetworkAuthenticatorNuclearOption.AuthData>().SaveData;
      ColorLog<NetworkAuthenticatorNuclearOption>.Info($"Found old save data for {player} -> (faction={savedPlayerData.Faction}, rank={savedPlayerData.Rank})");
      savedPlayerData.Rejoined = true;
      Player player1;
      if (networkPlayer.TryGetPlayer<Player>(out player1))
        savedPlayerData.Save(player1);
      if (networkPlayer.Connection.State == ConnectionState.Connected)
        Debug.LogWarning((object) $"New player connected with steam id {id}, disconnecting old player");
      networkPlayer.Disconnect();
    }
    else
      ColorLog<NetworkAuthenticatorNuclearOption>.Info($"New player {player}");
    return savedPlayerData;
  }

  private CSteamID GetSteamId(INetworkPlayer player)
  {
    return player.IsHost ? (SteamManager.ServerInitialized ? GameServer.GetSteamID() : SteamUser.GetSteamID()) : (player.ConnectionHandle is SteamConnection connectionHandle ? connectionHandle.SteamID : new CSteamID());
  }

  public void ClearRejoinSaveData()
  {
    foreach (KeyValuePair<CSteamID, INetworkPlayer> keyValuePair in this.steamPlayerLookup.ToArray<KeyValuePair<CSteamID, INetworkPlayer>>())
    {
      INetworkPlayer networkPlayer = keyValuePair.Value;
      if (networkPlayer.IsConnected)
        networkPlayer.Authentication.GetData<NetworkAuthenticatorNuclearOption.AuthData>().SaveData.Clear();
      else
        this.steamPlayerLookup.Remove(keyValuePair.Key);
    }
  }

  public void SetServerPassword(string lobbyPassword)
  {
    ColorLog<NetworkAuthenticatorNuclearOption>.Info("Enabling password on server");
    this.lobbyPassword = new LobbyPassword(lobbyPassword);
  }

  public void ClearServerPassword() => this.lobbyPassword = (LobbyPassword) null;

  public void SetClientPassword(string lobbyPassword) => this.clientPassword = lobbyPassword;

  public void ClearClientPassword() => this.clientPassword = (string) null;

  private async UniTask<(bool success, string failReason)> RunPasswordCheck(
    INetworkPlayer player,
    CancellationToken cancellationToken)
  {
    if (this.challenges.ContainsKey(player))
      return (false, "Client was already pending password challenge");
    NetworkAuthenticatorNuclearOption.Challenge challenge = new NetworkAuthenticatorNuclearOption.Challenge(this.lobbyPassword.GenerateChallenge());
    this.challenges.Add(player, challenge);
    NetworkAuthenticatorNuclearOption.PasswordChallenge message = new NetworkAuthenticatorNuclearOption.PasswordChallenge()
    {
      Nonce = challenge.PasswordChallenge.GetNonceBytes()
    };
    ColorLog<NetworkAuthenticatorNuclearOption>.Info($"Sending PasswordChallenge to {player} nonce length: {message.Nonce.Count}");
    player.Send<NetworkAuthenticatorNuclearOption.PasswordChallenge>(message);
    return await challenge.WaitForResult(cancellationToken);
  }

  private void HandlePasswordChallenge(
    INetworkPlayer player,
    NetworkAuthenticatorNuclearOption.PasswordChallenge challenge)
  {
    ColorLog<NetworkAuthenticatorNuclearOption>.Info($"Receiving PasswordChallenge nonce length: {challenge.Nonce.Count}");
    if (string.IsNullOrEmpty(this.clientPassword))
    {
      Debug.LogError((object) "Server gave password challenge but client did has no password set");
      player.Disconnect();
    }
    else
    {
      byte[] response = LobbyPassword.GenerateResponse(this.clientPassword, challenge.Nonce);
      NetworkAuthenticatorNuclearOption.PasswordResponse message = new NetworkAuthenticatorNuclearOption.PasswordResponse()
      {
        Response = ArraySegment<byte>.op_Implicit(response)
      };
      ColorLog<NetworkAuthenticatorNuclearOption>.Info($"Sending PasswordResponse response length: {message.Response.Count}");
      player.Send<NetworkAuthenticatorNuclearOption.PasswordResponse>(message);
    }
  }

  private void HandlePasswordResponse(
    INetworkPlayer player,
    NetworkAuthenticatorNuclearOption.PasswordResponse response)
  {
    if (this.lobbyPassword == null)
    {
      player.Disconnect();
    }
    else
    {
      ColorLog<NetworkAuthenticatorNuclearOption>.Info($"Receiving PasswordResponse from {player} response length: {response.Response.Count}");
      NetworkAuthenticatorNuclearOption.Challenge challenge;
      if (!this.challenges.TryGetValue(player, out challenge))
      {
        ColorLog<NetworkAuthenticatorNuclearOption>.InfoWarn("No pending challenge for player");
        player.Disconnect();
      }
      else
      {
        this.challenges.Remove(player);
        challenge.Check(response);
      }
    }
  }

  private static uint GetBuildHash()
  {
    if (!NetworkAuthenticatorNuclearOption.buildHash.HasValue)
    {
      try
      {
        if (File.Exists("build-hash.txt"))
        {
          string str = File.ReadAllText("build-hash.txt").Trim();
          if (str.Length > 8)
            str = str.Substring(0, 8);
          NetworkAuthenticatorNuclearOption.buildHash = new uint?(Convert.ToUInt32(str, 16 /*0x10*/));
          ColorLog<NetworkAuthenticatorNuclearOption>.Info($"Loading BuildHash: {NetworkAuthenticatorNuclearOption.buildHash}");
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine("Error reading build hash: " + ex.Message);
      }
      if (!NetworkAuthenticatorNuclearOption.buildHash.HasValue)
      {
        ColorLog<NetworkAuthenticatorNuclearOption>.InfoWarn("Failed to get BuildHash");
        NetworkAuthenticatorNuclearOption.buildHash = new uint?(0U);
      }
    }
    return NetworkAuthenticatorNuclearOption.buildHash.Value;
  }

  private static bool BuildHashDifferent(uint other)
  {
    uint buildHash = NetworkAuthenticatorNuclearOption.GetBuildHash();
    return buildHash != 0U && other != 0U && (int) buildHash != (int) other;
  }

  private static int HashArray(ArraySegment<byte> segment)
  {
    int num = 23;
    byte[] array = segment.Array;
    int offset = segment.Offset;
    int count = segment.Count;
    for (int index = 0; index < count; ++index)
      num = num * 31 /*0x1F*/ + (int) array[offset + index];
    return num;
  }

  private struct AuthTicketResult
  {
    public bool success;
    public string failReason;
    public CSteamID m_SteamID;
    public CSteamID m_OwnerSteamID;

    public static NetworkAuthenticatorNuclearOption.AuthTicketResult Success(
      CSteamID id,
      CSteamID owner)
    {
      return new NetworkAuthenticatorNuclearOption.AuthTicketResult()
      {
        success = true,
        m_SteamID = id,
        m_OwnerSteamID = owner
      };
    }

    public static NetworkAuthenticatorNuclearOption.AuthTicketResult Fail(string reason)
    {
      return new NetworkAuthenticatorNuclearOption.AuthTicketResult()
      {
        success = false,
        failReason = reason
      };
    }
  }

  [NetworkMessage]
  public struct AuthMessage
  {
    public uint BuildHash;
    public PlayerType JoinAs;
    public ArraySegment<byte> SteamAuthToken;

    public static void LogMessage(
      INetworkPlayer player,
      NetworkAuthenticatorNuclearOption.AuthMessage message)
    {
      int num;
      string str1;
      if (message.SteamAuthToken.Array == null)
      {
        str1 = "NULL";
      }
      else
      {
        num = message.SteamAuthToken.Count;
        str1 = num.ToString();
      }
      string str2 = str1;
      string str3;
      if (message.SteamAuthToken.Array == null)
      {
        str3 = "NULL";
      }
      else
      {
        num = NetworkAuthenticatorNuclearOption.HashArray(message.SteamAuthToken);
        str3 = num.ToString("X");
      }
      string str4 = str3;
      ColorLog<NetworkAuthenticatorNuclearOption>.Info($"{player} - JoinAs:{message.JoinAs}, BuildHash:{message.BuildHash}, SteamAuthToken Length:{str2} Hash:{str4}");
    }
  }

  [NetworkMessage]
  public struct PasswordChallenge
  {
    public ArraySegment<byte> Nonce;
  }

  [NetworkMessage]
  public struct PasswordResponse
  {
    public ArraySegment<byte> Response;
  }

  [NetworkMessage]
  public struct AuthFailReason
  {
    public string Reason;
  }

  [NetworkMessage]
  public struct BuildHashMismatch
  {
    public uint BuildHash;
  }

  public class Challenge
  {
    public readonly LobbyPassword.PasswordChallenge PasswordChallenge;
    private readonly UniTaskCompletionSource<NetworkAuthenticatorNuclearOption.Challenge.Result> result = new UniTaskCompletionSource<NetworkAuthenticatorNuclearOption.Challenge.Result>();

    public Challenge(LobbyPassword.PasswordChallenge passwordChallenge)
    {
      this.PasswordChallenge = passwordChallenge;
    }

    public async UniTask<(bool success, string failReason)> WaitForResult(
      CancellationToken cancellation)
    {
      (bool flag, NetworkAuthenticatorNuclearOption.Challenge.Result result) = await this.result.Task.AttachExternalCancellation<NetworkAuthenticatorNuclearOption.Challenge.Result>(cancellation).SuppressCancellationThrow();
      if (flag)
        return (false, "Async cancelled while password check was running");
      (bool, string) valueTuple;
      switch (result)
      {
        case NetworkAuthenticatorNuclearOption.Challenge.Result.Success:
          valueTuple = (true, (string) null);
          break;
        case NetworkAuthenticatorNuclearOption.Challenge.Result.Fail:
          valueTuple = (false, "Failed Password check");
          break;
        default:
          valueTuple = (false, "Internal error from challenge");
          break;
      }
      return valueTuple;
    }

    public void Check(
      NetworkAuthenticatorNuclearOption.PasswordResponse response)
    {
      this.result.TrySetResult(this.PasswordChallenge.VerifyResponse(response.Response) ? NetworkAuthenticatorNuclearOption.Challenge.Result.Success : NetworkAuthenticatorNuclearOption.Challenge.Result.Fail);
    }

    public enum Result
    {
      Pending,
      Success,
      Fail,
    }
  }

  public class AuthData
  {
    public readonly bool UsingSteamTransport;
    public readonly CSteamID SteamID;
    public readonly CSteamID OwnerID;
    public readonly SavedPlayerData SaveData;
    public readonly PlayerType JoinAs;
    public bool SteamSessionOk;
    public double SteamSessionDisconnectTime;
    public const float STEAM_DISCONNECT_GRACE_TIME = 60f;

    private AuthData(
      PlayerType joinAs,
      bool usingSteamTransport,
      CSteamID steamID,
      CSteamID ownerID,
      SavedPlayerData saveData = null)
    {
      this.JoinAs = joinAs;
      this.UsingSteamTransport = usingSteamTransport;
      this.SteamID = steamID;
      this.OwnerID = ownerID;
      this.SaveData = saveData ?? new SavedPlayerData();
      this.SteamSessionOk = true;
      this.SteamSessionDisconnectTime = 0.0;
    }

    public static NetworkAuthenticatorNuclearOption.AuthData FromSteam(
      PlayerType joinAs,
      CSteamID steamID,
      CSteamID ownerID,
      SavedPlayerData saveData)
    {
      return new NetworkAuthenticatorNuclearOption.AuthData(joinAs, true, steamID, ownerID, saveData);
    }

    public static NetworkAuthenticatorNuclearOption.AuthData FromSteamHost(
      PlayerType joinAs,
      CSteamID steamID,
      SavedPlayerData saveData)
    {
      return new NetworkAuthenticatorNuclearOption.AuthData(joinAs, true, steamID, steamID, saveData);
    }

    public static NetworkAuthenticatorNuclearOption.AuthData FromUdp(
      PlayerType joinAs,
      SavedPlayerData saveData = null)
    {
      return new NetworkAuthenticatorNuclearOption.AuthData(joinAs, false, new CSteamID(), new CSteamID(), saveData);
    }
  }
}

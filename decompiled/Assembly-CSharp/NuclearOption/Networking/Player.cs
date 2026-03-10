// Decompiled with JetBrains decompiler
// Type: NuclearOption.Networking.Player
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Cysharp.Threading.Tasks;
using Mirage;
using Mirage.Collections;
using Mirage.RemoteCalls;
using Mirage.Serialization;
using System;
using System.Threading;
using UnityEngine;

#nullable disable
namespace NuclearOption.Networking;

public class Player : BasePlayer
{
  [SerializeField]
  private float[] rankThresholds = new float[6]
  {
    0.0f,
    5f,
    15f,
    30f,
    60f,
    120f
  };
  private float scoreOffset;
  public readonly SyncList<OwnedAirframe> OwnedAirframes = new SyncList<OwnedAirframe>();
  [SyncVar]
  public OwnedAirframe? AirframeInUse;
  public float lastReserveGranted = -1000f;
  private string playerName_CensoredCache;
  public int Teamkills;
  private float previousContribution = -60f;
  [NonSerialized]
  private const int SYNC_VAR_COUNT = 7;
  [NonSerialized]
  private const int RPC_COUNT = 18;

  public FactionHQ HQ
  {
    get => this.Network\u003CHQ\u003Ek__BackingField;
    private set => this.Network\u003CHQ\u003Ek__BackingField = value;
  }

  public string PlayerName
  {
    get => this.\u003CPlayerName\u003Ek__BackingField;
    private set => this.Network\u003CPlayerName\u003Ek__BackingField = value;
  }

  public float PlayerScore
  {
    get => this.\u003CPlayerScore\u003Ek__BackingField;
    private set => this.Network\u003CPlayerScore\u003Ek__BackingField = value;
  }

  public int PlayerRank
  {
    get => this.\u003CPlayerRank\u003Ek__BackingField;
    private set => this.Network\u003CPlayerRank\u003Ek__BackingField = value;
  }

  public float Allocation
  {
    get => this.\u003CAllocation\u003Ek__BackingField;
    private set => this.Network\u003CAllocation\u003Ek__BackingField = value;
  }

  public Aircraft Aircraft { get; private set; }

  public PilotDismounted PilotDismounted { get; private set; }

  public PersistentID? UnitID { get; private set; }

  public PlayerRef PlayerRef { get; private set; }

  public event Action<ReserveNotice> onReserveNotice;

  public bool AircraftSpawnPending { get; private set; }

  protected override void Awake()
  {
    base.Awake();
    this.Identity.OnStartClient.AddListener(new Action(this.OnStartClient));
    this.Identity.OnStartServer.AddListener(new Action(this.OnStartServer));
    this.Identity.OnStartLocalPlayer.AddListener(new Action(this.OnStartLocalPlayer));
    this.Identity.OnStopClient.AddListener(new Action(this.OnStopClient));
  }

  public void SetSpawnPending(bool pending) => this.AircraftSpawnPending = pending;

  public void SetAircraft(Aircraft aircraft)
  {
    ColorLog<Player>.Info($"SetAircraft id={aircraft.persistentID} {aircraft.Identity}");
    if (this.IsServer && (UnityEngine.Object) this.Aircraft != (UnityEngine.Object) null)
    {
      this.RemoveAircraftAuthority(this.Aircraft);
      this.Aircraft.StartEjectionSequence();
    }
    this.Aircraft = aircraft;
    this.UnitID = new PersistentID?(aircraft.persistentID);
    PersistentUnit persistentUnit;
    if (UnitRegistry.TryGetPersistentUnit(aircraft.persistentID, out persistentUnit))
      persistentUnit.player = this;
    else
      Debug.LogError((object) $"Aircraft with id={aircraft.persistentID} was not found in UnitRegistry");
  }

  public void RemoveAircraft(Aircraft aircraft) => this.Aircraft = (Aircraft) null;

  [Mirage.Server]
  private void RemoveAircraftAuthority(Aircraft aircraft)
  {
    if (!this.IsServer)
      throw new MethodInvocationException("[Server] function 'RemoveAircraftAuthority' called when server not active");
    ColorLog<Player>.Info($"Removing auth from id={aircraft.persistentID} {aircraft.Identity}");
    INetworkPlayer owner1 = this.Identity.Owner;
    NetworkIdentity identity = aircraft.Identity;
    INetworkPlayer owner2 = identity.Owner;
    if (owner1 == owner2)
      identity.RemoveClientAuthority();
    else
      Debug.LogError((object) $"Aircraft owner was incorrect, expected:{owner1} but was {owner2}");
  }

  public void SetPilotDismounted(PilotDismounted pilotDismounted)
  {
    this.PilotDismounted = pilotDismounted;
  }

  public void RemovePilotDismounted(PilotDismounted pilotDismounted)
  {
    this.PilotDismounted = (PilotDismounted) null;
  }

  public override string ToString() => $"Player({this.PlayerName},netId={this.NetId})";

  public string GetNameOrCensored()
  {
    if (string.IsNullOrEmpty(this.PlayerName))
      return string.Empty;
    if (!PlayerSettings.chatFilter)
      return this.PlayerName;
    if (string.IsNullOrEmpty(this.playerName_CensoredCache))
      this.playerName_CensoredCache = this.PlayerName.ProfanityFilter();
    return this.playerName_CensoredCache;
  }

  public void PurchaseFuel(float fuelMass)
  {
    float num = 1f;
    this.Allocation -= fuelMass * num;
  }

  private void OnStartServer()
  {
    SavedPlayerData saveData = this.GetAuthData().SaveData;
    if (!((UnityEngine.Object) saveData.Faction != (UnityEngine.Object) null))
      return;
    this.SetFaction(saveData.Faction);
  }

  public bool OwnsAirframe(AircraftDefinition aircraftDef, bool includeReserved)
  {
    foreach (OwnedAirframe ownedAirframe in this.OwnedAirframes)
    {
      if ((UnityEngine.Object) ownedAirframe.Definition == (UnityEngine.Object) aircraftDef && (includeReserved || !ownedAirframe.Reserved))
        return true;
    }
    return false;
  }

  public bool PossessesReservedAirframe(AircraftDefinition aircraftDef)
  {
    foreach (OwnedAirframe ownedAirframe in this.OwnedAirframes)
    {
      if ((UnityEngine.Object) ownedAirframe.Definition == (UnityEngine.Object) aircraftDef && ownedAirframe.Reserved)
        return true;
    }
    return false;
  }

  public bool PossessesReservedAirframe()
  {
    foreach (OwnedAirframe ownedAirframe in this.OwnedAirframes)
    {
      if (ownedAirframe.Reserved)
        return true;
    }
    return false;
  }

  public int OwnedAirframeTypeCount(AircraftDefinition aircraftDef, bool includeReserved)
  {
    int num = 0;
    foreach (OwnedAirframe ownedAirframe in this.OwnedAirframes)
    {
      if ((UnityEngine.Object) ownedAirframe.Definition == (UnityEngine.Object) aircraftDef && (includeReserved || !ownedAirframe.Reserved))
        ++num;
    }
    return num;
  }

  public bool CanAffordAirframe(AircraftDefinition aircraftDef)
  {
    float num = 0.0f;
    foreach (OwnedAirframe ownedAirframe in this.OwnedAirframes)
      num += ownedAirframe.Definition.value;
    return (double) this.Allocation + (double) num > (double) aircraftDef.value + 2.0;
  }

  [ServerRpc]
  public UniTask<ReserveNotice> CmdCheckReservingAirframe(AircraftDefinition aircraftDef)
  {
    if (ServerRpcSender.ShouldInvokeLocally((NetworkBehaviour) this, true, false))
      return this.UserCode_CmdCheckReservingAirframe_1562388900(aircraftDef);
    PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
    writer.WriteAircraftDefinition(aircraftDef);
    UniTask<ReserveNotice> uniTask = ServerRpcSender.SendWithReturn<ReserveNotice>((NetworkBehaviour) this, 0, (NetworkWriter) writer, true);
    writer.Release();
    return uniTask;
  }

  [ServerRpc]
  public void CmdRequestReserveAirframe(AircraftDefinition aircraftDef)
  {
    if (ServerRpcSender.ShouldInvokeLocally((NetworkBehaviour) this, true, false))
    {
      this.UserCode_CmdRequestReserveAirframe_1710514814(aircraftDef);
    }
    else
    {
      PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
      writer.WriteAircraftDefinition(aircraftDef);
      ServerRpcSender.Send((NetworkBehaviour) this, 1, (NetworkWriter) writer, Mirage.Channel.Reliable, true);
      writer.Release();
    }
  }

  [ClientRpc(target = RpcTarget.Owner)]
  public void RpcReserveNotice(ReserveNotice reserveNotice)
  {
    if (ClientRpcSender.ShouldInvokeLocally((NetworkBehaviour) this, RpcTarget.Owner, (INetworkPlayer) null, false))
    {
      this.UserCode_RpcReserveNotice_\u002D1128933059(reserveNotice);
    }
    else
    {
      PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
      GeneratedNetworkCode._Write_NuclearOption\u002EReserveNotice((NetworkWriter) writer, reserveNotice);
      ClientRpcSender.SendTarget((NetworkBehaviour) this, 2, (NetworkWriter) writer, Mirage.Channel.Reliable, (INetworkPlayer) null);
      writer.Release();
    }
  }

  [ServerRpc]
  public void CmdPurchaseAirframe(AircraftDefinition aircraftDef)
  {
    if (ServerRpcSender.ShouldInvokeLocally((NetworkBehaviour) this, true, false))
    {
      this.UserCode_CmdPurchaseAirframe_787674834(aircraftDef);
    }
    else
    {
      PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
      writer.WriteAircraftDefinition(aircraftDef);
      ServerRpcSender.Send((NetworkBehaviour) this, 3, (NetworkWriter) writer, Mirage.Channel.Reliable, true);
      writer.Release();
    }
  }

  [ServerRpc]
  public void CmdSellAirframe(AircraftDefinition aircraftDef)
  {
    if (ServerRpcSender.ShouldInvokeLocally((NetworkBehaviour) this, true, false))
    {
      this.UserCode_CmdSellAirframe_1853434019(aircraftDef);
    }
    else
    {
      PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
      writer.WriteAircraftDefinition(aircraftDef);
      ServerRpcSender.Send((NetworkBehaviour) this, 4, (NetworkWriter) writer, Mirage.Channel.Reliable, true);
      writer.Release();
    }
  }

  [ServerRpc]
  public void CmdReturnAirframe(AircraftDefinition aircraftDef)
  {
    if (ServerRpcSender.ShouldInvokeLocally((NetworkBehaviour) this, true, false))
    {
      this.UserCode_CmdReturnAirframe_\u002D1503330303(aircraftDef);
    }
    else
    {
      PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
      writer.WriteAircraftDefinition(aircraftDef);
      ServerRpcSender.Send((NetworkBehaviour) this, 5, (NetworkWriter) writer, Mirage.Channel.Reliable, true);
      writer.Release();
    }
  }

  [Mirage.Server]
  public void CreditAirframe(AircraftDefinition aircraftDef, int amount, bool reserved)
  {
    if (!this.IsServer)
      throw new MethodInvocationException("[Server] function 'CreditAirframe' called when server not active");
    if (amount == 0)
      ColorLog<Player>.LogError("CreditAirframe should not be called with amount = 0");
    else if (amount > 0)
    {
      for (int index = 0; index < amount; ++index)
        this.OwnedAirframes.Add(new OwnedAirframe(aircraftDef, reserved));
    }
    else
    {
      int num = 0;
      for (int index = this.OwnedAirframes.Count - 1; index >= 0; --index)
      {
        if ((UnityEngine.Object) this.OwnedAirframes[index].Definition == (UnityEngine.Object) aircraftDef)
        {
          this.OwnedAirframes.RemoveAt(index);
          ++num;
          if (num >= amount)
            break;
        }
      }
    }
  }

  [ServerRpc]
  public void CmdDonateFactionFunds(float amount)
  {
    if (ServerRpcSender.ShouldInvokeLocally((NetworkBehaviour) this, true, false))
    {
      this.UserCode_CmdDonateFactionFunds_\u002D1612289293(amount);
    }
    else
    {
      PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
      writer.WriteSingleConverter(amount);
      ServerRpcSender.Send((NetworkBehaviour) this, 6, (NetworkWriter) writer, Mirage.Channel.Reliable, true);
      writer.Release();
    }
  }

  [ServerRpc]
  public void CmdPurchaseConvoy(string convoyName)
  {
    if (ServerRpcSender.ShouldInvokeLocally((NetworkBehaviour) this, true, false))
    {
      this.UserCode_CmdPurchaseConvoy_\u002D2013342036(convoyName);
    }
    else
    {
      PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
      writer.WriteString(convoyName);
      ServerRpcSender.Send((NetworkBehaviour) this, 7, (NetworkWriter) writer, Mirage.Channel.Reliable, true);
      writer.Release();
    }
  }

  [ServerRpc]
  public UniTask<float> CmdGetDelayContribute()
  {
    if (ServerRpcSender.ShouldInvokeLocally((NetworkBehaviour) this, true, false))
      return this.UserCode_CmdGetDelayContribute_219629401();
    PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
    UniTask<float> delayContribute = ServerRpcSender.SendWithReturn<float>((NetworkBehaviour) this, 8, (NetworkWriter) writer, true);
    writer.Release();
    return delayContribute;
  }

  [ServerRpc]
  public UniTask<float> CmdGetDelaySpawnConvoy()
  {
    if (ServerRpcSender.ShouldInvokeLocally((NetworkBehaviour) this, true, false))
      return this.UserCode_CmdGetDelaySpawnConvoy_\u002D1168798609();
    PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
    UniTask<float> delaySpawnConvoy = ServerRpcSender.SendWithReturn<float>((NetworkBehaviour) this, 9, (NetworkWriter) writer, true);
    writer.Release();
    return delaySpawnConvoy;
  }

  [Mirage.Server]
  public void AddAllocation(float amount)
  {
    if (!this.IsServer)
      throw new MethodInvocationException("[Server] function 'AddAllocation' called when server not active");
    this.Allocation += amount;
  }

  [Mirage.Server]
  public void SetAllocation(float newValue)
  {
    if (!this.IsServer)
      throw new MethodInvocationException("[Server] function 'SetAllocation' called when server not active");
    this.Allocation = newValue;
  }

  private bool NoOrSameFaction()
  {
    FactionHQ localHq;
    return !GameManager.GetLocalHQ(out localHq) || (UnityEngine.Object) localHq == (UnityEngine.Object) this.HQ;
  }

  [ClientRpc]
  private void RpcConvoyDonationMessage(string convoyName)
  {
    if (ClientRpcSender.ShouldInvokeLocally((NetworkBehaviour) this, RpcTarget.Observers, (INetworkPlayer) null, false))
      this.UserCode_RpcConvoyDonationMessage_1829517611(convoyName);
    PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
    writer.WriteString(convoyName);
    ClientRpcSender.Send((NetworkBehaviour) this, 10, (NetworkWriter) writer, Mirage.Channel.Reliable, false);
    writer.Release();
  }

  [ClientRpc]
  private void RpcUnitDonationMessage(UnitDefinition unitDefinition, int quantity)
  {
    if (ClientRpcSender.ShouldInvokeLocally((NetworkBehaviour) this, RpcTarget.Observers, (INetworkPlayer) null, false))
      this.UserCode_RpcUnitDonationMessage_178674173(unitDefinition, quantity);
    PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
    writer.WriteUnitDefinition(unitDefinition);
    writer.WritePackedInt32(quantity);
    ClientRpcSender.Send((NetworkBehaviour) this, 11, (NetworkWriter) writer, Mirage.Channel.Reliable, false);
    writer.Release();
  }

  [ClientRpc]
  private void RpcFundsDonationMessage(float amount)
  {
    if (ClientRpcSender.ShouldInvokeLocally((NetworkBehaviour) this, RpcTarget.Observers, (INetworkPlayer) null, false))
      this.UserCode_RpcFundsDonationMessage_\u002D1432044318(amount);
    PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
    writer.WriteSingleConverter(amount);
    ClientRpcSender.Send((NetworkBehaviour) this, 12, (NetworkWriter) writer, Mirage.Channel.Reliable, false);
    writer.Release();
  }

  private void OnStartClient()
  {
    this.PlayerRef = new PlayerRef(this);
    UnitRegistry.playerLookup.Add(this.PlayerRef, this);
  }

  private void OnStartLocalPlayer()
  {
    GameManager.SetLocalPlayer((BasePlayer) this);
    if ((UnityEngine.Object) SceneSingleton<ReserveReport>.i != (UnityEngine.Object) null)
      SceneSingleton<ReserveReport>.i.Initialize(this);
    this.CmdSetPlayerName(PlayerSettings.playerName_Unsanitized);
    if (!GameManager.gameState.IsSingleOrMultiplayer())
      return;
    this.WaitShowJoinMenu().Forget();
  }

  private void OnStopClient()
  {
    if (!((UnityEngine.Object) NetworkSceneSingleton<MessageManager>.i != (UnityEngine.Object) null))
      return;
    NetworkSceneSingleton<MessageManager>.i.DisconnectedMessage(this);
  }

  public void OnDestroy()
  {
    if ((UnityEngine.Object) this.Aircraft != (UnityEngine.Object) null)
      this.Aircraft.NetworkplayerRef = PlayerRef.Invalid;
    UnitRegistry.playerLookup.Remove(this.PlayerRef);
  }

  private async UniTask WaitShowJoinMenu()
  {
    Player player = this;
    CancellationToken cancel = player.destroyCancellationToken;
    await UniTask.Delay(1000);
    if (cancel.IsCancellationRequested)
      cancel = new CancellationToken();
    else if (GameManager.GetLocalAircraft(out Aircraft _))
    {
      MusicManager.i.FadeOut(2f);
      cancel = new CancellationToken();
    }
    else if ((UnityEngine.Object) player.HQ != (UnityEngine.Object) null)
    {
      MusicManager.i.CrossFadeMusic(NetworkSceneSingleton<LevelInfo>.i.LoadedMapSettings.GetStartMusic(player.HQ.faction), 2f, 0.0f, false, false, true);
      SceneSingleton<DynamicMap>.i.SetFaction(player.HQ);
      SceneSingleton<DynamicMap>.i.Maximize();
      cancel = new CancellationToken();
    }
    else
    {
      SceneSingleton<GameplayUI>.i.ShowJoinMenu();
      cancel = new CancellationToken();
    }
  }

  [Mirage.Server]
  public void AddScore(float score)
  {
    if (!this.IsServer)
      throw new MethodInvocationException("[Server] function 'AddScore' called when server not active");
    this.SetScore(this.PlayerScore + score);
  }

  [Mirage.Server]
  public void SetScore(float newScore)
  {
    if (!this.IsServer)
      throw new MethodInvocationException("[Server] function 'SetScore' called when server not active");
    this.PlayerScore = newScore;
    int rank = this.CalculateRank(this.GetRankScore());
    if (rank <= this.PlayerRank)
      return;
    this.SetRank(rank, false);
  }

  private float GetRankScore()
  {
    return (this.PlayerScore - this.scoreOffset) * MissionHelper.RankMultiplier;
  }

  [Mirage.Server]
  public void SetRank(int newRank, bool setScoreOffset)
  {
    if (!this.IsServer)
      throw new MethodInvocationException("[Server] function 'SetRank' called when server not active");
    newRank = Math.Clamp(newRank, 0, this.rankThresholds.Length - 1);
    ColorLog<Player>.Info($"Player rank up old={this.PlayerRank} new={newRank}");
    this.PlayerRank = newRank;
    if (!setScoreOffset)
      return;
    this.scoreOffset = this.PlayerScore - this.rankThresholds[newRank];
  }

  private int CalculateRank(float score)
  {
    int rank = 0;
    for (int index = 0; index < this.rankThresholds.Length && (double) this.rankThresholds[index] <= (double) score; ++index)
      rank = index;
    return rank;
  }

  public float ScoreNeededForNextRank()
  {
    int index = this.PlayerRank + 1;
    return index >= this.rankThresholds.Length ? 0.0f : this.rankThresholds[index] - this.GetRankScore();
  }

  private void RankChanged(int _, int __)
  {
    if (!this.IsLocalPlayer || (double) Time.timeSinceLevelLoad <= 10.0)
      return;
    KillDisplay.FlashRank(this.PlayerRank);
  }

  private void AllocationChanged(float prev, float current)
  {
    if (!this.IsLocalPlayer || (double) Time.timeSinceLevelLoad <= 10.0)
      return;
    SceneSingleton<AllocationDisplay>.i.Show(this, current - prev);
  }

  private void NameChanged(string _, string __)
  {
    if (GameManager.IsLocalPlayer<Player>(this) || !((UnityEngine.Object) NetworkSceneSingleton<MessageManager>.i != (UnityEngine.Object) null))
      return;
    NetworkSceneSingleton<MessageManager>.i.JoinMessage(this);
  }

  public void ShowMap(float delay)
  {
    UniTask.Void((Func<UniTaskVoid>) (async () =>
    {
      CancellationToken cancel = this.destroyCancellationToken;
      for (float timer = 0.0f; (double) timer < (double) delay; timer += Time.deltaTime)
      {
        await UniTask.Yield();
        if (cancel.IsCancellationRequested)
        {
          cancel = new CancellationToken();
          return;
        }
        if (DynamicMap.mapMaximized)
        {
          cancel = new CancellationToken();
          return;
        }
      }
      if (GameManager.gameResolution != GameResolution.Defeat)
      {
        SceneSingleton<DynamicMap>.i.Maximize();
        cancel = new CancellationToken();
      }
      else
      {
        MusicManager.i.PlayMusic(GameAssets.i.missionFailedMusic, false);
        SceneSingleton<GameplayUI>.i.PauseGame();
        cancel = new CancellationToken();
      }
    }));
  }

  public void AttachToAircraft(Aircraft aircraft)
  {
    aircraft.pilots[0].player = this;
    aircraft.pilots[0].aircraft = aircraft;
    SceneSingleton<GameplayUI>.i.hurt.gameObject.SetActive(false);
    CursorManager.Refresh();
  }

  [Mirage.Server]
  public void FlyOwnedAirframe(AircraftDefinition airframe)
  {
    if (!this.IsServer)
      throw new MethodInvocationException("[Server] function 'FlyOwnedAirframe' called when server not active");
    for (int index = this.OwnedAirframes.Count - 1; index >= 0; --index)
    {
      if ((UnityEngine.Object) this.OwnedAirframes[index].Definition == (UnityEngine.Object) airframe)
      {
        this.NetworkAirframeInUse = new OwnedAirframe?(this.OwnedAirframes[index]);
        this.OwnedAirframes.RemoveAt(index);
        break;
      }
    }
  }

  [Mirage.Server]
  public void RecoverAirframeInUse(AircraftDefinition airframe)
  {
    if (!this.IsServer)
      throw new MethodInvocationException("[Server] function 'RecoverAirframeInUse' called when server not active");
    if (this.AirframeInUse.HasValue)
      this.OwnedAirframes.Add(this.AirframeInUse.Value);
    else
      this.OwnedAirframes.Add(new OwnedAirframe(airframe, true));
    this.NetworkAirframeInUse = new OwnedAirframe?();
  }

  public void SetFaction(FactionHQ newHQ)
  {
    if (!this.ValidateFactionChange(newHQ))
      return;
    this.CmdSetFaction(newHQ);
    this.HQ = newHQ;
  }

  private bool ValidateFactionChange(FactionHQ newHQ)
  {
    if ((UnityEngine.Object) this.HQ == (UnityEngine.Object) newHQ)
      return false;
    if ((UnityEngine.Object) this.HQ != (UnityEngine.Object) null)
    {
      Debug.LogError((object) $"Faction already set to {this.HQ} it can't be changed to {newHQ}");
      return false;
    }
    if (MissionManager.CurrentMission == null)
    {
      Debug.LogError((object) "No mission loaded, can't join a faction");
      return false;
    }
    if (!newHQ.preventJoin)
      return true;
    Debug.LogError((object) "Faction has Prevent Join");
    return false;
  }

  [ServerRpc(allowServerToCall = true)]
  private void CmdSetFaction(FactionHQ newHQ)
  {
    if (ServerRpcSender.ShouldInvokeLocally((NetworkBehaviour) this, true, true))
    {
      this.UserCode_CmdSetFaction_\u002D1076503876(newHQ);
    }
    else
    {
      PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
      GeneratedNetworkCode._Write_FactionHQ((NetworkWriter) writer, newHQ);
      ServerRpcSender.Send((NetworkBehaviour) this, 13, (NetworkWriter) writer, Mirage.Channel.Reliable, true);
      writer.Release();
    }
  }

  [ClientRpc(target = RpcTarget.Owner)]
  public void RpcShowSortieBonus(float score)
  {
    if (ClientRpcSender.ShouldInvokeLocally((NetworkBehaviour) this, RpcTarget.Owner, (INetworkPlayer) null, false))
    {
      this.UserCode_RpcShowSortieBonus_366758595(score);
    }
    else
    {
      PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
      writer.WriteSingleConverter(score);
      ClientRpcSender.SendTarget((NetworkBehaviour) this, 14, (NetworkWriter) writer, Mirage.Channel.Reliable, (INetworkPlayer) null);
      writer.Release();
    }
  }

  [ServerRpc]
  public void CmdSetPlayerName(string playerName)
  {
    if (ServerRpcSender.ShouldInvokeLocally((NetworkBehaviour) this, true, false))
    {
      this.UserCode_CmdSetPlayerName_\u002D1114485719(playerName);
    }
    else
    {
      PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
      writer.WriteString(playerName);
      ServerRpcSender.Send((NetworkBehaviour) this, 15, (NetworkWriter) writer, Mirage.Channel.Reliable, true);
      writer.Release();
    }
  }

  [ClientRpc(target = RpcTarget.Owner)]
  public void KickReason(string reason, string hostName)
  {
    if (ClientRpcSender.ShouldInvokeLocally((NetworkBehaviour) this, RpcTarget.Owner, (INetworkPlayer) null, false))
    {
      this.UserCode_KickReason_980132143(reason, hostName);
    }
    else
    {
      PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
      writer.WriteString(reason);
      writer.WriteString(hostName);
      ClientRpcSender.SendTarget((NetworkBehaviour) this, 16 /*0x10*/, (NetworkWriter) writer, Mirage.Channel.Reliable, (INetworkPlayer) null);
      writer.Release();
    }
  }

  [ClientRpc(target = RpcTarget.Owner)]
  public void RpcClearSpawnPending()
  {
    if (ClientRpcSender.ShouldInvokeLocally((NetworkBehaviour) this, RpcTarget.Owner, (INetworkPlayer) null, false))
    {
      this.UserCode_RpcClearSpawnPending_2024023721();
    }
    else
    {
      PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
      ClientRpcSender.SendTarget((NetworkBehaviour) this, 17, (NetworkWriter) writer, Mirage.Channel.Reliable, (INetworkPlayer) null);
      writer.Release();
    }
  }

  public Player() => this.InitSyncObject((ISyncObject) this.OwnedAirframes);

  private void MirageProcessed()
  {
  }

  public FactionHQ Network\u003CHQ\u003Ek__BackingField
  {
    get => (FactionHQ) this.\u003CHQ\u003Ek__BackingField.Value;
    set
    {
      if (this.SyncVarEqual<FactionHQ>(value, (FactionHQ) this.\u003CHQ\u003Ek__BackingField.Value))
        return;
      FactionHQ factionHq = (FactionHQ) this.\u003CHQ\u003Ek__BackingField.Value;
      this.\u003CHQ\u003Ek__BackingField.Value = (NetworkBehaviour) value;
      this.SetDirtyBit(2UL);
    }
  }

  public string Network\u003CPlayerName\u003Ek__BackingField
  {
    get => this.\u003CPlayerName\u003Ek__BackingField;
    set
    {
      if (this.SyncVarEqual<string>(value, this.\u003CPlayerName\u003Ek__BackingField))
        return;
      string nameKBackingField = this.\u003CPlayerName\u003Ek__BackingField;
      this.\u003CPlayerName\u003Ek__BackingField = value;
      this.SetDirtyBit(4UL);
      if (!this.GetSyncVarHookGuard(4UL) && this.IsHost)
      {
        this.SetSyncVarHookGuard(4UL, true);
        this.NameChanged(nameKBackingField, value);
        this.SetSyncVarHookGuard(4UL, false);
      }
    }
  }

  public float Network\u003CPlayerScore\u003Ek__BackingField
  {
    get => this.\u003CPlayerScore\u003Ek__BackingField;
    set
    {
      if (this.SyncVarEqual<float>(value, this.\u003CPlayerScore\u003Ek__BackingField))
        return;
      float scoreKBackingField = this.\u003CPlayerScore\u003Ek__BackingField;
      this.\u003CPlayerScore\u003Ek__BackingField = value;
      this.SetDirtyBit(8UL);
    }
  }

  public int Network\u003CPlayerRank\u003Ek__BackingField
  {
    get => this.\u003CPlayerRank\u003Ek__BackingField;
    set
    {
      if (this.SyncVarEqual<int>(value, this.\u003CPlayerRank\u003Ek__BackingField))
        return;
      int rankKBackingField = this.\u003CPlayerRank\u003Ek__BackingField;
      this.\u003CPlayerRank\u003Ek__BackingField = value;
      this.SetDirtyBit(16UL /*0x10*/);
      if (!this.GetSyncVarHookGuard(16UL /*0x10*/) && this.IsHost)
      {
        this.SetSyncVarHookGuard(16UL /*0x10*/, true);
        this.RankChanged(rankKBackingField, value);
        this.SetSyncVarHookGuard(16UL /*0x10*/, false);
      }
    }
  }

  public float Network\u003CAllocation\u003Ek__BackingField
  {
    get => this.\u003CAllocation\u003Ek__BackingField;
    set
    {
      if (this.SyncVarEqual<float>(value, this.\u003CAllocation\u003Ek__BackingField))
        return;
      float allocationKBackingField = this.\u003CAllocation\u003Ek__BackingField;
      this.\u003CAllocation\u003Ek__BackingField = value;
      this.SetDirtyBit(32UL /*0x20*/);
      if (!this.GetSyncVarHookGuard(32UL /*0x20*/) && this.IsHost)
      {
        this.SetSyncVarHookGuard(32UL /*0x20*/, true);
        this.AllocationChanged(allocationKBackingField, value);
        this.SetSyncVarHookGuard(32UL /*0x20*/, false);
      }
    }
  }

  public OwnedAirframe? NetworkAirframeInUse
  {
    get => this.AirframeInUse;
    set
    {
      if (this.SyncVarEqual<OwnedAirframe?>(value, this.AirframeInUse))
        return;
      OwnedAirframe? airframeInUse = this.AirframeInUse;
      this.AirframeInUse = value;
      this.SetDirtyBit(64UL /*0x40*/);
    }
  }

  public override bool SerializeSyncVars(NetworkWriter writer, bool initialize)
  {
    ulong syncVarDirtyBits = this.SyncVarDirtyBits;
    bool flag = base.SerializeSyncVars(writer, initialize);
    if (initialize)
    {
      // ISSUE: reference to a compiler-generated field
      writer.WriteNetworkBehaviorSyncVar(this.\u003CHQ\u003Ek__BackingField);
      // ISSUE: reference to a compiler-generated field
      writer.WriteString(this.\u003CPlayerName\u003Ek__BackingField);
      // ISSUE: reference to a compiler-generated field
      writer.WriteSingleConverter(this.\u003CPlayerScore\u003Ek__BackingField);
      // ISSUE: reference to a compiler-generated field
      writer.WritePackedInt32(this.\u003CPlayerRank\u003Ek__BackingField);
      // ISSUE: reference to a compiler-generated field
      writer.WriteSingleConverter(this.\u003CAllocation\u003Ek__BackingField);
      GeneratedNetworkCode._Write_System\u002ENullable\u00601\u003CNuclearOption\u002EOwnedAirframe\u003E(writer, this.AirframeInUse);
      return true;
    }
    writer.Write(syncVarDirtyBits >> 1, 6);
    if (((long) syncVarDirtyBits & 2L) != 0L)
    {
      // ISSUE: reference to a compiler-generated field
      writer.WriteNetworkBehaviorSyncVar(this.\u003CHQ\u003Ek__BackingField);
      flag = true;
    }
    if (((long) syncVarDirtyBits & 4L) != 0L)
    {
      // ISSUE: reference to a compiler-generated field
      writer.WriteString(this.\u003CPlayerName\u003Ek__BackingField);
      flag = true;
    }
    if (((long) syncVarDirtyBits & 8L) != 0L)
    {
      // ISSUE: reference to a compiler-generated field
      writer.WriteSingleConverter(this.\u003CPlayerScore\u003Ek__BackingField);
      flag = true;
    }
    if (((long) syncVarDirtyBits & 16L /*0x10*/) != 0L)
    {
      // ISSUE: reference to a compiler-generated field
      writer.WritePackedInt32(this.\u003CPlayerRank\u003Ek__BackingField);
      flag = true;
    }
    if (((long) syncVarDirtyBits & 32L /*0x20*/) != 0L)
    {
      // ISSUE: reference to a compiler-generated field
      writer.WriteSingleConverter(this.\u003CAllocation\u003Ek__BackingField);
      flag = true;
    }
    if (((long) syncVarDirtyBits & 64L /*0x40*/) != 0L)
    {
      GeneratedNetworkCode._Write_System\u002ENullable\u00601\u003CNuclearOption\u002EOwnedAirframe\u003E(writer, this.AirframeInUse);
      flag = true;
    }
    return flag;
  }

  public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
  {
    base.DeserializeSyncVars(reader, initialState);
    if (initialState)
    {
      // ISSUE: reference to a compiler-generated field
      this.\u003CHQ\u003Ek__BackingField = reader.ReadNetworkBehaviourSyncVar();
      // ISSUE: reference to a compiler-generated field
      string nameKBackingField = this.\u003CPlayerName\u003Ek__BackingField;
      // ISSUE: reference to a compiler-generated field
      this.\u003CPlayerName\u003Ek__BackingField = reader.ReadString();
      // ISSUE: reference to a compiler-generated field
      this.\u003CPlayerScore\u003Ek__BackingField = reader.ReadSingleConverter();
      // ISSUE: reference to a compiler-generated field
      int rankKBackingField = this.\u003CPlayerRank\u003Ek__BackingField;
      // ISSUE: reference to a compiler-generated field
      this.\u003CPlayerRank\u003Ek__BackingField = reader.ReadPackedInt32();
      // ISSUE: reference to a compiler-generated field
      float allocationKBackingField = this.\u003CAllocation\u003Ek__BackingField;
      // ISSUE: reference to a compiler-generated field
      this.\u003CAllocation\u003Ek__BackingField = reader.ReadSingleConverter();
      this.AirframeInUse = GeneratedNetworkCode._Read_System\u002ENullable\u00601\u003CNuclearOption\u002EOwnedAirframe\u003E(reader);
      // ISSUE: reference to a compiler-generated field
      if (!this.IsServer && !this.SyncVarEqual<string>(nameKBackingField, this.\u003CPlayerName\u003Ek__BackingField))
      {
        // ISSUE: reference to a compiler-generated field
        this.NameChanged(nameKBackingField, this.\u003CPlayerName\u003Ek__BackingField);
      }
      // ISSUE: reference to a compiler-generated field
      if (!this.IsServer && !this.SyncVarEqual<int>(rankKBackingField, this.\u003CPlayerRank\u003Ek__BackingField))
      {
        // ISSUE: reference to a compiler-generated field
        this.RankChanged(rankKBackingField, this.\u003CPlayerRank\u003Ek__BackingField);
      }
      // ISSUE: reference to a compiler-generated field
      if (this.IsServer || this.SyncVarEqual<float>(allocationKBackingField, this.\u003CAllocation\u003Ek__BackingField))
        return;
      // ISSUE: reference to a compiler-generated field
      this.AllocationChanged(allocationKBackingField, this.\u003CAllocation\u003Ek__BackingField);
    }
    else
    {
      ulong dirtyBit = reader.Read(6);
      this.SetDeserializeMask(dirtyBit, 1);
      if (((long) dirtyBit & 1L) != 0L)
      {
        // ISSUE: reference to a compiler-generated field
        this.\u003CHQ\u003Ek__BackingField = reader.ReadNetworkBehaviourSyncVar();
      }
      if (((long) dirtyBit & 2L) != 0L)
      {
        // ISSUE: reference to a compiler-generated field
        string nameKBackingField = this.\u003CPlayerName\u003Ek__BackingField;
        // ISSUE: reference to a compiler-generated field
        this.\u003CPlayerName\u003Ek__BackingField = reader.ReadString();
        // ISSUE: reference to a compiler-generated field
        if (!this.IsServer && !this.SyncVarEqual<string>(nameKBackingField, this.\u003CPlayerName\u003Ek__BackingField))
        {
          // ISSUE: reference to a compiler-generated field
          this.NameChanged(nameKBackingField, this.\u003CPlayerName\u003Ek__BackingField);
        }
      }
      if (((long) dirtyBit & 4L) != 0L)
      {
        // ISSUE: reference to a compiler-generated field
        this.\u003CPlayerScore\u003Ek__BackingField = reader.ReadSingleConverter();
      }
      if (((long) dirtyBit & 8L) != 0L)
      {
        // ISSUE: reference to a compiler-generated field
        int rankKBackingField = this.\u003CPlayerRank\u003Ek__BackingField;
        // ISSUE: reference to a compiler-generated field
        this.\u003CPlayerRank\u003Ek__BackingField = reader.ReadPackedInt32();
        // ISSUE: reference to a compiler-generated field
        if (!this.IsServer && !this.SyncVarEqual<int>(rankKBackingField, this.\u003CPlayerRank\u003Ek__BackingField))
        {
          // ISSUE: reference to a compiler-generated field
          this.RankChanged(rankKBackingField, this.\u003CPlayerRank\u003Ek__BackingField);
        }
      }
      if (((long) dirtyBit & 16L /*0x10*/) != 0L)
      {
        // ISSUE: reference to a compiler-generated field
        float allocationKBackingField = this.\u003CAllocation\u003Ek__BackingField;
        // ISSUE: reference to a compiler-generated field
        this.\u003CAllocation\u003Ek__BackingField = reader.ReadSingleConverter();
        // ISSUE: reference to a compiler-generated field
        if (!this.IsServer && !this.SyncVarEqual<float>(allocationKBackingField, this.\u003CAllocation\u003Ek__BackingField))
        {
          // ISSUE: reference to a compiler-generated field
          this.AllocationChanged(allocationKBackingField, this.\u003CAllocation\u003Ek__BackingField);
        }
      }
      if (((long) dirtyBit & 32L /*0x20*/) == 0L)
        return;
      this.AirframeInUse = GeneratedNetworkCode._Read_System\u002ENullable\u00601\u003CNuclearOption\u002EOwnedAirframe\u003E(reader);
    }
  }

  public UniTask<ReserveNotice> UserCode_CmdCheckReservingAirframe_1562388900(
    AircraftDefinition aircraftDef)
  {
    if ((UnityEngine.Object) aircraftDef == (UnityEngine.Object) null)
      return UniTask.FromResult<ReserveNotice>(ReserveNotice.Invalid);
    if ((UnityEngine.Object) this.HQ == (UnityEngine.Object) null)
      return UniTask.FromResult<ReserveNotice>(ReserveNotice.Invalid);
    ReserveNotice reserveNotice = new ReserveNotice(ReserveEvent.acceptedInQueue, aircraftDef, false, 0);
    if (this.HQ.IsReservingAirframe(this, aircraftDef))
    {
      reserveNotice.isReserving = true;
      reserveNotice.outcome = ReserveEvent.rejectedDuplicate;
    }
    if (this.PossessesReservedAirframe())
    {
      reserveNotice.isReserving = false;
      reserveNotice.outcome = ReserveEvent.rejectedPossessesReserved;
    }
    if (this.PlayerRank < aircraftDef.aircraftParameters.rankRequired)
    {
      reserveNotice.isReserving = false;
      reserveNotice.outcome = ReserveEvent.rejectedRank;
    }
    if (this.OwnsAirframe(aircraftDef, true))
    {
      reserveNotice.isReserving = false;
      reserveNotice.outcome = ReserveEvent.rejectedOwned;
    }
    return UniTask.FromResult<ReserveNotice>(reserveNotice);
  }

  protected static UniTask<ReserveNotice> Skeleton_CmdCheckReservingAirframe_1562388900(
    NetworkBehaviour behaviour,
    NetworkReader reader,
    INetworkPlayer senderConnection,
    int replyId)
  {
    return ((Player) behaviour).UserCode_CmdCheckReservingAirframe_1562388900(reader.ReadAircraftDefinition());
  }

  public void UserCode_CmdRequestReserveAirframe_1710514814(AircraftDefinition aircraftDef)
  {
    if ((UnityEngine.Object) aircraftDef == (UnityEngine.Object) null || (UnityEngine.Object) this.HQ == (UnityEngine.Object) null)
      return;
    this.HQ.RequestReservedAirframe(this, aircraftDef);
  }

  protected static void Skeleton_CmdRequestReserveAirframe_1710514814(
    NetworkBehaviour behaviour,
    NetworkReader reader,
    INetworkPlayer senderConnection,
    int replyId)
  {
    ((Player) behaviour).UserCode_CmdRequestReserveAirframe_1710514814(reader.ReadAircraftDefinition());
  }

  public void UserCode_RpcReserveNotice_\u002D1128933059(ReserveNotice reserveNotice)
  {
    if (reserveNotice.outcome == ReserveEvent.granted)
      this.lastReserveGranted = Time.timeSinceLevelLoad;
    Action<ReserveNotice> onReserveNotice = this.onReserveNotice;
    if (onReserveNotice == null)
      return;
    onReserveNotice(reserveNotice);
  }

  protected static void Skeleton_RpcReserveNotice_\u002D1128933059(
    NetworkBehaviour behaviour,
    NetworkReader reader,
    INetworkPlayer senderConnection,
    int replyId)
  {
    ((Player) behaviour).UserCode_RpcReserveNotice_\u002D1128933059(GeneratedNetworkCode._Read_NuclearOption\u002EReserveNotice(reader));
  }

  public void UserCode_CmdPurchaseAirframe_787674834(AircraftDefinition aircraftDef)
  {
    if ((UnityEngine.Object) aircraftDef == (UnityEngine.Object) null)
      return;
    ColorLog<Player>.Info($"Purchasing Airframe {aircraftDef.unitPrefab}");
    if ((double) aircraftDef.value > (double) this.Allocation)
      return;
    this.CreditAirframe(aircraftDef, 1, false);
    this.Allocation -= aircraftDef.value;
  }

  protected static void Skeleton_CmdPurchaseAirframe_787674834(
    NetworkBehaviour behaviour,
    NetworkReader reader,
    INetworkPlayer senderConnection,
    int replyId)
  {
    ((Player) behaviour).UserCode_CmdPurchaseAirframe_787674834(reader.ReadAircraftDefinition());
  }

  public void UserCode_CmdSellAirframe_1853434019(AircraftDefinition aircraftDef)
  {
    if ((UnityEngine.Object) aircraftDef == (UnityEngine.Object) null || !this.OwnsAirframe(aircraftDef, false))
      return;
    this.CreditAirframe(aircraftDef, -1, false);
    this.AddAllocation(aircraftDef.value);
  }

  protected static void Skeleton_CmdSellAirframe_1853434019(
    NetworkBehaviour behaviour,
    NetworkReader reader,
    INetworkPlayer senderConnection,
    int replyId)
  {
    ((Player) behaviour).UserCode_CmdSellAirframe_1853434019(reader.ReadAircraftDefinition());
  }

  public void UserCode_CmdReturnAirframe_\u002D1503330303(AircraftDefinition aircraftDef)
  {
    if ((UnityEngine.Object) aircraftDef == (UnityEngine.Object) null || !this.OwnsAirframe(aircraftDef, true))
      return;
    this.CreditAirframe(aircraftDef, -1, false);
    this.HQ.AddSupplyUnit((UnitDefinition) aircraftDef, 1);
  }

  protected static void Skeleton_CmdReturnAirframe_\u002D1503330303(
    NetworkBehaviour behaviour,
    NetworkReader reader,
    INetworkPlayer senderConnection,
    int replyId)
  {
    ((Player) behaviour).UserCode_CmdReturnAirframe_\u002D1503330303(reader.ReadAircraftDefinition());
  }

  public void UserCode_CmdDonateFactionFunds_\u002D1612289293(float amount)
  {
    if (!NetworkFloatHelper.Validate(amount, false, (string) null) || (UnityEngine.Object) this.HQ == (UnityEngine.Object) null || (double) amount < 0.0 || (double) this.Allocation < (double) amount || (double) Time.timeSinceLevelLoad < (double) this.previousContribution + 60.0)
      return;
    this.AddAllocation(-amount);
    this.HQ.AddBonusFunds(amount);
    this.RpcFundsDonationMessage(amount);
    this.previousContribution = Time.timeSinceLevelLoad;
  }

  protected static void Skeleton_CmdDonateFactionFunds_\u002D1612289293(
    NetworkBehaviour behaviour,
    NetworkReader reader,
    INetworkPlayer senderConnection,
    int replyId)
  {
    ((Player) behaviour).UserCode_CmdDonateFactionFunds_\u002D1612289293(reader.ReadSingleConverter());
  }

  public void UserCode_CmdPurchaseConvoy_\u002D2013342036(string convoyName)
  {
    if ((double) Time.timeSinceLevelLoad < (double) this.previousContribution + 60.0 || (UnityEngine.Object) this.HQ == (UnityEngine.Object) null)
      return;
    Faction.ConvoyGroup convoyGroup = this.HQ.faction.GetConvoyGroup(convoyName);
    if (convoyGroup == null)
      return;
    float cost = convoyGroup.GetCost();
    if ((double) this.Allocation < (double) cost)
      return;
    this.HQ.AddConvoy(convoyGroup);
    this.AddAllocation(-cost);
    this.RpcConvoyDonationMessage(convoyGroup.Name);
    this.previousContribution = Time.timeSinceLevelLoad;
  }

  protected static void Skeleton_CmdPurchaseConvoy_\u002D2013342036(
    NetworkBehaviour behaviour,
    NetworkReader reader,
    INetworkPlayer senderConnection,
    int replyId)
  {
    ((Player) behaviour).UserCode_CmdPurchaseConvoy_\u002D2013342036(reader.ReadString());
  }

  public UniTask<float> UserCode_CmdGetDelayContribute_219629401()
  {
    return UniTask.FromResult<float>(Mathf.Max(60f + this.previousContribution - Time.timeSinceLevelLoad, 0.0f));
  }

  protected static UniTask<float> Skeleton_CmdGetDelayContribute_219629401(
    NetworkBehaviour behaviour,
    NetworkReader reader,
    INetworkPlayer senderConnection,
    int replyId)
  {
    return ((Player) behaviour).UserCode_CmdGetDelayContribute_219629401();
  }

  public UniTask<float> UserCode_CmdGetDelaySpawnConvoy_\u002D1168798609()
  {
    if (!((UnityEngine.Object) this.HQ == (UnityEngine.Object) null))
      return UniTask.FromResult<float>(this.HQ.CmdGetDelaySpawnConvoy());
    this.Owner.SetError(5, PlayerErrorFlags.RpcNullException);
    return UniTask.FromResult<float>(0.0f);
  }

  protected static UniTask<float> Skeleton_CmdGetDelaySpawnConvoy_\u002D1168798609(
    NetworkBehaviour behaviour,
    NetworkReader reader,
    INetworkPlayer senderConnection,
    int replyId)
  {
    return ((Player) behaviour).UserCode_CmdGetDelaySpawnConvoy_\u002D1168798609();
  }

  private void UserCode_RpcConvoyDonationMessage_1829517611(string convoyName)
  {
    if (!this.NoOrSameFaction())
      return;
    SceneSingleton<GameplayUI>.i.GameMessage($"{this.PlayerName} provisioned {convoyName}");
  }

  protected static void Skeleton_RpcConvoyDonationMessage_1829517611(
    NetworkBehaviour behaviour,
    NetworkReader reader,
    INetworkPlayer senderConnection,
    int replyId)
  {
    ((Player) behaviour).UserCode_RpcConvoyDonationMessage_1829517611(reader.ReadString());
  }

  private void UserCode_RpcUnitDonationMessage_178674173(
    UnitDefinition unitDefinition,
    int quantity)
  {
    if (!this.NoOrSameFaction())
      return;
    SceneSingleton<GameplayUI>.i.GameMessage($"{this.PlayerName} donated {quantity} {unitDefinition.unitName} to the war effort");
  }

  protected static void Skeleton_RpcUnitDonationMessage_178674173(
    NetworkBehaviour behaviour,
    NetworkReader reader,
    INetworkPlayer senderConnection,
    int replyId)
  {
    ((Player) behaviour).UserCode_RpcUnitDonationMessage_178674173(reader.ReadUnitDefinition(), reader.ReadPackedInt32());
  }

  private void UserCode_RpcFundsDonationMessage_\u002D1432044318(float amount)
  {
    if (!this.NoOrSameFaction())
      return;
    SceneSingleton<GameplayUI>.i.GameMessage($"{this.PlayerName} donated {UnitConverter.ValueReading(amount)} to the war effort");
  }

  protected static void Skeleton_RpcFundsDonationMessage_\u002D1432044318(
    NetworkBehaviour behaviour,
    NetworkReader reader,
    INetworkPlayer senderConnection,
    int replyId)
  {
    ((Player) behaviour).UserCode_RpcFundsDonationMessage_\u002D1432044318(reader.ReadSingleConverter());
  }

  private void UserCode_CmdSetFaction_\u002D1076503876(FactionHQ newHQ)
  {
    if (!this.ValidateFactionChange(newHQ))
      return;
    this.HQ = newHQ;
    this.HQ.AddPlayer(this);
    this.HQ.RequestTrackingStates(this);
  }

  protected static void Skeleton_CmdSetFaction_\u002D1076503876(
    NetworkBehaviour behaviour,
    NetworkReader reader,
    INetworkPlayer senderConnection,
    int replyId)
  {
    ((Player) behaviour).UserCode_CmdSetFaction_\u002D1076503876(GeneratedNetworkCode._Read_FactionHQ(reader));
  }

  public void UserCode_RpcShowSortieBonus_366758595(float score)
  {
    if ((UnityEngine.Object) SceneSingleton<KillDisplay>.i == (UnityEngine.Object) null)
      UnityEngine.Object.Instantiate<GameObject>(GameAssets.i.killDisplay, SceneSingleton<GameplayUI>.i.gameplayCanvas.transform);
    SceneSingleton<KillDisplay>.i.DisplayBonus(score);
  }

  protected static void Skeleton_RpcShowSortieBonus_366758595(
    NetworkBehaviour behaviour,
    NetworkReader reader,
    INetworkPlayer senderConnection,
    int replyId)
  {
    ((Player) behaviour).UserCode_RpcShowSortieBonus_366758595(reader.ReadSingleConverter());
  }

  public void UserCode_CmdSetPlayerName_\u002D1114485719(string playerName)
  {
    if (!string.IsNullOrEmpty(this.PlayerName))
      return;
    this.PlayerName = playerName.SanitizeRichText(32 /*0x20*/).ReplaceCharactersNotInFont(GameAssets.i.playerNameFont);
  }

  protected static void Skeleton_CmdSetPlayerName_\u002D1114485719(
    NetworkBehaviour behaviour,
    NetworkReader reader,
    INetworkPlayer senderConnection,
    int replyId)
  {
    ((Player) behaviour).UserCode_CmdSetPlayerName_\u002D1114485719(reader.ReadString());
  }

  public void UserCode_KickReason_980132143(string reason, string hostName)
  {
    GameManager.SetDisconnectReason(new DisconnectInfo(string.IsNullOrEmpty(reason) ? $"Host {hostName} kicked you from the game" : $"Host {hostName} kicked you from the game\nreason: {reason}"));
  }

  protected static void Skeleton_KickReason_980132143(
    NetworkBehaviour behaviour,
    NetworkReader reader,
    INetworkPlayer senderConnection,
    int replyId)
  {
    ((Player) behaviour).UserCode_KickReason_980132143(reader.ReadString(), reader.ReadString());
  }

  public void UserCode_RpcClearSpawnPending_2024023721()
  {
    Debug.Log((object) "Clearing spawn pending");
    this.SetSpawnPending(false);
  }

  protected static void Skeleton_RpcClearSpawnPending_2024023721(
    NetworkBehaviour behaviour,
    NetworkReader reader,
    INetworkPlayer senderConnection,
    int replyId)
  {
    ((Player) behaviour).UserCode_RpcClearSpawnPending_2024023721();
  }

  protected override int GetRpcCount() => 18;

  public override void RegisterRpc(RemoteCallCollection collection)
  {
    base.RegisterRpc(collection);
    collection.RegisterRequest<ReserveNotice>(0, "NuclearOption.Networking.Player.CmdCheckReservingAirframe", true, RpcInvokeType.ServerRpc, (NetworkBehaviour) this, new RequestDelegate<ReserveNotice>(Player.Skeleton_CmdCheckReservingAirframe_1562388900));
    collection.Register(1, "NuclearOption.Networking.Player.CmdRequestReserveAirframe", true, RpcInvokeType.ServerRpc, (NetworkBehaviour) this, new RpcDelegate(Player.Skeleton_CmdRequestReserveAirframe_1710514814));
    collection.Register(2, "NuclearOption.Networking.Player.RpcReserveNotice", false, RpcInvokeType.ClientRpc, (NetworkBehaviour) this, new RpcDelegate(Player.Skeleton_RpcReserveNotice_\u002D1128933059));
    collection.Register(3, "NuclearOption.Networking.Player.CmdPurchaseAirframe", true, RpcInvokeType.ServerRpc, (NetworkBehaviour) this, new RpcDelegate(Player.Skeleton_CmdPurchaseAirframe_787674834));
    collection.Register(4, "NuclearOption.Networking.Player.CmdSellAirframe", true, RpcInvokeType.ServerRpc, (NetworkBehaviour) this, new RpcDelegate(Player.Skeleton_CmdSellAirframe_1853434019));
    collection.Register(5, "NuclearOption.Networking.Player.CmdReturnAirframe", true, RpcInvokeType.ServerRpc, (NetworkBehaviour) this, new RpcDelegate(Player.Skeleton_CmdReturnAirframe_\u002D1503330303));
    collection.Register(6, "NuclearOption.Networking.Player.CmdDonateFactionFunds", true, RpcInvokeType.ServerRpc, (NetworkBehaviour) this, new RpcDelegate(Player.Skeleton_CmdDonateFactionFunds_\u002D1612289293));
    collection.Register(7, "NuclearOption.Networking.Player.CmdPurchaseConvoy", true, RpcInvokeType.ServerRpc, (NetworkBehaviour) this, new RpcDelegate(Player.Skeleton_CmdPurchaseConvoy_\u002D2013342036));
    collection.RegisterRequest<float>(8, "NuclearOption.Networking.Player.CmdGetDelayContribute", true, RpcInvokeType.ServerRpc, (NetworkBehaviour) this, new RequestDelegate<float>(Player.Skeleton_CmdGetDelayContribute_219629401));
    collection.RegisterRequest<float>(9, "NuclearOption.Networking.Player.CmdGetDelaySpawnConvoy", true, RpcInvokeType.ServerRpc, (NetworkBehaviour) this, new RequestDelegate<float>(Player.Skeleton_CmdGetDelaySpawnConvoy_\u002D1168798609));
    collection.Register(10, "NuclearOption.Networking.Player.RpcConvoyDonationMessage", false, RpcInvokeType.ClientRpc, (NetworkBehaviour) this, new RpcDelegate(Player.Skeleton_RpcConvoyDonationMessage_1829517611));
    collection.Register(11, "NuclearOption.Networking.Player.RpcUnitDonationMessage", false, RpcInvokeType.ClientRpc, (NetworkBehaviour) this, new RpcDelegate(Player.Skeleton_RpcUnitDonationMessage_178674173));
    collection.Register(12, "NuclearOption.Networking.Player.RpcFundsDonationMessage", false, RpcInvokeType.ClientRpc, (NetworkBehaviour) this, new RpcDelegate(Player.Skeleton_RpcFundsDonationMessage_\u002D1432044318));
    collection.Register(13, "NuclearOption.Networking.Player.CmdSetFaction", true, RpcInvokeType.ServerRpc, (NetworkBehaviour) this, new RpcDelegate(Player.Skeleton_CmdSetFaction_\u002D1076503876));
    collection.Register(14, "NuclearOption.Networking.Player.RpcShowSortieBonus", false, RpcInvokeType.ClientRpc, (NetworkBehaviour) this, new RpcDelegate(Player.Skeleton_RpcShowSortieBonus_366758595));
    collection.Register(15, "NuclearOption.Networking.Player.CmdSetPlayerName", true, RpcInvokeType.ServerRpc, (NetworkBehaviour) this, new RpcDelegate(Player.Skeleton_CmdSetPlayerName_\u002D1114485719));
    collection.Register(16 /*0x10*/, "NuclearOption.Networking.Player.KickReason", false, RpcInvokeType.ClientRpc, (NetworkBehaviour) this, new RpcDelegate(Player.Skeleton_KickReason_980132143));
    collection.Register(17, "NuclearOption.Networking.Player.RpcClearSpawnPending", false, RpcInvokeType.ClientRpc, (NetworkBehaviour) this, new RpcDelegate(Player.Skeleton_RpcClearSpawnPending_2024023721));
  }
}

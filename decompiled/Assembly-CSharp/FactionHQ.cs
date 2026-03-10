// Decompiled with JetBrains decompiler
// Type: FactionHQ
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Cysharp.Threading.Tasks;
using Mirage;
using Mirage.Collections;
using Mirage.RemoteCalls;
using Mirage.Serialization;
using NuclearOption;
using NuclearOption.Networking;
using NuclearOption.SavedMission;
using NuclearOption.SavedMission.ObjectiveV2.Outcomes;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

#nullable disable
public class FactionHQ : NetworkBehaviour
{
  public Faction faction;
  public MissionStatsTracker missionStatsTracker;
  public Dictionary<PersistentID, TrackingInfo> trackingDatabase = new Dictionary<PersistentID, TrackingInfo>();
  public readonly SyncList<PersistentID> factionUnits = new SyncList<PersistentID>();
  public readonly SyncList<PersistentID> factionRadarReturn = new SyncList<PersistentID>();
  public readonly SyncList<PlayerRef> factionPlayers = new SyncList<PlayerRef>();
  [ShowInInspector]
  private readonly SyncList<NetworkBehaviorSyncvar<Airbase>> airbasesUnsorted = new SyncList<NetworkBehaviorSyncvar<Airbase>>();
  private Dictionary<Unit, int> lasedUnits = new Dictionary<Unit, int>();
  public float excessFundsThreshold;
  public float playerJoinAllowance = 20f;
  public float playerTaxRate = 0.2f;
  public float regularIncome = 10f;
  private float bonusIncome;
  public float excessFundsDistributePercent = 0.25f;
  public float killReward = 1f;
  private ThreatTracker aircraftThreatTracker;
  [SyncVar]
  public bool preventJoin;
  [SyncVar]
  public bool preventDonation;
  [SyncVar]
  public float factionScore;
  [SyncVar]
  public List<string> restrictedWeapons = new List<string>();
  [SyncVar]
  public List<string> restrictedAircraft = new List<string>();
  public int warheadsInitial;
  public int warheadsReserve;
  public int reserveAirframes;
  public int extraReservesPerPlayer = 1;
  public int AIAircraftLimit = 6;
  public float reduceAIPerFriendlyPlayer = 1f;
  public float addAIPerEnemyPlayer = 1f;
  public readonly SyncDictionary<AircraftDefinition, FactionHQ.RuntimeSupply> AircraftSupply = new SyncDictionary<AircraftDefinition, FactionHQ.RuntimeSupply>();
  public readonly SyncDictionary<VehicleDefinition, FactionHQ.RuntimeSupply> VehicleSupply = new SyncDictionary<VehicleDefinition, FactionHQ.RuntimeSupply>();
  private Dictionary<Player, FactionHQ.ReserveRequest> reserveRequests = new Dictionary<Player, FactionHQ.ReserveRequest>();
  private List<UnitDefinition> unitsSpawned = new List<UnitDefinition>();
  [SerializeField]
  private List<Aircraft> activeAIAircraft = new List<Aircraft>();
  private List<Missile> activeCruiseMissiles = new List<Missile>();
  private float lastSortedTime = float.MinValue;
  private List<(VehicleDepot depot, float distance)> depotSorted = new List<(VehicleDepot, float)>();
  private List<(Airbase airbase, float distance)> airbasesSorted = new List<(Airbase, float)>();
  [SerializeField]
  private List<Radar> radars = new List<Radar>();
  [SerializeField]
  private List<FireControl> fireControls = new List<FireControl>();
  private List<ExclusionZone> exclusionZones = new List<ExclusionZone>();
  private List<Unit> unitsNeedingRearm = new List<Unit>();
  private List<GlobalPosition> activeDropZones = new List<GlobalPosition>();
  private static List<Player> playersCache = new List<Player>();
  private List<Unit> unitsNeedingRepair = new List<Unit>();
  private readonly List<(Airbase airbase, int warheads)> warheadStockpileCache = new List<(Airbase, int)>();
  private List<TrackingInfo> strategicTargets = new List<TrackingInfo>();
  private bool airbaseNeedSorting;
  private float lastConvoySpawned = -60f;
  [NonSerialized]
  private const int SYNC_VAR_COUNT = 6;
  [NonSerialized]
  private const int RPC_COUNT = 5;

  public float factionFunds
  {
    get => this.\u003CfactionFunds\u003Ek__BackingField;
    private set => this.Network\u003CfactionFunds\u003Ek__BackingField = value;
  }

  public event Action<ExclusionZone> onExclusionZone;

  public event Action onPlayerChangedFaction;

  public event Action<PersistentID> onDiscoverUnit;

  public event Action<PersistentID> onForgetUnit;

  public event Action<Unit> onRegisterUnit;

  public event Action<Unit> onRemoveUnit;

  public event Action<Airbase> onAirbaseAdded;

  public event Action<Airbase> onAirbaseRemoved;

  private void Awake() => this.Identity.OnStartClient.AddListener(new Action(this.OnStartClient));

  public List<Player> GetPlayers(bool sortByScore)
  {
    FactionHQ.playersCache.Clear();
    foreach (PlayerRef factionPlayer in this.factionPlayers)
    {
      Player player = factionPlayer.Player;
      if ((UnityEngine.Object) player != (UnityEngine.Object) null)
        FactionHQ.playersCache.Add(player);
    }
    if (sortByScore)
      FactionHQ.playersCache.Sort(new Comparison<Player>(FactionHQ.SortByScore));
    return FactionHQ.playersCache;
  }

  private static int SortByScore(Player a, Player b) => b.PlayerScore.CompareTo(a.PlayerScore);

  [Mirage.Server]
  public void AddPlayer(Player player)
  {
    if (!this.IsServer)
      throw new MethodInvocationException("[Server] function 'AddPlayer' called when server not active");
    this.factionPlayers.Add(new PlayerRef(player));
    SavedPlayerData saveData = player.GetAuthData().SaveData;
    if ((UnityEngine.Object) saveData.Faction != (UnityEngine.Object) null)
    {
      ColorLog<FactionHQ>.Info($"{player} rejoined {this.faction.factionName}");
      saveData.OnRejoinFaction(player);
    }
    else
    {
      ColorLog<FactionHQ>.Info($"New {player} joined {this.faction.factionName}");
      this.factionFunds -= this.playerJoinAllowance;
      player.AddAllocation(this.playerJoinAllowance);
    }
    UniTask.Void((Func<UniTaskVoid>) (async () =>
    {
      await UniTask.Yield();
      NetworkSceneSingleton<MessageManager>.i.RpcPlayerJoinFactionMessage(player, this);
    }));
  }

  [Mirage.Server]
  public void RemovePlayer(Player player)
  {
    if (!this.IsServer)
      throw new MethodInvocationException("[Server] function 'RemovePlayer' called when server not active");
    this.reserveRequests.Remove(player);
    this.factionPlayers.Remove(new PlayerRef(player));
    if ((double) player.Allocation > 0.0)
      this.factionFunds += player.Allocation;
    player.GetAuthData().SaveData.Save(player);
  }

  [Mirage.Server]
  public void AddAirbase(Airbase airbase)
  {
    if (!this.IsServer)
      throw new MethodInvocationException("[Server] function 'AddAirbase' called when server not active");
    if (this.airbasesUnsorted.Contains((NetworkBehaviorSyncvar<Airbase>) airbase))
      return;
    Debug.Log((object) $"Adding airbase {airbase.name} to {this.gameObject.name}");
    this.airbasesUnsorted.Add((NetworkBehaviorSyncvar<Airbase>) airbase);
    this.airbaseNeedSorting = true;
  }

  [Mirage.Server]
  public void RemoveAirbase(Airbase airbase)
  {
    if (!this.IsServer)
      throw new MethodInvocationException("[Server] function 'RemoveAirbase' called when server not active");
    Debug.Log((object) $"Removing airbase {airbase.name} from {this.gameObject.name}");
    this.airbasesUnsorted.Remove((NetworkBehaviorSyncvar<Airbase>) airbase);
    this.airbaseNeedSorting = true;
  }

  public void AddDepot(VehicleDepot depot) => this.depotSorted.Add((depot, float.MaxValue));

  public void AddExclusionZone(Unit weapon, GlobalPosition position, float radius)
  {
    ExclusionZone exclusionZone = new ExclusionZone(weapon, position, radius);
    this.RpcExclusionZone(exclusionZone);
    Action<ExclusionZone> onExclusionZone = this.onExclusionZone;
    if (onExclusionZone == null)
      return;
    onExclusionZone(exclusionZone);
  }

  [ClientRpc]
  public void RpcExclusionZone(ExclusionZone exclusionZone)
  {
    if (ClientRpcSender.ShouldInvokeLocally((NetworkBehaviour) this, RpcTarget.Observers, (INetworkPlayer) null, false))
      this.UserCode_RpcExclusionZone_1543365140(exclusionZone);
    PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
    GeneratedNetworkCode._Write_NuclearOption\u002EExclusionZone((NetworkWriter) writer, exclusionZone);
    ClientRpcSender.Send((NetworkBehaviour) this, 0, (NetworkWriter) writer, Mirage.Channel.Reliable, false);
    writer.Release();
  }

  private async UniTask RegisterExclusionZone(ExclusionZone exclusionZone)
  {
    FactionHQ hq = this;
    CancellationToken cancel;
    if (DynamicMap.GetFactionMode(hq) == FactionMode.Enemy)
    {
      cancel = new CancellationToken();
    }
    else
    {
      cancel = hq.destroyCancellationToken;
      while (!UnitRegistry.TryGetUnit(new PersistentID?(exclusionZone.sourceId), out Unit _))
      {
        await UniTask.Delay(100);
        if (cancel.IsCancellationRequested)
        {
          cancel = new CancellationToken();
          return;
        }
      }
      hq.exclusionZones.Add(exclusionZone);
      SceneSingleton<DynamicMap>.i.DisplayExclusionZone(exclusionZone);
      SceneSingleton<GameplayUI>.i.GameMessage("Nuclear weapon launched, exclusion zone active!");
      Action<ExclusionZone> onExclusionZone = hq.onExclusionZone;
      if (onExclusionZone != null)
        onExclusionZone(exclusionZone);
      if (DynamicMap.GetFactionMode(hq) != FactionMode.Friendly)
      {
        cancel = new CancellationToken();
      }
      else
      {
        SoundManager.PlayInterfaceOneShot(GameAssets.i.radioStatic);
        cancel = new CancellationToken();
      }
    }
  }

  public List<ExclusionZone> GetExclusionZones()
  {
    for (int index = this.exclusionZones.Count - 1; index >= 0; --index)
    {
      Unit unit;
      if (!UnitRegistry.TryGetUnit(new PersistentID?(this.exclusionZones[index].sourceId), out unit) || (UnityEngine.Object) unit == (UnityEngine.Object) null)
        this.exclusionZones.RemoveAt(index);
    }
    return this.exclusionZones;
  }

  public void ModifyUnitSupply(UnitDefinition unitDefinition, int count)
  {
    switch (unitDefinition)
    {
      case AircraftDefinition aircraftDefinition:
        FactionHQ.RuntimeSupply runtimeSupply1;
        if (this.AircraftSupply.TryGetValue(aircraftDefinition, out runtimeSupply1))
        {
          int count1 = runtimeSupply1.Count + count;
          this.AircraftSupply[aircraftDefinition] = new FactionHQ.RuntimeSupply(count1);
          break;
        }
        this.AircraftSupply[aircraftDefinition] = new FactionHQ.RuntimeSupply(count);
        break;
      case VehicleDefinition vehicleDefinition:
        FactionHQ.RuntimeSupply runtimeSupply2;
        if (this.VehicleSupply.TryGetValue(vehicleDefinition, out runtimeSupply2))
        {
          int count2 = runtimeSupply2.Count + count;
          this.VehicleSupply[vehicleDefinition] = new FactionHQ.RuntimeSupply(count2);
          break;
        }
        this.VehicleSupply[vehicleDefinition] = new FactionHQ.RuntimeSupply(count);
        break;
      default:
        throw new ArgumentException($"UnitDefinition for supply should either be AircraftDefinition or VehicleDefinition but was {unitDefinition?.GetType()}", nameof (unitDefinition));
    }
  }

  public int GetUnitSupply(UnitDefinition unitDefinition)
  {
    int unitSupply = 0;
    switch (unitDefinition)
    {
      case AircraftDefinition key1:
        FactionHQ.RuntimeSupply runtimeSupply1;
        if (this.AircraftSupply.TryGetValue(key1, out runtimeSupply1))
          unitSupply += runtimeSupply1.Count;
        return unitSupply;
      case VehicleDefinition key2:
        FactionHQ.RuntimeSupply runtimeSupply2;
        if (this.VehicleSupply.TryGetValue(key2, out runtimeSupply2))
          unitSupply += runtimeSupply2.Count;
        return unitSupply;
      default:
        throw new ArgumentException($"UnitDefinition for supply should either be AircraftDefinition or VehicleDefinition but was {unitDefinition?.GetType()}", nameof (unitDefinition));
    }
  }

  [Mirage.Server]
  public float CmdGetDelaySpawnConvoy()
  {
    if (!this.IsServer)
      throw new MethodInvocationException("[Server] function 'CmdGetDelaySpawnConvoy' called when server not active");
    return Mathf.Max(60f + this.lastConvoySpawned - Time.timeSinceLevelLoad, 0.0f);
  }

  [Mirage.Server]
  public void AddConvoy(Faction.ConvoyGroup convoy)
  {
    if (!this.IsServer)
      throw new MethodInvocationException("[Server] function 'AddConvoy' called when server not active");
    this.lastConvoySpawned = Time.timeSinceLevelLoad;
    foreach (Faction.ConvoyUnit constituent in convoy.Constituents)
      this.AddSupplyUnit(constituent.Type, constituent.Count);
  }

  [Mirage.Server]
  public void AddSupplyUnit(UnitDefinition unitDefinition, int amount)
  {
    if (!this.IsServer)
      throw new MethodInvocationException("[Server] function 'AddSupplyUnit' called when server not active");
    if (amount > 0 && unitDefinition is AircraftDefinition aircraftDefinition1)
    {
      Player key1 = (Player) null;
      float num = -1f;
      List<Player> playerList = new List<Player>();
      foreach (KeyValuePair<Player, FactionHQ.ReserveRequest> reserveRequest in this.reserveRequests)
      {
        Player key2 = reserveRequest.Key;
        AircraftDefinition aircraftDefinition = reserveRequest.Value.aircraftDefinition;
        if (key2.CanAffordAirframe(aircraftDefinition1))
        {
          playerList.Add(key2);
          key2.RpcReserveNotice(new ReserveNotice(ReserveEvent.cancelledAfford, aircraftDefinition1, false, 0));
        }
        else if ((UnityEngine.Object) key2 != (UnityEngine.Object) null && (UnityEngine.Object) aircraftDefinition == (UnityEngine.Object) aircraftDefinition1 && (double) key2.PlayerScore > (double) num)
        {
          key1 = key2;
          num = key2.PlayerScore;
        }
      }
      if ((UnityEngine.Object) key1 != (UnityEngine.Object) null)
      {
        this.reserveRequests.Remove(key1);
        key1.CreditAirframe(aircraftDefinition1, 1, true);
        key1.RpcReserveNotice(new ReserveNotice(ReserveEvent.granted, aircraftDefinition1, false, 0));
        return;
      }
      foreach (Player key3 in playerList)
        this.reserveRequests.Remove(key3);
    }
    this.ModifyUnitSupply(unitDefinition, amount);
  }

  public int GetWarheadStockpile()
  {
    int warheadStockpile = 0;
    if (this.airbasesUnsorted != null && this.airbasesUnsorted.Count > 0)
    {
      for (int i = 0; i < this.airbasesUnsorted.Count; ++i)
      {
        Airbase airbase = this.airbasesUnsorted[i].Value;
        if (airbase.HasStorage())
          warheadStockpile += airbase.GetWarheads();
      }
    }
    return warheadStockpile;
  }

  public int GetWarheadAvailableForAI() => this.GetWarheadStockpile() - this.warheadsReserve;

  public void AddWarheadStockpile(int amount)
  {
    this.warheadStockpileCache.Clear();
    for (int i = 0; i < this.airbasesUnsorted.Count; ++i)
    {
      Airbase airbase = this.airbasesUnsorted[i].Value;
      if (airbase.HasStorage())
      {
        int num = airbase.GetWarheads() - airbase.GetStorage();
        this.warheadStockpileCache.Add((airbase, num));
      }
    }
    if (this.warheadStockpileCache.Count == 0)
    {
      Debug.Log((object) $"NO STORAGE FACILITY FOUND FOR {amount} WARHEADS - QUIT");
    }
    else
    {
      Debug.Log((object) $"SPLIT {amount} WARHEADS BETWEEN : {this.warheadStockpileCache.Count} AIRBASES");
      if (this.warheadStockpileCache.Count == 1)
      {
        this.warheadStockpileCache[0].airbase.AddWarheads(amount);
      }
      else
      {
        Span<int> span = stackalloc int[this.warheadStockpileCache.Count];
        for (int index = 0; index < this.warheadStockpileCache.Count; ++index)
          span[index] = this.warheadStockpileCache[index].warheads;
        for (; amount > 0; --amount)
        {
          int num1 = int.MaxValue;
          int num2 = 0;
          for (int index = 0; index < span.Length; ++index)
          {
            int num3 = span[index];
            if (num3 < num1)
            {
              num1 = num3;
              num2 = index;
            }
          }
          ++span[num2];
        }
        for (int index = 0; index < span.Length; ++index)
        {
          (Airbase airbase, int warheads) = this.warheadStockpileCache[index];
          int number = span[index] - warheads;
          if (number > 0)
            airbase.AddWarheads(number);
          Debug.Log((object) $"RESULTS : {airbase} RECEIVED {number} NEW WARHEADS, NEW TOTAL {airbase.GetWarheads()} WARHEADS");
        }
      }
    }
  }

  public void AddFunds(float funds) => this.factionFunds += funds;

  public void SetFunds(float newFunds) => this.factionFunds = newFunds;

  public void AddBonusFunds(float funds)
  {
    this.factionFunds += funds;
    this.bonusIncome += funds;
  }

  public void AddScore(float score) => this.SetScore(this.factionScore + score);

  public void SetScore(float score)
  {
    this.NetworkfactionScore = score;
    this.CheckEscalation();
  }

  private void CheckEscalation()
  {
    float oldScore = NetworkSceneSingleton<MissionManager>.i.currentEscalation;
    float newScore = this.factionScore;
    if ((double) newScore <= (double) oldScore)
      return;
    NetworkSceneSingleton<MissionManager>.i.NetworkcurrentEscalation = this.factionScore;
    ReportIfAbove(NetworkSceneSingleton<MissionManager>.i.tacticalThreshold, "Warning: Tactical nuclear weapons are cleared for deployment");
    ReportIfAbove(NetworkSceneSingleton<MissionManager>.i.strategicThreshold, "Warning: Strategic nuclear weapons are cleared for deployment");

    void ReportIfAbove(float threshold, string message)
    {
      if ((double) oldScore >= (double) threshold || (double) newScore < (double) threshold)
        return;
      NetworkSceneSingleton<MessageManager>.i.RpcAllHQMessage(message);
    }
  }

  private void OnStartClient()
  {
    FactionRegistry.RegisterFaction(this.faction, this);
    this.factionUnits.OnInsert += new Action<int, PersistentID>(this.OnUnitsAdded);
    this.factionPlayers.OnChange += new Action(this.OnPlayersChange);
    this.airbasesUnsorted.OnInsert += new Action<int, NetworkBehaviorSyncvar<Airbase>>(this.OnAirbaseAdded);
    this.airbasesUnsorted.OnRemove += new Action<int, NetworkBehaviorSyncvar<Airbase>>(this.OnAirbaseRemoved);
    for (int index = 0; index < this.factionUnits.Count; ++index)
      this.OnUnitsAdded(index, this.factionUnits[index]);
    this.SetupAirbaseFactions();
    if (!this.IsServer)
      return;
    this.ServerSetup();
  }

  private void SetupAirbaseFactions()
  {
    foreach (NetworkBehaviorSyncvar<Airbase> networkBehaviorSyncvar in this.airbasesUnsorted)
    {
      Airbase airbase = networkBehaviorSyncvar.Value;
      if ((UnityEngine.Object) airbase != (UnityEngine.Object) null && !airbase.AttachedAirbase)
        airbase.SetFactionWithoutEvent(this);
    }
    SceneSingleton<DynamicMap>.i.RefreshAirbases();
  }

  public bool ContainsAirbase(Airbase airbase)
  {
    return this.airbasesUnsorted.Contains((NetworkBehaviorSyncvar<Airbase>) airbase);
  }

  private void ServerSetup()
  {
    MissionManager.onMissionLoad += new Action<Mission>(this.OnMissionLoad);
    if (MissionManager.CurrentMission != null)
      this.OnMissionLoad(MissionManager.CurrentMission);
    this.aircraftThreatTracker = new ThreatTracker(this, 0.1f, new TypeIdentity(0.0f, 1f, 0.0f, 0.0f, 0.0f));
    this.StartSlowUpdateDelayed(5f, new Action(this.DeployUnits));
    this.StartSlowUpdateDelayed(30f, new Action(this.DistributeFunds));
    if (this.warheadsInitial <= 0)
      return;
    UniTask.Delay(5000).ContinueWith((Action) (() =>
    {
      if (!((UnityEngine.Object) this != (UnityEngine.Object) null))
        return;
      this.AddWarheadStockpile(this.warheadsInitial);
    })).Forget();
  }

  private void DistributeFunds()
  {
    if ((double) this.factionFunds <= 0.0)
      return;
    float num1 = 0.0f;
    if ((double) this.factionFunds > (double) this.excessFundsThreshold)
      num1 = (this.factionFunds - this.excessFundsThreshold) * Mathf.Clamp01(this.excessFundsDistributePercent);
    float num2 = (float) (((double) this.regularIncome + (double) this.bonusIncome + (double) num1) * (double) this.factionPlayers.Count * 0.5);
    if ((double) num2 <= 0.0)
      return;
    if ((double) num2 > (double) this.factionFunds)
      num2 = this.factionFunds;
    float amount = num2 / (float) Mathf.Max(this.factionPlayers.Count, 1);
    foreach (Player player in this.GetPlayers(false))
      player.AddAllocation(amount);
    this.AddFunds(-num2);
    this.bonusIncome = 0.0f;
  }

  public bool IsReservingAirframe(Player player, AircraftDefinition aircraftDefinition)
  {
    FactionHQ.ReserveRequest reserveRequest;
    return this.reserveRequests.TryGetValue(player, out reserveRequest) && (UnityEngine.Object) reserveRequest.aircraftDefinition == (UnityEngine.Object) aircraftDefinition;
  }

  public bool IsReservingAirframe(Player player)
  {
    return this.reserveRequests.TryGetValue(player, out FactionHQ.ReserveRequest _);
  }

  public void RequestReservedAirframe(Player player, AircraftDefinition aircraftDefinition)
  {
    if (player.OwnsAirframe(aircraftDefinition, true))
      player.RpcReserveNotice(new ReserveNotice(ReserveEvent.rejectedOwned, aircraftDefinition, false, 0));
    else if (player.PlayerRank < aircraftDefinition.aircraftParameters.rankRequired)
      player.RpcReserveNotice(new ReserveNotice(ReserveEvent.rejectedRank, aircraftDefinition, false, 0));
    else if (this.AircraftSupply.ContainsKey(aircraftDefinition) && this.AircraftSupply[aircraftDefinition].Count > 0)
    {
      this.AddSupplyUnit((UnitDefinition) aircraftDefinition, -1);
      player.CreditAirframe(aircraftDefinition, 1, true);
      player.RpcReserveNotice(new ReserveNotice(ReserveEvent.granted, aircraftDefinition, false, 0));
      this.reserveRequests.Remove(player);
    }
    else
    {
      FactionHQ.ReserveRequest reserveRequest1;
      if (this.reserveRequests.TryGetValue(player, out reserveRequest1))
      {
        if ((UnityEngine.Object) reserveRequest1.aircraftDefinition == (UnityEngine.Object) aircraftDefinition)
        {
          player.RpcReserveNotice(new ReserveNotice(ReserveEvent.rejectedDuplicate, aircraftDefinition, true, -1));
          return;
        }
        this.reserveRequests[player] = new FactionHQ.ReserveRequest(aircraftDefinition);
      }
      else
        this.reserveRequests.Add(player, new FactionHQ.ReserveRequest(aircraftDefinition));
      int queuePosition = 1;
      foreach (KeyValuePair<Player, FactionHQ.ReserveRequest> reserveRequest2 in this.reserveRequests)
      {
        if ((UnityEngine.Object) reserveRequest2.Value.aircraftDefinition == (UnityEngine.Object) aircraftDefinition && (double) reserveRequest2.Key.PlayerScore > (double) player.PlayerScore)
          ++queuePosition;
      }
      player.RpcReserveNotice(new ReserveNotice(queuePosition == 1 ? ReserveEvent.accepted : ReserveEvent.acceptedInQueue, aircraftDefinition, true, queuePosition));
    }
  }

  private void DeployUnits()
  {
    if (this.airbaseNeedSorting || (double) Time.timeSinceLevelLoad - (double) this.lastSortedTime > 30.0)
    {
      this.SortAirbases();
      this.SortDepots();
      this.airbaseNeedSorting = false;
      this.lastSortedTime = Time.timeSinceLevelLoad;
    }
    this.DeployAIAircraft();
    this.DeployVehicles();
  }

  private void DeployAIAircraft()
  {
    int count = this.factionPlayers.Count;
    int num1 = 0;
    foreach (FactionHQ allHq in FactionRegistry.GetAllHQs())
    {
      if ((UnityEngine.Object) allHq != (UnityEngine.Object) this)
        num1 += allHq.GetPlayers(false).Count;
    }
    if ((double) this.activeAIAircraft.Count >= (double) this.AIAircraftLimit + (double) num1 * (double) this.addAIPerEnemyPlayer - (double) count * (double) this.reduceAIPerFriendlyPlayer)
      return;
    List<AircraftDefinition> aircraft = Encyclopedia.i.aircraft;
    for (int index1 = 0; index1 < aircraft.Count; ++index1)
    {
      int index2 = UnityEngine.Random.Range(index1, aircraft.Count);
      AircraftDefinition aircraftDefinition = aircraft[index1];
      aircraft[index1] = aircraft[index2];
      aircraft[index2] = aircraftDefinition;
    }
    int num2 = this.reserveAirframes + count * this.extraReservesPerPlayer;
    foreach (AircraftDefinition aircraftDefinition in aircraft)
    {
      FactionHQ.RuntimeSupply runtimeSupply;
      if (this.AircraftSupply.TryGetValue(aircraftDefinition, out runtimeSupply) && runtimeSupply.Count > num2)
      {
        foreach ((Airbase airbase, float _) in this.airbasesSorted)
        {
          if (!((UnityEngine.Object) airbase == (UnityEngine.Object) null) && airbase.CanSpawnAircraft(aircraftDefinition))
          {
            Loadout loadout = (Loadout) null;
            float fuelLevel = aircraftDefinition.aircraftParameters.DefaultFuelLevel;
            StandardLoadout randomStandardLoadout = aircraftDefinition.aircraftParameters.GetRandomStandardLoadout(aircraftDefinition, this);
            if (randomStandardLoadout != null)
            {
              loadout = randomStandardLoadout.loadout;
              fuelLevel = randomStandardLoadout.FuelRatio;
            }
            int liveryForFaction = aircraftDefinition.aircraftParameters.GetRandomLiveryForFaction(this.faction);
            if (airbase.TrySpawnAircraft((Player) null, aircraftDefinition, new LiveryKey(liveryForFaction), loadout, fuelLevel).Allowed)
              return;
          }
        }
      }
    }
  }

  private void DeployVehicles()
  {
    for (int index = this.depotSorted.Count - 1; index >= 0; --index)
    {
      if ((UnityEngine.Object) this.depotSorted[index].depot == (UnityEngine.Object) null || this.depotSorted[index].depot.disabled)
        this.depotSorted.RemoveAt(index);
    }
    this.unitsSpawned.Clear();
    foreach (KeyValuePair<VehicleDefinition, FactionHQ.RuntimeSupply> keyValuePair in this.VehicleSupply)
    {
      if (keyValuePair.Value.Count > 0)
      {
        VehicleDefinition key = keyValuePair.Key;
        foreach ((VehicleDepot depot, float distance) tuple in this.depotSorted)
        {
          if (tuple.depot.TrySpawnVehicle(key))
          {
            this.unitsSpawned.Add((UnitDefinition) key);
            break;
          }
        }
      }
    }
    foreach (UnitDefinition unitDefinition in this.unitsSpawned)
      this.ModifyUnitSupply(unitDefinition, -1);
  }

  private void OnMissionLoad(Mission mission)
  {
    MissionFaction missionFaction;
    mission.GetFactionFromHq(this, out missionFaction);
    this.NetworkpreventJoin = missionFaction.preventJoin;
    this.NetworkpreventDonation = missionFaction.preventDonation;
    this.factionFunds = missionFaction.startingBalance;
    this.excessFundsThreshold = missionFaction.startingBalance;
    this.playerJoinAllowance = missionFaction.playerJoinAllowance;
    this.playerTaxRate = missionFaction.playerTaxRate;
    this.regularIncome = missionFaction.regularIncome;
    this.excessFundsDistributePercent = missionFaction.excessFundsDistributePercent;
    this.killReward = missionFaction.killReward;
    this.warheadsInitial = missionFaction.startingWarheads;
    this.warheadsReserve = missionFaction.reserveWarheads;
    this.reserveAirframes = missionFaction.reserveAirframes;
    this.extraReservesPerPlayer = missionFaction.extraReservesPerPlayer;
    this.AIAircraftLimit = missionFaction.AIAircraftLimit;
    this.reduceAIPerFriendlyPlayer = missionFaction.reduceAIPerFriendlyPlayer;
    this.addAIPerEnemyPlayer = missionFaction.addAIPerEnemyPlayer;
    this.AircraftSupply.Clear();
    this.VehicleSupply.Clear();
    this.restrictedWeapons.Clear();
    this.restrictedAircraft.Clear();
    foreach (FactionSupply supply in missionFaction.supplies)
    {
      UnitDefinition unitDefinition;
      if (Encyclopedia.Lookup.TryGetValue(supply.unitType, out unitDefinition))
        this.ModifyUnitSupply(unitDefinition, supply.count);
    }
    foreach (string weapon in missionFaction.restrictions.weapons)
      this.restrictedWeapons.Add(weapon);
    foreach (string str in missionFaction.restrictions.aircraft)
      this.restrictedAircraft.Add(str);
  }

  private void OnDestroy()
  {
    MissionManager.onMissionLoad -= new Action<Mission>(this.OnMissionLoad);
  }

  [Mirage.Server]
  public void RequestTrackingStates(Player requestingPlayer)
  {
    if (!this.IsServer)
      throw new MethodInvocationException("[Server] function 'RequestTrackingStates' called when server not active");
    if (this.trackingDatabase.Count == 0)
      return;
    using (PooledNetworkWriter writer = NetworkWriterPool.GetWriter())
    {
      foreach (KeyValuePair<PersistentID, TrackingInfo> keyValuePair in this.trackingDatabase)
      {
        if (UnitRegistry.TryGetUnit(new PersistentID?(keyValuePair.Key), out Unit _))
        {
          TrackingInfo trackingInfo = keyValuePair.Value;
          writer.Write<PersistentID>(trackingInfo.id);
          writer.Write<GlobalPosition>(trackingInfo.lastKnownPosition);
          writer.Write<float>(trackingInfo.lastSpottedTime);
        }
      }
      this.RpcGetTrackingStateBatched(requestingPlayer.Owner, writer.ToArraySegment());
    }
  }

  [ClientRpc(target = RpcTarget.Player)]
  private void RpcGetTrackingStateBatched(INetworkPlayer _, ArraySegment<byte> segment)
  {
    if (ClientRpcSender.ShouldInvokeLocally((NetworkBehaviour) this, RpcTarget.Player, _, false))
    {
      this.UserCode_RpcGetTrackingStateBatched_\u002D1045112216(this.Client.Player, segment);
    }
    else
    {
      PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
      writer.WriteBytesAndSizeSegment(segment);
      ClientRpcSender.SendTarget((NetworkBehaviour) this, 1, (NetworkWriter) writer, Mirage.Channel.Reliable, _);
      writer.Release();
    }
  }

  private void SetTrackingState(
    PersistentID id,
    GlobalPosition lastKnownPosition,
    float lastSpottedTime)
  {
    Unit unit;
    if (!UnitRegistry.TryGetUnit(new PersistentID?(id), out unit) || this.trackingDatabase.ContainsKey(id))
      return;
    this.trackingDatabase.Add(id, new TrackingInfo(id, lastKnownPosition, lastSpottedTime));
    unit.onDisableUnit += new Action<Unit>(this.DeregisterTrackedUnit);
    unit.onChangeFaction += new Action<Unit>(this.HQ_OnUnitChangeFaction);
    if (!DynamicMap.IsFactionMode(this, FactionMode.Spectator | FactionMode.Friendly))
      return;
    SceneSingleton<DynamicMap>.i.AddIcon(id);
  }

  public void UpdateLasedState(Unit unit, bool lased)
  {
    int num1;
    if (this.lasedUnits.TryGetValue(unit, out num1))
    {
      int num2 = num1 + (lased ? 1 : -1);
      this.lasedUnits[unit] = num2;
      if (num2 > 0)
        return;
      this.lasedUnits.Remove(unit);
    }
    else
    {
      if (!lased)
        return;
      this.lasedUnits.Add(unit, 1);
    }
  }

  public bool IsTargetLased(Unit target) => this.lasedUnits.TryGetValue(target, out int _);

  public bool TryGetLasedTargetInView(
    Transform fromTransform,
    float fov,
    float maxRange,
    out Unit lasedTarget)
  {
    lasedTarget = (Unit) null;
    foreach (KeyValuePair<Unit, int> lasedUnit in this.lasedUnits)
    {
      Unit key = lasedUnit.Key;
      Vector3 to = key.transform.position - fromTransform.position;
      float num = Vector3.Angle(fromTransform.forward, to);
      if ((double) num < (double) fov && FastMath.InRange(key.transform.position, fromTransform.position, maxRange) && key.LineOfSight(fromTransform.position, 1000f))
      {
        lasedTarget = key;
        fov = num;
      }
    }
    return (UnityEngine.Object) lasedTarget != (UnityEngine.Object) null;
  }

  public void RegisterFactionUnit(Unit unit)
  {
    if (!this.factionUnits.Contains(unit.persistentID))
      this.factionUnits.Add(unit.persistentID);
    if (unit is Aircraft aircraft && (UnityEngine.Object) aircraft.Player == (UnityEngine.Object) null)
      this.activeAIAircraft.Add(aircraft);
    if (unit is IRadarReturn)
      this.factionRadarReturn.Add(unit.persistentID);
    Action<Unit> onRegisterUnit = this.onRegisterUnit;
    if (onRegisterUnit == null)
      return;
    onRegisterUnit(unit);
  }

  public void RemoveFactionUnit(Unit unit)
  {
    this.factionUnits.Remove(unit.persistentID);
    if (unit is Aircraft aircraft)
      this.activeAIAircraft.Remove(aircraft);
    if (unit is IRadarReturn)
      this.factionRadarReturn.Remove(unit.persistentID);
    Action<Unit> onRemoveUnit = this.onRemoveUnit;
    if (onRemoveUnit == null)
      return;
    onRemoveUnit(unit);
  }

  public void RegisterCruiseMissile(Missile cruiseMissile)
  {
    this.activeCruiseMissiles.Add(cruiseMissile);
  }

  public void DeregisterCruiseMissile(Missile cruiseMissile)
  {
    this.activeCruiseMissiles.Remove(cruiseMissile);
  }

  public void RegisterRadar(Radar radar) => this.radars.Add(radar);

  public void DeregisterRadar(Radar radar) => this.radars.Remove(radar);

  public bool TryGetRadar(GlobalPosition searchPosition, float maxRange, out Radar nearestRadar)
  {
    nearestRadar = (Radar) null;
    float range = maxRange;
    foreach (Radar radar in this.radars)
    {
      if (FastMath.InRange(searchPosition, radar.transform.GlobalPosition(), range))
      {
        nearestRadar = radar;
        range = FastMath.Distance(searchPosition, radar.transform.GlobalPosition());
      }
    }
    return (UnityEngine.Object) nearestRadar != (UnityEngine.Object) null;
  }

  public void RegisterFireControl(FireControl fireControl) => this.fireControls.Add(fireControl);

  public void DeregisterFireControl(FireControl fireControl)
  {
    this.fireControls.Remove(fireControl);
  }

  public bool TryGetFireControl(
    GlobalPosition searchPosition,
    float maxRange,
    out FireControl nearestFireControl)
  {
    nearestFireControl = (FireControl) null;
    float range = maxRange;
    foreach (FireControl fireControl in this.fireControls)
    {
      if (FastMath.InRange(searchPosition, fireControl.transform.GlobalPosition(), range))
      {
        nearestFireControl = fireControl;
        range = FastMath.Distance(searchPosition, fireControl.transform.GlobalPosition());
      }
    }
    return (UnityEngine.Object) nearestFireControl != (UnityEngine.Object) null;
  }

  public List<Missile> GetCruiseMissiles() => this.activeCruiseMissiles;

  private void OnUnitsAdded(int index, PersistentID value)
  {
    this.DisplayFactionUnit(value);
    Unit unit;
    if (!NetworkManagerNuclearOption.i.Server.Active || !UnitRegistry.TryGetUnit(new PersistentID?(value), out unit))
      return;
    Action<Unit> onRegisterUnit = this.onRegisterUnit;
    if (onRegisterUnit == null)
      return;
    onRegisterUnit(unit);
  }

  private void OnAirbaseAdded(int index, NetworkBehaviorSyncvar<Airbase> value)
  {
    Airbase airbase = value.Value;
    airbase.SetFactionWithoutEvent(this);
    SceneSingleton<DynamicMap>.i.RefreshAirbases();
    if ((double) Time.timeSinceLevelLoad > 10.0)
    {
      SoundManager.PlayInterfaceOneShot(GameAssets.i.radioStatic);
      if ((double) Time.timeSinceLevelLoad > 10.0 && !airbase.AttachedAirbase)
      {
        string displayName = airbase.SavedAirbase.DisplayName;
        SceneSingleton<GameplayUI>.i.GameMessage($"{displayName} has been captured by {this.faction.factionExtendedName}");
      }
    }
    Action<Airbase> onAirbaseAdded = this.onAirbaseAdded;
    if (onAirbaseAdded == null)
      return;
    onAirbaseAdded(airbase);
  }

  private void OnAirbaseRemoved(int index, NetworkBehaviorSyncvar<Airbase> value)
  {
    SceneSingleton<DynamicMap>.i.RefreshAirbases();
    Action<Airbase> onAirbaseRemoved = this.onAirbaseRemoved;
    if (onAirbaseRemoved == null)
      return;
    onAirbaseRemoved(value.Value);
  }

  private void OnPlayersChange()
  {
    Action playerChangedFaction = this.onPlayerChangedFaction;
    if (playerChangedFaction == null)
      return;
    playerChangedFaction();
  }

  [ServerRpc(requireAuthority = false)]
  public void CmdUpdateTrackingInfo(PersistentID id)
  {
    if (ServerRpcSender.ShouldInvokeLocally((NetworkBehaviour) this, false, false))
    {
      this.UserCode_CmdUpdateTrackingInfo_\u002D1717917610(id);
    }
    else
    {
      PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
      GeneratedNetworkCode._Write_PersistentID((NetworkWriter) writer, id);
      ServerRpcSender.Send((NetworkBehaviour) this, 2, (NetworkWriter) writer, Mirage.Channel.Reliable, false);
      writer.Release();
    }
  }

  [ClientRpc]
  public void RpcUpdateTrackingInfo(PersistentID id)
  {
    if (ClientRpcSender.ShouldInvokeLocally((NetworkBehaviour) this, RpcTarget.Observers, (INetworkPlayer) null, false))
      this.UserCode_RpcUpdateTrackingInfo_\u002D640322303(id);
    PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
    GeneratedNetworkCode._Write_PersistentID((NetworkWriter) writer, id);
    ClientRpcSender.Send((NetworkBehaviour) this, 3, (NetworkWriter) writer, Mirage.Channel.Reliable, false);
    writer.Release();
  }

  public bool TryGetStrategicTargets(
    List<TrackingInfo> targetList,
    int ammo,
    float range,
    Unit requestingUnit)
  {
    GlobalPosition a = requestingUnit.GlobalPosition();
    if (this.strategicTargets.Count == 0)
      return false;
    targetList.Clear();
    for (int index = 0; index < this.strategicTargets.Count && index < ammo; ++index)
    {
      if (!FastMath.OutOfRange(a, this.strategicTargets[index].lastKnownPosition, range))
        targetList.Add(this.strategicTargets[index]);
    }
    return targetList.Count > 0;
  }

  private void HQ_OnUnitChangeFaction(Unit unit)
  {
    if (!((UnityEngine.Object) unit.NetworkHQ == (UnityEngine.Object) this))
      return;
    Action<PersistentID> onForgetUnit = this.onForgetUnit;
    if (onForgetUnit != null)
      onForgetUnit(unit.persistentID);
    this.trackingDatabase.Remove(unit.persistentID);
    TrackingInfo trackingData = this.GetTrackingData(unit.persistentID);
    if (trackingData == null || !this.strategicTargets.Contains(trackingData))
      return;
    this.strategicTargets.Remove(trackingData);
  }

  public void DeclareEndGame(EndType endType) => this.RpcDeclareEndGame(endType);

  [ClientRpc]
  public void RpcDeclareEndGame(EndType endType)
  {
    if (ClientRpcSender.ShouldInvokeLocally((NetworkBehaviour) this, RpcTarget.Observers, (INetworkPlayer) null, false))
      this.UserCode_RpcDeclareEndGame_2091801907(endType);
    PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
    GeneratedNetworkCode._Write_NuclearOption\u002ESavedMission\u002EObjectiveV2\u002EOutcomes\u002EEndType((NetworkWriter) writer, endType);
    ClientRpcSender.Send((NetworkBehaviour) this, 4, (NetworkWriter) writer, Mirage.Channel.Reliable, false);
    writer.Release();
  }

  public void DisplayFactionUnit(PersistentID id)
  {
    if (!DynamicMap.IsFactionMode(this, FactionMode.Spectator | FactionMode.Friendly))
      return;
    SceneSingleton<DynamicMap>.i.AddIcon(id);
  }

  public TrackingInfo GetTrackingData(PersistentID id)
  {
    TrackingInfo trackingInfo;
    return !this.trackingDatabase.TryGetValue(id, out trackingInfo) ? (TrackingInfo) null : trackingInfo;
  }

  public bool IsStrategicTarget(TrackingInfo trackingInfo)
  {
    return this.strategicTargets.Contains(trackingInfo);
  }

  public List<Unit> GetTargetsWithinCone(
    List<Unit> targetList,
    Transform fromTransform,
    FiringCone[] firingCones,
    bool requireLineOfSight)
  {
    targetList.Clear();
    GlobalPosition globalPosition = fromTransform.GlobalPosition();
    foreach (KeyValuePair<PersistentID, TrackingInfo> keyValuePair in this.trackingDatabase)
    {
      TrackingInfo trackingInfo = keyValuePair.Value;
      GlobalPosition position = trackingInfo.GetPosition();
      Unit unit;
      if (FiringConeChecker.VectorWithinFiringCones(firingCones, position - globalPosition, out Vector3 _) && trackingInfo.TryGetUnit(out unit) && this.IsTargetPositionAccurate(unit, 10f) && (!requireLineOfSight || unit.LineOfSight(fromTransform.position, 1000f)))
        targetList.Add(unit);
    }
    return targetList;
  }

  public List<TrackingInfo> GetTargetsWithinRange(
    List<TrackingInfo> targetList,
    Transform fromTransform,
    float range,
    bool requireLineOfSight)
  {
    targetList.Clear();
    GlobalPosition a = fromTransform.GlobalPosition();
    foreach (KeyValuePair<PersistentID, TrackingInfo> keyValuePair in this.trackingDatabase)
    {
      TrackingInfo trackingInfo = keyValuePair.Value;
      Unit unit;
      if (trackingInfo.TryGetUnit(out unit) && this.IsTargetPositionAccurate(unit, 100f) && !((UnityEngine.Object) unit.NetworkHQ == (UnityEngine.Object) null) && !((UnityEngine.Object) unit.NetworkHQ == (UnityEngine.Object) this) && FastMath.InRange(a, unit.GlobalPosition(), range) && (!requireLineOfSight || unit.LineOfSight(fromTransform.position, 1000f)))
        targetList.Add(trackingInfo);
    }
    return targetList;
  }

  public bool GetNearestGroundEnemy(GlobalPosition from, out TrackingInfo nearestUnit)
  {
    nearestUnit = (TrackingInfo) null;
    float num1 = float.MaxValue;
    foreach (KeyValuePair<PersistentID, TrackingInfo> keyValuePair in this.trackingDatabase)
    {
      TrackingInfo trackingInfo = keyValuePair.Value;
      Unit unit;
      if (trackingInfo.TryGetUnit(out unit) && (unit is GroundVehicle || unit is Building))
      {
        float num2 = FastMath.SquareDistance(trackingInfo.lastKnownPosition, from);
        if ((double) num2 < (double) num1 && (UnityEngine.Object) unit.NetworkHQ != (UnityEngine.Object) null)
        {
          nearestUnit = trackingInfo;
          num1 = num2;
        }
      }
    }
    return nearestUnit != null;
  }

  public bool TryGetKnownPosition(Unit unit, out GlobalPosition knownPosition)
  {
    GlobalPosition? knownPosition1 = this.GetKnownPosition(unit);
    knownPosition = knownPosition1.GetValueOrDefault();
    return knownPosition1.HasValue;
  }

  public GlobalPosition? GetKnownPosition(Unit unit)
  {
    if ((UnityEngine.Object) unit.NetworkHQ == (UnityEngine.Object) this)
      return new GlobalPosition?(unit.GlobalPosition());
    TrackingInfo trackingInfo;
    return this.trackingDatabase.TryGetValue(unit.persistentID, out trackingInfo) ? new GlobalPosition?(trackingInfo.GetPosition()) : new GlobalPosition?();
  }

  public void DeregisterTrackedUnit(Unit unit)
  {
    PersistentID persistentId = unit.persistentID;
    if (!this.trackingDatabase.ContainsKey(persistentId))
      return;
    Action<PersistentID> onForgetUnit = this.onForgetUnit;
    if (onForgetUnit != null)
      onForgetUnit(persistentId);
    TrackingInfo trackingInfo = this.trackingDatabase[persistentId];
    if (this.strategicTargets.Contains(trackingInfo))
      this.strategicTargets.Remove(trackingInfo);
    this.trackingDatabase.Remove(persistentId);
  }

  public bool IsTargetBeingTracked(Unit target)
  {
    PersistentID persistentId = target.persistentID;
    if ((UnityEngine.Object) target.NetworkHQ == (UnityEngine.Object) this)
      return true;
    TrackingInfo trackingData = this.GetTrackingData(persistentId);
    return trackingData != null && (double) Time.timeSinceLevelLoad - (double) trackingData.lastSpottedTime <= 4.0;
  }

  public bool IsTargetPositionAccurate(Unit target, float threshold)
  {
    PersistentID persistentId = target.persistentID;
    if ((UnityEngine.Object) target.NetworkHQ == (UnityEngine.Object) this)
      return true;
    TrackingInfo trackingData = this.GetTrackingData(persistentId);
    if (trackingData == null)
      return false;
    if ((double) Time.timeSinceLevelLoad - (double) trackingData.lastSpottedTime < 4.0)
      return true;
    GlobalPosition knownPosition;
    return this.TryGetKnownPosition(target, out knownPosition) && FastMath.InRange(target.GlobalPosition(), knownPosition, threshold);
  }

  public void RegisterDropZone(GlobalPosition position) => this.activeDropZones.Add(position);

  public void DeregisterDropZone(GlobalPosition position)
  {
    for (int index = this.activeDropZones.Count - 1; index >= 0; --index)
    {
      if ((double) FastMath.SquareDistance(this.activeDropZones[index], position) < 625.0)
        this.activeDropZones.RemoveAt(index);
    }
  }

  public bool IsDropZoneClear(GlobalPosition position)
  {
    foreach (GlobalPosition activeDropZone in this.activeDropZones)
    {
      if ((double) FastMath.SquareDistance(position, activeDropZone) < 2500.0)
        return false;
    }
    return true;
  }

  public void NotifyNeedsRearm(Unit unit)
  {
    if (this.unitsNeedingRearm.Contains(unit))
      return;
    this.unitsNeedingRearm.Add(unit);
  }

  public void NotifyRearmed(Unit unit) => this.unitsNeedingRearm.Remove(unit);

  public bool GetUnitNeedingRearming(bool ships, bool vehicles, out Unit lowestAmmoUnit)
  {
    lowestAmmoUnit = (Unit) null;
    float num = 0.0f;
    for (int index = this.unitsNeedingRearm.Count - 1; index >= 0; --index)
    {
      Unit unit = this.unitsNeedingRearm[index];
      if ((UnityEngine.Object) unit == (UnityEngine.Object) null || unit.disabled || (double) unit.radarAlt > 10.0)
        this.unitsNeedingRearm.RemoveAt(index);
      else if (!(unit is Aircraft) && (ships || !(unit is Ship)) && (vehicles || !(unit is GroundVehicle)))
      {
        float missing = unit.GetAmmoValue().Missing;
        if ((double) missing > (double) num)
        {
          lowestAmmoUnit = unit;
          num = missing;
        }
      }
    }
    return (UnityEngine.Object) lowestAmmoUnit != (UnityEngine.Object) null;
  }

  public bool GetListUnitsNeedingRearm(out List<Unit> listUnitsNeedingRearm)
  {
    listUnitsNeedingRearm = this.unitsNeedingRearm;
    return listUnitsNeedingRearm != null && listUnitsNeedingRearm.Count > 0;
  }

  public void NotifyNeedsRepair(Unit unit)
  {
    if (this.unitsNeedingRepair.Contains(unit))
      return;
    this.unitsNeedingRepair.Add(unit);
  }

  public void NotifyRepaired(Unit unit) => this.unitsNeedingRepair.Remove(unit);

  public bool TryGetUnitNeedingRepair(Unit repairerUnit, float range, out Unit priorityUnit)
  {
    priorityUnit = (Unit) null;
    GlobalPosition repairerPosition = repairerUnit.GlobalPosition();
    float num = 0.0f;
    foreach (Unit unit in this.unitsNeedingRepair)
    {
      float repairPriority = (unit as IRepairable).GetRepairPriority(repairerPosition, range);
      if ((double) repairPriority > (double) num)
      {
        num = repairPriority;
        priorityUnit = unit;
      }
    }
    return (UnityEngine.Object) priorityUnit != (UnityEngine.Object) null;
  }

  public IEnumerable<Airbase> GetAirbases()
  {
    foreach (NetworkBehaviorSyncvar<Airbase> networkBehaviorSyncvar in this.airbasesUnsorted)
    {
      Airbase airbase = networkBehaviorSyncvar.Value;
      if ((UnityEngine.Object) airbase != (UnityEngine.Object) null)
        yield return airbase;
    }
  }

  private void SortAirbases()
  {
    this.airbasesSorted.Clear();
    if (!MissionManager.IsRunning || this.airbasesUnsorted.Count == 0)
      return;
    if (MissionPosition.HasObjectiveWithPosition(this))
    {
      foreach (NetworkBehaviorSyncvar<Airbase> networkBehaviorSyncvar in this.airbasesUnsorted)
      {
        Airbase airbase = networkBehaviorSyncvar.Value;
        if (!((UnityEngine.Object) airbase == (UnityEngine.Object) null))
        {
          float distance = this.GetDistance(airbase.center);
          this.airbasesSorted.Add((airbase, distance));
        }
      }
      this.airbasesSorted.Sort((Comparison<(Airbase, float)>) ((a, b) => a.distance.CompareTo(b.distance)));
      foreach ((Airbase airbase, float distance) tuple in this.airbasesSorted)
        ;
    }
    else
    {
      foreach (NetworkBehaviorSyncvar<Airbase> networkBehaviorSyncvar in this.airbasesUnsorted)
      {
        Airbase airbase = networkBehaviorSyncvar.Value;
        if (!((UnityEngine.Object) airbase == (UnityEngine.Object) null))
          this.airbasesSorted.Add((airbase, 0.0f));
      }
    }
  }

  private void SortDepots()
  {
    if (!MissionManager.IsRunning || this.depotSorted.Count == 0 || !MissionPosition.HasObjectiveWithPosition(this))
      return;
    for (int index = 0; index < this.depotSorted.Count; ++index)
    {
      float distance = this.GetDistance(this.depotSorted[index].depot.transform);
      this.depotSorted[index] = (this.depotSorted[index].depot, distance);
    }
    this.depotSorted.Sort((Comparison<(VehicleDepot, float)>) ((a, b) => a.distance.CompareTo(b.distance)));
  }

  private float GetDistance(Transform from)
  {
    float distance;
    return !MissionPosition.TryGetClosestDistance(this, from, out distance) ? float.MaxValue : distance;
  }

  public float GetAircraftThreat(PersistentID id) => this.aircraftThreatTracker.GetThreat(id);

  public Airbase GetNearestAirbase(Vector3 fromPosition, RunwayQuery query)
  {
    Airbase nearestAirbase;
    return !this.TryGetNearestAirbase(fromPosition, float.MaxValue, out nearestAirbase, query) ? (Airbase) null : nearestAirbase;
  }

  public Airbase GetNearestAirbase(Vector3 fromPosition, float range = 3.40282347E+38f, RunwayQuery query = default (RunwayQuery))
  {
    Airbase nearestAirbase;
    return !this.TryGetNearestAirbase(fromPosition, range, out nearestAirbase, query) ? (Airbase) null : nearestAirbase;
  }

  public bool TryGetNearestAirbase(
    Vector3 fromPosition,
    out Airbase nearestAirbase,
    RunwayQuery query = default (RunwayQuery))
  {
    return this.TryGetNearestAirbase(fromPosition, float.MaxValue, out nearestAirbase, query);
  }

  public bool TryGetNearestAirbase(
    Vector3 fromPosition,
    float range,
    out Airbase nearestAirbase,
    RunwayQuery query = default (RunwayQuery))
  {
    bool nearestAirbase1 = false;
    nearestAirbase = (Airbase) null;
    float num1 = range * range;
    foreach (NetworkBehaviorSyncvar<Airbase> networkBehaviorSyncvar in this.airbasesUnsorted)
    {
      Airbase airbase = networkBehaviorSyncvar.Value;
      if (!((UnityEngine.Object) airbase == (UnityEngine.Object) null) && airbase.IsSuitable(query))
      {
        float num2 = FastMath.SquareDistance(fromPosition, airbase.center.position);
        if ((double) num2 < (double) num1)
        {
          num1 = num2;
          nearestAirbase = airbase;
          nearestAirbase1 = true;
        }
      }
    }
    return nearestAirbase1;
  }

  public bool AnyNearAirbase(Vector3 fromPosition, out Airbase airbase)
  {
    airbase = (Airbase) null;
    foreach (NetworkBehaviorSyncvar<Airbase> networkBehaviorSyncvar in this.airbasesUnsorted)
    {
      Airbase airbase1 = networkBehaviorSyncvar.Value;
      if (!((UnityEngine.Object) airbase1 == (UnityEngine.Object) null))
      {
        float radius = airbase1.GetRadius();
        if (FastMath.InRange(fromPosition, airbase1.center.position, radius))
        {
          airbase = airbase1;
          return true;
        }
      }
    }
    return false;
  }

  public bool TryGetNearestShip(
    GlobalPosition fromPosition,
    out Ship nearestShip,
    out float nearestDistance)
  {
    nearestShip = (Ship) null;
    nearestDistance = float.MaxValue;
    foreach (PersistentID factionUnit in this.factionUnits)
    {
      Unit unit1;
      if (UnitRegistry.TryGetUnit(new PersistentID?(factionUnit), out unit1) && unit1 is Ship unit2)
      {
        float num = FastMath.SquareDistance(unit2.GlobalPosition(), fromPosition);
        if ((double) num < (double) nearestDistance)
        {
          nearestShip = unit2;
          nearestDistance = num;
        }
      }
    }
    return (UnityEngine.Object) nearestShip != (UnityEngine.Object) null;
  }

  public bool TryGetNearestUnitStorage(
    Unit fromUnit,
    bool requireStoredUnits,
    out UnitStorage nearestUnitStorage,
    out float nearestDistance)
  {
    nearestUnitStorage = (UnitStorage) null;
    nearestDistance = float.MaxValue;
    foreach (PersistentID factionUnit in this.factionUnits)
    {
      Unit unit1;
      if (UnitRegistry.TryGetUnit(new PersistentID?(factionUnit), out unit1) && unit1 is Ship unit2 && (UnityEngine.Object) unit2 != (UnityEngine.Object) fromUnit)
      {
        float num = FastMath.SquareDistance(unit2.GlobalPosition(), fromUnit.GlobalPosition());
        UnitStorage component;
        if ((double) num < (double) nearestDistance && unit2.gameObject.TryGetComponent<UnitStorage>(out component) && component.CanFit(fromUnit.definition) && (!requireStoredUnits || component.HasUnits()))
        {
          nearestUnitStorage = component;
          nearestDistance = num;
        }
      }
    }
    return (UnityEngine.Object) nearestUnitStorage != (UnityEngine.Object) null;
  }

  public VehicleDepot GetNearestDepot(Vector3 fromPosition)
  {
    VehicleDepot nearestDepot;
    return !this.TryGetNearestDepot(fromPosition, float.MaxValue, out nearestDepot) ? (VehicleDepot) null : nearestDepot;
  }

  public VehicleDepot GetNearestDepot(Vector3 fromPosition, float range = 3.40282347E+38f)
  {
    VehicleDepot nearestDepot;
    return !this.TryGetNearestDepot(fromPosition, range, out nearestDepot) ? (VehicleDepot) null : nearestDepot;
  }

  public bool TryGetNearestDepot(Vector3 fromPosition, float range, out VehicleDepot nearestDepot)
  {
    bool nearestDepot1 = false;
    nearestDepot = (VehicleDepot) null;
    float num1 = range * range;
    foreach ((VehicleDepot depot, float _) in this.depotSorted)
    {
      if (!((UnityEngine.Object) depot == (UnityEngine.Object) null))
      {
        float num2 = FastMath.SquareDistance(fromPosition, depot.transform.position);
        if ((double) num2 < (double) num1)
        {
          num1 = num2;
          nearestDepot = depot;
          nearestDepot1 = true;
        }
      }
    }
    return nearestDepot1;
  }

  private void Update()
  {
    if (!this.IsServer || GameManager.gameState == GameState.Editor)
      return;
    this.aircraftThreatTracker.CheckThreats();
  }

  public void ReportKillAction(Player player, Unit target, float factor)
  {
    float rewardScore = factor * Mathf.Sqrt(target.definition.value);
    float rewardAllocation = this.killReward * factor * Mathf.Sqrt(target.definition.value);
    this.RewardPlayer(player, target, rewardAllocation, rewardScore, FactionHQ.RewardType.Kill);
  }

  public void ReportReconAction(Player player, float totalValue)
  {
    this.RewardPlayer(player, (Unit) null, totalValue, totalValue, FactionHQ.RewardType.Recon);
  }

  public void ReportJammingAction(Player player, Unit target, float totalJamValue)
  {
    this.RewardPlayer(player, target, totalJamValue, totalJamValue, FactionHQ.RewardType.Jamming);
  }

  public void ReportSupplyAction(Player player, Unit target, float refillValue, bool singleUse)
  {
    float num = Mathf.Sqrt(refillValue);
    this.RewardPlayer(player, target, num, num, FactionHQ.RewardType.Supply);
  }

  public void ReportRefuelAction(Player player, Unit target, float refillValue, bool singleUse)
  {
    float num = Mathf.Sqrt(refillValue);
    this.RewardPlayer(player, target, num, num, FactionHQ.RewardType.Refuel);
  }

  public void ReportRescuePilotsAction(Player player, PilotDismounted pilotDismounted)
  {
    float rewardScore = (float) (2.0 * (1.0 + (double) pilotDismounted.GetPilotRank()));
    float rewardAllocation = (float) (2.0 * (1.0 + (double) pilotDismounted.GetPilotRank()));
    this.RewardPlayer(player, (Unit) pilotDismounted, rewardAllocation, rewardScore, FactionHQ.RewardType.RescuePilots);
  }

  public void ReportCapturePilotsAction(Player player, PilotDismounted pilotDismounted)
  {
    float rewardScore = (float) (3.0 * (1.0 + (double) pilotDismounted.GetPilotRank()));
    float rewardAllocation = (float) (3.0 * (1.0 + (double) pilotDismounted.GetPilotRank()));
    this.RewardPlayer(player, (Unit) pilotDismounted, rewardAllocation, rewardScore, FactionHQ.RewardType.CapturePilots);
  }

  public void ReportCaptureLocationAction(Player player, Airbase capturedAirbase, float value)
  {
    float rewardScore = value;
    float rewardAllocation = value;
    this.RewardPlayer(player, (Unit) null, rewardAllocation, rewardScore, FactionHQ.RewardType.CaptureLocation);
  }

  public void ReportRepairAction(Player player, float fundsAwarded, float pointsAwarded)
  {
    this.RewardPlayer(player, (Unit) null, fundsAwarded * this.killReward, pointsAwarded, FactionHQ.RewardType.Repair);
  }

  [Mirage.Server]
  public void RewardPlayer(
    Player player,
    Unit target,
    float rewardAllocation,
    float rewardScore,
    FactionHQ.RewardType missionType)
  {
    if (!this.IsServer)
      throw new MethodInvocationException("[Server] function 'RewardPlayer' called when server not active");
    player.AddAllocation(rewardAllocation * (1f - this.playerTaxRate));
    player.AddScore(rewardScore);
    if ((UnityEngine.Object) player.Aircraft != (UnityEngine.Object) null && !player.Aircraft.disabled)
    {
      Aircraft aircraft = player.Aircraft;
      aircraft.NetworksortieScore = aircraft.sortieScore + Mathf.Max(rewardAllocation, rewardScore);
    }
    Debug.Log((object) $"{player.PlayerName} received ${rewardAllocation:F2} and {rewardScore:F2} reward for {missionType}");
    if (missionType == FactionHQ.RewardType.None)
      return;
    NetworkSceneSingleton<MessageManager>.i.TargetCreditMessage(player.Owner, (UnityEngine.Object) target != (UnityEngine.Object) null ? target.persistentID : PersistentID.None, rewardScore, missionType);
  }

  public FactionHQ()
  {
    this.InitSyncObject((ISyncObject) this.factionUnits);
    this.InitSyncObject((ISyncObject) this.factionRadarReturn);
    this.InitSyncObject((ISyncObject) this.factionPlayers);
    this.InitSyncObject((ISyncObject) this.airbasesUnsorted);
    this.InitSyncObject((ISyncObject) this.AircraftSupply);
    this.InitSyncObject((ISyncObject) this.VehicleSupply);
  }

  private void MirageProcessed()
  {
  }

  public float Network\u003CfactionFunds\u003Ek__BackingField
  {
    get => this.\u003CfactionFunds\u003Ek__BackingField;
    set
    {
      if (this.SyncVarEqual<float>(value, this.\u003CfactionFunds\u003Ek__BackingField))
        return;
      float fundsKBackingField = this.\u003CfactionFunds\u003Ek__BackingField;
      this.\u003CfactionFunds\u003Ek__BackingField = value;
      this.SetDirtyBit(1UL);
    }
  }

  public bool NetworkpreventJoin
  {
    get => this.preventJoin;
    set
    {
      if (this.SyncVarEqual<bool>(value, this.preventJoin))
        return;
      bool preventJoin = this.preventJoin;
      this.preventJoin = value;
      this.SetDirtyBit(2UL);
    }
  }

  public bool NetworkpreventDonation
  {
    get => this.preventDonation;
    set
    {
      if (this.SyncVarEqual<bool>(value, this.preventDonation))
        return;
      bool preventDonation = this.preventDonation;
      this.preventDonation = value;
      this.SetDirtyBit(4UL);
    }
  }

  public float NetworkfactionScore
  {
    get => this.factionScore;
    set
    {
      if (this.SyncVarEqual<float>(value, this.factionScore))
        return;
      float factionScore = this.factionScore;
      this.factionScore = value;
      this.SetDirtyBit(8UL);
    }
  }

  public List<string> NetworkrestrictedWeapons
  {
    get => this.restrictedWeapons;
    set
    {
      if (this.SyncVarEqual<List<string>>(value, this.restrictedWeapons))
        return;
      List<string> restrictedWeapons = this.restrictedWeapons;
      this.restrictedWeapons = value;
      this.SetDirtyBit(16UL /*0x10*/);
    }
  }

  public List<string> NetworkrestrictedAircraft
  {
    get => this.restrictedAircraft;
    set
    {
      if (this.SyncVarEqual<List<string>>(value, this.restrictedAircraft))
        return;
      List<string> restrictedAircraft = this.restrictedAircraft;
      this.restrictedAircraft = value;
      this.SetDirtyBit(32UL /*0x20*/);
    }
  }

  public override bool SerializeSyncVars(NetworkWriter writer, bool initialize)
  {
    ulong syncVarDirtyBits = this.SyncVarDirtyBits;
    bool flag = base.SerializeSyncVars(writer, initialize);
    if (initialize)
    {
      // ISSUE: reference to a compiler-generated field
      writer.WriteSingleConverter(this.\u003CfactionFunds\u003Ek__BackingField);
      writer.WriteBooleanExtension(this.preventJoin);
      writer.WriteBooleanExtension(this.preventDonation);
      writer.WriteSingleConverter(this.factionScore);
      GeneratedNetworkCode._Write_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CSystem\u002EString\u003E(writer, this.restrictedWeapons);
      GeneratedNetworkCode._Write_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CSystem\u002EString\u003E(writer, this.restrictedAircraft);
      return true;
    }
    writer.Write(syncVarDirtyBits, 6);
    if (((long) syncVarDirtyBits & 1L) != 0L)
    {
      // ISSUE: reference to a compiler-generated field
      writer.WriteSingleConverter(this.\u003CfactionFunds\u003Ek__BackingField);
      flag = true;
    }
    if (((long) syncVarDirtyBits & 2L) != 0L)
    {
      writer.WriteBooleanExtension(this.preventJoin);
      flag = true;
    }
    if (((long) syncVarDirtyBits & 4L) != 0L)
    {
      writer.WriteBooleanExtension(this.preventDonation);
      flag = true;
    }
    if (((long) syncVarDirtyBits & 8L) != 0L)
    {
      writer.WriteSingleConverter(this.factionScore);
      flag = true;
    }
    if (((long) syncVarDirtyBits & 16L /*0x10*/) != 0L)
    {
      GeneratedNetworkCode._Write_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CSystem\u002EString\u003E(writer, this.restrictedWeapons);
      flag = true;
    }
    if (((long) syncVarDirtyBits & 32L /*0x20*/) != 0L)
    {
      GeneratedNetworkCode._Write_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CSystem\u002EString\u003E(writer, this.restrictedAircraft);
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
      this.\u003CfactionFunds\u003Ek__BackingField = reader.ReadSingleConverter();
      this.preventJoin = reader.ReadBooleanExtension();
      this.preventDonation = reader.ReadBooleanExtension();
      this.factionScore = reader.ReadSingleConverter();
      this.restrictedWeapons = GeneratedNetworkCode._Read_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CSystem\u002EString\u003E(reader);
      this.restrictedAircraft = GeneratedNetworkCode._Read_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CSystem\u002EString\u003E(reader);
    }
    else
    {
      ulong dirtyBit = reader.Read(6);
      this.SetDeserializeMask(dirtyBit, 0);
      if (((long) dirtyBit & 1L) != 0L)
      {
        // ISSUE: reference to a compiler-generated field
        this.\u003CfactionFunds\u003Ek__BackingField = reader.ReadSingleConverter();
      }
      if (((long) dirtyBit & 2L) != 0L)
        this.preventJoin = reader.ReadBooleanExtension();
      if (((long) dirtyBit & 4L) != 0L)
        this.preventDonation = reader.ReadBooleanExtension();
      if (((long) dirtyBit & 8L) != 0L)
        this.factionScore = reader.ReadSingleConverter();
      if (((long) dirtyBit & 16L /*0x10*/) != 0L)
        this.restrictedWeapons = GeneratedNetworkCode._Read_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CSystem\u002EString\u003E(reader);
      if (((long) dirtyBit & 32L /*0x20*/) == 0L)
        return;
      this.restrictedAircraft = GeneratedNetworkCode._Read_System\u002ECollections\u002EGeneric\u002EList\u00601\u003CSystem\u002EString\u003E(reader);
    }
  }

  public void UserCode_RpcExclusionZone_1543365140(ExclusionZone exclusionZone)
  {
    this.RegisterExclusionZone(exclusionZone).Forget();
  }

  protected static void Skeleton_RpcExclusionZone_1543365140(
    NetworkBehaviour behaviour,
    NetworkReader reader,
    INetworkPlayer senderConnection,
    int replyId)
  {
    ((FactionHQ) behaviour).UserCode_RpcExclusionZone_1543365140(GeneratedNetworkCode._Read_NuclearOption\u002EExclusionZone(reader));
  }

  private void UserCode_RpcGetTrackingStateBatched_\u002D1045112216(
    INetworkPlayer _,
    ArraySegment<byte> segment)
  {
    using (PooledNetworkReader reader = NetworkReaderPool.GetReader(segment, (IObjectLocator) null))
    {
      while (reader.CanReadBytes(4))
        this.SetTrackingState(reader.Read<PersistentID>(), reader.Read<GlobalPosition>(), reader.Read<float>());
    }
  }

  protected static void Skeleton_RpcGetTrackingStateBatched_\u002D1045112216(
    NetworkBehaviour behaviour,
    NetworkReader reader,
    INetworkPlayer senderConnection,
    int replyId)
  {
    ((FactionHQ) behaviour).UserCode_RpcGetTrackingStateBatched_\u002D1045112216(behaviour.Client.Player, reader.ReadBytesAndSizeSegment());
  }

  public void UserCode_CmdUpdateTrackingInfo_\u002D1717917610(PersistentID id)
  {
    this.RpcUpdateTrackingInfo(id);
  }

  protected static void Skeleton_CmdUpdateTrackingInfo_\u002D1717917610(
    NetworkBehaviour behaviour,
    NetworkReader reader,
    INetworkPlayer senderConnection,
    int replyId)
  {
    ((FactionHQ) behaviour).UserCode_CmdUpdateTrackingInfo_\u002D1717917610(GeneratedNetworkCode._Read_PersistentID(reader));
  }

  public void UserCode_RpcUpdateTrackingInfo_\u002D640322303(PersistentID id)
  {
    Unit unit;
    if (!UnitRegistry.TryGetUnit(new PersistentID?(id), out unit))
      return;
    TrackingInfo trackingInfo1;
    if (this.trackingDatabase.TryGetValue(id, out trackingInfo1))
    {
      trackingInfo1.UpdateInfo(unit.GlobalPosition());
    }
    else
    {
      TrackingInfo trackingInfo2 = new TrackingInfo(unit);
      this.trackingDatabase.Add(id, new TrackingInfo(unit));
      unit.onDisableUnit += new Action<Unit>(this.DeregisterTrackedUnit);
      unit.onChangeFaction += new Action<Unit>(this.HQ_OnUnitChangeFaction);
      Action<PersistentID> onDiscoverUnit = this.onDiscoverUnit;
      if (onDiscoverUnit != null)
        onDiscoverUnit(id);
      if (DynamicMap.IsFactionMode(this, FactionMode.Spectator | FactionMode.Friendly))
        SceneSingleton<DynamicMap>.i.AddIcon(id);
      if (!((UnityEngine.Object) unit.NetworkHQ != (UnityEngine.Object) null) || !((UnityEngine.Object) unit.NetworkHQ != (UnityEngine.Object) this) || (double) unit.definition.typeIdentity.strategic <= 0.0)
        return;
      this.strategicTargets.Add(trackingInfo2);
    }
  }

  protected static void Skeleton_RpcUpdateTrackingInfo_\u002D640322303(
    NetworkBehaviour behaviour,
    NetworkReader reader,
    INetworkPlayer senderConnection,
    int replyId)
  {
    ((FactionHQ) behaviour).UserCode_RpcUpdateTrackingInfo_\u002D640322303(GeneratedNetworkCode._Read_PersistentID(reader));
  }

  public void UserCode_RpcDeclareEndGame_2091801907(EndType endType)
  {
    if (GameManager.gameResolution != GameResolution.Ongoing)
      return;
    FactionHQ localHq;
    if ((!GameManager.GetLocalHQ(out localHq) ? 0 : ((UnityEngine.Object) localHq == (UnityEngine.Object) this ? 1 : 0)) == (endType == EndType.Victory ? 1 : 0))
    {
      GameManager.FinishGame(GameResolution.Victory);
      MusicManager.i.PlayMusic(GameAssets.i.missionSuccessMusic, false);
    }
    else
    {
      GameManager.FinishGame(GameResolution.Defeat);
      MusicManager.i.PlayMusic(GameAssets.i.missionFailedMusic, false);
    }
    SceneSingleton<GameplayUI>.i.PauseGame();
  }

  protected static void Skeleton_RpcDeclareEndGame_2091801907(
    NetworkBehaviour behaviour,
    NetworkReader reader,
    INetworkPlayer senderConnection,
    int replyId)
  {
    ((FactionHQ) behaviour).UserCode_RpcDeclareEndGame_2091801907(GeneratedNetworkCode._Read_NuclearOption\u002ESavedMission\u002EObjectiveV2\u002EOutcomes\u002EEndType(reader));
  }

  protected override int GetRpcCount() => 5;

  public override void RegisterRpc(RemoteCallCollection collection)
  {
    base.RegisterRpc(collection);
    collection.Register(0, "FactionHQ.RpcExclusionZone", false, RpcInvokeType.ClientRpc, (NetworkBehaviour) this, new RpcDelegate(FactionHQ.Skeleton_RpcExclusionZone_1543365140));
    collection.Register(1, "FactionHQ.RpcGetTrackingStateBatched", false, RpcInvokeType.ClientRpc, (NetworkBehaviour) this, new RpcDelegate(FactionHQ.Skeleton_RpcGetTrackingStateBatched_\u002D1045112216));
    collection.Register(2, "FactionHQ.CmdUpdateTrackingInfo", false, RpcInvokeType.ServerRpc, (NetworkBehaviour) this, new RpcDelegate(FactionHQ.Skeleton_CmdUpdateTrackingInfo_\u002D1717917610));
    collection.Register(3, "FactionHQ.RpcUpdateTrackingInfo", false, RpcInvokeType.ClientRpc, (NetworkBehaviour) this, new RpcDelegate(FactionHQ.Skeleton_RpcUpdateTrackingInfo_\u002D640322303));
    collection.Register(4, "FactionHQ.RpcDeclareEndGame", false, RpcInvokeType.ClientRpc, (NetworkBehaviour) this, new RpcDelegate(FactionHQ.Skeleton_RpcDeclareEndGame_2091801907));
  }

  private struct NetworkSupplyItem(UnitDefinition unitDefinition, int count)
  {
    public readonly UnitDefinition unitDefinition = unitDefinition;
    public readonly int count = count;
  }

  private struct ReserveRequest(AircraftDefinition aircraftDefinition)
  {
    public AircraftDefinition aircraftDefinition = aircraftDefinition;
    public float timeRequested = Time.timeSinceLevelLoad;
  }

  public struct RuntimeSupply(int count)
  {
    public readonly int Count = count;
  }

  public enum RewardType : byte
  {
    None,
    Kill,
    Recon,
    Jamming,
    Supply,
    Refuel,
    Repair,
    RescuePilots,
    CapturePilots,
    CaptureLocation,
  }
}

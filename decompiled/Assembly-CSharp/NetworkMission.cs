// Decompiled with JetBrains decompiler
// Type: NetworkMission
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Mirage;
using Mirage.Events;
using Mirage.Logging;
using Mirage.Serialization;
using NuclearOption.SavedMission;
using NuclearOption.SavedMission.ObjectiveV2;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Events;

#nullable disable
public class NetworkMission
{
  private static readonly ProfilerMarker serverAuthenticatedMarker = new ProfilerMarker("NetworkMission.Authenticated");
  private static readonly ProfilerMarker setMissionMarker = new ProfilerMarker("NetworkMission.SetMission");
  private static readonly ProfilerMarker sendPartsToPlayersMarker = new ProfilerMarker("NetworkMission.SendParts");
  private static readonly ProfilerMarker trySingleSendMarker = new ProfilerMarker("NetworkMission.SendSingle");
  private static readonly ILogger logger = LogFactory.GetLogger<NetworkMission>();
  private const int SINGLE_SEND_MISSION_SIZE = 64000;
  private const int MAX_MESSAGE_SIZE = 1100;
  private NetworkMission.SyncMission current;
  private NetworkMission.State currentState;
  private NetworkWriter missionWriter;
  private bool sendAsParts;
  private readonly NetworkServer server;
  private readonly NetworkClient client;
  private readonly List<INetworkPlayer> playerCache = new List<INetworkPlayer>();
  private NetworkMission.PartTracker partTracker;
  private readonly AddLateEvent<(NetworkMission.SyncMission mission, NetworkMission.State state, bool stateChangeOnly)> changed = new AddLateEvent<(NetworkMission.SyncMission, NetworkMission.State, bool)>();

  public IAddLateEvent<(NetworkMission.SyncMission mission, NetworkMission.State state, bool stateChangeOnly)> Changed
  {
    get => (IAddLateEvent<(NetworkMission.SyncMission, NetworkMission.State, bool)>) this.changed;
  }

  public NetworkMission(NetworkServer server, NetworkClient client)
  {
    this.server = server;
    this.client = client;
    client.Started.AddListener(new UnityAction(this.ClientStarted));
    server.Authenticated.AddListener(new UnityAction<INetworkPlayer>(this.ServerAuthenticated));
  }

  private void ServerAuthenticated(INetworkPlayer player)
  {
    using (NetworkMission.serverAuthenticatedMarker.Auto())
    {
      if (player == this.server.LocalPlayer || this.current.Mission == null)
        return;
      if (this.sendAsParts)
      {
        this.SendPartsToPlayer(player);
      }
      else
      {
        ArraySegment<byte> arraySegment = this.missionWriter.ToArraySegment();
        player.Send(arraySegment);
      }
      if (this.currentState != NetworkMission.State.Running)
        return;
      player.Send<NetworkMission.SyncMissionStart>(new NetworkMission.SyncMissionStart());
    }
  }

  private void ClientStarted()
  {
    this.client.MessageHandler.RegisterHandler<NetworkMission.SyncMissionStart>(new MessageDelegate<NetworkMission.SyncMissionStart>(this.OnReceiveMissionStart));
    this.client.MessageHandler.RegisterHandler<NetworkMission.SyncMission>(new MessageDelegate<NetworkMission.SyncMission>(this.OnReceiveMission));
    this.client.MessageHandler.RegisterHandler<NetworkMission.SyncMissionHeader>(new MessageDelegateWithPlayer<NetworkMission.SyncMissionHeader>(this.OnReceiveMissionHeader));
    this.client.MessageHandler.RegisterHandler<NetworkMission.SyncMissionPart>(new MessageDelegateWithPlayer<NetworkMission.SyncMissionPart>(this.OnReceiveMissionPart));
    this.client.MessageHandler.RegisterHandler<NetworkMission.SyncMissionFooter>(new MessageDelegateWithPlayer<NetworkMission.SyncMissionFooter>(this.OnReceiveMissionFooter));
  }

  private void OnReceiveMission(NetworkMission.SyncMission message)
  {
    NetworkMission.State state = message.Mission != null ? NetworkMission.State.Loaded : NetworkMission.State.Unloaded;
    if (message.Mission == null)
      message.Mission = Mission.NullMission;
    ColorLog<NetworkMission>.Info($"OnReceiveMission ({this.currentState}, {this.current.Name}) -> ({state}, {message.Name})");
    this.current = message;
    this.currentState = state;
    this.changed?.Invoke((this.current, this.currentState, false));
  }

  private void OnReceiveMissionStart(NetworkMission.SyncMissionStart message)
  {
    NetworkMission.State state = NetworkMission.State.Running;
    ColorLog<NetworkMission>.Info($"OnReceiveState: {this.currentState} -> {state}");
    this.currentState = state;
    this.changed?.Invoke((this.current, this.currentState, true));
  }

  public void SetRunning()
  {
    if (this.currentState == NetworkMission.State.Running)
    {
      ColorLog<NetworkMission>.Info($"Mission was already in state {(Enum) NetworkMission.State.Running}");
    }
    else
    {
      ColorLog<NetworkMission>.Info($"Setting state to {this.currentState} -> {(Enum) NetworkMission.State.Running}");
      this.currentState = NetworkMission.State.Running;
      this.server.SendToAll<NetworkMission.SyncMissionStart>(new NetworkMission.SyncMissionStart(), true, true);
    }
  }

  public void Set(Mission mission, NetworkMission.State state)
  {
    using (NetworkMission.setMissionMarker.Auto())
    {
      if (mission == Mission.NullMission)
        mission = (Mission) null;
      if (mission == this.current.Mission)
      {
        ColorLog<NetworkMission>.Info($"Mission already equal to {mission?.Name ?? "NULL"}, skipping set");
      }
      else
      {
        if (this.missionWriter != null)
          this.missionWriter.Reset();
        else
          this.missionWriter = new NetworkWriter(64000, false);
        ColorLog<NetworkMission>.Info($"Setting mission to ({state}, {mission?.Name ?? "NULL"})");
        this.current = new NetworkMission.SyncMission(mission);
        this.currentState = state;
        bool flag = this.TrySingleSend();
        this.sendAsParts = !flag;
        if (!flag)
        {
          if (mission == null)
            throw new InvalidOperationException("Null mission should have been sent with TrySingleSend");
          this.SendPartsToAll();
        }
        if (state != NetworkMission.State.Running)
          return;
        this.server.SendToAll<NetworkMission.SyncMissionStart>(new NetworkMission.SyncMissionStart(), true, true);
      }
    }
  }

  public void Clear()
  {
    ColorLog<NetworkMission>.Info("Clear Mission");
    this.Set((Mission) null, NetworkMission.State.Unloaded);
  }

  private bool TrySingleSend()
  {
    using (NetworkMission.trySingleSendMarker.Auto())
    {
      try
      {
        MessagePacker.Pack<NetworkMission.SyncMission>(this.current, this.missionWriter);
        ArraySegment<byte> arraySegment = this.missionWriter.ToArraySegment();
        foreach (INetworkPlayer authenticatedPlayer in (IEnumerable<INetworkPlayer>) this.server.AuthenticatedPlayers)
        {
          if (!authenticatedPlayer.IsHost)
            authenticatedPlayer.Send(arraySegment);
        }
        return true;
      }
      catch (InvalidOperationException ex)
      {
        this.missionWriter.Reset();
        if (ex.Message.Contains("Can not write over end of buffer, new length "))
        {
          if (NetworkMission.logger.WarnEnabled())
            NetworkMission.logger.LogWarning((object) $"Mission was larger than {64000}, Sending as parts instead");
          return false;
        }
        throw;
      }
    }
  }

  private void SendPartsToAll()
  {
    this.playerCache.Clear();
    foreach (INetworkPlayer authenticatedPlayer in (IEnumerable<INetworkPlayer>) this.server.AuthenticatedPlayers)
    {
      if (!authenticatedPlayer.IsHost)
        this.playerCache.Add(authenticatedPlayer);
    }
    if (this.playerCache.Count == 0)
      return;
    this.SendPartsToPlayers(this.playerCache);
    this.playerCache.Clear();
  }

  private void SendPartsToPlayer(INetworkPlayer player)
  {
    this.playerCache.Clear();
    this.playerCache.Add(player);
    this.SendPartsToPlayers(this.playerCache);
    this.playerCache.Clear();
  }

  private void SendPartsToPlayers(List<INetworkPlayer> players)
  {
    using (NetworkMission.sendPartsToPlayersMarker.Auto())
    {
      NetworkMission.SyncMissionHeader msg = new NetworkMission.SyncMissionHeader()
      {
        Name = this.current.Name,
        JsonVersion = this.current.Mission.JsonVersion,
        environment = this.current.Mission.environment,
        missionSettings = this.current.Mission.missionSettings
      };
      if (NetworkMission.logger.LogEnabled())
        NetworkMission.logger.Log((object) ("Sending header " + this.current.Name));
      this.server.SendToMany<NetworkMission.SyncMissionHeader>((IReadOnlyList<INetworkPlayer>) players, msg, true);
      using (PooledNetworkWriter partWriter = NetworkWriterPool.GetWriter())
      {
        using (PooledNetworkWriter fullWriter = NetworkWriterPool.GetWriter())
        {
          WriteList<SavedAircraft>(this.current.Mission.aircraft);
          WriteList<SavedVehicle>(this.current.Mission.vehicles);
          WriteList<SavedShip>(this.current.Mission.ships);
          WriteList<SavedBuilding>(this.current.Mission.buildings);
          WriteList<SavedScenery>(this.current.Mission.scenery);
          WriteList<SavedContainer>(this.current.Mission.containers);
          WriteList<SavedMissile>(this.current.Mission.missiles);
          WriteList<SavedPilot>(this.current.Mission.pilots);
          WriteList<MissionFaction>(this.current.Mission.factions);
          WriteList<SavedAirbase>(this.current.Mission.airbases);
          WriteList<SavedObjective>(this.current.Mission.objectives.Objectives);
          WriteList<SavedOutcome>(this.current.Mission.objectives.Outcomes);

          void WriteList<T>(List<T> list)
          {
            if (NetworkMission.logger.LogEnabled())
              NetworkMission.logger.Log((object) $"Writing list {list.Count} items");
            fullWriter.WritePackedUInt32((uint) list.Count);
            foreach (T obj in list)
            {
              partWriter.Write<T>(obj);
              if (partWriter.ByteLength + fullWriter.ByteLength > 1100)
              {
                if (NetworkMission.logger.LogEnabled())
                  NetworkMission.logger.Log((object) $"Sending part {fullWriter.ByteLength} bytes");
                this.server.SendToMany<NetworkMission.SyncMissionPart>((IReadOnlyList<INetworkPlayer>) players, new NetworkMission.SyncMissionPart()
                {
                  Bytes = fullWriter.ToArraySegment()
                }, true);
                fullWriter.Reset();
              }
              fullWriter.PadToByte();
              fullWriter.CopyFromWriter((NetworkWriter) partWriter);
              partWriter.Reset();
            }
            if (fullWriter.ByteLength > 0)
            {
              if (NetworkMission.logger.LogEnabled())
                NetworkMission.logger.Log((object) $"Sending part {fullWriter.ByteLength} bytes (last)");
              this.server.SendToMany<NetworkMission.SyncMissionPart>((IReadOnlyList<INetworkPlayer>) players, new NetworkMission.SyncMissionPart()
              {
                Bytes = fullWriter.ToArraySegment()
              }, true);
            }
            fullWriter.Reset();
          }
        }
      }
      if (NetworkMission.logger.LogEnabled())
        NetworkMission.logger.Log((object) ("Sending footer " + this.current.Name));
      this.server.SendToMany<NetworkMission.SyncMissionFooter>((IReadOnlyList<INetworkPlayer>) players, new NetworkMission.SyncMissionFooter()
      {
        Name = this.current.Name
      }, true);
    }
  }

  private void OnReceiveMissionHeader(
    INetworkPlayer player,
    NetworkMission.SyncMissionHeader message)
  {
    this.current = new NetworkMission.SyncMission()
    {
      Name = message.Name,
      Mission = new Mission()
    };
    ColorLog<NetworkMission>.Info($"Reading header {this.current.Name}. {this.currentState} -> {(Enum) NetworkMission.State.Loading}");
    this.currentState = NetworkMission.State.Loading;
    this.current.Mission.JsonVersion = message.JsonVersion;
    this.current.Mission.missionSettings = message.missionSettings;
    this.current.Mission.environment = message.environment;
    this.current.Mission.objectives.Objectives = new List<SavedObjective>();
    this.current.Mission.objectives.Outcomes = new List<SavedOutcome>();
    this.partTracker = new NetworkMission.PartTracker(this.current.Mission);
  }

  private void OnReceiveMissionPart(INetworkPlayer player, NetworkMission.SyncMissionPart message)
  {
    if (this.partTracker == null)
    {
      NetworkMission.logger.LogWarning((object) "Received another SyncMissionPart but tracker was null. (this could be from error in previous part)");
    }
    else
    {
      try
      {
        if (NetworkMission.logger.LogEnabled())
          NetworkMission.logger.Log((object) $"Part {message.Bytes.Count} bytes");
        this.partTracker.ReadNext(message.Bytes);
      }
      catch
      {
        NetworkMission.logger.LogError((object) "Failed to read SyncMissionPart");
        this.current = new NetworkMission.SyncMission();
        this.partTracker = (NetworkMission.PartTracker) null;
        throw;
      }
    }
  }

  private void OnReceiveMissionFooter(
    INetworkPlayer player,
    NetworkMission.SyncMissionFooter message)
  {
    if (message.Name != this.current.Name)
      throw new ArgumentException("Name mismatch for missions");
    this.partTracker = this.partTracker.IsAtEnd() ? (NetworkMission.PartTracker) null : throw new ArgumentException("Not at end of parts");
    if (NetworkMission.logger.LogEnabled())
      NetworkMission.logger.Log((object) ("Reading footer " + this.current.Name));
    ColorLog<NetworkMission>.Info($"OnReceiveMissionFooter: {message.Name}. {this.currentState} -> {(Enum) NetworkMission.State.Loaded}");
    this.currentState = NetworkMission.State.Loaded;
    this.changed.Invoke((this.current, this.currentState, false));
  }

  [NetworkMessage]
  public struct SyncMissionPart
  {
    public ArraySegment<byte> Bytes;
  }

  [NetworkMessage]
  public struct SyncMissionHeader
  {
    public string Name;
    public NetworkMission.State State;
    public int JsonVersion;
    public MissionSettings missionSettings;
    public MissionEnvironment environment;
  }

  [NetworkMessage]
  public struct SyncMissionFooter
  {
    public string Name;
  }

  [NetworkMessage]
  public struct SyncMission
  {
    public string Name;
    public Mission Mission;

    public SyncMission(Mission mission)
    {
      this.Mission = mission;
      this.Name = mission?.Name;
    }
  }

  [NetworkMessage]
  [StructLayout(LayoutKind.Sequential, Size = 1)]
  public struct SyncMissionStart
  {
  }

  [Serializable]
  public enum State
  {
    Unloaded,
    Loading,
    Loaded,
    Running,
  }

  private class PartTracker
  {
    private readonly List<Func<ArraySegment<byte>, bool>> readers;
    private int currentList;
    private uint? currentListCount;

    public PartTracker(Mission mission)
    {
      NetworkMission.PartTracker partTracker = this;
      this.readers = new List<Func<ArraySegment<byte>, bool>>()
      {
        (Func<ArraySegment<byte>, bool>) (b => partTracker.ReadList<SavedAircraft>(b, mission.aircraft)),
        (Func<ArraySegment<byte>, bool>) (b => partTracker.ReadList<SavedVehicle>(b, mission.vehicles)),
        (Func<ArraySegment<byte>, bool>) (b => partTracker.ReadList<SavedShip>(b, mission.ships)),
        (Func<ArraySegment<byte>, bool>) (b => partTracker.ReadList<SavedBuilding>(b, mission.buildings)),
        (Func<ArraySegment<byte>, bool>) (b => partTracker.ReadList<SavedScenery>(b, mission.scenery)),
        (Func<ArraySegment<byte>, bool>) (b => partTracker.ReadList<SavedContainer>(b, mission.containers)),
        (Func<ArraySegment<byte>, bool>) (b => partTracker.ReadList<SavedMissile>(b, mission.missiles)),
        (Func<ArraySegment<byte>, bool>) (b => partTracker.ReadList<SavedPilot>(b, mission.pilots)),
        (Func<ArraySegment<byte>, bool>) (b => partTracker.ReadList<MissionFaction>(b, mission.factions)),
        (Func<ArraySegment<byte>, bool>) (b => partTracker.ReadList<SavedAirbase>(b, mission.airbases)),
        (Func<ArraySegment<byte>, bool>) (b => partTracker.ReadList<SavedObjective>(b, mission.objectives.Objectives)),
        (Func<ArraySegment<byte>, bool>) (b => partTracker.ReadList<SavedOutcome>(b, mission.objectives.Outcomes))
      };
    }

    private bool ReadList<T>(ArraySegment<byte> bytes, List<T> list)
    {
      using (PooledNetworkReader reader = NetworkReaderPool.GetReader(bytes, (IObjectLocator) null))
      {
        if (!this.currentListCount.HasValue)
        {
          this.currentListCount = new uint?(reader.ReadPackedUInt32());
          if (NetworkMission.logger.LogEnabled())
            NetworkMission.logger.Log((object) $"Start reading {this.currentListCount.Value} items");
        }
        long count1 = (long) list.Count;
        uint? currentListCount1 = this.currentListCount;
        long? nullable1 = currentListCount1.HasValue ? new long?((long) currentListCount1.GetValueOrDefault()) : new long?();
        long valueOrDefault1 = nullable1.GetValueOrDefault();
        if (count1 == valueOrDefault1 & nullable1.HasValue)
        {
          this.currentListCount = new uint?();
          if (NetworkMission.logger.LogEnabled())
            NetworkMission.logger.Log((object) "Read all items");
          return true;
        }
        while (reader.CanReadBytes(1))
        {
          T obj = reader.Read<T>();
          reader.PadToByte();
          list.Add(obj);
          long count2 = (long) list.Count;
          uint? currentListCount2 = this.currentListCount;
          long? nullable2 = currentListCount2.HasValue ? new long?((long) currentListCount2.GetValueOrDefault()) : new long?();
          long valueOrDefault2 = nullable2.GetValueOrDefault();
          if (count2 == valueOrDefault2 & nullable2.HasValue)
          {
            this.currentListCount = new uint?();
            if (NetworkMission.logger.LogEnabled())
              NetworkMission.logger.Log((object) "Read all items");
            return true;
          }
        }
        if (NetworkMission.logger.LogEnabled())
          NetworkMission.logger.Log((object) "Done with part, not finished list");
        return false;
      }
    }

    public void ReadNext(ArraySegment<byte> bytes)
    {
      if (!this.readers[this.currentList](bytes))
        return;
      if (NetworkMission.logger.LogEnabled())
        NetworkMission.logger.Log((object) $"Done with list {this.currentList}/{this.readers.Count}");
      ++this.currentList;
    }

    public bool IsAtEnd() => this.readers.Count == this.currentList;
  }
}

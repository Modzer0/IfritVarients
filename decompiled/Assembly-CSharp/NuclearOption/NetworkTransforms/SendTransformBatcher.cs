// Decompiled with JetBrains decompiler
// Type: NuclearOption.NetworkTransforms.SendTransformBatcher
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Cysharp.Threading.Tasks;
using Mirage;
using Mirage.Serialization;
using NuclearOption.DebugScripts;
using NuclearOption.Networking;
using System;
using System.Collections.Generic;
using System.Threading;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Events;

#nullable disable
namespace NuclearOption.NetworkTransforms;

public class SendTransformBatcher : MonoBehaviour
{
  private static readonly ProfilerMarker clientUpdateMarker = new ProfilerMarker("ClientUpdate");
  private static readonly ProfilerMarker serverUpdateMarker = new ProfilerMarker("ServerUpdate");
  private const int MAX_MESSAGE_SIZE = 1100;
  private const int SNAPSHOT_BITCOUNT_SIZE = 16 /*0x10*/;
  [SerializeField]
  private NetworkServer Server;
  [SerializeField]
  private NetworkClient Client;
  [SerializeField]
  private SendTransformBatcherDebugger debugger;
  [Header("Time Smoothing Settings")]
  [Tooltip("how many seconds to use min smooth values for after joining")]
  [SerializeField]
  private float delayAfterJoin = 2f;
  [Tooltip("how many seconds to lerp up to max smooth values")]
  [SerializeField]
  private float delayTillMaxSmooth = 10f;
  [SerializeField]
  private SendTransformBatcher.SmoothTimeValues startTimeValues = new SendTransformBatcher.SmoothTimeValues()
  {
    MaxExtrapolation = 0.0f,
    MaxSnapshotAge = 0.0f,
    SnapThreshold = 0.4f
  };
  [SerializeField]
  private SendTransformBatcher.SmoothTimeValues fullTimeValues = new SendTransformBatcher.SmoothTimeValues()
  {
    MaxExtrapolation = 0.2f,
    MaxSnapshotAge = 0.5f,
    SnapThreshold = 2f
  };
  private readonly List<AircraftNetworkTransform> _behavioursClientAircraft = new List<AircraftNetworkTransform>();
  private readonly List<NetworkTransformBase> _behavioursClientOther = new List<NetworkTransformBase>();
  private readonly List<NetworkTransformBase> _behavioursAll = new List<NetworkTransformBase>();
  private readonly Dictionary<NetworkIdentity, NetworkTransformBase> _lookup = new Dictionary<NetworkIdentity, NetworkTransformBase>();
  private readonly List<INetworkPlayer> playerCache = new List<INetworkPlayer>();
  private double lastReceive;
  private SmoothNetworkTime smoothTime;
  private double lastServerTime;
  private bool snapTime;
  private double firstReceivedTime;
  private SendTransformBatcher.SmoothTimeValues currentTimeValues;
  [NonSerialized]
  public ClientAuthStream clientAuthDebugStream;
  [Header("Debug Settings")]
  public LineRenderer LineRendererPrefab;
  [Tooltip("Can be used to sync more often")]
  public float IntervalMultiplier = 1f;

  public bool NoPlayers { get; private set; }

  public double LocalSnapshotTime => Time.fixedTimeAsDouble;

  public double ServerSmoothTime => this.smoothTime.InterpolationTime;

  public float MaxExtrapolation => this.currentTimeValues.MaxExtrapolation;

  public double Debug_extrapolationOffset
  {
    get
    {
      SmoothNetworkTime smoothTime = this.smoothTime;
      return smoothTime == null ? 0.0 : smoothTime.GetExtrapolationOffset(this.MaxExtrapolation);
    }
  }

  private void Start()
  {
    this.Server.ManualUpdate = true;
    this.Client.ManualUpdate = true;
    this.EarlyUpdate().Forget();
    this.Server.Started.AddListener(new UnityAction(this.ServerStarted));
    this.Server.Stopped.AddListener(new UnityAction(this.ServerStopped));
    this.Client.Started.AddListener(new UnityAction(this.ClientStarted));
    this.Client.Disconnected.AddListener(new UnityAction<ClientStoppedReason>(this.ClientStopped));
  }

  private void OnDestroy()
  {
    this.clientAuthDebugStream?.Dispose();
    this.clientAuthDebugStream = (ClientAuthStream) null;
  }

  private void ClientStarted()
  {
    if (this.Server.Active)
      return;
    this.AddWorldEvents(this.Client.World);
    this.Client.MessageHandler.RegisterHandler<SendTransformBatcher.TransformMessage>(new MessageDelegate<SendTransformBatcher.TransformMessage>(this.HandleTransformMessage));
    this.lastServerTime = 0.0;
    this.lastReceive = 0.0;
    this.currentTimeValues = this.startTimeValues;
    this.smoothTime = new SmoothNetworkTime();
  }

  private void ServerStarted()
  {
    this.AddWorldEvents(this.Server.World);
    this.lastServerTime = 0.0;
    this.lastReceive = 0.0;
    this.currentTimeValues = this.startTimeValues;
    this.smoothTime = new SmoothNetworkTime();
    if (string.IsNullOrEmpty(ClientAuthStream.OpenPath))
      return;
    this.clientAuthDebugStream = new ClientAuthStream();
    this.clientAuthDebugStream.Open(ClientAuthStream.OpenPath);
  }

  private void ClientStopped(ClientStoppedReason arg0)
  {
    if (this.Server.Active)
      return;
    this.Cleanup();
  }

  private void ServerStopped()
  {
    this.Cleanup();
    this.clientAuthDebugStream?.Dispose();
    this.clientAuthDebugStream = (ClientAuthStream) null;
  }

  private void Cleanup()
  {
    this._behavioursClientAircraft.Clear();
    this._behavioursClientOther.Clear();
    this._behavioursAll.Clear();
    this._lookup.Clear();
    this.lastReceive = 0.0;
    this.currentTimeValues = this.startTimeValues;
    this.smoothTime = (SmoothNetworkTime) null;
    this.lastServerTime = 0.0;
    this.snapTime = false;
  }

  private void AddWorldEvents(NetworkWorld world)
  {
    world.onUnspawn += new Action<NetworkIdentity>(this.World_onUnspawn);
    world.AddAndInvokeOnSpawn(new Action<NetworkIdentity>(this.World_onSpawn));
  }

  private void World_onSpawn(NetworkIdentity identity)
  {
    NetworkTransformBase component;
    if (!identity.TryGetComponent<NetworkTransformBase>(out component))
      return;
    if (component is AircraftNetworkTransform networkTransform)
      this._behavioursClientAircraft.Add(networkTransform);
    else
      this._behavioursClientOther.Add(component);
    this._behavioursAll.Add(component);
    this._lookup.Add(identity, component);
    component.Setup(this);
    component.ResetUpdateTime(Time.timeAsDouble);
    component.SyncSettings.Interval *= this.IntervalMultiplier;
  }

  private void World_onUnspawn(NetworkIdentity identity)
  {
    NetworkTransformBase networkTransformBase;
    if (!this._lookup.TryGetValue(identity, out networkTransformBase))
      return;
    if (networkTransformBase is AircraftNetworkTransform networkTransform)
      this._behavioursClientAircraft.Remove(networkTransform);
    else
      this._behavioursClientOther.Remove(networkTransformBase);
    this._behavioursAll.Remove(networkTransformBase);
    this._lookup.Remove(identity);
  }

  private async UniTask EarlyUpdate()
  {
    SendTransformBatcher transformBatcher = this;
    CancellationToken cancel = transformBatcher.destroyCancellationToken;
    YieldAwaitable yieldAwaitable = UniTask.Yield(PlayerLoopTiming.LastEarlyUpdate);
    await yieldAwaitable;
    while (!cancel.IsCancellationRequested)
    {
      transformBatcher.Server.UpdateReceive();
      transformBatcher.Client.UpdateReceive();
      transformBatcher.clientAuthDebugStream?.MarkUpdatedFinished();
      yieldAwaitable = UniTask.Yield(PlayerLoopTiming.LastEarlyUpdate);
      await yieldAwaitable;
    }
    cancel = new CancellationToken();
  }

  private void LateUpdate()
  {
    if (GameManager.gameState == GameState.Multiplayer && !NetworkManagerNuclearOption.IsLoadingScene)
    {
      if (this.Client.Active)
        this.ClientUpdate();
      if (this.Server.Active)
        this.ServerUpdate();
    }
    this.Server.UpdateSent();
    this.Client.UpdateSent();
  }

  private void ClientUpdate()
  {
    using (SendTransformBatcher.clientUpdateMarker.Auto())
    {
      this.smoothTime.Update(Time.unscaledDeltaTime);
      if ((!this.Server.Active ? 0 : (this.Server.AuthenticatedPlayers.Count <= 1 ? 1 : 0)) == 0)
        this.VisualUpdate();
      this.snapTime = false;
    }
  }

  private void VisualUpdate()
  {
    bool active = this.Server.Active;
    double interpolationTime = this.smoothTime.InterpolationTime;
    double extrapolationOffset = this.smoothTime.GetExtrapolationOffset(this.MaxExtrapolation);
    VisualUpdateTime visualTime = new VisualUpdateTime()
    {
      interpolationTime = interpolationTime,
      extrapolationOffset = extrapolationOffset,
      maxExtrapolateAge = this.currentTimeValues.MaxSnapshotAge,
      snap = this.snapTime
    };
    foreach (AircraftNetworkTransform networkTransform in this._behavioursClientAircraft)
    {
      if (!active || networkTransform != null)
      {
        networkTransform.VisualUpdate(ref visualTime);
        double timestamp = interpolationTime - (double) networkTransform.SyncSettings.Interval * 8.0;
        networkTransform.SnapshotBuffer.RemoveOld(timestamp);
      }
    }
    foreach (NetworkTransformBase networkTransformBase in this._behavioursClientOther)
    {
      networkTransformBase.VisualUpdate(ref visualTime);
      double timestamp = interpolationTime - (double) networkTransformBase.SyncSettings.Interval * 8.0;
      networkTransformBase.SnapshotBuffer.RemoveOld(timestamp);
    }
    if (!DebugVis.Enabled || !((UnityEngine.Object) SceneSingleton<CameraStateManager>.i != (UnityEngine.Object) null))
      return;
    this.debugger.UpdateDebugFollow(ref visualTime);
  }

  private void ServerUpdate()
  {
    using (SendTransformBatcher.serverUpdateMarker.Auto())
    {
      double localSnapshotTime = this.LocalSnapshotTime;
      if (this.lastServerTime == localSnapshotTime)
        return;
      this.lastServerTime = localSnapshotTime;
      if (this.Server.IsHost)
      {
        bool snap;
        this.smoothTime.OnMessage(this.lastServerTime, 0.0, 2f, out snap);
        this.snapTime |= snap;
      }
      double unscaledTimeAsDouble = Time.unscaledTimeAsDouble;
      this.playerCache.Clear();
      foreach (INetworkPlayer authenticatedPlayer in (IEnumerable<INetworkPlayer>) this.Server.AuthenticatedPlayers)
      {
        if (authenticatedPlayer.SceneIsReady && !authenticatedPlayer.IsHost)
          this.playerCache.Add(authenticatedPlayer);
      }
      this.NoPlayers = this.playerCache.Count == 0;
      using (PooledNetworkWriter writer1 = NetworkWriterPool.GetWriter())
      {
        using (PooledNetworkWriter writer2 = NetworkWriterPool.GetWriter())
        {
          bool flag = false;
          foreach (NetworkTransformBase networkTransformBase in this._behavioursAll)
          {
            if (networkTransformBase.TimeToUpdate(unscaledTimeAsDouble) && !this.NoPlayers && networkTransformBase.ShouldSend())
            {
              writer1.WriteNetworkIdentity(networkTransformBase.Identity);
              networkTransformBase.Write((NetworkWriter) writer1);
              if (writer1.ByteLength + writer2.ByteLength > 1100)
              {
                this.Send(writer2, localSnapshotTime);
                flag = true;
                writer2.Reset();
              }
              int bitPosition = writer1.BitPosition;
              writer2.Write((ulong) bitPosition, 16 /*0x10*/);
              writer2.CopyFromWriter((NetworkWriter) writer1);
              writer1.Reset();
            }
          }
          if (flag && writer2.ByteLength <= 0)
            return;
          this.Send(writer2, localSnapshotTime);
        }
      }
    }
  }

  private void Send(PooledNetworkWriter fullWriter, double networkTime)
  {
    NetworkServer.SendToMany<SendTransformBatcher.TransformMessage>(this.playerCache, new SendTransformBatcher.TransformMessage()
    {
      timestamp = networkTime,
      data = fullWriter.ToArraySegment()
    }, Mirage.Channel.Unreliable);
  }

  private void HandleTransformMessage(SendTransformBatcher.TransformMessage message)
  {
    if (this.lastReceive > message.timestamp)
      return;
    if (this.lastReceive != message.timestamp)
    {
      if (this.lastReceive == 0.0)
        this.firstReceivedTime = message.timestamp;
      float num1 = (float) (message.timestamp - this.firstReceivedTime);
      if ((double) num1 <= (double) this.delayAfterJoin)
      {
        this.currentTimeValues = this.startTimeValues;
      }
      else
      {
        float num2 = num1 - this.delayAfterJoin;
        this.currentTimeValues = (double) num2 >= (double) this.delayTillMaxSmooth ? this.fullTimeValues : SendTransformBatcher.SmoothTimeValues.Lerp(this.startTimeValues, this.fullTimeValues, num2 / this.delayTillMaxSmooth);
      }
      double extrapolationOffset = this.Client.World.Time.Rtt / 2.0;
      bool snap;
      this.smoothTime.OnMessage(message.timestamp, extrapolationOffset, this.currentTimeValues.SnapThreshold, out snap);
      this.snapTime |= snap;
    }
    this.lastReceive = message.timestamp;
    using (PooledNetworkReader reader = NetworkReaderPool.GetReader(message.data, (IObjectLocator) this.Client.World))
    {
      while (reader.CanReadBits(16 /*0x10*/))
      {
        int num = (int) reader.Read(16 /*0x10*/);
        int bitPosition1 = reader.BitPosition;
        NetworkIdentity key = reader.ReadNetworkIdentity();
        NetworkTransformBase networkTransformBase;
        if ((UnityEngine.Object) key != (UnityEngine.Object) null && this._lookup.TryGetValue(key, out networkTransformBase))
          networkTransformBase.Receive(message.timestamp, (NetworkReader) reader);
        else
          reader.MoveBitPosition(bitPosition1 + num);
        int bitPosition2 = reader.BitPosition;
      }
    }
  }

  [Serializable]
  public struct SmoothTimeValues
  {
    public float MaxExtrapolation;
    public float MaxSnapshotAge;
    public float SnapThreshold;

    public static SendTransformBatcher.SmoothTimeValues Lerp(
      SendTransformBatcher.SmoothTimeValues a,
      SendTransformBatcher.SmoothTimeValues b,
      float t)
    {
      return new SendTransformBatcher.SmoothTimeValues()
      {
        MaxExtrapolation = Mathf.Lerp(a.MaxExtrapolation, b.MaxExtrapolation, t),
        MaxSnapshotAge = Mathf.Lerp(a.MaxSnapshotAge, b.MaxSnapshotAge, t),
        SnapThreshold = Mathf.Lerp(a.SnapThreshold, b.SnapThreshold, t)
      };
    }
  }

  [Serializable]
  public enum ClientTimeMode
  {
    EMA_Offset,
    SmoothNetworkTimeStep,
  }

  [NetworkMessage]
  private struct TransformMessage
  {
    public double timestamp;
    public ArraySegment<byte> data;
  }
}

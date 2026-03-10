// Decompiled with JetBrains decompiler
// Type: NuclearOption.NetworkTransforms.AircraftNetworkTransform
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Cysharp.Threading.Tasks;
using Mirage;
using Mirage.RemoteCalls;
using Mirage.Serialization;
using NuclearOption.Networking;
using System;
using System.Threading;
using Unity.Profiling;
using UnityEngine;

#nullable disable
namespace NuclearOption.NetworkTransforms;

public class AircraftNetworkTransform : NetworkTransformBase
{
  private static readonly ProfilerMarker applySnapshotMarker = new ProfilerMarker("ApplySnapshot");
  private static readonly ProfilerMarker visualUpdateMarker = new ProfilerMarker("Aircraft.VisualUpdate");
  public Aircraft Aircraft;
  [Tooltip("now many snapshots to send inputs with (4 = send it every 4th snapshot)")]
  public int SendInputsInterval = 4;
  private int inputIntervalCounter;
  private const int CLIENT_SNAPSHOTS_RESENDS = 4;
  private readonly (int sendsRemaining, NetworkTransformBase.NetworkSnapshot snapshot)[] clientSnapshots = new (int, NetworkTransformBase.NetworkSnapshot)[4];
  private int clientSnapshotsNextIndex;
  private Vector3 smoothingVel;
  private Vector3 rotationSmoothingVel;
  private ExponentialMovingAverage clientAuthOffset = new ExponentialMovingAverage(20);
  private ExponentialMovingAverage clientRttEMA = new ExponentialMovingAverage(20);
  private float[] clientRTTSMA = new float[20];
  private int clientRTTSMAIndex;
  private int clientRTTSMAIndexMax;
  private SmoothNetworkTime clientAuthTimer = new SmoothNetworkTime();
  private double lastClientLocalTime;
  private double lastServerLocalTime;
  private double lastClientSnapshotTime;
  private ClientAuthChecks clientAuth;
  private int rejectResetCount;
  private CancellationTokenSource cancelClientAuthUpdate;
  [NonSerialized]
  private const int SYNC_VAR_COUNT = 0;
  [NonSerialized]
  private const int RPC_COUNT = 2;

  public bool UseClientValue => this.Owner != null;

  protected virtual void Awake()
  {
    this.Identity.OnStartClient.AddListener(new Action(this.OnStartClient));
  }

  private void OnDestroy() => this.cancelClientAuthUpdate?.Cancel();

  private void OnStartClient()
  {
    if (!this.HasAuthority)
      return;
    this.cancelClientAuthUpdate = new CancellationTokenSource();
    this.StartSlowUpdateDelayed(this.SyncSettings.Interval, new Action(this.ClientAuthUpdate), this.cancelClientAuthUpdate.Token);
  }

  public override void Setup(SendTransformBatcher sendBatcher)
  {
    base.Setup(sendBatcher);
    this.clientAuth = new ClientAuthChecks(this.Aircraft.startPosition, this.Aircraft.startingVelocity, sendBatcher.LocalSnapshotTime);
  }

  private void ClientAuthUpdate()
  {
    if (!this.HasAuthority)
    {
      this.cancelClientAuthUpdate.Cancel();
      this.cancelClientAuthUpdate = (CancellationTokenSource) null;
    }
    else
    {
      NetworkTransformBase.NetworkSnapshot snapshot = this.CreateSnapshot();
      double localSnapshotTime = this.SendBatcher.LocalSnapshotTime;
      double serverSmoothTime = this.SendBatcher.ServerSmoothTime;
      if (AircraftNetworkTransform.ValidateCmdClientAuth(snapshot, localSnapshotTime, serverSmoothTime, true))
        this.CmdClientAuth(snapshot, localSnapshotTime, serverSmoothTime);
      else
        Debug.LogError((object) "Not sending CmdClientAuth because values were invalid");
    }
  }

  private static bool ValidateCmdClientAuth(
    NetworkTransformBase.NetworkSnapshot snapshot,
    double clientLocalTime,
    double clientServerTime,
    bool logErrors)
  {
    return (1 & (NetworkFloatHelper.Validate(clientLocalTime, logErrors, nameof (clientLocalTime)) ? 1 : 0) & (NetworkFloatHelper.Validate(clientServerTime, logErrors, nameof (clientServerTime)) ? 1 : 0) & (snapshot.Valid(logErrors) ? 1 : 0)) != 0;
  }

  [ServerRpc(channel = Mirage.Channel.Unreliable)]
  private void CmdClientAuth(
    NetworkTransformBase.NetworkSnapshot snapshot,
    double clientLocalTime,
    double clientServerTime)
  {
    if (ServerRpcSender.ShouldInvokeLocally((NetworkBehaviour) this, true, false))
    {
      this.UserCode_CmdClientAuth_65459396(snapshot, clientLocalTime, clientServerTime);
    }
    else
    {
      PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
      GeneratedNetworkCode._Write_NuclearOption\u002ENetworkTransforms\u002ENetworkTransformBase\u002FNetworkSnapshot((NetworkWriter) writer, snapshot);
      writer.WriteDoubleConverter(clientLocalTime);
      writer.WriteDoubleConverter(clientServerTime);
      ServerRpcSender.Send((NetworkBehaviour) this, 0, (NetworkWriter) writer, Mirage.Channel.Unreliable, true);
      writer.Release();
    }
  }

  private async UniTaskVoid KickAsync()
  {
    AircraftNetworkTransform networkTransform = this;
    Player player = networkTransform.Aircraft.Player;
    INetworkPlayer conn = networkTransform.Owner;
    player.KickReason("Transform Sync Error", "server");
    await UniTask.Delay(100);
    conn.Disconnect();
    conn = (INetworkPlayer) null;
  }

  [ClientRpc(target = RpcTarget.Owner)]
  private void RpcResetClientAuthPosition(NetworkTransformBase.NetworkSnapshot value)
  {
    if (ClientRpcSender.ShouldInvokeLocally((NetworkBehaviour) this, RpcTarget.Owner, (INetworkPlayer) null, false))
    {
      this.UserCode_RpcResetClientAuthPosition_726277261(value);
    }
    else
    {
      PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
      GeneratedNetworkCode._Write_NuclearOption\u002ENetworkTransforms\u002ENetworkTransformBase\u002FNetworkSnapshot((NetworkWriter) writer, value);
      ClientRpcSender.SendTarget((NetworkBehaviour) this, 1, (NetworkWriter) writer, Mirage.Channel.Reliable, (INetworkPlayer) null);
      writer.Release();
    }
  }

  public override bool ShouldSend()
  {
    if (!this.UseClientValue)
      return !this.Aircraft.rb.isKinematic;
    for (int index = 0; index < this.clientSnapshots.Length; ++index)
    {
      if (this.clientSnapshots[index].sendsRemaining > 0)
        return true;
    }
    return false;
  }

  public override void Write(NetworkWriter writer)
  {
    if (this.UseClientValue)
    {
      int num = 0;
      for (int index = 0; index < this.clientSnapshots.Length; ++index)
      {
        if (this.clientSnapshots[index].sendsRemaining > 0)
          ++num;
      }
      writer.Write((ulong) (num - 1), 2);
      for (int index = 0; index < this.clientSnapshots.Length; ++index)
      {
        ref (int, NetworkTransformBase.NetworkSnapshot) local = ref this.clientSnapshots[(index + this.clientSnapshotsNextIndex) % this.clientSnapshots.Length];
        if (local.Item1 > 0)
        {
          --local.Item1;
          writer.Write<NetworkTransformBase.NetworkSnapshot>(local.Item2);
        }
      }
    }
    else
    {
      writer.Write(0UL, 2);
      NetworkTransformBase.NetworkSnapshot snapshot = this.CreateSnapshot();
      writer.Write<NetworkTransformBase.NetworkSnapshot>(snapshot);
    }
  }

  private NetworkTransformBase.NetworkSnapshot CreateSnapshot()
  {
    CompressedInputs? clientInputs = new CompressedInputs?();
    --this.inputIntervalCounter;
    if (this.inputIntervalCounter <= 0)
    {
      this.inputIntervalCounter = this.SendInputsInterval;
      clientInputs = new CompressedInputs?(new CompressedInputs(this.Aircraft.GetInputs()));
    }
    Rigidbody rb = this.Aircraft.rb;
    Transform transform = rb.transform;
    return new NetworkTransformBase.NetworkSnapshot(clientInputs, transform.GlobalPosition(), transform.rotation, rb.velocity);
  }

  public override void Receive(double timestamp, NetworkReader reader)
  {
    int num = (int) ((long) reader.Read(2) + 1L);
    for (int index = 0; index < num; ++index)
    {
      NetworkTransformBase.NetworkSnapshot fullSnapshot = reader.Read<NetworkTransformBase.NetworkSnapshot>();
      if (fullSnapshot.Valid(true))
        this.QueueNewSnapshot(timestamp, fullSnapshot, index == num - 1);
      else
        Debug.LogError((object) "Ignoring invalid Aircraft snapshot from server");
    }
  }

  private void QueueNewSnapshot(
    double timestamp,
    NetworkTransformBase.NetworkSnapshot fullSnapshot,
    bool isLast)
  {
    if (this.Aircraft.LocalSim)
      return;
    if (isLast && fullSnapshot.ClientInputs.HasValue)
      this.Aircraft.ApplySetInputs(fullSnapshot.ClientInputs.Value);
    this.SnapshotBuffer.Insert(fullSnapshot.timestamp ?? timestamp, fullSnapshot.ToLocal(), true);
  }

  public override void VisualUpdate(ref VisualUpdateTime visualTime)
  {
    using (AircraftNetworkTransform.visualUpdateMarker.Auto())
    {
      NetworkTransformBase.ViewSnapshot snapshot;
      if (this.Aircraft.LocalSim || !this.TryGetSnapshot(ref visualTime, out snapshot))
        return;
      this.ApplySnapshot(snapshot);
      this.Aircraft.CheckSpawnedInPosition();
    }
  }

  private void ApplySnapshot(NetworkTransformBase.ViewSnapshot snapshot)
  {
    using (AircraftNetworkTransform.applySnapshotMarker.Auto())
    {
      Vector3 position1 = snapshot.Position;
      Vector3 velocity = snapshot.Velocity;
      Quaternion rotation = snapshot.Rotation;
      Vector3 position2 = this.transform.position;
      if (this.debugActive)
        this.debugGui.CalculateInfluence(snapshot, position2);
      Rigidbody rb = this.Aircraft.rb;
      this.transform.SetPositionAndRotation(position1, rotation);
      rb.velocity = velocity;
      rb.angularVelocity = Vector3.zero;
      rb.Move(position1, rotation);
    }
  }

  private void MirageProcessed()
  {
  }

  private void UserCode_CmdClientAuth_65459396(
    NetworkTransformBase.NetworkSnapshot snapshot,
    double clientLocalTime,
    double clientServerTime)
  {
    if (clientLocalTime <= this.lastClientLocalTime || !AircraftNetworkTransform.ValidateCmdClientAuth(snapshot, clientLocalTime, clientServerTime, false))
      return;
    double localSnapshotTime = this.SendBatcher.LocalSnapshotTime;
    double serverSmoothTime = this.SendBatcher.ServerSmoothTime;
    double lastClientLocalTime = this.lastClientLocalTime;
    this.lastClientLocalTime = clientLocalTime;
    this.lastServerLocalTime = localSnapshotTime;
    double num1 = clientServerTime;
    float newValue = (float) (serverSmoothTime - num1);
    this.clientRttEMA.Add((double) newValue);
    this.clientRTTSMA[this.clientRTTSMAIndex] = newValue;
    this.clientRTTSMAIndex = (this.clientRTTSMAIndex + 1) % this.clientRTTSMA.Length;
    this.clientRTTSMAIndexMax = Mathf.Max(this.clientRTTSMAIndexMax, this.clientRTTSMAIndex);
    double num2 = this.clientRttEMA.Value;
    this.clientAuthOffset.Add(localSnapshotTime - clientLocalTime);
    double num3 = this.clientAuthOffset.Value;
    double num4 = clientLocalTime + num3;
    float num5 = (float) (num2 / 2.0);
    if (num4 < this.lastClientSnapshotTime)
      num4 = this.lastClientSnapshotTime + 0.5 * (double) this.SyncSettings.Interval;
    snapshot.timestamp = new double?(num4);
    this.lastClientSnapshotTime = num4;
    float num6 = Mathf.Clamp(num5, 0.0f, this.SendBatcher.MaxExtrapolation);
    snapshot.extraExtrapolation = new float?(num6);
    ClientAuthChecks.RejectMask rejectMask;
    bool flag;
    switch (ClientAuthChecks.Mode)
    {
      case ClientAuthChecks.CheckMode.LogOnly:
        this.clientAuth.Run(ref snapshot, out rejectMask);
        flag = false;
        break;
      case ClientAuthChecks.CheckMode.Ignore:
        flag = !this.clientAuth.Run(ref snapshot, out rejectMask);
        break;
      default:
        flag = false;
        rejectMask = ClientAuthChecks.RejectMask.Accepted;
        break;
    }
    this.SendBatcher.clientAuthDebugStream?.Log(this, rejectMask, snapshot, clientLocalTime, clientServerTime);
    if (rejectMask == ClientAuthChecks.RejectMask.Accepted)
      this.rejectResetCount = 0;
    else
      ++this.rejectResetCount;
    if (this.rejectResetCount != 0 && this.rejectResetCount % 10 == 0)
      ColorLog<AircraftNetworkTransform>.Info($"{this.Owner} Invalid snapshot {this.rejectResetCount}");
    if (flag)
    {
      if (this.rejectResetCount % 10 == 0)
        this.RpcResetClientAuthPosition(this.clientAuth.CreateServerSnapshot());
      if (this.rejectResetCount != 100)
        return;
      this.KickAsync().Forget();
    }
    else
    {
      this.clientSnapshots[this.clientSnapshotsNextIndex] = (4, snapshot);
      this.clientSnapshotsNextIndex = (this.clientSnapshotsNextIndex + 1) % this.clientSnapshots.Length;
      this.ForceSync();
      if (this.Owner.IsHost)
        return;
      this.QueueNewSnapshot(snapshot.timestamp.Value, snapshot, true);
    }
  }

  protected static void Skeleton_CmdClientAuth_65459396(
    NetworkBehaviour behaviour,
    NetworkReader reader,
    INetworkPlayer senderConnection,
    int replyId)
  {
    ((AircraftNetworkTransform) behaviour).UserCode_CmdClientAuth_65459396(GeneratedNetworkCode._Read_NuclearOption\u002ENetworkTransforms\u002ENetworkTransformBase\u002FNetworkSnapshot(reader), reader.ReadDoubleConverter(), reader.ReadDoubleConverter());
  }

  private void UserCode_RpcResetClientAuthPosition_726277261(
    NetworkTransformBase.NetworkSnapshot value)
  {
    Vector3 localPosition = value.globalPos.ToLocalPosition();
    this.transform.SetPositionAndRotation(localPosition, value.rotation);
    this.Aircraft.rb.Move(localPosition, value.rotation);
    this.Aircraft.rb.velocity = value.velocity.Decompress();
  }

  protected static void Skeleton_RpcResetClientAuthPosition_726277261(
    NetworkBehaviour behaviour,
    NetworkReader reader,
    INetworkPlayer senderConnection,
    int replyId)
  {
    ((AircraftNetworkTransform) behaviour).UserCode_RpcResetClientAuthPosition_726277261(GeneratedNetworkCode._Read_NuclearOption\u002ENetworkTransforms\u002ENetworkTransformBase\u002FNetworkSnapshot(reader));
  }

  protected override int GetRpcCount() => 2;

  public override void RegisterRpc(RemoteCallCollection collection)
  {
    base.RegisterRpc(collection);
    collection.Register(0, "NuclearOption.NetworkTransforms.AircraftNetworkTransform.CmdClientAuth", true, RpcInvokeType.ServerRpc, (NetworkBehaviour) this, new RpcDelegate(AircraftNetworkTransform.Skeleton_CmdClientAuth_65459396));
    collection.Register(1, "NuclearOption.NetworkTransforms.AircraftNetworkTransform.RpcResetClientAuthPosition", false, RpcInvokeType.ClientRpc, (NetworkBehaviour) this, new RpcDelegate(AircraftNetworkTransform.Skeleton_RpcResetClientAuthPosition_726277261));
  }
}

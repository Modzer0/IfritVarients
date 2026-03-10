// Decompiled with JetBrains decompiler
// Type: UnitCommand
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Cysharp.Threading.Tasks;
using Mirage;
using Mirage.RemoteCalls;
using Mirage.Serialization;
using NuclearOption.Networking;
using System;
using Unity.Profiling;
using UnityEngine;

#nullable disable
public class UnitCommand : NetworkBehaviour
{
  private static readonly ProfilerMarker setDestinationMarker = new ProfilerMarker("UnitCommand.SetDestination");
  private ICommandable target;
  private UnitCommand.Command currentCommand;
  [NonSerialized]
  private const int SYNC_VAR_COUNT = 0;
  [NonSerialized]
  private const int RPC_COUNT = 2;

  public event UnitCommand.ProcessCommand ProcessSetDestination;

  private void Awake()
  {
    this.target = this.GetComponent<ICommandable>();
    if (this.target != null)
      return;
    Debug.LogError((object) ("Capture could not find ICapturable on " + this.name));
  }

  public void SetDestination(GlobalPosition waypoint, bool playerCommand)
  {
    if (this.target.Disabled)
      return;
    using (UnitCommand.setDestinationMarker.Auto())
    {
      if (NetworkManagerNuclearOption.i.Server.Active)
      {
        Player localPlayer = (Player) null;
        if (playerCommand)
          GameManager.GetLocalPlayer<Player>(out localPlayer);
        this.ServerSetDestination(waypoint, localPlayer);
      }
      else
        this.CmdSetDestination(waypoint);
    }
  }

  [ServerRpc(requireAuthority = false)]
  private void CmdSetDestination(GlobalPosition waypoint, INetworkPlayer sender = null)
  {
    if (ServerRpcSender.ShouldInvokeLocally((NetworkBehaviour) this, false, false))
    {
      this.UserCode_CmdSetDestination_1791143641(waypoint, this.Server.LocalPlayer);
    }
    else
    {
      PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
      writer.WriteGlobalPosition(waypoint);
      ServerRpcSender.Send((NetworkBehaviour) this, 0, (NetworkWriter) writer, Mirage.Channel.Reliable, false);
      writer.Release();
    }
  }

  [Mirage.Server]
  private void ServerSetDestination(GlobalPosition waypoint, Player player)
  {
    if (!this.IsServer)
      throw new MethodInvocationException("[Server] function 'ServerSetDestination' called when server not active");
    this.currentCommand = new UnitCommand.Command(NetworkSceneSingleton<MissionManager>.i.MissionTime, waypoint, player);
    UnitCommand.ProcessCommand processSetDestination = this.ProcessSetDestination;
    if (processSetDestination == null)
      return;
    processSetDestination(ref this.currentCommand);
  }

  public UnitCommand.Command GetCommandCached() => this.currentCommand;

  public async UniTask<UnitCommand.Command> CmdGetCommand()
  {
    this.currentCommand = await this.CmdGetCommandInternal();
    return this.currentCommand;
  }

  [ServerRpc(requireAuthority = false)]
  private UniTask<UnitCommand.Command> CmdGetCommandInternal()
  {
    if (ServerRpcSender.ShouldInvokeLocally((NetworkBehaviour) this, false, false))
      return this.UserCode_CmdGetCommandInternal_\u002D133123890();
    PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
    UniTask<UnitCommand.Command> commandInternal = ServerRpcSender.SendWithReturn<UnitCommand.Command>((NetworkBehaviour) this, 1, (NetworkWriter) writer, false);
    writer.Release();
    return commandInternal;
  }

  private void MirageProcessed()
  {
  }

  private void UserCode_CmdSetDestination_1791143641(GlobalPosition waypoint, INetworkPlayer sender)
  {
    if (!NetworkFloatHelper.Validate(waypoint, false, (string) null))
      return;
    Player player;
    if (!sender.TryGetPlayer<Player>(out player))
    {
      Debug.LogError((object) "Sender did not have player object");
    }
    else
    {
      if (!player.HasAuthority && !((UnityEngine.Object) player.HQ == (UnityEngine.Object) this.target.HQ))
        return;
      this.ServerSetDestination(waypoint, player);
    }
  }

  protected static void Skeleton_CmdSetDestination_1791143641(
    NetworkBehaviour behaviour,
    NetworkReader reader,
    INetworkPlayer senderConnection,
    int replyId)
  {
    ((UnitCommand) behaviour).UserCode_CmdSetDestination_1791143641(reader.ReadGlobalPosition(), senderConnection);
  }

  private UniTask<UnitCommand.Command> UserCode_CmdGetCommandInternal_\u002D133123890()
  {
    return UniTask.FromResult<UnitCommand.Command>(this.currentCommand);
  }

  protected static UniTask<UnitCommand.Command> Skeleton_CmdGetCommandInternal_\u002D133123890(
    NetworkBehaviour behaviour,
    NetworkReader reader,
    INetworkPlayer senderConnection,
    int replyId)
  {
    return ((UnitCommand) behaviour).UserCode_CmdGetCommandInternal_\u002D133123890();
  }

  protected override int GetRpcCount() => 2;

  public override void RegisterRpc(RemoteCallCollection collection)
  {
    base.RegisterRpc(collection);
    collection.Register(0, "UnitCommand.CmdSetDestination", false, RpcInvokeType.ServerRpc, (NetworkBehaviour) this, new RpcDelegate(UnitCommand.Skeleton_CmdSetDestination_1791143641));
    collection.RegisterRequest<UnitCommand.Command>(1, "UnitCommand.CmdGetCommandInternal", false, RpcInvokeType.ServerRpc, (NetworkBehaviour) this, new RequestDelegate<UnitCommand.Command>(UnitCommand.Skeleton_CmdGetCommandInternal_\u002D133123890));
  }

  public delegate void ProcessCommand(ref UnitCommand.Command command);

  public struct Command(float time, GlobalPosition position, Player player)
  {
    public float time = time;
    public Player player = player;
    public GlobalPosition position = position;

    public bool FromPlayer => (UnityEngine.Object) this.player != (UnityEngine.Object) null;
  }
}

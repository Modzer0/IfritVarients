// Decompiled with JetBrains decompiler
// Type: MirageRemote
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Mirage;
using Mirage.Authentication;
using Mirage.SocketLayer;
using Mirage.Sockets.Udp;
using System;
using System.Collections.Generic;

#nullable disable
public class MirageRemote : IDataHandler, IDisposable
{
  private const int PORT = 7071;
  private readonly Peer peer;
  private readonly Dictionary<IConnection, NetworkPlayer> players = new Dictionary<IConnection, NetworkPlayer>();
  public readonly MessageHandler MessageHandler;
  private bool active;

  public void Dispose()
  {
    this.peer.Close();
    this.active = false;
  }

  public MirageRemote()
  {
    this.peer = new Peer((ISocket) new NanoSocket(262144 /*0x040000*/), 1200, (IDataHandler) this);
    this.MessageHandler = new MessageHandler((IObjectLocator) null, false);
    this.peer.OnConnected += (Action<IConnection>) (conn =>
    {
      NetworkPlayer networkPlayer = new NetworkPlayer(conn, false, (NetworkServer) null, new RateLimitBucket.RefillConfig?());
      networkPlayer.SetAuthentication(new PlayerAuthentication((INetworkAuthenticator) null, (object) null), true);
      this.players.Add(conn, networkPlayer);
    });
    this.peer.OnDisconnected += (Action<IConnection, DisconnectReason>) ((conn, _) => this.players.Remove(conn));
  }

  public void Bind()
  {
    this.peer.Bind((IBindEndPoint) new NanoConnectionHandle("::0", (ushort) 7071));
    this.active = true;
  }

  public void Connect()
  {
    this.peer.Connect((IConnectEndPoint) new NanoConnectionHandle("127.0.0.1", (ushort) 7071));
    this.active = true;
  }

  public void Poll()
  {
    if (!this.active)
      return;
    this.peer.UpdateReceive();
    this.peer.UpdateSent();
  }

  void IDataHandler.ReceiveMessage(IConnection connection, ArraySegment<byte> message)
  {
    this.MessageHandler.HandleMessage((INetworkPlayer) this.players[connection], message);
  }

  public void Send<T>(T message)
  {
    foreach (NetworkPlayer networkPlayer in this.players.Values)
      networkPlayer.Send<T>(message, Channel.Reliable);
  }
}

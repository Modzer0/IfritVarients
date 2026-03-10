// Decompiled with JetBrains decompiler
// Type: NuclearOption.Networking.ConnectOptions
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

#nullable disable
namespace NuclearOption.Networking;

public class ConnectOptions
{
  public readonly SocketType SocketType;
  public string Password;
  public readonly string UdpHost;
  public readonly int? UdpPort;
  public readonly string SteamLobbyIDString;

  public ConnectOptions(SocketType socketType, string udpHost = null, int? udpPort = null)
  {
    this.SocketType = socketType;
    this.UdpHost = udpHost;
    this.UdpPort = udpPort;
  }

  public ConnectOptions(SocketType socketType, string steamLobbyIDString)
  {
    this.SocketType = socketType;
    this.SteamLobbyIDString = steamLobbyIDString;
  }
}

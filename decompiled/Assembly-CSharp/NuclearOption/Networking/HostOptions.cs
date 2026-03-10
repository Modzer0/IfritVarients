// Decompiled with JetBrains decompiler
// Type: NuclearOption.Networking.HostOptions
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.SceneLoading;

#nullable disable
namespace NuclearOption.Networking;

public class HostOptions
{
  public readonly GameState GameState;
  public readonly SocketType SocketType;
  public int? MaxConnections;
  public readonly MapKey Map;
  public readonly string SystemScene;
  public string Password;
  public int? UdpPort;

  public HostOptions(SocketType socketType, GameState gameState, MapKey map)
  {
    this.GameState = gameState;
    this.SocketType = socketType;
    this.Map = map;
  }

  public HostOptions(SocketType socketType, GameState gameState, string systemScene)
  {
    this.GameState = gameState;
    this.SocketType = socketType;
    this.SystemScene = systemScene;
  }
}

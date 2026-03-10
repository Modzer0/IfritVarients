// Decompiled with JetBrains decompiler
// Type: NuclearOption.Networking.Authentication.PlayerTimeout
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Steamworks;

#nullable disable
namespace NuclearOption.Networking.Authentication;

public class PlayerTimeout
{
  public readonly CSteamID SteamId;
  public double LastDisconnectTime;
  public double LastJoinWhileTimeoutLogTime;
  public double LastErrorKickTime;
  public double TimeoutExpiry;
  public int ViolationLevel;
  public int ErrorKickCount;

  public PlayerTimeout(CSteamID id)
  {
    this.SteamId = id;
    this.LastDisconnectTime = 0.0;
    this.TimeoutExpiry = 0.0;
    this.LastErrorKickTime = 0.0;
  }
}

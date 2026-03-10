// Decompiled with JetBrains decompiler
// Type: NuclearOption.Networking.Lobbies.PlayerLobbyInstance
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Steamworks;

#nullable disable
namespace NuclearOption.Networking.Lobbies;

public class PlayerLobbyInstance : LobbyInstance
{
  private const string OWNER_KEY = "OWNER";
  private readonly CSteamID id;

  public PlayerLobbyInstance(CSteamID id) => this.id = id;

  protected override string GetData(string key) => SteamMatchmaking.GetLobbyData(this.id, key);

  public CSteamID LobbyId => this.id;

  public override bool DedicatedServer => false;

  public override string HostAddress => this.GetData(nameof (HostAddress));

  public override string UdpAddress => this.GetData("UDP_Address");

  public override string UdpPort => this.GetData("UDP_Port");

  public override int? CalculatePing()
  {
    int ping;
    return SteamLobby.EstimatePing(this.GetData("HostPing"), out ping) ? new int?(ping) : new int?();
  }

  public override bool GetPlayerCounts(out int current, out int max)
  {
    string lobbyData1 = SteamMatchmaking.GetLobbyData(this.id, "max_members");
    string lobbyData2 = SteamMatchmaking.GetLobbyData(this.id, "open_member_spots");
    if (string.IsNullOrEmpty(lobbyData1) || string.IsNullOrEmpty(lobbyData2))
    {
      current = SteamMatchmaking.GetNumLobbyMembers(this.id);
      max = SteamMatchmaking.GetLobbyMemberLimit(this.id);
      return true;
    }
    int result;
    if (int.TryParse(lobbyData2, out result) && int.TryParse(lobbyData1, out max))
    {
      current = max - result;
      return true;
    }
    current = 0;
    max = 0;
    return false;
  }

  public override bool IsPasswordProtected(out string shortPassword)
  {
    shortPassword = this.GetData("short_password");
    return !string.IsNullOrEmpty(shortPassword);
  }
}

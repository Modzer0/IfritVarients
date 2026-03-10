// Decompiled with JetBrains decompiler
// Type: NuclearOption.Networking.Lobbies.HostedLobbyInstance
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Steamworks;
using System;

#nullable disable
namespace NuclearOption.Networking.Lobbies;

public readonly struct HostedLobbyInstance(CSteamID id) : IEquatable<HostedLobbyInstance>
{
  public readonly CSteamID Id = id;

  public bool IsValid => this.Id.IsValid();

  public void SetData(string key, string value)
  {
    SteamMatchmaking.SetLobbyData(this.Id, key, value);
  }

  public void SetStartTime() => this.SetData("start_time", LobbyInstance.CreateStartTime());

  public void SetLobbyEnded() => this.SetData("name", "Game Ended");

  public void SetCurrentPlayers(int current, int max)
  {
    SteamMatchmaking.SetLobbyData(this.Id, "open_member_spots", (max - current).ToString());
  }

  public bool Equals(HostedLobbyInstance other) => this.Id == other.Id;

  public override bool Equals(object obj) => obj is HostedLobbyInstance other && this.Equals(other);

  public override int GetHashCode() => this.Id.GetHashCode();
}

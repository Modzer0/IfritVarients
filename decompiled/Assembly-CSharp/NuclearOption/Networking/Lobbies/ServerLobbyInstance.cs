// Decompiled with JetBrains decompiler
// Type: NuclearOption.Networking.Lobbies.ServerLobbyInstance
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Mirage;
using Steamworks;
using System;
using System.Collections.Generic;
using UnityEngine;

#nullable disable
namespace NuclearOption.Networking.Lobbies;

public class ServerLobbyInstance : LobbyInstance
{
  public const int MAX_CHUNKS_DESCRIPTION = 10;
  private readonly Dictionary<string, string> rules = new Dictionary<string, string>();
  private ExponentialMovingAverage pingAverage = new ExponentialMovingAverage(5);
  public float NextPingTime;

  public gameserveritem_t details { get; private set; }

  public int PingCount { get; private set; }

  public void SetDetails(gameserveritem_t details)
  {
    this.rules.Clear();
    this.details = details;
    this.SetPingResult(details.m_nPing);
    this.rules["has_password"] = LobbyInstance.BoolToTag(this.details.m_bPassword);
    try
    {
      DedicatedServerKeyValues.ParseTags(details.GetGameTags(), this.rules);
    }
    catch (Exception ex)
    {
      Debug.LogError((object) $"Failed to parse tags for server {this.details.m_steamID}. {ex}");
    }
  }

  public void SetRule(string key, string value)
  {
    try
    {
      DedicatedServerKeyValues.ParseKeyValue(key, value, this.rules);
    }
    catch (Exception ex)
    {
      Debug.LogError((object) $"Failed to parse rule for server {this.details.m_steamID}. {ex}");
    }
  }

  protected override string GetData(string key)
  {
    string str;
    return !this.rules.TryGetValue(key, out str) ? string.Empty : str;
  }

  protected override string LobbyName => this.details.GetServerName();

  public override string MissionDescriptionSanitized
  {
    get => DedicatedServerKeyValues.ParseDescription(this.rules);
  }

  public override bool DedicatedServer => true;

  public override string HostAddress => this.details.m_steamID.ToString();

  public override string UdpAddress => "";

  public override string UdpPort => "";

  public override int? CalculatePing() => new int?((int) this.pingAverage.Value);

  public override bool GetPlayerCounts(out int current, out int max)
  {
    current = this.details.m_nPlayers;
    max = this.details.m_nMaxPlayers;
    return true;
  }

  public override bool IsPasswordProtected(out string shortPassword)
  {
    shortPassword = this.GetData("short_password");
    return this.details.m_bPassword;
  }

  public void SetPingResult(int ping)
  {
    this.pingAverage.Add((double) ping);
    ++this.PingCount;
  }
}

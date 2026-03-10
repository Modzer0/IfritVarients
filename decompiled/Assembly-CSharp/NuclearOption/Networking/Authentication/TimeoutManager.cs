// Decompiled with JetBrains decompiler
// Type: NuclearOption.Networking.Authentication.TimeoutManager
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Mirage;
using Steamworks;
using System;
using System.Collections.Generic;
using UnityEngine;

#nullable disable
namespace NuclearOption.Networking.Authentication;

public class TimeoutManager
{
  private readonly Dictionary<CSteamID, PlayerTimeout> _players = new Dictionary<CSteamID, PlayerTimeout>();
  private readonly TimeoutConfig _config;

  public TimeoutManager(TimeoutConfig config) => this._config = config;

  public double Now => Time.unscaledTimeAsDouble;

  private PlayerTimeout GetPlayer(CSteamID steamId)
  {
    PlayerTimeout player1;
    if (this._players.TryGetValue(steamId, out player1))
      return player1;
    PlayerTimeout player2 = new PlayerTimeout(steamId);
    this._players[steamId] = player2;
    return player2;
  }

  public bool HasTimeout(CSteamID steamId)
  {
    PlayerTimeout player = this.GetPlayer(steamId);
    double now = this.Now;
    double num = player.TimeoutExpiry - now;
    if (0.0 >= num)
      return false;
    player.TimeoutExpiry += (double) this._config.SpamConnectPenaltySeconds;
    if (now - player.LastJoinWhileTimeoutLogTime > 60.0)
    {
      player.LastJoinWhileTimeoutLogTime = now;
      ColorLog<TimeoutManager>.Info($"{steamId} tried to join while timeout, timeoutDuration={num + (double) this._config.SpamConnectPenaltySeconds} seconds");
    }
    return true;
  }

  public void OnDisconnect(CSteamID steamId)
  {
    PlayerTimeout player = this.GetPlayer(steamId);
    if (this.Now - player.LastErrorKickTime < 1.0)
      return;
    if (player.LastDisconnectTime == 0.0)
    {
      ColorLog<TimeoutManager>.Info($"{steamId} first disconnect");
      player.LastDisconnectTime = this.Now;
    }
    else
    {
      double timeSinceLast = this.Now - player.LastDisconnectTime;
      if (timeSinceLast < (double) this._config.RapidDisconnectWindowSeconds)
      {
        ++player.ViolationLevel;
        if (player.ViolationLevel > this._config.FreeDisconnectAllowed)
        {
          float num = this.ApplyTimeout(player, this._config.BaseTimeoutSeconds);
          ColorLog<TimeoutManager>.Info($"{steamId} rapid disconnect, ViolationLevel={player.ViolationLevel} ErrorKickCount={player.ErrorKickCount} Timeout={num}s");
        }
        else
          ColorLog<TimeoutManager>.Info($"{steamId} rapid disconnect, ViolationLevel={player.ViolationLevel} ErrorKickCount={player.ErrorKickCount} no timeout");
      }
      else
      {
        int violationLevel = player.ViolationLevel;
        int errorKickCount = player.ErrorKickCount;
        int num = TimeoutManager.Decay(ref player.ViolationLevel, timeSinceLast, (double) this._config.PenaltyDecayRateSeconds) ? 1 : 0;
        bool flag = TimeoutManager.Decay(ref player.ErrorKickCount, timeSinceLast, (double) this._config.ErrorKickDecayRateSeconds);
        if (num != 0)
          ColorLog<TimeoutManager>.Info($"{steamId} violation level lowered, {violationLevel} -> {player.ViolationLevel}");
        if (flag)
          ColorLog<TimeoutManager>.Info($"{steamId} error level lowered, {errorKickCount} -> {player.ErrorKickCount}");
      }
      player.LastDisconnectTime = this.Now;
    }
  }

  public bool OnKickFromError(CSteamID steamId, PlayerErrorFlags errorFlag)
  {
    PlayerTimeout player = this.GetPlayer(steamId);
    if (this._config.CheckInstantBanErrorFlags(errorFlag))
      return true;
    ++player.ErrorKickCount;
    if (this._config.BanOnRepeatedErrorKicks && player.ErrorKickCount > this._config.MaxErrorKicksBeforeBan)
      return true;
    player.ViolationLevel += 2;
    float num = this.ApplyTimeout(player, this._config.ErrorKickBaseTimeout);
    ColorLog<TimeoutManager>.Info($"{steamId} kick from error, ViolationLevel={player.ViolationLevel} ErrorKickCount={player.ErrorKickCount} Timeout={num}s");
    player.LastErrorKickTime = this.Now;
    player.LastDisconnectTime = this.Now;
    return false;
  }

  public void AddCustomTimeout(CSteamID steamId, int increaseViolationLevel, float baseSeconds)
  {
    PlayerTimeout player = this.GetPlayer(steamId);
    player.ViolationLevel += increaseViolationLevel;
    double num = (double) this.ApplyTimeout(player, baseSeconds);
  }

  private static bool Decay(ref int level, double timeSinceLast, double decayRate)
  {
    if (level <= 0)
      return false;
    int num1 = (int) (timeSinceLast / decayRate);
    int num2 = level - num1;
    if (num2 < 0)
      num2 = 0;
    int num3 = level != num2 ? 1 : 0;
    level = num2;
    return num3 != 0;
  }

  private float ApplyTimeout(PlayerTimeout player, float baseSeconds)
  {
    int num1 = player.ViolationLevel - this._config.FreeDisconnectAllowed;
    float val1 = num1 > 1 ? baseSeconds * Mathf.Pow(this._config.TimeoutMultiplier, (float) (num1 - 1)) : baseSeconds;
    float num2 = !float.IsFinite(val1) ? this._config.MaxTimeoutSeconds : Math.Min(val1, this._config.MaxTimeoutSeconds);
    if (player.TimeoutExpiry >= this.Now)
      ColorLog<TimeoutManager>.LogError("Should not be applying timeout if player is already timeout");
    player.TimeoutExpiry = this.Now + (double) num2;
    return num2;
  }
}

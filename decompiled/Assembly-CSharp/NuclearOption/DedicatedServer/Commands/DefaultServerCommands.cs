// Decompiled with JetBrains decompiler
// Type: NuclearOption.DedicatedServer.Commands.DefaultServerCommands
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Mirage;
using NuclearOption.Chat;
using NuclearOption.Networking;
using NuclearOption.Networking.Authentication;
using System;
using System.Collections.Generic;
using UnityEngine;

#nullable disable
namespace NuclearOption.DedicatedServer.Commands;

public static class DefaultServerCommands
{
  public static List<ServerCommand> CreateCommands()
  {
    return new List<ServerCommand>()
    {
      new ServerCommand("update-ready", new ServerCommand.CommandDelegate(DefaultServerCommands.RunUpdateReady)),
      new ServerCommand("send-chat-message", new ServerCommand.CommandDelegate(DefaultServerCommands.RunSendChatMessage)),
      new ServerCommand("reload-config", new ServerCommand.CommandDelegate(DefaultServerCommands.RunReloadConfig)),
      new ServerCommand("get-mission-time", new ServerCommand.CommandDelegate(DefaultServerCommands.RunGetMissionTime)),
      new ServerCommand("get-mission", new ServerCommand.CommandDelegate(DefaultServerCommands.RunGetMission)),
      new ServerCommand("set-time-remaining", new ServerCommand.CommandDelegate(DefaultServerCommands.RunSetTimeRemaining)),
      new ServerCommand("set-next-mission", new ServerCommand.CommandDelegate(DefaultServerCommands.RunSetNextMission)),
      new ServerCommand("get-player-list", new ServerCommand.CommandDelegate(DefaultServerCommands.RunGetPlayerList))
    };
  }

  private static CommandResponse RunUpdateReady(ServerRemoteCommands server, string[] arguments)
  {
    DedicatedServerManager.UpdateReady = true;
    return CommandResponse.Create(StatusCode.Success);
  }

  private static CommandResponse RunSendChatMessage(ServerRemoteCommands server, string[] arguments)
  {
    if (arguments.Length < 1)
      return CommandResponse.Create(StatusCode.BadArguments, "Expected Arguments [string message]");
    server.RunOnMainThread((Action) (() => NetworkSceneSingleton<ChatManager>.i.RpcServerMessage(arguments[0], false)));
    return CommandResponse.Create(StatusCode.Success);
  }

  private static CommandResponse RunReloadConfig(ServerRemoteCommands server, string[] arguments)
  {
    string newConfigPath = arguments.Length != 0 ? arguments[0] : (string) null;
    DedicatedServerConfig newConfig = (DedicatedServerConfig) null;
    if (!string.IsNullOrEmpty(newConfigPath) && !DedicatedServerConfig.TryLoad(newConfigPath, out newConfig))
      return CommandResponse.Create(StatusCode.BadArguments, "could not find new config at: " + newConfigPath);
    server.RunOnMainThread((Action) (() => DedicatedServerManager.Instance.ReloadConfig(newConfig, newConfigPath)));
    return CommandResponse.Create(StatusCode.Success);
  }

  private static CommandResponse RunGetMissionTime(ServerRemoteCommands server, string[] arguments)
  {
    DedicatedServerManager instance = DedicatedServerManager.Instance;
    DefaultServerCommands.GetTimeRemainingResult body = new DefaultServerCommands.GetTimeRemainingResult();
    if (instance.HasPlayers())
    {
      MissionOptions currentMissionOption = instance.CurrentMissionOption;
      body.currentTime = Time.timeSinceLevelLoad;
      body.maxTime = currentMissionOption.MaxTime;
    }
    return CommandResponse.Create<DefaultServerCommands.GetTimeRemainingResult>(StatusCode.Success, body);
  }

  private static CommandResponse RunGetMission(ServerRemoteCommands server, string[] arguments)
  {
    DedicatedServerManager instance = DedicatedServerManager.Instance;
    return CommandResponse.Create<DefaultServerCommands.GetMissionResult>(StatusCode.Success, new DefaultServerCommands.GetMissionResult()
    {
      currentMission = instance.CurrentMissionOption,
      nextMission = instance.NextMissionOption
    });
  }

  private static CommandResponse RunSetTimeRemaining(
    ServerRemoteCommands server,
    string[] arguments)
  {
    if (arguments.Length == 0)
      return CommandResponse.Create(StatusCode.BadArguments, "Expected Arguments [float RemainingTime]");
    float remainingTime;
    if (!float.TryParse(arguments[0], out remainingTime))
      return CommandResponse.Create(StatusCode.BadArguments, $"Failed to parse {arguments[0]} as float RemainingTime");
    server.RunOnMainThread((Action) (() => DedicatedServerManager.Instance.SetTimeRemaining(remainingTime)));
    return CommandResponse.Create(StatusCode.Success);
  }

  private static CommandResponse RunSetNextMission(ServerRemoteCommands server, string[] arguments)
  {
    if (arguments.Length <= 2)
      return CommandResponse.Create(StatusCode.BadArguments, "Expected Arguments [string Group, string Name, float MaxTime]");
    string str1 = arguments[0];
    string str2 = arguments[1];
    float result;
    if (!float.TryParse(arguments[2], out result))
      return CommandResponse.Create(StatusCode.BadArguments, $"Failed to parse {arguments[2]} as float MaxTime");
    MissionOptions missionOption = new MissionOptions()
    {
      Key = new MissionKeySaveable()
      {
        Group = str1,
        Name = str2
      },
      MaxTime = result
    };
    server.RunOnMainThread((Action) (() => DedicatedServerManager.Instance.SetNextMission(missionOption)));
    return CommandResponse.Create(StatusCode.Success);
  }

  private static CommandResponse RunGetPlayerList(ServerRemoteCommands server, string[] arguments)
  {
    (bool ok, DefaultServerCommands.GetPlayerListResult playerListResult1) = server.RunOnMainThreadBlocking<DefaultServerCommands.GetPlayerListResult>((Func<(bool, DefaultServerCommands.GetPlayerListResult)>) (() =>
    {
      List<INetworkPlayer> networkPlayerList = new List<INetworkPlayer>();
      foreach (INetworkPlayer authenticatedPlayer in (IEnumerable<INetworkPlayer>) NetworkManagerNuclearOption.i.Server.AuthenticatedPlayers)
      {
        if ((!DedicatedServerManager.IsRunning || !authenticatedPlayer.IsHost) && !authenticatedPlayer.TryGetPlayer<DedicatedServerPlayer>(out DedicatedServerPlayer _))
          networkPlayerList.Add(authenticatedPlayer);
      }
      DefaultServerCommands.GetPlayerListResult playerListResult2 = new DefaultServerCommands.GetPlayerListResult()
      {
        Players = new DefaultServerCommands.GetPlayerListResultItem[networkPlayerList.Count]
      };
      for (int index = 0; index < networkPlayerList.Count; ++index)
      {
        INetworkPlayer networkPlayer = networkPlayerList[index];
        NetworkAuthenticatorNuclearOption.AuthData authData = networkPlayer.GetAuthData();
        DefaultServerCommands.GetPlayerListResultItem playerListResultItem = new DefaultServerCommands.GetPlayerListResultItem()
        {
          steamId = authData.SteamID.ToString()
        };
        Player player;
        if (networkPlayer.TryGetPlayer<Player>(out player))
        {
          playerListResultItem.displayName = player.PlayerName;
          if (playerListResultItem.displayName.Length > 64 /*0x40*/)
            playerListResultItem.displayName = playerListResultItem.displayName.Substring(0, 64 /*0x40*/);
          playerListResultItem.faction = !((UnityEngine.Object) player.HQ != (UnityEngine.Object) null) ? "None" : player.HQ.faction.factionName;
        }
        playerListResult2.Players[index] = playerListResultItem;
      }
      return (true, playerListResult2);
    }));
    return ok ? CommandResponse.Create<DefaultServerCommands.GetPlayerListResult>(StatusCode.Success, playerListResult1) : CommandResponse.Create(StatusCode.InternalServerError);
  }

  [Serializable]
  public struct GetTimeRemainingResult
  {
    public float currentTime;
    public float maxTime;
  }

  [Serializable]
  public struct GetMissionResult
  {
    public MissionOptions currentMission;
    public MissionOptions nextMission;
  }

  [Serializable]
  public struct GetPlayerListResult
  {
    public DefaultServerCommands.GetPlayerListResultItem[] Players;
  }

  [Serializable]
  public struct GetPlayerListResultItem
  {
    public string steamId;
    public string displayName;
    public string faction;
  }
}

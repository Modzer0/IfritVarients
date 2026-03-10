// Decompiled with JetBrains decompiler
// Type: NuclearOption.DedicatedServer.Commands.BanServerCommands
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Mirage;
using NuclearOption.Networking;
using Steamworks;
using System;
using System.Collections.Generic;
using UnityEngine;

#nullable disable
namespace NuclearOption.DedicatedServer.Commands;

public static class BanServerCommands
{
  public static List<ServerCommand> CreateCommands()
  {
    return new List<ServerCommand>()
    {
      new ServerCommand("kick-player", new ServerCommand.CommandDelegate(BanServerCommands.RunKickPlayer)),
      new ServerCommand("unkick-player", new ServerCommand.CommandDelegate(BanServerCommands.RunUnKickPlayer)),
      new ServerCommand("clear-kicked-player", new ServerCommand.CommandDelegate(BanServerCommands.RunClearKickedPlayers)),
      new ServerCommand("clear-kicked-players", new ServerCommand.CommandDelegate(BanServerCommands.RunClearKickedPlayers)),
      new ServerCommand("banlist-reload", new ServerCommand.CommandDelegate(BanServerCommands.RunBanListReload)),
      new ServerCommand("banlist-add", new ServerCommand.CommandDelegate(BanServerCommands.RunBanListAdd)),
      new ServerCommand("banlist-remove", new ServerCommand.CommandDelegate(BanServerCommands.RunBanListRemove)),
      new ServerCommand("banlist-clear", new ServerCommand.CommandDelegate(BanServerCommands.RunBanListClear))
    };
  }

  private static bool ParseSteamId(
    string[] arguments,
    int index,
    out CSteamID id,
    out CommandResponse errorResponse)
  {
    if (arguments.Length <= index)
    {
      Debug.LogWarning((object) $"Wrong number of arguments, expected atleast {index} arguments");
      id = new CSteamID();
      errorResponse = CommandResponse.Create(StatusCode.BadArguments, $"expected atleast {index} arguments");
      return false;
    }
    ulong result;
    if (!ulong.TryParse(arguments[0], out result))
    {
      Debug.LogWarning((object) "Failed to parse arg[0] as ulong SteamID");
      id = new CSteamID();
      errorResponse = CommandResponse.Create(StatusCode.BadArguments, $"failed to parse {arguments[0]} as ulong SteamID");
      return false;
    }
    id = new CSteamID(result);
    errorResponse = new CommandResponse();
    return true;
  }

  private static bool CheckBanListFileExists(out CommandResponse errorResponse)
  {
    if (DedicatedServerManager.Instance.Config.BanListPaths.Length == 0)
    {
      Debug.LogWarning((object) "Could not modify ban list because server config had no ban list files");
      errorResponse = CommandResponse.Create(StatusCode.ConfigError, "No ban files because `Config.BanListPaths` was empty");
      return false;
    }
    errorResponse = new CommandResponse();
    return true;
  }

  private static string SafeGet(string[] arguments, int index, string defaultValue)
  {
    return arguments.Length <= index ? defaultValue : arguments[index];
  }

  private static bool OptionalBool(
    string[] arguments,
    int index,
    bool defaultValue,
    out bool value,
    out CommandResponse errorResponse)
  {
    if (arguments.Length <= index)
    {
      value = defaultValue;
      errorResponse = new CommandResponse();
      return true;
    }
    if (bool.TryParse(arguments[index], out value))
    {
      errorResponse = new CommandResponse();
      return true;
    }
    errorResponse = CommandResponse.Create(StatusCode.BadArguments, $"failed to parse {arguments[1]} as bool");
    return false;
  }

  private static CommandResponse RunClearKickedPlayers(
    ServerRemoteCommands server,
    string[] arguments)
  {
    server.RunOnMainThread((Action) (() => NetworkManagerNuclearOption.i.Authenticator.ClearKickList()));
    return CommandResponse.Create(StatusCode.Success);
  }

  private static CommandResponse RunKickPlayer(ServerRemoteCommands server, string[] arguments)
  {
    CommandResponse errorResponse;
    CSteamID steamId;
    if (!BanServerCommands.ParseSteamId(arguments, 0, out steamId, out errorResponse))
      return errorResponse;
    server.RunOnMainThread((Action) (() =>
    {
      if (BanServerCommands.KickPlayer(steamId))
        return;
      Debug.LogWarning((object) $"RunKickPlayer failed to find user with id={steamId}, but adding them to kicked list so they can not rejoin");
      NetworkManagerNuclearOption.i.Authenticator.AddKicked(steamId);
    }));
    return CommandResponse.Create(StatusCode.Success);
  }

  private static bool KickPlayer(CSteamID steamId)
  {
    foreach (INetworkPlayer authenticatedPlayer in (IEnumerable<INetworkPlayer>) NetworkManagerNuclearOption.i.Server.AuthenticatedPlayers)
    {
      if (authenticatedPlayer.GetAuthData().SteamID == steamId)
      {
        Player player;
        if (authenticatedPlayer.TryGetPlayer<Player>(out player))
          NetworkManagerNuclearOption.i.KickPlayerAsync(player).Forget();
        else
          NetworkManagerNuclearOption.i.KickPlayer(authenticatedPlayer);
        Debug.Log((object) $"RunKickPlayer kicking {player}");
        return true;
      }
    }
    return false;
  }

  private static CommandResponse RunUnKickPlayer(ServerRemoteCommands server, string[] arguments)
  {
    CommandResponse errorResponse;
    CSteamID steamId;
    if (!BanServerCommands.ParseSteamId(arguments, 0, out steamId, out errorResponse))
      return errorResponse;
    server.RunOnMainThread((Action) (() => NetworkManagerNuclearOption.i.Authenticator.RemoveKicked(steamId)));
    return CommandResponse.Create(StatusCode.Success);
  }

  private static CommandResponse RunBanListReload(ServerRemoteCommands server, string[] arguments)
  {
    server.RunOnMainThread((Action) (() => DedicatedServerManager.Instance.LoadAllowBanList()));
    return CommandResponse.Create(StatusCode.Success);
  }

  private static CommandResponse RunBanListAdd(ServerRemoteCommands server, string[] arguments)
  {
    CommandResponse errorResponse1;
    CSteamID steamId;
    if (!BanServerCommands.ParseSteamId(arguments, 0, out steamId, out errorResponse1))
      return errorResponse1;
    string reason = BanServerCommands.SafeGet(arguments, 1, (string) null);
    reason = reason?.Replace("\r", "").Replace("\n", " ");
    CommandResponse errorResponse2;
    if (!BanServerCommands.CheckBanListFileExists(out errorResponse2))
      return errorResponse2;
    server.RunOnMainThread((Action) (() =>
    {
      NetworkManagerNuclearOption.i.Authenticator.BanList.Add(steamId, reason);
      AllowBanList.AppendId(DedicatedServerManager.Instance.Config.BanListPaths[0], steamId, reason);
      BanServerCommands.KickPlayer(steamId);
    }));
    return CommandResponse.Create(StatusCode.Success);
  }

  private static CommandResponse RunBanListRemove(ServerRemoteCommands server, string[] arguments)
  {
    CommandResponse errorResponse1;
    CSteamID steamId;
    if (!BanServerCommands.ParseSteamId(arguments, 0, out steamId, out errorResponse1))
      return errorResponse1;
    CommandResponse errorResponse2;
    if (!BanServerCommands.CheckBanListFileExists(out errorResponse2))
      return errorResponse2;
    server.RunOnMainThread((Action) (() =>
    {
      NetworkManagerNuclearOption.i.Authenticator.BanList.Remove(steamId);
      AllowBanList.RemoveId(DedicatedServerManager.Instance.Config.BanListPaths[0], steamId);
    }));
    return CommandResponse.Create(StatusCode.Success);
  }

  private static CommandResponse RunBanListClear(ServerRemoteCommands server, string[] arguments)
  {
    server.RunOnMainThread((Action) (() => NetworkManagerNuclearOption.i.Authenticator.BanList.Clear()));
    return CommandResponse.Create(StatusCode.Success);
  }
}

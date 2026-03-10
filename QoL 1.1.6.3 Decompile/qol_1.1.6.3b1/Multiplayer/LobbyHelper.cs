// Decompiled with JetBrains decompiler
// Type: qol.Multiplayer.LobbyHelper
// Assembly: qol, Version=1.1.6.3, Culture=neutral, PublicKeyToken=null
// MVID: 3B6539EA-E38C-45EE-8FFF-FC58CC42BADC
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\BepInEx\plugins\qol_1.1.6.3b1.dll

using BepInEx.Logging;
using HarmonyLib;
using NuclearOption.Networking.Lobbies;
using Steamworks;
using System;
using System.Reflection;

#nullable disable
namespace qol.Multiplayer;

public static class LobbyHelper
{
  private static ManualLogSource _logger;

  public static void Initialize(ManualLogSource logger) => LobbyHelper._logger = logger;

  public static CSteamID GetCurrentLobby()
  {
    LobbyHelper._logger?.LogWarning((object) "[LobbyHelper] GetCurrentLobby called");
    try
    {
      if ((UnityEngine.Object) SteamLobby.instance == (UnityEngine.Object) null)
      {
        LobbyHelper._logger?.LogError((object) "[LobbyHelper] SteamLobby instance is null");
        return new CSteamID();
      }
      LobbyHelper._logger?.LogWarning((object) "[LobbyHelper] SteamLobby instance found, checking hosted lobby...");
      HostedLobbyInstance hostedLobbyInstance = (HostedLobbyInstance) AccessTools.Field(typeof (SteamLobby), "_hostedLobby").GetValue((object) SteamLobby.instance);
      if (hostedLobbyInstance.IsValid)
      {
        LobbyHelper._logger?.LogWarning((object) $"[LobbyHelper] Hosting valid lobby: {hostedLobbyInstance.Id.m_SteamID}");
        return hostedLobbyInstance.Id;
      }
      LobbyHelper._logger?.LogWarning((object) "[LobbyHelper] No valid hosted lobby found");
      LobbyHelper._logger?.LogWarning((object) "[LobbyHelper] Checking joined lobby...");
      LobbyInstance lobbyInstance = (LobbyInstance) AccessTools.Field(typeof (SteamLobby), "_joinedLobby").GetValue((object) SteamLobby.instance);
      if (lobbyInstance != null)
      {
        LobbyHelper._logger?.LogWarning((object) ("[LobbyHelper] Joined lobby type: " + lobbyInstance.GetType().Name));
        switch (lobbyInstance)
        {
          case PlayerLobbyInstance playerLobbyInstance:
            LobbyHelper._logger?.LogWarning((object) "[LobbyHelper] Processing PlayerLobbyInstance");
            PropertyInfo propertyInfo = AccessTools.Property(typeof (PlayerLobbyInstance), "LobbyId");
            if (propertyInfo != (PropertyInfo) null)
            {
              CSteamID currentLobby = (CSteamID) propertyInfo.GetValue((object) playerLobbyInstance);
              LobbyHelper._logger?.LogWarning((object) $"[LobbyHelper] Player lobby ID: {currentLobby.m_SteamID}");
              if (currentLobby.m_SteamID != 0UL)
              {
                LobbyHelper._logger?.LogWarning((object) $"[LobbyHelper] Returning valid player lobby: {currentLobby.m_SteamID}");
                return currentLobby;
              }
              break;
            }
            ManualLogSource logger1 = LobbyHelper._logger;
            if (logger1 != null)
            {
              logger1.LogError((object) "[LobbyHelper] LobbyId property not found in PlayerLobbyInstance");
              break;
            }
            break;
          case ServerLobbyInstance serverLobbyInstance:
            LobbyHelper._logger?.LogWarning((object) "[LobbyHelper] Processing ServerLobbyInstance");
            FieldInfo fieldInfo1 = AccessTools.Field(typeof (ServerLobbyInstance), "details");
            if (fieldInfo1 != (FieldInfo) null)
            {
              object obj = fieldInfo1.GetValue((object) serverLobbyInstance);
              if (obj != null)
              {
                FieldInfo fieldInfo2 = AccessTools.Field(obj.GetType(), "m_steamID");
                if (fieldInfo2 != (FieldInfo) null)
                {
                  CSteamID currentLobby = (CSteamID) fieldInfo2.GetValue(obj);
                  LobbyHelper._logger?.LogWarning((object) $"[LobbyHelper] Server lobby ID: {currentLobby.m_SteamID}");
                  if (currentLobby.m_SteamID != 0UL)
                  {
                    LobbyHelper._logger?.LogWarning((object) $"[LobbyHelper] Returning valid server lobby: {currentLobby.m_SteamID}");
                    return currentLobby;
                  }
                  break;
                }
                ManualLogSource logger2 = LobbyHelper._logger;
                if (logger2 != null)
                {
                  logger2.LogError((object) "[LobbyHelper] m_steamID field not found in lobby details");
                  break;
                }
                break;
              }
              ManualLogSource logger3 = LobbyHelper._logger;
              if (logger3 != null)
              {
                logger3.LogError((object) "[LobbyHelper] Lobby details are null");
                break;
              }
              break;
            }
            ManualLogSource logger4 = LobbyHelper._logger;
            if (logger4 != null)
            {
              logger4.LogError((object) "[LobbyHelper] Details field not found in ServerLobbyInstance");
              break;
            }
            break;
        }
      }
      else
        LobbyHelper._logger?.LogWarning((object) "[LobbyHelper] No joined lobby found");
      LobbyHelper._logger?.LogWarning((object) "[LobbyHelper] Not in any valid lobby");
      return new CSteamID();
    }
    catch (Exception ex)
    {
      LobbyHelper._logger?.LogError((object) $"[LobbyHelper] Exception getting lobby SteamID: {ex}");
      return new CSteamID();
    }
  }

  public static void RefreshLobbyData(CSteamID lobbyId)
  {
    LobbyHelper._logger?.LogWarning((object) $"[LobbyHelper] RefreshLobbyData called for lobby: {lobbyId.m_SteamID}");
    try
    {
      string lobbyData = SteamMatchmaking.GetLobbyData(lobbyId, "name");
      LobbyHelper._logger?.LogWarning((object) ("[LobbyHelper] Current lobby name: " + lobbyData));
      SteamMatchmaking.SetLobbyData(lobbyId, "name", lobbyData);
      LobbyHelper._logger?.LogWarning((object) $"[LobbyHelper] Refreshed lobby data for lobby {lobbyId.m_SteamID}");
    }
    catch (Exception ex)
    {
      LobbyHelper._logger?.LogError((object) $"[LobbyHelper] Refresh failed: {ex}");
    }
  }
}

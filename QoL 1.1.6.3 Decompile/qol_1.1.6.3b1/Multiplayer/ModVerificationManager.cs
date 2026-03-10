// Decompiled with JetBrains decompiler
// Type: qol.Multiplayer.ModVerificationManager
// Assembly: qol, Version=1.1.6.3, Culture=neutral, PublicKeyToken=null
// MVID: 3B6539EA-E38C-45EE-8FFF-FC58CC42BADC
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\BepInEx\plugins\qol_1.1.6.3b1.dll

using BepInEx.Logging;
using System;
using System.Collections.Generic;

#nullable disable
namespace qol.Multiplayer;

public static class ModVerificationManager
{
  private static ManualLogSource _logger;
  private static bool _isInitialized = false;
  private static readonly Dictionary<ulong, bool> _pendingMemberDataVerifications = new Dictionary<ulong, bool>();
  private static readonly Dictionary<ulong, Action<bool>> _pendingVerificationCallbacks = new Dictionary<ulong, Action<bool>>();

  public static void Initialize(ManualLogSource logger)
  {
    if (ModVerificationManager._isInitialized)
      return;
    ModVerificationManager._logger = logger;
    ModVerificationManager._isInitialized = true;
    ModVerificationManager._logger?.LogWarning((object) "[ModVerificationManager] Initialized");
  }

  public static void WaitForMemberDataVerification(ulong steamId, Action<bool> callback)
  {
    ModVerificationManager._pendingVerificationCallbacks[steamId] = callback;
    ModVerificationManager._pendingMemberDataVerifications[steamId] = false;
  }

  public static void OnMemberDataUpdated(ulong steamId, bool success)
  {
    Action<bool> action;
    if (!ModVerificationManager._pendingVerificationCallbacks.TryGetValue(steamId, out action))
      return;
    action(success);
    ModVerificationManager._pendingVerificationCallbacks.Remove(steamId);
    ModVerificationManager._pendingMemberDataVerifications.Remove(steamId);
  }

  public static bool IsVerificationPending(ulong steamId)
  {
    bool flag;
    return ModVerificationManager._pendingMemberDataVerifications.TryGetValue(steamId, out flag) && !flag;
  }

  public static void MarkVerificationReceived(ulong steamId)
  {
    if (!ModVerificationManager._pendingMemberDataVerifications.ContainsKey(steamId))
      return;
    ModVerificationManager._pendingMemberDataVerifications[steamId] = true;
  }
}

// Decompiled with JetBrains decompiler
// Type: GameManager
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Mirage.Events;
using NuclearOption.AddressableScripts;
using NuclearOption.Jobs;
using NuclearOption.Networking;
using Rewired;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Jobs.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;

#nullable disable
public class GameManager
{
  public static Rewired.Player playerInput;
  public static bool IsPlayingFromEditor;
  public static bool IsHeadless;
  public static int? OverrideTargetFrameRate;
  public static AddLateEvent _onGameStateChanged = new AddLateEvent();
  public static readonly List<ISceneSingleton> SceneSingletons = new List<ISceneSingleton>();
  public static Dictionary<AircraftDefinition, AircraftCustomization> aircraftCustomization = new Dictionary<AircraftDefinition, AircraftCustomization>();
  public static int playerLivery;
  public static Transform playerSpawnPoint;
  public static Rewired.UI.ControlMapper.ControlMapper controlMapper;
  public static GameObject controlMapperCanvas;
  public static bool flightControlsEnabled = true;
  public static EventSystem eventSystem;
  private static BasePlayer _localPlayer;

  private GameManager()
  {
  }

  public static GameState gameState { get; private set; }

  public static bool ShowEffects
  {
    [MethodImpl(MethodImplOptions.AggressiveInlining)] get => !GameManager.IsHeadless;
  }

  public static IAddLateEvent OnGameStateChanged => (IAddLateEvent) GameManager._onGameStateChanged;

  public static GameResolution gameResolution { get; private set; }

  public static DisconnectInfo disconnectInfo { get; private set; }

  public static void SetLocalPlayer(BasePlayer player) => GameManager._localPlayer = player;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool GetLocalPlayer<T>(out T localPlayer) where T : BasePlayer
  {
    localPlayer = GameManager._localPlayer as T;
    return (Object) localPlayer != (Object) null;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool GetLocalHQ(out FactionHQ localHq)
  {
    NuclearOption.Networking.Player localPlayer;
    if (GameManager.GetLocalPlayer<NuclearOption.Networking.Player>(out localPlayer))
    {
      localHq = localPlayer.HQ;
      return (Object) localHq != (Object) null;
    }
    localHq = (FactionHQ) null;
    return false;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool GetLocalFaction(out Faction localFaction)
  {
    FactionHQ localHq;
    if (GameManager.GetLocalHQ(out localHq))
    {
      localFaction = localHq.faction;
      return true;
    }
    localFaction = (Faction) null;
    return false;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool GetLocalAircraft(out Aircraft localAircraft)
  {
    NuclearOption.Networking.Player localPlayer;
    if (GameManager.GetLocalPlayer<NuclearOption.Networking.Player>(out localPlayer))
    {
      localAircraft = localPlayer.Aircraft;
      return (Object) localAircraft != (Object) null;
    }
    localAircraft = (Aircraft) null;
    return false;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool GetLocalPilotDismounted(out PilotDismounted pilot)
  {
    NuclearOption.Networking.Player localPlayer;
    if (GameManager.GetLocalPlayer<NuclearOption.Networking.Player>(out localPlayer))
    {
      pilot = localPlayer.PilotDismounted;
      return (Object) pilot != (Object) null;
    }
    pilot = (PilotDismounted) null;
    return false;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsLocalPlayer<T>(T playerToCheck) where T : BasePlayer
  {
    T localPlayer;
    return GameManager.GetLocalPlayer<T>(out localPlayer) && (Object) playerToCheck == (Object) localPlayer;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsLocalAircraft(Unit unitToCheck)
  {
    return unitToCheck is Aircraft aircraftToCheck && GameManager.IsLocalAircraft(aircraftToCheck);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsLocalAircraft(Aircraft aircraftToCheck)
  {
    Aircraft localAircraft;
    return GameManager.GetLocalAircraft(out localAircraft) && (Object) aircraftToCheck == (Object) localAircraft;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool IsLocalHQ(FactionHQ hqToCheck)
  {
    FactionHQ localHq;
    return GameManager.GetLocalHQ(out localHq) && (Object) hqToCheck == (Object) localHq;
  }

  public static void FinishGame(GameResolution resolution)
  {
    if (GameManager.gameResolution != GameResolution.Ongoing)
    {
      Debug.LogWarning((object) $"GameResolution set twice. CurrentValue={GameManager.gameResolution} newValue={resolution}");
    }
    else
    {
      ColorLog<GameResolution>.Info($"FinishGame {resolution}");
      GameManager.gameResolution = resolution;
    }
  }

  public static void ResetGameResolution()
  {
    ColorLog<GameResolution>.Info(nameof (ResetGameResolution));
    GameManager.gameResolution = GameResolution.Ongoing;
  }

  public static void SetGameState(GameState gameState)
  {
    GameManager.gameState = gameState;
    GameManager._onGameStateChanged.Invoke();
    GameManager.LimitFrameRate(gameState);
    CursorManager.SetFlag(CursorFlags.NotInGame, !gameState.IsSingleOrMultiplayer());
    if (gameState != GameState.Menu)
      return;
    GameManager.IsPlayingFromEditor = false;
  }

  public static void ClearDisconnectReason() => GameManager.disconnectInfo = (DisconnectInfo) null;

  public static void SetDisconnectReason(DisconnectInfo disconnectInfo)
  {
    if (GameManager.disconnectInfo != null && GameManager.disconnectInfo.ShowReason && disconnectInfo.ShowReason)
      GameManager.disconnectInfo.Merge(disconnectInfo);
    else
      GameManager.disconnectInfo = disconnectInfo;
  }

  public static void LimitFrameRate(GameState gameState)
  {
    bool flag;
    switch (gameState)
    {
      case GameState.SinglePlayer:
      case GameState.Multiplayer:
      case GameState.Editor:
      case GameState.Encyclopedia:
        flag = false;
        break;
      case GameState.ServerWaiting:
        Application.targetFrameRate = 5;
        return;
      default:
        flag = true;
        break;
    }
    Application.targetFrameRate = !GameManager.OverrideTargetFrameRate.HasValue ? (GameManager.IsHeadless | flag ? 60 : -1) : GameManager.OverrideTargetFrameRate.Value;
  }

  public static void PreSetupGame()
  {
    GameManager.IsHeadless = SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null;
    GameManager.SetJobCount();
  }

  public static void SetupGame()
  {
    GameManager.playerInput = ReInput.players.GetPlayer(0);
    PlayerSettings.LoadPrefs();
    PlayerSettings.ApplyPrefs();
    MusicManager.ResetPlayedMusic();
    HitValidator.Initialize();
    GameManager.ResetGame();
    if (GameManager.gameState == GameState.Editor)
      return;
    GameManager.ResetGameResolution();
  }

  private static void SetJobCount()
  {
    int workerMaximumCount = JobsUtility.JobWorkerMaximumCount;
    if (workerMaximumCount <= 2)
      return;
    int num = Mathf.Clamp((workerMaximumCount + 1) / 2, 2, 8);
    JobsUtility.JobWorkerCount = num;
    Debug.Log((object) $"Setting Job worker Count to {num}");
  }

  public static void ResetGame()
  {
    for (int index = 0; index < GameManager.SceneSingletons.Count; ++index)
    {
      if (GameManager.SceneSingletons[index].ClearInstance())
        GameManager.SceneSingletons.RemoveAt(index);
    }
    JobsAllocatorShared.SortAll();
    ModLoadCache.Clear();
    FactionRegistry.Clear();
    UnitRegistry.Clear();
    BattlefieldGrid.Clear();
    GameManager.aircraftCustomization.Clear();
    GameManager.playerSpawnPoint = (Transform) null;
    GameManager.SetLocalPlayer((BasePlayer) null);
  }
}

// Decompiled with JetBrains decompiler
// Type: GameplayUI
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Cysharp.Threading.Tasks;
using Rewired;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

#nullable disable
public class GameplayUI : SceneSingleton<GameplayUI>
{
  public Image hurt;
  public Canvas gameplayCanvas;
  public Canvas menuCanvas;
  public float pilotHitPoints;
  public DialogueBox DialogueBox;
  public MessageUI MessageUI;
  public Transform topPanelTransform;
  [SerializeField]
  private GameObject selectAirbasePanel;
  [SerializeField]
  private TMP_Text airbaseName;
  [SerializeField]
  private UnityEngine.UI.Button selectAircraftButton;
  [SerializeField]
  private GameObject aircraftSelectionMenu;
  [SerializeField]
  private GameObject joinMenu;
  [SerializeField]
  private GameObject spectatorPanel;
  private Airbase homeAirbase;
  private float lastResumed;
  private Rewired.Player player;
  [SerializeField]
  private GameObject factionInfoPanel_BDF;
  [SerializeField]
  private GameObject factionInfoPanel_PALA;

  public static bool GameIsPaused { get; private set; } = false;

  public static bool GameSlowMotion { get; private set; } = false;

  public static bool AllowPauseKeybind { get; set; } = true;

  protected override void Awake()
  {
    base.Awake();
    GameplayUI.GameIsPaused = false;
    GameplayUI.GameSlowMotion = false;
    GameplayUI.AllowPauseKeybind = true;
    this.player = ReInput.players.GetPlayer(0);
    this.HideSpectatorPanel();
  }

  public void ShowSelectAirbase()
  {
    this.selectAirbasePanel.SetActive(true);
    bool flag = false;
    NuclearOption.Networking.Player localPlayer;
    if (GameManager.gameResolution == GameResolution.Ongoing && GameManager.GetLocalPlayer<NuclearOption.Networking.Player>(out localPlayer) && (UnityEngine.Object) localPlayer.HQ != (UnityEngine.Object) null && (UnityEngine.Object) SceneSingleton<CombatHUD>.i.aircraft == (UnityEngine.Object) null && !localPlayer.AircraftSpawnPending && (UnityEngine.Object) this.homeAirbase != (UnityEngine.Object) null)
    {
      this.airbaseName.text = this.homeAirbase.SavedAirbase.DisplayName;
      flag = true;
    }
    if (GameManager.gameResolution == GameResolution.Defeat)
      this.airbaseName.text = "Mission Failed, no spawn points available ";
    LayoutRebuilder.ForceRebuildLayoutImmediate(this.selectAirbasePanel.GetComponent<RectTransform>());
    this.selectAircraftButton.gameObject.SetActive(flag);
  }

  public void HideSelectAirbase() => this.selectAirbasePanel.SetActive(false);

  public void ShowJoinMenu()
  {
    SceneSingleton<DynamicMap>.i.Minimize();
    this.joinMenu.SetActive(true);
  }

  public void ShowSpectatorPanel()
  {
    NuclearOption.Networking.Player localPlayer;
    if (!GameManager.GetLocalPlayer<NuclearOption.Networking.Player>(out localPlayer) || !((UnityEngine.Object) localPlayer.HQ == (UnityEngine.Object) null))
      return;
    this.spectatorPanel.SetActive(true);
    this.HideSelectAirbase();
  }

  public void HideSpectatorPanel() => this.spectatorPanel.SetActive(false);

  public void SpectatorPanelDeselectAll()
  {
    this.factionInfoPanel_BDF.GetComponent<InfoPanel_Faction>().DeselectPlayers();
    this.factionInfoPanel_PALA.GetComponent<InfoPanel_Faction>().DeselectPlayers();
  }

  public void HideJoinMenu() => this.joinMenu.SetActive(false);

  public void GameMessage(string message) => this.MessageUI.GameMessage(message);

  public void GameMessage(string message, float delay)
  {
    this.MessageUI.DelayedGameMessage(message, delay).Forget();
  }

  public void KillFeed(string message) => this.MessageUI.KillFeed(message);

  public void SelectAirbase(Airbase airbase)
  {
    this.homeAirbase = airbase;
    this.selectAirbasePanel.SetActive(true);
    this.airbaseName.text = airbase.SavedAirbase.DisplayName;
    this.selectAircraftButton.gameObject.SetActive(true);
    LayoutRebuilder.ForceRebuildLayoutImmediate(this.selectAirbasePanel.GetComponent<RectTransform>());
  }

  public void SelectAircraft()
  {
    NuclearOption.Networking.Player localPlayer;
    if (!GameManager.GetLocalPlayer<NuclearOption.Networking.Player>(out localPlayer))
    {
      Debug.LogError((object) "SelectAircraft was clicked but no local player");
    }
    else
    {
      FactionHQ hq = localPlayer.HQ;
      if ((UnityEngine.Object) hq == (UnityEngine.Object) null)
        Debug.LogError((object) "SelectAircraft was clicked without local faction");
      else if ((UnityEngine.Object) hq != (UnityEngine.Object) this.homeAirbase.CurrentHQ)
        Debug.LogWarning((object) "SelectAircraft was clicked but airbase faction was not local faction");
      else
        UnityEngine.Object.Instantiate<GameObject>(this.aircraftSelectionMenu, this.gameplayCanvas.transform).GetComponent<AircraftSelectionMenu>().Initialize(localPlayer, this.homeAirbase);
    }
  }

  public void FlashHurt(float damage, float remainingHitPoints)
  {
    this.pilotHitPoints = remainingHitPoints;
    this.hurt.gameObject.SetActive(true);
    this.hurt.color = new Color(1f, 1f, 1f, this.hurt.color.a + Mathf.Max(damage * 0.01f, 0.2f));
  }

  public void PauseGame()
  {
    if (GameplayUI.GameIsPaused)
      return;
    GameplayUI.GameIsPaused = true;
    CursorManager.SetFlag(CursorFlags.Pause, true);
    this.menuCanvas.enabled = true;
    SceneSingleton<DynamicMap>.i.Minimize();
    this.gameplayCanvas.enabled = false;
    FlightHud.EnableCanvas(false);
    UnityEngine.Object.Instantiate<GameObject>(GameAssets.i.leaderboard, this.menuCanvas.transform);
  }

  public void ResumeGame()
  {
    if (!GameplayUI.GameIsPaused)
      return;
    GameplayUI.GameIsPaused = false;
    CursorManager.SetFlag(CursorFlags.Pause, false);
    this.menuCanvas.enabled = false;
    this.gameplayCanvas.enabled = true;
    if (SceneSingleton<CameraStateManager>.i.currentState == SceneSingleton<CameraStateManager>.i.cockpitState)
      FlightHud.EnableCanvas(true);
    if ((UnityEngine.Object) SceneSingleton<CombatHUD>.i.aircraft == (UnityEngine.Object) null)
      SceneSingleton<DynamicMap>.i.Maximize();
    this.lastResumed = Time.timeSinceLevelLoad;
  }

  private void OnDestroy()
  {
    GameplayUI.GameIsPaused = false;
    CursorManager.SetFlag(CursorFlags.Pause, false);
  }

  public void Update()
  {
    if (this.hurt.gameObject.activeSelf)
    {
      float a = Mathf.Clamp01(this.hurt.color.a - 1f / 500f * Time.deltaTime * Mathf.Max(this.pilotHitPoints, 10f));
      this.hurt.color = new Color(1f, 1f, 1f, a);
      if ((double) a == 0.0)
        this.hurt.gameObject.SetActive(false);
    }
    if (GameplayUI.AllowPauseKeybind && (GameManager.playerInput.GetButtonDown("Pause") || Input.GetKeyDown(KeyCode.Escape)) && !GameplayUI.GameIsPaused && (double) Time.timeSinceLevelLoad - (double) this.lastResumed > 0.10000000149011612)
    {
      this.lastResumed = Time.timeSinceLevelLoad;
      if (GameManager.gameState == GameState.Multiplayer || GameManager.gameState == GameState.SinglePlayer)
        this.PauseGame();
    }
    if (Application.isEditor && GameManager.gameState == GameState.SinglePlayer && Input.GetKeyDown(KeyCode.Y))
      Time.timeScale = 4f;
    if (GameManager.gameState == GameState.SinglePlayer && GameManager.playerInput.GetButtonDown("Slow Motion"))
    {
      GameplayUI.GameSlowMotion = !GameplayUI.GameSlowMotion;
      this.GameMessage(GameplayUI.GameSlowMotion ? "Slow Motion Enabled" : "Slow Motion Disabled");
      if (GameplayUI.GameIsPaused)
        return;
      this.SetTimeFactor(GameplayUI.GameSlowMotion ? 0.05f : 1f);
    }
    if ((UnityEngine.Object) SceneSingleton<CombatHUD>.i.aircraft != (UnityEngine.Object) null)
      return;
    int facChange = 0;
    int pChange = 0;
    if (this.player.GetButtonDown("Change Spectate Faction"))
      facChange = 1;
    if (this.player.GetButtonDown("Spectate Next Aircraft"))
      pChange = 1;
    else if (this.player.GetButtonDown("Spectate Previous Aircraft"))
      pChange = -1;
    if (pChange == 0 && facChange == 0)
      return;
    this.SwitchSpectatedAircraft(facChange, pChange);
  }

  public void SwitchSpectatedAircraft(int facChange, int pChange)
  {
    List<Aircraft> aircraftList = new List<Aircraft>();
    List<Aircraft> source = new List<Aircraft>();
    CameraStateManager cam = SceneSingleton<CameraStateManager>.i;
    int num = 0;
    foreach (Unit allUnit in UnitRegistry.allUnits)
    {
      if (allUnit is Aircraft aircraft && ((UnityEngine.Object) SceneSingleton<DynamicMap>.i.HQ == (UnityEngine.Object) null || (UnityEngine.Object) SceneSingleton<DynamicMap>.i.HQ != (UnityEngine.Object) null && SceneSingleton<DynamicMap>.i.HQ.IsTargetBeingTracked(allUnit)))
        aircraftList.Add(aircraft);
    }
    if (aircraftList.Count == 0)
      return;
    if ((UnityEngine.Object) cam.followingUnit != (UnityEngine.Object) null && cam.followingUnit is Aircraft followingUnit)
    {
      FactionHQ networkHq = followingUnit.NetworkHQ;
      foreach (Aircraft aircraft in aircraftList)
      {
        if (facChange == 0 && (UnityEngine.Object) aircraft.NetworkHQ == (UnityEngine.Object) networkHq || facChange != 0 && (UnityEngine.Object) aircraft.NetworkHQ != (UnityEngine.Object) networkHq)
          source.Add(aircraft);
      }
      if (facChange != 0)
        source = source.OrderBy<Aircraft, float>((Func<Aircraft, float>) (_x => FastMath.Distance(_x.transform.GlobalPosition(), cam.transform.GlobalPosition()))).ToList<Aircraft>();
      num = source.Contains(followingUnit) ? source.IndexOf(followingUnit) : 0;
    }
    else
      source = aircraftList;
    if (source.Count == 0)
      return;
    int index = num + pChange;
    if (index > source.Count - 1)
      index = 0;
    else if (index < 0)
      index = source.Count - 1;
    cam.SetFollowingUnit((Unit) source[index]);
    SceneSingleton<DynamicMap>.i.DeselectAllIcons();
    SceneSingleton<DynamicMap>.i.SelectIcon((Unit) source[index]);
  }

  public void SetTimeFactor(float value) => Time.timeScale = value;
}

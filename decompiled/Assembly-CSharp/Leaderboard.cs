// Decompiled with JetBrains decompiler
// Type: Leaderboard
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.Networking;
using NuclearOption.Networking.Lobbies;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

#nullable disable
public class Leaderboard : SceneSingleton<Leaderboard>
{
  [SerializeField]
  private Leaderboard.LeaderboardFactionDisplay[] factionDisplays;
  [SerializeField]
  private Text lobbyName;
  [SerializeField]
  private Text missionTime;
  [SerializeField]
  private GameObject missionFailedPanel;
  [SerializeField]
  private GameObject missionSucceededPanel;
  [SerializeField]
  private Button resumeButton;
  [SerializeField]
  private RectTransform rectTransform;
  private GameObject submenu;

  private void OnEnable()
  {
    DynamicMap.AllowedToOpen = false;
    this.enabled = true;
    FactionHQ[] array = FactionRegistry.GetAllHQs().ToArray<FactionHQ>();
    for (int index = 0; index < array.Length; ++index)
      this.factionDisplays[index].SetFaction(array[index]);
    if (GameManager.gameState == GameState.Multiplayer && (UnityEngine.Object) SteamLobby.instance != (UnityEngine.Object) null)
      this.lobbyName.text = SteamLobby.instance.CurrentLobbyName;
    if (GameManager.gameState == GameState.SinglePlayer)
    {
      this.resumeButton.Select();
      Time.timeScale = 0.0f;
      AudioListener.pause = true;
      this.lobbyName.text = MissionManager.CurrentMission.Name;
    }
    if (GameManager.gameResolution == GameResolution.Victory)
    {
      this.missionSucceededPanel.SetActive(true);
      this.missionFailedPanel.SetActive(false);
    }
    if (GameManager.gameResolution == GameResolution.Defeat)
    {
      this.missionSucceededPanel.SetActive(false);
      this.missionFailedPanel.SetActive(true);
    }
    LayoutRebuilder.ForceRebuildLayoutImmediate(this.rectTransform);
  }

  private void OnDisable()
  {
    DynamicMap.AllowedToOpen = true;
    this.enabled = false;
    Time.timeScale = GameplayUI.GameSlowMotion ? 0.05f : 1f;
    AudioListener.pause = false;
  }

  public static bool IsOpen()
  {
    return !((UnityEngine.Object) SceneSingleton<Leaderboard>.i == (UnityEngine.Object) null) && SceneSingleton<Leaderboard>.i.enabled;
  }

  public void Resume()
  {
    SceneSingleton<GameplayUI>.i.menuCanvas.enabled = true;
    foreach (Leaderboard.LeaderboardFactionDisplay factionDisplay in this.factionDisplays)
      factionDisplay.CloseDisplay();
    UnityEngine.Object.Destroy((UnityEngine.Object) this.gameObject);
    SceneSingleton<GameplayUI>.i.ResumeGame();
  }

  public void OpenSettings()
  {
    if ((UnityEngine.Object) this.submenu != (UnityEngine.Object) null)
      UnityEngine.Object.Destroy((UnityEngine.Object) this.submenu);
    this.submenu = UnityEngine.Object.Instantiate<GameObject>(GameAssets.i.settingsMenu, SceneSingleton<GameplayUI>.i.menuCanvas.transform);
  }

  public void HideUI()
  {
    SceneSingleton<GameplayUI>.i.menuCanvas.enabled = false;
    GameManager.flightControlsEnabled = true;
    CursorManager.ForceHidden(true);
  }

  private void Update()
  {
    if ((UnityEngine.Object) this.submenu != (UnityEngine.Object) null)
      return;
    if (GameManager.playerInput.GetButtonDown("Pause") || Input.GetKeyDown(KeyCode.Escape))
    {
      CursorManager.ForceHidden(false);
      this.Resume();
    }
    else
    {
      for (int index = 0; index < this.factionDisplays.Length; ++index)
        this.factionDisplays[index].UpdateScores();
      this.missionTime.text = UnitConverter.TimeOfDay(NetworkSceneSingleton<MissionManager>.i.MissionTime / 3600f, true);
    }
  }

  [Serializable]
  private class LeaderboardFactionDisplay
  {
    [SerializeField]
    private List<LeaderboardPlayerEntry> entries = new List<LeaderboardPlayerEntry>();
    [SerializeField]
    private Text titleText;
    [SerializeField]
    private Image factionFlag;
    [SerializeField]
    private Text factionScore;
    [SerializeField]
    private Transform playerListContent;
    [SerializeField]
    private RectTransform scrollView;
    [SerializeField]
    private GameObject playerEntryPrefab;
    private FactionHQ HQ;

    public void SetFaction(FactionHQ HQ)
    {
      this.HQ = HQ;
      HQ.onPlayerChangedFaction += new Action(this.DisplayPlayers);
      this.DisplayPlayers();
    }

    private void DisplayPlayers()
    {
      foreach (Component entry in this.entries)
        UnityEngine.Object.Destroy((UnityEngine.Object) entry.gameObject);
      this.entries.Clear();
      List<Player> players = this.HQ.GetPlayers(true);
      Vector2 sizeDelta = this.scrollView.sizeDelta with
      {
        y = 0.0f
      };
      foreach (Player player in players)
      {
        if (!((UnityEngine.Object) player == (UnityEngine.Object) null))
        {
          LeaderboardPlayerEntry component = UnityEngine.Object.Instantiate<GameObject>(this.playerEntryPrefab, this.playerListContent).GetComponent<LeaderboardPlayerEntry>();
          component.Setup(player);
          this.entries.Add(component);
          sizeDelta.y += 25f;
        }
      }
      if ((double) sizeDelta.y > 450.0)
        sizeDelta.y = 450f;
      this.scrollView.sizeDelta = sizeDelta;
      this.titleText.text = this.HQ.faction.factionExtendedName ?? "";
      this.factionScore.text = $"{this.HQ.factionScore:F1}";
      this.factionFlag.sprite = this.HQ.faction.factionColorLogo;
    }

    public void UpdateScores()
    {
      foreach (LeaderboardPlayerEntry entry in this.entries)
        entry.UpdateScore();
    }

    public void CloseDisplay() => this.HQ.onPlayerChangedFaction -= new Action(this.DisplayPlayers);
  }
}

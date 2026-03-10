// Decompiled with JetBrains decompiler
// Type: JoinMenu
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.Networking;
using NuclearOption.Networking.Lobbies;
using NuclearOption.SavedMission;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

#nullable disable
public class JoinMenu : MonoBehaviour
{
  [SerializeField]
  private JoinMenu.JoinFactionDisplay[] factionDisplays;
  [SerializeField]
  private Button spectateButton;
  [SerializeField]
  private Button leaveGameButton;
  [SerializeField]
  private AudioClip joinMusic;
  [SerializeField]
  private Text lobbyName;

  private void OnEnable()
  {
    this.spectateButton.Select();
    FactionHQ[] array = FactionRegistry.GetAllHQs().OrderBy<FactionHQ, string>((Func<FactionHQ, string>) (factionHQ => factionHQ.faction.factionName)).ToArray<FactionHQ>();
    for (int index = 0; index < array.Length; ++index)
      this.factionDisplays[index].SetFaction(array[index]);
    DynamicMap.AllowedToOpen = false;
    GameplayUI.AllowPauseKeybind = false;
    if (GameManager.gameState == GameState.Multiplayer && (UnityEngine.Object) SteamLobby.instance != (UnityEngine.Object) null)
      this.lobbyName.text = SteamLobby.instance.CurrentLobbyName;
    if (GameManager.gameState == GameState.SinglePlayer)
      this.lobbyName.text = MissionManager.CurrentMission.Name;
    CursorManager.SetFlag(CursorFlags.SelectionMenu, true);
  }

  public void JoinFaction(int index)
  {
    this.factionDisplays[index].JoinFaction();
    foreach (JoinMenu.JoinFactionDisplay factionDisplay in this.factionDisplays)
      factionDisplay.SetUnjoinable();
  }

  public void Spectate()
  {
    Player localPlayer;
    if (!GameManager.GetLocalPlayer<Player>(out localPlayer))
      throw new InvalidOperationException("Can't join spectate because there is no local player");
    localPlayer.SetFaction((FactionHQ) null);
    SceneSingleton<GameplayUI>.i.HideJoinMenu();
    SceneSingleton<DynamicMap>.i.Maximize();
    SceneSingleton<DynamicMap>.i.SetFaction((FactionHQ) null);
  }

  private void OnDisable()
  {
    DynamicMap.AllowedToOpen = true;
    GameplayUI.AllowPauseKeybind = true;
    SceneSingleton<GameplayUI>.i.ResumeGame();
    CursorManager.SetFlag(CursorFlags.SelectionMenu, false);
  }

  [Serializable]
  private class JoinFactionDisplay
  {
    private readonly List<Player> factionPlayers = new List<Player>();
    [SerializeField]
    private List<GameObject> playerList = new List<GameObject>();
    [SerializeField]
    private Button joinButton;
    [SerializeField]
    private Text factionName;
    [SerializeField]
    private Text factionScore;
    [SerializeField]
    private Image factionFlag;
    [SerializeField]
    private Transform playerListContent;
    [SerializeField]
    private RectTransform scrollView;
    [SerializeField]
    private GameObject playerEntryPrefab;
    [SerializeField]
    private RectTransform rootContainer;
    private FactionHQ HQ;

    public void JoinFaction()
    {
      Player localPlayer;
      if (!GameManager.GetLocalPlayer<Player>(out localPlayer))
        throw new InvalidOperationException("Can't join faction because there i no local player");
      this.CheckCameraStartPosition();
      localPlayer.SetFaction(this.HQ);
      MusicManager.i.CrossFadeMusic(NetworkSceneSingleton<LevelInfo>.i.LoadedMapSettings.GetStartMusic(this.HQ.faction), 2f, 0.0f, false, false, true);
      SceneSingleton<DynamicMap>.i.SetFaction(this.HQ);
      SceneSingleton<GameplayUI>.i.HideJoinMenu();
      SceneSingleton<DynamicMap>.i.Maximize();
    }

    private void CheckCameraStartPosition()
    {
      if (!(SceneSingleton<CameraStateManager>.i.currentState is CameraFreeState))
        return;
      MissionFaction missionFaction;
      MissionManager.CurrentMission.GetFactionFromHq(this.HQ, out missionFaction);
      if (!missionFaction.cameraStartPosition.IsOverride)
        return;
      SceneSingleton<CameraStateManager>.i.SetCameraPosition(missionFaction.cameraStartPosition.Value);
    }

    public void SetUnjoinable() => this.joinButton.gameObject.SetActive(false);

    public void SetFaction(FactionHQ HQ)
    {
      this.HQ = HQ;
      HQ.onPlayerChangedFaction += new Action(this.DisplayPlayers);
      this.factionFlag.sprite = HQ.faction.factionHeaderSprite;
      if (HQ.preventJoin)
        this.SetUnjoinable();
      this.DisplayPlayers();
    }

    private void DisplayPlayers()
    {
      foreach (UnityEngine.Object player in this.playerList)
        UnityEngine.Object.Destroy(player);
      this.playerList.Clear();
      List<Player> players = this.HQ.GetPlayers(true);
      Vector2 sizeDelta = this.scrollView.sizeDelta with
      {
        y = 0.0f
      };
      foreach (Player player in players)
      {
        if (!((UnityEngine.Object) player == (UnityEngine.Object) null))
        {
          GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.playerEntryPrefab, this.playerListContent);
          gameObject.GetComponent<LeaderboardPlayerEntry>().Setup(player);
          this.playerList.Add(gameObject);
          sizeDelta.y += 25f;
        }
      }
      if ((double) sizeDelta.y > 450.0)
        sizeDelta.y = 450f;
      this.scrollView.sizeDelta = sizeDelta;
      this.factionName.text = this.HQ.faction.factionExtendedName ?? "";
      this.factionScore.text = $"{this.HQ.factionScore:F1}";
    }
  }
}

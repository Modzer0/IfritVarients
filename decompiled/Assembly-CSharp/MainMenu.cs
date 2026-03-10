// Decompiled with JetBrains decompiler
// Type: MainMenu
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Cysharp.Threading.Tasks;
using Mirage.Logging;
using NuclearOption.DedicatedServer;
using NuclearOption.MissionEditorScripts;
using NuclearOption.Networking;
using NuclearOption.SavedMission;
using NuclearOption.SceneLoading;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

#nullable disable
public class MainMenu : MonoBehaviour
{
  [SerializeField]
  private TMP_InputField inputPlayerName;
  [SerializeField]
  private Transform overlayMenuLayer;
  [SerializeField]
  private GameObject unableToConnect;
  [SerializeField]
  private GameObject disconnectNotification;
  [SerializeField]
  private Text disconnectReason;
  [SerializeField]
  private GameObject roadmapPanel;
  [SerializeField]
  private GameObject controlChangesPanel;
  [SerializeField]
  private GameObject hintPanel;
  [SerializeField]
  private Button missionsButton;
  [SerializeField]
  private GameObject firstLoadOverlay;
  [SerializeField]
  private CanvasGroup firstLoadOverlayFade;
  [SerializeField]
  private GameObject workshopPrefab;
  [SerializeField]
  private MissionEditorPopupMenus openMissionEditorMenu;
  [Header("Steam app id")]
  [SerializeField]
  private string steamAppID;
  private static bool addedQuitting;

  public static MainMenu.LoadingState State { get; private set; }

  public static async UniTask WaitForLoaded(CancellationToken cancellation)
  {
    await UniTask.WaitUntil((Func<bool>) (() => MainMenu.State == MainMenu.LoadingState.Loaded), cancellationToken: cancellation);
  }

  public static bool ApplicationIsQuitting { get; private set; }

  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
  private static void Init()
  {
    Debug.Log((object) "[MainMenu] RuntimeInitializeOnLoadMethod");
    MainMenu.State = MainMenu.LoadingState.None;
    MainMenu.ApplicationIsQuitting = false;
    if (MainMenu.addedQuitting)
      return;
    MainMenu.addedQuitting = true;
    Application.quitting += (Action) (() =>
    {
      Debug.Log((object) "[MainMenu] Application_quitting");
      MainMenu.ApplicationIsQuitting = true;
    });
  }

  private void Awake()
  {
    if (MainMenu.State == MainMenu.LoadingState.None)
    {
      Debug.Log((object) $"Nuclear Option {Application.version} Starting at {DateTime.UtcNow} UTC");
      if (!LogTimeUpdater.IsRunning)
        LogTimeUpdater.RunForever().Forget();
      MirageLogHandler.Settings logSettings = new MirageLogHandler.Settings(MirageLogHandler.TimePrefix.UnscaledTime, Application.isEditor, true);
      LogFactory.ReplaceLogHandler((Func<string, ILogHandler>) (name => (ILogHandler) new MirageLogHandler(logSettings, name)));
      PlayerSettings.FirstInit();
      GameManager.PreSetupGame();
      this.firstLoadOverlay.SetActive(true);
    }
    this.openMissionEditorMenu.CloseMenu();
    this.roadmapPanel.SetActive(false);
    if (!PlayerPrefs.HasKey("FirstStartup0.32"))
      PlayerPrefs.SetInt("FirstStartup0.32", 1);
    else
      this.controlChangesPanel.SetActive(false);
  }

  private void Start() => this.StartAsync().Forget();

  private async UniTaskVoid StartAsync()
  {
    MainMenu mainMenu = this;
    if (MainMenu.State == MainMenu.LoadingState.None)
    {
      List<UniTask> tasks = new List<UniTask>();
      MainMenu.State = MainMenu.LoadingState.Loading;
      CancellationToken cancel = mainMenu.destroyCancellationToken;
      tasks.Add(Encyclopedia.Preload(cancel));
      tasks.Add(GameAssets.Preload(cancel));
      tasks.Add(DebugUI.Preload(cancel));
      tasks.Add(NetworkManagerNuclearOption.Preload(cancel));
      tasks.Add(SoundManager.Preload(cancel));
      tasks.Add(SteamManager.CheckIdFileAsync(mainMenu.steamAppID));
      tasks.Add(MissionSaveLoad.ConvertMissionToFolders());
      tasks.Add(ResourcesAsyncLoader.LoadPrefab("Rewired", cancel, (Action<GameObject>) (clone =>
      {
        GameManager.controlMapper = clone.GetComponentInChildren<Rewired.UI.ControlMapper.ControlMapper>();
        GameManager.controlMapper.ScreenClosedEvent += (Action) (() =>
        {
          if (!((UnityEngine.Object) SceneSingleton<GameplayUI>.i != (UnityEngine.Object) null))
            return;
          SceneSingleton<GameplayUI>.i.menuCanvas.enabled = true;
        });
      })));
      tasks.Add(ResourcesAsyncLoader.LoadPrefab("EventSystem", cancel, (Action<GameObject>) (clone => GameManager.eventSystem = clone.GetComponent<EventSystem>())));
      ColorLog<MainMenu>.Info("Waiting for Tasks");
      await UniTask.WhenAll((IEnumerable<UniTask>) tasks);
      if (cancel.IsCancellationRequested)
        return;
      ColorLog<MainMenu>.Info("All Task finished");
      MainMenu.State = MainMenu.LoadingState.Loaded;
      mainMenu.LoadOverlayFade(cancel).Forget();
      if (DedicatedServerManager.AutoRun)
      {
        NetworkManagerNuclearOption.i.SteamManager.InitAsServer(DedicatedServerManager.GetConfig().config);
      }
      else
      {
        NetworkManagerNuclearOption.i.SteamManager.InitAsClient();
        string rawName;
        string safeName;
        StringHelper.GetSanitizeSteamName(out rawName, out safeName, GameAssets.i.playerNameFont);
        PlayerSettings.playerName = safeName;
        PlayerSettings.playerName_Unsanitized = rawName;
      }
      cancel = new CancellationToken();
    }
    else
      mainMenu.firstLoadOverlay.SetActive(false);
    if (!DedicatedServerManager.AutoRun)
    {
      CursorManager.Refresh();
      mainMenu.missionsButton.Select();
      mainMenu.inputPlayerName.text = PlayerSettings.playerName ?? "";
      MusicManager.i.PlayMenuMusic();
    }
    Time.timeScale = 1f;
    GameManager.SetLocalPlayer((BasePlayer) null);
    GameManager.SetGameState(GameState.Menu);
    DisconnectInfo disconnectInfo = GameManager.disconnectInfo;
    if ((disconnectInfo != null ? (disconnectInfo.ShowReason ? 1 : 0) : 0) != 0)
    {
      mainMenu.disconnectNotification.SetActive(true);
      mainMenu.disconnectReason.text = GameManager.disconnectInfo.Message;
    }
    GameManager.ClearDisconnectReason();
    GameManager.SetupGame();
  }

  private async UniTaskVoid LoadOverlayFade(CancellationToken cancel)
  {
    float unscaledTime = Time.unscaledTime;
    float start = unscaledTime;
    float end = start + 0.5f;
    while ((double) end > (double) unscaledTime)
    {
      await UniTask.Yield();
      if (cancel.IsCancellationRequested)
        return;
      unscaledTime = Time.unscaledTime;
      this.firstLoadOverlayFade.alpha = Mathf.Lerp(1f, 0.0f, Mathf.InverseLerp(start, end, unscaledTime));
    }
    this.firstLoadOverlay.SetActive(false);
  }

  private void Update()
  {
    if (this.overlayMenuLayer.childCount == 0 && !this.hintPanel.activeSelf)
    {
      this.ShowHints();
    }
    else
    {
      if (this.overlayMenuLayer.childCount <= 0 || !this.hintPanel.activeSelf)
        return;
      this.HideHints();
    }
  }

  public void SelectMissionEditor() => this.openMissionEditorMenu.ShowSetupMenu();

  public void SelectMissions() => SceneManager.LoadScene(MapLoader.MissionsMenu);

  public void EnterName() => PlayerSettings.playerName = this.inputPlayerName.text;

  public void LinktreeButton() => Application.OpenURL("https://linktr.ee/shockfrontstudios");

  public void ChangelogButton()
  {
    Application.OpenURL("https://store.steampowered.com/news/app/2168680");
  }

  public void SelectMultiplayer()
  {
    if (!SteamAPI.IsSteamRunning())
      this.unableToConnect.SetActive(true);
    else
      SceneManager.LoadScene(MapLoader.MultiplayerMenu);
  }

  public void SelectSettings()
  {
    UnityEngine.Object.Instantiate<GameObject>(GameAssets.i.settingsMenu, this.overlayMenuLayer);
  }

  public void SelectEncyclopedia()
  {
    MissionManager.SetNullMission();
    NetworkManagerNuclearOption.i.StartHost(new HostOptions(SocketType.Offline, GameState.Encyclopedia, MapLoader.Encyclopedia));
  }

  public void SelectWorkshop()
  {
    UnityEngine.Object.Instantiate<GameObject>(this.workshopPrefab, this.overlayMenuLayer);
  }

  public void ShowHints() => this.hintPanel.SetActive(true);

  public void HideHints() => this.hintPanel.SetActive(false);

  public void QuitGame() => Application.Quit();

  public enum LoadingState
  {
    None,
    Loading,
    Loaded,
  }
}

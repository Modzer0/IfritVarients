// Decompiled with JetBrains decompiler
// Type: NuclearOption.Networking.Lobbies.LobbyList
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Cysharp.Threading.Tasks;
using NuclearOption.MissionEditorScripts.Buttons;
using NuclearOption.SceneLoading;
using NuclearOption.UI;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

#nullable disable
namespace NuclearOption.Networking.Lobbies;

public class LobbyList : MonoBehaviour
{
  [Header("List")]
  [SerializeField]
  private LobbyListFadeOverlay refreshOverlay;
  [SerializeField]
  private RectTransform lobbyListContent;
  [SerializeField]
  private LobbyListItem entryPrefab;
  [SerializeField]
  private List<LobbyListItem> startingEntries;
  [Header("References")]
  [SerializeField]
  private LobbyDetailsModal lobbyPopup;
  [SerializeField]
  private Button createLobbyButton;
  [SerializeField]
  private CreateLobbyModal createLobbyModal;
  [SerializeField]
  private Button mainMenuButton;
  [Header("Filters")]
  [SerializeField]
  private SliderToggle hideFullServersToggle;
  [SerializeField]
  private SliderToggle hidePasswordProtectedToggle;
  [SerializeField]
  private BetterToggleGroup missionTypeToggleGroup;
  [SerializeField]
  private BetterToggleGroup serverTypeToggleGroup;
  [SerializeField]
  private BetterToggleGroup distanceFilterToggleGroup;
  [SerializeField]
  private TMP_InputField searchInputField;
  [SerializeField]
  private bool DEBUG_ignoreVersionFilter;
  [Header("Sorting Buttons")]
  [SerializeField]
  private Button refreshButton;
  [SerializeField]
  private Button clearSearchButton;
  [SerializeField]
  private Color activeHeaderColor = Color.white;
  [SerializeField]
  private Color inactiveHeaderColor = Color.grey;
  [SerializeField]
  private ListSortButton sortByNameButton;
  [SerializeField]
  private ListSortButton sortByMissionButton;
  [SerializeField]
  private ListSortButton sortByMapButton;
  [SerializeField]
  private ListSortButton sortByPlayerCountButton;
  [SerializeField]
  private ListSortButton sortByPingButton;
  [SerializeField]
  private ListSortButton sortByDurationButton;
  [SerializeField]
  private int fuzzySearchMaxDistance = 8;
  [Header("Too many players")]
  [SerializeField]
  public int TooManyPlayerLimit = 16 /*0x10*/;
  [SerializeField]
  public HoverText TooManyPlayerHover;
  private FuzzySearch fuzzySearch;
  private readonly Stack<LobbyListItem> pool = new Stack<LobbyListItem>();
  private readonly Dictionary<LobbyInstance, LobbyListItem> allLobbies = new Dictionary<LobbyInstance, LobbyListItem>();
  private List<LobbyListItem> sortList = new List<LobbyListItem>();
  private List<int> sortListScore = new List<int>();
  private readonly HashSet<LobbyListItem> _processedLobbiesSet = new HashSet<LobbyListItem>();
  private LobbyList.SortMode _activeSortMode = LobbyList.SortMode.PlayerCountDesc;
  private LobbyList.SortMode _previousHeaderSortMode = LobbyList.SortMode.PlayerCountDesc;
  private LobbySearchFilter activeFilter;
  private bool refreshPending;

  private void Awake()
  {
    foreach (LobbyListItem startingEntry in this.startingEntries)
    {
      startingEntry.Hide();
      this.pool.Push(startingEntry);
    }
    this.createLobbyButton.onClick.AddListener(new UnityAction(this.CreateLobbyClicked));
    this.mainMenuButton.onClick.AddListener(UniTask.UnityAction(new Func<UniTaskVoid>(this.MainMenuClicked)));
    SteamLobby.instance.CheckRelayLocationTask();
    SteamLobby.instance.OnLobbyListCleared += new Action(this.RefreshLobbyListCleared);
    SteamLobby.instance.OnLobbyDataUpdated += new Action<LobbyInstance>(this.OnLobbyDataUpdated);
    SteamLobby.instance.OnLobbyPingUpdated += new Action<ServerLobbyInstance>(this.OnLobbyPingUpdated);
    SteamLobby.instance.OnLobbyRefreshFinished += new Action(this.HideRefreshOverlay);
    SteamLobby.instance.OnLocationUpdated += new Action<string>(this.Instance_OnLocationUpdated);
    this.hideFullServersToggle.onValueChanged.AddListener((UnityAction<bool>) (_ => this.GetListOfLobbies()));
    this.hidePasswordProtectedToggle.onValueChanged.AddListener((UnityAction<bool>) (_ => this.GetListOfLobbies()));
    this.missionTypeToggleGroup.OnChangeValue += (BetterToggleGroup.ToggleIndexOn) (_ => this.GetListOfLobbies());
    this.serverTypeToggleGroup.OnChangeValue += (BetterToggleGroup.ToggleIndexOn) (_ => this.GetListOfLobbies());
    this.distanceFilterToggleGroup.OnChangeValue += (BetterToggleGroup.ToggleIndexOn) (_ => this.GetListOfLobbies());
    this.searchInputField.onValueChanged.AddListener(new UnityAction<string>(this.OnSearchTermChanged));
    this.clearSearchButton.onClick.AddListener(new UnityAction(this.OnClearSearchClicked));
    this.refreshButton.onClick.AddListener(new UnityAction(this.GetListOfLobbies));
    this.sortByNameButton.Init(new UnityAction(this.OnSortByNameClicked), this.inactiveHeaderColor);
    this.sortByMissionButton.Init(new UnityAction(this.OnSortByMissionClicked), this.inactiveHeaderColor);
    this.sortByMapButton.Init(new UnityAction(this.OnSortByMapClicked), this.inactiveHeaderColor);
    this.sortByPlayerCountButton.Init(new UnityAction(this.OnSortByPlayersClicked), this.inactiveHeaderColor);
    this.sortByPingButton.Init(new UnityAction(this.OnSortByPingClicked), this.inactiveHeaderColor);
    this.sortByDurationButton.Init(new UnityAction(this.OnSortByDurationClicked), this.inactiveHeaderColor);
    this.SetSortMode(this._activeSortMode, true);
    this.UpdateTimeLoop().Forget();
  }

  private void Update() => SteamLobby.instance.CheckPingServers();

  private void OnLobbyPingUpdated(ServerLobbyInstance lobby)
  {
    LobbyListItem lobbyListItem;
    if (this.allLobbies.TryGetValue((LobbyInstance) lobby, out lobbyListItem))
    {
      lobbyListItem.UpdatePing();
    }
    else
    {
      if (!this.activeFilter.PingDistanceAllowed(lobby.CalculatePing().Value))
        return;
      this.OnLobbyDataUpdated((LobbyInstance) lobby);
    }
  }

  private void Instance_OnLocationUpdated(string _)
  {
    foreach (LobbyListItem sort in this.sortList)
    {
      if (sort.lobby is PlayerLobbyInstance)
        sort.UpdatePing();
    }
  }

  private async UniTaskVoid UpdateTimeLoop()
  {
    LobbyList lobbyList = this;
    CancellationToken cancel = lobbyList.destroyCancellationToken;
    while (!cancel.IsCancellationRequested)
    {
      foreach (LobbyListItem sort in lobbyList.sortList)
        sort.UpdateTime();
      await UniTask.Delay(TimeSpan.FromMinutes(1.0));
    }
    cancel = new CancellationToken();
  }

  private void CreateLobbyClicked() => this.createLobbyModal.Show(this);

  private async UniTaskVoid MainMenuClicked()
  {
    this.mainMenuButton.interactable = false;
    int num = await NetworkManagerNuclearOption.i.LoadSystemScene(MapLoader.MainMenu, false) ? 1 : 0;
    if (!((UnityEngine.Object) this.mainMenuButton != (UnityEngine.Object) null))
      return;
    this.mainMenuButton.interactable = true;
  }

  private void Start()
  {
    this.refreshOverlay.Show();
    this.GetListOfLobbies();
    LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform) this.transform);
  }

  private void OnDestroy()
  {
    if (!((UnityEngine.Object) SteamLobby.instance != (UnityEngine.Object) null))
      return;
    SteamLobby.instance.OnLobbyListCleared -= new Action(this.RefreshLobbyListCleared);
    SteamLobby.instance.OnLobbyDataUpdated -= new Action<LobbyInstance>(this.OnLobbyDataUpdated);
    SteamLobby.instance.OnLobbyPingUpdated -= new Action<ServerLobbyInstance>(this.OnLobbyPingUpdated);
    SteamLobby.instance.OnLobbyRefreshFinished -= new Action(this.HideRefreshOverlay);
    SteamLobby.instance.OnLocationUpdated -= new Action<string>(this.Instance_OnLocationUpdated);
    SteamLobby.instance.ClearLobbyCache();
  }

  public void ShowLobbyPopup(LobbyInstance lobby) => this.lobbyPopup.Show(this, lobby);

  private void RefreshLobbyListCleared()
  {
    if (this.refreshPending)
      return;
    this.refreshPending = true;
    this.refreshOverlay.Show();
    this.ClearLobbies();
  }

  private void ClearLobbies()
  {
    foreach (LobbyListItem lobbyListItem in this.allLobbies.Values)
    {
      lobbyListItem.Hide();
      this.pool.Push(lobbyListItem);
    }
    this.allLobbies.Clear();
    this.UpdateLobbyList();
  }

  private LobbyListItem GetFromPool()
  {
    LobbyListItem lobbyListItem;
    return this.pool.TryPop(ref lobbyListItem) ? lobbyListItem : UnityEngine.Object.Instantiate<LobbyListItem>(this.entryPrefab, this.lobbyListContent.transform).GetComponent<LobbyListItem>();
  }

  private void HideRefreshOverlay()
  {
    if (!this.refreshPending)
      return;
    this.refreshPending = false;
    this.refreshOverlay.Hide();
  }

  private void OnLobbyDataUpdated(LobbyInstance lobby)
  {
    this.HideRefreshOverlay();
    LobbyListItem fromPool;
    if (!this.allLobbies.TryGetValue(lobby, out fromPool))
      fromPool = this.GetFromPool();
    bool flag = false;
    try
    {
      flag = fromPool.Show(this, lobby);
    }
    catch (Exception ex)
    {
      Debug.LogError((object) $"Error caught in OnLobbyDataUpdated: {ex}");
    }
    if (flag)
    {
      this.allLobbies[lobby] = fromPool;
      this.InsertLobbyDataEntry(fromPool);
    }
    else
      this.pool.Push(fromPool);
  }

  public void GetListOfLobbies()
  {
    this.activeFilter = new LobbySearchFilter()
    {
      HideFull = this.hideFullServersToggle.isOn,
      HidePasswordProtected = this.hidePasswordProtectedToggle.isOn,
      MissionPvpType = (MissionPvpType) this.missionTypeToggleGroup.Value,
      ServerType = (FilterServerType) this.serverTypeToggleGroup.Value,
      distanceFilter = this.DistanceToggleToEnum()
    };
    SteamLobby.instance.GetLobbiesList(this.activeFilter);
  }

  private ELobbyDistanceFilter? DistanceToggleToEnum()
  {
    switch (this.distanceFilterToggleGroup.Value)
    {
      case 0:
        return new ELobbyDistanceFilter?(ELobbyDistanceFilter.k_ELobbyDistanceFilterDefault);
      case 1:
        return new ELobbyDistanceFilter?(ELobbyDistanceFilter.k_ELobbyDistanceFilterFar);
      default:
        return new ELobbyDistanceFilter?();
    }
  }

  private void OnSearchTermChanged(string searchTerm)
  {
    if (!string.IsNullOrWhiteSpace(searchTerm))
      this.SetSortMode(LobbyList.SortMode.FuzzySearch, false);
    else
      this.SetSortMode(this._previousHeaderSortMode, false);
    this.UpdateLobbyList();
  }

  private void OnClearSearchClicked()
  {
    this.searchInputField.SetTextWithoutNotify(string.Empty);
    this.SetSortMode(this._previousHeaderSortMode, false);
    this.UpdateLobbyList();
  }

  public void OnSortByNameClicked()
  {
    this.SetSortMode(this._activeSortMode == LobbyList.SortMode.NameAsc ? LobbyList.SortMode.NameDesc : LobbyList.SortMode.NameAsc, true);
    this.UpdateLobbyList();
  }

  public void OnSortByMissionClicked()
  {
    this.SetSortMode(this._activeSortMode == LobbyList.SortMode.MissionAsc ? LobbyList.SortMode.MissionDesc : LobbyList.SortMode.MissionAsc, true);
    this.UpdateLobbyList();
  }

  public void OnSortByMapClicked()
  {
    this.SetSortMode(this._activeSortMode == LobbyList.SortMode.MapAsc ? LobbyList.SortMode.MapDesc : LobbyList.SortMode.MapAsc, true);
    this.UpdateLobbyList();
  }

  public void OnSortByPlayersClicked()
  {
    this.SetSortMode(this._activeSortMode == LobbyList.SortMode.PlayerCountAsc ? LobbyList.SortMode.PlayerCountDesc : LobbyList.SortMode.PlayerCountAsc, true);
    this.UpdateLobbyList();
  }

  public void OnSortByPingClicked()
  {
    this.SetSortMode(this._activeSortMode == LobbyList.SortMode.PingAsc ? LobbyList.SortMode.PingDesc : LobbyList.SortMode.PingAsc, true);
    this.UpdateLobbyList();
  }

  public void OnSortByDurationClicked()
  {
    this.SetSortMode(this._activeSortMode == LobbyList.SortMode.DurationAsc ? LobbyList.SortMode.DurationDesc : LobbyList.SortMode.DurationAsc, true);
    this.UpdateLobbyList();
  }

  private void SetSortMode(LobbyList.SortMode newSortMode, bool setPrevious)
  {
    if (setPrevious)
      this._previousHeaderSortMode = this._activeSortMode;
    this._activeSortMode = newSortMode;
    this.sortByNameButton.UpdateLabel(ListSortButton.SortState.None, this.inactiveHeaderColor);
    this.sortByMissionButton.UpdateLabel(ListSortButton.SortState.None, this.inactiveHeaderColor);
    this.sortByMapButton.UpdateLabel(ListSortButton.SortState.None, this.inactiveHeaderColor);
    this.sortByPlayerCountButton.UpdateLabel(ListSortButton.SortState.None, this.inactiveHeaderColor);
    this.sortByPingButton.UpdateLabel(ListSortButton.SortState.None, this.inactiveHeaderColor);
    this.sortByDurationButton.UpdateLabel(ListSortButton.SortState.None, this.inactiveHeaderColor);
    switch (newSortMode)
    {
      case LobbyList.SortMode.PlayerCountAsc:
        this.sortByPlayerCountButton.UpdateLabel(ListSortButton.SortState.ascending, this.activeHeaderColor);
        break;
      case LobbyList.SortMode.PlayerCountDesc:
        this.sortByPlayerCountButton.UpdateLabel(ListSortButton.SortState.descending, this.activeHeaderColor);
        break;
      case LobbyList.SortMode.PingAsc:
        this.sortByPingButton.UpdateLabel(ListSortButton.SortState.ascending, this.activeHeaderColor);
        break;
      case LobbyList.SortMode.PingDesc:
        this.sortByPingButton.UpdateLabel(ListSortButton.SortState.descending, this.activeHeaderColor);
        break;
      case LobbyList.SortMode.NameAsc:
        this.sortByNameButton.UpdateLabel(ListSortButton.SortState.ascending, this.activeHeaderColor);
        break;
      case LobbyList.SortMode.NameDesc:
        this.sortByNameButton.UpdateLabel(ListSortButton.SortState.descending, this.activeHeaderColor);
        break;
      case LobbyList.SortMode.MissionAsc:
        this.sortByMissionButton.UpdateLabel(ListSortButton.SortState.ascending, this.activeHeaderColor);
        break;
      case LobbyList.SortMode.MissionDesc:
        this.sortByMissionButton.UpdateLabel(ListSortButton.SortState.descending, this.activeHeaderColor);
        break;
      case LobbyList.SortMode.MapAsc:
        this.sortByMapButton.UpdateLabel(ListSortButton.SortState.ascending, this.activeHeaderColor);
        break;
      case LobbyList.SortMode.MapDesc:
        this.sortByMapButton.UpdateLabel(ListSortButton.SortState.descending, this.activeHeaderColor);
        break;
      case LobbyList.SortMode.DurationAsc:
        this.sortByDurationButton.UpdateLabel(ListSortButton.SortState.ascending, this.activeHeaderColor);
        break;
      case LobbyList.SortMode.DurationDesc:
        this.sortByDurationButton.UpdateLabel(ListSortButton.SortState.descending, this.activeHeaderColor);
        break;
    }
  }

  private void UpdateLobbyList()
  {
    this.sortList.Clear();
    this._processedLobbiesSet.Clear();
    FuzzySearch.SubstringSearch<LobbyListItem>((IEnumerable<LobbyListItem>) this.allLobbies.Values, (Func<LobbyListItem, string>) (l => l.LobbyName), this.searchInputField.text, this.sortList);
    if (this._activeSortMode != LobbyList.SortMode.FuzzySearch)
      this.sortList.Sort(this.GetComparer(this._activeSortMode));
    foreach (LobbyListItem sort in this.sortList)
      this._processedLobbiesSet.Add(sort);
    foreach (LobbyListItem lobbyListItem in this.allLobbies.Values)
      lobbyListItem.gameObject.SetActive(this._processedLobbiesSet.Contains(lobbyListItem));
    for (int index = 0; index < this.sortList.Count; ++index)
      this.sortList[index].transform.SetSiblingIndex(index);
  }

  private void InsertLobbyDataEntry(LobbyListItem newEntry)
  {
    if (!string.IsNullOrWhiteSpace(this.searchInputField.text))
    {
      this.UpdateLobbyList();
    }
    else
    {
      Comparison<LobbyListItem> comparer = this.GetComparer(this._activeSortMode);
      int index1 = -1;
      for (int index2 = 0; index2 < this.sortList.Count; ++index2)
      {
        if (comparer(newEntry, this.sortList[index2]) < 0)
        {
          index1 = index2;
          break;
        }
      }
      if (index1 == -1)
        index1 = this.sortList.Count;
      this.sortList.Insert(index1, newEntry);
      newEntry.gameObject.SetActive(true);
      for (int index3 = index1; index3 < this.sortList.Count; ++index3)
        this.sortList[index3].transform.SetSiblingIndex(index3);
    }
  }

  private Comparison<LobbyListItem> GetComparer(LobbyList.SortMode mode)
  {
    switch (mode)
    {
      case LobbyList.SortMode.PlayerCountAsc:
        return (Comparison<LobbyListItem>) ((a, b) => a.PlayerCount.CompareTo(b.PlayerCount));
      case LobbyList.SortMode.PlayerCountDesc:
        return (Comparison<LobbyListItem>) ((a, b) => b.PlayerCount.CompareTo(a.PlayerCount));
      case LobbyList.SortMode.PingAsc:
        return (Comparison<LobbyListItem>) ((a, b) => LobbyList.CompareNullable<int>(a.Ping, b.Ping));
      case LobbyList.SortMode.PingDesc:
        return (Comparison<LobbyListItem>) ((a, b) => LobbyList.CompareNullable<int>(b.Ping, a.Ping));
      case LobbyList.SortMode.NameAsc:
        return (Comparison<LobbyListItem>) ((a, b) => string.Compare(a.LobbyName, b.LobbyName, StringComparison.OrdinalIgnoreCase));
      case LobbyList.SortMode.NameDesc:
        return (Comparison<LobbyListItem>) ((a, b) => string.Compare(b.LobbyName, a.LobbyName, StringComparison.OrdinalIgnoreCase));
      case LobbyList.SortMode.MissionAsc:
        return (Comparison<LobbyListItem>) ((a, b) => string.Compare(a.MissionName, b.MissionName, StringComparison.OrdinalIgnoreCase));
      case LobbyList.SortMode.MissionDesc:
        return (Comparison<LobbyListItem>) ((a, b) => string.Compare(b.MissionName, a.MissionName, StringComparison.OrdinalIgnoreCase));
      case LobbyList.SortMode.MapAsc:
        return (Comparison<LobbyListItem>) ((a, b) => string.Compare(a.MapName, b.MapName, StringComparison.OrdinalIgnoreCase));
      case LobbyList.SortMode.MapDesc:
        return (Comparison<LobbyListItem>) ((a, b) => string.Compare(b.MapName, a.MapName, StringComparison.OrdinalIgnoreCase));
      case LobbyList.SortMode.DurationAsc:
        return (Comparison<LobbyListItem>) ((a, b) => LobbyList.CompareNullable<DateTime>(a.StartTime, b.StartTime));
      case LobbyList.SortMode.DurationDesc:
        return (Comparison<LobbyListItem>) ((a, b) => LobbyList.CompareNullable<DateTime>(b.StartTime, a.StartTime));
      default:
        return (Comparison<LobbyListItem>) ((a, b) => 0);
    }
  }

  private static int CompareNullable<T>(T? a, T? b) where T : struct, IComparable
  {
    if (a.HasValue && b.HasValue)
      return a.Value.CompareTo((object) b.Value);
    if (a.HasValue)
      return -1;
    return b.HasValue ? 1 : 0;
  }

  public enum SortMode
  {
    None,
    FuzzySearch,
    PlayerCountAsc,
    PlayerCountDesc,
    PingAsc,
    PingDesc,
    NameAsc,
    NameDesc,
    MissionAsc,
    MissionDesc,
    MapAsc,
    MapDesc,
    DurationAsc,
    DurationDesc,
  }
}

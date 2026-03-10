// Decompiled with JetBrains decompiler
// Type: NuclearOption.Networking.Lobbies.LobbyListItem
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.MissionEditorScripts.Buttons;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

#nullable disable
namespace NuclearOption.Networking.Lobbies;

public class LobbyListItem : 
  MonoBehaviour,
  IPointerClickHandler,
  IEventSystemHandler,
  IPointerEnterHandler,
  IPointerExitHandler
{
  private const string PRIMARY_HEX = "#58e187";
  private const string MUTED_HEX = "#8a93a2";
  private const string STATUS_SUCCESS_HEX = "#4ade80";
  private const string STATUS_WARNING_HEX = "#facc15";
  private const string STATUS_DANGER_HEX = "#f87171";
  public static readonly Color StatusSuccessColor;
  public static readonly Color StatusWarningColor;
  public static readonly Color StatusDangerColor;
  public static readonly Color TextMutedColor;
  private static readonly string serverIcon = "\uE875".AddColor("#58e187");
  private static readonly string playerIcon = "\uE7FD".AddColor("#8a93a2");
  private static readonly string lockIcon = "\uE897".AddColor("#8a93a2");
  private static readonly string moddedIcon = "\uEF48".AddColor("#FF7700");
  [Header("Colors")]
  [SerializeField]
  private Image backgroundImage;
  [SerializeField]
  private Color normalBackground;
  [SerializeField]
  private Color hoverBackground;
  [Space]
  [SerializeField]
  private Graphic boarderImage;
  [SerializeField]
  private Color normalBoarder;
  [SerializeField]
  private Color hoverBoarder;
  [Header("Text")]
  [SerializeField]
  private TextMeshProUGUI lobbyNameText;
  [SerializeField]
  private TextMeshProUGUI missionNameText;
  [SerializeField]
  private TextMeshProUGUI iconsText;
  [SerializeField]
  private TextMeshProUGUI playerText;
  [SerializeField]
  private TextMeshProUGUI uptimeText;
  [SerializeField]
  private TextMeshProUGUI pingText;
  [SerializeField]
  private ShowHoverText tooManyPlayersWarning;
  private LobbyList multiplayerLobbyList;
  private bool isPasswordProtected;
  private bool shown;

  static LobbyListItem()
  {
    ColorUtility.TryParseHtmlString("#4ade80", out LobbyListItem.StatusSuccessColor);
    ColorUtility.TryParseHtmlString("#facc15", out LobbyListItem.StatusWarningColor);
    ColorUtility.TryParseHtmlString("#f87171", out LobbyListItem.StatusDangerColor);
    ColorUtility.TryParseHtmlString("#8a93a2", out LobbyListItem.TextMutedColor);
  }

  public LobbyInstance lobby { get; private set; }

  public string LobbyName { get; private set; }

  public string MissionName { get; private set; }

  public string MapName { get; private set; }

  public int PlayerCount { get; private set; }

  public int? Ping { get; private set; }

  public DateTime? StartTime { get; private set; }

  public bool IsFull { get; private set; }

  public bool IsServer => this.lobby is ServerLobbyInstance;

  public bool Show(LobbyList multiplayerLobbyList, LobbyInstance lobby)
  {
    if (this.shown && !this.lobby.Equals((object) lobby))
    {
      Debug.LogError((object) "LobbyDataEntry Shown twice");
      return false;
    }
    this.LobbyName = lobby.LobbyNameSanitized;
    if (!LobbyInstance.ValidName(this.LobbyName))
      return false;
    this.tooManyPlayersWarning.SetHover(multiplayerLobbyList.TooManyPlayerHover);
    this.tooManyPlayersWarning.SetText((string) null);
    this.multiplayerLobbyList = multiplayerLobbyList;
    this.lobby = lobby;
    this.lobbyNameText.text = this.LobbyName;
    this.MissionName = lobby.MissionNameSanitized;
    this.MapName = lobby.MapNameSanitized;
    if (string.IsNullOrEmpty(this.MapName))
      this.missionNameText.text = this.MissionName ?? "";
    else if (string.IsNullOrEmpty(this.MissionName))
      this.missionNameText.text = this.MapName ?? "";
    else
      this.missionNameText.text = $"{this.MapName} | {this.MissionName}";
    this.isPasswordProtected = lobby.IsPasswordProtected(out string _);
    bool dedicatedServer = lobby.DedicatedServer;
    string str = !this.isPasswordProtected ? (!dedicatedServer ? LobbyListItem.playerIcon : LobbyListItem.serverIcon) : (!dedicatedServer ? $"{LobbyListItem.playerIcon} {LobbyListItem.lockIcon}" : $"{LobbyListItem.serverIcon} {LobbyListItem.lockIcon}");
    if (lobby.ModdedServer)
      str = LobbyListItem.moddedIcon + str;
    this.iconsText.text = str;
    int current;
    int max;
    if (lobby.GetPlayerCounts(out current, out max))
    {
      this.PlayerCount = current;
      this.playerText.text = $"{(max <= multiplayerLobbyList.TooManyPlayerLimit ? (object) "" : (object) (GoogleIconFont.FontString("\uE002").AddColor(Color.yellow) + " "))}[{current} / {max}]";
    }
    else
      this.playerText.text = "";
    this.tooManyPlayersWarning.enabled = max > multiplayerLobbyList.TooManyPlayerLimit;
    this.StartTime = lobby.StartTime;
    this.uptimeText.text = LobbyInstance.TimeSpanString(this.StartTime, false);
    this.SetPingTextAndColor(lobby.CalculatePing());
    this.shown = true;
    this.gameObject.SetActive(true);
    return true;
  }

  private void SetPingTextAndColor(int? ping)
  {
    this.Ping = ping;
    string str = ping.HasValue ? $"{ping.Value}ms" : "";
    this.pingText.color = this.ColorFromPing(ping);
    this.pingText.text = str;
  }

  private Color ColorFromPing(int? ping)
  {
    if (ping.HasValue)
    {
      int valueOrDefault = ping.GetValueOrDefault();
      if (valueOrDefault < 120)
        return valueOrDefault < 60 ? LobbyListItem.StatusSuccessColor : LobbyListItem.StatusWarningColor;
      if (valueOrDefault >= 150)
        return LobbyListItem.StatusDangerColor;
    }
    return LobbyListItem.TextMutedColor;
  }

  public void Hide()
  {
    this.shown = false;
    this.lobby = (LobbyInstance) null;
    this.ShowBoarder(false);
    this.gameObject.SetActive(false);
  }

  public void UpdatePing() => this.SetPingTextAndColor(this.lobby.CalculatePing());

  public void UpdateTime()
  {
    this.uptimeText.text = LobbyInstance.TimeSpanString(this.StartTime, false);
  }

  public void OnPointerClick(PointerEventData eventData)
  {
    this.multiplayerLobbyList.ShowLobbyPopup(this.lobby);
  }

  public void OnPointerEnter(PointerEventData eventData) => this.ShowBoarder(true);

  public void OnPointerExit(PointerEventData eventData) => this.ShowBoarder(false);

  private void ShowBoarder(bool show)
  {
    this.backgroundImage.color = show ? this.hoverBackground : this.normalBackground;
    this.boarderImage.color = show ? this.hoverBoarder : this.normalBoarder;
  }
}

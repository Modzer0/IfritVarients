// Decompiled with JetBrains decompiler
// Type: NuclearOption.Networking.Lobbies.LobbyDetailsModal
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Cysharp.Threading.Tasks;
using NuclearOption.SavedMission;
using NuclearOption.Workshop;
using Steamworks;
using System;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

#nullable disable
namespace NuclearOption.Networking.Lobbies;

public class LobbyDetailsModal : MonoBehaviour
{
  [Header("References")]
  [SerializeField]
  private Sprite defaultMissionImage;
  [SerializeField]
  private GameObject holder;
  [Header("Fields")]
  [SerializeField]
  private TextMeshProUGUI lobbyNameText;
  [SerializeField]
  private TextMeshProUGUI missionNameText;
  [SerializeField]
  private TextMeshProUGUI mapText;
  [SerializeField]
  private Image missionImage;
  [SerializeField]
  private TextMeshProUGUI missionDescriptionText;
  [SerializeField]
  private TextMeshProUGUI serverTypeText;
  [SerializeField]
  private GameObject moddedWarning;
  [SerializeField]
  private TextMeshProUGUI missionTypeText;
  [SerializeField]
  private TextMeshProUGUI pingText;
  [SerializeField]
  private TextMeshProUGUI upTimeText;
  [SerializeField]
  private TextMeshProUGUI playersText;
  [Header("Buttons")]
  [SerializeField]
  private Button openWorkshopButton;
  [SerializeField]
  private Button closeDetails;
  [SerializeField]
  private Button joinButton;
  [SerializeField]
  private Button blockPlayerButton;
  [SerializeField]
  private GameObject blockPlayerButtonConfirmOverlay;
  [SerializeField]
  private Button blockPlayerButtonConfirm;
  [SerializeField]
  private Button blockPlayerButtonCancel;
  private LobbyInstance lobby;
  private CancellationTokenSource detailsOpenCancelSource;
  private LobbyList lobbyList;

  private void Awake()
  {
    this.openWorkshopButton.onClick.AddListener(new UnityAction(this.OnOpenWorkshop));
    this.closeDetails.onClick.AddListener(new UnityAction(this.Hide));
    this.joinButton.onClick.AddListener(new UnityAction(this.Join));
    this.blockPlayerButtonConfirmOverlay.SetActive(false);
    this.blockPlayerButton.gameObject.SetActive(false);
  }

  public void Show(LobbyList lobbyList, LobbyInstance lobby)
  {
    if (this.lobby != null)
      throw new InvalidOperationException("LobbyDetailPopup already had lobby, it can't be shown multiple times");
    if (lobby == null)
      throw new ArgumentException("can't show invalid lobby");
    this.lobbyList = lobbyList;
    this.lobby = lobby;
    this.detailsOpenCancelSource = new CancellationTokenSource();
    this.holder.SetActive(true);
    this.lobbyNameText.text = lobby.LobbyNameSanitized;
    this.missionNameText.text = lobby.MissionNameSanitized;
    this.mapText.text = lobby.MapNameSanitized;
    this.missionDescriptionText.text = lobby.MissionDescriptionSanitized;
    this.serverTypeText.text = lobby.DedicatedServer ? "Dedicated Server" : "Player Hosted";
    this.moddedWarning.SetActive(lobby.ModdedServer);
    TextMeshProUGUI missionTypeText = this.missionTypeText;
    string str;
    switch (lobby.MissionPvpType)
    {
      case MissionPvpType.Pvp:
        str = "PVP";
        break;
      case MissionPvpType.Pve:
        str = "PVE";
        break;
      default:
        str = "";
        break;
    }
    missionTypeText.text = str;
    PublishedFileId_t missionWorkshopId = lobby.MissionWorkshopId;
    this.openWorkshopButton.gameObject.SetActive(missionWorkshopId != PublishedFileId_t.Invalid);
    this.SetPreviewImage(missionWorkshopId, lobby.MissionNameRaw, this.detailsOpenCancelSource.Token).Forget();
    this.UpdateLoop(this.detailsOpenCancelSource.Token).Forget();
  }

  private async UniTask SetPreviewImage(
    PublishedFileId_t id,
    string missionName,
    CancellationToken cancellationToken)
  {
    this.missionImage.color = new Color(0.2f, 0.2f, 0.2f);
    this.missionImage.sprite = (Sprite) null;
    Sprite sprite = await LobbyDetailsModal.GetSprite(id, missionName, cancellationToken);
    this.missionImage.color = Color.white;
    this.missionImage.sprite = sprite ?? this.defaultMissionImage;
  }

  private static async UniTask<Sprite> GetSprite(
    PublishedFileId_t id,
    string missionName,
    CancellationToken cancellationToken)
  {
    if (id != PublishedFileId_t.Invalid)
    {
      (bool flag, SteamWorkshopItem steamWorkshopItem) = await SteamWorkshop.GetDetails(id);
      if (flag)
        return await steamWorkshopItem.GetPreview(cancellationToken);
    }
    return MissionGroup.BuiltIn.ContainsMission(missionName) ? await MissionGroup.BuiltIn.GetPreview(missionName, cancellationToken) : (Sprite) null;
  }

  private async UniTask UpdateLoop(CancellationToken cancellation)
  {
    int pingCheck = 10;
    DateTime? startTime = this.lobby.StartTime;
    if (!startTime.HasValue)
      this.upTimeText.text = "";
    while (!cancellation.IsCancellationRequested)
    {
      if (startTime.HasValue)
        this.upTimeText.text = LobbyInstance.TimeSpanString(startTime, true);
      ++pingCheck;
      if (pingCheck >= 10)
      {
        pingCheck = 0;
        int? ping = this.lobby.CalculatePing();
        this.pingText.text = ping.HasValue ? $"{ping.Value}ms" : "";
        int current;
        int max;
        if (this.lobby.GetPlayerCounts(out current, out max))
        {
          this.joinButton.interactable = current < max;
          this.playersText.text = $"{current}/{max}";
        }
        else
        {
          this.joinButton.interactable = false;
          this.playersText.text = "";
        }
      }
      await UniTask.Delay(1000, true);
    }
  }

  private void OnDisable() => this.Hide();

  private void Hide()
  {
    this.lobby = (LobbyInstance) null;
    this.detailsOpenCancelSource?.Cancel();
    this.detailsOpenCancelSource = (CancellationTokenSource) null;
    if (!this.holder.activeSelf)
      return;
    this.holder.SetActive(false);
  }

  private void OnOpenWorkshop()
  {
    PublishedFileId_t missionWorkshopId = this.lobby.MissionWorkshopId;
    if (!(missionWorkshopId != PublishedFileId_t.Invalid))
      return;
    SteamWorkshopItem.OpenSteamPage(missionWorkshopId);
  }

  private void Join() => SteamLobby.instance.TryJoinLobby(this.lobby, (string) null, true);

  private void BlockPlayer()
  {
    this.blockPlayerButtonConfirmOverlay.SetActive(false);
    throw new NotImplementedException("no way to verify ownerId, so can't block from lobby");
  }
}

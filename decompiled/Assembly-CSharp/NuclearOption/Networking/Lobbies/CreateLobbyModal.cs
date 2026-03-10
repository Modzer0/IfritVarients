// Decompiled with JetBrains decompiler
// Type: NuclearOption.Networking.Lobbies.CreateLobbyModal
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Cysharp.Threading.Tasks;
using NuclearOption.SavedMission;
using NuclearOption.SceneLoading;
using NuclearOption.UI;
using Steamworks;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

#nullable disable
namespace NuclearOption.Networking.Lobbies;

public class CreateLobbyModal : MonoBehaviour
{
  private static readonly string noPasswordString = "<color=#FF0><font=\"MaterialSymbolsSharp\">\uE002</font></color> <i>no password</i>";
  [Header("Modal")]
  [SerializeField]
  private GameObject holder;
  [SerializeField]
  private Button close;
  [SerializeField]
  private GameObject creatingLobbyOverlay;
  [Header("Mission picker")]
  [SerializeField]
  private Button openMissionPickerButton;
  [SerializeField]
  private MissionsPicker missionsPicker;
  [SerializeField]
  private TextMeshProUGUI missionNameText;
  [SerializeField]
  private string noMissionText = "<select a mission>";
  [SerializeField]
  private MapLoader mapLoader;
  [Header("New Lobby Settings")]
  [SerializeField]
  private TMP_InputField inputLobbyName;
  [SerializeField]
  private Slider maxPlayersSlider;
  [SerializeField]
  private GameObject tooManyPlayerWarningHolder;
  [SerializeField]
  private BetterToggleGroup lobbyTypeGroup;
  [SerializeField]
  private BoxToggle passwordInputToggle;
  [SerializeField]
  private TextMeshProUGUI passwordPlaceholder;
  [SerializeField]
  private TMP_InputField passwordInput;
  [SerializeField]
  private Button createLobby;
  private bool usingDefaultLobbyName;
  private Mission selectedMission;
  private Color missionNameTextStartColor;
  private LobbyList lobbyList;

  private void OnValidate()
  {
    if (!((UnityEngine.Object) this.missionNameText != (UnityEngine.Object) null) || this.selectedMission != null)
      return;
    this.missionNameText.text = this.noMissionText;
  }

  public void Show(LobbyList lobbyList)
  {
    this.holder.SetActive(true);
    this.lobbyList = lobbyList;
  }

  public void Hide() => this.holder.SetActive(false);

  private void OnEnable()
  {
    this.creatingLobbyOverlay.SetActive(false);
    this.inputLobbyName.text = "";
  }

  private void Awake()
  {
    SteamLobby.instance.CheckRelayLocationTask();
    this.close.onClick.AddListener(new UnityAction(this.Hide));
    this.inputLobbyName.onValueChanged.AddListener((UnityAction<string>) (name => this.usingDefaultLobbyName = string.IsNullOrEmpty(name)));
    this.missionNameTextStartColor = this.missionNameText.color;
    this.usingDefaultLobbyName = true;
    this.OnMissionConfirmed((Mission) null);
    this.missionsPicker.SetPickerFilter(new MissionsPicker.Filter()
    {
      DisallowedGroups = new List<MissionGroup>()
      {
        (MissionGroup) MissionGroup.Tutorial
      },
      RequiredTags = new List<MissionTag>()
      {
        MissionTag.Multiplayer
      }
    });
    this.missionsPicker.OnMissionConfirmed += new Action<Mission>(this.OnMissionConfirmed);
    this.openMissionPickerButton.onClick.AddListener(new UnityAction(this.missionsPicker.ShowPicker));
    this.createLobby.onClick.AddListener(UniTask.UnityAction(new Func<UniTaskVoid>(this.HostLobby)));
    this.maxPlayersSlider.onValueChanged.AddListener(new UnityAction<float>(this.MaxPlayersSlider));
    this.passwordInputToggle.isOn = false;
    this.PasswordToggleChanged((bool) (UnityEngine.Object) this.passwordInput);
    this.passwordInputToggle.onValueChanged.AddListener(new UnityAction<bool>(this.PasswordToggleChanged));
  }

  private void MaxPlayersSlider(float _)
  {
    this.tooManyPlayerWarningHolder.SetActive((int) this.maxPlayersSlider.value > this.lobbyList.TooManyPlayerLimit);
  }

  private void PasswordToggleChanged(bool hasPassword)
  {
    this.passwordPlaceholder.text = hasPassword ? "<i>password</i>" : CreateLobbyModal.noPasswordString;
  }

  private void OnMissionConfirmed(Mission mission)
  {
    if (this.usingDefaultLobbyName)
      this.inputLobbyName.SetTextWithoutNotify($"{mission?.Name ?? "Mission"} [Hosted by {PlayerSettings.playerName_Unsanitized}]");
    this.missionNameText.text = mission?.Name ?? "<select a mission>";
    this.missionNameText.color = mission != null ? this.missionNameTextStartColor : this.missionNameTextStartColor * 0.6f;
    this.selectedMission = mission;
    this.createLobby.interactable = mission != null;
    this.missionsPicker.HidePicker();
  }

  public async UniTaskVoid HostLobby()
  {
    MissionManager.SetMission(this.selectedMission, false);
    int maxPlayers = (int) this.maxPlayersSlider.value;
    ELobbyType elobbyType;
    switch (this.lobbyTypeGroup.Value)
    {
      case 1:
        elobbyType = ELobbyType.k_ELobbyTypeFriendsOnly;
        break;
      case 2:
        elobbyType = ELobbyType.k_ELobbyTypePrivate;
        break;
      default:
        elobbyType = ELobbyType.k_ELobbyTypePublic;
        break;
    }
    ELobbyType lobbyType = elobbyType;
    this.creatingLobbyOverlay.SetActive(true);
    HostedLobbyInstance? nullable1 = await SteamLobby.instance.HostLobby(maxPlayers, lobbyType);
    if (nullable1.HasValue)
    {
      HostedLobbyInstance hostedLobbyInstance = nullable1.Value;
      string text1 = this.inputLobbyName.text;
      string input = !string.IsNullOrEmpty(text1) ? text1 : SteamFriends.GetPersonaName() + "'s lobby";
      SteamLobby.instance.CurrentLobbyName = input;
      hostedLobbyInstance.SetData("name", input.SanitizeRichText(128 /*0x80*/));
      hostedLobbyInstance.SetData("mission_name", this.selectedMission.Name.SanitizeRichText(128 /*0x80*/));
      hostedLobbyInstance.SetData("mission_description", this.selectedMission.missionSettings.description.SanitizeRichText(1000));
      hostedLobbyInstance.SetData("mission_pvp_type", MissionTag.GetPvpTypeLobbyString(this.selectedMission));
      string mapName;
      if (this.mapLoader.TryGetMapName(this.selectedMission.MapKey, out mapName))
        hostedLobbyInstance.SetData("map_name", mapName);
      ref MissionKey? local1 = ref this.selectedMission.LoadKey;
      PublishedFileId_t? nullable2 = local1.HasValue ? local1.GetValueOrDefault().WorkshopId : new PublishedFileId_t?();
      if (nullable2.HasValue)
        hostedLobbyInstance.SetData("mission_workshop_id", nullable2.Value.m_PublishedFileId.ToString("X"));
      string text2 = this.passwordInput.text;
      bool flag = this.passwordInputToggle.isOn && !string.IsNullOrEmpty(text2);
      if (flag)
        hostedLobbyInstance.SetData("short_password", LobbyPassword.GetShortPassword(text2));
      bool? moddedServer = NetworkManagerNuclearOption.ModdedServer;
      if (moddedServer.HasValue)
      {
        ref HostedLobbyInstance local2 = ref hostedLobbyInstance;
        moddedServer = NetworkManagerNuclearOption.ModdedServer;
        string tag = LobbyInstance.BoolToTag(moddedServer.Value);
        local2.SetData("modded_server", tag);
      }
      await NetworkManagerNuclearOption.i.StartHostAsync(new HostOptions(SocketType.Steam, GameState.Multiplayer, this.selectedMission.MapKey)
      {
        MaxConnections = new int?(maxPlayers - 1),
        Password = flag ? text2 : (string) null
      });
    }
    if (!((UnityEngine.Object) this.creatingLobbyOverlay != (UnityEngine.Object) null))
      return;
    this.creatingLobbyOverlay.SetActive(false);
  }
}

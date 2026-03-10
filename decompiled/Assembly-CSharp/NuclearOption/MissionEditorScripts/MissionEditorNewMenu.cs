// Decompiled with JetBrains decompiler
// Type: NuclearOption.MissionEditorScripts.MissionEditorNewMenu
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Cysharp.Threading.Tasks;
using NuclearOption.SavedMission;
using NuclearOption.SceneLoading;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

#nullable disable
namespace NuclearOption.MissionEditorScripts;

public class MissionEditorNewMenu : MonoBehaviour
{
  [Header("Name")]
  [SerializeField]
  private TMP_InputField missionName;
  [SerializeField]
  private GameObject nameWarning;
  [Header("Player mode")]
  [SerializeField]
  private Toggle singlePlayerToggle;
  [SerializeField]
  private Toggle multiPlayerToggle;
  [SerializeField]
  private GameObject playerModeInvalid;
  [Header("Factions")]
  [SerializeField]
  private Toggle boscaliToggle;
  [SerializeField]
  private Toggle primevaToggle;
  [SerializeField]
  private GameObject factionInvalid;
  [Header("Maps")]
  [SerializeField]
  private MapLoader mapLoader;
  [SerializeField]
  private RectTransform mapListParent;
  [SerializeField]
  private NewMissionMapButton mapButtonPrefab;
  [SerializeField]
  private NewMissionMapButton[] mapButtons;
  [Header("Buttons")]
  [SerializeField]
  private Button createButton;
  [SerializeField]
  private GameObject loadingNotification;
  private MapDetails selectedMap;

  private void Awake()
  {
    this.singlePlayerToggle.onValueChanged.AddListener(new UnityAction<bool>(this.OnPlayerModeToggle));
    this.multiPlayerToggle.onValueChanged.AddListener(new UnityAction<bool>(this.OnPlayerModeToggle));
    this.boscaliToggle.onValueChanged.AddListener(new UnityAction<bool>(this.OnFactionToggle));
    this.primevaToggle.onValueChanged.AddListener(new UnityAction<bool>(this.OnFactionToggle));
    this.missionName.onValueChanged.AddListener(new UnityAction<string>(this.NameChanged));
    this.missionName.onEndEdit.AddListener(new UnityAction<string>(this.NameEditEnd));
    this.createButton.onClick.AddListener(new UnityAction(this.Create));
    for (int index = 0; index < this.mapButtons.Length; ++index)
    {
      MapDetails map = this.mapLoader.Maps[index];
      this.mapButtons[index].Button.onClick.AddListener((UnityAction) (() => this.MapSelected(map)));
    }
    this.MapSelected(((IEnumerable<MapDetails>) this.mapLoader.Maps).First<MapDetails>());
  }

  private void MapSelected(MapDetails mapDetails)
  {
    this.selectedMap = mapDetails;
    foreach (NewMissionMapButton mapButton in this.mapButtons)
      mapButton.OnMapSelected(this.selectedMap);
  }

  private void NameChanged(string newName)
  {
    this.nameWarning.SetActive(MissionSaveLoad.Exist(newName));
    FixLayout.ForceRebuildRecursive((RectTransform) this.transform);
    this.CheckCreateInteractable();
  }

  private void NameEditEnd(string newName)
  {
    if (MissionSaveLoad.ValidateName(ref newName, true))
      this.missionName.SetTextWithoutNotify(newName);
    this.NameChanged(newName);
  }

  private void OnEnable()
  {
    this.missionName.text = "";
    this.singlePlayerToggle.isOn = true;
    this.multiPlayerToggle.isOn = true;
    this.boscaliToggle.isOn = true;
    this.primevaToggle.isOn = true;
    this.NameChanged("");
    this.OnPlayerModeToggle(false);
    this.OnFactionToggle(false);
  }

  private void OnPlayerModeToggle(bool _)
  {
    this.playerModeInvalid.SetActive(!this.singlePlayerToggle.isOn && !this.multiPlayerToggle.isOn);
    this.CheckCreateInteractable();
    FixLayout.ForceRebuildRecursive((RectTransform) this.transform);
  }

  private void OnFactionToggle(bool _)
  {
    this.factionInvalid.SetActive(!this.boscaliToggle.isOn && !this.primevaToggle.isOn);
    this.CheckCreateInteractable();
    FixLayout.ForceRebuildRecursive((RectTransform) this.transform);
  }

  private void CheckCreateInteractable()
  {
    this.createButton.interactable = !string.IsNullOrEmpty(this.missionName.text) && !this.playerModeInvalid.activeSelf && !this.factionInvalid.activeSelf;
  }

  private void Create()
  {
    string text = this.missionName.text;
    PlayerMode playerMode;
    if (this.singlePlayerToggle.isOn && this.multiPlayerToggle.isOn)
      playerMode = PlayerMode.SingleAndMultiplayer;
    else if (this.singlePlayerToggle.isOn)
    {
      playerMode = PlayerMode.Singleplayer;
    }
    else
    {
      if (!this.multiPlayerToggle.isOn)
        throw new InvalidOperationException("Invalid Player Mode toggle");
      playerMode = PlayerMode.Multiplayer;
    }
    List<string> joinableFactions = new List<string>();
    if (this.boscaliToggle.isOn)
      joinableFactions.Add("Boscali");
    if (this.primevaToggle.isOn)
      joinableFactions.Add("Primeva");
    if (joinableFactions.Count == 0)
      throw new InvalidOperationException("Invalid faction toggle");
    if ((UnityEngine.Object) this.loadingNotification != (UnityEngine.Object) null)
      this.loadingNotification.SetActive(true);
    MapKey map = MapKey.GameWorldPrefab(this.selectedMap.PrefabName);
    MissionEditor.LoadEditor(new NewMissionConfig(text, map, playerMode, joinableFactions)).Forget();
  }
}

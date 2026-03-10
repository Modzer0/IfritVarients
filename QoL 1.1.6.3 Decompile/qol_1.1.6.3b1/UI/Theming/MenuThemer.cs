// Decompiled with JetBrains decompiler
// Type: qol.UI.Theming.MenuThemer
// Assembly: qol, Version=1.1.6.3, Culture=neutral, PublicKeyToken=null
// MVID: 3B6539EA-E38C-45EE-8FFF-FC58CC42BADC
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\BepInEx\plugins\qol_1.1.6.3b1.dll

using BepInEx.Logging;
using qol.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

#nullable disable
namespace qol.UI.Theming;

public static class MenuThemer
{
  private static ManualLogSource _logger;

  public static void Initialize(ManualLogSource logger) => MenuThemer._logger = logger;

  public static IEnumerator ApplyOneTimeTheme(bool notDedicatedServer)
  {
    Stopwatch stopwatch = Stopwatch.StartNew();
    if (notDedicatedServer)
      yield return (object) null;
    UIPathHelpers.ApplyImageColors(("MissionSelectItem/Tag List/Tag Badge/Image", UIThemeConfigs.BackgroundMenu), ("MissionSelectItem/Tag List/Tag Badge (1)/Image", UIThemeConfigs.BackgroundMenu), ("Tag Filter Button/Boarder", UIThemeConfigs.Transparent), ("MissionEditor/PopupMenus/LoadMenu/Mission Picker/SelectionPanel/MissionSelectPanel/Filter Tags/Tag Toggle Button/Boarder", UIThemeConfigs.Transparent), ("MissionEditor/PopupMenus/LoadMenu/Mission Picker/SelectionPanel/MissionSelectPanel/Filter Tags", UIThemeConfigs.Transparent));
    UIPathHelpers.ApplyButtonColors(UIThemeConfigs.ButtonColor, "MissionTabButton", "MissionSelectItem");
    MenuThemer.ApplyWorkshopItemTheme();
    UIPathHelpers.ApplyImageColors(("WorkshopMenu/Panel/Main Panel", UIThemeConfigs.Transparent), ("WorkshopMenu/Panel/Header", UIThemeConfigs.Transparent), ("WorkshopMenu/Panel/Header/Outline", UIThemeConfigs.Transparent), ("WorkshopMenu/Panel/Main Panel/Workshop List panel/Scroll View", UIThemeConfigs.Transparent), ("WorkshopMenu/Panel/Main Panel/Workshop List panel/Tabs", UIThemeConfigs.Transparent), ("WorkshopMenu/Background", UIThemeConfigs.BackgroundMenuDark), ("WorkshopMenu/Panel/Button Panel/Outline", UIThemeConfigs.Transparent), ("WorkshopMenu/Panel/Button Panel", UIThemeConfigs.BackgroundMenuDark));
    UIPathHelpers.ApplyButtonColors(UIThemeConfigs.ButtonColor, "WorkshopMenu/Panel/Button Panel/Top Group List Controls/ClearFilterButton", "WorkshopMenu/Panel/Button Panel/Top Group List Controls/RefreshButton", "WorkshopMenu/Panel/Button Panel/Bottom group/UpdateButton", "WorkshopMenu/Panel/Button Panel/Bottom group/UploadButton", "WorkshopMenu/Panel/Button Panel/Bottom group/OpenWorkshopButton", "WorkshopMenu/Panel/Button Panel/Bottom group/BacktoListButton", "WorkshopMenu/Panel/Button Panel/Bottom group/MenuExit_Button");
    UIPathHelpers.ApplyImageColors(("SettingsMenu/Background", UIThemeConfigs.Transparent), ("SettingsMenu/BackgroundMasker", UIThemeConfigs.BackgroundMenuDark), ("SettingsMenu/LeftPanel/Outline", UIThemeConfigs.Transparent), ("SettingsMenu/SettingsTitle", UIThemeConfigs.Transparent), ("SettingsMenu/SettingsTitle/outline", UIThemeConfigs.Transparent), ("ControlsSettingsRewired", UIThemeConfigs.Transparent), ("GameplaySettings", UIThemeConfigs.Transparent), ("AudioSettings", UIThemeConfigs.Transparent), ("GraphicsSettings", UIThemeConfigs.Transparent), ("ChatSettings", UIThemeConfigs.Transparent), ("HUDSettings", UIThemeConfigs.Transparent), ("SettingsMenu/LeftPanel", UIThemeConfigs.BackgroundMenuDark));
    MenuThemer.ApplySettingsPanelThemes();
    UIPathHelpers.ApplyButtonColors(UIThemeConfigs.ButtonColor, "SettingsMenu/LeftPanel/TabsPanel/ControlsButton", "SettingsMenu/LeftPanel/TabsPanel/GameplayButton", "SettingsMenu/LeftPanel/TabsPanel/AudioButton", "SettingsMenu/LeftPanel/TabsPanel/GraphicsButton", "SettingsMenu/LeftPanel/TabsPanel/HUDButton", "SettingsMenu/LeftPanel/TabsPanel/ChatButton");
    UIPathHelpers.SetButtonColors("SettingsMenu/LeftPanel/MenuExit_Button", UIThemeConfigs.ExitButtonColorBlock);
    UIPathHelpers.ApplyButtonColors(UIThemeConfigs.ButtonColor, "ControlsSettingsRewired/Scroll View/Viewport/Content/BindingsPanel/Edit Control Bindings", "ChatSettings/TestButton");
    UIPathHelpers.SetTextColor("ChatSettings/TestButton/Text (TMP)", Color.white);
    stopwatch.Stop();
    MenuThemer._logger?.LogInfo((object) $"One-time theme loading elapsed {stopwatch.Elapsed.TotalSeconds}s");
  }

  public static IEnumerator ApplySceneTheme(bool notDedicatedServer)
  {
    Stopwatch stopwatch = Stopwatch.StartNew();
    if (notDedicatedServer)
      yield return (object) null;
    MenuThemer._logger?.LogDebug((object) "Searching for menu");
    if ((UnityEngine.Object) GameObject.Find("MainCanvas/Prejoin menu") != (UnityEngine.Object) null)
      MenuThemer.ApplyMainMenuTheme();
    else if ((UnityEngine.Object) GameObject.Find("MainCanvas/Header") != (UnityEngine.Object) null && (UnityEngine.Object) GameObject.Find("MainCanvas/LobbiesManager") == (UnityEngine.Object) null)
      MenuThemer.ApplySingleplayerMenuTheme();
    else if ((UnityEngine.Object) GameObject.Find("MainCanvas/LobbiesManager") != (UnityEngine.Object) null)
      MenuThemer.ApplyMultiplayerMenuTheme();
    if (notDedicatedServer)
      yield return (object) null;
    stopwatch.Stop();
    MenuThemer._logger?.LogInfo((object) $"Scene theme loading elapsed {stopwatch.Elapsed.TotalSeconds}s");
  }

  private static void ApplyWorkshopItemTheme()
  {
    string str1 = "WorkshopMenu/Panel/Main Panel/Workshop List panel/Scroll View/Viewport/Content/ListContent";
    UIPathHelpers.SetImageColor("Workshop Item", UIThemeConfigs.ButtonMain);
    UIPathHelpers.SetTextColor("Workshop Item/Name", Color.white);
    for (int index = 0; index <= 5; ++index)
    {
      string str2 = index == 0 ? "" : $" ({index})";
      UIPathHelpers.SetImageColor($"{str1}/Workshop Item{str2}", UIThemeConfigs.ButtonMain);
      UIPathHelpers.SetTextColor($"{str1}/Workshop Item{str2}/Name", Color.white);
    }
  }

  private static void ApplySettingsPanelThemes()
  {
    string[] strArray1 = new string[3]
    {
      "ControlsSettingsRewired/Scroll View/Viewport/Content/BindingsPanel",
      "ControlsSettingsRewired/Scroll View/Viewport/Content/SideBySidePanel/TogglesPanel",
      "ControlsSettingsRewired/Scroll View/Viewport/Content/SideBySidePanel/SlidersPanel"
    };
    foreach (string path in strArray1)
    {
      UIPathHelpers.SetImageColor(path, UIThemeConfigs.BackgroundMenu);
      UIPathHelpers.SetImageFillCenter(path, true);
    }
    string[] strArray2 = new string[3]
    {
      "GameplaySettings/Scroll View/Viewport/Content/UnitsPanel",
      "GameplaySettings/Scroll View/Viewport/Content/SidebySideContainer/CameraPanel",
      "GameplaySettings/Scroll View/Viewport/Content/SidebySideContainer/KillFeedPanel"
    };
    foreach (string path in strArray2)
    {
      UIPathHelpers.SetImageColor(path, UIThemeConfigs.BackgroundMenu);
      UIPathHelpers.SetImageFillCenter(path, true);
    }
    string[] strArray3 = new string[3]
    {
      "HUDSettings/TopPanel",
      "HUDSettings/SideBySidePanel/HUDPanel",
      "HUDSettings/SideBySidePanel/HMDPanel"
    };
    foreach (string path in strArray3)
    {
      UIPathHelpers.SetImageColor(path, UIThemeConfigs.BackgroundMenu);
      UIPathHelpers.SetImageFillCenter(path, true);
    }
  }

  private static void ApplyMainMenuTheme()
  {
    MenuThemer._logger?.LogDebug((object) "Found main menu");
    GameObject.Find("MainCanvas/Prejoin menu/LeftPanel").GetComponent<Image>().color = UIThemeConfigs.BackgroundMenuDark;
    GameObject.Find("MainCanvas/Prejoin menu/LeftPanel/Container").GetComponent<Image>().color = UIThemeConfigs.Transparent;
    GameObject.Find("MainCanvas/HintPanel").GetComponent<Image>().color = UIThemeConfigs.BackgroundMenuAlt;
    GameObject.Find("MainCanvas/NewsPanel").GetComponent<Image>().color = UIThemeConfigs.BackgroundMenuAlt;
    GameObject.Find("MainCanvas/Prejoin menu/LeftPanel/Container/MenuButtonsPanel/SinglePlayerButton").GetComponent<Button>().colors = UIThemeConfigs.ButtonColor;
    GameObject.Find("MainCanvas/Prejoin menu/LeftPanel/Container/MenuButtonsPanel/MultiplayerButton").GetComponent<Button>().colors = UIThemeConfigs.ButtonColor;
    GameObject.Find("MainCanvas/Prejoin menu/LeftPanel/Container/MenuButtonsPanel/MissionEditorButton").GetComponent<Button>().colors = UIThemeConfigs.ButtonColor;
    GameObject.Find("MainCanvas/Prejoin menu/LeftPanel/Container/MenuButtonsPanel/SettingsButton").GetComponent<Button>().colors = UIThemeConfigs.ButtonColor;
    GameObject.Find("MainCanvas/Prejoin menu/LeftPanel/Container/MenuButtonsPanel/EncyclopediaButton").GetComponent<Button>().colors = UIThemeConfigs.ButtonColor;
    GameObject.Find("MainCanvas/Prejoin menu/LeftPanel/Container/MenuButtonsPanel/WorkshopButton").GetComponent<Button>().colors = UIThemeConfigs.ButtonColor;
    GameObject.Find("MainCanvas/Prejoin menu/LeftPanel/ExitButton").GetComponent<Button>().colors = UIThemeConfigs.ExitButtonColorBlock;
    PathLookup.Find("MainCanvas/Editor PopupMenus/LoadMenu/Mission Picker/SelectionPanel/MissionSelectPanel/Background/Mission List/Viewport/Content/MissionSelectItem").GetComponent<Button>().colors = UIThemeConfigs.ButtonColor;
    PathLookup.Find("MainCanvas/Editor PopupMenus/LoadMenu/Mission Picker/SelectionPanel/MissionSelectPanel/Background/Mission List/Viewport/Content/MissionSelectItem (1)").GetComponent<Button>().colors = UIThemeConfigs.ButtonColor;
    PathLookup.Find("MainCanvas/Editor PopupMenus/LoadMenu/Mission Picker/SelectionPanel/MissionSelectPanel/Background/Mission List/Viewport/Content/MissionSelectItem (2)").GetComponent<Button>().colors = UIThemeConfigs.ButtonColor;
    PathLookup.Find("MainCanvas/ConfirmLeave/Panel").GetComponent<Image>().color = UIThemeConfigs.BackgroundMenuAlt;
    PathLookup.Find("MainCanvas/ConfirmLeave/Confirm").GetComponent<Button>().colors = UIThemeConfigs.ExitButtonColorBlock;
    PathLookup.Find("MainCanvas/ConfirmLeave/Cancel").GetComponent<Button>().colors = UIThemeConfigs.ButtonColor;
    UnityEngine.Object.Destroy((UnityEngine.Object) GameObject.Find("MainCanvas/NewsPanel"));
    PathLookup.Find("MainCanvas/DisconnectNotification/Panel").GetComponent<Image>().color = UIThemeConfigs.BackgroundMenuAlt;
    PathLookup.Find("MainCanvas/DisconnectNotification/Button (Legacy)").GetComponent<Button>().colors = UIThemeConfigs.ButtonColor;
    PathLookup.Find("MainCanvas/SteamConnectionFail/Panel").GetComponent<Image>().color = UIThemeConfigs.BackgroundMenuAlt;
    PathLookup.Find("MainCanvas/SteamConnectionFail/Button (Legacy)").GetComponent<Button>().colors = UIThemeConfigs.ButtonColor;
    PathLookup.Find("MainCanvas/Editor PopupMenus").GetComponent<Image>().color = UIThemeConfigs.BackgroundMenuDark;
    PathLookup.Find("MainCanvas/Editor PopupMenus/SetupMenu").GetComponent<Image>().color = UIThemeConfigs.BackgroundMenuAlt;
    PathLookup.Find("MainCanvas/Editor PopupMenus/SetupMenu/Buttons/NewButton").GetComponent<Button>().colors = UIThemeConfigs.ButtonColor;
    PathLookup.Find("MainCanvas/Editor PopupMenus/SetupMenu/Buttons/LoadButton").GetComponent<Button>().colors = UIThemeConfigs.ButtonColor;
    PathLookup.Find("MainCanvas/Editor PopupMenus/SetupMenu/Close Button").GetComponent<Button>().colors = UIThemeConfigs.ExitButtonColorBlock;
    PathLookup.Find("MainCanvas/Editor PopupMenus/NewMenu").GetComponent<Image>().color = UIThemeConfigs.BackgroundMenu;
    PathLookup.Find("MainCanvas/Editor PopupMenus/NewMenu/Header").GetComponent<Image>().color = UIThemeConfigs.BackgroundMenu;
    PathLookup.Find("MainCanvas/Editor PopupMenus/NewMenu/Body/Buttons/New Button (1)").GetComponent<Button>().colors = UIThemeConfigs.ButtonColor;
    PathLookup.Find("MainCanvas/Editor PopupMenus/NewMenu/Body/Buttons/QuitButton (1)").GetComponent<Button>().colors = UIThemeConfigs.ExitButtonColorBlock;
    PathLookup.Find("MainCanvas/Editor PopupMenus/LoadMenu/Mission Picker/SelectionPanel").GetComponent<Image>().color = UIThemeConfigs.Transparent;
    PathLookup.Find("MainCanvas/Editor PopupMenus/LoadMenu/Mission Picker/InfoPanel").GetComponent<Image>().color = UIThemeConfigs.Transparent;
    PathLookup.Find("MainCanvas/Editor PopupMenus/LoadMenu/Mission Picker/SelectionPanel/MissionSelectPanel/Background").GetComponent<Image>().color = UIThemeConfigs.Transparent;
    PathLookup.Find("MainCanvas/Editor PopupMenus/LoadMenu/Mission Picker/SelectionPanel/HostingPanelTitle/HostGameTitle").GetComponent<Image>().color = UIThemeConfigs.Transparent;
    PathLookup.Find("MainCanvas/Editor PopupMenus/LoadMenu/Mission Picker/SelectionPanel/HostingPanelTitle").GetComponent<Image>().color = UIThemeConfigs.Transparent;
    PathLookup.Find("MainCanvas/Editor PopupMenus/LoadMenu/Mission Picker/SelectionPanel/MissionSelectPanel/Filter Tags").GetComponent<Image>().color = UIThemeConfigs.Transparent;
    PathLookup.Find("MainCanvas/Editor PopupMenus/LoadMenu/Mission Picker/SelectionPanel/MissionSelectPanel/Background/Mission List/Viewport/Content/MissionSelectItem").GetComponent<Button>().colors = UIThemeConfigs.ButtonColor;
    PathLookup.Find("MainCanvas/Editor PopupMenus/LoadMenu/Mission Picker/SelectionPanel/MissionSelectPanel/Background/Mission List/Viewport/Content/MissionSelectItem (1)").GetComponent<Button>().colors = UIThemeConfigs.ButtonColor;
    PathLookup.Find("MainCanvas/Editor PopupMenus/LoadMenu/Mission Picker/SelectionPanel/MissionSelectPanel/Background/Mission List/Viewport/Content/MissionSelectItem (2)").GetComponent<Button>().colors = UIThemeConfigs.ButtonColor;
    PathLookup.Find("MainCanvas/Editor PopupMenus/LoadMenu/Mission Picker/SelectionPanel/MissionSelectPanel/Background/UserFolderButton").GetComponent<Button>().colors = UIThemeConfigs.ButtonColor;
    PathLookup.Find("MainCanvas/Editor PopupMenus/LoadMenu/Mission Picker/InfoPanel/Buttons/Confirm Button").GetComponent<Button>().colors = UIThemeConfigs.ButtonColor;
    PathLookup.Find("MainCanvas/Editor PopupMenus/LoadMenu/Mission Picker/MenuExit_Button").GetComponent<Button>().colors = UIThemeConfigs.ExitButtonColorBlock;
  }

  private static void ApplyGameplayUITheme()
  {
    MenuThemer._logger?.LogDebug((object) "Found gameplay UI");
    PathLookup.Find("SceneEssentials/Canvas/GameplayUICanvas/JoinMenu/mainOrganizer").GetComponent<VerticalLayoutGroup>().spacing = 10f;
    PathLookup.Find("SceneEssentials/Canvas/GameplayUICanvas/JoinMenu/mainOrganizer/JoinHeader/outline").GetComponent<Image>().color = UIThemeConfigs.Transparent;
    PathLookup.Find("SceneEssentials/Canvas/GameplayUICanvas/JoinMenu/mainOrganizer/JoinHeader").GetComponent<Image>().color = UIThemeConfigs.BackgroundMenu;
    PathLookup.Find("SceneEssentials/Canvas/GameplayUICanvas/JoinMenu/mainOrganizer/LobbyTitle/outline").GetComponent<Image>().color = UIThemeConfigs.Transparent;
    PathLookup.Find("SceneEssentials/Canvas/GameplayUICanvas/JoinMenu/mainOrganizer/LobbyTitle").GetComponent<Image>().color = UIThemeConfigs.BackgroundMenu;
    PathLookup.Find("SceneEssentials/Canvas/GameplayUICanvas/JoinMenu/mainOrganizer/ButtonsPanel/SpectateButton").GetComponent<Button>().colors = UIThemeConfigs.ButtonColor;
    PathLookup.Find("SceneEssentials/Canvas/GameplayUICanvas/JoinMenu/mainOrganizer/ButtonsPanel/QuitMission_Button").GetComponent<Button>().colors = UIThemeConfigs.ExitButtonColorBlock;
    PathLookup.Find("SceneEssentials/Canvas/GameplayUICanvas/JoinMenu/mainOrganizer/ButtonsPanel/SpectateButton").GetComponent<Image>().color = UIThemeConfigs.BackgroundMenuDark;
    PathLookup.Find("SceneEssentials/Canvas/GameplayUICanvas/JoinMenu/mainOrganizer/ButtonsPanel/QuitMission_Button").GetComponent<Image>().color = UIThemeConfigs.BackgroundMenuDark;
    PathLookup.Find("SceneEssentials/Canvas/GameplayUICanvas/JoinMenu/mainOrganizer/PlayerListOrganizer").GetComponent<Image>().color = UIThemeConfigs.Transparent;
    PathLookup.Find("SceneEssentials/Canvas/GameplayUICanvas/JoinMenu/mainOrganizer/PlayerListOrganizer/LeftPanel").GetComponent<Image>().color = UIThemeConfigs.BackgroundMenuDark;
    PathLookup.Find("SceneEssentials/Canvas/GameplayUICanvas/JoinMenu/mainOrganizer/PlayerListOrganizer/LeftPanel/FactionHeader").GetComponent<Image>().color = UIThemeConfigs.Transparent;
    PathLookup.Find("SceneEssentials/Canvas/GameplayUICanvas/JoinMenu/mainOrganizer/PlayerListOrganizer/RightPanel").GetComponent<Image>().color = UIThemeConfigs.BackgroundMenuDark;
    PathLookup.Find("SceneEssentials/Canvas/GameplayUICanvas/JoinMenu/mainOrganizer/PlayerListOrganizer/RightPanel/FactionHeader").GetComponent<Image>().color = UIThemeConfigs.Transparent;
    UnityEngine.Object.Destroy((UnityEngine.Object) PathLookup.Find("SceneEssentials/Canvas/GameplayUICanvas/JoinMenu/mainOrganizer/ButtonsPanel/SPACER"));
  }

  private static void ApplySingleplayerMenuTheme()
  {
    MenuThemer._logger?.LogDebug((object) "Found singleplayer menu");
    GameObject.Find("MainCanvas/background").GetComponent<Image>().color = UIThemeConfigs.BackgroundDarkened;
    GameObject.Find("MainCanvas/Header").GetComponent<Image>().color = UIThemeConfigs.Transparent;
    GameObject.Find("MainCanvas/Header/outline").GetComponent<Image>().color = UIThemeConfigs.Transparent;
    GameObject.Find("MainCanvas/Mission Picker/SelectionPanel").GetComponent<Image>().color = UIThemeConfigs.Transparent;
    GameObject.Find("MainCanvas/Mission Picker/InfoPanel").GetComponent<Image>().color = UIThemeConfigs.Transparent;
    GameObject.Find("MainCanvas/Mission Picker/SelectionPanel/MissionSelectPanel/Background").GetComponent<Image>().color = UIThemeConfigs.Transparent;
    GameObject.Find("MainCanvas/Mission Picker/SelectionPanel/HostingPanelTitle/HostGameTitle").GetComponent<Image>().color = UIThemeConfigs.Transparent;
    GameObject.Find("MainCanvas/Mission Picker/SelectionPanel/HostingPanelTitle").GetComponent<Image>().color = UIThemeConfigs.Transparent;
    GameObject.Find("MainCanvas/Mission Picker/SelectionPanel/MissionSelectPanel/Filter Tags").GetComponent<Image>().color = UIThemeConfigs.Transparent;
    GameObject.Find("MainCanvas/Mission Picker/SelectionPanel/MissionSelectPanel/Background/Mission List/Viewport/Content/MissionSelectItem").GetComponent<Button>().colors = UIThemeConfigs.ButtonColor;
    GameObject.Find("MainCanvas/Mission Picker/SelectionPanel/MissionSelectPanel/Background/Mission List/Viewport/Content/MissionSelectItem (1)").GetComponent<Button>().colors = UIThemeConfigs.ButtonColor;
    GameObject.Find("MainCanvas/Mission Picker/SelectionPanel/MissionSelectPanel/Background/Mission List/Viewport/Content/MissionSelectItem (2)").GetComponent<Button>().colors = UIThemeConfigs.ButtonColor;
    GameObject.Find("MainCanvas/Mission Picker/SelectionPanel/MissionSelectPanel/Background/UserFolderButton").GetComponent<Button>().colors = UIThemeConfigs.ButtonColor;
    GameObject.Find("MainCanvas/Mission Picker/InfoPanel/Buttons/Open Customize Button").GetComponent<Button>().colors = UIThemeConfigs.ButtonColor;
    GameObject.Find("MainCanvas/Mission Picker/InfoPanel/Buttons/Confirm Button").GetComponent<Button>().colors = UIThemeConfigs.ButtonColor;
    GameObject.Find("MainCanvas/Mission Picker/MenuExit_Button").GetComponent<Button>().colors = UIThemeConfigs.ExitButtonColorBlock;
    GameObject.Find("MainCanvas/Mission Picker/Mission Customize/Holder").GetComponent<Image>().color = UIThemeConfigs.BackgroundMenuDark;
    GameObject.Find("MainCanvas/Mission Picker/Mission Customize/Holder/Box").GetComponent<Image>().color = UIThemeConfigs.BackgroundMenuDark;
    GameObject.Find("MainCanvas/Mission Picker/Mission Customize/Holder/Box/TitleBar").GetComponent<Image>().color = UIThemeConfigs.BackgroundMenu;
    GameObject.Find("MainCanvas/Mission Picker/Mission Customize/Holder/Box/Footer").GetComponent<Image>().color = UIThemeConfigs.BackgroundMenu;
    GameObject.Find("MainCanvas/Mission Picker/Mission Customize/Holder/Box/Panels/MissionSettingsMenu").GetComponent<Image>().color = UIThemeConfigs.BackgroundMenu;
    GameObject.Find("MainCanvas/Mission Picker/Mission Customize/Holder/Box/Panels/EnvironmentPanel").GetComponent<Image>().color = UIThemeConfigs.BackgroundMenu;
    GameObject.Find("MainCanvas/Mission Picker/Mission Customize/Holder/Box/Panels/FactionsTab/FactionSettingsPanel").GetComponent<Image>().color = UIThemeConfigs.BackgroundMenu;
    GameObject.Find("MainCanvas/Mission Picker/Mission Customize/Holder/Box/Panels/FactionsTab/MiddelPanel").GetComponent<Image>().color = UIThemeConfigs.BackgroundMenu;
    GameObject.Find("MainCanvas/Mission Picker/Mission Customize/Holder/Box/Panels/FactionsTab/Hovertext/ButtonHoverText").GetComponent<Image>().color = UIThemeConfigs.BackgroundMenu;
    GameObject.Find("MainCanvas/Mission Picker/Mission Customize/Holder/Box/Panels/MissionSettingsMenu/NukePanel").GetComponent<Image>().color = UIThemeConfigs.Transparent;
    GameObject.Find("MainCanvas/Mission Picker/Mission Customize/Holder/Box/Panels/MissionSettingsMenu/WreckDespawnPanel").GetComponent<Image>().color = UIThemeConfigs.Transparent;
    GameObject.Find("MainCanvas/Mission Picker/Mission Customize/Holder/Box/Panels/EnvironmentPanel/TimeOfDayPanel/frame").GetComponent<Image>().color = UIThemeConfigs.Transparent;
    GameObject.Find("MainCanvas/Mission Picker/Mission Customize/Holder/Box/Panels/EnvironmentPanel/WeatherPanel/frame").GetComponent<Image>().color = UIThemeConfigs.Transparent;
    GameObject.Find("MainCanvas/Mission Picker/Mission Customize/Holder/Box/Panels/EnvironmentPanel/WindPanel/frame").GetComponent<Image>().color = UIThemeConfigs.Transparent;
    GameObject.Find("MainCanvas/Mission Picker/Mission Customize/Holder/Box/Panels/EnvironmentPanel/MoonPanel/frame").GetComponent<Image>().color = UIThemeConfigs.Transparent;
    GameObject.Find("MainCanvas/Mission Picker/Mission Customize/Holder/Box/Panels/FactionsTab/FactionSettingsPanel/FundsPanel").GetComponent<Image>().color = UIThemeConfigs.Transparent;
    GameObject.Find("MainCanvas/Mission Picker/Mission Customize/Holder/Box/Panels/FactionsTab/FactionSettingsPanel/NukesPanel").GetComponent<Image>().color = UIThemeConfigs.Transparent;
    GameObject.Find("MainCanvas/Mission Picker/Mission Customize/Holder/Box/Panels/FactionsTab/MiddelPanel/AirframesPanel").GetComponent<Image>().color = UIThemeConfigs.Transparent;
    Image component = GameObject.Find("MainCanvas/Mission Picker/InfoPanel/ThumbnailPanel/MissionImage").GetComponent<Image>();
    component.sprite = ((IEnumerable<Sprite>) UnityEngine.Resources.FindObjectsOfTypeAll<Sprite>()).FirstOrDefault<Sprite>((Func<Sprite, bool>) (sprite => sprite.name == "DefaultMissionImage"));
    component.color = new Color(1f, 1f, 1f, 1f);
  }

  private static void ApplyMultiplayerMenuTheme()
  {
    MenuThemer._logger?.LogDebug((object) "Found multiplayer menu");
    GameObject.Find("MainCanvas/BackgroundImage").GetComponent<Image>().color = UIThemeConfigs.BackgroundDarkened;
    GameObject.Find("MainCanvas/panel/LobbiesMenu/LobbiesBrowseTitle").GetComponent<Image>().color = UIThemeConfigs.Transparent;
    GameObject.Find("MainCanvas/panel/LobbiesMenu/LobbiesBrowseTitle/LobbiesBrowseTitleOutline").GetComponent<Image>().color = UIThemeConfigs.Transparent;
    GameObject.Find("MainCanvas/panel/LobbiesMenu/LobbiesBackground").GetComponent<Image>().color = UIThemeConfigs.Transparent;
    GameObject.Find("MainCanvas/panel/HostingPanel").GetComponent<Image>().color = UIThemeConfigs.Transparent;
    GameObject.Find("MainCanvas/panel/HostingPanel/PanelFill").GetComponent<Image>().color = UIThemeConfigs.Transparent;
    GameObject.Find("MainCanvas/panel/HostingPanel/HostingPanelTitle").GetComponent<Image>().color = UIThemeConfigs.Transparent;
    GameObject.Find("MainCanvas/panel/HostingPanel/HostingPanelTitle/HostGameTitle").GetComponent<Image>().color = UIThemeConfigs.Transparent;
    GameObject.Find("MainCanvas/panel/HostingPanel/Settings/GameTitlePanel/HostNameInput").GetComponent<Image>().color = UIThemeConfigs.Transparent;
    GameObject.Find("MainCanvas/panel/LobbiesMenu/LobbiesBackground/LobbiesScroll/Viewport/ViewportFill").GetComponent<Image>().color = UIThemeConfigs.BackgroundMenuAlt;
    GameObject.Find("MainCanvas/panel/HostingPanel/Settings/GameTitlePanel").GetComponent<Image>().color = UIThemeConfigs.BackgroundMenuAlt;
    GameObject.Find("MainCanvas/panel/HostingPanel/Settings/MissionSelectPanel").GetComponent<Image>().color = UIThemeConfigs.BackgroundMenuAlt;
    GameObject.Find("MainCanvas/panel/HostingPanel/Settings/MaxPlayers").GetComponent<Image>().color = UIThemeConfigs.BackgroundMenuAlt;
    PathLookup.Find("MainCanvas/panel/LobbiesMenu/LobbiesBrowseTitle/RefreshButton").GetComponent<Button>().colors = UIThemeConfigs.ButtonColor;
    PathLookup.Find("MainCanvas/panel/HostingPanel/HostButton").GetComponent<Button>().colors = UIThemeConfigs.ButtonColor;
    PathLookup.Find("MainCanvas/panel/HostingPanel/Settings/MissionSelectPanel/Open Mission Picker").GetComponent<Button>().colors = UIThemeConfigs.ButtonColor;
    PathLookup.Find("MainCanvas/Mission Picker/SelectionPanel/MissionSelectPanel/Background/Mission List/Viewport/Content/MissionSelectItem").GetComponent<Button>().colors = UIThemeConfigs.ButtonColor;
    PathLookup.Find("MainCanvas/Mission Picker/SelectionPanel/MissionSelectPanel/Background/Mission List/Viewport/Content/MissionSelectItem (1)").GetComponent<Button>().colors = UIThemeConfigs.ButtonColor;
    PathLookup.Find("MainCanvas/Mission Picker/SelectionPanel/MissionSelectPanel/Background/Mission List/Viewport/Content/MissionSelectItem (2)").GetComponent<Button>().colors = UIThemeConfigs.ButtonColor;
    PathLookup.Find("MainCanvas/Mission Picker/background").GetComponent<Image>().color = UIThemeConfigs.MissionPickerOverlay;
    PathLookup.Find("MainCanvas/Mission Picker/SelectionPanel").GetComponent<Image>().color = UIThemeConfigs.Transparent;
    PathLookup.Find("MainCanvas/Mission Picker/SelectionPanel/MissionSelectPanel/Background").GetComponent<Image>().color = UIThemeConfigs.Transparent;
    PathLookup.Find("MainCanvas/Mission Picker/SelectionPanel/HostingPanelTitle").GetComponent<Image>().color = UIThemeConfigs.Transparent;
    PathLookup.Find("MainCanvas/Mission Picker/SelectionPanel/HostingPanelTitle/HostGameTitle").GetComponent<Image>().color = UIThemeConfigs.Transparent;
    PathLookup.Find("MainCanvas/Mission Picker/InfoPanel").GetComponent<Image>().color = UIThemeConfigs.Transparent;
    PathLookup.Find("MainCanvas/Mission Picker/SelectionPanel/MissionSelectPanel/Filter Tags").GetComponent<Image>().color = UIThemeConfigs.Transparent;
    PathLookup.Find("MainCanvas/Mission Picker/SelectionPanel/PanelFill").GetComponent<Image>().color = UIThemeConfigs.BackgroundMenuAlt;
    PathLookup.Find("MainCanvas/Mission Picker/InfoPanel/PanelFill").GetComponent<Image>().color = UIThemeConfigs.BackgroundMenuAlt;
    PathLookup.Find("MainCanvas/Mission Picker/InfoPanel/Buttons/Confirm Button").GetComponent<Button>().colors = UIThemeConfigs.ButtonColor;
    PathLookup.Find("MainCanvas/Mission Picker/InfoPanel/Buttons/Open Customize Button").GetComponent<Button>().colors = UIThemeConfigs.ButtonColor;
    PathLookup.Find("MainCanvas/Mission Picker/MenuExit_Button").GetComponent<Button>().colors = UIThemeConfigs.ExitButtonColorBlock;
    PathLookup.Find("MainCanvas/Lobby Panel/Right/FilterSection/Filter group/h3").GetComponent<TextMeshProUGUI>().color = Color.white;
    PathLookup.Find("MainCanvas/Lobby Panel/Right/FilterSection/MissionTypeGroup/h3").GetComponent<TextMeshProUGUI>().color = Color.white;
    PathLookup.Find("MainCanvas/Lobby Panel/Right/FilterSection/ServerTypeGroup/h3").GetComponent<TextMeshProUGUI>().color = Color.white;
    PathLookup.Find("MainCanvas/Lobby Panel/Right/FilterSection/DistanceGroup/h3").GetComponent<TextMeshProUGUI>().color = Color.white;
  }
}

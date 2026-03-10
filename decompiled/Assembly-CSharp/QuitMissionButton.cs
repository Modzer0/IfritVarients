// Decompiled with JetBrains decompiler
// Type: QuitMissionButton
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Cysharp.Threading.Tasks;
using JamesFrowen.ScriptableVariables.UI;
using NuclearOption.MissionEditorScripts;
using NuclearOption.Networking;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

#nullable disable
public class QuitMissionButton : ButtonController
{
  [SerializeField]
  private TextMeshProUGUI buttonText;
  [SerializeField]
  private string normalText;
  [SerializeField]
  private string returnToEditorText;
  [SerializeField]
  private bool confirmSafetyIfOtherPlayers;
  [SerializeField]
  private Button confirmLeaveButton;
  [SerializeField]
  private GameObject confirmLeavePanel;
  [SerializeField]
  private TextMeshProUGUI leaveWarning;
  private bool quitting;

  protected override void Awake()
  {
    base.Awake();
    if (!this.confirmSafetyIfOtherPlayers)
      return;
    this.confirmLeaveButton.onClick.AddListener(new UnityAction(((ButtonController) this).onClick));
  }

  private void OnEnable()
  {
    this.buttonText.text = GameManager.IsPlayingFromEditor ? this.returnToEditorText : this.normalText;
  }

  protected override void onClick()
  {
    if (this.quitting)
      return;
    if (GameManager.IsPlayingFromEditor)
    {
      this.quitting = true;
      MissionEditor.ReturnToEditor().Forget();
    }
    else if (this.confirmSafetyIfOtherPlayers)
      this.TryQuitGame();
    else
      this.QuitGame();
  }

  public static bool HasOtherPlayers(out int numberOfPlayers)
  {
    numberOfPlayers = UnitRegistry.playerLookup.Count;
    return NetworkManagerNuclearOption.i.Server.Active && GameManager.gameState == GameState.Multiplayer && numberOfPlayers > 1;
  }

  public void TryQuitGame()
  {
    int numberOfPlayers;
    if (QuitMissionButton.HasOtherPlayers(out numberOfPlayers))
    {
      if (!this.confirmLeavePanel.activeSelf)
      {
        this.confirmLeavePanel.SetActive(true);
        this.leaveWarning.text = $"Are you sure you want to end the game? Doing so will kick {numberOfPlayers - 1} other players.";
      }
      else
        this.HostQuit();
    }
    else
      this.QuitGame();
  }

  private void HostQuit()
  {
    this.quitting = true;
    Player localPlayer;
    string str = !NetworkManagerNuclearOption.i.Client.Active || !GameManager.GetLocalPlayer<Player>(out localPlayer) ? "server" : localPlayer.PlayerName;
    NetworkManagerNuclearOption.i.Server.SendToAll<HostEndedMessage>(new HostEndedMessage()
    {
      HostName = str
    }, false, true);
    UniTask.Void((Func<UniTaskVoid>) (async () =>
    {
      await UniTask.Delay(100);
      this.QuitGame();
    }));
  }

  private void QuitGame()
  {
    this.quitting = true;
    if ((UnityEngine.Object) SceneSingleton<GameplayUI>.i != (UnityEngine.Object) null)
      SceneSingleton<GameplayUI>.i.ResumeGame();
    NetworkManagerNuclearOption.i.Stop(true);
  }
}

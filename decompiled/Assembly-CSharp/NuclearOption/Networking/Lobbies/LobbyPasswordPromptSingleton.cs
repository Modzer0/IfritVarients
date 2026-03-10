// Decompiled with JetBrains decompiler
// Type: NuclearOption.Networking.Lobbies.LobbyPasswordPromptSingleton
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

#nullable disable
namespace NuclearOption.Networking.Lobbies;

public class LobbyPasswordPromptSingleton : MonoBehaviour
{
  private static LobbyPasswordPromptSingleton instance;
  [SerializeField]
  private GameObject panel;
  [SerializeField]
  private TMP_InputField inputField;
  [SerializeField]
  private Button submitButton;
  [SerializeField]
  private Button cancelButton;
  private LobbyInstance lobby;

  private void Awake()
  {
    if ((Object) LobbyPasswordPromptSingleton.instance == (Object) null)
    {
      LobbyPasswordPromptSingleton.instance = this;
      this.submitButton.onClick.AddListener(new UnityAction(this.OnSubmit));
      this.inputField.onSubmit.AddListener((UnityAction<string>) (_ => this.OnSubmit()));
      this.inputField.onValueChanged.AddListener((UnityAction<string>) (txt => this.submitButton.interactable = !string.IsNullOrEmpty(txt)));
      this.cancelButton.onClick.AddListener(new UnityAction(this.OnCancel));
    }
    else
      Debug.LogError((object) "2 LobbyPasswordPrompt existed at once");
  }

  public static void ShowPrompt(LobbyInstance lobby)
  {
    if ((Object) LobbyPasswordPromptSingleton.instance == (Object) null)
      Object.Instantiate<GameObject>(GameAssets.i.lobbyPasswordPrompt);
    LobbyPasswordPromptSingleton.instance.ShowPromptInternal(lobby);
  }

  private void ShowPromptInternal(LobbyInstance lobby)
  {
    this.panel.SetActive(true);
    this.inputField.text = "";
    this.lobby = lobby;
  }

  private void OnSubmit()
  {
    string text = this.inputField.text;
    if (string.IsNullOrEmpty(text))
    {
      Debug.LogWarning((object) "ignoring password submit because it was empty");
    }
    else
    {
      SteamLobby.instance.TryJoinLobby(this.lobby, text, false);
      this.panel.SetActive(false);
    }
  }

  private void OnCancel() => this.panel.SetActive(false);
}

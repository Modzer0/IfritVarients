// Decompiled with JetBrains decompiler
// Type: ChatBox
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Cysharp.Threading.Tasks;
using NuclearOption.Chat;
using Rewired;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

#nullable disable
public class ChatBox : MonoBehaviour
{
  [SerializeField]
  private TMP_InputField input;
  [SerializeField]
  private UnityEngine.UI.Button sendButton;
  [SerializeField]
  private Toggle alliesOnlyToggle;
  private bool sendEnabled;
  private bool allyToggleEnabled;
  private Player player;

  public static bool ChatAllowed
  {
    get => GameManager.gameState == GameState.Multiplayer && PlayerSettings.chatEnabled;
  }

  private void Awake()
  {
    this.sendButton.onClick.AddListener(new UnityAction(this.SendChat));
    this.input.SetTextWithoutNotify(string.Empty);
    this.alliesOnlyToggle.isOn = false;
    this.EnableToggle(true);
    this.player = ReInput.players.GetPlayer(0);
  }

  private void OnEnable()
  {
    if (!ChatBox.ChatAllowed)
    {
      this.gameObject.SetActive(false);
    }
    else
    {
      CursorManager.SetFlag(CursorFlags.Chat, true);
      foreach (ControllerMap allMap in this.player.controllers.maps.GetAllMaps(ControllerType.Keyboard))
        allMap.enabled = false;
      foreach (ControllerMap allMap in this.player.controllers.maps.GetAllMaps(ControllerType.Mouse))
        allMap.enabled = false;
      GameplayUI.AllowPauseKeybind = false;
      this.EnableSend(false);
    }
  }

  public void OnDisable()
  {
    CursorManager.SetFlag(CursorFlags.Chat, false);
    this.WaitToReEnableKeyboard().Forget();
  }

  private async UniTaskVoid WaitToReEnableKeyboard()
  {
    await UniTask.Delay(100);
    foreach (ControllerMap allMap in this.player.controllers.maps.GetAllMaps(ControllerType.Keyboard))
      allMap.enabled = true;
    foreach (ControllerMap allMap in this.player.controllers.maps.GetAllMaps(ControllerType.Mouse))
      allMap.enabled = true;
    GameplayUI.AllowPauseKeybind = true;
  }

  public void SendChat()
  {
    if (!ChatManager.CanSend(this.input.text, false, false))
      return;
    ChatManager.SendChatMessage(this.input.text, !this.alliesOnlyToggle.isOn);
    this.CloseChat();
  }

  private void CloseChat()
  {
    this.gameObject.SetActive(false);
    this.input.SetTextWithoutNotify(string.Empty);
  }

  private void Update()
  {
    if (this.player.GetButtonDown("Cancel Chat") || Input.GetKeyDown(KeyCode.Escape))
    {
      this.CloseChat();
    }
    else
    {
      bool canSend = ChatManager.CanSend(this.input.text, false, false);
      if (canSend != this.sendEnabled)
        this.EnableSend(canSend);
      bool localFaction = GameManager.GetLocalFaction(out Faction _);
      if (localFaction != this.allyToggleEnabled)
        this.EnableToggle(localFaction);
      if (canSend && this.player.GetButtonDown("Submit Chat") || Input.GetKeyDown(KeyCode.Return))
        this.SendChat();
      this.input.ActivateInputField();
    }
  }

  private void EnableSend(bool canSend)
  {
    this.sendButton.interactable = canSend;
    this.sendEnabled = canSend;
  }

  private void EnableToggle(bool hasFaction)
  {
    this.alliesOnlyToggle.gameObject.SetActive(hasFaction);
    this.allyToggleEnabled = hasFaction;
  }
}

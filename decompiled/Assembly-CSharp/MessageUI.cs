// Decompiled with JetBrains decompiler
// Type: MessageUI
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Cysharp.Threading.Tasks;
using NuclearOption.MissionEditorScripts;
using Rewired;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

#nullable disable
[Serializable]
public class MessageUI : SceneSingleton<MessageUI>
{
  [SerializeField]
  private int maxLines = 10;
  [SerializeField]
  private float messageRemoveDelayBase = 8f;
  [SerializeField]
  private float killFeedRemoveDelayBase = 5f;
  [SerializeField]
  private float removeDelayPerCharacter = 0.1f;
  [SerializeField]
  private TextMeshProUGUI messageText;
  [SerializeField]
  private TextMeshProUGUI killFeedText;
  [SerializeField]
  private GameObject messageBackground;
  [SerializeField]
  private ContentSizeFitter contentSizeFitter;
  [Header("Chat")]
  [SerializeField]
  private ChatBox chat;
  private bool hasMessage;
  private bool hasKillFeed;
  private bool boxEnabled;
  private Player player;
  private readonly Queue<MessageUI.Message> messages = new Queue<MessageUI.Message>();
  private readonly Queue<MessageUI.Message> killFeed = new Queue<MessageUI.Message>();

  public static void SetFixedBoxSize()
  {
    SceneSingleton<MessageUI>.i.contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
    (SceneSingleton<MessageUI>.i.messageBackground.GetComponent(typeof (RectTransform)) as RectTransform).sizeDelta = new Vector2(0.0f, 100f);
  }

  public static void SetDynamicBoxSize()
  {
    if (!((UnityEngine.Object) SceneSingleton<MessageUI>.i != (UnityEngine.Object) null))
      return;
    SceneSingleton<MessageUI>.i.contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
    LayoutRebuilder.ForceRebuildLayoutImmediate(SceneSingleton<MessageUI>.i.transform as RectTransform);
  }

  protected override void Awake()
  {
    base.Awake();
    this.messageText.text = string.Empty;
    this.killFeedText.text = string.Empty;
    this.boxEnabled = false;
    this.messageBackground.SetActive(false);
    this.chat.gameObject.SetActive(false);
    this.player = ReInput.players.GetPlayer(0);
    GameManager.OnGameStateChanged.AddListener(new Action(this.OnGameStateChanged));
  }

  private void OnDestroy()
  {
    GameManager.OnGameStateChanged.RemoveListener(new Action(this.OnGameStateChanged));
  }

  private void OnGameStateChanged()
  {
    if (!((UnityEngine.Object) this.gameObject != (UnityEngine.Object) null))
      return;
    this.gameObject.SetActive(GameManager.gameState.IsSingleOrMultiplayer());
  }

  public void GameMessage(string message)
  {
    string[] strArray = message.Split('\n', StringSplitOptions.None);
    float removeTime = (float) ((double) Time.timeSinceLevelLoad + (double) this.messageRemoveDelayBase + (double) message.Length * (double) this.removeDelayPerCharacter);
    for (int index = 0; index < strArray.Length; ++index)
      this.messages.Enqueue(new MessageUI.Message(strArray[index], removeTime));
  }

  public void KillFeed(string message)
  {
    if (PlayerSettings.killFeedNbLines == 0)
      return;
    string[] strArray = message.Split('\n', StringSplitOptions.None);
    float removeTime = (float) ((double) Time.timeSinceLevelLoad + (double) this.killFeedRemoveDelayBase + (double) message.Length * (double) this.removeDelayPerCharacter);
    for (int index = 0; index < strArray.Length; ++index)
      this.killFeed.Enqueue(new MessageUI.Message(strArray[index], removeTime));
  }

  public async UniTask DelayedGameMessage(string message, float secondsBetweenLine)
  {
    string[] lines = message.Split('\n', StringSplitOptions.None);
    for (int i = 0; i < lines.Length; ++i)
    {
      await UniTask.Delay((int) ((double) secondsBetweenLine * 1000.0));
      float removeTime = (float) ((double) Time.timeSinceLevelLoad + (double) this.messageRemoveDelayBase + (double) message.Length * (double) this.removeDelayPerCharacter);
      this.messages.Enqueue(new MessageUI.Message(lines[i], removeTime));
    }
    lines = (string[]) null;
  }

  private void LateUpdate()
  {
    this.CheckBoxEnabled();
    this.CheckChatBox();
    this.UpdateMessages();
    this.UpdateKillFeed();
  }

  private void UpdateMessages()
  {
    while (this.messages.Count > this.maxLines)
      this.messages.Dequeue();
    float timeSinceLevelLoad = Time.timeSinceLevelLoad;
    while (this.messages.Count > 0 && (double) this.messages.Peek().RemoveTime < (double) timeSinceLevelLoad)
      this.messages.Dequeue();
    if (this.messages.Count > 0)
    {
      this.messageText.text = string.Join<MessageUI.Message>("\n", (IEnumerable<MessageUI.Message>) this.messages);
      this.hasMessage = true;
    }
    else
    {
      if (this.hasMessage)
        this.messageText.text = string.Empty;
      this.hasMessage = false;
    }
  }

  private void UpdateKillFeed()
  {
    while (this.killFeed.Count > 5 * PlayerSettings.killFeedNbLines)
      this.killFeed.Dequeue();
    float timeSinceLevelLoad = Time.timeSinceLevelLoad;
    while (this.killFeed.Count > 0 && (double) this.killFeed.Peek().RemoveTime < (double) timeSinceLevelLoad)
      this.killFeed.Dequeue();
    if (this.killFeed.Count > 0)
    {
      this.killFeedText.text = string.Join<MessageUI.Message>("\n", (IEnumerable<MessageUI.Message>) this.killFeed);
      this.hasKillFeed = true;
    }
    else
    {
      if (this.hasKillFeed)
        this.killFeedText.text = string.Empty;
      this.hasKillFeed = false;
    }
  }

  private void CheckBoxEnabled()
  {
    if (this.boxEnabled && this.ShouldHide())
    {
      this.boxEnabled = false;
      this.messageBackground.SetActive(false);
      this.chat.gameObject.SetActive(false);
    }
    else
    {
      if (this.boxEnabled || this.ShouldHide())
        return;
      this.boxEnabled = true;
      this.messageBackground.SetActive(true);
    }
  }

  private bool ShouldHide()
  {
    if (PlayerSettings.cinematicMode)
      return true;
    return this.messages.Count == 0 && this.killFeed.Count == 0 && !this.chat.gameObject.activeSelf;
  }

  private void CheckChatBox()
  {
    if (!ChatBox.ChatAllowed || PlayerSettings.cinematicMode || InputFieldChecker.InsideInputField || !this.player.GetButtonDown("Open Chat"))
      return;
    this.chat.gameObject.SetActive(true);
  }

  private readonly struct Message(string line, float removeTime)
  {
    public readonly string Line = line;
    public readonly float RemoveTime = removeTime;

    public override string ToString() => this.Line;
  }
}

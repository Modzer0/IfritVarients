// Decompiled with JetBrains decompiler
// Type: LeaderboardPlayerEntry
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.Chat;
using NuclearOption.Networking;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

#nullable disable
public class LeaderboardPlayerEntry : 
  MonoBehaviour,
  IPointerEnterHandler,
  IEventSystemHandler,
  IPointerExitHandler
{
  private static readonly string mutedText = "<size=80%><color=#AAA>(M)</color></size>";
  private static readonly string blockedText = "<size=80%><color=#FAA>(B)</color></size>";
  [Header("Player Stats")]
  [SerializeField]
  private Text textName;
  [SerializeField]
  private Text textRank;
  [SerializeField]
  private Text textScore;
  [Header("Kick Button")]
  [SerializeField]
  private Button kickButton;
  [Header("Mute Button")]
  [SerializeField]
  private Button muteButton;
  [SerializeField]
  private TextMeshProUGUI muteButtonText;
  [SerializeField]
  private string muteString = "Mute";
  [SerializeField]
  private string unmuteString = "Unmute";
  [Header("Block Button")]
  [SerializeField]
  private Button blockButton;
  [SerializeField]
  private TextMeshProUGUI blockButtonText;
  [SerializeField]
  private string blockString = "Block";
  [SerializeField]
  private string unblockString = "UnbLock";
  private bool muted;
  private bool blocked;

  public Player Player { get; private set; }

  private void Awake()
  {
    this.ShowButtons(false, false);
    this.kickButton.onClick.AddListener(new UnityAction(this.KickPressed));
    this.muteButton.onClick.AddListener(new UnityAction(this.MutePressed));
    this.blockButton.onClick.AddListener(new UnityAction(this.BlockPressed));
  }

  public void Setup(Player player)
  {
    this.Player = player;
    this.muted = ChatManager.IsMuted(player);
    this.SetMuteText();
    this.blocked = BlockList.IsBlocked(player);
    this.SetBlockText();
    this.UpdateScore();
  }

  private void SetMuteText()
  {
    this.muteButtonText.text = this.muted ? this.unmuteString : this.muteString;
  }

  private void SetBlockText()
  {
    this.blockButtonText.text = this.blocked ? this.unblockString : this.blockString;
  }

  public void UpdateScore()
  {
    if ((Object) this.Player == (Object) null)
      return;
    this.textName.text = $"{(!this.blocked ? (!this.muted ? string.Empty : LeaderboardPlayerEntry.mutedText) : LeaderboardPlayerEntry.blockedText)} {this.Player.GetNameOrCensored()}";
    this.textRank.text = $"{this.Player.PlayerRank:F0}";
    this.textScore.text = $"{this.Player.PlayerScore:F1}";
  }

  private void ShowButtons(bool showKick, bool showMute)
  {
    this.kickButton.gameObject.SetActive(showKick);
    this.muteButton.gameObject.SetActive(showMute);
    this.blockButton.gameObject.SetActive(showMute);
  }

  public void OnPointerEnter(PointerEventData eventData)
  {
    if (GameManager.gameState != GameState.Multiplayer)
      return;
    bool flag = GameManager.IsLocalPlayer<Player>(this.Player);
    bool showMute = !flag;
    this.ShowButtons(NetworkManagerNuclearOption.i.Server.Active && !flag, showMute);
  }

  public void OnPointerExit(PointerEventData eventData) => this.ShowButtons(false, false);

  private void KickPressed() => NetworkManagerNuclearOption.i.KickPlayerAsync(this.Player).Forget();

  private void MutePressed()
  {
    this.muted = ChatManager.ToggleMute(this.Player);
    this.SetMuteText();
    this.UpdateScore();
  }

  private void BlockPressed()
  {
    this.blocked = BlockList.ToggleBlock(this.Player);
    this.SetBlockText();
    this.UpdateScore();
  }
}

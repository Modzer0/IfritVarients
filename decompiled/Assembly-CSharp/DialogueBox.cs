// Decompiled with JetBrains decompiler
// Type: DialogueBox
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

#nullable disable
public class DialogueBox : MonoBehaviour
{
  [SerializeField]
  private GameObject holder;
  [SerializeField]
  private TextMeshProUGUI titleText;
  [SerializeField]
  private TextMeshProUGUI bodyText;
  [SerializeField]
  private TextMeshProUGUI buttonText;
  [SerializeField]
  private Button button;

  public event Action<string> ButtonPressed;

  public string Title => this.titleText.text;

  private void Awake() => this.button.onClick.AddListener(new UnityAction(this.InvokeButtonPress));

  public void InvokeButtonPress() => this.ButtonPressed(this.titleText.text);

  public void Show(string title, string body, string button)
  {
    if (string.IsNullOrEmpty(title))
      title = "Dialogue";
    this.titleText.text = title;
    this.bodyText.text = body;
    this.buttonText.text = button;
    this.EnableBox(true);
  }

  public void Hide()
  {
    this.titleText.text = "";
    this.EnableBox(false);
  }

  private void EnableBox(bool show)
  {
    this.holder.SetActive(show);
    GameplayUI.AllowPauseKeybind = !show;
    GameManager.flightControlsEnabled = !show;
    CursorManager.SetFlag(CursorFlags.Dialogue, show);
    if (GameManager.gameState != GameState.SinglePlayer)
      return;
    Time.timeScale = show ? 0.0f : (GameplayUI.GameSlowMotion ? 0.05f : 1f);
    AudioListener.pause = show;
  }

  public void SetServerTitle(string title) => this.titleText.text = title;
}

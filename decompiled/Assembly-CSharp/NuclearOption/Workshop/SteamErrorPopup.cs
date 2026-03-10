// Decompiled with JetBrains decompiler
// Type: NuclearOption.Workshop.SteamErrorPopup
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

#nullable disable
namespace NuclearOption.Workshop;

public class SteamErrorPopup : MonoBehaviour
{
  [SerializeField]
  private GameObject holder;
  [SerializeField]
  private TextMeshProUGUI titleText;
  [SerializeField]
  private TextMeshProUGUI descriptionText;
  [SerializeField]
  private TextMeshProUGUI buttonText;
  [SerializeField]
  private Button actionButton;
  private Action callback;

  private void Awake()
  {
    this.actionButton.onClick.AddListener(new UnityAction(this.OnActionButtonClicked));
  }

  public void Show(string title, string description, string buttonLabel, Action callback)
  {
    this.holder.SetActive(true);
    this.titleText.text = title;
    this.descriptionText.text = description;
    this.buttonText.text = buttonLabel;
    this.callback = callback;
  }

  private void OnActionButtonClicked()
  {
    Action callback = this.callback;
    if (callback != null)
      callback();
    this.callback = (Action) null;
    this.holder.SetActive(false);
  }
}

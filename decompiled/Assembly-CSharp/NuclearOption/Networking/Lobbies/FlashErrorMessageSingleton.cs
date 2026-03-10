// Decompiled with JetBrains decompiler
// Type: NuclearOption.Networking.Lobbies.FlashErrorMessageSingleton
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

#nullable disable
namespace NuclearOption.Networking.Lobbies;

public class FlashErrorMessageSingleton : MonoBehaviour
{
  private static FlashErrorMessageSingleton instance;
  [SerializeField]
  private Canvas _canvas;
  [SerializeField]
  private Transform _parent;
  [SerializeField]
  private FlashErrorMessageModal prefab;
  private Stack<FlashErrorMessageModal> pool = new Stack<FlashErrorMessageModal>();
  private Queue<FlashErrorMessageModal> active = new Queue<FlashErrorMessageModal>();
  [SerializeField]
  private int maxMessages = 10;
  [SerializeField]
  private float defaultHideTime = 5f;

  private void Awake()
  {
    if ((Object) FlashErrorMessageSingleton.instance == (Object) null)
    {
      FlashErrorMessageSingleton.instance = this;
      Object.DontDestroyOnLoad((Object) this.gameObject);
    }
    else
      Debug.LogError((object) "2 FlashErrorMessageSingleton existed at once");
  }

  public static void ShowError(string message, float? hideSeconds = null)
  {
    if ((Object) FlashErrorMessageSingleton.instance == (Object) null)
      Object.Instantiate<GameObject>(GameAssets.i.flashErrorMessage);
    if (!((Object) FlashErrorMessageSingleton.instance != (Object) null))
      return;
    FlashErrorMessageSingleton.instance.ShowErrorInternal(message, hideSeconds);
  }

  private void ShowErrorInternal(string message, float? hideSeconds = null)
  {
    FlashErrorMessageModal modal;
    if (this.active.Count == this.maxMessages)
      modal = this.active.Dequeue();
    else if (this.pool.Count > 0)
    {
      modal = this.pool.Pop();
    }
    else
    {
      modal = Object.Instantiate<FlashErrorMessageModal>(this.prefab, this._parent);
      modal.cancelButton.onClick.AddListener((UnityAction) (() => this.Hide(modal)));
    }
    modal.HideTime = Time.timeSinceLevelLoad + (float) ((double) hideSeconds ?? (double) this.defaultHideTime);
    modal.message.text = message;
    modal.panel.SetActive(true);
    modal.transform.SetAsLastSibling();
    this.active.Enqueue(modal);
    if (this.enabled)
      return;
    this.enabled = true;
    this._canvas.gameObject.SetActive(true);
  }

  private void Hide(FlashErrorMessageModal modal)
  {
    this.pool.Push(modal);
    modal.panel.SetActive(false);
  }

  private void Update()
  {
    float timeSinceLevelLoad = Time.timeSinceLevelLoad;
    FlashErrorMessageModal modal;
    while (this.active.TryPeek(ref modal) && (double) timeSinceLevelLoad > (double) modal.HideTime)
    {
      this.active.Dequeue();
      this.Hide(modal);
    }
    if (this.active.Count != 0)
      return;
    this.enabled = false;
    this._canvas.gameObject.SetActive(false);
  }
}

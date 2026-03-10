// Decompiled with JetBrains decompiler
// Type: NuclearOption.Networking.Lobbies.ModalBackground
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using UnityEngine;
using UnityEngine.EventSystems;

#nullable disable
namespace NuclearOption.Networking.Lobbies;

public class ModalBackground : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
{
  [SerializeField]
  private GameObject modal;

  public void OnPointerClick(PointerEventData eventData) => this.modal.gameObject.SetActive(false);
}

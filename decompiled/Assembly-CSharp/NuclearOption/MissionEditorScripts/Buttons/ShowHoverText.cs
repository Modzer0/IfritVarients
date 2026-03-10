// Decompiled with JetBrains decompiler
// Type: NuclearOption.MissionEditorScripts.Buttons.ShowHoverText
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using UnityEngine;
using UnityEngine.EventSystems;

#nullable disable
namespace NuclearOption.MissionEditorScripts.Buttons;

public class ShowHoverText : 
  MonoBehaviour,
  IPointerEnterHandler,
  IEventSystemHandler,
  IPointerExitHandler,
  IPointerMoveHandler
{
  [SerializeField]
  private string showText;
  [SerializeField]
  private HoverText hover;

  public void SetText(string text) => this.showText = text;

  public void SetHover(HoverText hoverText) => this.hover = hoverText;

  public void OnPointerMove(PointerEventData eventData)
  {
    if (!((Object) this.hover != (Object) null))
      return;
    this.hover.Move((object) this, eventData.position);
  }

  public void OnPointerEnter(PointerEventData eventData)
  {
    this.hover.Refresh(this.showText);
    this.hover.Show((object) this, this.showText);
  }

  public void OnPointerExit(PointerEventData eventData) => this.hover.Hide((object) this);
}

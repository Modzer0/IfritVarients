// Decompiled with JetBrains decompiler
// Type: MapIcon
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

#nullable disable
public abstract class MapIcon : 
  MonoBehaviour,
  IPointerEnterHandler,
  IEventSystemHandler,
  IPointerExitHandler,
  IPointerClickHandler
{
  public Image iconImage;
  protected Vector3 globalPosition;
  protected bool isSelected;

  protected abstract FactionHQ GetHQ();

  protected abstract Color GetColor();

  protected abstract bool IsLocalPlayerAircraft();

  public abstract void ClickIcon(MapIcon.ClickSource clickSource);

  protected abstract void OnSelectIcon();

  protected abstract void OnDeselectIcon();

  protected abstract void OnRemoveIcon();

  public abstract string GetInfoText();

  public abstract void UpdateIcon(
    float mapDisplayFactor,
    float mapInverseScale,
    Transform mapTransform,
    bool mapMaximized);

  public void UpdateColor()
  {
    this.iconImage.color = this.GetColor();
    if (!this.IsLocalPlayerAircraft())
      return;
    this.HighlightIcon();
  }

  public void HighlightIcon()
  {
    Debug.Log((object) "Highlighting Player's aircraft icon");
    this.iconImage.color = Color.white;
    this.iconImage.raycastTarget = false;
  }

  public void SelectIcon()
  {
    this.isSelected = true;
    this.iconImage.raycastTarget = false;
    this.UpdateColor();
    this.OnSelectIcon();
  }

  public void DeselectIcon()
  {
    this.isSelected = false;
    this.iconImage.raycastTarget = true;
    this.UpdateColor();
    this.OnDeselectIcon();
  }

  public void RemoveIcon()
  {
    this.OnRemoveIcon();
    if (!((Object) this != (Object) null))
      return;
    Object.Destroy((Object) this.gameObject);
  }

  void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
  {
    if (eventData.button != PointerEventData.InputButton.Left)
      return;
    this.ClickIcon(MapIcon.ClickSource.Mouse);
  }

  void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
  {
    SceneSingleton<DynamicMap>.i.DisplayTooltip(this);
  }

  void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
  {
    SceneSingleton<DynamicMap>.i.HideTooltip();
  }

  public enum ClickSource
  {
    Mouse,
    Controller,
  }
}

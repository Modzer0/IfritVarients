// Decompiled with JetBrains decompiler
// Type: MapOptions_ToggleButton
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

#nullable disable
public class MapOptions_ToggleButton : 
  MonoBehaviour,
  IPointerEnterHandler,
  IEventSystemHandler,
  IPointerClickHandler,
  IPointerExitHandler
{
  public MapOptions_ToggleButton.CallBackEvent OnToggleMethod;
  [SerializeField]
  private MonoBehaviour script;
  [SerializeField]
  private Image image;
  [SerializeField]
  private Text label;
  [SerializeField]
  private string variableName;
  public bool isActive = true;
  public bool status;
  public bool onFocus;
  [SerializeField]
  private Color colorIsOn = Color.green;
  [SerializeField]
  private Color colorIsOff = Color.grey;
  [SerializeField]
  private List<MapOptions_ToggleButton> otherButtons = new List<MapOptions_ToggleButton>();

  private void Start()
  {
    if ((UnityEngine.Object) this.script != (UnityEngine.Object) null && this.variableName != "")
      this.status = (bool) this.script.GetType().GetField(this.variableName).GetValue((object) this.script);
    this.Set(this.status);
  }

  public void Toggle()
  {
    if (this.OnToggleMethod != null)
      this.OnToggleMethod.Invoke();
    if (this.otherButtons.Count > 0)
    {
      this.Set(true);
      foreach (MapOptions_ToggleButton otherButton in this.otherButtons)
        otherButton.Set(false);
    }
    else if (this.status)
      this.Set(false);
    else
      this.Set(true);
  }

  public void Set(bool arg)
  {
    this.status = arg;
    this.SetColor();
  }

  public void OnPointerEnter(PointerEventData eventData) => this.onFocus = true;

  public void SetColor()
  {
    if ((UnityEngine.Object) this.image != (UnityEngine.Object) null)
      this.image.color = this.status ? this.colorIsOn : this.colorIsOff;
    if (!((UnityEngine.Object) this.label != (UnityEngine.Object) null))
      return;
    this.label.color = this.status ? this.colorIsOn : this.colorIsOff;
  }

  public void OnPointerClick(PointerEventData eventData)
  {
    if (!this.onFocus || eventData.button != PointerEventData.InputButton.Left)
      return;
    this.Toggle();
  }

  public void OnPointerExit(PointerEventData eventData) => this.onFocus = false;

  [Serializable]
  public class CallBackEvent : UnityEvent
  {
  }
}

// Decompiled with JetBrains decompiler
// Type: NuclearOption.UI.BaseToggle
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

#nullable disable
namespace NuclearOption.UI;

public abstract class BaseToggle : 
  Selectable,
  IPointerClickHandler,
  IEventSystemHandler,
  ISubmitHandler,
  ICanvasElement
{
  public Graphic graphic;
  public BaseToggle.ToggleEvent onValueChanged = new BaseToggle.ToggleEvent();
  [Tooltip("Is the toggle currently on or off?")]
  [SerializeField]
  protected bool m_IsOn;

  public bool isOn
  {
    get => this.m_IsOn;
    set => this.Set(value);
  }

  public virtual void Rebuild(CanvasUpdate executing)
  {
    if (executing != CanvasUpdate.Prelayout)
      return;
    this.onValueChanged.Invoke(this.m_IsOn);
  }

  public virtual void LayoutComplete()
  {
  }

  public virtual void GraphicUpdateComplete()
  {
  }

  protected override void OnEnable()
  {
    base.OnEnable();
    this.PlayEffect(true);
  }

  protected override void OnDidApplyAnimationProperties()
  {
    if ((UnityEngine.Object) this.graphic != (UnityEngine.Object) null)
    {
      bool flag = !Mathf.Approximately(this.graphic.canvasRenderer.GetColor().a, 0.0f);
      if (this.m_IsOn != flag)
      {
        this.m_IsOn = flag;
        this.Set(!flag);
      }
    }
    base.OnDidApplyAnimationProperties();
  }

  public void SetIsOnWithoutNotify(bool value) => this.Set(value, false);

  private void Set(bool value, bool sendCallback = true)
  {
    if (this.m_IsOn == value)
      return;
    this.m_IsOn = value;
    this.PlayEffect(false);
    if (!sendCallback)
      return;
    UISystemProfilerApi.AddMarker("SliderToggle.value", (UnityEngine.Object) this);
    this.onValueChanged.Invoke(this.m_IsOn);
  }

  protected abstract void PlayEffect(bool instant);

  protected override void Start() => this.PlayEffect(true);

  private void InternalToggle()
  {
    if (!this.IsActive() || !this.IsInteractable())
      return;
    this.isOn = !this.isOn;
  }

  public virtual void OnPointerClick(PointerEventData eventData)
  {
    if (eventData.button != PointerEventData.InputButton.Left)
      return;
    this.InternalToggle();
  }

  public virtual void OnSubmit(BaseEventData eventData) => this.InternalToggle();

  Transform ICanvasElement.get_transform() => this.transform;

  [Serializable]
  public class ToggleEvent : UnityEvent<bool>
  {
  }
}

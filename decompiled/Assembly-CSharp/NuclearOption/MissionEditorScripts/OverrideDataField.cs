// Decompiled with JetBrains decompiler
// Type: NuclearOption.MissionEditorScripts.OverrideDataField
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.SavedMission.ObjectiveV2;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

#nullable disable
namespace NuclearOption.MissionEditorScripts;

public class OverrideDataField : MonoBehaviour
{
  [SerializeField]
  private Toggle overrideToggle;
  [SerializeField]
  public RectTransform innerHolder;
  [SerializeField]
  private Color labelDisabledColor = new Color(0.8f, 0.8f, 0.8f);
  private OverrideDataField.InnerField[] innerFields;
  private Action<bool> setOverride;
  private IValueWrapper wrapper;

  private void Awake()
  {
    this.overrideToggle.onValueChanged.AddListener(new UnityAction<bool>(this.OverrideToggled));
    if (this.setOverride != null)
      return;
    this.overrideToggle.interactable = false;
  }

  private void OnDestroy() => this.wrapper?.UnregisterOnChange((object) this);

  public void Setup<T>(
    ValueWrapper<Override<T>> wrapper,
    params OverrideDataField.InnerField[] innerFields)
    where T : IEquatable<T>
  {
    this.wrapper?.UnregisterOnChange((object) this);
    this.wrapper = (IValueWrapper) wrapper;
    this.setOverride = (Action<bool>) (isOverride =>
    {
      T obj = wrapper.Value.Value;
      wrapper.SetValue(new Override<T>(isOverride, obj), (object) this, true);
    });
    wrapper.RegisterOnChange((object) this, (ValueWrapper<Override<T>>.OnChangeDelegate) (value =>
    {
      if (this.overrideToggle.isOn == value.IsOverride)
        return;
      this.UpdateFields(value.IsOverride);
    }));
    this.SetupInternal(wrapper.Value.IsOverride, innerFields);
  }

  public void SetupInternal(bool isOverride, OverrideDataField.InnerField[] innerFields)
  {
    if (innerFields == null)
      innerFields = Array.Empty<OverrideDataField.InnerField>();
    this.innerFields = innerFields;
    for (int index = 0; index < this.innerFields.Length; ++index)
      this.innerFields[index].Setup();
    this.overrideToggle.interactable = true;
    this.UpdateFields(isOverride);
  }

  private void UpdateFields(bool isOverride)
  {
    this.overrideToggle.SetIsOnWithoutNotify(isOverride);
    for (int index = 0; index < this.innerFields.Length; ++index)
      this.innerFields[index].SetInteractable(isOverride, this.labelDisabledColor);
  }

  public void SetupReadOnly(params OverrideDataField.InnerField[] innerFields)
  {
    this.wrapper?.UnregisterOnChange((object) this);
    this.wrapper = (IValueWrapper) null;
    this.overrideToggle.SetIsOnWithoutNotify(false);
    this.overrideToggle.interactable = false;
    if (innerFields == null)
      return;
    foreach (OverrideDataField.InnerField innerField in innerFields)
      innerField.SetInteractable(false, this.labelDisabledColor);
  }

  private void OverrideToggled(bool isOn)
  {
    this.UpdateFields(isOn);
    this.setOverride(isOn);
  }

  public struct InnerField
  {
    public readonly DataField DataField;
    public readonly Button Button;
    private Color defaultColor;

    public InnerField(DataField dataField)
      : this()
    {
      this.DataField = dataField;
    }

    public InnerField(Button button)
      : this()
    {
      this.Button = button;
    }

    public void Setup()
    {
      if ((UnityEngine.Object) this.DataField != (UnityEngine.Object) null)
        this.defaultColor = this.DataField.LabelColor;
      if (!((UnityEngine.Object) this.Button != (UnityEngine.Object) null))
        return;
      TextMeshProUGUI componentInChildren = this.Button.GetComponentInChildren<TextMeshProUGUI>();
      if (!((UnityEngine.Object) componentInChildren != (UnityEngine.Object) null))
        return;
      this.defaultColor = componentInChildren.color;
    }

    public readonly void SetInteractable(bool interactable, Color disabledColor)
    {
      if ((UnityEngine.Object) this.DataField != (UnityEngine.Object) null)
      {
        this.DataField.Interactable = interactable;
        this.DataField.LabelColor = interactable ? this.defaultColor : disabledColor;
      }
      if (!((UnityEngine.Object) this.Button != (UnityEngine.Object) null))
        return;
      this.Button.interactable = interactable;
      TextMeshProUGUI componentInChildren = this.Button.GetComponentInChildren<TextMeshProUGUI>();
      if (!((UnityEngine.Object) componentInChildren != (UnityEngine.Object) null))
        return;
      componentInChildren.color = interactable ? this.defaultColor : disabledColor;
    }

    public static implicit operator OverrideDataField.InnerField(DataField dataField)
    {
      return new OverrideDataField.InnerField(dataField);
    }

    public static implicit operator OverrideDataField.InnerField(Button button)
    {
      return new OverrideDataField.InnerField(button);
    }
  }
}

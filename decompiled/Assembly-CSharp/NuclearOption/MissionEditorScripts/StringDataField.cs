// Decompiled with JetBrains decompiler
// Type: NuclearOption.MissionEditorScripts.StringDataField
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.SavedMission.ObjectiveV2;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

#nullable disable
namespace NuclearOption.MissionEditorScripts;

public class StringDataField : DataField, IDataField<string>
{
  [SerializeField]
  private TMP_InputField text;
  [SerializeField]
  private float heightPerLine = 30f;
  private Action<string> setValue;
  private IValueWrapper wrapper;

  protected override void SetFieldInteractable(bool value) => this.text.interactable = value;

  protected override void AwakeSetup()
  {
    this.text.onValueChanged.AddListener(new UnityAction<string>(this.OnValueChanged));
  }

  private void OnDestroy() => this.wrapper?.UnregisterOnChange((object) this);

  void IDataField<string>.Setup(string label, IValueWrapper<string> wrapper)
  {
    this.Setup(label, wrapper);
  }

  public void Setup(string label, IValueWrapper<string> wrapper, int? multiLineCount = null)
  {
    this.wrapper?.UnregisterOnChange((object) this);
    this.wrapper = (IValueWrapper) wrapper;
    wrapper.RegisterOnChange((object) this, (ValueWrapper<string>.OnChangeDelegate) (v => this.text.SetTextWithoutNotify(wrapper.Value ?? "")));
    this.Setup(label, wrapper.Value, (Action<string>) (v => wrapper.SetValue(v, (object) this)), multiLineCount);
  }

  public void Setup(string label, string current, Action<string> setValue, int? multiLineCount = null)
  {
    this.text.lineType = multiLineCount.HasValue ? TMP_InputField.LineType.MultiLineNewline : TMP_InputField.LineType.SingleLine;
    RectTransform transform = (RectTransform) this.text.transform;
    Vector2 sizeDelta = transform.sizeDelta;
    if (multiLineCount.HasValue)
      sizeDelta.y = this.heightPerLine * (float) multiLineCount.Value;
    transform.sizeDelta = sizeDelta;
    this.label.text = label;
    this.text.SetTextWithoutNotify(current ?? "");
    this.setValue = setValue;
    this.Interactable = true;
  }

  private void OnValueChanged(string value)
  {
    Action<string> setValue = this.setValue;
    if (setValue == null)
      return;
    setValue(value);
  }
}

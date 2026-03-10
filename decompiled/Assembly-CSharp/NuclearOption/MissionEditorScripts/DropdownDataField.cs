// Decompiled with JetBrains decompiler
// Type: NuclearOption.MissionEditorScripts.DropdownDataField
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

#nullable disable
namespace NuclearOption.MissionEditorScripts;

public class DropdownDataField : DataField
{
  [SerializeField]
  private TMP_Dropdown dropdown;
  private Action<int> setValue;
  private List<string> options;

  protected override void SetFieldInteractable(bool value) => this.dropdown.interactable = value;

  protected override void AwakeSetup()
  {
    this.dropdown.onValueChanged.AddListener(new UnityAction<int>(this.OnValueChanged));
  }

  public void Setup(string label, List<string> options, string current, Action<string> setValue)
  {
    int current1 = options.IndexOf(current);
    if (current1 == -1)
      current1 = 0;
    this.Setup(label, options, current1, (Action<int>) (i => setValue(options[i])));
  }

  public void Setup(string label, List<string> options, int current, Action<int> setValue)
  {
    if (!string.IsNullOrEmpty(label))
      this.label.text = label;
    this.dropdown.ClearOptions();
    this.options = options;
    this.dropdown.AddOptions(options);
    this.dropdown.SetValueWithoutNotify(current);
    this.setValue = setValue;
    this.Interactable = true;
  }

  private void OnValueChanged(int arg0)
  {
    Action<int> setValue = this.setValue;
    if (setValue == null)
      return;
    setValue(arg0);
  }

  public void LabelWidth(int width)
  {
    RectTransform transform = (RectTransform) this.label.transform;
    transform.sizeDelta = transform.sizeDelta with
    {
      x = (float) width
    };
  }

  internal void SetValue(string faction)
  {
    int input = this.options.IndexOf(faction);
    if (input == -1)
      input = 0;
    this.dropdown.SetValueWithoutNotify(input);
    Action<int> setValue = this.setValue;
    if (setValue == null)
      return;
    setValue(input);
  }
}

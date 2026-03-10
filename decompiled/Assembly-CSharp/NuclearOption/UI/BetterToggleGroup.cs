// Decompiled with JetBrains decompiler
// Type: NuclearOption.UI.BetterToggleGroup
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

#nullable disable
namespace NuclearOption.UI;

[AddComponentMenu("UI/Better Toggle Group", 532)]
[RequireComponent(typeof (RectTransform))]
public class BetterToggleGroup : MonoBehaviour
{
  [SerializeField]
  private int currentValue;
  [SerializeField]
  private List<Toggle> toggles;
  private bool settingValue;

  public event BetterToggleGroup.ToggleIndexOn OnChangeValue;

  public int Value => this.currentValue;

  private void Awake()
  {
    for (int index1 = 0; index1 < this.toggles.Count; ++index1)
    {
      int index = index1;
      this.toggles[index1].onValueChanged.AddListener((UnityAction<bool>) (on => this.ToggleChanged(on, index)));
    }
  }

  private void OnValidate()
  {
    List<Toggle> toggles = this.toggles;
    // ISSUE: explicit non-virtual call
    if ((toggles != null ? (__nonvirtual (toggles.Count) > 0 ? 1 : 0) : 0) == 0)
      return;
    this.SetValue(this.currentValue, false);
  }

  private void Start() => this.SetValue(this.currentValue, false);

  private void ToggleChanged(bool on, int i)
  {
    if (this.settingValue)
      return;
    this.SetValue(i, true);
  }

  public void SetValue(int value, bool notify)
  {
    if (value >= this.toggles.Count)
      throw new IndexOutOfRangeException($"Failed to set ToggleGroup to value {value} because there are only {this.toggles.Count} toggles");
    this.settingValue = true;
    this.currentValue = value;
    try
    {
      for (int index = 0; index < this.toggles.Count; ++index)
      {
        Toggle toggle = this.toggles[index];
        bool flag = index == value;
        toggle.interactable = !flag;
        if (notify)
        {
          try
          {
            toggle.isOn = flag;
          }
          catch (Exception ex)
          {
            Debug.LogError((object) $"Error setting inner Toggle {ex}");
          }
        }
        else
          toggle.SetIsOnWithoutNotify(flag);
      }
    }
    finally
    {
      this.settingValue = false;
    }
    if (!notify)
      return;
    BetterToggleGroup.ToggleIndexOn onChangeValue = this.OnChangeValue;
    if (onChangeValue == null)
      return;
    onChangeValue(value);
  }

  public delegate void ToggleIndexOn(int index);
}

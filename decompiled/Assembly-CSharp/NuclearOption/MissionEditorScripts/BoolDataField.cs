// Decompiled with JetBrains decompiler
// Type: NuclearOption.MissionEditorScripts.BoolDataField
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.SavedMission.ObjectiveV2;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

#nullable disable
namespace NuclearOption.MissionEditorScripts;

public class BoolDataField : DataField, IDataField<bool>
{
  [SerializeField]
  private Toggle toggle;
  private IValueWrapper<bool> wrapper;

  protected override void SetFieldInteractable(bool value) => this.toggle.interactable = value;

  protected override void AwakeSetup()
  {
    this.toggle.onValueChanged.AddListener(new UnityAction<bool>(this.OnValueChanged));
  }

  public void Setup(string label, IValueWrapper<bool> wrapper)
  {
    this.wrapper?.UnregisterOnChange((object) this);
    this.wrapper = wrapper;
    wrapper.RegisterOnChange((object) this, new ValueWrapper<bool>.OnChangeDelegate(this.Wrapper_OnChange));
    this.label.text = label;
    this.toggle.SetIsOnWithoutNotify(wrapper.Value);
    this.Interactable = true;
  }

  private void Wrapper_OnChange(bool newValue) => this.toggle.SetIsOnWithoutNotify(newValue);

  private void OnValueChanged(bool value) => this.wrapper.SetValue(value, (object) this);
}

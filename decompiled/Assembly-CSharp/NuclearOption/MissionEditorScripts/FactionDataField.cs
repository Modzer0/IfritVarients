// Decompiled with JetBrains decompiler
// Type: NuclearOption.MissionEditorScripts.FactionDataField
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.SavedMission;
using NuclearOption.SavedMission.ObjectiveV2;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

#nullable disable
namespace NuclearOption.MissionEditorScripts;

public class FactionDataField : DataField, IDataField<string>
{
  [SerializeField]
  private TMP_Dropdown dropdown;
  private List<string> factionNames;
  private IValueWrapper<string> wrapper;
  private string noFactionString;

  public void SetNoFactionString(string label)
  {
    this.noFactionString = label;
    this.factionNames[0] = label;
    this.dropdown.ClearOptions();
    this.dropdown.AddOptions(this.factionNames);
  }

  protected override void SetFieldInteractable(bool value) => this.dropdown.interactable = value;

  protected override void AwakeSetup()
  {
    this.factionNames = MissionManager.CurrentMission.factions.Select<MissionFaction, string>((Func<MissionFaction, string>) (x => x.factionName)).Prepend<string>("None").ToList<string>();
    this.dropdown.ClearOptions();
    this.dropdown.AddOptions(this.factionNames);
    this.dropdown.onValueChanged.AddListener(new UnityAction<int>(this.OnValueChanged));
  }

  private void OnDestroy() => this.wrapper?.UnregisterOnChange((object) this);

  public void Setup(string label, IValueWrapper<string> wrapper)
  {
    this.label.text = label;
    this.Setup(wrapper);
  }

  public void Setup(IValueWrapper<string> wrapper)
  {
    this.wrapper?.UnregisterOnChange((object) this);
    this.wrapper = wrapper;
    wrapper.RegisterOnChange((object) this, new ValueWrapper<string>.OnChangeDelegate(this.SetDropdown));
    this.SetDropdown(wrapper.Value);
    this.Interactable = true;
  }

  private void SetDropdown(string value)
  {
    int input = FactionHelper.EmptyOrNoFaction(value) ? 0 : this.factionNames.IndexOf(value);
    if (input == -1)
      Debug.LogError((object) ("Could not find faction with name " + value));
    this.dropdown.SetValueWithoutNotify(input);
  }

  private void OnValueChanged(int index)
  {
    string factionName = this.factionNames[index];
    this.wrapper.SetValue(factionName == this.noFactionString ? "None" : factionName, (object) this);
  }
}

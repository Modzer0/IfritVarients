// Decompiled with JetBrains decompiler
// Type: MissionSelectListItem
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using JamesFrowen.ScriptableVariables.UI;
using NuclearOption.SavedMission;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

#nullable disable
public class MissionSelectListItem : ListItem<MissionSelectListItem.Item>
{
  public Button Button;
  [SerializeField]
  private TextMeshProUGUI text;
  [SerializeField]
  private MissionTagList tagList;

  protected override void Awake() => this.Button.onClick.AddListener(new UnityAction(this.OnClick));

  private void OnClick() => this.Value.Menu.SetSelectedMission(this.Value.Key);

  protected override void SetValue(MissionSelectListItem.Item value)
  {
    this.text.text = value.Key.Name;
    this.Button.interactable = !value.Key.Equals(value.Menu.SelectedMission);
    this.tagList.UpdateList((IReadOnlyList<MissionTag>) value.Mission.missionSettings.Tags);
  }

  public readonly struct Item(MissionSelectPanel menu, MissionKey key, MissionQuickLoad mission)
  {
    public readonly MissionSelectPanel Menu = menu;
    public readonly MissionKey Key = key;
    public readonly MissionQuickLoad Mission = mission;
  }
}

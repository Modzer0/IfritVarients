// Decompiled with JetBrains decompiler
// Type: NuclearOption.MissionEditorScripts.SceneryOptions
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.SavedMission;
using UnityEngine;
using UnityEngine.UI;

#nullable disable
namespace NuclearOption.MissionEditorScripts;

public class SceneryOptions : UnitPanelOptions
{
  [SerializeField]
  private Toggle indestructibleToggle;
  [SerializeField]
  private GameObject indestructibleToggleDifferentValue;

  protected override void SetupInner()
  {
    this.targets.SetupToggle(this.indestructibleToggle, this.indestructibleToggleDifferentValue, (NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<SavedUnit>.GetFieldRef<bool>) (x => ref ((SavedScenery) x).indestructible));
  }

  public override void Cleanup() => this.targets.RemoveChanged((object) this.indestructibleToggle);

  public override void OnTargetsChanged()
  {
  }
}

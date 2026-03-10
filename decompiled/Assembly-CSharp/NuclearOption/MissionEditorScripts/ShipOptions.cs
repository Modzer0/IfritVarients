// Decompiled with JetBrains decompiler
// Type: NuclearOption.MissionEditorScripts.ShipOptions
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.SavedMission;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

#nullable disable
namespace NuclearOption.MissionEditorScripts;

public class ShipOptions : UnitPanelOptions
{
  [Header("Hold")]
  [SerializeField]
  private Toggle holdPosition;
  [SerializeField]
  private GameObject holdPositionDifferentValue;
  [Header("Skill")]
  [SerializeField]
  private Slider skillSlider;
  [SerializeField]
  private TextMeshProUGUI skillSliderLabel;
  [Header("Waypoints")]
  [SerializeField]
  private WaypointList waypointList;

  protected override void SetupInner()
  {
    this.targets.SetupToggle(this.holdPosition, this.holdPositionDifferentValue, (NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<SavedUnit>.GetFieldRef<bool>) (x => ref ((SavedShip) x).holdPosition));
    this.targets.SetupSlider(this.skillSlider, this.skillSliderLabel, (NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<SavedUnit>.GetFieldRef<float>) (x => ref ((SavedShip) x).skill), (NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<SavedUnit>.GetLabelString<float>) (v => v.ToString("F1")));
  }

  public override void Cleanup()
  {
    this.targets.RemoveChanged((object) this.holdPosition);
    this.targets.RemoveChanged((object) this.skillSlider);
  }

  public override void OnTargetsChanged() => this.waypointList.Setup(this.targets);
}

// Decompiled with JetBrains decompiler
// Type: NuclearOption.MissionEditorScripts.MissileOptions
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.SavedMission;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

#nullable disable
namespace NuclearOption.MissionEditorScripts;

public class MissileOptions : UnitPanelOptions
{
  [Header("Speed")]
  [SerializeField]
  private float maxSpeed = 1020f;
  [SerializeField]
  private Slider airspeedSlider;
  [SerializeField]
  private TextMeshProUGUI airspeedHandleLabel;

  protected override void SetupInner()
  {
    this.targets.SetupSlider<float>(this.airspeedSlider, this.airspeedHandleLabel, (NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<SavedUnit>.GetFieldRef<float>) (x => ref ((SavedMissile) x).startingSpeed), (NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<SavedUnit>.ModifyValue<float, float>) (v => Mathf.Sqrt(v / this.maxSpeed)), (NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<SavedUnit>.ModifyValue<float, float>) (v => v * v * this.maxSpeed), (NuclearOption.MissionEditorScripts.MultiSelect.MultiSelect<SavedUnit>.GetLabelString<float>) (v => UnitConverter.SpeedReading(v)));
  }

  public override void Cleanup() => this.targets.RemoveChanged((object) this.airspeedSlider);

  public override void OnTargetsChanged()
  {
  }
}

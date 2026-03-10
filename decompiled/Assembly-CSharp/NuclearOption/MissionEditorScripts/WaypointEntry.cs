// Decompiled with JetBrains decompiler
// Type: NuclearOption.MissionEditorScripts.WaypointEntry
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.MissionEditorScripts.MultiSelect;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

#nullable disable
namespace NuclearOption.MissionEditorScripts;

public class WaypointEntry : MonoBehaviour
{
  public const string UNIT_SPAWN_TIMING = "Unit Spawn";
  [SerializeField]
  private Button positionButton;
  [SerializeField]
  private Button removeButton;
  [SerializeField]
  private Text positionLabel;
  [SerializeField]
  private Dropdown objectiveSelect;
  private MultiField<GlobalPosition> positionField;
  private MultiField<string> objectiveField;
  private WaypointList waypointList;

  public int Index { get; private set; }

  private void Awake()
  {
    this.positionButton.onClick.AddListener(new UnityAction(this.PositionClicked));
    this.removeButton.onClick.AddListener(new UnityAction(this.RemoveClicked));
    this.objectiveSelect.onValueChanged.AddListener(new UnityAction<int>(this.ObjectiveSelected));
  }

  public void SetEntry(
    WaypointList waypointList,
    List<string> objectives,
    int index,
    MultiField<GlobalPosition> positionField,
    MultiField<string> objectiveField)
  {
    this.Index = index;
    this.positionField = positionField;
    this.objectiveField = objectiveField;
    this.waypointList = waypointList;
    this.positionLabel.text = $"Set Position [{positionField.Get()}]";
    this.objectiveSelect.options.Clear();
    this.objectiveSelect.AddOptions(objectives);
    int input = objectives.IndexOf(objectiveField.Get());
    if (input == -1)
    {
      Debug.LogError((object) $"Waypoint is using objective with name {objectiveField.Get()} but no objective with that names exists. Resetting waypoint to use UnitSpawn");
      objectiveField.Set("Unit Spawn");
      input = 0;
    }
    this.objectiveSelect.SetValueWithoutNotify(input);
  }

  private void PositionClicked()
  {
    GlobalPosition globalPosition = SceneSingleton<CameraStateManager>.i.transform.position.ToGlobalPosition();
    this.positionField.Set(globalPosition);
    this.positionLabel.text = $"Set Position [{globalPosition.ToString()}]";
  }

  private void ObjectiveSelected(int index)
  {
    this.objectiveField.Set(this.objectiveSelect.options[this.objectiveSelect.value].text);
  }

  public void RemoveClicked() => this.waypointList.RemoveWayPoint(this);
}

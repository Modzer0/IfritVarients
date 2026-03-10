// Decompiled with JetBrains decompiler
// Type: NuclearOption.MissionEditorScripts.NewObjectivePanel
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.SavedMission.ObjectiveV2;
using UnityEngine;

#nullable disable
namespace NuclearOption.MissionEditorScripts;

public class NewObjectivePanel : NewPanelBase<ObjectiveType>
{
  [SerializeField]
  private EditObjectivePanel editObjectivePanelPrefab;

  protected override void CreateItem(ObjectiveType type)
  {
    Objective objective = SavedObjective.Create(type);
    objective.LoadNew(new SavedObjective(this.nameField.text, type));
    MissionManager.Objectives.AddNewObjective(objective);
    SceneSingleton<MissionEditor>.i.CheckAutoSave();
    this.Panel.Parent?.Refresh();
    this.Panel.Create<EditObjectivePanel>(this.editObjectivePanelPrefab).SetObjective(objective);
  }
}

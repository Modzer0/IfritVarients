// Decompiled with JetBrains decompiler
// Type: NuclearOption.MissionEditorScripts.NewOutcomePanel
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.SavedMission.ObjectiveV2;
using UnityEngine;

#nullable disable
namespace NuclearOption.MissionEditorScripts;

public class NewOutcomePanel : NewPanelBase<OutcomeType>
{
  [SerializeField]
  private EditOutcomePanel editOutcomePanelPrefab;
  private Objective parentObjective;

  public void Setup(Objective parentObjective) => this.parentObjective = parentObjective;

  protected override void CreateItem(OutcomeType type)
  {
    Outcome outcome = SavedOutcome.Create(type);
    outcome.LoadNew(new SavedOutcome(this.nameField.text, type));
    MissionManager.Objectives.AddNewOutcome(outcome, this.parentObjective);
    SceneSingleton<MissionEditor>.i.CheckAutoSave();
    this.Panel.Parent?.Refresh();
    this.Panel.Create<EditOutcomePanel>(this.editOutcomePanelPrefab).SetOutcome(outcome, this.parentObjective);
  }
}

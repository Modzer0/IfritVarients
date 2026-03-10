// Decompiled with JetBrains decompiler
// Type: NuclearOption.SavedMission.ObjectiveV2.Outcomes.StartObjectiveOutcome
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.MissionEditorScripts;
using System.Collections.Generic;

#nullable disable
namespace NuclearOption.SavedMission.ObjectiveV2.Outcomes;

public class StartObjectiveOutcome : Outcome
{
  public List<Objective> objectivesToStart;

  protected override void WriteOutcome(ReadWriteObjective writer)
  {
    writer.ReferenceList<Objective>(ref this.objectivesToStart);
  }

  public override void ReferenceDestroyed(ISaveableReference reference)
  {
    if (!(reference is Objective objective))
      return;
    this.objectivesToStart.Remove(objective);
  }

  public override void Complete(Objective completedObjective)
  {
    foreach (Objective objective in this.objectivesToStart)
      MissionManager.Runner.StartObjective(objective);
  }

  public override void DrawData(DataDrawer drawer)
  {
    if (this.objectivesToStart == null)
      this.objectivesToStart = new List<Objective>();
    drawer.DrawList(300, this.objectivesToStart);
  }
}

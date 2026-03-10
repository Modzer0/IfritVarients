// Decompiled with JetBrains decompiler
// Type: NuclearOption.SavedMission.ObjectiveV2.Outcomes.CompleteObjectiveOutcome
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.MissionEditorScripts;
using System;
using System.Collections.Generic;

#nullable disable
namespace NuclearOption.SavedMission.ObjectiveV2.Outcomes;

internal class CompleteObjectiveOutcome : Outcome
{
  private List<Objective> objectivesToStart;
  private CompleteObjectiveOutcome.Options options;

  protected override void WriteOutcome(ReadWriteObjective writer)
  {
    writer.Enum<CompleteObjectiveOutcome.Options>(ref this.options);
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
    if (this.options == CompleteObjectiveOutcome.Options.Stop)
    {
      foreach (Objective objective in this.objectivesToStart)
        MissionManager.Runner.StopObjective(objective);
    }
    else
    {
      foreach (Objective objective in this.objectivesToStart)
        MissionManager.Runner.CompleteObjective(objective);
    }
  }

  public override void DrawData(DataDrawer drawer)
  {
    if (this.objectivesToStart == null)
      this.objectivesToStart = new List<Objective>();
    drawer.DrawEnum<CompleteObjectiveOutcome.Options>("Stop Mode", (int) this.options, (Action<int>) (v => this.options = (CompleteObjectiveOutcome.Options) v));
    drawer.DrawList(300, this.objectivesToStart);
  }

  public enum Options
  {
    Stop,
    Complete,
  }
}

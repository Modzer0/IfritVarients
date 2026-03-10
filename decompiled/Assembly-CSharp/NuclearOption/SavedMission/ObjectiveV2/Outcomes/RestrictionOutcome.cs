// Decompiled with JetBrains decompiler
// Type: NuclearOption.SavedMission.ObjectiveV2.Outcomes.RestrictionOutcome
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.MissionEditorScripts;
using System;
using System.Collections.Generic;

#nullable disable
namespace NuclearOption.SavedMission.ObjectiveV2.Outcomes;

internal class RestrictionOutcome : Outcome
{
  private RestrictionOutcome.Change endType;
  private List<string> restrictionNames;

  public override void Complete(Objective completedObjective)
  {
    throw new NotImplementedException();
  }

  protected override void WriteOutcome(ReadWriteObjective writer)
  {
    writer.Enum<RestrictionOutcome.Change>(ref this.endType, RestrictionOutcome.Change.Remove);
    writer.StringList(ref this.restrictionNames);
  }

  public override void ReferenceDestroyed(ISaveableReference reference)
  {
  }

  public override void DrawData(DataDrawer drawer)
  {
    drawer.DrawEnum<RestrictionOutcome.Change>("End Type", (int) this.endType, (Action<int>) (v => this.endType = (RestrictionOutcome.Change) v));
  }

  public enum Change
  {
    Add,
    Remove,
  }
}

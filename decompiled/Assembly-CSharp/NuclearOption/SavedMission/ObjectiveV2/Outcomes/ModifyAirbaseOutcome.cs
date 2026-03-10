// Decompiled with JetBrains decompiler
// Type: NuclearOption.SavedMission.ObjectiveV2.Outcomes.ModifyAirbaseOutcome
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.MissionEditorScripts;
using System;
using System.Linq;
using UnityEngine;

#nullable disable
namespace NuclearOption.SavedMission.ObjectiveV2.Outcomes;

internal class ModifyAirbaseOutcome : Outcome
{
  private SavedAirbase airbase;
  private ValueWrapperOverride<string> faction = new ValueWrapperOverride<string>();
  private ValueWrapperOverride<bool> disabled = new ValueWrapperOverride<bool>();
  private ValueWrapperOverride<bool> capturable = new ValueWrapperOverride<bool>();
  private ValueWrapperOverride<float> captureDefense = new ValueWrapperOverride<float>();

  protected override void WriteOutcome(ReadWriteObjective writer)
  {
    writer.Reference<SavedAirbase>(ref this.airbase);
    writer.Override<string>(ref this.faction.GetValueRef(), new ReadWriteObjective.ReadWriteValue<string>(writer.String));
    writer.Override<bool>(ref this.disabled.GetValueRef(), new ReadWriteObjective.ReadWriteValue<bool>(writer.Bool));
    writer.Override<bool>(ref this.capturable.GetValueRef(), new ReadWriteObjective.ReadWriteValue<bool>(writer.Bool));
    writer.Override<float>(ref this.captureDefense.GetValueRef(), new ReadWriteObjective.ReadWriteValue<float>(writer.Float));
  }

  public override void ReferenceDestroyed(ISaveableReference reference)
  {
    if (this.airbase != reference)
      return;
    this.airbase = (SavedAirbase) null;
  }

  public override void Complete(Objective completedObjective)
  {
    if (this.airbase == null)
    {
      Debug.LogWarning((object) "No airbase set for ModifyAirbaseOutcome");
    }
    else
    {
      Airbase airbase = this.airbase.Airbase;
      if (this.faction.Value.IsOverride)
      {
        FactionHQ hq = FactionRegistry.HqFromName(this.faction.Value.Value);
        if ((UnityEngine.Object) hq != (UnityEngine.Object) null)
          airbase.capture.ForceCapture(hq);
      }
      if (this.disabled.Value.IsOverride)
      {
        airbase.SavedAirbase.Disabled = this.disabled.Value.Value;
        airbase.SetDisabled(this.disabled.Value.Value);
      }
      if (this.capturable.Value.IsOverride)
      {
        airbase.SavedAirbase.Capturable = this.capturable.Value.Value;
        airbase.capture.SetCapturable(this.capturable.Value.Value);
      }
      if (!this.captureDefense.Value.IsOverride)
        return;
      airbase.SavedAirbase.CaptureDefense = this.captureDefense.Value.Value;
    }
  }

  public override void DrawData(DataDrawer drawer)
  {
    drawer.InstantiateWithParent<ReferenceDataField>(drawer.Prefabs.ReferenceDataPrefab).Setup<SavedAirbase>("Airbase", MissionManager.GetAllSavedAirbase().ToList<SavedAirbase>(), this.airbase, (Action<SavedAirbase>) (v => this.airbase = v));
    drawer.Space(12);
    drawer.DrawOverride<string, FactionDataField>("Faction", this.faction, drawer.Prefabs.FactionDataPrefab);
    drawer.DrawOverride<bool, BoolDataField>("Disabled", this.disabled, drawer.Prefabs.BoolFieldPrefab);
    drawer.DrawOverride<bool, BoolDataField>("Capturable", this.capturable, drawer.Prefabs.BoolFieldPrefab);
    drawer.DrawOverride<float, FloatDataField>("Capture Defense", this.captureDefense, drawer.Prefabs.FloatFieldPrefab);
  }
}

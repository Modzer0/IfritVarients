// Decompiled with JetBrains decompiler
// Type: NuclearOption.SavedMission.ObjectiveV2.Outcomes.ShowMessageOutcome
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.MissionEditorScripts;
using System;

#nullable disable
namespace NuclearOption.SavedMission.ObjectiveV2.Outcomes;

public class ShowMessageOutcome : Outcome
{
  public string Message;
  public readonly ValueWrapperBool PlaySound = new ValueWrapperBool();
  public readonly ValueWrapperBool ObjectiveFactionOnly = new ValueWrapperBool();

  protected override void WriteOutcome(ReadWriteObjective writer)
  {
    writer.String(ref this.Message);
    writer.Bool(ref this.PlaySound.GetValueRef());
    writer.Bool(ref this.ObjectiveFactionOnly.GetValueRef());
  }

  public override void ReferenceDestroyed(ISaveableReference reference)
  {
  }

  public override void Complete(Objective completedObjective)
  {
    MissionMessages.ShowMessage(this.Message, (bool) (ValueWrapper<bool>) this.PlaySound, !(bool) (ValueWrapper<bool>) this.ObjectiveFactionOnly ? (FactionHQ) null : completedObjective.FactionHQ, true);
  }

  public override void DrawData(DataDrawer drawer)
  {
    drawer.InstantiateWithParent<StringDataField>(drawer.Prefabs.StringFieldPrefab).Setup("Message", this.Message, (Action<string>) (v => this.Message = v));
    drawer.InstantiateWithParent<BoolDataField>(drawer.Prefabs.BoolFieldPrefab).Setup("Play Sound", (IValueWrapper<bool>) this.PlaySound);
    drawer.InstantiateWithParent<BoolDataField>(drawer.Prefabs.BoolFieldPrefab).Setup("Faction Only", (IValueWrapper<bool>) this.ObjectiveFactionOnly);
  }
}

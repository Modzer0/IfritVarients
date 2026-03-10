// Decompiled with JetBrains decompiler
// Type: NuclearOption.SavedMission.ObjectiveV2.Outcomes.EndGameOutcome
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Cysharp.Threading.Tasks;
using NuclearOption.MissionEditorScripts;
using System;

#nullable disable
namespace NuclearOption.SavedMission.ObjectiveV2.Outcomes;

internal class EndGameOutcome : Outcome
{
  private EndType endType;
  private readonly ValueWrapperFloat endDelay = new ValueWrapperFloat();

  public override void Complete(Objective completedObjective)
  {
    if ((double) (float) (ValueWrapper<float>) this.endDelay > 0.0)
      UniTask.Delay((int) (float) (ValueWrapper<float>) this.endDelay * 1000).ContinueWith((Action) (() => completedObjective.FactionHQ.DeclareEndGame(this.endType))).Forget();
    else
      completedObjective.FactionHQ.DeclareEndGame(this.endType);
  }

  protected override void WriteOutcome(ReadWriteObjective writer)
  {
    writer.Enum<EndType>(ref this.endType);
    writer.Float(ref this.endDelay.GetValueRef(), 2f);
  }

  public override void ReferenceDestroyed(ISaveableReference reference)
  {
  }

  public override void DrawData(DataDrawer drawer)
  {
    drawer.DrawEnum<EndType>("End Type", (int) this.endType, (Action<int>) (v => this.endType = (EndType) v));
    drawer.InstantiateWithParent<FloatDataField>(drawer.Prefabs.FloatFieldPrefab).Setup("End delay", (IValueWrapper<float>) this.endDelay);
  }
}

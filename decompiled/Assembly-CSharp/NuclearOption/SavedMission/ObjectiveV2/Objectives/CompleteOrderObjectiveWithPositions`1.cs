// Decompiled with JetBrains decompiler
// Type: NuclearOption.SavedMission.ObjectiveV2.Objectives.CompleteOrderObjectiveWithPositions`1
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;
using Unity.Profiling;

#nullable disable
namespace NuclearOption.SavedMission.ObjectiveV2.Objectives;

public abstract class CompleteOrderObjectiveWithPositions<T> : 
  CompleteOrderObjective<T>,
  IObjectiveWithPosition
{
  private static readonly ProfilerMarker updateAndCheckMarker = new ProfilerMarker("CompleteOrderObjectiveWithPositionsUpdateAndCheck");
  private readonly List<ObjectivePosition> positions = new List<ObjectivePosition>();
  private readonly Action<T> updateNotCompletedCached;

  IReadOnlyList<ObjectivePosition> IObjectiveWithPosition.Positions
  {
    get => (IReadOnlyList<ObjectivePosition>) this.positions;
  }

  public CompleteOrderObjectiveWithPositions()
  {
    this.updateNotCompletedCached = new Action<T>(this.UpdateNotCompleted);
  }

  public override bool UpdateAndCheck()
  {
    using (CompleteOrderObjectiveWithPositions<T>.updateAndCheckMarker.Auto())
    {
      if (base.UpdateAndCheck())
        return true;
      this.UpdatePositions();
      return false;
    }
  }

  public override void ClientOnlyUpdate() => this.UpdatePositions();

  protected void UpdatePositions()
  {
    this.positions.Clear();
    this.completeList.ForeachNotComplete(this.updateNotCompletedCached);
  }

  private void UpdateNotCompleted(T target)
  {
    ObjectivePosition position;
    if (!this.TryGetPosition(target, out position))
      return;
    this.positions.Add(position);
  }

  protected abstract bool TryGetPosition(T item, out ObjectivePosition position);
}

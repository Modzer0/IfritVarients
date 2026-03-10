// Decompiled with JetBrains decompiler
// Type: MissionPosition
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.SavedMission.ObjectiveV2;
using System.Collections.Generic;
using UnityEngine;

#nullable disable
public static class MissionPosition
{
  internal static bool TryGetClosestDistance(Unit unit, out float distance)
  {
    return MissionPosition.TryGetClosestDistance(unit.NetworkHQ, unit.transform.GlobalPosition(), out distance);
  }

  internal static bool TryGetClosestDistance(
    FactionHQ factionHQ,
    Transform transform,
    out float distance)
  {
    return MissionPosition.TryGetClosestDistance(factionHQ, transform.GlobalPosition(), out distance);
  }

  internal static bool TryGetClosestDistance(
    FactionHQ factionHQ,
    GlobalPosition from,
    out float distance)
  {
    Objective objective;
    if (MissionPosition.TryGetClosestObjective(factionHQ, from, out objective))
    {
      MissionPosition.PositionResult result;
      MissionPosition.DistanceTo(objective, from, out result);
      distance = result.Distance;
      return true;
    }
    distance = 0.0f;
    return false;
  }

  public static bool TryGetClosestPosition(Unit unit, out GlobalPosition destination)
  {
    return MissionPosition.TryGetClosestPosition(unit.NetworkHQ, unit.transform.GlobalPosition(), out destination);
  }

  public static bool TryGetClosestPosition(
    FactionHQ factionHQ,
    GlobalPosition from,
    out GlobalPosition destination)
  {
    Objective objective;
    if (MissionPosition.TryGetClosestObjective(factionHQ, from, out objective))
    {
      MissionPosition.PositionResult result;
      MissionPosition.DistanceTo(objective, from, out result);
      destination = result.Position;
      return true;
    }
    destination = new GlobalPosition();
    return false;
  }

  public static bool TryGetClosestObjective(Unit unit, out Objective objective)
  {
    return MissionPosition.TryGetClosestObjective(unit.NetworkHQ, unit.transform.GlobalPosition(), out objective);
  }

  public static bool TryGetClosestObjective(
    FactionHQ factionHQ,
    GlobalPosition from,
    out Objective objective)
  {
    objective = (Objective) null;
    List<Objective> objectives;
    if (!MissionPosition.TryGetActiveObjectives(factionHQ, out objectives))
      return false;
    float num = float.MaxValue;
    foreach (Objective objective1 in objectives)
    {
      MissionPosition.PositionResult result;
      if (MissionPosition.DistanceTo(objective1, from, out result) && (double) result.Distance < (double) num)
      {
        num = result.Distance;
        objective = objective1;
      }
    }
    return objective != null;
  }

  public static bool TryGetClosestObjectivePosition(
    Unit unit,
    out MissionPosition.PositionResult result)
  {
    return MissionPosition.TryGetClosestObjectivePosition(unit.NetworkHQ, unit.transform.GlobalPosition(), out result);
  }

  public static bool TryGetClosestObjectivePosition(
    FactionHQ factionHQ,
    GlobalPosition from,
    out MissionPosition.PositionResult closest)
  {
    closest = new MissionPosition.PositionResult();
    List<Objective> objectives;
    if (!MissionPosition.TryGetActiveObjectives(factionHQ, out objectives))
      return false;
    closest = MissionPosition.PositionResult.MaxDistance;
    bool objectivePosition = false;
    foreach (Objective objective in objectives)
    {
      MissionPosition.PositionResult result;
      if (MissionPosition.DistanceTo(objective, from, out result) && (double) result.Distance < (double) closest.Distance)
      {
        closest = result;
        objectivePosition = true;
      }
    }
    return objectivePosition;
  }

  public static bool DistanceTo(
    Objective obj,
    GlobalPosition from,
    out MissionPosition.PositionResult result)
  {
    result = new MissionPosition.PositionResult();
    if (!(obj is IObjectiveWithPosition objectiveWithPosition))
      return false;
    IReadOnlyList<ObjectivePosition> positions = objectiveWithPosition.Positions;
    if (positions.Count == 0)
      return false;
    float dist = float.MaxValue;
    int index1 = 0;
    for (int index2 = 0; index2 < positions.Count; ++index2)
    {
      float num = FastMath.Distance(positions[index2].Position, from);
      if ((double) num < (double) dist)
      {
        dist = num;
        index1 = index2;
      }
    }
    ObjectivePosition objPos = positions[index1];
    result = new MissionPosition.PositionResult(obj, objPos, dist, from);
    return true;
  }

  public static MissionPosition.PositionResult ResultForPosition(
    GlobalPosition pos,
    GlobalPosition from,
    Objective obj = null)
  {
    return MissionPosition.ResultForPosition(new ObjectivePosition(pos, new float?()), from, obj);
  }

  public static MissionPosition.PositionResult ResultForPosition(
    ObjectivePosition pos,
    GlobalPosition from,
    Objective obj = null)
  {
    float dist = FastMath.Distance(pos.Position, from);
    return new MissionPosition.PositionResult(obj, pos, dist, from);
  }

  public static bool TryGetActiveObjectives(FactionHQ factionHQ, out List<Objective> objectives)
  {
    if ((Object) factionHQ == (Object) null)
    {
      objectives = (List<Objective>) null;
      return false;
    }
    if (MissionManager.Runner != null)
      return MissionManager.Runner.activeByFaction.TryGetValue(factionHQ, out objectives);
    Debug.LogWarning((object) "Mission Runner was null");
    objectives = (List<Objective>) null;
    return false;
  }

  public static bool HasObjectiveWithPosition(FactionHQ factionHQ)
  {
    List<Objective> objectives;
    if (MissionPosition.TryGetActiveObjectives(factionHQ, out objectives))
    {
      foreach (Objective objective in objectives)
      {
        if (objective is IObjectiveWithPosition objectiveWithPosition && objectiveWithPosition.Positions.Count > 0)
          return true;
      }
    }
    return false;
  }

  public static void GetAllPositionsResults(
    Unit unit,
    bool includeHidden,
    List<MissionPosition.PositionResult> results)
  {
    MissionPosition.GetAllPositionsResults(unit.NetworkHQ, unit.transform.GlobalPosition(), includeHidden, results);
  }

  public static void GetAllPositionsResults(
    FactionHQ factionHQ,
    GlobalPosition from,
    bool includeHidden,
    List<MissionPosition.PositionResult> results)
  {
    results.Clear();
    List<Objective> objectives;
    if (!MissionPosition.TryGetActiveObjectives(factionHQ, out objectives))
      return;
    foreach (Objective objective in objectives)
    {
      if ((includeHidden || !objective.SavedObjective.Hidden) && objective is IObjectiveWithPosition objectiveWithPosition)
      {
        foreach (ObjectivePosition position in (IEnumerable<ObjectivePosition>) objectiveWithPosition.Positions)
        {
          float dist = FastMath.Distance(position.Position, from);
          results.Add(new MissionPosition.PositionResult(objective, position, dist, from));
        }
      }
    }
  }

  public readonly struct PositionResult
  {
    public readonly Objective Objective;
    public readonly GlobalPosition Position;
    public readonly float Distance;
    public readonly Vector3 Direction;
    public readonly float? Range;

    public PositionResult(
      Objective obj,
      ObjectivePosition objPos,
      float dist,
      GlobalPosition from)
      : this()
    {
      this.Objective = obj;
      this.Position = objPos.Position;
      this.Range = objPos.Range;
      this.Distance = dist;
      this.Direction = objPos.Position - from;
    }

    private PositionResult(float dist)
      : this()
    {
      this.Distance = dist;
    }

    public static MissionPosition.PositionResult MaxDistance
    {
      get => new MissionPosition.PositionResult(float.MaxValue);
    }
  }
}

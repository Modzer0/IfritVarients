// Decompiled with JetBrains decompiler
// Type: NuclearOption.SavedMission.ObjectiveV2.Objectives.ReachUnitsObjective
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.MissionEditorScripts;
using NuclearOption.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Profiling;
using UnityEngine;

#nullable disable
namespace NuclearOption.SavedMission.ObjectiveV2.Objectives;

public class ReachUnitsObjective : Objective, IObjectiveWithPosition
{
  private static readonly ProfilerMarker updateAndCheckMarker = new ProfilerMarker("ReachUnitsObjectiveUpdateAndCheck");
  private CompleteOrder completeOrder;
  private List<ReachUnitsObjective.TargetUnit> targets;
  private int currentTarget;
  private readonly List<ObjectivePosition> nextPosition = new List<ObjectivePosition>();
  private readonly List<ReachUnitsObjective.TargetUnit> toCheck = new List<ReachUnitsObjective.TargetUnit>();
  private readonly List<ReachUnitsObjective.TargetUnit> allReached = new List<ReachUnitsObjective.TargetUnit>();
  private readonly List<int> networkList = new List<int>();
  private float completePercent;
  private readonly List<SavedUnit> allUnitsDropdownList = new List<SavedUnit>();
  private readonly List<string> allUnitsNamesDropdownList = new List<string>();

  public override bool NeedsFaction => true;

  public override float CompletePercent => this.completePercent;

  IReadOnlyList<ObjectivePosition> IObjectiveWithPosition.Positions
  {
    get
    {
      return this.targets.Count <= 0 ? (IReadOnlyList<ObjectivePosition>) Array.Empty<ObjectivePosition>() : (IReadOnlyList<ObjectivePosition>) this.nextPosition;
    }
  }

  private void UpdateCompletePercent()
  {
    switch (this.completeOrder)
    {
      case CompleteOrder.CompleteAny:
        this.completePercent = 0.0f;
        break;
      case CompleteOrder.CompleteAll:
        this.completePercent = (float) (1.0 - (double) this.nextPosition.Count / (double) this.targets.Count);
        break;
      case CompleteOrder.InOrder:
        this.completePercent = (float) this.currentTarget / (float) this.targets.Count;
        break;
    }
  }

  public override void ReceiveNetworkData(List<int> data)
  {
    this.toCheck.Clear();
    foreach (int index in data)
      this.toCheck.Add(this.targets[index]);
    this.UpdateCompletePercent();
  }

  protected override void WriteObjective(ReadWriteObjective writer)
  {
    writer.Enum<CompleteOrder>(ref this.completeOrder, CompleteOrder.InOrder);
    writer.DataList<ReachUnitsObjective.TargetUnit>(ref this.targets);
  }

  protected override void DataReferenceDestroyed(ISaveableReference reference)
  {
    if (!(reference is SavedUnit savedUnit))
      return;
    for (int index = 0; index < this.targets.Count; ++index)
    {
      if (this.targets[index].Unit == savedUnit)
      {
        this.targets.RemoveAt(index);
        break;
      }
    }
  }

  public override void OnStart() => this.RefreshTargets();

  public override void ClientOnlyUpdate() => this.UpdatePositions(true);

  public override bool UpdateAndCheck()
  {
    using (ReachUnitsObjective.updateAndCheckMarker.Auto())
    {
      ReachUnitsObjective.TargetUnit reached;
      if (this.CheckTargetsReached(out reached))
      {
        if (this.completeOrder == CompleteOrder.CompleteAny)
          return true;
        if (this.completeOrder == CompleteOrder.CompleteAll)
        {
          this.allReached.Add(reached);
          if (this.allReached.Count == this.targets.Count)
            return true;
        }
        if (this.completeOrder == CompleteOrder.InOrder)
        {
          ++this.currentTarget;
          if (this.currentTarget >= this.targets.Count)
            return true;
        }
        this.RefreshTargets();
      }
      this.UpdatePositions(false);
      return false;
    }
  }

  private bool CheckTargetsReached(out ReachUnitsObjective.TargetUnit reached)
  {
    reached = (ReachUnitsObjective.TargetUnit) null;
    List<Player> players = this.FactionHQ.GetPlayers(false);
    if (players.Count == 0)
      return false;
    foreach (ReachUnitsObjective.TargetUnit targetUnit in this.toCheck)
      UnitRegistry.TryGetPosition(targetUnit.Unit, out targetUnit.GlobalPosition);
    foreach (Player player in players)
    {
      Aircraft aircraft = player.Aircraft;
      if (!((UnityEngine.Object) aircraft == (UnityEngine.Object) null))
      {
        GlobalPosition a = aircraft.GlobalPosition();
        Vector3 velocity = aircraft.rb.velocity;
        foreach (ReachUnitsObjective.TargetUnit targetUnit in this.toCheck)
        {
          if (FastMath.InRange(a, targetUnit.GlobalPosition, (float) (ValueWrapper<float>) targetUnit.Range) && (double) Vector3.Dot(velocity, targetUnit.GlobalPosition - a) < 0.0)
          {
            reached = targetUnit;
            return true;
          }
        }
      }
    }
    reached = (ReachUnitsObjective.TargetUnit) null;
    return false;
  }

  private void RefreshTargets()
  {
    this.toCheck.Clear();
    this.networkList.Clear();
    if (this.targets.Count == 0)
      return;
    if (this.completeOrder == CompleteOrder.InOrder)
    {
      this.toCheck.Add(this.targets[this.currentTarget]);
      this.networkList.Add(this.currentTarget);
    }
    else
    {
      for (int index = 0; index < this.targets.Count; ++index)
      {
        ReachUnitsObjective.TargetUnit target = this.targets[index];
        if (!this.allReached.Contains(target))
        {
          this.toCheck.Add(target);
          this.networkList.Add(index);
        }
      }
    }
    this.UpdatePositions(false);
    this.UpdateCompletePercent();
    if (!this.MissionManager.IsServer)
      return;
    this.MissionManager.UpdateNetworkData((Objective) this, this.networkList);
  }

  private void UpdatePositions(bool updateCache)
  {
    if (updateCache)
    {
      foreach (ReachUnitsObjective.TargetUnit targetUnit in this.toCheck)
        UnitRegistry.TryGetPosition(targetUnit.Unit, out targetUnit.GlobalPosition);
    }
    this.nextPosition.Clear();
    foreach (ReachUnitsObjective.TargetUnit targetUnit in this.toCheck)
      this.nextPosition.Add(targetUnit.ToObjectivePosition());
  }

  public override void DrawData(DataDrawer drawer)
  {
    if (this.targets == null)
      this.targets = new List<ReachUnitsObjective.TargetUnit>();
    this.RefreshAllUnitsDropdown();
    drawer.DrawEnum<CompleteOrder>("Complete Order", (int) this.completeOrder, (Action<int>) (v => this.completeOrder = (CompleteOrder) v));
    drawer.Space(10);
    drawer.DrawList<ReachUnitsObjective.TargetUnit>(300, this.targets, new DrawInnerData<ReachUnitsObjective.TargetUnit>(this.CreateWaypointGUI));
  }

  private void RefreshAllUnitsDropdown()
  {
    this.allUnitsDropdownList.Clear();
    this.allUnitsDropdownList.Add((SavedUnit) null);
    this.allUnitsDropdownList.AddRange(MissionManager.GetAllSavedUnits(true).Where<SavedUnit>((Func<SavedUnit, bool>) (x => !string.IsNullOrEmpty(x.UniqueName))));
    this.allUnitsNamesDropdownList.Clear();
    foreach (SavedUnit allUnitsDropdown in this.allUnitsDropdownList)
      this.allUnitsNamesDropdownList.Add(allUnitsDropdown != null ? allUnitsDropdown.UniqueName : string.Empty);
  }

  private void CreateWaypointGUI(int _, ReachUnitsObjective.TargetUnit target, DataDrawer drawer)
  {
    drawer.InstantiateWithParent<FloatDataField>(drawer.Prefabs.FloatFieldPrefab).Setup("Range", (IValueWrapper<float>) target.Range);
    int num = this.allUnitsDropdownList.IndexOf(target.Unit);
    if (num < 0)
      num = 0;
    drawer.DrawDropdown("Unit", this.allUnitsNamesDropdownList, num, (Action<int>) (i => target.Unit = this.allUnitsDropdownList[i]));
  }

  public class TargetUnit : ISaveableData
  {
    public SavedUnit Unit;
    public readonly ValueWrapperFloat Range = new ValueWrapperFloat();
    public GlobalPosition GlobalPosition;

    public void FromObjectiveData(ObjectiveData data, MissionLookups lookups)
    {
      this.Range.SetValue(data.FloatValue, (object) this, true);
      this.Unit = lookups.SavedUnits[data.StringValue];
    }

    public ObjectiveData ToObjectiveData()
    {
      return new ObjectiveData()
      {
        FloatValue = (float) (ValueWrapper<float>) this.Range,
        StringValue = this.Unit.UniqueName
      };
    }

    public ObjectivePosition ToObjectivePosition()
    {
      return new ObjectivePosition(this.GlobalPosition, new float?((float) (ValueWrapper<float>) this.Range));
    }
  }
}

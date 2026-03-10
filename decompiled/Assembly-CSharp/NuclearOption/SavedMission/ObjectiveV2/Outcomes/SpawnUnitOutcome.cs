// Decompiled with JetBrains decompiler
// Type: NuclearOption.SavedMission.ObjectiveV2.Outcomes.SpawnUnitOutcome
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.MissionEditorScripts;
using System;
using System.Collections.Generic;
using UnityEngine;

#nullable disable
namespace NuclearOption.SavedMission.ObjectiveV2.Outcomes;

internal class SpawnUnitOutcome : Outcome
{
  private static readonly List<SavedUnit> Ordered = new List<SavedUnit>();
  public List<SavedUnit> UnitsToSpawn;
  private static readonly System.Type[] TypeOrder = new System.Type[7]
  {
    typeof (SavedBuilding),
    typeof (SavedContainer),
    typeof (SavedPilot),
    typeof (SavedShip),
    typeof (SavedVehicle),
    typeof (SavedMissile),
    typeof (SavedAircraft)
  };

  protected override void WriteOutcome(ReadWriteObjective writer)
  {
    writer.ReferenceList<SavedUnit>(ref this.UnitsToSpawn);
  }

  public override void ReferenceDestroyed(ISaveableReference reference)
  {
    if (!(reference is SavedUnit savedUnit))
      return;
    this.UnitsToSpawn.Remove(savedUnit);
  }

  public override void Complete(Objective completedObjective)
  {
    Spawner i = NetworkSceneSingleton<Spawner>.i;
    SpawnUnitOutcome.Ordered.Clear();
    SpawnUnitOutcome.Ordered.AddRange((IEnumerable<SavedUnit>) this.UnitsToSpawn);
    SpawnUnitOutcome.Ordered.Sort(new System.Comparison<SavedUnit>(this.Comparison));
    foreach (SavedUnit savedUnit in SpawnUnitOutcome.Ordered)
    {
      if (!savedUnit.HasSpawned)
      {
        savedUnit.HasSpawned = true;
        i.SpawnSavedUnit(savedUnit);
      }
    }
    Physics.SyncTransforms();
    SpawnUnitOutcome.Ordered.Clear();
  }

  private int Comparison(SavedUnit x, SavedUnit y)
  {
    return Array.IndexOf<System.Type>(SpawnUnitOutcome.TypeOrder, x.GetType()) - Array.IndexOf<System.Type>(SpawnUnitOutcome.TypeOrder, y.GetType());
  }

  public override void DrawData(DataDrawer drawer)
  {
    if (this.UnitsToSpawn == null)
      this.UnitsToSpawn = new List<SavedUnit>();
    drawer.DrawList(300, this.UnitsToSpawn, false);
  }
}

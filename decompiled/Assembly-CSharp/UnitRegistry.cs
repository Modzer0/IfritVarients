// Decompiled with JetBrains decompiler
// Type: UnitRegistry
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.Networking;
using NuclearOption.SavedMission;
using System;
using System.Collections.Generic;
using UnityEngine;

#nullable disable
public static class UnitRegistry
{
  public static readonly Dictionary<PersistentID, PersistentUnit> persistentUnitLookup = new Dictionary<PersistentID, PersistentUnit>();
  public static readonly Dictionary<string, Unit> customIDLookup = new Dictionary<string, Unit>();
  public static readonly Dictionary<PlayerRef, Player> playerLookup = new Dictionary<PlayerRef, Player>();
  public static readonly List<Unit> allUnits = new List<Unit>();
  private static uint nextIndex;

  private static void Registry_OnUnitDisable(Unit unit)
  {
    UnitRegistry.allUnits.Remove(unit);
    unit.onDisableUnit -= new Action<Unit>(UnitRegistry.Registry_OnUnitDisable);
  }

  public static bool TryGetNearestUnit(
    GlobalPosition fromPosition,
    out Unit nearestUnit,
    float maxDist)
  {
    nearestUnit = (Unit) null;
    float range = maxDist;
    foreach (Unit allUnit in UnitRegistry.allUnits)
    {
      if (FastMath.InRange(allUnit.GlobalPosition(), fromPosition, range))
      {
        range = FastMath.Distance(fromPosition, allUnit.GlobalPosition());
        nearestUnit = allUnit;
      }
    }
    return (UnityEngine.Object) nearestUnit != (UnityEngine.Object) null;
  }

  public static PersistentID GetNextIndex()
  {
    ++UnitRegistry.nextIndex;
    return new PersistentID()
    {
      Id = UnitRegistry.nextIndex
    };
  }

  public static void Clear()
  {
    Debug.LogWarning((object) "UnitRegistry.Reinitialize Called");
    UnitRegistry.customIDLookup.Clear();
    UnitRegistry.persistentUnitLookup.Clear();
    UnitRegistry.playerLookup.Clear();
    UnitRegistry.allUnits.Clear();
    UnitRegistry.nextIndex = 0U;
  }

  public static void RegisterCustomID(string customID, Unit unit)
  {
    Unit unit1;
    if (UnitRegistry.customIDLookup.TryGetValue(customID, out unit1))
    {
      if ((UnityEngine.Object) unit != (UnityEngine.Object) unit1)
      {
        Debug.LogError((object) $"2 different units had the same name {customID}, unit1:{unit1}, unit2:{unit}");
      }
      else
      {
        if (unit.BuiltIn)
          return;
        Debug.LogWarning((object) $"Unit with name {customID} already registered");
      }
    }
    else
      UnitRegistry.customIDLookup.Add(customID, unit);
  }

  public static void RegisterUnit(Unit unit, PersistentID id)
  {
    if (!UnitRegistry.persistentUnitLookup.ContainsKey(id))
      UnitRegistry.persistentUnitLookup.Add(id, new PersistentUnit(unit, id));
    unit.onDisableUnit += new Action<Unit>(UnitRegistry.Registry_OnUnitDisable);
    UnitRegistry.allUnits.Add(unit);
    if (!((UnityEngine.Object) unit.NetworkHQ == (UnityEngine.Object) null))
      return;
    SceneSingleton<DynamicMap>.i.AddIcon(unit.persistentID);
  }

  public static void UnregisterUnit(Unit unit) => UnitRegistry.Registry_OnUnitDisable(unit);

  public static bool TryGetPersistentUnit(PersistentID id, out PersistentUnit persistentUnit)
  {
    return UnitRegistry.persistentUnitLookup.TryGetValue(id, out persistentUnit);
  }

  public static bool TryGetUnit<TUnit>(PersistentID? id, out TUnit unit) where TUnit : Unit
  {
    Unit unit1;
    if (UnitRegistry.TryGetUnit(id, out unit1) && unit1 is TUnit unit2)
    {
      unit = unit2;
      return true;
    }
    unit = default (TUnit);
    return false;
  }

  public static bool TryGetUnit(PersistentID? id, out Unit unit)
  {
    if (!id.HasValue)
    {
      unit = (Unit) null;
      return false;
    }
    PersistentUnit persistentUnit;
    if (!UnitRegistry.persistentUnitLookup.TryGetValue(id.Value, out persistentUnit))
    {
      unit = (Unit) null;
      return false;
    }
    if ((UnityEngine.Object) persistentUnit.unit != (UnityEngine.Object) null)
    {
      unit = persistentUnit.unit;
      return true;
    }
    unit = (Unit) null;
    return false;
  }

  public static bool TryGetPosition(SavedUnit savedUnit, out GlobalPosition pos)
  {
    Unit unit;
    if (UnitRegistry.customIDLookup.TryGetValue(savedUnit.UniqueName, out unit) && (UnityEngine.Object) unit != (UnityEngine.Object) null)
    {
      pos = unit.GlobalPosition();
      return true;
    }
    pos = new GlobalPosition();
    return false;
  }
}

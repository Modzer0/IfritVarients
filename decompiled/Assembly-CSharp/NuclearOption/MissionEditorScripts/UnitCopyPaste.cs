// Decompiled with JetBrains decompiler
// Type: NuclearOption.MissionEditorScripts.UnitCopyPaste
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.SavedMission;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#nullable disable
namespace NuclearOption.MissionEditorScripts;

public class UnitCopyPaste : SceneSingleton<UnitCopyPaste>
{
  public SavedUnit Clipboard;

  public static void CopyPaste(
    Mission mission,
    SavedUnit copyFrom,
    Unit pasteToUnit,
    SavedUnit pasteTo)
  {
    if (UnitPanel.CanHaveFaction(copyFrom) && UnitPanel.CanHaveFaction(pasteTo))
      UnitPanel.SetUnitFaction(pasteTo, copyFrom.faction);
    if (UnitPanel.CanCapture(copyFrom) && UnitPanel.CanCapture(pasteTo))
    {
      pasteTo.CaptureStrength = copyFrom.CaptureStrength;
      pasteTo.CaptureDefense = copyFrom.CaptureDefense;
    }
    if (copyFrom.type == pasteTo.type)
      UnitCopyPaste.CopyPasteInventory(mission, copyFrom, pasteTo);
    if (copyFrom is SavedAircraft savedAircraft1 && pasteTo is SavedAircraft savedAircraft2)
    {
      if (copyFrom.type == pasteTo.type)
      {
        UnitCopyPaste.CopyPasteListValue<SavedLoadout.SelectedMount>(savedAircraft1.savedLoadout.Selected, ref savedAircraft2.savedLoadout.Selected);
        savedAircraft2.livery = savedAircraft1.livery;
      }
      savedAircraft2.fuel = savedAircraft1.fuel;
      float maxSpeed = ((Aircraft) savedAircraft2.Unit).GetAircraftParameters().maxSpeed;
      savedAircraft2.startingSpeed = Mathf.Min(maxSpeed, savedAircraft1.startingSpeed);
    }
    if (copyFrom is SavedBuilding savedBuilding1 && pasteTo is SavedBuilding savedBuilding2)
    {
      Factory component1;
      Factory component2;
      if (copyFrom.Unit.TryGetComponent<Factory>(out component1) && pasteToUnit.TryGetComponent<Factory>(out component2) && component1.factoryType == component2.factoryType)
        savedBuilding2.factoryOptions = (SavedBuilding.FactoryOptions) savedBuilding1.factoryOptions.Clone();
      savedBuilding2.capturable = savedBuilding1.capturable;
      if (savedBuilding2.Airbase != savedBuilding1.Airbase)
        savedBuilding2.SetOrRemoveAirbase(savedBuilding1.AirbaseRef);
    }
    if (copyFrom is SavedVehicle savedVehicle2)
    {
      if (pasteTo is SavedVehicle savedVehicle1)
      {
        savedVehicle1.holdPosition = savedVehicle2.holdPosition;
        savedVehicle1.skill = savedVehicle2.skill;
        UnitCopyPaste.CopyPasteListClone<VehicleWaypoint>(savedVehicle2.waypoints, ref savedVehicle1.waypoints);
      }
      else if (pasteTo is SavedShip savedShip1)
      {
        savedShip1.holdPosition = savedVehicle2.holdPosition;
        savedShip1.skill = savedVehicle2.skill;
      }
    }
    if (!(copyFrom is SavedShip savedShip2))
      return;
    switch (pasteTo)
    {
      case SavedShip savedShip3:
        savedShip3.holdPosition = savedShip2.holdPosition;
        savedShip3.skill = savedShip2.skill;
        UnitCopyPaste.CopyPasteListClone<VehicleWaypoint>(savedShip2.waypoints, ref savedShip3.waypoints);
        break;
      case SavedVehicle savedVehicle3:
        savedVehicle3.holdPosition = savedShip2.holdPosition;
        savedVehicle3.skill = savedShip2.skill;
        break;
    }
  }

  private static void CopyPasteInventory(Mission mission, SavedUnit copyFrom, SavedUnit pasteTo)
  {
    UnitInventory unitInventory1 = mission.unitInventories.FirstOrDefault<UnitInventory>((Func<UnitInventory, bool>) (x => x.AttachedUnitUniqueName == copyFrom.UniqueName));
    int index = mission.unitInventories.FindIndex((Predicate<UnitInventory>) (x => x.AttachedUnitUniqueName == pasteTo.UniqueName));
    if (unitInventory1 != null)
    {
      UnitInventory unitInventory2;
      if (index >= 0)
      {
        unitInventory2 = mission.unitInventories[index];
      }
      else
      {
        unitInventory2 = new UnitInventory();
        unitInventory2.AttachToSavedUnit(pasteTo);
        mission.unitInventories.Add(unitInventory2);
      }
      UnitCopyPaste.CopyPasteListClone<StoredUnitCount>(unitInventory1.StoredList, ref unitInventory2.StoredList);
    }
    else
    {
      if (index < 0)
        return;
      mission.unitInventories.RemoveAt(index);
    }
  }

  public static void CopyPasteListClone<T>(List<T> fromList, ref List<T> toList) where T : class, ICloneable
  {
    if (toList == null)
      toList = new List<T>();
    else
      toList.Clear();
    IEnumerable<T> collection = fromList.Select<T, T>((Func<T, T>) (x => (T) x.Clone()));
    toList.AddRange(collection);
  }

  public static void CopyPasteListValue<T>(List<T> fromList, ref List<T> toList) where T : struct
  {
    if (toList == null)
      toList = new List<T>();
    else
      toList.Clear();
    toList.AddRange((IEnumerable<T>) fromList);
  }
}

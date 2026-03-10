// Decompiled with JetBrains decompiler
// Type: NuclearOption.SavedMission.UnitInventory
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;
using UnityEngine;

#nullable disable
namespace NuclearOption.SavedMission;

[Serializable]
public class UnitInventory
{
  public string AttachedUnitUniqueName;
  public List<StoredUnitCount> StoredList = new List<StoredUnitCount>();

  public UnitInventory()
  {
    this.AttachedUnitUniqueName = string.Empty;
    this.StoredList = new List<StoredUnitCount>();
  }

  public UnitInventory(Unit parentUnit)
  {
    this.AttachedUnitUniqueName = $"{parentUnit.name}[{parentUnit.persistentID}]";
    this.StoredList = new List<StoredUnitCount>();
  }

  public void AttachToSavedUnit(SavedUnit savedUnit)
  {
    if (savedUnit == null)
    {
      Debug.Log((object) "no saved unit found to link inventory with");
    }
    else
    {
      this.AttachedUnitUniqueName = savedUnit.UniqueName;
      savedUnit.OnUniqueNameChanged += new Action<string>(this.UnitInventory_OnUniqueNameChanged);
    }
  }

  public void AttachToUnit(Unit unit) => this.AttachedUnitUniqueName = unit.UniqueName;

  private void UnitInventory_OnUniqueNameChanged(string newUniqueName)
  {
    this.AttachedUnitUniqueName = newUniqueName;
  }

  public void Transfer(UnitInventory fromInventory)
  {
    foreach (StoredUnitCount stored1 in fromInventory.StoredList)
    {
      if (stored1.Count != 0)
      {
        bool flag = false;
        for (int index = this.StoredList.Count - 1; index >= 0; --index)
        {
          StoredUnitCount stored2 = this.StoredList[index];
          if (stored2.UnitType == stored1.UnitType)
          {
            flag = true;
            stored2.Count += stored1.Count;
          }
        }
        if (!flag)
          this.StoredList.Add(new StoredUnitCount(stored1.UnitType, stored1.Count));
      }
    }
    fromInventory.Clear();
  }

  public void AddOrRemove(UnitDefinition unitDefinition, int number)
  {
    if (this.StoredList == null && number > 0)
      this.StoredList = new List<StoredUnitCount>();
    bool flag = false;
    for (int index = this.StoredList.Count - 1; index >= 0; --index)
    {
      StoredUnitCount stored = this.StoredList[index];
      if (stored.UnitType == unitDefinition.unitPrefab.name)
      {
        flag = true;
        stored.Count += number;
        if (stored.Count <= 0)
        {
          this.StoredList.RemoveAt(index);
          break;
        }
        break;
      }
    }
    if (!flag && number > 0)
    {
      Debug.Log((object) $"[UnitStorage] adding entry for {number} {unitDefinition.unitName} to inventory stored list");
      this.StoredList.Add(new StoredUnitCount(unitDefinition.unitPrefab.name, number));
    }
    if (this.StoredList.Count > 0)
      return;
    MissionManager.CurrentMission.unitInventories.Remove(this);
  }

  public void Clear() => this.StoredList.Clear();
}

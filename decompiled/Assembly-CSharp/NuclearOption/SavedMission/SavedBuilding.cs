// Decompiled with JetBrains decompiler
// Type: NuclearOption.SavedMission.SavedBuilding
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;

#nullable disable
namespace NuclearOption.SavedMission;

[Serializable]
public class SavedBuilding : SavedUnit
{
  public bool capturable;
  public string Airbase;
  public SavedBuilding.FactoryOptions factoryOptions;
  [Obsolete("from v1, Use globalPosition instead")]
  public float placementOffset;

  public SavedAirbase AirbaseRef { get; private set; }

  protected override void SetOverrideDefaultValues(Unit unit)
  {
    base.SetOverrideDefaultValues(unit);
    Building building = (Building) unit;
    this.capturable = building.capturable;
    this.Airbase = (UnityEngine.Object) building.MapAirbase != (UnityEngine.Object) null ? building.MapAirbase.SavedAirbase.UniqueName : "";
    Factory component;
    if (!building.TryGetComponent<Factory>(out component))
      return;
    if (this.factoryOptions == null)
      this.factoryOptions = new SavedBuilding.FactoryOptions();
    this.factoryOptions.productionTime = component.ProductionInterval;
    UnitDefinition productionUnit = component.ProductionUnit;
    this.factoryOptions.productionType = (UnityEngine.Object) productionUnit != (UnityEngine.Object) null ? productionUnit.jsonKey : "";
  }

  public void LoadAirbaseRef()
  {
    if (string.IsNullOrEmpty(this.Airbase))
      return;
    SavedAirbase saved;
    if (MissionManager.TryFindSavedAirbase(this.Airbase, out saved))
      this.SetAirbase(saved);
    else
      Debug.LogError((object) ("Failed to find airbase with name " + this.Airbase));
  }

  public void SaveAirbaseString() => this.Airbase = this.AirbaseRef?.UniqueName;

  public void SetOrRemoveAirbase(SavedAirbase airbase)
  {
    if (airbase != null)
      this.SetAirbase(airbase);
    else
      this.RemoveAirbase(airbase);
  }

  public void SetAirbase(SavedAirbase airbase)
  {
    if (this.AirbaseRef != null)
      this.RemoveAirbase(this.AirbaseRef);
    this.AirbaseRef = airbase;
    this.Airbase = airbase.UniqueName;
    if (!airbase.BuildingsRef.Contains(this))
      airbase.BuildingsRef.Add(this);
    this.faction = airbase.faction;
    if (!((UnityEngine.Object) this.Unit != (UnityEngine.Object) null))
      return;
    this.Unit.NetworkHQ = FactionRegistry.HqFromName(this.faction);
  }

  public void RemoveAirbase(SavedAirbase hint = null)
  {
    if (this.AirbaseRef != null)
    {
      if (this.AirbaseRef.BuildingsRef.Contains(this))
        this.AirbaseRef.BuildingsRef.Remove(this);
      if (this.AirbaseRef.TowerRef == this)
      {
        this.AirbaseRef.TowerRef = (SavedBuilding) null;
        this.AirbaseRef.Tower = "";
      }
    }
    this.AirbaseRef = (SavedAirbase) null;
    this.Airbase = "";
  }

  [Serializable]
  public class FactoryOptions : ICloneable
  {
    public string productionType;
    public float productionTime = 900f;

    public FactoryOptions()
    {
      this.productionTime = 900f;
      this.productionType = string.Empty;
    }

    public object Clone()
    {
      return (object) new SavedBuilding.FactoryOptions()
      {
        productionTime = this.productionTime,
        productionType = this.productionType
      };
    }
  }
}

// Decompiled with JetBrains decompiler
// Type: Factory
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Mirage;
using Mirage.Serialization;
using NuclearOption.SavedMission;
using System;
using UnityEngine;

#nullable disable
public class Factory : NetworkBehaviour
{
  public Unit attachedUnit;
  [SyncVar]
  [SerializeField]
  public UnitDefinition productionUnit;
  [SyncVar]
  [SerializeField]
  public float productionInterval;
  [SyncVar]
  [SerializeField]
  public float lastProductionTime;
  [SerializeField]
  private bool aircraft;
  [SerializeField]
  private bool vehicles;
  [SerializeField]
  private bool weapons;
  [Obsolete("Unused")]
  [SerializeField]
  private bool warheads;
  [NonSerialized]
  private const int SYNC_VAR_COUNT = 3;
  [NonSerialized]
  private const int RPC_COUNT = 0;

  public Factory.FactoryType factoryType
  {
    get => this.weapons ? Factory.FactoryType.Nukes : Factory.FactoryType.AircraftAndVehicles;
  }

  public float ProductionInterval => this.productionInterval;

  public UnitDefinition ProductionUnit => this.productionUnit;

  public void SetFactory(SavedBuilding.FactoryOptions factoryOptions)
  {
    this.SetFactory(factoryOptions.productionType, factoryOptions.productionTime);
  }

  public void SetFactory(string productionType, float productionInterval)
  {
    if (string.IsNullOrEmpty(productionType))
      return;
    this.NetworklastProductionTime = NetworkSceneSingleton<MissionManager>.i.MissionTime;
    UnitDefinition unitDefinition;
    if (Encyclopedia.Lookup.TryGetValue(productionType, out unitDefinition))
    {
      this.NetworkproductionUnit = unitDefinition;
      this.NetworkproductionInterval = productionInterval;
      this.attachedUnit.NetworkunitName = unitDefinition.code + " factory";
      this.StartSlowUpdateDelayed(productionInterval, new Action(this.ProduceUnit));
    }
    else
    {
      if (!(productionType == "Nuclear Warhead"))
        return;
      this.warheads = true;
      this.NetworkproductionInterval = productionInterval;
      this.StartSlowUpdateDelayed(productionInterval, new Action(this.ProduceWarhead));
    }
  }

  public string GetProduction()
  {
    string production = "";
    if ((UnityEngine.Object) this.productionUnit != (UnityEngine.Object) null)
      production = this.productionUnit.code;
    else if ((double) this.productionInterval > 0.0)
      production = "Nuclear Warhead";
    return production;
  }

  public float GetInterval()
  {
    float interval = 0.0f;
    if ((UnityEngine.Object) this.productionUnit != (UnityEngine.Object) null || (double) this.productionInterval > 0.0)
      interval = this.productionInterval;
    return interval;
  }

  public float GetNextProduction(bool absolute)
  {
    float nextProduction = 0.0f;
    if ((UnityEngine.Object) this.productionUnit != (UnityEngine.Object) null || (double) this.productionInterval > 0.0)
    {
      if (absolute)
        nextProduction = (float) Mathf.RoundToInt(this.lastProductionTime + this.productionInterval - NetworkSceneSingleton<MissionManager>.i.MissionTime);
      else if ((double) this.productionInterval > 0.0)
        nextProduction = (this.lastProductionTime + this.productionInterval - NetworkSceneSingleton<MissionManager>.i.MissionTime) / this.productionInterval;
    }
    return nextProduction;
  }

  private void ProduceUnit()
  {
    if (!((UnityEngine.Object) this.attachedUnit.NetworkHQ != (UnityEngine.Object) null))
      return;
    this.attachedUnit.NetworkHQ.AddSupplyUnit(this.productionUnit, 1);
    this.NetworklastProductionTime = NetworkSceneSingleton<MissionManager>.i.MissionTime;
  }

  private void ProduceWarhead()
  {
    if (!((UnityEngine.Object) this.attachedUnit.NetworkHQ != (UnityEngine.Object) null))
      return;
    this.attachedUnit.NetworkHQ.AddWarheadStockpile(1);
  }

  private void MirageProcessed()
  {
  }

  public UnitDefinition NetworkproductionUnit
  {
    get => this.productionUnit;
    set
    {
      if (this.SyncVarEqual<UnitDefinition>(value, this.productionUnit))
        return;
      UnitDefinition productionUnit = this.productionUnit;
      this.productionUnit = value;
      this.SetDirtyBit(1UL);
    }
  }

  public float NetworkproductionInterval
  {
    get => this.productionInterval;
    set
    {
      if (this.SyncVarEqual<float>(value, this.productionInterval))
        return;
      float productionInterval = this.productionInterval;
      this.productionInterval = value;
      this.SetDirtyBit(2UL);
    }
  }

  public float NetworklastProductionTime
  {
    get => this.lastProductionTime;
    set
    {
      if (this.SyncVarEqual<float>(value, this.lastProductionTime))
        return;
      float lastProductionTime = this.lastProductionTime;
      this.lastProductionTime = value;
      this.SetDirtyBit(4UL);
    }
  }

  public override bool SerializeSyncVars(NetworkWriter writer, bool initialize)
  {
    ulong syncVarDirtyBits = this.SyncVarDirtyBits;
    bool flag = base.SerializeSyncVars(writer, initialize);
    if (initialize)
    {
      writer.WriteUnitDefinition(this.productionUnit);
      writer.WriteSingleConverter(this.productionInterval);
      writer.WriteSingleConverter(this.lastProductionTime);
      return true;
    }
    writer.Write(syncVarDirtyBits, 3);
    if (((long) syncVarDirtyBits & 1L) != 0L)
    {
      writer.WriteUnitDefinition(this.productionUnit);
      flag = true;
    }
    if (((long) syncVarDirtyBits & 2L) != 0L)
    {
      writer.WriteSingleConverter(this.productionInterval);
      flag = true;
    }
    if (((long) syncVarDirtyBits & 4L) != 0L)
    {
      writer.WriteSingleConverter(this.lastProductionTime);
      flag = true;
    }
    return flag;
  }

  public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
  {
    base.DeserializeSyncVars(reader, initialState);
    if (initialState)
    {
      this.productionUnit = reader.ReadUnitDefinition();
      this.productionInterval = reader.ReadSingleConverter();
      this.lastProductionTime = reader.ReadSingleConverter();
    }
    else
    {
      ulong dirtyBit = reader.Read(3);
      this.SetDeserializeMask(dirtyBit, 0);
      if (((long) dirtyBit & 1L) != 0L)
        this.productionUnit = reader.ReadUnitDefinition();
      if (((long) dirtyBit & 2L) != 0L)
        this.productionInterval = reader.ReadSingleConverter();
      if (((long) dirtyBit & 4L) == 0L)
        return;
      this.lastProductionTime = reader.ReadSingleConverter();
    }
  }

  protected override int GetRpcCount() => 0;

  public enum FactoryType
  {
    AircraftAndVehicles,
    Nukes,
  }
}

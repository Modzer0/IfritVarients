// Decompiled with JetBrains decompiler
// Type: MissionStatsTracker
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Mirage;
using Mirage.Collections;
using Mirage.Serialization;
using NuclearOption.Networking;
using System;

#nullable disable
public class MissionStatsTracker : NetworkBehaviour
{
  public FactionHQ hq;
  public readonly SyncDictionary<UnitDefinition, int> currentUnits = new SyncDictionary<UnitDefinition, int>();
  public readonly SyncDictionary<UnitDefinition, int> lostUnits = new SyncDictionary<UnitDefinition, int>();
  [SyncVar]
  public MissionStatsTracker.TypeStat manpower;
  [SyncVar]
  public MissionStatsTracker.TypeStat value;
  [SyncVar]
  public MissionStatsTracker.TypeStat units;
  [NonSerialized]
  private const int SYNC_VAR_COUNT = 3;
  [NonSerialized]
  private const int RPC_COUNT = 0;

  public void Start()
  {
    this.hq.onRemoveUnit += new Action<Unit>(this.LostUnit);
    this.hq.onRegisterUnit += new Action<Unit>(this.NewUnit);
  }

  [Mirage.Server]
  public void LostUnit(Unit unit)
  {
    if (!this.IsServer)
      throw new MethodInvocationException("[Server] function 'LostUnit' called when server not active");
    if (!NetworkManagerNuclearOption.i.Server.Active || !unit.disabled)
      return;
    int num1;
    if (this.currentUnits.TryGetValue(unit.definition, out num1))
    {
      int num2 = num1 - 1;
      this.currentUnits[unit.definition] = num2 > 0 ? num2 : 0;
    }
    if (unit.unitState != Unit.UnitState.Returned)
    {
      int num3;
      if (this.lostUnits.TryGetValue(unit.definition, out num3))
      {
        int num4 = num3 + 1;
        this.lostUnits[unit.definition] = num4;
      }
      else
        this.lostUnits.Add(unit.definition, 1);
      if (!(unit is Missile))
      {
        this.value.total.current -= unit.definition.value;
        this.value.total.lost += unit.definition.value;
        if (!(unit is Aircraft))
        {
          this.manpower.total.current -= unit.definition.manpower;
          this.manpower.total.lost += unit.definition.manpower;
        }
      }
    }
    if (unit is Building)
    {
      --this.units.buildings.current;
      ++this.units.buildings.lost;
      this.value.buildings.current -= unit.definition.value;
      this.value.buildings.lost += unit.definition.value;
      this.manpower.buildings.current -= unit.definition.manpower;
      this.manpower.buildings.lost += unit.definition.manpower;
    }
    else if (unit is Ship)
    {
      --this.units.ships.current;
      ++this.units.ships.lost;
      this.value.ships.current -= unit.definition.value;
      this.value.ships.lost += unit.definition.value;
      this.manpower.ships.current -= unit.definition.manpower;
      this.manpower.ships.lost += unit.definition.manpower;
    }
    else if (unit is GroundVehicle)
    {
      --this.units.vehicles.current;
      ++this.units.vehicles.lost;
      this.value.vehicles.current -= unit.definition.value;
      this.value.vehicles.lost += unit.definition.value;
      this.manpower.vehicles.current -= unit.definition.manpower;
      this.manpower.vehicles.lost += unit.definition.manpower;
    }
    else if (unit is Aircraft aircraft)
    {
      --this.units.aircraft.current;
      this.value.aircraft.current -= unit.definition.value;
      if (aircraft.unitState != Unit.UnitState.Returned)
      {
        ++this.units.aircraft.lost;
        this.value.aircraft.lost += unit.definition.value;
      }
      foreach (Pilot pilot in aircraft.pilots)
      {
        if (pilot.dead)
        {
          --this.manpower.total.current;
          ++this.manpower.total.lost;
          --this.manpower.aircraft.current;
          ++this.manpower.aircraft.lost;
        }
      }
    }
    else
    {
      if (!(unit is PilotDismounted))
        return;
      --this.manpower.aircraft.current;
      ++this.manpower.aircraft.lost;
    }
  }

  [Mirage.Server]
  public void NewUnit(Unit unit)
  {
    if (!this.IsServer)
      throw new MethodInvocationException("[Server] function 'NewUnit' called when server not active");
    if (!NetworkManagerNuclearOption.i.Server.Active)
      return;
    int num1;
    if (this.currentUnits.TryGetValue(unit.definition, out num1))
    {
      int num2 = num1 + 1;
      this.currentUnits[unit.definition] = num2;
    }
    else
      this.currentUnits.Add(unit.definition, 1);
    if (!(unit is Missile))
    {
      this.value.total.total += unit.definition.value;
      this.value.total.current += unit.definition.value;
      this.manpower.total.total += unit.definition.manpower;
      this.manpower.total.current += unit.definition.manpower;
    }
    if (unit is Building)
    {
      ++this.units.buildings.total;
      ++this.units.buildings.current;
      this.value.buildings.total += unit.definition.value;
      this.value.buildings.current += unit.definition.value;
      this.manpower.buildings.total += unit.definition.manpower;
      this.manpower.buildings.current += unit.definition.manpower;
    }
    else if (unit is Ship)
    {
      ++this.units.ships.total;
      ++this.units.ships.current;
      this.value.ships.total += unit.definition.value;
      this.value.ships.current += unit.definition.value;
      this.manpower.ships.total += unit.definition.manpower;
      this.manpower.ships.current += unit.definition.manpower;
    }
    else if (unit is GroundVehicle)
    {
      ++this.units.vehicles.total;
      ++this.units.vehicles.current;
      this.value.vehicles.total += unit.definition.value;
      this.value.vehicles.current += unit.definition.value;
      this.manpower.vehicles.total += unit.definition.manpower;
      this.manpower.vehicles.current += unit.definition.manpower;
    }
    else
    {
      if (!(unit is Aircraft))
        return;
      ++this.units.aircraft.total;
      ++this.units.aircraft.current;
      this.value.aircraft.total += unit.definition.value;
      this.value.aircraft.current += unit.definition.value;
      this.manpower.aircraft.total += unit.definition.manpower;
      this.manpower.aircraft.current += unit.definition.manpower;
    }
  }

  [Mirage.Server]
  public void MunitionCost(Unit unit, float cost)
  {
    if (!this.IsServer)
      throw new MethodInvocationException("[Server] function 'MunitionCost' called when server not active");
    this.value.total.total += cost;
    this.value.total.spent += cost;
    switch (unit)
    {
      case Ship _:
        this.value.ships.total += cost;
        this.value.ships.spent += cost;
        break;
      case Building _:
        this.value.buildings.total += cost;
        this.value.buildings.spent += cost;
        break;
      case GroundVehicle _:
        this.value.vehicles.total += cost;
        this.value.vehicles.spent += cost;
        break;
      case Aircraft _:
        this.value.aircraft.total += cost;
        this.value.aircraft.spent += cost;
        break;
    }
  }

  public int GetLostUnits(UnitDefinition definition)
  {
    int num;
    return this.lostUnits.TryGetValue(definition, out num) ? num : 0;
  }

  public int GetCurrentUnits(UnitDefinition definition)
  {
    int num;
    return this.currentUnits.TryGetValue(definition, out num) ? num : 0;
  }

  public MissionStatsTracker()
  {
    this.InitSyncObject((ISyncObject) this.currentUnits);
    this.InitSyncObject((ISyncObject) this.lostUnits);
  }

  private void MirageProcessed()
  {
  }

  public MissionStatsTracker.TypeStat Networkmanpower
  {
    get => this.manpower;
    set
    {
      if (this.SyncVarEqual<MissionStatsTracker.TypeStat>(value, this.manpower))
        return;
      MissionStatsTracker.TypeStat manpower = this.manpower;
      this.manpower = value;
      this.SetDirtyBit(1UL);
    }
  }

  public MissionStatsTracker.TypeStat Networkvalue
  {
    get => this.value;
    set
    {
      if (this.SyncVarEqual<MissionStatsTracker.TypeStat>(value, this.value))
        return;
      MissionStatsTracker.TypeStat typeStat = this.value;
      this.value = value;
      this.SetDirtyBit(2UL);
    }
  }

  public MissionStatsTracker.TypeStat Networkunits
  {
    get => this.units;
    set
    {
      if (this.SyncVarEqual<MissionStatsTracker.TypeStat>(value, this.units))
        return;
      MissionStatsTracker.TypeStat units = this.units;
      this.units = value;
      this.SetDirtyBit(4UL);
    }
  }

  public override bool SerializeSyncVars(NetworkWriter writer, bool initialize)
  {
    ulong syncVarDirtyBits = this.SyncVarDirtyBits;
    bool flag = base.SerializeSyncVars(writer, initialize);
    if (initialize)
    {
      GeneratedNetworkCode._Write_MissionStatsTracker\u002FTypeStat(writer, this.manpower);
      GeneratedNetworkCode._Write_MissionStatsTracker\u002FTypeStat(writer, this.value);
      GeneratedNetworkCode._Write_MissionStatsTracker\u002FTypeStat(writer, this.units);
      return true;
    }
    writer.Write(syncVarDirtyBits, 3);
    if (((long) syncVarDirtyBits & 1L) != 0L)
    {
      GeneratedNetworkCode._Write_MissionStatsTracker\u002FTypeStat(writer, this.manpower);
      flag = true;
    }
    if (((long) syncVarDirtyBits & 2L) != 0L)
    {
      GeneratedNetworkCode._Write_MissionStatsTracker\u002FTypeStat(writer, this.value);
      flag = true;
    }
    if (((long) syncVarDirtyBits & 4L) != 0L)
    {
      GeneratedNetworkCode._Write_MissionStatsTracker\u002FTypeStat(writer, this.units);
      flag = true;
    }
    return flag;
  }

  public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
  {
    base.DeserializeSyncVars(reader, initialState);
    if (initialState)
    {
      this.manpower = GeneratedNetworkCode._Read_MissionStatsTracker\u002FTypeStat(reader);
      this.value = GeneratedNetworkCode._Read_MissionStatsTracker\u002FTypeStat(reader);
      this.units = GeneratedNetworkCode._Read_MissionStatsTracker\u002FTypeStat(reader);
    }
    else
    {
      ulong dirtyBit = reader.Read(3);
      this.SetDeserializeMask(dirtyBit, 0);
      if (((long) dirtyBit & 1L) != 0L)
        this.manpower = GeneratedNetworkCode._Read_MissionStatsTracker\u002FTypeStat(reader);
      if (((long) dirtyBit & 2L) != 0L)
        this.value = GeneratedNetworkCode._Read_MissionStatsTracker\u002FTypeStat(reader);
      if (((long) dirtyBit & 4L) == 0L)
        return;
      this.units = GeneratedNetworkCode._Read_MissionStatsTracker\u002FTypeStat(reader);
    }
  }

  protected override int GetRpcCount() => 0;

  public struct Stat
  {
    public float total;
    public float current;
    public float spent;
    public float lost;
    public static readonly MissionStatsTracker.Stat Default = new MissionStatsTracker.Stat()
    {
      total = 0.0f,
      current = 0.0f,
      spent = 0.0f,
      lost = 0.0f
    };
  }

  public struct TypeStat
  {
    public MissionStatsTracker.Stat total;
    public MissionStatsTracker.Stat buildings;
    public MissionStatsTracker.Stat ships;
    public MissionStatsTracker.Stat vehicles;
    public MissionStatsTracker.Stat aircraft;
  }
}

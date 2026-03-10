// Decompiled with JetBrains decompiler
// Type: DefinitionWriters
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Mirage.Serialization;
using System;
using UnityEngine;

#nullable disable
public static class DefinitionWriters
{
  public static void WriteNetworkDefinition(this NetworkWriter writer, INetworkDefinition item)
  {
    uint num = item != null ? checked ((uint) (DefinitionWriters.GetIndex(item) + 1)) : 0U;
    writer.WritePackedUInt32(num);
  }

  private static int GetIndex(INetworkDefinition item)
  {
    if (item.LookupIndex.HasValue)
      return item.LookupIndex.Value;
    Debug.LogWarning((object) "Index was not assigned to LookupIndex in Encyclopedia.AfterLoad");
    int index = Encyclopedia.i.IndexLookup.IndexOf(item);
    item.LookupIndex = index != -1 ? new int?(index) : throw new Exception($"Trying to write {item} but it was not in Encyclopedia lookup");
    return index;
  }

  public static T ReadNetworkDefinition<T>(this NetworkReader reader) where T : ScriptableObject, INetworkDefinition
  {
    uint num = reader.ReadPackedUInt32();
    return num == 0U ? default (T) : (T) Encyclopedia.i.IndexLookup[checked ((int) num - 1)];
  }

  public static void WriteUnitDefinition(this NetworkWriter writer, UnitDefinition definition)
  {
    writer.WriteNetworkDefinition((INetworkDefinition) definition);
  }

  public static void WriteAircraftDefinition(
    this NetworkWriter writer,
    AircraftDefinition definition)
  {
    writer.WriteNetworkDefinition((INetworkDefinition) definition);
  }

  public static void WriteVehicleDefinition(this NetworkWriter writer, VehicleDefinition definition)
  {
    writer.WriteNetworkDefinition((INetworkDefinition) definition);
  }

  public static void WriteMissileDefinition(this NetworkWriter writer, MissileDefinition definition)
  {
    writer.WriteNetworkDefinition((INetworkDefinition) definition);
  }

  public static void WriteBuildingDefinition(
    this NetworkWriter writer,
    BuildingDefinition definition)
  {
    writer.WriteNetworkDefinition((INetworkDefinition) definition);
  }

  public static void WriteShipDefinition(this NetworkWriter writer, ShipDefinition definition)
  {
    writer.WriteNetworkDefinition((INetworkDefinition) definition);
  }

  public static void WriteWeaponMount(this NetworkWriter writer, WeaponMount definition)
  {
    writer.WriteNetworkDefinition((INetworkDefinition) definition);
  }

  public static UnitDefinition ReadUnitDefinition(this NetworkReader reader)
  {
    return reader.ReadNetworkDefinition<UnitDefinition>();
  }

  public static AircraftDefinition ReadAircraftDefinition(this NetworkReader reader)
  {
    return reader.ReadNetworkDefinition<AircraftDefinition>();
  }

  public static VehicleDefinition ReadVehicleDefinition(this NetworkReader reader)
  {
    return reader.ReadNetworkDefinition<VehicleDefinition>();
  }

  public static MissileDefinition ReadMissileDefinition(this NetworkReader reader)
  {
    return reader.ReadNetworkDefinition<MissileDefinition>();
  }

  public static BuildingDefinition ReadBuildingDefinition(this NetworkReader reader)
  {
    return reader.ReadNetworkDefinition<BuildingDefinition>();
  }

  public static ShipDefinition ReadShipDefinition(this NetworkReader reader)
  {
    return reader.ReadNetworkDefinition<ShipDefinition>();
  }

  public static WeaponMount ReadWeaponMount(this NetworkReader reader)
  {
    return reader.ReadNetworkDefinition<WeaponMount>();
  }
}

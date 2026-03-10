// Decompiled with JetBrains decompiler
// Type: qol.FieldModification.Configs.FieldModificationConfigs
// Assembly: qol, Version=1.1.6.3, Culture=neutral, PublicKeyToken=null
// MVID: 3B6539EA-E38C-45EE-8FFF-FC58CC42BADC
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\BepInEx\plugins\qol_1.1.6.3b1.dll

using System;

#nullable disable
namespace qol.FieldModification.Configs;

public static class FieldModificationConfigs
{
  public static readonly Type[] DefinitionTypes = new Type[5]
  {
    typeof (MissileDefinition),
    typeof (WeaponInfo),
    typeof (VehicleDefinition),
    typeof (AircraftDefinition),
    typeof (BuildingDefinition)
  };
}

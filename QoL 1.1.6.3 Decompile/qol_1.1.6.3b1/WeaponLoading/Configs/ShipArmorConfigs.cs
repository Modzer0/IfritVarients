// Decompiled with JetBrains decompiler
// Type: qol.WeaponLoading.Configs.ShipArmorConfigs
// Assembly: qol, Version=1.1.6.3, Culture=neutral, PublicKeyToken=null
// MVID: 3B6539EA-E38C-45EE-8FFF-FC58CC42BADC
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\BepInEx\plugins\qol_1.1.6.3b1.dll

using System.Collections.Generic;

#nullable disable
namespace qol.WeaponLoading.Configs;

public static class ShipArmorConfigs
{
  public static readonly ShipArmorConfigs.ArmorDef RadarArmor = new ShipArmorConfigs.ArmorDef(0.0f, 0.0f, 0.0f, 5f, 2.5f, 10f);
  public static readonly ShipArmorConfigs.ArmorDef TurretArmor = new ShipArmorConfigs.ArmorDef(0.0f, 0.0f, 0.0f, 10f, 5f, 20f);
  public static readonly ShipArmorConfigs.ArmorDef StoresArmor = new ShipArmorConfigs.ArmorDef(250f, 500f, 0.0f, 5f, 20f, 10f);
  public static readonly ShipArmorConfigs.ArmorDef HullArmor = new ShipArmorConfigs.ArmorDef(250f, 250f, 20f, 1000f, 10f, 20f);
  public static readonly ShipArmorConfigs.ArmorDef WeakHullArmor = new ShipArmorConfigs.ArmorDef(250f, 100f, 0.0f, 250f, 5f, 10f);
  public static readonly IReadOnlyList<string> ShipPrefabs = (IReadOnlyList<string>) new List<string>()
  {
    "AssaultCarrier1",
    "Corvette1",
    "Destroyer1",
    "FleetCarrier1"
  };
  public static readonly IReadOnlyList<string> RadarPaths = (IReadOnlyList<string>) new List<string>()
  {
    "Corvette1/Radar",
    "AssaultCarrier1/tower_R1/tower_R2/Radar",
    "FleetCarrier1/hull_FR/bridge_F/Radar"
  };
  public static readonly IReadOnlyList<string> WeaponStoresPaths = (IReadOnlyList<string>) new List<string>()
  {
    "Corvette1/bow1/magazine",
    "AssaultCarrier1/weaponStores2",
    "AssaultCarrier1/hull_L/hull_FR/hull_FL/weaponStores1",
    "Destroyer1/Hull_CF/Hull_CFF/weaponStores2",
    "Destroyer1/Hull_CR/Hull_CruiseMissileBattery3/weaponStores1",
    "FleetCarrier1/hull_F/weaponStores1",
    "FleetCarrier1/hull_R/weaponStores2"
  };

  public struct ArmorDef(
    float pierceArmor,
    float blastArmor,
    float fireArmor,
    float pierceTolerance,
    float blastTolerance,
    float fireTolerance)
  {
    public float PierceArmor = pierceArmor;
    public float BlastArmor = blastArmor;
    public float FireArmor = fireArmor;
    public float PierceTolerance = pierceTolerance;
    public float BlastTolerance = blastTolerance;
    public float FireTolerance = fireTolerance;
  }
}

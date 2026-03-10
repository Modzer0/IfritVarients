// Decompiled with JetBrains decompiler
// Type: qol.WeaponLoading.Configs.AircraftLoadoutConfigs
// Assembly: qol, Version=1.1.6.3, Culture=neutral, PublicKeyToken=null
// MVID: 3B6539EA-E38C-45EE-8FFF-FC58CC42BADC
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\BepInEx\plugins\qol_1.1.6.3b1.dll

using System.Collections.Generic;

#nullable disable
namespace qol.WeaponLoading.Configs;

public static class AircraftLoadoutConfigs
{
  public static readonly IReadOnlyList<AircraftLoadoutConfigs.AircraftLoadoutDef> AircraftLoadouts = (IReadOnlyList<AircraftLoadoutConfigs.AircraftLoadoutDef>) new List<AircraftLoadoutConfigs.AircraftLoadoutDef>()
  {
    new AircraftLoadoutConfigs.AircraftLoadoutDef("COINParameters", false, (IReadOnlyList<AircraftLoadoutConfigs.LoadoutDef>) new List<AircraftLoadoutConfigs.LoadoutDef>()
    {
      new AircraftLoadoutConfigs.LoadoutDef("default", 0.3f, new string[5]
      {
        "gun_12.7mm_internal",
        "RocketPod1_triple_db",
        "AGM1_triple",
        "IRMS1_double",
        "ECMPod1"
      }),
      new AircraftLoadoutConfigs.LoadoutDef("ground1", 0.3f, new string[5]
      {
        "gun_12.7mm_internal",
        "AGM1_triple",
        "AGM_heavy_single",
        "Rocket2_4Pod",
        "ECMPod1"
      }),
      new AircraftLoadoutConfigs.LoadoutDef("ground2", 0.3f, new string[5]
      {
        "gun_12.7mm_internal",
        "Rocket2_4Podx3_db",
        "AGM1_triple",
        "AAM3_single",
        "ECMPod1"
      }),
      new AircraftLoadoutConfigs.LoadoutDef("ground3", 0.3f, new string[5]
      {
        "gun_12.7mm_internal",
        "bomb_125_double",
        "AGM_heavy_single",
        "ARM1_mini_single",
        "ECMPod1"
      })
    }, (IReadOnlyList<string>) new List<string>()
    {
      "default",
      "default",
      "ground1",
      "ground1",
      "ground2",
      "ground3"
    }),
    new AircraftLoadoutConfigs.AircraftLoadoutDef("trainerParameters", false, (IReadOnlyList<AircraftLoadoutConfigs.LoadoutDef>) new List<AircraftLoadoutConfigs.LoadoutDef>()
    {
      new AircraftLoadoutConfigs.LoadoutDef("default", 0.5f, new string[5]
      {
        "gun_25mm_trainer_pod",
        "AGM_heavy_double",
        "AGM1_triple",
        "AAM1_single",
        "TailHook_Trainer"
      }),
      new AircraftLoadoutConfigs.LoadoutDef("ground1", 0.5f, new string[5]
      {
        "gun_25mm_trainer_pod",
        "bomb_250_triple",
        "AGM1_triple",
        "RocketPod1_single",
        "TailHook_Trainer"
      }),
      new AircraftLoadoutConfigs.LoadoutDef("ground2", 0.5f, new string[5]
      {
        "AGM_heavy_internalx2",
        "bomb_glide1_triple",
        "Rocket2_4Pod",
        "IRMS1_double",
        "TailHook_Trainer"
      }),
      new AircraftLoadoutConfigs.LoadoutDef("ground3", 0.5f, new string[5]
      {
        "AAM3_triple_internal",
        "Rocket2_4Podx3_db",
        "AGM_heavy_single",
        "ARM1_mini_single",
        "TailHook_Trainer"
      }),
      new AircraftLoadoutConfigs.LoadoutDef("ground4", 0.5f, new string[5]
      {
        "bomb_500_internal",
        "bomb_500_double",
        "RocketPod1_single",
        "AAM1_single",
        "TailHook_Trainer"
      }),
      new AircraftLoadoutConfigs.LoadoutDef("air1", 0.5f, new string[5]
      {
        "AAM2_double_internal",
        "AAM2_single",
        "AAM1_single",
        "AAM1_single",
        "TailHook_Trainer"
      }),
      new AircraftLoadoutConfigs.LoadoutDef("air2", 0.5f, new string[5]
      {
        "gun_25mm_trainer_pod",
        "AAM2_single",
        "AAM1_single",
        "IRMS1_double",
        "TailHook_Trainer"
      })
    }, (IReadOnlyList<string>) new List<string>()
    {
      "default",
      "default",
      "ground1",
      "ground1",
      "ground2",
      "ground2",
      "ground3",
      "ground4",
      "air1",
      "air2"
    }),
    new AircraftLoadoutConfigs.AircraftLoadoutDef("UtilityHelo1_Parameters", true, (IReadOnlyList<AircraftLoadoutConfigs.LoadoutDef>) new List<AircraftLoadoutConfigs.LoadoutDef>()
    {
      new AircraftLoadoutConfigs.LoadoutDef("gunship", 0.6f, new string[6]
      {
        "turret_25mm_flexpod",
        "turret_12.7mm_door",
        null,
        "AGM1_rotaryLauncher_UtilityHelo1",
        "UGV1_grenadex1",
        "ECMPod1"
      }),
      new AircraftLoadoutConfigs.LoadoutDef("cargo1", 0.4f, new string[6]
      {
        "AGM1_quad",
        "turret_40mm_grenade",
        null,
        "UGV1_SAMx1",
        "UGV1_grenadex1",
        "ECMPod1"
      }),
      new AircraftLoadoutConfigs.LoadoutDef("cargo2", 0.5f, new string[6]
      {
        "IRMS1_triple",
        "turret_12.7mm_door",
        "MunitionsPallet1",
        null,
        null,
        "ECMPod1"
      }),
      new AircraftLoadoutConfigs.LoadoutDef("cargo3", 0.5f, new string[6]
      {
        "IRMS1_triple",
        "turret_12.7mm_door",
        "NavalPallet1",
        null,
        null,
        "ECMPod1"
      })
    }, (IReadOnlyList<string>) new List<string>()
    {
      "gunship",
      "gunship",
      "gunship",
      "cargo1",
      "cargo1",
      "cargo2",
      "cargo3"
    }),
    new AircraftLoadoutConfigs.AircraftLoadoutDef("ChicaneParameters", false, (IReadOnlyList<AircraftLoadoutConfigs.LoadoutDef>) new List<AircraftLoadoutConfigs.LoadoutDef>(), (IReadOnlyList<string>) new List<string>()),
    new AircraftLoadoutConfigs.AircraftLoadoutDef("F1Parameters", false, (IReadOnlyList<AircraftLoadoutConfigs.LoadoutDef>) new List<AircraftLoadoutConfigs.LoadoutDef>()
    {
      new AircraftLoadoutConfigs.LoadoutDef("a2a1", 0.75f, new string[5]
      {
        "gun_20mm_internal",
        "AAM2_double_internal",
        "AAM2_double",
        "AAM1_single",
        "AAM1_single"
      }),
      new AircraftLoadoutConfigs.LoadoutDef("a2a2", 0.75f, new string[5]
      {
        "gun_20mm_internal",
        "AAM3_triple_internal_flat",
        "AAM4_double",
        "AAM4_single",
        "AAM2_single"
      }),
      new AircraftLoadoutConfigs.LoadoutDef("a2g1", 0.75f, new string[5]
      {
        "gun_20mm_internal",
        "AAM2_double_internal",
        "AGM_heavy_triple",
        "Rocket2_4Pod",
        "AAM1_single"
      })
    }, (IReadOnlyList<string>) new List<string>()),
    new AircraftLoadoutConfigs.AircraftLoadoutDef("VortexParameters", false, (IReadOnlyList<AircraftLoadoutConfigs.LoadoutDef>) new List<AircraftLoadoutConfigs.LoadoutDef>(), (IReadOnlyList<string>) new List<string>()),
    new AircraftLoadoutConfigs.AircraftLoadoutDef("IfritParameters", false, (IReadOnlyList<AircraftLoadoutConfigs.LoadoutDef>) new List<AircraftLoadoutConfigs.LoadoutDef>(), (IReadOnlyList<string>) new List<string>()),
    new AircraftLoadoutConfigs.AircraftLoadoutDef("QuadVTOL1_Parameters", true, (IReadOnlyList<AircraftLoadoutConfigs.LoadoutDef>) new List<AircraftLoadoutConfigs.LoadoutDef>()
    {
      new AircraftLoadoutConfigs.LoadoutDef("gunship", 0.5f, new string[6]
      {
        null,
        "turret_76mm_SideMount",
        "AGM1_floorLauncher_QuadVTOL1",
        "turret_30mm_chaingun",
        "IRMS1_triple",
        null
      }),
      new AircraftLoadoutConfigs.LoadoutDef("vehicle1", 0.5f, new string[6]
      {
        "LightTruck1x2",
        null,
        null,
        "turret_12.7mm_rotary",
        "Rocket2_4Podx3",
        null
      }),
      new AircraftLoadoutConfigs.LoadoutDef("vehicle2", 0.5f, new string[6]
      {
        "UGV1_mixed_x6",
        null,
        null,
        "turret_12.7mm_rotary",
        "RocketPod1_triple",
        null
      }),
      new AircraftLoadoutConfigs.LoadoutDef("vehicle3", 0.5f, new string[6]
      {
        "6x6_1_AAx1",
        null,
        null,
        "turret_12.7mm_rotary",
        "AGM1_quad",
        null
      }),
      new AircraftLoadoutConfigs.LoadoutDef("vehicle4", 0.5f, new string[6]
      {
        "6x6_1_ATx1",
        null,
        null,
        "turret_12.7mm_rotary",
        "IRMS1_triple",
        null
      }),
      new AircraftLoadoutConfigs.LoadoutDef("cargo1", 0.5f, new string[6]
      {
        "MunitionsContainerx2",
        null,
        null,
        null,
        null,
        null
      }),
      new AircraftLoadoutConfigs.LoadoutDef("cargo2", 0.5f, new string[6]
      {
        "NavalSupplyContainerx2",
        null,
        null,
        null,
        null,
        null
      })
    }, (IReadOnlyList<string>) new List<string>()
    {
      "gunship",
      "gunship",
      "vehicle1",
      "vehicle1",
      "vehicle2",
      "vehicle3",
      "vehicle4",
      "cargo1",
      "cargo2"
    }),
    new AircraftLoadoutConfigs.AircraftLoadoutDef("EW1Parameters", true, (IReadOnlyList<AircraftLoadoutConfigs.LoadoutDef>) new List<AircraftLoadoutConfigs.LoadoutDef>()
    {
      new AircraftLoadoutConfigs.LoadoutDef("awacs", 0.8f, new string[5]
      {
        "Laser_EW1",
        "JammingPod1",
        "Radome1",
        "JammingPod1",
        "JammingPod1"
      }),
      new AircraftLoadoutConfigs.LoadoutDef("sead1", 0.4f, new string[5]
      {
        "Laser_EW1",
        "ARM1_internalx2",
        null,
        "JammingPod1",
        "ARM1_double"
      }),
      new AircraftLoadoutConfigs.LoadoutDef("sead2", 0.4f, new string[5]
      {
        "Laser_EW1",
        "JammingPod1",
        null,
        "bomb_glide1_triple",
        "AGM_heavy_double"
      }),
      new AircraftLoadoutConfigs.LoadoutDef("sead3", 0.4f, new string[5]
      {
        "Laser_EW1",
        "ARM1_internalx2",
        null,
        "JammingPod1",
        "bomb_glide1_triple"
      }),
      new AircraftLoadoutConfigs.LoadoutDef("standoff", 0.4f, new string[5]
      {
        null,
        "AShM1_internal",
        null,
        "AShM2_double",
        "AShM1_single"
      })
    }, (IReadOnlyList<string>) new List<string>()
    {
      "awacs",
      "sead1",
      "sead1",
      "sead1",
      "sead2",
      "sead3",
      "sead3",
      "standoff",
      "standoff"
    }),
    new AircraftLoadoutConfigs.AircraftLoadoutDef("SFBParameters", false, (IReadOnlyList<AircraftLoadoutConfigs.LoadoutDef>) new List<AircraftLoadoutConfigs.LoadoutDef>()
    {
      new AircraftLoadoutConfigs.LoadoutDef("load1", 0.4f, new string[5]
      {
        null,
        "AGM_heavy_internalx8",
        "bomb_500_internalx6",
        "bomb_penetrator1_internalx3",
        "JammingPod1"
      }),
      new AircraftLoadoutConfigs.LoadoutDef("load2", 0.4f, new string[5]
      {
        null,
        "CruiseMissile1_internalx4",
        "AShM1_internalx4_stack",
        "AShM3_internalx4_stack",
        "P_PassiveJammer1"
      }),
      new AircraftLoadoutConfigs.LoadoutDef("load3", 0.4f, new string[5]
      {
        null,
        "AShM3_internalx4_stack",
        "AGM_heavy_internalx8",
        "bomb_250_internalx18",
        "JammingPod1"
      }),
      new AircraftLoadoutConfigs.LoadoutDef("load5", 0.4f, new string[5]
      {
        "BallisticMissile1_internalx2",
        null,
        null,
        "AShM1_internalx4_flat",
        "P_PassiveJammer1"
      }),
      new AircraftLoadoutConfigs.LoadoutDef("load6", 0.4f, new string[5]
      {
        "BallisticMissile1_tacNuke_internalx2",
        null,
        null,
        "CruiseMissile20kt_internalx2",
        "P_PassiveJammer1"
      })
    }, (IReadOnlyList<string>) new List<string>()
    {
      "load1",
      "load1",
      "load2",
      "load2",
      "load2",
      "load2",
      "load2",
      "load3",
      "load3",
      "load5",
      "load5",
      "load6"
    })
  };

  public struct LoadoutDef(string id, float fuelRatio, params string[] weapons)
  {
    public string Id = id;
    public float FuelRatio = fuelRatio;
    public string[] Weapons = weapons;
  }

  public struct AircraftLoadoutDef(
    string parametersName,
    bool enabled,
    IReadOnlyList<AircraftLoadoutConfigs.LoadoutDef> loadouts,
    IReadOnlyList<string> slotAssignments)
  {
    public string ParametersName = parametersName;
    public bool Enabled = enabled;
    public IReadOnlyList<AircraftLoadoutConfigs.LoadoutDef> Loadouts = loadouts;
    public IReadOnlyList<string> SlotAssignments = slotAssignments;
  }
}

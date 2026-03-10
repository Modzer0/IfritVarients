// Decompiled with JetBrains decompiler
// Type: qol.WeaponLoading.Configs.WeaponVariantConfigs
// Assembly: qol, Version=1.1.6.3, Culture=neutral, PublicKeyToken=null
// MVID: 3B6539EA-E38C-45EE-8FFF-FC58CC42BADC
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\BepInEx\plugins\qol_1.1.6.3b1.dll

#nullable disable
namespace qol.WeaponLoading.Configs;

public static class WeaponVariantConfigs
{
  public static readonly WeaponVariantDef[] Variants = new WeaponVariantDef[46]
  {
    new WeaponVariantDef()
    {
      VariantId = "ARM1_mini",
      Type = VariantType.Missile,
      Enabled = true,
      SourcePrefab = "ARM1",
      SourceInfo = "ARM1_info",
      SourceDefinition = "ARM1",
      NewPrefabName = "ARM1_mini",
      NewInfoName = "ARM1_mini_info",
      Mounts = new MountDef[3]
      {
        MountDef.Simple("ARM1_single", "ARM1_mini_single"),
        MountDef.Simple("ARM1_double", "ARM1_mini_double"),
        MountDef.Simple("ARM1_internalx2", "ARM1_mini_internalx2")
      }
    },
    new WeaponVariantDef()
    {
      VariantId = "bomb_demo_mini",
      Type = VariantType.Missile,
      Enabled = true,
      SourcePrefab = "bomb_demolition",
      SourceInfo = "info_bomb_demolition",
      SourceDefinition = "Bomb_demolition",
      NewPrefabName = "bomb_demo_mini",
      NewInfoName = "bomb_demo_mini_info",
      Mounts = new MountDef[2]
      {
        MountDef.Simple("bomb_demolition_internal", "bomb_demo_mini_single"),
        MountDef.WithMeshSwap("AShM3_internalx4_stack", "bomb_demo_mini_internalx4", "ashm3", 0.5f)
      }
    },
    new WeaponVariantDef()
    {
      VariantId = "Rocket2_rls",
      Type = VariantType.Missile,
      Enabled = true,
      SourcePrefab = "Rocket2",
      SourceInfo = "info_rocket2",
      SourceDefinition = "Rocket2",
      NewPrefabName = "Rocket2_rls",
      NewInfoName = "Rocket2_rls_info",
      PostProcessorId = "Rocket2_rls_Particles",
      Mounts = (MountDef[]) null
    },
    new WeaponVariantDef()
    {
      VariantId = "bomb_125_fire",
      Type = VariantType.Missile,
      Enabled = true,
      SourcePrefab = "bomb_125_1",
      SourceInfo = "info_bomb_125_1",
      SourceDefinition = "bomb_125_1",
      NewPrefabName = "bomb_125_fire",
      NewInfoName = "bomb_125_fire_info",
      Mounts = new MountDef[1]
      {
        MountDef.Simple("bomb_125_single", "bomb_125_fire_single")
      }
    },
    new WeaponVariantDef()
    {
      VariantId = "P_GLB1",
      Type = VariantType.Missile,
      Enabled = true,
      SourcePrefab = "bomb_500_glide",
      SourceInfo = "info_bomb_500_glide",
      SourceDefinition = "Bomb_500_glide",
      NewPrefabName = "P_GLB1",
      NewInfoName = "P_GLB1_info",
      PostProcessorId = "P_GLB1_DragCurve",
      Mounts = new MountDef[6]
      {
        MountDef.Simple("bomb_glide1_single", "P_GLB1_single"),
        MountDef.Simple("bomb_glide1_double", "P_GLB1_double"),
        MountDef.Simple("bomb_glide1_triple", "P_GLB1_triple"),
        MountDef.Simple("bomb_500_internal", "P_GLB1_internal"),
        MountDef.Simple("bomb_500_internalx2", "P_GLB1_internalx2"),
        MountDef.Simple("bomb_500_internalx6", "P_GLB1_internalx6")
      }
    },
    new WeaponVariantDef()
    {
      VariantId = "P_KEM1",
      Type = VariantType.Missile,
      Enabled = true,
      SourcePrefab = "AGM_heavy",
      SourceInfo = "info_AGM_heavy",
      SourceDefinition = "AGM_heavy",
      NewPrefabName = "P_KEM1",
      NewInfoName = "P_KEM1_info",
      PostProcessorId = "P_KEM1_Complex",
      Mounts = new MountDef[2]
      {
        MountDef.Simple("AGM_heavy_single", "P_KEM1_single"),
        MountDef.Simple("AGM_heavy_double", "P_KEM1_double")
      }
    },
    new WeaponVariantDef()
    {
      VariantId = "P_SAMRadar1",
      Type = VariantType.Missile,
      Enabled = true,
      SourcePrefab = "SAM_Radar2",
      SourceInfo = "info_SAM_Radar2",
      SourceDefinition = "SAM_Radar2",
      NewPrefabName = "P_SAMRadar1",
      NewInfoName = "P_SAMRadar1_info",
      PostProcessorId = "P_SAMRadar1_Complex",
      Mounts = new MountDef[2]
      {
        MountDef.Simple("AAM2_single", "P_SAMRadar1_single"),
        MountDef.WithMeshSwap("bomb_penetrator1_internalx3", "P_SAMRadar1_internalx3", "bomb500")
      }
    },
    new WeaponVariantDef()
    {
      VariantId = "P_HAsM1",
      Type = VariantType.Missile,
      Enabled = true,
      SourcePrefab = "AShM1",
      SourceInfo = "info_AShM1",
      SourceDefinition = "AShM1",
      NewPrefabName = "P_HAsM1",
      NewInfoName = "P_HAsM1_info",
      PostProcessorId = "P_HAsM1_Complex",
      Mounts = new MountDef[1]
      {
        MountDef.Simple("AShM1_internal", "P_HAsM1_internal")
      }
    },
    new WeaponVariantDef()
    {
      VariantId = "P_RAM29",
      Type = VariantType.Missile,
      Enabled = true,
      SourcePrefab = "SAM_Radar1",
      SourceInfo = "info_SAM_Radar1",
      SourceDefinition = "SAM_Radar1",
      NewPrefabName = "P_RAM29",
      NewInfoName = "P_RAM29_info",
      PostProcessorId = "P_RAM29_Complex",
      Mounts = (MountDef[]) null
    },
    new WeaponVariantDef()
    {
      VariantId = "Emplacement1_AT_P",
      Type = VariantType.Building,
      Enabled = true,
      SourcePrefab = "Emplacement1_MANPADS",
      SourceDefinition = "Emplacement1_MANPADS",
      NewPrefabName = "Emplacement1_AT_P"
    },
    new WeaponVariantDef()
    {
      VariantId = "Emplacement1_57mm_P",
      Type = VariantType.Building,
      Enabled = true,
      SourcePrefab = "Emplacement1_23mm",
      SourceDefinition = "Emplacement1_23mm",
      NewPrefabName = "Emplacement1_57mm_P"
    },
    new WeaponVariantDef()
    {
      VariantId = "SAMTrailer1_as",
      Type = VariantType.Vehicle,
      Enabled = true,
      SourcePrefab = "SAMTrailer1",
      SourceDefinition = "SAMTrailer1",
      NewPrefabName = "SAMTrailer1_as"
    },
    new WeaponVariantDef()
    {
      VariantId = "AFV8_SAMR",
      Type = VariantType.Vehicle,
      Enabled = true,
      SourcePrefab = "AFV8_SAM",
      SourceDefinition = "AFV8_SAM",
      NewPrefabName = "AFV8_SAMR"
    },
    new WeaponVariantDef()
    {
      VariantId = "Linebreaker_SAMR",
      Type = VariantType.Vehicle,
      Enabled = true,
      SourcePrefab = "Linebreaker_SAM",
      SourceDefinition = "Linebreaker_SAM",
      NewPrefabName = "Linebreaker_SAMR"
    },
    new WeaponVariantDef()
    {
      VariantId = "Linebreaker_ARTY",
      Type = VariantType.Vehicle,
      Enabled = true,
      SourcePrefab = "Linebreaker_IFV",
      SourceDefinition = "Linebreaker_IFV",
      NewPrefabName = "Linebreaker_ARTY",
      PostProcessorId = "Linebreaker_ARTY_Setup"
    },
    new WeaponVariantDef()
    {
      VariantId = "UGV1_AT_P",
      Type = VariantType.Vehicle,
      Enabled = true,
      SourcePrefab = "UGV1_SAM",
      SourceDefinition = "UGV1_sam",
      NewPrefabName = "UGV1_AT_P",
      PostProcessorId = "UGV1_AT_P_Setup"
    },
    new WeaponVariantDef()
    {
      VariantId = "Horse1",
      Type = VariantType.Vehicle,
      Enabled = true,
      SourcePrefab = "MBT1",
      SourceDefinition = "MBT1",
      NewPrefabName = "Horse1",
      PostProcessorId = "Horse1_AssetBundle"
    },
    new WeaponVariantDef()
    {
      VariantId = "P_LRAA1",
      Type = VariantType.Vehicle,
      Enabled = true,
      SourcePrefab = "MBT",
      SourceDefinition = "MBT",
      NewPrefabName = "P_LRAA1",
      PostProcessorId = "P_LRAA1_Setup"
    },
    new WeaponVariantDef()
    {
      VariantId = "RocketPod1_triple_db",
      Type = VariantType.MountOnly,
      Enabled = true,
      SourceInfo = "info_rocket1",
      Mounts = new MountDef[1]
      {
        MountDef.Simple("RocketPod1_triple", "RocketPod1_triple_db")
      }
    },
    new WeaponVariantDef()
    {
      VariantId = "Rocket2_4Podx3_db",
      Type = VariantType.MountOnly,
      Enabled = true,
      SourceInfo = "info_rocket2",
      PostProcessorId = "Rocket2_4Podx3_db_Setup",
      Mounts = new MountDef[1]
      {
        MountDef.Simple("Rocket2_4Podx3", "Rocket2_4Podx3_db")
      }
    },
    new WeaponVariantDef()
    {
      VariantId = "AShM2_internalx8P",
      Type = VariantType.MountOnly,
      Enabled = true,
      SourceInfo = "AShM2_info",
      Mounts = new MountDef[1]
      {
        MountDef.WithMeshSwap("AGM_heavy_internalx8", "AShM2_internalx8P", "agm1")
      }
    },
    new WeaponVariantDef()
    {
      VariantId = "AShM1_cargox4",
      Type = VariantType.MountOnly,
      Enabled = true,
      SourceInfo = "info_AShM1",
      Mounts = new MountDef[1]
      {
        MountDef.Simple("AShM1_internalx4_stack", "AShM1_cargox4")
      }
    },
    new WeaponVariantDef()
    {
      VariantId = "CruiseMissile1_cargox16",
      Type = VariantType.MountOnly,
      Enabled = true,
      SourceInfo = "info_CruiseMissile1",
      Mounts = new MountDef[1]
      {
        MountDef.Simple("CruiseMissile1_internalx4", "CruiseMissile1_cargox16")
      }
    },
    new WeaponVariantDef()
    {
      VariantId = "AGM1_internal_mounts",
      Type = VariantType.MountOnly,
      Enabled = true,
      SourceInfo = "info_AGM1",
      Mounts = new MountDef[4]
      {
        MountDef.WithMeshSwap("bomb_125_internalx2_line", "AGM1_internalx2_lineP", "bomb", meshSource: "AGM1"),
        MountDef.WithMeshSwap("bomb_125_internalx4", "AGM1_internalx4P", "bomb", meshSource: "AGM1"),
        MountDef.WithMeshSwap("bomb_125_internalx6", "AGM1_internalx6P", "bomb", meshSource: "AGM1"),
        MountDef.WithMeshSwap("bomb_125_quad", "AGM1_quadP", "bomb", meshSource: "AGM1")
      }
    },
    new WeaponVariantDef()
    {
      VariantId = "bomb_500_triple_mount",
      Type = VariantType.MountOnly,
      Enabled = true,
      SourceInfo = "info_blastFrag500",
      Mounts = new MountDef[1]
      {
        MountDef.WithMeshSwap("AGM_heavy_triple", "bomb_500_tripleP", "AGM_heavy", meshSource: "bomb_500_single/pylon/bomb")
      }
    },
    new WeaponVariantDef()
    {
      VariantId = "bomb_250_quad_mount",
      Type = VariantType.MountOnly,
      Enabled = true,
      SourceInfo = "info_bomb_250_1",
      Mounts = new MountDef[1]
      {
        MountDef.WithMeshSwap("bomb_125_quad", "bomb_250_quadP", "bomb", meshSource: "bomb_250_1")
      }
    },
    new WeaponVariantDef()
    {
      VariantId = "bomb_250_glide_triple",
      Type = VariantType.MountOnly,
      Enabled = true,
      SourceInfo = "info_bomb_250_glide",
      Mounts = new MountDef[1]
      {
        MountDef.WithMeshSwap("bomb_250_triple", "bomb_250_glide_tripleP", "bomb", meshSource: "bomb_250_glide")
      }
    },
    new WeaponVariantDef()
    {
      VariantId = "AAM2_quad_internalP",
      Type = VariantType.MountOnly,
      Enabled = true,
      SourceInfo = "AAM2_info",
      Mounts = new MountDef[1]
      {
        MountDef.WithMeshSwap("AAM3_quad_internal", "AAM2_quad_internalP", "aam3", meshSource: "AAM2")
      }
    },
    new WeaponVariantDef()
    {
      VariantId = "AAM4_quad_internalP",
      Type = VariantType.MountOnly,
      Enabled = true,
      SourceInfo = "AAM4_info",
      Mounts = new MountDef[1]
      {
        MountDef.WithMeshSwap("AAM3_quad_internal", "AAM4_quad_internalP", "aam3", meshSource: "AAM4")
      }
    },
    new WeaponVariantDef()
    {
      VariantId = "AAM1_quad_internalP",
      Type = VariantType.MountOnly,
      Enabled = true,
      SourceInfo = "AAM1_info",
      Mounts = new MountDef[1]
      {
        MountDef.WithMeshSwap("AAM3_quad_internal", "AAM1_quad_internalP", "aam3", meshSource: "AAM1")
      }
    },
    new WeaponVariantDef()
    {
      VariantId = "bomb_250_doubleP",
      Type = VariantType.MountOnly,
      Enabled = true,
      SourceInfo = "info_bomb_250_1",
      Mounts = new MountDef[1]
      {
        MountDef.WithMeshSwap("bomb_125_double", "bomb_250_doubleP", "bomb", meshSource: "bomb_250_1")
      }
    },
    new WeaponVariantDef()
    {
      VariantId = "AShM3_doubleP",
      Type = VariantType.MountOnly,
      Enabled = true,
      SourceInfo = "AShM3_info",
      Mounts = new MountDef[1]
      {
        MountDef.WithMeshSwap("AGM_heavy_double", "AShM3_doubleP", "agm1", meshSource: "AShM3")
      }
    },
    new WeaponVariantDef()
    {
      VariantId = "bomb_penetrator1_doubleP",
      Type = VariantType.MountOnly,
      Enabled = true,
      SourceInfo = "info_bomb_penetrator1",
      Mounts = new MountDef[1]
      {
        MountDef.WithMeshSwap("AGM_heavy_double", "bomb_penetrator1_doubleP", "agm1", meshSource: "bomb_penetrator1/bomb")
      }
    },
    new WeaponVariantDef()
    {
      VariantId = "gun_20mm_variants",
      Type = VariantType.MountOnly,
      Enabled = true,
      SourceInfo = "Gun20mm_Rotary",
      PostProcessorId = "gun_20mm_variants_Setup",
      Mounts = new MountDef[3]
      {
        MountDef.Simple("gun_20mm_internal", "gun_20mm_internal_stealth"),
        MountDef.Simple("gun_20mm_internal", "gun_20mm_internal_ap"),
        MountDef.Simple("gun_20mm_internal", "gun_20mm_internal_he")
      }
    },
    new WeaponVariantDef()
    {
      VariantId = "gun_27mm_variants",
      Type = VariantType.MountOnly,
      Enabled = true,
      SourceInfo = "Gun27mm_Autocannon",
      PostProcessorId = "gun_27mm_variants_Setup",
      Mounts = new MountDef[3]
      {
        MountDef.Simple("gun_27mm_internal", "gun_27mm_internal_stealth"),
        MountDef.Simple("gun_27mm_internal", "gun_27mm_internal_ap"),
        MountDef.Simple("gun_27mm_internal", "gun_27mm_internal_he")
      }
    },
    new WeaponVariantDef()
    {
      VariantId = "FuelPod1_P",
      Type = VariantType.MountOnly,
      Enabled = true,
      SourceInfo = "JammingPod1",
      PostProcessorId = "FuelPod1_P_Setup",
      Mounts = new MountDef[1]
      {
        MountDef.Simple("JammingPod1", "FuelPod1_P")
      }
    },
    new WeaponVariantDef()
    {
      VariantId = "JammingPod1_mini_P",
      Type = VariantType.MountOnly,
      Enabled = true,
      SourceInfo = "JammingPod1",
      PostProcessorId = "JammingPod1_mini_P_Setup",
      Mounts = new MountDef[1]
      {
        MountDef.Simple("JammingPod1", "JammingPod1_mini_P")
      }
    },
    new WeaponVariantDef()
    {
      VariantId = "P_PassiveJammer1",
      Type = VariantType.MountOnly,
      Enabled = true,
      SourceInfo = "Radome",
      PostProcessorId = "P_PassiveJammer1_Setup",
      Mounts = new MountDef[1]
      {
        MountDef.Simple("Radome1", "P_PassiveJammer1")
      }
    },
    new WeaponVariantDef()
    {
      VariantId = "RocketPod1_triple_internalP",
      Type = VariantType.MountOnly,
      Enabled = true,
      SourceInfo = "info_rocket1",
      Mounts = new MountDef[1]
      {
        MountDef.Simple("RocketPod1_triple", "RocketPod1_triple_internalP")
      }
    },
    new WeaponVariantDef()
    {
      VariantId = "Rocket2_4Podx3_internalP",
      Type = VariantType.MountOnly,
      Enabled = true,
      SourceInfo = "info_rocket2",
      Mounts = new MountDef[1]
      {
        MountDef.Simple("Rocket2_4Podx3", "Rocket2_4Podx3_internalP")
      }
    },
    new WeaponVariantDef()
    {
      VariantId = "IRMS1_triple_internalP",
      Type = VariantType.MountOnly,
      Enabled = true,
      SourceInfo = "info_SAM_IR1",
      Mounts = new MountDef[1]
      {
        MountDef.Simple("IRMS1_triple", "IRMS1_triple_internalP")
      }
    },
    new WeaponVariantDef()
    {
      VariantId = "AAM3_single_stealth",
      Type = VariantType.MountOnly,
      Enabled = true,
      SourceInfo = "AAM3_info",
      Mounts = new MountDef[1]
      {
        MountDef.Simple("AAM3_single", "AAM3_single_stealth")
      }
    },
    new WeaponVariantDef()
    {
      VariantId = "AAM4_x18",
      Type = VariantType.MountOnly,
      Enabled = true,
      SourceInfo = "AAM4_info",
      Mounts = new MountDef[1]
      {
        MountDef.Simple("bomb_250_internalx18", "AAM4_x18")
      }
    },
    new WeaponVariantDef()
    {
      VariantId = "AAM2_x18",
      Type = VariantType.MountOnly,
      Enabled = true,
      SourceInfo = "AAM2_info",
      Mounts = new MountDef[1]
      {
        MountDef.Simple("bomb_250_internalx18", "AAM2_x18")
      }
    },
    new WeaponVariantDef()
    {
      VariantId = "bomb_125_sextupleP",
      Type = VariantType.MountOnly,
      Enabled = true,
      SourceInfo = "info_bomb_125_1",
      PostProcessorId = "bomb_125_sextupleP_Setup",
      Mounts = new MountDef[1]
      {
        MountDef.Simple("bomb_125_triple", "bomb_125_sextupleP")
      }
    },
    new WeaponVariantDef()
    {
      VariantId = "P_FlarePod1",
      Type = VariantType.MountOnly,
      Enabled = true,
      SourceInfo = "JammingPod1",
      PostProcessorId = "P_FlarePod1_Setup",
      Mounts = new MountDef[1]
      {
        MountDef.Simple("gun_12.7mm_pod", "P_FlarePod1")
      }
    }
  };
}

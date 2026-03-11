using HarmonyLib;
using System;
using System.Linq;
using UnityEngine;

namespace IfritVariants.Patches
{
    [HarmonyPatch(typeof(Encyclopedia), "AfterLoad", new Type[] { })]
    public static class EncyclopediaPatch
    {
        [HarmonyPrefix]
        public static void Prefix(Encyclopedia __instance)
        {
            AircraftDefinition original = __instance.aircraft
                .FirstOrDefault(a => a.jsonKey == Plugin.OriginalJsonKey);
            if (original == null)
            {
                Debug.LogError("[IfritVariants] Could not find KR-67A (Multirole1) in Encyclopedia!");
                return;
            }

            // ── Add AAM-29/AAM-36 quad mounts to KR-67A prefab pylons ──
            try
            {
                AddWeaponsToPrefab(original);
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[IfritVariants] Failed to add weapons to prefab: {ex.Message}");
            }

            // ── KR-67EX ──
            if (Plugin.EX_Enable.Value && !__instance.aircraft.Any(a => a.jsonKey == Plugin.EX_JsonKey))
            {
                var clone = UnityEngine.Object.Instantiate(original);
                clone.name = Plugin.EX_JsonKey;
                clone.jsonKey = Plugin.EX_JsonKey;
                clone.unitName = Plugin.EX_DisplayName;
                clone.code = Plugin.EX_Code;
                clone.value = original.value * Plugin.EX_CostMultiplier.Value;
                clone.disabled = false;
                clone.unitPrefab = original.unitPrefab;

                clone.aircraftParameters = UnityEngine.Object.Instantiate(original.aircraftParameters);
                clone.aircraftParameters.aircraftName = Plugin.EX_DisplayName;
                clone.aircraftParameters.aircraftDescription = Plugin.EX_Description;
                clone.aircraftParameters.aircraftGLimit = Plugin.EX_GLimit.Value;
                clone.aircraftParameters.rankRequired = 0;

                clone.aircraftInfo = new AircraftInfo
                {
                    emptyWeight = original.aircraftInfo.emptyWeight,
                    maxSpeed = original.aircraftInfo.maxSpeed,
                    stallSpeed = original.aircraftInfo.stallSpeed,
                    maneuverability = original.aircraftInfo.maneuverability,
                    maxWeight = original.aircraftInfo.maxWeight
                };

                __instance.aircraft.Add(clone);
                Plugin.EX_CloneDef = clone;
                Debug.Log($"[IfritVariants] KR-67EX registered. Cost: ${clone.value}M");
            }

            // ── KR-67R ──
            if (Plugin.R_Enable.Value && !__instance.aircraft.Any(a => a.jsonKey == Plugin.R_JsonKey))
            {
                var clone = UnityEngine.Object.Instantiate(original);
                clone.name = Plugin.R_JsonKey;
                clone.jsonKey = Plugin.R_JsonKey;
                clone.unitName = Plugin.R_DisplayName;
                clone.code = Plugin.R_Code;
                clone.value = original.value * Plugin.R_CostMultiplier.Value;
                clone.disabled = false;
                clone.unitPrefab = original.unitPrefab;
                clone.radarSize = original.radarSize / Plugin.R_RCSDivisor.Value;

                clone.aircraftParameters = UnityEngine.Object.Instantiate(original.aircraftParameters);
                clone.aircraftParameters.aircraftName = Plugin.R_DisplayName;
                clone.aircraftParameters.aircraftDescription = Plugin.R_Description;
                clone.aircraftParameters.rankRequired = 0;

                clone.aircraftInfo = new AircraftInfo
                {
                    emptyWeight = original.aircraftInfo.emptyWeight,
                    maxSpeed = original.aircraftInfo.maxSpeed,
                    stallSpeed = original.aircraftInfo.stallSpeed,
                    maneuverability = original.aircraftInfo.maneuverability,
                    maxWeight = original.aircraftInfo.maxWeight
                };

                __instance.aircraft.Add(clone);
                Plugin.R_CloneDef = clone;
                Debug.Log($"[IfritVariants] KR-67R registered. Cost: ${clone.value}M, RCS: {clone.radarSize}");
            }
        }

        private static void AddWeaponsToPrefab(AircraftDefinition original)
        {
            if (original.unitPrefab == null) return;

            WeaponManager wm = original.unitPrefab.GetComponentInChildren<WeaponManager>();
            if (wm == null)
            {
                Aircraft ac = original.unitPrefab.GetComponent<Aircraft>();
                if (ac != null) wm = ac.weaponManager;
            }
            if (wm == null)
            {
                Debug.LogWarning("[IfritVariants] AddWeapons: no WeaponManager found on prefab");
                return;
            }

            // Find quad AAM mounts: AAM-26=AAM1, AAM-29=AAM2
            var allMounts = Resources.FindObjectsOfTypeAll<WeaponMount>();

            WeaponMount aam26Quad = null; // AAM1_quad_internalP
            WeaponMount aam29Quad = null; // AAM2_quad_internalP

            foreach (var m in allMounts)
            {
                string key = m.jsonKey ?? m.name ?? "";
                if (key == "AAM1_quad_internalP") aam26Quad = m;
                if (key == "AAM2_quad_internalP") aam29Quad = m;
            }

            if (aam26Quad == null) Debug.LogWarning("[IfritVariants] Could not find AAM1_quad_internalP (AAM-26 quad)");
            if (aam29Quad == null) Debug.LogWarning("[IfritVariants] Could not find AAM2_quad_internalP (AAM-29 quad)");

            int[] pylonIndices = { 4, 5 };
            foreach (int idx in pylonIndices)
            {
                if (idx >= wm.hardpointSets.Length) continue;
                var options = wm.hardpointSets[idx].weaponOptions;

                if (aam26Quad != null && !options.Contains(aam26Quad))
                {
                    options.Add(aam26Quad);
                    Debug.Log($"[IfritVariants] Added AAM-26 quad to hardpoint {idx}");
                }
                if (aam29Quad != null && !options.Contains(aam29Quad))
                {
                    options.Add(aam29Quad);
                    Debug.Log($"[IfritVariants] Added AAM-29 quad to hardpoint {idx}");
                }
            }
        }
    }
}

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
                Plugin.Log("Could not find KR-67A (Multirole1) in Encyclopedia!");
                return;
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
                Plugin.Log($"KR-67EX registered. Cost: ${clone.value}M");
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
                Plugin.Log($"KR-67R registered. Cost: ${clone.value}M, RCS: {clone.radarSize}");
            }
        }

        [HarmonyPostfix]
        public static void Postfix(Encyclopedia __instance)
        {
            // Add quad AAM mounts to KR-67A pylons AFTER WeaponLookup is populated
            try
            {
                AddWeaponsToPrefab(__instance);
            }
            catch (Exception ex)
            {
                Plugin.Log($"Failed to add weapons to prefab: {ex}");
            }
        }

        private static void AddWeaponsToPrefab(Encyclopedia encyclopedia)
        {
            AircraftDefinition original = encyclopedia.aircraft
                .FirstOrDefault(a => a.jsonKey == Plugin.OriginalJsonKey);
            if (original?.unitPrefab == null) return;

            WeaponManager wm = original.unitPrefab.GetComponent<Aircraft>()?.weaponManager;
            if (wm == null)
                wm = original.unitPrefab.GetComponentInChildren<WeaponManager>();
            if (wm == null)
            {
                Plugin.Log("AddWeapons: no WeaponManager found on prefab");
                return;
            }

            // Use Encyclopedia.WeaponLookup (populated by AfterLoad) to find mounts by jsonKey
            WeaponMount aam26Quad = null;
            WeaponMount aam29Quad = null;

            if (Encyclopedia.WeaponLookup != null)
            {
                Encyclopedia.WeaponLookup.TryGetValue("AAM1_quad_internalP", out aam26Quad);
                Encyclopedia.WeaponLookup.TryGetValue("AAM2_quad_internalP", out aam29Quad);
            }

            // Fallback to searching all loaded WeaponMount assets
            if (aam26Quad == null || aam29Quad == null)
            {
                Plugin.Log("WeaponLookup miss, falling back to Resources search");
                foreach (var m in Resources.FindObjectsOfTypeAll<WeaponMount>())
                {
                    string key = m.jsonKey ?? m.name ?? "";
                    if (aam26Quad == null && key == "AAM1_quad_internalP") aam26Quad = m;
                    if (aam29Quad == null && key == "AAM2_quad_internalP") aam29Quad = m;
                }
            }

            // Second fallback: search by name
            if (aam26Quad == null || aam29Quad == null)
            {
                Plugin.Log("jsonKey search miss, trying name search");
                foreach (var m in encyclopedia.weaponMounts)
                {
                    if (aam26Quad == null && m.name == "AAM1_quad_internalP") aam26Quad = m;
                    if (aam29Quad == null && m.name == "AAM2_quad_internalP") aam29Quad = m;
                }
            }

            if (aam26Quad == null) Plugin.Log("WARN: Could not find AAM1_quad_internalP (AAM-26 quad)");
            else Plugin.Log($"Found AAM-26 quad: name={aam26Quad.name}, jsonKey={aam26Quad.jsonKey}");
            if (aam29Quad == null) Plugin.Log("WARN: Could not find AAM2_quad_internalP (AAM-29 quad)");
            else Plugin.Log($"Found AAM-29 quad: name={aam29Quad.name}, jsonKey={aam29Quad.jsonKey}");

            // Log all weapon mounts if we couldn't find them
            if (aam26Quad == null || aam29Quad == null)
            {
                Plugin.Log("Dumping all encyclopedia weaponMounts:");
                foreach (var m in encyclopedia.weaponMounts)
                    Plugin.Log($"  {m.name} | jsonKey={m.jsonKey}");
            }

            int[] pylonIndices = { 4, 5 };
            foreach (int idx in pylonIndices)
            {
                if (idx >= wm.hardpointSets.Length) continue;
                var options = wm.hardpointSets[idx].weaponOptions;

                if (aam26Quad != null && !options.Contains(aam26Quad))
                {
                    options.Add(aam26Quad);
                    Plugin.Log($"Added AAM-26 quad to hardpoint {idx}");
                }
                if (aam29Quad != null && !options.Contains(aam29Quad))
                {
                    options.Add(aam29Quad);
                    Plugin.Log($"Added AAM-29 quad to hardpoint {idx}");
                }
            }
        }
    }
}

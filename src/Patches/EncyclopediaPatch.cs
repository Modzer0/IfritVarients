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

            // Log existing pylon options for debugging
            for (int i = 0; i < wm.hardpointSets.Length; i++)
            {
                var hs = wm.hardpointSets[i];
                var names = string.Join(", ", hs.weaponOptions.ConvertAll(w => w != null ? $"{w.name}({w.jsonKey})" : "null"));
                Debug.Log($"[IfritVariants] Hardpoint {i} '{hs.name}': [{names}]");
            }

            // Find quad AAM mounts by partial name/jsonKey match
            var allMounts = Resources.FindObjectsOfTypeAll<WeaponMount>();
            Debug.Log($"[IfritVariants] Total WeaponMounts found: {allMounts.Length}");

            WeaponMount aam29Quad = null;
            WeaponMount aam36Quad = null;

            foreach (var m in allMounts)
            {
                string n = m.name?.ToLowerInvariant() ?? "";
                string k = m.jsonKey?.ToLowerInvariant() ?? "";

                // AAM-29 quad (AAM2 in game code)
                if (aam29Quad == null && (n.Contains("aam2") || k.Contains("aam2")) && (n.Contains("quad") || k.Contains("quad")))
                {
                    aam29Quad = m;
                    Debug.Log($"[IfritVariants] Found AAM-29 quad: name={m.name}, jsonKey={m.jsonKey}");
                }
                // AAM-36 quad (AAM4 in game code)
                if (aam36Quad == null && (n.Contains("aam4") || k.Contains("aam4")) && (n.Contains("quad") || k.Contains("quad")))
                {
                    aam36Quad = m;
                    Debug.Log($"[IfritVariants] Found AAM-36 quad: name={m.name}, jsonKey={m.jsonKey}");
                }
            }

            if (aam29Quad == null)
                Debug.LogWarning("[IfritVariants] Could not find AAM-29 quad mount. Logging all AAM mounts:");
            if (aam36Quad == null)
                Debug.LogWarning("[IfritVariants] Could not find AAM-36 quad mount. Logging all AAM mounts:");

            if (aam29Quad == null || aam36Quad == null)
            {
                foreach (var m in allMounts)
                {
                    string n = m.name?.ToLowerInvariant() ?? "";
                    if (n.Contains("aam") || n.Contains("missile") || n.Contains("internal"))
                        Debug.Log($"[IfritVariants]   mount: name={m.name}, jsonKey={m.jsonKey}, mountName={m.mountName}");
                }
            }

            int[] pylonIndices = { 4, 5 };
            foreach (int idx in pylonIndices)
            {
                if (idx >= wm.hardpointSets.Length) continue;
                var options = wm.hardpointSets[idx].weaponOptions;

                if (aam29Quad != null && !options.Contains(aam29Quad))
                {
                    options.Add(aam29Quad);
                    Debug.Log($"[IfritVariants] Added {aam29Quad.name} to hardpoint {idx}");
                }
                if (aam36Quad != null && !options.Contains(aam36Quad))
                {
                    options.Add(aam36Quad);
                    Debug.Log($"[IfritVariants] Added {aam36Quad.name} to hardpoint {idx}");
                }
            }
        }
    }
}

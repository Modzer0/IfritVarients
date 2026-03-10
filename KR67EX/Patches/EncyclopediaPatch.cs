using HarmonyLib;
using System.Linq;
using UnityEngine;

namespace KR67EX.Patches
{
    /// <summary>
    /// Clone the KR-67A into a KR-67EX variant and register it in Encyclopedia.
    /// Uses PREFIX so AfterLoad handles Lookup, IndexLookup, CacheMass, and sorting.
    /// </summary>
    [HarmonyPatch(typeof(Encyclopedia), "AfterLoad")]
    public static class EncyclopediaPatch
    {
        [HarmonyPrefix]
        public static void Prefix(Encyclopedia __instance)
        {
            // Avoid duplicate registration
            if (__instance.aircraft.Any(a => a.jsonKey == Plugin.CloneJsonKey))
                return;

            AircraftDefinition original = __instance.aircraft
                .FirstOrDefault(a => a.jsonKey == Plugin.OriginalJsonKey);
            if (original == null)
            {
                Debug.LogError("[KR-67EX] Could not find KR-67A (Multirole1) in Encyclopedia!");
                return;
            }

            // Clone the definition
            AircraftDefinition clone = Object.Instantiate(original);
            clone.name = Plugin.CloneJsonKey;
            clone.jsonKey = Plugin.CloneJsonKey;
            clone.unitName = Plugin.CloneDisplayName;
            clone.code = Plugin.CloneCode;
            clone.value = original.value * Plugin.CostMultiplier.Value;
            clone.disabled = false;
            clone.unitPrefab = original.unitPrefab;

            // Clone aircraftParameters (separate ScriptableObject — must be independent)
            clone.aircraftParameters = Object.Instantiate(original.aircraftParameters);
            clone.aircraftParameters.aircraftName = Plugin.CloneDisplayName;
            clone.aircraftParameters.aircraftDescription = Plugin.CloneDescription;
            clone.aircraftParameters.aircraftGLimit = Plugin.GLimit.Value;
            clone.aircraftParameters.rankRequired = 0;

            // Clone aircraftInfo
            clone.aircraftInfo = Object.Instantiate(original.aircraftInfo);

            // Register — AfterLoad will handle Lookup, IndexLookup, CacheMass
            __instance.aircraft.Add(clone);
            Plugin.CloneDef = clone;

            Debug.Log($"[KR-67EX] Registered variant. Cost: ${clone.value}M (base ${original.value}M × {Plugin.CostMultiplier.Value})");
        }
    }
}

using HarmonyLib;
using System.Linq;
using UnityEngine;

namespace KR67R.Patches
{
    /// <summary>
    /// Clone the KR-67A into a KR-67R recon variant.
    /// RCS divided by 100, cost 4x, no radar.
    /// Forward bay and wing pylons disabled.
    /// </summary>
    [HarmonyPatch(typeof(Encyclopedia), "AfterLoad")]
    public static class EncyclopediaPatch
    {
        [HarmonyPrefix]
        public static void Prefix(Encyclopedia __instance)
        {
            if (__instance.aircraft.Any(a => a.jsonKey == Plugin.CloneJsonKey))
                return;

            AircraftDefinition original = __instance.aircraft
                .FirstOrDefault(a => a.jsonKey == Plugin.OriginalJsonKey);
            if (original == null)
            {
                Debug.LogError("[KR-67R] Could not find KR-67A (Multirole1) in Encyclopedia!");
                return;
            }

            AircraftDefinition clone = Object.Instantiate(original);
            clone.name = Plugin.CloneJsonKey;
            clone.jsonKey = Plugin.CloneJsonKey;
            clone.unitName = Plugin.CloneDisplayName;
            clone.code = Plugin.CloneCode;
            clone.value = original.value * Plugin.CostMultiplier.Value;
            clone.disabled = false;
            clone.unitPrefab = original.unitPrefab;

            // RCS divided by 100
            clone.radarSize = original.radarSize / Plugin.RCSDivisor.Value;

            // Clone aircraftParameters
            clone.aircraftParameters = Object.Instantiate(original.aircraftParameters);
            clone.aircraftParameters.aircraftName = Plugin.CloneDisplayName;
            clone.aircraftParameters.aircraftDescription = Plugin.CloneDescription;
            clone.aircraftParameters.rankRequired = 0;

            // Clone aircraftInfo
            clone.aircraftInfo = Object.Instantiate(original.aircraftInfo);

            __instance.aircraft.Add(clone);
            Plugin.CloneDef = clone;

            Debug.Log($"[KR-67R] Registered variant. Cost: ${clone.value}M, RCS: {clone.radarSize} (base {original.radarSize} / {Plugin.RCSDivisor.Value})");
        }
    }
}

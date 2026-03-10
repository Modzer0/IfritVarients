using HarmonyLib;
using System.Linq;
using UnityEngine;

namespace IfritVariants.Patches
{
    [HarmonyPatch(typeof(Encyclopedia), "AfterLoad")]
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

            // ── KR-67EX ──
            if (Plugin.EX_Enable.Value && !__instance.aircraft.Any(a => a.jsonKey == Plugin.EX_JsonKey))
            {
                var clone = Object.Instantiate(original);
                clone.name = Plugin.EX_JsonKey;
                clone.jsonKey = Plugin.EX_JsonKey;
                clone.unitName = Plugin.EX_DisplayName;
                clone.code = Plugin.EX_Code;
                clone.value = original.value * Plugin.EX_CostMultiplier.Value;
                clone.disabled = false;
                clone.unitPrefab = original.unitPrefab;

                clone.aircraftParameters = Object.Instantiate(original.aircraftParameters);
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
                var clone = Object.Instantiate(original);
                clone.name = Plugin.R_JsonKey;
                clone.jsonKey = Plugin.R_JsonKey;
                clone.unitName = Plugin.R_DisplayName;
                clone.code = Plugin.R_Code;
                clone.value = original.value * Plugin.R_CostMultiplier.Value;
                clone.disabled = false;
                clone.unitPrefab = original.unitPrefab;
                clone.radarSize = original.radarSize / Plugin.R_RCSDivisor.Value;

                clone.aircraftParameters = Object.Instantiate(original.aircraftParameters);
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
    }
}

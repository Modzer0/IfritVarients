using HarmonyLib;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace KR67EX.Patches
{
    /// <summary>
    /// Copy EW-25 Medusa (EW1) radar parameters to KR-67EX at runtime.
    /// Maintains the KR-67A's original radarCone (arc) but matches Medusa's
    /// range and detection stats (RadarParameters: maxRange, maxSignal, minSignal, etc.).
    /// </summary>
    [HarmonyPatch(typeof(Radar), "Awake")]
    public static class RadarPatch
    {
        private static readonly FieldInfo radarConeField =
            AccessTools.Field(typeof(Radar), "radarCone");

        // Cached EW1 radar params — read once at runtime
        private static RadarParams? cachedMedusaParams;
        private static bool medusaSearched;

        [HarmonyPostfix]
        public static void Postfix(Radar __instance)
        {
            // Get the attached unit's aircraft
            var unitField = AccessTools.Field(typeof(TargetDetector), "attachedUnit");
            var unit = unitField?.GetValue(__instance) as Unit;
            var aircraft = unit as Aircraft;
            if (!Plugin.IsKR67EX(aircraft)) return;

            // Cache the Medusa's radar params on first use
            if (!medusaSearched)
            {
                medusaSearched = true;
                var allRadars = Resources.FindObjectsOfTypeAll<Radar>();
                foreach (var r in allRadars)
                {
                    var rUnit = unitField?.GetValue(r) as Unit;
                    if (rUnit != null && rUnit is Aircraft rAircraft &&
                        rAircraft.definition != null &&
                        rAircraft.definition.jsonKey == "EW1")
                    {
                        cachedMedusaParams = r.RadarParameters;
                        Debug.Log($"[KR-67EX] Captured EW-25 Medusa radar params: " +
                                  $"maxRange={r.RadarParameters.maxRange}, " +
                                  $"maxSignal={r.RadarParameters.maxSignal}, " +
                                  $"minSignal={r.RadarParameters.minSignal}");
                        break;
                    }
                }

                // Fallback: search by prefab name if runtime instances aren't available yet
                if (!cachedMedusaParams.HasValue)
                {
                    var allRadarsFallback = Resources.FindObjectsOfTypeAll<Radar>();
                    foreach (var r in allRadarsFallback)
                    {
                        if (r.gameObject.name.Contains("EW1") ||
                            r.transform.root.name.Contains("EW1"))
                        {
                            cachedMedusaParams = r.RadarParameters;
                            Debug.Log($"[KR-67EX] Captured EW-25 Medusa radar params (fallback): " +
                                      $"maxRange={r.RadarParameters.maxRange}");
                            break;
                        }
                    }
                }

                if (!cachedMedusaParams.HasValue)
                    Debug.LogWarning("[KR-67EX] Could not find EW-25 Medusa radar to copy params from!");
            }

            if (cachedMedusaParams.HasValue)
            {
                // Save original cone (arc) — we keep the KR-67A's scan arc
                float originalCone = (float)radarConeField.GetValue(__instance);

                // Apply Medusa's detection parameters
                __instance.RadarParameters = cachedMedusaParams.Value;

                // Restore original cone
                radarConeField.SetValue(__instance, originalCone);

                Debug.Log($"[KR-67EX] Applied Medusa radar params, kept cone={originalCone}");
            }
        }
    }
}

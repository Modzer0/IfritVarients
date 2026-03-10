using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace IfritVariants.Patches.EX
{
    /// <summary>
    /// Copy EW-25 Medusa (EW1) radar parameters to KR-67EX at runtime.
    /// Keeps the KR-67A's original scan arc (radarCone).
    /// </summary>
    [HarmonyPatch(typeof(Radar), "Awake")]
    public static class RadarPatch
    {
        private static readonly FieldInfo radarConeField =
            AccessTools.Field(typeof(Radar), "radarCone");
        private static readonly FieldInfo attachedUnitField =
            AccessTools.Field(typeof(TargetDetector), "attachedUnit");

        private static RadarParams? cachedMedusaParams;
        private static bool medusaSearched;

        [HarmonyPostfix]
        public static void Postfix(Radar __instance)
        {
            var unit = attachedUnitField?.GetValue(__instance) as Unit;
            var aircraft = unit as Aircraft;
            if (!Plugin.IsKR67EX(aircraft)) return;

            if (!medusaSearched)
            {
                medusaSearched = true;
                foreach (var r in Resources.FindObjectsOfTypeAll<Radar>())
                {
                    var rUnit = attachedUnitField?.GetValue(r) as Unit;
                    if (rUnit is Aircraft rAc && rAc.definition?.jsonKey == "EW1")
                    {
                        cachedMedusaParams = r.RadarParameters;
                        Debug.Log($"[IfritVariants] Captured EW-25 Medusa radar params: maxRange={r.RadarParameters.maxRange}");
                        break;
                    }
                }
                // Fallback: search by prefab name
                if (!cachedMedusaParams.HasValue)
                {
                    foreach (var r in Resources.FindObjectsOfTypeAll<Radar>())
                    {
                        if (r.gameObject.name.Contains("EW1") || r.transform.root.name.Contains("EW1"))
                        {
                            cachedMedusaParams = r.RadarParameters;
                            break;
                        }
                    }
                }
                if (!cachedMedusaParams.HasValue)
                    Debug.LogWarning("[IfritVariants] Could not find EW-25 Medusa radar params!");
            }

            if (cachedMedusaParams.HasValue)
            {
                float originalCone = (float)radarConeField.GetValue(__instance);
                __instance.RadarParameters = cachedMedusaParams.Value;
                radarConeField.SetValue(__instance, originalCone);
            }
        }
    }
}

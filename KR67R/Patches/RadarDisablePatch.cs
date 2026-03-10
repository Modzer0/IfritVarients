using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace KR67R.Patches
{
    /// <summary>
    /// Disable radar on KR-67R. The recon variant has no radar — only optical sensors.
    /// Deactivates the Radar component when it awakens on a KR-67R aircraft.
    /// </summary>
    [HarmonyPatch(typeof(Radar), "Awake")]
    public static class RadarDisablePatch
    {
        private static readonly FieldInfo attachedUnitField =
            AccessTools.Field(typeof(TargetDetector), "attachedUnit");

        [HarmonyPostfix]
        public static void Postfix(Radar __instance)
        {
            var unit = attachedUnitField?.GetValue(__instance) as Unit;
            var aircraft = unit as Aircraft;
            if (!Plugin.IsKR67R(aircraft)) return;

            // Deactivate the radar — it won't scan or emit
            __instance.activated = false;
            Debug.Log("[KR-67R] Radar deactivated — recon variant has no radar.");
        }
    }

    /// <summary>
    /// Prevent the player from toggling radar on for the KR-67R.
    /// Intercept the RPC toggle to keep radar off.
    /// </summary>
    [HarmonyPatch(typeof(Aircraft), "UserCode_CmdToggleRadar_1821461427")]
    public static class RadarTogglePatch
    {
        [HarmonyPrefix]
        public static bool Prefix(Aircraft __instance)
        {
            if (Plugin.IsKR67R(__instance))
            {
                // Block radar toggle — KR-67R has no radar
                return false;
            }
            return true;
        }
    }
}

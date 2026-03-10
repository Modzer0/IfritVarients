using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace IfritVariants.Patches.R
{
    /// <summary>
    /// Disable radar on KR-67R. Recon variant has no radar — only optical sensors.
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

            __instance.activated = false;
        }
    }

    /// <summary>
    /// Prevent the player from toggling radar on for the KR-67R.
    /// </summary>
    [HarmonyPatch(typeof(Aircraft), "UserCode_CmdToggleRadar_1821461427")]
    public static class RadarTogglePatch
    {
        [HarmonyPrefix]
        public static bool Prefix(Aircraft __instance)
        {
            if (Plugin.IsKR67R(__instance))
                return false; // Block radar toggle
            return true;
        }
    }
}

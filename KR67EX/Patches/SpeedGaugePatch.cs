using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace KR67EX.Patches
{
    /// <summary>
    /// Raise overspeed warning threshold to Mach 2.5 for the KR-67EX.
    /// </summary>
    [HarmonyPatch(typeof(SpeedGauge), "Initialize")]
    public static class SpeedGaugeInitPatch
    {
        private static readonly FieldInfo overspeedField =
            AccessTools.Field(typeof(SpeedGauge), "overspeedThreshold");
        private static readonly FieldInfo aircraftField =
            AccessTools.Field(typeof(SpeedGauge), "aircraft");

        [HarmonyPostfix]
        public static void Postfix(SpeedGauge __instance, Aircraft aircraft)
        {
            if (Plugin.IsKR67EX(aircraft))
            {
                overspeedField.SetValue(__instance, Plugin.MaxSpeedMs.Value);
            }
        }
    }

    /// <summary>
    /// Suppress overspeed warnings during Refresh for KR-67EX if below Mach 2.5.
    /// </summary>
    [HarmonyPatch(typeof(SpeedGauge), "Refresh")]
    public static class SpeedGaugeRefreshPatch
    {
        private static readonly FieldInfo overspeedField =
            AccessTools.Field(typeof(SpeedGauge), "overspeedThreshold");
        private static readonly FieldInfo aircraftField =
            AccessTools.Field(typeof(SpeedGauge), "aircraft");

        [HarmonyPrefix]
        public static void Prefix(SpeedGauge __instance)
        {
            var aircraft = aircraftField?.GetValue(__instance) as Aircraft;
            if (Plugin.IsKR67EX(aircraft))
            {
                overspeedField.SetValue(__instance, Plugin.MaxSpeedMs.Value);
            }
        }
    }
}

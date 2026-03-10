using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace KR67R.Patches
{
    /// <summary>
    /// Raise overspeed warning to Mach 5 at altitude for the KR-67R.
    /// </summary>
    [HarmonyPatch(typeof(SpeedGauge), "Initialize")]
    public static class SpeedGaugeInitPatch
    {
        private static readonly FieldInfo overspeedField =
            AccessTools.Field(typeof(SpeedGauge), "overspeedThreshold");

        [HarmonyPostfix]
        public static void Postfix(SpeedGauge __instance, Aircraft aircraft)
        {
            if (Plugin.IsKR67R(aircraft))
                overspeedField.SetValue(__instance, Plugin.Mach5At80kFt);
        }
    }

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
            if (Plugin.IsKR67R(aircraft))
                overspeedField.SetValue(__instance, Plugin.Mach5At80kFt);
        }
    }
}

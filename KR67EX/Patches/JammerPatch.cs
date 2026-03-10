using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace KR67EX.Patches
{
    /// <summary>
    /// Set jammer intensity to 10 for the KR-67EX.
    /// Patches RadarJammer.Fire to override jammingIntensity at runtime.
    /// </summary>
    [HarmonyPatch(typeof(RadarJammer), "Fire")]
    public static class JammerPatch
    {
        private static readonly FieldInfo intensityField =
            AccessTools.Field(typeof(RadarJammer), "jammingIntensity");
        private static readonly FieldInfo aircraftField =
            AccessTools.Field(typeof(Countermeasure), "aircraft");

        // Track original values per instance
        private static readonly System.Collections.Generic.Dictionary<int, float> origIntensity = new();

        [HarmonyPrefix]
        public static void Prefix(RadarJammer __instance)
        {
            var aircraft = aircraftField?.GetValue(__instance) as Aircraft;
            if (aircraft == null) return;

            int id = __instance.GetInstanceID();

            if (Plugin.IsKR67EX(aircraft))
            {
                if (!origIntensity.ContainsKey(id))
                    origIntensity[id] = (float)intensityField.GetValue(__instance);

                intensityField.SetValue(__instance, Plugin.JammerIntensity.Value);
            }
            else if (origIntensity.ContainsKey(id))
            {
                intensityField.SetValue(__instance, origIntensity[id]);
            }
        }
    }
}

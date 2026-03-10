using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace KR67R.Patches
{
    /// <summary>
    /// Set jammer intensity to 12 for the KR-67R.
    /// </summary>
    [HarmonyPatch(typeof(RadarJammer), "Fire")]
    public static class JammerPatch
    {
        private static readonly FieldInfo intensityField =
            AccessTools.Field(typeof(RadarJammer), "jammingIntensity");
        private static readonly FieldInfo aircraftField =
            AccessTools.Field(typeof(Countermeasure), "aircraft");

        private static readonly System.Collections.Generic.Dictionary<int, float> origIntensity = new();

        [HarmonyPrefix]
        public static void Prefix(RadarJammer __instance)
        {
            var aircraft = aircraftField?.GetValue(__instance) as Aircraft;
            if (aircraft == null) return;

            int id = __instance.GetInstanceID();

            if (Plugin.IsKR67R(aircraft))
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

using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace IfritVariants.Patches
{
    /// <summary>
    /// Set jammer intensity for both KR-67EX and KR-67R.
    /// </summary>
    [HarmonyPatch(typeof(RadarJammer), "Fire")]
    public static class JammerPatch
    {
        private static readonly FieldInfo intensityField =
            AccessTools.Field(typeof(RadarJammer), "jammingIntensity");
        private static readonly FieldInfo aircraftField =
            AccessTools.Field(typeof(Countermeasure), "aircraft");

        private static readonly Dictionary<int, float> origIntensity = new();

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
                intensityField.SetValue(__instance, Plugin.EX_JammerIntensity.Value);
            }
            else if (Plugin.IsKR67R(aircraft))
            {
                if (!origIntensity.ContainsKey(id))
                    origIntensity[id] = (float)intensityField.GetValue(__instance);
                intensityField.SetValue(__instance, Plugin.R_JammerIntensity.Value);
            }
            else if (origIntensity.ContainsKey(id))
            {
                intensityField.SetValue(__instance, origIntensity[id]);
            }
        }
    }
}

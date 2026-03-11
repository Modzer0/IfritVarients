using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace IfritVariants.Patches.R
{
    /// <summary>
    /// KR-67R engine: 85,000ft ceiling, Mach 5 at 80,000ft, 3x thrust.
    /// Disables stock minDensity flameout, uses altitudeThrust curve to taper,
    /// and Postfix zeros thrust above service ceiling for hard flameout.
    /// </summary>
    [HarmonyPatch(typeof(Turbojet), "FixedUpdate")]
    public static class EnginePatch
    {
        private static readonly FieldInfo maxThrustField = AccessTools.Field(typeof(Turbojet), "maxThrust");
        private static readonly FieldInfo maxSpeedField = AccessTools.Field(typeof(Turbojet), "maxSpeed");
        private static readonly FieldInfo minDensityField = AccessTools.Field(typeof(Turbojet), "minDensity");
        private static readonly FieldInfo altitudeThrustField = AccessTools.Field(typeof(Turbojet), "altitudeThrust");
        private static readonly FieldInfo aircraftField = AccessTools.Field(typeof(Turbojet), "aircraft");
        private static readonly FieldInfo thrustField = AccessTools.Field(typeof(Turbojet), "thrust");

        private static readonly Dictionary<int, float> origMaxThrust = new();
        private static bool logged;

        // Full thrust up to cruise alt, tapers to 0 at ceiling
        private static readonly AnimationCurve altCurve;

        static EnginePatch()
        {
            altCurve = new AnimationCurve(
                new Keyframe(0f, 1f),
                new Keyframe(10000f, 0.9f),
                new Keyframe(Plugin.R_CruiseAltMeters, 0.6f),       // 80,000 ft
                new Keyframe(Plugin.R_ServiceCeilingMeters, 0f)      // 85,000 ft — hard zero
            );
            altCurve.preWrapMode = WrapMode.ClampForever;
            altCurve.postWrapMode = WrapMode.ClampForever;
        }

        [HarmonyPrefix]
        public static void Prefix(Turbojet __instance)
        {
            Aircraft aircraft = aircraftField?.GetValue(__instance) as Aircraft;
            if (aircraft == null) return;
            if (!Plugin.IsKR67R(aircraft)) return;

            int id = __instance.GetInstanceID();

            if (!origMaxThrust.ContainsKey(id))
                origMaxThrust[id] = (float)maxThrustField.GetValue(__instance);

            maxThrustField.SetValue(__instance, origMaxThrust[id] * 3f);
            maxSpeedField.SetValue(__instance, Plugin.R_Mach5At80kFt);

            // Disable stock density flameout — our altitude curve handles the ceiling
            if ((float)minDensityField.GetValue(__instance) > -0.5f)
                minDensityField.SetValue(__instance, -1f);

            altitudeThrustField.SetValue(__instance, altCurve);

            if (!logged)
            {
                logged = true;
                Plugin.Log($"R engine: ceiling={Plugin.R_ServiceCeilingMeters}m, speed={Plugin.R_Mach5At80kFt}m/s");
            }
        }

        [HarmonyPostfix]
        public static void Postfix(Turbojet __instance)
        {
            Aircraft aircraft = aircraftField?.GetValue(__instance) as Aircraft;
            if (aircraft == null) return;
            if (!Plugin.IsKR67R(aircraft)) return;

            // Hard flameout above service ceiling
            float altitude = __instance.transform.position.y - Datum.LocalSeaY;
            if (altitude >= Plugin.R_ServiceCeilingMeters)
                thrustField.SetValue(__instance, 0f);
        }
    }
}

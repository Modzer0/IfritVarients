using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace IfritVariants.Patches.R
{
    /// <summary>
    /// KR-67R engine: 85,000ft ceiling, Mach 5 at 80,000ft, 3x thrust.
    /// Uses flat altitude curve with ClampForever (matching Ifrit X mod approach).
    /// </summary>
    [HarmonyPatch(typeof(Turbojet), "FixedUpdate")]
    public static class EnginePatch
    {
        private static readonly FieldInfo maxThrustField = AccessTools.Field(typeof(Turbojet), "maxThrust");
        private static readonly FieldInfo maxSpeedField = AccessTools.Field(typeof(Turbojet), "maxSpeed");
        private static readonly FieldInfo minDensityField = AccessTools.Field(typeof(Turbojet), "minDensity");
        private static readonly FieldInfo altitudeThrustField = AccessTools.Field(typeof(Turbojet), "altitudeThrust");
        private static readonly FieldInfo aircraftField = AccessTools.Field(typeof(Turbojet), "aircraft");

        private static readonly Dictionary<int, float> origMaxThrust = new();
        private static bool logged;

        // Flat curve: full thrust at all altitudes, ClampForever so it never extrapolates to 0
        private static readonly AnimationCurve flatCurve;

        static EnginePatch()
        {
            flatCurve = new AnimationCurve(
                new Keyframe(0f, 1f),
                new Keyframe(Plugin.R_ServiceCeilingMeters, 1f)
            );
            flatCurve.preWrapMode = WrapMode.ClampForever;
            flatCurve.postWrapMode = WrapMode.ClampForever;
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

            if ((float)minDensityField.GetValue(__instance) > -0.5f)
                minDensityField.SetValue(__instance, -1f);

            altitudeThrustField.SetValue(__instance, flatCurve);

            if (!logged)
            {
                logged = true;
                Plugin.Log($"R engine: ceiling={Plugin.R_ServiceCeilingMeters}m, speed={Plugin.R_Mach5At80kFt}m/s, flat altitude curve with ClampForever");
            }
        }
    }
}

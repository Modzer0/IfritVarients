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

        // Thrust tapers above cruise alt, hits 0 above ceiling. ClampForever keeps it at 0 beyond.
        private static readonly AnimationCurve altCurve;

        static EnginePatch()
        {
            altCurve = new AnimationCurve(
                new Keyframe(0f, 1f),
                new Keyframe(10000f, 0.9f),
                new Keyframe(Plugin.R_CruiseAltMeters, 0.6f),       // 80,000 ft
                new Keyframe(Plugin.R_ServiceCeilingMeters, 0.3f),   // 85,000 ft
                new Keyframe(Plugin.R_ServiceCeilingMeters + 2000f, 0f)
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

            if ((float)minDensityField.GetValue(__instance) > -0.5f)
                minDensityField.SetValue(__instance, -1f);

            altitudeThrustField.SetValue(__instance, altCurve);

            if (!logged)
            {
                logged = true;
                Plugin.Log($"R engine: ceiling={Plugin.R_ServiceCeilingMeters}m, speed={Plugin.R_Mach5At80kFt}m/s, flat altitude curve with ClampForever");
            }
        }
    }
}

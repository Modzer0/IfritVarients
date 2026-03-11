using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace IfritVariants.Patches.EX
{
    [HarmonyPatch(typeof(Turbojet), "FixedUpdate")]
    public static class ThrustPatch
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

        static ThrustPatch()
        {
            flatCurve = new AnimationCurve(
                new Keyframe(0f, 1f),
                new Keyframe(Plugin.EX_ServiceCeilingMeters, 1f)
            );
            flatCurve.preWrapMode = WrapMode.ClampForever;
            flatCurve.postWrapMode = WrapMode.ClampForever;
        }

        [HarmonyPrefix]
        public static void Prefix(Turbojet __instance)
        {
            Aircraft aircraft = aircraftField?.GetValue(__instance) as Aircraft;
            if (aircraft == null) return;
            if (!Plugin.IsKR67EX(aircraft)) return;

            int id = __instance.GetInstanceID();

            if (!origMaxThrust.ContainsKey(id))
                origMaxThrust[id] = (float)maxThrustField.GetValue(__instance);

            maxThrustField.SetValue(__instance, origMaxThrust[id] * Plugin.EX_ThrustMultiplier.Value);
            maxSpeedField.SetValue(__instance, float.MaxValue);

            if ((float)minDensityField.GetValue(__instance) > -0.5f)
                minDensityField.SetValue(__instance, -1f);

            altitudeThrustField.SetValue(__instance, flatCurve);

            if (!logged)
            {
                logged = true;
                Plugin.Log($"EX engine: ceiling={Plugin.EX_ServiceCeilingMeters}m, thrust x{Plugin.EX_ThrustMultiplier.Value}, flat altitude curve with ClampForever");
            }
        }
    }
}

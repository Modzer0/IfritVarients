using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace IfritVariants.Patches.R
{
    /// <summary>
    /// KR-67R engine: 85,000ft ceiling, Mach 5 at 80,000ft, 3x thrust.
    /// </summary>
    [HarmonyPatch(typeof(Turbojet), "FixedUpdate")]
    public static class EnginePatch
    {
        private static readonly FieldInfo maxThrustField = AccessTools.Field(typeof(Turbojet), "maxThrust");
        private static readonly FieldInfo maxSpeedField = AccessTools.Field(typeof(Turbojet), "maxSpeed");
        private static readonly FieldInfo minDensityField = AccessTools.Field(typeof(Turbojet), "minDensity");
        private static readonly FieldInfo altitudeThrustField = AccessTools.Field(typeof(Turbojet), "altitudeThrust");

        private static readonly Dictionary<int, float> origMaxThrust = new();
        private static bool logged;

        [HarmonyPrefix]
        public static void Prefix(Turbojet __instance)
        {
            Aircraft aircraft = __instance.GetComponentInParent<Aircraft>();
            if (aircraft == null) return;
            if (!Plugin.IsKR67R(aircraft)) return;

            int id = __instance.GetInstanceID();

            if (!origMaxThrust.ContainsKey(id))
                origMaxThrust[id] = (float)maxThrustField.GetValue(__instance);

            maxThrustField.SetValue(__instance, origMaxThrust[id] * 3f);
            maxSpeedField.SetValue(__instance, Plugin.R_Mach5At80kFt);
            minDensityField.SetValue(__instance, -1f);
            altitudeThrustField.SetValue(__instance, new AnimationCurve(
                new Keyframe(0f, 1f),
                new Keyframe(10000f, 0.9f),
                new Keyframe(Plugin.R_CruiseAltMeters, 0.6f),
                new Keyframe(Plugin.R_ServiceCeilingMeters, 0.3f),
                new Keyframe(Plugin.R_ServiceCeilingMeters + 2000f, 0f)
            ));

            if (!logged) { logged = true; Plugin.Log($"R engine: ceiling={Plugin.R_ServiceCeilingMeters}m, speed={Plugin.R_Mach5At80kFt}m/s"); }
        }
    }
}

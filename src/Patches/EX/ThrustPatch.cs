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

        private static readonly Dictionary<int, float> origMaxThrust = new();
        private static readonly Dictionary<int, float> origMaxSpeed = new();
        private static readonly Dictionary<int, float> origMinDensity = new();
        private static readonly Dictionary<int, AnimationCurve> origAltCurve = new();

        [HarmonyPrefix]
        [HarmonyBefore("com.nuclearoption.ifritvariants.r.engine")]
        public static void Prefix(Turbojet __instance)
        {
            Aircraft aircraft = __instance.GetComponentInParent<Aircraft>();
            if (aircraft == null) return;

            int id = __instance.GetInstanceID();

            if (Plugin.IsKR67EX(aircraft))
            {
                if (!origMaxThrust.ContainsKey(id))
                {
                    origMaxThrust[id] = (float)maxThrustField.GetValue(__instance);
                    origMaxSpeed[id] = (float)maxSpeedField.GetValue(__instance);
                    origMinDensity[id] = (float)minDensityField.GetValue(__instance);
                    origAltCurve[id] = (AnimationCurve)altitudeThrustField.GetValue(__instance);
                }

                float baseThrust = origMaxThrust[id];
                maxThrustField.SetValue(__instance, baseThrust * Plugin.EX_ThrustMultiplier.Value);
                maxSpeedField.SetValue(__instance, float.MaxValue); // No speed limit
                minDensityField.SetValue(__instance, -1f);

                AnimationCurve flatCurve = new AnimationCurve(
                    new Keyframe(0f, 1f),
                    new Keyframe(10000f, 0.85f),
                    new Keyframe(Plugin.EX_ServiceCeilingMeters, 0.4f),
                    new Keyframe(Plugin.EX_ServiceCeilingMeters + 2000f, 0f)
                );
                altitudeThrustField.SetValue(__instance, flatCurve);
            }
            else if (origMaxThrust.ContainsKey(id))
            {
                maxThrustField.SetValue(__instance, origMaxThrust[id]);
                maxSpeedField.SetValue(__instance, origMaxSpeed[id]);
                minDensityField.SetValue(__instance, origMinDensity[id]);
                altitudeThrustField.SetValue(__instance, origAltCurve[id]);
            }
        }
    }
}

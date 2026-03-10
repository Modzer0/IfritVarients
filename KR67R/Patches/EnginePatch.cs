using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace KR67R.Patches
{
    /// <summary>
    /// Engine modifications for KR-67R:
    /// - Service ceiling: 85,000ft (25,908m)
    /// - At 80,000ft on afterburner: Mach 5 (1,450 m/s)
    /// - Speed of sound at 80,000ft ≈ 290 m/s (game formula floors at 290)
    /// 
    /// To achieve Mach 5 at 80,000ft:
    /// - maxSpeed raised to 1,450 m/s (speed limiter won't kick in until then)
    /// - minDensity bypassed (-1) for high altitude operation
    /// - altitudeThrust curve flattened to maintain thrust up to 85,000ft
    /// - Thrust needs significant increase to overcome drag at Mach 5
    /// </summary>
    [HarmonyPatch(typeof(Turbojet), "FixedUpdate")]
    public static class EnginePatch
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
        public static void Prefix(Turbojet __instance)
        {
            Aircraft aircraft = __instance.GetComponentInParent<Aircraft>();
            if (aircraft == null) return;

            int id = __instance.GetInstanceID();

            if (Plugin.IsKR67R(aircraft))
            {
                if (!origMaxThrust.ContainsKey(id))
                {
                    origMaxThrust[id] = (float)maxThrustField.GetValue(__instance);
                    origMaxSpeed[id] = (float)maxSpeedField.GetValue(__instance);
                    origMinDensity[id] = (float)minDensityField.GetValue(__instance);
                    origAltCurve[id] = (AnimationCurve)altitudeThrustField.GetValue(__instance);
                }

                // Significant thrust increase needed for Mach 5 at altitude
                // At 80,000ft air density is very low — need ~3x thrust to push through
                float baseThrust = origMaxThrust[id];
                maxThrustField.SetValue(__instance, baseThrust * 3f);

                // Speed limit at Mach 5 at 80,000ft
                maxSpeedField.SetValue(__instance, Plugin.Mach5At80kFt);

                // Bypass density gate
                minDensityField.SetValue(__instance, -1f);

                // Altitude thrust curve for 85,000ft ceiling
                AnimationCurve curve = new AnimationCurve(
                    new Keyframe(0f, 1f),
                    new Keyframe(10000f, 0.9f),
                    new Keyframe(Plugin.CruiseAltMeters, 0.6f),   // 80,000ft — still good thrust
                    new Keyframe(Plugin.ServiceCeilingMeters, 0.3f), // 85,000ft — reduced but functional
                    new Keyframe(Plugin.ServiceCeilingMeters + 2000f, 0f) // Above ceiling — zero
                );
                altitudeThrustField.SetValue(__instance, curve);
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

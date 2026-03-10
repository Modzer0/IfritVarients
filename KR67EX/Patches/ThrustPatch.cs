using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace KR67EX.Patches
{
    /// <summary>
    /// 20% thrust increase, raised speed limit to Mach 2.5, and extended service ceiling to 60,000ft.
    /// Patches Turbojet.FixedUpdate to modify thrust, maxSpeed, altitudeThrust curve, and minDensity.
    /// </summary>
    [HarmonyPatch(typeof(Turbojet), "FixedUpdate")]
    public static class ThrustPatch
    {
        private static readonly FieldInfo maxThrustField = AccessTools.Field(typeof(Turbojet), "maxThrust");
        private static readonly FieldInfo maxSpeedField = AccessTools.Field(typeof(Turbojet), "maxSpeed");
        private static readonly FieldInfo minDensityField = AccessTools.Field(typeof(Turbojet), "minDensity");
        private static readonly FieldInfo altitudeThrustField = AccessTools.Field(typeof(Turbojet), "altitudeThrust");

        // Store original values per engine instance to restore when not our variant
        private static readonly Dictionary<int, float> origMaxThrust = new();
        private static readonly Dictionary<int, float> origMaxSpeed = new();
        private static readonly Dictionary<int, float> origMinDensity = new();
        private static readonly Dictionary<int, AnimationCurve> origAltCurve = new();

        private static readonly System.Collections.Generic.Dictionary<int, float> placeholder = new();

        [HarmonyPrefix]
        public static void Prefix(Turbojet __instance)
        {
            Aircraft aircraft = __instance.GetComponentInParent<Aircraft>();
            if (aircraft == null) return;

            int id = __instance.GetInstanceID();

            if (Plugin.IsKR67EX(aircraft))
            {
                // Save originals on first encounter
                if (!origMaxThrust.ContainsKey(id))
                {
                    origMaxThrust[id] = (float)maxThrustField.GetValue(__instance);
                    origMaxSpeed[id] = (float)maxSpeedField.GetValue(__instance);
                    origMinDensity[id] = (float)minDensityField.GetValue(__instance);
                    origAltCurve[id] = (AnimationCurve)altitudeThrustField.GetValue(__instance);
                }

                // Apply KR-67EX modifications
                float baseThrust = origMaxThrust[id];
                maxThrustField.SetValue(__instance, baseThrust * Plugin.ThrustMultiplier.Value);
                maxSpeedField.SetValue(__instance, Plugin.MaxSpeedMs.Value);
                minDensityField.SetValue(__instance, -1f); // Bypass density gate for high altitude

                // Flatten altitude thrust curve for 60,000ft ceiling
                AnimationCurve flatCurve = new AnimationCurve(
                    new Keyframe(0f, 1f),
                    new Keyframe(10000f, 0.85f),
                    new Keyframe(Plugin.ServiceCeilingMeters, 0.4f),
                    new Keyframe(Plugin.ServiceCeilingMeters + 2000f, 0f)
                );
                altitudeThrustField.SetValue(__instance, flatCurve);
            }
            else if (origMaxThrust.ContainsKey(id))
            {
                // Restore originals for non-variant aircraft sharing the prefab
                maxThrustField.SetValue(__instance, origMaxThrust[id]);
                maxSpeedField.SetValue(__instance, origMaxSpeed[id]);
                minDensityField.SetValue(__instance, origMinDensity[id]);
                altitudeThrustField.SetValue(__instance, origAltCurve[id]);
            }
        }
    }
}

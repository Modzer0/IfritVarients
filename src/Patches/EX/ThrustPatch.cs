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
        private static bool logged;

        [HarmonyPrefix]
        public static void Prefix(Turbojet __instance)
        {
            Aircraft aircraft = __instance.GetComponentInParent<Aircraft>();
            if (aircraft == null) return;
            if (!Plugin.IsKR67EX(aircraft)) return;

            int id = __instance.GetInstanceID();

            if (!origMaxThrust.ContainsKey(id))
                origMaxThrust[id] = (float)maxThrustField.GetValue(__instance);

            maxThrustField.SetValue(__instance, origMaxThrust[id] * Plugin.EX_ThrustMultiplier.Value);
            maxSpeedField.SetValue(__instance, float.MaxValue);
            minDensityField.SetValue(__instance, -1f);
            altitudeThrustField.SetValue(__instance, new AnimationCurve(
                new Keyframe(0f, 1f),
                new Keyframe(10000f, 0.85f),
                new Keyframe(Plugin.EX_ServiceCeilingMeters, 0.4f),
                new Keyframe(Plugin.EX_ServiceCeilingMeters + 2000f, 0f)
            ));

            if (!logged) { logged = true; Plugin.Log($"EX engine: ceiling={Plugin.EX_ServiceCeilingMeters}m, thrust x{Plugin.EX_ThrustMultiplier.Value}"); }
        }
    }
}

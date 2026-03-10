using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace KR67EX.Patches
{
    /// <summary>
    /// Increase G overload to 12Gs for the KR-67EX.
    /// Patches:
    ///   - ControlsFilter.Filter (postfix) — override FBW gLimitPositive and GLimiter gLimit
    ///   - GForceDamage threshold is on AircraftParameters.aircraftGLimit (set in Encyclopedia clone)
    /// </summary>
    [HarmonyPatch(typeof(ControlsFilter), "Filter")]
    public static class GLimitFilterPatch
    {
        // FlyByWire nested class fields
        private static readonly FieldInfo fbwField =
            AccessTools.Field(typeof(ControlsFilter), "flyByWire");
        private static readonly FieldInfo aircraftField =
            AccessTools.Field(typeof(ControlsFilter), "aircraft");

        // FlyByWire.gLimitPositive
        private static FieldInfo fbwGLimitField;
        // GLimiter fields
        private static readonly FieldInfo glimiterField =
            AccessTools.Field(typeof(ControlsFilter), "gLimiter");
        private static FieldInfo glimiterGLimitField;

        // Track originals
        private static readonly System.Collections.Generic.Dictionary<int, float> origFbwGLimit = new();
        private static readonly System.Collections.Generic.Dictionary<int, float> origGLimiterGLimit = new();

        [HarmonyPrefix]
        public static void Prefix(ControlsFilter __instance)
        {
            var aircraft = aircraftField?.GetValue(__instance) as Aircraft;
            if (aircraft == null) return;

            int id = __instance.GetInstanceID();

            // Lazy-init nested type field references
            if (fbwGLimitField == null)
            {
                var fbwObj = fbwField?.GetValue(__instance);
                if (fbwObj != null)
                    fbwGLimitField = AccessTools.Field(fbwObj.GetType(), "gLimitPositive");
            }
            if (glimiterGLimitField == null && glimiterField != null)
            {
                var glObj = glimiterField?.GetValue(__instance);
                if (glObj != null)
                    glimiterGLimitField = AccessTools.Field(glObj.GetType(), "gLimit");
            }

            if (Plugin.IsKR67EX(aircraft))
            {
                // Override FBW G limit
                var fbw = fbwField?.GetValue(__instance);
                if (fbw != null && fbwGLimitField != null)
                {
                    if (!origFbwGLimit.ContainsKey(id))
                        origFbwGLimit[id] = (float)fbwGLimitField.GetValue(fbw);
                    fbwGLimitField.SetValue(fbw, Plugin.GLimit.Value);
                }

                // Override structural G limiter
                if (glimiterField != null)
                {
                    var gl = glimiterField?.GetValue(__instance);
                    if (gl != null && glimiterGLimitField != null)
                    {
                        if (!origGLimiterGLimit.ContainsKey(id))
                            origGLimiterGLimit[id] = (float)glimiterGLimitField.GetValue(gl);
                        glimiterGLimitField.SetValue(gl, Plugin.GLimit.Value);
                    }
                }
            }
            else
            {
                // Restore originals for non-variant
                var fbw = fbwField?.GetValue(__instance);
                if (fbw != null && fbwGLimitField != null && origFbwGLimit.ContainsKey(id))
                    fbwGLimitField.SetValue(fbw, origFbwGLimit[id]);

                if (glimiterField != null)
                {
                    var gl = glimiterField?.GetValue(__instance);
                    if (gl != null && glimiterGLimitField != null && origGLimiterGLimit.ContainsKey(id))
                        glimiterGLimitField.SetValue(gl, origGLimiterGLimit[id]);
                }
            }
        }
    }
}

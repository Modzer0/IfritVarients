using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace IfritVariants.Patches.EX
{
    [HarmonyPatch(typeof(ControlsFilter), "Filter")]
    public static class GLimitFilterPatch
    {
        private static readonly FieldInfo fbwField =
            AccessTools.Field(typeof(ControlsFilter), "flyByWire");
        private static readonly FieldInfo aircraftField =
            AccessTools.Field(typeof(ControlsFilter), "aircraft");
        private static readonly FieldInfo glimiterField =
            AccessTools.Field(typeof(ControlsFilter), "gLimiter");

        private static FieldInfo fbwGLimitField;
        private static FieldInfo glimiterGLimitField;

        private static readonly Dictionary<int, float> origFbwGLimit = new();
        private static readonly Dictionary<int, float> origGLimiterGLimit = new();

        [HarmonyPrefix]
        public static void Prefix(ControlsFilter __instance)
        {
            var aircraft = aircraftField?.GetValue(__instance) as Aircraft;
            if (aircraft == null) return;

            int id = __instance.GetInstanceID();

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
                var fbw = fbwField?.GetValue(__instance);
                if (fbw != null && fbwGLimitField != null)
                {
                    if (!origFbwGLimit.ContainsKey(id))
                        origFbwGLimit[id] = (float)fbwGLimitField.GetValue(fbw);
                    fbwGLimitField.SetValue(fbw, Plugin.EX_GLimit.Value);
                }
                if (glimiterField != null)
                {
                    var gl = glimiterField?.GetValue(__instance);
                    if (gl != null && glimiterGLimitField != null)
                    {
                        if (!origGLimiterGLimit.ContainsKey(id))
                            origGLimiterGLimit[id] = (float)glimiterGLimitField.GetValue(gl);
                        glimiterGLimitField.SetValue(gl, Plugin.EX_GLimit.Value);
                    }
                }
            }
            else
            {
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

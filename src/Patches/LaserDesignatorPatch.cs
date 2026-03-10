using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace IfritVariants.Patches
{
    /// <summary>
    /// Set LaserDesignator maxTargets to 12 for cooperative strikes on both variants.
    /// </summary>
    [HarmonyPatch(typeof(LaserDesignator), "Awake")]
    public static class LaserDesignatorPatch
    {
        private static readonly FieldInfo maxTargetsField =
            AccessTools.Field(typeof(LaserDesignator), "maxTargets");
        private static readonly FieldInfo aircraftField =
            AccessTools.Field(typeof(LaserDesignator), "aircraft");

        [HarmonyPostfix]
        public static void Postfix(LaserDesignator __instance)
        {
            var aircraft = aircraftField?.GetValue(__instance) as Aircraft;
            if (Plugin.IsKR67EX(aircraft) || Plugin.IsKR67R(aircraft))
            {
                maxTargetsField.SetValue(__instance, 12);
            }
        }
    }
}

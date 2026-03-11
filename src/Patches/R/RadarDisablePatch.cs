using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace IfritVariants.Patches.R
{
    /// <summary>
    /// Keep radar permanently off on KR-67R. The initial disable happens in
    /// SpawnerPatch.ApplyKR67R, but we also need to block FixedUpdate power draw
    /// and prevent the player from toggling it back on.
    /// </summary>
    [HarmonyPatch(typeof(Radar), "FixedUpdate")]
    public static class RadarFixedUpdatePatch
    {
        [HarmonyPrefix]
        public static bool Prefix(Radar __instance)
        {
            // Skip FixedUpdate entirely for KR-67R radars (no power draw, no scanning)
            var unit = AccessTools.Field(typeof(TargetDetector), "attachedUnit")
                ?.GetValue(__instance) as Unit;
            if (unit is Aircraft ac && Plugin.IsKR67R(ac))
            {
                __instance.activated = false;
                return false;
            }
            return true;
        }
    }

    /// <summary>
    /// Prevent the player from toggling radar on for the KR-67R.
    /// </summary>
    [HarmonyPatch(typeof(Aircraft), "UserCode_CmdToggleRadar_1821461427")]
    public static class RadarTogglePatch
    {
        [HarmonyPrefix]
        public static bool Prefix(Aircraft __instance)
        {
            if (Plugin.IsKR67R(__instance))
                return false;
            return true;
        }
    }
}

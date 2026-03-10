using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace IfritVariants.Patches.R
{
    /// <summary>
    /// Enhanced optical sensor for KR-67R: 50NM range, bottom-arc only detection.
    /// </summary>
    [HarmonyPatch(typeof(TargetDetector), "Awake")]
    public static class OpticalSensorAwakePatch
    {
        private static readonly FieldInfo visualRangeField =
            AccessTools.Field(typeof(TargetDetector), "visualRange");
        private static readonly FieldInfo magnificationField =
            AccessTools.Field(typeof(TargetDetector), "magnification");
        private static readonly FieldInfo attachedUnitField =
            AccessTools.Field(typeof(TargetDetector), "attachedUnit");

        [HarmonyPostfix]
        public static void Postfix(TargetDetector __instance)
        {
            if (__instance is Radar) return;

            var unit = attachedUnitField?.GetValue(__instance) as Unit;
            var aircraft = unit as Aircraft;
            if (!Plugin.IsKR67R(aircraft)) return;

            float opticalRange = Plugin.GetOpticalRangeMeters();
            visualRangeField.SetValue(__instance, opticalRange);
            magnificationField.SetValue(__instance, 4f);
        }
    }

    /// <summary>
    /// Filter visual detection to bottom-arc only for KR-67R.
    /// </summary>
    [HarmonyPatch(typeof(TargetDetector), "TargetSearch")]
    public static class OpticalSensorArcPatch
    {
        private static readonly FieldInfo attachedUnitField =
            AccessTools.Field(typeof(TargetDetector), "attachedUnit");
        private static readonly FieldInfo scannerField =
            AccessTools.Field(typeof(TargetDetector), "scanner");

        [HarmonyPostfix]
        public static void Postfix(TargetDetector __instance)
        {
            if (__instance is Radar) return;

            var unit = attachedUnitField?.GetValue(__instance) as Unit;
            var aircraft = unit as Aircraft;
            if (!Plugin.IsKR67R(aircraft)) return;

            var scanner = scannerField?.GetValue(__instance) as Transform;
            if (scanner == null) return;

            float scannerY = scanner.position.y;
            __instance.detectedTargets.RemoveAll(target =>
            {
                if (target == null) return true;
                return target.transform.position.y > scannerY;
            });
        }
    }
}

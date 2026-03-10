using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace KR67R.Patches
{
    /// <summary>
    /// Enhanced optical sensor for KR-67R: 50NM range, bottom-arc only detection.
    /// 
    /// The base TargetDetector.VisualCheck is omnidirectional — it uses range + LoS raycast.
    /// For the KR-67R, we:
    ///   1. Set visualRange to 92,600m (50NM) on the TargetDetector
    ///   2. Filter VisualCheck to only detect targets below the aircraft (bottom arc)
    ///      by checking that the target is below the scanner's horizontal plane
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
            // Only modify the base TargetDetector, not Radar subclass
            if (__instance is Radar) return;

            var unit = attachedUnitField?.GetValue(__instance) as Unit;
            var aircraft = unit as Aircraft;
            if (!Plugin.IsKR67R(aircraft)) return;

            // Set optical sensor range to 50NM
            float opticalRange = Plugin.GetOpticalRangeMeters();
            visualRangeField.SetValue(__instance, opticalRange);

            // Increase magnification for long-range optical detection
            magnificationField.SetValue(__instance, 4f);

            Debug.Log($"[KR-67R] Optical sensor configured: range={opticalRange}m ({Plugin.OpticalRangeNM.Value}NM), magnification=4");
        }
    }

    /// <summary>
    /// Filter visual detection to bottom-arc only for KR-67R.
    /// After VisualCheck runs, remove any detected targets that are above the aircraft.
    /// The optical sensor only sees contacts visible from below the aircraft.
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
            // Only filter the base TargetDetector, not Radar subclass
            if (__instance is Radar) return;

            var unit = attachedUnitField?.GetValue(__instance) as Unit;
            var aircraft = unit as Aircraft;
            if (!Plugin.IsKR67R(aircraft)) return;

            var scanner = scannerField?.GetValue(__instance) as Transform;
            if (scanner == null) return;

            // Remove targets that are above the aircraft (not in bottom arc)
            // The optical sensor only works for contacts visible from below
            float scannerY = scanner.position.y;
            __instance.detectedTargets.RemoveAll(target =>
            {
                if (target == null) return true;
                // Target must be below the scanner position
                return target.transform.position.y > scannerY;
            });
        }
    }
}

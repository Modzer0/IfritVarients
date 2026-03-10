using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace KR67EX.Patches
{
    /// <summary>
    /// Set LaserDesignator maxTargets to 12 for cooperative strikes on the KR-67EX.
    /// The KR-67A base has 2 targeting lasers (set by QoL). The EX variant gets 12.
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
            if (Plugin.IsKR67EX(aircraft))
            {
                maxTargetsField.SetValue(__instance, 12);
                Debug.Log("[KR-67EX] Laser designator set to 12 targets for cooperative strikes.");
            }
        }
    }
}

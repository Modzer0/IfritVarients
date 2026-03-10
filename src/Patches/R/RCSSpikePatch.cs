using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace IfritVariants.Patches.R
{
    /// <summary>
    /// When a weapon fires on KR-67R, spike RCS to normal KR-67A levels for 1 second.
    /// </summary>
    [HarmonyPatch(typeof(WeaponStation), "Fire", typeof(Unit), typeof(Unit))]
    public static class RCSSpikePatch
    {
        private static readonly Dictionary<int, float> lastFireTime = new();
        private static readonly Dictionary<int, bool> rcsSpiked = new();
        private static float normalRCS = -1f;

        [HarmonyPostfix]
        public static void Postfix(Unit owner)
        {
            if (owner == null) return;
            var aircraft = owner as Aircraft;
            if (!Plugin.IsKR67R(aircraft)) return;

            int id = aircraft.GetInstanceID();

            if (normalRCS < 0f)
                normalRCS = Plugin.R_CloneDef.radarSize * Plugin.R_RCSDivisor.Value;

            if (!rcsSpiked.ContainsKey(id) || !rcsSpiked[id])
            {
                aircraft.RCS = normalRCS;
                rcsSpiked[id] = true;
            }

            lastFireTime[id] = Time.timeSinceLevelLoad;
        }

        internal static void CheckRCSDecay(Aircraft aircraft)
        {
            if (!Plugin.IsKR67R(aircraft)) return;

            int id = aircraft.GetInstanceID();
            if (!rcsSpiked.ContainsKey(id) || !rcsSpiked[id]) return;
            if (!lastFireTime.ContainsKey(id)) return;

            if (Time.timeSinceLevelLoad - lastFireTime[id] >= 1f)
            {
                aircraft.RCS = Plugin.R_CloneDef.radarSize;
                rcsSpiked[id] = false;
            }
        }
    }

    /// <summary>
    /// Use Turbojet.FixedUpdate as a per-physics-frame hook to check RCS decay.
    /// </summary>
    [HarmonyPatch(typeof(Turbojet), "FixedUpdate")]
    public static class RCSDecayTickPatch
    {
        [HarmonyPostfix]
        public static void Postfix(Turbojet __instance)
        {
            Aircraft aircraft = __instance.GetComponentInParent<Aircraft>();
            if (aircraft == null) return;
            RCSSpikePatch.CheckRCSDecay(aircraft);
        }
    }
}

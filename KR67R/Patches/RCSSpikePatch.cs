using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace KR67R.Patches
{
    /// <summary>
    /// When a weapon is fired on the KR-67R, spike the RCS to normal KR-67A levels
    /// for 1 second. The RCS returns to the stealth (1/100) value 1 second after
    /// the last weapon fire.
    ///
    /// Weapon bay doors opening exposes the aircraft's cross-section to radar.
    /// </summary>
    [HarmonyPatch(typeof(WeaponStation), "Fire", typeof(Unit), typeof(Unit))]
    public static class RCSSpikePatch
    {
        // Track last fire time per aircraft instance
        private static readonly Dictionary<int, float> lastFireTime = new();
        // Track whether RCS is currently spiked per aircraft instance
        private static readonly Dictionary<int, bool> rcsSpiked = new();
        // Store the normal (unspiked) KR-67A RCS value
        private static float normalRCS = -1f;

        [HarmonyPostfix]
        public static void Postfix(Unit owner)
        {
            if (owner == null) return;
            var aircraft = owner as Aircraft;
            if (!Plugin.IsKR67R(aircraft)) return;

            int id = aircraft.GetInstanceID();

            // Cache the normal KR-67A RCS on first encounter
            if (normalRCS < 0f)
            {
                // Normal RCS = stealth RCS * divisor (i.e. the original KR-67A value)
                normalRCS = Plugin.CloneDef.radarSize * Plugin.RCSDivisor.Value;
            }

            // Spike RCS to normal KR-67A level
            float stealthRCS = Plugin.CloneDef.radarSize;
            if (!rcsSpiked.ContainsKey(id) || !rcsSpiked[id])
            {
                // Raise RCS from stealth to normal
                aircraft.RCS = normalRCS;
                rcsSpiked[id] = true;
            }

            // Record fire time
            lastFireTime[id] = Time.timeSinceLevelLoad;
        }

        /// <summary>
        /// Check every FixedUpdate on Aircraft if the RCS spike should decay.
        /// We patch Aircraft's FixedUpdate-equivalent — but since Aircraft doesn't
        /// have a convenient FixedUpdate, we use Unit.FixedUpdate or a lightweight
        /// approach via the Turbojet patch (which runs every physics frame).
        /// </summary>
        internal static void CheckRCSDecay(Aircraft aircraft)
        {
            if (!Plugin.IsKR67R(aircraft)) return;

            int id = aircraft.GetInstanceID();
            if (!rcsSpiked.ContainsKey(id) || !rcsSpiked[id]) return;

            if (!lastFireTime.ContainsKey(id)) return;

            float elapsed = Time.timeSinceLevelLoad - lastFireTime[id];
            if (elapsed >= 1f)
            {
                // Return to stealth RCS
                aircraft.RCS = Plugin.CloneDef.radarSize;
                rcsSpiked[id] = false;
            }
        }
    }

    /// <summary>
    /// Use Turbojet.FixedUpdate as a per-physics-frame hook to check RCS decay.
    /// This runs every frame for every engine on every aircraft — we filter to KR-67R only.
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

using HarmonyLib;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace IfritVariants.Patches.EX
{
    /// <summary>
    /// Add AAM-29 (AAM2) quad and AAM-36 (AAM4) quad to inner/outer wing pylons.
    /// Modifies the shared prefab so WeaponChecker.VetLoadout won't strip them.
    /// </summary>
    [HarmonyPatch(typeof(Encyclopedia), "AfterLoad")]
    public static class WeaponPatch
    {
        [HarmonyPrefix]
        [HarmonyPriority(Priority.Low)]
        public static void Prefix(Encyclopedia __instance)
        {
            AircraftDefinition original = __instance.aircraft
                .FirstOrDefault(a => a.jsonKey == Plugin.OriginalJsonKey);
            if (original?.unitPrefab == null) return;

            WeaponManager wm = original.unitPrefab.GetComponentInChildren<WeaponManager>();
            if (wm == null)
            {
                Aircraft ac = original.unitPrefab.GetComponent<Aircraft>();
                if (ac != null) wm = ac.weaponManager;
            }
            if (wm == null) return;

            WeaponMount aam29Quad = FindMount("AAM2_quad_internalP");
            WeaponMount aam36Quad = FindMount("AAM4_quad_internalP");

            int[] pylonIndices = { 4, 5 };
            foreach (int idx in pylonIndices)
            {
                if (idx >= wm.hardpointSets.Length) continue;
                var options = wm.hardpointSets[idx].weaponOptions;

                if (aam29Quad != null && !options.Contains(aam29Quad))
                    options.Add(aam29Quad);
                if (aam36Quad != null && !options.Contains(aam36Quad))
                    options.Add(aam36Quad);
            }
        }

        private static WeaponMount FindMount(string name)
        {
            return Resources.FindObjectsOfTypeAll<WeaponMount>()
                .FirstOrDefault(m => m.name.Equals(name, System.StringComparison.InvariantCultureIgnoreCase));
        }
    }
}

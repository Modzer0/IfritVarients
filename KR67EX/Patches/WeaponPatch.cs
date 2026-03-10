using HarmonyLib;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace KR67EX.Patches
{
    /// <summary>
    /// Add AAM-29 (AAM2) quad and AAM-36 (AAM4) quad mount options to the
    /// KR-67A/EX prefab's inner and outer wing pylons.
    /// 
    /// Multirole1 hardpoint sets (0-indexed):
    ///   0 = Gun (internal)
    ///   1 = Internal Bay Left
    ///   2 = Internal Bay Right
    ///   3 = Wingtip
    ///   4 = Inner Wing Pylons (addweapon5 in QoL = index 4)
    ///   5 = Outer Wing Pylons (addweapon6 in QoL = index 5)
    ///
    /// We add AAM2_quad_internalP (AAM-29 Scythe x4) and AAM4_quad_internalP (AAM-36 Scimitar x4)
    /// to hardpoint sets 4 and 5 on the shared prefab.
    /// 
    /// NOTE: This modifies the PREFAB, so both KR-67A and KR-67EX get the weapon options.
    /// This is necessary because WeaponChecker.VetLoadout validates against the prefab.
    /// If you want KR-67EX-only weapons, you'd need a VetLoadout patch to allow them.
    /// </summary>
    [HarmonyPatch(typeof(Encyclopedia), "AfterLoad")]
    public static class WeaponPatch
    {
        [HarmonyPrefix]
        [HarmonyPriority(Priority.Low)] // Run after EncyclopediaPatch
        public static void Prefix(Encyclopedia __instance)
        {
            // Find the Multirole1 prefab's WeaponManager
            AircraftDefinition original = __instance.aircraft
                .FirstOrDefault(a => a.jsonKey == Plugin.OriginalJsonKey);
            if (original == null || original.unitPrefab == null) return;

            WeaponManager wm = original.unitPrefab.GetComponentInChildren<WeaponManager>();
            if (wm == null)
            {
                // Try getting it from the Aircraft component
                Aircraft ac = original.unitPrefab.GetComponent<Aircraft>();
                if (ac != null)
                    wm = ac.weaponManager;
            }
            if (wm == null)
            {
                Debug.LogWarning("[KR-67EX] Could not find WeaponManager on Multirole1 prefab!");
                return;
            }

            // Find the AAM quad mounts
            WeaponMount aam29Quad = FindMount("AAM2_quad_internalP");
            WeaponMount aam36Quad = FindMount("AAM4_quad_internalP");

            if (aam29Quad == null)
                Debug.LogWarning("[KR-67EX] Could not find AAM2_quad_internalP (AAM-29 Scythe x4) mount!");
            if (aam36Quad == null)
                Debug.LogWarning("[KR-67EX] Could not find AAM4_quad_internalP (AAM-36 Scimitar x4) mount!");

            // Add to inner wing pylons (index 4) and outer wing pylons (index 5)
            int[] pylonIndices = { 4, 5 };
            foreach (int idx in pylonIndices)
            {
                if (idx >= wm.hardpointSets.Length)
                {
                    Debug.LogWarning($"[KR-67EX] Hardpoint set index {idx} out of range (max {wm.hardpointSets.Length - 1})");
                    continue;
                }

                var options = wm.hardpointSets[idx].weaponOptions;

                if (aam29Quad != null && !options.Contains(aam29Quad))
                {
                    options.Add(aam29Quad);
                    Debug.Log($"[KR-67EX] Added AAM-29 Scythe x4 to hardpoint set {idx}");
                }
                if (aam36Quad != null && !options.Contains(aam36Quad))
                {
                    options.Add(aam36Quad);
                    Debug.Log($"[KR-67EX] Added AAM-36 Scimitar x4 to hardpoint set {idx}");
                }
            }
        }

        private static WeaponMount FindMount(string name)
        {
            return Resources.FindObjectsOfTypeAll<WeaponMount>()
                .FirstOrDefault(m => m.name.Equals(name, System.StringComparison.InvariantCultureIgnoreCase));
        }
    }
}

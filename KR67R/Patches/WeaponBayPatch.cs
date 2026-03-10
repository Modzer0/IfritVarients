using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace KR67R.Patches
{
    /// <summary>
    /// Disable Forward Bay (index 1), Inner Pylon (index 4), and Outer Pylon (index 5)
    /// for the KR-67R. The forward bay is occupied by the optical sensor.
    /// Only Internal Cannon (0), Rear Bay (2), Auxiliary Bay (3), and Tail (6) remain usable.
    /// 
    /// Approach: Patch the loadout at spawn time to null out disabled hardpoints,
    /// and filter the weapon selection menu to hide disabled sets.
    /// </summary>
    [HarmonyPatch(typeof(Spawner), "SpawnAircraft")]
    public static class WeaponBaySpawnPatch
    {
        private static readonly FieldInfo defField =
            AccessTools.Field(typeof(Unit), "definition");

        [HarmonyPostfix]
        public static void Postfix(Aircraft __result)
        {
            if (__result == null) return;

            // Check if this is a KR-67R (either by SpawningClone flag or definition)
            var def = defField?.GetValue(__result) as AircraftDefinition;
            if (def == null || def.jsonKey != Plugin.CloneJsonKey) return;

            // Null out weapons on disabled hardpoint sets
            WeaponManager wm = __result.weaponManager;
            if (wm == null) return;

            foreach (int idx in Plugin.DisabledHardpoints)
            {
                if (idx < wm.hardpointSets.Length)
                {
                    // Clear the weapon mount on this hardpoint set
                    var hpSet = wm.hardpointSets[idx];
                    hpSet.weaponMount = null;
                }
            }
        }
    }

    /// <summary>
    /// Filter the aircraft selection menu's weapon selector to show disabled bays
    /// as unavailable for the KR-67R. When populating weapon options, skip
    /// disabled hardpoint sets.
    /// </summary>
    [HarmonyPatch(typeof(WeaponSelector), "PopulateOptions")]
    public static class WeaponSelectorPatch
    {
        [HarmonyPostfix]
        public static void Postfix(WeaponSelector __instance, int hardpointSetIndex)
        {
            // Check if the current aircraft selection is KR-67R
            // WeaponSelector is part of the loadout UI — we need to check the selected aircraft
            if (Plugin.CloneDef == null) return;

            // If this hardpoint set is disabled for KR-67R, clear the options
            if (System.Array.IndexOf(Plugin.DisabledHardpoints, hardpointSetIndex) >= 0)
            {
                // We need to check if the currently selected aircraft is KR-67R
                // This is checked via the AircraftSelectionMenu's selected type
                var selMenuField = AccessTools.Field(typeof(AircraftSelectionMenu), "selectedType");
                var selMenu = Object.FindObjectOfType<AircraftSelectionMenu>();
                if (selMenu != null)
                {
                    var selectedDef = selMenuField?.GetValue(selMenu) as AircraftDefinition;
                    if (selectedDef != null && selectedDef.jsonKey == Plugin.CloneJsonKey)
                    {
                        // Clear dropdown options — this bay is disabled
                        var optionsField = AccessTools.Field(typeof(WeaponSelector), "options");
                        var options = optionsField?.GetValue(__instance) as List<WeaponMount>;
                        options?.Clear();
                    }
                }
            }
        }
    }

    /// <summary>
    /// When the loadout is being validated (VetLoadout), ensure disabled hardpoints
    /// on KR-67R have no weapons assigned.
    /// </summary>
    [HarmonyPatch(typeof(WeaponChecker), "VetLoadout")]
    public static class VetLoadoutPatch
    {
        [HarmonyPostfix]
        public static void Postfix(AircraftDefinition definition, ref Loadout loadout)
        {
            if (definition == null || definition.jsonKey != Plugin.CloneJsonKey) return;

            // Null out weapons on disabled hardpoint indices
            foreach (int idx in Plugin.DisabledHardpoints)
            {
                if (idx < loadout.weapons.Count)
                    loadout.weapons[idx] = null;
            }
        }
    }
}

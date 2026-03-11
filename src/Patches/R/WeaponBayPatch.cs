using HarmonyLib;
using NuclearOption.SavedMission;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace IfritVariants.Patches.R
{
    /// <summary>
    /// Hide weapon options for KR-67R disabled hardpoints in the selection UI.
    /// Disabled hardpoints: 0=gun, 1=forward bay, 4=inner pylon, 5=outer pylon.
    /// </summary>
    [HarmonyPatch(typeof(WeaponChecker), "GetAvailableWeaponsNonAlloc")]
    public static class WeaponAvailabilityPatch
    {
        [HarmonyPostfix]
        public static void Postfix(HardpointSet hardpointSet, List<WeaponMount> outAvailable)
        {
            if (Plugin.R_CloneDef == null) return;

            // Check if the currently selected aircraft in the menu is the KR-67R
            var selMenu = Object.FindObjectOfType<AircraftSelectionMenu>();
            if (selMenu == null) return;

            var selectedField = AccessTools.Field(typeof(AircraftSelectionMenu), "selectedType");
            var selectedDef = selectedField?.GetValue(selMenu) as AircraftDefinition;
            if (selectedDef?.jsonKey != Plugin.R_JsonKey) return;

            // Find which hardpoint index this HardpointSet corresponds to
            WeaponManager wm = Plugin.R_CloneDef.unitPrefab?.GetComponent<Aircraft>()?.weaponManager;
            if (wm == null) return;

            for (int i = 0; i < wm.hardpointSets.Length; i++)
            {
                if (wm.hardpointSets[i] == hardpointSet &&
                    System.Array.IndexOf(Plugin.R_DisabledHardpoints, i) >= 0)
                {
                    outAvailable.Clear();
                    return;
                }
            }
        }
    }

    /// <summary>
    /// Strip disabled hardpoint weapons from loadout validation.
    /// </summary>
    [HarmonyPatch(typeof(WeaponChecker), "VetLoadout")]
    public static class VetLoadoutPatch
    {
        [HarmonyPostfix]
        public static void Postfix(AircraftDefinition definition, Loadout loadout)
        {
            if (definition?.jsonKey != Plugin.R_JsonKey) return;

            foreach (int idx in Plugin.R_DisabledHardpoints)
            {
                if (idx < loadout.weapons.Count)
                    loadout.weapons[idx] = null;
            }
        }
    }
}

using HarmonyLib;
using NuclearOption.SavedMission;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace IfritVariants.Patches.R
{
    /// <summary>
    /// Disable weapon selectors for KR-67R disabled hardpoints in the aircraft selection UI.
    /// </summary>
    [HarmonyPatch(typeof(AircraftSelectionMenu), "ShowHardpoints")]
    public static class ShowHardpointsPatch
    {
        private static readonly FieldInfo aircraftSelectionField =
            AccessTools.Field(typeof(AircraftSelectionMenu), "aircraftSelection");
        private static readonly FieldInfo selectionIndexField =
            AccessTools.Field(typeof(AircraftSelectionMenu), "selectionIndex");
        private static readonly FieldInfo dropdownField =
            AccessTools.Field(typeof(WeaponSelector), "dropdown");

        [HarmonyPostfix]
        public static void Postfix(AircraftSelectionMenu __instance)
        {
            var selection = aircraftSelectionField?.GetValue(__instance) as List<AircraftDefinition>;
            if (selection == null) return;
            int index = (int)(selectionIndexField?.GetValue(__instance) ?? -1);
            if (index < 0 || index >= selection.Count) return;

            var selectedDef = selection[index];
            if (selectedDef?.jsonKey != Plugin.R_JsonKey) return;

            for (int i = 0; i < __instance.weaponSelectors.Count; i++)
            {
                if (System.Array.IndexOf(Plugin.R_DisabledHardpoints, i) >= 0)
                {
                    // Deactivate the entire weapon selector GameObject to hide it
                    __instance.weaponSelectors[i].gameObject.SetActive(false);
                }
            }
            Plugin.Log("KR-67R: disabled weapon selectors for hardpoints 0,1,4,5");
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

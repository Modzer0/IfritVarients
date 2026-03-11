using HarmonyLib;
using NuclearOption.SavedMission;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace IfritVariants.Patches.R
{
    /// <summary>
    /// UI and loadout validation for KR-67R disabled hardpoints.
    /// Runtime hardpoint clearing is done in SpawnerPatch.ApplyKR67R.
    /// </summary>
    [HarmonyPatch(typeof(WeaponSelector), "PopulateOptions")]
    public static class WeaponSelectorPatch
    {
        [HarmonyPostfix]
        public static void Postfix(WeaponSelector __instance, int hardpointSetIndex)
        {
            if (Plugin.R_CloneDef == null) return;
            if (System.Array.IndexOf(Plugin.R_DisabledHardpoints, hardpointSetIndex) < 0) return;

            var selMenuField = AccessTools.Field(typeof(AircraftSelectionMenu), "selectedType");
            var selMenu = Object.FindObjectOfType<AircraftSelectionMenu>();
            if (selMenu == null) return;

            var selectedDef = selMenuField?.GetValue(selMenu) as AircraftDefinition;
            if (selectedDef?.jsonKey == Plugin.R_JsonKey)
            {
                var optionsField = AccessTools.Field(typeof(WeaponSelector), "options");
                var options = optionsField?.GetValue(__instance) as List<WeaponMount>;
                options?.Clear();
            }
        }
    }

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

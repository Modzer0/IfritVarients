using HarmonyLib;
using NuclearOption.SavedMission;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace IfritVariants.Patches.R
{
    /// <summary>
    /// Disable hardpoints 0 (gun), 1 (forward bay), 4 (inner pylon), 5 (outer pylon) for KR-67R.
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

            var def = defField?.GetValue(__result) as AircraftDefinition;
            if (def == null || def.jsonKey != Plugin.R_JsonKey) return;

            WeaponManager wm = __result.weaponManager;
            if (wm == null) return;

            foreach (int idx in Plugin.R_DisabledHardpoints)
            {
                if (idx < wm.hardpointSets.Length)
                    wm.hardpointSets[idx].weaponMount = null;
            }
        }
    }

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

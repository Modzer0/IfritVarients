using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace IfritVariants.Patches
{
    /// <summary>Inject both variants into hangars that carry the KR-67A.</summary>
    [HarmonyPatch(typeof(Hangar), "GetAvailableAircraft")]
    public static class HangarGetAvailablePatch
    {
        [HarmonyPostfix]
        public static void Postfix(Hangar __instance, ref AircraftDefinition[] __result)
        {
            if (!__result.Any(a => a.jsonKey == Plugin.OriginalJsonKey)) return;

            var list = __result.ToList();
            if (Plugin.EX_CloneDef != null && !list.Any(a => a.jsonKey == Plugin.EX_JsonKey))
                list.Add(Plugin.EX_CloneDef);
            if (Plugin.R_CloneDef != null && !list.Any(a => a.jsonKey == Plugin.R_JsonKey))
                list.Add(Plugin.R_CloneDef);

            __result = list.ToArray();
        }
    }

    [HarmonyPatch(typeof(Hangar), "CanSpawnAircraft")]
    public static class HangarCanSpawnPatch
    {
        private static readonly FieldInfo availableField =
            AccessTools.Field(typeof(Hangar), "availableAircraft");

        [HarmonyPostfix]
        public static void Postfix(Hangar __instance, AircraftDefinition definition, ref bool __result)
        {
            if (__result) return;
            if (!__instance.Available) return;
            if (definition != Plugin.EX_CloneDef && definition != Plugin.R_CloneDef) return;

            var origArray = availableField?.GetValue(__instance) as AircraftDefinition[];
            if (origArray != null && origArray.Any(a => a.jsonKey == Plugin.OriginalJsonKey))
                __result = true;
        }
    }

    [HarmonyPatch(typeof(Hangar), "TrySpawnAircraft")]
    public static class HangarTrySpawnPatch
    {
        [HarmonyPrefix]
        public static void Prefix(AircraftDefinition definition)
        {
            Plugin.SpawningEX = (definition == Plugin.EX_CloneDef);
            Plugin.SpawningR = (definition == Plugin.R_CloneDef);
        }
    }

    [HarmonyPatch(typeof(Spawner), "SpawnAircraft")]
    public static class SpawnerPatch
    {
        private static readonly FieldInfo defField =
            AccessTools.Field(typeof(Unit), "definition");

        [HarmonyPostfix]
        public static void Postfix(Aircraft __result)
        {
            if (__result == null) return;

            if (Plugin.SpawningEX)
            {
                defField?.SetValue(__result, Plugin.EX_CloneDef);
                Plugin.SpawningEX = false;
            }
            else if (Plugin.SpawningR)
            {
                defField?.SetValue(__result, Plugin.R_CloneDef);
                Plugin.SpawningR = false;
            }
        }
    }

    [HarmonyPatch(typeof(AircraftSelectionMenu), "SpawnPreview")]
    public static class PreviewPatch
    {
        private static readonly FieldInfo selectedField =
            AccessTools.Field(typeof(AircraftSelectionMenu), "selectedType");
        private static readonly FieldInfo selectionField =
            AccessTools.Field(typeof(AircraftSelectionMenu), "aircraftSelection");
        private static readonly FieldInfo indexField =
            AccessTools.Field(typeof(AircraftSelectionMenu), "selectionIndex");

        [HarmonyPostfix]
        public static void Postfix(AircraftSelectionMenu __instance)
        {
            var selection = selectionField?.GetValue(__instance) as List<AircraftDefinition>;
            if (selection == null) return;
            int index = (int)(indexField?.GetValue(__instance) ?? -1);
            if (index < 0 || index >= selection.Count) return;

            var selected = selection[index];
            if (selected == Plugin.EX_CloneDef || selected == Plugin.R_CloneDef)
                selectedField?.SetValue(__instance, selected);
        }
    }
}

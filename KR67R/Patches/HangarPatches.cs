using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace KR67R.Patches
{
    [HarmonyPatch(typeof(Hangar), "GetAvailableAircraft")]
    public static class HangarGetAvailablePatch
    {
        [HarmonyPostfix]
        public static void Postfix(Hangar __instance, ref AircraftDefinition[] __result)
        {
            if (Plugin.CloneDef == null) return;
            if (__result.Any(a => a.jsonKey == Plugin.OriginalJsonKey) &&
                !__result.Any(a => a.jsonKey == Plugin.CloneJsonKey))
            {
                var list = __result.ToList();
                list.Add(Plugin.CloneDef);
                __result = list.ToArray();
            }
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
            if (__result || Plugin.CloneDef == null) return;
            if (definition != Plugin.CloneDef) return;
            if (!__instance.Available) return;

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
            Plugin.SpawningClone = (definition == Plugin.CloneDef);
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
            if (!Plugin.SpawningClone || __result == null) return;
            defField?.SetValue(__result, Plugin.CloneDef);
            Plugin.SpawningClone = false;
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
            if (Plugin.CloneDef == null) return;
            var selection = selectionField?.GetValue(__instance) as List<AircraftDefinition>;
            if (selection == null) return;
            int index = (int)(indexField?.GetValue(__instance) ?? -1);
            if (index >= 0 && index < selection.Count && selection[index] == Plugin.CloneDef)
                selectedField?.SetValue(__instance, Plugin.CloneDef);
        }
    }
}

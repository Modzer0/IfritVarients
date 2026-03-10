using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace KR67EX.Patches
{
    /// <summary>Inject KR-67EX into hangars that carry the KR-67A.</summary>
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

    /// <summary>Allow KR-67EX to spawn from hangars that have the KR-67A.</summary>
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

    /// <summary>Flag when the clone is being spawned so we can reassign definition post-spawn.</summary>
    [HarmonyPatch(typeof(Hangar), "TrySpawnAircraft")]
    public static class HangarTrySpawnPatch
    {
        [HarmonyPrefix]
        public static void Prefix(AircraftDefinition definition)
        {
            Plugin.SpawningClone = (definition == Plugin.CloneDef);
        }
    }

    /// <summary>Reassign definition on the spawned aircraft to our clone.</summary>
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

    /// <summary>Fix the aircraft selection menu preview to show KR-67EX stats.</summary>
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

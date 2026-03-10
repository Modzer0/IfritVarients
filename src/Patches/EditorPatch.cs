using HarmonyLib;
using NuclearOption.MissionEditorScripts.Buttons;
using System.Collections;
using System.Reflection;

namespace IfritVariants.Patches
{
    /// <summary>
    /// Clear the static unitProviders cache so the mission editor rebuilds
    /// its aircraft list from Encyclopedia (which now includes our variants).
    /// </summary>
    [HarmonyPatch(typeof(NewUnitPanel), "Awake")]
    public static class EditorPatch
    {
        private static readonly FieldInfo unitProvidersField =
            AccessTools.Field(typeof(NewUnitPanel), "unitProviders");

        [HarmonyPrefix]
        public static void Prefix()
        {
            var dict = unitProvidersField?.GetValue(null) as IDictionary;
            if (dict != null && dict.Count > 0)
                dict.Clear();
        }
    }
}

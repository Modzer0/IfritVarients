using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;

namespace IfritVariants
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    public class Plugin : BaseUnityPlugin
    {
        public const string PluginGUID = "com.nuclearoption.ifritvariants";
        public const string PluginName = "Ifrit Variants";
        public const string PluginVersion = "1.3.0";

        // ═══════════════════════════════════════════
        //  KR-67EX Constants
        // ═══════════════════════════════════════════
        internal const string OriginalJsonKey = "Multirole1";

        internal const string EX_JsonKey = "KR-67EX";
        internal const string EX_DisplayName = "KR-67EX";
        internal const string EX_Code = "KR-67EX";
        internal const string EX_Description =
            "With the success of the KR-67 airframe an upgrade program was put in place giving it " +
            "more powerful engines, radar, and mounts for more missiles along with higher G tolerances " +
            "for the airframe.";
        internal const float EX_Mach25SeaLevel = 850f;
        internal const float EX_ServiceCeilingMeters = 18288f; // 60,000 ft

        // ═══════════════════════════════════════════
        //  KR-67R Constants
        // ═══════════════════════════════════════════
        internal const string R_JsonKey = "KR-67R";
        internal const string R_DisplayName = "KR-67R";
        internal const string R_Code = "KR-67R";
        internal const string R_Description =
            "The KR-67R is a heavily redesigned KR-67 designed for stealth recon missions. " +
            "It has a tiny cross-section and a camera with a 50NM range on the bottom of the aircraft. " +
            "It can carry a small amount of ordnance but it is highly advised not to employ any as the " +
            "weapon bay doors opening will spike the RCS of the craft well into the visible range of any " +
            "nearby aircraft or air defense. It has a service ceiling at 85k feet and an operational ceiling " +
            "at 80kft. On the afterburner the KR-67R will approach Mach 5 at altitude. Unlike the " +
            "experimental X variant it does not have scramjets.";
        internal const float R_OpticalRange50NM = 92600f;
        internal const float R_ServiceCeilingMeters = 25908f; // 85,000 ft
        internal const float R_CruiseAltMeters = 24384f;      // 80,000 ft
        internal const float R_Mach5At80kFt = 1450f;
        internal static readonly int[] R_DisabledHardpoints = { 0, 1, 4, 5 };

        // ═══════════════════════════════════════════
        //  Configuration
        // ═══════════════════════════════════════════
        // KR-67EX config
        internal static ConfigEntry<bool> EX_Enable;
        internal static ConfigEntry<float> EX_ThrustMultiplier;
        internal static ConfigEntry<float> EX_MaxSpeedMs;
        internal static ConfigEntry<float> EX_GLimit;
        internal static ConfigEntry<float> EX_JammerIntensity;
        internal static ConfigEntry<float> EX_CostMultiplier;
        internal static ConfigEntry<float> EX_ServiceCeilingFt;

        // KR-67R config
        internal static ConfigEntry<bool> R_Enable;
        internal static ConfigEntry<float> R_CostMultiplier;
        internal static ConfigEntry<float> R_RCSDivisor;
        internal static ConfigEntry<float> R_OpticalRangeNM;
        internal static ConfigEntry<float> R_JammerIntensity;
        internal static ConfigEntry<float> R_ServiceCeilingFt;
        internal static ConfigEntry<float> R_TargetSpeedMach;
        internal static ConfigEntry<float> R_TargetSpeedAltFt;

        // ═══════════════════════════════════════════
        //  Runtime State
        // ═══════════════════════════════════════════
        internal static AircraftDefinition EX_CloneDef;
        internal static AircraftDefinition R_CloneDef;
        internal static bool SpawningEX;
        internal static bool SpawningR;

        private static BepInEx.Logging.ManualLogSource _log;
        internal static void Log(string msg) => _log?.LogInfo($"[IfritVariants] {msg}");

        // ═══════════════════════════════════════════
        //  Helpers
        // ═══════════════════════════════════════════
        internal static bool IsKR67EX(Aircraft aircraft)
        {
            return aircraft != null &&
                   aircraft.definition != null &&
                   aircraft.definition.jsonKey == EX_JsonKey;
        }

        internal static bool IsKR67R(Aircraft aircraft)
        {
            return aircraft != null &&
                   aircraft.definition != null &&
                   aircraft.definition.jsonKey == R_JsonKey;
        }

        internal static float GetOpticalRangeMeters()
        {
            return R_OpticalRangeNM.Value * 1852f;
        }

        private void Awake()
        {
            _log = Logger;
            // KR-67EX config
            EX_Enable = Config.Bind("KR-67EX", "Enable", true, "Enable the KR-67EX variant");
            EX_ThrustMultiplier = Config.Bind("KR-67EX", "ThrustMultiplier", 1.2f,
                "Thrust multiplier over KR-67A baseline (1.2 = 20% increase)");
            EX_MaxSpeedMs = Config.Bind("KR-67EX", "MaxSpeedMs", EX_Mach25SeaLevel,
                "Speed limit in m/s (850 ≈ Mach 2.5 at sea level)");
            EX_GLimit = Config.Bind("KR-67EX", "GLimit", 12f, "G overload limit");
            EX_JammerIntensity = Config.Bind("KR-67EX", "JammerIntensity", 12f, "Radar jammer intensity");
            EX_CostMultiplier = Config.Bind("KR-67EX", "CostMultiplier", 2f,
                "Cost multiplier vs KR-67A base cost");
            EX_ServiceCeilingFt = Config.Bind("KR-67EX", "ServiceCeilingFt", 60000f,
                "Service ceiling in feet");

            // KR-67R config
            R_Enable = Config.Bind("KR-67R", "Enable", true, "Enable the KR-67R recon variant");
            R_CostMultiplier = Config.Bind("KR-67R", "CostMultiplier", 4f,
                "Cost multiplier vs KR-67A base cost");
            R_RCSDivisor = Config.Bind("KR-67R", "RCSDivisor", 100f, "Divide base RCS by this value");
            R_OpticalRangeNM = Config.Bind("KR-67R", "OpticalRangeNM", 50f,
                "Optical sensor maximum range in nautical miles");
            R_JammerIntensity = Config.Bind("KR-67R", "JammerIntensity", 12f, "Radar jammer intensity");
            R_ServiceCeilingFt = Config.Bind("KR-67R", "ServiceCeilingFt", 85000f,
                "Service ceiling in feet");
            R_TargetSpeedMach = Config.Bind("KR-67R", "TargetSpeedMach", 5f,
                "Target speed in Mach at cruise altitude on afterburner");
            R_TargetSpeedAltFt = Config.Bind("KR-67R", "TargetSpeedAltFt", 80000f,
                "Altitude in feet where target Mach speed is achieved");

            try
            {
                Harmony harmony = new Harmony(PluginGUID);
                harmony.PatchAll();
                Logger.LogInfo($"{PluginName} v{PluginVersion} loaded. All patches applied.");
            }
            catch (System.Exception ex)
            {
                Logger.LogError($"{PluginName} failed to apply patches: {ex}");
            }
        }
    }
}

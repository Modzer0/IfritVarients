using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace KR67EX
{
    [BepInPlugin("com.nuclearoption.kr67ex", "KR-67EX Variant", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        // ── Configuration ──
        internal static ConfigEntry<bool> EnableVariant;
        internal static ConfigEntry<float> ThrustMultiplier;
        internal static ConfigEntry<float> MaxSpeedMs;
        internal static ConfigEntry<float> GLimit;
        internal static ConfigEntry<float> JammerIntensity;
        internal static ConfigEntry<float> CostMultiplier;
        internal static ConfigEntry<float> ServiceCeilingFt;

        // ── Runtime State ──
        internal static AircraftDefinition CloneDef;
        internal static bool SpawningClone;

        // ── Constants ──
        internal const string OriginalJsonKey = "Multirole1";
        internal const string CloneJsonKey = "KR-67EX";
        internal const string CloneDisplayName = "KR-67EX";
        internal const string CloneCode = "KR-67EX";
        internal const string CloneDescription =
            "Enhanced multirole variant with upgraded engines, extended radar range, " +
            "increased G tolerance, and expanded weapons capability.";

        // Mach 2.5 at sea level ≈ 340 * 2.5 = 850 m/s
        internal const float Mach25SeaLevel = 850f;
        // 60,000 ft in meters
        internal const float ServiceCeilingMeters = 18288f;

        private void Awake()
        {
            EnableVariant = Config.Bind("General", "EnableVariant", true,
                "Enable the KR-67EX variant");
            ThrustMultiplier = Config.Bind("Engine", "ThrustMultiplier", 1.2f,
                "Thrust multiplier over KR-67A baseline (1.2 = 20% increase)");
            MaxSpeedMs = Config.Bind("Engine", "MaxSpeedMs", Mach25SeaLevel,
                "Speed limit in m/s (850 ≈ Mach 2.5 at sea level)");
            GLimit = Config.Bind("Flight", "GLimit", 12f,
                "G overload limit");
            JammerIntensity = Config.Bind("EW", "JammerIntensity", 10f,
                "Radar jammer intensity");
            CostMultiplier = Config.Bind("General", "CostMultiplier", 2f,
                "Cost multiplier vs KR-67A base cost");
            ServiceCeilingFt = Config.Bind("Engine", "ServiceCeilingFt", 60000f,
                "Service ceiling in feet");

            if (EnableVariant.Value)
            {
                Harmony harmony = new Harmony("com.nuclearoption.kr67ex");
                harmony.PatchAll();
                Logger.LogInfo("KR-67EX variant loaded.");
            }
        }

        /// <summary>Check if an Aircraft instance is our variant.</summary>
        internal static bool IsKR67EX(Aircraft aircraft)
        {
            return aircraft != null &&
                   aircraft.definition != null &&
                   aircraft.definition.jsonKey == CloneJsonKey;
        }
    }
}

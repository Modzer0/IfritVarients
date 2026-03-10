using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace KR67R
{
    [BepInPlugin("com.nuclearoption.kr67r", "KR-67R Recon Variant", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        // ── Configuration ──
        internal static ConfigEntry<bool> EnableVariant;
        internal static ConfigEntry<float> CostMultiplier;
        internal static ConfigEntry<float> RCSDivisor;
        internal static ConfigEntry<float> OpticalRangeNM;
        internal static ConfigEntry<float> JammerIntensity;
        internal static ConfigEntry<float> ServiceCeilingFt;
        internal static ConfigEntry<float> TargetSpeedMach;
        internal static ConfigEntry<float> TargetSpeedAltFt;

        // ── Runtime State ──
        internal static AircraftDefinition CloneDef;
        internal static bool SpawningClone;

        // ── Constants ──
        internal const string OriginalJsonKey = "Multirole1";
        internal const string CloneJsonKey = "KR-67R";
        internal const string CloneDisplayName = "KR-67R";
        internal const string CloneCode = "KR-67R";
        internal const string CloneDescription =
            "High-altitude reconnaissance variant. No radar. Forward bay replaced by " +
            "long-range optical sensor. Reduced RCS. Wing pylons removed. " +
            "Optimized for Mach 5 cruise at 80,000ft.";

        // 50 NM in meters
        internal const float OpticalRange50NM = 92600f;
        // 85,000 ft in meters
        internal const float ServiceCeilingMeters = 25908f;
        // 80,000 ft in meters
        internal const float CruiseAltMeters = 24384f;
        // Speed of sound at 80,000ft: Max(-0.005*24384+340, 290) = 290 m/s
        // Mach 5 at 80,000ft = 1450 m/s
        internal const float Mach5At80kFt = 1450f;

        // Disabled hardpoint set indices (0-indexed):
        // 1 = Forward Bay (optical sensor), 4 = Inner Pylon, 5 = Outer Pylon
        internal static readonly int[] DisabledHardpoints = { 1, 4, 5 };

        private void Awake()
        {
            EnableVariant = Config.Bind("General", "EnableVariant", true,
                "Enable the KR-67R recon variant");
            CostMultiplier = Config.Bind("General", "CostMultiplier", 4f,
                "Cost multiplier vs KR-67A base cost");
            RCSDivisor = Config.Bind("Stealth", "RCSDivisor", 100f,
                "Divide base RCS by this value");
            OpticalRangeNM = Config.Bind("Sensor", "OpticalRangeNM", 50f,
                "Optical sensor maximum range in nautical miles");
            JammerIntensity = Config.Bind("EW", "JammerIntensity", 12f,
                "Radar jammer intensity");
            ServiceCeilingFt = Config.Bind("Engine", "ServiceCeilingFt", 85000f,
                "Service ceiling in feet");
            TargetSpeedMach = Config.Bind("Engine", "TargetSpeedMach", 5f,
                "Target speed in Mach at cruise altitude on afterburner");
            TargetSpeedAltFt = Config.Bind("Engine", "TargetSpeedAltFt", 80000f,
                "Altitude in feet where target Mach speed is achieved");

            if (EnableVariant.Value)
            {
                Harmony harmony = new Harmony("com.nuclearoption.kr67r");
                harmony.PatchAll();
                Logger.LogInfo("KR-67R recon variant loaded.");
            }
        }

        /// <summary>Check if an Aircraft instance is our variant.</summary>
        internal static bool IsKR67R(Aircraft aircraft)
        {
            return aircraft != null &&
                   aircraft.definition != null &&
                   aircraft.definition.jsonKey == CloneJsonKey;
        }

        /// <summary>Get optical range in meters from config.</summary>
        internal static float GetOpticalRangeMeters()
        {
            return OpticalRangeNM.Value * 1852f;
        }
    }
}

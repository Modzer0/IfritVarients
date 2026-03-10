// Decompiled with JetBrains decompiler
// Type: qol.WeaponLoading.Modifiers.Weapons.WeaponVariantPostProcessors
// Assembly: qol, Version=1.1.6.3, Culture=neutral, PublicKeyToken=null
// MVID: 3B6539EA-E38C-45EE-8FFF-FC58CC42BADC
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\BepInEx\plugins\qol_1.1.6.3b1.dll

using HarmonyLib;
using Mirage;
using qol.Utilities;
using qol.WeaponLoading.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

#nullable disable
namespace qol.WeaponLoading.Modifiers.Weapons;

public static class WeaponVariantPostProcessors
{
  private static readonly Dictionary<string, Action<GameObject, WeaponInfo, ModificationContext>> Handlers = new Dictionary<string, Action<GameObject, WeaponInfo, ModificationContext>>()
  {
    ["Rocket2_rls_Particles"] = (Action<GameObject, WeaponInfo, ModificationContext>) ((go, info, ctx) =>
    {
      GameObject gameObject1 = PathLookup.Find("Rocket2_rls/FireParticles");
      GameObject gameObject2 = PathLookup.Find("Rocket2_rls/smokeParticles");
      GameObject gameObject3 = PathLookup.Find("Rocket2_rls/smokeParticles/smokeTrail");
      if ((UnityEngine.Object) gameObject1 != (UnityEngine.Object) null && (UnityEngine.Object) gameObject2 != (UnityEngine.Object) null && (UnityEngine.Object) gameObject3 != (UnityEngine.Object) null)
      {
        ParticleSystem component1 = gameObject1.GetComponent<ParticleSystem>();
        ParticleSystem component2 = gameObject2.GetComponent<ParticleSystem>();
        ParticleSystem component3 = gameObject3.GetComponent<ParticleSystem>();
        TrailEmitter component4 = gameObject3.GetComponent<TrailEmitter>();
        if ((UnityEngine.Object) component1 != (UnityEngine.Object) null)
        {
          ParticleSystem.MainModule main1 = component1.main with
          {
            duration = 10f,
            startSize = (ParticleSystem.MinMaxCurve) 2.5f,
            startSpeed = (ParticleSystem.MinMaxCurve) 100f
          };
        }
        if ((UnityEngine.Object) component2 != (UnityEngine.Object) null)
        {
          ParticleSystem.MainModule main2 = component2.main with
          {
            duration = 2.5f,
            startSize = (ParticleSystem.MinMaxCurve) 240f
          };
        }
        if ((UnityEngine.Object) component3 != (UnityEngine.Object) null)
        {
          ParticleSystem.MainModule main3 = component3.main with
          {
            duration = 60f,
            startSize = (ParticleSystem.MinMaxCurve) 50f
          };
        }
        if ((UnityEngine.Object) component4 != (UnityEngine.Object) null)
        {
          component4.opacity = 0.5f;
          ReflectionHelpers.SetFieldValue((object) component4, "emitFrequency", (object) 1f);
          ReflectionHelpers.SetFieldValue((object) component4, "opacityVariation", (object) 0.6f);
          ReflectionHelpers.SetFieldValue((object) component4, "scaleVariation", (object) 0.6f);
          ReflectionHelpers.SetFieldValue((object) component4, "segmentLength", (object) 100f);
        }
      }
      Missile component = go.GetComponent<Missile>();
      if (!((UnityEngine.Object) component != (UnityEngine.Object) null))
        return;
      AnimationCurve animationCurve = new AnimationCurve();
      Keyframe keyframe = new Keyframe(0.0f, 0.02f);
      keyframe.inTangent = 0.4f;
      keyframe.outTangent = 0.4f;
      keyframe.inWeight = 0.333f;
      keyframe.outWeight = 0.08f;
      keyframe.weightedMode = WeightedMode.None;
      Keyframe key1 = keyframe;
      keyframe = new Keyframe(0.999f, 5f);
      keyframe.inTangent = 10.55f;
      keyframe.outTangent = 10.55f;
      keyframe.inWeight = 0.028f;
      keyframe.outWeight = 0.333f;
      keyframe.weightedMode = WeightedMode.None;
      Keyframe key2 = keyframe;
      animationCurve.AddKey(key1);
      animationCurve.AddKey(key2);
      ReflectionHelpers.SetFieldValue((object) component, "dragCurve", (object) animationCurve);
    }),
    ["P_GLB1_DragCurve"] = (Action<GameObject, WeaponInfo, ModificationContext>) ((go, info, ctx) =>
    {
      Missile component = go.GetComponent<Missile>();
      if ((UnityEngine.Object) component != (UnityEngine.Object) null)
      {
        AnimationCurve animationCurve = new AnimationCurve(new Keyframe[2]
        {
          new Keyframe(0.0f, 1f / 1000f),
          new Keyframe(1f, 0.5f)
        });
        Traverse.Create((object) component).Field("dragCurve").SetValue((object) animationCurve);
      }
      PathLookup.Find("P_GLB1/wingL")?.SetActive(false);
      PathLookup.Find("P_GLB1/wingR")?.SetActive(false);
      GameObject gameObject = PathLookup.Find("P_GLB1_double/pylon", false);
      if ((UnityEngine.Object) gameObject != (UnityEngine.Object) null)
        gameObject.name = "pylon2";
      WeaponVariantPostProcessors.LoadP_GLB1AssetBundle(ctx, go);
    }),
    ["P_KEM1_Complex"] = (Action<GameObject, WeaponInfo, ModificationContext>) ((go, info, ctx) => WeaponVariantPostProcessors.LoadP_KEM1AssetBundle(ctx, go)),
    ["P_RAM29_Complex"] = (Action<GameObject, WeaponInfo, ModificationContext>) ((go, info, ctx) =>
    {
      SARHSeeker component = go.GetComponent<SARHSeeker>();
      if ((UnityEngine.Object) component != (UnityEngine.Object) null)
        UnityEngine.Object.Destroy((UnityEngine.Object) component);
      ReflectionHelpers.SetFieldValue((object) go.AddComponent<ARHSeeker>(), "radarParameters", (object) new RadarParams()
      {
        maxRange = 12000f,
        maxSignal = 2f,
        minSignal = 0.4f,
        clutterFactor = 0.1f,
        dopplerFactor = 0.1f
      });
      WeaponVariantPostProcessors.LoadAssetBundleMesh(ctx, "Resources.FBXMod.RAM-29.bundle", go, (string) null, "AAM2");
    }),
    ["BallisticMissile_Alt_Complex"] = (Action<GameObject, WeaponInfo, ModificationContext>) ((go, info, ctx) =>
    {
      BallisticMissileGuidance component = go.GetComponent<BallisticMissileGuidance>();
      if ((UnityEngine.Object) component != (UnityEngine.Object) null)
        UnityEngine.Object.Destroy((UnityEngine.Object) component);
      go.AddComponent<OpticalSeekerCruiseMissile>();
    }),
    ["BallisticMissile_AltN_Complex"] = (Action<GameObject, WeaponInfo, ModificationContext>) ((go, info, ctx) =>
    {
      BallisticMissileGuidance component = go.GetComponent<BallisticMissileGuidance>();
      if ((UnityEngine.Object) component != (UnityEngine.Object) null)
        UnityEngine.Object.Destroy((UnityEngine.Object) component);
      go.AddComponent<OpticalSeekerCruiseMissile>();
    }),
    ["P_SAMRadar1_Complex"] = (Action<GameObject, WeaponInfo, ModificationContext>) ((go, info, ctx) =>
    {
      SARHSeeker component5 = go.GetComponent<SARHSeeker>();
      if ((UnityEngine.Object) component5 != (UnityEngine.Object) null)
        UnityEngine.Object.Destroy((UnityEngine.Object) component5);
      ReflectionHelpers.SetFieldValue((object) go.AddComponent<ARHSeeker>(), "radarParameters", (object) new RadarParams()
      {
        maxRange = 17500f,
        maxSignal = 4f,
        minSignal = 0.5f,
        clutterFactor = 0.06f,
        dopplerFactor = 0.06f
      });
      GameObject original1 = PathLookup.Find("AShM1/VLS_Booster");
      if ((UnityEngine.Object) original1 != (UnityEngine.Object) null)
      {
        GameObject gameObject4 = UnityEngine.Object.Instantiate<GameObject>(original1);
        gameObject4.name = "VLS_Booster";
        gameObject4.transform.SetParent(go.transform);
        GameObject original2 = PathLookup.Find("P_SAMRadar1/smokeParticles/smokeTrail");
        if ((UnityEngine.Object) original2 != (UnityEngine.Object) null)
        {
          GameObject gameObject5 = UnityEngine.Object.Instantiate<GameObject>(original2);
          gameObject5.transform.SetParent(gameObject4.transform);
          ParticleSystem component6 = gameObject5.GetComponent<ParticleSystem>();
          if ((UnityEngine.Object) component6 != (UnityEngine.Object) null)
          {
            component6.startLifetime = 6f;
            component6.startSpeed = 0.0f;
            component6.startSize = 120f;
          }
        }
      }
      GameObject gameObject6 = PathLookup.Find("P_SAMRadar1/smokeParticles");
      if ((UnityEngine.Object) gameObject6 != (UnityEngine.Object) null)
      {
        ParticleSystem component7 = gameObject6.GetComponent<ParticleSystem>();
        if ((UnityEngine.Object) component7 != (UnityEngine.Object) null)
        {
          ParticleSystem.MainModule main = component7.main with
          {
            duration = 11f,
            startDelay = (ParticleSystem.MinMaxCurve) 0.0f
          };
          Missile component8 = go.GetComponent<Missile>();
          if ((UnityEngine.Object) component8 != (UnityEngine.Object) null)
          {
            Array array = Traverse.Create((object) component8).Field("motors").GetValue<Array>();
            if (array != null && array.Length > 0)
            {
              Traverse traverse = Traverse.Create(array.GetValue(0));
              ParticleSystem[] source = traverse.Field("particleSystems").GetValue<ParticleSystem[]>();
              if (source != null)
              {
                List<ParticleSystem> list = ((IEnumerable<ParticleSystem>) source).ToList<ParticleSystem>();
                list.Add(component7);
                traverse.Field("particleSystems").SetValue((object) list.ToArray());
              }
            }
          }
        }
      }
      GameObject gameObject7 = PathLookup.Find("P_SAMRadar1/smokeParticles/smokeTrail");
      if ((UnityEngine.Object) gameObject7 != (UnityEngine.Object) null)
      {
        ParticleSystem component9 = gameObject7.GetComponent<ParticleSystem>();
        if ((UnityEngine.Object) component9 != (UnityEngine.Object) null)
        {
          ParticleSystem.MainModule main = component9.main with
          {
            duration = 11f,
            loop = false,
            startSize = (ParticleSystem.MinMaxCurve) 60f
          };
        }
      }
      WeaponVariantPostProcessors.LoadP_SAMRadar1AssetBundle(ctx, go);
    }),
    ["P_HAsM1_Complex"] = (Action<GameObject, WeaponInfo, ModificationContext>) ((go, info, ctx) =>
    {
      string path = "P_HAsM1/VLS_Booster/smokeParticles";
      GameObject original3 = PathLookup.Find("P_HAsM1/VLS_Booster/smokeParticles/fireParticles");
      GameObject gameObject8 = PathLookup.Find(path);
      if ((UnityEngine.Object) original3 != (UnityEngine.Object) null)
      {
        ParticleSystem component = original3.GetComponent<ParticleSystem>();
        if ((UnityEngine.Object) component != (UnityEngine.Object) null)
        {
          ParticleSystem.MainModule main = component.main with
          {
            duration = 16f
          };
          main.startLifetimeMultiplier *= 2f;
          main.startSpeedMultiplier *= 2f;
          main.startSizeMultiplier *= 1.5f;
        }
      }
      if ((UnityEngine.Object) gameObject8 != (UnityEngine.Object) null)
      {
        ParticleSystem component = gameObject8.GetComponent<ParticleSystem>();
        if ((UnityEngine.Object) component != (UnityEngine.Object) null)
        {
          ParticleSystem.MainModule main = component.main with
          {
            duration = 16f
          };
          main.startLifetimeMultiplier *= 2f;
          main.startSpeedMultiplier *= 2f;
          main.startSizeMultiplier *= 1.5f;
        }
      }
      if ((UnityEngine.Object) original3 != (UnityEngine.Object) null)
      {
        GameObject gameObject9 = UnityEngine.Object.Instantiate<GameObject>(original3);
        gameObject9.transform.SetParent(go.transform);
        ParticleSystem component = gameObject9.GetComponent<ParticleSystem>();
        if ((UnityEngine.Object) component != (UnityEngine.Object) null)
        {
          ParticleSystem.MainModule main = component.main with
          {
            playOnAwake = true,
            startDelay = (ParticleSystem.MinMaxCurve) 16f,
            loop = true,
            startSize = (ParticleSystem.MinMaxCurve) 0.5f,
            startSpeed = (ParticleSystem.MinMaxCurve) 30f,
            startLifetime = (ParticleSystem.MinMaxCurve) 0.1f
          };
        }
      }
      GameObject original4 = PathLookup.Find("SAM_Radar2/smokeParticles/smokeTrail");
      if ((UnityEngine.Object) original4 != (UnityEngine.Object) null)
      {
        GameObject gameObject10 = UnityEngine.Object.Instantiate<GameObject>(original4);
        gameObject10.transform.SetParent(go.transform);
        ParticleSystem component10 = gameObject10.GetComponent<ParticleSystem>();
        if ((UnityEngine.Object) component10 != (UnityEngine.Object) null)
        {
          ParticleSystem.MainModule main = component10.main with
          {
            playOnAwake = true,
            startLifetime = (ParticleSystem.MinMaxCurve) 10f,
            startSpeed = (ParticleSystem.MinMaxCurve) 0.0f,
            startSize = (ParticleSystem.MinMaxCurve) 60f
          };
        }
        TrailEmitter component11 = gameObject10.GetComponent<TrailEmitter>();
        if ((UnityEngine.Object) component11 != (UnityEngine.Object) null)
          component11.rb = go.GetComponent<Rigidbody>();
      }
      WeaponVariantPostProcessors.LoadP_HAsM1AssetBundle(ctx, go);
    }),
    ["Linebreaker_ARTY_Setup"] = (Action<GameObject, WeaponInfo, ModificationContext>) ((go, info, ctx) =>
    {
      QOLPlugin.DuplicatePrefab("Shell_76mm_casing", "Shell_152mm_casingP")?.SetActive(true);
      QOLPlugin.DuplicatePrefab("Shell_76mm_casing", "Shell_130mm_casingP")?.SetActive(true);
    }),
    ["UGV1_AT_P_Setup"] = (Action<GameObject, WeaponInfo, ModificationContext>) ((go, info, ctx) =>
    {
      TargetDetector component = go.GetComponent<TargetDetector>();
      if ((UnityEngine.Object) component != (UnityEngine.Object) null)
        component.enabled = true;
      WeaponInfo info1 = QOLPlugin.DuplicateWeaponInfo("UGV1_samx1_info", "UGV1_AT_Px1_info", (GameObject) null);
      GameObject duplicateMount = QOLPlugin.DuplicatePrefab("UGV1_SAMx1", "UGV1_AT_Px1");
      if (!((UnityEngine.Object) duplicateMount != (UnityEngine.Object) null) || !((UnityEngine.Object) info1 != (UnityEngine.Object) null))
        return;
      WeaponMount mount = QOLPlugin.DuplicateWeaponMount("UGV1_SAMx1", "UGV1_AT_Px1", duplicateMount, info1);
      VehicleDefinition vehicleDefinition = ((IEnumerable<VehicleDefinition>) Resources.FindObjectsOfTypeAll<VehicleDefinition>()).FirstOrDefault<VehicleDefinition>((Func<VehicleDefinition, bool>) (v => v.name.Equals("UGV1_AT_P", StringComparison.InvariantCultureIgnoreCase)));
      MountedCargo componentInChildren = duplicateMount.GetComponentInChildren<MountedCargo>();
      if ((UnityEngine.Object) componentInChildren != (UnityEngine.Object) null && (UnityEngine.Object) vehicleDefinition != (UnityEngine.Object) null)
        componentInChildren.cargo = (UnitDefinition) vehicleDefinition;
      QOLPlugin.AddMountToEncyclopedia(ctx.Encyclopedia, "UGV1_AT_Px1", mount);
    }),
    ["Horse1_AssetBundle"] = (Action<GameObject, WeaponInfo, ModificationContext>) ((go, info, ctx) => WeaponVariantPostProcessors.LoadAssetBundleMesh(ctx, "Resources.Horse.H1.bundle", go, (string) null, (string) null)),
    ["P_LRAA1_Setup"] = (Action<GameObject, WeaponInfo, ModificationContext>) ((go, info, ctx) =>
    {
      GameObject original = PathLookup.Find("Destroyer1/Hull_CF/SAM_mount_F");
      if ((UnityEngine.Object) original != (UnityEngine.Object) null)
      {
        GameObject gameObject11 = UnityEngine.Object.Instantiate<GameObject>(original);
        gameObject11.hideFlags = HideFlags.HideAndDontSave;
        gameObject11.transform.SetParent(go.transform);
        GameObject gameObject12 = PathLookup.Find("P_LRAA1/MBT_turret");
        if ((UnityEngine.Object) gameObject12 != (UnityEngine.Object) null)
          UnityEngine.Object.Destroy((UnityEngine.Object) gameObject12);
        GameObject gameObject13 = PathLookup.Find("P_LRAA1/SAM_mount_F(Clone)/SAM_F");
        if ((UnityEngine.Object) gameObject13 != (UnityEngine.Object) null)
        {
          TargetDetector target = gameObject13.AddComponent<TargetDetector>();
          ReflectionHelpers.SetFieldValue((object) target, "scanner", (object) gameObject13.transform);
          Turret component12 = gameObject13.GetComponent<Turret>();
          if ((UnityEngine.Object) component12 != (UnityEngine.Object) null)
          {
            List<TargetDetector> targetDetectorList = new List<TargetDetector>()
            {
              target
            };
            ReflectionHelpers.SetFieldValue((object) component12, "targetDetectors", (object) targetDetectorList);
            ReflectionHelpers.SetFieldValue((object) component12, "criticalParts", (object) null);
          }
          UnitPart component13 = go.GetComponent<UnitPart>();
          ReflectionHelpers.SetFieldValue((object) target, "part", (object) component13);
          target.SetAttachedUnit(go.GetComponent<Unit>());
        }
      }
      VehicleDefinition vehicleDefinition = ((IEnumerable<VehicleDefinition>) Resources.FindObjectsOfTypeAll<VehicleDefinition>()).FirstOrDefault<VehicleDefinition>((Func<VehicleDefinition, bool>) (v => v.name.Equals("P_LRAA1", StringComparison.InvariantCultureIgnoreCase)));
      if (!((UnityEngine.Object) vehicleDefinition != (UnityEngine.Object) null))
        return;
      vehicleDefinition.friendlyIcon = ((IEnumerable<Sprite>) Resources.FindObjectsOfTypeAll<Sprite>()).FirstOrDefault<Sprite>((Func<Sprite, bool>) (s => s.name == "hudIcon_mechMissile_friendly"));
      vehicleDefinition.hostileIcon = ((IEnumerable<Sprite>) Resources.FindObjectsOfTypeAll<Sprite>()).FirstOrDefault<Sprite>((Func<Sprite, bool>) (s => s.name == "hudIcon_mechMissile_hostile"));
    }),
    ["gun_20mm_variants_Setup"] = (Action<GameObject, WeaponInfo, ModificationContext>) ((go, info, ctx) =>
    {
      WeaponInfo weaponInfo1 = QOLPlugin.DuplicateWeaponInfo("Gun20mm_Rotary", "Gun20mm_Rotary_AP", (GameObject) null);
      WeaponInfo weaponInfo2 = QOLPlugin.DuplicateWeaponInfo("Gun20mm_Rotary", "Gun20mm_Rotary_HE", (GameObject) null);
      GameObject gameObject14 = PathLookup.Find("gun_20mm_internal_ap", false);
      GameObject gameObject15 = PathLookup.Find("gun_20mm_internal_he", false);
      if ((UnityEngine.Object) gameObject14 != (UnityEngine.Object) null && (UnityEngine.Object) weaponInfo1 != (UnityEngine.Object) null)
      {
        Gun componentInChildren = gameObject14.GetComponentInChildren<Gun>();
        if ((UnityEngine.Object) componentInChildren != (UnityEngine.Object) null)
          componentInChildren.info = weaponInfo1;
      }
      if (!((UnityEngine.Object) gameObject15 != (UnityEngine.Object) null) || !((UnityEngine.Object) weaponInfo2 != (UnityEngine.Object) null))
        return;
      Gun componentInChildren1 = gameObject15.GetComponentInChildren<Gun>();
      if (!((UnityEngine.Object) componentInChildren1 != (UnityEngine.Object) null))
        return;
      componentInChildren1.info = weaponInfo2;
    }),
    ["gun_27mm_variants_Setup"] = (Action<GameObject, WeaponInfo, ModificationContext>) ((go, info, ctx) =>
    {
      WeaponInfo weaponInfo3 = QOLPlugin.DuplicateWeaponInfo("Gun27mm_Autocannon", "Gun27mm_Autocannon_AP", (GameObject) null);
      WeaponInfo weaponInfo4 = QOLPlugin.DuplicateWeaponInfo("Gun27mm_Autocannon", "Gun27mm_Autocannon_HE", (GameObject) null);
      GameObject gameObject16 = PathLookup.Find("gun_27mm_internal_ap", false);
      GameObject gameObject17 = PathLookup.Find("gun_27mm_internal_he", false);
      if ((UnityEngine.Object) gameObject16 != (UnityEngine.Object) null && (UnityEngine.Object) weaponInfo3 != (UnityEngine.Object) null)
      {
        Gun componentInChildren = gameObject16.GetComponentInChildren<Gun>();
        if ((UnityEngine.Object) componentInChildren != (UnityEngine.Object) null)
          componentInChildren.info = weaponInfo3;
      }
      if (!((UnityEngine.Object) gameObject17 != (UnityEngine.Object) null) || !((UnityEngine.Object) weaponInfo4 != (UnityEngine.Object) null))
        return;
      Gun componentInChildren2 = gameObject17.GetComponentInChildren<Gun>();
      if (!((UnityEngine.Object) componentInChildren2 != (UnityEngine.Object) null))
        return;
      componentInChildren2.info = weaponInfo4;
    }),
    ["FuelPod1_P_Setup"] = (Action<GameObject, WeaponInfo, ModificationContext>) ((go, info, ctx) =>
    {
      WeaponInfo weaponInfo = QOLPlugin.DuplicateWeaponInfo("JammingPod1", "FuelPod1_P_info", go);
      if ((UnityEngine.Object) weaponInfo != (UnityEngine.Object) null)
      {
        weaponInfo.name = "Fuel Pod";
        weaponInfo.description = "External fuel pod providing additional fuel capacity for extended range and endurance.";
      }
      GameObject gameObject18 = go.transform.Find("pod")?.gameObject;
      if (!((UnityEngine.Object) gameObject18 != (UnityEngine.Object) null))
        return;
      JammingPod component = gameObject18.GetComponent<JammingPod>();
      if ((UnityEngine.Object) component != (UnityEngine.Object) null)
        UnityEngine.Object.Destroy((UnityEngine.Object) component);
      GameObject gameObject19 = gameObject18.transform.Find("transmitter")?.gameObject;
      if ((UnityEngine.Object) gameObject19 != (UnityEngine.Object) null)
        UnityEngine.Object.Destroy((UnityEngine.Object) gameObject19);
      FuelTank fuelTank = gameObject18.AddComponent<FuelTank>();
      AeroPart aeroPart = gameObject18.AddComponent<AeroPart>();
      aeroPart.mass = 250f;
      Traverse traverse = Traverse.Create((object) fuelTank);
      traverse.Field("fuelCapacity").SetValue((object) 400f);
      traverse.Field("fuelFlowRate").SetValue((object) 0.5f);
      traverse.Field("ignitionPierceMin").SetValue((object) 25);
      traverse.Field("ignitionPierceMax").SetValue((object) 300);
      traverse.Field("ignitionBlastMin").SetValue((object) 25);
      traverse.Field("ignitionBlastMax").SetValue((object) 50);
      traverse.Field("fireIntensity").SetValue((object) 4);
      traverse.Field("leakEffect").SetValue((object) PathLookup.Find("fuelLeak"));
      traverse.Field("fireEffect").SetValue((object) PathLookup.Find("fire_med"));
      traverse.Field("fireball").SetValue((object) PathLookup.Find("fireball_medium"));
      traverse.Field("name").SetValue((object) "FuelPod1");
      traverse.Field("part").SetValue((object) aeroPart);
    }),
    ["JammingPod1_mini_P_Setup"] = (Action<GameObject, WeaponInfo, ModificationContext>) ((go, info, ctx) =>
    {
      WeaponInfo weaponInfo5 = QOLPlugin.DuplicateWeaponInfo("JammingPod1", "JammingPod1_mini_P_info", go);
      if ((UnityEngine.Object) weaponInfo5 != (UnityEngine.Object) null)
      {
        weaponInfo5.jammer = true;
        weaponInfo5.targetRequirements.minRange = 5000f;
        weaponInfo5.targetRequirements.maxRange = 20000f;
      }
      JammingPod componentInChildren = go.GetComponentInChildren<JammingPod>();
      if ((UnityEngine.Object) componentInChildren != (UnityEngine.Object) null)
      {
        FieldInfo field = typeof (JammingPod).GetField("rangeFalloff", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (field != (FieldInfo) null && field.GetValue((object) componentInChildren) is AnimationCurve animationCurve2)
        {
          Keyframe[] keys = new Keyframe[2]
          {
            new Keyframe(0.0f, 0.5f),
            new Keyframe(27000f, 0.0f)
          };
          animationCurve2.SetKeys(keys);
        }
      }
      WeaponInfo weaponInfo6 = ((IEnumerable<WeaponInfo>) Resources.FindObjectsOfTypeAll<WeaponInfo>()).FirstOrDefault<WeaponInfo>((Func<WeaponInfo, bool>) (w => w.name.Equals("JammingPod1", StringComparison.InvariantCultureIgnoreCase)));
      if (!((UnityEngine.Object) weaponInfo6 != (UnityEngine.Object) null))
        return;
      weaponInfo6.jammer = true;
      weaponInfo6.targetRequirements.minRange = 25000f;
      weaponInfo6.targetRequirements.maxRange = 60000f;
    }),
    ["P_PassiveJammer1_Setup"] = (Action<GameObject, WeaponInfo, ModificationContext>) ((go, info, ctx) =>
    {
      AeroPart component = go.GetComponent<AeroPart>();
      if ((UnityEngine.Object) component != (UnityEngine.Object) null)
        UnityEngine.Object.Destroy((UnityEngine.Object) component);
      WeaponInfo original = ((IEnumerable<WeaponInfo>) Resources.FindObjectsOfTypeAll<WeaponInfo>()).FirstOrDefault<WeaponInfo>((Func<WeaponInfo, bool>) (i => i.name.Equals("Radome", StringComparison.InvariantCultureIgnoreCase)));
      if ((UnityEngine.Object) original != (UnityEngine.Object) null)
      {
        WeaponInfo weaponInfo = UnityEngine.Object.Instantiate<WeaponInfo>(original);
        weaponInfo.name = "P_PassiveJammer1_info";
        weaponInfo.weaponPrefab = (GameObject) null;
        weaponInfo.description = "High-tech de-amplification device designed to significantly reduce apparent RCS of the parent aircraft. Consumes electricity while operating passively.";
      }
      foreach (UnityEngine.Object componentsInChild in go.GetComponentsInChildren<MeshCollider>())
        UnityEngine.Object.Destroy(componentsInChild);
    }),
    ["Rocket2_4Podx3_db_Setup"] = (Action<GameObject, WeaponInfo, ModificationContext>) ((go, info, ctx) =>
    {
      PathLookup.Find("Rocket2_4Podx3_db/pylon/pod", false).name = "pod_R";
      PathLookup.Find("Rocket2_4Podx3_db/pylon/pod", false).name = "pod_L";
      PathLookup.Find("Rocket2_4Podx3_db/pylon/pod", false).name = "pod_B";
      PathLookup.Find("Rocket2_4Podx3_db/pylon/pod_B", false)?.SetActive(false);
      PathLookup.Find("Rocket2_4Podx3/pylon/pod", false).name = "pod_A";
      PathLookup.Find("Rocket2_4Podx3/pylon/pod", false).name = "pod_B";
      PathLookup.Find("Rocket2_4Podx3/pylon/pod", false).name = "pod_C";
      PathLookup.Find("AShM2_double/pylon", false).name = "pylon1";
    }),
    ["bomb_125_sextupleP_Setup"] = (Action<GameObject, WeaponInfo, ModificationContext>) ((go, info, ctx) =>
    {
      GameObject original = PathLookup.Find("bomb_125_triple");
      if (!((UnityEngine.Object) original != (UnityEngine.Object) null) || !((UnityEngine.Object) go != (UnityEngine.Object) null))
        return;
      UnityEngine.Object.Instantiate<GameObject>(original).transform.SetParent(go.transform);
    }),
    ["P_FlarePod1_Setup"] = (Action<GameObject, WeaponInfo, ModificationContext>) ((go, info, ctx) =>
    {
      WeaponInfo weaponInfo = QOLPlugin.DuplicateWeaponInfo("JammingPod1", "P_FlarePod1_info", (GameObject) null);
      GameObject gameObject20 = go?.transform.Find("gunpod")?.gameObject;
      if ((UnityEngine.Object) gameObject20 == (UnityEngine.Object) null)
      {
        ctx.Logger.LogWarning((object) "[P_FlarePod1_Setup] gunpod child not found");
      }
      else
      {
        GameObject gameObject21 = new GameObject("ejection_L");
        gameObject21.transform.SetParent(gameObject20.transform);
        GameObject gameObject22 = new GameObject("ejection_R");
        gameObject22.transform.SetParent(gameObject20.transform);
        Gun component14 = gameObject20.GetComponent<Gun>();
        if ((UnityEngine.Object) component14 != (UnityEngine.Object) null)
          UnityEngine.Object.Destroy((UnityEngine.Object) component14);
        SetGlobalParticles component15 = gameObject20.GetComponent<SetGlobalParticles>();
        if ((UnityEngine.Object) component15 != (UnityEngine.Object) null)
          UnityEngine.Object.Destroy((UnityEngine.Object) component15);
        foreach (UnityEngine.Object component16 in gameObject20.GetComponents<AudioSource>())
          UnityEngine.Object.Destroy(component16);
        QOLPlugin.DestroyAtPaths("P_FlarePod1/gunpod/recoil", "P_FlarePod1/gunpod/muzzle");
        FlareEjector flareEjector = gameObject20.AddComponent<FlareEjector>();
        Traverse traverse = Traverse.Create((object) flareEjector);
        FlareEjector component17 = PathLookup.Find("trainer")?.GetComponent<FlareEjector>();
        if ((UnityEngine.Object) component17 != (UnityEngine.Object) null)
        {
          flareEjector.displayImage = component17.displayImage;
          flareEjector.displayName = component17.displayName;
        }
        traverse.Field("flarePrefab").SetValue((object) PathLookup.Find("IRFlare"));
        AudioSource componentInChildren = go.GetComponentInChildren<AudioSource>();
        FlareEjector.EjectionPoint ejectionPoint1 = new FlareEjector.EjectionPoint()
        {
          transform = gameObject21.transform,
          sound = componentInChildren
        };
        FlareEjector.EjectionPoint ejectionPoint2 = new FlareEjector.EjectionPoint()
        {
          transform = gameObject22.transform,
          sound = componentInChildren
        };
        traverse.Field("ejectionPoints").SetValue((object) new FlareEjector.EjectionPoint[2]
        {
          ejectionPoint1,
          ejectionPoint2
        });
        if ((UnityEngine.Object) weaponInfo != (UnityEngine.Object) null)
        {
          weaponInfo.gun = false;
          weaponInfo.description = "A Modular Countermeasures Pod loaded with 112 IR Flares. Automatically syncs with existing internal IR Flare launchers. The IR Flare icon will turn red if all internal flares are depleted first.";
          weaponInfo.boresight = false;
          weaponInfo.pierceDamage = 0.0f;
          weaponInfo.armorTierEffectiveness = 0.0f;
          weaponInfo.effectiveness.antiAir = 0.0f;
          weaponInfo.effectiveness.antiRadar = 0.0f;
          weaponInfo.effectiveness.antiSurface = 0.0f;
          weaponInfo.effectiveness.antiMissile = 0.0f;
        }
        GameObject gameObject23 = QOLPlugin.LoadFirstAssetFromBundle<GameObject>(QOLPlugin.GetBundlePath("P_Pods1.P_FlarePod1.bundle"));
        if (!((UnityEngine.Object) gameObject23 != (UnityEngine.Object) null))
          return;
        Mesh sharedMesh = gameObject23.GetComponent<MeshFilter>()?.sharedMesh;
        MeshFilter component18 = gameObject20.GetComponent<MeshFilter>();
        MeshRenderer component19 = gameObject20.GetComponent<MeshRenderer>();
        if ((UnityEngine.Object) component18 != (UnityEngine.Object) null && (UnityEngine.Object) sharedMesh != (UnityEngine.Object) null)
          component18.mesh = sharedMesh;
        if (!((UnityEngine.Object) component19 != (UnityEngine.Object) null))
          return;
        Material sharedMaterial1 = PathLookup.Find("gun_12.7mm_pod/gunpod")?.GetComponent<MeshRenderer>()?.sharedMaterial;
        MeshRenderer component20 = PathLookup.Find("EW1")?.GetComponent<MeshRenderer>();
        Material sharedMaterial2 = component20 == null || component20.sharedMaterials.Length <= 1 ? (Material) null : component20.sharedMaterials[1];
        if ((UnityEngine.Object) sharedMaterial1 != (UnityEngine.Object) null && (UnityEngine.Object) sharedMaterial2 != (UnityEngine.Object) null)
        {
          component19.sharedMaterials = new Material[2]
          {
            sharedMaterial1,
            sharedMaterial2
          };
        }
        else
        {
          if (!((UnityEngine.Object) sharedMaterial1 != (UnityEngine.Object) null))
            return;
          component19.sharedMaterial = sharedMaterial1;
        }
      }
    })
  };

  public static void Run(string id, GameObject go, WeaponInfo info, ModificationContext ctx)
  {
    Action<GameObject, WeaponInfo, ModificationContext> action;
    if (WeaponVariantPostProcessors.Handlers.TryGetValue(id, out action))
    {
      try
      {
        action(go, info, ctx);
      }
      catch (Exception ex)
      {
        ctx.Logger.LogError((object) $"[WeaponVariantPostProcessors] Handler '{id}' failed: {ex.Message}");
      }
    }
    else
      ctx.Logger.LogWarning((object) ("[WeaponVariantPostProcessors] Unknown post-processor: " + id));
  }

  private static void LoadAssetBundleMesh(
    ModificationContext ctx,
    string bundlePath,
    GameObject target,
    string bundleObjectName,
    string materialSourcePath)
  {
    string bundlePath1 = QOLPlugin.GetBundlePath(bundlePath.StartsWith("Resources.") ? bundlePath.Substring(10) : bundlePath);
    GameObject gameObject1 = bundleObjectName != null ? QOLPlugin.LoadNamedAssetFromBundle<GameObject>(bundlePath1, bundleObjectName) : QOLPlugin.LoadFirstAssetFromBundle<GameObject>(bundlePath1);
    if ((UnityEngine.Object) gameObject1 == (UnityEngine.Object) null)
      return;
    MeshFilter component1 = gameObject1.GetComponent<MeshFilter>();
    MeshFilter component2 = target.GetComponent<MeshFilter>();
    if ((UnityEngine.Object) component1 != (UnityEngine.Object) null && (UnityEngine.Object) component2 != (UnityEngine.Object) null)
      component2.mesh = component1.sharedMesh;
    MeshRenderer component3 = target.GetComponent<MeshRenderer>();
    if (!((UnityEngine.Object) component3 != (UnityEngine.Object) null))
      return;
    if (materialSourcePath != null)
    {
      GameObject gameObject2 = PathLookup.Find(materialSourcePath);
      if (!((UnityEngine.Object) gameObject2 != (UnityEngine.Object) null))
        return;
      Material sharedMaterial = gameObject2.GetComponent<MeshRenderer>()?.sharedMaterial;
      if (!((UnityEngine.Object) sharedMaterial != (UnityEngine.Object) null))
        return;
      component3.sharedMaterial = sharedMaterial;
    }
    else
    {
      MeshRenderer component4 = gameObject1.GetComponent<MeshRenderer>();
      if (!((UnityEngine.Object) component4 != (UnityEngine.Object) null))
        return;
      component3.sharedMaterial = component4.sharedMaterial;
    }
  }

  private static void LoadP_SAMRadar1AssetBundle(ModificationContext ctx, GameObject go)
  {
    GameObject gameObject1 = QOLPlugin.LoadNamedAssetFromBundle<GameObject>(QOLPlugin.GetBundlePath("P_Missiles1.P_SAMRadar1.bundle"), "P_SAMRadar1");
    if ((UnityEngine.Object) gameObject1 == (UnityEngine.Object) null)
      return;
    Transform transform = gameObject1.transform.Find("booster");
    Mesh sharedMesh1 = gameObject1.GetComponent<MeshFilter>()?.sharedMesh;
    Mesh sharedMesh2 = transform?.GetComponent<MeshFilter>()?.sharedMesh;
    Material pMissiles1 = QOLPlugin.p_missiles1;
    MeshFilter component1 = go.GetComponent<MeshFilter>();
    MeshRenderer component2 = go.GetComponent<MeshRenderer>();
    if ((UnityEngine.Object) component1 != (UnityEngine.Object) null && (UnityEngine.Object) sharedMesh1 != (UnityEngine.Object) null)
      component1.mesh = sharedMesh1;
    if ((UnityEngine.Object) component2 != (UnityEngine.Object) null && (UnityEngine.Object) pMissiles1 != (UnityEngine.Object) null)
      component2.sharedMaterial = pMissiles1;
    GameObject gameObject2 = PathLookup.Find("P_SAMRadar1/VLS_Booster", false);
    if ((UnityEngine.Object) gameObject2 != (UnityEngine.Object) null)
    {
      MeshFilter component3 = gameObject2.GetComponent<MeshFilter>();
      MeshRenderer component4 = gameObject2.GetComponent<MeshRenderer>();
      if ((UnityEngine.Object) component3 != (UnityEngine.Object) null && (UnityEngine.Object) sharedMesh2 != (UnityEngine.Object) null)
        component3.mesh = sharedMesh2;
      if ((UnityEngine.Object) component4 != (UnityEngine.Object) null && (UnityEngine.Object) pMissiles1 != (UnityEngine.Object) null)
        component4.sharedMaterial = pMissiles1;
    }
    GameObject gameObject3 = PathLookup.Find("P_SAMRadar1_single/pylon/aam2", false);
    if ((UnityEngine.Object) gameObject3 != (UnityEngine.Object) null)
    {
      MeshFilter component5 = gameObject3.GetComponent<MeshFilter>();
      MeshRenderer component6 = gameObject3.GetComponent<MeshRenderer>();
      if ((UnityEngine.Object) component5 != (UnityEngine.Object) null && (UnityEngine.Object) sharedMesh1 != (UnityEngine.Object) null)
        component5.mesh = sharedMesh1;
      if ((UnityEngine.Object) component6 != (UnityEngine.Object) null && (UnityEngine.Object) pMissiles1 != (UnityEngine.Object) null)
        component6.sharedMaterial = pMissiles1;
    }
    GameObject gameObject4 = PathLookup.Find("P_SAMRadar1_internalx3", false);
    if (!((UnityEngine.Object) gameObject4 != (UnityEngine.Object) null))
      return;
    foreach (Transform componentsInChild in gameObject4.GetComponentsInChildren<Transform>(true))
    {
      if (componentsInChild.gameObject.name.Contains("bomb500"))
      {
        MeshFilter component7 = componentsInChild.gameObject.GetComponent<MeshFilter>();
        MeshRenderer component8 = componentsInChild.gameObject.GetComponent<MeshRenderer>();
        if ((UnityEngine.Object) component7 != (UnityEngine.Object) null && (UnityEngine.Object) sharedMesh1 != (UnityEngine.Object) null)
          component7.sharedMesh = sharedMesh1;
        if ((UnityEngine.Object) component8 != (UnityEngine.Object) null && (UnityEngine.Object) pMissiles1 != (UnityEngine.Object) null)
          component8.sharedMaterial = pMissiles1;
      }
    }
  }

  private static void LoadP_HAsM1AssetBundle(ModificationContext ctx, GameObject go)
  {
    GameObject gameObject1 = QOLPlugin.LoadNamedAssetFromBundle<GameObject>(QOLPlugin.GetBundlePath("P_Missiles1.P_HAsM1.bundle"), "HAsM1");
    if ((UnityEngine.Object) gameObject1 == (UnityEngine.Object) null)
      return;
    Transform transform1 = gameObject1.transform.Find("VLSBooster");
    Mesh sharedMesh1 = gameObject1.GetComponent<MeshFilter>()?.sharedMesh;
    Mesh sharedMesh2 = transform1?.GetComponent<MeshFilter>()?.sharedMesh;
    Material pMissiles1 = QOLPlugin.p_missiles1;
    MeshFilter component1 = go.GetComponent<MeshFilter>();
    MeshRenderer component2 = go.GetComponent<MeshRenderer>();
    if ((UnityEngine.Object) component1 != (UnityEngine.Object) null && (UnityEngine.Object) sharedMesh1 != (UnityEngine.Object) null)
      component1.mesh = sharedMesh1;
    if ((UnityEngine.Object) component2 != (UnityEngine.Object) null && (UnityEngine.Object) pMissiles1 != (UnityEngine.Object) null)
      component2.sharedMaterial = pMissiles1;
    GameObject gameObject2 = PathLookup.Find("P_HAsM1/VLS_Booster", false);
    if ((UnityEngine.Object) gameObject2 != (UnityEngine.Object) null)
    {
      MeshFilter component3 = gameObject2.GetComponent<MeshFilter>();
      MeshRenderer component4 = gameObject2.GetComponent<MeshRenderer>();
      if ((UnityEngine.Object) component3 != (UnityEngine.Object) null && (UnityEngine.Object) sharedMesh2 != (UnityEngine.Object) null)
        component3.mesh = sharedMesh2;
      if ((UnityEngine.Object) component4 != (UnityEngine.Object) null && (UnityEngine.Object) pMissiles1 != (UnityEngine.Object) null)
        component4.sharedMaterial = pMissiles1;
    }
    GameObject gameObject3 = PathLookup.Find("P_HAsM1_internal", false);
    if (!((UnityEngine.Object) gameObject3 != (UnityEngine.Object) null))
      return;
    Transform transform2 = gameObject3.transform.Find("AShM1");
    if (!((UnityEngine.Object) transform2 != (UnityEngine.Object) null))
      return;
    MeshFilter component5 = transform2.GetComponent<MeshFilter>();
    MeshRenderer component6 = transform2.GetComponent<MeshRenderer>();
    if ((UnityEngine.Object) component5 != (UnityEngine.Object) null && (UnityEngine.Object) sharedMesh1 != (UnityEngine.Object) null)
      component5.mesh = sharedMesh1;
    if ((UnityEngine.Object) component6 != (UnityEngine.Object) null && (UnityEngine.Object) pMissiles1 != (UnityEngine.Object) null)
      component6.sharedMaterial = pMissiles1;
    Transform transform3 = transform2.Find("fin_LL");
    if (!((UnityEngine.Object) transform3 != (UnityEngine.Object) null))
      return;
    MeshFilter component7 = transform3.GetComponent<MeshFilter>();
    MeshRenderer component8 = transform3.GetComponent<MeshRenderer>();
    if ((UnityEngine.Object) component7 != (UnityEngine.Object) null && (UnityEngine.Object) sharedMesh2 != (UnityEngine.Object) null)
      component7.mesh = sharedMesh2;
    if (!((UnityEngine.Object) component8 != (UnityEngine.Object) null) || !((UnityEngine.Object) pMissiles1 != (UnityEngine.Object) null))
      return;
    component8.sharedMaterial = pMissiles1;
  }

  private static void CleanupTrailerMount(GameObject mountGO, string mountName)
  {
    GameObject gameObject = PathLookup.Find(mountName + "/HLT-R", false);
    if ((UnityEngine.Object) gameObject != (UnityEngine.Object) null)
    {
      MeshRenderer component = gameObject.GetComponent<MeshRenderer>();
      if ((UnityEngine.Object) component != (UnityEngine.Object) null)
        UnityEngine.Object.Destroy((UnityEngine.Object) component);
    }
    NetworkIdentity componentInChildren1 = mountGO.GetComponentInChildren<NetworkIdentity>();
    if ((UnityEngine.Object) componentInChildren1 != (UnityEngine.Object) null)
      UnityEngine.Object.Destroy((UnityEngine.Object) componentInChildren1);
    Rigidbody componentInChildren2 = mountGO.GetComponentInChildren<Rigidbody>();
    if ((UnityEngine.Object) componentInChildren2 != (UnityEngine.Object) null)
      UnityEngine.Object.Destroy((UnityEngine.Object) componentInChildren2);
    GroundVehicle componentInChildren3 = mountGO.GetComponentInChildren<GroundVehicle>();
    if ((UnityEngine.Object) componentInChildren3 != (UnityEngine.Object) null)
      UnityEngine.Object.Destroy((UnityEngine.Object) componentInChildren3);
    Gun componentInChildren4 = mountGO.GetComponentInChildren<Gun>();
    if ((UnityEngine.Object) componentInChildren4 != (UnityEngine.Object) null)
      UnityEngine.Object.Destroy((UnityEngine.Object) componentInChildren4);
    Turret componentInChildren5 = mountGO.GetComponentInChildren<Turret>();
    if ((UnityEngine.Object) componentInChildren5 != (UnityEngine.Object) null)
      UnityEngine.Object.Destroy((UnityEngine.Object) componentInChildren5);
    Laser componentInChildren6 = mountGO.GetComponentInChildren<Laser>();
    if ((UnityEngine.Object) componentInChildren6 != (UnityEngine.Object) null)
      UnityEngine.Object.Destroy((UnityEngine.Object) componentInChildren6);
    TargetDetector componentInChildren7 = mountGO.GetComponentInChildren<TargetDetector>();
    if ((UnityEngine.Object) componentInChildren7 != (UnityEngine.Object) null)
      UnityEngine.Object.Destroy((UnityEngine.Object) componentInChildren7);
    MissileLauncher componentInChildren8 = mountGO.GetComponentInChildren<MissileLauncher>();
    if ((UnityEngine.Object) componentInChildren8 != (UnityEngine.Object) null)
      UnityEngine.Object.Destroy((UnityEngine.Object) componentInChildren8);
    foreach (UnityEngine.Object componentsInChild in mountGO.GetComponentsInChildren<BoxCollider>())
      UnityEngine.Object.Destroy(componentsInChild);
    foreach (UnityEngine.Object componentsInChild in mountGO.GetComponentsInChildren<MeshCollider>())
      UnityEngine.Object.Destroy(componentsInChild);
    foreach (UnityEngine.Object componentsInChild in mountGO.GetComponentsInChildren<UnitPart>())
      UnityEngine.Object.Destroy(componentsInChild);
  }

  private static void LoadP_GLB1AssetBundle(ModificationContext ctx, GameObject go)
  {
    QOLPlugin.WithAssetBundle(QOLPlugin.GetBundlePath("P_Missiles1.P_GLB1.bundle"), (Action<AssetBundle>) (bundle =>
    {
      Mesh mesh1 = ((IEnumerable<Mesh>) bundle.LoadAllAssets<Mesh>()).FirstOrDefault<Mesh>((Func<Mesh, bool>) (m => m.name.Equals("P_GLB1", StringComparison.InvariantCultureIgnoreCase)));
      Mesh mesh2 = ((IEnumerable<Mesh>) bundle.LoadAllAssets<Mesh>()).FirstOrDefault<Mesh>((Func<Mesh, bool>) (m => m.name.Equals("P_GLB1_closed", StringComparison.InvariantCultureIgnoreCase)));
      if ((UnityEngine.Object) mesh1 == (UnityEngine.Object) null || (UnityEngine.Object) mesh2 == (UnityEngine.Object) null)
        return;
      Material pMissiles1 = QOLPlugin.p_missiles1;
      MeshFilter component1 = go.GetComponent<MeshFilter>();
      MeshRenderer component2 = go.GetComponent<MeshRenderer>();
      if ((UnityEngine.Object) component1 != (UnityEngine.Object) null)
        component1.mesh = mesh1;
      if ((UnityEngine.Object) component2 != (UnityEngine.Object) null)
        component2.sharedMaterials = new Material[1]
        {
          pMissiles1
        };
      (string, string)[] valueTupleArray = new (string, string)[6]
      {
        ("P_GLB1_single", "bomb"),
        ("P_GLB1_double", "bomb"),
        ("P_GLB1_triple", "bomb"),
        ("P_GLB1_internal", "agm1"),
        ("P_GLB1_internalx2", "bomb"),
        ("P_GLB1_internalx6", "bomb500")
      };
      foreach ((string path, string str) in valueTupleArray)
      {
        GameObject gameObject = PathLookup.Find(path, false);
        if (!((UnityEngine.Object) gameObject == (UnityEngine.Object) null))
        {
          foreach (Transform componentsInChild in gameObject.GetComponentsInChildren<Transform>(true))
          {
            if (componentsInChild.gameObject.name.Contains(str))
            {
              MeshFilter component3 = componentsInChild.gameObject.GetComponent<MeshFilter>();
              MeshRenderer component4 = componentsInChild.gameObject.GetComponent<MeshRenderer>();
              if ((UnityEngine.Object) component3 != (UnityEngine.Object) null)
                component3.sharedMesh = mesh2;
              if ((UnityEngine.Object) component4 != (UnityEngine.Object) null)
                component4.sharedMaterials = new Material[1]
                {
                  pMissiles1
                };
            }
            else if (componentsInChild.gameObject.name.Contains("RotationTransform"))
              UnityEngine.Object.Destroy((UnityEngine.Object) componentsInChild.gameObject);
          }
        }
      }
    }));
  }

  private static void LoadP_KEM1AssetBundle(ModificationContext ctx, GameObject go)
  {
    QOLPlugin.WithAssetBundle(QOLPlugin.GetBundlePath("P_Missiles1.P_KEM1.bundle"), (Action<AssetBundle>) (bundle =>
    {
      GameObject gameObject1 = ((IEnumerable<GameObject>) bundle.LoadAllAssets<GameObject>()).FirstOrDefault<GameObject>((Func<GameObject, bool>) (obj => obj.name.Equals("P_KEM1", StringComparison.InvariantCultureIgnoreCase)));
      if ((UnityEngine.Object) gameObject1 == (UnityEngine.Object) null)
        return;
      Mesh sharedMesh = gameObject1.GetComponent<MeshFilter>()?.sharedMesh;
      if ((UnityEngine.Object) sharedMesh == (UnityEngine.Object) null)
        return;
      Material pMissiles1 = QOLPlugin.p_missiles1;
      MeshFilter component1 = go.GetComponent<MeshFilter>();
      MeshRenderer component2 = go.GetComponent<MeshRenderer>();
      if ((UnityEngine.Object) component1 != (UnityEngine.Object) null)
        component1.mesh = sharedMesh;
      if ((UnityEngine.Object) component2 != (UnityEngine.Object) null)
        component2.sharedMaterials = new Material[1]
        {
          pMissiles1
        };
      string[] strArray = new string[2]
      {
        "P_KEM1_single",
        "P_KEM1_double"
      };
      foreach (string path in strArray)
      {
        GameObject gameObject2 = PathLookup.Find(path, false);
        if (!((UnityEngine.Object) gameObject2 == (UnityEngine.Object) null))
        {
          foreach (Transform componentsInChild in gameObject2.GetComponentsInChildren<Transform>(true))
          {
            if (componentsInChild.gameObject.name.Contains("agm1"))
            {
              MeshFilter component3 = componentsInChild.gameObject.GetComponent<MeshFilter>();
              MeshRenderer component4 = componentsInChild.gameObject.GetComponent<MeshRenderer>();
              if ((UnityEngine.Object) component3 != (UnityEngine.Object) null)
                component3.sharedMesh = sharedMesh;
              if ((UnityEngine.Object) component4 != (UnityEngine.Object) null)
                component4.sharedMaterials = new Material[1]
                {
                  pMissiles1
                };
            }
          }
        }
      }
    }), true);
  }
}

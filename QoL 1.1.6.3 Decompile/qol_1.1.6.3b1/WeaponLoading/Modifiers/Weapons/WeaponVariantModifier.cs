// Decompiled with JetBrains decompiler
// Type: qol.WeaponLoading.Modifiers.Weapons.WeaponVariantModifier
// Assembly: qol, Version=1.1.6.3, Culture=neutral, PublicKeyToken=null
// MVID: 3B6539EA-E38C-45EE-8FFF-FC58CC42BADC
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\BepInEx\plugins\qol_1.1.6.3b1.dll

using qol.Utilities;
using qol.WeaponLoading.Configs;
using qol.WeaponLoading.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#nullable disable
namespace qol.WeaponLoading.Modifiers.Weapons;

public class WeaponVariantModifier : IEntityModifier
{
  public string ModifierId => "WeaponVariants";

  public int Priority => 5;

  public IReadOnlyList<string> Dependencies => (IReadOnlyList<string>) Array.Empty<string>();

  public bool CanApply(ModificationContext context) => true;

  public void Apply(ModificationContext context)
  {
    int num1 = 0;
    int num2 = 0;
    foreach (WeaponVariantDef variant in WeaponVariantConfigs.Variants)
    {
      if (!variant.Enabled)
      {
        ++num2;
      }
      else
      {
        try
        {
          switch (variant.Type)
          {
            case VariantType.Missile:
              this.CreateMissileVariant(context, variant);
              break;
            case VariantType.Vehicle:
              this.CreateVehicleVariant(context, variant);
              break;
            case VariantType.Building:
              this.CreateBuildingVariant(context, variant);
              break;
            case VariantType.MountOnly:
              this.CreateMountOnlyVariant(context, variant);
              break;
          }
          ++num1;
        }
        catch (Exception ex)
        {
          context.Logger.LogError((object) $"[{this.ModifierId}] Failed to create variant '{variant.VariantId}': {ex.Message}");
        }
      }
    }
    int count1 = context.Encyclopedia.missiles.Count;
    int count2 = context.Encyclopedia.weaponMounts.Count;
    int count3 = context.Encyclopedia.vehicles.Count;
    int count4 = context.Encyclopedia.buildings.Count;
    context.Logger.LogInfo((object) $"[{this.ModifierId}] Created {num1} variants, skipped {num2} disabled");
    context.Logger.LogInfo((object) $"[{this.ModifierId}] Encyclopedia totals - Missiles: {count1}, Mounts: {count2}, Vehicles: {count3}, Buildings: {count4}");
  }

  private void CreateMissileVariant(ModificationContext ctx, WeaponVariantDef def)
  {
    GameObject gameObject = QOLPlugin.DuplicatePrefab(def.SourcePrefab, def.NewPrefabName);
    if ((UnityEngine.Object) gameObject == (UnityEngine.Object) null)
    {
      ctx.Logger.LogWarning((object) $"[{this.ModifierId}] Prefab '{def.SourcePrefab}' not found for '{def.VariantId}'");
    }
    else
    {
      WeaponInfo info;
      if (!string.IsNullOrEmpty(def.NewInfoName))
      {
        info = QOLPlugin.DuplicateWeaponInfo(def.SourceInfo, def.NewInfoName, gameObject);
        if ((UnityEngine.Object) info == (UnityEngine.Object) null)
        {
          ctx.Logger.LogWarning((object) $"[{this.ModifierId}] WeaponInfo '{def.SourceInfo}' not found for '{def.VariantId}'");
          return;
        }
      }
      else
        info = ((IEnumerable<WeaponInfo>) Resources.FindObjectsOfTypeAll<WeaponInfo>()).FirstOrDefault<WeaponInfo>((Func<WeaponInfo, bool>) (i => i.name.Equals(def.SourceInfo, StringComparison.InvariantCultureIgnoreCase)));
      MissileDefinition missileDefinition = QOLPlugin.DuplicateMissileDefinition(!string.IsNullOrEmpty(def.SourceDefinition) ? def.SourceDefinition : def.SourcePrefab, def.NewPrefabName, gameObject);
      if ((UnityEngine.Object) missileDefinition != (UnityEngine.Object) null)
        QOLPlugin.AddMissileToEncyclopedia(ctx.Encyclopedia, def.NewPrefabName, missileDefinition);
      ctx.Registry.RegisterGameObject(def.NewPrefabName, gameObject);
      if ((UnityEngine.Object) info != (UnityEngine.Object) null)
        ctx.Registry.RegisterWeaponInfo(def.NewInfoName ?? def.SourceInfo, info);
      if ((UnityEngine.Object) missileDefinition != (UnityEngine.Object) null)
        ctx.Registry.RegisterMissileDefinition(def.NewPrefabName, missileDefinition);
      if (def.Mounts != null)
      {
        foreach (MountDef mount in def.Mounts)
          this.CreateMount(ctx, mount, info, gameObject);
      }
      if (!string.IsNullOrEmpty(def.PostProcessorId))
        WeaponVariantPostProcessors.Run(def.PostProcessorId, gameObject, info, ctx);
      ctx.Logger.LogDebug((object) $"[{this.ModifierId}] Created missile variant '{def.VariantId}'");
    }
  }

  private void CreateVehicleVariant(ModificationContext ctx, WeaponVariantDef def)
  {
    GameObject gameObject = QOLPlugin.DuplicatePrefab(def.SourcePrefab, def.NewPrefabName);
    if ((UnityEngine.Object) gameObject == (UnityEngine.Object) null)
    {
      ctx.Logger.LogWarning((object) $"[{this.ModifierId}] Prefab '{def.SourcePrefab}' not found for '{def.VariantId}'");
    }
    else
    {
      VehicleDefinition vehicle = QOLPlugin.DuplicateVehicleDefinition(!string.IsNullOrEmpty(def.SourceDefinition) ? def.SourceDefinition : def.SourcePrefab, def.NewPrefabName, gameObject);
      if ((UnityEngine.Object) vehicle != (UnityEngine.Object) null)
        QOLPlugin.AddVehicleToEncyclopedia(ctx.Encyclopedia, def.NewPrefabName, vehicle);
      ctx.Registry.RegisterGameObject(def.NewPrefabName, gameObject);
      if (!string.IsNullOrEmpty(def.PostProcessorId))
        WeaponVariantPostProcessors.Run(def.PostProcessorId, gameObject, (WeaponInfo) null, ctx);
      ctx.Logger.LogDebug((object) $"[{this.ModifierId}] Created vehicle variant '{def.VariantId}'");
    }
  }

  private void CreateBuildingVariant(ModificationContext ctx, WeaponVariantDef def)
  {
    GameObject gameObject = QOLPlugin.DuplicatePrefab(def.SourcePrefab, def.NewPrefabName);
    if ((UnityEngine.Object) gameObject == (UnityEngine.Object) null)
    {
      ctx.Logger.LogWarning((object) $"[{this.ModifierId}] Prefab '{def.SourcePrefab}' not found for '{def.VariantId}'");
    }
    else
    {
      BuildingDefinition building = QOLPlugin.DuplicateBuildingDefinition(!string.IsNullOrEmpty(def.SourceDefinition) ? def.SourceDefinition : def.SourcePrefab, def.NewPrefabName, gameObject);
      if ((UnityEngine.Object) building != (UnityEngine.Object) null)
        QOLPlugin.AddBuildingToEncyclopedia(ctx.Encyclopedia, def.NewPrefabName, building);
      ctx.Registry.RegisterGameObject(def.NewPrefabName, gameObject);
      if (!string.IsNullOrEmpty(def.PostProcessorId))
        WeaponVariantPostProcessors.Run(def.PostProcessorId, gameObject, (WeaponInfo) null, ctx);
      ctx.Logger.LogDebug((object) $"[{this.ModifierId}] Created building variant '{def.VariantId}'");
    }
  }

  private void CreateMountOnlyVariant(ModificationContext ctx, WeaponVariantDef def)
  {
    WeaponInfo info = ((IEnumerable<WeaponInfo>) Resources.FindObjectsOfTypeAll<WeaponInfo>()).FirstOrDefault<WeaponInfo>((Func<WeaponInfo, bool>) (i => i.name.Equals(def.SourceInfo, StringComparison.InvariantCultureIgnoreCase)));
    if ((UnityEngine.Object) info == (UnityEngine.Object) null)
    {
      ctx.Logger.LogWarning((object) $"[{this.ModifierId}] WeaponInfo '{def.SourceInfo}' not found for '{def.VariantId}'");
    }
    else
    {
      GameObject weaponPrefab = info.weaponPrefab;
      if (def.Mounts != null)
      {
        foreach (MountDef mount in def.Mounts)
          this.CreateMount(ctx, mount, info, weaponPrefab);
      }
      if (!string.IsNullOrEmpty(def.PostProcessorId))
      {
        GameObject go = (GameObject) null;
        if (def.Mounts != null && def.Mounts.Length != 0)
          go = PathLookup.Find(def.Mounts[0].NewName, false);
        WeaponVariantPostProcessors.Run(def.PostProcessorId, go, info, ctx);
      }
      ctx.Logger.LogDebug((object) $"[{this.ModifierId}] Created mount-only variant '{def.VariantId}'");
    }
  }

  private void CreateMount(
    ModificationContext ctx,
    MountDef mount,
    WeaponInfo info,
    GameObject meshSourcePrefab)
  {
    GameObject gameObject = QOLPlugin.DuplicatePrefab(mount.SourceMount, mount.NewName);
    if ((UnityEngine.Object) gameObject == (UnityEngine.Object) null)
    {
      ctx.Logger.LogWarning((object) $"[{this.ModifierId}] Mount prefab '{mount.SourceMount}' not found for '{mount.NewName}'");
    }
    else
    {
      WeaponMount mount1 = QOLPlugin.DuplicateWeaponMount(mount.SourceMount, mount.NewName, gameObject, info);
      if ((UnityEngine.Object) mount1 == (UnityEngine.Object) null)
      {
        ctx.Logger.LogWarning((object) $"[{this.ModifierId}] Failed to duplicate mount '{mount.SourceMount}'");
      }
      else
      {
        QOLPlugin.AddMountToEncyclopedia(ctx.Encyclopedia, mount.NewName, mount1);
        ctx.Registry.RegisterWeaponMount(mount.NewName, mount1);
        ctx.Registry.RegisterGameObject(mount.NewName, gameObject);
        if (!string.IsNullOrEmpty(mount.MeshSwapPattern))
        {
          GameObject source = meshSourcePrefab;
          if (!string.IsNullOrEmpty(mount.MeshSourcePath))
            source = PathLookup.Find(mount.MeshSourcePath, false);
          if ((UnityEngine.Object) source != (UnityEngine.Object) null)
            this.ApplyMeshSwap(gameObject, source, mount.MeshSwapPattern, mount.MeshScale);
        }
        if (mount.DestroyPatterns == null)
          return;
        foreach (string destroyPattern in mount.DestroyPatterns)
          this.DestroyChildrenMatching(gameObject, destroyPattern);
      }
    }
  }

  private void ApplyMeshSwap(GameObject mount, GameObject source, string pattern, float scale)
  {
    Mesh mesh = (Mesh) null;
    Material material = (Material) null;
    MeshFilter component1 = source.GetComponent<MeshFilter>();
    MeshRenderer component2 = source.GetComponent<MeshRenderer>();
    if ((UnityEngine.Object) component1 != (UnityEngine.Object) null)
      mesh = component1.sharedMesh;
    if ((UnityEngine.Object) component2 != (UnityEngine.Object) null)
      material = component2.sharedMaterial;
    if ((UnityEngine.Object) mesh == (UnityEngine.Object) null)
      return;
    foreach (Transform componentsInChild in mount.GetComponentsInChildren<Transform>(true))
    {
      if (!((UnityEngine.Object) componentsInChild.gameObject == (UnityEngine.Object) mount) && componentsInChild.gameObject.name.Contains(pattern, StringComparison.InvariantCultureIgnoreCase))
      {
        MeshFilter component3 = componentsInChild.gameObject.GetComponent<MeshFilter>();
        MeshRenderer component4 = componentsInChild.gameObject.GetComponent<MeshRenderer>();
        if ((UnityEngine.Object) component3 != (UnityEngine.Object) null)
          component3.sharedMesh = mesh;
        if ((UnityEngine.Object) component4 != (UnityEngine.Object) null && (UnityEngine.Object) material != (UnityEngine.Object) null)
          component4.sharedMaterials = new Material[1]
          {
            material
          };
        if ((double) scale != 1.0)
          componentsInChild.localScale = Vector3.one * scale;
      }
    }
  }

  private void DestroyChildrenMatching(GameObject parent, string pattern)
  {
    foreach (Transform componentsInChild in parent.GetComponentsInChildren<Transform>(true))
    {
      if (!((UnityEngine.Object) componentsInChild.gameObject == (UnityEngine.Object) parent) && componentsInChild.gameObject.name.Contains(pattern, StringComparison.InvariantCultureIgnoreCase))
        UnityEngine.Object.Destroy((UnityEngine.Object) componentsInChild.gameObject);
    }
  }
}

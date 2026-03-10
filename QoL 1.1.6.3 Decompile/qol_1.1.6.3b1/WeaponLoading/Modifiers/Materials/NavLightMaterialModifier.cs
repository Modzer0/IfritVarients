// Decompiled with JetBrains decompiler
// Type: qol.WeaponLoading.Modifiers.Materials.NavLightMaterialModifier
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
namespace qol.WeaponLoading.Modifiers.Materials;

public class NavLightMaterialModifier : IEntityModifier
{
  public string ModifierId => "NavLightMaterials";

  public int Priority => 15;

  public IReadOnlyList<string> Dependencies => (IReadOnlyList<string>) Array.Empty<string>();

  public bool CanApply(ModificationContext context) => true;

  public void Apply(ModificationContext context)
  {
    Material[] objectsOfTypeAll = Resources.FindObjectsOfTypeAll<Material>();
    int num1 = 0;
    foreach ((string MaterialName, Color Color) in (IEnumerable<(string MaterialName, Color Color)>) NavLightConfigs.NavLightColors)
    {
      Material material = ((IEnumerable<Material>) objectsOfTypeAll).FirstOrDefault<Material>((Func<Material, bool>) (mat => mat.name.Equals(MaterialName, StringComparison.InvariantCultureIgnoreCase)));
      if ((UnityEngine.Object) material != (UnityEngine.Object) null)
      {
        material.color = Color;
        context.Logger.LogDebug((object) $"[{this.ModifierId}] Set {MaterialName} color to ({Color.r:F2}, {Color.g:F2}, {Color.b:F2}, {Color.a:F2})");
        ++num1;
      }
      else
        context.Logger.LogWarning((object) $"[{this.ModifierId}] Material '{MaterialName}' not found");
    }
    (string MaterialName, Color Color, float Smoothness) matteBlackConfig = NavLightConfigs.MatteBlack;
    Material material1 = ((IEnumerable<Material>) objectsOfTypeAll).FirstOrDefault<Material>((Func<Material, bool>) (mat => mat.name.Equals(matteBlackConfig.MaterialName, StringComparison.InvariantCultureIgnoreCase)));
    if ((UnityEngine.Object) material1 != (UnityEngine.Object) null)
    {
      material1.color = matteBlackConfig.Color;
      material1.SetFloat("_Smoothness", matteBlackConfig.Smoothness);
      context.Logger.LogDebug((object) $"[{this.ModifierId}] Set {matteBlackConfig.MaterialName} color and smoothness");
      ++num1;
    }
    else
      context.Logger.LogWarning((object) $"[{this.ModifierId}] Material '{matteBlackConfig.MaterialName}' not found");
    int num2 = 0;
    foreach (string deactivatePath in (IEnumerable<string>) NavLightConfigs.DeactivatePaths)
    {
      GameObject gameObject = PathLookup.Find(deactivatePath, false);
      if ((UnityEngine.Object) gameObject != (UnityEngine.Object) null)
      {
        gameObject.SetActive(false);
        context.Logger.LogDebug((object) $"[{this.ModifierId}] Deactivated {deactivatePath}");
        ++num2;
      }
    }
    context.Logger.LogInfo((object) $"[{this.ModifierId}] Modified {num1} materials, deactivated {num2} objects");
  }
}

// Decompiled with JetBrains decompiler
// Type: qol.WeaponLoading.Modifiers.Materials.GlassMaterialModifier
// Assembly: qol, Version=1.1.6.3, Culture=neutral, PublicKeyToken=null
// MVID: 3B6539EA-E38C-45EE-8FFF-FC58CC42BADC
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\BepInEx\plugins\qol_1.1.6.3b1.dll

using qol.WeaponLoading.Configs;
using qol.WeaponLoading.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#nullable disable
namespace qol.WeaponLoading.Modifiers.Materials;

public class GlassMaterialModifier : IEntityModifier
{
  public string ModifierId => "GlassMaterials";

  public int Priority => 10;

  public IReadOnlyList<string> Dependencies => (IReadOnlyList<string>) Array.Empty<string>();

  public bool CanApply(ModificationContext context) => true;

  public void Apply(ModificationContext context)
  {
    Material[] objectsOfTypeAll = Resources.FindObjectsOfTypeAll<Material>();
    int num = 0;
    foreach ((string MaterialName, Color Color) in (IEnumerable<(string MaterialName, Color Color)>) MaterialConfigs.GlassColors)
    {
      Material material = ((IEnumerable<Material>) objectsOfTypeAll).FirstOrDefault<Material>((Func<Material, bool>) (mat => mat.name.Equals(MaterialName, StringComparison.InvariantCultureIgnoreCase)));
      if ((UnityEngine.Object) material != (UnityEngine.Object) null)
      {
        material.color = Color;
        context.Logger.LogDebug((object) $"[{this.ModifierId}] Set {MaterialName} color to ({Color.r:F2}, {Color.g:F2}, {Color.b:F2}, {Color.a:F2})");
        ++num;
      }
      else
        context.Logger.LogWarning((object) $"[{this.ModifierId}] Material '{MaterialName}' not found");
    }
    context.Logger.LogInfo((object) $"[{this.ModifierId}] Modified {num}/{MaterialConfigs.GlassColors.Count} glass materials");
  }
}

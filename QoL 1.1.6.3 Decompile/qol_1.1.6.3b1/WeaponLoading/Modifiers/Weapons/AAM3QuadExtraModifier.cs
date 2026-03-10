// Decompiled with JetBrains decompiler
// Type: qol.WeaponLoading.Modifiers.Weapons.AAM3QuadExtraModifier
// Assembly: qol, Version=1.1.6.3, Culture=neutral, PublicKeyToken=null
// MVID: 3B6539EA-E38C-45EE-8FFF-FC58CC42BADC
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\BepInEx\plugins\qol_1.1.6.3b1.dll

using qol.Utilities;
using qol.WeaponLoading.Core;
using System;
using System.Collections.Generic;
using UnityEngine;

#nullable disable
namespace qol.WeaponLoading.Modifiers.Weapons;

public class AAM3QuadExtraModifier : IEntityModifier
{
  public string ModifierId => "AAM3QuadExtra";

  public int Priority => 66;

  public IReadOnlyList<string> Dependencies => (IReadOnlyList<string>) Array.Empty<string>();

  public bool CanApply(ModificationContext context) => true;

  public void Apply(ModificationContext context)
  {
    GameObject original = PathLookup.Find("AAM3_quad_internal/aam3", false);
    GameObject gameObject1 = PathLookup.Find("AAM3_quad_internal", false);
    if ((UnityEngine.Object) original == (UnityEngine.Object) null || (UnityEngine.Object) gameObject1 == (UnityEngine.Object) null)
    {
      context.Logger.LogWarning((object) $"[{this.ModifierId}] AAM3_quad_internal or aam3 child not found");
    }
    else
    {
      GameObject gameObject2 = UnityEngine.Object.Instantiate<GameObject>(original);
      gameObject2.transform.SetParent(gameObject1.transform);
      gameObject2.name = "aam3 (4)";
      context.Logger.LogInfo((object) $"[{this.ModifierId}] Added extra AAM3 to quad internal mount");
    }
  }
}

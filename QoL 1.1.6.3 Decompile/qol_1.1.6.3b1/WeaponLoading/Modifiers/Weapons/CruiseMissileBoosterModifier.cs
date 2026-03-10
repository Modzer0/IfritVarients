// Decompiled with JetBrains decompiler
// Type: qol.WeaponLoading.Modifiers.Weapons.CruiseMissileBoosterModifier
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

public class CruiseMissileBoosterModifier : IEntityModifier
{
  public string ModifierId => "CruiseMissileBooster";

  public int Priority => 65;

  public IReadOnlyList<string> Dependencies => (IReadOnlyList<string>) Array.Empty<string>();

  public bool CanApply(ModificationContext context) => true;

  public void Apply(ModificationContext context)
  {
    GameObject original1 = PathLookup.Find("AShM2/VLS_Booster", false);
    GameObject gameObject1 = PathLookup.Find("CruiseMissile1", false);
    if ((UnityEngine.Object) original1 == (UnityEngine.Object) null || (UnityEngine.Object) gameObject1 == (UnityEngine.Object) null)
    {
      context.Logger.LogWarning((object) $"[{this.ModifierId}] AShM2 booster or CruiseMissile1 not found");
    }
    else
    {
      GameObject gameObject2 = UnityEngine.Object.Instantiate<GameObject>(original1);
      gameObject2.name = "VLS_Booster";
      gameObject2.transform.SetParent(gameObject1.transform);
      GameObject gameObject3 = PathLookup.Find("CruiseMissile1/VLS_Booster/smokeParticles", false);
      if ((UnityEngine.Object) gameObject3 != (UnityEngine.Object) null)
        gameObject3.SetActive(false);
      GameObject original2 = PathLookup.Find("AAM2_single/pylon", false);
      GameObject gameObject4 = PathLookup.Find("CruiseMissile1_internal", false);
      if ((UnityEngine.Object) original2 != (UnityEngine.Object) null && (UnityEngine.Object) gameObject4 != (UnityEngine.Object) null)
      {
        GameObject gameObject5 = UnityEngine.Object.Instantiate<GameObject>(original2);
        gameObject5.name = "pylon";
        gameObject5.transform.SetParent(gameObject4.transform);
        GameObject gameObject6 = PathLookup.Find("CruiseMissile1_internal/pylon/aam2", false);
        if ((UnityEngine.Object) gameObject6 != (UnityEngine.Object) null)
          UnityEngine.Object.Destroy((UnityEngine.Object) gameObject6);
      }
      context.Logger.LogInfo((object) $"[{this.ModifierId}] Added VLS_Booster to CruiseMissile1 and created pylon mount");
    }
  }
}

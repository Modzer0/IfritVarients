// Decompiled with JetBrains decompiler
// Type: qol.WeaponLoading.Modifiers.Balance.AssaultCarrierLaserTurretModifier
// Assembly: qol, Version=1.1.6.3, Culture=neutral, PublicKeyToken=null
// MVID: 3B6539EA-E38C-45EE-8FFF-FC58CC42BADC
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\BepInEx\plugins\qol_1.1.6.3b1.dll

using qol.Utilities;
using qol.WeaponLoading.Core;
using System;
using System.Collections.Generic;
using UnityEngine;

#nullable disable
namespace qol.WeaponLoading.Modifiers.Balance;

public class AssaultCarrierLaserTurretModifier : IEntityModifier
{
  public string ModifierId => "AssaultCarrierLaserTurret";

  public int Priority => 64 /*0x40*/;

  public IReadOnlyList<string> Dependencies => (IReadOnlyList<string>) Array.Empty<string>();

  public bool CanApply(ModificationContext context) => true;

  public void Apply(ModificationContext context)
  {
    GameObject original = PathLookup.Find("Truck2-LADS/laser_turret", false);
    if ((UnityEngine.Object) original == (UnityEngine.Object) null)
    {
      context.Logger.LogWarning((object) $"[{this.ModifierId}] Truck2-LADS laser turret not found");
    }
    else
    {
      GameObject gameObject1 = PathLookup.Find("AssaultCarrier1/hull_L/hull_FR/tower_F1/tower_F2", false);
      if ((UnityEngine.Object) gameObject1 == (UnityEngine.Object) null)
      {
        context.Logger.LogWarning((object) $"[{this.ModifierId}] AssaultCarrier1 tower_F2 not found");
      }
      else
      {
        GameObject gameObject2 = UnityEngine.Object.Instantiate<GameObject>(original);
        gameObject2.hideFlags = HideFlags.HideAndDontSave;
        gameObject2.transform.SetParent(gameObject1.transform);
        TargetDetector component = gameObject2.GetComponent<TargetDetector>();
        if ((UnityEngine.Object) component != (UnityEngine.Object) null)
          component.enabled = true;
        GameObject gameObject3 = PathLookup.Find("AssaultCarrier1/hull_L/hull_FR/tower_F1/tower_F2/laser_turret(Clone)/laser_barrel/aimTransform", false);
        if ((UnityEngine.Object) gameObject3 != (UnityEngine.Object) null)
        {
          ParticleSystem componentInChildren = gameObject3.GetComponentInChildren<ParticleSystem>();
          if ((UnityEngine.Object) componentInChildren != (UnityEngine.Object) null)
            componentInChildren.startSize = 30f;
        }
        context.Logger.LogInfo((object) $"[{this.ModifierId}] Cloned laser turret to AssaultCarrier1 tower_F2");
      }
    }
  }
}

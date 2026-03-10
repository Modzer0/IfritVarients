// Decompiled with JetBrains decompiler
// Type: qol.WeaponLoading.Modifiers.Balance.ShardFireControlModifier
// Assembly: qol, Version=1.1.6.3, Culture=neutral, PublicKeyToken=null
// MVID: 3B6539EA-E38C-45EE-8FFF-FC58CC42BADC
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\BepInEx\plugins\qol_1.1.6.3b1.dll

using HarmonyLib;
using qol.Utilities;
using qol.WeaponLoading.Core;
using System;
using System.Collections.Generic;
using UnityEngine;

#nullable disable
namespace qol.WeaponLoading.Modifiers.Balance;

public class ShardFireControlModifier : IEntityModifier
{
  public string ModifierId => "ShardFireControl";

  public int Priority => 58;

  public IReadOnlyList<string> Dependencies => (IReadOnlyList<string>) Array.Empty<string>();

  public bool CanApply(ModificationContext context) => true;

  public void Apply(ModificationContext context)
  {
    GameObject gameObject1 = PathLookup.Find("Corvette1", false);
    if ((UnityEngine.Object) gameObject1 == (UnityEngine.Object) null)
    {
      context.Logger.LogWarning((object) $"[{this.ModifierId}] Corvette not found: Corvette1");
    }
    else
    {
      GameObject gameObject2 = PathLookup.Find("Corvette1/bow1/missileStation2", false);
      if ((UnityEngine.Object) gameObject2 == (UnityEngine.Object) null)
      {
        context.Logger.LogWarning((object) $"[{this.ModifierId}] Turret not found: Corvette1/bow1/missileStation2");
      }
      else
      {
        FireControl fireControl = gameObject1.AddComponent<FireControl>();
        Turret component = gameObject2.GetComponent<Turret>();
        if ((UnityEngine.Object) component == (UnityEngine.Object) null)
        {
          context.Logger.LogWarning((object) $"[{this.ModifierId}] No Turret component on Corvette1/bow1/missileStation2");
        }
        else
        {
          Traverse.Create((object) component).Field("fireControl").SetValue((object) fireControl);
          context.Logger.LogInfo((object) $"[{this.ModifierId}] Added FireControl to Corvette1 and configured missile turret");
        }
      }
    }
  }
}

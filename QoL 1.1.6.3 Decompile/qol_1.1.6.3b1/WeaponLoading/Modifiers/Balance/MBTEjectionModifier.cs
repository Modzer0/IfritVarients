// Decompiled with JetBrains decompiler
// Type: qol.WeaponLoading.Modifiers.Balance.MBTEjectionModifier
// Assembly: qol, Version=1.1.6.3, Culture=neutral, PublicKeyToken=null
// MVID: 3B6539EA-E38C-45EE-8FFF-FC58CC42BADC
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\BepInEx\plugins\qol_1.1.6.3b1.dll

using HarmonyLib;
using qol.Utilities;
using qol.WeaponLoading.Configs;
using qol.WeaponLoading.Core;
using System;
using System.Collections.Generic;
using UnityEngine;

#nullable disable
namespace qol.WeaponLoading.Modifiers.Balance;

public class MBTEjectionModifier : IEntityModifier
{
  public string ModifierId => "MBTEjection";

  public int Priority => 59;

  public IReadOnlyList<string> Dependencies => (IReadOnlyList<string>) Array.Empty<string>();

  public bool CanApply(ModificationContext context) => true;

  public void Apply(ModificationContext context)
  {
    int num = 0;
    foreach ((string str1, string str2) in (IEnumerable<(string GunPath, string EjectionTransformPath)>) MBTEjectionConfigs.EjectionConfigs)
    {
      GameObject gameObject1 = PathLookup.Find(str1, false);
      GameObject gameObject2 = PathLookup.Find(str2, false);
      if ((UnityEngine.Object) gameObject1 == (UnityEngine.Object) null)
        context.Logger.LogWarning((object) $"[{this.ModifierId}] Gun not found: {str1}");
      else if ((UnityEngine.Object) gameObject2 == (UnityEngine.Object) null)
      {
        context.Logger.LogWarning((object) $"[{this.ModifierId}] Ejection transform not found: {str2}");
      }
      else
      {
        Gun component = gameObject1.GetComponent<Gun>();
        if ((UnityEngine.Object) component == (UnityEngine.Object) null)
        {
          context.Logger.LogWarning((object) $"[{this.ModifierId}] No Gun component on {str1}");
        }
        else
        {
          Traverse.Create((object) component).Field("ejectionTransform").SetValue((object) gameObject2.transform);
          ++num;
        }
      }
    }
    context.Logger.LogInfo((object) $"[{this.ModifierId}] Configured ejection transforms for {num} MBT guns");
  }
}

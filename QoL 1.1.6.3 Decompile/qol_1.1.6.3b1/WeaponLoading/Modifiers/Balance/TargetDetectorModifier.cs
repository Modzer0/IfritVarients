// Decompiled with JetBrains decompiler
// Type: qol.WeaponLoading.Modifiers.Balance.TargetDetectorModifier
// Assembly: qol, Version=1.1.6.3, Culture=neutral, PublicKeyToken=null
// MVID: 3B6539EA-E38C-45EE-8FFF-FC58CC42BADC
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\BepInEx\plugins\qol_1.1.6.3b1.dll

using HarmonyLib;
using qol.WeaponLoading.Core;
using System;
using System.Collections.Generic;
using UnityEngine;

#nullable disable
namespace qol.WeaponLoading.Modifiers.Balance;

public class TargetDetectorModifier : IEntityModifier
{
  public string ModifierId => "TargetDetector";

  public int Priority => 32 /*0x20*/;

  public IReadOnlyList<string> Dependencies => (IReadOnlyList<string>) Array.Empty<string>();

  public bool CanApply(ModificationContext context) => true;

  public void Apply(ModificationContext context)
  {
    int num1 = 0;
    int num2 = 0;
    foreach (TargetDetector targetDetector in Resources.FindObjectsOfTypeAll<TargetDetector>())
    {
      ++num2;
      Traverse traverse = Traverse.Create((object) targetDetector);
      if ((double) traverse.Field("checkInterval").GetValue<float>() < 4.0)
      {
        traverse.Field("checkInterval").SetValue((object) 4f);
        ++num1;
      }
    }
    context.Logger.LogInfo((object) $"[{this.ModifierId}] Raised {num1}/{num2} target detector intervals to {(ValueType) 4f}s minimum");
  }
}

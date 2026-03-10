// Decompiled with JetBrains decompiler
// Type: qol.WeaponLoading.Modifiers.Balance.CorvetteBalanceModifier
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

public class CorvetteBalanceModifier : IEntityModifier
{
  private const string CorvettePath = "Corvette1";

  public string ModifierId => "CorvetteBalance";

  public int Priority => 42;

  public IReadOnlyList<string> Dependencies => (IReadOnlyList<string>) Array.Empty<string>();

  public bool CanApply(ModificationContext context) => true;

  public void Apply(ModificationContext context)
  {
    GameObject gameObject = PathLookup.Find("Corvette1", false);
    if ((UnityEngine.Object) gameObject == (UnityEngine.Object) null)
    {
      context.Logger.LogWarning((object) $"[{this.ModifierId}] Corvette1 not found");
    }
    else
    {
      int num = 0;
      foreach (object componentsInChild in gameObject.GetComponentsInChildren<UnitPart>())
      {
        Traverse.Create(componentsInChild).Field("criticalPart").SetValue((object) false);
        ++num;
      }
      context.Logger.LogInfo((object) $"[{this.ModifierId}] Set {num} Corvette parts to non-critical");
    }
  }
}

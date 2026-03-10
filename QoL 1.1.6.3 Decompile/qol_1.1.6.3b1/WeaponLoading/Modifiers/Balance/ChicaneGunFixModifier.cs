// Decompiled with JetBrains decompiler
// Type: qol.WeaponLoading.Modifiers.Balance.ChicaneGunFixModifier
// Assembly: qol, Version=1.1.6.3, Culture=neutral, PublicKeyToken=null
// MVID: 3B6539EA-E38C-45EE-8FFF-FC58CC42BADC
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\BepInEx\plugins\qol_1.1.6.3b1.dll

using qol.Utilities;
using qol.WeaponLoading.Core;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

#nullable disable
namespace qol.WeaponLoading.Modifiers.Balance;

public class ChicaneGunFixModifier : IEntityModifier
{
  private static readonly FieldInfo BuiltInWeaponsField = typeof (Hardpoint).GetField("BuiltInWeapons", BindingFlags.Instance | BindingFlags.Public);
  private static readonly FieldInfo BuiltInTurretsField = typeof (Hardpoint).GetField("BuiltInTurrets", BindingFlags.Instance | BindingFlags.Public);

  public string ModifierId => "ChicaneGunFix";

  public int Priority => 68;

  public IReadOnlyList<string> Dependencies => (IReadOnlyList<string>) Array.Empty<string>();

  public bool CanApply(ModificationContext context)
  {
    return ChicaneGunFixModifier.BuiltInWeaponsField != (FieldInfo) null && ChicaneGunFixModifier.BuiltInTurretsField != (FieldInfo) null;
  }

  public void Apply(ModificationContext context)
  {
    GameObject gameObject = PathLookup.Find("AttackHelo1/cockpit_R", false);
    if ((UnityEngine.Object) gameObject == (UnityEngine.Object) null)
    {
      context.Logger.LogWarning((object) $"[{this.ModifierId}] AttackHelo1/cockpit_R not found");
    }
    else
    {
      WeaponManager component = gameObject.GetComponent<WeaponManager>();
      if ((UnityEngine.Object) component == (UnityEngine.Object) null || component.hardpointSets.Length == 0 || component.hardpointSets[0].hardpoints.Count == 0)
      {
        context.Logger.LogWarning((object) $"[{this.ModifierId}] AttackHelo1 WeaponManager or hardpoints not found");
      }
      else
      {
        Hardpoint hardpoint = component.hardpointSets[0].hardpoints[0];
        ChicaneGunFixModifier.BuiltInWeaponsField.SetValue((object) hardpoint, (object) new Weapon[0]);
        ChicaneGunFixModifier.BuiltInTurretsField.SetValue((object) hardpoint, (object) new Turret[0]);
        context.Logger.LogInfo((object) $"[{this.ModifierId}] Cleared chicane built-in weapons/turrets");
      }
    }
  }
}

// Decompiled with JetBrains decompiler
// Type: qol.WeaponLoading.Modifiers.Balance.ShipArmorModifier
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

public class ShipArmorModifier : IEntityModifier
{
  public string ModifierId => "ShipArmor";

  public int Priority => 30;

  public IReadOnlyList<string> Dependencies => (IReadOnlyList<string>) Array.Empty<string>();

  public bool CanApply(ModificationContext context) => true;

  public void Apply(ModificationContext context)
  {
    ArmorProperties armorProperties1 = this.CreateArmorProperties(ShipArmorConfigs.RadarArmor);
    ArmorProperties armorProperties2 = this.CreateArmorProperties(ShipArmorConfigs.TurretArmor);
    ArmorProperties armorProperties3 = this.CreateArmorProperties(ShipArmorConfigs.StoresArmor);
    ArmorProperties armorProperties4 = this.CreateArmorProperties(ShipArmorConfigs.HullArmor);
    ArmorProperties armorProperties5 = this.CreateArmorProperties(ShipArmorConfigs.WeakHullArmor);
    GameObject gameObject1 = PathLookup.Find("fire_large");
    GameObject gameObject2 = PathLookup.Find("fire_med");
    List<UnitPart> unitPartList = new List<UnitPart>();
    foreach (string shipPrefab in (IEnumerable<string>) ShipArmorConfigs.ShipPrefabs)
    {
      GameObject gameObject3 = PathLookup.Find(shipPrefab);
      if ((UnityEngine.Object) gameObject3 != (UnityEngine.Object) null)
        unitPartList.AddRange((IEnumerable<UnitPart>) gameObject3.GetComponentsInChildren<UnitPart>());
      else
        context.Logger.LogWarning((object) $"[{this.ModifierId}] Ship '{shipPrefab}' not found");
    }
    int num1 = 0;
    foreach (UnitPart unitPart in unitPartList)
    {
      Traverse traverse = Traverse.Create((object) unitPart);
      bool flag = unitPart.name.Contains("CIWS") || unitPart.name.Contains("turret") || (UnityEngine.Object) unitPart.gameObject.GetComponent<Turret>() != (UnityEngine.Object) null;
      List<DamageEffect> damageEffectList = traverse.Field("damageEffects").GetValue<List<DamageEffect>>();
      traverse.Field("armorProperties").SetValue((object) armorProperties4);
      if (unitPart.name.Contains("Corvette1"))
        traverse.Field("armorProperties").SetValue((object) armorProperties5);
      if (flag)
        traverse.Field("armorProperties").SetValue((object) armorProperties2);
      if (damageEffectList != null)
      {
        foreach (DamageEffect damageEffect in damageEffectList)
        {
          if ((UnityEngine.Object) damageEffect.prefab != (UnityEngine.Object) null)
          {
            if (damageEffect.prefab.name == "fire_med" && (UnityEngine.Object) gameObject1 != (UnityEngine.Object) null)
              damageEffect.prefab = gameObject1;
            if (flag && damageEffect.prefab.name == "smoulder_small" && (UnityEngine.Object) gameObject2 != (UnityEngine.Object) null)
              damageEffect.prefab = gameObject2;
          }
        }
      }
      ++num1;
    }
    int num2 = 0;
    foreach (string radarPath in (IEnumerable<string>) ShipArmorConfigs.RadarPaths)
    {
      GameObject gameObject4 = PathLookup.Find(radarPath);
      if ((UnityEngine.Object) gameObject4 != (UnityEngine.Object) null)
      {
        UnitPart component = gameObject4.GetComponent<UnitPart>();
        if ((UnityEngine.Object) component != (UnityEngine.Object) null)
        {
          Traverse.Create((object) component).Field("armorProperties").SetValue((object) armorProperties1);
          ++num2;
        }
      }
    }
    int num3 = 0;
    foreach (string weaponStoresPath in (IEnumerable<string>) ShipArmorConfigs.WeaponStoresPaths)
    {
      GameObject gameObject5 = PathLookup.Find(weaponStoresPath);
      if ((UnityEngine.Object) gameObject5 != (UnityEngine.Object) null)
      {
        UnitPart component = gameObject5.GetComponent<UnitPart>();
        if ((UnityEngine.Object) component != (UnityEngine.Object) null)
        {
          Traverse.Create((object) component).Field("armorProperties").SetValue((object) armorProperties3);
          ++num3;
        }
      }
    }
    context.Logger.LogInfo((object) $"[{this.ModifierId}] Modified {num1} ship parts, {num2} radars, {num3} weapon stores");
  }

  private ArmorProperties CreateArmorProperties(ShipArmorConfigs.ArmorDef def)
  {
    return new ArmorProperties()
    {
      pierceArmor = def.PierceArmor,
      blastArmor = def.BlastArmor,
      fireArmor = def.FireArmor,
      pierceTolerance = def.PierceTolerance,
      blastTolerance = def.BlastTolerance,
      fireTolerance = def.FireTolerance
    };
  }
}

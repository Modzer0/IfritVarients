// Decompiled with JetBrains decompiler
// Type: qol.WeaponLoading.Modifiers.Balance.ECMPodInfoModifier
// Assembly: qol, Version=1.1.6.3, Culture=neutral, PublicKeyToken=null
// MVID: 3B6539EA-E38C-45EE-8FFF-FC58CC42BADC
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\BepInEx\plugins\qol_1.1.6.3b1.dll

using qol.WeaponLoading.Core;
using System;
using System.Collections.Generic;
using UnityEngine;

#nullable disable
namespace qol.WeaponLoading.Modifiers.Balance;

public class ECMPodInfoModifier : IEntityModifier
{
  private const string ECMPodMountName = "ECMPod1";
  private const string WeaponName = "DJP-10 ECM Pod";
  private const string ShortName = "ECM";
  private const string Description = "This small, modular electronic countermeasures pod allows aircraft without internal electronic warfare suites to jam and defeat incoming radar missiles.";

  public string ModifierId => "ECMPodInfo";

  public int Priority => 43;

  public IReadOnlyList<string> Dependencies => (IReadOnlyList<string>) Array.Empty<string>();

  public bool CanApply(ModificationContext context) => true;

  public void Apply(ModificationContext context)
  {
    WeaponMount mount = QOLPlugin.GetMount("ECMPod1");
    if ((UnityEngine.Object) mount == (UnityEngine.Object) null)
    {
      context.Logger.LogWarning((object) $"[{this.ModifierId}] Mount 'ECMPod1' not found");
    }
    else
    {
      mount.info = ScriptableObject.CreateInstance<WeaponInfo>();
      mount.info.weaponName = "DJP-10 ECM Pod";
      mount.info.shortName = "ECM";
      mount.info.description = "This small, modular electronic countermeasures pod allows aircraft without internal electronic warfare suites to jam and defeat incoming radar missiles.";
      mount.info.energy = true;
      context.Logger.LogInfo((object) $"[{this.ModifierId}] Set ECMPod1 weapon info");
    }
  }
}

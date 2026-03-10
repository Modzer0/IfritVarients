// Decompiled with JetBrains decompiler
// Type: qol.WeaponLoading.Modifiers.Balance.PylonRenameModifier
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

public class PylonRenameModifier : IEntityModifier
{
  public string ModifierId => "PylonRename";

  public int Priority => 70;

  public IReadOnlyList<string> Dependencies => (IReadOnlyList<string>) Array.Empty<string>();

  public bool CanApply(ModificationContext context) => true;

  public void Apply(ModificationContext context)
  {
    GameObject gameObject1 = PathLookup.Find("AAM3_double/pylon", false);
    if ((UnityEngine.Object) gameObject1 != (UnityEngine.Object) null)
      gameObject1.name = "pylon_L";
    GameObject gameObject2 = PathLookup.Find("AAM3_double/pylon", false);
    if ((UnityEngine.Object) gameObject2 != (UnityEngine.Object) null)
      gameObject2.name = "pylon_R";
    GameObject gameObject3 = PathLookup.Find("AAM1_double/pylon", false);
    if ((UnityEngine.Object) gameObject3 != (UnityEngine.Object) null)
      gameObject3.name = "pylon1";
    context.Logger.LogInfo((object) $"[{this.ModifierId}] Applied pylon renames");
  }
}

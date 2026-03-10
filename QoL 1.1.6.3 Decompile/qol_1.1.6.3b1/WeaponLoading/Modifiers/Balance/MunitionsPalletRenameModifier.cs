// Decompiled with JetBrains decompiler
// Type: qol.WeaponLoading.Modifiers.Balance.MunitionsPalletRenameModifier
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

public class MunitionsPalletRenameModifier : IEntityModifier
{
  public string ModifierId => "MunitionsPalletRename";

  public int Priority => 72;

  public IReadOnlyList<string> Dependencies => (IReadOnlyList<string>) Array.Empty<string>();

  public bool CanApply(ModificationContext context) => true;

  public void Apply(ModificationContext context)
  {
    GameObject gameObject = PathLookup.Find("MunitionsPallet1", false);
    if ((UnityEngine.Object) gameObject != (UnityEngine.Object) null)
    {
      gameObject.name = "MunitionPallet1";
      context.Logger.LogInfo((object) $"[{this.ModifierId}] Renamed MunitionsPallet1 to MunitionPallet1");
    }
    else
      context.Logger.LogWarning((object) $"[{this.ModifierId}] MunitionsPallet1 not found");
  }
}

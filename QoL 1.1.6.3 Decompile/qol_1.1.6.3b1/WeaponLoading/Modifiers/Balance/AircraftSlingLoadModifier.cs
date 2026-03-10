// Decompiled with JetBrains decompiler
// Type: qol.WeaponLoading.Modifiers.Balance.AircraftSlingLoadModifier
// Assembly: qol, Version=1.1.6.3, Culture=neutral, PublicKeyToken=null
// MVID: 3B6539EA-E38C-45EE-8FFF-FC58CC42BADC
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\BepInEx\plugins\qol_1.1.6.3b1.dll

using qol.WeaponLoading.Core;
using System;
using System.Collections.Generic;
using UnityEngine;

#nullable disable
namespace qol.WeaponLoading.Modifiers.Balance;

public class AircraftSlingLoadModifier : IEntityModifier
{
  public string ModifierId => "AircraftSlingLoad";

  public int Priority => 52;

  public IReadOnlyList<string> Dependencies => (IReadOnlyList<string>) Array.Empty<string>();

  public bool CanApply(ModificationContext context) => true;

  public void Apply(ModificationContext context)
  {
    int num = 0;
    foreach (AircraftDefinition aircraftDefinition in Resources.FindObjectsOfTypeAll<AircraftDefinition>())
    {
      if (!aircraftDefinition.CanSlingLoad)
      {
        aircraftDefinition.CanSlingLoad = true;
        ++num;
        context.Logger.LogDebug((object) $"[{this.ModifierId}] Enabled sling-load for {aircraftDefinition.name}");
      }
    }
    context.Logger.LogInfo((object) $"[{this.ModifierId}] Enabled sling-load for {num} aircraft definitions");
  }
}

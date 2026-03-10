// Decompiled with JetBrains decompiler
// Type: qol.WeaponLoading.Modifiers.Balance.UH1PilotSwapModifier
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

public class UH1PilotSwapModifier : IEntityModifier
{
  public string ModifierId => "UH1PilotSwap";

  public int Priority => 71;

  public IReadOnlyList<string> Dependencies => (IReadOnlyList<string>) Array.Empty<string>();

  public bool CanApply(ModificationContext context) => true;

  public void Apply(ModificationContext context)
  {
    GameObject gameObject1 = PathLookup.Find("UtilityHelo1/fuelTank_F/fuselage_F/cockpit/pilot", false);
    GameObject gameObject2 = PathLookup.Find("UtilityHelo1/fuelTank_F/fuselage_F/cockpit/copilot", false);
    if ((UnityEngine.Object) gameObject1 == (UnityEngine.Object) null || (UnityEngine.Object) gameObject2 == (UnityEngine.Object) null)
    {
      context.Logger.LogWarning((object) $"[{this.ModifierId}] UH1 pilot or copilot not found");
    }
    else
    {
      Vector3 position = gameObject1.transform.position;
      gameObject1.transform.position = gameObject2.transform.position;
      gameObject2.transform.position = position;
      Pilot component1 = gameObject1.GetComponent<Pilot>();
      Pilot component2 = gameObject2.GetComponent<Pilot>();
      if ((UnityEngine.Object) component1 != (UnityEngine.Object) null)
        component1.exitDirection = Pilot.ExitDirection.Right;
      if ((UnityEngine.Object) component2 != (UnityEngine.Object) null)
        component2.exitDirection = Pilot.ExitDirection.Left;
      context.Logger.LogInfo((object) $"[{this.ModifierId}] Swapped UH1 pilot/copilot positions and exit directions");
    }
  }
}

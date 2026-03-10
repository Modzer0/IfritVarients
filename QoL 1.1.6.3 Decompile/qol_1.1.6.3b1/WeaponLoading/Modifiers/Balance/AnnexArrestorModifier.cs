// Decompiled with JetBrains decompiler
// Type: qol.WeaponLoading.Modifiers.Balance.AnnexArrestorModifier
// Assembly: qol, Version=1.1.6.3, Culture=neutral, PublicKeyToken=null
// MVID: 3B6539EA-E38C-45EE-8FFF-FC58CC42BADC
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\BepInEx\plugins\qol_1.1.6.3b1.dll

using qol.Utilities;
using qol.WeaponLoading.Configs;
using qol.WeaponLoading.Core;
using System;
using System.Collections.Generic;
using UnityEngine;

#nullable disable
namespace qol.WeaponLoading.Modifiers.Balance;

public class AnnexArrestorModifier : IEntityModifier
{
  public string ModifierId => "AnnexArrestor";

  public int Priority => 57;

  public IReadOnlyList<string> Dependencies => (IReadOnlyList<string>) Array.Empty<string>();

  public bool CanApply(ModificationContext context) => true;

  public void Apply(ModificationContext context)
  {
    GameObject original = PathLookup.Find("FleetCarrier1/hull_R/hull_R2/arrestorCable1", false);
    if ((UnityEngine.Object) original == (UnityEngine.Object) null)
    {
      context.Logger.LogWarning((object) $"[{this.ModifierId}] Source cable not found: FleetCarrier1/hull_R/hull_R2/arrestorCable1");
    }
    else
    {
      GameObject gameObject1 = PathLookup.Find("AssaultCarrier1/hull_RL/deck_R", false);
      if ((UnityEngine.Object) gameObject1 == (UnityEngine.Object) null)
      {
        context.Logger.LogWarning((object) $"[{this.ModifierId}] Target parent not found: AssaultCarrier1/hull_RL/deck_R");
      }
      else
      {
        GameObject gameObject2 = UnityEngine.Object.Instantiate<GameObject>(original);
        gameObject2.hideFlags = HideFlags.HideAndDontSave;
        gameObject2.transform.SetParent(gameObject1.transform);
        gameObject2.transform.localPosition = AnnexArrestorConfigs.CableLocalPosition;
        gameObject2.transform.localRotation = Quaternion.identity;
        GameObject gameObject3 = PathLookup.Find("AssaultCarrier1", false);
        if ((UnityEngine.Object) gameObject3 == (UnityEngine.Object) null)
        {
          context.Logger.LogWarning((object) $"[{this.ModifierId}] Airbase not found: AssaultCarrier1");
        }
        else
        {
          Airbase component = gameObject3.GetComponent<Airbase>();
          if ((UnityEngine.Object) component == (UnityEngine.Object) null || component.runways == null)
          {
            context.Logger.LogWarning((object) $"[{this.ModifierId}] No Airbase component or runways on AssaultCarrier1");
          }
          else
          {
            if (component.runways.Length <= 2)
              return;
            component.runways[2].Arrestor = true;
            component.runways[2].Landing = true;
            context.Logger.LogInfo((object) $"[{this.ModifierId}] Added arresting cable to AssaultCarrier1 and configured runway {2}");
          }
        }
      }
    }
  }
}

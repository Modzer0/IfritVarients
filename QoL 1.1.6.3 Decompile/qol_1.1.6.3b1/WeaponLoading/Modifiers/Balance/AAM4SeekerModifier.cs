// Decompiled with JetBrains decompiler
// Type: qol.WeaponLoading.Modifiers.Balance.AAM4SeekerModifier
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

public class AAM4SeekerModifier : IEntityModifier
{
  public string ModifierId => "AAM4Seeker";

  public int Priority => 67;

  public IReadOnlyList<string> Dependencies => (IReadOnlyList<string>) Array.Empty<string>();

  public bool CanApply(ModificationContext context) => true;

  public void Apply(ModificationContext context)
  {
    GameObject gameObject = PathLookup.Find("AAM4", false);
    if ((UnityEngine.Object) gameObject == (UnityEngine.Object) null)
    {
      context.Logger.LogWarning((object) $"[{this.ModifierId}] AAM4 not found");
    }
    else
    {
      ARHSeeker component = gameObject.GetComponent<ARHSeeker>();
      if ((UnityEngine.Object) component == (UnityEngine.Object) null)
      {
        context.Logger.LogWarning((object) $"[{this.ModifierId}] AAM4 ARHSeeker component not found");
      }
      else
      {
        RadarParams radarParams = new RadarParams()
        {
          maxRange = 14000f,
          maxSignal = 4f,
          minSignal = 0.25f,
          clutterFactor = 0.1f,
          dopplerFactor = 0.08f
        };
        ReflectionHelpers.SetFieldValue((object) component, "radarParameters", (object) radarParams);
        context.Logger.LogInfo((object) $"[{this.ModifierId}] Set AAM4 ARH seeker radar parameters");
      }
    }
  }
}

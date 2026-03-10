// Decompiled with JetBrains decompiler
// Type: qol.WeaponLoading.Modifiers.Balance.MedusaAirbrakeModifier
// Assembly: qol, Version=1.1.6.3, Culture=neutral, PublicKeyToken=null
// MVID: 3B6539EA-E38C-45EE-8FFF-FC58CC42BADC
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\BepInEx\plugins\qol_1.1.6.3b1.dll

using qol.Utilities;
using qol.WeaponLoading.Configs;
using qol.WeaponLoading.Core;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

#nullable disable
namespace qol.WeaponLoading.Modifiers.Balance;

public class MedusaAirbrakeModifier : IEntityModifier
{
  private static readonly FieldInfo TransformsField = typeof (Airbrake).GetField("transforms", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
  private static readonly FieldInfo PartField = typeof (Airbrake).GetField("part", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
  private static readonly FieldInfo AircraftField = typeof (Airbrake).GetField("aircraft", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

  public string ModifierId => "MedusaAirbrake";

  public int Priority => 56;

  public IReadOnlyList<string> Dependencies => (IReadOnlyList<string>) Array.Empty<string>();

  public bool CanApply(ModificationContext context)
  {
    return MedusaAirbrakeModifier.TransformsField != (FieldInfo) null && MedusaAirbrakeModifier.PartField != (FieldInfo) null && MedusaAirbrakeModifier.AircraftField != (FieldInfo) null;
  }

  public void Apply(ModificationContext context)
  {
    GameObject gameObject1 = PathLookup.Find("EW1", false);
    if ((UnityEngine.Object) gameObject1 == (UnityEngine.Object) null)
    {
      context.Logger.LogWarning((object) $"[{this.ModifierId}] Aircraft not found: EW1");
    }
    else
    {
      Aircraft component = gameObject1.GetComponent<Aircraft>();
      if ((UnityEngine.Object) component == (UnityEngine.Object) null)
      {
        context.Logger.LogWarning((object) $"[{this.ModifierId}] No Aircraft component on EW1");
      }
      else
      {
        int num = 0;
        foreach ((string str1, string str2) in (IEnumerable<(string RudderPath, string HingePath)>) MedusaAirbrakeConfigs.AirbrakeConfigs)
        {
          GameObject gameObject2 = PathLookup.Find(str1, false);
          GameObject gameObject3 = PathLookup.Find(str2, false);
          if ((UnityEngine.Object) gameObject2 == (UnityEngine.Object) null || (UnityEngine.Object) gameObject3 == (UnityEngine.Object) null)
          {
            context.Logger.LogWarning((object) $"[{this.ModifierId}] Rudder or hinge not found: {str1}");
          }
          else
          {
            Airbrake airbrake = gameObject2.AddComponent<Airbrake>();
            MedusaAirbrakeModifier.TransformsField.SetValue((object) airbrake, (object) new Transform[1]
            {
              gameObject3.transform
            });
            MedusaAirbrakeModifier.PartField.SetValue((object) airbrake, (object) gameObject2.GetComponent<AeroPart>());
            MedusaAirbrakeModifier.AircraftField.SetValue((object) airbrake, (object) component);
            ++num;
          }
        }
        context.Logger.LogInfo((object) $"[{this.ModifierId}] Added {num} airbrake components to EW1");
      }
    }
  }
}

// Decompiled with JetBrains decompiler
// Type: qol.WeaponLoading.Modifiers.Balance.RadarParametersModifier
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

public class RadarParametersModifier : IEntityModifier
{
  public string ModifierId => "RadarParameters";

  public int Priority => 40;

  public IReadOnlyList<string> Dependencies => (IReadOnlyList<string>) Array.Empty<string>();

  public bool CanApply(ModificationContext context) => true;

  public void Apply(ModificationContext context)
  {
    int num1 = 0;
    int num2 = 0;
    foreach ((string str, float? MaxRange, float? MaxSignal, float? MinSignal, float? ClutterFactor, float? DopplerFactor) in (IEnumerable<(string Path, float? MaxRange, float? MaxSignal, float? MinSignal, float? ClutterFactor, float? DopplerFactor)>) RadarParametersConfigs.RadarModifications)
    {
      GameObject gameObject = PathLookup.Find(str, false);
      if ((UnityEngine.Object) gameObject == (UnityEngine.Object) null)
      {
        ++num2;
        context.Logger.LogDebug((object) $"[{this.ModifierId}] Path not found (may be conditional): {str}");
      }
      else
      {
        Radar component = gameObject.GetComponent<Radar>();
        if ((UnityEngine.Object) component == (UnityEngine.Object) null)
        {
          context.Logger.LogWarning((object) $"[{this.ModifierId}] No Radar component on {str}");
        }
        else
        {
          RadarParams radarParameters = component.RadarParameters;
          bool flag = false;
          if (MaxRange.HasValue)
          {
            radarParameters.maxRange = MaxRange.Value;
            flag = true;
          }
          if (MaxSignal.HasValue)
          {
            radarParameters.maxSignal = MaxSignal.Value;
            flag = true;
          }
          if (MinSignal.HasValue)
          {
            radarParameters.minSignal = MinSignal.Value;
            flag = true;
          }
          if (ClutterFactor.HasValue)
          {
            radarParameters.clutterFactor = ClutterFactor.Value;
            flag = true;
          }
          if (DopplerFactor.HasValue)
          {
            radarParameters.dopplerFactor = DopplerFactor.Value;
            flag = true;
          }
          if (flag)
          {
            component.RadarParameters = radarParameters;
            ++num1;
            context.Logger.LogDebug((object) $"[{this.ModifierId}] Modified radar on {str}");
          }
        }
      }
    }
    context.Logger.LogInfo((object) $"[{this.ModifierId}] Modified {num1} radars ({num2} paths not found)");
  }
}

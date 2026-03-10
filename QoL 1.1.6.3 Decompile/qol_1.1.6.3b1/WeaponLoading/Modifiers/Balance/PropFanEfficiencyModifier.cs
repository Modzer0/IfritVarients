// Decompiled with JetBrains decompiler
// Type: qol.WeaponLoading.Modifiers.Balance.PropFanEfficiencyModifier
// Assembly: qol, Version=1.1.6.3, Culture=neutral, PublicKeyToken=null
// MVID: 3B6539EA-E38C-45EE-8FFF-FC58CC42BADC
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\BepInEx\plugins\qol_1.1.6.3b1.dll

using HarmonyLib;
using qol.Utilities;
using qol.WeaponLoading.Configs;
using qol.WeaponLoading.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#nullable disable
namespace qol.WeaponLoading.Modifiers.Balance;

public class PropFanEfficiencyModifier : IEntityModifier
{
  public string ModifierId => "PropFanEfficiency";

  public int Priority => 41;

  public IReadOnlyList<string> Dependencies => (IReadOnlyList<string>) Array.Empty<string>();

  public bool CanApply(ModificationContext context) => true;

  public void Apply(ModificationContext context)
  {
    AnimationCurve animationCurve = new AnimationCurve(PropFanEfficiencyConfigs.BrawlerEfficiencyKeyframes.ToArray<Keyframe>());
    int num = 0;
    foreach (string brawlerPropFanPath in (IEnumerable<string>) PropFanEfficiencyConfigs.BrawlerPropFanPaths)
    {
      GameObject gameObject = PathLookup.Find(brawlerPropFanPath, false);
      if ((UnityEngine.Object) gameObject == (UnityEngine.Object) null)
      {
        context.Logger.LogWarning((object) $"[{this.ModifierId}] Path not found: {brawlerPropFanPath}");
      }
      else
      {
        PropFan component = gameObject.GetComponent<PropFan>();
        if ((UnityEngine.Object) component == (UnityEngine.Object) null)
        {
          context.Logger.LogWarning((object) $"[{this.ModifierId}] No PropFan component on {brawlerPropFanPath}");
        }
        else
        {
          Traverse.Create((object) component).Field("efficiencyCurve").SetValue((object) animationCurve);
          ++num;
          context.Logger.LogDebug((object) $"[{this.ModifierId}] Set efficiency curve on {brawlerPropFanPath}");
        }
      }
    }
    context.Logger.LogInfo((object) $"[{this.ModifierId}] Modified {num} propfan engines");
  }
}

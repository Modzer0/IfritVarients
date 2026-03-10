// Decompiled with JetBrains decompiler
// Type: qol.WeaponLoading.Modifiers.Effects.ParticleEmissionModifier
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
namespace qol.WeaponLoading.Modifiers.Effects;

public class ParticleEmissionModifier : IEntityModifier
{
  public string ModifierId => "ParticleEmissions";

  public int Priority => 25;

  public IReadOnlyList<string> Dependencies => (IReadOnlyList<string>) Array.Empty<string>();

  public bool CanApply(ModificationContext context) => true;

  public void Apply(ModificationContext context)
  {
    int num1 = 0;
    int num2 = 0;
    foreach (string disableEmissionPath in (IEnumerable<string>) ParticleEmissionConfigs.DisableEmissionPaths)
    {
      GameObject gameObject = PathLookup.Find(disableEmissionPath, false);
      if ((UnityEngine.Object) gameObject != (UnityEngine.Object) null)
      {
        ParticleSystem component = gameObject.GetComponent<ParticleSystem>();
        if ((UnityEngine.Object) component != (UnityEngine.Object) null)
        {
          component.emissionRate = 0.0f;
          ++num1;
        }
        else
          context.Logger.LogDebug((object) $"[{this.ModifierId}] No ParticleSystem on {disableEmissionPath}");
      }
      else
        ++num2;
    }
    context.Logger.LogInfo((object) $"[{this.ModifierId}] Disabled {num1}/{ParticleEmissionConfigs.DisableEmissionPaths.Count} particle emitters ({num2} paths not found)");
  }
}

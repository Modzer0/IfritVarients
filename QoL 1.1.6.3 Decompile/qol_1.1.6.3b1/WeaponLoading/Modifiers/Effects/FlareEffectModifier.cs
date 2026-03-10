// Decompiled with JetBrains decompiler
// Type: qol.WeaponLoading.Modifiers.Effects.FlareEffectModifier
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

public class FlareEffectModifier : IEntityModifier
{
  public string ModifierId => "FlareEffect";

  public int Priority => 37;

  public IReadOnlyList<string> Dependencies => (IReadOnlyList<string>) Array.Empty<string>();

  public bool CanApply(ModificationContext context) => true;

  public void Apply(ModificationContext context)
  {
    int num = 0;
    foreach (string flareParticlePath in (IEnumerable<string>) FlareEffectConfigs.FlareParticlePaths)
    {
      GameObject gameObject = PathLookup.Find(flareParticlePath, false);
      if ((UnityEngine.Object) gameObject != (UnityEngine.Object) null)
      {
        ParticleSystem component = gameObject.GetComponent<ParticleSystem>();
        if ((UnityEngine.Object) component != (UnityEngine.Object) null)
        {
          component.main.duration = 10f;
          ++num;
          context.Logger.LogDebug((object) $"[{this.ModifierId}] Set {flareParticlePath} duration to {(ValueType) 10f}s");
        }
        else
          context.Logger.LogWarning((object) $"[{this.ModifierId}] No ParticleSystem on {flareParticlePath}");
      }
      else
        context.Logger.LogDebug((object) $"[{this.ModifierId}] Path not found: {flareParticlePath}");
    }
    context.Logger.LogInfo((object) $"[{this.ModifierId}] Modified {num}/{FlareEffectConfigs.FlareParticlePaths.Count} flare effects");
  }
}

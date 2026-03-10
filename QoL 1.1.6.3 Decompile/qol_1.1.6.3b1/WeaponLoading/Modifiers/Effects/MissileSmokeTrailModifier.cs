// Decompiled with JetBrains decompiler
// Type: qol.WeaponLoading.Modifiers.Effects.MissileSmokeTrailModifier
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

public class MissileSmokeTrailModifier : IEntityModifier
{
  public string ModifierId => "MissileSmokeTrail";

  public int Priority => 36;

  public IReadOnlyList<string> Dependencies => (IReadOnlyList<string>) Array.Empty<string>();

  public bool CanApply(ModificationContext context) => true;

  public void Apply(ModificationContext context)
  {
    int num1 = 0;
    int num2 = 0;
    foreach ((string str, float Duration) in (IEnumerable<(string Path, float Duration)>) MissileSmokeTrailConfigs.ParticleDurations)
    {
      GameObject gameObject = PathLookup.Find(str, false);
      if ((UnityEngine.Object) gameObject != (UnityEngine.Object) null)
      {
        ParticleSystem component = gameObject.GetComponent<ParticleSystem>();
        if ((UnityEngine.Object) component != (UnityEngine.Object) null)
        {
          component.main.duration = Duration;
          ++num1;
          context.Logger.LogDebug((object) $"[{this.ModifierId}] Set {str} duration to {Duration}s");
        }
        else
          context.Logger.LogWarning((object) $"[{this.ModifierId}] No ParticleSystem on {str}");
      }
      else
        context.Logger.LogDebug((object) $"[{this.ModifierId}] Path not found: {str}");
    }
    foreach ((string str, float Opacity) in (IEnumerable<(string Path, float Opacity)>) MissileSmokeTrailConfigs.TrailOpacities)
    {
      GameObject gameObject = PathLookup.Find(str, false);
      if ((UnityEngine.Object) gameObject != (UnityEngine.Object) null)
      {
        TrailEmitter component = gameObject.GetComponent<TrailEmitter>();
        if ((UnityEngine.Object) component != (UnityEngine.Object) null)
        {
          component.opacity = Opacity;
          ++num2;
          context.Logger.LogDebug((object) $"[{this.ModifierId}] Set {str} opacity to {Opacity}");
        }
        else
          context.Logger.LogWarning((object) $"[{this.ModifierId}] No TrailEmitter on {str}");
      }
      else
        context.Logger.LogDebug((object) $"[{this.ModifierId}] Path not found: {str}");
    }
    context.Logger.LogInfo((object) $"[{this.ModifierId}] Modified {num1} particle durations, {num2} trail opacities");
  }
}

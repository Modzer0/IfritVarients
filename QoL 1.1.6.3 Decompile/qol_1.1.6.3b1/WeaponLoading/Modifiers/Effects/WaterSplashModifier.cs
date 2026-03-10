// Decompiled with JetBrains decompiler
// Type: qol.WeaponLoading.Modifiers.Effects.WaterSplashModifier
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

public class WaterSplashModifier : IEntityModifier
{
  public string ModifierId => "WaterSplash";

  public int Priority => 35;

  public IReadOnlyList<string> Dependencies => (IReadOnlyList<string>) Array.Empty<string>();

  public bool CanApply(ModificationContext context) => true;

  public void Apply(ModificationContext context)
  {
    int num1 = 0;
    int num2 = 0;
    foreach (string waterEffectPath in (IEnumerable<string>) WaterSplashConfigs.WaterEffectPaths)
    {
      GameObject gameObject = PathLookup.Find(waterEffectPath, false);
      if ((UnityEngine.Object) gameObject != (UnityEngine.Object) null)
      {
        ParticleSystem component = gameObject.GetComponent<ParticleSystem>();
        if ((UnityEngine.Object) component != (UnityEngine.Object) null)
        {
          ParticleSystem.MainModule main = component.main;
          main.startLifetimeMultiplier *= 2f;
          main.startSizeMultiplier *= 2f;
          ++num1;
          context.Logger.LogDebug((object) $"[{this.ModifierId}] Modified {waterEffectPath}: lifetime x{(ValueType) 2f}, size x{(ValueType) 2f}");
        }
        else
          context.Logger.LogWarning((object) $"[{this.ModifierId}] No ParticleSystem on {waterEffectPath}");
      }
      else
        ++num2;
    }
    context.Logger.LogInfo((object) $"[{this.ModifierId}] Modified {num1}/{WaterSplashConfigs.WaterEffectPaths.Count} water effects ({num2} not found)");
  }
}

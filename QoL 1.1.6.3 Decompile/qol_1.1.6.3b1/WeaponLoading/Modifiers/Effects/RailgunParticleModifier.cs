// Decompiled with JetBrains decompiler
// Type: qol.WeaponLoading.Modifiers.Effects.RailgunParticleModifier
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

public class RailgunParticleModifier : IEntityModifier
{
  public string ModifierId => "RailgunParticle";

  public int Priority => 55;

  public IReadOnlyList<string> Dependencies => (IReadOnlyList<string>) Array.Empty<string>();

  public bool CanApply(ModificationContext context) => true;

  public void Apply(ModificationContext context)
  {
    GameObject gameObject1 = PathLookup.Find("P_KEM1/FireParticles", false);
    if ((UnityEngine.Object) gameObject1 != (UnityEngine.Object) null)
    {
      ParticleSystem component = gameObject1.GetComponent<ParticleSystem>();
      if ((UnityEngine.Object) component != (UnityEngine.Object) null)
      {
        component.main.duration = 14f;
        context.Logger.LogDebug((object) $"[{this.ModifierId}] Set KEM fire duration to {(ValueType) 14f}");
      }
    }
    this.ApplyRailgunHitEffects(context, "railgunHit_armor", 0.1f, 3f, 1f);
    this.ApplyRailgunHitEffects(context, "railgunHit_dusty", 0.1f, 2f, 2f);
    foreach (string smokeLingerPath in (IEnumerable<string>) RailgunParticleConfigs.SmokeLingerPaths)
    {
      GameObject gameObject2 = PathLookup.Find(smokeLingerPath, false);
      if ((UnityEngine.Object) gameObject2 != (UnityEngine.Object) null)
      {
        ParticleSystem component = gameObject2.GetComponent<ParticleSystem>();
        if ((UnityEngine.Object) component != (UnityEngine.Object) null)
        {
          component.emissionRate = 30f;
          context.Logger.LogDebug((object) $"[{this.ModifierId}] Set smoke linger emission to {(ValueType) 30f}");
        }
      }
    }
    context.Logger.LogInfo((object) $"[{this.ModifierId}] Applied railgun and KEM particle effects");
  }

  private void ApplyRailgunHitEffects(
    ModificationContext context,
    string path,
    float gravityModifier,
    float sizeMultiplier,
    float lifetimeMultiplier)
  {
    GameObject gameObject = PathLookup.Find(path, false);
    if ((UnityEngine.Object) gameObject == (UnityEngine.Object) null)
      return;
    foreach (ParticleSystem componentsInChild in gameObject.GetComponentsInChildren<ParticleSystem>())
    {
      componentsInChild.playOnAwake = true;
      componentsInChild.gravityModifier = gravityModifier;
      componentsInChild.startSize *= sizeMultiplier;
      if ((double) lifetimeMultiplier != 1.0)
        componentsInChild.startLifetime *= lifetimeMultiplier;
      if ((double) componentsInChild.emissionRate <= 1.0)
        componentsInChild.main.duration = componentsInChild.startLifetime;
    }
  }
}

// Decompiled with JetBrains decompiler
// Type: qol.WeaponLoading.Modifiers.Effects.ExplosionParticleModifier
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
namespace qol.WeaponLoading.Modifiers.Effects;

public class ExplosionParticleModifier : IEntityModifier
{
  public string ModifierId => "ExplosionParticles";

  public int Priority => 20;

  public IReadOnlyList<string> Dependencies => (IReadOnlyList<string>) Array.Empty<string>();

  public bool CanApply(ModificationContext context) => true;

  public void Apply(ModificationContext context)
  {
    int num1 = 0;
    foreach (string explosionPrefab in (IEnumerable<string>) ExplosionConfigs.ExplosionPrefabs)
    {
      GameObject gameObject = PathLookup.Find(explosionPrefab);
      if ((UnityEngine.Object) gameObject != (UnityEngine.Object) null)
      {
        SetGlobalParticles setGlobalParticles = gameObject.AddComponent<SetGlobalParticles>();
        List<ParticleSystem> list = ((IEnumerable<ParticleSystem>) gameObject.GetComponentsInChildren<ParticleSystem>()).ToList<ParticleSystem>();
        Traverse.Create((object) setGlobalParticles).Field("systems").SetValue((object) list);
        context.Logger.LogDebug((object) $"[{this.ModifierId}] Added SetGlobalParticles to {explosionPrefab} ({list.Count} systems)");
        ++num1;
      }
      else
        context.Logger.LogWarning((object) $"[{this.ModifierId}] Prefab '{explosionPrefab}' not found");
    }
    foreach (string cannonHitPrefab in (IEnumerable<string>) ExplosionConfigs.CannonHitPrefabs)
    {
      GameObject gameObject = PathLookup.Find(cannonHitPrefab);
      if ((UnityEngine.Object) gameObject != (UnityEngine.Object) null)
      {
        SetGlobalParticles component = gameObject.GetComponent<SetGlobalParticles>();
        if ((UnityEngine.Object) component != (UnityEngine.Object) null)
        {
          List<ParticleSystem> list = ((IEnumerable<ParticleSystem>) gameObject.GetComponentsInChildren<ParticleSystem>()).ToList<ParticleSystem>();
          Traverse.Create((object) component).Field("systems").SetValue((object) list);
          context.Logger.LogDebug((object) $"[{this.ModifierId}] Updated SetGlobalParticles on {cannonHitPrefab} ({list.Count} systems)");
          ++num1;
        }
        else
          context.Logger.LogWarning((object) $"[{this.ModifierId}] SetGlobalParticles not found on {cannonHitPrefab}");
      }
      else
        context.Logger.LogWarning((object) $"[{this.ModifierId}] Prefab '{cannonHitPrefab}' not found");
    }
    int num2 = ExplosionConfigs.ExplosionPrefabs.Count + ExplosionConfigs.CannonHitPrefabs.Count;
    context.Logger.LogInfo((object) $"[{this.ModifierId}] Modified {num1}/{num2} explosion/impact prefabs");
  }
}

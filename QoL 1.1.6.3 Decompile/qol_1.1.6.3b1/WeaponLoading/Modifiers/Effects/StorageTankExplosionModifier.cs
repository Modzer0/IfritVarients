// Decompiled with JetBrains decompiler
// Type: qol.WeaponLoading.Modifiers.Effects.StorageTankExplosionModifier
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
namespace qol.WeaponLoading.Modifiers.Effects;

public class StorageTankExplosionModifier : IEntityModifier
{
  private static readonly FieldInfo DamageParticlesFireDamageField = typeof (DamageParticles).GetField("fireDamage", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
  private static readonly FieldInfo DamageParticlesFireLifetimeField = typeof (DamageParticles).GetField("fireLifetime", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
  private static readonly FieldInfo DamageParticlesFireRangeField = typeof (DamageParticles).GetField("fireRange", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

  public string ModifierId => "StorageTankExplosion";

  public int Priority => 61;

  public IReadOnlyList<string> Dependencies => (IReadOnlyList<string>) Array.Empty<string>();

  public bool CanApply(ModificationContext context)
  {
    return StorageTankExplosionModifier.DamageParticlesFireDamageField != (FieldInfo) null && StorageTankExplosionModifier.DamageParticlesFireLifetimeField != (FieldInfo) null && StorageTankExplosionModifier.DamageParticlesFireRangeField != (FieldInfo) null;
  }

  public void Apply(ModificationContext context)
  {
    GameObject gameObject1 = PathLookup.Find("StorageTankExplosion/Shockwave", false);
    if ((UnityEngine.Object) gameObject1 != (UnityEngine.Object) null)
    {
      Shockwave component = gameObject1.GetComponent<Shockwave>();
      if ((UnityEngine.Object) component != (UnityEngine.Object) null)
        component.enabled = false;
    }
    GameObject gameObject2 = PathLookup.Find("StorageTankExplosion/fireball_large/Embers", false);
    if ((UnityEngine.Object) gameObject2 != (UnityEngine.Object) null)
    {
      DamageParticles damageParticles = gameObject2.AddComponent<DamageParticles>();
      StorageTankExplosionModifier.DamageParticlesFireDamageField.SetValue((object) damageParticles, (object) 50f);
      StorageTankExplosionModifier.DamageParticlesFireLifetimeField.SetValue((object) damageParticles, (object) 20f);
      StorageTankExplosionModifier.DamageParticlesFireRangeField.SetValue((object) damageParticles, (object) 250f);
    }
    GameObject gameObject3 = PathLookup.Find("StorageTankExplosion/fireball_large/fireballs", false);
    if ((UnityEngine.Object) gameObject3 != (UnityEngine.Object) null)
    {
      ParticleSystem component = gameObject3.GetComponent<ParticleSystem>();
      if ((UnityEngine.Object) component != (UnityEngine.Object) null)
      {
        component.emissionRate = 1f;
        component.main.startSpeedMultiplier = 0.0f;
      }
    }
    foreach (string fireballChildPath in (IEnumerable<string>) StorageTankExplosionConfigs.FireballChildPaths)
    {
      GameObject gameObject4 = PathLookup.Find(fireballChildPath, false);
      if ((UnityEngine.Object) gameObject4 != (UnityEngine.Object) null)
      {
        ParticleSystem component = gameObject4.GetComponent<ParticleSystem>();
        if ((UnityEngine.Object) component != (UnityEngine.Object) null)
        {
          ParticleSystem.MainModule main = component.main with
          {
            startSpeedMultiplier = 0.5f,
            gravityModifier = (ParticleSystem.MinMaxCurve) -0.1f
          };
        }
      }
    }
    context.Logger.LogInfo((object) $"[{this.ModifierId}] Modified StorageTankExplosion particle effects");
  }
}

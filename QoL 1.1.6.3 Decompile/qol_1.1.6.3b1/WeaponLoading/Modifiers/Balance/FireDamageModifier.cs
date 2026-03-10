// Decompiled with JetBrains decompiler
// Type: qol.WeaponLoading.Modifiers.Balance.FireDamageModifier
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

public class FireDamageModifier : IEntityModifier
{
  private static readonly FieldInfo DamageField = typeof (DamageParticles).GetField("fireDamage", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
  private static readonly FieldInfo RangeField = typeof (DamageParticles).GetField("fireRange", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
  private static readonly FieldInfo DurationField = typeof (DamageParticles).GetField("fireLifetime", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

  public string ModifierId => "FireDamage";

  public int Priority => 50;

  public IReadOnlyList<string> Dependencies => (IReadOnlyList<string>) Array.Empty<string>();

  public bool CanApply(ModificationContext context)
  {
    return FireDamageModifier.DamageField != (FieldInfo) null && FireDamageModifier.RangeField != (FieldInfo) null && FireDamageModifier.DurationField != (FieldInfo) null;
  }

  public void Apply(ModificationContext context)
  {
    int num = 0;
    foreach ((string str, float Damage, float Range, float Duration) in (IEnumerable<(string Path, float Damage, float Range, float Duration)>) FireDamageConfigs.FireDamageSettings)
    {
      GameObject gameObject = PathLookup.Find(str, false);
      if ((UnityEngine.Object) gameObject == (UnityEngine.Object) null)
      {
        context.Logger.LogWarning((object) $"[{this.ModifierId}] Path not found: {str}");
      }
      else
      {
        DamageParticles component = gameObject.GetComponent<DamageParticles>();
        if ((UnityEngine.Object) component == (UnityEngine.Object) null)
        {
          context.Logger.LogWarning((object) $"[{this.ModifierId}] No DamageParticles component on {str}");
        }
        else
        {
          FireDamageModifier.DamageField.SetValue((object) component, (object) Damage);
          FireDamageModifier.RangeField.SetValue((object) component, (object) Range);
          FireDamageModifier.DurationField.SetValue((object) component, (object) Duration);
          ++num;
          context.Logger.LogDebug((object) $"[{this.ModifierId}] Set {str} damage={Damage}, range={Range}, duration={Duration}");
        }
      }
    }
    context.Logger.LogInfo((object) $"[{this.ModifierId}] Modified {num} fire damage settings");
  }
}

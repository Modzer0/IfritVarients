// Decompiled with JetBrains decompiler
// Type: qol.WeaponLoading.Modifiers.Balance.EW1LaserModifier
// Assembly: qol, Version=1.1.6.3, Culture=neutral, PublicKeyToken=null
// MVID: 3B6539EA-E38C-45EE-8FFF-FC58CC42BADC
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\BepInEx\plugins\qol_1.1.6.3b1.dll

using qol.Utilities;
using qol.WeaponLoading.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

#nullable disable
namespace qol.WeaponLoading.Modifiers.Balance;

public class EW1LaserModifier : IEntityModifier
{
  private static readonly FieldInfo DamageAtRangeField = typeof (Laser).GetField("damageAtRange", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

  public string ModifierId => "EW1Laser";

  public int Priority => 60;

  public IReadOnlyList<string> Dependencies => (IReadOnlyList<string>) Array.Empty<string>();

  public bool CanApply(ModificationContext context)
  {
    return EW1LaserModifier.DamageAtRangeField != (FieldInfo) null;
  }

  public void Apply(ModificationContext context)
  {
    WeaponInfo weaponInfo = ((IEnumerable<WeaponInfo>) Resources.FindObjectsOfTypeAll<WeaponInfo>()).FirstOrDefault<WeaponInfo>((Func<WeaponInfo, bool>) (wep => wep.name.Equals("Laser_EW1", StringComparison.InvariantCultureIgnoreCase)));
    if ((UnityEngine.Object) weaponInfo == (UnityEngine.Object) null)
    {
      context.Logger.LogWarning((object) $"[{this.ModifierId}] WeaponInfo not found: Laser_EW1");
    }
    else
    {
      weaponInfo.effectiveness.antiMissile = 0.5f;
      weaponInfo.effectiveness.antiAir = 0.0f;
      GameObject gameObject = PathLookup.Find("Laser_EW1", false);
      if ((UnityEngine.Object) gameObject == (UnityEngine.Object) null)
      {
        context.Logger.LogWarning((object) $"[{this.ModifierId}] Laser object not found: Laser_EW1");
      }
      else
      {
        Laser component = gameObject.GetComponent<Laser>();
        if ((UnityEngine.Object) component == (UnityEngine.Object) null)
        {
          context.Logger.LogWarning((object) $"[{this.ModifierId}] No Laser component on Laser_EW1");
        }
        else
        {
          if (EW1LaserModifier.DamageAtRangeField.GetValue((object) component) is AnimationCurve animationCurve && animationCurve.keys.Length > 1)
          {
            Keyframe[] keys = animationCurve.keys;
            keys[1].time = 30000f;
            animationCurve.keys = keys;
          }
          context.Logger.LogInfo((object) $"[{this.ModifierId}] Tuned EW1 laser: antiMissile={(ValueType) 0.5f}, maxRange={(ValueType) 30000f}");
        }
      }
    }
  }
}

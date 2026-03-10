// Decompiled with JetBrains decompiler
// Type: qol.WeaponLoading.Modifiers.Effects.DecalSizeModifier
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

public class DecalSizeModifier : IEntityModifier
{
  private static readonly FieldInfo DecalSizeField = typeof (DecalSpawner).GetField("decalSize", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
  private static readonly FieldInfo FadeInTimeField = typeof (DecalSpawner).GetField("fadeInTime", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

  public string ModifierId => "DecalSize";

  public int Priority => 51;

  public IReadOnlyList<string> Dependencies => (IReadOnlyList<string>) Array.Empty<string>();

  public bool CanApply(ModificationContext context)
  {
    return DecalSizeModifier.DecalSizeField != (FieldInfo) null && DecalSizeModifier.FadeInTimeField != (FieldInfo) null;
  }

  public void Apply(ModificationContext context)
  {
    int num1 = 0;
    int num2 = 0;
    foreach ((string str, float DecalSize, float FadeInTime) in (IEnumerable<(string Path, float DecalSize, float FadeInTime)>) DecalSizeConfigs.ExplicitDecalSettings)
    {
      GameObject gameObject = PathLookup.Find(str, false);
      if ((UnityEngine.Object) gameObject == (UnityEngine.Object) null)
      {
        context.Logger.LogWarning((object) $"[{this.ModifierId}] Path not found: {str}");
      }
      else
      {
        DecalSpawner component = gameObject.GetComponent<DecalSpawner>();
        if ((UnityEngine.Object) component == (UnityEngine.Object) null)
        {
          context.Logger.LogWarning((object) $"[{this.ModifierId}] No DecalSpawner component on {str}");
        }
        else
        {
          DecalSizeModifier.DecalSizeField.SetValue((object) component, (object) DecalSize);
          DecalSizeModifier.FadeInTimeField.SetValue((object) component, (object) FadeInTime);
          ++num1;
          context.Logger.LogDebug((object) $"[{this.ModifierId}] Set {str} decalSize={DecalSize}, fadeInTime={FadeInTime}");
        }
      }
    }
    foreach (string explosionDecalPath in (IEnumerable<string>) DecalSizeConfigs.ExplosionDecalPaths)
    {
      GameObject gameObject = PathLookup.Find(explosionDecalPath, false);
      if ((UnityEngine.Object) gameObject == (UnityEngine.Object) null)
      {
        context.Logger.LogWarning((object) $"[{this.ModifierId}] Path not found: {explosionDecalPath}");
      }
      else
      {
        DecalSpawner component = gameObject.GetComponent<DecalSpawner>();
        if ((UnityEngine.Object) component == (UnityEngine.Object) null)
        {
          context.Logger.LogWarning((object) $"[{this.ModifierId}] No DecalSpawner component on {explosionDecalPath}");
        }
        else
        {
          float num3 = (float) DecalSizeModifier.DecalSizeField.GetValue((object) component);
          float num4 = num3 * 2f;
          DecalSizeModifier.DecalSizeField.SetValue((object) component, (object) num4);
          ++num2;
          context.Logger.LogDebug((object) $"[{this.ModifierId}] Scaled {explosionDecalPath} decalSize from {num3} to {num4}");
        }
      }
    }
    context.Logger.LogInfo((object) $"[{this.ModifierId}] Set {num1} explicit decal settings, scaled {num2} explosion decals");
  }
}

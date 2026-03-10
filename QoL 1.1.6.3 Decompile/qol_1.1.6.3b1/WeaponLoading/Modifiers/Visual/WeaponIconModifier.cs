// Decompiled with JetBrains decompiler
// Type: qol.WeaponLoading.Modifiers.Visual.WeaponIconModifier
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
namespace qol.WeaponLoading.Modifiers.Visual;

public class WeaponIconModifier : IEntityModifier
{
  public string ModifierId => "WeaponIcon";

  public int Priority => 80 /*0x50*/;

  public IReadOnlyList<string> Dependencies => (IReadOnlyList<string>) Array.Empty<string>();

  public bool CanApply(ModificationContext context) => true;

  public void Apply(ModificationContext context)
  {
    string str1 = context.ExecutingAssembly.GetName().Name + ".";
    int num = 0;
    foreach ((string ResourceSuffix, string str2) in WeaponIconConfigs.IconMappings)
    {
      Texture2D texture = QOLPlugin.LoadTextureFromResource($"{str1}Resources.P_Missiles1.{ResourceSuffix}");
      if ((UnityEngine.Object) texture == (UnityEngine.Object) null)
      {
        context.Logger.LogWarning((object) $"[{this.ModifierId}] Failed to load icon: {ResourceSuffix}");
      }
      else
      {
        WeaponInfo weaponInfo = ResourceLookup.FindWeaponInfo(str2);
        if ((UnityEngine.Object) weaponInfo == (UnityEngine.Object) null)
        {
          context.Logger.LogWarning((object) $"[{this.ModifierId}] WeaponInfo not found: {str2}");
        }
        else
        {
          weaponInfo.weaponIcon = Sprite.Create(texture, new Rect(0.0f, 0.0f, (float) texture.width, (float) texture.height), new Vector2(0.5f, 0.5f));
          ++num;
        }
      }
    }
    context.Logger.LogInfo((object) $"[{this.ModifierId}] Loaded {num} weapon icons");
  }
}

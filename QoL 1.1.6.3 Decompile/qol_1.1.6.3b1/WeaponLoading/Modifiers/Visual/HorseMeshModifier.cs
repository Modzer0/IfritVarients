// Decompiled with JetBrains decompiler
// Type: qol.WeaponLoading.Modifiers.Visual.HorseMeshModifier
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

public class HorseMeshModifier : IEntityModifier
{
  public string ModifierId => "HorseMesh";

  public int Priority => 54;

  public IReadOnlyList<string> Dependencies => (IReadOnlyList<string>) Array.Empty<string>();

  public bool CanApply(ModificationContext context) => true;

  public void Apply(ModificationContext context)
  {
    int num = 0;
    foreach (string disabledMeshPath in (IEnumerable<string>) HorseMeshConfigs.DisabledMeshPaths)
    {
      GameObject gameObject = PathLookup.Find(disabledMeshPath, false);
      if ((UnityEngine.Object) gameObject == (UnityEngine.Object) null)
      {
        context.Logger.LogWarning((object) $"[{this.ModifierId}] Path not found: {disabledMeshPath}");
      }
      else
      {
        MeshRenderer component = gameObject.GetComponent<MeshRenderer>();
        if ((UnityEngine.Object) component != (UnityEngine.Object) null)
        {
          component.enabled = false;
          ++num;
        }
      }
    }
    context.Logger.LogInfo((object) $"[{this.ModifierId}] Disabled {num} mesh renderers on Horse1");
  }
}

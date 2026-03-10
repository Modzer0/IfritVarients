// Decompiled with JetBrains decompiler
// Type: qol.WeaponLoading.Modifiers.Balance.HeloRadarJammerModifier
// Assembly: qol, Version=1.1.6.3, Culture=neutral, PublicKeyToken=null
// MVID: 3B6539EA-E38C-45EE-8FFF-FC58CC42BADC
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\BepInEx\plugins\qol_1.1.6.3b1.dll

using qol.Utilities;
using qol.WeaponLoading.Configs;
using qol.WeaponLoading.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#nullable disable
namespace qol.WeaponLoading.Modifiers.Balance;

public class HeloRadarJammerModifier : IEntityModifier
{
  public string ModifierId => "HeloRadarJammer";

  public int Priority => 53;

  public IReadOnlyList<string> Dependencies => (IReadOnlyList<string>) Array.Empty<string>();

  public bool CanApply(ModificationContext context) => true;

  public void Apply(ModificationContext context)
  {
    Sprite sprite1 = ((IEnumerable<Sprite>) Resources.FindObjectsOfTypeAll<Sprite>()).FirstOrDefault<Sprite>((Func<Sprite, bool>) (sprite => sprite.name == "weaponicon_radarJammer"));
    if ((UnityEngine.Object) sprite1 == (UnityEngine.Object) null)
    {
      context.Logger.LogWarning((object) $"[{this.ModifierId}] Sprite 'weaponicon_radarJammer' not found");
    }
    else
    {
      int num = 0;
      foreach (string heloJammerPath in (IEnumerable<string>) HeloRadarJammerConfigs.HeloJammerPaths)
      {
        GameObject gameObject = PathLookup.Find(heloJammerPath, false);
        if ((UnityEngine.Object) gameObject == (UnityEngine.Object) null)
          context.Logger.LogWarning((object) $"[{this.ModifierId}] Path not found: {heloJammerPath}");
        else if ((UnityEngine.Object) gameObject.GetComponent<RadarJammer>() != (UnityEngine.Object) null)
        {
          context.Logger.LogDebug((object) $"[{this.ModifierId}] {heloJammerPath} already has RadarJammer, skipping");
        }
        else
        {
          gameObject.AddComponent<RadarJammer>().displayImage = sprite1;
          ++num;
          context.Logger.LogDebug((object) $"[{this.ModifierId}] Added RadarJammer to {heloJammerPath}");
        }
      }
      context.Logger.LogInfo((object) $"[{this.ModifierId}] Added RadarJammer to {num} helicopters");
    }
  }
}

// Decompiled with JetBrains decompiler
// Type: qol.CommandProcessing.Handlers.WeaponHandler
// Assembly: qol, Version=1.1.6.3, Culture=neutral, PublicKeyToken=null
// MVID: 3B6539EA-E38C-45EE-8FFF-FC58CC42BADC
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\BepInEx\plugins\qol_1.1.6.3b1.dll

using qol.CommandProcessing.Core;
using qol.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

#nullable disable
namespace qol.CommandProcessing.Handlers;

public class WeaponHandler : ICommandHandler
{
  public static readonly Regex WeaponPattern = new Regex("^(?<path>[^\\s]+)\\s+(?<operation>add|remove)weapon(?<setIndex>\\d+)\\s+(?<prefabName>[^\\s]+)$", RegexOptions.Compiled);

  public Regex Pattern => WeaponHandler.WeaponPattern;

  public int Priority => 70;

  public void Handle(Match match, CommandContext context)
  {
    string path = match.Groups["path"].Value.Trim();
    int num = int.Parse(match.Groups["setIndex"].Value);
    string prefabName = match.Groups["prefabName"].Value.Trim();
    bool flag = match.Groups["operation"].Value == "add";
    GameObject gameObject = PathLookup.Find(path);
    if ((UnityEngine.Object) gameObject == (UnityEngine.Object) null)
    {
      context.Logger.LogError((object) ("Weapon target not found: " + path));
    }
    else
    {
      WeaponManager[] componentsInChildren = gameObject.GetComponentsInChildren<WeaponManager>(true);
      if (componentsInChildren.Length == 0)
      {
        context.Logger.LogError((object) ("No WeaponManagers found in " + path));
      }
      else
      {
        WeaponMount weaponPrefabReference = WeaponHandler.FindWeaponPrefabReference(prefabName);
        if ((UnityEngine.Object) weaponPrefabReference == (UnityEngine.Object) null && !flag)
          return;
        foreach (WeaponManager weaponManager in componentsInChildren)
        {
          try
          {
            int index = num - 1;
            HardpointSet hardpointSet = weaponManager.hardpointSets[index];
            if (flag)
            {
              if ((UnityEngine.Object) weaponPrefabReference == (UnityEngine.Object) null)
                hardpointSet.weaponOptions.Add((WeaponMount) null);
              if (!hardpointSet.weaponOptions.Contains(weaponPrefabReference))
                hardpointSet.weaponOptions.Add(weaponPrefabReference);
            }
            else
              hardpointSet.weaponOptions.RemoveAll((Predicate<WeaponMount>) (w => (UnityEngine.Object) w != (UnityEngine.Object) null && w.name.Equals(prefabName, StringComparison.OrdinalIgnoreCase)));
          }
          catch (Exception ex)
          {
            context.Logger.LogError((object) ("Failed to modify weapons: " + ex.Message));
          }
        }
      }
    }
  }

  private static WeaponMount FindWeaponPrefabReference(string prefabName)
  {
    return Resources.Load<WeaponMount>(prefabName) ?? ((IEnumerable<WeaponMount>) Resources.FindObjectsOfTypeAll<WeaponMount>()).FirstOrDefault<WeaponMount>((Func<WeaponMount, bool>) (w => w.name.Equals(prefabName, StringComparison.OrdinalIgnoreCase)));
  }
}

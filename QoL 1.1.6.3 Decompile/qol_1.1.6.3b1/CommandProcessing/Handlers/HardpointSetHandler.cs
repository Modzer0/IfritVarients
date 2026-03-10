// Decompiled with JetBrains decompiler
// Type: qol.CommandProcessing.Handlers.HardpointSetHandler
// Assembly: qol, Version=1.1.6.3, Culture=neutral, PublicKeyToken=null
// MVID: 3B6539EA-E38C-45EE-8FFF-FC58CC42BADC
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\BepInEx\plugins\qol_1.1.6.3b1.dll

using qol.CommandProcessing.Core;
using qol.CommandProcessing.Helpers;
using qol.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

#nullable disable
namespace qol.CommandProcessing.Handlers;

public class HardpointSetHandler : ICommandHandler
{
  public static readonly Regex HardpointSetPattern = new Regex("^(?<path>(?:\"[^\"]+\"|\\S+))\\s+hardpointset\\s+(?<operation>add|name|addhardpoint|modifyhardpoint|precludehardpoint)\\s+(?:(?<setIndex>\\d+)\\s*)?(?:(?<name>(?:\"[^\"]+\"|\\S+))\\s*)?(?:(?<hpIndex>\\d+)\\s*)?(?:(?<transformPath>(?:\"[^\"]+\"|\\S+))\\s*)?(?:(?<partPath>(?:\"[^\"]+\"|\\S+))\\s*)?$", RegexOptions.Compiled);

  public Regex Pattern => HardpointSetHandler.HardpointSetPattern;

  public int Priority => 90;

  public void Handle(Match match, CommandContext context)
  {
    string path = match.Groups["path"].Value.Trim();
    string lower = match.Groups["operation"].Value.Trim().ToLower();
    GameObject gameObject = PathLookup.Find(path);
    if ((UnityEngine.Object) gameObject == (UnityEngine.Object) null)
    {
      context.Logger.LogError((object) ("WeaponManager target not found: " + path));
    }
    else
    {
      WeaponManager component = gameObject.GetComponent<WeaponManager>();
      if ((UnityEngine.Object) component == (UnityEngine.Object) null)
      {
        context.Logger.LogError((object) ("No WeaponManager found at: " + path));
      }
      else
      {
        try
        {
          switch (lower)
          {
            case "add":
              this.ProcessAddHardpointSet(component, context);
              break;
            case "name":
              this.ProcessNameHardpointSet(component, match, context);
              break;
            case "addhardpoint":
              this.ProcessAddHardpoint(component, match, context);
              break;
            case "modifyhardpoint":
              this.ProcessModifyHardpoint(component, match, context);
              break;
            case "precludehardpoint":
              this.ProcessPrecludeHardpoint(component, match, context);
              break;
            default:
              context.Logger.LogError((object) ("Unknown hardpointset operation: " + lower));
              break;
          }
        }
        catch (Exception ex)
        {
          context.Logger.LogError((object) ("Failed to process hardpoint command: " + ex.Message));
        }
      }
    }
  }

  private void ProcessAddHardpointSet(WeaponManager weaponManager, CommandContext context)
  {
    HardpointSet hardpointSet = new HardpointSet()
    {
      name = $"NewSet_{weaponManager.hardpointSets.Length + 1}",
      hardpoints = new List<Hardpoint>(),
      weaponOptions = new List<WeaponMount>()
    };
    HardpointSet[] destinationArray = new HardpointSet[weaponManager.hardpointSets.Length + 1];
    Array.Copy((Array) weaponManager.hardpointSets, (Array) destinationArray, weaponManager.hardpointSets.Length);
    HardpointSet[] hardpointSetArray = destinationArray;
    hardpointSetArray[hardpointSetArray.Length - 1] = hardpointSet;
    weaponManager.hardpointSets = destinationArray;
    context.Logger.LogDebug((object) $"Added new HardpointSet at index {weaponManager.hardpointSets.Length - 1} for {weaponManager} {weaponManager.name}");
  }

  private void ProcessNameHardpointSet(
    WeaponManager weaponManager,
    Match match,
    CommandContext context)
  {
    int result;
    if (!int.TryParse(match.Groups["setIndex"].Value, out result))
    {
      context.Logger.LogError((object) "Invalid set index format");
    }
    else
    {
      if (!ValidationHelpers.ValidateArrayIndex<HardpointSet>(result, weaponManager.hardpointSets, context.Logger, "HardpointSet"))
        return;
      string str = StringHelpers.CleanValue(match.Groups["name"].Value);
      weaponManager.hardpointSets[result].name = str;
      weaponManager.hardpointSets[result].precludingHardpointSets = new List<byte>();
      context.Logger.LogDebug((object) $"Renamed HardpointSet {result} of {weaponManager} {weaponManager.name} to '{str}'");
    }
  }

  private void ProcessAddHardpoint(
    WeaponManager weaponManager,
    Match match,
    CommandContext context)
  {
    int result;
    if (!int.TryParse(match.Groups["setIndex"].Value, out result))
    {
      context.Logger.LogError((object) "Invalid set index format");
    }
    else
    {
      if (!ValidationHelpers.ValidateArrayIndex<HardpointSet>(result, weaponManager.hardpointSets, context.Logger, $"HardpointSet for {weaponManager} {match}"))
        return;
      Hardpoint hardpoint = new Hardpoint()
      {
        transform = new GameObject("Hardpoint_" + Guid.NewGuid().ToString().Substring(0, 8)).transform,
        part = (UnitPart) null
      };
      weaponManager.hardpointSets[result].hardpoints.Add(hardpoint);
      context.Logger.LogDebug((object) $"Added new Hardpoint to set {result} on {weaponManager} {weaponManager.name}");
    }
  }

  private void ProcessModifyHardpoint(
    WeaponManager weaponManager,
    Match match,
    CommandContext context)
  {
    int result1;
    int result2;
    if (!int.TryParse(match.Groups["setIndex"].Value, out result1) || !int.TryParse(match.Groups["hpIndex"].Value, out result2))
    {
      context.Logger.LogError((object) "Invalid index format");
    }
    else
    {
      if (!ValidationHelpers.ValidateArrayIndex<HardpointSet>(result1, weaponManager.hardpointSets, context.Logger, "HardpointSet"))
        return;
      List<Hardpoint> hardpoints = weaponManager.hardpointSets[result1].hardpoints;
      if (!ValidationHelpers.ValidateListIndex<Hardpoint>(result2, (IList<Hardpoint>) hardpoints, context.Logger, $"Hardpoint for {weaponManager} {match}"))
        return;
      string path = StringHelpers.CleanValue(match.Groups["transformPath"].Value);
      if (!string.IsNullOrEmpty(path))
      {
        GameObject gameObject = PathLookup.Find(path);
        if ((UnityEngine.Object) gameObject != (UnityEngine.Object) null)
        {
          hardpoints[result2].transform = gameObject.transform;
          context.Logger.LogDebug((object) $"Updated hardpoint {result2} transform to {path}");
        }
        else
          context.Logger.LogWarning((object) ("Transform object not found: " + path));
      }
      string partPath = StringHelpers.CleanValue(match.Groups["partPath"].Value);
      if (string.IsNullOrEmpty(partPath))
        return;
      AeroPart aeroPart = ((IEnumerable<AeroPart>) Resources.FindObjectsOfTypeAll<AeroPart>()).FirstOrDefault<AeroPart>((Func<AeroPart, bool>) (p => HardpointSetHandler.GetFullPath(p.transform) == partPath));
      if ((UnityEngine.Object) aeroPart != (UnityEngine.Object) null)
      {
        hardpoints[result2].part = (UnitPart) aeroPart;
        context.Logger.LogDebug((object) $"Updated hardpoint {result2} part to {partPath}");
      }
      else
        context.Logger.LogWarning((object) ("AeroPart not found: " + partPath));
    }
  }

  private void ProcessPrecludeHardpoint(
    WeaponManager weaponManager,
    Match match,
    CommandContext context)
  {
    int result1;
    int result2;
    if (!int.TryParse(match.Groups["setIndex"].Value, out result1) || !int.TryParse(match.Groups["hpIndex"].Value, out result2))
    {
      context.Logger.LogError((object) "Invalid index format for preclude command");
    }
    else
    {
      if (!ValidationHelpers.ValidateArrayIndex<HardpointSet>(result1, weaponManager.hardpointSets, context.Logger, "HardpointSet"))
        return;
      HardpointSet hardpointSet = weaponManager.hardpointSets[result1];
      hardpointSet.precludingHardpointSets.Capacity = 4;
      hardpointSet.precludingHardpointSets.Add((byte) result2);
      context.Logger.LogDebug((object) $"Added preclusion: Set {result1} on {weaponManager} {weaponManager.name} will now disable when set {result2} is active");
    }
  }

  private static string GetFullPath(Transform transform)
  {
    string fullPath = transform.name;
    while ((UnityEngine.Object) transform.parent != (UnityEngine.Object) null)
    {
      transform = transform.parent;
      fullPath = $"{transform.name}/{fullPath}";
    }
    return fullPath;
  }
}

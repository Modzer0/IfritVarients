// Decompiled with JetBrains decompiler
// Type: qol.CommandProcessing.Handlers.DefaultLoadoutHandler
// Assembly: qol, Version=1.1.6.3, Culture=neutral, PublicKeyToken=null
// MVID: 3B6539EA-E38C-45EE-8FFF-FC58CC42BADC
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\BepInEx\plugins\qol_1.1.6.3b1.dll

using qol.CommandProcessing.Core;
using qol.CommandProcessing.Helpers;
using qol.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;

#nullable disable
namespace qol.CommandProcessing.Handlers;

public class DefaultLoadoutHandler : ICommandHandler
{
  public static readonly Regex DefaultLoadoutPattern = new Regex("^(?<aircraftParams>(?:\"[^\"]+\"|\\S+))\\s+modifydefaultloadout\\s+(?<weaponIndex>\\d+)\\s+(?<weaponMount>(?:\"[^\"]+\"|\\S+))$", RegexOptions.Compiled);

  public Regex Pattern => DefaultLoadoutHandler.DefaultLoadoutPattern;

  public int Priority => 30;

  public void Handle(Match match, CommandContext context)
  {
    string aircraftParamsName = StringHelpers.StripQuotes(match.Groups["aircraftParams"].Value);
    int result;
    if (!int.TryParse(match.Groups["weaponIndex"].Value, out result))
    {
      context.Logger.LogError((object) "Invalid weapon index format");
    }
    else
    {
      string weaponMountName = StringHelpers.StripQuotes(match.Groups["weaponMount"].Value);
      AircraftParameters aircraftParameters = ((IEnumerable<AircraftParameters>) Resources.FindObjectsOfTypeAll<AircraftParameters>()).FirstOrDefault<AircraftParameters>((Func<AircraftParameters, bool>) (p => p.name.Equals(aircraftParamsName, StringComparison.OrdinalIgnoreCase)));
      if ((UnityEngine.Object) aircraftParameters == (UnityEngine.Object) null)
      {
        context.Logger.LogError((object) $"AircraftParameters '{aircraftParamsName}' not found");
      }
      else
      {
        FieldInfo field1 = typeof (AircraftParameters).GetField("loadouts", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (field1 == (FieldInfo) null)
          context.Logger.LogError((object) "Could not find 'loadouts' field in AircraftParameters");
        else if (!(field1.GetValue((object) aircraftParameters) is IList list1))
          context.Logger.LogError((object) "'loadouts' field is not a list");
        else if (list1.Count < 2)
        {
          context.Logger.LogError((object) "Loadouts list has less than 2 elements");
        }
        else
        {
          object obj = list1[1];
          if (aircraftParamsName == "sd2_Parameters")
            obj = list1[0];
          FieldInfo field2 = ReflectionHelpers.GetField(obj.GetType(), "weapons");
          if (field2 == (FieldInfo) null)
            context.Logger.LogError((object) "Loadout has no 'weapons' field");
          else if (!(field2.GetValue(obj) is IList list))
          {
            context.Logger.LogError((object) "'weapons' field is not a list");
          }
          else
          {
            WeaponMount weaponMount = Resources.Load<WeaponMount>(weaponMountName) ?? ((IEnumerable<WeaponMount>) Resources.FindObjectsOfTypeAll<WeaponMount>()).FirstOrDefault<WeaponMount>((Func<WeaponMount, bool>) (w => w.name.Equals(weaponMountName, StringComparison.OrdinalIgnoreCase)));
            if ((UnityEngine.Object) weaponMount == (UnityEngine.Object) null && weaponMountName != "")
            {
              context.Logger.LogError((object) $"WeaponMount '{weaponMountName}' not found");
            }
            else
            {
              while (result >= list.Count)
                list.Add((object) null);
              list[result] = (object) weaponMount;
              context.Logger.LogDebug((object) $"{(result >= list.Count ? (object) "Added" : (object) "Replaced")} weapon index {result} with '{weaponMountName}' in default loadout");
            }
          }
        }
      }
    }
  }
}

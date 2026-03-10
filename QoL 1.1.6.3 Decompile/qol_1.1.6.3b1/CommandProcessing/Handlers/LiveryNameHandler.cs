// Decompiled with JetBrains decompiler
// Type: qol.CommandProcessing.Handlers.LiveryNameHandler
// Assembly: qol, Version=1.1.6.3, Culture=neutral, PublicKeyToken=null
// MVID: 3B6539EA-E38C-45EE-8FFF-FC58CC42BADC
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\BepInEx\plugins\qol_1.1.6.3b1.dll

using qol.CommandProcessing.Core;
using qol.CommandProcessing.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;

#nullable disable
namespace qol.CommandProcessing.Handlers;

public class LiveryNameHandler : ICommandHandler
{
  public static readonly Regex LiveryNamePattern = new Regex("^(?<aircraftParams>(?:\"[^\"]+\"|\\S+))\\s+modifyliveryname\\s+(?<index>\\d+)\\s+(?<newName>(?:\"[^\"]+\"|\\S+))$", RegexOptions.Compiled);

  public Regex Pattern => LiveryNameHandler.LiveryNamePattern;

  public int Priority => 40;

  public void Handle(Match match, CommandContext context)
  {
    string aircraftParamsName = StringHelpers.StripQuotes(match.Groups["aircraftParams"].Value);
    int result;
    if (!int.TryParse(match.Groups["index"].Value, out result))
    {
      context.Logger.LogError((object) "Invalid livery index format");
    }
    else
    {
      string str = StringHelpers.StripQuotes(match.Groups["newName"].Value);
      AircraftParameters aircraftParameters = ((IEnumerable<AircraftParameters>) Resources.FindObjectsOfTypeAll<AircraftParameters>()).FirstOrDefault<AircraftParameters>((Func<AircraftParameters, bool>) (p => p.name.Equals(aircraftParamsName, StringComparison.OrdinalIgnoreCase)));
      if ((UnityEngine.Object) aircraftParameters == (UnityEngine.Object) null)
      {
        context.Logger.LogError((object) $"AircraftParameters '{aircraftParamsName}' not found");
      }
      else
      {
        FieldInfo field1 = typeof (AircraftParameters).GetField("liveries", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (field1 == (FieldInfo) null)
        {
          context.Logger.LogError((object) "Could not find 'liveries' field in AircraftParameters");
        }
        else
        {
          object obj1 = field1.GetValue((object) aircraftParameters);
          if (!(obj1 is IList list))
          {
            context.Logger.LogError((object) "'liveries' field is not a list");
          }
          else
          {
            if (!ValidationHelpers.ValidateIListIndex(result, list, context.Logger, "Livery"))
              return;
            object obj2 = list[result];
            if (obj2 == null)
            {
              context.Logger.LogError((object) $"Livery at index {result} is null");
            }
            else
            {
              Type type = obj2.GetType();
              FieldInfo field2 = type.GetField("name", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
              if (field2 != (FieldInfo) null)
              {
                field2.SetValue(obj2, (object) str);
              }
              else
              {
                PropertyInfo property = type.GetProperty("name", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (property != (PropertyInfo) null && property.CanWrite)
                {
                  property.SetValue(obj2, (object) str);
                }
                else
                {
                  context.Logger.LogError((object) "Could not find writable 'name' field or property on Livery object");
                  return;
                }
              }
              if (!(obj1 is Array) || !type.IsValueType)
                return;
              list[result] = obj2;
            }
          }
        }
      }
    }
  }
}

// Decompiled with JetBrains decompiler
// Type: qol.CommandProcessing.Handlers.MountModHandler
// Assembly: qol, Version=1.1.6.3, Culture=neutral, PublicKeyToken=null
// MVID: 3B6539EA-E38C-45EE-8FFF-FC58CC42BADC
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\BepInEx\plugins\qol_1.1.6.3b1.dll

using qol.CommandProcessing.Core;
using qol.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;

#nullable disable
namespace qol.CommandProcessing.Handlers;

public class MountModHandler : ICommandHandler
{
  public static readonly Regex MountModPattern = new Regex("^(?<path>[^\\s]+)\\s+modifymount\\s+(?<fieldName>[^\\s]+)\\s+(?<value>(\"[^\"]*\"|'[^']*'|[^\\s]+))$", RegexOptions.Compiled);

  public Regex Pattern => MountModHandler.MountModPattern;

  public int Priority => 60;

  public void Handle(Match match, CommandContext context)
  {
    try
    {
      string path = match.Groups["path"].Value.Trim();
      string fieldName = match.Groups["fieldName"].Value.Trim();
      string value = match.Groups["value"].Value.Trim().Replace("\"", "");
      WeaponMount weaponMount = ((IEnumerable<WeaponMount>) Resources.FindObjectsOfTypeAll<WeaponMount>()).FirstOrDefault<WeaponMount>((Func<WeaponMount, bool>) (m => m.name == path));
      if ((UnityEngine.Object) weaponMount == (UnityEngine.Object) null)
      {
        context.Logger.LogWarning((object) $"Mount {path} not found");
      }
      else
      {
        FieldInfo field = ReflectionHelpers.GetField(weaponMount.GetType(), fieldName);
        if (field == (FieldInfo) null)
          context.Logger.LogWarning((object) $"Field {fieldName} not found on mount {path}");
        else if (fieldName == "info")
        {
          weaponMount.info = ((IEnumerable<WeaponInfo>) Resources.FindObjectsOfTypeAll<WeaponInfo>()).FirstOrDefault<WeaponInfo>((Func<WeaponInfo, bool>) (info => info.name.Equals(value, StringComparison.InvariantCultureIgnoreCase)));
        }
        else
        {
          object obj = Convert.ChangeType((object) value, field.FieldType, (IFormatProvider) CultureInfo.InvariantCulture);
          field.SetValue((object) weaponMount, obj);
        }
      }
    }
    catch (Exception ex)
    {
      context.Logger.LogError((object) $"Failed to process mount for {context.RawLine} and got error: {ex.Message}");
    }
  }
}

// Decompiled with JetBrains decompiler
// Type: qol.CommandProcessing.Handlers.AddComponentHandler
// Assembly: qol, Version=1.1.6.3, Culture=neutral, PublicKeyToken=null
// MVID: 3B6539EA-E38C-45EE-8FFF-FC58CC42BADC
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\BepInEx\plugins\qol_1.1.6.3b1.dll

using qol.CommandProcessing.Core;
using qol.CommandProcessing.Helpers;
using qol.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;

#nullable disable
namespace qol.CommandProcessing.Handlers;

public class AddComponentHandler : ICommandHandler
{
  public static readonly Regex AddComponentPattern = new Regex("^(?<path>(?:\"[^\"]+\"|\\S+))\\s+addcomponent\\s+(?<componentName>[^\\s]+)$", RegexOptions.Compiled);

  public Regex Pattern => AddComponentHandler.AddComponentPattern;

  public int Priority => 50;

  public void Handle(Match match, CommandContext context)
  {
    string path = StringHelpers.StripQuotes(match.Groups["path"].Value);
    string componentName = match.Groups["componentName"].Value.Trim();
    GameObject gameObject = PathLookup.Find(path);
    if ((UnityEngine.Object) gameObject == (UnityEngine.Object) null)
      context.Logger.LogError((object) ("Target object not found: " + path));
    else if (componentName == "Destroy")
    {
      gameObject.SetActive(false);
    }
    else
    {
      Type componentType = AddComponentHandler.FindComponentType(componentName);
      if (componentType == (Type) null)
      {
        context.Logger.LogError((object) ("Component type not found: " + componentName));
      }
      else
      {
        Component component = gameObject.GetComponent(componentType);
        if ((UnityEngine.Object) component != (UnityEngine.Object) null)
        {
          if (componentType == typeof (MountedMissile))
            UnityEngine.Object.Destroy((UnityEngine.Object) component.gameObject);
          else
            UnityEngine.Object.Destroy((UnityEngine.Object) component);
        }
        else
          gameObject.AddComponent(componentType);
      }
    }
  }

  private static Type FindComponentType(string componentName)
  {
    Type type = ((IEnumerable<Assembly>) AppDomain.CurrentDomain.GetAssemblies()).SelectMany<Assembly, Type>((Func<Assembly, IEnumerable<Type>>) (a => (IEnumerable<Type>) a.GetTypes())).FirstOrDefault<Type>((Func<Type, bool>) (t =>
    {
      if (!typeof (Component).IsAssignableFrom(t))
        return false;
      return t.Name.Equals(componentName) || t.FullName.Equals(componentName);
    }));
    return (object) type != null ? type : Type.GetType($"UnityEngine.{componentName}, UnityEngine");
  }
}

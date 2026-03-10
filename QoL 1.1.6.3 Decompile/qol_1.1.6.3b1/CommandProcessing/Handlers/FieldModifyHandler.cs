// Decompiled with JetBrains decompiler
// Type: qol.CommandProcessing.Handlers.FieldModifyHandler
// Assembly: qol, Version=1.1.6.3, Culture=neutral, PublicKeyToken=null
// MVID: 3B6539EA-E38C-45EE-8FFF-FC58CC42BADC
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\BepInEx\plugins\qol_1.1.6.3b1.dll

using qol.CommandProcessing.Core;
using qol.CommandProcessing.Helpers;
using qol.FieldModification.Core;
using qol.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;

#nullable disable
namespace qol.CommandProcessing.Handlers;

public class FieldModifyHandler : ICommandHandler
{
  public static readonly Regex FieldModifyPattern = new Regex("^(?<path>(?:\"[^\"]+\"|\\S+))\\s+(?<component>[^\\s/]+(?:\\/[^\\s.]+)*)\\s+(?<field>[^\\s.]+(?:\\[(?<index>\\d+)\\])?(?:\\.[^\\s.]+(?:\\[\\d+\\])?)*)\\s+(?<operation>modify|check|color|modifyobj|modifyvector|modifyaircraft|modifyvehicle|modifyship|modifypart|modifyaudio|modify2|modify3|modify4|modifyactive)\\s+(?<value>\"[^\"]+\"|\\S+)", RegexOptions.Compiled);

  public Regex Pattern => FieldModifyHandler.FieldModifyPattern;

  public int Priority => 100;

  public void Handle(Match match, CommandContext context)
  {
    string path = StringHelpers.StripQuotes(match.Groups["path"].Value);
    string componentPath = match.Groups["component"].Value;
    string fieldName = match.Groups["field"].Value;
    string operation = match.Groups["operation"].Value;
    string str = StringHelpers.StripQuotes(match.Groups["value"].Value);
    if (operation == "check")
      return;
    GameObject targetGameObject = PathLookup.Find(path);
    bool flag = componentPath == "roleIdentity" || componentPath == "typeIdentity";
    if ((UnityEngine.Object) targetGameObject == (UnityEngine.Object) null)
    {
      if (operation == "modify4" | flag)
      {
        try
        {
          FieldProcessingContext context1 = new FieldProcessingContext((object) null, (GameObject) null, path, componentPath, fieldName, str, match, context.Logger, context.Plugin);
          IFieldProcessor processor;
          if (!context.Plugin.FieldProcessorRegistry.TryGetProcessor("modify4", out processor))
            return;
          processor.Process(context1);
        }
        catch (Exception ex)
        {
          context.Logger.LogWarning((object) ("Target object not found: " + path));
          context.Logger.LogError((object) $"Exception: {ex}");
        }
      }
      else
        context.Logger.LogWarning((object) $"Path not found: {path} (skipping {operation} on {componentPath}.{fieldName})");
    }
    else if (flag)
    {
      try
      {
        FieldProcessingContext context2 = new FieldProcessingContext((object) null, targetGameObject, path, componentPath, fieldName, str, match, context.Logger, context.Plugin);
        IFieldProcessor processor;
        if (!context.Plugin.FieldProcessorRegistry.TryGetProcessor("modify4", out processor))
          return;
        processor.Process(context2);
      }
      catch (Exception ex)
      {
        context.Logger.LogWarning((object) ("Failed to process struct field: " + path));
        context.Logger.LogError((object) $"Exception: {ex}");
      }
    }
    else
    {
      object targetObject = FieldModifyHandler.ResolveTargetObject(targetGameObject, componentPath);
      if (targetObject == null)
      {
        context.Logger.LogWarning((object) $"Component not found: {componentPath} on {targetGameObject} for line {context.RawLine}");
      }
      else
      {
        try
        {
          FieldProcessingContext context3 = new FieldProcessingContext(targetObject, targetGameObject, path, componentPath, fieldName, str, match, context.Logger, context.Plugin);
          IFieldProcessor processor;
          if (context.Plugin.FieldProcessorRegistry.TryGetProcessor(operation, out processor))
            processor.Process(context3);
          else
            context.Logger.LogError((object) ("Unknown field modification operation: " + operation));
        }
        catch (Exception ex)
        {
          context.Logger.LogError((object) $"Failed to modify for line '{context.RawLine}': {ex.Message}");
        }
      }
    }
  }

  private static object ResolveTargetObject(GameObject obj, string componentPath)
  {
    string[] parts = componentPath.Split(new char[2]
    {
      '/',
      '.'
    }, StringSplitOptions.RemoveEmptyEntries);
    if (parts.Length == 0)
      return (object) null;
    Component component = obj.GetComponent(parts[0]) ?? ((IEnumerable<Component>) obj.GetComponents<Component>()).FirstOrDefault<Component>((Func<Component, bool>) (c => c.GetType().Name.Equals(parts[0], StringComparison.OrdinalIgnoreCase)));
    if ((UnityEngine.Object) component == (UnityEngine.Object) null)
      return (object) null;
    object obj1 = (object) component;
    for (int i = 1; i < parts.Length; i++)
    {
      FieldInfo fieldInfo1 = obj1.GetType().GetField(parts[i], BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
      if ((object) fieldInfo1 == null)
        fieldInfo1 = ((IEnumerable<FieldInfo>) obj1.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)).FirstOrDefault<FieldInfo>((Func<FieldInfo, bool>) (f => f.Name.Equals(parts[i], StringComparison.OrdinalIgnoreCase)));
      FieldInfo fieldInfo2 = fieldInfo1;
      if (fieldInfo2 == (FieldInfo) null)
        return (object) null;
      obj1 = fieldInfo2.GetValue(obj1);
      if (obj1 == null)
        return (object) null;
    }
    return obj1;
  }
}

// Decompiled with JetBrains decompiler
// Type: qol.FieldModification.Processors.WeaponInfoEditProcessor
// Assembly: qol, Version=1.1.6.3, Culture=neutral, PublicKeyToken=null
// MVID: 3B6539EA-E38C-45EE-8FFF-FC58CC42BADC
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\BepInEx\plugins\qol_1.1.6.3b1.dll

using qol.FieldModification.Configs;
using qol.FieldModification.Core;
using qol.FieldModification.Helpers;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

#nullable disable
namespace qol.FieldModification.Processors;

public class WeaponInfoEditProcessor : IFieldProcessor
{
  public string Operation => "modify4";

  public void Process(FieldProcessingContext context)
  {
    context.Logger.LogDebug((object) (context.ComponentPath + context.FieldName));
    FieldInfo structField1 = FieldHelpers.FindStructField(context.ComponentPath, context.Path);
    if (structField1 == (FieldInfo) null)
    {
      context.Logger.LogError((object) $"Struct field {context.ComponentPath} not found on any definition type");
    }
    else
    {
      foreach (Type definitionType in FieldModificationConfigs.DefinitionTypes)
      {
        UnityEngine.Object target = Enumerable.Cast<UnityEngine.Object>(Resources.FindObjectsOfTypeAll(definitionType)).FirstOrDefault<UnityEngine.Object>((Func<UnityEngine.Object, bool>) (r => r.name == context.Path));
        if (target != (UnityEngine.Object) null)
        {
          FieldInfo fieldInfo = definitionType.GetField(context.ComponentPath, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
          if ((object) fieldInfo == null)
            fieldInfo = structField1;
          FieldInfo structField2 = fieldInfo;
          FieldHelpers.TrySetStructField((object) target, structField2, context.FieldName, context.Value, context.Path, context.Logger);
          break;
        }
      }
    }
  }
}

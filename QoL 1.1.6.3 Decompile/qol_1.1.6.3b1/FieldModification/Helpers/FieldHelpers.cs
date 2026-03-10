// Decompiled with JetBrains decompiler
// Type: qol.FieldModification.Helpers.FieldHelpers
// Assembly: qol, Version=1.1.6.3, Culture=neutral, PublicKeyToken=null
// MVID: 3B6539EA-E38C-45EE-8FFF-FC58CC42BADC
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\BepInEx\plugins\qol_1.1.6.3b1.dll

using BepInEx.Logging;
using qol.FieldModification.Configs;
using qol.Utilities;
using System;
using System.Globalization;
using System.Reflection;

#nullable disable
namespace qol.FieldModification.Helpers;

public static class FieldHelpers
{
  public static bool TrySetField(object target, string fieldName, object value, bool useCache = true)
  {
    FieldInfo field = FieldHelpers.GetField(target.GetType(), fieldName, useCache);
    if (field == (FieldInfo) null)
      return false;
    field.SetValue(target, value);
    return true;
  }

  public static FieldInfo GetField(Type type, string fieldName, bool useCache = true)
  {
    if (type == (Type) null)
      return (FieldInfo) null;
    return !useCache ? type.GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) : ReflectionHelpers.GetField(type, fieldName);
  }

  public static bool TrySetResourceField<T>(object target, string fieldName, string resourceName) where T : UnityEngine.Object
  {
    T resource = ResourceHelpers.FindResource<T>(resourceName);
    return !((UnityEngine.Object) resource == (UnityEngine.Object) null) && FieldHelpers.TrySetField(target, fieldName, (object) resource);
  }

  public static bool TrySetStructField(
    object target,
    FieldInfo structField,
    string fieldName,
    string valueStr,
    string path,
    ManualLogSource logger)
  {
    if (!(structField == (FieldInfo) null))
    {
      if (target != null)
      {
        try
        {
          object obj1 = structField.GetValue(target);
          FieldInfo field = obj1.GetType().GetField(fieldName);
          object obj2 = Convert.ChangeType((object) valueStr, field.FieldType, (IFormatProvider) CultureInfo.InvariantCulture);
          field.SetValue(obj1, obj2);
          structField.SetValue(target, obj1);
          return true;
        }
        catch (Exception ex)
        {
          logger.LogError((object) $"{path} {fieldName} struct modification failed: {ex}");
          return false;
        }
      }
    }
    return false;
  }

  public static FieldInfo FindStructField(string componentPath, string path)
  {
    if (componentPath == "roleIdentity" && (UnityEngine.Object) ResourceHelpers.FindResource<MissileDefinition>(path) != (UnityEngine.Object) null)
      return typeof (MissileDefinition).GetField(componentPath, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
    foreach (Type definitionType in FieldModificationConfigs.DefinitionTypes)
    {
      FieldInfo field = definitionType.GetField(componentPath, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
      if (field != (FieldInfo) null)
        return field;
    }
    return (FieldInfo) null;
  }
}

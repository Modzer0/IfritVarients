// Decompiled with JetBrains decompiler
// Type: qol.Utilities.ReflectionHelpers
// Assembly: qol, Version=1.1.6.3, Culture=neutral, PublicKeyToken=null
// MVID: 3B6539EA-E38C-45EE-8FFF-FC58CC42BADC
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\BepInEx\plugins\qol_1.1.6.3b1.dll

using System;
using System.Collections.Generic;
using System.Reflection;

#nullable disable
namespace qol.Utilities;

public static class ReflectionHelpers
{
  public const BindingFlags AllInstanceFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
  public const BindingFlags AllStaticFlags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
  public const BindingFlags AllFlags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
  private static readonly Dictionary<(Type, string), FieldInfo> _fieldCache = new Dictionary<(Type, string), FieldInfo>();
  private static readonly Dictionary<(Type, string), PropertyInfo> _propertyCache = new Dictionary<(Type, string), PropertyInfo>();

  public static FieldInfo GetField(Type type, string fieldName, BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
  {
    (Type, string) key = (type, fieldName);
    FieldInfo field1;
    if (ReflectionHelpers._fieldCache.TryGetValue(key, out field1))
      return field1;
    FieldInfo field2 = type.GetField(fieldName, flags);
    ReflectionHelpers._fieldCache[key] = field2;
    return field2;
  }

  public static FieldInfo GetField<T>(string fieldName, BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
  {
    return ReflectionHelpers.GetField(typeof (T), fieldName, flags);
  }

  public static T GetFieldValue<T>(object target, string fieldName)
  {
    FieldInfo field = ReflectionHelpers.GetField(target.GetType(), fieldName);
    return !(field != (FieldInfo) null) ? default (T) : (T) field.GetValue(target);
  }

  public static bool SetFieldValue(object target, string fieldName, object value)
  {
    FieldInfo field = ReflectionHelpers.GetField(target.GetType(), fieldName);
    if (!(field != (FieldInfo) null))
      return false;
    field.SetValue(target, value);
    return true;
  }

  public static PropertyInfo GetProperty(Type type, string propertyName, BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
  {
    (Type, string) key = (type, propertyName);
    PropertyInfo property1;
    if (ReflectionHelpers._propertyCache.TryGetValue(key, out property1))
      return property1;
    PropertyInfo property2 = type.GetProperty(propertyName, flags);
    ReflectionHelpers._propertyCache[key] = property2;
    return property2;
  }

  public static PropertyInfo GetProperty<T>(string propertyName, BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
  {
    return ReflectionHelpers.GetProperty(typeof (T), propertyName, flags);
  }

  public static T GetPropertyValue<T>(object target, string propertyName)
  {
    PropertyInfo property = ReflectionHelpers.GetProperty(target.GetType(), propertyName);
    return !(property != (PropertyInfo) null) ? default (T) : (T) property.GetValue(target);
  }

  public static bool SetPropertyValue(object target, string propertyName, object value)
  {
    PropertyInfo property = ReflectionHelpers.GetProperty(target.GetType(), propertyName);
    if (!(property != (PropertyInfo) null) || !property.CanWrite)
      return false;
    property.SetValue(target, value);
    return true;
  }

  public static void ClearCaches()
  {
    ReflectionHelpers._fieldCache.Clear();
    ReflectionHelpers._propertyCache.Clear();
  }
}

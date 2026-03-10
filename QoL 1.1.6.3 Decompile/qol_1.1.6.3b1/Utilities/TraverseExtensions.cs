// Decompiled with JetBrains decompiler
// Type: qol.Utilities.TraverseExtensions
// Assembly: qol, Version=1.1.6.3, Culture=neutral, PublicKeyToken=null
// MVID: 3B6539EA-E38C-45EE-8FFF-FC58CC42BADC
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\BepInEx\plugins\qol_1.1.6.3b1.dll

using HarmonyLib;

#nullable disable
namespace qol.Utilities;

public static class TraverseExtensions
{
  public static Traverse SetField(this Traverse traverse, string fieldName, object value)
  {
    traverse.Field(fieldName).SetValue(value);
    return traverse;
  }

  public static T GetField<T>(this Traverse traverse, string fieldName)
  {
    return traverse.Field(fieldName).GetValue<T>();
  }

  public static Traverse SetProperty(this Traverse traverse, string propertyName, object value)
  {
    traverse.Property(propertyName, (object[]) null).SetValue(value);
    return traverse;
  }

  public static T GetProperty<T>(this Traverse traverse, string propertyName)
  {
    return traverse.Property(propertyName, (object[]) null).GetValue<T>();
  }

  public static Traverse SetFields(
    object target,
    params (string fieldName, object value)[] fieldValues)
  {
    Traverse traverse = Traverse.Create(target);
    foreach ((string fieldName, object value) in fieldValues)
      traverse.Field(fieldName).SetValue(value);
    return traverse;
  }

  public static Traverse Trav(this object obj) => Traverse.Create(obj);
}

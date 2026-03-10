// Decompiled with JetBrains decompiler
// Type: qol.FieldModification.Processors.ColorProcessor
// Assembly: qol, Version=1.1.6.3, Culture=neutral, PublicKeyToken=null
// MVID: 3B6539EA-E38C-45EE-8FFF-FC58CC42BADC
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\BepInEx\plugins\qol_1.1.6.3b1.dll

using qol.FieldModification.Core;
using qol.FieldModification.Helpers;
using System;
using System.Globalization;
using System.Reflection;
using UnityEngine;

#nullable disable
namespace qol.FieldModification.Processors;

public class ColorProcessor : IFieldProcessor
{
  public string Operation => "color";

  public void Process(FieldProcessingContext context)
  {
    string[] strArray = context.Value.Split(',', StringSplitOptions.None);
    if (strArray.Length != 4)
    {
      context.Logger.LogError((object) "Color requires exactly 4 values (R,G,B,A)");
    }
    else
    {
      Color color = new Color(float.Parse(strArray[0], (IFormatProvider) CultureInfo.InvariantCulture), float.Parse(strArray[1], (IFormatProvider) CultureInfo.InvariantCulture), float.Parse(strArray[2], (IFormatProvider) CultureInfo.InvariantCulture), float.Parse(strArray[3], (IFormatProvider) CultureInfo.InvariantCulture));
      FieldInfo field = FieldHelpers.GetField(context.TargetObject.GetType(), context.FieldName);
      if (field != (FieldInfo) null)
      {
        field.SetValue(context.TargetObject, (object) color);
      }
      else
      {
        PropertyInfo property = context.TargetObject.GetType().GetProperty(context.FieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (property != (PropertyInfo) null)
          property.SetValue(context.TargetObject, (object) color);
        else
          context.Logger.LogError((object) $"Color field/property {context.FieldName} not found");
      }
    }
  }
}

// Decompiled with JetBrains decompiler
// Type: qol.FieldModification.Processors.VectorProcessor
// Assembly: qol, Version=1.1.6.3, Culture=neutral, PublicKeyToken=null
// MVID: 3B6539EA-E38C-45EE-8FFF-FC58CC42BADC
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\BepInEx\plugins\qol_1.1.6.3b1.dll

using qol.FieldModification.Core;
using qol.FieldModification.Helpers;
using System;
using System.Reflection;
using UnityEngine;

#nullable disable
namespace qol.FieldModification.Processors;

public class VectorProcessor : IFieldProcessor
{
  public string Operation => "modifyvector";

  public void Process(FieldProcessingContext context)
  {
    string[] strArray = context.Value.Split(',', StringSplitOptions.None);
    if (strArray.Length != 3)
    {
      context.Logger.LogError((object) $"Invalid Vector3 format. Expected 'x,y,z' but got '{context.Value}'");
    }
    else
    {
      float result1;
      float result2;
      float result3;
      if (!float.TryParse(strArray[0], out result1) || !float.TryParse(strArray[1], out result2) || !float.TryParse(strArray[2], out result3))
      {
        context.Logger.LogError((object) $"Failed to parse Vector3 components from '{context.Value}'");
      }
      else
      {
        FieldInfo field = FieldHelpers.GetField(context.TargetObject.GetType(), context.FieldName);
        if (field == (FieldInfo) null)
          return;
        if (field.FieldType != typeof (Vector3))
          context.Logger.LogError((object) $"Field {context.FieldName} is not of type Vector3");
        else
          field.SetValue(context.TargetObject, (object) new Vector3(result1, result2, result3));
      }
    }
  }
}

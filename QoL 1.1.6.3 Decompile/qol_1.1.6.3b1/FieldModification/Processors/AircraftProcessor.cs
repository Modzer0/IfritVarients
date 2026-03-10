// Decompiled with JetBrains decompiler
// Type: qol.FieldModification.Processors.AircraftProcessor
// Assembly: qol, Version=1.1.6.3, Culture=neutral, PublicKeyToken=null
// MVID: 3B6539EA-E38C-45EE-8FFF-FC58CC42BADC
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\BepInEx\plugins\qol_1.1.6.3b1.dll

using qol.FieldModification.Core;
using qol.FieldModification.Helpers;
using System;
using System.Reflection;
using System.Text.RegularExpressions;

#nullable disable
namespace qol.FieldModification.Processors;

public class AircraftProcessor : IFieldProcessor
{
  private static readonly Regex FieldPattern = new Regex("^(?<field>\\w+)(?:\\[(?<index>\\d+)\\])?(?:\\.(?<subfield>\\w+))?$", RegexOptions.Compiled);

  public string Operation => "modifyaircraft";

  public void Process(FieldProcessingContext context)
  {
    Aircraft resource1 = ResourceHelpers.FindResource<Aircraft>(context.Value);
    AircraftDefinition resource2 = ResourceHelpers.FindResource<AircraftDefinition>(context.Value);
    if ((UnityEngine.Object) resource1 == (UnityEngine.Object) null && (UnityEngine.Object) resource2 == (UnityEngine.Object) null)
    {
      context.Logger.LogError((object) $"Aircraft or definition {context.Value} not found");
    }
    else
    {
      Match match = AircraftProcessor.FieldPattern.Match(context.FieldName);
      string fieldName1 = match.Groups["field"].Value;
      int result;
      bool flag = int.TryParse(match.Groups["index"].Value, out result);
      string fieldName2 = match.Groups["subfield"].Value;
      FieldInfo field1 = FieldHelpers.GetField(context.TargetObject.GetType(), fieldName1);
      if (field1 == (FieldInfo) null)
        context.Logger.LogError((object) $"Field {fieldName1} not found");
      else if (!flag)
      {
        field1.SetValue(context.TargetObject, (object) resource1);
      }
      else
      {
        object obj1 = field1.GetValue(context.TargetObject);
        if (obj1 == null)
          context.Logger.LogError((object) $"Array {fieldName1} is null");
        else if (!(obj1 is Array sourceArray))
        {
          context.Logger.LogError((object) (fieldName1 + " is not an array"));
        }
        else
        {
          if (result >= sourceArray.Length)
          {
            Array instance = Array.CreateInstance(sourceArray.GetType().GetElementType(), result + 1);
            Array.Copy(sourceArray, instance, sourceArray.Length);
            field1.SetValue(context.TargetObject, (object) instance);
            sourceArray = instance;
          }
          object obj2 = sourceArray.GetValue(result);
          if (!string.IsNullOrEmpty(fieldName2))
          {
            FieldInfo field2 = FieldHelpers.GetField(obj2?.GetType(), fieldName2);
            if (field2 == (FieldInfo) null)
              context.Logger.LogError((object) $"Subfield {fieldName2} not found in {obj2?.GetType().Name}");
            else
              field2.SetValue(obj2, (object) resource2);
          }
          else
            sourceArray.SetValue((object) resource2, result);
        }
      }
    }
  }
}

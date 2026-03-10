// Decompiled with JetBrains decompiler
// Type: qol.FieldModification.Processors.StandardProcessor
// Assembly: qol, Version=1.1.6.3, Culture=neutral, PublicKeyToken=null
// MVID: 3B6539EA-E38C-45EE-8FFF-FC58CC42BADC
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\BepInEx\plugins\qol_1.1.6.3b1.dll

using qol.FieldModification.Core;
using qol.FieldModification.Helpers;
using System;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;

#nullable disable
namespace qol.FieldModification.Processors;

public class StandardProcessor : IFieldProcessor
{
  private static readonly Regex FieldPattern = new Regex("^(?<baseField>\\w+)(?:\\[(?<index>\\d+)\\])?(?:\\.(?<subField>\\w+))?$", RegexOptions.Compiled);

  public string Operation => "modify";

  public void Process(FieldProcessingContext context)
  {
    Match match = StandardProcessor.FieldPattern.Match(context.FieldName);
    string fieldName1 = match.Groups["baseField"].Value;
    int result;
    bool flag = int.TryParse(match.Groups["index"].Value, out result);
    string fieldName2 = match.Groups["subField"].Value;
    FieldInfo fieldInfo = FieldHelpers.GetField(context.TargetObject.GetType(), fieldName1);
    if (fieldInfo == (FieldInfo) null)
    {
      context.Logger.LogError((object) $"Field {fieldName1} not found in {context.TargetObject.GetType().Name}");
    }
    else
    {
      object targetObject = context.TargetObject;
      if (flag)
      {
        object obj = fieldInfo.GetValue(context.TargetObject);
        if (obj == null)
        {
          context.Logger.LogError((object) $"Array {fieldName1} is null");
          return;
        }
        if (!(obj is Array sourceArray))
        {
          context.Logger.LogError((object) (fieldName1 + " is not an array"));
          return;
        }
        if (result >= sourceArray.Length)
        {
          Array instance = Array.CreateInstance(sourceArray.GetType().GetElementType(), result + 1);
          Array.Copy(sourceArray, instance, sourceArray.Length);
          fieldInfo.SetValue(context.TargetObject, (object) instance);
          sourceArray = instance;
        }
        targetObject = sourceArray.GetValue(result);
        if (!string.IsNullOrEmpty(fieldName2))
        {
          FieldInfo field = FieldHelpers.GetField(targetObject.GetType(), fieldName2);
          if (field == (FieldInfo) null)
          {
            context.Logger.LogError((object) $"Subfield {fieldName2} not found in {targetObject.GetType().Name}");
            return;
          }
          fieldInfo = field;
        }
      }
      try
      {
        object obj = fieldInfo.FieldType.IsEnum ? Enum.Parse(fieldInfo.FieldType, context.Value, true) : Convert.ChangeType((object) context.Value, fieldInfo.FieldType, (IFormatProvider) CultureInfo.InvariantCulture);
        if (context.TargetObject.GetType() == typeof (AircraftDefinition) && context.FieldName == "unitName")
          QOLPlugin.newAircraftName[((UnitDefinition) context.TargetObject).unitName] = (string) obj;
        fieldInfo.SetValue(targetObject, obj);
      }
      catch (Exception ex)
      {
        context.Logger.LogError((object) $"Failed to set {context.FieldName}: {ex.Message}");
      }
    }
  }
}

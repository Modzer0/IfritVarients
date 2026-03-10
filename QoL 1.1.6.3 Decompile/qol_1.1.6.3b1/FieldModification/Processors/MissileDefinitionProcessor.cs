// Decompiled with JetBrains decompiler
// Type: qol.FieldModification.Processors.MissileDefinitionProcessor
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

public class MissileDefinitionProcessor : IFieldProcessor
{
  public string Operation => "modify2";

  public void Process(FieldProcessingContext context)
  {
    FieldInfo field = FieldHelpers.GetField(context.TargetObject.GetType(), context.FieldName);
    if (field == (FieldInfo) null)
    {
      context.Logger.LogWarning((object) $"Field {context.FieldName} not found on {context.TargetObject.GetType().Name}");
    }
    else
    {
      Type fieldType = field.FieldType;
      if (fieldType == typeof (MissileDefinition))
        field.SetValue(context.TargetObject, (object) ResourceHelpers.FindResource<MissileDefinition>(context.Value));
      else if (fieldType == typeof (Missile))
        field.SetValue(context.TargetObject, (object) ResourceHelpers.FindResource<Missile>(context.Value));
      else if (fieldType == typeof (Unit))
      {
        Unit unit = ResourceHelpers.FindResource<Unit>(context.Value) ?? (Unit) ResourceHelpers.FindResource<GroundVehicle>(context.Value);
        field.SetValue(context.TargetObject, (object) unit);
      }
      else if (fieldType == typeof (Transform))
      {
        GameObject resource = ResourceHelpers.FindResource<GameObject>(context.Value);
        field.SetValue(context.TargetObject, (object) resource?.transform);
      }
      else if (fieldType == typeof (UnitPart))
      {
        GameObject resource = ResourceHelpers.FindResource<GameObject>(context.Value);
        field.SetValue(context.TargetObject, (object) resource?.GetComponent<UnitPart>());
      }
      else
        context.Logger.LogWarning((object) $"MissileDefinitionProcessor: unsupported field type {fieldType.Name} for {context.TargetObject}.{context.FieldName}");
    }
  }
}

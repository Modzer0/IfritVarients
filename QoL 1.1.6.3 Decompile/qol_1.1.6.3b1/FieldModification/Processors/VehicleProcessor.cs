// Decompiled with JetBrains decompiler
// Type: qol.FieldModification.Processors.VehicleProcessor
// Assembly: qol, Version=1.1.6.3, Culture=neutral, PublicKeyToken=null
// MVID: 3B6539EA-E38C-45EE-8FFF-FC58CC42BADC
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\BepInEx\plugins\qol_1.1.6.3b1.dll

using qol.FieldModification.Core;
using qol.FieldModification.Helpers;
using UnityEngine;

#nullable disable
namespace qol.FieldModification.Processors;

public class VehicleProcessor : IFieldProcessor
{
  public string Operation => "modifyvehicle";

  public void Process(FieldProcessingContext context)
  {
    if (context.Value.StartsWith("Unit_"))
    {
      string name = context.Value.Substring(5);
      context.Logger.LogDebug((object) $"Transferred unit type tag to {name} while changing {context.FieldName}");
      GroundVehicle resource = ResourceHelpers.FindResource<GroundVehicle>(name);
      FieldHelpers.TrySetField(context.TargetObject, context.FieldName, (object) resource);
    }
    else
    {
      VehicleDefinition resource = ResourceHelpers.FindResource<VehicleDefinition>(context.Value);
      if ((Object) resource == (Object) null)
        context.Logger.LogError((object) $"VehicleDefinition {context.Value} not found");
      else
        FieldHelpers.TrySetField(context.TargetObject, context.FieldName, (object) resource);
    }
  }
}

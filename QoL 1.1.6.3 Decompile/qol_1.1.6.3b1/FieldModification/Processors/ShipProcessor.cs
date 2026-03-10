// Decompiled with JetBrains decompiler
// Type: qol.FieldModification.Processors.ShipProcessor
// Assembly: qol, Version=1.1.6.3, Culture=neutral, PublicKeyToken=null
// MVID: 3B6539EA-E38C-45EE-8FFF-FC58CC42BADC
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\BepInEx\plugins\qol_1.1.6.3b1.dll

using qol.FieldModification.Core;
using qol.FieldModification.Helpers;
using UnityEngine;

#nullable disable
namespace qol.FieldModification.Processors;

public class ShipProcessor : IFieldProcessor
{
  public string Operation => "modifyship";

  public void Process(FieldProcessingContext context)
  {
    Ship resource = ResourceHelpers.FindResource<Ship>(context.Value);
    if ((Object) resource == (Object) null)
      context.Logger.LogError((object) $"Ship {context.Value} not found");
    else
      FieldHelpers.TrySetField(context.TargetObject, context.FieldName, (object) resource);
  }
}

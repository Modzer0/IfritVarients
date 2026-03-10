// Decompiled with JetBrains decompiler
// Type: qol.FieldModification.Processors.ObjectProcessor
// Assembly: qol, Version=1.1.6.3, Culture=neutral, PublicKeyToken=null
// MVID: 3B6539EA-E38C-45EE-8FFF-FC58CC42BADC
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\BepInEx\plugins\qol_1.1.6.3b1.dll

using qol.FieldModification.Core;
using qol.FieldModification.Helpers;
using UnityEngine;

#nullable disable
namespace qol.FieldModification.Processors;

public class ObjectProcessor : IFieldProcessor
{
  public string Operation => "modifyobj";

  public void Process(FieldProcessingContext context)
  {
    GameObject resource = ResourceHelpers.FindResource<GameObject>(context.Value);
    if ((Object) resource == (Object) null && context.Value != "null")
      context.Logger.LogError((object) $"GameObject {context.Value} not found");
    else
      FieldHelpers.TrySetField(context.TargetObject, context.FieldName, (object) resource);
  }
}

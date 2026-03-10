// Decompiled with JetBrains decompiler
// Type: qol.FieldModification.Processors.ActiveProcessor
// Assembly: qol, Version=1.1.6.3, Culture=neutral, PublicKeyToken=null
// MVID: 3B6539EA-E38C-45EE-8FFF-FC58CC42BADC
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\BepInEx\plugins\qol_1.1.6.3b1.dll

using qol.FieldModification.Core;
using UnityEngine;

#nullable disable
namespace qol.FieldModification.Processors;

public class ActiveProcessor : IFieldProcessor
{
  public string Operation => "modifyactive";

  public void Process(FieldProcessingContext context)
  {
    if ((Object) context.TargetGameObject == (Object) null)
    {
      context.Logger.LogError((object) "ActiveProcessor requires a valid TargetGameObject");
    }
    else
    {
      bool result;
      if (bool.TryParse(context.Value, out result))
        context.TargetGameObject.SetActive(result);
      else
        context.Logger.LogError((object) ("Invalid boolean value: " + context.Value));
    }
  }
}

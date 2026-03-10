// Decompiled with JetBrains decompiler
// Type: NuclearOption.SavedMission.ObjectiveV2.LoadErrors
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;
using UnityEngine;

#nullable disable
namespace NuclearOption.SavedMission.ObjectiveV2;

public class LoadErrors
{
  public readonly List<string> Warnings = new List<string>();
  public readonly List<Exception> Exceptions = new List<Exception>();

  public void AddExceptions(List<Exception> errors)
  {
    this.Exceptions.AddRange((IEnumerable<Exception>) errors);
  }

  public void Warn(string message) => this.Warnings.Add(message);

  public void LogAllErrors()
  {
    if (this.Warnings.Count > 0)
    {
      Debug.Log((object) $"{this.Warnings.Count} load warnings");
      foreach (object warning in this.Warnings)
        Debug.Log(warning);
    }
    if (this.Exceptions.Count <= 0)
      return;
    Debug.Log((object) $"{this.Exceptions.Count} load errors");
    foreach (object exception in this.Exceptions)
      Debug.Log(exception);
  }
}

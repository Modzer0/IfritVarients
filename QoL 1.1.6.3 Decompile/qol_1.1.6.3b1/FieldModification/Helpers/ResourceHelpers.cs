// Decompiled with JetBrains decompiler
// Type: qol.FieldModification.Helpers.ResourceHelpers
// Assembly: qol, Version=1.1.6.3, Culture=neutral, PublicKeyToken=null
// MVID: 3B6539EA-E38C-45EE-8FFF-FC58CC42BADC
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\BepInEx\plugins\qol_1.1.6.3b1.dll

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#nullable disable
namespace qol.FieldModification.Helpers;

public static class ResourceHelpers
{
  public static T FindResource<T>(string name) where T : UnityEngine.Object
  {
    return ((IEnumerable<T>) Resources.FindObjectsOfTypeAll<T>()).FirstOrDefault<T>((Func<T, bool>) (r => r.name.Equals(name, StringComparison.OrdinalIgnoreCase)));
  }
}

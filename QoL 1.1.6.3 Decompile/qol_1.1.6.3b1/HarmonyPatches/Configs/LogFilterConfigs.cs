// Decompiled with JetBrains decompiler
// Type: qol.HarmonyPatches.Configs.LogFilterConfigs
// Assembly: qol, Version=1.1.6.3, Culture=neutral, PublicKeyToken=null
// MVID: 3B6539EA-E38C-45EE-8FFF-FC58CC42BADC
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\BepInEx\plugins\qol_1.1.6.3b1.dll

using System.Collections.Generic;

#nullable disable
namespace qol.HarmonyPatches.Configs;

public static class LogFilterConfigs
{
  public static readonly IReadOnlyList<string> SuppressedWarnings = (IReadOnlyList<string>) new List<string>()
  {
    "BoxColliders does not support negative scale",
    "Setting linear velocity of a kinematic body"
  };
  public static readonly IReadOnlyList<string> SuppressedErrors = (IReadOnlyList<string>) new List<string>()
  {
    "_MainTex"
  };

  public static bool ShouldSuppressWarning(string message)
  {
    foreach (string suppressedWarning in (IEnumerable<string>) LogFilterConfigs.SuppressedWarnings)
    {
      if (message.Contains(suppressedWarning))
        return true;
    }
    return false;
  }

  public static bool ShouldSuppressError(string message)
  {
    foreach (string suppressedError in (IEnumerable<string>) LogFilterConfigs.SuppressedErrors)
    {
      if (message.Contains(suppressedError))
        return true;
    }
    return false;
  }
}

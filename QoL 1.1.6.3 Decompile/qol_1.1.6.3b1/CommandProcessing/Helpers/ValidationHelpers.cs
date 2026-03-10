// Decompiled with JetBrains decompiler
// Type: qol.CommandProcessing.Helpers.ValidationHelpers
// Assembly: qol, Version=1.1.6.3, Culture=neutral, PublicKeyToken=null
// MVID: 3B6539EA-E38C-45EE-8FFF-FC58CC42BADC
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\BepInEx\plugins\qol_1.1.6.3b1.dll

using BepInEx.Logging;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

#nullable disable
namespace qol.CommandProcessing.Helpers;

public static class ValidationHelpers
{
  public static bool ValidateArrayIndex<T>(
    int index,
    T[] array,
    ManualLogSource logger,
    string context)
  {
    if (array == null)
    {
      logger.LogError((object) (context + ": Array is null"));
      return false;
    }
    if (index >= 0 && index < array.Length)
      return true;
    logger.LogError((object) $"{context}: Index {index} out of range (0-{array.Length - 1})");
    return false;
  }

  public static bool ValidateListIndex<T>(
    int index,
    IList<T> list,
    ManualLogSource logger,
    string context)
  {
    if (list == null)
    {
      logger.LogError((object) (context + ": List is null"));
      return false;
    }
    if (index >= 0 && index < list.Count)
      return true;
    logger.LogError((object) $"{context}: Index {index} out of range (0-{list.Count - 1})");
    return false;
  }

  public static bool ValidateIListIndex(
    int index,
    IList list,
    ManualLogSource logger,
    string context)
  {
    if (list == null)
    {
      logger.LogError((object) (context + ": List is null"));
      return false;
    }
    if (index >= 0 && index < list.Count)
      return true;
    logger.LogError((object) $"{context}: Index {index} out of range (0-{list.Count - 1})");
    return false;
  }

  public static bool TryParseIndex(Match match, string groupName, out int result)
  {
    result = 0;
    return match.Groups[groupName].Success && int.TryParse(match.Groups[groupName].Value, out result);
  }

  public static bool TryParseIndexWithLogging(
    Match match,
    string groupName,
    out int result,
    ManualLogSource logger,
    string context)
  {
    result = 0;
    if (!match.Groups[groupName].Success)
    {
      logger.LogError((object) $"{context}: Missing required group '{groupName}'");
      return false;
    }
    if (int.TryParse(match.Groups[groupName].Value, out result))
      return true;
    logger.LogError((object) $"{context}: Invalid index format in group '{groupName}': {match.Groups[groupName].Value}");
    return false;
  }
}

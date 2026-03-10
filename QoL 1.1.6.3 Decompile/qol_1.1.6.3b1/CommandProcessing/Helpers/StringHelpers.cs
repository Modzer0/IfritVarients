// Decompiled with JetBrains decompiler
// Type: qol.CommandProcessing.Helpers.StringHelpers
// Assembly: qol, Version=1.1.6.3, Culture=neutral, PublicKeyToken=null
// MVID: 3B6539EA-E38C-45EE-8FFF-FC58CC42BADC
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\BepInEx\plugins\qol_1.1.6.3b1.dll

#nullable disable
namespace qol.CommandProcessing.Helpers;

public static class StringHelpers
{
  public static string StripQuotes(string value)
  {
    return string.IsNullOrEmpty(value) || value.Length < 2 || !value.StartsWith("\"") || !value.EndsWith("\"") ? value : value.Substring(1, value.Length - 2);
  }

  public static string CleanValue(string value)
  {
    return string.IsNullOrEmpty(value) ? value : StringHelpers.StripQuotes(value.Trim());
  }

  public static string StripSingleQuotes(string value)
  {
    return string.IsNullOrEmpty(value) || value.Length < 2 || !value.StartsWith("'") || !value.EndsWith("'") ? value : value.Substring(1, value.Length - 2);
  }

  public static string StripAnyQuotes(string value)
  {
    if (string.IsNullOrEmpty(value))
      return value;
    value = StringHelpers.StripQuotes(value);
    value = StringHelpers.StripSingleQuotes(value);
    return value;
  }
}

// Decompiled with JetBrains decompiler
// Type: NuclearOption.MissionEditorScripts.EnumNames
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Steamworks;
using System;
using System.Text.RegularExpressions;

#nullable disable
namespace NuclearOption.MissionEditorScripts;

public static class EnumNames
{
  private static Regex pattern = new Regex("(\\B[A-Z])");
  private static readonly int eResultStartIndex = "k_EResult".Length;

  public static string Nicify(string name) => EnumNames.pattern.Replace(name, " $1");

  public static string ToNicifyString<T>(this T value) where T : struct, Enum
  {
    return EnumNames.Nicify(value.ToString());
  }

  public static string Nicify(this EResult eResult)
  {
    return EnumNames.Nicify(eResult.ToString().Substring(EnumNames.eResultStartIndex));
  }
}

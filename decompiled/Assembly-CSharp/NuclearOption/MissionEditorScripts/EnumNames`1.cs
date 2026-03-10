// Decompiled with JetBrains decompiler
// Type: NuclearOption.MissionEditorScripts.EnumNames`1
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;
using System.Linq;

#nullable disable
namespace NuclearOption.MissionEditorScripts;

public static class EnumNames<T> where T : struct, Enum
{
  private static List<string> realNames;
  private static List<string> names;

  public static T Parse(string name)
  {
    int index = EnumNames<T>.names.IndexOf(name);
    return Enum.Parse<T>(EnumNames<T>.realNames[index]);
  }

  public static List<string> GetNames()
  {
    if (EnumNames<T>.names == null)
    {
      EnumNames<T>.realNames = new List<string>((IEnumerable<string>) Enum.GetNames(typeof (T)));
      EnumNames<T>.names = new List<string>(EnumNames<T>.realNames.Select<string, string>(new Func<string, string>(EnumNames.Nicify)));
    }
    return EnumNames<T>.names;
  }
}

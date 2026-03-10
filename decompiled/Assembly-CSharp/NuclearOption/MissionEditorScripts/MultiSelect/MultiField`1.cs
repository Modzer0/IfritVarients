// Decompiled with JetBrains decompiler
// Type: NuclearOption.MissionEditorScripts.MultiSelect.MultiField`1
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;

#nullable disable
namespace NuclearOption.MissionEditorScripts.MultiSelect;

public struct MultiField<T>(Func<T> get, Action<T> set)
{
  public readonly Func<T> Get = get;
  public readonly Action<T> Set = set;
}

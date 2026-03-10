// Decompiled with JetBrains decompiler
// Type: StringOptionsAttribute
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

#nullable disable
public class StringOptionsAttribute : PropertyAttribute
{
  public string[] Options { get; private set; }

  public StringOptionsAttribute(params string[] options) => this.Options = options;
}

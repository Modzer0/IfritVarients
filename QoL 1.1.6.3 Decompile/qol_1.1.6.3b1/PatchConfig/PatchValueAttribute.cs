// Decompiled with JetBrains decompiler
// Type: qol.PatchConfig.PatchValueAttribute
// Assembly: qol, Version=1.1.6.3, Culture=neutral, PublicKeyToken=null
// MVID: 3B6539EA-E38C-45EE-8FFF-FC58CC42BADC
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\BepInEx\plugins\qol_1.1.6.3b1.dll

using System;

#nullable disable
namespace qol.PatchConfig;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
public class PatchValueAttribute : Attribute
{
  public string Key { get; }

  public string Description { get; }

  public PatchValueAttribute(string key, string description = "")
  {
    this.Key = key;
    this.Description = description;
  }
}

// Decompiled with JetBrains decompiler
// Type: qol.PatchConfig.PatchConfigAttribute
// Assembly: qol, Version=1.1.6.3, Culture=neutral, PublicKeyToken=null
// MVID: 3B6539EA-E38C-45EE-8FFF-FC58CC42BADC
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\BepInEx\plugins\qol_1.1.6.3b1.dll

using System;

#nullable disable
namespace qol.PatchConfig;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class PatchConfigAttribute : Attribute
{
  public string Category { get; }

  public string Key { get; }

  public bool DefaultEnabled { get; }

  public string Description { get; }

  public PatchConfigAttribute(
    string category,
    string key,
    bool defaultEnabled,
    string description)
  {
    this.Category = category;
    this.Key = key;
    this.DefaultEnabled = defaultEnabled;
    this.Description = description;
  }
}

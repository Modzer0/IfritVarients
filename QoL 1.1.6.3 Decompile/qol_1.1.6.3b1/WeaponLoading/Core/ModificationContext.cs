// Decompiled with JetBrains decompiler
// Type: qol.WeaponLoading.Core.ModificationContext
// Assembly: qol, Version=1.1.6.3, Culture=neutral, PublicKeyToken=null
// MVID: 3B6539EA-E38C-45EE-8FFF-FC58CC42BADC
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\BepInEx\plugins\qol_1.1.6.3b1.dll

using BepInEx.Logging;
using System.Reflection;
using UnityEngine;

#nullable disable
namespace qol.WeaponLoading.Core;

public class ModificationContext
{
  private Shader _litShader;

  public Encyclopedia Encyclopedia { get; }

  public ModificationRegistry Registry { get; }

  public ManualLogSource Logger { get; }

  public Assembly ExecutingAssembly { get; }

  public Shader LitShader
  {
    get
    {
      if ((Object) this._litShader == (Object) null)
        this._litShader = Shader.Find("Universal Render Pipeline/Lit");
      return this._litShader;
    }
  }

  public ModificationContext(
    Encyclopedia encyclopedia,
    ModificationRegistry registry,
    ManualLogSource logger)
  {
    this.Encyclopedia = encyclopedia;
    this.Registry = registry;
    this.Logger = logger;
    this.ExecutingAssembly = Assembly.GetExecutingAssembly();
  }
}

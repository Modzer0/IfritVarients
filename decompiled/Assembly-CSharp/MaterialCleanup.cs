// Decompiled with JetBrains decompiler
// Type: MaterialCleanup
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System.Collections.Generic;
using UnityEngine;

#nullable disable
public class MaterialCleanup
{
  private readonly HashSet<Material> needCleanup = new HashSet<Material>();

  public void Add(Material material) => this.needCleanup.Add(material);

  public void CleanupAll()
  {
    foreach (Material material in this.needCleanup)
    {
      if ((Object) material != (Object) null)
        Object.Destroy((Object) material);
    }
    this.needCleanup.Clear();
  }
}

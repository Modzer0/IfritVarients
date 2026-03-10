// Decompiled with JetBrains decompiler
// Type: DebrisManager
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System.Collections.Generic;
using UnityEngine;

#nullable disable
public static class DebrisManager
{
  private static int maxDebris = 100;
  private static List<GameObject> debrisObjects = new List<GameObject>();

  public static void RegisterDebris(GameObject debrisObject)
  {
    DebrisManager.debrisObjects.Add(debrisObject);
    if (DebrisManager.debrisObjects.Count <= DebrisManager.maxDebris)
      return;
    NetworkSceneSingleton<Spawner>.i.DestroyLocal(DebrisManager.debrisObjects[0], 0.0f);
    DebrisManager.debrisObjects.RemoveAt(0);
  }
}

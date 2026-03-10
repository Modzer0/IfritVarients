// Decompiled with JetBrains decompiler
// Type: LayerHelper
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

#nullable disable
public static class LayerHelper
{
  public static void SetLayerRecursively(GameObject target, Component layerObject)
  {
    LayerHelper.SetLayerRecursively(target, layerObject.gameObject.layer);
  }

  public static void SetLayerRecursively(GameObject target, GameObject layerObject)
  {
    LayerHelper.SetLayerRecursively(target, layerObject.layer);
  }

  public static void SetLayerRecursively(GameObject target, int layer)
  {
    foreach (Component componentsInChild in target.GetComponentsInChildren<Transform>())
      componentsInChild.gameObject.layer = layer;
  }
}

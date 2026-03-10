// Decompiled with JetBrains decompiler
// Type: TerrainColorPicker
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

#nullable disable
public class TerrainColorPicker : MonoBehaviour
{
  [SerializeField]
  private Color defaultColor;
  [SerializeField]
  private Renderer[] renderers;
  [SerializeField]
  private int[] submeshIndices;

  private void Start()
  {
    Color colorFromRaycast = this.GetColorFromRaycast();
    for (int index = 0; index < this.renderers.Length; ++index)
    {
      int submeshIndex = this.submeshIndices[index];
      this.renderers[index].materials[submeshIndex].color = colorFromRaycast;
    }
    Object.Destroy((Object) this, 5f);
  }

  private Color GetColorFromRaycast()
  {
    int layer = this.gameObject.layer;
    Color colorFromRaycast = this.defaultColor;
    this.gameObject.layer = 2;
    Color sampledColor;
    if ((Object) NetworkSceneSingleton<LevelInfo>.i != (Object) null && NetworkSceneSingleton<LevelInfo>.i.TryGetTerrainColorAtCoordinate(this.transform.GlobalPosition(), out sampledColor))
      colorFromRaycast = sampledColor;
    this.gameObject.layer = layer;
    return colorFromRaycast;
  }
}

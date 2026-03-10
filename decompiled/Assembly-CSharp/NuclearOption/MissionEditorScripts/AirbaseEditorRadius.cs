// Decompiled with JetBrains decompiler
// Type: NuclearOption.MissionEditorScripts.AirbaseEditorRadius
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

#nullable disable
namespace NuclearOption.MissionEditorScripts;

public class AirbaseEditorRadius : MonoBehaviour
{
  private static readonly List<AirbaseEditorRadius> cache = new List<AirbaseEditorRadius>();
  [SerializeField]
  private DecalProjector projector;
  [SerializeField]
  private float colorAlpha = 0.5f;
  private Material material;

  private void Awake()
  {
    this.material = Object.Instantiate<Material>(this.projector.material);
    this.projector.material = this.material;
  }

  private void OnDestroy()
  {
    if (!((Object) this.material != (Object) null))
      return;
    Object.Destroy((Object) this.material);
  }

  public void Setup(Color color, float radius)
  {
    this.projector.size = this.projector.size with
    {
      x = radius * 2f,
      y = radius * 2f
    };
    color.a = this.colorAlpha;
    this.material.SetColor("_Color", color);
  }

  public static AirbaseEditorRadius Create(Transform parent, GameObject prefab)
  {
    return Object.Instantiate<GameObject>(prefab, parent).GetComponent<AirbaseEditorRadius>();
  }

  public static AirbaseEditorRadius Find(Transform directParent)
  {
    AirbaseEditorRadius.cache.Clear();
    directParent.GetComponentsInChildren<AirbaseEditorRadius>(AirbaseEditorRadius.cache);
    foreach (AirbaseEditorRadius airbaseEditorRadius in AirbaseEditorRadius.cache)
    {
      if ((Object) airbaseEditorRadius.transform.parent == (Object) directParent)
        return airbaseEditorRadius;
    }
    return (AirbaseEditorRadius) null;
  }
}

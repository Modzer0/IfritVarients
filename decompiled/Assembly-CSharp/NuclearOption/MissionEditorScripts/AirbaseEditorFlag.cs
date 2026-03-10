// Decompiled with JetBrains decompiler
// Type: NuclearOption.MissionEditorScripts.AirbaseEditorFlag
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System.Collections.Generic;
using UnityEngine;

#nullable disable
namespace NuclearOption.MissionEditorScripts;

public class AirbaseEditorFlag : MonoBehaviour
{
  private static readonly List<AirbaseEditorFlag> cache = new List<AirbaseEditorFlag>();
  private Material material;

  public void SetColor(Color color) => this.material.SetColor("_Color", color);

  private void OnDestroy() => Object.Destroy((Object) this.material);

  public static AirbaseEditorFlag Create(
    Transform parent,
    GameObject prefab,
    Color color,
    float extraScale)
  {
    Vector3 vector3 = Vector3.one * extraScale;
    AirbaseEditorFlag airbaseEditorFlag = Object.Instantiate<GameObject>(prefab, parent).AddComponent<AirbaseEditorFlag>();
    airbaseEditorFlag.transform.localScale = vector3;
    color.a = 0.5f;
    airbaseEditorFlag.material = airbaseEditorFlag.GetComponent<Renderer>().material;
    airbaseEditorFlag.SetColor(color);
    return airbaseEditorFlag;
  }

  public static AirbaseEditorFlag Find(Transform directParent)
  {
    AirbaseEditorFlag.cache.Clear();
    directParent.GetComponentsInChildren<AirbaseEditorFlag>(AirbaseEditorFlag.cache);
    foreach (AirbaseEditorFlag airbaseEditorFlag in AirbaseEditorFlag.cache)
    {
      if ((Object) airbaseEditorFlag.transform.parent == (Object) directParent)
        return airbaseEditorFlag;
    }
    return (AirbaseEditorFlag) null;
  }
}

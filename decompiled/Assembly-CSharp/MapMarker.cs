// Decompiled with JetBrains decompiler
// Type: MapMarker
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using UnityEngine;
using UnityEngine.UI;

#nullable disable
public abstract class MapMarker : MonoBehaviour
{
  [SerializeField]
  public Image markerImg;
  public Color color = Color.white;
  public float lastRefresh;
  public float refreshDelay = 1f;

  public virtual void Remove()
  {
    if (!((Object) this != (Object) null))
      return;
    SceneSingleton<DynamicMap>.i.mapMarkers.Remove(this);
    Object.Destroy((Object) this.gameObject);
  }

  public virtual void DynamicHide()
  {
  }

  public virtual void Mask()
  {
  }

  public virtual void Show(bool value)
  {
  }
}

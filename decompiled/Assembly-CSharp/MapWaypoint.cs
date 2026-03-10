// Decompiled with JetBrains decompiler
// Type: MapWaypoint
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

#nullable disable
public class MapWaypoint
{
  public Vector3 previousWaypoint;
  public GameObject marker;
  public GameObject vector;

  public MapWaypoint(Vector3 previousWaypoint, GameObject marker, GameObject vector)
  {
    this.previousWaypoint = previousWaypoint;
    this.marker = marker;
    this.vector = vector;
    this.PlaceMarker();
  }

  public void PlaceMarker()
  {
    float num = 1f / SceneSingleton<DynamicMap>.i.mapImage.transform.localScale.x;
    this.marker.transform.position = Input.mousePosition;
    this.marker.transform.localScale = 1f * Vector3.one * num;
    this.vector.transform.position = Input.mousePosition;
    this.vector.transform.eulerAngles = new Vector3(0.0f, 0.0f, (float) (-(double) Mathf.Atan2(this.marker.transform.localPosition.x - this.previousWaypoint.x, this.marker.transform.localPosition.y - this.previousWaypoint.y) * 57.295780181884766 + 180.0));
    this.vector.transform.localScale = new Vector3(4f * num, (this.marker.transform.localPosition - this.previousWaypoint).magnitude, 4f * num);
  }

  public void UpdateMarker()
  {
    if ((Object) this.marker == (Object) null || (Object) this.vector == (Object) null)
      return;
    float num = 1f / SceneSingleton<DynamicMap>.i.mapImage.transform.localScale.x;
    this.marker.transform.localScale = 1f * Vector3.one * num;
    this.vector.transform.localScale = new Vector3(4f * num, (this.marker.transform.localPosition - this.previousWaypoint).magnitude, 4f * num);
  }
}

// Decompiled with JetBrains decompiler
// Type: HUDFunctions
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

#nullable disable
public static class HUDFunctions
{
  public static bool PinToScreenEdge(Vector3 coords, out Vector3 rayToScreen, out float arrowAngle)
  {
    bool screenEdge = false;
    Vector3 to = coords - SceneSingleton<CameraStateManager>.i.mainCamera.transform.position;
    rayToScreen = SceneSingleton<CameraStateManager>.i.mainCamera.WorldToScreenPoint(coords);
    Vector3 vector3_1 = new Vector3((float) Screen.width * 0.5f, (float) Screen.height * 0.5f, 0.0f);
    Vector3 vector3_2 = vector3_1;
    rayToScreen -= vector3_1;
    rayToScreen.z = 0.0f;
    arrowAngle = Mathf.Atan2(rayToScreen.y, rayToScreen.x);
    float num = Mathf.Tan(arrowAngle);
    if ((double) Vector3.Angle(SceneSingleton<CameraStateManager>.i.transform.forward, to) > 90.0 || (double) Mathf.Abs(rayToScreen.x) > (double) Screen.width * 0.5 || (double) Mathf.Abs(rayToScreen.y) > (double) Screen.height * 0.5)
    {
      screenEdge = true;
      rayToScreen = (double) rayToScreen.x <= 0.0 ? new Vector3(-vector3_2.x, -vector3_2.x * num, 0.0f) : new Vector3(vector3_2.x, vector3_2.x * num, 0.0f);
      if ((double) rayToScreen.y > (double) vector3_2.y)
        rayToScreen = new Vector3(vector3_2.y / num, vector3_2.y, 0.0f);
      else if ((double) rayToScreen.y < -(double) vector3_2.y)
        rayToScreen = new Vector3(-vector3_2.y / num, -vector3_2.y, 0.0f);
    }
    rayToScreen += vector3_1;
    return screenEdge;
  }
}

// Decompiled with JetBrains decompiler
// Type: ExposureController
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

#nullable disable
public static class ExposureController
{
  private static ColorAdjustments colorAdjustments;
  private static List<Light> brightLights = new List<Light>();
  private static float exposure;
  private static float targetExposure;
  private static float adjustmentSpeed;
  private static float lastExposureUpdate;

  public static void SetColorAdjustments(ColorAdjustments colorAdjustments)
  {
    ExposureController.brightLights = new List<Light>();
    ExposureController.colorAdjustments = colorAdjustments;
  }

  public static void RegisterBrightLight(Light light) => ExposureController.brightLights.Add(light);

  public static bool BrightLightsExist() => ExposureController.brightLights.Count > 1;

  public static void UpdateExposure()
  {
    if ((Object) ExposureController.colorAdjustments == (Object) null)
      return;
    if ((double) Time.unscaledTime - (double) ExposureController.lastExposureUpdate > 0.10000000149011612)
    {
      ExposureController.lastExposureUpdate = Time.unscaledTime;
      ExposureController.targetExposure = Mathf.LerpUnclamped(SceneSingleton<CameraStateManager>.i.maxExposure, SceneSingleton<CameraStateManager>.i.minExposure, NetworkSceneSingleton<LevelInfo>.i.GetAmbientLight() / SceneSingleton<CameraStateManager>.i.lightSensitivity);
      float f = 0.0f;
      for (int index = ExposureController.brightLights.Count - 1; index >= 0; --index)
      {
        Light brightLight = ExposureController.brightLights[index];
        if ((Object) brightLight == (Object) null)
        {
          ExposureController.brightLights.RemoveAt(index);
        }
        else
        {
          Vector3 vector3 = brightLight.transform.position - SceneSingleton<CameraStateManager>.i.transform.position;
          float num1 = brightLight.intensity * 2f;
          if (brightLight.type == UnityEngine.LightType.Directional)
            vector3 = -brightLight.transform.forward;
          else
            num1 = brightLight.intensity / vector3.sqrMagnitude;
          float num2 = (float) ((double) Vector3.Dot(vector3.normalized, SceneSingleton<CameraStateManager>.i.transform.forward) * 2.0 - 1.0);
          if ((double) num2 >= 0.0 && !Physics.Linecast(SceneSingleton<CameraStateManager>.i.transform.position, brightLight.transform.position, 64 /*0x40*/))
            f += 0.05f * num2 * num1;
        }
      }
      float num = Mathf.Clamp(Mathf.Sqrt(f), 0.0f, 5f);
      ExposureController.targetExposure -= num;
    }
    float smoothTime = (double) ExposureController.targetExposure < (double) ExposureController.exposure ? 0.5f : 4f;
    ExposureController.colorAdjustments.postExposure.value = Mathf.SmoothDamp(ExposureController.colorAdjustments.postExposure.value, ExposureController.targetExposure, ref ExposureController.adjustmentSpeed, smoothTime, 10f, Time.unscaledDeltaTime);
  }
}

// Decompiled with JetBrains decompiler
// Type: UnityGraphicsBullshit
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

#nullable disable
public static class UnityGraphicsBullshit
{
  private static FieldInfo MainLightCastShadows_FieldInfo;
  private static FieldInfo AdditionalLightCastShadows_FieldInfo;
  private static FieldInfo MainLightShadowmapResolution_FieldInfo;
  private static FieldInfo AdditionalLightShadowmapResolution_FieldInfo;
  private static FieldInfo Cascade2Split_FieldInfo;
  private static FieldInfo Cascade4Split_FieldInfo;
  private static FieldInfo SoftShadowsEnabled_FieldInfo;

  static UnityGraphicsBullshit()
  {
    System.Type type = typeof (UniversalRenderPipelineAsset);
    BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.NonPublic;
    UnityGraphicsBullshit.MainLightCastShadows_FieldInfo = type.GetField("m_MainLightShadowsSupported", bindingAttr);
    UnityGraphicsBullshit.AdditionalLightCastShadows_FieldInfo = type.GetField("m_AdditionalLightShadowsSupported", bindingAttr);
    UnityGraphicsBullshit.MainLightShadowmapResolution_FieldInfo = type.GetField("m_MainLightShadowmapResolution", bindingAttr);
    UnityGraphicsBullshit.AdditionalLightShadowmapResolution_FieldInfo = type.GetField("m_AdditionalLightsShadowmapResolution", bindingAttr);
    UnityGraphicsBullshit.Cascade2Split_FieldInfo = type.GetField("m_Cascade2Split", bindingAttr);
    UnityGraphicsBullshit.Cascade4Split_FieldInfo = type.GetField("m_Cascade4Split", bindingAttr);
    UnityGraphicsBullshit.SoftShadowsEnabled_FieldInfo = type.GetField("m_SoftShadowsSupported", bindingAttr);
  }

  public static bool MainLightCastShadows
  {
    get
    {
      return (bool) UnityGraphicsBullshit.MainLightCastShadows_FieldInfo.GetValue((object) GraphicsSettings.currentRenderPipeline);
    }
    set
    {
      UnityGraphicsBullshit.MainLightCastShadows_FieldInfo.SetValue((object) GraphicsSettings.currentRenderPipeline, (object) value);
    }
  }

  public static bool AdditionalLightCastShadows
  {
    get
    {
      return (bool) UnityGraphicsBullshit.AdditionalLightCastShadows_FieldInfo.GetValue((object) GraphicsSettings.currentRenderPipeline);
    }
    set
    {
      UnityGraphicsBullshit.AdditionalLightCastShadows_FieldInfo.SetValue((object) GraphicsSettings.currentRenderPipeline, (object) value);
    }
  }

  public static UnityEngine.Rendering.Universal.ShadowResolution MainLightShadowResolution
  {
    get
    {
      return (UnityEngine.Rendering.Universal.ShadowResolution) UnityGraphicsBullshit.MainLightShadowmapResolution_FieldInfo.GetValue((object) GraphicsSettings.currentRenderPipeline);
    }
    set
    {
      UnityGraphicsBullshit.MainLightShadowmapResolution_FieldInfo.SetValue((object) GraphicsSettings.currentRenderPipeline, (object) value);
    }
  }

  public static UnityEngine.Rendering.Universal.ShadowResolution AdditionalLightShadowResolution
  {
    get
    {
      return (UnityEngine.Rendering.Universal.ShadowResolution) UnityGraphicsBullshit.AdditionalLightShadowmapResolution_FieldInfo.GetValue((object) GraphicsSettings.currentRenderPipeline);
    }
    set
    {
      UnityGraphicsBullshit.AdditionalLightShadowmapResolution_FieldInfo.SetValue((object) GraphicsSettings.currentRenderPipeline, (object) value);
    }
  }

  public static float Cascade2Split
  {
    get
    {
      return (float) UnityGraphicsBullshit.Cascade2Split_FieldInfo.GetValue((object) GraphicsSettings.currentRenderPipeline);
    }
    set
    {
      UnityGraphicsBullshit.Cascade2Split_FieldInfo.SetValue((object) GraphicsSettings.currentRenderPipeline, (object) value);
    }
  }

  public static Vector3 Cascade4Split
  {
    get
    {
      return (Vector3) UnityGraphicsBullshit.Cascade4Split_FieldInfo.GetValue((object) GraphicsSettings.currentRenderPipeline);
    }
    set
    {
      UnityGraphicsBullshit.Cascade4Split_FieldInfo.SetValue((object) GraphicsSettings.currentRenderPipeline, (object) value);
    }
  }

  public static bool SoftShadowsEnabled
  {
    get
    {
      return (bool) UnityGraphicsBullshit.SoftShadowsEnabled_FieldInfo.GetValue((object) GraphicsSettings.currentRenderPipeline);
    }
    set
    {
      UnityGraphicsBullshit.SoftShadowsEnabled_FieldInfo.SetValue((object) GraphicsSettings.currentRenderPipeline, (object) value);
    }
  }
}

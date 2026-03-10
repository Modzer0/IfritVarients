// Decompiled with JetBrains decompiler
// Type: PlayerSettings
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;

#nullable disable
public static class PlayerSettings
{
  public static PlayerSettings.UnitSystem unitSystem = PlayerSettings.UnitSystem.Metric;
  public static string playerName;
  public static float cockpitCamInertia = 0.5f;
  public static float defaultFoV = 50f;
  public static float defaultExternalFoV = 50f;
  public static bool zoomOnBoresight = false;
  public static bool padLockTarget = true;
  public static bool tacScreenIR = false;
  public static bool cameraAutoNVG = true;
  public static bool lagPip = false;
  public static bool rangeCircle = false;
  public static bool gauges = true;
  public static int hudTime = 0;
  public static bool hudWeapons = false;
  public static float hmdWidth = 1920f;
  public static float hmdHeight = 1080f;
  public static float hmdTopHeight = 300f;
  public static float hmdSideDist = 350f;
  public static float hmdSideAngle = 45f;
  public static float hmdHideDist = 0.33f;
  public static float hmdIconSize = 30f;
  public static float hudTextSize = 40f;
  public static float hmdTextSize = 40f;
  public static float overlayTextSize = 32f;
  public static int hudColorR = 0;
  public static int hudColorG = (int) byte.MaxValue;
  public static int hudColorB = 0;
  public static string playerName_Unsanitized;
  public static int landingCam = 1;
  public static int radialControl = 0;
  public static bool virtualJoystickEnabled = true;
  public static bool virtualJoystickInvertPitch = true;
  public static bool viewInvertPitch = false;
  public static bool invertCollective = false;
  public static float virtualJoystickSensitivity = 0.25f;
  public static float virtualJoystickCentering = 0.0f;
  public static float viewSensitivity = 0.5f;
  public static float viewSmoothing = 0.5f;
  public static float pressDelay = 0.15f;
  public static float clickDelay = 0.25f;
  public static bool useCompositeYaw;
  public static bool throttleUseNegative = true;
  public static bool throttleUseRelative = false;
  public static bool controllerMenuNavigation = true;
  public static bool menuWeaponSafety = true;
  public static bool useTrackIR;
  public static int mipmapLevel = 0;
  public static int shadowQuality = 3;
  public static int antiAliasing = 1;
  public static float cloudDetail = 1f;
  public static bool debugVis = false;
  public static bool controlFiltersTuning = false;
  public static bool cinematicMode = false;
  public static bool vsync;
  public static bool chatEnabled;
  public static bool chatFilter;
  public static bool chatTts;
  public static int chatTtsSpeed;
  public static int chatTtsVolume;
  public static PlayerSettings.KillFeedFilter killFeedMunition = PlayerSettings.KillFeedFilter.All;
  public static PlayerSettings.KillFeedFilter killFeedAircraft = PlayerSettings.KillFeedFilter.All;
  public static PlayerSettings.KillFeedFilter killFeedBuilding = PlayerSettings.KillFeedFilter.All;
  public static PlayerSettings.KillFeedFilter killFeedShip = PlayerSettings.KillFeedFilter.All;
  public static PlayerSettings.KillFeedFilter killFeedVehicle = PlayerSettings.KillFeedFilter.All;
  public static float killFeedMinValue = 0.0f;
  public static int killFeedNbLines = 20;
  public static bool showHitMarkers = true;

  public static event Action OnApplyOptions;

  public static void FirstInit()
  {
    if (GameManager.IsHeadless)
      return;
    ColorLog<PlayerSettings.QualityLevels>.Info($"BeforeInit QualityLevel:{(Enum) (PlayerSettings.QualityLevels) QualitySettings.GetQualityLevel()}, LOD:{QualitySettings.lodBias}");
    QualitySettings.SetQualityLevel(3, true);
    ColorLog<PlayerSettings.QualityLevels>.Info($"QualityLevel:{(Enum) (PlayerSettings.QualityLevels) QualitySettings.GetQualityLevel()}, LOD:{QualitySettings.lodBias}");
  }

  public static void LoadPrefs()
  {
    if (PlayerPrefs.HasKey("UnitSystem"))
      PlayerSettings.unitSystem = (PlayerSettings.UnitSystem) PlayerPrefs.GetInt("UnitSystem");
    if (PlayerPrefs.HasKey("CockpitCamInertia"))
      PlayerSettings.cockpitCamInertia = PlayerPrefs.GetFloat("CockpitCamInertia");
    if (PlayerPrefs.HasKey("DefaultFoV"))
      PlayerSettings.defaultFoV = PlayerPrefs.GetFloat("DefaultFoV");
    if (PlayerPrefs.HasKey("DefaultExternalFoV"))
      PlayerSettings.defaultExternalFoV = PlayerPrefs.GetFloat("DefaultExternalFoV");
    if (PlayerPrefs.HasKey("ZoomOnBoresight"))
      PlayerSettings.zoomOnBoresight = PlayerPrefs.GetInt("ZoomOnBoresight", 0) == 1;
    if (PlayerPrefs.HasKey("PadLockTarget"))
      PlayerSettings.padLockTarget = PlayerPrefs.GetInt("PadLockTarget", 0) == 1;
    if (PlayerPrefs.HasKey("TacScreenIR"))
      PlayerSettings.tacScreenIR = PlayerPrefs.GetInt("TacScreenIR", 0) == 1;
    if (PlayerPrefs.HasKey("CameraAutoNVG"))
      PlayerSettings.cameraAutoNVG = PlayerPrefs.GetInt("CameraAutoNVG", 0) == 1;
    if (PlayerPrefs.HasKey("LagPip"))
      PlayerSettings.lagPip = PlayerPrefs.GetInt("LagPip", 0) == 1;
    if (PlayerPrefs.HasKey("RangeCircle"))
      PlayerSettings.rangeCircle = PlayerPrefs.GetInt("RangeCircle", 0) == 1;
    if (PlayerPrefs.HasKey("Gauges"))
      PlayerSettings.gauges = PlayerPrefs.GetInt("Gauges", 1) == 1;
    if (PlayerPrefs.HasKey("HUDTime"))
      PlayerSettings.hudTime = PlayerPrefs.GetInt("HUDTime", 0);
    if (PlayerPrefs.HasKey("HUDWeapons"))
      PlayerSettings.hudWeapons = PlayerPrefs.GetInt("HUDWeapons", 0) == 1;
    PlayerSettings.hmdWidth = (float) (1080.0 * ((double) Screen.width / (double) Screen.height));
    PlayerSettings.hmdHeight = 1080f;
    if (PlayerPrefs.HasKey("HMDHeight"))
      PlayerSettings.hmdHeight = PlayerPrefs.GetFloat("HMDHeight");
    if (PlayerPrefs.HasKey("HMDWidth"))
      PlayerSettings.hmdWidth = PlayerPrefs.GetFloat("HMDWidth");
    if (PlayerPrefs.HasKey("HMDTopHeight"))
      PlayerSettings.hmdTopHeight = PlayerPrefs.GetFloat("HMDTopHeight");
    if (PlayerPrefs.HasKey("HMDSideDist"))
      PlayerSettings.hmdSideDist = PlayerPrefs.GetFloat("HMDSideDist");
    if (PlayerPrefs.HasKey("HMDSideAngle"))
      PlayerSettings.hmdSideAngle = PlayerPrefs.GetFloat("HMDSideAngle");
    if (PlayerPrefs.HasKey("HMDHideDist"))
      PlayerSettings.hmdHideDist = PlayerPrefs.GetFloat("HMDHideDist");
    if (PlayerPrefs.HasKey("HMDIconSize"))
      PlayerSettings.hmdIconSize = PlayerPrefs.GetFloat("HMDIconSize", 30f);
    if (PlayerPrefs.HasKey("HUDTextSize"))
      PlayerSettings.hudTextSize = PlayerPrefs.GetFloat("HUDTextSize", 40f);
    if (PlayerPrefs.HasKey("HMDTextSize"))
      PlayerSettings.hmdTextSize = PlayerPrefs.GetFloat("HMDTextSize", 40f);
    if (PlayerPrefs.HasKey("OverlayTextSize"))
      PlayerSettings.overlayTextSize = PlayerPrefs.GetFloat("OverlayTextSize", 32f);
    if (PlayerPrefs.HasKey("HUDColorR"))
      PlayerSettings.hudColorR = PlayerPrefs.GetInt("HUDColorR", 0);
    if (PlayerPrefs.HasKey("HUDColorG"))
      PlayerSettings.hudColorG = PlayerPrefs.GetInt("HUDColorG", (int) byte.MaxValue);
    if (PlayerPrefs.HasKey("HUDColorB"))
      PlayerSettings.hudColorB = PlayerPrefs.GetInt("HUDColorB", 0);
    if (PlayerPrefs.HasKey("LandingCam"))
      PlayerSettings.landingCam = PlayerPrefs.GetInt("LandingCam", 1);
    if (PlayerPrefs.HasKey("RadialControl"))
      PlayerSettings.radialControl = PlayerPrefs.GetInt("RadialControl", 0);
    PlayerSettings.vsync = PlayerPrefs.GetInt("Vsync", 1) == 1;
    QualitySettings.vSyncCount = PlayerSettings.vsync ? 1 : 0;
    PlayerSettings.mipmapLevel = PlayerPrefs.GetInt("MipmapLevel", 0);
    QualitySettings.globalTextureMipmapLimit = PlayerSettings.mipmapLevel;
    PlayerSettings.shadowQuality = PlayerPrefs.GetInt("ShadowQuality", 3);
    UnityEngine.Rendering.Universal.ShadowResolution shadowResolution = UnityEngine.Rendering.Universal.ShadowResolution._512;
    if (PlayerSettings.shadowQuality == 2)
      shadowResolution = UnityEngine.Rendering.Universal.ShadowResolution._1024;
    if (PlayerSettings.shadowQuality == 3)
      shadowResolution = UnityEngine.Rendering.Universal.ShadowResolution._2048;
    if (PlayerSettings.shadowQuality == 4)
      shadowResolution = UnityEngine.Rendering.Universal.ShadowResolution._4096;
    UnityGraphicsBullshit.MainLightCastShadows = PlayerSettings.shadowQuality > 0;
    UnityGraphicsBullshit.MainLightShadowResolution = shadowResolution;
    PlayerSettings.antiAliasing = PlayerPrefs.GetInt("AntiAliasing", 1);
    QualitySettings.antiAliasing = PlayerSettings.antiAliasing;
    if (PlayerPrefs.HasKey("DebugVis"))
      PlayerSettings.debugVis = PlayerPrefs.GetInt("DebugVis") == 1;
    if (PlayerPrefs.HasKey("CloudDetail"))
      PlayerSettings.cloudDetail = PlayerPrefs.GetFloat("CloudDetail");
    if (PlayerPrefs.HasKey("VirtualJoystickEnabled"))
      PlayerSettings.virtualJoystickEnabled = PlayerPrefs.GetInt("VirtualJoystickEnabled") == 1;
    if (PlayerPrefs.HasKey("VirtualJoystickInvertPitch"))
      PlayerSettings.virtualJoystickInvertPitch = PlayerPrefs.GetInt("VirtualJoystickInvertPitch") == 1;
    if (PlayerPrefs.HasKey("ViewInvertPitch"))
      PlayerSettings.viewInvertPitch = PlayerPrefs.GetInt("ViewInvertPitch") == 1;
    if (PlayerPrefs.HasKey("ThrottleUseNegative"))
      PlayerSettings.throttleUseNegative = PlayerPrefs.GetInt("ThrottleUseNegative") == 1;
    if (PlayerPrefs.HasKey("ThrottleUseRelative"))
      PlayerSettings.throttleUseRelative = PlayerPrefs.GetInt("ThrottleUseRelative") == 1;
    if (PlayerPrefs.HasKey("ControllerMenuNavigation"))
      PlayerSettings.controllerMenuNavigation = PlayerPrefs.GetInt("ControllerMenuNavigation") == 1;
    if (PlayerPrefs.HasKey("MenuWeaponSafety"))
      PlayerSettings.menuWeaponSafety = PlayerPrefs.GetInt("MenuWeaponSafety") == 1;
    if (PlayerPrefs.HasKey("InvertCollective"))
      PlayerSettings.invertCollective = PlayerPrefs.GetInt("InvertCollective") == 1;
    if (PlayerPrefs.HasKey("VirtualJoystickSensitivity"))
      PlayerSettings.virtualJoystickSensitivity = PlayerPrefs.GetFloat("VirtualJoystickSensitivity");
    if (PlayerPrefs.HasKey("VirtualJoystickCentering"))
      PlayerSettings.virtualJoystickCentering = PlayerPrefs.GetFloat("VirtualJoystickCentering");
    if (PlayerPrefs.HasKey("ViewSensitivity"))
      PlayerSettings.viewSensitivity = PlayerPrefs.GetFloat("ViewSensitivity");
    if (PlayerPrefs.HasKey("ViewSmoothing"))
      PlayerSettings.viewSmoothing = PlayerPrefs.GetFloat("ViewSmoothing");
    if (PlayerPrefs.HasKey("PressDelay"))
      PlayerSettings.pressDelay = PlayerPrefs.GetFloat("PressDelay");
    if (PlayerPrefs.HasKey("ClickDelay"))
      PlayerSettings.clickDelay = PlayerPrefs.GetFloat("ClickDelay");
    PlayerSettings.useTrackIR = PlayerPrefs.GetInt("UseTrackIR", 0) == 1;
    PlayerSettings.chatEnabled = PlayerPrefs.GetInt("ChatEnabled", 1) == 1;
    PlayerSettings.chatFilter = PlayerPrefs.GetInt("ChatFilter", 1) == 1;
    PlayerSettings.chatTts = PlayerPrefs.GetInt("ChatTts", 0) == 1;
    PlayerSettings.chatTtsSpeed = PlayerPrefs.GetInt("ChatTtsSpeed", 0);
    PlayerSettings.chatTtsVolume = PlayerPrefs.GetInt("ChatTtsVolume", 60);
    PlayerSettings.killFeedMunition = (PlayerSettings.KillFeedFilter) PlayerPrefs.GetInt("KillFeedMunition", 4);
    PlayerSettings.killFeedAircraft = (PlayerSettings.KillFeedFilter) PlayerPrefs.GetInt("KillFeedAircraft", 4);
    PlayerSettings.killFeedBuilding = (PlayerSettings.KillFeedFilter) PlayerPrefs.GetInt("KillFeedBuilding", 4);
    PlayerSettings.killFeedVehicle = (PlayerSettings.KillFeedFilter) PlayerPrefs.GetInt("KillFeedVehicle", 4);
    PlayerSettings.killFeedShip = (PlayerSettings.KillFeedFilter) PlayerPrefs.GetInt("KillFeedShip", 4);
    PlayerSettings.killFeedMinValue = PlayerPrefs.GetFloat("KillFeedMinValue", 0.0f);
    PlayerSettings.killFeedNbLines = PlayerPrefs.GetInt("KillFeedNbLines", 2);
    if (!PlayerPrefs.HasKey("ShowHitMarkers"))
      return;
    PlayerSettings.showHitMarkers = PlayerPrefs.GetInt("ShowHitMarkers", 1) == 1;
  }

  public static void ApplyPrefs()
  {
    GameManager.eventSystem.sendNavigationEvents = PlayerSettings.controllerMenuNavigation;
    Debug.Log((object) $"Setting event system Send Navigation Events to {PlayerSettings.controllerMenuNavigation}");
    if ((UnityEngine.Object) SceneSingleton<CameraStateManager>.i != (UnityEngine.Object) null)
    {
      CameraStateManager i = SceneSingleton<CameraStateManager>.i;
      float num = i.currentState == i.cockpitState ? PlayerSettings.defaultFoV : PlayerSettings.defaultExternalFoV;
      i.SetDesiredFoV(num, num);
    }
    Action onApplyOptions = PlayerSettings.OnApplyOptions;
    if (onApplyOptions == null)
      return;
    onApplyOptions();
  }

  public enum QualityLevels
  {
    NoShadows,
    Low,
    Medium,
    High,
  }

  public enum UnitSystem
  {
    Metric,
    Imperial,
  }

  public enum KillFeedFilter
  {
    None,
    Player,
    Friendly,
    Enemy,
    All,
  }
}

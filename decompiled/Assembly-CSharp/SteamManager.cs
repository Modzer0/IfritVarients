// Decompiled with JetBrains decompiler
// Type: SteamManager
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using AOT;
using Cysharp.Threading.Tasks;
using NuclearOption.BuildScripts;
using NuclearOption.DedicatedServer;
using NuclearOption.SavedMission.ObjectiveV2;
using Steamworks;
using System;
using System.IO;
using System.Text;
using UnityEngine;

#nullable disable
[DisallowMultipleComponent]
public class SteamManager : MonoBehaviour
{
  private static bool s_EverInitialized;
  private static SteamManager instance;
  private bool initialized;
  private bool isServer;
  private SteamAPIWarningMessageHook_t m_SteamAPIWarningMessageHook;
  private Steamworks.FSteamNetworkingSocketsDebugOutput m_FSteamNetworkingSocketsDebugOutput;

  public static string SteamAppId { get; private set; }

  [MonoPInvokeCallback(typeof (SteamAPIWarningMessageHook_t))]
  protected static void SteamAPIWarningMessageHook(int nSeverity, StringBuilder pchDebugText)
  {
    string str;
    switch (nSeverity)
    {
      case 0:
        str = "[msg]";
        break;
      case 1:
        str = "[warning]";
        break;
      default:
        str = "[unknown]";
        break;
    }
    ColorLog<SteamManager>.Info($"{str} {pchDebugText}");
  }

  [MonoPInvokeCallback(typeof (SteamAPIWarningMessageHook_t))]
  protected static void FSteamNetworkingSocketsDebugOutput(
    ESteamNetworkingSocketsDebugOutputType nType,
    StringBuilder pszMsg)
  {
    string str;
    switch (nType)
    {
      case ESteamNetworkingSocketsDebugOutputType.k_ESteamNetworkingSocketsDebugOutputType_None:
        str = "[None]";
        break;
      case ESteamNetworkingSocketsDebugOutputType.k_ESteamNetworkingSocketsDebugOutputType_Bug:
        str = "[Bug]";
        break;
      case ESteamNetworkingSocketsDebugOutputType.k_ESteamNetworkingSocketsDebugOutputType_Error:
        str = "[Error]";
        break;
      case ESteamNetworkingSocketsDebugOutputType.k_ESteamNetworkingSocketsDebugOutputType_Important:
        str = "[Important]";
        break;
      case ESteamNetworkingSocketsDebugOutputType.k_ESteamNetworkingSocketsDebugOutputType_Warning:
        str = "[Warning]";
        break;
      case ESteamNetworkingSocketsDebugOutputType.k_ESteamNetworkingSocketsDebugOutputType_Msg:
        str = "[Msg]";
        break;
      case ESteamNetworkingSocketsDebugOutputType.k_ESteamNetworkingSocketsDebugOutputType_Verbose:
        str = "[Verbose]";
        break;
      case ESteamNetworkingSocketsDebugOutputType.k_ESteamNetworkingSocketsDebugOutputType_Debug:
        str = "[Debug]";
        break;
      case ESteamNetworkingSocketsDebugOutputType.k_ESteamNetworkingSocketsDebugOutputType_Everything:
        str = "[Everything]";
        break;
      default:
        str = "[unknown]";
        break;
    }
    ColorLog<SteamManager>.Info($"{str} {pszMsg}");
  }

  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
  private static void InitOnPlayMode() => SteamManager.s_EverInitialized = false;

  public static UniTask CheckIdFileAsync(string steamAppID)
  {
    SteamManager.SteamAppId = steamAppID;
    return UniTask.RunOnThreadPool((Action) (() =>
    {
      ColorLog<SteamManager>.Info("[SideThread] Running CheckIdFile");
      if (File.Exists("steam_appid.txt"))
      {
        ColorLog<SteamManager>.Info("[SideThread] File exists");
        string str = File.ReadAllText("steam_appid.txt");
        if (!(str != steamAppID))
          return;
        ColorLog<SteamManager>.Info("[SideThread] Id different");
        File.WriteAllText("steam_appid.txt", steamAppID.ToString());
        ColorLog<SteamManager>.Info($"[SideThread] Updating steam_appid.txt. Previous: {str}, new SteamAppID {steamAppID}");
      }
      else
      {
        ColorLog<SteamManager>.Info("[SideThread] No file");
        File.WriteAllText("steam_appid.txt", steamAppID.ToString());
        ColorLog<SteamManager>.Info("[SideThread] New steam_appid.txt written with SteamAppID " + steamAppID);
      }
    }));
  }

  public void InitAsClient()
  {
    ColorLog<SteamManager>.Info("Init As Client");
    this.CheckInit();
    if (!this.ClientInit())
      return;
    this.DebugHooks();
  }

  public void InitAsServer(DedicatedServerConfig serverConfig)
  {
    ColorLog<SteamManager>.Info("Init As Server");
    this.CheckInit();
    if (!this.ServerInit(serverConfig.Port.AsNullable<ushort>(), serverConfig.QueryPort.AsNullable<ushort>()))
      CommandLineArgParser.Quit();
    else
      this.DebugHooks();
  }

  private void CheckInit()
  {
    ColorLog<SteamManager>.Info(nameof (CheckInit));
    if (this.initialized)
      throw new Exception("This SteamManager instance was already initialized");
    if (SteamManager.s_EverInitialized)
      throw new Exception("Tried to Initialize the SteamAPI twice in one session!");
    if ((UnityEngine.Object) SteamManager.instance != (UnityEngine.Object) null)
      throw new Exception("SteamManager.instance should be null when init is called");
    if (!Packsize.Test())
      Debug.LogError((object) "[Steamworks.NET] Packsize Test returned false, the wrong version of Steamworks.NET is being run in this platform.", (UnityEngine.Object) this);
    if (DllCheck.Test())
      return;
    Debug.LogError((object) "[Steamworks.NET] DllCheck Test returned false, One or more of the Steamworks binaries seems to be the wrong version.", (UnityEngine.Object) this);
  }

  private void CheckRestart()
  {
    ColorLog<SteamManager>.Info(nameof (CheckRestart));
    try
    {
      if (!SteamAPI.RestartAppIfNecessary(AppId_t.Invalid))
        return;
      Application.Quit();
    }
    catch (DllNotFoundException ex)
    {
      Debug.LogError((object) ("[Steamworks.NET] Could not load [lib]steam_api.dll/so/dylib. It's likely not in the correct location. Refer to the README for more details.\n" + ex?.ToString()), (UnityEngine.Object) this);
      Application.Quit();
    }
  }

  private bool ClientInit()
  {
    ColorLog<SteamManager>.Info(nameof (ClientInit));
    this.initialized = SteamAPI.Init();
    if (!this.initialized)
    {
      Debug.LogError((object) "[Steamworks.NET] SteamAPI_Init() failed.");
      return false;
    }
    ColorLog<SteamManager>.Info("ClientInit Success");
    this.MarkInit(false);
    return true;
  }

  private bool ServerInit(ushort? portNullable, ushort? queryPortNullable)
  {
    ColorLog<SteamManager>.Info(nameof (ServerInit));
    this.initialized = GameServer.Init(0U, (ushort) ((int) portNullable ?? 7777), (ushort) ((int) queryPortNullable ?? 7778), EServerMode.eServerModeAuthenticationAndSecure, Application.version);
    if (!this.initialized)
    {
      Debug.LogError((object) "[Steamworks.NET] SteamGameServer_Init call failed");
      SteamAPI.Shutdown();
      return false;
    }
    ColorLog<SteamManager>.Info("ServerInit Success");
    this.MarkInit(true);
    return true;
  }

  private void MarkInit(bool isServer)
  {
    SteamManager.s_EverInitialized = true;
    SteamManager.instance = this;
    this.isServer = isServer;
  }

  private void DebugHooks()
  {
    ColorLog<SteamManager>.Info(nameof (DebugHooks));
    if (this.m_SteamAPIWarningMessageHook != null)
      return;
    this.m_SteamAPIWarningMessageHook = new SteamAPIWarningMessageHook_t(SteamManager.SteamAPIWarningMessageHook);
    this.m_FSteamNetworkingSocketsDebugOutput = new Steamworks.FSteamNetworkingSocketsDebugOutput(SteamManager.FSteamNetworkingSocketsDebugOutput);
    ESteamNetworkingSocketsDebugOutputType eDetailLevel = CommandLineArgParser.UseSteamNetworkingVerboseLogging ? ESteamNetworkingSocketsDebugOutputType.k_ESteamNetworkingSocketsDebugOutputType_Everything : ESteamNetworkingSocketsDebugOutputType.k_ESteamNetworkingSocketsDebugOutputType_Msg;
    if (this.isServer)
    {
      SteamGameServerClient.SetWarningMessageHook(this.m_SteamAPIWarningMessageHook);
      SteamGameServerNetworkingUtils.SetDebugOutputFunction(eDetailLevel, this.m_FSteamNetworkingSocketsDebugOutput);
    }
    else
    {
      SteamClient.SetWarningMessageHook(this.m_SteamAPIWarningMessageHook);
      SteamNetworkingUtils.SetDebugOutputFunction(eDetailLevel, this.m_FSteamNetworkingSocketsDebugOutput);
    }
  }

  private void OnDestroy()
  {
    if ((UnityEngine.Object) SteamManager.instance != (UnityEngine.Object) this)
    {
      ColorLog<SteamManager>.InfoWarn("instance of SteamManager destroyed that is not the main instance");
    }
    else
    {
      SteamManager.instance = (SteamManager) null;
      if (!this.initialized)
        return;
      ColorLog<SteamManager>.Info("Shutdown");
      if (this.isServer)
        GameServer.Shutdown();
      else
        SteamAPI.Shutdown();
    }
  }

  private void Update()
  {
    if (!this.initialized)
      return;
    if (this.isServer)
      GameServer.RunCallbacks();
    else
      SteamAPI.RunCallbacks();
  }

  public static bool ClientInitialized
  {
    get
    {
      return (UnityEngine.Object) SteamManager.instance != (UnityEngine.Object) null && SteamManager.instance.initialized && !SteamManager.instance.isServer;
    }
  }

  public static bool ServerInitialized
  {
    get
    {
      return (UnityEngine.Object) SteamManager.instance != (UnityEngine.Object) null && SteamManager.instance.initialized && SteamManager.instance.isServer;
    }
  }
}

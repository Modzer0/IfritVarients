// Decompiled with JetBrains decompiler
// Type: SteamDebugLog
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Steamworks;
using System;
using System.IO;
using System.Text;
using UnityEngine;

#nullable disable
public class SteamDebugLog
{
  private static string logFilePath;

  public static void Initialize(bool server)
  {
    SteamDebugLog.logFilePath = Path.Combine(Application.persistentDataPath, Application.isEditor ? "SteamLog_editor.Log" : "SteamLog_player.Log");
    File.WriteAllText(SteamDebugLog.logFilePath, $"--- NEW SESSION START: {DateTime.Now.ToString()} ---\n");
    if (server)
      SteamGameServerNetworkingUtils.SetDebugOutputFunction(ESteamNetworkingSocketsDebugOutputType.k_ESteamNetworkingSocketsDebugOutputType_Everything, new FSteamNetworkingSocketsDebugOutput(SteamDebugLog.DebugOutput));
    else
      SteamNetworkingUtils.SetDebugOutputFunction(ESteamNetworkingSocketsDebugOutputType.k_ESteamNetworkingSocketsDebugOutputType_Everything, new FSteamNetworkingSocketsDebugOutput(SteamDebugLog.DebugOutput));
  }

  public static void DebugOutput(ESteamNetworkingSocketsDebugOutputType nType, StringBuilder pszMsg)
  {
    string str1;
    switch (nType)
    {
      case ESteamNetworkingSocketsDebugOutputType.k_ESteamNetworkingSocketsDebugOutputType_None:
        str1 = "[NONE]";
        break;
      case ESteamNetworkingSocketsDebugOutputType.k_ESteamNetworkingSocketsDebugOutputType_Bug:
        str1 = "[BUG]";
        break;
      case ESteamNetworkingSocketsDebugOutputType.k_ESteamNetworkingSocketsDebugOutputType_Error:
        str1 = "[ERROR]";
        break;
      case ESteamNetworkingSocketsDebugOutputType.k_ESteamNetworkingSocketsDebugOutputType_Important:
        str1 = "[IMPORTANT]";
        break;
      case ESteamNetworkingSocketsDebugOutputType.k_ESteamNetworkingSocketsDebugOutputType_Warning:
        str1 = "[WARN]";
        break;
      case ESteamNetworkingSocketsDebugOutputType.k_ESteamNetworkingSocketsDebugOutputType_Msg:
        str1 = "[INFO]";
        break;
      case ESteamNetworkingSocketsDebugOutputType.k_ESteamNetworkingSocketsDebugOutputType_Verbose:
        str1 = "[VERBOSE]";
        break;
      case ESteamNetworkingSocketsDebugOutputType.k_ESteamNetworkingSocketsDebugOutputType_Debug:
        str1 = "[DEBUG]";
        break;
      case ESteamNetworkingSocketsDebugOutputType.k_ESteamNetworkingSocketsDebugOutputType_Everything:
        str1 = "[EVERYTHING]";
        break;
      default:
        str1 = "[UNKNOWN]";
        break;
    }
    string str2 = string.Format("{0:HH:mm:ss:fff} {1} {2}", (object) DateTime.Now, (object) str1, (object) pszMsg.ToString());
    try
    {
      File.AppendAllText(SteamDebugLog.logFilePath, str2 + Environment.NewLine);
    }
    catch (Exception ex)
    {
      Debug.LogError((object) ("Failed to write to log file: " + ex.Message));
    }
  }
}

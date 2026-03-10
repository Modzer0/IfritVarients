// Decompiled with JetBrains decompiler
// Type: qol.CommandProcessing.Processing.ConfigLoader
// Assembly: qol, Version=1.1.6.3, Culture=neutral, PublicKeyToken=null
// MVID: 3B6539EA-E38C-45EE-8FFF-FC58CC42BADC
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\BepInEx\plugins\qol_1.1.6.3b1.dll

using BepInEx.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;

#nullable disable
namespace qol.CommandProcessing.Processing;

public static class ConfigLoader
{
  private const string ResourceSuffix = "commands.qol";

  public static string LoadEmbeddedConfig(Assembly assembly, ManualLogSource logger)
  {
    string name = ((IEnumerable<string>) assembly.GetManifestResourceNames()).FirstOrDefault<string>((Func<string, bool>) (n => n.EndsWith("commands.qol", StringComparison.OrdinalIgnoreCase)));
    if (name == null)
    {
      logger.LogWarning((object) "No embedded config file found");
      return (string) null;
    }
    using (Stream manifestResourceStream = assembly.GetManifestResourceStream(name))
    {
      if (manifestResourceStream == null)
      {
        logger.LogWarning((object) "Issue with ManifestResourceStream occurred");
        return (string) null;
      }
      using (StreamReader streamReader = new StreamReader(manifestResourceStream))
        return streamReader.ReadToEnd();
    }
  }

  public static bool CheckIntegrity(Assembly assembly, string expectedHash, ManualLogSource logger)
  {
    logger.LogInfo((object) "Starting integrity check");
    string name = ((IEnumerable<string>) assembly.GetManifestResourceNames()).FirstOrDefault<string>((Func<string, bool>) (n => n.EndsWith("commands.qol", StringComparison.OrdinalIgnoreCase)));
    if (name == null)
    {
      logger.LogError((object) "Resource not found!");
      return false;
    }
    using (Stream manifestResourceStream = assembly.GetManifestResourceStream(name))
    {
      if (manifestResourceStream == null)
      {
        logger.LogError((object) "Cannot read resource!");
        return false;
      }
      using (SHA256 shA256 = SHA256.Create())
      {
        if (!string.Equals(BitConverter.ToString(shA256.ComputeHash(manifestResourceStream)).Replace("-", "").ToLower(), expectedHash, StringComparison.OrdinalIgnoreCase))
        {
          logger.LogError((object) "Resource integrity check failed!");
          return false;
        }
      }
    }
    logger.LogInfo((object) "Integrity check complete");
    return true;
  }

  public static string GetResourceHash(Assembly assembly)
  {
    string name = ((IEnumerable<string>) assembly.GetManifestResourceNames()).FirstOrDefault<string>((Func<string, bool>) (n => n.EndsWith("commands.qol", StringComparison.OrdinalIgnoreCase)));
    if (name == null)
      return "RESOURCE_NOT_FOUND";
    using (Stream manifestResourceStream = assembly.GetManifestResourceStream(name))
    {
      using (SHA256 shA256 = SHA256.Create())
        return BitConverter.ToString(shA256.ComputeHash(manifestResourceStream)).Replace("-", "").ToLower();
    }
  }
}

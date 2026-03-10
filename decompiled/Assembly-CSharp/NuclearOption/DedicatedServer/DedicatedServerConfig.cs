// Decompiled with JetBrains decompiler
// Type: NuclearOption.DedicatedServer.DedicatedServerConfig
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.SavedMission.ObjectiveV2;
using System;
using System.IO;
using UnityEngine;

#nullable disable
namespace NuclearOption.DedicatedServer;

[Serializable]
public class DedicatedServerConfig
{
  public string MissionDirectory;
  public bool ModdedServer;
  public bool Hidden;
  public string ServerName;
  public Override<ushort> Port;
  public Override<ushort> QueryPort;
  public string Password;
  public int MaxPlayers;
  public string[] BanListPaths;
  public bool DisableErrorKick;
  public string[] ErrorKickImmuneListPaths;
  public float NoPlayerStopTime;
  public RotationType RotationType;
  public MissionOptions[] MissionRotation;

  public bool HasPassword => !string.IsNullOrEmpty(this.Password);

  public static DedicatedServerConfig CreateDefault()
  {
    DedicatedServerConfig dedicatedServerConfig = new DedicatedServerConfig();
    dedicatedServerConfig.MissionDirectory = "/home/steam/NuclearOption-Missions";
    dedicatedServerConfig.ModdedServer = false;
    dedicatedServerConfig.ServerName = "Nuclear Option Server";
    dedicatedServerConfig.Port = new Override<ushort>();
    dedicatedServerConfig.QueryPort = new Override<ushort>();
    dedicatedServerConfig.MaxPlayers = 16 /*0x10*/;
    dedicatedServerConfig.Password = (string) null;
    dedicatedServerConfig.BanListPaths = new string[1]
    {
      "ban_list.txt"
    };
    dedicatedServerConfig.ErrorKickImmuneListPaths = new string[0];
    dedicatedServerConfig.DisableErrorKick = false;
    dedicatedServerConfig.NoPlayerStopTime = 30f;
    dedicatedServerConfig.RotationType = RotationType.Sequence;
    dedicatedServerConfig.MissionRotation = new MissionOptions[2]
    {
      new MissionOptions()
      {
        Key = new MissionKeySaveable()
        {
          Group = "BuiltIn",
          Name = "Escalation"
        },
        MaxTime = 7200f
      },
      new MissionOptions()
      {
        Key = new MissionKeySaveable()
        {
          Group = "BuiltIn",
          Name = "Terminal Control"
        },
        MaxTime = 7200f
      }
    };
    return dedicatedServerConfig;
  }

  public static void Save(string path, DedicatedServerConfig config, bool overwrite)
  {
    if (!overwrite && File.Exists(path))
      throw new InvalidOperationException(path + " already exists");
    string json = JsonUtility.ToJson((object) config);
    File.WriteAllText(path, json);
  }

  public static bool TryLoad(string path, out DedicatedServerConfig config)
  {
    ColorLog<DedicatedServerConfig>.Info("TryLoad " + path);
    if (File.Exists(path))
    {
      try
      {
        string json = File.ReadAllText(path);
        config = JsonUtility.FromJson<DedicatedServerConfig>(json);
        ColorLog<DedicatedServerConfig>.Info("Load success " + path);
        return true;
      }
      catch (Exception ex)
      {
        Debug.LogException(ex);
      }
    }
    config = (DedicatedServerConfig) null;
    return false;
  }

  public static (DedicatedServerConfig config, string path) AutoFindOrCreate()
  {
    DedicatedServerConfig config1;
    if (DedicatedServerConfig.TryLoad("DedicatedServerConfig.json", out config1))
      return (config1, "DedicatedServerConfig.json");
    string path = Path.Combine(Application.persistentDataPath, "DedicatedServerConfig.json");
    if (DedicatedServerConfig.TryLoad(path, out config1))
      return (config1, path);
    DedicatedServerConfig config2 = DedicatedServerConfig.CreateDefault();
    if (config2.BanListPaths.Length != 0 && !File.Exists(config2.BanListPaths[0]))
      File.WriteAllText(config2.BanListPaths[0], "");
    DedicatedServerConfig.Save("DedicatedServerConfig.json", config2, false);
    return (config2, "DedicatedServerConfig.json");
  }
}

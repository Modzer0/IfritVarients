// Decompiled with JetBrains decompiler
// Type: NuclearOption.DedicatedServer.AllowBanList
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Steamworks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

#nullable disable
namespace NuclearOption.DedicatedServer;

public class AllowBanList
{
  private readonly Dictionary<CSteamID, string> players = new Dictionary<CSteamID, string>();

  public bool Contains(CSteamID id) => this.players.ContainsKey(id);

  public void Add(CSteamID id, string reason)
  {
    if (this.players.TryAdd(id, reason))
      ColorLog<AllowBanList>.Info($"Adding {id} to list");
    else
      ColorLog<AllowBanList>.Info($"{id} already in list");
  }

  public void Remove(CSteamID id)
  {
    if (this.players.Remove(id))
      ColorLog<AllowBanList>.Info($"Removed {id} from list");
    else
      ColorLog<AllowBanList>.Info($"{id} not in list");
  }

  public void Clear()
  {
    ColorLog<AllowBanList>.Info("Clearing list");
    this.players.Clear();
  }

  public void Load(string path)
  {
    ColorLog<AllowBanList>.Info("Loading Steam Ids from " + path);
    List<(CSteamID, string)> banned = new List<(CSteamID, string)>();
    AllowBanList.Load(path, banned);
    foreach ((CSteamID csteamId, string str) in banned)
    {
      if (this.players.TryAdd(csteamId, str))
        ColorLog<AllowBanList>.Info($"Adding {csteamId} to list");
      else
        ColorLog<AllowBanList>.Info($"{csteamId} already in list");
    }
  }

  public static void Load(string path, List<(CSteamID, string reason)> banned)
  {
    if (!File.Exists(path))
    {
      ColorLog<AllowBanList>.LogError("File does not exist: " + path);
    }
    else
    {
      string str1 = File.ReadAllText(path);
      char[] chArray = new char[2]{ '\r', '\n' };
      foreach (string str2 in str1.Split(chArray))
      {
        string[] strArray = str2.Split("//", 2, StringSplitOptions.None);
        string str3 = strArray[0];
        string str4 = strArray.Length > 1 ? strArray[1] : (string) null;
        string s = str3.Trim();
        if (!string.IsNullOrEmpty(s))
        {
          string str5 = str4?.Trim();
          ulong result;
          if (ulong.TryParse(s, out result))
          {
            CSteamID csteamId = new CSteamID(result);
            banned.Add((csteamId, str5));
          }
          else
            ColorLog<AllowBanList>.LogError("Failed to parse " + s);
        }
      }
    }
  }

  public static void Save(string path, List<(CSteamID id, string reason)> banned)
  {
    StringBuilder stringBuilder = new StringBuilder();
    foreach ((CSteamID id, string reason) in banned)
      stringBuilder.AppendLine(AllowBanList.FormatLine(id, reason));
    File.WriteAllText(path, stringBuilder.ToString());
  }

  private static string FormatLine(CSteamID id, string reason)
  {
    string str = id.m_SteamID.ToString();
    return string.IsNullOrEmpty(reason) ? str : $"{str} // {reason}";
  }

  public static void BanAndAppendId(AllowBanList list, string path, CSteamID id, string reason)
  {
    list.Add(id, reason);
    AllowBanList.AppendId(path, id, reason);
  }

  public static void AppendId(string path, CSteamID id, string reason)
  {
    if (!File.Exists(path))
    {
      ColorLog<AllowBanList>.LogError("File does not exist: " + path);
    }
    else
    {
      ColorLog<AllowBanList>.Info($"Appending Steam {id} to {path}");
      File.AppendAllLines(path, (IEnumerable<string>) new string[1]
      {
        AllowBanList.FormatLine(id, reason)
      });
    }
  }

  public static void RemoveId(string path, CSteamID id)
  {
    if (!File.Exists(path))
    {
      ColorLog<AllowBanList>.LogError("File does not exist: " + path);
    }
    else
    {
      ColorLog<AllowBanList>.Info($"Removing Steam {id} from {path}");
      List<(CSteamID, string)> banned = new List<(CSteamID, string)>();
      AllowBanList.Load(path, banned);
      for (int index = banned.Count - 1; index >= 0; --index)
      {
        if (banned[index].Item1 == id)
          banned.RemoveAt(index);
      }
      AllowBanList.Save(path, banned);
    }
  }
}

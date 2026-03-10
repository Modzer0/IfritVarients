// Decompiled with JetBrains decompiler
// Type: NuclearOption.Networking.Lobbies.DedicatedServerKeyValues
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Steamworks;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

#nullable disable
namespace NuclearOption.Networking.Lobbies;

public class DedicatedServerKeyValues
{
  public const string SHORT_HOST_VERSION_KEY = "v";
  public const string SHORT_MODDED_KEY = "m";
  public const string SHORT_MISSION_PVP_TYPE = "t";
  public const string SHORT_HAS_PASSWORD = "p";
  public const string SHORT_START_TIME_KEY = "s";
  public const string SHORT_SHORT_PASSWORD = "p";
  public const string SHORT_MISSION_NAME_KEY = "mi";
  public const string SHORT_MISSION_DESCRIPTION_KEY = "d";
  public const int SHORT_MISSION_DESCRIPTION_KEY_CHUNKS = 4;
  public const string SHORT_MAP_NAME_KEY = "ma";
  public const string SHORT_MISSION_WORKSHOP_ID_KEY = "id";
  private readonly StringBuilder builder = new StringBuilder();
  private readonly Dictionary<string, string> keyValues = new Dictionary<string, string>();
  private readonly Dictionary<string, string> tags = new Dictionary<string, string>();
  private bool hasPassword;

  public void SetKeyValue(string key, string value)
  {
    switch (key)
    {
      case "map_name":
        this.keyValues["ma"] = value;
        break;
      case "mission_description":
        for (int index = 0; index < 4; ++index)
          this.keyValues.Remove($"{"d"}{index}");
        int num = 0;
        while (num < 4)
          ++num;
        List<string> stringList = StringHelper.SplitStringByByteCount(value, false, 125, 4);
        for (int index = 0; index < stringList.Count; ++index)
          this.keyValues[$"{"d"}{index}"] = stringList[index];
        break;
      case "mission_name":
        this.keyValues["mi"] = value;
        break;
      case "mission_pvp_type":
        this.tags["t"] = value;
        break;
      case "mission_workshop_id":
        this.keyValues["id"] = value;
        break;
      case "modded_server":
        this.tags["m"] = value;
        break;
      case "short_password":
        if (!string.IsNullOrEmpty(value))
        {
          this.hasPassword = true;
          this.keyValues["p"] = value;
          this.tags["p"] = "1";
          break;
        }
        this.hasPassword = false;
        this.tags["p"] = "0";
        break;
      case "start_time":
        this.keyValues["s"] = value;
        break;
      case "version":
        this.tags["v"] = value;
        break;
      default:
        throw new ArgumentException("Invalid server key " + key);
    }
  }

  public void ApplyValuesToSteam()
  {
    SteamGameServer.SetPasswordProtected(this.hasPassword);
    this.ApplyTags();
    this.ApplyMapName();
    this.ApplyKeyValue();
  }

  private void ApplyTags()
  {
    this.builder.Clear();
    foreach (KeyValuePair<string, string> tag in this.tags)
    {
      this.builder.Append(tag.Key);
      this.builder.Append("=");
      this.builder.Append(tag.Value);
      this.builder.Append(",");
    }
    string str = this.builder.ToString();
    int byteLength = StringHelper.GetByteLength(str);
    if (byteLength > 128 /*0x80*/)
      Debug.LogError((object) $"tags are {byteLength} bytes, but the limit is {128 /*0x80*/}");
    ColorLog<DedicatedServerKeyValues>.Info($"Settings steam tags to '{str}'");
    SteamGameServer.SetGameTags(str);
  }

  private void ApplyMapName()
  {
    string str1;
    this.keyValues.TryGetValue("ma", out str1);
    string str2;
    this.keyValues.TryGetValue("mi", out str2);
    SteamGameServer.SetMapName(!string.IsNullOrEmpty(str1) ? (!string.IsNullOrEmpty(str2) ? $"{str1} | {str2}" : str2 ?? "") : str2 ?? "");
  }

  private void ApplyKeyValue()
  {
    int num1 = 0;
    foreach (KeyValuePair<string, string> keyValue in this.keyValues)
    {
      int num2 = StringHelper.GetByteLength(keyValue.Key) + StringHelper.GetByteLength(keyValue.Value);
      if (num2 > (int) sbyte.MaxValue)
      {
        Debug.LogError((object) $"pair ({num2} bytes) is over 127 bytes and will be truncated, {keyValue.Key} = {keyValue.Value}");
        num2 = (int) sbyte.MaxValue;
      }
      num1 += num2;
      if (num1 > 1300)
        Debug.LogError((object) "total SetKeyValue are over 1300 no more values will be set");
      ColorLog<DedicatedServerKeyValues>.Info($"Settings pair ({num2}/{num1} bytes) {keyValue.Key}={keyValue.Value}");
      SteamGameServer.SetKeyValue(keyValue.Key, keyValue.Value);
    }
  }

  public static void ParseTags(string rawTags, Dictionary<string, string> outRules)
  {
    foreach (string str1 in rawTags.Split(",", StringSplitOptions.None))
    {
      if (!string.IsNullOrEmpty(str1))
      {
        string[] strArray = str1.Split("=", StringSplitOptions.None);
        if (strArray.Length == 2)
        {
          string tagKey = DedicatedServerKeyValues.ParseTagKey(strArray[0]);
          string str2 = strArray[1];
          outRules[tagKey] = str2;
        }
        else
        {
          Debug.LogError((object) $"Failed to parse tag '{str1}', full tags '{rawTags}'");
          break;
        }
      }
    }
  }

  private static string ParseTagKey(string key)
  {
    switch (key)
    {
      case "v":
        return "version";
      case "m":
        return "modded_server";
      case "t":
        return "mission_pvp_type";
      case "p":
        return "has_password";
      default:
        throw new ArgumentException("Invalid tag key " + key);
    }
  }

  public static void ParseKeyValue(
    string shortKey,
    string value,
    Dictionary<string, string> outRules)
  {
    string keyValueKey = DedicatedServerKeyValues.ParseKeyValueKey(shortKey);
    outRules[keyValueKey] = value;
  }

  private static string ParseKeyValueKey(string key)
  {
    switch (key)
    {
      case "p":
        return "short_password";
      case "s":
        return "start_time";
      case "mi":
        return "mission_name";
      case "ma":
        return "map_name";
      case "id":
        return "mission_workshop_id";
      default:
        for (int index = 0; index < 4; ++index)
        {
          if (key == $"{"d"}{index}")
            return key;
        }
        throw new ArgumentException("Invalid KeyValue key " + key);
    }
  }

  public static string ParseDescription(Dictionary<string, string> rules)
  {
    StringBuilder stringBuilder = new StringBuilder();
    string str;
    for (int index = 0; index < 4 && rules.TryGetValue($"{"d"}{index}", out str); ++index)
      stringBuilder.Append(str);
    return stringBuilder.ToString();
  }
}

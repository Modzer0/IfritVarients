// Decompiled with JetBrains decompiler
// Type: NuclearOption.Workshop.WorkshopJson
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Steamworks;
using System;
using System.IO;
using UnityEngine;

#nullable disable
namespace NuclearOption.Workshop;

public struct WorkshopJson
{
  public ulong Id;
  public SubscribedItemType Type;
  public string TypeHint;

  public readonly PublishedFileId_t PublishedId => new PublishedFileId_t(this.Id);

  private WorkshopJson(PublishedFileId_t id, SubscribedItemType type)
  {
    this.Id = id.m_PublishedFileId;
    this.Type = type;
    this.TypeHint = type.ToString();
  }

  private static string GetPath(string folder) => Path.Combine(folder, "workshop.json");

  private static string GetPath2(string folder) => Path.Combine(folder, "Workshop.json");

  public static WorkshopJson ReadFileOrInvalid(string folder)
  {
    return WorkshopJson.ReadFile(folder) ?? new WorkshopJson(PublishedFileId_t.Invalid, SubscribedItemType.Unknown);
  }

  public static WorkshopJson? ReadFile(string folder, bool ignoreNotFoundWarning = false)
  {
    string path = WorkshopJson.GetPath(folder);
    if (!File.Exists(path))
    {
      string path2 = WorkshopJson.GetPath2(folder);
      if (File.Exists(path2))
      {
        path = path2;
      }
      else
      {
        if (!ignoreNotFoundWarning)
          Debug.LogWarning((object) ("workshop.json file not found in " + folder));
        return new WorkshopJson?();
      }
    }
    try
    {
      return new WorkshopJson?(JsonUtility.FromJson<WorkshopJson>(File.ReadAllText(path)));
    }
    catch (Exception ex)
    {
      Debug.LogException(ex);
      return new WorkshopJson?();
    }
  }

  public static void WriteFile(string folder, PublishedFileId_t id, SubscribedItemType type)
  {
    string json = JsonUtility.ToJson((object) new WorkshopJson(id, type));
    File.WriteAllText(WorkshopJson.GetPath(folder), json);
  }
}

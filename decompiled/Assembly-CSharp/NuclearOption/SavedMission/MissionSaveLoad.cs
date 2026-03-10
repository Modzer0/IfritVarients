// Decompiled with JetBrains decompiler
// Type: NuclearOption.SavedMission.MissionSaveLoad
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Cysharp.Threading.Tasks;
using NuclearOption.AddressableScripts;
using NuclearOption.Workshop;
using Steamworks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

#nullable disable
namespace NuclearOption.SavedMission;

public static class MissionSaveLoad
{
  private static readonly StringBuilder stringBuilder = new StringBuilder();
  private static readonly Dictionary<MissionKey, MissionQuickLoad> quickLoadCache = new Dictionary<MissionKey, MissionQuickLoad>();

  public static bool ValidateName(ref string saveName, bool allowEmpty)
  {
    if (!allowEmpty && string.IsNullOrEmpty(saveName))
    {
      saveName = "Untitled Mission";
      return true;
    }
    if (saveName == "meta")
    {
      Debug.LogWarning((object) "'meta.json' is a reserved name, changing name to 'meta2.json' instead");
      saveName = "meta2";
      return true;
    }
    if (!(saveName == "workshop"))
      return false;
    Debug.LogWarning((object) "'workshop.json' is a reserved name, changing name to 'workshop2.json' instead");
    saveName = "workshop2";
    return true;
  }

  public static bool Exist(string name)
  {
    MissionSaveLoad.ValidateName(ref name, false);
    return Directory.Exists(MissionGroup.UserGroup.UserMissionDirectory) && File.Exists(MissionGroup.UserGroup.GetJsonPath(name));
  }

  public static void SaveMission(Mission mission, bool callBeforeSave = true)
  {
    MissionSaveLoad.SaveMission(mission, ref mission.Name, callBeforeSave);
  }

  public static void SaveMission(Mission mission, ref string saveName, bool callBeforeSave = true)
  {
    MissionSaveLoad.ValidateName(ref saveName, false);
    if (callBeforeSave)
      mission.BeforeSave();
    MissionGroup.UserGroup.SaveMission(saveName, mission);
  }

  public static bool SaveMissionTemp(
    Mission mission,
    bool runBeforeSave,
    out Mission missionCopy,
    out string loadErrors)
  {
    return MissionSaveLoad.SaveMissionTemp(mission, mission.Name, runBeforeSave, out missionCopy, out loadErrors);
  }

  public static bool SaveMissionTemp(
    Mission mission,
    string overrideName,
    bool runBeforeSave,
    out Mission missionCopy,
    out string loadErrors)
  {
    MissionSaveLoad.ValidateName(ref overrideName, false);
    if (runBeforeSave)
      mission.BeforeSave();
    MissionGroup.TempGroup.SaveMission(overrideName, mission);
    return new MissionKey(overrideName, mission.Name, new PublishedFileId_t?(), (MissionGroup) MissionGroup.Temp).TryLoad(out missionCopy, out loadErrors);
  }

  public static bool LoadMissionTemp(
    Mission mission,
    out Mission missionCopy,
    out string loadErrors)
  {
    return new MissionKey(mission.Name, (MissionGroup) MissionGroup.Temp).TryLoad(out missionCopy, out loadErrors);
  }

  public static bool TryLoad(MissionKey item, out Mission mission, out string error)
  {
    item.ThrowIfInvalid();
    mission = (Mission) null;
    string json = (string) null;
    bool flag = false;
    error = (string) null;
    string name = item.Name;
    try
    {
      flag = item.Group.TryGetJson(item.Key, out json);
    }
    catch (Exception ex)
    {
      string str = $"Failed to load mission with name {name} because of {ex.GetType().Name}";
      Debug.LogError((object) (str + " (see below)"));
      Debug.LogException(ex);
      error = $"{str}\n{ex}";
    }
    if (flag)
      return MissionSaveLoad.TryReadJson(item, json, out mission, out error);
    if (string.IsNullOrEmpty(error))
      error = "Could not find mission with name " + name;
    return false;
  }

  public static Mission LoadDefault()
  {
    Mission mission;
    string error;
    if (!MissionSaveLoad.TryReadJson(new MissionKey("Free Flight", (MissionGroup) MissionGroup.Default), MissionGroup.Default.ReadFirst(), out mission, out error))
      throw new Exception("Default to load default mission: " + error);
    return mission;
  }

  public static bool TryReadJson(
    MissionKey key,
    string json,
    out Mission mission,
    out string error)
  {
    try
    {
      mission = JsonUtility.FromJson<Mission>(json);
      mission.AfterLoad(key);
      error = (string) null;
      return true;
    }
    catch (ArgumentException ex) when (ex.Message.StartsWith("JSON parse error"))
    {
      error = ex.Message;
    }
    catch (AggregateException ex)
    {
      MissionSaveLoad.stringBuilder.Clear();
      string str = $"Failed to load mission with name {key.Name} because of {ex.InnerExceptions.Count} Exception(s)";
      Debug.LogError((object) (str + " (see below)"));
      MissionSaveLoad.stringBuilder.AppendLine(str);
      foreach (Exception innerException in ex.InnerExceptions)
      {
        Debug.LogException(innerException);
        MissionSaveLoad.stringBuilder.AppendLine($"- {innerException.GetType()}: {innerException.Message}");
      }
      error = MissionSaveLoad.stringBuilder.ToString();
    }
    catch (Exception ex)
    {
      Debug.LogError((object) "Unexpected error when loading (see below)");
      Debug.LogException(ex);
      error = ex.ToString();
    }
    mission = (Mission) null;
    return false;
  }

  public static IEnumerable<(MissionKey key, MissionQuickLoad mission)> QuickLoadMany(
    IEnumerable<MissionKey> items)
  {
    foreach (IGrouping<MissionGroup, MissionKey> items1 in items.GroupBy<MissionKey, MissionGroup>((Func<MissionKey, MissionGroup>) (x => x.Group)))
    {
      foreach ((MissionKey key, string json) in items1.Key.GetManyJson((IEnumerable<MissionKey>) items1))
      {
        MissionQuickLoad? nullable = new MissionQuickLoad?();
        try
        {
          nullable = new MissionQuickLoad?(MissionSaveLoad.ReadQuickLoad(key, json));
        }
        catch (Exception ex)
        {
          Debug.LogError((object) $"Failed to quickLoad {key}, {ex}");
        }
        if (nullable.HasValue)
          yield return (key, nullable.Value);
      }
    }
  }

  private static MissionQuickLoad ReadQuickLoad(MissionKey key, string json)
  {
    MissionQuickLoad mission = JsonUtility.FromJson<MissionQuickLoad>(json);
    mission.AfterLoad(key);
    MissionTag.AddAutoTags(mission);
    MissionSaveLoad.quickLoadCache[key] = mission;
    return mission;
  }

  public static bool QuickLoadOne(MissionKey key, out MissionQuickLoad mission)
  {
    if (MissionSaveLoad.quickLoadCache.TryGetValue(key, out mission))
      return true;
    mission = new MissionQuickLoad();
    try
    {
      string json;
      if (!key.Group.TryGetJson(key.Key, out json))
        return false;
      mission = MissionSaveLoad.ReadQuickLoad(key, json);
      return true;
    }
    catch (Exception ex)
    {
      Debug.LogError((object) "Unexpected error when loading (see below)");
      Debug.LogException(ex);
      return false;
    }
  }

  public static UniTask ConvertMissionToFolders()
  {
    MissionGroup.Init();
    return UniTask.RunOnThreadPool((Action) (() =>
    {
      Debug.Log((object) "[SideThread] Running ConvertMissionToFolders");
      try
      {
        string missionDirectory = MissionGroup.UserGroup.UserMissionDirectory;
        if (!Directory.Exists(missionDirectory))
          return;
        foreach (string file in Directory.GetFiles(missionDirectory))
        {
          string withoutExtension = Path.GetFileNameWithoutExtension(file);
          string str = Path.Combine(missionDirectory, withoutExtension);
          if (Directory.Exists(str))
          {
            Debug.LogError((object) $"Failed to move mission file to folder because {str} already exists");
          }
          else
          {
            Directory.CreateDirectory(str);
            File.Move(file, Path.Combine(str, withoutExtension + ".json"));
            ModLoader.WriteMetaData<MissionGroup.MissionMetaData>(str, new MissionGroup.MissionMetaData()
            {
              FileName = withoutExtension
            });
            MissionSaveLoad.CreateWorkshopJson(withoutExtension, str);
          }
        }
      }
      catch (Exception ex)
      {
        Debug.LogException(ex);
      }
    }));
  }

  [Obsolete("need Obsolete to use WorkshopId")]
  private static void CreateWorkshopJson(string name, string folder)
  {
    MissionQuickLoad mission;
    if (!MissionSaveLoad.QuickLoadOne(new MissionKey(name, (MissionGroup) MissionGroup.User), out mission) || mission.WorkshopId == 0UL)
      return;
    WorkshopJson.WriteFile(folder, new PublishedFileId_t(mission.WorkshopId), SubscribedItemType.Mission);
  }
}

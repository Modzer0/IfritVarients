// Decompiled with JetBrains decompiler
// Type: NuclearOption.SavedMission.MissionGroup
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Cysharp.Threading.Tasks;
using JamesFrowen.ScriptableVariables;
using NuclearOption.AddressableScripts;
using NuclearOption.Workshop;
using Steamworks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using UnityEngine;

#nullable disable
namespace NuclearOption.SavedMission;

public abstract class MissionGroup
{
  private static MissionGroup.Fields i;
  public readonly string Name;

  public static MissionGroup.AllGroup All => MissionGroup.i._all;

  public static MissionGroup.ResourceGroup Default => MissionGroup.i._default;

  public static MissionGroup.ResourceGroup Tutorial => MissionGroup.i._tutorial;

  public static MissionGroup.ResourceGroup BuiltIn => MissionGroup.i._builtIn;

  public static MissionGroup.UserGroup User => MissionGroup.i._user;

  public static MissionGroup.TempGroup Temp => MissionGroup.i._temp;

  public static MissionGroup.WorkshopGroup Workshop => MissionGroup.i._workshop;

  public static MissionGroup[] AllGroups => MissionGroup.i._allGroups;

  public static void Init() => MissionGroup.i = new MissionGroup.Fields();

  public abstract IEnumerable<MissionKey> GetMissions();

  public abstract bool TryGetJson(string key, out string json);

  public virtual IEnumerable<(MissionKey item, string json)> GetManyJson(
    IEnumerable<MissionKey> items)
  {
    foreach (MissionKey missionKey in items)
    {
      string json;
      if (this.TryGetJson(missionKey.Key, out json))
        yield return (missionKey, json);
    }
  }

  public abstract UniTask<Sprite> GetPreview(string key, CancellationToken token = default (CancellationToken));

  public virtual bool ContainsMission(string key)
  {
    foreach (MissionKey mission in this.GetMissions())
    {
      if (mission.Key == key)
        return true;
    }
    return false;
  }

  protected MissionGroup(string name) => this.Name = name;

  private class Fields
  {
    public readonly MissionGroup.AllGroup _all = new MissionGroup.AllGroup();
    public readonly MissionGroup.ResourceGroup _default = new MissionGroup.ResourceGroup("Free Flight", "DefaultMission");
    public readonly MissionGroup.ResourceGroup _tutorial = new MissionGroup.ResourceGroup("Tutorials");
    public readonly MissionGroup.ResourceGroup _builtIn = new MissionGroup.ResourceGroup("Missions");
    public readonly MissionGroup.UserGroup _user = new MissionGroup.UserGroup();
    public readonly MissionGroup.TempGroup _temp = new MissionGroup.TempGroup();
    public readonly MissionGroup.WorkshopGroup _workshop = new MissionGroup.WorkshopGroup();
    public readonly MissionGroup[] _allGroups;

    public Fields()
    {
      this._allGroups = new MissionGroup[6]
      {
        (MissionGroup) this._all,
        (MissionGroup) this._default,
        (MissionGroup) this._tutorial,
        (MissionGroup) this._builtIn,
        (MissionGroup) this._user,
        (MissionGroup) this._workshop
      };
    }
  }

  [Serializable]
  public struct MissionMetaData : IMetaData
  {
    public string FileName;

    public PublishedFileId_t Id { get; set; }

    public string FolderFullPath { get; set; }

    public string GetMissionPath() => Path.Combine(this.FolderFullPath, this.FileName + ".json");
  }

  public sealed class ResourceGroup : MissionGroup
  {
    private readonly TextAsset[] assets;
    private readonly MissionKey[] names;

    public ResourceGroup(string nameAndPath)
      : this(nameAndPath, nameAndPath)
    {
    }

    public ResourceGroup(string name, string path)
      : base(name)
    {
      this.assets = Resources.LoadAll<TextAsset>(path);
      this.names = ((IEnumerable<TextAsset>) this.assets).Select<TextAsset, MissionKey>((Func<TextAsset, MissionKey>) (x => new MissionKey(x.name, (MissionGroup) this))).ToArray<MissionKey>();
    }

    public override IEnumerable<MissionKey> GetMissions() => (IEnumerable<MissionKey>) this.names;

    public override bool TryGetJson(string key, out string json)
    {
      for (int index = 0; index < this.names.Length; ++index)
      {
        if (this.names[index].Name == key)
        {
          json = this.assets[index].text;
          return true;
        }
      }
      ColorLog<MissionGroup>.Info("Failed to find key in Resources: " + key);
      json = string.Empty;
      return false;
    }

    public override UniTask<Sprite> GetPreview(string key, CancellationToken token)
    {
      return UniTask.FromResult<Sprite>(Resources.Load<Sprite>("MissionImages/" + key));
    }

    public string ReadFirst() => this.assets[0].text;

    public MissionKey First() => this.names[0];
  }

  public sealed class UserGroup : MissionGroup
  {
    public static string UserMissionDirectory;
    private readonly List<MissionKey> names = new List<MissionKey>();

    [RuntimeInitializeOnLoadMethod]
    private static void SetDefaultDirectory()
    {
      MissionGroup.UserGroup.UserMissionDirectory = Application.persistentDataPath + "/Missions";
    }

    public static void SetDirectory(string path)
    {
      ColorLog<MissionGroup>.Info("Setting load path to " + path);
      MissionGroup.UserGroup.UserMissionDirectory = path;
    }

    public static void OpenFolder()
    {
      if (!Directory.Exists(MissionGroup.UserGroup.UserMissionDirectory))
        Directory.CreateDirectory(MissionGroup.UserGroup.UserMissionDirectory);
      Application.OpenURL(MissionGroup.UserGroup.UserMissionDirectory);
    }

    public UserGroup()
      : base("User")
    {
    }

    public static string GetFolder(string itemName)
    {
      return Path.Combine(MissionGroup.UserGroup.UserMissionDirectory, itemName);
    }

    public static string GetJsonPath(string itemName)
    {
      return Path.Combine(MissionGroup.UserGroup.UserMissionDirectory, itemName, itemName + ".json");
    }

    public override IEnumerable<MissionKey> GetMissions()
    {
      if (!Directory.Exists(MissionGroup.UserGroup.UserMissionDirectory))
        return Enumerable.Empty<MissionKey>();
      this.names.Clear();
      foreach (string directory in Directory.GetDirectories(MissionGroup.UserGroup.UserMissionDirectory))
        this.names.Add(new MissionKey(Path.GetFileName(directory), (MissionGroup) this));
      return (IEnumerable<MissionKey>) this.names;
    }

    public override bool TryGetJson(string key, out string json)
    {
      json = string.Empty;
      string jsonPath = MissionGroup.UserGroup.GetJsonPath(key);
      if (!File.Exists(jsonPath))
      {
        ColorLog<MissionGroup>.Info("No file at path: " + jsonPath);
        return false;
      }
      json = File.ReadAllText(jsonPath);
      return true;
    }

    public override UniTask<Sprite> GetPreview(string key, CancellationToken token)
    {
      return UniTask.FromResult<Sprite>((Sprite) null);
    }

    public static void SaveMission(string name, Mission mission)
    {
      string folder = MissionGroup.UserGroup.GetFolder(name);
      if (!Directory.Exists(folder))
        Directory.CreateDirectory(folder);
      File.WriteAllText(MissionGroup.UserGroup.GetJsonPath(name), JsonUtility.ToJson((object) mission, true));
      ModLoader.WriteMetaData<MissionGroup.MissionMetaData>(folder, new MissionGroup.MissionMetaData()
      {
        FileName = name
      });
    }
  }

  public sealed class TempGroup : MissionGroup
  {
    public static readonly string TempMissionDirectory = Application.persistentDataPath + "/TempMissions";

    public TempGroup()
      : base("Temp")
    {
    }

    public static string GetFolder(string itemName)
    {
      return Path.Combine(MissionGroup.TempGroup.TempMissionDirectory, itemName);
    }

    public static string GetJsonPath(string itemName)
    {
      return Path.Combine(MissionGroup.TempGroup.TempMissionDirectory, itemName, itemName + ".json");
    }

    public override IEnumerable<MissionKey> GetMissions() => throw new NotSupportedException();

    public override UniTask<Sprite> GetPreview(string key, CancellationToken token)
    {
      throw new NotSupportedException();
    }

    public override bool TryGetJson(string key, out string json)
    {
      json = string.Empty;
      string jsonPath = MissionGroup.TempGroup.GetJsonPath(key);
      if (!File.Exists(jsonPath))
      {
        ColorLog<MissionGroup>.Info("No file at temp path: " + jsonPath);
        return false;
      }
      json = File.ReadAllText(jsonPath);
      return true;
    }

    public static void SaveMission(string name, Mission mission)
    {
      string folder = MissionGroup.TempGroup.GetFolder(name);
      if (!Directory.Exists(folder))
        Directory.CreateDirectory(folder);
      File.WriteAllText(MissionGroup.TempGroup.GetJsonPath(name), JsonUtility.ToJson((object) mission, true));
      ModLoader.WriteMetaData<MissionGroup.MissionMetaData>(folder, new MissionGroup.MissionMetaData()
      {
        FileName = name
      });
    }
  }

  public sealed class WorkshopGroup : MissionGroup
  {
    public WorkshopGroup()
      : base("Workshop")
    {
    }

    public override IEnumerable<MissionKey> GetMissions()
    {
      return SteamWorkshop.GetSubscribedItems(true, new SubscribedItemType?(SubscribedItemType.Mission)).Select<SubscribedItem, MissionKey?>(new Func<SubscribedItem, MissionKey?>(this.CreateItem)).WhereNotNullable<MissionKey>();
    }

    private MissionKey? CreateItem(SubscribedItem item)
    {
      string missionJsonFile = this.GetMissionJsonFile(item);
      if (string.IsNullOrEmpty(missionJsonFile))
        return new MissionKey?();
      string withoutExtension = Path.GetFileNameWithoutExtension(missionJsonFile);
      return new MissionKey?(new MissionKey(item.Folder, withoutExtension, new PublishedFileId_t?(item.Id), (MissionGroup) this));
    }

    private string GetMissionJsonFile(SubscribedItem item)
    {
      string folder = item.Folder;
      return ModLoader.ReadMetaData<MissionGroup.MissionMetaData>(item.Id, folder)?.GetMissionPath();
    }

    private List<SubscribedItem> GetAllItems()
    {
      return SteamWorkshop.GetSubscribedItems(false, new SubscribedItemType?());
    }

    private bool TryGet(List<SubscribedItem> allItems, string key, out SubscribedItem item)
    {
      item = allItems.FirstOrDefault<SubscribedItem>((Func<SubscribedItem, bool>) (x => x.Folder == key));
      return !string.IsNullOrEmpty(item.Folder);
    }

    public override bool TryGetJson(string key, out string json)
    {
      if (this.TryGetJson(this.GetAllItems(), key, out json))
        return true;
      ColorLog<MissionGroup>.Info("no workshop item with id: " + key);
      return false;
    }

    private bool TryGetJson(List<SubscribedItem> allItems, string key, out string json)
    {
      json = (string) null;
      SubscribedItem subscribedItem;
      if (!this.TryGet(allItems, key, out subscribedItem))
        return false;
      string missionJsonFile = this.GetMissionJsonFile(subscribedItem);
      if (string.IsNullOrEmpty(missionJsonFile))
        return false;
      json = File.ReadAllText(missionJsonFile);
      return true;
    }

    public override IEnumerable<(MissionKey item, string json)> GetManyJson(
      IEnumerable<MissionKey> items)
    {
      List<SubscribedItem> allItems = SteamWorkshop.GetSubscribedItems(false, new SubscribedItemType?());
      foreach (MissionKey missionKey in items)
      {
        string json;
        if (this.TryGetJson(allItems, missionKey.Key, out json))
          yield return (missionKey, json);
      }
    }

    public override async UniTask<Sprite> GetPreview(string key, CancellationToken token)
    {
      SubscribedItem subscribedItem;
      if (!this.TryGet(SteamWorkshop.GetSubscribedItems(false, new SubscribedItemType?()), key, out subscribedItem))
        return (Sprite) null;
      (bool flag, SteamWorkshopItem steamWorkshopItem) = await SteamWorkshop.GetDetails(subscribedItem.Id);
      return !flag || token.IsCancellationRequested ? (Sprite) null : await steamWorkshopItem.GetPreview(token);
    }
  }

  public sealed class AllGroup : MissionGroup
  {
    public AllGroup()
      : base("All")
    {
    }

    public override IEnumerable<MissionKey> GetMissions()
    {
      MissionGroup.AllGroup allGroup = this;
      MissionGroup[] missionGroupArray = MissionGroup.AllGroups;
      for (int index = 0; index < missionGroupArray.Length; ++index)
      {
        MissionGroup missionGroup = missionGroupArray[index];
        if (missionGroup != allGroup)
        {
          foreach (MissionKey mission in missionGroup.GetMissions())
            yield return mission;
        }
      }
      missionGroupArray = (MissionGroup[]) null;
    }

    public override bool TryGetJson(string key, out string json)
    {
      throw new NotSupportedException();
    }

    public override UniTask<Sprite> GetPreview(string key, CancellationToken token)
    {
      throw new NotSupportedException();
    }
  }
}

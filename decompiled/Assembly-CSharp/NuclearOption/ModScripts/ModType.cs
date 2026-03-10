// Decompiled with JetBrains decompiler
// Type: NuclearOption.ModScripts.ModType
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.Workshop;
using Steamworks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;

#nullable disable
namespace NuclearOption.ModScripts;

public abstract class ModType
{
  public readonly string Name;
  public readonly string Tag;

  public ModType(string nameAndTag)
    : this(nameAndTag, nameAndTag)
  {
  }

  public ModType(string name, string tag)
  {
    this.Name = name;
    this.Tag = tag;
  }

  public void PopulateItemDropdown(TMP_Dropdown dropdown)
  {
    dropdown.ClearOptions();
    dropdown.options.Add(new TMP_Dropdown.OptionData()
    {
      text = ""
    });
    dropdown.AddOptions(this.ListItems());
    dropdown.SetValueWithoutNotify(0);
  }

  public abstract List<string> ListItems();

  public abstract WorkshopUploadItem GetItem(string itemName);

  protected abstract string GetRootFolder();

  public PublishedFileId_t ReadWorkshopId(string itemName)
  {
    WorkshopJson? nullable = WorkshopJson.ReadFile(Path.Combine(this.GetRootFolder(), itemName), true);
    return !nullable.HasValue ? PublishedFileId_t.Invalid : nullable.Value.PublishedId;
  }

  public bool TryGetLocalItem(SteamWorkshopItem details, out WorkshopUploadItem item)
  {
    PublishedFileId_t id = details.WorkshopId;
    IEnumerable<(string, PublishedFileId_t)> source = this.ListItems().Select<string, (string, PublishedFileId_t)>((Func<string, (string, PublishedFileId_t)>) (itemName => (itemName, this.ReadWorkshopId(itemName)))).Where<(string, PublishedFileId_t)>((Func<(string, PublishedFileId_t), bool>) (x => x.id == id));
    int num = source.Count<(string, PublishedFileId_t)>();
    switch (num)
    {
      case 0:
        Debug.LogError((object) $"Failed to find local item with id {id}");
        break;
      case 1:
        string itemName1 = source.First<(string, PublishedFileId_t)>().Item1;
        item = this.GetItem(itemName1);
        return true;
      default:
        if (num > 2)
        {
          Debug.LogError((object) $"Multiple local items with the id {id}");
          break;
        }
        break;
    }
    item = (WorkshopUploadItem) null;
    return false;
  }
}

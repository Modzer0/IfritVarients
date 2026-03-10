// Decompiled with JetBrains decompiler
// Type: NuclearOption.ModScripts.WorkshopUploadItem
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.Workshop;
using Steamworks;

#nullable disable
namespace NuclearOption.ModScripts;

public class WorkshopUploadItem : IWorkshopCreateCallbacks
{
  public readonly string Tag;
  public readonly SubscribedItemType Type;
  public readonly string Folder;
  public readonly string DisplayName;
  public string Description;
  public string ImagePath;
  public bool Public;

  public PublishedFileId_t WorkshopId { get; private set; }

  public WorkshopUploadItem(
    string tag,
    SubscribedItemType type,
    string folder,
    string displayName)
  {
    this.Tag = tag;
    this.Type = type;
    this.Folder = folder;
    this.DisplayName = displayName;
    WorkshopJson? nullable = WorkshopJson.ReadFile(this.Folder, true);
    this.WorkshopId = nullable.HasValue ? nullable.Value.PublishedId : PublishedFileId_t.Invalid;
  }

  public SteamWorkshopItem ToSteamItem()
  {
    return new SteamWorkshopItem(this.DisplayName, this.Tag, this.WorkshopId)
    {
      Description = this.Description,
      ImagePath = this.ImagePath,
      Public = this.Public,
      ContentPath = this.Folder
    };
  }

  void IWorkshopCreateCallbacks.OnCreateOrFail(PublishedFileId_t id)
  {
    this.WorkshopId = id;
    WorkshopJson.WriteFile(this.Folder, id, this.Type);
  }

  public void ClearSteamId() => this.WorkshopId = PublishedFileId_t.Invalid;
}

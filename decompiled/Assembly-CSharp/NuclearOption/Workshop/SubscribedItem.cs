// Decompiled with JetBrains decompiler
// Type: NuclearOption.Workshop.SubscribedItem
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Steamworks;

#nullable disable
namespace NuclearOption.Workshop;

public readonly struct SubscribedItem
{
  public readonly PublishedFileId_t Id;
  public readonly string Folder;
  public readonly WorkshopJson Workshop;

  public SubscribedItem(PublishedFileId_t itemId, string folder, WorkshopJson workshop)
    : this()
  {
    this.Id = itemId;
    this.Folder = folder;
    this.Workshop = workshop;
  }
}

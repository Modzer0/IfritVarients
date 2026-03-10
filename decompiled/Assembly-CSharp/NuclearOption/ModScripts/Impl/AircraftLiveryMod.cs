// Decompiled with JetBrains decompiler
// Type: NuclearOption.ModScripts.Impl.AircraftLiveryMod
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.AddressableScripts.ModFoldersImpl;
using NuclearOption.Workshop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

#nullable disable
namespace NuclearOption.ModScripts.Impl;

internal sealed class AircraftLiveryMod : ModType
{
  public AircraftLiveryMod()
    : base("Aircraft Livery")
  {
  }

  protected override string GetRootFolder() => AppDataSkins.Folder;

  public override List<string> ListItems()
  {
    return ((IEnumerable<string>) AppDataSkins.ListItems()).Select<string, string>((Func<string, string>) (x => Path.GetFileName(x))).ToList<string>();
  }

  public override WorkshopUploadItem GetItem(string itemName)
  {
    LiveryMetaData? nullable = !string.IsNullOrEmpty(itemName) ? AppDataSkins.FromFolderName(itemName) : throw new InvalidOperationException("Page 3 should not be active when mission name is null");
    return !nullable.HasValue ? (WorkshopUploadItem) null : new WorkshopUploadItem(this.Tag, SubscribedItemType.AircraftLivery, AppDataSkins.GetCatalogFolder(itemName), nullable.Value.DisplayName);
  }
}

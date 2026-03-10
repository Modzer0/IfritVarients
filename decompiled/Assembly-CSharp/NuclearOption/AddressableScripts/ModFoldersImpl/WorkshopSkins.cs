// Decompiled with JetBrains decompiler
// Type: NuclearOption.AddressableScripts.ModFoldersImpl.WorkshopSkins
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Cysharp.Threading.Tasks;
using JamesFrowen.ScriptableVariables;
using NuclearOption.Workshop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

#nullable disable
namespace NuclearOption.AddressableScripts.ModFoldersImpl;

public class WorkshopSkins : Skins
{
  private static readonly Regex pattern = new Regex("\\{NuclearOption\\.AddressablePaths\\.AppDataSkinPath\\}\\\\(.+)\\\\(.+_assets_all\\.bundle)");
  private const string CATALOG_WORKSHOP = "catalog_workshop.json";
  private const string CATALOG_1 = "catalog_1.json";

  private async UniTask<string> CreateWorkshopCatalog(string folder)
  {
    return await UniTask.RunOnThreadPool<string>((Func<string>) (() =>
    {
      File.Copy(Path.Combine(folder, "catalog_1.hash"), Path.Combine(folder, "catalog_workshop.hash"), true);
      string input = File.ReadAllText(Path.Combine(folder, "catalog_1.json"));
      string replacement = folder.Replace("\\", "\\\\") + "\\\\$2";
      string contents = WorkshopSkins.pattern.Replace(input, replacement);
      string path = Path.Combine(folder, "catalog_workshop.json");
      File.WriteAllText(path, contents);
      return path;
    }));
  }

  public override async UniTask<string> GetCatalogPath(string folderName)
  {
    return await this.CreateWorkshopCatalog(folderName);
  }

  public override IEnumerable<LiveryMetaData> ListMetaData()
  {
    return SteamWorkshop.GetSubscribedItems(true, new SubscribedItemType?(SubscribedItemType.AircraftLivery)).Select<SubscribedItem, LiveryMetaData?>(new Func<SubscribedItem, LiveryMetaData?>(ModLoader.ReadMetaData<LiveryMetaData>)).WhereNotNullable<LiveryMetaData>();
  }
}

// Decompiled with JetBrains decompiler
// Type: NuclearOption.AddressableScripts.ModFoldersImpl.AppDataSkins
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Cysharp.Threading.Tasks;
using JamesFrowen.ScriptableVariables;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

#nullable disable
namespace NuclearOption.AddressableScripts.ModFoldersImpl;

public class AppDataSkins : Skins
{
  public static string Folder => AddressablePaths.AppDataSkinPath;

  public static string[] ListItems()
  {
    return !Directory.Exists(AppDataSkins.Folder) ? Array.Empty<string>() : Directory.GetDirectories(AppDataSkins.Folder);
  }

  public static string GetCatalogFolder(string itemName)
  {
    return Path.Combine(AppDataSkins.Folder, itemName);
  }

  public static LiveryMetaData? FromFolderName(string itemName)
  {
    return ModLoader.ReadMetaData<LiveryMetaData>(AppDataSkins.GetCatalogFolder(itemName));
  }

  public override UniTask<string> GetCatalogPath(string folderName)
  {
    return UniTask.FromResult<string>(folderName + "/catalog_1.json");
  }

  public override IEnumerable<LiveryMetaData> ListMetaData()
  {
    return ((IEnumerable<string>) AppDataSkins.ListItems()).Select<string, LiveryMetaData?>(new Func<string, LiveryMetaData?>(ModLoader.ReadMetaData<LiveryMetaData>)).WhereNotNullable<LiveryMetaData>();
  }
}

// Decompiled with JetBrains decompiler
// Type: NuclearOption.AddressableScripts.ModFoldersImpl.Skins
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.Workshop;
using UnityEngine;

#nullable disable
namespace NuclearOption.AddressableScripts.ModFoldersImpl;

public abstract class Skins : ModLoader<LiveryMetaData, LiveryData>
{
  public override string Label => "Skin";

  public static bool CanLoad(LiveryKey key, Aircraft aircraft, out string folder)
  {
    string unitName = aircraft.definition.unitName;
    string factionName = (Object) aircraft.NetworkHQ != (Object) null ? aircraft.NetworkHQ.faction.factionName : "";
    return Skins.CanLoad(key, unitName, factionName, out folder);
  }

  public static bool CanLoad(
    LiveryKey key,
    string aircraft,
    string faction,
    out string catalogFolder)
  {
    if (key.Type == LiveryKey.KeyType.Builtin)
    {
      catalogFolder = (string) null;
      return true;
    }
    if (key.Type == LiveryKey.KeyType.AppData)
      catalogFolder = AppDataSkins.GetCatalogFolder(key.AppDataName);
    else if (key.Type == LiveryKey.KeyType.Workshop)
    {
      if (!SteamWorkshop.TryGetInstallFolder(key.WorkshopId, out catalogFolder))
        return false;
    }
    else
    {
      Debug.LogError((object) "Invalid key");
      catalogFolder = (string) null;
      return false;
    }
    LiveryMetaData? nullable = ModLoader.ReadMetaData<LiveryMetaData>(key.WorkshopId, catalogFolder);
    return nullable.HasValue && Skins.CheckMetaData(nullable.Value, aircraft, faction);
  }

  private static bool CheckMetaData(LiveryMetaData data, string aircraft, string faction)
  {
    if (data.Aircraft != aircraft)
    {
      Debug.LogError((object) $"Aircraft did not match meta data. data={data.Aircraft} faction={aircraft}");
      return false;
    }
    if (string.IsNullOrEmpty(data.Faction) || string.IsNullOrEmpty(faction) || !(data.Faction != faction))
      return true;
    Debug.LogError((object) $"Faction did not match meta data. data={data.Faction} faction={faction}");
    return false;
  }
}

// Decompiled with JetBrains decompiler
// Type: NuclearOption.AddressableScripts.ModLoadCache
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System.Collections.Generic;
using UnityEngine.ResourceManagement.ResourceLocations;

#nullable disable
namespace NuclearOption.AddressableScripts;

public static class ModLoadCache
{
  public static readonly Dictionary<string, ModLoadCache.LocationCache> CacheLocations = new Dictionary<string, ModLoadCache.LocationCache>();
  public static readonly List<LiveryMetaData> SkinMetaData = new List<LiveryMetaData>();
  public static bool HasSkinMetaData;

  public static void Clear()
  {
    ColorLog<ModLoadCache.LocationCache>.Info("Clearing Addressable Location cache");
    ModLoadCache.CacheLocations.Clear();
    ModLoadCache.SkinMetaData.Clear();
    ModLoadCache.HasSkinMetaData = false;
  }

  public class LocationCache
  {
    public bool TaskPending;
    public IResourceLocation Location;
  }
}

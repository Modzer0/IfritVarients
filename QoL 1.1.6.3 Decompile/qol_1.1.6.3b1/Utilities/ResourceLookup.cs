// Decompiled with JetBrains decompiler
// Type: qol.Utilities.ResourceLookup
// Assembly: qol, Version=1.1.6.3, Culture=neutral, PublicKeyToken=null
// MVID: 3B6539EA-E38C-45EE-8FFF-FC58CC42BADC
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\BepInEx\plugins\qol_1.1.6.3b1.dll

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#nullable disable
namespace qol.Utilities;

public static class ResourceLookup
{
  private static readonly Dictionary<(Type, string), UnityEngine.Object> _resourceCache = new Dictionary<(Type, string), UnityEngine.Object>();

  public static T Find<T>(string name, bool useCache = true) where T : UnityEngine.Object
  {
    if (string.IsNullOrEmpty(name))
      return default (T);
    (Type, string) key = (typeof (T), name.ToLowerInvariant());
    UnityEngine.Object @object;
    if (useCache && ResourceLookup._resourceCache.TryGetValue(key, out @object))
      return @object as T;
    T obj1 = ((IEnumerable<T>) Resources.FindObjectsOfTypeAll<T>()).FirstOrDefault<T>((Func<T, bool>) (obj => obj.name.Equals(name, StringComparison.OrdinalIgnoreCase)));
    if ((UnityEngine.Object) obj1 != (UnityEngine.Object) null)
      ResourceLookup._resourceCache[key] = (UnityEngine.Object) obj1;
    return obj1;
  }

  public static WeaponMount FindWeaponMount(string name) => ResourceLookup.Find<WeaponMount>(name);

  public static WeaponInfo FindWeaponInfo(string name) => ResourceLookup.Find<WeaponInfo>(name);

  public static AircraftDefinition FindAircraftDefinition(string name)
  {
    return ResourceLookup.Find<AircraftDefinition>(name);
  }

  public static AircraftParameters FindAircraftParameters(string name)
  {
    return ResourceLookup.Find<AircraftParameters>(name);
  }

  public static MissileDefinition FindMissileDefinition(string name)
  {
    return ResourceLookup.Find<MissileDefinition>(name);
  }

  public static VehicleDefinition FindVehicleDefinition(string name)
  {
    return ResourceLookup.Find<VehicleDefinition>(name);
  }

  public static ShipDefinition FindShipDefinition(string name)
  {
    return ResourceLookup.Find<ShipDefinition>(name);
  }

  public static BuildingDefinition FindBuildingDefinition(string name)
  {
    return ResourceLookup.Find<BuildingDefinition>(name);
  }

  public static AudioClip FindAudioClip(string name) => ResourceLookup.Find<AudioClip>(name);

  public static void ClearCache() => ResourceLookup._resourceCache.Clear();
}

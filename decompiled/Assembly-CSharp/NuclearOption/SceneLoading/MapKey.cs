// Decompiled with JetBrains decompiler
// Type: NuclearOption.SceneLoading.MapKey
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;

#nullable disable
namespace NuclearOption.SceneLoading;

[Serializable]
public struct MapKey : IEquatable<MapKey>
{
  public MapKey.KeyType Type;
  [HideInInspector]
  public string TypeName;
  public string Path;
  [NonSerialized]
  public bool SkipChecks;

  public MapKey(MapKey.KeyType type, string path)
  {
    this.Type = type;
    this.TypeName = type.ToString();
    this.Path = path;
    this.SkipChecks = false;
  }

  public static MapKey GameWorldPrefab(string path)
  {
    return new MapKey(MapKey.KeyType.GameWorldPrefab, path);
  }

  public static MapKey BuiltinScene(string path) => new MapKey(MapKey.KeyType.BuiltinScene, path);

  public static MapKey AddressableBuiltin(string path) => throw new NotImplementedException();

  public static MapKey AddressableAppData(string path) => throw new NotImplementedException();

  public static MapKey AddressableWorkshop(string path) => throw new NotImplementedException();

  public override string ToString() => $"({this.Type},{this.Path})";

  public readonly bool Equals(MapKey other) => this.Type == other.Type && this.Path == other.Path;

  public readonly bool IsDefault() => new MapKey().Equals(this);

  [Serializable]
  public enum KeyType : byte
  {
    None,
    GameWorldPrefab,
    BuiltinScene,
  }
}

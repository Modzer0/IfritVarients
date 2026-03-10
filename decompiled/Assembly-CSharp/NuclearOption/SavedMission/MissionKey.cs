// Decompiled with JetBrains decompiler
// Type: NuclearOption.SavedMission.MissionKey
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Cysharp.Threading.Tasks;
using Steamworks;
using System;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;

#nullable disable
namespace NuclearOption.SavedMission;

public readonly struct MissionKey(
  string key,
  string name,
  PublishedFileId_t? workshopId,
  MissionGroup group) : IEquatable<MissionKey>
{
  public readonly string Name = name;
  public readonly string Key = key;
  public readonly PublishedFileId_t? WorkshopId = workshopId;
  public readonly MissionGroup Group = group;

  public bool IsValid() => !string.IsNullOrEmpty(this.Key);

  public MissionKey(string name, MissionGroup group)
    : this(name, name, new PublishedFileId_t?(), group)
  {
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public bool TryGetJson(out string json) => this.Group.TryGetJson(this.Key, out json);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public UniTask<Sprite> GetPreview(CancellationToken token = default (CancellationToken))
  {
    return this.Group.GetPreview(this.Key, token);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public bool TryLoad(out Mission mission, out string error)
  {
    return MissionSaveLoad.TryLoad(this, out mission, out error);
  }

  public override bool Equals(object obj) => obj is MissionKey other && this.Equals(other);

  public bool Equals(MissionKey other)
  {
    return this.Name == other.Name && this.Key == other.Key && this.Group == other.Group;
  }

  public override int GetHashCode()
  {
    return HashCode.Combine<string, string, MissionGroup>(this.Name, this.Key, this.Group);
  }

  public void ThrowIfInvalid()
  {
    if (string.IsNullOrEmpty(this.Name))
      throw new ArgumentException("Mission had no name");
    if (string.IsNullOrEmpty(this.Key))
      throw new ArgumentException("Mission had no key");
    if (this.Group == null)
      throw new ArgumentException("Mission had no load group");
  }

  public override string ToString()
  {
    string str = this.Group?.Name ?? "NULL";
    return this.Key == this.Name ? $"[{str},{this.Key}]" : $"[{str},{this.Key},{this.Name}]";
  }
}

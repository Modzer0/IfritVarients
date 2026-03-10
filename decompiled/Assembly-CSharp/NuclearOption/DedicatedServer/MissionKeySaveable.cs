// Decompiled with JetBrains decompiler
// Type: NuclearOption.DedicatedServer.MissionKeySaveable
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.SavedMission;
using System;
using System.Collections.Generic;
using UnityEngine;

#nullable disable
namespace NuclearOption.DedicatedServer;

[Serializable]
public struct MissionKeySaveable : IEquatable<MissionKeySaveable>
{
  public static Dictionary<string, MissionGroup> TypeNameToGroup = new Dictionary<string, MissionGroup>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase)
  {
    {
      "",
      (MissionGroup) MissionGroup.All
    },
    {
      "Default",
      (MissionGroup) MissionGroup.Default
    },
    {
      "Tutorial",
      (MissionGroup) MissionGroup.Tutorial
    },
    {
      "BuiltIn",
      (MissionGroup) MissionGroup.BuiltIn
    },
    {
      "User",
      (MissionGroup) MissionGroup.User
    },
    {
      "Workshop",
      (MissionGroup) MissionGroup.Workshop
    }
  };
  public string Group;
  public string Name;

  public bool TryGetKey(out MissionKey missionKey)
  {
    missionKey = new MissionKey();
    if (string.IsNullOrEmpty(this.Name))
    {
      Debug.LogError((object) "Mission name should not be empty");
      return false;
    }
    MissionGroup group;
    if (MissionKeySaveable.TypeNameToGroup.TryGetValue(this.Group ?? "", out group))
    {
      missionKey = new MissionKey(this.Name, group);
      return true;
    }
    Debug.LogError((object) $"Group '{this.Group}' is invalid");
    return false;
  }

  public override bool Equals(object obj) => obj is MissionKeySaveable other && this.Equals(other);

  public override int GetHashCode()
  {
    return (17 * 23 + this.Group.GetHashCode()) * 23 + this.Name.GetHashCode();
  }

  public bool Equals(MissionKeySaveable other)
  {
    return this.Group == other.Group && this.Name == other.Name;
  }

  public override string ToString() => $"({this.Group},{this.Name})";
}

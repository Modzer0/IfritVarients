// Decompiled with JetBrains decompiler
// Type: NuclearOption.SavedMission.MissionTag
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.Networking.Lobbies;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#nullable disable
namespace NuclearOption.SavedMission;

[Serializable]
public struct MissionTag(string tag, Color color, int order) : 
  IEquatable<MissionTag>,
  IEquatable<string>
{
  public string Tag = tag;
  public Color Color = color;
  public int SortOrder = order;
  private const float SATURATION = 0.35f;
  private const float BRIGHTNESS = 0.8f;
  public static readonly MissionTag SinglePlayer = new MissionTag("Single Player", 0.17f, 1);
  public static readonly MissionTag Multiplayer = new MissionTag(nameof (Multiplayer), 0.67f, 2);
  public static readonly MissionTag PVE = new MissionTag("PvE", 0.33f, 3);
  public static readonly MissionTag PVP = new MissionTag("PvP", 0.0f, 4);
  public static readonly MissionTag Dawn = new MissionTag(nameof (Dawn), 0.4f, 5);
  public static readonly MissionTag Day = new MissionTag(nameof (Day), 0.55f, 6);
  public static readonly MissionTag Dusk = new MissionTag(nameof (Dusk), 0.75f, 7);
  public static readonly MissionTag Night = new MissionTag(nameof (Night), 0.83f, 8);
  public static readonly MissionTag[] InternalTags = new MissionTag[8]
  {
    MissionTag.SinglePlayer,
    MissionTag.Multiplayer,
    MissionTag.PVE,
    MissionTag.PVP,
    MissionTag.Dawn,
    MissionTag.Day,
    MissionTag.Dusk,
    MissionTag.Night
  };

  public override int GetHashCode() => this.Tag.GetHashCode();

  public override bool Equals(object obj) => obj is MissionTag other && this.Equals(other);

  public bool Equals(MissionTag other) => this.Tag == other.Tag;

  public bool Equals(string other) => this.Tag == other;

  private MissionTag(string tag, float hue, int order)
    : this(tag, Color.HSVToRGB(hue, 0.35f, 0.8f), order)
  {
  }

  public static void AddAutoTags(Mission mission)
  {
    MissionQuickLoad mission1 = JsonUtility.FromJson<MissionQuickLoad>(JsonUtility.ToJson((object) mission));
    MissionTag.AddAutoTags(mission1);
    List<MissionTag> tags = mission.missionSettings.Tags;
    foreach (MissionTag tag in mission1.missionSettings.Tags)
      MissionTag.AddSet(tags, tag);
  }

  public static void AddAutoTags(MissionQuickLoad mission)
  {
    MissionSettings missionSettings = mission.missionSettings;
    if (missionSettings.Tags == null)
      missionSettings.Tags = new List<MissionTag>();
    List<MissionTag> tags = mission.missionSettings.Tags;
    PlayerMode playerMode = mission.missionSettings.playerMode;
    if (playerMode == PlayerMode.SingleAndMultiplayer || playerMode == PlayerMode.Singleplayer)
      MissionTag.AddSet(tags, MissionTag.SinglePlayer);
    if (playerMode == PlayerMode.SingleAndMultiplayer || playerMode == PlayerMode.Multiplayer)
      MissionTag.AddSet(tags, MissionTag.Multiplayer);
    float timeOfDay = mission.environment.timeOfDay;
    if ((double) timeOfDay < 5.0)
      MissionTag.AddSet(tags, MissionTag.Night);
    else if ((double) timeOfDay < 8.0)
      MissionTag.AddSet(tags, MissionTag.Dawn);
    else if ((double) timeOfDay < 16.5)
      MissionTag.AddSet(tags, MissionTag.Day);
    else if ((double) timeOfDay < 18.5)
      MissionTag.AddSet(tags, MissionTag.Dusk);
    else
      MissionTag.AddSet(tags, MissionTag.Night);
    if (MissionTag.GetPvpType(mission) == MissionPvpType.Pve)
      MissionTag.AddSet(tags, MissionTag.PVE);
    else
      MissionTag.AddSet(tags, MissionTag.PVP);
  }

  private static void AddSet(List<MissionTag> allTags, MissionTag newTag)
  {
    if (allTags.Any<MissionTag>((Func<MissionTag, bool>) (x => x.Tag == newTag.Tag)))
      return;
    allTags.Add(newTag);
  }

  public static MissionPvpType GetPvpType(MissionQuickLoad mission)
  {
    foreach (QuickLoadMissionFaction faction in mission.factions)
    {
      if (faction.preventJoin)
        return MissionPvpType.Pve;
    }
    return MissionPvpType.Pvp;
  }

  public static MissionPvpType GetPvpType(Mission mission)
  {
    foreach (MissionFaction faction in mission.factions)
    {
      if (faction.preventJoin)
        return MissionPvpType.Pve;
    }
    return MissionPvpType.Pvp;
  }

  public static string GetPvpTypeLobbyString(Mission mission)
  {
    return MissionTag.GetPvpTypeLobbyString(MissionTag.GetPvpType(mission));
  }

  public static string GetPvpTypeLobbyString(MissionPvpType lobbyType)
  {
    return ((int) lobbyType).ToString();
  }
}

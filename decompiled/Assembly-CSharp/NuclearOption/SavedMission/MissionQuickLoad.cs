// Decompiled with JetBrains decompiler
// Type: NuclearOption.SavedMission.MissionQuickLoad
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.SceneLoading;
using System;
using System.Collections.Generic;

#nullable disable
namespace NuclearOption.SavedMission;

[Serializable]
public struct MissionQuickLoad
{
  [Obsolete("WorkshopId has been moved to workshop.json", true)]
  public ulong WorkshopId;
  public MapKey MapKey;
  public MissionSettings missionSettings;
  public MissionEnvironment environment;
  public List<QuickLoadMissionFaction> factions;
  [NonSerialized]
  public string Name;
  [NonSerialized]
  public MissionKey? LoadKey;

  public void AfterLoad(MissionKey key)
  {
    this.LoadKey = new MissionKey?(key);
    this.Name = key.Name;
  }
}

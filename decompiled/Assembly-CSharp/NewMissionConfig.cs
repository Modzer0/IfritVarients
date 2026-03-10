// Decompiled with JetBrains decompiler
// Type: NewMissionConfig
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.SavedMission;
using NuclearOption.SceneLoading;
using System.Collections.Generic;

#nullable disable
public struct NewMissionConfig(
  string name,
  MapKey map,
  PlayerMode playerMode,
  List<string> joinableFactions)
{
  public string Name = name;
  public MapKey Map = map;
  public PlayerMode PlayerMode = playerMode;
  public List<string> JoinableFactions = joinableFactions;

  public bool CanJoinAllFactions => this.JoinableFactions == null;

  public static NewMissionConfig DefaultMission()
  {
    return new NewMissionConfig("new mission", new MapKey(), PlayerMode.SingleAndMultiplayer, new List<string>()
    {
      "Boscali",
      "Primeva"
    });
  }
}

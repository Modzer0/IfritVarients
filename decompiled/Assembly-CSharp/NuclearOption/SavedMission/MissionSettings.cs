// Decompiled with JetBrains decompiler
// Type: NuclearOption.SavedMission.MissionSettings
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.SavedMission.ObjectiveV2;
using RoadPathfinding;
using System;
using System.Collections.Generic;
using UnityEngine;

#nullable disable
namespace NuclearOption.SavedMission;

[Serializable]
public class MissionSettings
{
  public string description;
  public List<MissionTag> Tags = new List<MissionTag>();
  public PlayerMode playerMode;
  public bool allowRespawn;
  public int playerStartingRank;
  public float rankMultiplier = 1f;
  public float successfulSortieBonus = 0.25f;
  public float nuclearEscalationThreshold;
  public float strategicEscalationThreshold;
  public int minRankTacticalWarhead;
  public int minRankStrategicWarhead;
  public Override<PositionRotation> cameraStartPosition;
  public RoadNetwork missionRoads = new RoadNetwork();
  public int wrecksMaxNumber;
  public float wrecksDecayTime;

  public void SetMinimumStartingRank(int rank)
  {
    this.playerStartingRank = Mathf.Max(this.playerStartingRank, rank);
  }
}

// Decompiled with JetBrains decompiler
// Type: NuclearOption.SavedMission.MissionFaction
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.SavedMission.ObjectiveV2;
using System;
using System.Collections.Generic;

#nullable disable
namespace NuclearOption.SavedMission;

[Serializable]
public class MissionFaction
{
  public string factionName;
  public bool preventJoin;
  public bool preventDonation;
  public List<FactionSupply> supplies = new List<FactionSupply>();
  public float startingBalance;
  public float playerJoinAllowance = 20f;
  public float playerTaxRate = 0.2f;
  public float regularIncome = 5f;
  public float excessFundsDistributePercent = 0.25f;
  public float killReward = 1f;
  public int startingWarheads;
  public int reserveWarheads;
  public int reserveAirframes;
  public int extraReservesPerPlayer = 1;
  public int AIAircraftLimit = 6;
  public float reduceAIPerFriendlyPlayer = 1f;
  public float addAIPerEnemyPlayer = 1f;
  [Obsolete("Use V2 instead", true)]
  public List<MissionObjective> objectives = new List<MissionObjective>();
  public Restrictions restrictions = new Restrictions();
  public Override<PositionRotation> cameraStartPosition;
  [NonSerialized]
  public FactionHQ FactionHQ;

  public MissionFaction()
  {
  }

  public MissionFaction(string factionName) => this.factionName = factionName;
}

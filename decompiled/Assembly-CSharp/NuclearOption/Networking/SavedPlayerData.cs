// Decompiled with JetBrains decompiler
// Type: NuclearOption.Networking.SavedPlayerData
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System.Collections.Generic;

#nullable disable
namespace NuclearOption.Networking;

public class SavedPlayerData
{
  public bool Rejoined;
  public FactionHQ Faction;
  public float Score;
  public int Rank;
  public float Allocation;
  public readonly List<OwnedAirframe> OwnedAirframes = new List<OwnedAirframe>();

  public void Save(Player player)
  {
    this.Faction = player.HQ;
    this.Score = player.PlayerScore;
    this.Rank = player.PlayerRank;
    this.Allocation = player.Allocation;
    this.OwnedAirframes.Clear();
    this.OwnedAirframes.AddRange((IEnumerable<OwnedAirframe>) player.OwnedAirframes);
  }

  public void OnRejoinFaction(Player player)
  {
    player.AddAllocation(this.Allocation);
    if ((double) this.Allocation > 0.0)
      this.Faction.AddFunds(-this.Allocation);
    player.SetScore(this.Score);
    player.SetRank(this.Rank, true);
    player.OwnedAirframes.AddRange((IEnumerable<OwnedAirframe>) this.OwnedAirframes);
  }

  public void Clear()
  {
    this.Faction = (FactionHQ) null;
    this.Rejoined = false;
    this.Score = 0.0f;
    this.Rank = 0;
    this.Allocation = 0.0f;
    this.OwnedAirframes.Clear();
  }
}

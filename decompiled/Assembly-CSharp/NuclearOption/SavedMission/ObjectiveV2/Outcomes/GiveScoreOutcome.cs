// Decompiled with JetBrains decompiler
// Type: NuclearOption.SavedMission.ObjectiveV2.Outcomes.GiveScoreOutcome
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.MissionEditorScripts;
using NuclearOption.Networking;
using System;
using UnityEngine.UI;

#nullable disable
namespace NuclearOption.SavedMission.ObjectiveV2.Outcomes;

public class GiveScoreOutcome : Outcome
{
  private readonly ValueWrapperBool bothFactions = new ValueWrapperBool();
  private readonly ValueWrapperFloat playerFunds = new ValueWrapperFloat();
  private readonly ValueWrapperFloat factionFunds = new ValueWrapperFloat();
  private readonly ValueWrapperFloat playerScore = new ValueWrapperFloat();
  private readonly ValueWrapperInt rank = new ValueWrapperInt();
  private readonly ValueWrapperFloat factionScore = new ValueWrapperFloat();
  private ChangeType playerFundsType;
  private ChangeType factionFundsType;
  private ChangeType playerScoreType;
  private ChangeType rankType;
  private ChangeType factionScoreType;

  protected override void WriteOutcome(ReadWriteObjective writer)
  {
    writer.Bool(ref this.bothFactions.GetValueRef());
    writer.Enum<ChangeType>(ref this.playerFundsType);
    writer.Float(ref this.playerFunds.GetValueRef());
    writer.Enum<ChangeType>(ref this.factionFundsType);
    writer.Float(ref this.factionFunds.GetValueRef());
    writer.Enum<ChangeType>(ref this.playerScoreType);
    writer.Float(ref this.playerScore.GetValueRef());
    writer.Enum<ChangeType>(ref this.rankType);
    writer.Int(ref this.rank.GetValueRef());
    writer.Enum<ChangeType>(ref this.factionScoreType);
    writer.Float(ref this.factionScore.GetValueRef());
  }

  public override void ReferenceDestroyed(ISaveableReference reference)
  {
  }

  public override void Complete(Objective completedObjective)
  {
    if ((bool) (ValueWrapper<bool>) this.bothFactions)
    {
      foreach (FactionHQ faction in FactionRegistry.HQLookup.Values)
        this.RewardFaction(faction);
    }
    else
    {
      FactionHQ factionHq = completedObjective.FactionHQ;
      if (!((UnityEngine.Object) factionHq != (UnityEngine.Object) null))
        return;
      this.RewardFaction(factionHq);
    }
  }

  private void RewardFaction(FactionHQ faction)
  {
    ChangeTypeHelper.ChangeValue(this.factionFunds.Value, this.factionFundsType, faction.factionFunds, (Action<float>) (v => faction.SetFunds(v)));
    ChangeTypeHelper.ChangeValue(this.factionScore.Value, this.factionScoreType, faction.factionScore, (Action<float>) (v => faction.SetScore(v)));
    foreach (Player player1 in faction.GetPlayers(false))
    {
      Player player = player1;
      ChangeTypeHelper.ChangeValue(this.playerFunds.Value, this.playerFundsType, player.Allocation, (Action<float>) (v => player.SetAllocation(v)));
      ChangeTypeHelper.ChangeValue(this.playerScore.Value, this.playerScoreType, player.PlayerScore, (Action<float>) (v => player.SetScore(v)));
      ChangeTypeHelper.ChangeValue((float) this.rank.Value, this.rankType, (float) player.PlayerRank, (Action<float>) (v => player.SetRank((int) v, true)));
    }
  }

  public override void DrawData(DataDrawer drawer)
  {
    drawer.CheckGroupBox();
    float num = drawer.Width.Value;
    float fieldWidth = (float) ((double) num * 2.0 / 3.0 - 10.0);
    float typeWidth = (float) ((double) num * 1.0 / 3.0);
    drawer.InstantiateWithParent<BoolDataField>(drawer.Prefabs.BoolFieldPrefab).Setup("Both Factions", (IValueWrapper<bool>) this.bothFactions);
    drawer.HorizontalGroup((Action<HorizontalLayoutGroup>) (group =>
    {
      group.spacing = 10f;
      FloatDataField comp1 = drawer.InstantiateWithParent<FloatDataField>(drawer.Prefabs.FloatFieldPrefab);
      comp1.Setup("Player Funds", (IValueWrapper<float>) this.playerFunds);
      comp1.SetRectWidth(fieldWidth);
      DropdownDataField comp2 = drawer.DrawEnum<ChangeType>("", (int) this.playerFundsType, (Action<int>) (v => this.playerFundsType = (ChangeType) v));
      comp2.HideLabel();
      comp2.FieldLayout.minWidth = typeWidth;
      comp2.SetRectWidth(typeWidth);
    }));
    drawer.HorizontalGroup((Action<HorizontalLayoutGroup>) (group =>
    {
      group.spacing = 10f;
      FloatDataField comp3 = drawer.InstantiateWithParent<FloatDataField>(drawer.Prefabs.FloatFieldPrefab);
      comp3.Setup("Faction Funds", (IValueWrapper<float>) this.factionFunds);
      comp3.SetRectWidth(fieldWidth);
      DropdownDataField comp4 = drawer.DrawEnum<ChangeType>("", (int) this.factionFundsType, (Action<int>) (v => this.factionFundsType = (ChangeType) v));
      comp4.HideLabel();
      comp4.FieldLayout.minWidth = typeWidth;
      comp4.SetRectWidth(typeWidth);
    }));
    drawer.HorizontalGroup((Action<HorizontalLayoutGroup>) (group =>
    {
      group.spacing = 10f;
      FloatDataField comp5 = drawer.InstantiateWithParent<FloatDataField>(drawer.Prefabs.FloatFieldPrefab);
      comp5.Setup("Player Score", (IValueWrapper<float>) this.playerScore);
      comp5.SetRectWidth(fieldWidth);
      DropdownDataField comp6 = drawer.DrawEnum<ChangeType>("", (int) this.playerScoreType, (Action<int>) (v => this.playerScoreType = (ChangeType) v));
      comp6.HideLabel();
      comp6.FieldLayout.minWidth = typeWidth;
      comp6.SetRectWidth(typeWidth);
    }));
    drawer.HorizontalGroup((Action<HorizontalLayoutGroup>) (group =>
    {
      group.spacing = 10f;
      FloatDataField comp7 = drawer.InstantiateWithParent<FloatDataField>(drawer.Prefabs.FloatFieldPrefab);
      comp7.Setup("Faction Score", (IValueWrapper<float>) this.factionScore);
      comp7.SetRectWidth(fieldWidth);
      DropdownDataField comp8 = drawer.DrawEnum<ChangeType>("", (int) this.factionScoreType, (Action<int>) (v => this.factionScoreType = (ChangeType) v));
      comp8.HideLabel();
      comp8.FieldLayout.minWidth = typeWidth;
      comp8.SetRectWidth(typeWidth);
    }));
    drawer.HorizontalGroup((Action<HorizontalLayoutGroup>) (group =>
    {
      group.spacing = 10f;
      FloatDataField comp9 = drawer.InstantiateWithParent<FloatDataField>(drawer.Prefabs.FloatFieldPrefab);
      comp9.Setup("Rank", this.rank);
      comp9.SetRectWidth(fieldWidth);
      DropdownDataField comp10 = drawer.DrawEnum<ChangeType>("", (int) this.rankType, (Action<int>) (v => this.rankType = (ChangeType) v));
      comp10.HideLabel();
      comp10.FieldLayout.minWidth = typeWidth;
      comp10.SetRectWidth(typeWidth);
    }));
  }
}

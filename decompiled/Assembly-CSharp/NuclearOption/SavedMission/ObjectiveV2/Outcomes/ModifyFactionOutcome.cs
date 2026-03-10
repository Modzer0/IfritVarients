// Decompiled with JetBrains decompiler
// Type: NuclearOption.SavedMission.ObjectiveV2.Outcomes.ModifyFactionOutcome
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.MissionEditorScripts;
using System;

#nullable disable
namespace NuclearOption.SavedMission.ObjectiveV2.Outcomes;

public class ModifyFactionOutcome : Outcome
{
  private static readonly MissionFaction defaultSettings = new MissionFaction("Defaults");
  private readonly ValueWrapperBool bothFactions = new ValueWrapperBool();
  private readonly ValueWrapperOverride<float> excessFundsThreshold = new ValueWrapperOverride<float>();
  private readonly ValueWrapperOverride<float> playerJoinAllowance = new ValueWrapperOverride<float>();
  private readonly ValueWrapperOverride<float> playerTaxRate = new ValueWrapperOverride<float>();
  private readonly ValueWrapperOverride<float> regularIncome = new ValueWrapperOverride<float>();
  private readonly ValueWrapperOverride<float> killReward = new ValueWrapperOverride<float>();
  private readonly ValueWrapperOverride<bool> preventDonation = new ValueWrapperOverride<bool>();
  private readonly ValueWrapperOverride<float> aiAircraftLimit = new ValueWrapperOverride<float>();
  private readonly ValueWrapperOverride<float> reduceAIPerFriendlyPlayer = new ValueWrapperOverride<float>();
  private readonly ValueWrapperOverride<float> addAIPerEnemyPlayer = new ValueWrapperOverride<float>();
  private readonly ValueWrapperOverride<float> warheadsReserve = new ValueWrapperOverride<float>();
  private readonly ValueWrapperOverride<float> reserveAirframes = new ValueWrapperOverride<float>();
  private readonly ValueWrapperOverride<float> extraReservesPerPlayer = new ValueWrapperOverride<float>();
  private readonly ValueWrapperOverride<float> excessFundsDistributePercent = new ValueWrapperOverride<float>();

  protected override void WriteOutcome(ReadWriteObjective writer)
  {
    writer.Bool(ref this.bothFactions.GetValueRef());
    writer.Override<float>(ref this.excessFundsThreshold.GetValueRef(), new ReadWriteObjective.ReadWriteValue<float>(writer.Float));
    writer.Override<float>(ref this.playerJoinAllowance.GetValueRef(), new ReadWriteObjective.ReadWriteValue<float>(writer.Float));
    writer.Override<float>(ref this.playerTaxRate.GetValueRef(), new ReadWriteObjective.ReadWriteValue<float>(writer.Float));
    writer.Override<float>(ref this.regularIncome.GetValueRef(), new ReadWriteObjective.ReadWriteValue<float>(writer.Float));
    writer.Override<float>(ref this.killReward.GetValueRef(), new ReadWriteObjective.ReadWriteValue<float>(writer.Float));
    writer.Override<bool>(ref this.preventDonation.GetValueRef(), new ReadWriteObjective.ReadWriteValue<bool>(writer.Bool));
    writer.Override<float>(ref this.aiAircraftLimit.GetValueRef(), new ReadWriteObjective.ReadWriteValue<float>(writer.Float));
    writer.Override<float>(ref this.reduceAIPerFriendlyPlayer.GetValueRef(), new ReadWriteObjective.ReadWriteValue<float>(writer.Float));
    writer.Override<float>(ref this.addAIPerEnemyPlayer.GetValueRef(), new ReadWriteObjective.ReadWriteValue<float>(writer.Float));
    writer.Override<float>(ref this.warheadsReserve.GetValueRef(), new ReadWriteObjective.ReadWriteValue<float>(writer.Float));
    writer.Override<float>(ref this.reserveAirframes.GetValueRef(), new ReadWriteObjective.ReadWriteValue<float>(writer.Float));
    writer.Override<float>(ref this.extraReservesPerPlayer.GetValueRef(), new ReadWriteObjective.ReadWriteValue<float>(writer.Float));
    writer.Override<float>(ref this.excessFundsDistributePercent.GetValueRef(), new ReadWriteObjective.ReadWriteValue<float>(writer.Float));
    if (writer.mode != ReadWriteObjective.Mode.Read)
      return;
    SetIfNoOverride<float>(this.excessFundsThreshold, ModifyFactionOutcome.defaultSettings.startingBalance);
    SetIfNoOverride<float>(this.playerJoinAllowance, ModifyFactionOutcome.defaultSettings.playerJoinAllowance);
    SetIfNoOverride<float>(this.playerTaxRate, ModifyFactionOutcome.defaultSettings.playerTaxRate);
    SetIfNoOverride<float>(this.regularIncome, ModifyFactionOutcome.defaultSettings.regularIncome);
    SetIfNoOverride<float>(this.excessFundsDistributePercent, ModifyFactionOutcome.defaultSettings.excessFundsDistributePercent);
    SetIfNoOverride<float>(this.killReward, ModifyFactionOutcome.defaultSettings.killReward);
    SetIfNoOverride<bool>(this.preventDonation, ModifyFactionOutcome.defaultSettings.preventDonation);
    SetIfNoOverride<float>(this.aiAircraftLimit, (float) ModifyFactionOutcome.defaultSettings.AIAircraftLimit);
    SetIfNoOverride<float>(this.reduceAIPerFriendlyPlayer, ModifyFactionOutcome.defaultSettings.reduceAIPerFriendlyPlayer);
    SetIfNoOverride<float>(this.addAIPerEnemyPlayer, ModifyFactionOutcome.defaultSettings.addAIPerEnemyPlayer);
    SetIfNoOverride<float>(this.warheadsReserve, (float) ModifyFactionOutcome.defaultSettings.reserveWarheads);
    SetIfNoOverride<float>(this.reserveAirframes, (float) ModifyFactionOutcome.defaultSettings.reserveAirframes);
    SetIfNoOverride<float>(this.extraReservesPerPlayer, (float) ModifyFactionOutcome.defaultSettings.extraReservesPerPlayer);

    static void SetIfNoOverride<T>(ValueWrapperOverride<T> wrapper, T value) where T : IEquatable<T>
    {
      if (wrapper.Value.IsOverride)
        return;
      wrapper.SetValue(new Override<T>(false, value), (object) null, true);
    }
  }

  public override void ReferenceDestroyed(ISaveableReference reference)
  {
  }

  public override void Complete(Objective completedObjective)
  {
    if ((bool) (ValueWrapper<bool>) this.bothFactions)
    {
      foreach (FactionHQ faction in FactionRegistry.HQLookup.Values)
        this.ApplySettings(faction);
    }
    else
    {
      FactionHQ factionHq = completedObjective.FactionHQ;
      if (!((UnityEngine.Object) factionHq != (UnityEngine.Object) null))
        return;
      this.ApplySettings(factionHq);
    }
  }

  private void ApplySettings(FactionHQ faction)
  {
    if (this.excessFundsThreshold.Value.IsOverride)
      faction.excessFundsThreshold = this.excessFundsThreshold.Value.Value;
    if (this.playerJoinAllowance.Value.IsOverride)
      faction.playerJoinAllowance = this.playerJoinAllowance.Value.Value;
    if (this.playerTaxRate.Value.IsOverride)
      faction.playerTaxRate = this.playerTaxRate.Value.Value;
    if (this.regularIncome.Value.IsOverride)
      faction.regularIncome = this.regularIncome.Value.Value;
    if (this.killReward.Value.IsOverride)
      faction.killReward = this.killReward.Value.Value;
    if (this.excessFundsDistributePercent.Value.IsOverride)
      faction.excessFundsDistributePercent = this.excessFundsDistributePercent.Value.Value;
    if (this.preventDonation.Value.IsOverride)
      faction.NetworkpreventDonation = this.preventDonation.Value.Value;
    if (this.aiAircraftLimit.Value.IsOverride)
      faction.AIAircraftLimit = (int) this.aiAircraftLimit.Value.Value;
    if (this.reduceAIPerFriendlyPlayer.Value.IsOverride)
      faction.reduceAIPerFriendlyPlayer = this.reduceAIPerFriendlyPlayer.Value.Value;
    if (this.addAIPerEnemyPlayer.Value.IsOverride)
      faction.addAIPerEnemyPlayer = this.addAIPerEnemyPlayer.Value.Value;
    if (this.warheadsReserve.Value.IsOverride)
      faction.warheadsReserve = (int) this.warheadsReserve.Value.Value;
    if (this.reserveAirframes.Value.IsOverride)
      faction.reserveAirframes = (int) this.reserveAirframes.Value.Value;
    if (!this.extraReservesPerPlayer.Value.IsOverride)
      return;
    faction.extraReservesPerPlayer = (int) this.extraReservesPerPlayer.Value.Value;
  }

  public override void DrawData(DataDrawer drawer)
  {
    drawer.CheckGroupBox();
    drawer.InstantiateWithParent<BoolDataField>(drawer.Prefabs.BoolFieldPrefab).Setup("Both Factions", (IValueWrapper<bool>) this.bothFactions);
    drawer.DrawHeader("Funds");
    drawer.DrawOverride<float, FloatDataField>("Excess Funds", this.excessFundsThreshold, drawer.Prefabs.FloatFieldPrefab);
    drawer.DrawOverride<float, FloatDataField>("Join Allowance", this.playerJoinAllowance, drawer.Prefabs.FloatFieldPrefab);
    drawer.DrawOverride<float, FloatDataField>("Player Tax Rate", this.playerTaxRate, drawer.Prefabs.FloatFieldPrefab);
    drawer.DrawOverride<float, FloatDataField>("Regular Income", this.regularIncome, drawer.Prefabs.FloatFieldPrefab);
    drawer.DrawOverride<float, FloatDataField>("Distribute Excess Funds", this.excessFundsDistributePercent, drawer.Prefabs.FloatFieldPrefab);
    drawer.DrawOverride<float, FloatDataField>("Kill Reward", this.killReward, drawer.Prefabs.FloatFieldPrefab);
    drawer.DrawOverride<bool, BoolDataField>("Prevent Donation", this.preventDonation, drawer.Prefabs.BoolFieldPrefab);
    drawer.DrawHeader("Warheads");
    drawer.DrawOverride<float, FloatDataField>("Warheads Reserve", this.warheadsReserve, drawer.Prefabs.FloatFieldPrefab).Item2.SetContentType(true);
    drawer.DrawHeader("Ai limits");
    drawer.DrawOverride<float, FloatDataField>("AI Aircraft", this.aiAircraftLimit, drawer.Prefabs.FloatFieldPrefab).Item2.SetContentType(true);
    drawer.DrawOverride<float, FloatDataField>("Reduce AI Per Friendly", this.reduceAIPerFriendlyPlayer, drawer.Prefabs.FloatFieldPrefab);
    drawer.DrawOverride<float, FloatDataField>("Add AI Per Enemy", this.addAIPerEnemyPlayer, drawer.Prefabs.FloatFieldPrefab);
    drawer.DrawHeader("Reserve Airframes");
    drawer.DrawOverride<float, FloatDataField>("Baseline Reserve", this.reserveAirframes, drawer.Prefabs.FloatFieldPrefab).Item2.SetContentType(true);
    drawer.DrawOverride<float, FloatDataField>("Reserves Per Player", this.extraReservesPerPlayer, drawer.Prefabs.FloatFieldPrefab).Item2.SetContentType(true);
    foreach (DataField componentsInChild in drawer.Parent.GetComponentsInChildren<DataField>())
    {
      componentsInChild.LabelLayout.minWidth = 160f;
      componentsInChild.FieldLayout.minWidth = 140f;
    }
  }
}

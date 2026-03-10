// Decompiled with JetBrains decompiler
// Type: NuclearOption.SavedMission.ObjectiveV2.SavedOutcome
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.SavedMission.ObjectiveV2.Outcomes;
using System;
using System.Collections.Generic;
using System.ComponentModel;

#nullable disable
namespace NuclearOption.SavedMission.ObjectiveV2;

[Serializable]
public struct SavedOutcome
{
  public string UniqueName;
  public OutcomeType Type;
  public string TypeName;
  public List<ObjectiveData> Data;

  public SavedOutcome(string name, OutcomeType type)
    : this()
  {
    this.UniqueName = name;
    this.Type = type;
    this.TypeName = type.ToString();
  }

  public static Outcome Create(OutcomeType type)
  {
    switch (type)
    {
      case OutcomeType.StartObjective:
        return (Outcome) new StartObjectiveOutcome();
      case OutcomeType.StopOrCompleteObjective:
        return (Outcome) new CompleteObjectiveOutcome();
      case OutcomeType.ShowMessage:
        return (Outcome) new ShowMessageOutcome();
      case OutcomeType.GiveScore:
        return (Outcome) new GiveScoreOutcome();
      case OutcomeType.SpawnUnit:
        return (Outcome) new SpawnUnitOutcome();
      case OutcomeType.RemoveUnit:
        return (Outcome) new RemoveUnitOutcome();
      case OutcomeType.EndGame:
        return (Outcome) new EndGameOutcome();
      case OutcomeType.ModifyAirbase:
        return (Outcome) new ModifyAirbaseOutcome();
      case OutcomeType.ModifyEnvironment:
        return (Outcome) new ModifyEnvironmentOutcome();
      case OutcomeType.ModifyFaction:
        return (Outcome) new ModifyFactionOutcome();
      default:
        throw new InvalidEnumArgumentException($"Invalid objective type {type}");
    }
  }
}

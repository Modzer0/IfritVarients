// Decompiled with JetBrains decompiler
// Type: NuclearOption.SavedMission.ObjectiveV2.MissionLookups
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System.Collections.Generic;

#nullable disable
namespace NuclearOption.SavedMission.ObjectiveV2;

public class MissionLookups
{
  public readonly Dictionary<string, Objective> Objectives = new Dictionary<string, Objective>();
  public readonly Dictionary<string, Outcome> Outcomes = new Dictionary<string, Outcome>();
  public readonly Dictionary<string, SavedUnit> SavedUnits = new Dictionary<string, SavedUnit>();
  public readonly Dictionary<string, SavedAirbase> Airbases = new Dictionary<string, SavedAirbase>();
  public readonly LoadErrors LoadErrors;

  public MissionLookups(LoadErrors loadErrors) => this.LoadErrors = loadErrors;
}

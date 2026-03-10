// Decompiled with JetBrains decompiler
// Type: NuclearOption.SavedMission.ObjectiveV2.SavedObjective
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.SavedMission.ObjectiveV2.Objectives;
using System;
using System.Collections.Generic;
using System.ComponentModel;

#nullable disable
namespace NuclearOption.SavedMission.ObjectiveV2;

[Serializable]
public struct SavedObjective
{
  public string UniqueName;
  public string Faction;
  public string DisplayName;
  public bool Hidden;
  public ObjectiveType Type;
  public string TypeName;
  public List<ObjectiveData> Data;
  public List<string> Outcomes;

  public SavedObjective(string name, ObjectiveType type)
    : this()
  {
    this.UniqueName = name;
    this.DisplayName = name;
    this.Type = type;
    this.TypeName = type.ToString();
  }

  public static Objective Create(ObjectiveType type)
  {
    switch (type)
    {
      case ObjectiveType.None:
        return (Objective) new NoObjective();
      case ObjectiveType.DestroyUnits:
        return (Objective) new DestroyUnitObjective();
      case ObjectiveType.ReachUnits:
        return (Objective) new ReachUnitsObjective();
      case ObjectiveType.ReachWaypoints:
        return (Objective) new ReachWaypointsObjective();
      case ObjectiveType.WaitSeconds:
        return (Objective) new WaitTimeObjective();
      case ObjectiveType.CaptureAirbase:
        return (Objective) new CaptureAirbaseObjective();
      case ObjectiveType.DialogueBox:
        return (Objective) new DialogueBoxObjective();
      case ObjectiveType.CompleteOtherObjective:
        return (Objective) new CompleteOtherObjectiveObjective();
      case ObjectiveType.SpotUnit:
        return (Objective) new SpotUnitObjective();
      case ObjectiveType.CrashAircraft:
        return (Objective) new CrashAircraftObjective();
      case ObjectiveType.SuccessfulSortie:
        return (Objective) new SuccessfulSortieObjective();
      default:
        throw new InvalidEnumArgumentException($"Invalid objective type {type}");
    }
  }
}

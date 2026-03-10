// Decompiled with JetBrains decompiler
// Type: NuclearOption.SavedMission.ObjectiveV2.OutcomeType
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;

#nullable disable
namespace NuclearOption.SavedMission.ObjectiveV2;

[Serializable]
public enum OutcomeType
{
  StartObjective = 1,
  StopOrCompleteObjective = 2,
  ShowMessage = 3,
  GiveScore = 4,
  SpawnUnit = 5,
  RemoveUnit = 6,
  EndGame = 8,
  ModifyAirbase = 9,
  ModifyEnvironment = 10, // 0x0000000A
  ModifyFaction = 11, // 0x0000000B
}

// Decompiled with JetBrains decompiler
// Type: NuclearOption.SavedMission.ObjectiveV2.ObjectiveType
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;

#nullable disable
namespace NuclearOption.SavedMission.ObjectiveV2;

[Serializable]
public enum ObjectiveType
{
  None = 0,
  DestroyUnits = 1,
  ReachUnits = 2,
  ReachWaypoints = 3,
  WaitSeconds = 4,
  CaptureAirbase = 6,
  DialogueBox = 7,
  CompleteOtherObjective = 8,
  SpotUnit = 9,
  CrashAircraft = 10, // 0x0000000A
  SuccessfulSortie = 11, // 0x0000000B
}

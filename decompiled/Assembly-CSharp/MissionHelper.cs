// Decompiled with JetBrains decompiler
// Type: MissionHelper
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

#nullable disable
public static class MissionHelper
{
  public static bool CanRespawn => MissionManager.CurrentMission.missionSettings.allowRespawn;

  public static float RankMultiplier
  {
    get => MissionManager.CurrentMission.missionSettings.rankMultiplier;
  }

  public static int StartingRank
  {
    get => MissionManager.CurrentMission.missionSettings.playerStartingRank;
  }
}

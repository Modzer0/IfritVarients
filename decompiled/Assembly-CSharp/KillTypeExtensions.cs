// Decompiled with JetBrains decompiler
// Type: KillTypeExtensions
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

#nullable disable
public static class KillTypeExtensions
{
  public static string GetVerb(this KillType killType, bool hasKiller)
  {
    if (hasKiller)
    {
      switch (killType)
      {
        case KillType.Aircraft:
          return "shot down";
        case KillType.Vehicle:
          return "destroyed";
        case KillType.Building:
          return "demolished";
        case KillType.Missile:
          return "intercepted";
        case KillType.Ship:
          return "sank";
      }
    }
    else
    {
      switch (killType)
      {
        case KillType.Aircraft:
          return "crashed";
        case KillType.Vehicle:
          return "was destroyed";
        case KillType.Building:
          return "collapsed";
        case KillType.Missile:
          return "";
        case KillType.Ship:
          return "sank";
      }
    }
    Debug.LogError((object) $"Unknown kill verb for {killType} with hasKiller={hasKiller}");
    return "";
  }

  public static PlayerSettings.KillFeedFilter GetFilterLevel(this KillType killType)
  {
    switch (killType)
    {
      case KillType.Aircraft:
        return PlayerSettings.killFeedAircraft;
      case KillType.Vehicle:
        return PlayerSettings.killFeedVehicle;
      case KillType.Building:
        return PlayerSettings.killFeedBuilding;
      case KillType.Missile:
        return PlayerSettings.killFeedMunition;
      case KillType.Ship:
        return PlayerSettings.killFeedShip;
      default:
        Debug.LogError((object) $"Unknown kill type: {killType}");
        return PlayerSettings.KillFeedFilter.None;
    }
  }
}

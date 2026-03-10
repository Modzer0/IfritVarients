// Decompiled with JetBrains decompiler
// Type: NuclearOption.Networking.Lobbies.LobbySearchFilter
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Steamworks;

#nullable disable
namespace NuclearOption.Networking.Lobbies;

public struct LobbySearchFilter
{
  public bool HideFull;
  public bool HideEmpty;
  public bool HidePasswordProtected;
  public MissionPvpType MissionPvpType;
  public FilterServerType ServerType;
  public ELobbyDistanceFilter? distanceFilter;
  public bool ignoreVersionFilter;

  public bool PingDistanceAllowed(int ping)
  {
    ELobbyDistanceFilter? distanceFilter = this.distanceFilter;
    if (distanceFilter.HasValue)
    {
      switch (distanceFilter.GetValueOrDefault())
      {
        case ELobbyDistanceFilter.k_ELobbyDistanceFilterClose:
          return ping < 50;
        case ELobbyDistanceFilter.k_ELobbyDistanceFilterDefault:
          return ping < 90;
        case ELobbyDistanceFilter.k_ELobbyDistanceFilterFar:
          return ping < 160 /*0xA0*/;
      }
    }
    return true;
  }
}

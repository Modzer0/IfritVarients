// Decompiled with JetBrains decompiler
// Type: NuclearOption.Networking.PlayerHelper
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Mirage;
using NuclearOption.Networking.Authentication;
using UnityEngine;

#nullable disable
namespace NuclearOption.Networking;

public static class PlayerHelper
{
  public static bool TryGetPlayer<T>(this INetworkPlayer networkPlayer, out T player) where T : BasePlayer
  {
    player = default (T);
    NetworkIdentity identity = networkPlayer.Identity;
    return (UnityEngine.Object) identity != (UnityEngine.Object) null && identity.TryGetComponent<T>(out player);
  }

  public static Player GetPlayer(this Unit unit)
  {
    return unit is Aircraft aircraft ? aircraft.Player : (Player) null;
  }

  public static NetworkAuthenticatorNuclearOption.AuthData GetAuthData(
    this INetworkPlayer networkPlayer)
  {
    return networkPlayer.Authentication.GetData<NetworkAuthenticatorNuclearOption.AuthData>();
  }

  public static void RegisterPrefabs(ClientObjectManager clientObjectManager)
  {
    PlayerHelper.RegisterPrefab<SpectatorPlayer>(clientObjectManager);
    PlayerHelper.RegisterPrefab<DedicatedServerPlayer>(clientObjectManager);
  }

  public static void RegisterPrefab<T>(ClientObjectManager clientObjectManager) where T : BasePlayer
  {
    clientObjectManager.RegisterSpawnHandler(PlayerHelper.PlayerSpawnHash<T>(), (SpawnHandlerDelegate) (_ => PlayerHelper.CreatePrefab<T>().Identity), (UnSpawnDelegate) null);
  }

  public static T CreatePrefab<T>() where T : BasePlayer
  {
    T component = new GameObject("player", new System.Type[2]
    {
      typeof (NetworkIdentity),
      typeof (T)
    }).GetComponent<T>();
    component.Identity.PrefabHash = PlayerHelper.PlayerSpawnHash<T>();
    return component;
  }

  private static int PlayerSpawnHash<T>() => typeof (T).Name.GetStableHashCode();
}

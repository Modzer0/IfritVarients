// Decompiled with JetBrains decompiler
// Type: NuclearOption.SceneLoading.NetworkMap
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Mirage;
using Mirage.Serialization;
using System;
using UnityEngine;

#nullable disable
namespace NuclearOption.SceneLoading;

public class NetworkMap : MonoBehaviour
{
  private const int INDEX_SIZE = 24;
  private static readonly int indexMask = (int) BitMask.Mask(24);
  private static readonly int prefixMask = (int) BitMask.Mask(8);
  [Tooltip("Value that will be used for largest byte in PrefabHash")]
  public int MapPrefix = 1;
  public NetworkIdentity[] NetworkObjects;
  private bool clientRegistered;
  private ClientObjectManager clientObjectManager;

  private void OnValidate()
  {
    if (this.MapPrefix != 0)
      return;
    Debug.LogError((object) "MapPrefix can't be zero");
    this.MapPrefix = 1;
  }

  private void OnDestroy()
  {
    if (!this.clientRegistered)
      return;
    ColorLog<NetworkMap>.Info("Unregister from destroy");
    this.ClientUnregister(this.clientObjectManager);
  }

  public void ServerSpawn(ServerObjectManager manager)
  {
    ColorLog<NetworkMap>.Info("Server Spawn scene objects");
    this.ForeachObject((Action<NetworkIdentity, int>) ((identity, id) => manager.Spawn(identity, id)));
  }

  public void ServerUnspawn(ServerObjectManager manager)
  {
    ColorLog<NetworkMap>.Info("Server Unspawn scene objects");
    this.ForeachObject((Action<NetworkIdentity, int>) ((identity, id) => manager.Destroy(identity, false)));
  }

  public void ClientRegister(ClientObjectManager manager)
  {
    ColorLog<NetworkMap>.Info("Register scene objects");
    this.clientRegistered = !this.clientRegistered ? true : throw new InvalidOperationException("NetworkMap.ClientRegister already called");
    this.clientObjectManager = manager;
    this.ForeachObject((Action<NetworkIdentity, int>) ((identity, id) =>
    {
      manager.RegisterSpawnHandler(id, new SpawnHandlerDelegate(ClientSpawn), new UnSpawnDelegate(ClientUnspawn));

      NetworkIdentity ClientSpawn(SpawnMessage message)
      {
        return identity.NetId == 0U ? identity : throw new Exception($"Map object ({identity}) is already spawned and can not be enabled by NetworkMap");
      }
    }));

    static void ClientUnspawn(NetworkIdentity identity)
    {
    }
  }

  public void ClientUnregister(ClientObjectManager manager)
  {
    ColorLog<NetworkMap>.Info("Unregister scene objects");
    if (!this.clientRegistered)
    {
      ColorLog<NetworkMap>.InfoWarn("ClientUnregister called when map was not registered");
    }
    else
    {
      this.clientRegistered = false;
      this.ForeachObject((Action<NetworkIdentity, int>) ((_, id) => manager.UnregisterSpawnHandler(id)));
    }
  }

  private void ForeachObject(Action<NetworkIdentity, int> callback)
  {
    for (int index = 0; index < this.NetworkObjects.Length; ++index)
    {
      int num = this.MapPrefix << 24 | index & NetworkMap.indexMask;
      callback(this.NetworkObjects[index], num);
    }
  }
}

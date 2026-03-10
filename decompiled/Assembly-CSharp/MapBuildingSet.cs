// Decompiled with JetBrains decompiler
// Type: MapBuildingSet
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Mirage;
using Mirage.Collections;
using Mirage.RemoteCalls;
using Mirage.Serialization;
using NuclearOption.Jobs;
using System;
using System.Collections.Generic;
using UnityEngine;

#nullable disable
public class MapBuildingSet : NetworkBehaviour
{
  [SerializeField]
  private bool animate;
  [Tooltip("Should powerlines be drawn between map buildings")]
  [SerializeField]
  private bool powerlines;
  private MapBuilding[] mapBuildings;
  public readonly SyncList<bool> buildingStates = new SyncList<bool>();
  public readonly List<MapBuilding> animatedBuildings = new List<MapBuilding>();
  [NonSerialized]
  private const int SYNC_VAR_COUNT = 0;
  [NonSerialized]
  private const int RPC_COUNT = 1;

  private void Awake()
  {
    this.Identity.OnStartServer.AddListener(new Action(this.OnStartServer));
    this.Identity.OnStartClient.AddListener(new Action(this.OnStartClient));
    this.buildingStates.OnSet += new Action<int, bool, bool>(this.OnBuildingStatesChanged);
    this.enabled = false;
  }

  private void OnStartServer()
  {
    this.NetworkStart();
    this.buildingStates.Clear();
    for (int index = 0; index < this.mapBuildings.Length; ++index)
      this.buildingStates.Add(true);
  }

  private void NetworkStart()
  {
    this.mapBuildings = this.GetComponentsInChildren<MapBuilding>();
    this.animatedBuildings.Clear();
    for (int index = 0; index < this.mapBuildings.Length; ++index)
    {
      this.mapBuildings[index].AssignBuildingSet(this, index);
      if (this.powerlines && index < this.mapBuildings.Length - 1)
        this.mapBuildings[index].SpawnPowerline(this.mapBuildings[index + 1]);
    }
  }

  private void OnStartClient()
  {
    if (!this.IsServer)
      this.NetworkStart();
    if (this.animate && GameManager.ShowEffects)
      this.enabled = true;
    for (int index = 0; index < this.buildingStates.Count; ++index)
      this.OnBuildingStatesChanged(index, true, this.buildingStates[index]);
  }

  public void DestroyBuilding(int index)
  {
    if (this.IsServer)
      this.buildingStates[index] = false;
    else
      this.CmdDestroyBuilding(index);
  }

  [ServerRpc(requireAuthority = false)]
  private void CmdDestroyBuilding(int index)
  {
    if (ServerRpcSender.ShouldInvokeLocally((NetworkBehaviour) this, false, false))
    {
      this.UserCode_CmdDestroyBuilding_\u002D380645060(index);
    }
    else
    {
      PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
      writer.WritePackedInt32(index);
      ServerRpcSender.Send((NetworkBehaviour) this, 0, (NetworkWriter) writer, Channel.Reliable, false);
      writer.Release();
    }
  }

  private void OnBuildingStatesChanged(int index, bool oldValue, bool newValue)
  {
    if (newValue)
      return;
    this.mapBuildings[index].Destruct();
  }

  private void Update()
  {
    for (int index = this.animatedBuildings.Count - 1; index >= 0; --index)
    {
      if (this.animatedBuildings[index].MapBuilding_OnUpdate() == PartResult.Remove)
        this.animatedBuildings.RemoveAt(index);
    }
  }

  public MapBuildingSet() => this.InitSyncObject((ISyncObject) this.buildingStates);

  private void MirageProcessed()
  {
  }

  private void UserCode_CmdDestroyBuilding_\u002D380645060(int index)
  {
    if (0 > index || index >= this.buildingStates.Count)
      return;
    this.buildingStates[index] = false;
  }

  protected static void Skeleton_CmdDestroyBuilding_\u002D380645060(
    NetworkBehaviour behaviour,
    NetworkReader reader,
    INetworkPlayer senderConnection,
    int replyId)
  {
    ((MapBuildingSet) behaviour).UserCode_CmdDestroyBuilding_\u002D380645060(reader.ReadPackedInt32());
  }

  protected override int GetRpcCount() => 1;

  public override void RegisterRpc(RemoteCallCollection collection)
  {
    base.RegisterRpc(collection);
    collection.Register(0, "MapBuildingSet.CmdDestroyBuilding", false, RpcInvokeType.ServerRpc, (NetworkBehaviour) this, new RpcDelegate(MapBuildingSet.Skeleton_CmdDestroyBuilding_\u002D380645060));
  }
}

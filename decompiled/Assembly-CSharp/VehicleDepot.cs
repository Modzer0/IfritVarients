// Decompiled with JetBrains decompiler
// Type: VehicleDepot
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.Networking;
using System;
using UnityEngine;

#nullable disable
public class VehicleDepot : Building
{
  [SerializeField]
  private Transform spawnTransform;
  [SerializeField]
  private float spawnCooldown;
  private float lastSpawnedTime;
  [NonSerialized]
  private const int SYNC_VAR_COUNT = 10;
  [NonSerialized]
  private const int RPC_COUNT = 21;

  protected override void OnStartClient()
  {
    if (GameManager.gameState == GameState.Encyclopedia)
      return;
    this.transform.position = this.startPosition.ToLocalPosition();
    this.RegisterUnit(new float?());
    this.InitializeUnit();
    if ((UnityEngine.Object) this.NetworkHQ != (UnityEngine.Object) null && NetworkManagerNuclearOption.i.Server.Active)
      this.NetworkHQ.AddDepot(this);
    if (!this.IsServer || (double) this.repairTime >= 999999.0)
      return;
    foreach (UnitPart unitPart in this.partLookup)
      unitPart.onApplyDamage += new Action<UnitPart.OnApplyDamage>(((Building) this).Building_OnPartDamage);
  }

  public bool TrySpawnVehicle(VehicleDefinition vehicleDefinition)
  {
    if ((double) Time.timeSinceLevelLoad - (double) this.lastSpawnedTime < (double) this.spawnCooldown)
      return false;
    this.lastSpawnedTime = Time.timeSinceLevelLoad;
    NetworkSceneSingleton<Spawner>.i.SpawnVehicle(vehicleDefinition.unitPrefab, this.spawnTransform.GlobalPosition() + Vector3.up * vehicleDefinition.spawnOffset.y, this.spawnTransform.rotation, Vector3.zero, this.NetworkHQ, (string) null, 1f, false, (Player) null).MoveFromDepot();
    return true;
  }

  public override void OnRepairComplete()
  {
    base.OnRepairComplete();
    if (!((UnityEngine.Object) this.NetworkHQ != (UnityEngine.Object) null) || !NetworkManagerNuclearOption.i.Server.Active)
      return;
    this.NetworkHQ.AddDepot(this);
  }

  private void MirageProcessed()
  {
  }

  protected override int GetRpcCount() => 21;
}

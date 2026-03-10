// Decompiled with JetBrains decompiler
// Type: WarheadStorage
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Mirage;
using Mirage.Serialization;
using System;
using UnityEngine;

#nullable disable
public class WarheadStorage : NetworkBehaviour
{
  public Unit attachedUnit;
  [SyncVar]
  public int number;
  [SerializeField]
  private UnitPart criticalPart;
  private bool selfDisabled;
  [NonSerialized]
  private const int SYNC_VAR_COUNT = 1;
  [NonSerialized]
  private const int RPC_COUNT = 0;

  public bool Disabled => this.selfDisabled || this.attachedUnit.disabled;

  private void OnValidate()
  {
  }

  private void Awake() => this.Identity.OnStartServer.AddListener(new Action(this.OnStartServer));

  private void OnStartServer()
  {
    this.attachedUnit.onDisableUnit += new Action<Unit>(this.AttachedUnit_OnDisable);
    if (!((UnityEngine.Object) this.criticalPart != (UnityEngine.Object) null))
      return;
    this.criticalPart.onApplyDamage += new Action<UnitPart.OnApplyDamage>(this.Part_OnApplyDamage);
  }

  public bool IsFunctional()
  {
    return !this.Disabled && (double) this.transform.position.y > (double) Datum.LocalSeaY;
  }

  public void Repair()
  {
    if ((UnityEngine.Object) this.attachedUnit == (UnityEngine.Object) null)
      return;
    this.attachedUnit.Networkdisabled = false;
    this.attachedUnit.onDisableUnit += new Action<Unit>(this.AttachedUnit_OnDisable);
  }

  private void AttachedUnit_OnDisable(Unit unit)
  {
    if (!this.selfDisabled)
      this.Disable();
    this.attachedUnit.onDisableUnit -= new Action<Unit>(this.AttachedUnit_OnDisable);
  }

  private void Part_OnApplyDamage(UnitPart.OnApplyDamage e)
  {
    if ((double) e.hitPoints >= 0.0 && !e.detached || this.selfDisabled)
      return;
    this.Disable();
  }

  private void Disable()
  {
    this.selfDisabled = true;
    if (this.number == 0)
      return;
    Airbase airbase = this.attachedUnit.GetAirbase();
    if ((UnityEngine.Object) airbase == (UnityEngine.Object) null)
      return;
    if ((UnityEngine.Object) NetworkSceneSingleton<MessageManager>.i != (UnityEngine.Object) null)
      NetworkSceneSingleton<MessageManager>.i.RpcWarheadDestroyedMessage(airbase, this.attachedUnit.NetworkHQ, this.number);
    this.Networknumber = 0;
  }

  public void CheckAttachCamera() => this.AttachCamera();

  private void AttachCamera()
  {
    SceneSingleton<CameraStateManager>.i.followingUnit = this.attachedUnit;
    SceneSingleton<CameraStateManager>.i.SwitchState((CameraBaseState) SceneSingleton<CameraStateManager>.i.relativeState);
    SceneSingleton<CameraStateManager>.i.transform.position = this.transform.position;
    SceneSingleton<CameraStateManager>.i.transform.rotation = this.transform.rotation;
  }

  public Unit GetUnit() => this.attachedUnit;

  public int GetNumber() => this.number;

  public void AddWarhead(int n) => this.Networknumber = this.number + n;

  public void RemoveWarhead(int n)
  {
    this.Networknumber = this.number - n;
    if (this.number >= 0)
      return;
    this.Networknumber = 0;
  }

  private void OnDestroy() => this.selfDisabled = true;

  private void MirageProcessed()
  {
  }

  public int Networknumber
  {
    get => this.number;
    set
    {
      if (this.SyncVarEqual<int>(value, this.number))
        return;
      int number = this.number;
      this.number = value;
      this.SetDirtyBit(1UL);
    }
  }

  public override bool SerializeSyncVars(NetworkWriter writer, bool initialize)
  {
    ulong syncVarDirtyBits = this.SyncVarDirtyBits;
    bool flag = base.SerializeSyncVars(writer, initialize);
    if (initialize)
    {
      writer.WritePackedInt32(this.number);
      return true;
    }
    writer.Write(syncVarDirtyBits, 1);
    if (((long) syncVarDirtyBits & 1L) != 0L)
    {
      writer.WritePackedInt32(this.number);
      flag = true;
    }
    return flag;
  }

  public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
  {
    base.DeserializeSyncVars(reader, initialState);
    if (initialState)
    {
      this.number = reader.ReadPackedInt32();
    }
    else
    {
      ulong dirtyBit = reader.Read(1);
      this.SetDeserializeMask(dirtyBit, 0);
      if (((long) dirtyBit & 1L) == 0L)
        return;
      this.number = reader.ReadPackedInt32();
    }
  }

  protected override int GetRpcCount() => 0;
}

// Decompiled with JetBrains decompiler
// Type: NuclearOption.Networking.BasePlayer
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Mirage;
using Mirage.Serialization;
using NuclearOption.Networking.Authentication;
using System;

#nullable disable
namespace NuclearOption.Networking;

public abstract class BasePlayer : NetworkBehaviour
{
  [NonSerialized]
  private const int SYNC_VAR_COUNT = 1;
  [NonSerialized]
  private const int RPC_COUNT = 0;

  public ulong SteamID
  {
    get => this.\u003CSteamID\u003Ek__BackingField;
    private set => this.Network\u003CSteamID\u003Ek__BackingField = value;
  }

  protected virtual void Awake()
  {
    this.transform.SetParent(Datum.origin);
    this.Identity.OnStartServer.AddListener(new Action(this.OnStartServer));
    this.Identity.OnStartClient.AddListener(new Action(this.OnStartClient));
  }

  private void OnStartServer()
  {
    NetworkAuthenticatorNuclearOption.AuthData authData = this.Owner.GetAuthData();
    if (!authData.UsingSteamTransport)
      return;
    this.SteamID = authData.SteamID.m_SteamID;
  }

  private void OnStartClient()
  {
    if (this.IsServer)
      return;
    ColorLog<BasePlayer>.Info("Client local player spawned");
  }

  public NetworkAuthenticatorNuclearOption.AuthData GetAuthData() => this.Owner.GetAuthData();

  private void MirageProcessed()
  {
  }

  public ulong Network\u003CSteamID\u003Ek__BackingField
  {
    get => this.\u003CSteamID\u003Ek__BackingField;
    set
    {
      if (this.SyncVarEqual<ulong>(value, this.\u003CSteamID\u003Ek__BackingField))
        return;
      ulong steamIdKBackingField = this.\u003CSteamID\u003Ek__BackingField;
      this.\u003CSteamID\u003Ek__BackingField = value;
      this.SetDirtyBit(1UL);
    }
  }

  public override bool SerializeSyncVars(NetworkWriter writer, bool initialize)
  {
    ulong syncVarDirtyBits = this.SyncVarDirtyBits;
    bool flag = base.SerializeSyncVars(writer, initialize);
    if (initialize)
    {
      // ISSUE: reference to a compiler-generated field
      writer.WritePackedUInt64(this.\u003CSteamID\u003Ek__BackingField);
      return true;
    }
    writer.Write(syncVarDirtyBits, 1);
    if (((long) syncVarDirtyBits & 1L) != 0L)
    {
      // ISSUE: reference to a compiler-generated field
      writer.WritePackedUInt64(this.\u003CSteamID\u003Ek__BackingField);
      flag = true;
    }
    return flag;
  }

  public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
  {
    base.DeserializeSyncVars(reader, initialState);
    if (initialState)
    {
      // ISSUE: reference to a compiler-generated field
      this.\u003CSteamID\u003Ek__BackingField = reader.ReadPackedUInt64();
    }
    else
    {
      ulong dirtyBit = reader.Read(1);
      this.SetDeserializeMask(dirtyBit, 0);
      if (((long) dirtyBit & 1L) == 0L)
        return;
      // ISSUE: reference to a compiler-generated field
      this.\u003CSteamID\u003Ek__BackingField = reader.ReadPackedUInt64();
    }
  }

  protected override int GetRpcCount() => 0;
}

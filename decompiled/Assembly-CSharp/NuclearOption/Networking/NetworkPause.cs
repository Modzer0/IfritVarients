// Decompiled with JetBrains decompiler
// Type: NuclearOption.Networking.NetworkPause
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Cysharp.Threading.Tasks;
using Mirage;
using Mirage.Serialization;
using NuclearOption.DedicatedServer;
using System;
using System.Threading;
using UnityEngine;

#nullable disable
namespace NuclearOption.Networking;

public class NetworkPause : NetworkSceneSingleton<NetworkPause>
{
  public GameObject overlay;
  [SyncVar(hook = "HookPausedChanged", invokeHookOnServer = true)]
  private bool serverPaused;
  private bool localPaused;
  private DedicatedServerManager serverManager;
  [NonSerialized]
  private const int SYNC_VAR_COUNT = 1;
  [NonSerialized]
  private const int RPC_COUNT = 0;

  protected override void Awake()
  {
    if ((UnityEngine.Object) this.overlay != (UnityEngine.Object) null)
      this.overlay.SetActive(false);
    base.Awake();
    this.Identity.OnStartServer.AddListener(new Action(this.OnStartServer));
  }

  private void OnDestroy()
  {
    if (!this.localPaused)
      return;
    Time.timeScale = 1f;
    this.localPaused = false;
  }

  private void OnStartServer()
  {
    this.serverManager = NetworkManagerNuclearOption.i.DedicatedServerManager;
    if (!DedicatedServerManager.IsRunning)
      return;
    this.NoPlayerCheck().Forget();
  }

  private void HookPausedChanged()
  {
    if ((UnityEngine.Object) this.overlay != (UnityEngine.Object) null)
      this.overlay.SetActive(this.serverPaused);
    Time.timeScale = this.serverPaused ? 0.0f : 1f;
    this.localPaused = this.serverPaused;
  }

  private async UniTask NoPlayerCheck()
  {
    NetworkPause networkPause = this;
    ColorLog<NetworkPause>.Info("Starting NetworkPause server loop");
    CancellationToken cancel = networkPause.destroyCancellationToken;
    while (!cancel.IsCancellationRequested)
    {
      await UniTask.Delay(1000, true);
      bool flag = !networkPause.serverManager.HasPlayers();
      if (networkPause.serverPaused != flag)
      {
        networkPause.NetworkserverPaused = flag;
        ColorLog<NetworkPause>.Info($"Toggling Pause => {networkPause.serverPaused}");
      }
    }
    cancel = new CancellationToken();
  }

  private void MirageProcessed()
  {
  }

  public bool NetworkserverPaused
  {
    get => this.serverPaused;
    set
    {
      if (this.SyncVarEqual<bool>(value, this.serverPaused))
        return;
      bool serverPaused = this.serverPaused;
      this.serverPaused = value;
      this.SetDirtyBit(1UL);
      if (!this.GetSyncVarHookGuard(1UL) && this.IsServer)
      {
        this.SetSyncVarHookGuard(1UL, true);
        this.HookPausedChanged();
        this.SetSyncVarHookGuard(1UL, false);
      }
    }
  }

  public override bool SerializeSyncVars(NetworkWriter writer, bool initialize)
  {
    ulong syncVarDirtyBits = this.SyncVarDirtyBits;
    bool flag = base.SerializeSyncVars(writer, initialize);
    if (initialize)
    {
      writer.WriteBooleanExtension(this.serverPaused);
      return true;
    }
    writer.Write(syncVarDirtyBits, 1);
    if (((long) syncVarDirtyBits & 1L) != 0L)
    {
      writer.WriteBooleanExtension(this.serverPaused);
      flag = true;
    }
    return flag;
  }

  public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
  {
    base.DeserializeSyncVars(reader, initialState);
    if (initialState)
    {
      bool serverPaused = this.serverPaused;
      this.serverPaused = reader.ReadBooleanExtension();
      if (this.SyncVarEqual<bool>(serverPaused, this.serverPaused))
        return;
      this.HookPausedChanged();
    }
    else
    {
      ulong dirtyBit = reader.Read(1);
      this.SetDeserializeMask(dirtyBit, 0);
      if (((long) dirtyBit & 1L) == 0L)
        return;
      bool serverPaused = this.serverPaused;
      this.serverPaused = reader.ReadBooleanExtension();
      if (!this.SyncVarEqual<bool>(serverPaused, this.serverPaused))
        this.HookPausedChanged();
    }
  }

  protected override int GetRpcCount() => 0;
}

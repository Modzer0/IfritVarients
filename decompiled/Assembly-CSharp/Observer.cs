// Decompiled with JetBrains decompiler
// Type: Observer
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.Networking;
using System;
using UnityEngine;

#nullable disable
public class Observer : Unit
{
  [SerializeField]
  private float lifetime;
  private float spawnTime;
  [NonSerialized]
  private const int SYNC_VAR_COUNT = 9;
  [NonSerialized]
  private const int RPC_COUNT = 19;

  private new void Awake()
  {
    this.Identity.OnStartClient.AddListener(new Action(this.OnStartClient));
  }

  private void OnStartClient() => this.spawnTime = Time.timeSinceLevelLoad;

  private void Update()
  {
    if (!NetworkManagerNuclearOption.i.Server.Active || (double) Time.timeSinceLevelLoad - (double) this.spawnTime <= (double) this.lifetime)
      return;
    UnityEngine.Object.Destroy((UnityEngine.Object) this.gameObject);
  }

  private void MirageProcessed()
  {
  }

  protected override int GetRpcCount() => 19;
}

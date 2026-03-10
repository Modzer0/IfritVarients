// Decompiled with JetBrains decompiler
// Type: Scenery
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Cysharp.Threading.Tasks;
using NuclearOption.Networking;
using System;
using System.Threading;
using UnityEngine;

#nullable disable
public class Scenery : Unit
{
  private float collapseTime = 10f;
  [NonSerialized]
  private const int SYNC_VAR_COUNT = 9;
  [NonSerialized]
  private const int RPC_COUNT = 19;

  public SceneryDefinition definition => (SceneryDefinition) this.definition;

  public override void Awake()
  {
    base.Awake();
    this.Identity.OnStartClient.AddListener(new Action(this.OnStartClient));
    if (!((UnityEngine.Object) NetworkManagerNuclearOption.i != (UnityEngine.Object) null) || !NetworkManagerNuclearOption.i.Server.Active)
      return;
    this.SetLocalSim(true);
    this.NetworkunitName = this.definition.unitName;
    this.NetworkstartPosition = this.transform.position.ToGlobalPosition();
  }

  private void OnStartClient()
  {
    if (GameManager.gameState == GameState.Encyclopedia)
      return;
    this.transform.position = this.startPosition.ToLocalPosition();
    this.RegisterUnit(new float?());
    this.InitializeUnit();
  }

  public override void UnitDisabled(bool oldState, bool newState)
  {
    base.UnitDisabled(oldState, newState);
    if (GameManager.gameState == GameState.Editor || !newState)
      return;
    this.Collapse().Forget();
  }

  private async UniTask Collapse()
  {
    Scenery scenery = this;
    Vector3 velocity = Vector3.zero;
    CancellationToken cancel = scenery.destroyCancellationToken;
    while ((double) scenery.collapseTime > 0.0)
    {
      await UniTask.Yield();
      if (cancel.IsCancellationRequested)
      {
        cancel = new CancellationToken();
        return;
      }
      velocity += Vector3.down * 9.81f * Time.deltaTime;
      scenery.collapseTime -= Time.deltaTime;
      scenery.transform.position += velocity * Time.deltaTime;
    }
    if (!NetworkManagerNuclearOption.i.Server.Active)
    {
      cancel = new CancellationToken();
    }
    else
    {
      UnityEngine.Object.Destroy((UnityEngine.Object) scenery.gameObject);
      cancel = new CancellationToken();
    }
  }

  private void MirageProcessed()
  {
  }

  protected override int GetRpcCount() => 19;
}

// Decompiled with JetBrains decompiler
// Type: NuclearOption.SavedMission.ObjectiveV2.Objectives.WaitTimeObjective
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.MissionEditorScripts;
using System.Collections.Generic;
using Unity.Profiling;
using UnityEngine;

#nullable disable
namespace NuclearOption.SavedMission.ObjectiveV2.Objectives;

internal class WaitTimeObjective : Objective
{
  private static readonly ProfilerMarker updateAndCheckMarker = new ProfilerMarker("WaitTimeObjectiveUpdateAndCheck");
  private const int NETWORK_ACCURACY = 10000;
  private readonly ValueWrapperFloat seconds = new ValueWrapperFloat();
  private float startTime;
  private float duration;

  public float Timer
  {
    get
    {
      return GameManager.gameState != GameState.SinglePlayer ? (float) this.MissionManager.NetworkTime.Time : Time.timeSinceLevelLoad;
    }
  }

  public override float CompletePercent
  {
    get
    {
      return (double) this.startTime == 0.0 ? 0.0f : this.duration / (float) (ValueWrapper<float>) this.seconds;
    }
  }

  public override void ReceiveNetworkData(List<int> data)
  {
    this.startTime = (float) data[0] / 10000f;
  }

  protected override void WriteObjective(ReadWriteObjective writer)
  {
    writer.Float(ref this.seconds.GetValueRef(), 5f);
  }

  protected override void DataReferenceDestroyed(ISaveableReference reference)
  {
  }

  public override void OnStart()
  {
    if (!this.MissionManager.IsServer)
      return;
    float timer = this.Timer;
    this.startTime = timer;
    int num = (int) ((double) timer * 10000.0);
    if ((double) this.startTime == 0.0)
      this.startTime = 0.0001f;
    if (num == 0)
      num = 1;
    this.MissionManager.UpdateNetworkData((Objective) this, new List<int>()
    {
      num
    });
  }

  public override void ClientOnlyUpdate() => this.duration = this.Timer - this.startTime;

  public override bool UpdateAndCheck()
  {
    using (WaitTimeObjective.updateAndCheckMarker.Auto())
    {
      this.duration = this.Timer - this.startTime;
      return (double) this.duration > (double) (float) (ValueWrapper<float>) this.seconds;
    }
  }

  public override void DrawData(DataDrawer drawer)
  {
    drawer.InstantiateWithParent<FloatDataField>(drawer.Prefabs.FloatFieldPrefab).Setup("Seconds", (IValueWrapper<float>) this.seconds);
  }
}

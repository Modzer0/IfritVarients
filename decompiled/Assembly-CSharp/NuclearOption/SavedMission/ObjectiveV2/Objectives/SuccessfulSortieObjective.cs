// Decompiled with JetBrains decompiler
// Type: NuclearOption.SavedMission.ObjectiveV2.Objectives.SuccessfulSortieObjective
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Mirage;
using NuclearOption.MissionEditorScripts;
using System;
using System.Collections.Generic;
using Unity.Profiling;
using UnityEngine;

#nullable disable
namespace NuclearOption.SavedMission.ObjectiveV2.Objectives;

public class SuccessfulSortieObjective : Objective
{
  private static readonly ProfilerMarker updateAndCheckMarker = new ProfilerMarker("SuccessfulSortieObjectiveUpdateAndCheck");
  private const float NETWORK_FACTOR = 0.1f;
  private readonly ValueWrapperFloat minimumScore = new ValueWrapperFloat();
  private readonly ValueWrapperBool additive = new ValueWrapperBool();
  private NetworkWorld world;
  private float score;
  private readonly List<int> networkList = new List<int>();

  public override string FactionLabelOverride => "Both";

  public override float CompletePercent
  {
    get
    {
      return (double) this.minimumScore.Value == 0.0 ? ((double) this.score > 0.0 ? 1f : 0.0f) : this.score / this.minimumScore.Value;
    }
  }

  public override void ReceiveNetworkData(List<int> data)
  {
    if (data.Count <= 0)
      return;
    this.score = (float) data[0] * 0.1f;
  }

  private void UpdateNetworkData()
  {
    if (!this.MissionManager.IsServer)
      return;
    this.networkList.Clear();
    this.networkList.Add((int) ((double) this.score / 0.10000000149011612));
    this.MissionManager.UpdateNetworkData((Objective) this, this.networkList);
  }

  protected override void WriteObjective(ReadWriteObjective writer)
  {
    writer.Float(ref this.minimumScore.GetValueRef());
  }

  protected override void DataReferenceDestroyed(ISaveableReference reference)
  {
  }

  public override void OnStart()
  {
    if (!this.MissionManager.IsServer)
      return;
    this.world = this.MissionManager.Server.World;
    this.world.AddAndInvokeOnSpawn(new Action<NetworkIdentity>(this.OnSpawn));
  }

  public override void Cleanup()
  {
    if (this.world == null)
      return;
    this.world.onSpawn -= new Action<NetworkIdentity>(this.OnSpawn);
  }

  private void OnSpawn(NetworkIdentity identity)
  {
    Aircraft component;
    if (!identity.TryGetComponent<Aircraft>(out component) || !((UnityEngine.Object) component.Player != (UnityEngine.Object) null) || !((UnityEngine.Object) this.FactionHQ == (UnityEngine.Object) null) && !((UnityEngine.Object) component.NetworkHQ == (UnityEngine.Object) this.FactionHQ))
      return;
    component.onSortieSuccessful += new Action<float>(this.Aircraft_onSortieSuccessful);
  }

  private void Aircraft_onSortieSuccessful(float score)
  {
    Debug.LogWarning((object) $"[CrashAircraftObjective] sortie = {score}");
    if (this.additive.Value)
    {
      this.score += score;
      this.UpdateNetworkData();
    }
    else
    {
      if ((double) score <= (double) this.score)
        return;
      this.score = score;
      this.UpdateNetworkData();
    }
  }

  public override void ClientOnlyUpdate()
  {
  }

  public override bool UpdateAndCheck()
  {
    using (SuccessfulSortieObjective.updateAndCheckMarker.Auto())
      return (double) this.minimumScore.Value == 0.0 || (double) this.score >= (double) this.minimumScore.Value;
  }

  public override void DrawData(DataDrawer drawer)
  {
    drawer.InstantiateWithParent<FloatDataField>(drawer.Prefabs.FloatFieldPrefab).Setup("Minimum Score", (IValueWrapper<float>) this.minimumScore);
    drawer.InstantiateWithParent<BoolDataField>(drawer.Prefabs.BoolFieldPrefab).Setup("Additive", (IValueWrapper<bool>) this.additive);
  }
}

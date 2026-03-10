// Decompiled with JetBrains decompiler
// Type: NuclearOption.SavedMission.ObjectiveV2.Objectives.CrashAircraftObjective
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

public class CrashAircraftObjective : Objective
{
  private static readonly ProfilerMarker updateAndCheckMarker = new ProfilerMarker("CrashAircraftObjectiveUpdateAndCheck");
  private readonly ValueWrapperInt livesPerPlayer = new ValueWrapperInt();
  private readonly ValueWrapperInt extraLives = new ValueWrapperInt();
  private readonly ValueWrapperBool includeDestroy = new ValueWrapperBool();
  private readonly ValueWrapperBool includeEject = new ValueWrapperBool();
  private int crashes;
  private NetworkWorld world;
  private readonly List<int> networkList = new List<int>();

  public override string FactionLabelOverride => "Both";

  private int GetTotalLives()
  {
    return (int) (ValueWrapper<int>) this.extraLives + (int) (ValueWrapper<int>) this.livesPerPlayer * this.MissionManager.Server.AuthenticatedPlayers.Count;
  }

  public override float CompletePercent
  {
    get
    {
      int totalLives = this.GetTotalLives();
      return totalLives == 0 ? 0.0f : (float) (this.crashes / totalLives);
    }
  }

  public override void ReceiveNetworkData(List<int> data)
  {
    if (data.Count <= 0)
      return;
    this.crashes = data[0];
  }

  private void UpdateNetworkData()
  {
    if (!this.MissionManager.IsServer)
      return;
    this.networkList.Clear();
    this.networkList.Add(this.crashes);
    this.MissionManager.UpdateNetworkData((Objective) this, this.networkList);
  }

  protected override void WriteObjective(ReadWriteObjective writer)
  {
    writer.Int(ref this.livesPerPlayer.GetValueRef());
    writer.Int(ref this.extraLives.GetValueRef(), 1);
    writer.Bool(ref this.includeDestroy.GetValueRef(), true);
    writer.Bool(ref this.includeEject.GetValueRef(), true);
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
    Aircraft a = component;
    if (this.includeDestroy.Value)
      component.onDisableUnit += new Action<Unit>(this.Aircraft_onDisableUnit);
    if (!this.includeEject.Value)
      return;
    component.onEject += (Action) (() => this.Aircraft_onEject(a));
  }

  private void Aircraft_onDisableUnit(Unit obj)
  {
    Aircraft aircraft = (Aircraft) obj;
    this.DebugWhere("Disable", aircraft);
    if (this.LandingNearAirbase(aircraft))
      return;
    ++this.crashes;
    this.UpdateNetworkData();
  }

  private void Aircraft_onEject(Aircraft aircraft)
  {
    this.DebugWhere("Eject", aircraft);
    ++this.crashes;
    this.UpdateNetworkData();
  }

  private void DebugWhere(string what, Aircraft aircraft)
  {
    bool flag1 = aircraft.IsLanded();
    bool flag2 = this.NearAirbase(aircraft);
    Debug.LogWarning((object) $"[CrashAircraftObjective] {what} landed:{flag1} nearAirbase:{flag2}");
  }

  private bool LandingNearAirbase(Aircraft aircraft)
  {
    return aircraft.IsLanded() && this.NearAirbase(aircraft);
  }

  private bool NearAirbase(Aircraft aircraft)
  {
    return (UnityEngine.Object) aircraft.NetworkHQ != (UnityEngine.Object) null && aircraft.NetworkHQ.AnyNearAirbase(aircraft.transform.position, out Airbase _);
  }

  public override void ClientOnlyUpdate()
  {
  }

  public override bool UpdateAndCheck()
  {
    using (CrashAircraftObjective.updateAndCheckMarker.Auto())
      return this.crashes >= this.GetTotalLives();
  }

  public override void DrawData(DataDrawer drawer)
  {
    drawer.InstantiateWithParent<FloatDataField>(drawer.Prefabs.FloatFieldPrefab).Setup("Number of Crashes", this.extraLives);
    drawer.InstantiateWithParent<FloatDataField>(drawer.Prefabs.FloatFieldPrefab).Setup("Extra Crash per player", this.livesPerPlayer);
    drawer.DrawHeader("Trigger Conditions");
    drawer.InstantiateWithParent<BoolDataField>(drawer.Prefabs.BoolFieldPrefab).Setup("Destroy", (IValueWrapper<bool>) this.includeDestroy);
    drawer.InstantiateWithParent<BoolDataField>(drawer.Prefabs.BoolFieldPrefab).Setup("Eject", (IValueWrapper<bool>) this.includeEject);
  }
}

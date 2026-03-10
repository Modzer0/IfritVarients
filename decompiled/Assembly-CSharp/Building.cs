// Decompiled with JetBrains decompiler
// Type: Building
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Cysharp.Threading.Tasks;
using Mirage;
using Mirage.RemoteCalls;
using Mirage.Serialization;
using NuclearOption.Networking;
using NuclearOption.SavedMission;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

#nullable disable
public class Building : Unit, IRepairable, IRearmable
{
  [SerializeField]
  private GameObject wreckage;
  [SerializeField]
  private float collapseTime;
  [SerializeField]
  private bool hideOnMap;
  [SerializeField]
  private GameObject repairPrefab;
  [SerializeField]
  protected float repairTime = 1000000f;
  public bool capturable;
  public bool needsRepair;
  public bool canRearm;
  private List<GameObject> wreckageSpawn;
  private Building.RecentExplosion recentExplosion;
  private GameObject repairInstance;
  private float timeLastDamaged;
  [SyncVar(initialOnly = true)]
  [NonSerialized]
  internal NetworkBehaviorSyncvar airbase;
  [NonSerialized]
  private const int SYNC_VAR_COUNT = 10;
  [NonSerialized]
  private const int RPC_COUNT = 21;

  public event Action<RearmEventArgs> OnRearm;

  public override Airbase GetAirbase() => this.Networkairbase;

  public override void Awake()
  {
    base.Awake();
    this.Identity.OnStartServer.AddListener(new Action(this.OnStartServer));
    this.Identity.OnStartClient.AddListener(new Action(this.OnStartClient));
    this.Identity.OnStopClient.AddListener(new Action(this.OnStopClient));
    if (!((UnityEngine.Object) NetworkManagerNuclearOption.i != (UnityEngine.Object) null) || !NetworkManagerNuclearOption.i.Server.Active)
      return;
    this.SetLocalSim(true);
    this.NetworkunitName = this.definition.unitName;
    this.NetworkstartPosition = this.transform.position.ToGlobalPosition();
  }

  protected override SavedUnit CreateBuiltInSavedUnit()
  {
    SavedBuilding savedBuilding1 = new SavedBuilding();
    savedBuilding1.UniqueName = this.MapUniqueName;
    savedBuilding1.PlacementType = PlacementType.BuiltIn;
    savedBuilding1.type = this.definition.jsonKey;
    savedBuilding1.Unit = (Unit) this;
    savedBuilding1.Airbase = (UnityEngine.Object) this.MapAirbase != (UnityEngine.Object) null ? this.MapAirbase.SavedAirbase.UniqueName : (string) null;
    savedBuilding1.capturable = this.capturable;
    SavedBuilding builtInSavedUnit = savedBuilding1;
    Factory component;
    if (this.TryGetComponent<Factory>(out component))
    {
      SavedBuilding savedBuilding2 = builtInSavedUnit;
      if (savedBuilding2.factoryOptions == null)
        savedBuilding2.factoryOptions = new SavedBuilding.FactoryOptions();
      builtInSavedUnit.factoryOptions.productionTime = component.ProductionInterval;
      builtInSavedUnit.factoryOptions.productionType = (UnityEngine.Object) component.ProductionUnit != (UnityEngine.Object) null ? component.ProductionUnit.jsonKey : "";
    }
    return (SavedUnit) builtInSavedUnit;
  }

  private void OnStartServer() => this.CheckBuildingsBelow();

  protected virtual void OnStartClient()
  {
    if (this.IsServer && (double) this.repairTime < 999999.0)
    {
      foreach (UnitPart unitPart in this.partLookup)
        unitPart.onApplyDamage += new Action<UnitPart.OnApplyDamage>(this.Building_OnPartDamage);
    }
    this.transform.position = this.startPosition.ToLocalPosition();
    if (!this.hideOnMap)
      this.RegisterUnit(new float?());
    this.InitializeUnit();
    this.CheckUnderground();
    if (GameManager.gameState == GameState.Encyclopedia)
      return;
    this.ClientAddBuildingToAirbase();
    if (!this.capturable)
      return;
    this.CheckForCapture().Forget();
  }

  public void ClientAddBuildingToAirbase()
  {
    if (!((UnityEngine.Object) this.Networkairbase != (UnityEngine.Object) null))
      return;
    this.Networkairbase.AddBuilding(this, false);
  }

  public void SetAirbase(Airbase airbase)
  {
    if ((UnityEngine.Object) airbase == (UnityEngine.Object) null)
    {
      Debug.LogError((object) "SetAirbase should not be set with null airbase");
    }
    else
    {
      if (GameManager.gameState == GameState.Editor)
        return;
      this.Networkairbase = airbase;
      if (!this.Identity.IsSpawned)
        return;
      this.RpcSetAirbase(airbase);
      airbase.AddBuilding(this, true);
    }
  }

  [ClientRpc(excludeHost = true)]
  private void RpcSetAirbase(Airbase airbase)
  {
    PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
    GeneratedNetworkCode._Write_Airbase((NetworkWriter) writer, airbase);
    ClientRpcSender.Send((NetworkBehaviour) this, 19, (NetworkWriter) writer, Mirage.Channel.Reliable, false);
    writer.Release();
  }

  public bool IsRepairable()
  {
    return (double) this.repairTime < 999999.0 && (double) Time.timeSinceLevelLoad - (double) this.timeLastDamaged > 30.0;
  }

  private void OnStopClient()
  {
    if (!((UnityEngine.Object) this.Networkairbase != (UnityEngine.Object) null) || this.Identity.IsSceneObject)
      return;
    this.Networkairbase.RemoveBuilding(this);
    Hangar component;
    if (!this.TryGetComponent<Hangar>(out component))
      return;
    this.Networkairbase.RemoveHangar(component);
  }

  public void RegisterRecentExplosion(GlobalPosition globalPosition, float yield)
  {
    this.recentExplosion = new Building.RecentExplosion(globalPosition, yield);
  }

  private void CheckUnderground()
  {
    Physics.SyncTransforms();
    this.radarAlt = this.definition.height * 0.5f;
    RaycastHit hitInfo1;
    if (!Physics.Linecast(this.transform.position + Vector3.up * 200f, this.transform.position, out hitInfo1, 64 /*0x40*/))
      return;
    if ((UnityEngine.Object) hitInfo1.collider.sharedMaterial == (UnityEngine.Object) GameAssets.i.terrainMaterial)
    {
      RaycastHit hitInfo2;
      if (!Physics.Linecast(hitInfo1.point - Vector3.up * 0.1f, this.transform.position, out hitInfo2, 64 /*0x40*/))
        return;
      this.radarAlt = -hitInfo2.distance;
    }
    else
    {
      if ((double) hitInfo1.point.y >= (double) Datum.LocalSeaY)
        return;
      this.radarAlt = hitInfo1.point.y - Datum.LocalSeaY;
    }
  }

  private void CheckBuildingsBelow()
  {
    Vector3 position = this.transform.position;
    Collider[] componentsInChildren = this.GetComponentsInChildren<Collider>();
    foreach (Collider collider in componentsInChildren)
    {
      if (collider.gameObject.layer != 13)
        collider.gameObject.layer = 2;
    }
    RaycastHit hitInfo;
    Unit component;
    if (Physics.Linecast(position + Vector3.up * 5f, position - Vector3.up * 200f, out hitInfo, 64 /*0x40*/) && hitInfo.collider.gameObject.TryGetComponent<Unit>(out component) && (UnityEngine.Object) component != (UnityEngine.Object) this)
    {
      switch (component)
      {
        case Building building:
          building.onDisableUnit += new Action<Unit>(this.OnBuildingBelowCollapse);
          break;
        case Scenery scenery:
          scenery.onDisableUnit += new Action<Unit>(this.OnSceneryBelowCollapse);
          break;
      }
    }
    foreach (Collider collider in componentsInChildren)
    {
      if (collider.gameObject.layer != 13)
        collider.gameObject.layer = 6;
    }
  }

  private void OnSceneryBelowCollapse(Unit obj)
  {
    if (this.disabled)
      return;
    this.Networkdisabled = true;
  }

  private void OnBuildingBelowCollapse(Unit unit)
  {
    if (this.disabled)
      return;
    this.ReportKilled();
    this.Networkdisabled = true;
  }

  public override void UnitDisabled(bool oldState, bool newState)
  {
    base.UnitDisabled(oldState, newState);
    if (GameManager.gameState == GameState.Editor)
      return;
    if ((UnityEngine.Object) this.repairInstance != (UnityEngine.Object) null)
      UnityEngine.Object.Destroy((UnityEngine.Object) this.repairInstance);
    if (newState)
    {
      if ((UnityEngine.Object) this.wreckage != (UnityEngine.Object) null)
      {
        if (this.wreckageSpawn == null)
          this.wreckageSpawn = new List<GameObject>();
        else
          this.wreckageSpawn.Clear();
        if ((double) Time.timeSinceLevelLoad > 10.0)
        {
          this.wreckageSpawn.Add(UnityEngine.Object.Instantiate<GameObject>(this.wreckage, this.transform.position, this.transform.rotation, Datum.origin));
          FragmentManager component = this.wreckageSpawn[0].GetComponent<FragmentManager>();
          foreach (Renderer componentsInChild in this.gameObject.GetComponentsInChildren<MeshRenderer>())
            componentsInChild.enabled = false;
          foreach (Collider componentsInChild in this.gameObject.GetComponentsInChildren<Collider>())
            componentsInChild.enabled = false;
          if ((UnityEngine.Object) component != (UnityEngine.Object) null)
          {
            this.wreckageSpawn.AddRange((IEnumerable<GameObject>) component.GetWreckageObjects());
            if (this.recentExplosion.IsRecent())
              component.ApplyExplosionForce(this.recentExplosion);
          }
        }
      }
      this.Collapse().Forget();
    }
    if (newState)
      return;
    foreach (UnitPart unitPart in this.partLookup)
      unitPart.Repair();
    this.transform.position = this.startPosition.ToLocalPosition();
    foreach (Renderer componentsInChild in this.gameObject.GetComponentsInChildren<MeshRenderer>())
      componentsInChild.enabled = true;
    foreach (Collider componentsInChild in this.gameObject.GetComponentsInChildren<Collider>())
      componentsInChild.enabled = true;
    if (NetworkManagerNuclearOption.i.Server.Active)
      NetworkSceneSingleton<MessageManager>.i.RpcRepairMessage(this.persistentID);
    this.RegisterUnit(new float?());
    this.damageCredit = (Dictionary<PersistentID, float>) null;
    if (this.wreckageSpawn != null)
    {
      foreach (GameObject gameObject in this.wreckageSpawn)
      {
        if ((UnityEngine.Object) gameObject != (UnityEngine.Object) null)
          UnityEngine.Object.Destroy((UnityEngine.Object) gameObject);
      }
      this.wreckageSpawn.Clear();
    }
    this.collapseTime = 0.0f;
  }

  private async UniTask CheckForCapture()
  {
    Building building = this;
    List<GridSquare> gridSquares = BattlefieldGrid.GetGridSquaresInRange(building.transform.GlobalPosition(), 1000f);
    FactionHQ capturingHQ = (FactionHQ) null;
    float controlBalance = 0.0f;
    CancellationToken cancel = building.destroyCancellationToken;
    UniTask uniTask = UniTask.Delay(UnityEngine.Random.Range(1, 3) * 1000, true);
    await uniTask;
    while (!cancel.IsCancellationRequested)
    {
      controlBalance = Mathf.Min(controlBalance, 1f);
      for (int index1 = 0; index1 < gridSquares.Count; ++index1)
      {
        for (int index2 = 0; index2 < gridSquares[index1].units.Count; ++index2)
        {
          Unit unit = gridSquares[index1].units[index2];
          if (!((UnityEngine.Object) unit == (UnityEngine.Object) null) && !unit.disabled && !((UnityEngine.Object) unit.NetworkHQ == (UnityEngine.Object) null) && unit is GroundVehicle && !FastMath.OutOfRange(building.transform.position, unit.transform.position, building.maxRadius * 2f))
          {
            controlBalance += (UnityEngine.Object) unit.NetworkHQ == (UnityEngine.Object) building.NetworkHQ ? 0.1f : -0.1f;
            if ((UnityEngine.Object) unit.NetworkHQ != (UnityEngine.Object) building.NetworkHQ)
              capturingHQ = unit.NetworkHQ;
          }
        }
      }
      if ((double) controlBalance < 0.0 && (UnityEngine.Object) building.NetworkHQ == (UnityEngine.Object) null)
        controlBalance = -1f;
      if ((double) controlBalance <= -1.0)
        building.NetworkHQ = capturingHQ;
      uniTask = UniTask.Delay(5000, true);
      await uniTask;
    }
    gridSquares = (List<GridSquare>) null;
    capturingHQ = (FactionHQ) null;
    cancel = new CancellationToken();
  }

  private async UniTask Collapse()
  {
    Building building = this;
    Vector3 velocity = Vector3.zero;
    CancellationToken cancel = building.destroyCancellationToken;
    while ((double) building.collapseTime > 0.0)
    {
      await UniTask.Yield();
      if (cancel.IsCancellationRequested)
      {
        cancel = new CancellationToken();
        return;
      }
      velocity += Vector3.down * 9.81f * Time.deltaTime;
      building.collapseTime -= Time.deltaTime;
      building.transform.position += velocity * Time.deltaTime;
    }
    if ((double) building.repairTime < 999999.0)
    {
      building.gameObject.GetComponent<Renderer>().enabled = false;
      foreach (Collider componentsInChild in building.gameObject.GetComponentsInChildren<Collider>())
        componentsInChild.enabled = false;
      cancel = new CancellationToken();
    }
    else if (!NetworkManagerNuclearOption.i.Server.Active)
    {
      cancel = new CancellationToken();
    }
    else
    {
      UnityEngine.Object.Destroy((UnityEngine.Object) building.gameObject);
      cancel = new CancellationToken();
    }
  }

  public float GetRepairPriority(GlobalPosition repairerPosition, float range)
  {
    if (!this.IsRepairable() || !this.needsRepair)
      return 0.0f;
    float num1 = 0.0f;
    if (!this.disabled)
    {
      foreach (UnitPart unitPart in this.partLookup)
        num1 += Mathf.Max(unitPart.hitPoints, 0.0f) * 0.01f;
    }
    float num2 = num1 / (float) this.partLookup.Count;
    float num3 = FastMath.SquareDistance(this.startPosition, repairerPosition);
    float num4 = 1f / num3;
    if ((double) num3 > (double) range * (double) range)
      num4 *= 1f / 1000f;
    if (!this.disabled)
      num4 *= 0.5f;
    if ((UnityEngine.Object) this.repairInstance != (UnityEngine.Object) null)
      num4 *= 0.01f;
    return (1f - num2) * num4 * this.definition.value;
  }

  public bool NeedsRepair() => this.needsRepair;

  public bool CanRearm(bool aircraftRearm, bool vehicleRearm, bool shipRearm)
  {
    return this.canRearm & vehicleRearm;
  }

  public void RequestRearm()
  {
    if (this.canRearm)
      return;
    this.canRearm = true;
    this.NetworkHQ.NotifyNeedsRearm((Unit) this);
  }

  public void Rearm(RearmEventArgs args)
  {
    Action<RearmEventArgs> onRearm = this.OnRearm;
    if (onRearm != null)
      onRearm(args);
    this.canRearm = false;
    this.NetworkHQ.NotifyRearmed((Unit) this);
  }

  public void Building_OnPartDamage(UnitPart.OnApplyDamage e)
  {
    if ((double) this.repairTime >= 999999.0 || this.needsRepair)
      return;
    this.timeLastDamaged = Time.timeSinceLevelLoad;
    this.needsRepair = true;
    if (!((UnityEngine.Object) this.NetworkHQ != (UnityEngine.Object) null))
      return;
    this.NetworkHQ.NotifyNeedsRepair((Unit) this);
  }

  public override void HQChanged(FactionHQ oldHQ, FactionHQ newHQ)
  {
    base.HQChanged(oldHQ, newHQ);
    if (!this.needsRepair || !NetworkManagerNuclearOption.i.Server.Active)
      return;
    this.NetworkHQ.NotifyNeedsRepair((Unit) this);
    if (!((UnityEngine.Object) oldHQ != (UnityEngine.Object) null))
      return;
    oldHQ.NotifyRepaired((Unit) this);
  }

  [ClientRpc]
  private void RpcToggleRepairStatus(bool repairInProgress)
  {
    if (ClientRpcSender.ShouldInvokeLocally((NetworkBehaviour) this, RpcTarget.Observers, (INetworkPlayer) null, false))
      this.UserCode_RpcToggleRepairStatus_1687816056(repairInProgress);
    PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
    writer.WriteBooleanExtension(repairInProgress);
    ClientRpcSender.Send((NetworkBehaviour) this, 20, (NetworkWriter) writer, Mirage.Channel.Reliable, false);
    writer.Release();
  }

  [Mirage.Server]
  public void Repair(Unit repairer, float strength)
  {
    if (!this.IsServer)
      throw new MethodInvocationException("[Server] function 'Repair' called when server not active");
    if ((double) this.repairTime > 999999.0 || !this.needsRepair)
      return;
    if (this.disabled)
      this.RpcToggleRepairStatus(true);
    float num = 0.0f;
    foreach (UnitPart unitPart in this.partLookup)
    {
      unitPart.hitPoints = Mathf.Clamp(unitPart.hitPoints + strength / this.repairTime, 0.0f, 100f);
      num += 100f - unitPart.hitPoints;
    }
    if ((double) num > 0.0)
      return;
    this.needsRepair = false;
    this.NetworkHQ.NotifyRepaired((Unit) this);
    if (!this.disabled)
      return;
    this.Networkdisabled = false;
    this.OnRepairComplete();
  }

  public virtual void OnRepairComplete()
  {
    Debug.Log((object) ("BUILDING REPAIRS COMPLETE " + this.UniqueName));
  }

  private void MirageProcessed()
  {
  }

  public Airbase Networkairbase
  {
    get => (Airbase) this.airbase.Value;
    set => this.airbase.Value = (NetworkBehaviour) value;
  }

  public override bool SerializeSyncVars(NetworkWriter writer, bool initialize)
  {
    ulong syncVarDirtyBits = this.SyncVarDirtyBits;
    bool flag = base.SerializeSyncVars(writer, initialize);
    if (initialize)
    {
      writer.WriteNetworkBehaviorSyncVar(this.airbase);
      return true;
    }
    writer.Write(syncVarDirtyBits >> 9, 1);
    return flag;
  }

  public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
  {
    base.DeserializeSyncVars(reader, initialState);
    if (initialState)
      this.airbase = reader.ReadNetworkBehaviourSyncVar();
    else
      this.SetDeserializeMask(reader.Read(1), 9);
  }

  private void UserCode_RpcSetAirbase_\u002D1101525328(Airbase airbase)
  {
    this.Networkairbase = airbase;
    airbase.AddBuilding(this, true);
  }

  protected static void Skeleton_RpcSetAirbase_\u002D1101525328(
    NetworkBehaviour behaviour,
    NetworkReader reader,
    INetworkPlayer senderConnection,
    int replyId)
  {
    ((Building) behaviour).UserCode_RpcSetAirbase_\u002D1101525328(GeneratedNetworkCode._Read_Airbase(reader));
  }

  private void UserCode_RpcToggleRepairStatus_1687816056(bool repairInProgress)
  {
    if (repairInProgress)
    {
      if (!((UnityEngine.Object) this.repairInstance == (UnityEngine.Object) null) || !((UnityEngine.Object) this.repairPrefab != (UnityEngine.Object) null))
        return;
      this.repairInstance = UnityEngine.Object.Instantiate<GameObject>(this.repairPrefab, this.transform);
      this.repairInstance.transform.position = this.startPosition.ToLocalPosition() - this.definition.spawnOffset;
      this.repairInstance.transform.localScale = new Vector3(this.definition.width * 1.05f, this.definition.height, this.definition.length * 1.05f);
    }
    else
    {
      if (!((UnityEngine.Object) this.repairInstance != (UnityEngine.Object) null))
        return;
      UnityEngine.Object.Destroy((UnityEngine.Object) this.repairInstance);
    }
  }

  protected static void Skeleton_RpcToggleRepairStatus_1687816056(
    NetworkBehaviour behaviour,
    NetworkReader reader,
    INetworkPlayer senderConnection,
    int replyId)
  {
    ((Building) behaviour).UserCode_RpcToggleRepairStatus_1687816056(reader.ReadBooleanExtension());
  }

  protected override int GetRpcCount() => 21;

  public override void RegisterRpc(RemoteCallCollection collection)
  {
    base.RegisterRpc(collection);
    collection.Register(19, "Building.RpcSetAirbase", false, RpcInvokeType.ClientRpc, (NetworkBehaviour) this, new RpcDelegate(Building.Skeleton_RpcSetAirbase_\u002D1101525328));
    collection.Register(20, "Building.RpcToggleRepairStatus", false, RpcInvokeType.ClientRpc, (NetworkBehaviour) this, new RpcDelegate(Building.Skeleton_RpcToggleRepairStatus_1687816056));
  }

  public struct RecentExplosion(GlobalPosition globalPosition, float yield)
  {
    private readonly float time = Time.timeSinceLevelLoad;
    public readonly GlobalPosition globalPosition = globalPosition;
    public readonly float yield = yield;

    public bool IsRecent() => (double) Time.timeSinceLevelLoad - (double) this.time < 1.0;
  }
}

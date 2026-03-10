// Decompiled with JetBrains decompiler
// Type: Spawner
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
using UnityEngine;

#nullable disable
public class Spawner : NetworkSceneSingleton<Spawner>
{
  private readonly LazySortedQueue<SavedAircraft> pendingPlayerControlled = new LazySortedQueue<SavedAircraft>(new Comparison<SavedAircraft>(Spawner.SortControlledAircraft));
  private bool requestSpawnInProgress;
  [NonSerialized]
  private const int SYNC_VAR_COUNT = 0;
  [NonSerialized]
  private const int RPC_COUNT = 2;

  public static int SortControlledAircraft(SavedAircraft x, SavedAircraft y)
  {
    return y.playerControlledPriority.CompareTo(x.playerControlledPriority);
  }

  public GameObject SpawnLocal(GameObject prefab, Transform parent)
  {
    return UnityEngine.Object.Instantiate<GameObject>(prefab, parent);
  }

  public void DestroyLocal(GameObject gameObject, float delay)
  {
    UnityEngine.Object.Destroy((UnityEngine.Object) gameObject, delay);
  }

  public bool TrySpawnPlayerControlled(Player player, out Aircraft aircraft)
  {
    SavedAircraft savedAircraft;
    return !this.pendingPlayerControlled.TryDequeue(out savedAircraft) ? Spawner.Fail<Aircraft>(out aircraft) : this.TrySpawnPlayerAircraft(savedAircraft, player, out aircraft);
  }

  private bool TrySpawnPlayerAircraft(
    SavedAircraft savedAircraft,
    Player player,
    out Aircraft aircraft)
  {
    this.RpcOnGivenAircraftControl(player.Owner);
    MissionManager.CurrentMission.missionSettings.SetMinimumStartingRank(Spawner.GetAircraftRank(savedAircraft));
    if (!this.TrySpawnAircraft(savedAircraft, player, out aircraft))
      return false;
    this.AfterSpawn((SavedUnit) savedAircraft, (Unit) aircraft);
    return true;
  }

  private static int GetAircraftRank(SavedAircraft savedAircraft)
  {
    return ((AircraftDefinition) Encyclopedia.Lookup[savedAircraft.type]).aircraftParameters.rankRequired;
  }

  [ClientRpc(target = RpcTarget.Player)]
  public void RpcOnGivenAircraftControl(INetworkPlayer _)
  {
    if (ClientRpcSender.ShouldInvokeLocally((NetworkBehaviour) this, RpcTarget.Player, _, false))
    {
      this.UserCode_RpcOnGivenAircraftControl_2052851100(this.Client.Player);
    }
    else
    {
      PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
      ClientRpcSender.SendTarget((NetworkBehaviour) this, 0, (NetworkWriter) writer, Mirage.Channel.Reliable, _);
      writer.Release();
    }
  }

  public void SpawnSavedUnit(SavedUnit savedUnit)
  {
    Unit unit;
    if (!this.TrySpawnSavedUnitInternal(savedUnit, out unit))
      return;
    this.AfterSpawn(savedUnit, unit);
  }

  private void AfterSpawn(SavedUnit savedUnit, Unit spawnedUnit)
  {
    spawnedUnit.LinkSavedUnit(savedUnit);
    if (string.IsNullOrEmpty(savedUnit.UniqueName))
      return;
    UnitRegistry.RegisterCustomID(savedUnit.UniqueName, spawnedUnit);
  }

  private bool TrySpawnSavedUnitInternal(SavedUnit savedUnit, out Unit unit)
  {
    switch (savedUnit)
    {
      case SavedAircraft savedAircraft:
        if (!savedAircraft.playerControlled || GameManager.gameState == GameState.Editor)
        {
          Aircraft spawnedAircraft;
          return Spawner.Result<Aircraft, Unit>(this.TrySpawnAircraft(savedAircraft, out spawnedAircraft), spawnedAircraft, out unit);
        }
        this.pendingPlayerControlled.Enqueue(savedAircraft);
        return Spawner.Fail<Unit>(out unit);
      case SavedVehicle savedVehicle:
        GroundVehicle spawnedVehicle;
        if (!this.TrySpawnVehicle(savedVehicle, out spawnedVehicle))
          return Spawner.Fail<Unit>(out unit);
        if (savedVehicle.waypoints.Count > 0)
          spawnedVehicle.GetMissionWaypoints(savedVehicle);
        return Spawner.Success<GroundVehicle, Unit>(spawnedVehicle, out unit);
      case SavedShip savedShip:
        Ship spawnedShip;
        if (!this.TrySpawnShip(savedShip, out spawnedShip))
          return Spawner.Fail<Unit>(out unit);
        if (savedShip.waypoints.Count > 0)
          spawnedShip.GetMissionWaypoints(savedShip);
        return Spawner.Success<Ship, Unit>(spawnedShip, out unit);
      case SavedBuilding savedBuilding:
        Building spawnedBuilding;
        return this.TrySpawnBuilding(savedBuilding, out spawnedBuilding) ? Spawner.Success<Building, Unit>(spawnedBuilding, out unit) : Spawner.Fail<Unit>(out unit);
      case SavedScenery savedScenery:
        Scenery spawnedScenery;
        return this.TrySpawnScenery(savedScenery, out spawnedScenery) ? Spawner.Success<Scenery, Unit>(spawnedScenery, out unit) : Spawner.Fail<Unit>(out unit);
      case SavedContainer savedContainer:
        Container spawnedContainer;
        return this.TrySpawnContainer(savedContainer, out spawnedContainer) ? Spawner.Success<Container, Unit>(spawnedContainer, out unit) : Spawner.Fail<Unit>(out unit);
      case SavedMissile savedMissile:
        Missile spawnedMissile;
        return this.TrySpawnMissile(savedMissile, out spawnedMissile) ? Spawner.Success<Missile, Unit>(spawnedMissile, out unit) : Spawner.Fail<Unit>(out unit);
      case SavedPilot savedPilot:
        PilotDismounted spawnedPilot;
        return this.TrySpawnPilot(savedPilot, out spawnedPilot) ? Spawner.Success<PilotDismounted, Unit>(spawnedPilot, out unit) : Spawner.Fail<Unit>(out unit);
      default:
        throw new ArgumentException($"No Spawn code for SpawnUnit of type {savedUnit?.GetType()}");
    }
  }

  public void SpawnFromMissionInEditor(Mission mission, Action<Unit, SavedUnit> registerEditorUnit)
  {
    if (mission == null)
      return;
    foreach (SavedScenery saved in mission.scenery)
      FindOrSpawn<SavedScenery, Scenery>(saved, new Spawner.TrySpawnAction<SavedScenery, Scenery>(this.TrySpawnScenery));
    foreach (SavedContainer container in mission.containers)
      FindOrSpawn<SavedContainer, Container>(container, new Spawner.TrySpawnAction<SavedContainer, Container>(this.TrySpawnContainer));
    foreach (SavedMissile missile in mission.missiles)
      FindOrSpawn<SavedMissile, Missile>(missile, new Spawner.TrySpawnAction<SavedMissile, Missile>(this.TrySpawnMissile));
    foreach (SavedPilot pilot in mission.pilots)
      FindOrSpawn<SavedPilot, PilotDismounted>(pilot, new Spawner.TrySpawnAction<SavedPilot, PilotDismounted>(this.TrySpawnPilot));
    foreach (SavedBuilding building in mission.buildings)
      FindOrSpawn<SavedBuilding, Building>(building, new Spawner.TrySpawnAction<SavedBuilding, Building>(this.TrySpawnBuilding));
    foreach (SavedShip ship in mission.ships)
      FindOrSpawn<SavedShip, Ship>(ship, new Spawner.TrySpawnAction<SavedShip, Ship>(this.TrySpawnShip));
    foreach (SavedVehicle vehicle in mission.vehicles)
      FindOrSpawn<SavedVehicle, GroundVehicle>(vehicle, new Spawner.TrySpawnAction<SavedVehicle, GroundVehicle>(this.TrySpawnVehicle));
    foreach (SavedAircraft saved in mission.aircraft)
      FindOrSpawn<SavedAircraft, Aircraft>(saved, new Spawner.TrySpawnAction<SavedAircraft, Aircraft>(this.TrySpawnAircraft));
    Physics.SyncTransforms();

    void FindOrSpawn<TSaved, TUnit>(
      TSaved saved,
      Spawner.TrySpawnAction<TSaved, TUnit> trySpawnAction)
      where TSaved : SavedUnit
      where TUnit : Unit
    {
      Unit unit1;
      if (UnitRegistry.customIDLookup.TryGetValue(saved.UniqueName, out unit1))
      {
        registerEditorUnit(unit1, (SavedUnit) saved);
      }
      else
      {
        TUnit unit2;
        if (!trySpawnAction(saved, out unit2))
          return;
        registerEditorUnit((Unit) unit2, (SavedUnit) saved);
      }
    }
  }

  public Unit SpawnFromUnitDefinitionInEditor(
    UnitDefinition placingDefinition,
    GlobalPosition position,
    Quaternion rotation,
    FactionHQ factionHq,
    string uniqueName)
  {
    switch (placingDefinition)
    {
      case VehicleDefinition vehicleDefinition:
        return (Unit) this.SpawnVehicle(vehicleDefinition.unitPrefab, position, rotation, Vector3.zero, factionHq, uniqueName, 1f, false, (Player) null);
      case ShipDefinition shipDefinition:
        return (Unit) this.SpawnShip(shipDefinition.unitPrefab, position, rotation, factionHq, uniqueName, 1f, false);
      case BuildingDefinition buildingDefinition:
        return (Unit) this.SpawnBuilding(buildingDefinition.unitPrefab, position, rotation, factionHq, (Airbase) null, uniqueName, false, (SavedBuilding.FactoryOptions) null);
      case AircraftDefinition aircraftDefinition:
        return (Unit) this.SpawnAircraft((Player) null, aircraftDefinition.unitPrefab, (Loadout) null, 1f, new LiveryKey(), position, rotation, Vector3.zero, (Hangar) null, factionHq, uniqueName, 1f, 0.5f);
      case SceneryDefinition sceneryDefinition:
        return (Unit) this.SpawnScenery(sceneryDefinition.unitPrefab, position, rotation, uniqueName);
      case MissileDefinition missileDefinition:
        return (Unit) this.SpawnSavedMissile(missileDefinition.unitPrefab, position, rotation, Vector3.zero, uniqueName);
      case null:
        throw new ArgumentException($"Can't spawn object with unity type: {placingDefinition?.GetType()}");
      default:
        UnitDefinition unitDefinition = placingDefinition;
        return unitDefinition.code == "PILOT" ? (Unit) this.SpawnPilot(unitDefinition.unitPrefab, position, rotation, factionHq, uniqueName) : (Unit) this.SpawnContainer(unitDefinition.unitPrefab, position, rotation, factionHq, uniqueName);
    }
  }

  public bool TrySpawnVehicle(SavedVehicle savedVehicle, out GroundVehicle spawnedVehicle)
  {
    GameObject prefab;
    if (!Encyclopedia.i.TryGetPrefab(savedVehicle.type, out prefab))
      return Spawner.Fail<GroundVehicle>(out spawnedVehicle);
    spawnedVehicle = this.SpawnVehicle(prefab, savedVehicle.globalPosition, savedVehicle.rotation, Vector3.zero, FactionRegistry.HqFromName(savedVehicle.faction), savedVehicle.UniqueName, savedVehicle.skill, savedVehicle.holdPosition, (Player) null);
    return true;
  }

  public GroundVehicle SpawnVehicle(
    GameObject prefab,
    GlobalPosition globalPosition,
    Quaternion rotation,
    Vector3 velocity,
    FactionHQ hq,
    string uniqueName,
    float skill,
    bool holdPosition,
    Player player)
  {
    GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(prefab);
    gameObject.transform.position = globalPosition.ToLocalPosition();
    gameObject.transform.rotation = rotation;
    GroundVehicle component = gameObject.GetComponent<GroundVehicle>();
    component.rb.MovePosition(globalPosition.ToLocalPosition());
    component.rb.MoveRotation(rotation);
    component.rb.velocity = velocity;
    component.Networkowner = player;
    component.NetworkHQ = hq;
    component.NetworkUniqueName = uniqueName;
    component.NetworkstartPosition = globalPosition;
    component.NetworkstartRotation = rotation;
    component.NetworkunitName = component.definition.unitName;
    component.skill = skill;
    component.SetHoldPosition(holdPosition);
    this.ServerObjectManager.Spawn(gameObject);
    return component;
  }

  public bool TrySpawnShip(SavedShip savedShip, out Ship spawnedShip)
  {
    GameObject prefab;
    if (!Encyclopedia.i.TryGetPrefab(savedShip.type, out prefab))
      return Spawner.Fail<Ship>(out spawnedShip);
    spawnedShip = this.SpawnShip(prefab, savedShip.globalPosition, savedShip.rotation, FactionRegistry.HqFromName(savedShip.faction), savedShip.UniqueName, savedShip.skill, savedShip.holdPosition);
    return true;
  }

  public Ship SpawnShip(
    GameObject prefab,
    GlobalPosition globalPosition,
    Quaternion rotation,
    FactionHQ HQ,
    string uniqueName,
    float skill,
    bool holdPosition)
  {
    GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(prefab);
    gameObject.transform.position = globalPosition.ToLocalPosition();
    gameObject.transform.rotation = rotation;
    Ship component1 = gameObject.GetComponent<Ship>();
    component1.NetworkHQ = HQ;
    component1.NetworkUniqueName = uniqueName;
    component1.NetworkstartPosition = globalPosition;
    component1.NetworkstartRotation = rotation;
    component1.NetworkunitName = component1.definition.unitName;
    component1.skill = skill;
    component1.SetHoldPosition(holdPosition);
    Airbase component2;
    if (component1.TryGetComponent<Airbase>(out component2))
      component2.SetupAttachedAirbase((Unit) component1);
    this.ServerObjectManager.Spawn(gameObject);
    return component1;
  }

  private bool TrySpawnAircraft(SavedAircraft savedAircraft, out Aircraft spawnedAircraft)
  {
    return this.TrySpawnAircraft(savedAircraft, (Player) null, out spawnedAircraft);
  }

  private bool TrySpawnAircraft(
    SavedAircraft savedAircraft,
    Player player,
    out Aircraft spawnedAircraft)
  {
    GameObject prefab;
    if (!Encyclopedia.i.TryGetPrefab(savedAircraft.type, out prefab))
      return Spawner.Fail<Aircraft>(out spawnedAircraft);
    spawnedAircraft = this.SpawnAircraft(player, prefab, savedAircraft.savedLoadout.CreateLoadout(prefab), savedAircraft.fuel, savedAircraft.liveryKey, savedAircraft.globalPosition, savedAircraft.rotation, savedAircraft.startingSpeed * (savedAircraft.rotation * Vector3.forward), (Hangar) null, FactionRegistry.HqFromName(savedAircraft.faction), savedAircraft.UniqueName, savedAircraft.skill, savedAircraft.bravery);
    return true;
  }

  public Aircraft SpawnAircraft(
    Player player,
    GameObject prefab,
    Loadout loadout,
    float fuelLevel,
    LiveryKey livery,
    GlobalPosition globalPosition,
    Quaternion rotation,
    Vector3 startingVel,
    Hangar spawningHangar,
    FactionHQ HQ,
    string uniqueName,
    float skill,
    float bravery)
  {
    PlayerRef playerRef = (UnityEngine.Object) player != (UnityEngine.Object) null ? player.PlayerRef : PlayerRef.Invalid;
    Vector3 localPosition = globalPosition.ToLocalPosition();
    GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(prefab, localPosition, rotation);
    Aircraft component = gameObject.GetComponent<Aircraft>();
    component.NetworkHQ = HQ;
    component.NetworkUniqueName = uniqueName;
    component.NetworkspawningHangar = spawningHangar;
    component.NetworkstartPosition = globalPosition;
    component.NetworkstartRotation = rotation;
    component.NetworkstartingVelocity = startingVel;
    component.Networkloadout = loadout;
    component.NetworkfuelLevel = Mathf.Clamp01(fuelLevel);
    component.skill = skill;
    component.bravery = bravery;
    component.SetLiveryKey(livery);
    component.NetworkplayerRef = playerRef;
    component.NetworkunitName = (UnityEngine.Object) player != (UnityEngine.Object) null ? $"{player.GetNameOrCensored()} [{component.definition.unitName}]" : component.definition.unitName;
    if ((UnityEngine.Object) player != (UnityEngine.Object) null)
    {
      this.ServerObjectManager.Spawn(gameObject, player.Owner);
      return component;
    }
    this.ServerObjectManager.Spawn(gameObject);
    return component;
  }

  private bool TrySpawnBuilding(SavedBuilding savedBuilding, out Building spawnedBuilding)
  {
    GameObject prefab;
    if (!Encyclopedia.i.TryGetPrefab(savedBuilding.type, out prefab))
      return Spawner.Fail<Building>(out spawnedBuilding);
    if ((double) savedBuilding.placementOffset != 0.0)
    {
      SavedBuilding savedBuilding1 = savedBuilding;
      savedBuilding1.globalPosition = savedBuilding1.globalPosition + Vector3.up * savedBuilding.placementOffset;
      savedBuilding.placementOffset = 0.0f;
    }
    Airbase airbase = savedBuilding.AirbaseRef?.Airbase;
    spawnedBuilding = this.SpawnBuilding(prefab, savedBuilding.globalPosition, savedBuilding.rotation, FactionRegistry.HqFromName(savedBuilding.faction), airbase, savedBuilding.UniqueName, savedBuilding.capturable, savedBuilding.factoryOptions);
    return true;
  }

  public Building SpawnBuilding(
    GameObject prefab,
    GlobalPosition globalPosition,
    Quaternion rotation,
    FactionHQ HQ,
    Airbase airbase,
    string uniqueName,
    bool capturable,
    SavedBuilding.FactoryOptions factoryOptions)
  {
    GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(prefab);
    gameObject.transform.SetPositionAndRotation(globalPosition.ToLocalPosition(), rotation);
    Building component1 = gameObject.GetComponent<Building>();
    component1.NetworkHQ = HQ;
    component1.NetworkUniqueName = uniqueName;
    component1.NetworkstartPosition = globalPosition;
    component1.NetworkstartRotation = rotation;
    component1.NetworkunitName = component1.definition.unitName;
    component1.capturable = capturable;
    if ((UnityEngine.Object) airbase != (UnityEngine.Object) null)
      component1.SetAirbase(airbase);
    Factory component2;
    if (factoryOptions != null && component1.TryGetComponent<Factory>(out component2))
      component2.SetFactory(factoryOptions);
    this.ServerObjectManager.Spawn(gameObject);
    return component1;
  }

  private bool TrySpawnScenery(SavedScenery savedScenery, out Scenery spawnedScenery)
  {
    GameObject prefab;
    if (!Encyclopedia.i.TryGetPrefab(savedScenery.type, out prefab))
      return Spawner.Fail<Scenery>(out spawnedScenery);
    spawnedScenery = this.SpawnScenery(prefab, savedScenery.globalPosition, savedScenery.rotation, savedScenery.UniqueName);
    return true;
  }

  public Scenery SpawnScenery(
    GameObject prefab,
    GlobalPosition globalPosition,
    Quaternion rotation,
    string uniqueName)
  {
    GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(prefab);
    gameObject.transform.SetPositionAndRotation(globalPosition.ToLocalPosition(), rotation);
    Scenery component = gameObject.GetComponent<Scenery>();
    component.NetworkUniqueName = uniqueName;
    component.NetworkstartPosition = globalPosition;
    component.NetworkstartRotation = rotation;
    component.NetworkunitName = component.definition.unitName;
    this.ServerObjectManager.Spawn(gameObject);
    return component;
  }

  private bool TrySpawnContainer(SavedContainer savedContainer, out Container spawnedContainer)
  {
    GameObject prefab;
    if (!Encyclopedia.i.TryGetPrefab(savedContainer.type, out prefab))
      return Spawner.Fail<Container>(out spawnedContainer);
    spawnedContainer = this.SpawnContainer(prefab, savedContainer.globalPosition, savedContainer.rotation, FactionRegistry.HqFromName(savedContainer.faction), savedContainer.UniqueName);
    return true;
  }

  public Container SpawnContainer(
    GameObject prefab,
    GlobalPosition globalPosition,
    Quaternion rotation,
    FactionHQ hq,
    string uniqueName)
  {
    GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(prefab);
    gameObject.transform.SetPositionAndRotation(globalPosition.ToLocalPosition(), rotation);
    Container component = gameObject.GetComponent<Container>();
    component.NetworkUniqueName = uniqueName;
    component.NetworkstartPosition = globalPosition;
    component.NetworkstartRotation = rotation;
    component.NetworkHQ = hq;
    component.NetworkunitName = component.definition.unitName;
    this.ServerObjectManager.Spawn(gameObject);
    return component;
  }

  private bool TrySpawnPilot(SavedPilot savedPilot, out PilotDismounted spawnedPilot)
  {
    GameObject prefab;
    if (!Encyclopedia.i.TryGetPrefab(savedPilot.type, out prefab))
      return Spawner.Fail<PilotDismounted>(out spawnedPilot);
    spawnedPilot = this.SpawnPilot(prefab, savedPilot.globalPosition, savedPilot.rotation, FactionRegistry.HqFromName(savedPilot.faction), savedPilot.UniqueName);
    return true;
  }

  public PilotDismounted SpawnPilot(
    GameObject prefab,
    GlobalPosition globalPosition,
    Quaternion rotation,
    FactionHQ hq,
    string uniqueName)
  {
    GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(prefab);
    gameObject.transform.SetPositionAndRotation(globalPosition.ToLocalPosition(), rotation);
    PilotDismounted component = gameObject.GetComponent<PilotDismounted>();
    component.NetworkUniqueName = uniqueName;
    component.NetworkstartPosition = globalPosition;
    component.NetworkstartRotation = rotation;
    component.NetworkHQ = hq;
    component.NetworkunitName = component.definition.unitName;
    component.SetCollidable(true);
    this.ServerObjectManager.Spawn(gameObject);
    return component;
  }

  private bool TrySpawnMissile(SavedMissile savedMissile, out Missile spawnedMissile)
  {
    GameObject prefab;
    if (!Encyclopedia.i.TryGetPrefab(savedMissile.type, out prefab))
      return Spawner.Fail<Missile>(out spawnedMissile);
    spawnedMissile = this.SpawnSavedMissile(prefab, savedMissile.globalPosition, savedMissile.rotation, savedMissile.startingSpeed * (savedMissile.rotation * Vector3.forward), savedMissile.UniqueName);
    return true;
  }

  public Missile SpawnSavedMissile(
    GameObject prefab,
    GlobalPosition globalPosition,
    Quaternion rotation,
    Vector3 startingVel,
    string uniqueName)
  {
    GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(prefab);
    gameObject.transform.SetPositionAndRotation(globalPosition.ToLocalPosition(), rotation);
    Missile component = gameObject.GetComponent<Missile>();
    component.NetworkUniqueName = uniqueName;
    component.NetworkstartPosition = globalPosition;
    component.NetworkstartRotation = rotation;
    component.NetworkstartingVelocity = startingVel;
    component.NetworkunitName = component.definition.unitName;
    gameObject.GetComponent<Rigidbody>().velocity = startingVel;
    this.ServerObjectManager.Spawn(gameObject);
    return component;
  }

  [Mirage.Server]
  public Missile SpawnMissileEncyclopedia(MissileDefinition missile, Transform spawnTransform)
  {
    if (!this.IsServer)
      throw new MethodInvocationException("[Server] function 'SpawnMissileEncyclopedia' called when server not active");
    Vector3 position = spawnTransform.position + missile.spawnOffset.y * Vector3.up;
    GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(missile.unitPrefab, position, spawnTransform.rotation);
    Missile component = gameObject.GetComponent<Missile>();
    component.NetworkunitName = component.definition.unitName;
    component.NetworkstartPosition = position.ToGlobalPosition();
    this.ServerObjectManager.Spawn(gameObject);
    return component;
  }

  [Mirage.Server]
  public Missile SpawnMissile(
    MissileDefinition missile,
    Vector3 launchPosition,
    Quaternion rotation,
    Vector3 velocity,
    Unit target,
    Unit owner)
  {
    if (!this.IsServer)
      throw new MethodInvocationException("[Server] function 'SpawnMissile' called when server not active");
    GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(missile.unitPrefab, launchPosition, rotation);
    gameObject.GetComponent<Rigidbody>().velocity = velocity;
    Missile component = gameObject.GetComponent<Missile>();
    component.NetworkHQ = (UnityEngine.Object) owner != (UnityEngine.Object) null ? owner.NetworkHQ : (FactionHQ) null;
    component.NetworkunitName = component.definition.unitName;
    component.NetworkownerID = (UnityEngine.Object) owner != (UnityEngine.Object) null ? owner.persistentID : PersistentID.None;
    component.SetTarget(target);
    component.NetworkstartPosition = launchPosition.ToGlobalPosition();
    component.NetworkstartOffsetFromOwner = launchPosition - owner.transform.position;
    component.NetworkstartingVelocity = velocity;
    component.NetworkstartRotation = rotation;
    this.ServerObjectManager.Spawn(gameObject);
    return component;
  }

  [Mirage.Server]
  public Missile SpawnMissile(
    GameObject missile,
    Vector3 launchPosition,
    Quaternion rotation,
    Vector3 velocity,
    Unit target,
    Unit owner)
  {
    if (!this.IsServer)
      throw new MethodInvocationException("[Server] function 'SpawnMissile' called when server not active");
    GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(missile, launchPosition, rotation);
    gameObject.GetComponent<Rigidbody>().velocity = velocity;
    Missile component = gameObject.GetComponent<Missile>();
    component.NetworkHQ = (UnityEngine.Object) owner != (UnityEngine.Object) null ? owner.NetworkHQ : (FactionHQ) null;
    component.NetworkunitName = component.definition.unitName;
    component.NetworkownerID = (UnityEngine.Object) owner != (UnityEngine.Object) null ? owner.persistentID : PersistentID.None;
    component.SetTarget(target);
    component.NetworkstartPosition = launchPosition.ToGlobalPosition();
    component.NetworkstartOffsetFromOwner = launchPosition - owner.transform.position;
    component.NetworkstartingVelocity = velocity;
    component.NetworkstartRotation = rotation;
    this.ServerObjectManager.Spawn(gameObject);
    return component;
  }

  [Mirage.Server]
  public Unit SpawnUnit(
    UnitDefinition unit,
    Vector3 spawnPosition,
    Quaternion rotation,
    Vector3 velocity,
    Unit owner,
    Player player)
  {
    if (!this.IsServer)
      throw new MethodInvocationException("[Server] function 'SpawnUnit' called when server not active");
    if (unit is VehicleDefinition)
      return (Unit) this.SpawnVehicle(unit.unitPrefab, spawnPosition.ToGlobalPosition(), rotation, velocity, owner.NetworkHQ, unit.unitName, 1f, false, player);
    GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(unit.unitPrefab, spawnPosition, rotation);
    Rigidbody component1 = gameObject.GetComponent<Rigidbody>();
    if ((UnityEngine.Object) component1 != (UnityEngine.Object) null)
      component1.velocity = velocity;
    Unit component2 = gameObject.GetComponent<Unit>();
    component2.NetworkHQ = (UnityEngine.Object) owner != (UnityEngine.Object) null ? owner.NetworkHQ : (FactionHQ) null;
    component2.NetworkunitName = component2.definition.unitName;
    component2.NetworkstartPosition = spawnPosition.ToGlobalPosition();
    component2.NetworkstartRotation = rotation;
    if (component2 is Container container)
      container.NetworkownerID = owner.persistentID;
    this.ServerObjectManager.Spawn(gameObject);
    return component2;
  }

  public async UniTask<bool> RequestSpawnAtAirbase(
    Airbase airbase,
    AircraftDefinition definition,
    LiveryKey livery,
    Loadout playerLoadout,
    float fuelLevel)
  {
    if (this.requestSpawnInProgress)
      throw new InvalidOperationException("Spawn request already pending");
    Player player;
    if (!GameManager.GetLocalPlayer<Player>(out player))
    {
      Debug.LogError((object) "No Local player, can't request spawn");
      return false;
    }
    if ((double) fuelLevel < 0.0 || (double) fuelLevel > 1.0)
    {
      Debug.LogError((object) "Fuel should be between 0 and 1");
      return false;
    }
    if (!this.AllowedToSpawn(airbase, definition, playerLoadout, player))
      return false;
    bool delaySpawn = false;
    this.requestSpawnInProgress = true;
    try
    {
      player.SetSpawnPending(true);
      Airbase.TrySpawnResult trySpawnResult = await this.CmdRequestSpawnAircraft(airbase, definition, livery, playerLoadout, fuelLevel);
      delaySpawn = trySpawnResult.DelayedSpawn;
      if (!trySpawnResult.Allowed)
        return false;
      Hangar hangar = trySpawnResult.Hangar;
      if ((UnityEngine.Object) hangar != (UnityEngine.Object) null)
        hangar.CheckAttachCamera();
      return true;
    }
    finally
    {
      this.requestSpawnInProgress = false;
      if (!delaySpawn)
        player.SetSpawnPending(false);
    }
  }

  private bool AllowedToSpawn(
    Airbase airbase,
    AircraftDefinition definition,
    Loadout playerLoadout,
    Player player)
  {
    if ((UnityEngine.Object) player.HQ == (UnityEngine.Object) null)
    {
      ColorLog<Spawner>.InfoWarn("Client Spawn Request failed: player not part of faction");
      return false;
    }
    if ((UnityEngine.Object) player.HQ != (UnityEngine.Object) airbase.CurrentHQ)
    {
      ColorLog<Spawner>.InfoWarn($"Client Spawn Request failed: player HQ ({player.HQ}) was not the same as airbase HQ ({airbase.CurrentHQ})");
      return false;
    }
    if (airbase.CurrentHQ.restrictedAircraft.Contains(definition.unitPrefab.name))
    {
      ColorLog<Spawner>.InfoWarn($"Client Spawn Request failed: {definition.unitName} not allowed {airbase}");
      return false;
    }
    if (player.OwnsAirframe(definition, true))
      return true;
    ColorLog<Spawner>.InfoWarn($"Client Spawn Request failed: {player.PlayerName} doesn't own {definition.unitName}");
    return false;
  }

  [ServerRpc(requireAuthority = false)]
  public UniTask<Airbase.TrySpawnResult> CmdRequestSpawnAircraft(
    Airbase airbase,
    AircraftDefinition definition,
    LiveryKey livery,
    Loadout loadout,
    float fuelAmount,
    INetworkPlayer sender = null)
  {
    if (ServerRpcSender.ShouldInvokeLocally((NetworkBehaviour) this, false, false))
      return this.UserCode_CmdRequestSpawnAircraft_\u002D421181233(airbase, definition, livery, loadout, fuelAmount, this.Server.LocalPlayer);
    PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
    GeneratedNetworkCode._Write_Airbase((NetworkWriter) writer, airbase);
    writer.WriteAircraftDefinition(definition);
    GeneratedNetworkCode._Write_LiveryKey((NetworkWriter) writer, livery);
    GeneratedNetworkCode._Write_NuclearOption\u002ESavedMission\u002ELoadout((NetworkWriter) writer, loadout);
    writer.WriteSingleConverter(fuelAmount);
    UniTask<Airbase.TrySpawnResult> uniTask = ServerRpcSender.SendWithReturn<Airbase.TrySpawnResult>((NetworkBehaviour) this, 1, (NetworkWriter) writer, false);
    writer.Release();
    return uniTask;
  }

  private Airbase.TrySpawnResult TrySpawnAircraft(
    Airbase airbase,
    AircraftDefinition definition,
    LiveryKey livery,
    Loadout loadout,
    float fuelLevel,
    INetworkPlayer sender)
  {
    if ((UnityEngine.Object) airbase == (UnityEngine.Object) null)
      return new Airbase.TrySpawnResult();
    if ((UnityEngine.Object) definition == (UnityEngine.Object) null)
      return new Airbase.TrySpawnResult();
    if (!NetworkFloatHelper.Validate(fuelLevel, false, (string) null))
      return new Airbase.TrySpawnResult();
    if ((double) fuelLevel < 0.0 || (double) fuelLevel > 1.0)
    {
      sender.SetError(1, PlayerErrorFlags.LikelyCheater);
      return new Airbase.TrySpawnResult();
    }
    Player player;
    if (!sender.TryGetPlayer<Player>(out player))
    {
      Debug.LogError((object) "Sender did not have player object");
      return new Airbase.TrySpawnResult();
    }
    if (!this.AllowedToSpawn(airbase, definition, loadout, player))
      return new Airbase.TrySpawnResult();
    WeaponChecker.VetLoadout(definition, loadout, player, airbase.CurrentHQ);
    return airbase.TrySpawnAircraft(player, definition, livery, loadout, fuelLevel);
  }

  private static bool Result<TIn, TOut>(bool success, TIn result, out TOut unit)
    where TIn : TOut
    where TOut : Unit
  {
    unit = (TOut) (success ? result : default (TIn));
    return success;
  }

  private static bool Success<TIn, TOut>(TIn result, out TOut unit)
    where TIn : TOut
    where TOut : Unit
  {
    unit = (TOut) result;
    return true;
  }

  private static bool Fail<T>(out T unit) where T : Unit
  {
    unit = default (T);
    return false;
  }

  private void MirageProcessed()
  {
  }

  public void UserCode_RpcOnGivenAircraftControl_2052851100(INetworkPlayer _)
  {
    SceneSingleton<GameplayUI>.i.HideSelectAirbase();
  }

  protected static void Skeleton_RpcOnGivenAircraftControl_2052851100(
    NetworkBehaviour behaviour,
    NetworkReader reader,
    INetworkPlayer senderConnection,
    int replyId)
  {
    ((Spawner) behaviour).UserCode_RpcOnGivenAircraftControl_2052851100(behaviour.Client.Player);
  }

  public UniTask<Airbase.TrySpawnResult> UserCode_CmdRequestSpawnAircraft_\u002D421181233(
    Airbase airbase,
    AircraftDefinition definition,
    LiveryKey livery,
    Loadout loadout,
    float fuelAmount,
    INetworkPlayer sender)
  {
    Airbase.TrySpawnResult trySpawnResult = new Airbase.TrySpawnResult();
    try
    {
      trySpawnResult = this.TrySpawnAircraft(airbase, definition, livery, loadout, fuelAmount, sender);
    }
    catch (Exception ex)
    {
      Debug.LogException(ex);
    }
    return UniTask.FromResult<Airbase.TrySpawnResult>(trySpawnResult);
  }

  protected static UniTask<Airbase.TrySpawnResult> Skeleton_CmdRequestSpawnAircraft_\u002D421181233(
    NetworkBehaviour behaviour,
    NetworkReader reader,
    INetworkPlayer senderConnection,
    int replyId)
  {
    return ((Spawner) behaviour).UserCode_CmdRequestSpawnAircraft_\u002D421181233(GeneratedNetworkCode._Read_Airbase(reader), reader.ReadAircraftDefinition(), GeneratedNetworkCode._Read_LiveryKey(reader), GeneratedNetworkCode._Read_NuclearOption\u002ESavedMission\u002ELoadout(reader), reader.ReadSingleConverter(), senderConnection);
  }

  protected override int GetRpcCount() => 2;

  public override void RegisterRpc(RemoteCallCollection collection)
  {
    base.RegisterRpc(collection);
    collection.Register(0, "Spawner.RpcOnGivenAircraftControl", false, RpcInvokeType.ClientRpc, (NetworkBehaviour) this, new RpcDelegate(Spawner.Skeleton_RpcOnGivenAircraftControl_2052851100));
    collection.RegisterRequest<Airbase.TrySpawnResult>(1, "Spawner.CmdRequestSpawnAircraft", false, RpcInvokeType.ServerRpc, (NetworkBehaviour) this, new RequestDelegate<Airbase.TrySpawnResult>(Spawner.Skeleton_CmdRequestSpawnAircraft_\u002D421181233));
  }

  private delegate bool TrySpawnAction<TSaved, TUnit>(TSaved saved, out TUnit unit)
    where TSaved : SavedUnit
    where TUnit : Unit;
}

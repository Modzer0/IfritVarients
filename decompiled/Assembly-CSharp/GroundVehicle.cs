// Decompiled with JetBrains decompiler
// Type: GroundVehicle
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Cysharp.Threading.Tasks;
using Mirage;
using Mirage.RemoteCalls;
using Mirage.Serialization;
using NuclearOption.DebugScripts;
using NuclearOption.Jobs;
using NuclearOption.Networking;
using NuclearOption.SavedMission;
using NuclearOption.SavedMission.ObjectiveV2;
using System;
using System.Collections.Generic;
using System.Threading;
using Unity.Profiling;
using UnityEngine;

#nullable disable
public class GroundVehicle : Unit, IRearmable, ICommandable
{
  private static readonly ProfilerMarker setArgsFieldsMarker = new ProfilerMarker("GroundVehicle SetArgs_fields");
  private static readonly ProfilerMarker setArgsPathFinderMarker = new ProfilerMarker("GroundVehicle SetArgs_pathfinder");
  private static readonly ProfilerMarker setArgsObstaclesMarker = new ProfilerMarker("GroundVehicle SetArgs_obstacles");
  [Header("Config")]
  [SerializeField]
  private bool mobile = true;
  [SerializeField]
  private float topSpeedOnroad;
  [SerializeField]
  private float topSpeedOffroad;
  [SerializeField]
  private float acceleration;
  [SerializeField]
  private float suspensionTravel;
  [SerializeField]
  private float springRate;
  [SerializeField]
  private float dampingRate;
  [SerializeField]
  private float frictionCoef;
  [Header("AI")]
  [SerializeField]
  private bool holdAtPlayerCommandedDestination = true;
  [SerializeField]
  private bool navigateToObjectives = true;
  [Header("References")]
  [SerializeField]
  private GameObject wreckage;
  [SerializeField]
  private GameObject parachuteSystem;
  [SerializeField]
  private UnitCommand unitCommand;
  [Header("Visuals")]
  [SerializeField]
  private GameObject[] wheels;
  [SerializeField]
  private GroundVehicle.DeployablePart[] deployableParts;
  [SerializeField]
  private float wheelRadius = 0.3f;
  [SerializeField]
  private Renderer tracks;
  [Header("Sound")]
  [SerializeField]
  private AudioSource engineIdleSound;
  [SerializeField]
  private AudioSource engineDriveSound;
  [SerializeField]
  private float engineMinPitch = 0.5f;
  [SerializeField]
  private float engineMaxPitch = 1f;
  [SerializeField]
  private float enginePitchMult = 5f;
  [SyncVar(initialOnly = true)]
  public NetworkBehaviorSyncvar owner;
  [SyncVar(hook = "OnStationaryChanged")]
  private bool networkStationary;
  private PtrAllocation<GroundVehicleFields> JobFields;
  private NuclearOption.Jobs.JobPart<GroundVehicle, GroundVehicleFields> JobPart;
  [NonSerialized]
  public float skill = 1f;
  private PathfindingAgent pathfinder;
  private bool canRearm;
  private readonly List<Obstacle> obstacles = new List<Obstacle>();
  private GlobalPosition destination;
  private bool commandedDestination;
  private bool holdPosition;
  private bool anchored;
  private float inertiaTensor;
  private List<VehicleWaypoint> savedWaypoints = new List<VehicleWaypoint>();
  private Unit unitBelow;
  protected Collider[] colliders;
  private GameObject debugSteerMarker;
  private GameObject debugPlayerCommandClickPosition;
  private GameObject debugPlayerCommandPosition;
  private bool wheelsLocked;
  private bool slung;
  [NonSerialized]
  public Rigidbody hitRB;
  private bool resetStationary;
  [NonSerialized]
  private const int SYNC_VAR_COUNT = 11;
  [NonSerialized]
  private const int RPC_COUNT = 20;

  public event Action<RearmEventArgs> OnRearm;

  public UnitCommand UnitCommand => this.unitCommand;

  bool ICommandable.Disabled => this.disabled;

  FactionHQ ICommandable.HQ => this.NetworkHQ;

  public override void Awake()
  {
    base.Awake();
    this.Identity.OnStartClient.AddListener(new Action(this.OnStartClient));
    this.Identity.OnStartServer.AddListener(new Action(this.OnStartServer));
    this.pathfinder = new PathfindingAgent((Unit) this);
  }

  private void OnStartServer()
  {
    this.unitCommand.ProcessSetDestination += new UnitCommand.ProcessCommand(this.UnitCommand_ProcessSetDestination);
    this.JobPart = new NuclearOption.Jobs.JobPart<GroundVehicle, GroundVehicleFields>(this, this.GetOrCreateJobField());
    JobManager.Add(this.JobPart);
    if (GameManager.gameState == GameState.Encyclopedia)
      this.NetworknetworkStationary = true;
    if (!this.holdPosition || (double) Vector3.Dot(this.transform.up, Vector3.up) >= 0.5 || !this.TryAttachToSurface())
      return;
    this.NetworknetworkStationary = true;
  }

  protected override void OnDestroy()
  {
    base.OnDestroy();
    JobManager.Remove(ref this.JobPart);
    GroundVehicle.DisposeJobFields(ref this.JobFields);
    MissionManager.onObjectiveStarted -= new Action<Objective>(this.Vehicle_OnObjectiveStarted);
    if (!DebugVis.Enabled)
      return;
    if ((UnityEngine.Object) this.debugPlayerCommandClickPosition != (UnityEngine.Object) null)
      UnityEngine.Object.Destroy((UnityEngine.Object) this.debugPlayerCommandClickPosition);
    if (!((UnityEngine.Object) this.debugPlayerCommandPosition != (UnityEngine.Object) null))
      return;
    UnityEngine.Object.Destroy((UnityEngine.Object) this.debugPlayerCommandPosition);
  }

  public void GetMissionWaypoints(SavedVehicle savedVehicle)
  {
    this.savedWaypoints = new List<VehicleWaypoint>((IEnumerable<VehicleWaypoint>) savedVehicle.waypoints);
    MissionManager.onObjectiveStarted += new Action<Objective>(this.Vehicle_OnObjectiveStarted);
    if (!(this.savedWaypoints[0].objective == "Unit Spawn"))
      return;
    this.unitCommand.SetDestination(this.savedWaypoints[0].position, false);
    this.savedWaypoints.RemoveAt(0);
  }

  [Mirage.Server]
  private bool TryAttachToSurface()
  {
    if (!this.IsServer)
      throw new MethodInvocationException("[Server] function 'TryAttachToSurface' called when server not active");
    Vector3 position1;
    Quaternion rotation1;
    this.transform.GetPositionAndRotation(out position1, out rotation1);
    if (Physics.Raycast(new Ray(position1, rotation1 * Vector3.down), out this.hit, 10f, 64 /*0x40*/))
    {
      this.unitBelow = this.hit.collider.gameObject.GetComponent<Unit>();
      if ((UnityEngine.Object) this.unitBelow != (UnityEngine.Object) null)
        this.unitBelow.onDisableUnit += new Action<Unit>(this.Vehicle_OnUnitBelowDestroyed);
      Quaternion rotation2 = Quaternion.LookRotation(Vector3.Cross(this.hit.normal, rotation1 * Vector3.left), this.hit.normal);
      Vector3 position2 = this.hit.point + rotation2 * Vector3.up * this.definition.spawnOffset.y;
      this.transform.SetPositionAndRotation(position2, rotation2);
      this.NetworkstartPosition = position2.ToGlobalPosition();
      this.anchored = this.holdPosition;
      return true;
    }
    this.anchored = false;
    return false;
  }

  public GlobalPosition GetDestination() => this.destination;

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

  public float GetTopSpeed() => this.topSpeedOnroad;

  public void MoveFromDepot()
  {
    this.unitCommand.SetDestination(this.transform.GlobalPosition() + this.transform.forward * 35f, false);
  }

  public void SetHoldPosition(bool hold) => this.holdPosition = hold;

  public bool GetHoldPosition() => this.holdPosition;

  public override bool IsSlung() => this.slung;

  public void StowTurrets(bool stowed)
  {
    foreach (WeaponStation weaponStation in this.weaponStations)
    {
      foreach (Turret turret in weaponStation.Turrets)
      {
        if ((UnityEngine.Object) turret != (UnityEngine.Object) null)
          turret.SetStowed(stowed);
      }
    }
  }

  [ClientRpc]
  public void RpcDeployFireControl(bool deploy)
  {
    if (ClientRpcSender.ShouldInvokeLocally((NetworkBehaviour) this, RpcTarget.Observers, (INetworkPlayer) null, false))
      this.UserCode_RpcDeployFireControl_549297332(deploy);
    PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
    writer.WriteBooleanExtension(deploy);
    ClientRpcSender.Send((NetworkBehaviour) this, 19, (NetworkWriter) writer, Mirage.Channel.Reliable, false);
    writer.Release();
  }

  public override void AttachOrDetachSlingHook(Aircraft aircraft, bool attached)
  {
    base.AttachOrDetachSlingHook(aircraft, attached);
    this.SetWheelsLocked(attached);
    this.StowTurrets(attached);
    this.enabled = !attached;
    this.slung = attached;
    if (this.IsServer)
    {
      this.resetStationary = true;
      this.NetworknetworkStationary = false;
      if ((UnityEngine.Object) aircraft.Player != (UnityEngine.Object) null)
        this.Networkowner = aircraft.Player;
      if (!attached && !this.gameObject.TryGetComponent<ImpactDetector>(out ImpactDetector _))
        this.gameObject.AddComponent<ImpactDetector>().SetGLimit(100f);
    }
    BoxCollider component;
    if (this.gameObject.TryGetComponent<BoxCollider>(out component))
      component.enabled = attached;
    this.radarAlt = 100f;
  }

  public void SetWheelsLocked(bool wheelsLocked) => this.wheelsLocked = wheelsLocked;

  public void OnStationaryChanged(bool nowStationary)
  {
    this.rb.isKinematic = nowStationary;
    if (nowStationary)
      this.rb.interpolation = RigidbodyInterpolation.None;
    this.DeployParts(nowStationary).Forget();
    if (this.IsServer)
    {
      if ((UnityEngine.Object) this.unitBelow != (UnityEngine.Object) null)
      {
        this.unitBelow.onDisableUnit -= new Action<Unit>(this.Vehicle_OnUnitBelowDestroyed);
        this.unitBelow = (Unit) null;
      }
      if (nowStationary)
        this.TryAttachToSurface();
    }
    if (!this.IsClient || !nowStationary || !((UnityEngine.Object) this.engineDriveSound != (UnityEngine.Object) null))
      return;
    this.engineDriveSound.Stop();
    this.engineIdleSound.Stop();
  }

  private void Vehicle_OnObjectiveStarted(Objective objective)
  {
    for (int index = this.savedWaypoints.Count - 1; index >= 0; --index)
    {
      if (this.savedWaypoints[index].objective == objective.SavedObjective.UniqueName)
      {
        this.unitCommand.SetDestination(this.savedWaypoints[index].position, false);
        this.savedWaypoints.RemoveAt(index);
      }
    }
  }

  private void Vehicle_OnUnitBelowDestroyed(Unit unit)
  {
    this.resetStationary = true;
    this.unitBelow.onDisableUnit -= new Action<Unit>(this.Vehicle_OnUnitBelowDestroyed);
    this.unitBelow = (Unit) null;
  }

  private void OnStartClient()
  {
    if (GameManager.gameState == GameState.Encyclopedia)
      this.DeployParts(true).Forget();
    this.SetLocalSim(NetworkManagerNuclearOption.i.Server.Active);
    if (this.remoteSim && GameManager.IsLocalPlayer<Player>(this.Networkowner))
      this.ClientCollisionDelay().Forget();
    this.transform.SetPositionAndRotation(this.startPosition.ToLocalPosition(), this.startRotation);
    if ((UnityEngine.Object) this.parachuteSystem != (UnityEngine.Object) null && !Physics.Linecast(this.startPosition.ToLocalPosition(), this.startPosition.ToLocalPosition() - Vector3.up * 10f, 8256))
      UnityEngine.Object.Instantiate<GameObject>(this.parachuteSystem, this.transform).GetComponent<CargoDeploymentSystem>().Initialize((Unit) this);
    this.RegisterUnit(new float?(4f));
    this.InitializeUnit();
    this.OnStationaryChanged(this.networkStationary);
  }

  public override void InitializeUnit()
  {
    base.InitializeUnit();
    this.colliders = this.gameObject.GetComponents<Collider>();
    this.destination = this.transform.GlobalPosition();
    if (!NetworkManagerNuclearOption.i.Server.Active || GameManager.gameState == GameState.Editor)
      return;
    this.StartSlowUpdateDelayed(4f, new Action(this.CheckObstacles));
  }

  public override void SetLocalSim(bool localSim)
  {
    base.SetLocalSim(localSim);
    this.rb.useGravity = localSim;
  }

  private async UniTask ClientCollisionDelay()
  {
    GroundVehicle groundVehicle = this;
    groundVehicle.transform.gameObject.layer = 15;
    CancellationToken cancel = groundVehicle.destroyCancellationToken;
    await UniTask.Delay(5000);
    if (cancel.IsCancellationRequested)
    {
      cancel = new CancellationToken();
    }
    else
    {
      groundVehicle.transform.gameObject.layer = 0;
      cancel = new CancellationToken();
    }
  }

  private async UniTask DeployParts(bool deployed)
  {
    GroundVehicle groundVehicle = this;
    bool partsMoving = true;
    CancellationToken cancel = groundVehicle.destroyCancellationToken;
    if (groundVehicle.deployableParts.Length == 0)
      cancel = new CancellationToken();
    else if (cancel.IsCancellationRequested)
    {
      cancel = new CancellationToken();
    }
    else
    {
      foreach (GroundVehicle.DeployablePart deployablePart in groundVehicle.deployableParts)
        deployablePart.StartSequence(deployed);
      while (partsMoving)
      {
        partsMoving = false;
        foreach (GroundVehicle.DeployablePart deployablePart in groundVehicle.deployableParts)
        {
          if (deployablePart.TryAnimate())
            partsMoving = true;
        }
        await UniTask.Yield();
        if (cancel.IsCancellationRequested)
        {
          cancel = new CancellationToken();
          return;
        }
      }
      foreach (GroundVehicle.DeployablePart deployablePart in groundVehicle.deployableParts)
        deployablePart.CompleteSequence(deployed);
      cancel = new CancellationToken();
    }
  }

  private void CheckObstacles()
  {
    if ((double) this.speed > 1.0)
      this.UpdateObstacles();
    GlobalPosition destination;
    if (!this.holdPosition && !this.commandedDestination && MissionPosition.TryGetClosestPosition((Unit) this, out destination))
    {
      float num1 = FastMath.Distance(this.transform.GlobalPosition(), this.destination);
      float num2 = FastMath.Distance(this.destination, destination);
      if (this.destination != destination && !this.pathfinder.IsOnBridge() && (double) num2 / (double) num1 > 0.20000000298023224)
        this.unitCommand.SetDestination(destination, false);
    }
    if (this.commandedDestination && !this.holdAtPlayerCommandedDestination && FastMath.InRange(this.GlobalPosition(), this.destination, 50f) && (double) Mathf.Abs(this.speed) < 1.0)
      this.commandedDestination = false;
    if (!PlayerSettings.debugVis || !((UnityEngine.Object) SceneSingleton<CameraStateManager>.i.followingUnit == (UnityEngine.Object) this))
      return;
    Debug.Log((object) $"Setting destination for {this.unitName}, holdPosition == {this.holdPosition}, commandedDestination == {this.commandedDestination}");
  }

  private void UpdateObstacles()
  {
    this.obstacles.Clear();
    GridSquare gridSquare;
    if (!BattlefieldGrid.TryGetGridSquare(this.transform.GlobalPosition(), out gridSquare))
      return;
    Vector3 position = this.transform.position;
    foreach (Unit unit in gridSquare.units)
    {
      if (!((UnityEngine.Object) unit == (UnityEngine.Object) this) && unit.definition.IsObstacle && FastMath.InRange(unit.transform.position, position, 200f))
        this.obstacles.Add(new Obstacle(unit.transform, unit.maxRadius * 1.4f, unit.obstacleTop));
    }
    for (int index = gridSquare.obstacles.Count - 1; index >= 0; --index)
    {
      Obstacle obstacle = gridSquare.obstacles[index];
      if ((UnityEngine.Object) obstacle.Transform == (UnityEngine.Object) null)
        gridSquare.obstacles.RemoveAt(index);
      else if (FastMath.InRange(obstacle.Transform.position, position, 200f))
        this.obstacles.Add(obstacle);
    }
  }

  public override void UnitDisabled(bool oldState, bool newState)
  {
    base.UnitDisabled(oldState, newState);
    if (GameManager.gameState != GameState.Encyclopedia)
      this.WreckAndRemove().Forget();
    if (!NetworkManagerNuclearOption.i.Server.Active)
      return;
    this.NetworknetworkStationary = false;
    MissionManager.onObjectiveStarted -= new Action<Objective>(this.Vehicle_OnObjectiveStarted);
  }

  private async UniTask WreckAndRemove()
  {
    GroundVehicle groundVehicle = this;
    CancellationToken cancel = groundVehicle.destroyCancellationToken;
    UniTask uniTask = UniTask.Delay(5000);
    await uniTask;
    if (cancel.IsCancellationRequested)
    {
      cancel = new CancellationToken();
    }
    else
    {
      while (!groundVehicle.rb.isKinematic && (double) groundVehicle.transform.position.y > (double) Datum.LocalSeaY - 10.0 && (double) groundVehicle.rb.velocity.sqrMagnitude > 1.0)
      {
        uniTask = UniTask.Delay(1000);
        await uniTask;
        if (cancel.IsCancellationRequested)
        {
          cancel = new CancellationToken();
          return;
        }
      }
      groundVehicle.enabled = false;
      groundVehicle.SpawnWreckage();
      foreach (Renderer componentsInChild in groundVehicle.GetComponentsInChildren<Renderer>())
        componentsInChild.enabled = false;
      foreach (UnityEngine.Object collider in groundVehicle.colliders)
        UnityEngine.Object.Destroy(collider);
      if ((UnityEngine.Object) groundVehicle.unitBelow != (UnityEngine.Object) null)
      {
        groundVehicle.unitBelow.onDisableUnit -= new Action<Unit>(groundVehicle.Vehicle_OnUnitBelowDestroyed);
        groundVehicle.unitBelow = (Unit) null;
      }
      groundVehicle.rb.isKinematic = true;
      uniTask = UniTask.Delay(5000);
      await uniTask;
      if (cancel.IsCancellationRequested)
        cancel = new CancellationToken();
      else if (!NetworkManagerNuclearOption.i.Server.Active)
      {
        cancel = new CancellationToken();
      }
      else
      {
        UnityEngine.Object.Destroy((UnityEngine.Object) groundVehicle.gameObject);
        cancel = new CancellationToken();
      }
    }
  }

  private void SpawnWreckage()
  {
    GridSquare gridSquare;
    if (!BattlefieldGrid.TryGetGridSquare(this.transform.GlobalPosition(), out gridSquare))
      return;
    foreach (DamageParticles spawnedEffect in this.spawnedEffects)
    {
      if ((UnityEngine.Object) spawnedEffect != (UnityEngine.Object) null)
        spawnedEffect.ParentObjectCulled();
    }
    if ((UnityEngine.Object) this.wreckage == (UnityEngine.Object) null)
      return;
    GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.wreckage, this.transform.position, this.transform.rotation);
    gameObject.transform.SetParent((UnityEngine.Object) this.unitBelow != (UnityEngine.Object) null ? this.unitBelow.transform : Datum.origin);
    if (Physics.Linecast(this.transform.position, this.transform.position - this.transform.up * 10f, out this.hit, 64 /*0x40*/))
      gameObject.transform.SetParent(this.hit.collider.transform);
    gridSquare.obstacles.Add(new Obstacle(gameObject.transform, this.definition.length, float.MaxValue));
  }

  private void UnitCommand_ProcessSetDestination(ref UnitCommand.Command command)
  {
    if (this.pathfinder.IsOnBridge())
      return;
    GlobalPosition position = command.position;
    command.position = this.pathfinder.GetDryPosition(this.transform.GlobalPosition(), command.position);
    this.anchored = false;
    this.destination = command.position;
    this.pathfinder.Pathfind(NetworkSceneSingleton<LevelInfo>.i.roadNetwork, command.position, false, (Transform) null);
    this.resetStationary = true;
    if (!command.FromPlayer)
      return;
    this.commandedDestination = true;
    if (!DebugVis.Enabled)
      return;
    if (DebugVis.Create<GameObject>(ref this.debugPlayerCommandPosition, GameAssets.i.debugArrowGreen))
    {
      this.debugPlayerCommandPosition.name = $"Debug CommandPosition {this.name} Id={this.persistentID}";
      this.debugPlayerCommandPosition.transform.forward = Vector3.down;
      this.debugPlayerCommandPosition.transform.localScale = new Vector3(10f, 10f, 30f);
    }
    if (DebugVis.Create<GameObject>(ref this.debugPlayerCommandClickPosition, GameAssets.i.debugArrowGreen))
    {
      this.debugPlayerCommandClickPosition.name = $"Debug CommandPositionClick {this.name} Id={this.persistentID}";
      this.debugPlayerCommandClickPosition.transform.forward = Vector3.down;
      this.debugPlayerCommandClickPosition.transform.localScale = new Vector3(10f, 10f, 30f);
      MeshRenderer component;
      if (this.debugPlayerCommandClickPosition.TryGetComponent<MeshRenderer>(out component))
        component.material.color = Color.cyan;
    }
    this.debugPlayerCommandPosition.transform.position = command.position.ToLocalPosition() + new Vector3(0.0f, 30f, 0.0f);
    RaycastHit hit;
    if ((double) position.y == 0.0 && PathfindingAgent.RaycastTerrain(position, out hit))
      position.y = hit.point.GlobalY();
    this.debugPlayerCommandClickPosition.transform.position = position.ToLocalPosition() + new Vector3(0.0f, 30f, 0.0f);
  }

  private void Update()
  {
    if (this.IsClientOnly)
      this.speed = Vector3.Dot(this.rb.velocity, this.transform.forward);
    if ((double) this.displayDetail < 1.0)
    {
      if ((UnityEngine.Object) this.engineIdleSound != (UnityEngine.Object) null && this.engineIdleSound.isPlaying)
        this.engineIdleSound.Stop();
      if ((UnityEngine.Object) this.engineDriveSound != (UnityEngine.Object) null && this.engineDriveSound.isPlaying)
        this.engineDriveSound.Stop();
    }
    else
    {
      if ((UnityEngine.Object) this.engineDriveSound != (UnityEngine.Object) null && (UnityEngine.Object) this.engineIdleSound != (UnityEngine.Object) null)
      {
        if (!this.disabled)
        {
          this.engineDriveSound.pitch = Mathf.Clamp(this.speed * this.enginePitchMult / Mathf.Max(this.topSpeedOnroad, 1f), this.engineMinPitch, this.engineMaxPitch);
          if ((double) this.speed < 3.0)
          {
            if (!this.engineIdleSound.isPlaying)
            {
              this.engineIdleSound.Play();
              this.engineIdleSound.time = UnityEngine.Random.Range(0.0f, this.engineIdleSound.clip.length);
            }
            if ((double) this.engineIdleSound.volume < 0.5)
              this.engineIdleSound.volume += Time.deltaTime;
            if ((double) this.engineDriveSound.volume > 0.0)
              this.engineDriveSound.volume -= Time.deltaTime;
            else
              this.engineDriveSound.Stop();
          }
          else
          {
            if ((double) this.engineIdleSound.volume > 0.0)
              this.engineIdleSound.volume -= Time.deltaTime;
            else
              this.engineIdleSound.Stop();
            if (!this.engineDriveSound.isPlaying)
            {
              this.engineDriveSound.Play();
              this.engineDriveSound.time = UnityEngine.Random.Range(0.0f, this.engineIdleSound.clip.length);
            }
            if ((double) this.engineDriveSound.volume < 0.5)
              this.engineDriveSound.volume += Time.deltaTime;
            else
              this.engineDriveSound.volume = (float) (0.5 + (double) this.engineDriveSound.pitch * 0.5);
          }
        }
        else
        {
          float num = Time.deltaTime * 0.5f;
          this.engineDriveSound.volume -= num;
          if ((double) this.engineDriveSound.pitch - (double) num > 0.0)
            this.engineDriveSound.pitch -= num;
          this.engineIdleSound.volume -= num;
          if ((double) this.engineIdleSound.pitch - (double) num > 0.0)
            this.engineIdleSound.pitch -= num;
        }
      }
      this.AnimateWheels(this.speed);
    }
    if (!DebugVis.Enabled || !((UnityEngine.Object) this.debugSteerMarker != (UnityEngine.Object) null) || !this.JobFields.IsCreated)
      return;
    GroundVehicleFields groundVehicleFields = this.JobFields.Ref();
    if (!groundVehicleFields.steeringInfoNullable.HasValue)
      return;
    this.debugSteerMarker.transform.rotation = Quaternion.LookRotation(groundVehicleFields.steeringInfoNullable.Value.steerVector);
  }

  public void AnimateWheels(float speed)
  {
    if (this.wheelsLocked)
      return;
    if ((UnityEngine.Object) this.tracks != (UnityEngine.Object) null)
      this.tracks.material.mainTextureOffset += new Vector2(0.0f, speed * -0.2f * Time.deltaTime);
    if (this.wheels.Length == 0)
      return;
    Vector3 eulers = new Vector3(speed * 57.29578f * Time.deltaTime / this.wheelRadius, 0.0f, 0.0f);
    for (int index = 0; index < this.wheels.Length; ++index)
      this.wheels[index].transform.Rotate(eulers, Space.Self);
  }

  public void ContactDustParticles(Vector3 point)
  {
    if ((double) this.speed <= 2.0 || !FastMath.InRange(this.transform.position, SceneSingleton<CameraStateManager>.i.transform.position, 3000f))
      return;
    SceneSingleton<ParticleEffectManager>.i.EmitParticles("ContactDust", 1, point.ToGlobalPosition(), this.rb.velocity, 0.0f, Mathf.Max((float) (5.0 - (double) this.speed * 0.10000000149011612), 1f), 0.5f, Mathf.Max(this.maxRadius * 2f, 3f) + Mathf.Min(this.speed * 0.05f, 10f), 0.3f, this.speed * 0.3f, 0.5f, 0.2f);
  }

  private static void DisposeJobFields(ref PtrAllocation<GroundVehicleFields> fields)
  {
    if (fields.IsCreated)
      fields.Ref().ObstaclesArray.Dispose();
    fields.Dispose();
  }

  public Transform GetJobTransforms() => this.transform;

  private Ptr<GroundVehicleFields> GetOrCreateJobField()
  {
    if (!this.JobFields.IsCreated)
    {
      JobsAllocator<GroundVehicleFields>.Allocate(ref this.JobFields);
      ref GroundVehicleFields local = ref this.JobFields.Ref();
      local.maxRadius = this.maxRadius;
      local.acceleration = this.acceleration;
      this.rb.mass = this.definition.mass;
      local.mass = this.rb.mass;
      local.inertiaTensor = (float) (((double) this.rb.inertiaTensor.x + (double) this.rb.inertiaTensor.y + (double) this.rb.inertiaTensor.z) / 3.0);
      local.mobile = this.mobile;
      local.topSpeedOnroad = this.topSpeedOnroad;
      local.topSpeedOffroad = this.topSpeedOffroad;
      local.suspensionTravel = this.suspensionTravel;
      local.dampingRate = this.dampingRate;
      local.springRate = this.springRate;
      local.frictionCoef = this.frictionCoef;
    }
    return (Ptr<GroundVehicleFields>) this.JobFields;
  }

  ~GroundVehicle() => GroundVehicle.DisposeJobFields(ref this.JobFields);

  public void UpdateJobFields()
  {
    ref GroundVehicleFields local = ref this.JobFields.Ref();
    local.monoBehaviourEnabled = this.enabled;
    if (!local.monoBehaviourEnabled)
      return;
    local.velocity = this.rb.velocity;
    local.angularVelocity = this.rb.angularVelocity;
    local.unitDisabled = this.disabled;
    if (this.resetStationary)
    {
      this.resetStationary = false;
      local.stationary = false;
      local.stationaryTime = 0.0f;
    }
    local.DEBUG_VIS = PlayerSettings.debugVis && (UnityEngine.Object) SceneSingleton<CameraStateManager>.i.followingUnit == (UnityEngine.Object) this;
  }

  public void UpdateJobFields_Pathfinder()
  {
    ref GroundVehicleFields local = ref this.JobFields.Ref();
    if (!local.monoBehaviourEnabled)
      return;
    local.steeringInfoNullable = (SteeringInfo?) this.pathfinder?.GetSteerpoint(this.transform.GlobalPosition(), this.transform.forward, this.speed, false);
    if (!PlayerSettings.debugVis || !((UnityEngine.Object) SceneSingleton<CameraStateManager>.i.followingUnit == (UnityEngine.Object) this) || !local.steeringInfoNullable.HasValue || !DebugVis.Create<GameObject>(ref this.debugSteerMarker, GameAssets.i.debugArrowGreen, this.transform))
      return;
    this.debugSteerMarker.transform.localPosition = Vector3.zero;
    this.debugSteerMarker.transform.localScale = new Vector3(4f, 4f, 12f);
  }

  public void UpdateJobFields_Obstacles()
  {
    ref GroundVehicleFields local = ref this.JobFields.Ref();
    if (!local.monoBehaviourEnabled)
      return;
    GroundVehicle.ObstacleCopy(ref local.ObstaclesArray, this.obstacles);
    if (!PlayerSettings.debugVis || !((UnityEngine.Object) SceneSingleton<CameraStateManager>.i.followingUnit == (UnityEngine.Object) this))
      return;
    foreach (Obstacle obstacle in this.obstacles)
    {
      if ((UnityEngine.Object) obstacle.Transform != (UnityEngine.Object) null)
      {
        GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(GameAssets.i.debugSphere, Datum.origin);
        gameObject.transform.position = obstacle.Transform.position;
        gameObject.transform.localScale = Vector3.one * (obstacle.Radius * 2f);
        UnityEngine.Object.Destroy((UnityEngine.Object) gameObject, 0.2f);
      }
    }
  }

  public static void ObstacleCopy(ref PtrList<ObstaclePosition> array, List<Obstacle> obstacles)
  {
    int count = obstacles.Count;
    array.EnsureCapacity(count);
    array.Length = count;
    for (int index = 0; index < count; ++index)
    {
      Obstacle obstacle = obstacles[index];
      ObstaclePosition obstaclePosition1;
      if ((UnityEngine.Object) obstacle.Transform != (UnityEngine.Object) null)
      {
        ref ObstaclePosition local = ref array[index];
        obstaclePosition1 = new ObstaclePosition();
        obstaclePosition1.Position = obstacle.Transform.position;
        obstaclePosition1.Radius = obstacle.Radius;
        obstaclePosition1.Top = obstacle.Top;
        ObstaclePosition obstaclePosition2 = obstaclePosition1;
        local = obstaclePosition2;
      }
      else
      {
        ref ObstaclePosition local = ref array[index];
        obstaclePosition1 = new ObstaclePosition();
        obstaclePosition1.Radius = 0.0f;
        ObstaclePosition obstaclePosition3 = obstaclePosition1;
        local = obstaclePosition3;
      }
    }
  }

  public void ApplyJobFields()
  {
    if (!this.JobFields.IsCreated)
      return;
    ref GroundVehicleFields local = ref this.JobFields.Ref();
    if (!local.monoBehaviourEnabled || !this.enabled)
      return;
    if (local.underwater)
      this.Networkdisabled = true;
    local.ApplyForce(this.rb);
    this.speed = local.speed;
    this.radarAlt = local.radarAlt;
    if (this.anchored)
      return;
    this.NetworknetworkStationary = local.stationary;
  }

  private void MirageProcessed()
  {
  }

  public Player Networkowner
  {
    get => (Player) this.owner.Value;
    set => this.owner.Value = (NetworkBehaviour) value;
  }

  public bool NetworknetworkStationary
  {
    get => this.networkStationary;
    set
    {
      if (this.SyncVarEqual<bool>(value, this.networkStationary))
        return;
      bool networkStationary = this.networkStationary;
      this.networkStationary = value;
      this.SetDirtyBit(1024UL /*0x0400*/);
      if (!this.GetSyncVarHookGuard(1024UL /*0x0400*/) && this.IsHost)
      {
        this.SetSyncVarHookGuard(1024UL /*0x0400*/, true);
        this.OnStationaryChanged(value);
        this.SetSyncVarHookGuard(1024UL /*0x0400*/, false);
      }
    }
  }

  public override bool SerializeSyncVars(NetworkWriter writer, bool initialize)
  {
    ulong syncVarDirtyBits = this.SyncVarDirtyBits;
    bool flag = base.SerializeSyncVars(writer, initialize);
    if (initialize)
    {
      writer.WriteNetworkBehaviorSyncVar(this.owner);
      writer.WriteBooleanExtension(this.networkStationary);
      return true;
    }
    writer.Write(syncVarDirtyBits >> 9, 2);
    if (((long) syncVarDirtyBits & 1024L /*0x0400*/) != 0L)
    {
      writer.WriteBooleanExtension(this.networkStationary);
      flag = true;
    }
    return flag;
  }

  public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
  {
    base.DeserializeSyncVars(reader, initialState);
    if (initialState)
    {
      this.owner = reader.ReadNetworkBehaviourSyncVar();
      bool networkStationary = this.networkStationary;
      this.networkStationary = reader.ReadBooleanExtension();
      if (this.IsServer || this.SyncVarEqual<bool>(networkStationary, this.networkStationary))
        return;
      this.OnStationaryChanged(this.networkStationary);
    }
    else
    {
      ulong dirtyBit = reader.Read(2);
      this.SetDeserializeMask(dirtyBit, 9);
      if (((long) dirtyBit & 2L) == 0L)
        return;
      bool networkStationary = this.networkStationary;
      this.networkStationary = reader.ReadBooleanExtension();
      if (!this.IsServer && !this.SyncVarEqual<bool>(networkStationary, this.networkStationary))
        this.OnStationaryChanged(this.networkStationary);
    }
  }

  public void UserCode_RpcDeployFireControl_549297332(bool deploy)
  {
    FireControl component;
    if (!this.gameObject.TryGetComponent<FireControl>(out component))
      return;
    component.DeployOrStowLaunchers(deploy);
  }

  protected static void Skeleton_RpcDeployFireControl_549297332(
    NetworkBehaviour behaviour,
    NetworkReader reader,
    INetworkPlayer senderConnection,
    int replyId)
  {
    ((GroundVehicle) behaviour).UserCode_RpcDeployFireControl_549297332(reader.ReadBooleanExtension());
  }

  protected override int GetRpcCount() => 20;

  public override void RegisterRpc(RemoteCallCollection collection)
  {
    base.RegisterRpc(collection);
    collection.Register(19, "GroundVehicle.RpcDeployFireControl", false, RpcInvokeType.ClientRpc, (NetworkBehaviour) this, new RpcDelegate(GroundVehicle.Skeleton_RpcDeployFireControl_549297332));
  }

  public struct VehicleInputs
  {
    public float throttle;
    public float brake;
    public float steering;
  }

  [Serializable]
  private class DeployablePart
  {
    [SerializeField]
    private Transform transform;
    [SerializeField]
    private float deployRate;
    [SerializeField]
    private Vector3 angleStowed;
    [SerializeField]
    private Vector3 angleDeployed;
    [SerializeField]
    private Vector3 positionStowed;
    [SerializeField]
    private Vector3 positionDeployed;
    [SerializeField]
    private bool disableWhenStowed;
    [SerializeField]
    public bool staysDeployed;
    [SerializeField]
    private Radar radar;
    [SerializeField]
    private Turret turret;
    [SerializeField]
    private Collider collider;
    private float deployedAmount;
    private bool deploying;

    public bool TryAnimate()
    {
      if ((UnityEngine.Object) this.transform == (UnityEngine.Object) null || this.staysDeployed && (double) this.deployedAmount == 1.0)
        return false;
      this.deployedAmount = Mathf.Clamp01(this.deployedAmount + (this.deploying ? this.deployRate : -this.deployRate) * Time.deltaTime);
      this.transform.localEulerAngles = Vector3.Lerp(this.angleStowed, this.angleDeployed, this.deployedAmount);
      this.transform.localPosition = Vector3.Lerp(this.positionStowed, this.positionDeployed, this.deployedAmount);
      if ((UnityEngine.Object) this.collider != (UnityEngine.Object) null)
        this.collider.enabled = (double) this.deployedAmount == 1.0;
      return (double) this.deployedAmount != 0.0 && (double) this.deployedAmount != 1.0;
    }

    public void StartSequence(bool deploying)
    {
      if (this.deploying && this.staysDeployed)
        return;
      this.deploying = deploying;
      if ((UnityEngine.Object) this.radar != (UnityEngine.Object) null)
      {
        this.radar.enabled = false;
        this.radar.ResetRotators();
      }
      if (!this.disableWhenStowed)
        return;
      this.transform.gameObject.SetActive(deploying);
    }

    public void CompleteSequence(bool deployed)
    {
      if (deployed && (UnityEngine.Object) this.radar != (UnityEngine.Object) null)
        this.radar.enabled = true;
      if (!((UnityEngine.Object) this.turret != (UnityEngine.Object) null))
        return;
      this.turret.SetStowed(!deployed);
    }
  }
}

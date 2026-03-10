// Decompiled with JetBrains decompiler
// Type: Aircraft
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using Mirage;
using Mirage.RemoteCalls;
using Mirage.Serialization;
using NuclearOption.DebugScripts;
using NuclearOption.Networking;
using NuclearOption.SavedMission;
using System;
using System.Collections.Generic;
using System.Threading;
using Unity.Profiling;
using UnityEngine;

#nullable disable
public class Aircraft : Unit, IRadarReturn, IRearmable, IRefuelable
{
  private static readonly ProfilerMarker localSimFixedUpdateMarker = new ProfilerMarker("LocalSimFixedUpdate");
  [SyncVar]
  [NonSerialized]
  public PlayerRef playerRef;
  [SyncVar(hook = "onLoadoutChanged", hookType = SyncHookType.EventWith0Arg)]
  [NonSerialized]
  public Loadout loadout;
  [SyncVar]
  [NonSerialized]
  public NetworkBehaviorSyncvar spawningHangar;
  [SyncVar]
  [NonSerialized]
  public Vector3 startingVelocity;
  [SyncVar]
  [NonSerialized]
  private LiveryKey LiveryKey;
  [SyncVar]
  [NonSerialized]
  public float sortieScore;
  [SyncVar]
  [NonSerialized]
  public bool countermeasureTrigger;
  [SyncVar]
  [NonSerialized]
  public float fuelLevel;
  [SyncVar]
  [NonSerialized]
  public bool Ignition;
  [SyncVar(hook = "GearStateChangedHook")]
  [NonSerialized]
  public bool gearDeployed;
  public TargetDetector EOTS;
  public Pilot[] pilots;
  public TargetCam targetCam;
  public WeaponManager weaponManager;
  public CountermeasureManager countermeasureManager;
  public AudioClip scrapeSound;
  public UnitPart cockpit;
  [SerializeField]
  private ControlsFilter controlsFilter;
  [SerializeField]
  private RelaxedStabilityController relaxedStabilityController;
  public bool flightAssist = true;
  private TargetDetector defaultRadar;
  [SerializeField]
  private PowerSupply powerSupply;
  [SerializeField]
  private Canopy[] canopies;
  [SerializeField]
  private List<AudioSource> dopplerSounds = new List<AudioSource>();
  [SerializeField]
  private Renderer[] cockpitRenderers;
  [SerializeField]
  private Renderer[] exteriorRenderers;
  [SerializeField]
  private ParticleSystem sparksEmitter;
  [SerializeField]
  private GameObject[] groundEquipment;
  [HideInInspector]
  public Autopilot autopilot;
  [HideInInspector]
  public LandingGear.GearState gearState;
  [HideInInspector]
  public Vector3 accel = Vector3.zero;
  [HideInInspector]
  public Vector3 velocityPrev = Vector3.zero;
  private Vector3 windVelocity;
  [HideInInspector]
  public float gForce;
  [HideInInspector]
  public bool simplePhysics;
  [HideInInspector]
  public float skill = 1f;
  [HideInInspector]
  public float bravery = 0.5f;
  private StatusDisplay statusDisplay;
  private readonly ControlInputs controlInputs = new ControlInputs();
  private List<Unit> knownRadarSources = new List<Unit>();
  private List<TargetDetector> targetDetectors = new List<TargetDetector>();
  public List<IEngine> engineStates = new List<IEngine>();
  public List<IEngine> engines = new List<IEngine>();
  private List<FuelTank> fuelTanks = new List<FuelTank>();
  private MissileWarning missileWarning;
  private LaserDesignator laserDesignator;
  private bool needsFuel;
  private bool ejected;
  private bool airborne;
  [SerializeField]
  private float jammingIntensity;
  [SerializeField]
  private float fuelCapacity;
  private List<AeroPart> partsWithAero = new List<AeroPart>();
  public PartDamageTracker partDamageTracker;
  private ParticleSystem.EmitParams emitParams;
  private AudioSource scrapeSource;
  private Vector3 smoothingVel;
  private Vector3 rotationSmoothingVel;
  private bool spawnedInPosition;
  private Aircraft.PartChecker partChecker;
  private LiveryBehaviour liveryBehaviour;
  private GameObject CoMDebug;
  private DebugControlInputsDisplay debugControlInputsDisplay;
  private readonly List<ControlSurface> controlSurfaces = new List<ControlSurface>();
  private Unit slungUnit;
  private NavLights navLights;
  private int inputIntervalCounter;
  [NonSerialized]
  private const int SYNC_VAR_COUNT = 19;
  [NonSerialized]
  private const int RPC_COUNT = 43;

  public Player Player => this.playerRef.Player;

  public AircraftDefinition definition => (AircraftDefinition) this.definition;

  public event Action onLoadoutChanged;

  public event Action onSpawnedInPosition;

  public event Action onEject;

  public event Action OnTouchdown;

  public event Action<float> onSortieSuccessful;

  public event Action<Aircraft.OnSetGear> onSetGear;

  public event Action<Aircraft.OnFlightAssistToggle> onSetFlightAssist;

  public event Action<Aircraft.OnShake> onShake;

  public event Action<Aircraft.OnRadarWarning> onRadarWarning;

  public event Action<RearmEventArgs> OnRearm;

  public bool KnownRadarWarning(Unit radarSource)
  {
    if (this.knownRadarSources.Contains(radarSource))
      return true;
    this.knownRadarSources.Add(radarSource);
    return false;
  }

  public MissileWarning GetMissileWarningSystem() => this.missileWarning;

  public void SetLaserDesignator(LaserDesignator laserDesignator)
  {
    this.laserDesignator = laserDesignator;
  }

  public LaserDesignator GetLaserDesignator() => this.laserDesignator;

  public void LockedByMissile(Missile missile)
  {
    if (this.disabled)
      return;
    this.missileWarning.LockedByMissile(this, missile);
  }

  [ClientRpc]
  public void RpcGetRadarWarning(Unit radarSource)
  {
    if (ClientRpcSender.ShouldInvokeLocally((NetworkBehaviour) this, RpcTarget.Observers, (INetworkPlayer) null, false))
      this.UserCode_RpcGetRadarWarning_\u002D1586111906(radarSource);
    PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
    GeneratedNetworkCode._Write_Unit((NetworkWriter) writer, radarSource);
    ClientRpcSender.Send((NetworkBehaviour) this, 19, (NetworkWriter) writer, Mirage.Channel.Reliable, false);
    writer.Release();
  }

  public float GetRadarReturn(
    Vector3 source,
    Radar radar,
    Unit emitter,
    float dist,
    float clutter,
    RadarParams radarParams,
    bool triggerWarning)
  {
    Vector3 direction = FastMath.NormalizedDirection(source, this.transform.position);
    if (this.IsServer & triggerWarning)
      this.RpcGetRadarWarning(emitter);
    return radarParams.GetSignalStrength(direction, dist, this.rb, this.RCS, clutter, this.jammingIntensity);
  }

  public bool EstimateDetection(Radar radar, out float returnSignal)
  {
    returnSignal = 0.0f;
    if ((UnityEngine.Object) radar == (UnityEngine.Object) null)
      return false;
    float num1 = FastMath.Distance(radar.transform.position, this.transform.position);
    double maxRange = (double) radar.RadarParameters.maxRange;
    float maxSignal = radar.RadarParameters.maxSignal;
    float minSignal = radar.RadarParameters.minSignal;
    float num2 = (float) ((double) this.maxRadius * (double) this.maxRadius * 2.0 / ((double) this.radarAlt * (double) this.radarAlt));
    double num3 = (double) num1;
    float num4 = (float) (maxRange / num3);
    returnSignal = num4 * Mathf.Pow(this.RCS, 0.25f);
    returnSignal = Mathf.Min(returnSignal, maxSignal);
    returnSignal -= num2 * 0.15f;
    returnSignal = Mathf.Max(returnSignal, 0.0f);
    return (double) returnSignal > (double) minSignal;
  }

  [ServerRpc]
  public void CmdToggleRadar()
  {
    if (ServerRpcSender.ShouldInvokeLocally((NetworkBehaviour) this, true, false))
    {
      this.UserCode_CmdToggleRadar_1821461427();
    }
    else
    {
      PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
      ServerRpcSender.Send((NetworkBehaviour) this, 20, (NetworkWriter) writer, Mirage.Channel.Reliable, true);
      writer.Release();
    }
  }

  [ClientRpc]
  private void RpcToggleRadar(bool activated)
  {
    if (ClientRpcSender.ShouldInvokeLocally((NetworkBehaviour) this, RpcTarget.Observers, (INetworkPlayer) null, false))
      this.UserCode_RpcToggleRadar_1325449311(activated);
    PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
    writer.WriteBooleanExtension(activated);
    ClientRpcSender.Send((NetworkBehaviour) this, 21, (NetworkWriter) writer, Mirage.Channel.Reliable, false);
    writer.Release();
  }

  [ServerRpc]
  public void CmdToggleIgnition()
  {
    if (ServerRpcSender.ShouldInvokeLocally((NetworkBehaviour) this, true, false))
    {
      this.UserCode_CmdToggleIgnition_\u002D1067807998();
    }
    else
    {
      PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
      ServerRpcSender.Send((NetworkBehaviour) this, 22, (NetworkWriter) writer, Mirage.Channel.Reliable, true);
      writer.Release();
    }
  }

  public float GetJammingIntensity() => this.jammingIntensity;

  public void CheckNeedsFuel(float fuelRatio)
  {
    if (!this.networked || !this.LocalSim || this.needsFuel || (double) fuelRatio >= (double) this.fuelLevel * 0.89999997615814209)
      return;
    this.needsFuel = true;
    if (this.IsServer)
      return;
    this.CmdSetCanRefuel();
  }

  [ServerRpc]
  private void CmdSetCanRefuel()
  {
    if (ServerRpcSender.ShouldInvokeLocally((NetworkBehaviour) this, true, false))
    {
      this.UserCode_CmdSetCanRefuel_\u002D170919880();
    }
    else
    {
      PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
      ServerRpcSender.Send((NetworkBehaviour) this, 23, (NetworkWriter) writer, Mirage.Channel.Reliable, true);
      writer.Release();
    }
  }

  public void SetSlingLoadAttachment(Unit slingLoadUnit, SlingloadHook.DeployState state)
  {
    if (this.LocalSim)
      this.ApplySlingLoadAttachment(slingLoadUnit, state);
    if (this.IsServer)
    {
      this.RpcSlingLoadAttachment(slingLoadUnit, state);
    }
    else
    {
      if (!this.HasAuthority)
        return;
      this.CmdSlingLoadAttachment(slingLoadUnit, state);
    }
  }

  [ServerRpc]
  public void CmdSlingLoadAttachment(Unit slingLoadUnit, SlingloadHook.DeployState state)
  {
    if (ServerRpcSender.ShouldInvokeLocally((NetworkBehaviour) this, true, false))
    {
      this.UserCode_CmdSlingLoadAttachment_\u002D82313074(slingLoadUnit, state);
    }
    else
    {
      PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
      GeneratedNetworkCode._Write_Unit((NetworkWriter) writer, slingLoadUnit);
      GeneratedNetworkCode._Write_SlingloadHook\u002FDeployState((NetworkWriter) writer, state);
      ServerRpcSender.Send((NetworkBehaviour) this, 24, (NetworkWriter) writer, Mirage.Channel.Reliable, true);
      writer.Release();
    }
  }

  [ClientRpc(excludeOwner = true)]
  public void RpcSlingLoadAttachment(Unit slingLoadUnit, SlingloadHook.DeployState state)
  {
    if (ClientRpcSender.ShouldInvokeLocally((NetworkBehaviour) this, RpcTarget.Observers, (INetworkPlayer) null, true))
      this.UserCode_RpcSlingLoadAttachment_\u002D57940477(slingLoadUnit, state);
    PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
    GeneratedNetworkCode._Write_Unit((NetworkWriter) writer, slingLoadUnit);
    GeneratedNetworkCode._Write_SlingloadHook\u002FDeployState((NetworkWriter) writer, state);
    ClientRpcSender.Send((NetworkBehaviour) this, 25, (NetworkWriter) writer, Mirage.Channel.Reliable, true);
    writer.Release();
  }

  private void ApplySlingLoadAttachment(Unit slingLoadUnit, SlingloadHook.DeployState state)
  {
    bool attached = state == SlingloadHook.DeployState.Connected || state == SlingloadHook.DeployState.RescuePilot;
    if ((UnityEngine.Object) slingLoadUnit != (UnityEngine.Object) null)
      slingLoadUnit.AttachOrDetachSlingHook(this, attached);
    this.slungUnit = attached ? slingLoadUnit : (Unit) null;
    foreach (WeaponStation weaponStation in this.weaponStations)
    {
      if (weaponStation.WeaponInfo.sling && weaponStation.Weapons[0] is SlingloadHook weapon)
        weapon.ApplyState(slingLoadUnit, state);
    }
  }

  [ServerRpc]
  public void CmdSendSlungTransform(Vector3 position, Quaternion rotation)
  {
    if (ServerRpcSender.ShouldInvokeLocally((NetworkBehaviour) this, true, false))
    {
      this.UserCode_CmdSendSlungTransform_\u002D695501160(position, rotation);
    }
    else
    {
      PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
      writer.WriteVector3(position);
      writer.WriteQuaternion(rotation);
      ServerRpcSender.Send((NetworkBehaviour) this, 26, (NetworkWriter) writer, Mirage.Channel.Reliable, true);
      writer.Release();
    }
  }

  public bool CanRefuel()
  {
    return this.needsFuel && (double) this.radarAlt <= 5.0 && (double) this.speed <= 1.0;
  }

  public void RequestRearm()
  {
    this.weaponManager.SetCanRearm();
    this.NetworkHQ.NotifyNeedsRearm((Unit) this);
  }

  public bool CanRearm(bool aircraftRearm, bool vehicleRearm, bool shipRearm)
  {
    if (!aircraftRearm || (double) this.radarAlt > 5.0 || (double) this.speed > 1.0)
      return false;
    foreach (WeaponStation weaponStation in this.weaponStations)
    {
      if ((double) Time.timeSinceLevelLoad - (double) weaponStation.LastFiredTime < 10.0)
        return false;
    }
    return true;
  }

  public void Rearm(RearmEventArgs args)
  {
    if ((UnityEngine.Object) this.Player == (UnityEngine.Object) null)
      return;
    float score = this.sortieScore * MissionManager.CurrentMission.missionSettings.successfulSortieBonus;
    if ((double) score > 0.0 && (UnityEngine.Object) this.Player != (UnityEngine.Object) null)
      this.SuccessfulSortie(score);
    this.NetworkHQ.AddScore(score);
    if (!WeaponChecker.CanAffordRearm(this.Player, this.weaponManager, false))
      return;
    this.RpcRearm(args);
  }

  [ClientRpc]
  private void RpcRearm(RearmEventArgs args)
  {
    if (ClientRpcSender.ShouldInvokeLocally((NetworkBehaviour) this, RpcTarget.Observers, (INetworkPlayer) null, false))
      this.UserCode_RpcRearm_1198124017(args);
    PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
    GeneratedNetworkCode._Write_RearmEventArgs((NetworkWriter) writer, args);
    ClientRpcSender.Send((NetworkBehaviour) this, 27, (NetworkWriter) writer, Mirage.Channel.Reliable, false);
    writer.Release();
  }

  public void RegisterFuelTank(FuelTank tank) => this.fuelTanks.Add(tank);

  public List<FuelTank> GetFuelTanks() => this.fuelTanks;

  public void RecalcFuelCapacity()
  {
    this.fuelCapacity = 0.0f;
    foreach (FuelTank fuelTank in this.fuelTanks)
      this.fuelCapacity += fuelTank.GetCapacity();
  }

  public bool UseFuel(float fuelDrawn)
  {
    float num = 0.0f;
    foreach (FuelTank fuelTank in this.fuelTanks)
      num += fuelTank.fuelMass;
    float fuelRatio = num / this.fuelCapacity;
    if ((double) num <= 0.0)
    {
      this.CheckNeedsFuel(0.0f);
      return false;
    }
    foreach (FuelTank fuelTank in this.fuelTanks)
      fuelTank.UseFuel(fuelDrawn * (fuelTank.fuelMass / num));
    this.CheckNeedsFuel(fuelRatio);
    return true;
  }

  public bool GetMaxThrust(out float maxThrust)
  {
    maxThrust = 0.0f;
    DuctedThrustSystem component;
    if (this.gameObject.TryGetComponent<DuctedThrustSystem>(out component))
    {
      maxThrust = component.GetMaxThrust();
      return true;
    }
    foreach (IEngine engineState in this.engineStates)
    {
      if (engineState is IThrustSource thrustSource)
        maxThrust += thrustSource.GetMaxThrust();
    }
    return (double) maxThrust > 0.0;
  }

  public bool GetMaxPower(out float maxPower)
  {
    maxPower = 0.0f;
    foreach (IEngine engineState in this.engineStates)
    {
      if (engineState is IPowerSource powerSource)
        maxPower += powerSource.GetMaxPower();
    }
    return (double) maxPower > 0.0;
  }

  public float GetFuelLevel()
  {
    float num1 = 0.0f;
    float num2 = 0.0f;
    foreach (FuelTank fuelTank in this.fuelTanks)
    {
      num1 += fuelTank.GetCapacity();
      num2 += fuelTank.GetLevel();
    }
    return (double) num1 == 0.0 ? 0.0f : num2 / num1;
  }

  public float GetFuelQuantity()
  {
    float fuelQuantity = 0.0f;
    foreach (FuelTank fuelTank in this.fuelTanks)
      fuelQuantity += fuelTank.GetLevel();
    return fuelQuantity;
  }

  public override Vector3 GetWindVelocity() => this.windVelocity;

  public override float GetAirDensity() => this.airDensity;

  public void Refuel(Unit refueler)
  {
    if (this.networked)
      this.RpcRefuel(refueler);
    else
      this.SetFuelLevel(refueler);
  }

  [ClientRpc]
  private void RpcRefuel(Unit refueler)
  {
    if (ClientRpcSender.ShouldInvokeLocally((NetworkBehaviour) this, RpcTarget.Observers, (INetworkPlayer) null, false))
      this.UserCode_RpcRefuel_1587114129(refueler);
    PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
    GeneratedNetworkCode._Write_Unit((NetworkWriter) writer, refueler);
    ClientRpcSender.Send((NetworkBehaviour) this, 28, (NetworkWriter) writer, Mirage.Channel.Reliable, false);
    writer.Release();
  }

  private void SetFuelLevel(Unit refueler)
  {
    bool flag = false;
    foreach (FuelTank fuelTank in this.fuelTanks)
    {
      if (!fuelTank.Refuel(this.fuelLevel))
        flag = true;
    }
    if (PlayerSettings.debugVis)
      this.DebugCoM();
    if (GameManager.IsLocalAircraft(this) && (UnityEngine.Object) refueler != (UnityEngine.Object) null)
    {
      if (flag)
      {
        SceneSingleton<AircraftActionsReport>.i.ReportText("<b>Fuel leak detected; unable to refuel</b>", 5f);
        return;
      }
      SceneSingleton<AircraftActionsReport>.i.ReportText("Refueled by " + refueler.unitName, 5f);
    }
    this.needsFuel = false;
  }

  public void AddControlSurface(ControlSurface controlSurface)
  {
    this.controlSurfaces.Add(controlSurface);
  }

  public override void Awake()
  {
    base.Awake();
    this.Identity.OnStartClient.AddListener(new Action(this.OnStartClient));
    this.Identity.OnStartServer.AddListener(new Action(this.OnStartServer));
    this.Identity.OnAuthorityChanged.AddListener(new Action<bool>(this.OnAuthorityChanged));
  }

  private void OnAuthorityChanged(bool hasAuthority)
  {
    if (this.IsServer || !this.LocalSim || hasAuthority)
      return;
    ColorLog<Aircraft>.Info($"Authority removed from {this} disabling local sim");
    this.SetLocalSim(false);
  }

  public void DebugCoM()
  {
    if ((UnityEngine.Object) this.CoMDebug != (UnityEngine.Object) null)
      UnityEngine.Object.Destroy((UnityEngine.Object) this.CoMDebug);
    this.CoMDebug = UnityEngine.Object.Instantiate<GameObject>(GameAssets.i.debugArrow, this.GetCenterOfMass(), Quaternion.LookRotation(Vector3.up));
    this.CoMDebug.transform.localScale = new Vector3(1f, 1f, 5f);
    this.CoMDebug.transform.SetParent(this.transform);
  }

  private void OnStartServer()
  {
    this.RegisterUnit(new float?(1f));
    this.rb.velocity = this.startingVelocity;
    if ((UnityEngine.Object) this.Player != (UnityEngine.Object) null)
    {
      this.Player.SetAircraft(this);
      this.Player.SetFaction(this.NetworkHQ);
    }
    if (!this.networked)
      return;
    this.NetworkIgnition = true;
  }

  private void OnStartClient()
  {
    this.defaultRadar = this.radar;
    this.gearState = LandingGear.GearState.Uninitialized;
    this.partDamageTracker = new PartDamageTracker(this);
    if (!this.IsServer)
    {
      if ((UnityEngine.Object) this.NetworkspawningHangar == (UnityEngine.Object) null)
      {
        this.transform.SetPositionAndRotation(this.startPosition.ToLocalPosition(), this.startRotation);
        this.rb.MovePosition(this.startPosition.ToLocalPosition());
        this.rb.MoveRotation(this.startRotation);
        this.rb.velocity = this.startingVelocity;
      }
      else
      {
        Transform spawnTransform = this.NetworkspawningHangar.GetSpawnTransform();
        this.transform.SetPositionAndRotation(spawnTransform.position + spawnTransform.up * this.definition.spawnOffset.y + spawnTransform.forward * this.definition.spawnOffset.z, spawnTransform.rotation);
        this.rb.MovePosition(this.transform.position);
        this.rb.MoveRotation(this.transform.rotation);
        this.rb.velocity = this.NetworkspawningHangar.GetVelocity();
        this.rb.angularVelocity = this.NetworkspawningHangar.GetAngularVelocity();
      }
      this.RegisterUnit(new float?(1f));
    }
    this.missileWarning = this.gameObject.AddComponent<MissileWarning>();
    for (byte index = 0; (int) index < this.pilots.Length; ++index)
      this.pilots[(int) index].AssignPilotNumber(index);
    this.SetLocalSim(this.CheckIfLocalSim());
    if ((UnityEngine.Object) this.Player != (UnityEngine.Object) null)
    {
      if (!this.IsServer)
        this.Player.SetAircraft(this);
      if (this.Player.IsLocalPlayer)
      {
        SceneSingleton<DynamicMap>.i.SetFaction(this.NetworkHQ);
        SceneSingleton<CombatHUD>.i.SetAircraft(this);
        SceneSingleton<DynamicMap>.i.DeselectAllIcons();
      }
    }
    else
    {
      int num = this.remoteSim ? 1 : 0;
    }
    if (this.loadout == null || this.loadout.weapons.Count == 0)
      this.Networkloadout = this.definition.aircraftParameters.loadouts[1];
    this.SetLiveryKey(this.LiveryKey);
    this.countermeasureManager.Initialize();
    this.TogglePitchLimiter();
    this.StartSlowUpdateDelayed(0.1f, new Action(((Unit) this).CheckRadarAlt));
    if (this.LocalSim)
    {
      this.SpawnedInPosition();
      if ((double) this.radarAlt > (double) this.definition.spawnOffset.y + 1.0)
      {
        this.controlInputs.throttle = 0.6f;
        this.SetGear(false);
        this.GearStateChanged(false);
      }
      else
        this.SetGear(true);
      this.partChecker = new Aircraft.PartChecker(this);
      if (GameManager.gameState == GameState.Editor || GameManager.gameState == GameState.Encyclopedia)
        return;
      if ((UnityEngine.Object) this.Player != (UnityEngine.Object) null)
        this.SetupLocalPlayerAndUI();
    }
    else
      this.GearStateChanged(this.gearDeployed);
    this.SetFuelLevel((Unit) null);
    this.InitializeUnit();
  }

  private void SetupLocalPlayerAndUI()
  {
    int num = this.IsServer ? 1 : 0;
    this.Player.AttachToAircraft(this);
    this.pilots[0].SwitchState((PilotBaseState) this.pilots[0].playerState);
    AircraftParameters aircraftParameters = this.GetAircraftParameters();
    GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(aircraftParameters.StatusDisplay, Vector3.zero, Quaternion.identity);
    if ((UnityEngine.Object) aircraftParameters.HUDExtras != (UnityEngine.Object) null)
      UnityEngine.Object.Instantiate<GameObject>(aircraftParameters.HUDExtras, SceneSingleton<FlightHud>.i.GetHUDCenter());
    this.statusDisplay = gameObject.GetComponent<StatusDisplay>();
    this.statusDisplay.Initialize(this);
    SceneSingleton<CameraStateManager>.i.SetFollowingUnit((Unit) this);
    SceneSingleton<CameraStateManager>.i.SwitchState((CameraBaseState) SceneSingleton<CameraStateManager>.i.cockpitState);
    SceneSingleton<DynamicMap>.i.Maximize();
    SceneSingleton<DynamicMap>.i.Minimize();
    DynamicMap.EnableCanvas(true);
  }

  public override void SetLocalSim(bool localSim)
  {
    base.SetLocalSim(localSim);
    if (localSim)
      this.SetComplexPhysics();
    else
      this.SetSimplePhysics();
    this.rb.interpolation = localSim ? RigidbodyInterpolation.Interpolate : RigidbodyInterpolation.None;
    this.rb.useGravity = localSim;
  }

  public void RegisterAeroPart(AeroPart aeroPart) => this.partsWithAero.Add(aeroPart);

  public void DeregisterAeroPart(AeroPart aeroPart)
  {
    this.partsWithAero.Remove(aeroPart);
    Debug.Log((object) ("Deregistering aeroPart " + aeroPart.gameObject.name));
  }

  public void RegisterDopplerSound(AudioSource audioSource) => this.dopplerSounds.Add(audioSource);

  public void DeregisterDopplerSound(AudioSource audioSource)
  {
    this.dopplerSounds.Remove(audioSource);
  }

  private bool CheckIfLocalSim()
  {
    if ((UnityEngine.Object) this.Player != (UnityEngine.Object) null)
      return this.Player.IsLocalPlayer;
    return GameManager.gameState != GameState.Editor && NetworkManagerNuclearOption.i.Server.Active;
  }

  public void SetComplexPhysics()
  {
    ColorLog<Unit>.Info($"Setting {this.unitName} physics to Complex");
    foreach (UnitPart unitPart in this.partLookup)
      (unitPart as AeroPart).CreateRB(this.rb.GetPointVelocity(unitPart.transform.position), Vector3.zero);
    foreach (UnitPart unitPart in this.partLookup)
      (unitPart as AeroPart).CreateJoints();
    this.simplePhysics = false;
    this.rb.ResetCenterOfMass();
  }

  public void SetSimplePhysics()
  {
    ColorLog<Unit>.Info($"Setting {this.unitName} physics to Simplified");
    foreach (UnitPart unitPart in this.partLookup)
      (unitPart as AeroPart).MergeWithParent();
    this.rb.mass = this.definition.mass;
    this.rb.ResetCenterOfMass();
    this.rb.ResetInertiaTensor();
    this.simplePhysics = true;
  }

  public Vector3 CalcCenterOfMass()
  {
    Vector3 zero = Vector3.zero;
    float num = 0.0f;
    foreach (UnitPart unitPart in this.partLookup)
    {
      if (!unitPart.IsDetached())
      {
        num += unitPart.mass;
        zero += unitPart.transform.position * unitPart.mass;
      }
    }
    return zero / num;
  }

  public void SetLiveryKey(LiveryKey liveryKey, bool loadIfUnspawned = false)
  {
    this.NetworkLiveryKey = liveryKey;
    if (!loadIfUnspawned && !this.IsClient)
      return;
    if (this.liveryBehaviour == null && !this.TryGetComponent<LiveryBehaviour>(out this.liveryBehaviour))
    {
      this.liveryBehaviour = this.gameObject.AddComponent<LiveryBehaviour>();
      this.liveryBehaviour.Setup(this, new LiveryKey?(new LiveryKey(this.definition.aircraftParameters.GetFirstLiveryForFaction((UnityEngine.Object) this.NetworkHQ != (UnityEngine.Object) null ? this.NetworkHQ.faction : (Faction) null))));
    }
    this.liveryBehaviour.SetKey(liveryKey);
  }

  public LiveryBehaviour GetLiveryBehaviour() => this.liveryBehaviour;

  public void CheckSpawnedInPosition()
  {
    if (this.spawnedInPosition)
      return;
    this.SpawnedInPosition();
  }

  public void SpawnedInPosition()
  {
    this.spawnedInPosition = true;
    this.speed = this.rb.velocity.magnitude;
    this.radarAlt = (this.transform.position - Datum.origin.position).y - this.definition.spawnOffset.y;
    if (Physics.Linecast(this.transform.position, this.transform.position - Vector3.up * 10000f, out this.hit, 2112))
      this.radarAlt = this.hit.distance - this.definition.spawnOffset.y;
    Action spawnedInPosition = this.onSpawnedInPosition;
    if (spawnedInPosition != null)
      spawnedInPosition();
    this.RecalcFuelCapacity();
  }

  public void SetPreview() => this.OnStartClient();

  public void SetDoppler(bool enabled)
  {
    foreach (AudioSource dopplerSound in this.dopplerSounds)
    {
      if (!((UnityEngine.Object) dopplerSound == (UnityEngine.Object) null))
      {
        dopplerSound.dopplerLevel = enabled ? 1f : 0.0f;
        dopplerSound.spatialBlend = enabled ? 1f : 0.0f;
      }
    }
  }

  public void SetCockpitRenderers(bool enabled)
  {
    for (int index = 0; index < this.cockpitRenderers.Length; ++index)
      this.cockpitRenderers[index].enabled = enabled;
    for (int index = 0; index < this.exteriorRenderers.Length; ++index)
      this.exteriorRenderers[index].enabled = !enabled;
    foreach (IEngine engine in this.engines)
      engine.SetInteriorSounds(enabled);
  }

  public void ThrowSparks(Vector3 position, Vector3 extraVelocity)
  {
    if ((UnityEngine.Object) this.scrapeSource == (UnityEngine.Object) null)
    {
      GameObject gameObject = new GameObject("scrapes");
      gameObject.transform.SetParent(this.transform);
      this.scrapeSource = gameObject.AddComponent<AudioSource>();
      this.scrapeSource.outputAudioMixerGroup = SoundManager.i.EffectsMixer;
      this.scrapeSource.spatialBlend = 1f;
      this.scrapeSource.dopplerLevel = 0.0f;
      this.scrapeSource.minDistance = 20f;
      this.scrapeSource.maxDistance = 100f;
      this.scrapeSource.rolloffMode = AudioRolloffMode.Linear;
      this.scrapeSource.clip = this.scrapeSound;
      this.scrapeSource.loop = true;
    }
    if (!this.scrapeSource.isPlaying)
    {
      this.scrapeSource.time = UnityEngine.Random.value * this.scrapeSound.length;
      this.scrapeSource.Play();
    }
    if ((UnityEngine.Object) this.rb == (UnityEngine.Object) null)
      return;
    this.scrapeSource.transform.position = Vector3.Lerp(this.scrapeSource.transform.position, position, 5f * Time.deltaTime);
    this.scrapeSource.volume += 20f * Time.deltaTime;
    this.scrapeSource.volume = Mathf.Clamp01(this.scrapeSource.volume);
    this.emitParams.position = position.ToGlobalPosition().AsVector3();
    this.emitParams.velocity = this.rb.velocity + extraVelocity + Vector3.up * (float) UnityEngine.Random.Range(0, 5) + Vector3.right * (float) UnityEngine.Random.Range(-3, 3) + Vector3.forward * (float) UnityEngine.Random.Range(-3, 3);
    this.sparksEmitter.Emit(this.emitParams, 1);
  }

  [ClientRpc]
  public override void RpcDamage(byte index, DamageInfo damageInfo)
  {
    if (ClientRpcSender.ShouldInvokeLocally((NetworkBehaviour) this, RpcTarget.Observers, (INetworkPlayer) null, false))
      this.UserCode_RpcDamage_870168505(index, damageInfo);
    PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
    writer.WriteByteExtension(index);
    GeneratedNetworkCode._Write_DamageInfo((NetworkWriter) writer, damageInfo);
    ClientRpcSender.Send((NetworkBehaviour) this, 29, (NetworkWriter) writer, Mirage.Channel.Reliable, false);
    writer.Release();
  }

  public void ShakeAircraft(float lowFreqShake, float highFreqShake)
  {
    Action<Aircraft.OnShake> onShake = this.onShake;
    if (onShake == null)
      return;
    onShake(new Aircraft.OnShake()
    {
      lowFreqShake = lowFreqShake,
      highFreqShake = highFreqShake
    });
  }

  [ServerRpc]
  public void CmdLaunchMissile(byte stationIndex, Unit target, GlobalPosition aimpoint)
  {
    if (ServerRpcSender.ShouldInvokeLocally((NetworkBehaviour) this, true, false))
    {
      this.UserCode_CmdLaunchMissile_644415535(stationIndex, target, aimpoint);
    }
    else
    {
      PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
      writer.WriteByteExtension(stationIndex);
      GeneratedNetworkCode._Write_Unit((NetworkWriter) writer, target);
      writer.WriteGlobalPosition(aimpoint);
      ServerRpcSender.Send((NetworkBehaviour) this, 30, (NetworkWriter) writer, Mirage.Channel.Reliable, true);
      writer.Release();
    }
  }

  [ClientRpc(excludeOwner = true)]
  public void RpcLaunchMissile(byte stationIndex, Unit target, GlobalPosition aimpoint)
  {
    if (ClientRpcSender.ShouldInvokeLocally((NetworkBehaviour) this, RpcTarget.Observers, (INetworkPlayer) null, true))
      this.UserCode_RpcLaunchMissile_1465828762(stationIndex, target, aimpoint);
    PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
    writer.WriteByteExtension(stationIndex);
    GeneratedNetworkCode._Write_Unit((NetworkWriter) writer, target);
    writer.WriteGlobalPosition(aimpoint);
    ClientRpcSender.Send((NetworkBehaviour) this, 31 /*0x1F*/, (NetworkWriter) writer, Mirage.Channel.Reliable, true);
    writer.Release();
  }

  public Rigidbody CockpitRB()
  {
    return (UnityEngine.Object) this.cockpit.rb != (UnityEngine.Object) null ? this.cockpit.rb : this.rb;
  }

  public AircraftParameters GetAircraftParameters() => this.definition.aircraftParameters;

  public bool IsLanded() => (double) this.radarAlt < 5.0 && (double) this.speed < 2.5;

  public void OpenCanopies()
  {
    foreach (Canopy canopy in this.canopies)
      canopy.OpenHinges();
  }

  public void ShowGroundEquipment()
  {
    foreach (GameObject gameObject in this.groundEquipment)
    {
      gameObject.SetActive(true);
      RaycastHit hitInfo;
      if ((UnityEngine.Object) gameObject.transform.parent == (UnityEngine.Object) this.transform && Physics.Linecast(gameObject.transform.position + gameObject.transform.up, gameObject.transform.position - gameObject.transform.up, out hitInfo, 2112))
      {
        gameObject.transform.SetPositionAndRotation(hitInfo.point, Quaternion.LookRotation(gameObject.transform.forward, hitInfo.normal));
        gameObject.transform.SetParent(hitInfo.collider.transform);
      }
    }
  }

  public ControlInputs GetInputs() => this.controlInputs;

  public ControlsFilter GetControlsFilter() => this.controlsFilter;

  public override PowerSupply GetPowerSupply() => this.powerSupply;

  public void FilterInputs()
  {
    Vector3 rawInputs = new Vector3(this.controlInputs.pitch, this.controlInputs.yaw, this.controlInputs.roll);
    if ((UnityEngine.Object) this.relaxedStabilityController != (UnityEngine.Object) null)
      this.relaxedStabilityController.FilterInput(this.controlInputs, this.CockpitRB(), this.gForce, rawInputs.x);
    if (!((UnityEngine.Object) this.controlsFilter != (UnityEngine.Object) null))
      return;
    this.controlsFilter.Filter(this.controlInputs, rawInputs, this.CockpitRB(), this.gForce, this.flightAssist);
  }

  public void SetFlightAssist(bool enabled)
  {
    this.controlsFilter.SetFlightAssist(enabled, this);
    if (!this.controlsFilter.HasFlightAssist())
    {
      this.flightAssist = true;
    }
    else
    {
      this.flightAssist = enabled;
      Action<Aircraft.OnFlightAssistToggle> onSetFlightAssist = this.onSetFlightAssist;
      if (onSetFlightAssist == null)
        return;
      onSetFlightAssist(new Aircraft.OnFlightAssistToggle()
      {
        enabled = enabled
      });
    }
  }

  public void SetFlightAssistToDefault()
  {
    bool enabled = this.controlsFilter.FlightAssistDefault();
    this.controlsFilter.SetFlightAssist(enabled, this);
    if (!this.controlsFilter.HasFlightAssist())
      return;
    Action<Aircraft.OnFlightAssistToggle> onSetFlightAssist = this.onSetFlightAssist;
    if (onSetFlightAssist == null)
      return;
    onSetFlightAssist(new Aircraft.OnFlightAssistToggle()
    {
      enabled = enabled
    });
  }

  public bool IsAutoHoverEnabled() => this.controlsFilter.IsAutoHoverEnabled();

  public void TogglePitchLimiter()
  {
    if (GameManager.gameState != GameState.SinglePlayer && GameManager.gameState != GameState.Multiplayer)
      return;
    this.controlsFilter.SetFlightAssist(!this.flightAssist, this);
    if (!this.controlsFilter.HasFlightAssist())
      return;
    this.flightAssist = !this.flightAssist;
    Action<Aircraft.OnFlightAssistToggle> onSetFlightAssist = this.onSetFlightAssist;
    if (onSetFlightAssist == null)
      return;
    onSetFlightAssist(new Aircraft.OnFlightAssistToggle()
    {
      enabled = this.flightAssist
    });
  }

  public void ToggleNavLights()
  {
    if (GameManager.gameState != GameState.SinglePlayer && GameManager.gameState != GameState.Multiplayer)
      return;
    if ((UnityEngine.Object) this.navLights == (UnityEngine.Object) null)
      this.navLights = this.transform.GetComponentInChildren<NavLights>();
    this.navLights.ToggleNavLights();
  }

  public void SetRadar(Radar radar)
  {
    if ((UnityEngine.Object) this.radar != (UnityEngine.Object) null)
      this.radar.activated = false;
    this.radar = (UnityEngine.Object) radar != (UnityEngine.Object) null ? (TargetDetector) radar : this.defaultRadar;
  }

  public void Countermeasures(bool active, byte index)
  {
    this.countermeasureManager.activeIndex = index;
    if (this.IsServer)
    {
      this.RpcCountermeasures(index);
      this.NetworkcountermeasureTrigger = active;
    }
    else
    {
      if (!this.HasAuthority)
        return;
      this.CmdCountermeasures(active, index);
    }
  }

  [ServerRpc]
  private void CmdCountermeasures(bool active, byte index)
  {
    if (ServerRpcSender.ShouldInvokeLocally((NetworkBehaviour) this, true, false))
    {
      this.UserCode_CmdCountermeasures_\u002D1391811506(active, index);
    }
    else
    {
      PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
      writer.WriteBooleanExtension(active);
      writer.WriteByteExtension(index);
      ServerRpcSender.Send((NetworkBehaviour) this, 32 /*0x20*/, (NetworkWriter) writer, Mirage.Channel.Reliable, true);
      writer.Release();
    }
  }

  [ClientRpc]
  private void RpcCountermeasures(byte index)
  {
    if (ClientRpcSender.ShouldInvokeLocally((NetworkBehaviour) this, RpcTarget.Observers, (INetworkPlayer) null, false))
      this.UserCode_RpcCountermeasures_1466036644(index);
    PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
    writer.WriteByteExtension(index);
    ClientRpcSender.Send((NetworkBehaviour) this, 33, (NetworkWriter) writer, Mirage.Channel.Reliable, false);
    writer.Release();
  }

  public void SetGear(bool deployed)
  {
    this.NetworkgearDeployed = deployed;
    this.GearStateChanged(deployed);
    if (!this.HasAuthority || this.IsServer)
      return;
    this.CmdSetGear(deployed);
  }

  [ServerRpc]
  private void CmdSetGear(bool deployed)
  {
    if (ServerRpcSender.ShouldInvokeLocally((NetworkBehaviour) this, true, false))
    {
      this.UserCode_CmdSetGear_\u002D1969689879(deployed);
    }
    else
    {
      PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
      writer.WriteBooleanExtension(deployed);
      ServerRpcSender.Send((NetworkBehaviour) this, 34, (NetworkWriter) writer, Mirage.Channel.Reliable, true);
      writer.Release();
    }
  }

  private void GearStateChangedHook(bool _, bool newValue) => this.GearStateChanged(newValue);

  private void GearStateChanged(bool gearDeployed)
  {
    if (this.gearState == LandingGear.GearState.Extending || this.gearState == LandingGear.GearState.Retracting)
      return;
    if (this.gearState == LandingGear.GearState.Uninitialized)
    {
      this.SetGear(gearDeployed ? LandingGear.GearState.LockedExtended : LandingGear.GearState.LockedRetracted);
    }
    else
    {
      if (this.gearState == LandingGear.GearState.LockedRetracted && gearDeployed)
        this.SetGear(LandingGear.GearState.Extending);
      if (this.gearState != LandingGear.GearState.LockedExtended || gearDeployed)
        return;
      this.SetGear(LandingGear.GearState.Retracting);
    }
  }

  public void SetGear(LandingGear.GearState gearState)
  {
    if (this.gearState == gearState)
      return;
    this.gearState = gearState;
    Action<Aircraft.OnSetGear> onSetGear = this.onSetGear;
    if (onSetGear == null)
      return;
    onSetGear(new Aircraft.OnSetGear()
    {
      gearState = this.gearState
    });
  }

  public void ApplySetInputs(CompressedInputs inputs)
  {
    this.controlInputs.pitch = inputs.pitch.Decompress();
    this.controlInputs.roll = inputs.roll.Decompress();
    this.controlInputs.yaw = inputs.yaw.Decompress();
    this.controlInputs.brake = inputs.brake.Decompress();
    this.controlInputs.throttle = inputs.throttle.Decompress();
    this.controlInputs.customAxis1 = inputs.customAxis1.Decompress();
  }

  public override void CheckRadarAlt()
  {
    if (Physics.Linecast(this.transform.position, this.transform.position - Vector3.up * 10000f, out this.hit, 2112))
      this.radarAlt = this.hit.distance;
    else
      this.radarAlt = this.transform.position.GlobalY();
    this.radarAlt -= this.definition.spawnOffset.y;
    this.radarAlt = Mathf.Clamp(this.radarAlt, 0.0f, this.transform.position.GlobalY() - this.definition.spawnOffset.y);
    bool airborne = this.airborne;
    this.airborne = (double) this.radarAlt > 0.20000000298023224;
    if (this.airborne == airborne)
      return;
    if (this.airborne && !this.disabled && GameManager.IsLocalAircraft(this))
    {
      MusicManager.i.CrossFadeMusic(this.gearDeployed ? this.GetAircraftParameters().takeoffMusic : (AudioClip) null, 2f, 0.0f, false, false, true);
    }
    else
    {
      Action onTouchdown = this.OnTouchdown;
      if (onTouchdown == null)
        return;
      onTouchdown();
    }
  }

  private void FixedUpdate()
  {
    if (this.countermeasureTrigger)
      this.countermeasureManager.DeployCountermeasure(this);
    Vector3 velocity = this.cockpit.rb.velocity;
    if ((UnityEngine.Object) this.hit.collider != (UnityEngine.Object) null && (UnityEngine.Object) this.hit.collider.attachedRigidbody != (UnityEngine.Object) null)
      velocity -= this.hit.collider.attachedRigidbody.GetPointVelocity(this.hit.point);
    this.speed = velocity.magnitude;
    this.airDensity = LevelInfo.GetAirDensity(this.cockpit.xform.position.GlobalY());
    if (this.LocalSim)
      this.LocalSimFixedUpdate();
    if ((UnityEngine.Object) this.scrapeSource != (UnityEngine.Object) null && this.scrapeSource.isPlaying)
    {
      this.scrapeSource.volume -= 2f * Time.deltaTime;
      if ((double) this.scrapeSource.volume <= 0.0)
        this.scrapeSource.Stop();
    }
    if (DebugVis.Enabled && this.pilots.Length != 0 && DebugVis.Create<DebugControlInputsDisplay>(ref this.debugControlInputsDisplay, GameAssets.i.debugControlInputsDisplay, this.transform))
      this.debugControlInputsDisplay.Setup(this.pilots[0]);
    float speedOfSound = LevelInfo.GetSpeedOfSound(this.transform.GlobalPosition().y);
    if ((double) this.speed <= 0.99000000953674316 * (double) speedOfSound || (double) this.speed >= (double) speedOfSound)
      return;
    this.ShakeAircraft(0.0f, 0.25f * this.airDensity);
  }

  private void LocalSimFixedUpdate()
  {
    using (Aircraft.localSimFixedUpdateMarker.Auto())
    {
      this.accel = this.velocityPrev == Vector3.zero ? Vector3.zero : this.CockpitRB().velocity - this.velocityPrev;
      this.partChecker.Check();
      this.velocityPrev = this.CockpitRB().velocity;
      this.accel /= Time.fixedDeltaTime * 9.81f;
      this.gForce = this.accel.magnitude;
      this.windVelocity = NetworkSceneSingleton<LevelInfo>.i.GetWind(this.transform.GlobalPosition());
      foreach (ControlSurface controlSurface in this.controlSurfaces)
        controlSurface.Aero();
      if (!((UnityEngine.Object) SceneSingleton<CombatHUD>.i != (UnityEngine.Object) null) || !((UnityEngine.Object) SceneSingleton<CombatHUD>.i.aircraft == (UnityEngine.Object) this))
        return;
      this.countermeasureManager.UpdateHUD();
    }
  }

  private void CheckPhysicsLod()
  {
    bool flag = FastMath.InRange(SceneSingleton<CameraStateManager>.i.transform.position, this.transform.position, 10000f);
    if (this.simplePhysics)
    {
      if (!((double) this.gForce < 2.0 & flag))
        return;
      this.SetComplexPhysics();
    }
    else
    {
      if ((double) this.gForce >= 2.0 || flag)
        return;
      this.SetSimplePhysics();
    }
  }

  public void AddJammingIntensity(float jammingIntensity)
  {
    this.jammingIntensity += jammingIntensity;
    if ((double) Mathf.Abs(this.jammingIntensity) >= 1.0 / 1000.0)
      return;
    this.jammingIntensity = 0.0f;
  }

  public void SetStationTargets(byte stationIndex, ReadOnlySpan<PersistentID> targetIDs)
  {
    if (this.IsServer)
    {
      this.RpcSetStationTargets(stationIndex, targetIDs);
    }
    else
    {
      if (!this.HasAuthority)
        return;
      this.CmdSetStationTargets(stationIndex, targetIDs);
    }
  }

  public void SetStationTurretTarget(byte stationIndex, byte turretIndex, PersistentID targetID)
  {
    if (this.IsServer)
    {
      this.RpcSetStationTurretTarget(stationIndex, turretIndex, targetID);
    }
    else
    {
      if (!this.HasAuthority)
        return;
      this.CmdSetStationTurretTarget(stationIndex, turretIndex, targetID);
    }
  }

  [ServerRpc]
  public void CmdSetStationTurretTarget(byte stationIndex, byte turretIndex, PersistentID targetID)
  {
    if (ServerRpcSender.ShouldInvokeLocally((NetworkBehaviour) this, true, false))
    {
      this.UserCode_CmdSetStationTurretTarget_\u002D461984136(stationIndex, turretIndex, targetID);
    }
    else
    {
      PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
      writer.WriteByteExtension(stationIndex);
      writer.WriteByteExtension(turretIndex);
      GeneratedNetworkCode._Write_PersistentID((NetworkWriter) writer, targetID);
      ServerRpcSender.Send((NetworkBehaviour) this, 35, (NetworkWriter) writer, Mirage.Channel.Reliable, true);
      writer.Release();
    }
  }

  [ClientRpc]
  public void RpcSetStationTurretTarget(byte stationIndex, byte turretIndex, PersistentID targetID)
  {
    if (ClientRpcSender.ShouldInvokeLocally((NetworkBehaviour) this, RpcTarget.Observers, (INetworkPlayer) null, false))
      this.UserCode_RpcSetStationTurretTarget_740629667(stationIndex, turretIndex, targetID);
    PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
    writer.WriteByteExtension(stationIndex);
    writer.WriteByteExtension(turretIndex);
    GeneratedNetworkCode._Write_PersistentID((NetworkWriter) writer, targetID);
    ClientRpcSender.Send((NetworkBehaviour) this, 36, (NetworkWriter) writer, Mirage.Channel.Reliable, false);
    writer.Release();
  }

  public void SetActiveStation(byte stationIndex)
  {
    this.weaponManager.SetActiveStation(stationIndex);
    if (this.IsServer)
    {
      this.RpcSetActiveStation(stationIndex);
    }
    else
    {
      if (!this.HasAuthority)
        return;
      this.CmdSetActiveStation(stationIndex);
    }
  }

  [ServerRpc]
  private void CmdSetActiveStation(byte stationIndex)
  {
    if (ServerRpcSender.ShouldInvokeLocally((NetworkBehaviour) this, true, false))
    {
      this.UserCode_CmdSetActiveStation_126720094(stationIndex);
    }
    else
    {
      PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
      writer.WriteByteExtension(stationIndex);
      ServerRpcSender.Send((NetworkBehaviour) this, 37, (NetworkWriter) writer, Mirage.Channel.Reliable, true);
      writer.Release();
    }
  }

  [ClientRpc]
  private void RpcSetActiveStation(byte stationIndex)
  {
    if (ClientRpcSender.ShouldInvokeLocally((NetworkBehaviour) this, RpcTarget.Observers, (INetworkPlayer) null, false))
      this.UserCode_RpcSetActiveStation_1081017235(stationIndex);
    PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
    writer.WriteByteExtension(stationIndex);
    ClientRpcSender.Send((NetworkBehaviour) this, 38, (NetworkWriter) writer, Mirage.Channel.Reliable, false);
    writer.Release();
  }

  public void SetTurretVector(byte weaponStationIndex, Vector3 direction)
  {
    if (this.IsServer)
    {
      this.RpcSetTurretVector(weaponStationIndex, NetworkFloatHelper.CompressIfValid(direction, false, (string) null, Vector3.forward));
    }
    else
    {
      if (!this.HasAuthority)
        return;
      this.CmdSetTurretVector(weaponStationIndex, NetworkFloatHelper.CompressIfValid(direction, true, nameof (direction), Vector3.forward));
    }
  }

  [ServerRpc]
  private void CmdSetTurretVector(byte weaponStationIndex, Vector3Compressed direction)
  {
    if (ServerRpcSender.ShouldInvokeLocally((NetworkBehaviour) this, true, false))
    {
      this.UserCode_CmdSetTurretVector_\u002D1133982608(weaponStationIndex, direction);
    }
    else
    {
      PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
      writer.WriteByteExtension(weaponStationIndex);
      GeneratedNetworkCode._Write_Vector3Compressed((NetworkWriter) writer, direction);
      ServerRpcSender.Send((NetworkBehaviour) this, 39, (NetworkWriter) writer, Mirage.Channel.Reliable, true);
      writer.Release();
    }
  }

  [ClientRpc(excludeOwner = true)]
  public void RpcSetTurretVector(byte weaponStationIndex, Vector3Compressed direction)
  {
    if (ClientRpcSender.ShouldInvokeLocally((NetworkBehaviour) this, RpcTarget.Observers, (INetworkPlayer) null, true))
      this.UserCode_RpcSetTurretVector_\u002D312569381(weaponStationIndex, direction);
    PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
    writer.WriteByteExtension(weaponStationIndex);
    GeneratedNetworkCode._Write_Vector3Compressed((NetworkWriter) writer, direction);
    ClientRpcSender.Send((NetworkBehaviour) this, 40, (NetworkWriter) writer, Mirage.Channel.Reliable, true);
    writer.Release();
  }

  public void StartEjectionSequence()
  {
    if (this.ejected)
      return;
    this.ejected = true;
    if (this.IsServer)
    {
      this.EjectionSequence().Forget();
    }
    else
    {
      if (!this.HasAuthority)
        return;
      this.CmdStartEjectionSequence();
    }
  }

  [ServerRpc]
  private void CmdStartEjectionSequence()
  {
    if (ServerRpcSender.ShouldInvokeLocally((NetworkBehaviour) this, true, false))
    {
      this.UserCode_CmdStartEjectionSequence_1872290971();
    }
    else
    {
      PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
      ServerRpcSender.Send((NetworkBehaviour) this, 41, (NetworkWriter) writer, Mirage.Channel.Reliable, true);
      writer.Release();
    }
  }

  [ClientRpc]
  public void RpcJettisonCanopy()
  {
    if (ClientRpcSender.ShouldInvokeLocally((NetworkBehaviour) this, RpcTarget.Observers, (INetworkPlayer) null, false))
      this.UserCode_RpcJettisonCanopy_1196305304();
    PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
    ClientRpcSender.Send((NetworkBehaviour) this, 42, (NetworkWriter) writer, Mirage.Channel.Reliable, false);
    writer.Release();
  }

  [Mirage.Server]
  private UniTask EjectionSequence()
  {
    if (!this.IsServer)
      throw new MethodInvocationException("[Server] function 'EjectionSequence' called when server not active");
    // ISSUE: variable of a compiler-generated type
    Aircraft.\u003CEjectionSequence\u003Ed__212 stateMachine;
    // ISSUE: reference to a compiler-generated field
    stateMachine.\u003C\u003Et__builder = AsyncUniTaskMethodBuilder.Create();
    // ISSUE: reference to a compiler-generated field
    stateMachine.\u003C\u003E4__this = this;
    // ISSUE: reference to a compiler-generated field
    stateMachine.\u003C\u003E1__state = -1;
    // ISSUE: reference to a compiler-generated field
    stateMachine.\u003C\u003Et__builder.Start<Aircraft.\u003CEjectionSequence\u003Ed__212>(ref stateMachine);
    // ISSUE: reference to a compiler-generated field
    return stateMachine.\u003C\u003Et__builder.Task;
  }

  public void SpawnEjectingPilot(int pilotNumber)
  {
    UnitPart unitPart = this.pilots[pilotNumber].GetUnitPart();
    GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(GameAssets.i.pilotDismounted, this.pilots[pilotNumber].transform.position, this.pilots[pilotNumber].transform.rotation);
    PilotDismounted component = gameObject.GetComponent<PilotDismounted>();
    component.NetworkparentUnit = this.persistentID;
    component.NetworkunitPart = unitPart.id;
    component.NetworkunitName = this.unitName + " pilot";
    component.NetworkHQ = this.NetworkHQ;
    component.NetworkpilotNumber = (byte) pilotNumber;
    component.NetworkstartPosition = this.pilots[pilotNumber].transform.position.ToGlobalPosition();
    this.pilots[pilotNumber].ejected = true;
    if (pilotNumber == 0)
      component.Networkplayer = this.Player;
    this.ServerObjectManager.Spawn(gameObject, this.Owner);
  }

  protected override void ServerDisableUnit()
  {
    if (GameManager.gameState == GameState.Encyclopedia)
      return;
    base.ServerDisableUnit();
    if ((!this.IsLanded() || !((UnityEngine.Object) this.NetworkHQ != (UnityEngine.Object) null) ? 0 : (this.NetworkHQ.AnyNearAirbase(this.transform.position, out Airbase _) ? 1 : 0)) != 0)
      return;
    this.ReportKilled();
  }

  private void OnDisable()
  {
    foreach (UnitPart unitPart in this.partLookup)
    {
      if ((UnityEngine.Object) unitPart != (UnityEngine.Object) null)
        unitPart.DetachDamageParticles();
    }
  }

  protected override void OnDestroy()
  {
    base.OnDestroy();
    this.OnRearm = (Action<RearmEventArgs>) null;
    foreach (UnitPart unitPart in this.partLookup)
    {
      if ((UnityEngine.Object) unitPart != (UnityEngine.Object) null)
        unitPart.RemovePart();
    }
    foreach (GameObject gameObject in this.groundEquipment)
    {
      if ((UnityEngine.Object) gameObject != (UnityEngine.Object) null)
        UnityEngine.Object.Destroy((UnityEngine.Object) gameObject);
    }
  }

  public override void UnitDisabled(bool oldState, bool newState)
  {
    base.UnitDisabled(oldState, newState);
    this.SetDoppler(true);
    if (this.gameObject.activeSelf)
      this.WaitRemoveAircraft(30f).Forget();
    Player localPlayer;
    if (!this.LocalSim || !((UnityEngine.Object) this.Player != (UnityEngine.Object) null) || !GameManager.GetLocalPlayer<Player>(out localPlayer) || !((UnityEngine.Object) this.Player == (UnityEngine.Object) localPlayer))
      return;
    this.Player.RemoveAircraft(this);
    this.Player.ShowMap(5f);
    SceneSingleton<CombatHUD>.i.RemoveAircraft();
    if (!((UnityEngine.Object) this.statusDisplay != (UnityEngine.Object) null))
      return;
    UnityEngine.Object.Destroy((UnityEngine.Object) this.statusDisplay.gameObject);
  }

  [Mirage.Server]
  public void ReturnToInventory()
  {
    if (!this.IsServer)
      throw new MethodInvocationException("[Server] function 'ReturnToInventory' called when server not active");
    this.weaponManager.ReturnWeapons();
    float score = this.sortieScore * MissionManager.CurrentMission.missionSettings.successfulSortieBonus;
    if ((UnityEngine.Object) this.Player != (UnityEngine.Object) null)
    {
      if ((double) score > 0.0)
        this.SuccessfulSortie(score);
      this.Player.RecoverAirframeInUse(this.definition);
    }
    else
      this.NetworkHQ.AddSupplyUnit((UnitDefinition) this.definition, 1);
    this.NetworkHQ.AddScore(score);
    this.NetworkunitState = Unit.UnitState.Returned;
    this.Networkdisabled = true;
    this.WaitRemoveAircraft(2f).Forget();
  }

  private void SuccessfulSortie(float score)
  {
    this.Player.RpcShowSortieBonus(score);
    this.Player.AddScore(score);
    this.NetworksortieScore = 0.0f;
    Action<float> sortieSuccessful = this.onSortieSuccessful;
    if (sortieSuccessful == null)
      return;
    sortieSuccessful(score);
  }

  private async UniTask WaitRemoveAircraft(float delay)
  {
    Aircraft aircraft = this;
    CancellationToken cancel = aircraft.destroyCancellationToken;
    await UniTask.Delay((int) ((double) delay * 1000.0));
    if (cancel.IsCancellationRequested)
      cancel = new CancellationToken();
    else if (!NetworkManagerNuclearOption.i.Server.Active)
    {
      cancel = new CancellationToken();
    }
    else
    {
      UnityEngine.Object.Destroy((UnityEngine.Object) aircraft.gameObject);
      cancel = new CancellationToken();
    }
  }

  private void MirageProcessed()
  {
  }

  public PlayerRef NetworkplayerRef
  {
    get => this.playerRef;
    set
    {
      if (this.SyncVarEqual<PlayerRef>(value, this.playerRef))
        return;
      PlayerRef playerRef = this.playerRef;
      this.playerRef = value;
      this.SetDirtyBit(512UL /*0x0200*/);
    }
  }

  public Loadout Networkloadout
  {
    get => this.loadout;
    set
    {
      if (this.SyncVarEqual<Loadout>(value, this.loadout))
        return;
      Loadout loadout = this.loadout;
      this.loadout = value;
      this.SetDirtyBit(1024UL /*0x0400*/);
      if (!this.GetSyncVarHookGuard(1024UL /*0x0400*/) && this.IsHost)
      {
        this.SetSyncVarHookGuard(1024UL /*0x0400*/, true);
        Action onLoadoutChanged = this.onLoadoutChanged;
        if (onLoadoutChanged != null)
          onLoadoutChanged();
        this.SetSyncVarHookGuard(1024UL /*0x0400*/, false);
      }
    }
  }

  public Hangar NetworkspawningHangar
  {
    get => (Hangar) this.spawningHangar.Value;
    set
    {
      if (this.SyncVarEqual<Hangar>(value, (Hangar) this.spawningHangar.Value))
        return;
      Hangar hangar = (Hangar) this.spawningHangar.Value;
      this.spawningHangar.Value = (NetworkBehaviour) value;
      this.SetDirtyBit(2048UL /*0x0800*/);
    }
  }

  public Vector3 NetworkstartingVelocity
  {
    get => this.startingVelocity;
    set
    {
      if (this.SyncVarEqual<Vector3>(value, this.startingVelocity))
        return;
      Vector3 startingVelocity = this.startingVelocity;
      this.startingVelocity = value;
      this.SetDirtyBit(4096UL /*0x1000*/);
    }
  }

  public LiveryKey NetworkLiveryKey
  {
    get => this.LiveryKey;
    set
    {
      if (this.SyncVarEqual<LiveryKey>(value, this.LiveryKey))
        return;
      LiveryKey liveryKey = this.LiveryKey;
      this.LiveryKey = value;
      this.SetDirtyBit(8192UL /*0x2000*/);
    }
  }

  public float NetworksortieScore
  {
    get => this.sortieScore;
    set
    {
      if (this.SyncVarEqual<float>(value, this.sortieScore))
        return;
      float sortieScore = this.sortieScore;
      this.sortieScore = value;
      this.SetDirtyBit(16384UL /*0x4000*/);
    }
  }

  public bool NetworkcountermeasureTrigger
  {
    get => this.countermeasureTrigger;
    set
    {
      if (this.SyncVarEqual<bool>(value, this.countermeasureTrigger))
        return;
      bool countermeasureTrigger = this.countermeasureTrigger;
      this.countermeasureTrigger = value;
      this.SetDirtyBit(32768UL /*0x8000*/);
    }
  }

  public float NetworkfuelLevel
  {
    get => this.fuelLevel;
    set
    {
      if (this.SyncVarEqual<float>(value, this.fuelLevel))
        return;
      float fuelLevel = this.fuelLevel;
      this.fuelLevel = value;
      this.SetDirtyBit(65536UL /*0x010000*/);
    }
  }

  public bool NetworkIgnition
  {
    get => this.Ignition;
    set
    {
      if (this.SyncVarEqual<bool>(value, this.Ignition))
        return;
      bool ignition = this.Ignition;
      this.Ignition = value;
      this.SetDirtyBit(131072UL /*0x020000*/);
    }
  }

  public bool NetworkgearDeployed
  {
    get => this.gearDeployed;
    set
    {
      if (this.SyncVarEqual<bool>(value, this.gearDeployed))
        return;
      bool gearDeployed = this.gearDeployed;
      this.gearDeployed = value;
      this.SetDirtyBit(262144UL /*0x040000*/);
      if (!this.GetSyncVarHookGuard(262144UL /*0x040000*/) && this.IsHost)
      {
        this.SetSyncVarHookGuard(262144UL /*0x040000*/, true);
        this.GearStateChangedHook(gearDeployed, value);
        this.SetSyncVarHookGuard(262144UL /*0x040000*/, false);
      }
    }
  }

  public override bool SerializeSyncVars(NetworkWriter writer, bool initialize)
  {
    ulong syncVarDirtyBits = this.SyncVarDirtyBits;
    bool flag = base.SerializeSyncVars(writer, initialize);
    if (initialize)
    {
      GeneratedNetworkCode._Write_NuclearOption\u002ENetworking\u002EPlayerRef(writer, this.playerRef);
      GeneratedNetworkCode._Write_NuclearOption\u002ESavedMission\u002ELoadout(writer, this.loadout);
      writer.WriteNetworkBehaviorSyncVar(this.spawningHangar);
      writer.WriteVector3(this.startingVelocity);
      GeneratedNetworkCode._Write_LiveryKey(writer, this.LiveryKey);
      writer.WriteSingleConverter(this.sortieScore);
      writer.WriteBooleanExtension(this.countermeasureTrigger);
      writer.WriteSingleConverter(this.fuelLevel);
      writer.WriteBooleanExtension(this.Ignition);
      writer.WriteBooleanExtension(this.gearDeployed);
      return true;
    }
    writer.Write(syncVarDirtyBits >> 9, 10);
    if (((long) syncVarDirtyBits & 512L /*0x0200*/) != 0L)
    {
      GeneratedNetworkCode._Write_NuclearOption\u002ENetworking\u002EPlayerRef(writer, this.playerRef);
      flag = true;
    }
    if (((long) syncVarDirtyBits & 1024L /*0x0400*/) != 0L)
    {
      GeneratedNetworkCode._Write_NuclearOption\u002ESavedMission\u002ELoadout(writer, this.loadout);
      flag = true;
    }
    if (((long) syncVarDirtyBits & 2048L /*0x0800*/) != 0L)
    {
      writer.WriteNetworkBehaviorSyncVar(this.spawningHangar);
      flag = true;
    }
    if (((long) syncVarDirtyBits & 4096L /*0x1000*/) != 0L)
    {
      writer.WriteVector3(this.startingVelocity);
      flag = true;
    }
    if (((long) syncVarDirtyBits & 8192L /*0x2000*/) != 0L)
    {
      GeneratedNetworkCode._Write_LiveryKey(writer, this.LiveryKey);
      flag = true;
    }
    if (((long) syncVarDirtyBits & 16384L /*0x4000*/) != 0L)
    {
      writer.WriteSingleConverter(this.sortieScore);
      flag = true;
    }
    if (((long) syncVarDirtyBits & 32768L /*0x8000*/) != 0L)
    {
      writer.WriteBooleanExtension(this.countermeasureTrigger);
      flag = true;
    }
    if (((long) syncVarDirtyBits & 65536L /*0x010000*/) != 0L)
    {
      writer.WriteSingleConverter(this.fuelLevel);
      flag = true;
    }
    if (((long) syncVarDirtyBits & 131072L /*0x020000*/) != 0L)
    {
      writer.WriteBooleanExtension(this.Ignition);
      flag = true;
    }
    if (((long) syncVarDirtyBits & 262144L /*0x040000*/) != 0L)
    {
      writer.WriteBooleanExtension(this.gearDeployed);
      flag = true;
    }
    return flag;
  }

  public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
  {
    base.DeserializeSyncVars(reader, initialState);
    if (initialState)
    {
      this.playerRef = GeneratedNetworkCode._Read_NuclearOption\u002ENetworking\u002EPlayerRef(reader);
      Loadout loadout = this.loadout;
      this.loadout = GeneratedNetworkCode._Read_NuclearOption\u002ESavedMission\u002ELoadout(reader);
      this.spawningHangar = reader.ReadNetworkBehaviourSyncVar();
      this.startingVelocity = reader.ReadVector3();
      this.LiveryKey = GeneratedNetworkCode._Read_LiveryKey(reader);
      this.sortieScore = reader.ReadSingleConverter();
      this.countermeasureTrigger = reader.ReadBooleanExtension();
      this.fuelLevel = reader.ReadSingleConverter();
      this.Ignition = reader.ReadBooleanExtension();
      bool gearDeployed = this.gearDeployed;
      this.gearDeployed = reader.ReadBooleanExtension();
      if (!this.IsServer && !this.SyncVarEqual<Loadout>(loadout, this.loadout))
      {
        Action onLoadoutChanged = this.onLoadoutChanged;
        if (onLoadoutChanged != null)
          onLoadoutChanged();
      }
      if (this.IsServer || this.SyncVarEqual<bool>(gearDeployed, this.gearDeployed))
        return;
      this.GearStateChangedHook(gearDeployed, this.gearDeployed);
    }
    else
    {
      ulong dirtyBit = reader.Read(10);
      this.SetDeserializeMask(dirtyBit, 9);
      if (((long) dirtyBit & 1L) != 0L)
        this.playerRef = GeneratedNetworkCode._Read_NuclearOption\u002ENetworking\u002EPlayerRef(reader);
      if (((long) dirtyBit & 2L) != 0L)
      {
        Loadout loadout = this.loadout;
        this.loadout = GeneratedNetworkCode._Read_NuclearOption\u002ESavedMission\u002ELoadout(reader);
        if (!this.IsServer && !this.SyncVarEqual<Loadout>(loadout, this.loadout))
        {
          Action onLoadoutChanged = this.onLoadoutChanged;
          if (onLoadoutChanged != null)
            onLoadoutChanged();
        }
      }
      if (((long) dirtyBit & 4L) != 0L)
        this.spawningHangar = reader.ReadNetworkBehaviourSyncVar();
      if (((long) dirtyBit & 8L) != 0L)
        this.startingVelocity = reader.ReadVector3();
      if (((long) dirtyBit & 16L /*0x10*/) != 0L)
        this.LiveryKey = GeneratedNetworkCode._Read_LiveryKey(reader);
      if (((long) dirtyBit & 32L /*0x20*/) != 0L)
        this.sortieScore = reader.ReadSingleConverter();
      if (((long) dirtyBit & 64L /*0x40*/) != 0L)
        this.countermeasureTrigger = reader.ReadBooleanExtension();
      if (((long) dirtyBit & 128L /*0x80*/) != 0L)
        this.fuelLevel = reader.ReadSingleConverter();
      if (((long) dirtyBit & 256L /*0x0100*/) != 0L)
        this.Ignition = reader.ReadBooleanExtension();
      if (((long) dirtyBit & 512L /*0x0200*/) == 0L)
        return;
      bool gearDeployed = this.gearDeployed;
      this.gearDeployed = reader.ReadBooleanExtension();
      if (!this.IsServer && !this.SyncVarEqual<bool>(gearDeployed, this.gearDeployed))
        this.GearStateChangedHook(gearDeployed, this.gearDeployed);
    }
  }

  public void UserCode_RpcGetRadarWarning_\u002D1586111906(Unit radarSource)
  {
    Radar radar = radarSource.radar as Radar;
    float returnSignal;
    bool flag = this.EstimateDetection(radar, out returnSignal);
    Action<Aircraft.OnRadarWarning> onRadarWarning = this.onRadarWarning;
    if (onRadarWarning == null)
      return;
    onRadarWarning(new Aircraft.OnRadarWarning()
    {
      emitter = radarSource,
      radar = radar,
      power = returnSignal,
      detected = flag,
      isTarget = radarSource.CheckIsTarget((Unit) this)
    });
  }

  protected static void Skeleton_RpcGetRadarWarning_\u002D1586111906(
    NetworkBehaviour behaviour,
    NetworkReader reader,
    INetworkPlayer senderConnection,
    int replyId)
  {
    ((Aircraft) behaviour).UserCode_RpcGetRadarWarning_\u002D1586111906(GeneratedNetworkCode._Read_Unit(reader));
  }

  public void UserCode_CmdToggleRadar_1821461427() => this.RpcToggleRadar(!this.radar.activated);

  protected static void Skeleton_CmdToggleRadar_1821461427(
    NetworkBehaviour behaviour,
    NetworkReader reader,
    INetworkPlayer senderConnection,
    int replyId)
  {
    ((Aircraft) behaviour).UserCode_CmdToggleRadar_1821461427();
  }

  private void UserCode_RpcToggleRadar_1325449311(bool activated)
  {
    this.radar.activated = activated;
    if (!((UnityEngine.Object) SceneSingleton<CombatHUD>.i.aircraft == (UnityEngine.Object) this))
      return;
    string report = this.radar.activated ? "Radar Armed" : "Radar Disarmed";
    SceneSingleton<AircraftActionsReport>.i.ReportText(report, 5f);
  }

  protected static void Skeleton_RpcToggleRadar_1325449311(
    NetworkBehaviour behaviour,
    NetworkReader reader,
    INetworkPlayer senderConnection,
    int replyId)
  {
    ((Aircraft) behaviour).UserCode_RpcToggleRadar_1325449311(reader.ReadBooleanExtension());
  }

  public void UserCode_CmdToggleIgnition_\u002D1067807998()
  {
    this.NetworkIgnition = !this.Ignition;
  }

  protected static void Skeleton_CmdToggleIgnition_\u002D1067807998(
    NetworkBehaviour behaviour,
    NetworkReader reader,
    INetworkPlayer senderConnection,
    int replyId)
  {
    ((Aircraft) behaviour).UserCode_CmdToggleIgnition_\u002D1067807998();
  }

  private void UserCode_CmdSetCanRefuel_\u002D170919880() => this.needsFuel = true;

  protected static void Skeleton_CmdSetCanRefuel_\u002D170919880(
    NetworkBehaviour behaviour,
    NetworkReader reader,
    INetworkPlayer senderConnection,
    int replyId)
  {
    ((Aircraft) behaviour).UserCode_CmdSetCanRefuel_\u002D170919880();
  }

  public void UserCode_CmdSlingLoadAttachment_\u002D82313074(
    Unit slingLoadUnit,
    SlingloadHook.DeployState state)
  {
    bool flag1 = state == SlingloadHook.DeployState.Connected || state == SlingloadHook.DeployState.RescuePilot;
    bool flag2 = (UnityEngine.Object) slingLoadUnit != (UnityEngine.Object) null & flag1 && slingLoadUnit.IsSlung() && state != SlingloadHook.DeployState.RescuePilot;
    bool flag3 = (UnityEngine.Object) slingLoadUnit != (UnityEngine.Object) null && FastMath.OutOfRange(this.GlobalPosition(), slingLoadUnit.GlobalPosition(), flag1 ? 50f : 200f);
    bool flag4 = (UnityEngine.Object) slingLoadUnit != (UnityEngine.Object) null && !slingLoadUnit.definition.CanSlingLoad;
    if (flag2 | flag3 | flag4)
      Debug.LogError((object) $"Unable to validate sling load attachment: doubleAttach: {flag2}, outOfRange: {flag3}, invalidUnit: {flag4}");
    else
      this.RpcSlingLoadAttachment(slingLoadUnit, state);
  }

  protected static void Skeleton_CmdSlingLoadAttachment_\u002D82313074(
    NetworkBehaviour behaviour,
    NetworkReader reader,
    INetworkPlayer senderConnection,
    int replyId)
  {
    ((Aircraft) behaviour).UserCode_CmdSlingLoadAttachment_\u002D82313074(GeneratedNetworkCode._Read_Unit(reader), GeneratedNetworkCode._Read_SlingloadHook\u002FDeployState(reader));
  }

  public void UserCode_RpcSlingLoadAttachment_\u002D57940477(
    Unit slingLoadUnit,
    SlingloadHook.DeployState state)
  {
    this.ApplySlingLoadAttachment(slingLoadUnit, state);
  }

  protected static void Skeleton_RpcSlingLoadAttachment_\u002D57940477(
    NetworkBehaviour behaviour,
    NetworkReader reader,
    INetworkPlayer senderConnection,
    int replyId)
  {
    ((Aircraft) behaviour).UserCode_RpcSlingLoadAttachment_\u002D57940477(GeneratedNetworkCode._Read_Unit(reader), GeneratedNetworkCode._Read_SlingloadHook\u002FDeployState(reader));
  }

  public void UserCode_CmdSendSlungTransform_\u002D695501160(Vector3 position, Quaternion rotation)
  {
    if ((UnityEngine.Object) this.slungUnit == (UnityEngine.Object) null || !NetworkFloatHelper.Validate(position, false, (string) null) || !NetworkFloatHelper.Validate(rotation, false, (string) null))
      return;
    position = Vector3.ClampMagnitude(position, 50f);
    Vector3 position1 = this.transform.TransformPoint(position);
    this.slungUnit.transform.SetPositionAndRotation(position1, rotation);
    this.slungUnit.rb.MovePosition(position1);
    this.slungUnit.rb.MoveRotation(rotation);
    this.slungUnit.rb.velocity = this.rb.velocity;
  }

  protected static void Skeleton_CmdSendSlungTransform_\u002D695501160(
    NetworkBehaviour behaviour,
    NetworkReader reader,
    INetworkPlayer senderConnection,
    int replyId)
  {
    ((Aircraft) behaviour).UserCode_CmdSendSlungTransform_\u002D695501160(reader.ReadVector3(), reader.ReadQuaternion());
  }

  private void UserCode_RpcRearm_1198124017(RearmEventArgs args)
  {
    Action<RearmEventArgs> onRearm = this.OnRearm;
    if (onRearm == null)
      return;
    onRearm(args);
  }

  protected static void Skeleton_RpcRearm_1198124017(
    NetworkBehaviour behaviour,
    NetworkReader reader,
    INetworkPlayer senderConnection,
    int replyId)
  {
    ((Aircraft) behaviour).UserCode_RpcRearm_1198124017(GeneratedNetworkCode._Read_RearmEventArgs(reader));
  }

  private void UserCode_RpcRefuel_1587114129(Unit refueler) => this.SetFuelLevel(refueler);

  protected static void Skeleton_RpcRefuel_1587114129(
    NetworkBehaviour behaviour,
    NetworkReader reader,
    INetworkPlayer senderConnection,
    int replyId)
  {
    ((Aircraft) behaviour).UserCode_RpcRefuel_1587114129(GeneratedNetworkCode._Read_Unit(reader));
  }

  public virtual void UserCode_RpcDamage_870168505(byte index, DamageInfo damageInfo)
  {
    this.UserCode_RpcDamage_1709046923(index, damageInfo);
    this.ShakeAircraft(damageInfo.blastDamage.Decompress() * 0.01f, damageInfo.pierceDamage.Decompress() * 0.01f);
  }

  protected static void Skeleton_RpcDamage_870168505(
    NetworkBehaviour behaviour,
    NetworkReader reader,
    INetworkPlayer senderConnection,
    int replyId)
  {
    ((Aircraft) behaviour).UserCode_RpcDamage_870168505(reader.ReadByteExtension(), GeneratedNetworkCode._Read_DamageInfo(reader));
  }

  public void UserCode_CmdLaunchMissile_644415535(
    byte stationIndex,
    Unit target,
    GlobalPosition aimpoint)
  {
    if ((int) stationIndex >= this.weaponStations.Count || !NetworkFloatHelper.Validate(aimpoint, false, (string) null))
      return;
    this.RequestRearm();
    this.weaponStations[(int) stationIndex].LaunchMount((Unit) this, target, aimpoint);
    this.RpcLaunchMissile(stationIndex, target, aimpoint);
  }

  protected static void Skeleton_CmdLaunchMissile_644415535(
    NetworkBehaviour behaviour,
    NetworkReader reader,
    INetworkPlayer senderConnection,
    int replyId)
  {
    ((Aircraft) behaviour).UserCode_CmdLaunchMissile_644415535(reader.ReadByteExtension(), GeneratedNetworkCode._Read_Unit(reader), reader.ReadGlobalPosition());
  }

  public void UserCode_RpcLaunchMissile_1465828762(
    byte stationIndex,
    Unit target,
    GlobalPosition aimpoint)
  {
    if (NetworkManagerNuclearOption.i.Server.Active || this.CheckIfLocalSim())
      return;
    this.weaponStations[(int) stationIndex].LaunchMount((Unit) this, target, aimpoint);
  }

  protected static void Skeleton_RpcLaunchMissile_1465828762(
    NetworkBehaviour behaviour,
    NetworkReader reader,
    INetworkPlayer senderConnection,
    int replyId)
  {
    ((Aircraft) behaviour).UserCode_RpcLaunchMissile_1465828762(reader.ReadByteExtension(), GeneratedNetworkCode._Read_Unit(reader), reader.ReadGlobalPosition());
  }

  private void UserCode_CmdCountermeasures_\u002D1391811506(bool active, byte index)
  {
    this.NetworkcountermeasureTrigger = active;
    int activeIndex = (int) this.countermeasureManager.activeIndex;
    this.countermeasureManager.activeIndex = index;
    int num = (int) index;
    if (activeIndex == num)
      return;
    this.RpcCountermeasures(index);
  }

  protected static void Skeleton_CmdCountermeasures_\u002D1391811506(
    NetworkBehaviour behaviour,
    NetworkReader reader,
    INetworkPlayer senderConnection,
    int replyId)
  {
    ((Aircraft) behaviour).UserCode_CmdCountermeasures_\u002D1391811506(reader.ReadBooleanExtension(), reader.ReadByteExtension());
  }

  private void UserCode_RpcCountermeasures_1466036644(byte index)
  {
    this.countermeasureManager.activeIndex = index;
  }

  protected static void Skeleton_RpcCountermeasures_1466036644(
    NetworkBehaviour behaviour,
    NetworkReader reader,
    INetworkPlayer senderConnection,
    int replyId)
  {
    ((Aircraft) behaviour).UserCode_RpcCountermeasures_1466036644(reader.ReadByteExtension());
  }

  private void UserCode_CmdSetGear_\u002D1969689879(bool deployed)
  {
    this.NetworkgearDeployed = deployed;
  }

  protected static void Skeleton_CmdSetGear_\u002D1969689879(
    NetworkBehaviour behaviour,
    NetworkReader reader,
    INetworkPlayer senderConnection,
    int replyId)
  {
    ((Aircraft) behaviour).UserCode_CmdSetGear_\u002D1969689879(reader.ReadBooleanExtension());
  }

  public void UserCode_CmdSetStationTurretTarget_\u002D461984136(
    byte stationIndex,
    byte turretIndex,
    PersistentID targetID)
  {
    if ((int) stationIndex >= this.weaponStations.Count)
      ColorLog<Aircraft>.LogError("stationIndex was out of bounds");
    else
      this.RpcSetStationTurretTarget(stationIndex, turretIndex, targetID);
  }

  protected static void Skeleton_CmdSetStationTurretTarget_\u002D461984136(
    NetworkBehaviour behaviour,
    NetworkReader reader,
    INetworkPlayer senderConnection,
    int replyId)
  {
    ((Aircraft) behaviour).UserCode_CmdSetStationTurretTarget_\u002D461984136(reader.ReadByteExtension(), reader.ReadByteExtension(), GeneratedNetworkCode._Read_PersistentID(reader));
  }

  public void UserCode_RpcSetStationTurretTarget_740629667(
    byte stationIndex,
    byte turretIndex,
    PersistentID targetID)
  {
    this.weaponStations[(int) stationIndex].SetStationTurretTarget(turretIndex, targetID);
  }

  protected static void Skeleton_RpcSetStationTurretTarget_740629667(
    NetworkBehaviour behaviour,
    NetworkReader reader,
    INetworkPlayer senderConnection,
    int replyId)
  {
    ((Aircraft) behaviour).UserCode_RpcSetStationTurretTarget_740629667(reader.ReadByteExtension(), reader.ReadByteExtension(), GeneratedNetworkCode._Read_PersistentID(reader));
  }

  private void UserCode_CmdSetActiveStation_126720094(byte stationIndex)
  {
    if ((int) stationIndex >= this.weaponStations.Count)
      ColorLog<Aircraft>.LogError("stationIndex was out of bounds");
    else
      this.RpcSetActiveStation(stationIndex);
  }

  protected static void Skeleton_CmdSetActiveStation_126720094(
    NetworkBehaviour behaviour,
    NetworkReader reader,
    INetworkPlayer senderConnection,
    int replyId)
  {
    ((Aircraft) behaviour).UserCode_CmdSetActiveStation_126720094(reader.ReadByteExtension());
  }

  private void UserCode_RpcSetActiveStation_1081017235(byte stationIndex)
  {
    if (this.LocalSim)
      return;
    this.weaponManager.SetActiveStation(stationIndex);
  }

  protected static void Skeleton_RpcSetActiveStation_1081017235(
    NetworkBehaviour behaviour,
    NetworkReader reader,
    INetworkPlayer senderConnection,
    int replyId)
  {
    ((Aircraft) behaviour).UserCode_RpcSetActiveStation_1081017235(reader.ReadByteExtension());
  }

  private void UserCode_CmdSetTurretVector_\u002D1133982608(
    byte weaponStationIndex,
    Vector3Compressed direction)
  {
    if (!NetworkFloatHelper.Validate(direction, false, (string) null))
      return;
    if ((int) weaponStationIndex >= this.weaponStations.Count)
      ColorLog<Aircraft>.LogError("stationIndex was out of bounds");
    else
      this.RpcSetTurretVector(weaponStationIndex, direction);
  }

  protected static void Skeleton_CmdSetTurretVector_\u002D1133982608(
    NetworkBehaviour behaviour,
    NetworkReader reader,
    INetworkPlayer senderConnection,
    int replyId)
  {
    ((Aircraft) behaviour).UserCode_CmdSetTurretVector_\u002D1133982608(reader.ReadByteExtension(), GeneratedNetworkCode._Read_Vector3Compressed(reader));
  }

  public void UserCode_RpcSetTurretVector_\u002D312569381(
    byte weaponStationIndex,
    Vector3Compressed direction)
  {
    if (this.LocalSim)
      return;
    this.weaponStations[(int) weaponStationIndex].SetTurretVector(NetworkFloatHelper.DecompressIfValid(direction, true, nameof (direction), Vector3.forward));
  }

  protected static void Skeleton_RpcSetTurretVector_\u002D312569381(
    NetworkBehaviour behaviour,
    NetworkReader reader,
    INetworkPlayer senderConnection,
    int replyId)
  {
    ((Aircraft) behaviour).UserCode_RpcSetTurretVector_\u002D312569381(reader.ReadByteExtension(), GeneratedNetworkCode._Read_Vector3Compressed(reader));
  }

  private void UserCode_CmdStartEjectionSequence_1872290971()
  {
    if (this.ejected)
      return;
    this.ejected = true;
    this.EjectionSequence().Forget();
  }

  protected static void Skeleton_CmdStartEjectionSequence_1872290971(
    NetworkBehaviour behaviour,
    NetworkReader reader,
    INetworkPlayer senderConnection,
    int replyId)
  {
    ((Aircraft) behaviour).UserCode_CmdStartEjectionSequence_1872290971();
  }

  public void UserCode_RpcJettisonCanopy_1196305304()
  {
    if (this.IsLanded())
    {
      this.OpenCanopies();
      if ((UnityEngine.Object) this.NetworkHQ != (UnityEngine.Object) null && this.NetworkHQ.AnyNearAirbase(this.transform.position, out Airbase _))
        return;
    }
    else
    {
      foreach (Canopy canopy in this.canopies)
        canopy.Eject();
      Action onEject = this.onEject;
      if (onEject != null)
        onEject();
    }
    if (!GameManager.IsLocalAircraft(this) || MissionHelper.CanRespawn)
      return;
    GameManager.FinishGame(GameResolution.Defeat);
  }

  protected static void Skeleton_RpcJettisonCanopy_1196305304(
    NetworkBehaviour behaviour,
    NetworkReader reader,
    INetworkPlayer senderConnection,
    int replyId)
  {
    ((Aircraft) behaviour).UserCode_RpcJettisonCanopy_1196305304();
  }

  protected override int GetRpcCount() => 43;

  public override void RegisterRpc(RemoteCallCollection collection)
  {
    base.RegisterRpc(collection);
    collection.Register(19, "Aircraft.RpcGetRadarWarning", false, RpcInvokeType.ClientRpc, (NetworkBehaviour) this, new RpcDelegate(Aircraft.Skeleton_RpcGetRadarWarning_\u002D1586111906));
    collection.Register(20, "Aircraft.CmdToggleRadar", true, RpcInvokeType.ServerRpc, (NetworkBehaviour) this, new RpcDelegate(Aircraft.Skeleton_CmdToggleRadar_1821461427));
    collection.Register(21, "Aircraft.RpcToggleRadar", false, RpcInvokeType.ClientRpc, (NetworkBehaviour) this, new RpcDelegate(Aircraft.Skeleton_RpcToggleRadar_1325449311));
    collection.Register(22, "Aircraft.CmdToggleIgnition", true, RpcInvokeType.ServerRpc, (NetworkBehaviour) this, new RpcDelegate(Aircraft.Skeleton_CmdToggleIgnition_\u002D1067807998));
    collection.Register(23, "Aircraft.CmdSetCanRefuel", true, RpcInvokeType.ServerRpc, (NetworkBehaviour) this, new RpcDelegate(Aircraft.Skeleton_CmdSetCanRefuel_\u002D170919880));
    collection.Register(24, "Aircraft.CmdSlingLoadAttachment", true, RpcInvokeType.ServerRpc, (NetworkBehaviour) this, new RpcDelegate(Aircraft.Skeleton_CmdSlingLoadAttachment_\u002D82313074));
    collection.Register(25, "Aircraft.RpcSlingLoadAttachment", false, RpcInvokeType.ClientRpc, (NetworkBehaviour) this, new RpcDelegate(Aircraft.Skeleton_RpcSlingLoadAttachment_\u002D57940477));
    collection.Register(26, "Aircraft.CmdSendSlungTransform", true, RpcInvokeType.ServerRpc, (NetworkBehaviour) this, new RpcDelegate(Aircraft.Skeleton_CmdSendSlungTransform_\u002D695501160));
    collection.Register(27, "Aircraft.RpcRearm", false, RpcInvokeType.ClientRpc, (NetworkBehaviour) this, new RpcDelegate(Aircraft.Skeleton_RpcRearm_1198124017));
    collection.Register(28, "Aircraft.RpcRefuel", false, RpcInvokeType.ClientRpc, (NetworkBehaviour) this, new RpcDelegate(Aircraft.Skeleton_RpcRefuel_1587114129));
    collection.Register(29, "Aircraft.RpcDamage", false, RpcInvokeType.ClientRpc, (NetworkBehaviour) this, new RpcDelegate(Aircraft.Skeleton_RpcDamage_870168505));
    collection.Register(30, "Aircraft.CmdLaunchMissile", true, RpcInvokeType.ServerRpc, (NetworkBehaviour) this, new RpcDelegate(Aircraft.Skeleton_CmdLaunchMissile_644415535));
    collection.Register(31 /*0x1F*/, "Aircraft.RpcLaunchMissile", false, RpcInvokeType.ClientRpc, (NetworkBehaviour) this, new RpcDelegate(Aircraft.Skeleton_RpcLaunchMissile_1465828762));
    collection.Register(32 /*0x20*/, "Aircraft.CmdCountermeasures", true, RpcInvokeType.ServerRpc, (NetworkBehaviour) this, new RpcDelegate(Aircraft.Skeleton_CmdCountermeasures_\u002D1391811506));
    collection.Register(33, "Aircraft.RpcCountermeasures", false, RpcInvokeType.ClientRpc, (NetworkBehaviour) this, new RpcDelegate(Aircraft.Skeleton_RpcCountermeasures_1466036644));
    collection.Register(34, "Aircraft.CmdSetGear", true, RpcInvokeType.ServerRpc, (NetworkBehaviour) this, new RpcDelegate(Aircraft.Skeleton_CmdSetGear_\u002D1969689879));
    collection.Register(35, "Aircraft.CmdSetStationTurretTarget", true, RpcInvokeType.ServerRpc, (NetworkBehaviour) this, new RpcDelegate(Aircraft.Skeleton_CmdSetStationTurretTarget_\u002D461984136));
    collection.Register(36, "Aircraft.RpcSetStationTurretTarget", false, RpcInvokeType.ClientRpc, (NetworkBehaviour) this, new RpcDelegate(Aircraft.Skeleton_RpcSetStationTurretTarget_740629667));
    collection.Register(37, "Aircraft.CmdSetActiveStation", true, RpcInvokeType.ServerRpc, (NetworkBehaviour) this, new RpcDelegate(Aircraft.Skeleton_CmdSetActiveStation_126720094));
    collection.Register(38, "Aircraft.RpcSetActiveStation", false, RpcInvokeType.ClientRpc, (NetworkBehaviour) this, new RpcDelegate(Aircraft.Skeleton_RpcSetActiveStation_1081017235));
    collection.Register(39, "Aircraft.CmdSetTurretVector", true, RpcInvokeType.ServerRpc, (NetworkBehaviour) this, new RpcDelegate(Aircraft.Skeleton_CmdSetTurretVector_\u002D1133982608));
    collection.Register(40, "Aircraft.RpcSetTurretVector", false, RpcInvokeType.ClientRpc, (NetworkBehaviour) this, new RpcDelegate(Aircraft.Skeleton_RpcSetTurretVector_\u002D312569381));
    collection.Register(41, "Aircraft.CmdStartEjectionSequence", true, RpcInvokeType.ServerRpc, (NetworkBehaviour) this, new RpcDelegate(Aircraft.Skeleton_CmdStartEjectionSequence_1872290971));
    collection.Register(42, "Aircraft.RpcJettisonCanopy", false, RpcInvokeType.ClientRpc, (NetworkBehaviour) this, new RpcDelegate(Aircraft.Skeleton_RpcJettisonCanopy_1196305304));
  }

  public struct OnFlightAssistToggle
  {
    public bool enabled;
  }

  public struct OnShake
  {
    public float lowFreqShake;
    public float highFreqShake;
  }

  public struct OnRadarWarning
  {
    public Radar radar;
    public Unit emitter;
    public float power;
    public bool detected;
    public bool isTarget;
  }

  public struct OnSetGear
  {
    public LandingGear.GearState gearState;
  }

  private class PartChecker
  {
    private List<AeroPart> parts;
    private int i;

    public PartChecker(Aircraft aircraft) => this.parts = aircraft.partsWithAero;

    public void Check()
    {
      if (this.parts.Count == 0)
        return;
      ++this.i;
      if (this.i >= this.parts.Count)
        this.i = 0;
      this.parts[this.i].CheckAttachment();
    }
  }
}

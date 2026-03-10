// Decompiled with JetBrains decompiler
// Type: Unit
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Cysharp.Threading.Tasks;
using Mirage;
using Mirage.RemoteCalls;
using Mirage.Serialization;
using NuclearOption.MissionEditorScripts;
using NuclearOption.Networking;
using NuclearOption.SavedMission;
using NuclearOption.SavedMission.ObjectiveV2;
using NuclearOption.SceneLoading;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

#nullable disable
public class Unit : NetworkBehaviour, IEditorSelectable
{
  public const string BUILTIN_UNIT_PREFIX = "<MAP_UNIT>++";
  [SyncVar(initialOnly = true)]
  public PersistentID persistentID;
  [SyncVar(initialOnly = true)]
  [NonSerialized]
  public string unitName;
  [SyncVar(initialOnly = true)]
  [NonSerialized]
  public string UniqueName;
  [SyncVar]
  [NonSerialized]
  public GlobalPosition startPosition;
  [SyncVar]
  [NonSerialized]
  public Quaternion startRotation = Quaternion.identity;
  [SyncVar(hook = "HQChanged")]
  [NonSerialized]
  public NetworkBehaviorSyncvar HQ;
  [SyncVar(hook = "UnitDisabled")]
  [NonSerialized]
  public bool disabled;
  [SyncVar]
  [NonSerialized]
  public Unit.UnitState unitState;
  [SyncVar(hook = "OnFiringStateUpdated")]
  [NonSerialized]
  private WeaponMask remoteWeaponStates;
  [NonSerialized]
  private WeaponMask localWeaponStates;
  private bool isRemoteFiring;
  [Header("Map Values")]
  [SerializeField]
  public string MapUniqueName;
  [Tooltip("Starting HQ for map units")]
  [SerializeField]
  public FactionHQ MapHQ;
  [SerializeField]
  public Airbase MapAirbase;
  [Space]
  public List<WeaponStation> weaponStations = new List<WeaponStation>();
  [NonSerialized]
  public float maxRadius;
  [NonSerialized]
  public float obstacleTop;
  [SerializeField]
  private Transform obstacleTopTransform;
  [NonSerialized]
  public float displayDetail;
  protected float visibility = 6f;
  public Transform cockpitViewPoint;
  public UnitDefinition definition;
  protected Dictionary<PersistentID, float> damageCredit;
  [NonSerialized]
  public List<UnitPart> partLookup = new List<UnitPart>();
  [NonSerialized]
  public List<DamageablePart> damageables = new List<DamageablePart>();
  private float extraCaptureStrength;
  [HideInInspector]
  public bool networked = true;
  [HideInInspector]
  public float airDensity = 1f;
  [HideInInspector]
  public float radarAlt;
  [HideInInspector]
  public float speed;
  protected float lastAltitudeCheck;
  public float RCS;
  protected RaycastHit hit;
  private List<IRSource> IRSources = new List<IRSource>();
  public TargetDetector radar;
  [HideInInspector]
  public List<DamageParticles> spawnedEffects = new List<DamageParticles>();
  private GridSquare gridSquare;
  private ObjectType objectType;
  [NonSerialized]
  private const int SYNC_VAR_COUNT = 9;
  [NonSerialized]
  private const int RPC_COUNT = 19;

  public void ClearWeaponStates()
  {
    this.NetworkremoteWeaponStates = new WeaponMask();
    this.localWeaponStates = new WeaponMask();
  }

  public FactionHQ Editor_HQ => this.NetworkHQ;

  public SavedUnit SavedUnit { get; protected set; }

  public bool BuiltIn
  {
    get => this.ObjectType == ObjectType.MapObject || this.objectType == ObjectType.SceneObject;
  }

  public bool IsCustom
  {
    get => this.SavedUnit != null && this.SavedUnit.PlacementType == PlacementType.Custom;
  }

  public Rigidbody rb { get; private set; }

  [field: NonSerialized]
  public bool remoteSim { get; private set; } = true;

  public bool LocalSim => !this.remoteSim;

  public float CaptureStrength
  {
    get
    {
      double extraCaptureStrength = (double) this.extraCaptureStrength;
      SavedUnit savedUnit = this.SavedUnit;
      double num = (savedUnit != null ? (double) savedUnit.CaptureStrength.AsNullable<float>() : (double) new float?()) ?? (double) this.definition.captureStrength;
      return (float) (extraCaptureStrength + num);
    }
  }

  public float CaptureDefense
  {
    get
    {
      SavedUnit savedUnit = this.SavedUnit;
      return (savedUnit != null ? savedUnit.CaptureDefense.AsNullable<float>() : new float?()) ?? this.definition.captureDefense;
    }
  }

  public event Action onInitialize;

  public event Action<Unit> onDisableUnit;

  public event Action<Unit> onChangeFaction;

  public event Action<IRSource> onAddIRSource;

  public event Action<Missile> onRegisterMissile;

  public event Action<Missile> onDeregisterMissile;

  public event Action<Unit.JamEventArgs> onJam;

  public ObjectType ObjectType
  {
    get
    {
      if (this.objectType == ObjectType.NotSet)
        this.objectType = MapLoader.GetObjectType(this.Identity);
      return this.objectType;
    }
  }

  public virtual Airbase GetAirbase() => (Airbase) null;

  protected virtual void OnValidate()
  {
    if (!((UnityEngine.Object) this.GetComponentInParent<NetworkMap>() != (UnityEngine.Object) null) || !string.IsNullOrEmpty(this.MapUniqueName))
      return;
    Debug.LogError((object) "Map Unit in NetworkMap is missing MapUniqueName");
  }

  public virtual void Awake()
  {
    this.Identity.OnStartServer.AddListener(new Action(this.OnStartServer));
    this.RCS = this.definition.radarSize;
    if (string.IsNullOrEmpty(this.MapUniqueName))
      return;
    UnitRegistry.RegisterCustomID(this.MapUniqueName, this);
  }

  private void OnStartServer()
  {
    this.NetworkpersistentID = UnitRegistry.GetNextIndex();
    if (string.IsNullOrEmpty(this.MapUniqueName))
      return;
    this.NetworkUniqueName = this.MapUniqueName;
  }

  public void EditorMapLoaded()
  {
    if (string.IsNullOrEmpty(this.MapUniqueName))
      return;
    if (this.SavedUnit == null)
      this.SavedUnit = this.CreateBuiltInSavedUnit();
    else
      Debug.LogWarning((object) "MapObject had SavedUnit before Awake");
  }

  public void EditorMapCleanup() => this.RemoveSavedUnitOverride(false);

  public void RemoveSavedUnitOverride(bool createNew)
  {
    this.SavedUnit = createNew ? this.CreateBuiltInSavedUnit() : (SavedUnit) null;
    this.NetworkHQ = this.MapHQ;
  }

  protected virtual SavedUnit CreateBuiltInSavedUnit()
  {
    throw new NotSupportedException(this.GetType().Name + " has no method to create a built in SavedUnit");
  }

  public virtual void OnEnable()
  {
    this.visibility = this.definition.visibleRange;
    this.maxRadius = Mathf.Max(Mathf.Max(this.definition.length, this.definition.width), this.definition.height) * 0.5f;
    this.obstacleTop = !((UnityEngine.Object) this.obstacleTopTransform != (UnityEngine.Object) null) ? float.MaxValue : this.obstacleTopTransform.position.y - this.transform.position.y;
    if ((UnityEngine.Object) this.rb == (UnityEngine.Object) null)
      this.SetRB(this.gameObject.GetComponentInChildren<Rigidbody>());
    this.gameObject.name = this.definition.unitPrefab.name;
    this.NetworkunitState = Unit.UnitState.Active;
  }

  public void ModifyCaptureStrength(float captureStrength)
  {
    this.extraCaptureStrength += captureStrength;
  }

  public void LinkSavedUnit(SavedUnit savedUnit)
  {
    this.SavedUnit = savedUnit;
    savedUnit.Unit = this;
    if (this.IsServer)
    {
      if (!string.IsNullOrEmpty(this.UniqueName))
        return;
      this.NetworkUniqueName = this.MapUniqueName;
    }
    else
    {
      if (this.IsClient)
        return;
      Debug.LogError((object) "Linking unit with SavedUnit before NetworkSpawn");
    }
  }

  public virtual float GetPrefabMass()
  {
    float prefabMass = 0.0f;
    foreach (UnitPart componentsInChild in this.gameObject.GetComponentsInChildren<UnitPart>())
      prefabMass += componentsInChild.GetMass();
    return prefabMass;
  }

  public virtual PowerSupply GetPowerSupply() => (PowerSupply) null;

  public virtual Vector3 GetWindVelocity() => Vector3.zero;

  public virtual float GetAirDensity() => 0.0f;

  public virtual float GetMass()
  {
    float mass = 0.0f;
    foreach (UnitPart unitPart in this.partLookup)
      mass += unitPart.GetMass();
    return mass;
  }

  public void ModifyRCS(float changeInRCS) => this.RCS += changeInRCS;

  public Vector3 GetCenterOfMass()
  {
    Vector3 zero = Vector3.zero;
    float num = 0.0f;
    foreach (UnitPart unitPart in this.partLookup)
    {
      float mass = unitPart.GetMass();
      num += mass;
      Vector3 vector3 = unitPart.xform.TransformPoint(unitPart.rb.centerOfMass);
      zero += vector3 * mass;
    }
    return zero / num;
  }

  public bool HasIRSignature() => this.IRSources.Count > 0;

  public bool HasRadarEmission() => this.radar is Radar && this.radar.activated;

  public void RegisterUnit(float? updateInterval)
  {
    if (GameManager.gameState == GameState.Encyclopedia)
      return;
    if (NetworkManagerNuclearOption.i.Server.Active && (UnityEngine.Object) this.NetworkHQ != (UnityEngine.Object) null)
      this.NetworkHQ.RegisterFactionUnit(this);
    if (this.IsClientOnly && !string.IsNullOrEmpty(this.UniqueName))
      UnitRegistry.RegisterCustomID(this.UniqueName, this);
    this.RegisterOnGridAsync(updateInterval).Forget();
  }

  private async UniTaskVoid RegisterOnGridAsync(float? updateInterval)
  {
    Unit unit = this;
    while ((UnityEngine.Object) NetworkSceneSingleton<LevelInfo>.i == (UnityEngine.Object) null)
      await UniTask.Yield();
    if (updateInterval.HasValue)
      unit.StartSlowUpdate(updateInterval.Value, new Action(unit.UpdateGridNow));
    else
      unit.UpdateGridNow();
    UnitRegistry.RegisterUnit(unit, unit.persistentID);
  }

  public virtual void InitializeUnit()
  {
    Action onInitialize = this.onInitialize;
    if (onInitialize == null)
      return;
    onInitialize();
  }

  public virtual void SetLocalSim(bool localSim) => this.remoteSim = !localSim;

  public virtual List<UnitPart> GetAllParts() => this.partLookup;

  public virtual Transform GetRandomPart()
  {
    List<UnitPart> unitPartList = new List<UnitPart>();
    foreach (UnitPart unitPart in this.partLookup)
    {
      if ((UnityEngine.Object) unitPart != (UnityEngine.Object) null && !unitPart.IsDetached())
        unitPartList.Add(unitPart);
    }
    if (unitPartList.Count == 0)
      return this.transform;
    int index = Mathf.FloorToInt(UnityEngine.Random.Range(0.0f, (float) unitPartList.Count - 1E-05f));
    return unitPartList[index].transform;
  }

  public byte RegisterDamageable(IDamageable damageable)
  {
    int count = this.damageables.Count;
    this.damageables.Add(new DamageablePart(damageable));
    return (byte) count;
  }

  public void DeregisterDamageable(int index)
  {
    this.damageables[index] = this.damageables[index].CreateRemoved();
  }

  public void AddIRSource(IRSource source)
  {
    this.IRSources.Add(source);
    Action<IRSource> onAddIrSource = this.onAddIRSource;
    if (onAddIrSource == null)
      return;
    onAddIrSource(source);
  }

  public void RemoveIRSource(IRSource source) => this.IRSources.Remove(source);

  public IRSource GetIRSource()
  {
    return this.IRSources.Count == 0 ? (IRSource) null : this.IRSources[Mathf.FloorToInt(UnityEngine.Random.Range(0.0f, (float) this.IRSources.Count - 0.0001f))];
  }

  public void RegisterMissile(Missile missile)
  {
    Action<Missile> onRegisterMissile = this.onRegisterMissile;
    if (onRegisterMissile == null)
      return;
    onRegisterMissile(missile);
  }

  public void DeregisterMissile(Missile missile)
  {
    Action<Missile> deregisterMissile = this.onDeregisterMissile;
    if (deregisterMissile == null)
      return;
    deregisterMissile(missile);
  }

  public virtual void HQChanged(FactionHQ oldHQ, FactionHQ newHQ)
  {
    PersistentUnit persistentUnit;
    if (UnitRegistry.persistentUnitLookup.TryGetValue(this.persistentID, out persistentUnit))
      persistentUnit.SetHQ(newHQ);
    if (this.disabled || this.unitState != Unit.UnitState.Active || (UnityEngine.Object) newHQ == (UnityEngine.Object) null)
      return;
    Action<Unit> onChangeFaction = this.onChangeFaction;
    if (onChangeFaction != null)
      onChangeFaction(this);
    if (!NetworkManagerNuclearOption.i.Server.Active)
      return;
    this.NetworkHQ.factionUnits.Add(this.persistentID);
    if (!((UnityEngine.Object) oldHQ != (UnityEngine.Object) null))
      return;
    oldHQ.RemoveFactionUnit(this);
  }

  public void RegisterWeaponStation(WeaponStation weaponStation)
  {
    this.weaponStations.Add(weaponStation);
  }

  public void ClearWeaponStations() => this.weaponStations.Clear();

  public float GetMaxRange()
  {
    if (this.weaponStations.Count == 0)
      return 0.0f;
    float a = 0.0f;
    foreach (WeaponStation weaponStation in this.weaponStations)
      a = Mathf.Max(a, weaponStation.WeaponInfo.targetRequirements.maxRange);
    return a;
  }

  public virtual void UnitDisabled(bool _, bool nowDisabled)
  {
    if (!nowDisabled)
      return;
    this.RemoveFromGrid();
    Action<Unit> onDisableUnit = this.onDisableUnit;
    if (onDisableUnit != null)
      onDisableUnit(this);
    if (!NetworkManagerNuclearOption.i.Server.Active || !((UnityEngine.Object) this.NetworkHQ != (UnityEngine.Object) null))
      return;
    this.NetworkHQ.RemoveFactionUnit(this);
  }

  public virtual void AttachOrDetachSlingHook(Aircraft aircraft, bool attached)
  {
    this.rb.interpolation = RigidbodyInterpolation.Interpolate;
    this.rb.drag = 0.02f;
    this.rb.angularDrag = 0.1f;
    this.remoteSim = !(aircraft.LocalSim & attached) && !this.IsServer;
    this.rb.useGravity = !this.remoteSim;
  }

  public virtual void ChangeUnitState(Unit.UnitState newState) => this.NetworkunitState = newState;

  public virtual bool IsSlung() => false;

  private void RemoveFromGrid()
  {
    this.gridSquare?.units.Remove(this);
    this.gridSquare = (GridSquare) null;
  }

  private void UpdateGridNow()
  {
    if (this.disabled || this.unitState != Unit.UnitState.Active)
      return;
    BattlefieldGrid.UpdateUnit(this, ref this.gridSquare);
  }

  public void ModifyVisibility(float visibility) => this.visibility += visibility;

  public float GetVisibility() => this.visibility;

  public bool LineOfSight(Vector3 origin, float magnification)
  {
    if (FastMath.OutOfRange(origin, this.transform.position, this.visibility * magnification))
      return false;
    return !Physics.Linecast(origin, this.transform.position, out this.hit, 64 /*0x40*/) || FastMath.InRange(this.hit.point, this.transform.position, this.maxRadius * 2f);
  }

  public void Jam(Unit.JamEventArgs args) => this.RpcJam(args);

  [ClientRpc]
  public void RpcJam(Unit.JamEventArgs args)
  {
    if (ClientRpcSender.ShouldInvokeLocally((NetworkBehaviour) this, RpcTarget.Observers, (INetworkPlayer) null, false))
      this.UserCode_RpcJam_569024588(args);
    PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
    GeneratedNetworkCode._Write_Unit\u002FJamEventArgs((NetworkWriter) writer, args);
    ClientRpcSender.Send((NetworkBehaviour) this, 0, (NetworkWriter) writer, Mirage.Channel.Reliable, false);
    writer.Release();
  }

  [ClientRpc]
  public void RpcAssignRadar(Unit unit)
  {
    if (ClientRpcSender.ShouldInvokeLocally((NetworkBehaviour) this, RpcTarget.Observers, (INetworkPlayer) null, false))
      this.UserCode_RpcAssignRadar_\u002D1001571913(unit);
    PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
    GeneratedNetworkCode._Write_Unit((NetworkWriter) writer, unit);
    ClientRpcSender.Send((NetworkBehaviour) this, 1, (NetworkWriter) writer, Mirage.Channel.Reliable, false);
    writer.Release();
  }

  public void SetRB(Rigidbody rb) => this.rb = rb;

  public void SetFiringState(int index, bool firing)
  {
    if (this.remoteSim || this.localWeaponStates.Get(index) == firing)
      return;
    this.localWeaponStates = WeaponMask.Set(this.localWeaponStates, index, firing);
    if (firing)
      this.TrackFiringState(index).Forget();
    if (this.IsServer)
    {
      this.NetworkremoteWeaponStates = this.localWeaponStates;
      if (firing)
        return;
      this.RpcSyncAmmoCount((byte) index, this.weaponStations[index].Ammo);
    }
    else
    {
      this.CmdSetFiringState(this.localWeaponStates);
      if (firing)
        return;
      this.CmdStoppedFiring((byte) index);
    }
  }

  private async UniTask TrackFiringState(int stationNumber)
  {
    Unit unit = this;
    WeaponStation station;
    CancellationToken cancel;
    if (stationNumber < 0)
    {
      ColorLog<Unit>.LogError("stationNumber was negative");
      station = (WeaponStation) null;
      cancel = new CancellationToken();
    }
    else if (stationNumber >= unit.weaponStations.Count)
    {
      ColorLog<Unit>.LogError($"stationNumber was {stationNumber}, but count is {unit.weaponStations.Count}");
      station = (WeaponStation) null;
      cancel = new CancellationToken();
    }
    else
    {
      station = unit.weaponStations[stationNumber];
      cancel = unit.destroyCancellationToken;
      while ((double) Time.timeSinceLevelLoad - (double) station.LastFiredTime <= 0.20000000298023224)
      {
        await UniTask.Yield();
        if (cancel.IsCancellationRequested)
        {
          station = (WeaponStation) null;
          cancel = new CancellationToken();
          return;
        }
      }
      unit.SetFiringState(stationNumber, false);
      station = (WeaponStation) null;
      cancel = new CancellationToken();
    }
  }

  [ServerRpc]
  private void CmdSetFiringState(WeaponMask weaponMask)
  {
    if (ServerRpcSender.ShouldInvokeLocally((NetworkBehaviour) this, true, false))
    {
      this.UserCode_CmdSetFiringState_1458755207(weaponMask);
    }
    else
    {
      PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
      GeneratedNetworkCode._Write_WeaponMask((NetworkWriter) writer, weaponMask);
      ServerRpcSender.Send((NetworkBehaviour) this, 2, (NetworkWriter) writer, Mirage.Channel.Reliable, true);
      writer.Release();
    }
  }

  private void OnFiringStateUpdated(WeaponMask oldMask, WeaponMask newMask)
  {
    if (this.LocalSim || newMask.Mask == 0 || this.isRemoteFiring)
      return;
    this.RemoteFireWeaponStation().Forget();
  }

  private async UniTask RemoteFireWeaponStation()
  {
    Unit owner = this;
    owner.isRemoteFiring = true;
    YieldAwaitable yieldAwaitable = UniTask.Yield();
    await yieldAwaitable;
    CancellationToken cancel = owner.destroyCancellationToken;
    while (!cancel.IsCancellationRequested)
    {
      if (owner.remoteWeaponStates.Mask == 0)
      {
        owner.isRemoteFiring = false;
        cancel = new CancellationToken();
        return;
      }
      for (int index = 0; index < owner.weaponStations.Count; ++index)
      {
        if (owner.remoteWeaponStates.Get(index))
          owner.weaponStations[index].RemoteFireAuto(owner);
      }
      yieldAwaitable = UniTask.Yield();
      await yieldAwaitable;
    }
    cancel = new CancellationToken();
  }

  public void SingleRemoteFire(byte stationIndex, int ammo)
  {
    if (this.IsServer)
    {
      this.RpcSingleRemoteFire(stationIndex, ammo);
    }
    else
    {
      if (!this.HasAuthority)
        return;
      this.CmdSingleRemoteFire(stationIndex);
    }
  }

  [ServerRpc]
  private void CmdSingleRemoteFire(byte stationIndex)
  {
    if (ServerRpcSender.ShouldInvokeLocally((NetworkBehaviour) this, true, false))
    {
      this.UserCode_CmdSingleRemoteFire_1708169300(stationIndex);
    }
    else
    {
      PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
      writer.WriteByteExtension(stationIndex);
      ServerRpcSender.Send((NetworkBehaviour) this, 3, (NetworkWriter) writer, Mirage.Channel.Reliable, true);
      writer.Release();
    }
  }

  [ClientRpc]
  public void RpcSingleRemoteFire(byte stationIndex, int ammo)
  {
    if (ClientRpcSender.ShouldInvokeLocally((NetworkBehaviour) this, RpcTarget.Observers, (INetworkPlayer) null, false))
      this.UserCode_RpcSingleRemoteFire_\u002D1161895954(stationIndex, ammo);
    PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
    writer.WriteByteExtension(stationIndex);
    writer.WritePackedInt32(ammo);
    ClientRpcSender.Send((NetworkBehaviour) this, 4, (NetworkWriter) writer, Mirage.Channel.Reliable, false);
    writer.Release();
  }

  [ServerRpc]
  private void CmdStoppedFiring(byte stationIndex)
  {
    if (ServerRpcSender.ShouldInvokeLocally((NetworkBehaviour) this, true, false))
    {
      this.UserCode_CmdStoppedFiring_\u002D1238964004(stationIndex);
    }
    else
    {
      PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
      writer.WriteByteExtension(stationIndex);
      ServerRpcSender.Send((NetworkBehaviour) this, 5, (NetworkWriter) writer, Mirage.Channel.Reliable, true);
      writer.Release();
    }
  }

  [ClientRpc(excludeHost = true)]
  public void RpcSyncAmmoCount(byte stationIndex, int ammo)
  {
    PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
    writer.WriteByteExtension(stationIndex);
    writer.WritePackedInt32(ammo);
    ClientRpcSender.Send((NetworkBehaviour) this, 6, (NetworkWriter) writer, Mirage.Channel.Reliable, false);
    writer.Release();
  }

  [ServerRpc]
  public void CmdSetStationTargets(byte stationIndex, ReadOnlySpan<PersistentID> targetIDs)
  {
    if (ServerRpcSender.ShouldInvokeLocally((NetworkBehaviour) this, true, false))
    {
      this.UserCode_CmdSetStationTargets_872088460(stationIndex, targetIDs);
    }
    else
    {
      PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
      writer.WriteByteExtension(stationIndex);
      GeneratedNetworkCode._Write_System\u002EReadOnlySpan\u00601\u003CPersistentID\u003E((NetworkWriter) writer, targetIDs);
      ServerRpcSender.Send((NetworkBehaviour) this, 7, (NetworkWriter) writer, Mirage.Channel.Reliable, true);
      writer.Release();
    }
  }

  [ClientRpc]
  public virtual void RpcSetStationTargets(byte stationIndex, ReadOnlySpan<PersistentID> targetIDs)
  {
    if (ClientRpcSender.ShouldInvokeLocally((NetworkBehaviour) this, RpcTarget.Observers, (INetworkPlayer) null, false))
      this.UserCode_RpcSetStationTargets_1363862903(stationIndex, targetIDs);
    PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
    writer.WriteByteExtension(stationIndex);
    GeneratedNetworkCode._Write_System\u002EReadOnlySpan\u00601\u003CPersistentID\u003E((NetworkWriter) writer, targetIDs);
    ClientRpcSender.Send((NetworkBehaviour) this, 8, (NetworkWriter) writer, Mirage.Channel.Reliable, false);
    writer.Release();
  }

  public bool CheckIsTarget(Unit candidate)
  {
    if (this.weaponStations.Count == 0)
      return false;
    foreach (WeaponStation weaponStation in this.weaponStations)
    {
      Unit stationTarget = weaponStation.GetStationTarget();
      if ((UnityEngine.Object) candidate == (UnityEngine.Object) stationTarget)
        return true;
    }
    return false;
  }

  public void RecordDamage(PersistentID lastDamagedBy, float damageAmount)
  {
    if (this.damageCredit == null)
      this.damageCredit = new Dictionary<PersistentID, float>();
    float num;
    this.damageCredit.TryGetValue(lastDamagedBy, out num);
    this.damageCredit[lastDamagedBy] = num + damageAmount;
  }

  public void RegisterHit(
    Unit hitUnit,
    Vector3 relativePos,
    Vector3 bulletVelocity,
    WeaponInfo weaponInfo)
  {
    if (this.IsServer)
    {
      Vector3 position = hitUnit.transform.TransformPoint(relativePos);
      if (PlayerSettings.debugVis)
      {
        GameObject gameObject = NetworkSceneSingleton<Spawner>.i.SpawnLocal(GameAssets.i.debugArrow, hitUnit.transform);
        gameObject.GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", new Color(1f, 1f, 1f, 1f));
        gameObject.transform.position = position - bulletVelocity.normalized;
        gameObject.transform.rotation = Quaternion.LookRotation(bulletVelocity);
        gameObject.transform.localScale = new Vector3(0.5f, 0.5f, 1f);
        NetworkSceneSingleton<Spawner>.i.DestroyLocal(gameObject, 10f);
      }
      DamageEffects.ArmorPenetrate(position, bulletVelocity, weaponInfo.muzzleVelocity, weaponInfo.pierceDamage, weaponInfo.blastDamage, this.persistentID);
    }
    else
    {
      if (!this.HasAuthority)
        return;
      byte? nullable = new byte?();
      for (int index = 0; index < this.weaponStations.Count; ++index)
      {
        if ((UnityEngine.Object) this.weaponStations[index].WeaponInfo == (UnityEngine.Object) weaponInfo)
        {
          nullable = new byte?(checked ((byte) index));
          break;
        }
      }
      if (!nullable.HasValue)
        Debug.LogError((object) $"Could not find weaponStations for {weaponInfo}");
      else
        this.CmdClaimHit(hitUnit.persistentID, NetworkFloatHelper.CompressIfValid(relativePos, true, nameof (relativePos)), NetworkFloatHelper.CompressIfValid(bulletVelocity, true, nameof (bulletVelocity)), nullable.Value);
    }
  }

  [ServerRpc]
  private void CmdClaimHit(
    PersistentID hitID,
    Vector3Compressed relativePosCompressed,
    Vector3Compressed bulletVelocityCompressed,
    byte weaponStationIndex)
  {
    if (ServerRpcSender.ShouldInvokeLocally((NetworkBehaviour) this, true, false))
    {
      this.UserCode_CmdClaimHit_\u002D1122942669(hitID, relativePosCompressed, bulletVelocityCompressed, weaponStationIndex);
    }
    else
    {
      PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
      GeneratedNetworkCode._Write_PersistentID((NetworkWriter) writer, hitID);
      GeneratedNetworkCode._Write_Vector3Compressed((NetworkWriter) writer, relativePosCompressed);
      GeneratedNetworkCode._Write_Vector3Compressed((NetworkWriter) writer, bulletVelocityCompressed);
      writer.WriteByteExtension(weaponStationIndex);
      ServerRpcSender.Send((NetworkBehaviour) this, 9, (NetworkWriter) writer, Mirage.Channel.Reliable, true);
      writer.Release();
    }
  }

  private async UniTask HitOnPhysicsFrame(
    Unit hitUnit,
    Vector3 relativePos,
    Vector3 hitVelocity,
    byte weaponStationIndex)
  {
    Unit claimer = this;
    await UniTask.WaitForFixedUpdate();
    Vector3 position = hitUnit.transform.TransformPoint(relativePos);
    if (!HitValidator.HitValidated(claimer, position - Datum.origin.position, hitVelocity))
      return;
    WeaponInfo weaponInfo = claimer.weaponStations[(int) weaponStationIndex].WeaponInfo;
    DamageEffects.ArmorPenetrate(position, Vector3.ClampMagnitude(hitVelocity, weaponInfo.muzzleVelocity * 1.5f), weaponInfo.muzzleVelocity, weaponInfo.pierceDamage, weaponInfo.blastDamage, claimer.persistentID);
  }

  public void Damage(byte index, DamageInfo damageInfo)
  {
    if (this.IsServer)
    {
      this.RpcDamage(index, damageInfo);
    }
    else
    {
      if (!this.HasAuthority)
        return;
      this.CmdDamage(index, damageInfo);
    }
  }

  [ServerRpc]
  private void CmdDamage(byte partID, DamageInfo damageInfo)
  {
    if (ServerRpcSender.ShouldInvokeLocally((NetworkBehaviour) this, true, false))
    {
      this.UserCode_CmdDamage_\u002D2104360672(partID, damageInfo);
    }
    else
    {
      PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
      writer.WriteByteExtension(partID);
      GeneratedNetworkCode._Write_DamageInfo((NetworkWriter) writer, damageInfo);
      ServerRpcSender.Send((NetworkBehaviour) this, 10, (NetworkWriter) writer, Mirage.Channel.Reliable, true);
      writer.Release();
    }
  }

  [ClientRpc]
  public virtual void RpcDamage(byte index, DamageInfo damageInfo)
  {
    if (ClientRpcSender.ShouldInvokeLocally((NetworkBehaviour) this, RpcTarget.Observers, (INetworkPlayer) null, false))
      this.UserCode_RpcDamage_1709046923(index, damageInfo);
    PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
    writer.WriteByteExtension(index);
    GeneratedNetworkCode._Write_DamageInfo((NetworkWriter) writer, damageInfo);
    ClientRpcSender.Send((NetworkBehaviour) this, 11, (NetworkWriter) writer, Mirage.Channel.Reliable, false);
    writer.Release();
  }

  public void FuelTankStatus(byte partID, bool ruptured, bool onFire)
  {
    if (this.IsServer)
    {
      this.RpcFuelTankStatus(partID, ruptured, onFire);
    }
    else
    {
      if (!this.HasAuthority)
        return;
      this.CmdFuelTankStatus(partID, ruptured, onFire);
    }
  }

  [ServerRpc]
  private void CmdFuelTankStatus(byte partID, bool ruptured, bool onFire)
  {
    if (ServerRpcSender.ShouldInvokeLocally((NetworkBehaviour) this, true, false))
    {
      this.UserCode_CmdFuelTankStatus_423953574(partID, ruptured, onFire);
    }
    else
    {
      PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
      writer.WriteByteExtension(partID);
      writer.WriteBooleanExtension(ruptured);
      writer.WriteBooleanExtension(onFire);
      ServerRpcSender.Send((NetworkBehaviour) this, 12, (NetworkWriter) writer, Mirage.Channel.Reliable, true);
      writer.Release();
    }
  }

  [ClientRpc]
  public void RpcFuelTankStatus(byte partID, bool ruptured, bool onFire)
  {
    if (ClientRpcSender.ShouldInvokeLocally((NetworkBehaviour) this, RpcTarget.Observers, (INetworkPlayer) null, false))
      this.UserCode_RpcFuelTankStatus_414334555(partID, ruptured, onFire);
    PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
    writer.WriteByteExtension(partID);
    writer.WriteBooleanExtension(ruptured);
    writer.WriteBooleanExtension(onFire);
    ClientRpcSender.Send((NetworkBehaviour) this, 13, (NetworkWriter) writer, Mirage.Channel.Reliable, false);
    writer.Release();
  }

  public void DetachPart(byte partID, Vector3 velocity, Vector3 relativePos)
  {
    if (this.IsServer)
    {
      this.RpcBreakPart(partID, NetworkFloatHelper.CompressIfValid(velocity, false, (string) null), NetworkFloatHelper.CompressIfValid(relativePos, false, (string) null));
    }
    else
    {
      if (!this.HasAuthority)
        return;
      this.CmdBreakPart(partID, NetworkFloatHelper.CompressIfValid(velocity, true, nameof (velocity)), NetworkFloatHelper.CompressIfValid(relativePos, true, nameof (relativePos)));
    }
  }

  [ServerRpc]
  private void CmdBreakPart(byte partID, Vector3Compressed velocity, Vector3Compressed relativePos)
  {
    if (ServerRpcSender.ShouldInvokeLocally((NetworkBehaviour) this, true, false))
    {
      this.UserCode_CmdBreakPart_\u002D789722400(partID, velocity, relativePos);
    }
    else
    {
      PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
      writer.WriteByteExtension(partID);
      GeneratedNetworkCode._Write_Vector3Compressed((NetworkWriter) writer, velocity);
      GeneratedNetworkCode._Write_Vector3Compressed((NetworkWriter) writer, relativePos);
      ServerRpcSender.Send((NetworkBehaviour) this, 14, (NetworkWriter) writer, Mirage.Channel.Reliable, true);
      writer.Release();
    }
  }

  [ClientRpc]
  private void RpcBreakPart(
    byte partID,
    Vector3Compressed velocityCompressed,
    Vector3Compressed relativePosCompressed)
  {
    if (ClientRpcSender.ShouldInvokeLocally((NetworkBehaviour) this, RpcTarget.Observers, (INetworkPlayer) null, false))
      this.UserCode_RpcBreakPart_\u002D1087911989(partID, velocityCompressed, relativePosCompressed);
    PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
    writer.WriteByteExtension(partID);
    GeneratedNetworkCode._Write_Vector3Compressed((NetworkWriter) writer, velocityCompressed);
    GeneratedNetworkCode._Write_Vector3Compressed((NetworkWriter) writer, relativePosCompressed);
    ClientRpcSender.Send((NetworkBehaviour) this, 15, (NetworkWriter) writer, Mirage.Channel.Reliable, false);
    writer.Release();
  }

  public void DisableUnit()
  {
    if (this.IsServer)
    {
      this.ServerDisableUnit();
    }
    else
    {
      if (!this.HasAuthority)
        return;
      this.CmdDisableUnit();
    }
  }

  [ServerRpc]
  public void CmdDisableUnit()
  {
    if (ServerRpcSender.ShouldInvokeLocally((NetworkBehaviour) this, true, false))
    {
      this.UserCode_CmdDisableUnit_\u002D340399461();
    }
    else
    {
      PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
      ServerRpcSender.Send((NetworkBehaviour) this, 16 /*0x10*/, (NetworkWriter) writer, Mirage.Channel.Reliable, true);
      writer.Release();
    }
  }

  [Mirage.Server]
  protected virtual void ServerDisableUnit()
  {
    if (!this.IsServer)
      throw new MethodInvocationException("[Server] function 'ServerDisableUnit' called when server not active");
    if (this.disabled)
      return;
    this.Networkdisabled = true;
  }

  public virtual void ReportKilled()
  {
    PersistentID killerID = PersistentID.None;
    PersistentUnit persistentUnit1;
    if (!UnitRegistry.TryGetPersistentUnit(this.persistentID, out persistentUnit1))
      return;
    FactionHQ hq1 = persistentUnit1.GetHQ();
    float num1 = 0.0f;
    float num2 = 0.0f;
    if (this.damageCredit != null)
    {
      foreach (KeyValuePair<PersistentID, float> keyValuePair in this.damageCredit)
        num2 += keyValuePair.Value;
      Dictionary<Player, float> dictionary = new Dictionary<Player, float>();
      foreach (KeyValuePair<PersistentID, float> keyValuePair in this.damageCredit)
      {
        PersistentUnit persistentUnit2;
        if (UnitRegistry.TryGetPersistentUnit(keyValuePair.Key, out persistentUnit2))
        {
          float num3 = keyValuePair.Value / num2;
          if ((double) num3 >= 0.0099999997764825821)
          {
            if ((double) keyValuePair.Value >= (double) num1)
            {
              num1 = keyValuePair.Value;
              killerID = keyValuePair.Key;
            }
            FactionHQ hq2 = persistentUnit2.GetHQ();
            if (!((UnityEngine.Object) hq1 == (UnityEngine.Object) hq2) && !((UnityEngine.Object) hq1 == (UnityEngine.Object) null))
            {
              float score = Mathf.Sqrt(persistentUnit1.definition.value) * num3;
              float num4 = score * hq2.killReward;
              hq2.AddScore(score);
              hq2.AddFunds(num4 * hq2.playerTaxRate);
              if ((UnityEngine.Object) persistentUnit2.player != (UnityEngine.Object) null)
              {
                if (!dictionary.ContainsKey(persistentUnit2.player))
                  dictionary.Add(persistentUnit2.player, 0.0f);
                dictionary[persistentUnit2.player] += num3;
              }
            }
          }
        }
      }
      foreach (KeyValuePair<Player, float> keyValuePair in dictionary)
        keyValuePair.Key.HQ.ReportKillAction(keyValuePair.Key, this, keyValuePair.Value);
    }
    Unit unit1;
    if (persistentUnit1.unit is Aircraft unit2 && (UnityEngine.Object) unit2.Player != (UnityEngine.Object) null && (double) num2 > 1.0 && UnitRegistry.TryGetUnit(new PersistentID?(killerID), out unit1) && (UnityEngine.Object) unit1.NetworkHQ == (UnityEngine.Object) unit2.NetworkHQ && unit1 is Aircraft aircraft && (UnityEngine.Object) aircraft.Player != (UnityEngine.Object) null)
    {
      float amount = unit2.definition.value + unit2.weaponManager.GetCurrentValue(true);
      aircraft.Player.AddScore(-Mathf.Sqrt(unit2.definition.value));
      aircraft.Player.AddAllocation(-amount);
      unit2.Player.AddAllocation(amount);
    }
    KillType killedType = KillType.Vehicle;
    switch (this)
    {
      case Missile _:
        killedType = KillType.Missile;
        break;
      case Building _:
        killedType = KillType.Building;
        break;
      case Aircraft _:
        killedType = KillType.Aircraft;
        break;
      case Ship _:
        killedType = KillType.Ship;
        break;
    }
    if (!((UnityEngine.Object) NetworkSceneSingleton<MessageManager>.i != (UnityEngine.Object) null))
      return;
    NetworkSceneSingleton<MessageManager>.i.RpcKillMessage(killerID, this.persistentID, killedType);
  }

  protected virtual void OnDestroy()
  {
    if ((UnityEngine.Object) NetworkManagerNuclearOption.i != (UnityEngine.Object) null && (NetworkManagerNuclearOption.i.Client.Active || NetworkManagerNuclearOption.i.Server.Active) && GameManager.gameState != GameState.Encyclopedia && !this.disabled)
    {
      Action<Unit> onDisableUnit = this.onDisableUnit;
      if (onDisableUnit != null)
        onDisableUnit(this);
    }
    UnitRegistry.UnregisterUnit(this);
    this.RemoveFromGrid();
    this.Networkdisabled = true;
    this.NetworkunitState = Unit.UnitState.Destroyed;
    this.onInitialize = (Action) null;
    this.onDisableUnit = (Action<Unit>) null;
    this.onChangeFaction = (Action<Unit>) null;
    this.onAddIRSource = (Action<IRSource>) null;
    this.onRegisterMissile = (Action<Missile>) null;
    this.onDeregisterMissile = (Action<Missile>) null;
    this.onJam = (Action<Unit.JamEventArgs>) null;
  }

  public int[] GetAmmoByType(List<WeaponInfo> listTypes)
  {
    int[] ammoByType = new int[2];
    for (int index = 0; index < this.weaponStations.Count; ++index)
    {
      if (listTypes.Contains(this.weaponStations[index].WeaponInfo))
      {
        ammoByType[0] += this.weaponStations[index].Ammo;
        ammoByType[1] += this.weaponStations[index].FullAmmo;
      }
    }
    return ammoByType;
  }

  public virtual void CheckRadarAlt()
  {
    if ((double) Time.timeSinceLevelLoad <= (double) this.lastAltitudeCheck + 0.10000000149011612)
      return;
    this.lastAltitudeCheck = Time.timeSinceLevelLoad;
    this.radarAlt = !Physics.Linecast(this.transform.position, this.transform.position - Vector3.up * 10000f, out this.hit, 2112) ? this.transform.position.GlobalY() : this.hit.distance;
    this.radarAlt -= this.definition.spawnOffset.y;
    this.radarAlt = Mathf.Clamp(this.radarAlt, 0.0f, this.transform.position.GlobalY() - this.definition.spawnOffset.y);
  }

  public float GetAmmoLevel()
  {
    int num1 = 0;
    int num2 = 0;
    float ammoLevel = 0.0f;
    for (int index = 0; index < this.weaponStations.Count; ++index)
    {
      num1 += this.weaponStations[index].Ammo;
      num2 += this.weaponStations[index].FullAmmo;
    }
    if (num2 > 0)
      ammoLevel = (float) (num1 / num2);
    return ammoLevel;
  }

  public AmmoValue GetAmmoValue()
  {
    float currentValue = 0.0f;
    float totalValue = 0.0f;
    for (int index = 0; index < this.weaponStations.Count; ++index)
    {
      currentValue += (float) this.weaponStations[index].Ammo * this.weaponStations[index].WeaponInfo.costPerRound;
      totalValue += (float) this.weaponStations[index].FullAmmo * this.weaponStations[index].WeaponInfo.costPerRound;
    }
    return new AmmoValue(currentValue, totalValue);
  }

  public async UniTask CmdAskAmmo()
  {
    int[] ammoValues = await this.CmdAskAmmoInternal();
    int[] numArray = await this.CmdAskFullAmmoInternal();
    for (int index = 0; index < this.weaponStations.Count; ++index)
    {
      this.weaponStations[index].Ammo = ammoValues[index];
      this.weaponStations[index].FullAmmo = numArray[index];
    }
    ammoValues = (int[]) null;
  }

  [ServerRpc(requireAuthority = false)]
  private UniTask<int[]> CmdAskAmmoInternal()
  {
    if (ServerRpcSender.ShouldInvokeLocally((NetworkBehaviour) this, false, false))
      return this.UserCode_CmdAskAmmoInternal_1214831455();
    PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
    UniTask<int[]> uniTask = ServerRpcSender.SendWithReturn<int[]>((NetworkBehaviour) this, 17, (NetworkWriter) writer, false);
    writer.Release();
    return uniTask;
  }

  [ServerRpc(requireAuthority = false)]
  private UniTask<int[]> CmdAskFullAmmoInternal()
  {
    if (ServerRpcSender.ShouldInvokeLocally((NetworkBehaviour) this, false, false))
      return this.UserCode_CmdAskFullAmmoInternal_\u002D1739804082();
    PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
    UniTask<int[]> uniTask = ServerRpcSender.SendWithReturn<int[]>((NetworkBehaviour) this, 18, (NetworkWriter) writer, false);
    writer.Release();
    return uniTask;
  }

  public override string ToString() => $"({base.ToString()}, {this.persistentID})";

  SingleSelectionDetails IEditorSelectable.CreateSelectionDetails()
  {
    return (SingleSelectionDetails) new UnitSelectionDetails(this);
  }

  private void MirageProcessed()
  {
  }

  public PersistentID NetworkpersistentID
  {
    get => this.persistentID;
    set => this.persistentID = value;
  }

  public string NetworkunitName
  {
    get => this.unitName;
    set => this.unitName = value;
  }

  public string NetworkUniqueName
  {
    get => this.UniqueName;
    set => this.UniqueName = value;
  }

  public GlobalPosition NetworkstartPosition
  {
    get => this.startPosition;
    set
    {
      if (this.SyncVarEqual<GlobalPosition>(value, this.startPosition))
        return;
      GlobalPosition startPosition = this.startPosition;
      this.startPosition = value;
      this.SetDirtyBit(8UL);
    }
  }

  public Quaternion NetworkstartRotation
  {
    get => this.startRotation;
    set
    {
      if (this.SyncVarEqual<Quaternion>(value, this.startRotation))
        return;
      Quaternion startRotation = this.startRotation;
      this.startRotation = value;
      this.SetDirtyBit(16UL /*0x10*/);
    }
  }

  public FactionHQ NetworkHQ
  {
    get => (FactionHQ) this.HQ.Value;
    set
    {
      if (this.SyncVarEqual<FactionHQ>(value, (FactionHQ) this.HQ.Value))
        return;
      FactionHQ oldHQ = (FactionHQ) this.HQ.Value;
      this.HQ.Value = (NetworkBehaviour) value;
      this.SetDirtyBit(32UL /*0x20*/);
      if (!this.GetSyncVarHookGuard(32UL /*0x20*/) && this.IsHost)
      {
        this.SetSyncVarHookGuard(32UL /*0x20*/, true);
        this.HQChanged(oldHQ, value);
        this.SetSyncVarHookGuard(32UL /*0x20*/, false);
      }
    }
  }

  public bool Networkdisabled
  {
    get => this.disabled;
    set
    {
      if (this.SyncVarEqual<bool>(value, this.disabled))
        return;
      bool disabled = this.disabled;
      this.disabled = value;
      this.SetDirtyBit(64UL /*0x40*/);
      if (!this.GetSyncVarHookGuard(64UL /*0x40*/) && this.IsHost)
      {
        this.SetSyncVarHookGuard(64UL /*0x40*/, true);
        this.UnitDisabled(disabled, value);
        this.SetSyncVarHookGuard(64UL /*0x40*/, false);
      }
    }
  }

  public Unit.UnitState NetworkunitState
  {
    get => this.unitState;
    set
    {
      if (this.SyncVarEqual<Unit.UnitState>(value, this.unitState))
        return;
      Unit.UnitState unitState = this.unitState;
      this.unitState = value;
      this.SetDirtyBit(128UL /*0x80*/);
    }
  }

  public WeaponMask NetworkremoteWeaponStates
  {
    get => this.remoteWeaponStates;
    set
    {
      if (this.SyncVarEqual<WeaponMask>(value, this.remoteWeaponStates))
        return;
      WeaponMask remoteWeaponStates = this.remoteWeaponStates;
      this.remoteWeaponStates = value;
      this.SetDirtyBit(256UL /*0x0100*/);
      if (!this.GetSyncVarHookGuard(256UL /*0x0100*/) && this.IsHost)
      {
        this.SetSyncVarHookGuard(256UL /*0x0100*/, true);
        this.OnFiringStateUpdated(remoteWeaponStates, value);
        this.SetSyncVarHookGuard(256UL /*0x0100*/, false);
      }
    }
  }

  public override bool SerializeSyncVars(NetworkWriter writer, bool initialize)
  {
    ulong syncVarDirtyBits = this.SyncVarDirtyBits;
    bool flag = base.SerializeSyncVars(writer, initialize);
    if (initialize)
    {
      GeneratedNetworkCode._Write_PersistentID(writer, this.persistentID);
      writer.WriteString(this.unitName);
      writer.WriteString(this.UniqueName);
      writer.WriteGlobalPosition(this.startPosition);
      writer.WriteQuaternion(this.startRotation);
      writer.WriteNetworkBehaviorSyncVar(this.HQ);
      writer.WriteBooleanExtension(this.disabled);
      GeneratedNetworkCode._Write_Unit\u002FUnitState(writer, this.unitState);
      GeneratedNetworkCode._Write_WeaponMask(writer, this.remoteWeaponStates);
      return true;
    }
    writer.Write(syncVarDirtyBits, 9);
    if (((long) syncVarDirtyBits & 8L) != 0L)
    {
      writer.WriteGlobalPosition(this.startPosition);
      flag = true;
    }
    if (((long) syncVarDirtyBits & 16L /*0x10*/) != 0L)
    {
      writer.WriteQuaternion(this.startRotation);
      flag = true;
    }
    if (((long) syncVarDirtyBits & 32L /*0x20*/) != 0L)
    {
      writer.WriteNetworkBehaviorSyncVar(this.HQ);
      flag = true;
    }
    if (((long) syncVarDirtyBits & 64L /*0x40*/) != 0L)
    {
      writer.WriteBooleanExtension(this.disabled);
      flag = true;
    }
    if (((long) syncVarDirtyBits & 128L /*0x80*/) != 0L)
    {
      GeneratedNetworkCode._Write_Unit\u002FUnitState(writer, this.unitState);
      flag = true;
    }
    if (((long) syncVarDirtyBits & 256L /*0x0100*/) != 0L)
    {
      GeneratedNetworkCode._Write_WeaponMask(writer, this.remoteWeaponStates);
      flag = true;
    }
    return flag;
  }

  public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
  {
    base.DeserializeSyncVars(reader, initialState);
    if (initialState)
    {
      this.persistentID = GeneratedNetworkCode._Read_PersistentID(reader);
      this.unitName = reader.ReadString();
      this.UniqueName = reader.ReadString();
      this.startPosition = reader.ReadGlobalPosition();
      this.startRotation = reader.ReadQuaternion();
      FactionHQ oldHQ = (FactionHQ) this.HQ.Value;
      this.HQ = reader.ReadNetworkBehaviourSyncVar();
      bool disabled = this.disabled;
      this.disabled = reader.ReadBooleanExtension();
      this.unitState = GeneratedNetworkCode._Read_Unit\u002FUnitState(reader);
      WeaponMask remoteWeaponStates = this.remoteWeaponStates;
      this.remoteWeaponStates = GeneratedNetworkCode._Read_WeaponMask(reader);
      if (!this.IsServer && !this.SyncVarEqual<FactionHQ>(oldHQ, (FactionHQ) this.HQ.Value))
        this.HQChanged(oldHQ, (FactionHQ) this.HQ.Value);
      if (!this.IsServer && !this.SyncVarEqual<bool>(disabled, this.disabled))
        this.UnitDisabled(disabled, this.disabled);
      if (this.IsServer || this.SyncVarEqual<WeaponMask>(remoteWeaponStates, this.remoteWeaponStates))
        return;
      this.OnFiringStateUpdated(remoteWeaponStates, this.remoteWeaponStates);
    }
    else
    {
      ulong dirtyBit = reader.Read(9);
      this.SetDeserializeMask(dirtyBit, 0);
      if (((long) dirtyBit & 8L) != 0L)
        this.startPosition = reader.ReadGlobalPosition();
      if (((long) dirtyBit & 16L /*0x10*/) != 0L)
        this.startRotation = reader.ReadQuaternion();
      if (((long) dirtyBit & 32L /*0x20*/) != 0L)
      {
        FactionHQ oldHQ = (FactionHQ) this.HQ.Value;
        this.HQ = reader.ReadNetworkBehaviourSyncVar();
        if (!this.IsServer && !this.SyncVarEqual<FactionHQ>(oldHQ, (FactionHQ) this.HQ.Value))
          this.HQChanged(oldHQ, (FactionHQ) this.HQ.Value);
      }
      if (((long) dirtyBit & 64L /*0x40*/) != 0L)
      {
        bool disabled = this.disabled;
        this.disabled = reader.ReadBooleanExtension();
        if (!this.IsServer && !this.SyncVarEqual<bool>(disabled, this.disabled))
          this.UnitDisabled(disabled, this.disabled);
      }
      if (((long) dirtyBit & 128L /*0x80*/) != 0L)
        this.unitState = GeneratedNetworkCode._Read_Unit\u002FUnitState(reader);
      if (((long) dirtyBit & 256L /*0x0100*/) == 0L)
        return;
      WeaponMask remoteWeaponStates = this.remoteWeaponStates;
      this.remoteWeaponStates = GeneratedNetworkCode._Read_WeaponMask(reader);
      if (!this.IsServer && !this.SyncVarEqual<WeaponMask>(remoteWeaponStates, this.remoteWeaponStates))
        this.OnFiringStateUpdated(remoteWeaponStates, this.remoteWeaponStates);
    }
  }

  public void UserCode_RpcJam_569024588(Unit.JamEventArgs args)
  {
    Action<Unit.JamEventArgs> onJam = this.onJam;
    if (onJam == null)
      return;
    onJam(args);
  }

  protected static void Skeleton_RpcJam_569024588(
    NetworkBehaviour behaviour,
    NetworkReader reader,
    INetworkPlayer senderConnection,
    int replyId)
  {
    ((Unit) behaviour).UserCode_RpcJam_569024588(GeneratedNetworkCode._Read_Unit\u002FJamEventArgs(reader));
  }

  public void UserCode_RpcAssignRadar_\u002D1001571913(Unit unit)
  {
    if (!((UnityEngine.Object) unit != (UnityEngine.Object) null) || !((UnityEngine.Object) unit.radar != (UnityEngine.Object) null))
      return;
    this.radar = unit.radar;
  }

  protected static void Skeleton_RpcAssignRadar_\u002D1001571913(
    NetworkBehaviour behaviour,
    NetworkReader reader,
    INetworkPlayer senderConnection,
    int replyId)
  {
    ((Unit) behaviour).UserCode_RpcAssignRadar_\u002D1001571913(GeneratedNetworkCode._Read_Unit(reader));
  }

  private void UserCode_CmdSetFiringState_1458755207(WeaponMask weaponMask)
  {
    this.NetworkremoteWeaponStates = weaponMask;
  }

  protected static void Skeleton_CmdSetFiringState_1458755207(
    NetworkBehaviour behaviour,
    NetworkReader reader,
    INetworkPlayer senderConnection,
    int replyId)
  {
    ((Unit) behaviour).UserCode_CmdSetFiringState_1458755207(GeneratedNetworkCode._Read_WeaponMask(reader));
  }

  private void UserCode_CmdSingleRemoteFire_1708169300(byte stationIndex)
  {
    if ((int) stationIndex >= this.weaponStations.Count)
      return;
    this.RpcSingleRemoteFire(stationIndex, this.weaponStations[(int) stationIndex].Ammo - 1);
  }

  protected static void Skeleton_CmdSingleRemoteFire_1708169300(
    NetworkBehaviour behaviour,
    NetworkReader reader,
    INetworkPlayer senderConnection,
    int replyId)
  {
    ((Unit) behaviour).UserCode_CmdSingleRemoteFire_1708169300(reader.ReadByteExtension());
  }

  public void UserCode_RpcSingleRemoteFire_\u002D1161895954(byte stationIndex, int ammo)
  {
    WeaponStation weaponStation = this.weaponStations[(int) stationIndex];
    weaponStation.Ammo = ammo;
    if (this.LocalSim)
      return;
    weaponStation.RemoteFireSingle(this);
    weaponStation.Updated();
  }

  protected static void Skeleton_RpcSingleRemoteFire_\u002D1161895954(
    NetworkBehaviour behaviour,
    NetworkReader reader,
    INetworkPlayer senderConnection,
    int replyId)
  {
    ((Unit) behaviour).UserCode_RpcSingleRemoteFire_\u002D1161895954(reader.ReadByteExtension(), reader.ReadPackedInt32());
  }

  private void UserCode_CmdStoppedFiring_\u002D1238964004(byte stationIndex)
  {
    WeaponStation weaponStation = this.weaponStations[(int) stationIndex];
    this.RpcSyncAmmoCount(stationIndex, weaponStation.Ammo);
  }

  protected static void Skeleton_CmdStoppedFiring_\u002D1238964004(
    NetworkBehaviour behaviour,
    NetworkReader reader,
    INetworkPlayer senderConnection,
    int replyId)
  {
    ((Unit) behaviour).UserCode_CmdStoppedFiring_\u002D1238964004(reader.ReadByteExtension());
  }

  public void UserCode_RpcSyncAmmoCount_\u002D1454761002(byte stationIndex, int ammo)
  {
    WeaponStation weaponStation = this.weaponStations[(int) stationIndex];
    weaponStation.Ammo = ammo;
    weaponStation.Updated();
  }

  protected static void Skeleton_RpcSyncAmmoCount_\u002D1454761002(
    NetworkBehaviour behaviour,
    NetworkReader reader,
    INetworkPlayer senderConnection,
    int replyId)
  {
    ((Unit) behaviour).UserCode_RpcSyncAmmoCount_\u002D1454761002(reader.ReadByteExtension(), reader.ReadPackedInt32());
  }

  public void UserCode_CmdSetStationTargets_872088460(
    byte stationIndex,
    ReadOnlySpan<PersistentID> targetIDs)
  {
    if ((int) stationIndex >= this.weaponStations.Count)
      return;
    this.RpcSetStationTargets(stationIndex, targetIDs);
  }

  protected static void Skeleton_CmdSetStationTargets_872088460(
    NetworkBehaviour behaviour,
    NetworkReader reader,
    INetworkPlayer senderConnection,
    int replyId)
  {
    ((Unit) behaviour).UserCode_CmdSetStationTargets_872088460(reader.ReadByteExtension(), GeneratedNetworkCode._Read_System\u002EReadOnlySpan\u00601\u003CPersistentID\u003E(reader));
  }

  public virtual void UserCode_RpcSetStationTargets_1363862903(
    byte stationIndex,
    ReadOnlySpan<PersistentID> targetIDs)
  {
    if ((int) stationIndex < this.weaponStations.Count)
      this.weaponStations[(int) stationIndex].SetStationTargets(targetIDs);
    if (this.LocalSim || !(this is Aircraft aircraft))
      return;
    aircraft.weaponManager.SetTargetList(targetIDs);
  }

  protected static void Skeleton_RpcSetStationTargets_1363862903(
    NetworkBehaviour behaviour,
    NetworkReader reader,
    INetworkPlayer senderConnection,
    int replyId)
  {
    ((Unit) behaviour).UserCode_RpcSetStationTargets_1363862903(reader.ReadByteExtension(), GeneratedNetworkCode._Read_System\u002EReadOnlySpan\u00601\u003CPersistentID\u003E(reader));
  }

  private void UserCode_CmdClaimHit_\u002D1122942669(
    PersistentID hitID,
    Vector3Compressed relativePosCompressed,
    Vector3Compressed bulletVelocityCompressed,
    byte weaponStationIndex)
  {
    Unit unit;
    Vector3 outValue1;
    Vector3 outValue2;
    if (!UnitRegistry.TryGetUnit(new PersistentID?(hitID), out unit) || !NetworkFloatHelper.TryDecompress(relativePosCompressed, out outValue1, false, (string) null) || !NetworkFloatHelper.TryDecompress(bulletVelocityCompressed, out outValue2, false, (string) null) || (int) weaponStationIndex >= this.weaponStations.Count || weaponStationIndex < (byte) 0)
      return;
    this.HitOnPhysicsFrame(unit, outValue1, outValue2, weaponStationIndex).Forget();
  }

  protected static void Skeleton_CmdClaimHit_\u002D1122942669(
    NetworkBehaviour behaviour,
    NetworkReader reader,
    INetworkPlayer senderConnection,
    int replyId)
  {
    ((Unit) behaviour).UserCode_CmdClaimHit_\u002D1122942669(GeneratedNetworkCode._Read_PersistentID(reader), GeneratedNetworkCode._Read_Vector3Compressed(reader), GeneratedNetworkCode._Read_Vector3Compressed(reader), reader.ReadByteExtension());
  }

  private void UserCode_CmdDamage_\u002D2104360672(byte partID, DamageInfo damageInfo)
  {
    if (!damageInfo.IsValid() || (int) partID >= this.damageables.Count)
      return;
    this.RpcDamage(partID, damageInfo);
  }

  protected static void Skeleton_CmdDamage_\u002D2104360672(
    NetworkBehaviour behaviour,
    NetworkReader reader,
    INetworkPlayer senderConnection,
    int replyId)
  {
    ((Unit) behaviour).UserCode_CmdDamage_\u002D2104360672(reader.ReadByteExtension(), GeneratedNetworkCode._Read_DamageInfo(reader));
  }

  public virtual void UserCode_RpcDamage_1709046923(byte index, DamageInfo damageInfo)
  {
    DamageablePart damageable = this.damageables[(int) index];
    if (damageable.Removed)
      return;
    damageable.Damageable.ApplyDamage(damageInfo.pierceDamage.Decompress(), damageInfo.blastDamage.Decompress(), damageInfo.fireDamage.Decompress(), damageInfo.impactDamage.Decompress());
  }

  protected static void Skeleton_RpcDamage_1709046923(
    NetworkBehaviour behaviour,
    NetworkReader reader,
    INetworkPlayer senderConnection,
    int replyId)
  {
    ((Unit) behaviour).UserCode_RpcDamage_1709046923(reader.ReadByteExtension(), GeneratedNetworkCode._Read_DamageInfo(reader));
  }

  private void UserCode_CmdFuelTankStatus_423953574(byte partID, bool ruptured, bool onFire)
  {
    if ((int) partID >= this.damageables.Count)
      return;
    this.RpcFuelTankStatus(partID, ruptured, onFire);
  }

  protected static void Skeleton_CmdFuelTankStatus_423953574(
    NetworkBehaviour behaviour,
    NetworkReader reader,
    INetworkPlayer senderConnection,
    int replyId)
  {
    ((Unit) behaviour).UserCode_CmdFuelTankStatus_423953574(reader.ReadByteExtension(), reader.ReadBooleanExtension(), reader.ReadBooleanExtension());
  }

  public void UserCode_RpcFuelTankStatus_414334555(byte partID, bool ruptured, bool onFire)
  {
    DamageablePart damageable = this.damageables[(int) partID];
    FuelTank component;
    if (damageable.Removed || !damageable.Damageable.GetTransform().TryGetComponent<FuelTank>(out component))
      return;
    component.UpdateStatus(ruptured, onFire);
  }

  protected static void Skeleton_RpcFuelTankStatus_414334555(
    NetworkBehaviour behaviour,
    NetworkReader reader,
    INetworkPlayer senderConnection,
    int replyId)
  {
    ((Unit) behaviour).UserCode_RpcFuelTankStatus_414334555(reader.ReadByteExtension(), reader.ReadBooleanExtension(), reader.ReadBooleanExtension());
  }

  private void UserCode_CmdBreakPart_\u002D789722400(
    byte partID,
    Vector3Compressed velocity,
    Vector3Compressed relativePos)
  {
    if ((int) partID >= this.damageables.Count)
      return;
    if (!NetworkFloatHelper.Validate(velocity, false, (string) null))
      velocity = Vector3Compressed.zero;
    if (!NetworkFloatHelper.Validate(relativePos, false, (string) null))
      relativePos = Vector3Compressed.zero;
    this.RpcBreakPart(partID, velocity, relativePos);
  }

  protected static void Skeleton_CmdBreakPart_\u002D789722400(
    NetworkBehaviour behaviour,
    NetworkReader reader,
    INetworkPlayer senderConnection,
    int replyId)
  {
    ((Unit) behaviour).UserCode_CmdBreakPart_\u002D789722400(reader.ReadByteExtension(), GeneratedNetworkCode._Read_Vector3Compressed(reader), GeneratedNetworkCode._Read_Vector3Compressed(reader));
  }

  private void UserCode_RpcBreakPart_\u002D1087911989(
    byte partID,
    Vector3Compressed velocityCompressed,
    Vector3Compressed relativePosCompressed)
  {
    DamageablePart damageable1 = this.damageables[(int) partID];
    if (damageable1.Removed)
      return;
    IDamageable damageable2 = damageable1.Damageable;
    Vector3 vector3 = NetworkFloatHelper.DecompressIfValid(relativePosCompressed, true, nameof (relativePosCompressed));
    Vector3 velocity = NetworkFloatHelper.DecompressIfValid(velocityCompressed, true, nameof (velocityCompressed));
    Vector3 relativePos = vector3;
    damageable2.Detach(velocity, relativePos);
  }

  protected static void Skeleton_RpcBreakPart_\u002D1087911989(
    NetworkBehaviour behaviour,
    NetworkReader reader,
    INetworkPlayer senderConnection,
    int replyId)
  {
    ((Unit) behaviour).UserCode_RpcBreakPart_\u002D1087911989(reader.ReadByteExtension(), GeneratedNetworkCode._Read_Vector3Compressed(reader), GeneratedNetworkCode._Read_Vector3Compressed(reader));
  }

  public void UserCode_CmdDisableUnit_\u002D340399461() => this.ServerDisableUnit();

  protected static void Skeleton_CmdDisableUnit_\u002D340399461(
    NetworkBehaviour behaviour,
    NetworkReader reader,
    INetworkPlayer senderConnection,
    int replyId)
  {
    ((Unit) behaviour).UserCode_CmdDisableUnit_\u002D340399461();
  }

  private UniTask<int[]> UserCode_CmdAskAmmoInternal_1214831455()
  {
    int count = this.weaponStations.Count;
    int[] numArray = new int[count];
    for (int index = 0; index < count; ++index)
      numArray[index] = this.weaponStations[index].Ammo;
    return UniTask.FromResult<int[]>(numArray);
  }

  protected static UniTask<int[]> Skeleton_CmdAskAmmoInternal_1214831455(
    NetworkBehaviour behaviour,
    NetworkReader reader,
    INetworkPlayer senderConnection,
    int replyId)
  {
    return ((Unit) behaviour).UserCode_CmdAskAmmoInternal_1214831455();
  }

  private UniTask<int[]> UserCode_CmdAskFullAmmoInternal_\u002D1739804082()
  {
    int count = this.weaponStations.Count;
    int[] numArray = new int[count];
    for (int index = 0; index < count; ++index)
      numArray[index] = this.weaponStations[index].FullAmmo;
    return UniTask.FromResult<int[]>(numArray);
  }

  protected static UniTask<int[]> Skeleton_CmdAskFullAmmoInternal_\u002D1739804082(
    NetworkBehaviour behaviour,
    NetworkReader reader,
    INetworkPlayer senderConnection,
    int replyId)
  {
    return ((Unit) behaviour).UserCode_CmdAskFullAmmoInternal_\u002D1739804082();
  }

  protected override int GetRpcCount() => 19;

  public override void RegisterRpc(RemoteCallCollection collection)
  {
    base.RegisterRpc(collection);
    collection.Register(0, "Unit.RpcJam", false, RpcInvokeType.ClientRpc, (NetworkBehaviour) this, new RpcDelegate(Unit.Skeleton_RpcJam_569024588));
    collection.Register(1, "Unit.RpcAssignRadar", false, RpcInvokeType.ClientRpc, (NetworkBehaviour) this, new RpcDelegate(Unit.Skeleton_RpcAssignRadar_\u002D1001571913));
    collection.Register(2, "Unit.CmdSetFiringState", true, RpcInvokeType.ServerRpc, (NetworkBehaviour) this, new RpcDelegate(Unit.Skeleton_CmdSetFiringState_1458755207));
    collection.Register(3, "Unit.CmdSingleRemoteFire", true, RpcInvokeType.ServerRpc, (NetworkBehaviour) this, new RpcDelegate(Unit.Skeleton_CmdSingleRemoteFire_1708169300));
    collection.Register(4, "Unit.RpcSingleRemoteFire", false, RpcInvokeType.ClientRpc, (NetworkBehaviour) this, new RpcDelegate(Unit.Skeleton_RpcSingleRemoteFire_\u002D1161895954));
    collection.Register(5, "Unit.CmdStoppedFiring", true, RpcInvokeType.ServerRpc, (NetworkBehaviour) this, new RpcDelegate(Unit.Skeleton_CmdStoppedFiring_\u002D1238964004));
    collection.Register(6, "Unit.RpcSyncAmmoCount", false, RpcInvokeType.ClientRpc, (NetworkBehaviour) this, new RpcDelegate(Unit.Skeleton_RpcSyncAmmoCount_\u002D1454761002));
    collection.Register(7, "Unit.CmdSetStationTargets", true, RpcInvokeType.ServerRpc, (NetworkBehaviour) this, new RpcDelegate(Unit.Skeleton_CmdSetStationTargets_872088460));
    collection.Register(8, "Unit.RpcSetStationTargets", false, RpcInvokeType.ClientRpc, (NetworkBehaviour) this, new RpcDelegate(Unit.Skeleton_RpcSetStationTargets_1363862903));
    collection.Register(9, "Unit.CmdClaimHit", true, RpcInvokeType.ServerRpc, (NetworkBehaviour) this, new RpcDelegate(Unit.Skeleton_CmdClaimHit_\u002D1122942669));
    collection.Register(10, "Unit.CmdDamage", true, RpcInvokeType.ServerRpc, (NetworkBehaviour) this, new RpcDelegate(Unit.Skeleton_CmdDamage_\u002D2104360672));
    collection.Register(11, "Unit.RpcDamage", false, RpcInvokeType.ClientRpc, (NetworkBehaviour) this, new RpcDelegate(Unit.Skeleton_RpcDamage_1709046923));
    collection.Register(12, "Unit.CmdFuelTankStatus", true, RpcInvokeType.ServerRpc, (NetworkBehaviour) this, new RpcDelegate(Unit.Skeleton_CmdFuelTankStatus_423953574));
    collection.Register(13, "Unit.RpcFuelTankStatus", false, RpcInvokeType.ClientRpc, (NetworkBehaviour) this, new RpcDelegate(Unit.Skeleton_RpcFuelTankStatus_414334555));
    collection.Register(14, "Unit.CmdBreakPart", true, RpcInvokeType.ServerRpc, (NetworkBehaviour) this, new RpcDelegate(Unit.Skeleton_CmdBreakPart_\u002D789722400));
    collection.Register(15, "Unit.RpcBreakPart", false, RpcInvokeType.ClientRpc, (NetworkBehaviour) this, new RpcDelegate(Unit.Skeleton_RpcBreakPart_\u002D1087911989));
    collection.Register(16 /*0x10*/, "Unit.CmdDisableUnit", true, RpcInvokeType.ServerRpc, (NetworkBehaviour) this, new RpcDelegate(Unit.Skeleton_CmdDisableUnit_\u002D340399461));
    collection.RegisterRequest<int[]>(17, "Unit.CmdAskAmmoInternal", false, RpcInvokeType.ServerRpc, (NetworkBehaviour) this, new RequestDelegate<int[]>(Unit.Skeleton_CmdAskAmmoInternal_1214831455));
    collection.RegisterRequest<int[]>(18, "Unit.CmdAskFullAmmoInternal", false, RpcInvokeType.ServerRpc, (NetworkBehaviour) this, new RequestDelegate<int[]>(Unit.Skeleton_CmdAskFullAmmoInternal_\u002D1739804082));
  }

  public enum UnitState : byte
  {
    Active = 1,
    Damaged = 2,
    Abandoned = 3,
    Destroyed = 4,
    Returned = 5,
  }

  public struct JamEventArgs
  {
    public Unit jammingUnit;
    public float jamAmount;
  }
}

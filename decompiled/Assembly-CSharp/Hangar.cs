// Decompiled with JetBrains decompiler
// Type: Hangar
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
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
public class Hangar : NetworkBehaviour
{
  public Unit attachedUnit;
  [SerializeField]
  private float priority;
  [SerializeField]
  private UnitPart criticalPart;
  [SerializeField]
  private AircraftDefinition[] availableAircraft;
  [SerializeField]
  private bool waitForOpenBeforeSpawn;
  [SerializeField]
  private HangarDoor[] doors;
  [SerializeField]
  private bool elevator;
  [SerializeField]
  private Transform spawnTransform;
  [SerializeField]
  private float doorSpeed;
  [SerializeField]
  private AudioClip closedSound;
  [SerializeField]
  private AudioClip movingSound;
  [SerializeField]
  private AudioClip openSound;
  [SerializeField]
  private float pitchMin = 0.5f;
  [SerializeField]
  private float pitchMax = 1f;
  [SerializeField]
  [Range(0.0f, 2f)]
  private float movingVolume;
  [SerializeField]
  private bool speedVolume;
  [SerializeField]
  private bool speedPitch;
  private AudioSource oneShotSource;
  private AudioSource loopSource;
  private GameObject spawnedObject;
  private float doorCurrentSpeed;
  private CancellationTokenSource cancelMoveDoors;
  private bool selfDisabled;
  [SyncVar(initialOnly = true)]
  private Hangar.DoorState doorState;
  [SyncVar(initialOnly = true)]
  private bool available;
  [NonSerialized]
  private const int SYNC_VAR_COUNT = 2;
  [NonSerialized]
  private const int RPC_COUNT = 3;

  public bool Disabled => this.selfDisabled || this.attachedUnit.disabled;

  public bool Available => this.available && !this.Disabled;

  private void OnValidate()
  {
  }

  private void Awake()
  {
    this.Identity.OnStartServer.AddListener(new Action(this.OnStartServer));
    this.Identity.OnStartClient.AddListener(new Action(this.OnStartClient));
  }

  private void OnStartServer()
  {
    this.Networkavailable = true;
    this.NetworkdoorState = new Hangar.DoorState()
    {
      openAmount = 0.0f,
      opening = false
    };
    foreach (HangarDoor door in this.doors)
      door.Initialize(0.0f);
    if (!((UnityEngine.Object) this.criticalPart != (UnityEngine.Object) null))
      return;
    this.criticalPart.onApplyDamage += new Action<UnitPart.OnApplyDamage>(this.Hangar_OnApplyDamage);
  }

  public Transform GetSpawnTransform() => this.spawnTransform;

  public float GetPriority() => this.priority;

  private void OnStartClient() => this.InitClientDoors();

  public bool IsFunctional()
  {
    return !this.Disabled && (double) this.transform.position.y > (double) Datum.LocalSeaY;
  }

  public void Repair()
  {
    if ((UnityEngine.Object) this.attachedUnit == (UnityEngine.Object) null)
      return;
    this.attachedUnit.Networkdisabled = false;
    this.selfDisabled = false;
  }

  private void Hangar_OnApplyDamage(UnitPart.OnApplyDamage e)
  {
    if ((double) e.hitPoints >= 0.0 && !e.detached)
      return;
    this.selfDisabled = true;
  }

  public AircraftDefinition[] GetAvailableAircraft()
  {
    return this.attachedUnit.disabled ? Array.Empty<AircraftDefinition>() : this.availableAircraft;
  }

  public bool CanSpawnAircraft(AircraftDefinition definition)
  {
    return this.Available && ((ICollection<AircraftDefinition>) this.availableAircraft).Contains(definition);
  }

  [Mirage.Server]
  public Airbase.TrySpawnResult TrySpawnAircraft(
    Player player,
    AircraftDefinition definition,
    LiveryKey livery,
    Loadout loadout,
    float fuelLevel)
  {
    if (!this.IsServer)
      throw new MethodInvocationException("[Server] function 'TrySpawnAircraft' called when server not active");
    if (!this.CanSpawnAircraft(definition))
      return new Airbase.TrySpawnResult();
    if (this.waitForOpenBeforeSpawn)
    {
      this.DoorSequenceCarrier(new Hangar.QueuedAircraftToSpawn(player, definition, livery, loadout, fuelLevel)).Forget();
    }
    else
    {
      this.SpawnAircraft(player, definition, loadout, fuelLevel, livery);
      this.DoorSequenceNormal().Forget();
    }
    if ((UnityEngine.Object) player != (UnityEngine.Object) null)
      player.FlyOwnedAirframe(definition);
    else
      this.attachedUnit.NetworkHQ.AddSupplyUnit((UnitDefinition) definition, -1);
    return new Airbase.TrySpawnResult(true, this, this.waitForOpenBeforeSpawn);
  }

  public void CheckAttachCamera()
  {
    if (!this.waitForOpenBeforeSpawn)
      return;
    this.AttachCamera();
  }

  private void AttachCamera()
  {
    SceneSingleton<CameraStateManager>.i.followingUnit = this.attachedUnit;
    SceneSingleton<CameraStateManager>.i.SwitchState((CameraBaseState) SceneSingleton<CameraStateManager>.i.relativeState);
    SceneSingleton<CameraStateManager>.i.transform.SetPositionAndRotation(this.transform.position, this.transform.rotation);
  }

  public Unit GetUnit() => this.attachedUnit;

  public Vector3 GetVelocity()
  {
    return !((UnityEngine.Object) this.attachedUnit.rb == (UnityEngine.Object) null) ? this.attachedUnit.rb.GetPointVelocity(this.spawnTransform.position) : Vector3.zero;
  }

  public Vector3 GetAngularVelocity()
  {
    return !((UnityEngine.Object) this.attachedUnit.rb == (UnityEngine.Object) null) ? this.attachedUnit.rb.angularVelocity : Vector3.zero;
  }

  private void SpawnAircraft(
    Player player,
    AircraftDefinition definition,
    Loadout loadout,
    float fuelLevel,
    LiveryKey livery)
  {
    GlobalPosition globalPosition = this.spawnTransform.GlobalPosition() + this.spawnTransform.up * definition.spawnOffset.y + this.spawnTransform.forward * definition.spawnOffset.z;
    Vector3 velocity = this.GetVelocity();
    Aircraft aircraft = NetworkSceneSingleton<Spawner>.i.SpawnAircraft(player, definition.unitPrefab, loadout, fuelLevel, livery, globalPosition, this.spawnTransform.rotation, velocity, this, this.attachedUnit.NetworkHQ, (string) null, 1f, 0.5f);
    if (loadout == null)
      aircraft.Networkloadout = aircraft.weaponManager.SelectWeapons(true);
    this.spawnedObject = aircraft.gameObject;
  }

  private void OnDestroy()
  {
    this.cancelMoveDoors?.Cancel();
    this.selfDisabled = true;
  }

  [ClientRpc(excludeHost = true)]
  private void RpcOpenHangar()
  {
    PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
    ClientRpcSender.Send((NetworkBehaviour) this, 0, (NetworkWriter) writer, Mirage.Channel.Reliable, false);
    writer.Release();
  }

  [ClientRpc(excludeHost = true)]
  private void RpcCloseHangar()
  {
    PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
    ClientRpcSender.Send((NetworkBehaviour) this, 1, (NetworkWriter) writer, Mirage.Channel.Reliable, false);
    writer.Release();
  }

  [ClientRpc(excludeHost = true)]
  private void RpcHangarAvailable()
  {
    PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
    ClientRpcSender.Send((NetworkBehaviour) this, 2, (NetworkWriter) writer, Mirage.Channel.Reliable, false);
    writer.Release();
  }

  private void InitClientDoors()
  {
    foreach (HangarDoor door in this.doors)
      door.Initialize(this.doorState.openAmount);
    if (this.doorState.opening && (double) this.doorState.openAmount < 1.0)
    {
      this.OpenDoors().Forget();
    }
    else
    {
      if (this.doorState.opening || (double) this.doorState.openAmount <= 0.0)
        return;
      this.CloseDoors().Forget();
    }
  }

  [Mirage.Server]
  private UniTask DoorSequenceNormal()
  {
    if (!this.IsServer)
      throw new MethodInvocationException("[Server] function 'DoorSequenceNormal' called when server not active");
    // ISSUE: variable of a compiler-generated type
    Hangar.\u003CDoorSequenceNormal\u003Ed__54 stateMachine;
    // ISSUE: reference to a compiler-generated field
    stateMachine.\u003C\u003Et__builder = AsyncUniTaskMethodBuilder.Create();
    // ISSUE: reference to a compiler-generated field
    stateMachine.\u003C\u003E4__this = this;
    // ISSUE: reference to a compiler-generated field
    stateMachine.\u003C\u003E1__state = -1;
    // ISSUE: reference to a compiler-generated field
    stateMachine.\u003C\u003Et__builder.Start<Hangar.\u003CDoorSequenceNormal\u003Ed__54>(ref stateMachine);
    // ISSUE: reference to a compiler-generated field
    return stateMachine.\u003C\u003Et__builder.Task;
  }

  [Mirage.Server]
  private UniTask DoorSequenceCarrier(Hangar.QueuedAircraftToSpawn spawnAircraft)
  {
    if (!this.IsServer)
      throw new MethodInvocationException("[Server] function 'DoorSequenceCarrier' called when server not active");
    // ISSUE: variable of a compiler-generated type
    Hangar.\u003CDoorSequenceCarrier\u003Ed__55 stateMachine;
    // ISSUE: reference to a compiler-generated field
    stateMachine.\u003C\u003Et__builder = AsyncUniTaskMethodBuilder.Create();
    // ISSUE: reference to a compiler-generated field
    stateMachine.\u003C\u003E4__this = this;
    // ISSUE: reference to a compiler-generated field
    stateMachine.spawnAircraft = spawnAircraft;
    // ISSUE: reference to a compiler-generated field
    stateMachine.\u003C\u003E1__state = -1;
    // ISSUE: reference to a compiler-generated field
    stateMachine.\u003C\u003Et__builder.Start<Hangar.\u003CDoorSequenceCarrier\u003Ed__55>(ref stateMachine);
    // ISSUE: reference to a compiler-generated field
    return stateMachine.\u003C\u003Et__builder.Task;
  }

  private async UniTask OpenDoors()
  {
    Hangar hangar = this;
    hangar.Networkavailable = false;
    if (hangar.IsServer)
      hangar.RpcOpenHangar();
    if ((UnityEngine.Object) hangar.oneShotSource == (UnityEngine.Object) null)
      hangar.CreateAudioSources();
    hangar.loopSource.Play();
    if ((UnityEngine.Object) hangar.closedSound != (UnityEngine.Object) null)
      hangar.oneShotSource.PlayOneShot(hangar.closedSound, hangar.movingVolume);
    CancellationToken cancel = hangar.GetNewCancellationToken();
    string name = hangar.name;
    int netId = (int) hangar.NetId;
    await hangar.MoveDoors(true, cancel);
    if (cancel.IsCancellationRequested)
    {
      cancel = new CancellationToken();
    }
    else
    {
      hangar.NetworkdoorState = new Hangar.DoorState()
      {
        opening = true,
        openAmount = 1f
      };
      cancel = new CancellationToken();
    }
  }

  private async UniTask WaitForUnitToLeave(CancellationToken cancel)
  {
    Hangar hangar = this;
    string name = hangar.name;
    int netId = (int) hangar.NetId;
    await UniTask.Delay(1000);
    if (cancel.IsCancellationRequested)
      return;
    // ISSUE: method pointer
    int num = await UniTask.WaitUntil(new Func<bool>((object) hangar, __methodptr(\u003CWaitForUnitToLeave\u003Eg__UnitLeft\u007C57_0)), cancellationToken: cancel).SuppressCancellationThrow() ? 1 : 0;
  }

  private async UniTask CloseDoors()
  {
    Hangar hangar = this;
    if (hangar.IsServer)
      hangar.RpcCloseHangar();
    if ((UnityEngine.Object) hangar.oneShotSource == (UnityEngine.Object) null)
      hangar.CreateAudioSources();
    hangar.loopSource.Play();
    if ((UnityEngine.Object) hangar.openSound != (UnityEngine.Object) null)
      hangar.oneShotSource.PlayOneShot(hangar.openSound);
    CancellationToken cancel = hangar.GetNewCancellationToken();
    string name = hangar.name;
    int netId = (int) hangar.NetId;
    await hangar.MoveDoors(false, cancel);
    if (cancel.IsCancellationRequested)
    {
      cancel = new CancellationToken();
    }
    else
    {
      hangar.NetworkdoorState = new Hangar.DoorState()
      {
        opening = false,
        openAmount = 0.0f
      };
      cancel = new CancellationToken();
    }
  }

  private CancellationToken GetNewCancellationToken()
  {
    this.cancelMoveDoors?.Cancel();
    this.cancelMoveDoors = new CancellationTokenSource();
    return this.cancelMoveDoors.Token;
  }

  private async UniTask MoveDoors(bool open, CancellationToken cancel)
  {
    Hangar hangar = this;
    float openAmount = hangar.doorState.openAmount;
    float target = open ? 1.05f : -0.05f;
    while (!cancel.IsCancellationRequested)
    {
      float maxSpeed = 0.2f / hangar.doorSpeed;
      openAmount = Mathf.SmoothDamp(openAmount, target, ref hangar.doorCurrentSpeed, 0.5f / hangar.doorSpeed, maxSpeed);
      float t = Mathf.Abs(hangar.doorCurrentSpeed / maxSpeed);
      float openAmount1 = Mathf.Clamp01(openAmount);
      foreach (HangarDoor door in hangar.doors)
        door.Move(openAmount1);
      hangar.loopSource.volume = !hangar.speedVolume ? hangar.movingVolume : Mathf.Clamp01(t * hangar.movingVolume);
      if (hangar.speedPitch)
        hangar.loopSource.pitch = Mathf.Lerp(hangar.pitchMin, hangar.pitchMax, t);
      if (open)
      {
        if ((double) openAmount >= 1.0)
        {
          if ((UnityEngine.Object) hangar.openSound != (UnityEngine.Object) null)
            hangar.oneShotSource.PlayOneShot(hangar.openSound, hangar.movingVolume);
          if (!hangar.loopSource.isPlaying)
            break;
          hangar.loopSource.Stop();
          break;
        }
      }
      else if ((double) openAmount <= 0.0)
      {
        if ((UnityEngine.Object) hangar.closedSound != (UnityEngine.Object) null)
          hangar.oneShotSource.PlayOneShot(hangar.closedSound, hangar.movingVolume);
        if (!hangar.loopSource.isPlaying)
          break;
        hangar.loopSource.Stop();
        break;
      }
      hangar.NetworkdoorState = new Hangar.DoorState()
      {
        opening = open,
        openAmount = openAmount
      };
      await UniTask.WaitForFixedUpdate();
    }
  }

  private void CreateAudioSources()
  {
    this.oneShotSource = this.gameObject.AddComponent<AudioSource>();
    this.oneShotSource.outputAudioMixerGroup = SoundManager.i.EffectsMixer;
    this.oneShotSource.spatialBlend = 1f;
    this.oneShotSource.dopplerLevel = 0.0f;
    this.oneShotSource.minDistance = 20f;
    this.oneShotSource.maxDistance = 500f;
    this.loopSource = this.gameObject.AddComponent<AudioSource>();
    this.loopSource.outputAudioMixerGroup = SoundManager.i.EffectsMixer;
    this.loopSource.spatialBlend = 1f;
    this.loopSource.dopplerLevel = 0.0f;
    this.loopSource.minDistance = 20f;
    this.loopSource.maxDistance = 500f;
    this.loopSource.clip = this.movingSound;
    this.loopSource.loop = true;
  }

  private void MirageProcessed()
  {
  }

  public Hangar.DoorState NetworkdoorState
  {
    get => this.doorState;
    set => this.doorState = value;
  }

  public bool Networkavailable
  {
    get => this.available;
    set => this.available = value;
  }

  public override bool SerializeSyncVars(NetworkWriter writer, bool initialize)
  {
    ulong syncVarDirtyBits = this.SyncVarDirtyBits;
    bool flag = base.SerializeSyncVars(writer, initialize);
    if (initialize)
    {
      GeneratedNetworkCode._Write_Hangar\u002FDoorState(writer, this.doorState);
      writer.WriteBooleanExtension(this.available);
      return true;
    }
    writer.Write(syncVarDirtyBits, 2);
    return flag;
  }

  public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
  {
    base.DeserializeSyncVars(reader, initialState);
    if (initialState)
    {
      this.doorState = GeneratedNetworkCode._Read_Hangar\u002FDoorState(reader);
      this.available = reader.ReadBooleanExtension();
    }
    else
      this.SetDeserializeMask(reader.Read(2), 0);
  }

  private void UserCode_RpcOpenHangar_1172819820() => this.OpenDoors().Forget();

  protected static void Skeleton_RpcOpenHangar_1172819820(
    NetworkBehaviour behaviour,
    NetworkReader reader,
    INetworkPlayer senderConnection,
    int replyId)
  {
    ((Hangar) behaviour).UserCode_RpcOpenHangar_1172819820();
  }

  private void UserCode_RpcCloseHangar_\u002D1009291362() => this.CloseDoors().Forget();

  protected static void Skeleton_RpcCloseHangar_\u002D1009291362(
    NetworkBehaviour behaviour,
    NetworkReader reader,
    INetworkPlayer senderConnection,
    int replyId)
  {
    ((Hangar) behaviour).UserCode_RpcCloseHangar_\u002D1009291362();
  }

  private void UserCode_RpcHangarAvailable_\u002D1837624951() => this.Networkavailable = true;

  protected static void Skeleton_RpcHangarAvailable_\u002D1837624951(
    NetworkBehaviour behaviour,
    NetworkReader reader,
    INetworkPlayer senderConnection,
    int replyId)
  {
    ((Hangar) behaviour).UserCode_RpcHangarAvailable_\u002D1837624951();
  }

  protected override int GetRpcCount() => 3;

  public override void RegisterRpc(RemoteCallCollection collection)
  {
    base.RegisterRpc(collection);
    collection.Register(0, "Hangar.RpcOpenHangar", false, RpcInvokeType.ClientRpc, (NetworkBehaviour) this, new RpcDelegate(Hangar.Skeleton_RpcOpenHangar_1172819820));
    collection.Register(1, "Hangar.RpcCloseHangar", false, RpcInvokeType.ClientRpc, (NetworkBehaviour) this, new RpcDelegate(Hangar.Skeleton_RpcCloseHangar_\u002D1009291362));
    collection.Register(2, "Hangar.RpcHangarAvailable", false, RpcInvokeType.ClientRpc, (NetworkBehaviour) this, new RpcDelegate(Hangar.Skeleton_RpcHangarAvailable_\u002D1837624951));
  }

  public struct DoorState
  {
    public bool opening;
    public float openAmount;
  }

  private readonly struct QueuedAircraftToSpawn(
    Player player,
    AircraftDefinition definition,
    LiveryKey livery,
    Loadout loadout,
    float fuelLevel)
  {
    public readonly Player player = player;
    public readonly AircraftDefinition definition = definition;
    public readonly LiveryKey livery = livery;
    public readonly Loadout loadout = loadout;
    public readonly float fuelLevel = fuelLevel;
  }
}

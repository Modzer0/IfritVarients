// Decompiled with JetBrains decompiler
// Type: PilotDismounted
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Cysharp.Threading.Tasks;
using Mirage;
using Mirage.RemoteCalls;
using Mirage.Serialization;
using NuclearOption.Networking;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

#nullable disable
public class PilotDismounted : Unit, IDamageable
{
  private float hitPoints;
  private float chuteOpenTime;
  private float landedTime;
  [SerializeField]
  private ArmorProperties armorProperties;
  [SerializeField]
  private Animator animator;
  [SyncVar]
  public PersistentID parentUnit;
  [SyncVar]
  public byte unitPart;
  [SyncVar]
  public byte pilotNumber;
  [SyncVar(hook = "PilotStateChanged")]
  public PilotDismounted.PilotState animationState = PilotDismounted.PilotState.ejecting;
  [SyncVar]
  public NetworkBehaviorSyncvar player;
  [SyncVar(hook = "OnKinematicChanged")]
  private bool isKinematic;
  [SerializeField]
  private EjectionSeat ejectionSeat;
  [SerializeField]
  private Collider pilotCollider;
  [SerializeField]
  private Collider deadCollider;
  private new RaycastHit hit;
  [SyncVar]
  private bool chuteDeployedNetwork;
  private bool chuteDeployedLocal;
  [SyncVar]
  private bool seatDetachedNetwork;
  private bool seatDetachedLocal;
  private bool inWater;
  private bool checkingForCapture;
  private bool captured;
  private Vector3 posPrev;
  private Vector3? velocityPrev;
  private float runWeight;
  private bool slung;
  private int pilotRank;
  [NonSerialized]
  private const int SYNC_VAR_COUNT = 17;
  [NonSerialized]
  private const int RPC_COUNT = 20;

  public float timeSinceSpawn { get; private set; }

  public UnitPart cockpitPart { get; private set; }

  public bool IsOnEjectionRail => this.ejectionSeat.IsOnEjectionRail;

  public void TakeDamage(
    float pierceDamage,
    float blastDamage,
    float amountAffected,
    float fireDamage,
    float collisionDamage,
    PersistentID dealerID)
  {
    if (this.disabled)
      return;
    float num1 = Mathf.Max(pierceDamage - this.armorProperties.pierceArmor, 0.0f) / this.armorProperties.pierceTolerance;
    float num2 = Mathf.Max(blastDamage - this.armorProperties.blastArmor, 0.0f) * amountAffected / this.armorProperties.blastTolerance;
    float num3 = Mathf.Max(fireDamage - this.armorProperties.fireArmor, 0.0f) / this.armorProperties.fireTolerance;
    if ((double) this.hitPoints - ((double) num1 + (double) num2 + (double) num3) <= 0.0)
    {
      this.Networkdisabled = true;
      this.SetPilotState(PilotDismounted.PilotState.dead);
    }
    this.RpcTakeDamage(num1 + num2 + num3);
  }

  public void ApplyDamage(
    float pierceDamage,
    float blastDamage,
    float fireDamage,
    float impactDamage)
  {
    this.hitPoints -= pierceDamage;
  }

  [ClientRpc]
  public void RpcTakeDamage(float damage)
  {
    if (ClientRpcSender.ShouldInvokeLocally((NetworkBehaviour) this, RpcTarget.Observers, (INetworkPlayer) null, false))
      this.UserCode_RpcTakeDamage_446604653(damage);
    PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
    writer.WriteSingleConverter(damage);
    ClientRpcSender.Send((NetworkBehaviour) this, 19, (NetworkWriter) writer, Mirage.Channel.Reliable, false);
    writer.Release();
  }

  public void SetPilotState(PilotDismounted.PilotState state)
  {
    if (!this.IsServer || this.animationState == state)
      return;
    this.NetworkanimationState = state;
  }

  private void PilotStateChanged(
    PilotDismounted.PilotState oldState,
    PilotDismounted.PilotState newState)
  {
    this.animator.SetInteger("PilotState", (int) newState);
    if (oldState == PilotDismounted.PilotState.dead)
      return;
    if (newState == PilotDismounted.PilotState.dead)
    {
      this.pilotCollider.enabled = false;
      this.deadCollider.enabled = true;
      this.animator.SetLayerWeight(1, 0.0f);
      if ((UnityEngine.Object) this.ejectionSeat != (UnityEngine.Object) null)
        this.ejectionSeat.Detach();
      this.rb.angularDrag = 1f;
      this.rb.drag = 5f;
    }
    if (newState != PilotDismounted.PilotState.landing)
      return;
    this.rb.angularDrag = 5f;
    this.rb.drag = 5f;
  }

  public ArmorProperties GetArmorProperties() => this.armorProperties;

  public Unit GetUnit() => (Unit) this;

  public Transform GetTransform() => this.transform;

  public new float GetMass() => this.rb.mass;

  public void TakeShockwave(Vector3 origin, float blastEffectScale, float blastPower)
  {
    float num1 = Vector3.Distance(this.transform.position, origin) / blastEffectScale;
    float a = (float) (8000.0 / ((double) num1 * (double) num1 * (double) num1));
    float num2 = Mathf.Min((float) ((double) Mathf.Sqrt(this.rb.mass) * (double) Mathf.Min(Mathf.Min(a, 100f), blastPower) * 5.0), 100f * this.rb.mass);
    this.rb.AddForce(FastMath.NormalizedDirection(origin, this.transform.position) * num2, ForceMode.Impulse);
  }

  public void Detach(Vector3 velocity, Vector3 relativePos)
  {
  }

  public void OnKinematicChanged(bool _, bool isKinematic)
  {
    if (this.IsServer)
      this.NetworkstartPosition = this.GlobalPosition();
    this.enabled = !isKinematic;
    this.rb.isKinematic = isKinematic;
    this.rb.interpolation = isKinematic ? RigidbodyInterpolation.None : RigidbodyInterpolation.Interpolate;
    this.animator.enabled = !isKinematic;
  }

  public void SetCollidable(bool enabled) => this.pilotCollider.enabled = enabled;

  public override void Awake()
  {
    base.Awake();
    this.hitPoints = 100f;
    this.Identity.OnStartClient.AddListener(new Action(this.OnStartClient));
    this.Identity.OnStartServer.AddListener(new Action(this.OnStartServer));
  }

  private void OnStartServer() => this.Setup().Forget();

  private void OnStartClient()
  {
    if (this.IsServer)
      return;
    this.Setup().Forget();
  }

  private async UniTask Setup()
  {
    PilotDismounted pilotDismounted = this;
    await UniTask.Yield(PlayerLoopTiming.FixedUpdate);
    pilotDismounted.SetRB(pilotDismounted.gameObject.GetComponent<Rigidbody>());
    pilotDismounted.SetLocalSim(pilotDismounted.IsServer);
    pilotDismounted.ejectionSeat.LinkToPilot(pilotDismounted);
    Aircraft unit;
    if (UnitRegistry.TryGetUnit<Aircraft>(new PersistentID?(pilotDismounted.parentUnit), out unit))
    {
      Pilot pilot = unit.pilots[(int) pilotDismounted.pilotNumber];
      pilotDismounted.cockpitPart = pilot.GetUnitPart();
      pilot.SetEjected();
      pilotDismounted.rb.velocity = pilotDismounted.cockpitPart.rb.velocity;
      if ((double) unit.speed < 10.0 || !unit.pilots[(int) pilotDismounted.pilotNumber].HasEjectionSeat())
      {
        pilotDismounted.ejectionSeat.BailOut(pilotDismounted.rb, pilotDismounted.cockpitPart.rb);
        pilotDismounted.NetworkseatDetachedNetwork = true;
        pilotDismounted.seatDetachedLocal = true;
        pilotDismounted.rb.mass = 85f;
        if (pilotDismounted.LocalSim)
        {
          pilotDismounted.rb.velocity += Vector3.up * 2f;
          Vector3 vector3 = pilotDismounted.FindDismountSpot(unit);
          if (Physics.Linecast(vector3, vector3 - Vector3.up * 20f, out pilotDismounted.hit, -8193))
            vector3 = pilotDismounted.hit.point + Vector3.up * 1.3f;
          Quaternion rotation = Quaternion.LookRotation(vector3 - pilotDismounted.cockpitPart.transform.position, Vector3.up);
          pilotDismounted.transform.SetPositionAndRotation(vector3, rotation);
          pilotDismounted.rb.Move(vector3, rotation);
          pilotDismounted.NetworkstartPosition = vector3.ToGlobalPosition();
        }
        else
        {
          pilotDismounted.transform.SetPositionAndRotation(pilotDismounted.startPosition.ToLocalPosition(), unit.pilots[(int) pilotDismounted.pilotNumber].transform.rotation);
          pilotDismounted.rb.Move(pilotDismounted.startPosition.ToLocalPosition(), unit.pilots[(int) pilotDismounted.pilotNumber].transform.rotation);
        }
        pilotDismounted.SetCollidable(true);
      }
      else
      {
        pilotDismounted.transform.SetPositionAndRotation(unit.pilots[(int) pilotDismounted.pilotNumber].transform.position, unit.pilots[(int) pilotDismounted.pilotNumber].transform.rotation);
        pilotDismounted.rb.Move(unit.pilots[(int) pilotDismounted.pilotNumber].transform.position, unit.pilots[(int) pilotDismounted.pilotNumber].transform.rotation);
        pilotDismounted.ejectionSeat.Fire(pilotDismounted.cockpitPart);
      }
      if ((UnityEngine.Object) unit == (UnityEngine.Object) SceneSingleton<CameraStateManager>.i.followingUnit && pilotDismounted.pilotNumber == (byte) 0)
      {
        SceneSingleton<CameraStateManager>.i.SetFollowingUnit((Unit) pilotDismounted);
        if (GameManager.IsLocalPlayer<Player>(pilotDismounted.Networkplayer))
          pilotDismounted.StopFollowingAfterTime(5f).Forget();
      }
      if ((UnityEngine.Object) pilotDismounted.Networkplayer != (UnityEngine.Object) null)
        pilotDismounted.Networkplayer.SetPilotDismounted(pilotDismounted);
      unit.pilots[(int) pilotDismounted.pilotNumber].gameObject.SetActive(false);
      if (pilotDismounted.IsServer)
        pilotDismounted.NetworkanimationState = PilotDismounted.PilotState.ejecting;
      pilotDismounted.PilotStateChanged((PilotDismounted.PilotState) 0, pilotDismounted.animationState);
      pilotDismounted.pilotRank = (UnityEngine.Object) pilotDismounted.Networkplayer != (UnityEngine.Object) null ? pilotDismounted.Networkplayer.PlayerRank : unit.definition.aircraftParameters.rankRequired;
    }
    else
    {
      if (!pilotDismounted.IsServer)
      {
        Vector3 localPosition = pilotDismounted.startPosition.ToLocalPosition();
        pilotDismounted.transform.SetPositionAndRotation(localPosition, pilotDismounted.startRotation);
        pilotDismounted.rb.MovePosition(localPosition);
      }
      else
      {
        pilotDismounted.DetachSeat();
        pilotDismounted.NetworkanimationState = PilotDismounted.PilotState.detaching;
        pilotDismounted.PilotStateChanged((PilotDismounted.PilotState) 0, pilotDismounted.animationState);
      }
      pilotDismounted.SetCollidable(true);
    }
    if (pilotDismounted.remoteSim && pilotDismounted.chuteDeployedNetwork)
      pilotDismounted.DeployChute();
    if (pilotDismounted.remoteSim && pilotDismounted.seatDetachedNetwork)
      pilotDismounted.DetachSeat();
    if (pilotDismounted.remoteSim && pilotDismounted.isKinematic)
      pilotDismounted.OnKinematicChanged(false, true);
    pilotDismounted.RegisterUnit(new float?(4f));
  }

  private async UniTask StopFollowingAfterTime(float time)
  {
    PilotDismounted pilotDismounted = this;
    CancellationToken cancel = pilotDismounted.destroyCancellationToken;
    await UniTask.Delay((int) ((double) time * 1000.0));
    if (cancel.IsCancellationRequested)
      cancel = new CancellationToken();
    else if ((UnityEngine.Object) SceneSingleton<CameraStateManager>.i.followingUnit != (UnityEngine.Object) pilotDismounted)
    {
      cancel = new CancellationToken();
    }
    else
    {
      SceneSingleton<CameraStateManager>.i.SetFollowingUnit((Unit) null);
      pilotDismounted.Networkplayer.ShowMap(1f);
      cancel = new CancellationToken();
    }
  }

  private Vector3 FindDismountSpot(Aircraft aircraft)
  {
    Vector3 end = this.cockpitPart.transform.position;
    int num1 = 1;
    int num2 = aircraft.pilots[(int) this.pilotNumber].exitDirection == Pilot.ExitDirection.Left ? -1 : 1;
    while (num1 < 10)
    {
      end = this.cockpitPart.transform.position + this.cockpitPart.transform.right * (float) (num1 + 1) * (float) num2;
      if (!Physics.Linecast(end + Vector3.up * 10f, end, out this.hit, -8193))
        return end;
      ++num1;
      num2 *= -1;
    }
    return end;
  }

  private async UniTask CheckForCapture()
  {
    PilotDismounted pilotDismounted = this;
    pilotDismounted.checkingForCapture = true;
    if ((UnityEngine.Object) pilotDismounted.Networkplayer != (UnityEngine.Object) null)
      pilotDismounted.Networkplayer.RemovePilotDismounted(pilotDismounted);
    List<GridSquare> gridSquares = BattlefieldGrid.GetGridSquaresInRange(pilotDismounted.transform.GlobalPosition(), 1000f);
    Unit parentAircraft;
    UnitRegistry.TryGetUnit(new PersistentID?(pilotDismounted.parentUnit), out parentAircraft);
    CancellationToken cancel = pilotDismounted.destroyCancellationToken;
    UniTask uniTask = UniTask.Delay(3000);
    await uniTask;
    if (cancel.IsCancellationRequested)
    {
      gridSquares = (List<GridSquare>) null;
      parentAircraft = (Unit) null;
      cancel = new CancellationToken();
    }
    else if (pilotDismounted.NetworkHQ.AnyNearAirbase(pilotDismounted.transform.position, out Airbase _))
    {
      pilotDismounted.captured = true;
      pilotDismounted.Networkdisabled = true;
      uniTask = UniTask.Delay(2000);
      await uniTask;
      if (cancel.IsCancellationRequested)
      {
        gridSquares = (List<GridSquare>) null;
        parentAircraft = (Unit) null;
        cancel = new CancellationToken();
      }
      else
      {
        UnityEngine.Object.Destroy((UnityEngine.Object) pilotDismounted.gameObject);
        gridSquares = (List<GridSquare>) null;
        parentAircraft = (Unit) null;
        cancel = new CancellationToken();
      }
    }
    else
    {
      while ((double) pilotDismounted.hitPoints > 0.0)
      {
        if (pilotDismounted.slung)
        {
          uniTask = UniTask.Delay(5000, true);
          await uniTask;
          if (cancel.IsCancellationRequested)
          {
            gridSquares = (List<GridSquare>) null;
            parentAircraft = (Unit) null;
            cancel = new CancellationToken();
            return;
          }
        }
        else
        {
          Unit capturingUnit = (Unit) null;
          for (int index1 = 0; index1 < gridSquares.Count; ++index1)
          {
            for (int index2 = 0; index2 < gridSquares[index1].units.Count; ++index2)
            {
              Unit unit = gridSquares[index1].units[index2];
              if (!((UnityEngine.Object) unit == (UnityEngine.Object) null) && !unit.disabled && !((UnityEngine.Object) unit.NetworkHQ == (UnityEngine.Object) null) && (double) unit.radarAlt <= 3.0 && unit.definition.captureCapacity != 0 && !((UnityEngine.Object) unit == (UnityEngine.Object) parentAircraft))
              {
                Aircraft aircraft = unit as Aircraft;
                if ((!((UnityEngine.Object) aircraft != (UnityEngine.Object) null) || (double) aircraft.speed <= 1.0 && (double) aircraft.radarAlt <= (double) aircraft.definition.spawnOffset.y + 1.0) && !FastMath.OutOfRange(pilotDismounted.transform.position, unit.transform.position, 500f))
                {
                  capturingUnit = unit;
                  break;
                }
              }
            }
          }
          if ((UnityEngine.Object) capturingUnit != (UnityEngine.Object) null)
          {
            uniTask = UniTask.Delay(2000);
            await uniTask;
            if (cancel.IsCancellationRequested)
            {
              gridSquares = (List<GridSquare>) null;
              parentAircraft = (Unit) null;
              cancel = new CancellationToken();
              return;
            }
            pilotDismounted.Capture(capturingUnit);
            gridSquares = (List<GridSquare>) null;
            parentAircraft = (Unit) null;
            cancel = new CancellationToken();
            return;
          }
          uniTask = UniTask.Delay(5000, true);
          await uniTask;
          if (cancel.IsCancellationRequested)
          {
            gridSquares = (List<GridSquare>) null;
            parentAircraft = (Unit) null;
            cancel = new CancellationToken();
            return;
          }
          capturingUnit = (Unit) null;
        }
      }
      gridSquares = (List<GridSquare>) null;
      parentAircraft = (Unit) null;
      cancel = new CancellationToken();
    }
  }

  [Mirage.Server]
  public void Capture(Unit capturingUnit)
  {
    if (!this.IsServer)
      throw new MethodInvocationException("[Server] function 'Capture' called when server not active");
    this.captured = true;
    bool rescued = (UnityEngine.Object) capturingUnit.NetworkHQ == (UnityEngine.Object) this.NetworkHQ;
    if (!rescued)
      capturingUnit.NetworkHQ.AddScore((float) (2.0 * (1.0 + (double) this.pilotRank)));
    if (capturingUnit is Aircraft aircraft && (UnityEngine.Object) aircraft.Player != (UnityEngine.Object) null)
    {
      if (!rescued)
        this.NetworkHQ.ReportCapturePilotsAction(aircraft.Player, this);
      else
        this.NetworkHQ.ReportRescuePilotsAction(aircraft.Player, this);
    }
    NetworkSceneSingleton<MessageManager>.i.RpcPilotCaptureMessage(this.persistentID, rescued);
    this.Networkdisabled = true;
    UnityEngine.Object.Destroy((UnityEngine.Object) this.gameObject);
  }

  public override void UnitDisabled(bool oldState, bool newState)
  {
    base.UnitDisabled(oldState, newState);
    if (this.captured)
      return;
    if (Physics.Raycast(this.transform.position, -Vector3.up, out this.hit, float.MaxValue, 64 /*0x40*/))
      this.pilotCollider.enabled = false;
    if (!NetworkManagerNuclearOption.i.Server.Active)
      return;
    this.WaitDespawn().Forget();
  }

  private async UniTask WaitDespawn()
  {
    PilotDismounted pilotDismounted = this;
    await UniTask.Delay(60000);
    if ((UnityEngine.Object) pilotDismounted == (UnityEngine.Object) null)
      return;
    UnityEngine.Object.Destroy((UnityEngine.Object) pilotDismounted.gameObject);
  }

  public override bool IsSlung() => this.slung;

  private void Update()
  {
    this.timeSinceSpawn += Time.deltaTime;
    if (this.chuteDeployedLocal)
      this.chuteOpenTime += Time.deltaTime;
    if (!this.seatDetachedLocal && (double) this.timeSinceSpawn > 2.0 && (this.animationState == PilotDismounted.PilotState.dead || (double) this.radarAlt < 1000.0 && (double) this.rb.velocity.sqrMagnitude > 100.0))
    {
      this.DetachSeat();
      if (this.LocalSim)
        this.SetPilotState(PilotDismounted.PilotState.detaching);
    }
    if (!this.LocalSim || this.animationState == PilotDismounted.PilotState.dead || (double) this.chuteOpenTime <= 1.0 || (double) this.radarAlt <= 10.0 || this.slung)
      return;
    this.SetPilotState(PilotDismounted.PilotState.parachuting);
  }

  public override void CheckRadarAlt()
  {
    if ((double) Time.timeSinceLevelLoad <= (double) this.lastAltitudeCheck + 0.10000000149011612)
      return;
    this.lastAltitudeCheck = Time.timeSinceLevelLoad;
    Vector3 velocity = this.rb.velocity;
    if (Physics.Linecast(this.transform.position, this.transform.position - Vector3.up * 10000f, out this.hit, 2112))
    {
      this.radarAlt = this.hit.distance;
      if ((UnityEngine.Object) this.hit.collider.attachedRigidbody != (UnityEngine.Object) null)
        velocity -= this.hit.collider.attachedRigidbody.GetPointVelocity(this.hit.point);
    }
    else
      this.radarAlt = this.transform.position.GlobalY();
    if (this.inWater)
      this.radarAlt = 0.0f;
    this.speed = velocity.magnitude;
  }

  private void CheckLanded()
  {
    if ((UnityEngine.Object) this.NetworkHQ != (UnityEngine.Object) null && (this.inWater || (double) this.landedTime > 0.0) && !this.disabled && !this.checkingForCapture && this.IsServer)
      this.CheckForCapture().Forget();
    if ((double) this.landedTime <= 35.0 || !this.IsServer || this.slung)
      return;
    this.NetworkisKinematic = true;
  }

  public void DeployChute()
  {
    this.chuteDeployedLocal = true;
    this.NetworkchuteDeployedNetwork = true;
  }

  private void DetachSeat()
  {
    this.seatDetachedLocal = true;
    this.NetworkseatDetachedNetwork = true;
    if (!((UnityEngine.Object) this.ejectionSeat != (UnityEngine.Object) null))
      return;
    this.ejectionSeat.Detach();
  }

  private void KillPilot()
  {
    this.DisableUnit();
    this.SetPilotState(PilotDismounted.PilotState.dead);
  }

  private void FixedUpdate()
  {
    this.inWater = (double) this.transform.position.GlobalY() < 0.0;
    this.CheckRadarAlt();
    this.landedTime = (double) this.radarAlt < 2.0 ? this.landedTime + Time.deltaTime : 0.0f;
    if (this.LocalSim)
      this.LocalFixedUpdate();
    this.runWeight = Mathf.Lerp(this.runWeight, (float) ((double) this.speed * 2.0 - 0.5), 6f * Time.fixedDeltaTime);
    this.runWeight = Mathf.Clamp(this.runWeight, 0.0f, 1f);
    if (this.animationState == PilotDismounted.PilotState.landing && !this.inWater)
    {
      this.animator.SetLayerWeight(1, this.runWeight);
      this.animator.SetFloat("RunSpeed", this.speed * 0.25f);
    }
    else
      this.animator.SetLayerWeight(1, 0.0f);
  }

  private bool TooMuchForce()
  {
    return this.velocityPrev.HasValue && FastMath.OutOfRange(this.velocityPrev.Value, this.rb.velocity, 500f * Time.fixedDeltaTime);
  }

  private void LocalFixedUpdate()
  {
    this.CheckLanded();
    if ((double) this.speed > 30.0 && Physics.Raycast(this.transform.position, this.rb.velocity, out RaycastHit _, this.speed * 1.1f * Time.fixedDeltaTime, 64 /*0x40*/))
    {
      this.transform.position = this.hit.point + Vector3.up * 1f;
      this.rb.velocity = Vector3.Reflect(this.rb.velocity, this.hit.normal) * 0.3f;
    }
    if (this.inWater)
    {
      this.radarAlt = 0.0f;
      this.rb.AddForce(Vector3.up * this.rb.mass * 25f * Mathf.Clamp01(Datum.LocalSeaY - this.transform.position.y));
      this.rb.AddTorque(Vector3.Cross(this.transform.up, Vector3.up) * this.rb.mass * 8f, ForceMode.Force);
      if (this.animationState != PilotDismounted.PilotState.ejecting)
        this.SetPilotState(PilotDismounted.PilotState.ejecting);
    }
    if (!this.disabled && this.TooMuchForce())
      this.KillPilot();
    this.velocityPrev = new Vector3?(this.rb.velocity);
    Vector3 velocity = this.rb.velocity;
    this.rb.drag = 0.1f;
    if (this.animationState != PilotDismounted.PilotState.dead && (double) this.radarAlt < 20.0 && (double) this.speed < 10.0 && Physics.Linecast(this.transform.position, this.transform.position - Vector3.up * 1.5f, out this.hit, -8193))
    {
      Vector3 vector3 = (UnityEngine.Object) this.hit.collider.attachedRigidbody != (UnityEngine.Object) null ? this.hit.collider.attachedRigidbody.GetPointVelocity(this.hit.point) : Vector3.zero;
      this.rb.AddForce((float) (1.2999999523162842 - (double) this.hit.distance - (double) this.rb.velocity.y * 0.20000000298023224) * Vector3.up * this.rb.mass * 25f - (velocity - vector3) * this.rb.mass * 2f);
      this.rb.AddTorque(Vector3.Cross(this.transform.up, Vector3.up) * this.rb.mass * 4f, ForceMode.Force);
      if ((!((UnityEngine.Object) this.cockpitPart != (UnityEngine.Object) null) || !this.cockpitPart.parentUnit.disabled || this.cockpitPart.parentUnit.unitState == Unit.UnitState.Abandoned ? 0 : (this.cockpitPart.parentUnit.unitState != Unit.UnitState.Returned ? 1 : 0)) != 0 && FastMath.InRange(this.cockpitPart.transform.position, this.transform.position, 10f))
        this.rb.AddForce(((this.transform.position - this.cockpitPart.transform.position) with
        {
          y = 0.0f
        }).normalized * this.rb.mass * 8f);
      if (!this.slung && this.animationState != PilotDismounted.PilotState.landing)
        this.SetPilotState(PilotDismounted.PilotState.landing);
    }
    if (!this.inWater)
      return;
    this.rb.drag = 20f;
    this.rb.angularDrag = 5f;
  }

  public override void AttachOrDetachSlingHook(Aircraft aircraft, bool attached)
  {
    base.AttachOrDetachSlingHook(aircraft, attached);
    this.slung = attached;
    if (this.IsServer)
    {
      this.NetworkisKinematic = false;
      this.SetPilotState(PilotDismounted.PilotState.parachuting);
    }
    if (attached)
      return;
    this.inWater = false;
    this.landedTime = 0.0f;
  }

  public int GetPilotRank() => this.pilotRank;

  private void MirageProcessed()
  {
  }

  public PersistentID NetworkparentUnit
  {
    get => this.parentUnit;
    set
    {
      if (this.SyncVarEqual<PersistentID>(value, this.parentUnit))
        return;
      PersistentID parentUnit = this.parentUnit;
      this.parentUnit = value;
      this.SetDirtyBit(512UL /*0x0200*/);
    }
  }

  public byte NetworkunitPart
  {
    get => this.unitPart;
    set
    {
      if (this.SyncVarEqual<byte>(value, this.unitPart))
        return;
      byte unitPart = this.unitPart;
      this.unitPart = value;
      this.SetDirtyBit(1024UL /*0x0400*/);
    }
  }

  public byte NetworkpilotNumber
  {
    get => this.pilotNumber;
    set
    {
      if (this.SyncVarEqual<byte>(value, this.pilotNumber))
        return;
      byte pilotNumber = this.pilotNumber;
      this.pilotNumber = value;
      this.SetDirtyBit(2048UL /*0x0800*/);
    }
  }

  public PilotDismounted.PilotState NetworkanimationState
  {
    get => this.animationState;
    set
    {
      if (this.SyncVarEqual<PilotDismounted.PilotState>(value, this.animationState))
        return;
      PilotDismounted.PilotState animationState = this.animationState;
      this.animationState = value;
      this.SetDirtyBit(4096UL /*0x1000*/);
      if (!this.GetSyncVarHookGuard(4096UL /*0x1000*/) && this.IsHost)
      {
        this.SetSyncVarHookGuard(4096UL /*0x1000*/, true);
        this.PilotStateChanged(animationState, value);
        this.SetSyncVarHookGuard(4096UL /*0x1000*/, false);
      }
    }
  }

  public Player Networkplayer
  {
    get => (Player) this.player.Value;
    set
    {
      if (this.SyncVarEqual<Player>(value, (Player) this.player.Value))
        return;
      Player player = (Player) this.player.Value;
      this.player.Value = (NetworkBehaviour) value;
      this.SetDirtyBit(8192UL /*0x2000*/);
    }
  }

  public bool NetworkisKinematic
  {
    get => this.isKinematic;
    set
    {
      if (this.SyncVarEqual<bool>(value, this.isKinematic))
        return;
      bool isKinematic = this.isKinematic;
      this.isKinematic = value;
      this.SetDirtyBit(16384UL /*0x4000*/);
      if (!this.GetSyncVarHookGuard(16384UL /*0x4000*/) && this.IsHost)
      {
        this.SetSyncVarHookGuard(16384UL /*0x4000*/, true);
        this.OnKinematicChanged(isKinematic, value);
        this.SetSyncVarHookGuard(16384UL /*0x4000*/, false);
      }
    }
  }

  public bool NetworkchuteDeployedNetwork
  {
    get => this.chuteDeployedNetwork;
    set
    {
      if (this.SyncVarEqual<bool>(value, this.chuteDeployedNetwork))
        return;
      bool chuteDeployedNetwork = this.chuteDeployedNetwork;
      this.chuteDeployedNetwork = value;
      this.SetDirtyBit(32768UL /*0x8000*/);
    }
  }

  public bool NetworkseatDetachedNetwork
  {
    get => this.seatDetachedNetwork;
    set
    {
      if (this.SyncVarEqual<bool>(value, this.seatDetachedNetwork))
        return;
      bool seatDetachedNetwork = this.seatDetachedNetwork;
      this.seatDetachedNetwork = value;
      this.SetDirtyBit(65536UL /*0x010000*/);
    }
  }

  public override bool SerializeSyncVars(NetworkWriter writer, bool initialize)
  {
    ulong syncVarDirtyBits = this.SyncVarDirtyBits;
    bool flag = base.SerializeSyncVars(writer, initialize);
    if (initialize)
    {
      GeneratedNetworkCode._Write_PersistentID(writer, this.parentUnit);
      writer.WriteByteExtension(this.unitPart);
      writer.WriteByteExtension(this.pilotNumber);
      GeneratedNetworkCode._Write_PilotDismounted\u002FPilotState(writer, this.animationState);
      writer.WriteNetworkBehaviorSyncVar(this.player);
      writer.WriteBooleanExtension(this.isKinematic);
      writer.WriteBooleanExtension(this.chuteDeployedNetwork);
      writer.WriteBooleanExtension(this.seatDetachedNetwork);
      return true;
    }
    writer.Write(syncVarDirtyBits >> 9, 8);
    if (((long) syncVarDirtyBits & 512L /*0x0200*/) != 0L)
    {
      GeneratedNetworkCode._Write_PersistentID(writer, this.parentUnit);
      flag = true;
    }
    if (((long) syncVarDirtyBits & 1024L /*0x0400*/) != 0L)
    {
      writer.WriteByteExtension(this.unitPart);
      flag = true;
    }
    if (((long) syncVarDirtyBits & 2048L /*0x0800*/) != 0L)
    {
      writer.WriteByteExtension(this.pilotNumber);
      flag = true;
    }
    if (((long) syncVarDirtyBits & 4096L /*0x1000*/) != 0L)
    {
      GeneratedNetworkCode._Write_PilotDismounted\u002FPilotState(writer, this.animationState);
      flag = true;
    }
    if (((long) syncVarDirtyBits & 8192L /*0x2000*/) != 0L)
    {
      writer.WriteNetworkBehaviorSyncVar(this.player);
      flag = true;
    }
    if (((long) syncVarDirtyBits & 16384L /*0x4000*/) != 0L)
    {
      writer.WriteBooleanExtension(this.isKinematic);
      flag = true;
    }
    if (((long) syncVarDirtyBits & 32768L /*0x8000*/) != 0L)
    {
      writer.WriteBooleanExtension(this.chuteDeployedNetwork);
      flag = true;
    }
    if (((long) syncVarDirtyBits & 65536L /*0x010000*/) != 0L)
    {
      writer.WriteBooleanExtension(this.seatDetachedNetwork);
      flag = true;
    }
    return flag;
  }

  public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
  {
    base.DeserializeSyncVars(reader, initialState);
    if (initialState)
    {
      this.parentUnit = GeneratedNetworkCode._Read_PersistentID(reader);
      this.unitPart = reader.ReadByteExtension();
      this.pilotNumber = reader.ReadByteExtension();
      PilotDismounted.PilotState animationState = this.animationState;
      this.animationState = GeneratedNetworkCode._Read_PilotDismounted\u002FPilotState(reader);
      this.player = reader.ReadNetworkBehaviourSyncVar();
      bool isKinematic = this.isKinematic;
      this.isKinematic = reader.ReadBooleanExtension();
      this.chuteDeployedNetwork = reader.ReadBooleanExtension();
      this.seatDetachedNetwork = reader.ReadBooleanExtension();
      if (!this.IsServer && !this.SyncVarEqual<PilotDismounted.PilotState>(animationState, this.animationState))
        this.PilotStateChanged(animationState, this.animationState);
      if (this.IsServer || this.SyncVarEqual<bool>(isKinematic, this.isKinematic))
        return;
      this.OnKinematicChanged(isKinematic, this.isKinematic);
    }
    else
    {
      ulong dirtyBit = reader.Read(8);
      this.SetDeserializeMask(dirtyBit, 9);
      if (((long) dirtyBit & 1L) != 0L)
        this.parentUnit = GeneratedNetworkCode._Read_PersistentID(reader);
      if (((long) dirtyBit & 2L) != 0L)
        this.unitPart = reader.ReadByteExtension();
      if (((long) dirtyBit & 4L) != 0L)
        this.pilotNumber = reader.ReadByteExtension();
      if (((long) dirtyBit & 8L) != 0L)
      {
        PilotDismounted.PilotState animationState = this.animationState;
        this.animationState = GeneratedNetworkCode._Read_PilotDismounted\u002FPilotState(reader);
        if (!this.IsServer && !this.SyncVarEqual<PilotDismounted.PilotState>(animationState, this.animationState))
          this.PilotStateChanged(animationState, this.animationState);
      }
      if (((long) dirtyBit & 16L /*0x10*/) != 0L)
        this.player = reader.ReadNetworkBehaviourSyncVar();
      if (((long) dirtyBit & 32L /*0x20*/) != 0L)
      {
        bool isKinematic = this.isKinematic;
        this.isKinematic = reader.ReadBooleanExtension();
        if (!this.IsServer && !this.SyncVarEqual<bool>(isKinematic, this.isKinematic))
          this.OnKinematicChanged(isKinematic, this.isKinematic);
      }
      if (((long) dirtyBit & 64L /*0x40*/) != 0L)
        this.chuteDeployedNetwork = reader.ReadBooleanExtension();
      if (((long) dirtyBit & 128L /*0x80*/) == 0L)
        return;
      this.seatDetachedNetwork = reader.ReadBooleanExtension();
    }
  }

  public void UserCode_RpcTakeDamage_446604653(float damage)
  {
    this.ApplyDamage(damage, 0.0f, 0.0f, 0.0f);
  }

  protected static void Skeleton_RpcTakeDamage_446604653(
    NetworkBehaviour behaviour,
    NetworkReader reader,
    INetworkPlayer senderConnection,
    int replyId)
  {
    ((PilotDismounted) behaviour).UserCode_RpcTakeDamage_446604653(reader.ReadSingleConverter());
  }

  protected override int GetRpcCount() => 20;

  public override void RegisterRpc(RemoteCallCollection collection)
  {
    base.RegisterRpc(collection);
    collection.Register(19, "PilotDismounted.RpcTakeDamage", false, RpcInvokeType.ClientRpc, (NetworkBehaviour) this, new RpcDelegate(PilotDismounted.Skeleton_RpcTakeDamage_446604653));
  }

  public enum PilotState : byte
  {
    ejecting = 1,
    detaching = 2,
    parachuting = 3,
    landing = 4,
    dead = 5,
  }
}

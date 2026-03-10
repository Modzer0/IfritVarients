// Decompiled with JetBrains decompiler
// Type: Ship
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Cysharp.Threading.Tasks;
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
public class Ship : Unit, IRearmable, ICommandable
{
  private static readonly ProfilerMarker applyJobResultsMarker = new ProfilerMarker("Ship.ApplyJobResults");
  public List<ShipPart> parts = new List<ShipPart>();
  public List<ShipPart> criticalParts = new List<ShipPart>();
  public float criticalRatio = 0.5f;
  [SerializeField]
  private Ship.WakeParticles[] wakeParticles;
  [SerializeField]
  private AudioSource[] waterSounds;
  [SerializeField]
  private AudioSource[] hullSounds;
  [SerializeField]
  private float disabledDespawnDelay;
  [SerializeField]
  private UnitCommand unitCommand;
  public float damageControlAvailable;
  public float damageControlDeploymentThreshold = 0.2f;
  public float AllowedSteerRate = 1f;
  [SerializeField]
  private AudioSource collisionSource;
  [SerializeField]
  private AudioClip collisionClip;
  [NonSerialized]
  public float skill = 1f;
  [NonSerialized]
  public bool holdPosition;
  private ShipInputs inputs = new ShipInputs();
  private List<VehicleWaypoint> savedWaypoints = new List<VehicleWaypoint>();
  private bool canRearm;
  private Airbase attachedAirbase;
  [NonSerialized]
  private const int SYNC_VAR_COUNT = 9;
  [NonSerialized]
  private const int RPC_COUNT = 19;

  public event Action<RearmEventArgs> OnRearm;

  public event Action OnLaunch;

  public UnitCommand UnitCommand => this.unitCommand;

  bool ICommandable.Disabled => this.disabled;

  FactionHQ ICommandable.HQ => this.NetworkHQ;

  public override Airbase GetAirbase() => this.attachedAirbase;

  public bool CanRearm(bool aircraftRearm, bool vehicleRearm, bool shipRearm)
  {
    return this.canRearm & shipRearm;
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

  public override void Awake()
  {
    base.Awake();
    this.attachedAirbase = this.GetComponent<Airbase>();
    this.Identity.OnStartClient.AddListener(new Action(this.OnStartClient));
  }

  public override void OnEnable()
  {
    base.OnEnable();
    foreach (Ship.WakeParticles wakeParticle in this.wakeParticles)
      wakeParticle.Initialize((Unit) this);
    this.StartSlowUpdateDelayed(1f, new Action(this.UpdateParticles));
  }

  public void ReduceSteeringRate() => this.AllowedSteerRate = 0.1f;

  public void RestoreSteeringRate() => this.AllowedSteerRate = 1f;

  private void OnStartClient()
  {
    this.SetLocalSim(NetworkManagerNuclearOption.i.Server.Active);
    Vector3 localPosition = this.startPosition.ToLocalPosition();
    this.transform.position = localPosition;
    this.transform.rotation = this.startRotation;
    this.rb.MovePosition(localPosition);
    this.rb.MoveRotation(this.startRotation);
    this.RegisterUnit(new float?(4f));
    this.InitializeUnit();
    this.rb.ResetCenterOfMass();
    this.rb.ResetInertiaTensor();
    if (!this.LocalSim)
      return;
    JobManager.Add(this);
    this.StartSlowUpdateDelayed(2f, new Action(this.CheckShipBuoyancy));
    foreach (UnitPart criticalPart in this.criticalParts)
      criticalPart.onApplyDamage += new Action<UnitPart.OnApplyDamage>(this.Ship_OnCriticalPartDamage);
  }

  public void Launch()
  {
    Action onLaunch = this.OnLaunch;
    if (onLaunch == null)
      return;
    onLaunch();
  }

  public ShipInputs GetInputs() => this.inputs;

  public TargetDetector GetRadar() => this.radar;

  public override Transform GetRandomPart()
  {
    int index = Mathf.FloorToInt(UnityEngine.Random.Range(0.0f, (float) this.partLookup.Count - 0.0001f));
    return (UnityEngine.Object) this.partLookup[index] != (UnityEngine.Object) null ? this.partLookup[index].transform : this.transform;
  }

  private void Animate()
  {
    foreach (AudioSource waterSound in this.waterSounds)
    {
      float sqrMagnitude = this.rb.GetPointVelocity(waterSound.transform.position).sqrMagnitude;
      waterSound.volume = Mathf.Clamp01(sqrMagnitude * 0.005f);
    }
    foreach (AudioSource hullSound in this.hullSounds)
      hullSound.volume = 0.25f + Mathf.Clamp01(this.speed * 0.05f);
    if (!((UnityEngine.Object) this.collisionSource != (UnityEngine.Object) null) || !this.collisionSource.isPlaying)
      return;
    this.collisionSource.volume = Mathf.Lerp(this.collisionSource.volume, 0.0f, 0.05f);
    if ((double) this.collisionSource.volume >= 0.0099999997764825821)
      return;
    this.collisionSource.Stop();
  }

  private void UpdateParticles()
  {
    foreach (Ship.WakeParticles wakeParticle in this.wakeParticles)
      wakeParticle.Update(this.speed, this.rb.velocity);
  }

  public override void UnitDisabled(bool oldState, bool newState)
  {
    MissionManager.onObjectiveStarted -= new Action<Objective>(this.Ship_OnObjectiveStarted);
    foreach (ShipPart part in this.parts)
      part.Flood();
    this.inputs.throttle = 0.0f;
    this.inputs.steering = 0.0f;
    base.UnitDisabled(oldState, newState);
    if (!NetworkManagerNuclearOption.i.Server.Active)
      return;
    this.WaitRemove().Forget();
  }

  private async UniTask WaitRemove()
  {
    Ship ship = this;
    CancellationToken cancel = ship.destroyCancellationToken;
    await UniTask.Delay((int) ship.disabledDespawnDelay * 1000);
    if (cancel.IsCancellationRequested)
    {
      cancel = new CancellationToken();
    }
    else
    {
      UnityEngine.Object.Destroy((UnityEngine.Object) ship.gameObject);
      cancel = new CancellationToken();
    }
  }

  protected override void OnDestroy()
  {
    base.OnDestroy();
    JobManager.Remove(this);
    MissionManager.onObjectiveStarted -= new Action<Objective>(this.Ship_OnObjectiveStarted);
    foreach (ShipPart part in this.parts)
    {
      if ((UnityEngine.Object) part != (UnityEngine.Object) null)
        UnityEngine.Object.Destroy((UnityEngine.Object) part.gameObject);
    }
    if (!this.IsServer)
      return;
    foreach (UnitPart criticalPart in this.criticalParts)
      criticalPart.onApplyDamage -= new Action<UnitPart.OnApplyDamage>(this.Ship_OnCriticalPartDamage);
  }

  public void GetMissionWaypoints(SavedShip savedShip)
  {
    this.savedWaypoints = new List<VehicleWaypoint>((IEnumerable<VehicleWaypoint>) savedShip.waypoints);
    MissionManager.onObjectiveStarted += new Action<Objective>(this.Ship_OnObjectiveStarted);
    if (!(this.savedWaypoints[0].objective == "Unit Spawn"))
      return;
    this.unitCommand.SetDestination(this.savedWaypoints[0].position, false);
    this.savedWaypoints.RemoveAt(0);
  }

  private void Ship_OnObjectiveStarted(Objective objective)
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

  public void SetHoldPosition(bool enabled) => this.holdPosition = enabled;

  private void Update()
  {
    if (GameManager.ShowEffects)
      this.Animate();
    this.speed = this.rb.velocity.magnitude;
  }

  public void ApplyJobResults()
  {
    using (Ship.applyJobResultsMarker.Auto())
      this.ApplyPartsForce();
  }

  private void ApplyPartsForce()
  {
    Vector3 force = new Vector3();
    Vector3 torque = new Vector3();
    Vector3 vector3_1 = this.transform.TransformPoint(this.rb.centerOfMass);
    foreach (ShipPart part in this.parts)
    {
      if (!part.IsDetached() && part.JobFields.IsCreated)
      {
        ref ShipPartFields local = ref part.JobFields.Ref();
        Vector3 vector3_2 = local.forcePosition - vector3_1;
        Vector3 vector3_3 = Vector3.Cross(local.force, -vector3_2);
        force += local.force;
        torque += vector3_3;
      }
    }
    this.rb.AddForce(force);
    this.rb.AddTorque(torque);
  }

  private void CheckShipBuoyancy()
  {
    if (this.disabled || GameManager.gameState == GameState.Encyclopedia || (double) this.transform.position.y >= (double) Datum.LocalSeaY - (double) this.definition.spawnOffset.y && (double) Vector3.Dot(this.transform.up, Vector3.up) >= 0.5 && (double) Vector3.Dot(this.transform.forward, Vector3.up) <= 0.25)
      return;
    this.Networkdisabled = true;
    this.ReportKilled();
  }

  private void Ship_OnCriticalPartDamage(UnitPart.OnApplyDamage damageArgs)
  {
    if (this.disabled || (double) damageArgs.hitPoints > 0.0)
      return;
    int num = 0;
    foreach (ShipPart criticalPart in this.criticalParts)
    {
      if (criticalPart.IsCriticallyDamaged())
        ++num;
    }
    if ((double) num <= (double) this.criticalRatio * (double) this.criticalParts.Count)
      return;
    this.Networkdisabled = true;
    this.ReportKilled();
  }

  private void OnCollisionEnter(Collision collision)
  {
    if (!((UnityEngine.Object) this.collisionSource != (UnityEngine.Object) null) || !((UnityEngine.Object) collision.rigidbody == (UnityEngine.Object) null) && !collision.rigidbody.isKinematic && (double) collision.rigidbody.mass < (double) this.GetMass() || this.collisionSource.isPlaying)
      return;
    this.collisionSource.transform.position = collision.GetContact(0).point;
    this.collisionSource.time = this.collisionSource.clip.length * UnityEngine.Random.value;
    this.collisionSource.volume = 1f;
    this.collisionSource.pitch = UnityEngine.Random.Range(0.95f, 1.05f);
    this.collisionSource.Play();
    this.collisionSource.PlayOneShot(this.collisionClip, Mathf.Clamp01((float) (0.25 + (double) collision.relativeVelocity.sqrMagnitude * 0.0099999997764825821)));
  }

  private void OnCollisionStay()
  {
    if (!((UnityEngine.Object) this.collisionSource != (UnityEngine.Object) null) || !this.collisionSource.isPlaying || (double) this.speed <= 2.0)
      return;
    this.collisionSource.volume = Mathf.Lerp(this.collisionSource.volume, 1f, 0.1f);
  }

  private void MirageProcessed()
  {
  }

  protected override int GetRpcCount() => 19;

  [Serializable]
  private class WakeParticles
  {
    [SerializeField]
    private ParticleSystem system;
    private ParticleSystem.EmissionModule emit;
    private ParticleSystem.MainModule main;
    [SerializeField]
    private float minRate;
    [SerializeField]
    private float maxRate;
    [SerializeField]
    private float minOpacity;
    [SerializeField]
    private float maxOpacity;
    [SerializeField]
    private float minLife;
    [SerializeField]
    private float maxLife;
    [SerializeField]
    private float minSize;
    [SerializeField]
    private float maxSize;
    [SerializeField]
    private float minSpeed;
    [SerializeField]
    private float maxSpeed;
    private Unit parentUnit;

    public void Initialize(Unit parentUnit)
    {
      this.parentUnit = parentUnit;
      this.emit = this.system.emission;
      this.main = this.system.main;
      this.main.simulationSpace = ParticleSystemSimulationSpace.Custom;
      this.main.customSimulationSpace = Datum.origin;
      this.main.emitterVelocityMode = ParticleSystemEmitterVelocityMode.Custom;
    }

    public void Update(float speed, Vector3 velocity)
    {
      if ((UnityEngine.Object) this.system == (UnityEngine.Object) null)
        return;
      float t = Mathf.Clamp01((float) (((double) speed - (double) this.minSpeed) / ((double) this.maxSpeed - (double) this.minSpeed)));
      if ((double) t <= 0.0 && this.system.isPlaying)
        this.system.Stop();
      if ((double) t > 0.0 && !this.system.isPlaying)
        this.system.Play();
      this.emit.rateOverTime = (ParticleSystem.MinMaxCurve) Mathf.Lerp(this.minRate, this.maxRate, t);
      this.main.startRotation = (ParticleSystem.MinMaxCurve) (this.parentUnit.transform.eulerAngles.y * ((float) Math.PI / 180f));
      this.main.startColor = (ParticleSystem.MinMaxGradient) new Color(1f, 1f, 1f, Mathf.Lerp(this.minOpacity, this.maxOpacity, t));
      this.main.startSize = (ParticleSystem.MinMaxCurve) Mathf.Lerp(this.minSize, this.maxSize, t);
      this.main.startLifetime = (ParticleSystem.MinMaxCurve) Mathf.Lerp(this.minLife, this.maxLife, t);
      velocity.y = 0.0f;
      this.main.emitterVelocity = velocity;
    }
  }
}

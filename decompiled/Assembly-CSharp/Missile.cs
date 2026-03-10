// Decompiled with JetBrains decompiler
// Type: Missile
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Cysharp.Threading.Tasks;
using Mirage;
using Mirage.RemoteCalls;
using Mirage.Serialization;
using NuclearOption.Networking;
using System;
using System.Threading;
using UnityEngine;

#nullable disable
public class Missile : Unit, IDamageable, IRadarReturn
{
  [SyncVar]
  public PersistentID ownerID;
  [SyncVar(hook = "TargetIDChanged")]
  private PersistentID _targetID;
  [SyncVar]
  public Vector3 startingVelocity;
  [SyncVar]
  public Vector3 startOffsetFromOwner;
  [SyncVar]
  public Missile.SeekerMode seekerMode = Missile.SeekerMode.passive;
  private Unit target;
  [Header("Physics")]
  [SerializeField]
  private Missile.Motor[] motors;
  [SerializeField]
  private float mass;
  [SerializeField]
  private float finArea;
  [SerializeField]
  private float uprightPreference;
  [SerializeField]
  private float supersonicDrag;
  [SerializeField]
  private AnimationCurve liftCurve;
  [SerializeField]
  private AnimationCurve dragCurve;
  [SerializeField]
  private Missile.FoldingFin[] foldingFins;
  [SerializeField]
  private ArmorProperties armorProperties;
  private float hitpoints;
  private float engineCurrentThrust;
  private int motorStage;
  private Missile.Motor motor;
  [Header("Targeting")]
  [SerializeField]
  private PIDFactors PIDFactors;
  [SerializeField]
  private float torque;
  [SerializeField]
  private float maxTurnRate = 3f;
  private float pitchInput;
  private float rollInput;
  private float yawInput;
  private PID2D pid;
  [Header("Effects")]
  [SerializeField]
  private Transform effectsTransform;
  [SerializeField]
  private AudioSource flightSound;
  [SerializeField]
  private AudioClip nearbyDetonationClip;
  [SerializeField]
  private float basePitch = 0.5f;
  [SerializeField]
  private float pitchRange = 1f;
  [SerializeField]
  private float maxPitchSpeed = 340f;
  [Header("Payload")]
  [SerializeField]
  private Missile.Warhead warhead;
  [SerializeField]
  private float blastYield;
  [SerializeField]
  private float pierceDamage;
  [SerializeField]
  private bool proximityFuse;
  [SerializeField]
  private bool impactFuse;
  [SerializeField]
  private float impactFuseDelay;
  [Header("Unit")]
  [SerializeField]
  private WeaponInfo info;
  private float currentFinArea = 0.01f;
  private float throttle = 1f;
  private bool ignition;
  private bool tangible;
  private bool aimVelocity;
  private MissileSeeker seeker;
  [SerializeField]
  private GlobalPosition aimPoint;
  private Vector3 targetVel;
  private Vector3 torqueAxes;
  [NonSerialized]
  private const int SYNC_VAR_COUNT = 14;
  [NonSerialized]
  private const int RPC_COUNT = 21;

  public PersistentID targetID => this._targetID;

  public Unit owner { get; private set; }

  public float timeSinceSpawn { get; private set; }

  public event Action<Aircraft.OnRadarWarning> onRadarPing;

  public override void Awake()
  {
    base.Awake();
    this.seeker = this.gameObject.GetComponent<MissileSeeker>();
    this.Identity.OnStartClient.AddListener(new Action(this.OnStartClient));
    this.Identity.OnStartServer.AddListener(new Action(this.OnStartServer));
    this.SetTangible(false);
  }

  public override void OnEnable()
  {
    this.radarAlt = 1000f;
    this.hitpoints = 100f;
    this.currentFinArea = 0.1f * this.finArea;
    base.OnEnable();
    if (GameManager.gameState != GameState.Encyclopedia)
      return;
    this.enabled = false;
    this.rb.isKinematic = true;
  }

  public bool IsTangible() => this.tangible;

  public void SetTangible(bool tangible)
  {
    this.tangible = tangible;
    if (tangible)
      this.transform.gameObject.layer = LayerMask.NameToLayer("Default");
    else
      this.transform.gameObject.layer = LayerMask.NameToLayer("Ignore Collisions");
  }

  public WeaponInfo GetWeaponInfo() => this.info;

  public float GetYield() => this.blastYield;

  public float GetPierce() => this.pierceDamage;

  public float CalcDeltaV()
  {
    float num1 = 0.0f;
    float mass = this.mass;
    for (int index = 0; index < this.motors.Length; ++index)
    {
      Missile.Motor motor = this.motors[index];
      float num2 = (float) ((double) motor.thrust * (double) motor.burnTime / ((double) mass - ((double) mass - (double) motor.fuelMass)));
      num1 += num2 * Mathf.Log(mass / (mass - motor.fuelMass), 2.71828175f);
      mass -= motor.fuelMass;
    }
    return num1;
  }

  public float GetTopSpeed(float launchAltitude, float targetAltitude)
  {
    if (this.motors.Length == 0)
      return 0.0f;
    float b = this.CalcDeltaV();
    float dragCoef = this.GetDragCoef((float) Math.PI / 360f);
    float num = GameAssets.i.airDensityAltitude.Evaluate(Mathf.Lerp(launchAltitude, targetAltitude, 0.5f) * (1f / 1000f));
    Missile.Motor[] motors = this.motors;
    return Mathf.Min(Mathf.Sqrt(motors[motors.Length - 1].thrust / ((float) ((double) dragCoef * (double) num * 0.5) * this.finArea)), b);
  }

  public float CalcRange(
    float launchSpeed,
    float launchAltitude,
    float targetAltitude,
    float targetDist,
    float targetRelativeSpeed,
    out float noEscapeDistance)
  {
    float num1 = GameAssets.i.airDensityAltitude.Evaluate(Mathf.Lerp(launchAltitude, targetAltitude, 0.5f) * (1f / 1000f));
    float num2 = this.CalcDeltaV();
    float dragCoef = this.GetDragCoef((float) Math.PI / 360f);
    float minSpeed = this.GetComponent<MissileSeeker>().GetMinSpeed();
    noEscapeDistance = 0.0f;
    float num3 = 0.0f;
    float mass = this.mass;
    foreach (Missile.Motor motor in this.motors)
    {
      num3 += motor.burnTime;
      mass -= motor.fuelMass;
    }
    float num4 = -0.1f;
    if ((double) targetDist > 0.0)
      num4 = Mathf.Clamp((targetAltitude - launchAltitude) / targetDist, -0.5f, 0.5f);
    float num5 = Mathf.Abs(launchAltitude - targetAltitude);
    float num6 = 0.0f;
    float num7 = 0.0f;
    float num8 = launchSpeed;
    if (this.motors.Length != 0)
    {
      Missile.Motor[] motors = this.motors;
      float b1 = Mathf.Sqrt(motors[motors.Length - 1].thrust / ((float) ((double) dragCoef * (double) num1 * 0.5) * this.finArea));
      float b2 = Mathf.Min(launchSpeed + num2, b1);
      num6 = (double) this.GetTotalBurnTime() < 30.0 ? Mathf.Lerp(launchSpeed, b2, 0.5f) * num3 : b1 * num3;
      num8 = b2;
    }
    float num9 = 0.1f;
    int num10 = 0;
    float num11 = 0.5f * dragCoef * num1 * this.finArea / mass;
    bool flag = true;
    while (flag && num10 < 120)
    {
      ++num10;
      num6 += num9 * num8;
      num7 += num9;
      if ((double) num5 > 0.0)
      {
        double num12 = 0.5 * (double) mass * (double) num8 * (double) num8;
        float f = num9 * num4 * num8;
        num5 -= Mathf.Abs(f);
        double num13 = (double) mass * -9.8100004196167 * (double) f;
        num8 = Mathf.Sqrt(2f * (float) (num12 + num13) / mass);
      }
      num8 -= num9 * num8 * num8 * num11;
      num9 += 0.05f;
      if (num10 > 10)
      {
        if ((double) num8 < (double) minSpeed)
          flag = false;
        if ((double) noEscapeDistance == 0.0 && (double) num8 < (double) targetRelativeSpeed)
        {
          noEscapeDistance = num6;
          noEscapeDistance -= targetRelativeSpeed * num7;
          targetRelativeSpeed = 0.0f;
        }
      }
    }
    if ((double) noEscapeDistance <= 0.0)
      noEscapeDistance = num6 - num7 * targetRelativeSpeed;
    return num6;
  }

  private void OnStartServer() => this.OnStartNetwork();

  private void OnStartClient()
  {
    if (this.IsServer)
      return;
    this.OnStartNetwork();
  }

  private void OnStartNetwork()
  {
    this.StartMissile();
    if (!this.LocalSim)
      return;
    this.LocalStart();
  }

  private void StartMissile()
  {
    this.SetLocalSim(NetworkManagerNuclearOption.i.Server.Active);
    Unit foundOwner;
    if (UnitRegistry.TryGetUnit(new PersistentID?(this.ownerID), out foundOwner))
    {
      this.owner = foundOwner;
      this.owner.RegisterMissile(this);
    }
    else
      this.Arm();
    this.TargetIDChanged(PersistentID.None, this.targetID);
    if (this.LocalSim)
    {
      this.pid = new PID2D(this.PIDFactors, this.maxTurnRate, 3f);
      if ((UnityEngine.Object) foundOwner != (UnityEngine.Object) null)
        UniTask.Void((Func<UniTaskVoid>) (async () =>
        {
          await UniTask.Yield(PlayerLoopTiming.FixedUpdate);
          this.transform.position = foundOwner.transform.position + this.startOffsetFromOwner;
          this.rb.MovePosition(foundOwner.transform.position + this.startOffsetFromOwner);
        }));
    }
    else
    {
      this.rb.useGravity = false;
      if ((UnityEngine.Object) foundOwner != (UnityEngine.Object) null)
      {
        Vector3 position = this.startOffsetFromOwner + foundOwner.transform.position;
        this.transform.position = position;
        this.rb.MovePosition(position);
      }
      this.GetComponent<Collider>().enabled = false;
    }
    this.SetRB(this.gameObject.GetComponent<Rigidbody>());
    this.rb.mass = this.mass;
    this.rb.MoveRotation(this.startRotation);
    this.rb.velocity = this.startingVelocity;
    this.torqueAxes = this.rb.inertiaTensor * this.torque;
    this.RegisterUnit(new float?(1f));
    this.InitializeUnit();
    this.CheckExclusionZone();
    if (!((UnityEngine.Object) this.flightSound != (UnityEngine.Object) null))
      return;
    this.flightSound.volume = 0.0f;
    this.flightSound.Play();
  }

  private void CheckExclusionZone()
  {
    if (!this.LocalSim || (double) this.blastYield <= 100000.0 || !((UnityEngine.Object) this.target != (UnityEngine.Object) null))
      return;
    float num = Mathf.Pow(this.blastYield, 0.3333f) * 13f;
    GlobalPosition knownPosition;
    if (!this.NetworkHQ.TryGetKnownPosition(this.target, out knownPosition))
      return;
    this.NetworkHQ.AddExclusionZone((Unit) this, knownPosition, num * 2f);
  }

  private void LocalStart()
  {
    if (GameManager.gameState != GameState.SinglePlayer && GameManager.gameState != GameState.Multiplayer)
      return;
    this.aimPoint = ((UnityEngine.Object) this.owner != (UnityEngine.Object) null ? this.owner.transform.GlobalPosition() : this.transform.GlobalPosition()) + ((UnityEngine.Object) this.owner != (UnityEngine.Object) null ? this.owner.transform.forward : this.transform.forward) * 100000f;
    this.seeker.Initialize(this.target, this.aimPoint);
  }

  public ArmorProperties GetArmorProperties() => this.armorProperties;

  public Unit GetUnit() => (Unit) this;

  public Transform GetTransform() => this.transform;

  public void TakeShockwave(Vector3 origin, float overpressure, float blastPower)
  {
    double num1 = (double) Mathf.Sqrt(this.rb.mass);
    overpressure = Mathf.Min(overpressure, 100f);
    double num2 = (double) overpressure;
    float num3 = Mathf.Min((float) (num1 * num2 * 2.0), 30f * this.rb.mass);
    this.rb.AddForce(FastMath.NormalizedDirection(origin, this.transform.position) * num3, ForceMode.Impulse);
  }

  public override float GetMass() => this.mass;

  public override float GetPrefabMass() => this.mass;

  public float GetFinArea() => this.finArea;

  public float GetTorque() => this.torque;

  public float GetMaxTurnRate() => this.maxTurnRate;

  public void SetTorque(float torque, float maxTurnRate)
  {
    this.torque = torque;
    this.torqueAxes = this.rb.inertiaTensor * torque;
    if (!this.LocalSim)
      return;
    this.pid.SetPLimit(maxTurnRate);
  }

  public void SetAimVelocity(bool aimVelocity) => this.aimVelocity = aimVelocity;

  public void SetThrottle(float throttle) => this.throttle = throttle;

  public void DeployFins()
  {
    if (this.foldingFins.Length != 0)
      this.RpcUnfoldFins();
    else
      this.currentFinArea = this.finArea;
  }

  [ClientRpc]
  private void RpcUnfoldFins()
  {
    if (ClientRpcSender.ShouldInvokeLocally((NetworkBehaviour) this, RpcTarget.Observers, (INetworkPlayer) null, false))
      this.UserCode_RpcUnfoldFins_1465559174();
    PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
    ClientRpcSender.Send((NetworkBehaviour) this, 19, (NetworkWriter) writer, Mirage.Channel.Reliable, false);
    writer.Release();
  }

  private async UniTask UnfoldFins()
  {
    Missile missile = this;
    bool finishedDeploying = false;
    CancellationToken cancel = missile.destroyCancellationToken;
    while (!finishedDeploying)
    {
      missile.currentFinArea = Mathf.Lerp(missile.currentFinArea, missile.finArea, 0.1f);
      foreach (Missile.FoldingFin foldingFin in missile.foldingFins)
        finishedDeploying = foldingFin.UnfoldFin();
      await UniTask.Yield();
      if (cancel.IsCancellationRequested)
      {
        cancel = new CancellationToken();
        return;
      }
    }
    cancel = new CancellationToken();
  }

  public float GetDragCoef(float a) => this.dragCurve.Evaluate(a);

  public float GetTotalBurnTime()
  {
    float totalBurnTime = 0.0f;
    foreach (Missile.Motor motor in this.motors)
      totalBurnTime += motor.burnTime;
    return totalBurnTime;
  }

  public float GetRemainingBurnTime()
  {
    if (this.motor != null)
      return this.motor.GetRemainingBurnTime();
    return this.motors.Length == 0 ? 0.0f : this.motors[0].GetRemainingBurnTime();
  }

  public float GetRemainingDeltaV()
  {
    if (this.motor != null)
      return this.motor.GetRemainingDeltaV(this.rb.mass);
    return this.motors.Length == 0 ? 0.0f : this.motors[0].GetRemainingDeltaV(this.rb.mass);
  }

  public bool EngineOn() => (double) this.engineCurrentThrust > 0.0;

  public float GetLiftCoeff(float a) => this.liftCurve.Evaluate(a);

  public float GetThrust()
  {
    return this.motors.Length != 0 && this.motors[0] != null ? this.motors[0].thrust : 0.0f;
  }

  public float GetThrustDuration()
  {
    if (this.motors.Length == 0 || this.motors[0] == null)
      return 0.0f;
    float thrustDuration = 0.0f;
    for (int index = 0; index < this.motors.Length; ++index)
      thrustDuration += this.motors[index].burnTime;
    return thrustDuration;
  }

  public float GetMinRange(float launchSpeed)
  {
    if (this.motors.Length == 0 || this.motors[0] == null)
      return 0.0f;
    float dragCoef = this.GetDragCoef((float) Math.PI / 360f);
    float num1 = 0.0f;
    for (int index = 0; index < this.motors.Length; ++index)
      num1 += this.motors[index].burnTime;
    float num2 = this.CalcDeltaV();
    Missile.Motor[] motors = this.motors;
    float b = Mathf.Sqrt(motors[motors.Length - 1].thrust / ((float) ((double) dragCoef * (double) this.airDensity * 0.5) * this.finArea));
    return Mathf.Min(launchSpeed + num2, b) * num1;
  }

  public string GetSeekerType() => this.seeker.GetSeekerType();

  public GlobalPosition GetEvasionPoint() => this.seeker.GetEvasionPoint();

  public float GetRadarReturn(
    Vector3 source,
    Radar radar,
    Unit emitter,
    float dist,
    float clutter,
    RadarParams radarParameters,
    bool triggerWarning)
  {
    Vector3 direction = FastMath.NormalizedDirection(source, this.transform.position);
    Action<Aircraft.OnRadarWarning> onRadarPing = this.onRadarPing;
    if (onRadarPing != null)
      onRadarPing(new Aircraft.OnRadarWarning()
      {
        emitter = emitter,
        radar = radar
      });
    return radarParameters.GetSignalStrength(direction, dist, this.rb, this.RCS, clutter, 0.0f);
  }

  public float GetJammingIntensity() => 0.0f;

  public void TakeDamage(
    float pierceDamage,
    float blastDamage,
    float amountAffected,
    float fireDamage,
    float impactDamage,
    PersistentID dealerID)
  {
    if (this.disabled)
      return;
    if ((double) impactDamage > 0.0)
    {
      this.Detonate(-this.rb.velocity, false, false);
      NetworkSceneSingleton<MessageManager>.i.RpcBombFailMessage(this.persistentID, impactDamage / 9.81f);
    }
    if (dealerID == this.persistentID || dealerID == this.ownerID || dealerID.NotValid || this.disabled || (double) pierceDamage <= (double) this.armorProperties.pierceArmor && (double) blastDamage <= (double) this.armorProperties.blastArmor && (double) fireDamage <= (double) this.armorProperties.fireArmor)
      return;
    this.hitpoints -= Mathf.Max(pierceDamage - this.armorProperties.pierceArmor, 0.0f) / Mathf.Max(this.armorProperties.pierceTolerance, 0.1f) + Mathf.Max(blastDamage - this.armorProperties.blastArmor, 0.0f) * amountAffected / Mathf.Max(this.armorProperties.blastTolerance, 0.1f) + Mathf.Max(fireDamage - this.armorProperties.fireArmor, 0.0f) / Mathf.Max(this.armorProperties.fireTolerance, 0.1f);
    if ((double) this.hitpoints > 0.0)
      return;
    PersistentUnit persistentUnit;
    if (UnitRegistry.TryGetPersistentUnit(dealerID, out persistentUnit) && (UnityEngine.Object) persistentUnit.GetHQ() != (UnityEngine.Object) this.NetworkHQ)
    {
      this.RecordDamage(dealerID, 1000f);
      this.ReportKilled();
    }
    this.Detonate(this.rb.velocity, false, false);
  }

  public void Detach(Vector3 velocity, Vector3 relativePos)
  {
  }

  public void ApplyDamage(
    float pierceDamage,
    float blastDamage,
    float fireDamage,
    float impactDamage)
  {
  }

  public void SetTarget(Unit target)
  {
    PersistentID targetId = this.targetID;
    PersistentID persistentId1 = (UnityEngine.Object) target != (UnityEngine.Object) null ? target.persistentID : PersistentID.None;
    PersistentID persistentId2 = persistentId1;
    if (targetId == persistentId2)
      return;
    this.Network_targetID = persistentId1;
  }

  private void TargetIDChanged(PersistentID oldValue, PersistentID newValue)
  {
    if (!((UnityEngine.Object) this.NetworkHQ != (UnityEngine.Object) null))
      return;
    TrackingInfo trackingInfo1;
    if (oldValue.IsValid && this.NetworkHQ.trackingDatabase.TryGetValue(oldValue, out trackingInfo1))
      --trackingInfo1.missileAttacks;
    if (newValue.IsValid && UnitRegistry.TryGetUnit(new PersistentID?(newValue), out this.target))
    {
      if ((UnityEngine.Object) this.seeker != (UnityEngine.Object) null && this.seeker.triggerMissileWarning && this.target is Aircraft target)
        target.LockedByMissile(this);
      TrackingInfo trackingInfo2;
      if (!this.NetworkHQ.trackingDatabase.TryGetValue(newValue, out trackingInfo2))
        return;
      ++trackingInfo2.missileAttacks;
    }
    else
      this.target = (Unit) null;
  }

  public void UpdateRadarAlt()
  {
    this.radarAlt = Physics.Linecast(this.transform.position, this.transform.position - Vector3.up * 10000f, out this.hit, 2112) ? this.hit.distance : this.transform.position.GlobalY();
    this.radarAlt = Mathf.Min(this.radarAlt, this.transform.position.GlobalY());
  }

  public bool IsArmed() => this.warhead.Armed;

  public void Arm() => this.warhead.Arm();

  public void SetAimpoint(GlobalPosition aimPoint, Vector3 targetVel)
  {
    this.aimPoint = aimPoint;
    this.targetVel = targetVel;
  }

  private void Steering()
  {
    Vector3 other = this.aimPoint - this.transform.GlobalPosition();
    Vector3 self = this.rb.velocity;
    if (!this.aimVelocity)
      self = this.rb.velocity.normalized + this.transform.forward * (float) (1.1000000238418579 + (double) Vector3.Angle(this.rb.velocity, this.transform.forward) * 0.029999999329447746);
    float angleOnAxis1 = TargetCalc.GetAngleOnAxis(self, other, this.rb.transform.right);
    float angleOnAxis2 = TargetCalc.GetAngleOnAxis(self, other, this.rb.transform.up);
    float num = 0.0f;
    if ((double) this.uprightPreference > 0.0)
      num = TargetCalc.GetAngleOnAxis(this.transform.up, ((double) Vector3.Dot(this.transform.forward, Vector3.up) > 0.25 ? Vector3.up : Vector3.up + 0.03f * Mathf.Clamp(angleOnAxis2, -30f, 30f) * this.transform.right) - other.normalized * 0.5f, this.transform.forward) * this.uprightPreference;
    Vector3 vector3 = this.transform.InverseTransformVector(this.rb.angularVelocity);
    Vector2 output = this.pid.GetOutput(new Vector2(angleOnAxis1, angleOnAxis2), Time.fixedDeltaTime);
    this.pitchInput = Mathf.Clamp(output.x, -1f, 1f);
    this.yawInput = Mathf.Clamp(output.y, -1f, 1f);
    this.rollInput = (float) (-((double) vector3.z * 5.0) + (double) num * 0.30000001192092896);
  }

  private void ApplyAero()
  {
    Vector3 vector3_1 = this.rb.velocity - NetworkSceneSingleton<LevelInfo>.i.GetWind(this.transform.GlobalPosition());
    float sqrMagnitude = vector3_1.sqrMagnitude;
    Vector3 normalized = Vector3.Cross(Vector3.Cross(this.transform.forward, vector3_1), vector3_1).normalized;
    float time = (float) Math.PI / 180f * Vector3.Angle(this.transform.forward, vector3_1);
    double num1 = (double) this.liftCurve.Evaluate(time);
    float num2 = (float) ((double) this.dragCurve.Evaluate(time) * (double) this.airDensity * (double) sqrMagnitude * 0.5) * this.currentFinArea;
    double airDensity = (double) this.airDensity;
    float num3 = (float) (num1 * airDensity * (double) sqrMagnitude * -0.5) * this.currentFinArea;
    Vector3 vector3_2 = -vector3_1.normalized * num2;
    if ((double) this.supersonicDrag > 0.0)
    {
      float speedOfSound = LevelInfo.GetSpeedOfSound(this.transform.GlobalPosition().y);
      float b = 0.1f;
      if ((double) this.speed > (1.0 + (double) b) * (double) speedOfSound)
        vector3_2 *= 1f + this.supersonicDrag;
      else if ((double) this.speed > (1.0 - (double) b) * (double) speedOfSound)
      {
        float num4 = this.supersonicDrag + 0.15f;
        float num5 = Mathf.Min(Mathf.Abs((speedOfSound - this.speed) / speedOfSound), b);
        float num6 = (b - num5) / b;
        vector3_2 *= (float) (1.0 + (double) num6 * (double) num6 * (double) num6 * (double) num4);
      }
    }
    double num7 = (double) num3;
    this.rb.AddForce(normalized * (float) num7 + vector3_2);
    this.rb.AddTorque(this.torqueAxes.x * this.pitchInput * this.transform.right + this.torqueAxes.y * this.yawInput * this.transform.up + this.torqueAxes.z * this.rollInput * this.transform.forward);
  }

  public bool LosingGround()
  {
    return (double) Vector3.Dot(this.rb.velocity, this.rb.velocity - this.targetVel) < 0.0;
  }

  public bool MissedTarget()
  {
    return (double) Vector3.Dot(this.aimPoint - this.GlobalPosition(), this.rb.velocity) < 0.0;
  }

  private void DetectCollisions()
  {
    Vector3 velocity1;
    if ((double) this.transform.position.y < (double) Datum.LocalSeaY)
    {
      if (this.impactFuse && this.IsArmed())
      {
        this.Detonate(Vector3.up, false, false);
        this.rb.velocity = Vector3.zero;
      }
      else
      {
        if (this.motor != null)
          this.motor.Burnout(this);
        if ((double) this.speed > 30.0)
        {
          this.rb.angularDrag = 0.3f;
          Vector3 position = this.transform.position with
          {
            y = Datum.LocalSeaY
          };
          if ((double) UnityEngine.Random.value > 0.5)
            UnityEngine.Object.Destroy((UnityEngine.Object) UnityEngine.Object.Instantiate<GameObject>(GameAssets.i.rotorStrike_water, position, Quaternion.LookRotation(Vector3.up + new Vector3(this.rb.velocity.x, 0.0f, this.rb.velocity.z) * 0.1f)), 20f);
        }
        Rigidbody rb = this.rb;
        Vector3 velocity2 = rb.velocity;
        double num1 = 0.20000000298023224 * (double) Time.fixedDeltaTime;
        velocity1 = this.rb.velocity;
        double sqrMagnitude = (double) velocity1.sqrMagnitude;
        double num2 = num1 * sqrMagnitude;
        velocity1 = this.rb.velocity;
        Vector3 normalized = velocity1.normalized;
        Vector3 vector3 = Vector3.ClampMagnitude((float) num2 * normalized, this.speed);
        rb.velocity = velocity2 - vector3;
      }
    }
    int layerMask = this.tangible ? -8193 : 64 /*0x40*/;
    Vector3 vector3_1 = !((UnityEngine.Object) this.target != (UnityEngine.Object) null) || !((UnityEngine.Object) this.target.rb != (UnityEngine.Object) null) ? this.targetVel : this.target.rb.velocity;
    Vector3 b = (UnityEngine.Object) this.target != (UnityEngine.Object) null ? this.target.transform.position : this.aimPoint.ToLocalPosition();
    if (FastMath.InRange(this.transform.position, b, this.speed * 0.25f))
    {
      Vector3 vector3_2 = this.transform.position - b;
      Vector3 vector3_3 = this.rb.velocity - vector3_1;
      Vector3 rhs = vector3_2 + Time.fixedDeltaTime * 1.1f * vector3_3;
      RaycastHit hitInfo;
      if (Physics.Linecast(this.transform.position, this.transform.position + 1.1f * Time.fixedDeltaTime * vector3_3, out hitInfo, layerMask))
      {
        this.transform.position = hitInfo.point - vector3_3.normalized * 0.2f;
        this.rb.MovePosition(this.transform.position);
        if (this.impactFuse)
        {
          bool hitTerrain = (UnityEngine.Object) hitInfo.collider.sharedMaterial == (UnityEngine.Object) GameAssets.i.terrainMaterial;
          bool hitArmor = !hitTerrain;
          IDamageable component;
          if (!hitInfo.collider.gameObject.TryGetComponent<IDamageable>(out component) || !this.PenetrateObject(component, hitInfo.point, hitInfo.normal))
            this.Detonate(hitInfo.normal, hitArmor, hitTerrain);
        }
        if (this.rb.isKinematic)
          return;
        this.rb.velocity = Vector3.zero;
      }
      else
      {
        if (!this.proximityFuse || (double) Vector3.Dot(vector3_3, rhs) <= 0.0)
          return;
        this.transform.position += Vector3.Project(-vector3_2, vector3_3);
        this.rb.MovePosition(this.transform.position);
        this.Detonate(this.rb.velocity, false, false);
      }
    }
    else
    {
      RaycastHit hitInfo;
      if (!Physics.Linecast(this.transform.position, this.transform.position + 1.1f * Time.fixedDeltaTime * this.rb.velocity, out hitInfo, layerMask))
        return;
      Transform transform = this.transform;
      Vector3 point1 = hitInfo.point;
      velocity1 = this.rb.velocity;
      Vector3 vector3_4 = velocity1.normalized * 0.2f;
      Vector3 vector3_5 = point1 - vector3_4;
      transform.position = vector3_5;
      Rigidbody rb = this.rb;
      Vector3 point2 = hitInfo.point;
      velocity1 = this.rb.velocity;
      Vector3 vector3_6 = velocity1.normalized * 0.2f;
      Vector3 position = point2 - vector3_6;
      rb.MovePosition(position);
      Rigidbody attachedRigidbody = hitInfo.collider.attachedRigidbody;
      if ((UnityEngine.Object) attachedRigidbody != (UnityEngine.Object) null && !attachedRigidbody.isKinematic && FastMath.InRange(attachedRigidbody.velocity, this.rb.velocity, 100f))
        return;
      bool flag = (double) hitInfo.point.y < (double) Datum.LocalSeaY;
      bool hitTerrain = !flag && (UnityEngine.Object) hitInfo.collider.sharedMaterial == (UnityEngine.Object) GameAssets.i.terrainMaterial;
      if (this.impactFuse)
      {
        bool hitArmor = !flag && !hitTerrain;
        IDamageable component;
        if ((!hitInfo.collider.gameObject.TryGetComponent<IDamageable>(out component) || !this.PenetrateObject(component, hitInfo.point, hitInfo.normal)) && this.IsArmed())
          this.Detonate(hitInfo.normal, hitArmor, hitTerrain);
        this.rb.velocity = Vector3.zero;
      }
      else
      {
        if ((double) this.speed <= 10.0)
          return;
        this.rb.velocity = Vector3.Reflect(this.rb.velocity, hitInfo.normal) * 0.25f;
        UnityEngine.Object.Destroy(hitTerrain ? (UnityEngine.Object) UnityEngine.Object.Instantiate<GameObject>(GameAssets.i.rotorStrike_dirt, this.transform.position, Quaternion.LookRotation(Vector3.up + new Vector3(this.rb.velocity.x, 0.0f, this.rb.velocity.z) * 0.1f)) : (UnityEngine.Object) UnityEngine.Object.Instantiate<GameObject>(GameAssets.i.rotorStrike_solid, this.transform.position, Quaternion.LookRotation(Vector3.up + new Vector3(this.rb.velocity.x, 0.0f, this.rb.velocity.z) * 0.1f)), 20f);
      }
    }
  }

  private bool PenetrateObject(IDamageable damageable, Vector3 hitPoint, Vector3 hitNormal)
  {
    if ((double) this.impactFuseDelay == 0.0)
    {
      DamageEffects.ArmorPenetrate(hitPoint - this.transform.forward * 0.1f, this.transform.forward * 1000f, 1000f, this.pierceDamage, 0.0f, this.ownerID);
      return false;
    }
    if ((double) this.pierceDamage < (double) damageable.GetArmorProperties().pierceArmor)
      return false;
    this.impactFuse = false;
    Vector3 position = hitPoint + this.definition.length * 2f * this.rb.velocity.normalized;
    this.rb.MovePosition(position);
    this.transform.position = position;
    GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(GameAssets.i.rotorStrike_dirt, Datum.origin);
    gameObject.transform.SetPositionAndRotation(hitPoint, Quaternion.LookRotation(-this.rb.velocity));
    UnityEngine.Object.Destroy((UnityEngine.Object) gameObject, 10f);
    this.ImpactDelayedFuse(hitNormal, damageable, true, false).Forget();
    return true;
  }

  private async UniTask ImpactDelayedFuse(
    Vector3 normal,
    IDamageable penetratedObject,
    bool hitArmor,
    bool hitTerrain)
  {
    Missile missile = this;
    missile.warhead.Armed = false;
    missile.impactFuse = false;
    CancellationToken cancel = missile.destroyCancellationToken;
    await UniTask.Delay((int) ((double) missile.impactFuseDelay * 1000.0));
    if (cancel.IsCancellationRequested)
    {
      cancel = new CancellationToken();
    }
    else
    {
      missile.warhead.Armed = true;
      float blastArmorPrev = 0.0f;
      if (penetratedObject != null)
      {
        blastArmorPrev = penetratedObject.GetArmorProperties().blastArmor;
        penetratedObject.GetArmorProperties().blastArmor = 0.0f;
      }
      missile.Detonate(normal, hitArmor, hitTerrain);
      await UniTask.Delay(200);
      if (penetratedObject == null)
      {
        cancel = new CancellationToken();
      }
      else
      {
        penetratedObject.GetArmorProperties().blastArmor = blastArmorPrev;
        cancel = new CancellationToken();
      }
    }
  }

  public void Detonate(Vector3 normal, bool hitArmor, bool hitTerrain)
  {
    if (this.disabled)
      return;
    this.Networkdisabled = true;
    if ((UnityEngine.Object) this.target != (UnityEngine.Object) null && FastMath.InRange(this.transform.GlobalPosition(), this.target.GlobalPosition(), 200f))
      this.RpcDetonate(this.target, true, this.target.transform.InverseTransformPoint(this.transform.position), this.IsArmed(), hitArmor, hitTerrain, normal);
    else
      this.RpcDetonate((Unit) null, false, this.transform.GlobalPosition().AsVector3(), this.IsArmed(), hitArmor, hitTerrain, normal);
    this.SetTarget((Unit) null);
    this.DelayedDestroy(2f).Forget();
  }

  private async UniTask DelayedDestroy(float delay)
  {
    Missile missile = this;
    await UniTask.Delay((int) ((double) delay * 1000.0));
    if (!((UnityEngine.Object) missile != (UnityEngine.Object) null))
      return;
    UnityEngine.Object.Destroy((UnityEngine.Object) missile.gameObject);
  }

  public override void UnitDisabled(bool oldState, bool newState)
  {
    base.UnitDisabled(oldState, newState);
    if (!((UnityEngine.Object) this.owner != (UnityEngine.Object) null))
      return;
    this.owner.DeregisterMissile(this);
  }

  [ClientRpc]
  public void RpcDetonate(
    Unit relativeUnit,
    bool useUnit,
    Vector3 pos,
    bool armed,
    bool hitArmor,
    bool hitTerrain,
    Vector3 normal)
  {
    if (ClientRpcSender.ShouldInvokeLocally((NetworkBehaviour) this, RpcTarget.Observers, (INetworkPlayer) null, false))
      this.UserCode_RpcDetonate_897349600(relativeUnit, useUnit, pos, armed, hitArmor, hitTerrain, normal);
    PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
    GeneratedNetworkCode._Write_Unit((NetworkWriter) writer, relativeUnit);
    writer.WriteBooleanExtension(useUnit);
    writer.WriteVector3(pos);
    writer.WriteBooleanExtension(armed);
    writer.WriteBooleanExtension(hitArmor);
    writer.WriteBooleanExtension(hitTerrain);
    writer.WriteVector3(normal);
    ClientRpcSender.Send((NetworkBehaviour) this, 20, (NetworkWriter) writer, Mirage.Channel.Reliable, false);
    writer.Release();
  }

  private void Disappear()
  {
    this.GetComponent<Renderer>().enabled = false;
    this.SetTangible(false);
  }

  private async UniTask ExplosionForceOnPhysicsFrame(Transform relativeTransform, Vector3 pos)
  {
    Missile missile = this;
    await UniTask.WaitForFixedUpdate();
    Vector3 blastPosition = (UnityEngine.Object) relativeTransform != (UnityEngine.Object) null ? relativeTransform.TransformPoint(pos) : pos + Datum.origin.position;
    if (!((UnityEngine.Object) missile.target != (UnityEngine.Object) null))
    {
      Transform origin = Datum.origin;
    }
    else
    {
      Transform transform = missile.target.transform;
    }
    DamageEffects.BlastFrag(missile.blastYield, blastPosition, missile.ownerID, missile.persistentID);
  }

  private void MotorThrust()
  {
    if (this.motorStage >= this.motors.Length)
    {
      this.motor = (Missile.Motor) null;
    }
    else
    {
      this.motor = this.motors[this.motorStage];
      if ((double) this.motor.fuelMass <= 0.0)
      {
        this.engineCurrentThrust = 0.0f;
        ++this.motorStage;
        this.visibility = this.definition.visibleRange;
      }
      else
      {
        this.ignition = true;
        this.engineCurrentThrust = this.motor.Thrust(this, this.LocalSim, this.throttle);
        this.visibility = this.definition.visibleRange + Mathf.Sqrt(this.engineCurrentThrust) * 40f;
      }
    }
  }

  private void FixedUpdate()
  {
    this.MotorThrust();
    this.speed = this.rb.velocity.magnitude;
    this.timeSinceSpawn += Time.fixedDeltaTime;
    if ((UnityEngine.Object) this.flightSound != (UnityEngine.Object) null)
    {
      float num = Mathf.Clamp01((this.speed - 50f) / this.maxPitchSpeed);
      if ((double) this.flightSound.volume < (double) num)
        this.flightSound.volume += Time.deltaTime;
      else
        this.flightSound.volume = num;
      this.flightSound.pitch = this.pitchRange * num + this.basePitch;
    }
    if (this.disabled || !this.LocalSim)
      return;
    this.ServerFixedUpdate();
  }

  private void ServerFixedUpdate()
  {
    this.airDensity = GameAssets.i.airDensityAltitude.Evaluate(this.rb.transform.position.GlobalY() * (1f / 1000f));
    if ((UnityEngine.Object) this.seeker != (UnityEngine.Object) null)
      this.seeker.Seek();
    this.Steering();
    this.ApplyAero();
    this.DetectCollisions();
  }

  public void ApplyTerminalBoost(float value)
  {
    if (this.motor == null)
      return;
    this.motor.TerminalBoost(value);
  }

  private void MirageProcessed()
  {
  }

  public PersistentID NetworkownerID
  {
    get => this.ownerID;
    set
    {
      if (this.SyncVarEqual<PersistentID>(value, this.ownerID))
        return;
      PersistentID ownerId = this.ownerID;
      this.ownerID = value;
      this.SetDirtyBit(512UL /*0x0200*/);
    }
  }

  public PersistentID Network_targetID
  {
    get => this._targetID;
    set
    {
      if (this.SyncVarEqual<PersistentID>(value, this._targetID))
        return;
      PersistentID targetId = this._targetID;
      this._targetID = value;
      this.SetDirtyBit(1024UL /*0x0400*/);
      if (!this.GetSyncVarHookGuard(1024UL /*0x0400*/) && this.IsHost)
      {
        this.SetSyncVarHookGuard(1024UL /*0x0400*/, true);
        this.TargetIDChanged(targetId, value);
        this.SetSyncVarHookGuard(1024UL /*0x0400*/, false);
      }
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
      this.SetDirtyBit(2048UL /*0x0800*/);
    }
  }

  public Vector3 NetworkstartOffsetFromOwner
  {
    get => this.startOffsetFromOwner;
    set
    {
      if (this.SyncVarEqual<Vector3>(value, this.startOffsetFromOwner))
        return;
      Vector3 startOffsetFromOwner = this.startOffsetFromOwner;
      this.startOffsetFromOwner = value;
      this.SetDirtyBit(4096UL /*0x1000*/);
    }
  }

  public Missile.SeekerMode NetworkseekerMode
  {
    get => this.seekerMode;
    set
    {
      if (this.SyncVarEqual<Missile.SeekerMode>(value, this.seekerMode))
        return;
      Missile.SeekerMode seekerMode = this.seekerMode;
      this.seekerMode = value;
      this.SetDirtyBit(8192UL /*0x2000*/);
    }
  }

  public override bool SerializeSyncVars(NetworkWriter writer, bool initialize)
  {
    ulong syncVarDirtyBits = this.SyncVarDirtyBits;
    bool flag = base.SerializeSyncVars(writer, initialize);
    if (initialize)
    {
      GeneratedNetworkCode._Write_PersistentID(writer, this.ownerID);
      GeneratedNetworkCode._Write_PersistentID(writer, this._targetID);
      writer.WriteVector3(this.startingVelocity);
      writer.WriteVector3(this.startOffsetFromOwner);
      GeneratedNetworkCode._Write_Missile\u002FSeekerMode(writer, this.seekerMode);
      return true;
    }
    writer.Write(syncVarDirtyBits >> 9, 5);
    if (((long) syncVarDirtyBits & 512L /*0x0200*/) != 0L)
    {
      GeneratedNetworkCode._Write_PersistentID(writer, this.ownerID);
      flag = true;
    }
    if (((long) syncVarDirtyBits & 1024L /*0x0400*/) != 0L)
    {
      GeneratedNetworkCode._Write_PersistentID(writer, this._targetID);
      flag = true;
    }
    if (((long) syncVarDirtyBits & 2048L /*0x0800*/) != 0L)
    {
      writer.WriteVector3(this.startingVelocity);
      flag = true;
    }
    if (((long) syncVarDirtyBits & 4096L /*0x1000*/) != 0L)
    {
      writer.WriteVector3(this.startOffsetFromOwner);
      flag = true;
    }
    if (((long) syncVarDirtyBits & 8192L /*0x2000*/) != 0L)
    {
      GeneratedNetworkCode._Write_Missile\u002FSeekerMode(writer, this.seekerMode);
      flag = true;
    }
    return flag;
  }

  public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
  {
    base.DeserializeSyncVars(reader, initialState);
    if (initialState)
    {
      this.ownerID = GeneratedNetworkCode._Read_PersistentID(reader);
      PersistentID targetId = this._targetID;
      this._targetID = GeneratedNetworkCode._Read_PersistentID(reader);
      this.startingVelocity = reader.ReadVector3();
      this.startOffsetFromOwner = reader.ReadVector3();
      this.seekerMode = GeneratedNetworkCode._Read_Missile\u002FSeekerMode(reader);
      if (this.IsServer || this.SyncVarEqual<PersistentID>(targetId, this._targetID))
        return;
      this.TargetIDChanged(targetId, this._targetID);
    }
    else
    {
      ulong dirtyBit = reader.Read(5);
      this.SetDeserializeMask(dirtyBit, 9);
      if (((long) dirtyBit & 1L) != 0L)
        this.ownerID = GeneratedNetworkCode._Read_PersistentID(reader);
      if (((long) dirtyBit & 2L) != 0L)
      {
        PersistentID targetId = this._targetID;
        this._targetID = GeneratedNetworkCode._Read_PersistentID(reader);
        if (!this.IsServer && !this.SyncVarEqual<PersistentID>(targetId, this._targetID))
          this.TargetIDChanged(targetId, this._targetID);
      }
      if (((long) dirtyBit & 4L) != 0L)
        this.startingVelocity = reader.ReadVector3();
      if (((long) dirtyBit & 8L) != 0L)
        this.startOffsetFromOwner = reader.ReadVector3();
      if (((long) dirtyBit & 16L /*0x10*/) == 0L)
        return;
      this.seekerMode = GeneratedNetworkCode._Read_Missile\u002FSeekerMode(reader);
    }
  }

  private void UserCode_RpcUnfoldFins_1465559174() => this.UnfoldFins().Forget();

  protected static void Skeleton_RpcUnfoldFins_1465559174(
    NetworkBehaviour behaviour,
    NetworkReader reader,
    INetworkPlayer senderConnection,
    int replyId)
  {
    ((Missile) behaviour).UserCode_RpcUnfoldFins_1465559174();
  }

  public void UserCode_RpcDetonate_897349600(
    Unit relativeUnit,
    bool useUnit,
    Vector3 pos,
    bool armed,
    bool hitArmor,
    bool hitTerrain,
    Vector3 normal)
  {
    if (this.motor != null)
      this.motor.Destruct(this);
    if ((UnityEngine.Object) this.effectsTransform != (UnityEngine.Object) null)
    {
      this.effectsTransform.SetParent(Datum.origin);
      UnityEngine.Object.Destroy((UnityEngine.Object) this.effectsTransform.gameObject, 20f);
    }
    if ((UnityEngine.Object) this.flightSound != (UnityEngine.Object) null)
    {
      this.flightSound.Stop();
      this.flightSound.clip = this.nearbyDetonationClip;
      this.flightSound.pitch = 1f;
      this.flightSound.volume = 1f;
      this.flightSound.dopplerLevel = 1f;
      this.flightSound.loop = false;
      this.flightSound.Play();
    }
    if (armed)
    {
      this.Disappear();
      this.rb.isKinematic = true;
    }
    Vector3 zero = Vector3.zero;
    Vector3 position = !((UnityEngine.Object) relativeUnit != (UnityEngine.Object) null) ? pos + Datum.origin.position : relativeUnit.transform.TransformPoint(pos);
    this.transform.position = position;
    this.rb.MovePosition(position);
    this.warhead.Detonate(this.rb, this.ownerID, position, normal, armed, this.blastYield, hitArmor, hitTerrain);
    if ((double) this.blastYield > 200.0)
      return;
    this.ExplosionForceOnPhysicsFrame((UnityEngine.Object) relativeUnit != (UnityEngine.Object) null ? relativeUnit.transform : (Transform) null, pos).Forget();
  }

  protected static void Skeleton_RpcDetonate_897349600(
    NetworkBehaviour behaviour,
    NetworkReader reader,
    INetworkPlayer senderConnection,
    int replyId)
  {
    ((Missile) behaviour).UserCode_RpcDetonate_897349600(GeneratedNetworkCode._Read_Unit(reader), reader.ReadBooleanExtension(), reader.ReadVector3(), reader.ReadBooleanExtension(), reader.ReadBooleanExtension(), reader.ReadBooleanExtension(), reader.ReadVector3());
  }

  protected override int GetRpcCount() => 21;

  public override void RegisterRpc(RemoteCallCollection collection)
  {
    base.RegisterRpc(collection);
    collection.Register(19, "Missile.RpcUnfoldFins", false, RpcInvokeType.ClientRpc, (NetworkBehaviour) this, new RpcDelegate(Missile.Skeleton_RpcUnfoldFins_1465559174));
    collection.Register(20, "Missile.RpcDetonate", false, RpcInvokeType.ClientRpc, (NetworkBehaviour) this, new RpcDelegate(Missile.Skeleton_RpcDetonate_897349600));
  }

  [Serializable]
  public class FoldingFin
  {
    [SerializeField]
    private Transform fin;
    [SerializeField]
    private Vector3 foldAngle;
    [SerializeField]
    private Vector3 deployAngle;
    [SerializeField]
    private float deploySpeed = 1f;
    private float deployedAmount;

    public bool UnfoldFin()
    {
      this.deployedAmount = Mathf.Min(this.deployedAmount + Time.fixedDeltaTime * this.deploySpeed, 1f);
      this.fin.transform.localEulerAngles = Vector3.Lerp(this.foldAngle, this.deployAngle, this.deployedAmount);
      return (double) this.deployedAmount == 1.0;
    }
  }

  public enum SeekerMode : byte
  {
    activeLock = 1,
    activeSearch = 2,
    passive = 3,
  }

  [Serializable]
  public class Warhead
  {
    public bool Armed = true;
    [SerializeField]
    private GameObject airEffect;
    [SerializeField]
    private GameObject armorEffect;
    [SerializeField]
    private GameObject terrainEffect;
    [SerializeField]
    private GameObject waterSurfaceEffect;
    [SerializeField]
    private GameObject underwaterEffect;
    [SerializeField]
    private GameObject fizzleEffect;
    private bool detonated;

    public void Arm() => this.Armed = true;

    public void Detonate(
      Rigidbody rb,
      PersistentID ownerID,
      Vector3 position,
      Vector3 normal,
      bool armed,
      float blastYield,
      bool hitArmor,
      bool hitTerrain)
    {
      if (this.detonated)
        return;
      this.detonated = true;
      if (!armed)
      {
        if (!((UnityEngine.Object) this.fizzleEffect != (UnityEngine.Object) null))
          return;
        UnityEngine.Object.Instantiate<GameObject>(this.fizzleEffect, Datum.origin).transform.SetPositionAndRotation(rb.position, FastMath.LookRotation(rb.velocity));
      }
      else
      {
        float num1 = Mathf.Pow(blastYield, 0.3333f) * 2f;
        GameObject gameObject1 = (GameObject) null;
        int num2 = (double) position.y < (double) Datum.LocalSeaY + 0.10000000149011612 ? 1 : 0;
        Vector3 position1 = new Vector3(position.x, Datum.LocalSeaY, position.z);
        if (num2 != 0)
        {
          gameObject1 = UnityEngine.Object.Instantiate<GameObject>(this.underwaterEffect, Datum.origin);
          gameObject1.transform.SetPositionAndRotation(position1, Quaternion.identity);
        }
        else
        {
          if (hitTerrain)
          {
            gameObject1 = UnityEngine.Object.Instantiate<GameObject>(this.terrainEffect, Datum.origin);
            gameObject1.transform.SetPositionAndRotation(position, Quaternion.LookRotation(normal));
          }
          if (hitArmor)
          {
            gameObject1 = UnityEngine.Object.Instantiate<GameObject>(this.armorEffect, Datum.origin);
            gameObject1.transform.SetPositionAndRotation(position, Quaternion.LookRotation(normal));
          }
          RaycastHit hitInfo;
          bool flag = hitTerrain || Physics.Linecast(position, position - Vector3.up * num1, out hitInfo, 64 /*0x40*/) && (double) hitInfo.point.y > (double) Datum.LocalSeaY;
          if ((UnityEngine.Object) this.waterSurfaceEffect != (UnityEngine.Object) null && !flag && (double) position.y < (double) Datum.LocalSeaY + (double) num1 && (double) position.y > (double) Datum.LocalSeaY + 1.0)
          {
            GameObject gameObject2 = UnityEngine.Object.Instantiate<GameObject>(this.waterSurfaceEffect, Datum.origin);
            gameObject2.transform.SetPositionAndRotation(position1, Quaternion.identity);
            UnityEngine.Object.Destroy((UnityEngine.Object) gameObject2, 30f);
          }
        }
        if ((UnityEngine.Object) gameObject1 == (UnityEngine.Object) null)
        {
          gameObject1 = UnityEngine.Object.Instantiate<GameObject>(this.airEffect, Datum.origin);
          gameObject1.transform.SetPositionAndRotation(position, FastMath.LookRotation(rb.velocity));
        }
        if ((double) blastYield > 200.0)
        {
          Shockwave componentInChildren = gameObject1.GetComponentInChildren<Shockwave>();
          if (!((UnityEngine.Object) componentInChildren != (UnityEngine.Object) null))
            return;
          componentInChildren.SetOwner(ownerID, blastYield * 1E-06f);
        }
        else
          UnityEngine.Object.Destroy((UnityEngine.Object) gameObject1, 30f);
      }
    }
  }

  [Serializable]
  private class Motor
  {
    [SerializeField]
    private bool activated;
    [SerializeField]
    private float delayTimer;
    public float thrust;
    public float burnTime;
    public float fuelMass;
    public float IR_intensity = 1f;
    private float burnRate;
    [SerializeField]
    private ParticleSystem[] particleSystems;
    [SerializeField]
    private TrailEmitter[] trailEmitters;
    [SerializeField]
    private AudioSource[] audioSources;
    [SerializeField]
    private AudioSource startupSource;
    [SerializeField]
    private Light[] lights;
    [SerializeField]
    private GameObject[] destructEffects;

    private void Activate(Missile missile)
    {
      if (!missile.HasIRSignature())
        missile.AddIRSource(new IRSource(missile.transform, this.IR_intensity, false));
      this.burnRate = this.fuelMass / this.burnTime;
      foreach (ParticleSystem particleSystem in this.particleSystems)
        particleSystem.Play();
      foreach (AudioSource audioSource in this.audioSources)
        audioSource.Play();
      foreach (Behaviour light in this.lights)
        light.enabled = true;
    }

    public void Destruct(Missile missile)
    {
      this.Burnout(missile);
      if ((double) missile.transform.position.y > (double) Datum.LocalSeaY)
      {
        foreach (GameObject destructEffect in this.destructEffects)
          UnityEngine.Object.Instantiate<GameObject>(destructEffect, missile.transform);
      }
      foreach (AudioSource audioSource in this.audioSources)
        audioSource.Stop();
      if (!((UnityEngine.Object) this.startupSource != (UnityEngine.Object) null))
        return;
      this.startupSource.Stop();
    }

    public void Burnout(Missile missile)
    {
      foreach (ParticleSystem particleSystem in this.particleSystems)
        particleSystem.Stop();
      foreach (AudioSource audioSource in this.audioSources)
      {
        if (audioSource.loop)
          audioSource.Stop();
      }
      foreach (Behaviour light in this.lights)
        light.enabled = false;
    }

    public float Thrust(Missile missile, bool localSim, float throttle = 1f)
    {
      if ((double) this.delayTimer > 0.0)
      {
        this.delayTimer -= Time.deltaTime;
        if ((UnityEngine.Object) this.startupSource != (UnityEngine.Object) null && !this.startupSource.isPlaying)
          this.startupSource.Play();
        return 0.0f;
      }
      if (!this.activated)
      {
        this.activated = true;
        this.Activate(missile);
      }
      this.fuelMass -= this.burnRate * Time.deltaTime;
      missile.rb.mass -= this.burnRate * Time.deltaTime;
      if ((double) this.fuelMass <= 0.0)
        this.Burnout(missile);
      if (localSim)
        missile.rb.AddForce(missile.transform.forward * this.thrust * throttle);
      return this.thrust;
    }

    public float GetRemainingDeltaV(float currentMass)
    {
      if ((double) this.burnRate == 0.0)
        this.burnRate = this.fuelMass / this.burnTime;
      float num = currentMass - this.fuelMass;
      return (float) ((double) this.thrust * (double) (this.fuelMass / this.burnRate) / (((double) currentMass + (double) num) * 0.5));
    }

    public float GetRemainingBurnTime()
    {
      if ((double) this.burnRate == 0.0)
        this.burnRate = this.fuelMass / this.burnTime;
      return this.fuelMass / this.burnRate;
    }

    public void TerminalBoost(float value) => this.thrust = value;
  }
}

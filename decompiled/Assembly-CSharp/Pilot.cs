// Decompiled with JetBrains decompiler
// Type: Pilot
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Cysharp.Threading.Tasks;
using Mirage;
using NuclearOption.Jobs;
using NuclearOption.Networking;
using System;
using System.Threading;
using UnityEngine;

#nullable disable
public class Pilot : MonoBehaviour, IDamageable
{
  public Pilot.PilotType pilotType;
  public Pilot.ExitDirection exitDirection;
  public PilotBaseState currentState;
  public PilotPlayerState playerState = new PilotPlayerState();
  public PilotParkedState parkedState = new PilotParkedState();
  public AIPilotTaxiState AITaxiState;
  public AIPilotTakeoffState AITakeoffState;
  public AIPilotCombatModes AICombatState;
  public AIPilotLandingState AILandingState;
  public AIHeloTakeoffState AIHeloTakeoffState;
  public AIHeloCombatState AIHeloCombatState;
  public AIHeloLandingState AIHeloLandingState;
  public AIHeloTransportState AIHeloTransportState;
  public bool playerControlled;
  public bool dead;
  public bool ejected;
  public Aircraft aircraft;
  public Player player;
  private Unit primaryTarget;
  public Collider pilotCollider;
  [SerializeField]
  private SkinnedMeshRenderer skinnedMeshRenderer;
  [SerializeField]
  private Animator animator;
  [SerializeField]
  private UnitPart unitPart;
  [SerializeField]
  private bool ejectionSeat = true;
  public Vector3 accel = Vector3.zero;
  public Vector3 velocityPrev = Vector3.zero;
  public float gForce;
  private float hitPoints = 100f;
  [SerializeField]
  private ArmorProperties armorProperties;
  public RelaxedStabilityController relaxedStabilityController;
  [SerializeField]
  private ControlsFilter autoTrimmer;
  public Pilot.FlightInfo flightInfo = new Pilot.FlightInfo();
  private byte pilotNumber;
  private byte index;

  public event Action onFire;

  public event Action onEject;

  public event Action onSwitchWeapon;

  public event Action On10sCheck;

  public event Action On5sCheck;

  public event Action On2sCheck;

  public event Action On1sCheck;

  public event Action On100msCheck;

  public void Awake()
  {
    this.flightInfo.spawnTime = Time.timeSinceLevelLoad;
    this.index = this.aircraft.RegisterDamageable((IDamageable) this);
    this.aircraft.onInitialize += new Action(this.Pilot_OnInitialize);
    JobManager.Add(this);
    int num = (UnityEngine.Object) this.aircraft.pilots[0] != (UnityEngine.Object) this ? 1 : 0;
  }

  private void OnDestroy() => JobManager.Remove(this);

  public bool HasEjectionSeat() => this.ejectionSeat;

  public Rigidbody GetRB() => this.unitPart.rb;

  public string GetCurrentState()
  {
    if (this.currentState == null)
      return "null";
    return this.dead ? "dead" : this.currentState.GetCurrentState();
  }

  public Vector3 GetAccel() => this.accel;

  public Transform GetTransform() => this.transform;

  public void TakeShockwave(Vector3 origin, float blastEffectScale, float blastPower)
  {
  }

  public void Detach(Vector3 velocity, Vector3 relativePos)
  {
  }

  public void SwitchState(PilotBaseState state)
  {
    if (this.currentState == state)
      return;
    this.currentState?.LeaveState();
    this.currentState = state;
    this.currentState?.EnterState(this);
  }

  public void SwitchStateNew(PilotBaseState state)
  {
    this.currentState?.LeaveState();
    this.currentState = state;
    this.currentState?.EnterState(this);
  }

  public void AssignPilotNumber(byte index) => this.pilotNumber = index;

  public void SetPrimaryTarget(Unit target) => this.primaryTarget = target;

  public Unit GetPrimaryTarget() => this.primaryTarget;

  public void TogglePilotVisibility(bool enabled) => this.skinnedMeshRenderer.enabled = enabled;

  private void SlowCheck_10s()
  {
    Action on10sCheck = this.On10sCheck;
    if (on10sCheck == null)
      return;
    on10sCheck();
  }

  private void SlowCheck_5s()
  {
    Action on5sCheck = this.On5sCheck;
    if (on5sCheck == null)
      return;
    on5sCheck();
  }

  private void SlowCheck_2s()
  {
    Action on2sCheck = this.On2sCheck;
    if (on2sCheck == null)
      return;
    on2sCheck();
  }

  private void SlowCheck_1s()
  {
    Action on1sCheck = this.On1sCheck;
    if (on1sCheck == null)
      return;
    on1sCheck();
  }

  private void Pilot_OnInitialize()
  {
    this.player = this.aircraft.Player;
    if ((UnityEngine.Object) this.aircraft.pilots[0] != (UnityEngine.Object) this || GameManager.gameState == GameState.Editor || GameManager.gameState == GameState.Encyclopedia)
      return;
    if ((UnityEngine.Object) this.player == (UnityEngine.Object) null && this.aircraft.IsServer)
      this.SetStartingAiState();
    if (!GameManager.IsLocalPlayer<Player>(this.player))
      return;
    this.SwitchState((PilotBaseState) this.playerState);
  }

  protected virtual void SetStartingAiState()
  {
    if (!this.aircraft.IsServer)
      throw new MethodInvocationException("SetStartingAiState called when server is not active");
    if ((UnityEngine.Object) this.aircraft.NetworkHQ == (UnityEngine.Object) null)
    {
      this.SwitchState((PilotBaseState) this.parkedState);
    }
    else
    {
      bool flag = (double) this.aircraft.radarAlt > (double) this.aircraft.definition.spawnOffset.y + 1.0;
      this.StartSlowUpdate(10f, new Action(this.SlowCheck_10s));
      this.StartSlowUpdate(5f, new Action(this.SlowCheck_5s));
      this.StartSlowUpdate(2f, new Action(this.SlowCheck_2s));
      this.StartSlowUpdate(1f, new Action(this.SlowCheck_1s));
      if (this.pilotType == Pilot.PilotType.Plane)
      {
        this.AITaxiState = new AIPilotTaxiState();
        this.AITakeoffState = new AIPilotTakeoffState();
        this.AICombatState = new AIPilotCombatModes(this.aircraft);
        this.AILandingState = new AIPilotLandingState();
        this.SwitchState(flag ? (PilotBaseState) this.AICombatState : (PilotBaseState) this.AITaxiState);
      }
      else
      {
        if (this.pilotType != Pilot.PilotType.Helo && this.pilotType != Pilot.PilotType.Tiltwing)
          return;
        this.AIHeloTakeoffState = new AIHeloTakeoffState();
        this.AIHeloCombatState = new AIHeloCombatState(this);
        this.AIHeloLandingState = new AIHeloLandingState();
        this.SwitchState(flag ? (PilotBaseState) this.AIHeloCombatState : (PilotBaseState) this.AIHeloTakeoffState);
      }
    }
  }

  public void Fire() => this.aircraft.weaponManager.Fire();

  public void Remove() => this.gameObject.SetActive(false);

  public UnitPart GetUnitPart() => this.unitPart;

  public float GetMass() => 0.0f;

  public void NextWeapon() => this.aircraft.weaponManager.NextWeaponStation();

  public void PreviousWeapon() => this.aircraft.weaponManager.PreviousWeaponStation();

  public void TakeDamage(
    float pierceDamage,
    float blastDamage,
    float amountAffected,
    float fireDamage,
    float impactDamage,
    PersistentID dealerID)
  {
    float pierceDamage1 = Mathf.Max(pierceDamage - this.armorProperties.pierceArmor, 0.0f) / (this.armorProperties.pierceTolerance + 1f);
    float blastDamage1 = (float) ((double) Mathf.Max(blastDamage - this.armorProperties.blastArmor, 0.0f) * (double) amountAffected / ((double) this.armorProperties.blastTolerance + 1.0));
    float fireDamage1 = Mathf.Max(fireDamage - this.armorProperties.fireArmor, 0.0f) / (this.armorProperties.fireTolerance + 1f);
    if ((double) this.hitPoints <= 0.0 || (double) pierceDamage1 + (double) blastDamage1 + (double) fireDamage1 + (double) impactDamage <= 0.0)
      return;
    this.aircraft.Damage(this.index, new DamageInfo(pierceDamage1, blastDamage1, fireDamage1, impactDamage));
  }

  public void ApplyDamage(
    float pierceDamage,
    float blastDamage,
    float fireDamage,
    float impactDamage)
  {
    if (this.ejected || this.dead)
      return;
    float damage = pierceDamage + blastDamage + fireDamage + impactDamage;
    this.hitPoints -= damage;
    if (GameManager.IsLocalAircraft(this.aircraft))
      SceneSingleton<GameplayUI>.i.FlashHurt(damage, this.hitPoints);
    if ((double) this.hitPoints >= 0.0)
      return;
    this.dead = true;
    this.animator.SetLayerWeight(1, 0.0f);
    this.animator.SetInteger("PilotState", 6);
    if (this.pilotNumber != (byte) 0 || GameManager.gameState == GameState.Encyclopedia)
      return;
    if (GameManager.IsLocalAircraft(this.aircraft) && !MissionHelper.CanRespawn)
      GameManager.FinishGame(GameResolution.Defeat);
    if (GameManager.IsLocalPlayer<Player>(this.aircraft.Player))
    {
      SceneSingleton<CameraStateManager>.i.SetFollowingUnit((Unit) null);
      MusicManager.i.CrossFadeMusic(GameAssets.i.deathSound, 2f, 0.0f, false, true, true);
    }
    if (this.aircraft.IsServer)
    {
      this.aircraft.DisableUnit();
      this.CommandEjection().Forget();
    }
    this.SwitchState((PilotBaseState) null);
  }

  public Unit GetUnit() => (Unit) this.aircraft;

  public void TakeGForceDamage(float sqrGForces)
  {
    float num = (float) (((double) sqrGForces - 400.0) * 0.0070000002160668373);
    this.aircraft.Damage(this.index, new DamageInfo(0.0f, 0.0f, 0.0f, num));
    if (!GameManager.IsLocalPlayer<Player>(this.player))
      return;
    SceneSingleton<GameplayUI>.i.FlashHurt(num, this.hitPoints);
  }

  public void SetEjected()
  {
    this.ejected = true;
    JobManager.Remove(this);
  }

  public void TakeWaterDamage(float damage)
  {
    this.aircraft.Damage(this.index, new DamageInfo(0.0f, 0.0f, 0.0f, damage));
  }

  public ArmorProperties GetArmorProperties() => this.armorProperties;

  private void Update()
  {
    if (this.currentState == null)
      return;
    this.currentState.UpdateState(this);
  }

  private async UniTask CommandEjection()
  {
    Pilot pilot1 = this;
    CancellationToken cancel = pilot1.destroyCancellationToken;
    await UniTask.Delay(1000);
    if (cancel.IsCancellationRequested)
    {
      cancel = new CancellationToken();
    }
    else
    {
      int num = 0;
      foreach (Pilot pilot2 in pilot1.aircraft.pilots)
        num += pilot2.dead ? 0 : 1;
      if (num <= 0)
      {
        cancel = new CancellationToken();
      }
      else
      {
        pilot1.aircraft.StartEjectionSequence();
        cancel = new CancellationToken();
      }
    }
  }

  public PartResult Pilot_OnAeroInputsApplied()
  {
    if (this.aircraft.remoteSim)
      return PartResult.None;
    if (this.dead || this.ejected)
      return PartResult.Remove;
    if ((double) this.transform.position.y < (double) Datum.LocalSeaY - 10.0)
    {
      this.TakeWaterDamage(1000f);
      return PartResult.Remove;
    }
    this.accel = this.velocityPrev == Vector3.zero ? Vector3.zero : this.unitPart.rb.velocity - this.velocityPrev;
    this.velocityPrev = this.unitPart.rb.velocity;
    this.accel /= Time.fixedDeltaTime * 9.81f;
    this.gForce = Vector3.Dot(this.accel, this.transform.up);
    float magnitude = this.accel.magnitude;
    if ((double) magnitude > 20.0)
      this.TakeGForceDamage(magnitude * magnitude);
    this.currentState?.FixedUpdateState(this);
    return PartResult.None;
  }

  public enum PilotType
  {
    Plane,
    Helo,
    Tiltwing,
    VTOL,
  }

  public enum ExitDirection
  {
    Left,
    Right,
  }

  public class FlightInfo
  {
    public float spawnTime;
    public bool HasTakenOff;
    public bool EnemyContact;
    public bool DeliveredCargo;
    public float LastCargoDelivery;
  }
}

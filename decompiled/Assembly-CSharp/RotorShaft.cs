// Decompiled with JetBrains decompiler
// Type: RotorShaft
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

#nullable disable
public class RotorShaft : 
  MonoBehaviour,
  IEngine,
  IThrustSource,
  IPowerSource,
  IReportDamage,
  IPowerOutput
{
  public Aircraft aircraft;
  [SerializeField]
  private Transmission transmission;
  [SerializeField]
  private Transform hubRotator;
  [SerializeField]
  private UnitPart unitPart;
  [Header("Rotors")]
  [SerializeField]
  private SwashRotor[] rotors;
  [SerializeField]
  private float bladeLength;
  [SerializeField]
  private float bladeMass;
  [SerializeField]
  private float flapSpringUp;
  [SerializeField]
  private float flapSpringDown;
  [SerializeField]
  private float flapDamp;
  [SerializeField]
  private float liftNumber;
  [SerializeField]
  private float stallAngle;
  [SerializeField]
  private float dragBase;
  [SerializeField]
  private float dragExponent;
  [SerializeField]
  private float washout;
  [SerializeField]
  private float foldSpeed;
  [SerializeField]
  private bool detachOnEject;
  [SerializeField]
  private RotorShaft.RotorHinge[] rotorHinges;
  [Header("SwashPlate")]
  [SerializeField]
  private float hubRadius;
  [SerializeField]
  private float cyclicTravel = 5f;
  [SerializeField]
  private float collectiveTravel = 8f;
  [SerializeField]
  private float collectiveMin;
  [SerializeField]
  private float phaseLag;
  [SerializeField]
  private float yawCollective = 4f;
  private Transform swashPlate;
  [Header("Physics")]
  public bool debug;
  [SerializeField]
  private RotorShaft.TurnDirection turnDirection;
  [SerializeField]
  private int frameSamples = 4;
  [SerializeField]
  private int radialSamples = 3;
  [SerializeField]
  private float torqueLimit;
  [SerializeField]
  private float shaftFriction;
  [SerializeField]
  private float nominalPower = 2000f;
  [SerializeField]
  private float nominalRPM = 280f;
  [SerializeField]
  private float maxInputRate = 20f;
  [SerializeField]
  private float VRSThreshold = 4f;
  [SerializeField]
  private float VRSStrength = 1f;
  private ForceAndTorque forceAndTorque;
  [Header("Audio")]
  [SerializeField]
  private AudioSource rotorSource;
  [SerializeField]
  private AudioClip exteriorSound;
  [SerializeField]
  private AudioClip interiorSound;
  [SerializeField]
  private float volume = 1f;
  [SerializeField]
  private float pitch = 1f;
  private bool disengaged;
  private bool unfolded;
  private bool detached;
  private bool damageReported;
  private int directionMult = 1;
  private float angularPosition;
  private float angularSpeed;
  private float angularSpeedNominal;
  private float angularSpeedLimit;
  private float angularSpeedRatio;
  private float availablePower;
  private float torqueFromEngine;
  private float torqueFromRotors;
  private float momentOfInertia;
  private float currentThrust;
  private float downdraft;
  private float groundEffect;
  private float VRSSmoothed;
  private float rotorDiskArea;
  private float condition = 1f;
  private float swashPlatePitch;
  private float swashPlateRoll;
  private float swashPlateCollective;
  private float startupProgress;
  private ControlInputs inputs;
  [SerializeField]
  private string failureMessage;
  [SerializeField]
  private AudioClip failureMessageAudio;
  [SerializeField]
  private AudioClip rotorStrikeSound;
  private AudioSource strikeSource;
  private Transform xform;

  public event Action OnEngineDisable;

  public event Action OnEngineDamage;

  public event Action<OnReportDamage> onReportDamage;

  private void Awake()
  {
    if (this.turnDirection == RotorShaft.TurnDirection.anticlockwise)
      this.directionMult = -1;
    this.swashPlate = new GameObject("swashPlate").transform;
    this.swashPlate.transform.SetParent(this.transform);
    this.swashPlate.transform.localPosition = Vector3.zero;
    this.swashPlate.transform.localEulerAngles = new Vector3(0.0f, this.phaseLag, 0.0f);
    foreach (SwashRotor rotor in this.rotors)
      rotor.Setup(this, this.aircraft.RegisterDamageable((IDamageable) rotor), (float) this.directionMult, this.bladeLength, this.bladeMass, this.flapSpringUp, this.flapSpringDown, this.flapDamp, this.cyclicTravel, this.collectiveTravel, this.washout, this.radialSamples);
    this.xform = this.transform;
    float distanceFromAxis = this.rotors[0].GetDistanceFromAxis();
    this.aircraft.onInitialize += new Action(this.RotorShaft_OnSpawnedInPosition);
    if (this.detachOnEject)
      this.aircraft.onEject += new Action(this.RotorShaft_OnEject);
    this.inputs = this.aircraft.GetInputs();
    this.rotorDiskArea = 3.14159274f * this.bladeLength * this.bladeLength;
    this.unitPart.onParentDetached += new Action<UnitPart>(this.RotorShaft_OnDetach);
    this.rotorSource.time = UnityEngine.Random.Range(0.0f, this.rotorSource.clip.length);
    this.angularSpeedNominal = this.nominalRPM * 0.10472f;
    this.angularSpeedLimit = this.angularSpeedNominal * 1.06f;
    float num = this.bladeLength + distanceFromAxis;
    this.momentOfInertia = (float) (0.33333000540733337 * (double) this.rotors.Length * (double) this.bladeMass * ((double) num * (double) num));
    this.angularPosition = this.hubRotator.transform.localEulerAngles.y * ((float) Math.PI / 180f);
    this.aircraft.engines.Add((IEngine) this);
  }

  public float GetRPM() => this.angularSpeed * 9.55414f;

  public float GetRPMRatio() => this.angularSpeed / this.angularSpeedNominal;

  public float GetMaxRPM() => this.nominalRPM * 1.06f;

  public float GetVRSFactor() => this.VRSSmoothed;

  public float GetTorque() => this.torqueFromEngine;

  public float GetMaxThrust() => 0.0f;

  public float GetThrust() => this.currentThrust;

  public float GetPower() => this.availablePower;

  public float GetMaxPower() => this.nominalPower;

  public void Throttle(float throttle)
  {
  }

  public void SendPower(float power) => this.availablePower = power * this.condition;

  public UnitPart GetUnitPart() => this.unitPart;

  public void ReportDamage()
  {
    if (this.damageReported)
      return;
    this.damageReported = true;
    Action<OnReportDamage> onReportDamage = this.onReportDamage;
    if (onReportDamage == null)
      return;
    onReportDamage(new OnReportDamage()
    {
      failureMessage = this.failureMessage,
      audioReport = this.failureMessageAudio
    });
  }

  private void RotorShaft_OnEject()
  {
    this.aircraft.onEject -= new Action(this.RotorShaft_OnEject);
    foreach (SwashRotor rotor in this.rotors)
      rotor.ShatterRotor();
  }

  public void SetInteriorSounds(bool useInteriorSound)
  {
    if (this.rotorSource.isPlaying)
    {
      if (useInteriorSound)
      {
        this.rotorSource.Stop();
        this.rotorSource.clip = this.interiorSound;
        this.rotorSource.time = UnityEngine.Random.Range(0.0f, this.rotorSource.clip.length);
        this.rotorSource.Play();
      }
      else
      {
        this.rotorSource.Stop();
        this.rotorSource.clip = this.exteriorSound;
        this.rotorSource.time = UnityEngine.Random.Range(0.0f, this.rotorSource.clip.length);
        this.rotorSource.Play();
      }
    }
    else if (useInteriorSound)
      this.rotorSource.clip = this.interiorSound;
    else
      this.rotorSource.clip = this.exteriorSound;
  }

  private void RotorShaft_OnSpawnedInPosition()
  {
    if ((double) this.aircraft.radarAlt > (double) this.aircraft.definition.spawnOffset.y + 1.0)
    {
      this.angularSpeed = this.angularSpeedLimit * 1.1f;
      this.unfolded = true;
      this.startupProgress = 1f;
      foreach (RotorShaft.RotorHinge rotorHinge in this.rotorHinges)
        rotorHinge.Initialize(this.foldSpeed, 0.0f);
    }
    else
    {
      this.startupProgress = 0.0f;
      this.unfolded = false;
      foreach (RotorShaft.RotorHinge rotorHinge in this.rotorHinges)
        rotorHinge.Initialize(this.foldSpeed, 1f);
      this.UnfoldRotors().Forget();
    }
  }

  private void RotorShaft_OnDetach(UnitPart part)
  {
    this.detached = true;
    this.rotorSource.Stop();
    this.condition = 0.0f;
    this.shaftFriction *= 10f;
  }

  private void AnimateRotor()
  {
    this.rotorSource.pitch = this.detached ? 0.0f : this.angularSpeedRatio * this.pitch;
    this.rotorSource.volume = this.detached ? 0.0f : this.angularSpeedRatio * this.volume * this.condition;
    foreach (SwashRotor rotor in this.rotors)
      rotor.BendSegments(this.angularSpeedRatio);
    this.hubRotator.Rotate(new Vector3(0.0f, (float) ((double) this.angularSpeed * (double) this.directionMult * 57.295780181884766) * Time.deltaTime, 0.0f), Space.Self);
  }

  private async UniTask UnfoldRotors()
  {
    RotorShaft rotorShaft = this;
    CancellationToken cancel = rotorShaft.destroyCancellationToken;
    UniTask uniTask = UniTask.Delay(1000);
    await uniTask;
    if (cancel.IsCancellationRequested)
      cancel = new CancellationToken();
    else if (rotorShaft.rotorHinges.Length == 0)
    {
      rotorShaft.unfolded = true;
      cancel = new CancellationToken();
    }
    else
    {
      while ((double) rotorShaft.aircraft.rb.velocity.y > 1.0)
      {
        uniTask = UniTask.Delay(1000);
        await uniTask;
        if (cancel.IsCancellationRequested)
        {
          cancel = new CancellationToken();
          return;
        }
      }
      bool hingesMoving = true;
      while (hingesMoving)
      {
        hingesMoving = false;
        foreach (RotorShaft.RotorHinge rotorHinge in rotorShaft.rotorHinges)
          hingesMoving = rotorHinge.Unfold() | hingesMoving;
        await UniTask.Yield();
        if (cancel.IsCancellationRequested)
        {
          cancel = new CancellationToken();
          return;
        }
      }
      rotorShaft.unfolded = true;
      cancel = new CancellationToken();
    }
  }

  public void RotorStrike(float impactTorque)
  {
    this.ReportDamage();
    double angularSpeed = (double) this.angularSpeed;
    this.angularSpeed -= Mathf.Sign(this.angularSpeed) * impactTorque / this.momentOfInertia;
    if ((double) Mathf.Sign((float) angularSpeed) != (double) Mathf.Sign(this.angularSpeed))
      this.angularSpeed = 0.0f;
    if ((double) Mathf.Abs(this.angularSpeed) < 4.0)
      return;
    if (this.aircraft.LocalSim)
      this.condition = Mathf.Max(this.condition - 0.5f, 0.0f);
    if ((UnityEngine.Object) this.strikeSource == (UnityEngine.Object) null)
    {
      this.strikeSource = this.gameObject.AddComponent<AudioSource>();
      this.strikeSource.outputAudioMixerGroup = SoundManager.i.HeavyEffectsMixer;
      this.strikeSource.bypassListenerEffects = true;
      this.strikeSource.bypassEffects = true;
      this.strikeSource.spatialBlend = 1f;
      this.strikeSource.dopplerLevel = 1f;
      this.strikeSource.spread = 5f;
      this.strikeSource.maxDistance = 500f;
      this.strikeSource.minDistance = 10f;
      this.strikeSource.volume = 1f;
      this.strikeSource.priority = 128 /*0x80*/;
      this.strikeSource.clip = this.rotorStrikeSound;
    }
    this.strikeSource.pitch = UnityEngine.Random.Range(0.8f, 1.2f) * this.GetRPM() / this.nominalRPM;
    this.strikeSource.volume = Mathf.Clamp01(this.strikeSource.pitch);
    this.strikeSource.Play();
  }

  private void RotorPhysics()
  {
    if (!this.aircraft.disabled)
    {
      this.swashPlatePitch += Mathf.Clamp(this.inputs.pitch - this.swashPlatePitch, -this.maxInputRate * Time.fixedDeltaTime, this.maxInputRate * Time.fixedDeltaTime);
      this.swashPlatePitch = Mathf.Clamp(this.swashPlatePitch, -1f, 1f);
      this.swashPlateRoll += Mathf.Clamp(this.inputs.roll - this.swashPlateRoll, -this.maxInputRate * Time.fixedDeltaTime, this.maxInputRate * Time.fixedDeltaTime);
      this.swashPlateRoll = Mathf.Clamp(this.swashPlateRoll, -1f, 1f);
      float num = this.inputs.throttle + this.inputs.yaw * this.yawCollective;
      if (!this.unfolded)
        num = 0.0f;
      this.swashPlateCollective += Mathf.Clamp(num - this.swashPlateCollective, -this.maxInputRate * Time.fixedDeltaTime, this.maxInputRate * Time.fixedDeltaTime);
      this.swashPlateCollective = Mathf.Clamp(this.swashPlateCollective + this.collectiveMin, this.collectiveMin, 1.1f);
    }
    this.forceAndTorque.Clear();
    Vector3 vector3_1 = (UnityEngine.Object) NetworkSceneSingleton<LevelInfo>.i != (UnityEngine.Object) null ? NetworkSceneSingleton<LevelInfo>.i.GetWind(this.unitPart.xform.GlobalPosition()) : Vector3.zero;
    Vector3 vector3_2 = vector3_1 + this.unitPart.rb.velocity;
    Vector3 airVelocity = vector3_1 - this.downdraft * this.xform.up - this.VRSSmoothed * this.VRSStrength * this.xform.up;
    float deltaTime = Time.fixedDeltaTime / (float) this.frameSamples;
    this.torqueFromRotors = 0.0f;
    this.angularPosition += this.angularSpeed * (float) this.directionMult * Time.fixedDeltaTime;
    if ((double) Mathf.Abs(this.angularPosition) > 6.2831854820251465)
      this.angularPosition -= 6.28318548f * Mathf.Sign(this.angularPosition);
    this.hubRotator.transform.localEulerAngles = new Vector3(0.0f, this.angularPosition * 57.29578f, 0.0f);
    float num1 = 0.0f;
    for (int index = 0; index < this.frameSamples; ++index)
    {
      this.hubRotator.Rotate(new Vector3(0.0f, (float) ((double) this.angularSpeed * (double) this.directionMult * 57.295780181884766) * deltaTime, 0.0f), Space.Self);
      foreach (SwashRotor rotor in this.rotors)
      {
        rotor.SetPitch(this.swashPlate, this.swashPlatePitch, this.swashPlateRoll, this.swashPlateCollective, (float) this.directionMult);
        float angleOfAttack;
        this.forceAndTorque.Add(rotor.SampleForces(this.unitPart.rb, this.angularSpeed, (float) this.directionMult, this.liftNumber, this.stallAngle, this.dragBase, this.dragExponent, airVelocity, deltaTime, out angleOfAttack));
        num1 += angleOfAttack;
      }
    }
    if (this.unfolded)
    {
      foreach (SwashRotor rotor in this.rotors)
        rotor.CheckCollisions();
    }
    float num2 = num1 / (float) (this.frameSamples * this.radialSamples);
    Vector3 vector3_3 = this.forceAndTorque.force / (float) this.frameSamples;
    Vector3 lhs = this.forceAndTorque.torque / (float) this.frameSamples;
    this.torqueFromRotors = Vector3.Dot(lhs, this.xform.up);
    Vector3 vector3_4 = lhs - this.torqueFromRotors * this.xform.up;
    this.torqueFromRotors *= (float) this.directionMult;
    this.currentThrust = Vector3.Dot(vector3_3, this.xform.up);
    float num3 = Mathf.Abs(Vector3.Dot(this.xform.forward, vector3_2)) + Mathf.Abs(Vector3.Dot(this.xform.right, vector3_2));
    this.groundEffect = Mathf.Lerp(this.groundEffect, Mathf.Clamp01(this.bladeLength / (this.aircraft.radarAlt * this.bladeLength)), Time.fixedDeltaTime);
    this.downdraft = Mathf.Lerp(this.downdraft, (float) ((double) this.currentThrust * 0.039999999105930328 / ((double) this.rotorDiskArea * (1.0 + (double) num3 * 0.10000000149011612))), Time.fixedDeltaTime);
    this.downdraft *= Mathf.Clamp01((float) (1.0 - (double) this.groundEffect * 0.15000000596046448));
    this.VRSSmoothed = Mathf.Lerp(this.VRSSmoothed, Mathf.Clamp01(Mathf.Min(Vector3.Dot(vector3_2, -this.xform.up), 10f) - (this.VRSThreshold + num3 * 0.4f)), 0.25f * Time.fixedDeltaTime);
    if ((double) this.VRSSmoothed > 0.10000000149011612)
      this.aircraft.ShakeAircraft(this.VRSSmoothed * 0.02f, 0.0f);
    if (this.aircraft.remoteSim || this.detached)
      return;
    Vector3 vector3_5 = this.unitPart.rb.transform.InverseTransformDirection(this.unitPart.rb.angularVelocity);
    Vector3 torque = vector3_4 + this.angularSpeed * 0.5f * this.momentOfInertia * (-vector3_5.x * this.unitPart.rb.transform.right - vector3_5.z * this.unitPart.rb.transform.forward);
    this.unitPart.rb.AddForce(vector3_3);
    this.unitPart.rb.AddTorque(torque);
  }

  private void Update()
  {
    if ((double) Time.timeScale <= 0.0)
      return;
    this.AnimateRotor();
  }

  private void FixedUpdate()
  {
    this.angularSpeedRatio = Mathf.Max(this.angularSpeed / this.angularSpeedLimit, 0.0f);
    float torqueLimit = this.torqueLimit;
    if ((double) this.startupProgress < 1.0)
    {
      torqueLimit *= Mathf.Clamp01(this.startupProgress);
      if ((double) this.availablePower > (double) this.nominalPower * 0.10000000149011612)
        this.startupProgress += (double) this.angularSpeedRatio < 0.89999997615814209 ? 0.03f * Time.fixedDeltaTime : 0.2f * Time.fixedDeltaTime;
    }
    this.torqueFromEngine = Mathf.Min(this.condition * this.availablePower / Mathf.Max(this.angularSpeed, 1f), torqueLimit);
    if (this.disengaged || !this.unfolded)
      this.torqueFromEngine = 0.0f;
    this.angularSpeed += (this.torqueFromEngine + this.torqueFromRotors - this.shaftFriction * Mathf.Clamp(this.angularSpeed * 20f, -1f, 1f)) * Time.fixedDeltaTime / this.momentOfInertia;
    if (this.aircraft.LocalSim && !this.detached)
      this.unitPart.rb.AddTorque(this.torqueFromEngine * (float) this.directionMult * -this.transform.up);
    float powerRequested = Mathf.Clamp(this.nominalPower * (float) (1.0 + (double) ((this.angularSpeedNominal - this.angularSpeed) / this.angularSpeedNominal) * 30.0), 0.0f, this.nominalPower * 1.1f);
    if (this.disengaged)
      powerRequested = 0.0f;
    this.transmission.RequestPower((IPowerOutput) this, powerRequested);
    this.RotorPhysics();
  }

  Transform IEngine.get_transform() => this.transform;

  [Serializable]
  private class RotorHinge
  {
    [SerializeField]
    private Transform hinge;
    [SerializeField]
    private Vector3 foldAngle;
    private float foldSpeed;
    private float foldAmount;

    public void Initialize(float foldSpeed, float foldAmount)
    {
      this.foldAmount = foldAmount;
      this.foldSpeed = UnityEngine.Random.Range(foldSpeed * 0.8f, foldSpeed * 1.2f);
      this.hinge.localEulerAngles = this.foldAngle * Mathf.Clamp01(foldAmount);
    }

    public bool Unfold()
    {
      this.foldAmount -= this.foldSpeed * Time.deltaTime;
      this.hinge.localEulerAngles = this.foldAngle * Mathf.SmoothStep(0.0f, 1f, this.foldAmount);
      return (double) this.foldAmount > 0.0;
    }
  }

  public enum TurnDirection
  {
    clockwise,
    anticlockwise,
  }
}

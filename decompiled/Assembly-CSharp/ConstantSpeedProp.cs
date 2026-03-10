// Decompiled with JetBrains decompiler
// Type: ConstantSpeedProp
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.DebugScripts;
using System;
using System.Collections.Generic;
using UnityEngine;

#nullable disable
public class ConstantSpeedProp : MonoBehaviour, IEngine, IThrustSource, IPitchTelemetry, IPowerOutput
{
  [SerializeField]
  private UnitPart unitPart;
  private Aircraft aircraft;
  [SerializeField]
  private Transmission transmission;
  [SerializeField]
  private ConstantSpeedProp oppositeCorner;
  [SerializeField]
  private ConstantSpeedProp.TurnDirection turnDirection;
  [SerializeField]
  private ConstantSpeedProp.PitchDirection pitchDirection = ConstantSpeedProp.PitchDirection.pusher;
  [SerializeField]
  private List<ConstantSpeedProp.PropBlade> blades = new List<ConstantSpeedProp.PropBlade>();
  [SerializeField]
  private GameObject hubVisible;
  [SerializeField]
  private MeshFilter propDisc;
  [SerializeField]
  private Mesh[] bladeDamageMeshes;
  [SerializeField]
  private GameObject fragmentPrefab;
  [SerializeField]
  private List<Mesh> propDiscMeshes;
  [SerializeField]
  private Collider propDiscCollider;
  [SerializeField]
  private PropStrikeDetector propStrikeDetector;
  [SerializeField]
  private float propDiskThicknessMin;
  [SerializeField]
  private float propDiskThicknessMax;
  [HideInInspector]
  public float RPM;
  private float angularVelocity;
  private float rpmRatio;
  private float currentThrust;
  private int propBlurIndex;
  [SerializeField]
  private float momentOfInertia;
  [SerializeField]
  private float bladeLength;
  [SerializeField]
  private float hubRadius;
  [SerializeField]
  private float bladeEfficiency;
  [SerializeField]
  private float bladeDrag;
  [SerializeField]
  private float bladeStrength = 20000f;
  [SerializeField]
  private float propStrikeTolerance = 0.5f;
  [SerializeField]
  private float rpmLimit;
  [SerializeField]
  private float nominalPower;
  [SerializeField]
  private float bladeMinPitch;
  [SerializeField]
  private float bladeMaxPitch;
  [SerializeField]
  private float targetAOA;
  [SerializeField]
  private float pitchRate;
  [SerializeField]
  private float reversePitchBraking = 10f;
  [SerializeField]
  private Vector3 pitchPIDFactors;
  [SerializeField]
  private Vector3 differentialControlFactors;
  [SerializeField]
  private bool featherIfPowerLost;
  [SerializeField]
  private AudioSource propAudio;
  [SerializeField]
  private AudioClip interiorSound;
  [SerializeField]
  private AudioClip exteriorSound;
  [SerializeField]
  private float volumeBase;
  [SerializeField]
  private float pitchBase;
  [SerializeField]
  private float rpmModifyVolume;
  [SerializeField]
  private float angleModifyVolume;
  [SerializeField]
  private float rpmModifyPitch;
  [SerializeField]
  private float angleModifyPitch;
  [SerializeField]
  private bool debug;
  [SerializeField]
  private Material propBlurLightsMaterial;
  private PID propPitchPID;
  private Material propBlurBaseMaterial;
  [HideInInspector]
  public float PropPitch;
  private ControlInputs controlInputs;
  [SerializeField]
  private float averageAoA;
  private float powerAvailable;
  private float engineTorque;
  private ForceAndTorque forceAndTorque;
  [SerializeField]
  private float hubFriction;
  private float bladeNoise;
  private float originalMomentOfInertia;
  private float averageBladeLength;
  private float imbalance;
  private float imbalanceApplicationAngle;
  [SerializeField]
  private float propTorqueLimit;
  private bool operable;
  private bool featherMode;
  private bool propStrike;
  private Renderer propDiscRenderer;
  private GameObject totalForcesDebug;

  public event Action OnEngineDisable;

  public event Action OnEngineDamage;

  public event Action OnBladeDamage;

  private void Awake()
  {
    this.operable = true;
    if ((UnityEngine.Object) this.propBlurLightsMaterial != (UnityEngine.Object) null)
    {
      this.propDiscRenderer = this.propDisc.gameObject.GetComponent<Renderer>();
      this.propBlurBaseMaterial = this.propDiscRenderer.material;
    }
    if ((UnityEngine.Object) this.propStrikeDetector != (UnityEngine.Object) null)
      this.propStrikeDetector.OnStrike += new Action<float>(this.ConstantSpeedProp_OnPropStrike);
    if ((UnityEngine.Object) this.oppositeCorner != (UnityEngine.Object) null)
      this.oppositeCorner.OnBladeDamage += new Action(this.ConstantSpeedProp_OnOppositeCornerDamage);
    this.unitPart.onPartDetached += new Action<UnitPart>(this.Prop_OnAttachmentBreak);
    this.unitPart.onParentDetached += new Action<UnitPart>(this.Prop_OnAttachmentBreak);
    this.propPitchPID = new PID(this.pitchPIDFactors);
    this.originalMomentOfInertia = this.momentOfInertia;
    this.aircraft = this.unitPart.parentUnit as Aircraft;
    this.aircraft.engineStates.Add((IEngine) this);
    this.controlInputs = this.aircraft.GetInputs();
    foreach (ConstantSpeedProp.PropBlade blade in this.blades)
      blade.Initialize(this.hubRadius, this.bladeDamageMeshes);
    this.aircraft.engines.Add((IEngine) this);
    this.aircraft.onInitialize += new Action(this.ConstantSpeedProp_OnInitialize);
    if (!PlayerSettings.debugVis)
      return;
    this.totalForcesDebug = UnityEngine.Object.Instantiate<GameObject>(GameAssets.i.debugArrow, this.transform);
  }

  private void ConstantSpeedProp_OnInitialize()
  {
    if ((double) this.aircraft.radarAlt <= (double) this.aircraft.definition.spawnOffset.y + 1.0)
      return;
    this.RPM = this.rpmLimit * 1.1f;
    this.angularVelocity = (float) ((double) this.RPM * 6.2831854820251465 / 60.0);
  }

  private void ConstantSpeedProp_OnOppositeCornerDamage()
  {
    this.differentialControlFactors *= 3f;
    this.oppositeCorner.OnBladeDamage -= new Action(this.ConstantSpeedProp_OnOppositeCornerDamage);
  }

  public float GetMaxThrust() => 0.0f;

  public float GetThrust() => this.currentThrust;

  public float GetRPM() => this.RPM;

  public float GetRPMRatio() => this.rpmRatio;

  public void SetInteriorSounds(bool useInteriorSound)
  {
    if (this.propAudio.isPlaying)
    {
      if (useInteriorSound)
      {
        this.propAudio.Stop();
        this.propAudio.clip = this.interiorSound;
        this.propAudio.time = UnityEngine.Random.Range(0.0f, this.propAudio.clip.length);
        this.propAudio.Play();
      }
      else
      {
        this.propAudio.Stop();
        this.propAudio.clip = this.exteriorSound;
        this.propAudio.time = UnityEngine.Random.Range(0.0f, this.propAudio.clip.length);
        this.propAudio.Play();
      }
    }
    else if (useInteriorSound)
      this.propAudio.clip = this.interiorSound;
    else
      this.propAudio.clip = this.exteriorSound;
  }

  private void Prop_OnAttachmentBreak(UnitPart part)
  {
    this.operable = false;
    this.unitPart.onPartDetached -= new Action<UnitPart>(this.Prop_OnAttachmentBreak);
    this.unitPart.onParentDetached += new Action<UnitPart>(this.Prop_OnAttachmentBreak);
    Action onBladeDamage = this.OnBladeDamage;
    if (onBladeDamage != null)
      onBladeDamage();
    Action onEngineDisable = this.OnEngineDisable;
    if (onEngineDisable != null)
      onEngineDisable();
    this.hubFriction = 10000f;
  }

  private bool CheckBladeCollisions(bool checkWater, bool checkSolid)
  {
    float num1 = 0.0f;
    float num2 = 0.0f;
    this.averageBladeLength = 0.0f;
    foreach (ConstantSpeedProp.PropBlade blade in this.blades)
    {
      float lengthRatio = blade.GetLengthRatio();
      this.averageBladeLength += lengthRatio;
      float strikeLength1;
      if (checkSolid && blade.CheckCollision(out strikeLength1, this.fragmentPrefab, blade.GetTipVelocity(this.transform, this.unitPart.rb.velocity, (float) this.turnDirection, this.RPM * 0.1047f)))
        num1 += this.bladeStrength * Mathf.Max(this.rpmRatio, 0.5f) * Mathf.Clamp(strikeLength1, 0.05f, 2f);
      float strikeLength2;
      if (checkWater && blade.CheckWaterCollision(out strikeLength2, Datum.WaterPlane(), this.fragmentPrefab, blade.GetTipVelocity(this.transform, this.unitPart.rb.velocity, (float) this.turnDirection, this.RPM * 0.1047f)))
        num1 += this.bladeStrength * Mathf.Max(this.rpmRatio, 0.5f) * Mathf.Clamp(strikeLength2, 0.05f, 2f);
      num2 = Mathf.Max(num2, lengthRatio);
    }
    this.averageBladeLength /= (float) this.blades.Count;
    this.momentOfInertia = this.originalMomentOfInertia * this.averageBladeLength;
    if ((double) num1 == 0.0)
      return false;
    this.propDiscCollider.transform.localScale = new Vector3(num2, num2, 1f);
    this.angularVelocity -= Mathf.Min(num1 / this.momentOfInertia, this.angularVelocity);
    return true;
  }

  private void ConstantSpeedProp_OnPropStrike(float distance)
  {
    if (this.aircraft.remoteSim)
      return;
    if ((double) this.RPM < 60.0)
    {
      this.angularVelocity = 0.0f;
      this.hubFriction = 100000f;
      if (!this.propDiscCollider.isTrigger)
        return;
      float num1 = 0.0f;
      foreach (ConstantSpeedProp.PropBlade blade in this.blades)
        num1 += blade.GetLengthRatio();
      float num2 = num1 / (float) this.blades.Count;
      this.propDiscCollider.transform.localScale = new Vector3(num2, num2, 1f);
      this.propDiscCollider.isTrigger = false;
    }
    else
    {
      if (!this.CheckBladeCollisions(false, true))
        return;
      this.ApplyBladeDamage();
    }
  }

  private void ApplyBladeDamage()
  {
    Action onBladeDamage = this.OnBladeDamage;
    if (onBladeDamage != null)
      onBladeDamage();
    float num1 = 0.0f;
    float num2 = 0.0f;
    foreach (ConstantSpeedProp.PropBlade blade in this.blades)
    {
      num1 += blade.GetLengthRatio();
      num2 += blade.GetCurrentMass();
      blade.renderer.enabled = true;
    }
    float num3 = num1 / (float) this.blades.Count;
    this.imbalance = 0.1f;
    if ((double) this.aircraft.radarAlt < 5.0 || (double) num3 < 1.0 - (double) this.propStrikeTolerance)
      this.operable = false;
    this.propDisc.mesh = (Mesh) null;
    this.propStrike = true;
  }

  public float GetPitch() => this.PropPitch;

  public bool IsOperable() => this.operable;

  public float GetAoA() => this.averageAoA;

  private void PropAnimate()
  {
    if ((double) Time.timeScale == 0.0)
      return;
    this.propDisc.transform.localScale = new Vector3(1f, 1f, Mathf.Lerp(this.propDiskThicknessMin, this.propDiskThicknessMax, this.PropPitch / this.bladeMaxPitch));
    this.hubVisible.transform.Rotate(0.0f, 0.0f, this.RPM * -6f * (float) this.turnDirection * Time.deltaTime, Space.Self);
    this.rpmRatio = this.RPM / this.rpmLimit;
    this.propAudio.pitch = (float) ((double) this.pitchBase + (double) this.rpmRatio * (double) this.rpmModifyPitch + (double) this.angleModifyPitch * (double) this.averageAoA * (double) this.rpmRatio);
    this.propAudio.volume = (float) ((double) this.volumeBase + (double) this.rpmRatio * (double) this.rpmModifyVolume + (double) this.angleModifyVolume * (double) this.averageAoA * (double) this.rpmRatio);
    if (!this.propStrike)
    {
      this.propBlurIndex = Mathf.FloorToInt(Mathf.Clamp((float) this.propDiscMeshes.Count * (float) ((double) this.rpmRatio * 0.550000011920929 * (double) Time.timeScale + 0.25), 0.0f, (float) (this.propDiscMeshes.Count - 1)));
      this.propDisc.mesh = this.propDiscMeshes[this.propBlurIndex];
      if ((UnityEngine.Object) this.propBlurLightsMaterial != (UnityEngine.Object) null)
        this.propDiscRenderer.material = this.aircraft.gearDeployed ? this.propBlurLightsMaterial : this.propBlurBaseMaterial;
      foreach (ConstantSpeedProp.PropBlade blade in this.blades)
        blade.renderer.enabled = this.propBlurIndex == 0;
    }
    if ((double) this.RPM > 1.0 && !this.propAudio.isPlaying)
    {
      this.propAudio.time = UnityEngine.Random.Range(0.0f, this.propAudio.clip.length);
      this.propAudio.Play();
    }
    if ((double) this.RPM >= 1.0 || !this.propAudio.isPlaying)
      return;
    this.propAudio.Stop();
  }

  private void AutoPropPitch()
  {
    if (!this.operable)
      return;
    float a1 = Mathf.Max(this.controlInputs.throttle * this.targetAOA + Mathf.Min((float) (((double) this.rpmRatio - 1.0) * 10.0) * this.targetAOA, 0.0f), -1f);
    float a2 = Mathf.Clamp(Vector3.Dot(this.transform.forward, this.aircraft.rb.velocity) * 0.01f, -1f, 1f);
    float num1 = Mathf.Min(this.controlInputs.throttle - 0.3f, 0.0f) * Mathf.Max(a2, 0.0f);
    float num2 = Mathf.Lerp(a1, this.reversePitchBraking, num1 * 2f);
    Vector3 lhs = this.transform.position - this.aircraft.transform.position;
    float num3 = Mathf.Clamp01(Vector3.Dot(this.aircraft.transform.up, this.transform.forward));
    float num4 = Vector3.Dot(lhs, -this.aircraft.transform.right) * num3 * this.differentialControlFactors.z;
    float num5 = Vector3.Dot(lhs, -this.aircraft.transform.forward) * num3 * this.differentialControlFactors.x;
    float num6 = num2 + (float) ((double) this.controlInputs.roll * (double) num4 + (double) this.controlInputs.pitch * (double) num5);
    if (!this.featherMode && (double) this.RPM < 10.0 && (double) this.aircraft.speed < 10.0)
    {
      this.averageAoA = this.PropPitch;
      num6 = 0.0f;
    }
    float currentError = num6 - this.averageAoA;
    if (this.featherMode)
      currentError = 10f - this.averageAoA;
    this.PropPitch += Mathf.Clamp(this.propPitchPID.GetOutput(currentError, 100f, Time.fixedDeltaTime, this.pitchPIDFactors), -this.pitchRate, this.pitchRate) * Time.fixedDeltaTime;
    this.PropPitch = Mathf.Clamp(this.PropPitch, this.bladeMinPitch, this.bladeMaxPitch);
  }

  public void SendPower(float power) => this.powerAvailable = power;

  public float GetPowerAvailable() => this.powerAvailable;

  private void Update() => this.PropAnimate();

  private void FixedUpdate()
  {
    float propPitch = this.PropPitch;
    this.AutoPropPitch();
    bool flag = false;
    if (this.operable)
    {
      this.transmission.RequestPower((IPowerOutput) this, Mathf.Clamp((this.controlInputs.throttle + (float) ((1.0 - (double) this.rpmRatio) * 5.0)) * this.nominalPower, 0.0f, this.nominalPower * 1.5f));
      flag = true;
    }
    else
    {
      this.powerAvailable = 0.0f;
      this.engineTorque = 0.0f;
    }
    this.featherMode = !flag && this.featherIfPowerLost;
    this.engineTorque = this.powerAvailable * 9.5488f / Mathf.Max(this.RPM, 1f);
    if ((double) this.powerAvailable <= 0.0)
      this.engineTorque -= this.hubFriction;
    this.engineTorque = Mathf.Min(this.engineTorque, Mathf.Clamp(this.rpmRatio * 5f, 0.3f, 1f) * this.propTorqueLimit);
    this.averageAoA = 0.0f;
    Vector3 hubAirVelocity = (UnityEngine.Object) NetworkSceneSingleton<LevelInfo>.i != (UnityEngine.Object) null ? this.unitPart.rb.velocity - NetworkSceneSingleton<LevelInfo>.i.GetWind(this.transform.GlobalPosition()) : Vector3.zero;
    this.forceAndTorque.Clear();
    float hubTorque = 0.0f;
    if ((double) this.RPM > 60.0 && (double) this.transform.position.y - (double) Datum.LocalSeaY < (double) this.bladeLength && this.CheckBladeCollisions(true, false))
      this.ApplyBladeDamage();
    foreach (ConstantSpeedProp.PropBlade blade in this.blades)
    {
      blade.SetPitch((float) (((double) this.PropPitch - (double) propPitch) * -(double) this.turnDirection * -(double) this.pitchDirection));
      blade.ApplyForceAndTorque(this.unitPart.rb, this.transform, hubAirVelocity, (float) this.turnDirection, this.blades.Count, this.bladeEfficiency, this.bladeDrag, ref this.forceAndTorque, ref this.averageAoA, ref hubTorque, ref this.bladeNoise, this.RPM * 0.1047f, this.aircraft.airDensity);
    }
    if ((double) this.imbalance > 0.0)
    {
      this.imbalanceApplicationAngle += Mathf.Min(this.angularVelocity * Time.fixedDeltaTime, 2.09439516f);
      Vector3 vector3 = Vector3.right * Mathf.Sin(this.imbalanceApplicationAngle) + Vector3.up * Mathf.Cos(this.imbalanceApplicationAngle);
      ForceAndTorque forceAndTorque = new ForceAndTorque();
      foreach (ConstantSpeedProp.PropBlade blade in this.blades)
        forceAndTorque.Add(blade.GetCentripetalForce(this.angularVelocity));
      this.forceAndTorque.Add(new ForceAndTorque(forceAndTorque.force.magnitude * vector3, Vector3.zero));
    }
    this.averageAoA /= (float) this.blades.Count;
    this.bladeNoise = this.averageAoA * this.RPM;
    this.angularVelocity += Time.deltaTime * (this.engineTorque + hubTorque) / this.momentOfInertia;
    this.angularVelocity = Mathf.Max(this.angularVelocity, 0.0f);
    this.RPM = (float) ((double) this.angularVelocity * 60.0 / 6.2831854820251465);
    this.engineTorque = 0.0f;
    this.currentThrust = Vector3.Dot(this.forceAndTorque.force, this.transform.forward);
    if ((UnityEngine.Object) this.totalForcesDebug != (UnityEngine.Object) null)
    {
      this.totalForcesDebug.transform.rotation = Quaternion.LookRotation(this.forceAndTorque.force);
      this.totalForcesDebug.transform.localScale = new Vector3(2f, 2f, Mathf.Sqrt(this.forceAndTorque.force.magnitude) * 0.01f);
    }
    if (!this.aircraft.LocalSim)
      return;
    this.unitPart.rb.AddForce(this.forceAndTorque.force);
    this.unitPart.rb.AddTorque(this.forceAndTorque.torque + Mathf.Max(this.engineTorque, 0.0f) * this.transform.forward * (float) this.turnDirection);
  }

  Transform IEngine.get_transform() => this.transform;

  private enum TurnDirection
  {
    antiClockwise = -1, // 0xFFFFFFFF
    clockwise = 1,
  }

  private enum PitchDirection
  {
    puller = -1, // 0xFFFFFFFF
    pusher = 1,
  }

  [Serializable]
  public class PropBlade
  {
    [SerializeField]
    private Transform transform;
    [SerializeField]
    private Transform liftTransform;
    [SerializeField]
    private float mass;
    public Renderer renderer;
    public MeshFilter bladeMeshFilter;
    private Mesh[] bladeDamageMeshes;
    private float hubRadius;
    private float currentDamageIndex;
    private float originalLength;
    private float currentLength;
    private float originalMass;
    private float currentMass;
    private float lastWaterHit;
    [HideInInspector]
    public bool struck;
    private GameObject forceDebug;
    private GameObject incomingAirDebug;
    private GameObject deflectedAirDebug;
    private GameObject supersonicDebug;

    public void Initialize(float hubRadius, Mesh[] bladeDamageMeshes)
    {
      this.hubRadius = hubRadius;
      this.bladeDamageMeshes = bladeDamageMeshes;
      this.currentLength = this.liftTransform.localPosition.z;
      this.originalLength = this.currentLength;
      this.currentDamageIndex = (float) (bladeDamageMeshes.Length - 1);
      this.struck = false;
      this.originalMass = this.mass;
    }

    public float GetLengthRatio() => this.currentLength / this.originalLength;

    public bool CheckCollision(
      out float strikeLength,
      GameObject fragmentPrefab,
      Vector3 tipVelocity)
    {
      strikeLength = 0.0f;
      RaycastHit hitInfo;
      if (Physics.Linecast(this.transform.position, this.liftTransform.position, out hitInfo, -8193))
        this.ApplyDamage(hitInfo.distance, out strikeLength, hitInfo.point, hitInfo.normal, fragmentPrefab, tipVelocity);
      return (double) strikeLength > 0.0;
    }

    public bool CheckWaterCollision(
      out float strikeLength,
      Plane waterPlane,
      GameObject fragmentPrefab,
      Vector3 tipVelocity)
    {
      strikeLength = 0.0f;
      float enter;
      if (waterPlane.Raycast(new Ray(this.transform.position, this.transform.forward), out enter) && (double) enter > 0.0 && (double) enter < (double) this.currentLength)
      {
        Vector3 tipPosition = this.GetTipPosition();
        Vector3 forward = (Vector3.Reflect(tipVelocity.normalized, Vector3.up) + Vector3.Reflect(Vector3.Normalize(tipPosition - this.transform.position), Vector3.up)) / 2f;
        if ((double) Time.timeSinceLevelLoad - (double) this.lastWaterHit > 1.0)
        {
          this.lastWaterHit = Time.timeSinceLevelLoad;
          UnityEngine.Object.Destroy((UnityEngine.Object) UnityEngine.Object.Instantiate<GameObject>(GameAssets.i.rotorStrike_water, new Vector3(tipPosition.x, 0.0f, tipPosition.z), Quaternion.LookRotation(forward)), 10f);
        }
        this.ApplyDamage(enter, out strikeLength, this.transform.position + this.transform.forward * enter, Vector3.up, fragmentPrefab, tipVelocity);
      }
      return (double) strikeLength > 0.0;
    }

    private void ApplyDamage(
      float hitDistance,
      out float strikeLength,
      Vector3 hitPosition,
      Vector3 hitNormal,
      GameObject fragmentPrefab,
      Vector3 tipVelocity)
    {
      this.liftTransform.localPosition = new Vector3(0.0f, 0.0f, hitDistance);
      strikeLength = this.currentLength - hitDistance;
      this.currentLength = hitDistance;
      this.currentMass = this.originalMass * this.currentLength / this.originalLength;
      int index = Mathf.FloorToInt((float) this.bladeDamageMeshes.Length * Mathf.Max((this.currentLength - this.hubRadius) / this.originalLength, 0.0f));
      if ((double) index == (double) this.currentDamageIndex)
        return;
      this.currentDamageIndex = (float) index;
      this.bladeMeshFilter.mesh = this.bladeDamageMeshes[index];
      if (!((UnityEngine.Object) fragmentPrefab != (UnityEngine.Object) null))
        return;
      GameObject gameObject = NetworkSceneSingleton<Spawner>.i.SpawnLocal(fragmentPrefab, (Transform) null);
      gameObject.transform.position = hitPosition;
      Rigidbody component = gameObject.GetComponent<Rigidbody>();
      float magnitude = tipVelocity.magnitude;
      component.velocity = magnitude * 0.25f * hitNormal + magnitude * 0.25f * UnityEngine.Random.insideUnitSphere;
      component.angularVelocity = UnityEngine.Random.insideUnitSphere * 4f;
      UnityEngine.Object.Destroy((UnityEngine.Object) gameObject, 10f);
    }

    public void SetPitch(float pitchChange)
    {
      this.transform.Rotate(0.0f, 0.0f, pitchChange, Space.Self);
    }

    public Vector3 GetTipVelocity(
      Transform hub,
      Vector3 hubVelocity,
      float direction,
      float rotationRate)
    {
      Vector3 vector3 = Vector3.Cross(-hub.forward, this.transform.forward) * direction;
      return hubVelocity + 1f * this.currentLength * rotationRate * vector3;
    }

    public Vector3 GetTipPosition() => this.liftTransform.position;

    public ForceAndTorque GetCentripetalForce(float angularVelocity)
    {
      double num = (double) angularVelocity * (double) this.currentLength * 0.5;
      return new ForceAndTorque((float) (num * num) / this.currentLength * this.currentMass * this.transform.forward, Vector3.zero);
    }

    public float GetCurrentLength() => this.currentLength;

    public float GetCurrentMass() => this.currentMass;

    public void ApplyForceAndTorque(
      Rigidbody rb,
      Transform hub,
      Vector3 hubAirVelocity,
      float direction,
      int bladeNum,
      float efficiency,
      float drag,
      ref ForceAndTorque forceAndTorque,
      ref float meanAoA,
      ref float hubTorque,
      ref float noise,
      float rotationRate,
      float airDensity)
    {
      Vector3 rhs = Vector3.Cross(-hub.forward, this.transform.forward) * direction;
      float num1 = this.currentLength * rotationRate;
      Vector3 direction1 = hubAirVelocity + num1 * rhs * 0.75f;
      Vector3 vector3_1 = this.liftTransform.InverseTransformDirection(direction1);
      float num2 = (float) (-(double) Mathf.Atan2(vector3_1.y, vector3_1.z) * 57.295780181884766);
      meanAoA += num2;
      float magnitude = direction1.magnitude;
      Vector3 vector3_2 = airDensity * magnitude * magnitude * drag * direction1.normalized;
      float num3 = Mathf.Max((float) (((double) magnitude - 300.0) / 30.0), 1f);
      if ((double) num3 > 1.0)
      {
        efficiency /= num3;
        vector3_2 *= num3;
      }
      float num4 = 3.14159274f * this.currentLength * this.currentLength / (float) bladeNum;
      double num5 = (double) airDensity * (double) num4 * (double) direction1.magnitude * (double) efficiency;
      Vector3 forward1 = Vector3.Reflect(-direction1, -this.liftTransform.up);
      Vector3 vector3_3 = -direction1 - forward1;
      Vector3 forward2 = (float) num5 * vector3_3 - vector3_2;
      Vector3 vector3_4 = forward2;
      float sqrMagnitude = direction1.sqrMagnitude;
      noise += sqrMagnitude * num2;
      float num6 = Vector3.Dot(vector3_4, rhs);
      hubTorque += (float) ((double) num6 * (double) this.currentLength * 0.75);
      forceAndTorque.Add(new ForceAndTorque(vector3_4, (this.liftTransform.position - hub.position) * 0.75f));
      if (!DebugVis.Enabled)
        return;
      DebugVis.Create<GameObject>(ref this.forceDebug, GameAssets.i.debugArrow, this.liftTransform);
      if (DebugVis.Create<GameObject>(ref this.incomingAirDebug, GameAssets.i.debugArrow, this.liftTransform))
        this.incomingAirDebug.GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", Color.white);
      if (DebugVis.Create<GameObject>(ref this.deflectedAirDebug, GameAssets.i.debugArrow, this.liftTransform))
        this.deflectedAirDebug.GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", Color.yellow);
      if (DebugVis.Create<GameObject>(ref this.supersonicDebug, GameAssets.i.debugPoint, this.liftTransform))
        this.supersonicDebug.transform.localScale = Vector3.one * 0.5f;
      this.incomingAirDebug.transform.rotation = Quaternion.LookRotation(-direction1);
      this.incomingAirDebug.transform.localScale = new Vector3(0.5f, 0.5f, Mathf.Sqrt(direction1.magnitude) * 0.1f);
      this.deflectedAirDebug.transform.rotation = Quaternion.LookRotation(forward1);
      this.deflectedAirDebug.transform.localScale = new Vector3(0.5f, 0.5f, Mathf.Sqrt(forward1.magnitude) * 0.1f);
      this.forceDebug.transform.rotation = Quaternion.LookRotation(forward2);
      this.forceDebug.transform.localScale = new Vector3(1f, 1f, Mathf.Sqrt(forward2.magnitude) * 0.02f);
      this.supersonicDebug.SetActive((double) magnitude > 340.0);
    }
  }
}

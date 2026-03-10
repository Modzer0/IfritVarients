// Decompiled with JetBrains decompiler
// Type: LandingGear
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

#nullable disable
public class LandingGear : MonoBehaviour
{
  [Header("Physics")]
  [SerializeField]
  private AeroPart attachedPart;
  [SerializeField]
  private float extendedDrag;
  private float retractedDrag;
  [SerializeField]
  private GameObject bumpStop;
  [SerializeField]
  private GameObject unsprung;
  [SerializeField]
  private float suspensionTravel;
  [SerializeField]
  private float springRate;
  [SerializeField]
  private float dampingRate;
  [SerializeField]
  private Transform castPoint;
  [SerializeField]
  private float wheelRadius;
  [SerializeField]
  private Transform[] wheels;
  [SerializeField]
  private Transform axle;
  [SerializeField]
  private float frictionCoef = 1f;
  [SerializeField]
  private float contactArea;
  [SerializeField]
  private float rollingResistance;
  [SerializeField]
  private Aircraft aircraft;
  [Header("Audio")]
  [SerializeField]
  private AudioSource tireNoiseSound;
  [SerializeField]
  private AudioSource tireSkidSound;
  [SerializeField]
  private float skidVolumeFloor = -0.4f;
  [SerializeField]
  private float skidPitchMult = 1f;
  [Header("Strength")]
  [SerializeField]
  private float maxCompression;
  [SerializeField]
  private float mass;
  [SerializeField]
  private Collider gearCollider;
  [SerializeField]
  private AudioClip breakSound;
  [Header("Folding")]
  [SerializeField]
  private AudioClip foldSound;
  [SerializeField]
  private float foldVolume;
  private AudioSource foldSoundSource;
  [SerializeField]
  private AudioClip latchSound;
  [SerializeField]
  private float latchVolume;
  [SerializeField]
  private Transform gearHinge;
  [SerializeField]
  private Vector3 hingeFoldMotion;
  [SerializeField]
  private float foldDegrees;
  [SerializeField]
  private float foldSpeed;
  [SerializeField]
  private float strutRotation;
  [SerializeField]
  private GearPart[] movingParts;
  [SerializeField]
  private List<LandingGear.GearDoor> gearDoors = new List<LandingGear.GearDoor>();
  private Vector3 hingeBasePos;
  private Vector3 hingeBaseAngles;
  [Header("Steering")]
  [SerializeField]
  private bool steering;
  [Header("Steering")]
  [SerializeField]
  private bool braked;
  [SerializeField]
  private float steeringLock;
  [SerializeField]
  private float steeringSpeed;
  [SerializeField]
  private float aligningStrength;
  [SerializeField]
  private float differentialBrakeFactor;
  private float steeringAngle;
  private ControlInputs controlInputs;
  [Header("Effects")]
  [SerializeField]
  private ParticleSystem dust;
  private RaycastHit hit;
  private float groundDepth;
  private float compressionDistance;
  private float compressionForce;
  private float dampingForce;
  private float groundSpeed;
  private float foldAmount;
  private float wheelSpeed;
  private float wheelSpeedPrev;
  private bool onTarmac = true;
  private bool prevOnTarmac;
  private float brakeStrength;
  private Collider contactCollider;
  private Vector3 contactPatch;
  private float contactPatchSize;
  private GameObject skidEffect;

  public event Action<LandingGear> onGearBreak;

  private void Awake()
  {
    this.hingeBaseAngles = this.gearHinge.localEulerAngles;
    this.contactPatchSize = this.wheelRadius * 0.5f;
    this.aircraft.onSetGear += new Action<Aircraft.OnSetGear>(this.LandingGear_OnSetGear);
    this.retractedDrag = this.attachedPart.dragArea;
    this.controlInputs = this.aircraft.GetInputs();
    this.hingeBasePos = this.gearHinge.localPosition;
    this.attachedPart.onParentDetached += new Action<UnitPart>(this.LandingGear_OnPartDetached);
    if ((UnityEngine.Object) this.castPoint == (UnityEngine.Object) null)
      this.castPoint = this.bumpStop.transform;
    if (!((UnityEngine.Object) this.dust == (UnityEngine.Object) null))
      return;
    this.dust = this.wheels[0].GetComponent<ParticleSystem>();
  }

  private void OnDestroy()
  {
    if (!((UnityEngine.Object) this.aircraft != (UnityEngine.Object) null))
      return;
    this.aircraft.onSetGear -= new Action<Aircraft.OnSetGear>(this.LandingGear_OnSetGear);
  }

  private void PlayLatchSound()
  {
    AudioSource audioSource = this.gameObject.AddComponent<AudioSource>();
    audioSource.outputAudioMixerGroup = SoundManager.i.EffectsMixer;
    audioSource.bypassListenerEffects = true;
    audioSource.clip = this.latchSound;
    audioSource.volume = this.latchVolume;
    audioSource.dopplerLevel = 0.0f;
    audioSource.minDistance = 5f;
    audioSource.maxDistance = 20f;
    audioSource.spatialBlend = 1f;
    audioSource.Play();
    UnityEngine.Object.Destroy((UnityEngine.Object) audioSource, 2f);
    if (!((UnityEngine.Object) this.foldSoundSource != (UnityEngine.Object) null))
      return;
    this.foldSoundSource.Stop();
    UnityEngine.Object.Destroy((UnityEngine.Object) this.foldSoundSource, 2f);
  }

  private void LandingGear_OnPartDetached(UnitPart part) => this.BreakWheel();

  private void LandingGear_OnSetGear(Aircraft.OnSetGear e)
  {
    if (e.gearState == LandingGear.GearState.LockedExtended)
    {
      this.foldAmount = 0.0f;
      this.gearHinge.localEulerAngles = this.hingeBaseAngles;
      this.gearHinge.localPosition = Vector3.Lerp(this.hingeBasePos, this.hingeBasePos + this.hingeFoldMotion, this.foldAmount);
      this.unsprung.transform.localEulerAngles = Vector3.zero;
      this.attachedPart.dragArea = this.extendedDrag;
      foreach (LandingGear.GearDoor gearDoor in this.gearDoors)
        gearDoor.Animate(1f);
      this.UpdateMovingParts();
      this.enabled = true;
    }
    if (e.gearState == LandingGear.GearState.LockedRetracted)
    {
      this.foldAmount = 1f;
      this.gearHinge.localEulerAngles = this.hingeBaseAngles + new Vector3(this.foldDegrees, 0.0f, 0.0f);
      this.gearHinge.localPosition = Vector3.Lerp(this.hingeBasePos, this.hingeBasePos + this.hingeFoldMotion, this.foldAmount);
      this.attachedPart.dragArea = this.retractedDrag;
      this.unsprung.transform.localEulerAngles = new Vector3(0.0f, this.strutRotation, 0.0f);
      foreach (LandingGear.GearDoor gearDoor in this.gearDoors)
        gearDoor.Animate(0.0f);
      this.UpdateMovingParts();
      this.enabled = false;
    }
    if (e.gearState != LandingGear.GearState.Extending && e.gearState != LandingGear.GearState.Retracting)
      return;
    this.MoveGear(e.gearState).Forget();
    this.foldSoundSource = this.gameObject.AddComponent<AudioSource>();
    this.foldSoundSource.outputAudioMixerGroup = SoundManager.i.EffectsMixer;
    this.foldSoundSource.bypassListenerEffects = true;
    this.foldSoundSource.clip = this.foldSound;
    this.foldSoundSource.volume = this.foldVolume;
    this.foldSoundSource.dopplerLevel = 0.0f;
    this.foldSoundSource.minDistance = 5f;
    this.foldSoundSource.maxDistance = 20f;
    this.foldSoundSource.spatialBlend = 1f;
    this.foldSoundSource.Play();
    this.enabled = true;
  }

  private async UniTask MoveGear(LandingGear.GearState gearState)
  {
    LandingGear landingGear = this;
    float gearAngle = Vector3.Angle(landingGear.gearHinge.forward, landingGear.gearHinge.parent.forward);
    bool doorsOpen = gearState != LandingGear.GearState.Extending;
    float moveTime = 0.0f;
    CancellationToken cancel = landingGear.destroyCancellationToken;
    while (true)
    {
      do
      {
        await UniTask.Yield();
        if (cancel.IsCancellationRequested)
          goto label_2;
      }
      while ((UnityEngine.Object) landingGear.gearHinge == (UnityEngine.Object) null);
      if (gearState == LandingGear.GearState.Extending)
      {
        if (!doorsOpen)
        {
          moveTime += Time.deltaTime;
          for (int index = 0; index < landingGear.gearDoors.Count; ++index)
            landingGear.gearDoors[index].Animate(moveTime);
          if ((double) moveTime > 1.0)
            doorsOpen = true;
        }
        else
        {
          gearAngle -= landingGear.foldSpeed * Time.deltaTime;
          float y = Mathf.Abs(gearAngle / landingGear.foldDegrees) * landingGear.strutRotation;
          landingGear.gearHinge.localEulerAngles = landingGear.hingeBaseAngles + new Vector3(gearAngle * Mathf.Sign(landingGear.foldDegrees), 0.0f, 0.0f);
          landingGear.unsprung.transform.localEulerAngles = new Vector3(0.0f, y, 0.0f);
          landingGear.attachedPart.dragArea = Mathf.Lerp(landingGear.extendedDrag, landingGear.retractedDrag, gearAngle / Mathf.Abs(landingGear.foldDegrees));
          if ((double) gearAngle < 0.0)
            goto label_13;
        }
      }
      if (gearState == LandingGear.GearState.Retracting)
      {
        gearAngle += landingGear.foldSpeed * Time.deltaTime;
        float y = Mathf.Abs(gearAngle / landingGear.foldDegrees) * landingGear.strutRotation;
        landingGear.gearHinge.localEulerAngles = landingGear.hingeBaseAngles + new Vector3(gearAngle * Mathf.Sign(landingGear.foldDegrees), 0.0f, 0.0f);
        landingGear.unsprung.transform.localEulerAngles = new Vector3(0.0f, y, 0.0f);
        landingGear.attachedPart.dragArea = Mathf.Lerp(landingGear.extendedDrag, landingGear.retractedDrag, gearAngle / Mathf.Abs(landingGear.foldDegrees));
        if ((double) gearAngle > (double) Mathf.Abs(landingGear.foldDegrees))
        {
          landingGear.gearHinge.localEulerAngles = landingGear.hingeBaseAngles + new Vector3(landingGear.foldDegrees, 0.0f, 0.0f);
          landingGear.unsprung.transform.localEulerAngles = new Vector3(0.0f, landingGear.strutRotation, 0.0f);
          moveTime += Time.deltaTime;
          for (int index = 0; index < landingGear.gearDoors.Count; ++index)
          {
            landingGear.gearDoors[index].Animate(1f - moveTime);
            if ((double) moveTime > 1.0)
            {
              landingGear.gearDoors[index].transform.localEulerAngles = Vector3.zero;
              landingGear.aircraft.SetGear(LandingGear.GearState.LockedRetracted);
              landingGear.attachedPart.dragArea = landingGear.retractedDrag;
              landingGear.PlayLatchSound();
              cancel = new CancellationToken();
              return;
            }
          }
        }
      }
      landingGear.foldAmount = Mathf.Abs(gearAngle / landingGear.foldDegrees);
      landingGear.gearHinge.transform.localPosition = Vector3.Lerp(landingGear.hingeBasePos, landingGear.hingeBasePos + landingGear.hingeFoldMotion, landingGear.foldAmount);
      landingGear.UpdateMovingParts();
    }
label_2:
    cancel = new CancellationToken();
    return;
label_13:
    landingGear.gearHinge.localEulerAngles = landingGear.hingeBaseAngles;
    landingGear.unsprung.transform.localEulerAngles = Vector3.zero;
    landingGear.attachedPart.dragArea = landingGear.extendedDrag;
    landingGear.aircraft.SetGear(LandingGear.GearState.LockedExtended);
    landingGear.PlayLatchSound();
    cancel = new CancellationToken();
  }

  public bool WeightOnWheel(float threshold)
  {
    return (double) this.compressionDistance > (double) this.suspensionTravel * (double) threshold;
  }

  private void UpdateMovingParts()
  {
    foreach (GearPart movingPart in this.movingParts)
    {
      if ((UnityEngine.Object) movingPart.transform == (UnityEngine.Object) null)
        break;
      if ((UnityEngine.Object) movingPart.target != (UnityEngine.Object) null)
        movingPart.transform.LookAt(movingPart.target);
      if (movingPart.foldAngles != Vector3.zero)
        movingPart.transform.localEulerAngles = Vector3.Lerp(Vector3.zero, movingPart.foldAngles, this.foldAmount);
    }
  }

  private void BreakWheel()
  {
    Action<LandingGear> onGearBreak = this.onGearBreak;
    if (onGearBreak != null)
      onGearBreak(this);
    if (this.dust.isPlaying)
      this.dust.Stop();
    if ((UnityEngine.Object) this.aircraft != (UnityEngine.Object) null)
      this.aircraft.onSetGear -= new Action<Aircraft.OnSetGear>(this.LandingGear_OnSetGear);
    if ((UnityEngine.Object) this.foldSoundSource != (UnityEngine.Object) null && this.foldSoundSource.isPlaying)
      this.foldSoundSource.Stop();
    if (this.tireNoiseSound.isPlaying)
      this.tireNoiseSound.Stop();
    if (this.tireSkidSound.isPlaying)
      this.tireSkidSound.Stop();
    this.gearCollider.enabled = true;
    this.gearHinge.transform.SetParent((Transform) null, true);
    if ((UnityEngine.Object) this.attachedPart != (UnityEngine.Object) null)
    {
      this.attachedPart.rb.mass -= this.mass;
      this.attachedPart.onParentDetached -= new Action<UnitPart>(this.LandingGear_OnPartDetached);
    }
    Rigidbody rigidbody = this.gearHinge.gameObject.AddComponent<Rigidbody>();
    rigidbody.sleepThreshold = 0.0f;
    rigidbody.mass = this.mass;
    rigidbody.drag = 0.05f;
    rigidbody.angularDrag = 0.05f;
    rigidbody.useGravity = true;
    rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
    rigidbody.velocity = this.attachedPart.rb.velocity;
    rigidbody.angularVelocity = this.attachedPart.rb.angularVelocity;
    AudioSource audioSource = this.gameObject.AddComponent<AudioSource>();
    audioSource.outputAudioMixerGroup = SoundManager.i.EffectsMixer;
    audioSource.pitch = UnityEngine.Random.Range(0.8f, 1.2f);
    audioSource.clip = this.breakSound;
    audioSource.spatialBlend = 1f;
    audioSource.dopplerLevel = 1f;
    audioSource.spread = 5f;
    audioSource.maxDistance = 200f;
    audioSource.minDistance = 5f;
    audioSource.Play();
    UnityEngine.Object.Destroy((UnityEngine.Object) audioSource, 3f);
    UnityEngine.Object.Destroy((UnityEngine.Object) this.gearHinge.gameObject, 20f);
    UnityEngine.Object.Destroy((UnityEngine.Object) this);
  }

  private void FixedUpdate()
  {
    if (this.aircraft.remoteSim && this.aircraft.NetId != 0U)
    {
      this.enabled = false;
    }
    else
    {
      Rigidbody rigidbody = (Rigidbody) null;
      if (!((UnityEngine.Object) this.attachedPart.rb != (UnityEngine.Object) null))
        return;
      float b = this.braked ? this.controlInputs.brake : 0.0f;
      if (this.braked && (double) this.aircraft.speed < 1.0 && (double) this.controlInputs.throttle < 0.10000000149011612)
        b = 1f;
      if ((double) this.differentialBrakeFactor != 0.0 && (double) this.groundSpeed < 30.0 && (double) this.groundSpeed > 3.0)
        b += Mathf.Clamp01(this.controlInputs.yaw * 0.3f * this.differentialBrakeFactor);
      this.brakeStrength = Mathf.Lerp(this.brakeStrength, b, Time.deltaTime * 5f);
      for (int index = 0; index < this.wheels.Length; ++index)
        this.wheels[index].transform.Rotate((float) (360.0 * (double) Time.deltaTime * ((double) this.wheelSpeed / (6.2831802368164063 * (double) this.wheelRadius))), 0.0f, 0.0f, Space.Self);
      if ((double) this.foldAmount < 0.20000000298023224 && Physics.Linecast(this.castPoint.position, this.castPoint.position - this.castPoint.up * this.suspensionTravel, out this.hit, -40961))
      {
        this.contactCollider = this.hit.collider;
        Vector3 pointVelocity = this.attachedPart.rb.GetPointVelocity(this.hit.point);
        if ((UnityEngine.Object) this.contactCollider.attachedRigidbody != (UnityEngine.Object) null)
        {
          rigidbody = this.contactCollider.attachedRigidbody;
          pointVelocity -= rigidbody.GetPointVelocity(this.hit.point);
          AnimatedPhysicsSurface component = this.contactCollider.gameObject.GetComponent<AnimatedPhysicsSurface>();
          if ((UnityEngine.Object) component != (UnityEngine.Object) null)
            pointVelocity -= component.GetVelocity();
        }
        this.onTarmac = !((UnityEngine.Object) this.hit.collider.sharedMaterial == (UnityEngine.Object) GameAssets.i.terrainMaterial);
        Vector3 vector3_1 = Vector3.zero;
        if (!this.onTarmac)
        {
          float num1 = (float) (((double) this.compressionForce + (double) this.dampingForce) / ((double) this.contactArea * 10000.0));
          GlobalPosition globalPosition = this.transform.GlobalPosition();
          float num2 = (float) (1.0 + (4.0 + (double) Mathf.Sin(globalPosition.x * 0.8f) + (double) Mathf.Cos(globalPosition.z * 0.37f) + (double) Mathf.Sin(globalPosition.z * 0.8f) + (double) Mathf.Cos(globalPosition.z * 0.37f)));
          this.groundDepth = Mathf.Lerp(this.groundDepth, Mathf.Max(num1 * (3f / 1000f) / num2, 0.0f), 3f * Time.deltaTime / Mathf.Clamp(this.wheelSpeed * 0.1f, 1f, 3f));
          vector3_1 = 2.5f * this.groundDepth * this.groundDepth * this.wheelSpeed * num1 * num1 * -pointVelocity.normalized;
          if ((double) this.wheelSpeed > 2.0 && !this.dust.isPlaying)
            this.dust.Play();
        }
        else
        {
          this.groundDepth = 0.0f;
          if (this.dust.isPlaying)
            this.dust.Stop();
        }
        this.compressionDistance = this.suspensionTravel - (this.hit.distance + this.groundDepth);
        this.compressionDistance = Mathf.Max(this.compressionDistance, 0.0f);
        this.dampingForce = -Vector3.Dot(this.hit.normal, pointVelocity) * this.dampingRate;
        this.compressionForce = this.springRate * this.compressionDistance;
        this.groundSpeed = Vector3.Dot(pointVelocity, this.transform.forward);
        if (this.steering && (double) this.groundSpeed > 1.0)
          this.steeringAngle += TargetCalc.GetAngleOnAxis(this.axle.forward, pointVelocity, this.axle.up) * Mathf.Min(this.groundSpeed * 0.1f, 10f) * this.aligningStrength * Time.deltaTime;
        Vector3 normalized1 = Vector3.ProjectOnPlane(pointVelocity, this.hit.normal).normalized;
        float num = this.groundSpeed - this.wheelSpeed;
        if ((double) num > 20.0)
        {
          Vector3 vector3_2 = (this.hit.point - this.aircraft.transform.position) with
          {
            y = 0.0f
          };
          SceneSingleton<ParticleEffectManager>.i.EmitParticles("TireSmoke", (int) ((double) num * 0.029999999329447746), (this.hit.point + this.hit.normal * this.wheelRadius).ToGlobalPosition(), Vector3.Project(this.attachedPart.rb.velocity, normalized1) + vector3_2.normalized * this.aircraft.speed * 0.1f, 0.0f, Mathf.Max((float) (5.0 - (double) this.aircraft.speed * 0.029999999329447746), 1f), 0.3f, (float) ((double) this.wheelRadius * 10.0 + (double) num * 0.10000000149011612), 0.3f, this.aircraft.speed * 0.03f, Mathf.Clamp01(num * 0.01f), 0.3f);
        }
        this.wheelSpeed += Mathf.Clamp(num, -100f * Time.deltaTime, 100f * Time.deltaTime);
        if ((double) this.compressionDistance > (double) this.maxCompression || (double) this.gearHinge.transform.localEulerAngles.x > 10.0 || (double) vector3_1.sqrMagnitude > (double) this.springRate * (double) this.springRate)
        {
          this.BreakWheel();
          this.enabled = false;
          return;
        }
        Vector3 vector3_3 = this.hit.normal * Mathf.Max(this.compressionForce + this.dampingForce, 0.0f);
        float maxLength = (this.compressionForce + this.dampingForce) * this.frictionCoef;
        Vector3 normalized2 = Vector3.Cross(normalized1, this.hit.normal).normalized;
        Vector3 vector3_4 = new Vector3(0.0f, 0.0f, 0.0f);
        Vector3 vector3_5 = new Vector3(0.0f, 0.0f, 0.0f);
        Vector3 vector3_6 = -normalized1 * (float) ((double) this.rollingResistance * ((double) this.compressionForce + (double) this.dampingForce) * 1.0);
        float f = 0.0f;
        if ((double) Mathf.Abs(this.groundSpeed) < 1.0)
        {
          if (FastMath.OutOfRange(this.hit.point, this.contactCollider.transform.TransformPoint(this.contactPatch), this.contactPatchSize))
            this.contactPatch = this.contactCollider.transform.InverseTransformPoint(this.hit.point);
          if (this.dust.isPlaying)
            this.dust.Stop();
          Vector3 vector = (this.contactCollider.transform.TransformPoint(this.contactPatch) - this.hit.point) / this.contactPatchSize - pointVelocity * 1f;
          vector3_4 = Vector3.ClampMagnitude(Vector3.Project(vector, this.axle.transform.right) + Vector3.Project(vector, -Mathf.Sign(this.wheelSpeed) * this.axle.transform.forward) * (this.brakeStrength + 0.05f), 1f) * maxLength;
        }
        else
        {
          f = Mathf.Clamp(TargetCalc.GetAngleOnAxis(this.axle.transform.forward, normalized1, this.hit.normal), -10f, 10f);
          Vector3 vector3_7 = Mathf.Clamp(f * (float) (0.20000000298023224 + (double) this.wheelSpeed * 0.0099999997764825821), -1f, 1f) * maxLength * normalized2;
          Vector3 vector3_8 = Mathf.Clamp01(Mathf.Abs(Vector3.Dot(this.axle.transform.forward, normalized2))) * maxLength * -normalized1;
          Vector3 vector3_9 = -normalized1 * this.brakeStrength * (this.compressionForce + this.dampingForce) * this.frictionCoef;
          Vector3 vector3_10 = vector3_8;
          vector3_5 = Vector3.ClampMagnitude(vector3_7 + vector3_10 + vector3_9 + vector3_6, maxLength);
        }
        this.attachedPart.rb.AddForceAtPosition(vector3_3 + vector3_5 + vector3_4 + vector3_1, this.hit.point);
        if ((UnityEngine.Object) rigidbody != (UnityEngine.Object) null)
          rigidbody.AddForceAtPosition(-(vector3_3 + vector3_5 + vector3_4 + vector3_1), this.hit.point);
        this.unsprung.transform.position = this.bumpStop.transform.position - this.bumpStop.transform.up * (this.suspensionTravel - this.compressionDistance - this.wheelRadius);
        if (!this.tireNoiseSound.isPlaying || this.prevOnTarmac != this.onTarmac)
        {
          this.tireNoiseSound.Stop();
          this.tireSkidSound.Stop();
          if (this.onTarmac)
          {
            this.tireNoiseSound.clip = GameAssets.i.wheelRollingRoad;
            this.tireSkidSound.clip = GameAssets.i.wheelSlidingRoad;
          }
          else
          {
            this.tireNoiseSound.clip = GameAssets.i.wheelRollingDirt;
            this.tireSkidSound.clip = GameAssets.i.wheelSlidingDirt;
          }
          this.tireNoiseSound.time = this.tireNoiseSound.clip.length * UnityEngine.Random.value;
          this.tireNoiseSound.Play();
          this.tireSkidSound.time = this.tireSkidSound.clip.length * UnityEngine.Random.value;
          this.tireSkidSound.Play();
        }
        this.prevOnTarmac = this.onTarmac;
        this.tireNoiseSound.volume = (float) ((double) Mathf.Abs(this.compressionForce + this.dampingForce) / (double) this.springRate * (double) this.groundSpeed * (0.20000000298023224 + (double) this.groundDepth * 10.0));
        this.tireNoiseSound.pitch = (float) (0.5 + (double) this.groundSpeed * 0.014999999664723873);
        this.tireSkidSound.volume = Mathf.Max(this.skidVolumeFloor + Mathf.Abs(f) * 0.1f, (float) ((double) num * 0.10000000149011612 + (double) this.groundDepth * (double) this.groundDepth * (double) this.groundSpeed));
        this.tireSkidSound.pitch = Mathf.Min((float) (0.75 + ((double) Mathf.Abs(f) * (double) this.groundSpeed * 0.00039999998989515007 + (double) num * (3.0 / 1000.0))) * this.skidPitchMult, 3f);
      }
      else
      {
        if (this.dust.isPlaying)
          this.dust.Stop();
        if (this.tireNoiseSound.isPlaying)
          this.tireNoiseSound.Stop();
        if (this.tireSkidSound.isPlaying)
          this.tireSkidSound.Stop();
        this.wheelSpeed -= (float) ((double) this.wheelSpeed * 0.10000000149011612 + 1.0) * Time.deltaTime;
        this.wheelSpeed = Mathf.Max(this.wheelSpeed, 0.0f);
        this.compressionForce = 0.0f;
        this.dampingForce = 0.0f;
        this.compressionDistance = 0.0f;
        this.unsprung.transform.position = this.bumpStop.transform.position - this.unsprung.transform.up * (this.suspensionTravel - this.wheelRadius);
      }
      this.wheelSpeedPrev = this.wheelSpeed;
      if (!this.steering)
        return;
      float num3 = this.controlInputs.yaw * this.steeringLock - this.steeringAngle;
      int num4 = 1;
      this.steeringAngle += Mathf.Clamp(num3, -this.steeringSpeed * (float) num4 * Time.deltaTime, this.steeringSpeed * (float) num4 * Time.deltaTime);
      this.steeringAngle = Mathf.Clamp(this.steeringAngle, -Mathf.Abs(this.steeringLock), Mathf.Abs(this.steeringLock));
      this.unsprung.transform.localEulerAngles = new Vector3(0.0f, this.steeringAngle, 0.0f);
    }
  }

  [Serializable]
  public class GearDoor
  {
    public Transform transform;
    public Vector3 closedAngle;
    public Vector3 openAngle;

    public void Animate(float openAmount)
    {
      if (!((UnityEngine.Object) this.transform != (UnityEngine.Object) null))
        return;
      this.transform.localEulerAngles = Vector3.Lerp(this.closedAngle, this.openAngle, openAmount);
    }
  }

  public enum GearState
  {
    Uninitialized,
    LockedRetracted,
    LockedExtended,
    Retracting,
    Extending,
  }
}

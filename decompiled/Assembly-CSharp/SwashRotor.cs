// Decompiled with JetBrains decompiler
// Type: SwashRotor
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.DebugScripts;
using System;
using UnityEngine;

#nullable disable
public class SwashRotor : MonoBehaviour, IDamageable
{
  [SerializeField]
  private Aircraft aircraft;
  [SerializeField]
  private SwashRotor.RotorSegment[] segments;
  private RotorShaft rotorShaft;
  public Transform hinge;
  public Transform tip;
  [SerializeField]
  private ArmorProperties armorProperties;
  [SerializeField]
  private GameObject[] breakEffects;
  [SerializeField]
  private float stiffnessMultiplier = 1f;
  private Transform hub;
  private Transform span;
  private Transform pitchVis;
  private Transform flapVis;
  private Transform pitchTransform;
  private Transform[] velocityVis;
  private Transform[] liftVis;
  private int samples;
  private float length;
  private float originalLength;
  private float mass;
  private float originalMass;
  private float momentOfInertia;
  private float flapSpringUp;
  private float flapSpringDown;
  private float flapDamp;
  private float cyclicTravel;
  private float collectiveTravel;
  private float hubRadius;
  private float washout;
  private float flapAngularVelocity;
  private float flapAngle;
  private float shaftAngularSpeed;
  private float hitPoints = 100f;
  private ForceAndTorque forceAndTorque;
  private int lastAttachedSegmentIndex;
  private bool bendSegments;
  private byte damageableIndex;

  public void Setup(
    RotorShaft rotorShaft,
    byte damageableIndex,
    float directionMult,
    float length,
    float mass,
    float flapSpringUp,
    float flapSpringDown,
    float flapDamp,
    float cyclicTravel,
    float collectiveTravel,
    float washout,
    int samples)
  {
    this.forceAndTorque = new ForceAndTorque();
    this.rotorShaft = rotorShaft;
    this.hub = rotorShaft.transform;
    this.damageableIndex = damageableIndex;
    this.samples = samples;
    this.length = length;
    this.originalLength = length;
    this.mass = mass;
    this.originalMass = mass;
    this.flapSpringUp = flapSpringUp;
    this.flapSpringDown = flapSpringDown;
    this.flapDamp = flapDamp;
    this.cyclicTravel = cyclicTravel;
    this.collectiveTravel = collectiveTravel;
    this.washout = washout;
    this.hubRadius = this.GetDistanceFromAxis();
    this.momentOfInertia = (float) (0.33333000540733337 * (double) mass * ((double) length * (double) length));
    this.lastAttachedSegmentIndex = this.segments.Length - 1;
    this.span = this.transform;
    this.tip = new GameObject("tip").transform;
    this.tip.SetParent(this.span);
    this.tip.localPosition = new Vector3(0.0f, 0.0f, length);
    this.tip.localEulerAngles = new Vector3(0.0f, 90f * directionMult, 0.0f);
    this.pitchTransform = new GameObject("pitchTransform").transform;
    this.pitchTransform.SetParent(this.span);
    this.pitchTransform.localEulerAngles = new Vector3(0.0f, 90f * directionMult, 0.0f);
    this.pitchTransform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);
  }

  public void TakeDamage(
    float pierceDamage,
    float blastDamage,
    float amountAffected,
    float fireDamage,
    float impactDamage,
    PersistentID damagedBy)
  {
    float pierceDamage1 = Mathf.Max(pierceDamage - this.armorProperties.pierceArmor, 0.0f) / this.armorProperties.pierceTolerance;
    float blastDamage1 = Mathf.Max(blastDamage - this.armorProperties.blastArmor, 0.0f) / this.armorProperties.blastTolerance;
    float fireDamage1 = Mathf.Max(fireDamage - this.armorProperties.fireArmor, 0.0f) / this.armorProperties.fireTolerance;
    this.hitPoints -= pierceDamage1 + blastDamage1 + fireDamage1 + impactDamage;
    if ((double) this.hitPoints > 0.0)
      return;
    float impactDamage1 = UnityEngine.Random.Range(this.length * 0.5f, this.length * 0.9f) / this.originalLength;
    this.aircraft.Damage(this.damageableIndex, new DamageInfo(pierceDamage1, blastDamage1, fireDamage1, impactDamage1));
  }

  public void ApplyDamage(
    float pierceDamage,
    float blastDamage,
    float fireDamage,
    float impactDamage)
  {
    this.length = this.originalLength * Mathf.Max(impactDamage, 0.1f);
    this.BreakSegments();
  }

  public void Detach(Vector3 velocity, Vector3 relativePos)
  {
  }

  public void TakeShockwave(Vector3 origin, float overpressure, float blastPower)
  {
    if ((double) overpressure <= (double) this.armorProperties.overpressureLimit)
      return;
    this.TakeDamage(0.0f, overpressure - this.armorProperties.overpressureLimit, 1f, 0.0f, 0.0f, PersistentID.None);
  }

  public ArmorProperties GetArmorProperties() => this.armorProperties;

  public Unit GetUnit() => (Unit) this.aircraft;

  public Transform GetTransform() => this.transform;

  public float GetMass() => this.mass;

  public float GetDistanceFromAxis()
  {
    return FastMath.Distance(new Vector3(this.hinge.localPosition.x, 0.0f, this.hinge.localPosition.z), Vector3.zero);
  }

  private void SetupDebugVis()
  {
    if (!PlayerSettings.debugVis)
      return;
    if (DebugVis.Create<Transform>(ref this.pitchVis, GameAssets.i.debugArrow.transform, this.span))
    {
      this.pitchVis.gameObject.GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", Color.cyan);
      this.pitchVis.localPosition = new Vector3(0.0f, 0.0f, 1f);
      this.pitchVis.localEulerAngles = this.tip.localEulerAngles;
    }
    if (DebugVis.Create<Transform>(ref this.flapVis, GameAssets.i.debugArrow.transform, this.span))
    {
      this.flapVis.gameObject.GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", Color.magenta);
      this.flapVis.localPosition = new Vector3(0.0f, 0.0f, this.originalLength);
    }
    if (this.velocityVis != null && this.velocityVis.Length != 0)
      return;
    this.velocityVis = new Transform[this.samples];
    this.liftVis = new Transform[this.samples];
    for (int index = 0; index < this.samples; ++index)
    {
      this.velocityVis[index] = UnityEngine.Object.Instantiate<GameObject>(GameAssets.i.debugArrow, this.span).transform;
      DebugVis.AddMarker(this.velocityVis[index].gameObject);
      this.velocityVis[index].gameObject.GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", new Color(1f, 1f, 1f, 1f));
      this.velocityVis[index].localPosition = new Vector3(0.0f, 0.0f, this.originalLength * (float) (index + 1) / (float) this.samples);
      this.velocityVis[index].localScale = new Vector3(0.5f, 0.5f, 0.5f);
      this.velocityVis[index].rotation = this.tip.rotation;
      this.liftVis[index] = UnityEngine.Object.Instantiate<GameObject>(GameAssets.i.debugArrow, this.span).transform;
      DebugVis.AddMarker(this.liftVis[index].gameObject);
      this.liftVis[index].gameObject.GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", Color.yellow);
      this.liftVis[index].localPosition = new Vector3(0.0f, 0.0f, this.originalLength * (float) (index + 1) / (float) this.samples);
      this.liftVis[index].localScale = new Vector3(0.5f, 0.5f, 0.5f);
      this.liftVis[index].rotation = Quaternion.LookRotation(this.hinge.up);
    }
  }

  private void RefreshDebug(Vector3 velocity, Vector3 force, int sample)
  {
    this.SetupDebugVis();
    this.velocityVis[sample].localScale = new Vector3(0.5f, 0.5f, velocity.magnitude * Time.fixedDeltaTime);
    this.velocityVis[sample].rotation = Quaternion.LookRotation(velocity);
    float z = force.magnitude * 0.0005f;
    this.liftVis[sample].localPosition = new Vector3(0.0f, 0.0f, this.length * (1f + (float) sample) / (float) this.samples);
    this.liftVis[sample].localScale = new Vector3(1f, 1f, z);
    this.liftVis[sample].rotation = Quaternion.LookRotation(force);
  }

  private void VisualizeFlap(Vector3 flapDirection)
  {
    this.SetupDebugVis();
    this.flapVis.localScale = new Vector3(0.5f, 0.5f, this.flapAngularVelocity * this.length);
    this.flapVis.localRotation = Quaternion.LookRotation(flapDirection);
  }

  public void SetPitch(
    Transform swashPlate,
    float swashPlatePitch,
    float swashPlateRoll,
    float collective,
    float directionMult)
  {
    float num1 = Vector3.Dot(this.hinge.forward, swashPlate.forward);
    double num2 = (double) Vector3.Dot(this.hinge.forward, -swashPlate.right);
    float num3 = num1 * swashPlatePitch;
    double num4 = (double) swashPlateRoll;
    float num5 = (float) (num2 * num4);
    float num6 = (float) ((double) num3 * -(double) this.cyclicTravel + (double) num5 * (double) this.cyclicTravel + (double) collective * (double) this.collectiveTravel);
    this.span.localEulerAngles = this.span.localEulerAngles with
    {
      z = num6 * directionMult
    };
  }

  private Vector3 SampleForce(
    float liftNumber,
    float stallAngle,
    float dragBase,
    float dragExponent,
    Vector3 airVelocity,
    out float angleOfAttack)
  {
    Vector3 normalized = Vector3.Cross(airVelocity, -this.tip.right).normalized;
    float sqrMagnitude = airVelocity.sqrMagnitude;
    Vector3 vector3_1 = this.pitchTransform.InverseTransformDirection(airVelocity);
    angleOfAttack = Mathf.Atan2(vector3_1.y, vector3_1.z) * 57.29578f;
    float from = angleOfAttack * liftNumber;
    if ((double) Mathf.Abs(angleOfAttack) > (double) stallAngle)
      from = Mathf.SmoothStep(from, Mathf.Sin((float) (2.0 * (double) angleOfAttack * (Math.PI / 180.0))) * (liftNumber * stallAngle), (float) (((double) Mathf.Abs(angleOfAttack) - (double) stallAngle) * 0.10000000149011612));
    double num1 = (double) dragBase + -(double) Mathf.Cos((float) (2.0 * (double) angleOfAttack * (Math.PI / 180.0))) * (double) dragExponent + (double) dragExponent;
    Vector3 vector3_2 = from * sqrMagnitude * normalized;
    double num2 = (double) sqrMagnitude;
    return (float) (num1 * num2) * -airVelocity.normalized + vector3_2;
  }

  public ForceAndTorque SampleForces(
    Rigidbody rb,
    float angularSpeed,
    float directionMult,
    float liftNumber,
    float stallAngle,
    float dragBase,
    float dragExponent,
    Vector3 airVelocity,
    float deltaTime,
    out float angleOfAttack)
  {
    this.forceAndTorque.Clear();
    this.shaftAngularSpeed = angularSpeed;
    Vector3 vector3_1 = this.hinge.right * directionMult;
    Vector3 vector3_2 = Vector3.Cross(this.span.forward, this.hinge.right);
    float num1 = Mathf.Cos(Mathf.Abs(this.flapAngle));
    angleOfAttack = 0.0f;
    Vector3 zero1 = Vector3.zero;
    Vector3 zero2 = Vector3.zero;
    float num2 = 0.0f;
    float num3 = this.length / (this.originalLength * (float) this.samples);
    for (int sample = 0; sample < this.samples; ++sample)
    {
      float num4 = this.length * (float) (sample + 1) * num3;
      Vector3 worldPoint = this.hinge.position + this.span.forward * num4;
      this.pitchTransform.localEulerAngles = new Vector3(-this.washout * (float) (1.0 - (double) num4 / (double) this.originalLength), 90f * directionMult, 0.0f);
      float num5 = angularSpeed * (num4 + this.hubRadius) * num1;
      Vector3 vector3_3 = num5 * vector3_1 - airVelocity + rb.GetPointVelocity(worldPoint) + num4 * this.flapAngularVelocity * vector3_2;
      Vector3 force = this.SampleForce(liftNumber, stallAngle, dragBase, dragExponent, vector3_3, out angleOfAttack) + (float) ((double) this.originalMass * (double) num3 * -9.8100004196167) * Vector3.up;
      if (PlayerSettings.debugVis)
        this.RefreshDebug(vector3_3, force, sample);
      Vector3 vector3_4 = (float) ((double) this.originalMass * (double) num3 * ((double) num5 * (double) num5) / ((double) num4 + (double) this.hubRadius)) * this.hinge.forward;
      Vector3 lhs = force + vector3_4;
      num2 += Vector3.Dot(lhs, vector3_2) * num4;
      zero1 += lhs;
      zero2 += Vector3.Cross(lhs, this.hub.position - worldPoint);
    }
    Vector3 vector3_5 = zero1 * this.aircraft.airDensity;
    float num6 = num2 / this.stiffnessMultiplier;
    float num7 = (double) this.flapAngle < 0.0 ? this.flapSpringDown : this.flapSpringUp;
    if ((double) Mathf.Abs(this.flapAngle) > 0.5)
      num7 *= 5f;
    float num8 = num6 + num7 * -this.flapAngle + -this.flapAngularVelocity * this.flapDamp;
    this.flapAngularVelocity += deltaTime * num8 / this.momentOfInertia;
    this.flapAngle += deltaTime * this.flapAngularVelocity;
    float num9 = num8 / (this.length * 0.7f);
    Vector3 force1 = vector3_5 - num9 * vector3_2;
    Vector3 torque = zero2 - Vector3.Cross(num9 * vector3_2, (this.hub.position - this.tip.position) * 0.7f);
    if (PlayerSettings.debugVis)
      this.VisualizeFlap(vector3_2);
    this.span.localEulerAngles = this.span.localEulerAngles with
    {
      x = (float) (-(double) this.flapAngle * 57.295780181884766)
    };
    return new ForceAndTorque(force1, torque, true);
  }

  public void BendSegments(float speedRatio)
  {
    if (this.lastAttachedSegmentIndex < 0)
      return;
    for (int index = 0; index <= this.lastAttachedSegmentIndex; ++index)
      this.segments[index].Animate(speedRatio);
    int num1 = (double) speedRatio >= 0.20000000298023224 ? 0 : ((double) this.flapAngle < 0.0 ? 1 : 0);
    float num2 = (float) (-(double) this.flapAngle * 57.295780181884766);
    if (num1 != 0)
    {
      this.bendSegments = true;
      this.segments[0].transform.localEulerAngles = new Vector3((float) (-(double) num2 * 0.5), 0.0f, 0.0f);
      float x = num2 / (float) (this.segments.Length - 1);
      for (int index = 1; index <= this.lastAttachedSegmentIndex; ++index)
        this.segments[index].transform.localEulerAngles = new Vector3(x, 0.0f, 0.0f);
    }
    else
    {
      if (!this.bendSegments)
        return;
      this.bendSegments = false;
      for (int index = 0; index <= this.lastAttachedSegmentIndex; ++index)
        this.segments[index].transform.localEulerAngles = Vector3.zero;
    }
  }

  public float GetLength() => this.length;

  public void BreakRandomSegment()
  {
    this.length *= 0.5f;
    this.aircraft.Damage(this.damageableIndex, new DamageInfo(0.0f, 0.0f, 0.0f, this.length / this.originalLength));
  }

  private void BreakSegments()
  {
    float num1 = this.length / this.originalLength;
    this.mass = this.originalMass * num1;
    for (int attachedSegmentIndex = this.lastAttachedSegmentIndex; attachedSegmentIndex >= 0; --attachedSegmentIndex)
    {
      if ((double) num1 < (0.5 + (double) attachedSegmentIndex) / (double) this.segments.Length)
      {
        this.lastAttachedSegmentIndex = attachedSegmentIndex - 1;
        float num2 = this.shaftAngularSpeed * ((float) attachedSegmentIndex / (float) this.segments.Length) * this.originalLength;
        this.segments[attachedSegmentIndex].Detach(this.aircraft, num2 * 0.5f, this.originalMass / (float) this.segments.Length);
      }
    }
  }

  public bool CheckCollisions()
  {
    RaycastHit hitInfo;
    bool flag1 = Physics.Linecast(this.span.position, this.span.position + this.span.forward * this.length, out hitInfo, -40961);
    float enter;
    bool flag2 = Datum.WaterPlane().Raycast(new Ray(this.span.position, this.span.forward), out enter) && (double) enter < (double) this.length && (double) enter > 0.0;
    if (!flag1 && !flag2)
      return false;
    if (!flag2)
      enter = this.length;
    float num = Mathf.Min(hitInfo.distance, enter);
    Vector3 inNormal = flag1 ? hitInfo.normal : Vector3.up;
    Vector3 vector3 = this.hinge.forward * this.shaftAngularSpeed * this.length;
    this.rotorShaft.RotorStrike(this.length * 1000f);
    if ((double) vector3.sqrMagnitude < 100.0)
      return true;
    if ((double) num < (double) this.length && this.aircraft.LocalSim)
      this.aircraft.Damage(this.damageableIndex, new DamageInfo(0.0f, 0.0f, 0.0f, num / this.originalLength));
    Vector3 forward = (Vector3.Reflect(vector3.normalized, inNormal) + Vector3.Reflect(Vector3.Normalize(this.tip.position - this.span.position), inNormal)) / 2f;
    if (flag1)
      UnityEngine.Object.Destroy((UnityEngine.Object) UnityEngine.Object.Instantiate<GameObject>((UnityEngine.Object) hitInfo.collider.sharedMaterial == (UnityEngine.Object) GameAssets.i.terrainMaterial ? GameAssets.i.rotorStrike_dirt : GameAssets.i.rotorStrike_solid, hitInfo.point, Quaternion.LookRotation(forward)), 10f);
    else
      UnityEngine.Object.Destroy((UnityEngine.Object) UnityEngine.Object.Instantiate<GameObject>(GameAssets.i.rotorStrike_water, this.span.position + this.span.forward * enter, Quaternion.LookRotation(forward)), 10f);
    return true;
  }

  public void ShatterRotor()
  {
    if (!this.aircraft.LocalSim)
      return;
    this.aircraft.Damage(this.damageableIndex, new DamageInfo(0.0f, 0.0f, 0.0f, 0.1f));
  }

  [Serializable]
  private class RotorSegment
  {
    public Transform transform;
    [SerializeField]
    private MeshFilter meshFilter;
    [SerializeField]
    private MeshRenderer meshRenderer;
    [SerializeField]
    private Material slowMaterial;
    [SerializeField]
    private Material fastMaterial;
    [SerializeField]
    private Mesh slowMesh;
    [SerializeField]
    private Mesh fastMesh;
    private bool detached;
    private bool blurred;

    public void Animate(float speedRatio)
    {
      if (this.detached)
        return;
      bool flag = (double) speedRatio * (double) Time.timeScale > 0.60000002384185791;
      if (this.blurred)
      {
        if (flag)
          return;
        this.blurred = false;
        this.meshFilter.mesh = this.slowMesh;
        this.meshRenderer.material = this.slowMaterial;
      }
      else
      {
        if (!flag)
          return;
        this.blurred = true;
        this.meshFilter.mesh = this.fastMesh;
        this.meshRenderer.material = this.fastMaterial;
      }
    }

    public void Detach(Aircraft aircraft, float tipSpeed, float mass)
    {
      this.detached = true;
      this.meshFilter.mesh = this.slowMesh;
      this.meshRenderer.material = this.slowMaterial;
      this.transform.SetParent((Transform) null);
      this.transform.gameObject.AddComponent<BoxCollider>();
      Rigidbody rigidbody = this.transform.gameObject.AddComponent<Rigidbody>();
      this.transform.gameObject.layer = 15;
      rigidbody.mass = mass;
      rigidbody.velocity = aircraft.rb.velocity + new Vector3(UnityEngine.Random.Range(-tipSpeed, tipSpeed), UnityEngine.Random.Range(0.0f, tipSpeed), UnityEngine.Random.Range(-tipSpeed, tipSpeed));
      rigidbody.angularVelocity = UnityEngine.Random.insideUnitSphere * 2f;
      rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
      rigidbody.drag = 0.1f;
      UnityEngine.Object.Destroy((UnityEngine.Object) this.transform.gameObject, 30f);
    }
  }
}

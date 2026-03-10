// Decompiled with JetBrains decompiler
// Type: SoftBodyRotor
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;

#nullable disable
public class SoftBodyRotor : MonoBehaviour, IDamageable
{
  private SoftBodyRotor.MassPoint[] anchoredMassPoints;
  private SoftBodyRotor.MassPoint[] massPoints;
  private SoftBodyRotor.Segment[] segments;
  public RotorShaft rotorShaft;
  public Transform tip;
  [SerializeField]
  private Transform swashPlate;
  [SerializeField]
  private Aircraft aircraft;
  [SerializeField]
  private Mesh mesh_slow;
  [SerializeField]
  private Material material_slow;
  [SerializeField]
  private Mesh mesh_fast;
  [SerializeField]
  private Material material_fast;
  [SerializeField]
  private ArmorProperties armorProperties;
  private float hitpoints = 100f;
  private Vector3 centripetalForce;
  private GameObject vectorDebug1;
  private GameObject vectorDebug2;
  [SerializeField]
  private GameObject[] breakEffects;
  private float originalLength;
  private float length;
  private float originalMass;
  private float originalArea;
  [SerializeField]
  private float mass;
  [SerializeField]
  private float maxPitchRate;
  [SerializeField]
  private float flapStiffness;
  [SerializeField]
  private float flapDamping;
  [SerializeField]
  private float stiffnessAtSpeed;
  private int segmentsRemoved;
  private float bladePitch;
  private float swashPlatePitch;
  private float swashPlateRoll;
  private float swashPlateCollective;
  private float flapForce;
  private float flapSpeed;
  private float flapPosition;
  public float bladeArticulation;
  private Vector3 bladeTurnDirection;
  private ForceAndTorque forceAndTorque;
  private RaycastHit rotorHit;
  private Renderer rotorRenderer;
  private MeshFilter meshFilter;
  private float spawnTime;
  private bool broken;
  private byte index;

  private void Awake()
  {
    this.rotorRenderer = this.GetComponent<Renderer>();
    this.meshFilter = this.GetComponent<MeshFilter>();
    this.index = this.aircraft.RegisterDamageable((IDamageable) this);
    this.originalLength = Vector3.Distance(this.transform.position, this.tip.transform.position);
    this.originalMass = this.mass;
    this.length = this.originalLength;
    this.massPoints = new SoftBodyRotor.MassPoint[3];
    this.segments = new SoftBodyRotor.Segment[2];
    this.massPoints[0] = new SoftBodyRotor.MassPoint(25f, this.transform.position + this.transform.forward * 0.6f, this.swashPlate);
    this.massPoints[1] = new SoftBodyRotor.MassPoint(0.0f, this.transform.position + this.transform.forward * this.length, this.swashPlate);
    this.massPoints[2] = new SoftBodyRotor.MassPoint(25f, this.transform.position + this.transform.forward * this.length, (Transform) null);
    this.segments[0] = new SoftBodyRotor.Segment(this.massPoints[0], this.massPoints[2], 400000f, 1000f);
    this.segments[1] = new SoftBodyRotor.Segment(this.massPoints[1], this.massPoints[2], 300f, 50f);
    if (PlayerSettings.debugVis)
    {
      this.vectorDebug1 = UnityEngine.Object.Instantiate<GameObject>(GameAssets.i.debugArrow, this.tip.transform);
      this.vectorDebug2 = UnityEngine.Object.Instantiate<GameObject>(GameAssets.i.debugArrow, this.tip.transform);
    }
    this.rotorShaft.GetUnitPart().onParentDetached += new Action<UnitPart>(this.SwashRotor_OnRotorShaftDetach);
    this.aircraft.onEject += new Action(this.SwashRotor_OnEject);
  }

  public void TakeDamage(
    float pierceDamage,
    float blastDamage,
    float amountAffected,
    float fireDamage,
    float impactDamage,
    PersistentID damagedBy)
  {
    if ((double) impactDamage > 0.0)
    {
      if (!this.aircraft.remoteSim)
        return;
      int num = this.broken ? 1 : 0;
    }
    else
    {
      float pierceDamage1 = Mathf.Max(pierceDamage - this.armorProperties.pierceArmor, 0.0f) / this.armorProperties.pierceTolerance;
      float blastDamage1 = Mathf.Max(blastDamage - this.armorProperties.blastArmor, 0.0f) / this.armorProperties.blastTolerance;
      float fireDamage1 = Mathf.Max(fireDamage - this.armorProperties.fireArmor, 0.0f) / this.armorProperties.fireTolerance;
      if ((double) pierceDamage1 + (double) blastDamage1 + (double) fireDamage1 + (double) impactDamage <= 0.0)
        return;
      this.aircraft.Damage(this.index, new DamageInfo(pierceDamage1, blastDamage1, fireDamage1, impactDamage));
    }
  }

  public void ApplyDamage(
    float pierceDamage,
    float blastDamage,
    float fireDamage,
    float impactDamage)
  {
    this.hitpoints -= pierceDamage + blastDamage + fireDamage + impactDamage;
    if ((double) this.hitpoints > 0.0)
      return;
    this.rotorShaft.ReportDamage();
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

  private void SwashRotor_OnEject()
  {
  }

  public void Simulate(float deltaTime, out ForceAndTorque forceAndTorque)
  {
    forceAndTorque = new ForceAndTorque();
    foreach (SoftBodyRotor.MassPoint massPoint in this.massPoints)
    {
      ForceAndTorque forceAndTorque1;
      massPoint.Simulate(deltaTime, out forceAndTorque1);
      forceAndTorque.Add(forceAndTorque1);
    }
    foreach (SoftBodyRotor.Segment segment in this.segments)
      segment.Simulate(deltaTime);
  }

  public Vector3 GetCentripetalForce(float angularVelocity)
  {
    float num = angularVelocity * this.length;
    return Vector3.Cross(this.bladeTurnDirection, this.swashPlate.up) * this.mass * 0.5f * (num * num / Mathf.Max(this.length, 0.1f));
  }

  private Vector3 SampleForce(
    float liftNumber,
    float stallAngle,
    float dragBase,
    float dragExponent,
    Vector3 airVelocity,
    float vrsFactor)
  {
    float sqrMagnitude = airVelocity.sqrMagnitude;
    Vector3 normalized = Vector3.Cross(airVelocity, -this.tip.right).normalized;
    float f = Mathf.Clamp(TargetCalc.GetAngleOnAxis(airVelocity, this.tip.forward, this.tip.right), -45f, 45f);
    float num1 = (double) Mathf.Abs(f) < (double) stallAngle ? f * liftNumber : Mathf.Lerp(liftNumber * stallAngle * Mathf.Sign(f), 0.0f, (float) (((double) Mathf.Abs(f) - (double) stallAngle) * 0.039999999105930328));
    double num2 = (double) dragBase + (double) dragExponent * (double) f * (double) f;
    Vector3 vector3_1 = num1 * sqrMagnitude * normalized;
    double num3 = (double) sqrMagnitude;
    Vector3 vector3_2 = (float) (num2 * num3) * -airVelocity.normalized;
    return vector3_1 * (1f - vrsFactor) + vector3_2;
  }

  public Vector3 CalculateLift(
    ControlInputs controlInputs,
    float liftNumber,
    float stallAngle,
    float dragBase,
    float dragExponent,
    Rigidbody hubRB,
    float angularVelocity,
    Vector3 downdraft,
    float VRSFactor,
    out float shaftTorque)
  {
    Vector3 vector3_1 = hubRB.GetPointVelocity(this.tip.position) - ((UnityEngine.Object) NetworkSceneSingleton<LevelInfo>.i != (UnityEngine.Object) null ? NetworkSceneSingleton<LevelInfo>.i.GetWind(this.tip.GlobalPosition()) : Vector3.zero) + downdraft;
    Vector3 lhs = Vector3.Cross(this.tip.right, this.swashPlate.up);
    if ((double) this.spawnTime > 2.0)
      this.CheckCollisions(angularVelocity * this.length * lhs);
    else
      this.spawnTime += Time.fixedDeltaTime;
    Vector3 zero = Vector3.zero;
    shaftTorque = 0.0f;
    for (float num1 = 0.35f; (double) num1 < 1.0; num1 += 0.3f)
    {
      float num2 = num1 * this.length;
      Vector3 rhs = this.SampleForce(liftNumber, stallAngle, dragBase, dragExponent, vector3_1 + num2 * angularVelocity * lhs, VRSFactor);
      float num3 = Vector3.Dot(lhs, rhs);
      Vector3 vector3_2 = rhs - num3 * lhs;
      shaftTorque += num3 * num2;
      zero += vector3_2;
    }
    return zero;
  }

  private void SwashRotor_OnRotorShaftDetach(UnitPart part) => this.broken = true;

  private void CheckCollisions(Vector3 tipVelocity)
  {
    bool flag = false;
    int num1 = Physics.Linecast(this.transform.position, this.tip.position, out this.rotorHit, -40961) ? 1 : 0;
    if (num1 == 0 && !this.aircraft.remoteSim && (double) this.tip.position.y < (double) Datum.LocalSeaY && (double) this.transform.position.y > (double) Datum.LocalSeaY)
    {
      Plane plane = Datum.WaterPlane();
      Vector3 direction = Vector3.Normalize(this.tip.position - this.transform.position);
      Ray ray = new Ray(this.transform.position, direction);
      float enter;
      if (plane.Raycast(ray, out enter) && (double) enter < (double) this.length)
      {
        flag = true;
        this.rotorHit = new RaycastHit();
        this.rotorHit.distance = enter;
        this.rotorHit.point = this.transform.position + direction * enter;
        this.rotorHit.normal = Vector3.up;
      }
    }
    double magnitude = (double) tipVelocity.magnitude;
    double length = (double) this.length;
    double distance = (double) this.rotorHit.distance;
    int num2 = num1 | (flag ? 1 : 0);
  }

  private class MassPoint
  {
    public float mass;
    public Vector3 position;
    private Vector3 velocity;
    private Vector3 velocityPrev;
    private Vector3 frameForces;
    public bool anchoredToTransform;
    private Vector3 anchoredPosition;
    private Transform anchoredTransform;
    private Transform debugPointTransform;

    public MassPoint(float mass, Vector3 position, Transform anchoredTransform)
    {
      this.anchoredToTransform = (UnityEngine.Object) anchoredTransform != (UnityEngine.Object) null;
      this.mass = mass;
      this.position = position;
      if (this.anchoredToTransform)
      {
        this.anchoredTransform = anchoredTransform;
        this.anchoredPosition = anchoredTransform.InverseTransformPoint(position);
      }
      this.debugPointTransform = UnityEngine.Object.Instantiate<GameObject>(GameAssets.i.debugPoint, Datum.origin).transform;
      this.debugPointTransform.localScale = Vector3.one * 0.15f;
    }

    public void MoveAnchoredTransform(float deltaTime)
    {
      Vector3 vector3_1 = this.anchoredTransform.TransformPoint(this.anchoredPosition);
      this.velocity = (vector3_1 - this.position) / deltaTime;
      this.position = vector3_1;
      if ((double) this.mass <= 0.0)
        return;
      Vector3 vector3_2 = (this.velocity - this.velocityPrev) / deltaTime;
      this.velocityPrev = this.velocity;
      this.AddForce(vector3_2 * this.mass);
    }

    public void AddForce(Vector3 force) => this.frameForces += force;

    public void Simulate(float deltaTime, out ForceAndTorque forceAndTorque)
    {
      this.AddForce(this.mass * -9.81f * Vector3.up);
      forceAndTorque = new ForceAndTorque();
      if (!this.anchoredToTransform)
      {
        this.velocity += this.frameForces * deltaTime / this.mass;
        this.position += this.velocity * deltaTime;
      }
      else
      {
        this.MoveAnchoredTransform(deltaTime);
        forceAndTorque = new ForceAndTorque(this.frameForces, this.position - this.anchoredTransform.position);
      }
      this.debugPointTransform.position = this.position;
      this.frameForces = Vector3.zero;
    }
  }

  private class Segment
  {
    private float restLength;
    private float lengthPrev;
    private float spring;
    private float damp;
    private SoftBodyRotor.MassPoint startMass;
    private SoftBodyRotor.MassPoint endMass;
    private Transform debugArrow;

    public Segment(
      SoftBodyRotor.MassPoint startMass,
      SoftBodyRotor.MassPoint endMass,
      float spring,
      float damp)
    {
      this.spring = spring;
      this.damp = damp;
      this.startMass = startMass;
      this.endMass = endMass;
      this.restLength = FastMath.Distance(startMass.position, endMass.position);
      this.lengthPrev = this.restLength;
      this.debugArrow = UnityEngine.Object.Instantiate<GameObject>(GameAssets.i.debugArrowGreen, Datum.origin).transform;
      this.debugArrow.transform.localScale = new Vector3(0.2f, 0.2f, this.restLength);
      this.debugArrow.rotation = Quaternion.LookRotation(startMass.position - endMass.position);
      this.debugArrow.position = startMass.position;
    }

    public void Simulate(float deltaTime)
    {
      Vector3 normalized = (this.endMass.position - this.startMass.position).normalized;
      float z = FastMath.Distance(this.startMass.position, this.endMass.position);
      double num1 = ((double) z - (double) this.lengthPrev) / (double) deltaTime;
      float num2 = (z - this.restLength) * this.spring;
      double damp = (double) this.damp;
      float num3 = (float) (num1 * damp);
      this.lengthPrev = z;
      this.startMass.AddForce(normalized * (num2 + num3));
      this.endMass.AddForce(-normalized * (num2 + num3));
      this.debugArrow.SetPositionAndRotation(this.startMass.position, Quaternion.LookRotation(normalized));
      this.debugArrow.transform.localScale = new Vector3(0.2f, 0.2f, z);
    }
  }
}

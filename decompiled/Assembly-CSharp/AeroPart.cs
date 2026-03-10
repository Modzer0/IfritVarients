// Decompiled with JetBrains decompiler
// Type: AeroPart
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.Jobs;
using System;
using System.Collections.Generic;
using Unity.Profiling;
using UnityEngine;

#nullable disable
public class AeroPart : UnitPart
{
  private static readonly ProfilerMarker aeroPhysicsMarker = new ProfilerMarker("AeroPhysics");
  [Header("Structure")]
  [SerializeField]
  protected PartJoint[] joints;
  private List<PartJoint> movingJoints = new List<PartJoint>();
  [Header("Aerodynamics")]
  [SerializeField]
  private float wingArea;
  public float dragArea;
  private float wingEffectiveness = 1f;
  [SerializeField]
  private Transform liftNormal;
  [SerializeField]
  private Transform connectedAnchor;
  [SerializeField]
  private Vector3 centerOfLift;
  [SerializeField]
  private float buoyancy = 2f;
  [SerializeField]
  private float airflowChanneling;
  [SerializeField]
  private int airfoil = -1;
  private int airfoilID;
  private float submergedAmount;
  private bool simplePhysics = true;
  private GameObject debugArrow;
  private PtrAllocation<AeroPartFields> JobFields;
  private NuclearOption.Jobs.JobPart<AeroPart, AeroPartFields> JobPart;

  protected override void Awake()
  {
    base.Awake();
    this.enabled = false;
    if ((UnityEngine.Object) this.liftNormal == (UnityEngine.Object) null)
      this.liftNormal = this.xform;
    this.baseDrag = this.dragArea;
    if (this.attachInfo != null)
      this.attachInfo.attachmentStrength = 0.0f;
    for (int index = 0; index < this.joints.Length; ++index)
    {
      if (index == 0)
        this.attachInfo.attachmentStrength += this.joints[index].breakForce + this.joints[index].breakTorque;
      if ((UnityEngine.Object) this.joints[index].tensor != (UnityEngine.Object) null)
        this.joints[index].tensor.onJointBroken += new Action<UnitPart>(this.Part_OnTensorBreak);
    }
    if (this.parentUnit is Aircraft parentUnit)
      parentUnit.RegisterAeroPart(this);
    this.parentUnit.onInitialize += new Action(this.AeroPart_OnInitialize);
  }

  private void AeroPart_OnInitialize()
  {
    if (this.parentUnit.remoteSim || !(this.parentUnit is Aircraft))
      return;
    this.airfoilID = (this.parentUnit.definition as AircraftDefinition).aircraftParameters.GetAirfoilID(this.airfoil);
    this.JobPart = new NuclearOption.Jobs.JobPart<AeroPart, AeroPartFields>(this, this.GetOrCreateJobField());
    JobManager.Add(this.JobPart);
  }

  public float GetWingArea() => this.wingArea;

  public void SetWingArea(float area) => this.wingArea = area;

  public float GetAltitude() => this.liftNormal.position.GlobalY();

  public Transform GetLiftTransform() => this.liftNormal;

  public override void Repair()
  {
    base.Repair();
    if (this.attachInfo == null)
      return;
    this.detachedFromUnit = false;
    this.xform.position = this.attachInfo.parentPart.xform.TransformPoint(this.attachInfo.localPosition);
    this.xform.rotation = this.attachInfo.parentPart.xform.rotation * this.attachInfo.localRotation;
  }

  private void OnTriggerStay(Collider other)
  {
    Collider component;
    if (!((UnityEngine.Object) other.sharedMaterial == (UnityEngine.Object) GameAssets.i.WaterMaterial) || !this.gameObject.TryGetComponent<Collider>(out component))
      return;
    Transform transform1 = component.transform;
    Transform transform2 = other.transform;
    Physics.ComputePenetration(component, transform1.position, transform1.rotation, other, transform2.position, transform2.rotation, out Vector3 _, out this.submergedAmount);
  }

  private void OnCollisionEnter(Collision collision)
  {
    if ((UnityEngine.Object) collision.body == (UnityEngine.Object) null)
    {
      float magnitude = collision.impulse.magnitude;
      if ((double) magnitude > 1000.0 && (double) this.xform.position.y > (double) Datum.LocalSeaY)
        SceneSingleton<EffectManager>.i.ImpactDust(magnitude, collision.GetContact(0).point.ToGlobalPosition(), Quaternion.LookRotation(collision.impulse + this.rb.velocity * this.rb.mass * 5f));
    }
    if ((double) this.impactDamage.threshold <= 0.0 || !this.parentUnit.LocalSim)
      return;
    float impactDamage = ((float) ((double) collision.impulse.magnitude / (double) Time.fixedDeltaTime / ((double) this.rb.mass * 9.8100004196167)) - this.impactDamage.threshold) * this.impactDamage.multiplier;
    if ((double) impactDamage <= 0.0)
      return;
    if (this.parentUnit.IsServer)
      this.TakeDamage(0.0f, 0.0f, 1f, 0.0f, impactDamage, PersistentID.None);
    else
      this.parentUnit.Damage(this.id, new DamageInfo(0.0f, 0.0f, 0.0f, impactDamage));
  }

  private void OnCollisionStay(Collision collision)
  {
    if ((double) collision.relativeVelocity.sqrMagnitude < 25.0)
      return;
    Vector3 point = collision.GetContact(0).point;
    if ((double) point.y < (double) Datum.LocalSeaY)
      return;
    if ((double) UnityEngine.Random.value > 0.75 && (UnityEngine.Object) collision.collider.sharedMaterial == (UnityEngine.Object) GameAssets.i.terrainMaterial)
    {
      SceneSingleton<ParticleEffectManager>.i.EmitParticles("ContactDust", 1, this.xform.GlobalPosition(), this.rb.velocity, 0.0f, Mathf.Max((float) (5.0 - (double) this.parentUnit.speed * 0.10000000149011612), 1f), 0.5f, Mathf.Max(this.collisionSize.x + this.collisionSize.y + this.collisionSize.z, 4f) + Mathf.Min(this.parentUnit.speed * 0.05f, 10f), 0.3f, this.parentUnit.speed * 0.3f, 0.5f, 0.3f);
    }
    else
    {
      if ((double) UnityEngine.Random.value <= 0.75 || !((UnityEngine.Object) collision.collider.sharedMaterial == (UnityEngine.Object) null) || !(this.parentUnit is Aircraft parentUnit))
        return;
      parentUnit.ThrowSparks(point, Vector3.zero);
      SceneSingleton<ParticleEffectManager>.i.EmitParticles("ContactSmoke", 1, this.xform.GlobalPosition(), this.rb.velocity, 0.0f, Mathf.Max((float) (3.0 - (double) this.parentUnit.speed * 0.10000000149011612), 1f), 0.5f, Mathf.Max(this.collisionSize.x + this.collisionSize.y + this.collisionSize.z, 4f) + Mathf.Min(this.parentUnit.speed * 0.05f, 10f), 0.3f, this.parentUnit.speed * 0.3f, 0.2f, 0.3f);
    }
  }

  public override void ModifyDrag(float amount) => this.dragArea += amount;

  private void Part_OnTensorBreak(UnitPart part)
  {
    for (int index = 0; index < this.joints.Length; ++index)
    {
      if ((UnityEngine.Object) part == (UnityEngine.Object) this.joints[index].tensor && (UnityEngine.Object) this.joints[index].joint != (UnityEngine.Object) null)
      {
        this.joints[index].joint.breakForce = 0.0f;
        this.joints[index].joint.breakTorque = 0.0f;
        if ((double) this.parentUnit.displayDetail < 1.0)
          break;
        AudioSource audioSource = this.gameObject.AddComponent<AudioSource>();
        audioSource.outputAudioMixerGroup = SoundManager.i.EffectsMixer;
        audioSource.bypassListenerEffects = true;
        audioSource.clip = this.joints[index].breakSound;
        audioSource.pitch = UnityEngine.Random.Range(0.7f, 1.5f);
        audioSource.spatialBlend = 1f;
        audioSource.dopplerLevel = 1f;
        audioSource.spread = 5f;
        audioSource.maxDistance = 500f;
        audioSource.minDistance = 5f;
        audioSource.volume = 0.8f;
        audioSource.Play();
        UnityEngine.Object.Destroy((UnityEngine.Object) audioSource, 3f);
      }
    }
  }

  public void CheckAttachment()
  {
    if (this.simplePhysics || this.attachInfo == null || this.attachInfo.detachedFromParentPart || !FastMath.OutOfRange(this.attachInfo.localPosition, this.attachInfo.parentPart.xform.InverseTransformPoint(this.xform.position), 0.5f))
      return;
    this.attachInfo.detachedFromParentPart = true;
    this.parentUnit.DetachPart(this.id, this.rb.velocity, this.xform.position - this.parentUnit.transform.position);
  }

  public override void TakeShockwave(Vector3 origin, float overpressure, float blastPower)
  {
    float num = (float) (((double) this.collisionSize.x + (double) this.collisionSize.y + (double) this.collisionSize.z) * 0.33329999446868896);
    float a = Mathf.Min((float) ((double) this.wingArea + (double) this.dragArea + (double) num * (double) num), (float) ((double) blastPower * (double) blastPower * 1.0)) * overpressure;
    this.rb.AddForceAtPosition(Vector3.Lerp(FastMath.NormalizedDirection(origin, this.xform.position), UnityEngine.Random.insideUnitSphere.normalized, overpressure * (1f / 1000f)) * Mathf.Min(a, 60f * this.rb.mass), this.liftNormal.position, ForceMode.Impulse);
  }

  public override void Detach(Vector3 velocity, Vector3 relativePos)
  {
    base.Detach(velocity, relativePos);
    if (FastMath.InRange(SceneSingleton<CameraStateManager>.i.transform.position, this.transform.position, 5000f))
      this.SpawnFragments();
    if (this.simplePhysics)
    {
      Rigidbody rb = this.attachInfo.parentPart.rb;
      double mass = (double) this.attachInfo.parentPart.rb.mass;
      this.xform.SetParent((Transform) null);
      this.rb = this.gameObject.AddComponent<Rigidbody>();
      this.rb.mass = this.CalcMassWithChildren();
      this.rb.interpolation = RigidbodyInterpolation.Interpolate;
      this.rb.velocity = velocity;
      this.rb.angularVelocity = this.attachInfo.parentPart.rb.angularVelocity;
    }
    this.rb.drag = 0.15f;
    this.wingEffectiveness = 0.0f;
  }

  protected override void UnitPart_OnParentDetached(UnitPart parentPart)
  {
    base.UnitPart_OnParentDetached((UnitPart) this);
  }

  public void CreateRB(Vector3 velocity, Vector3 position)
  {
    if ((double) this.mass == 0.0 || this.attachInfo == null || (UnityEngine.Object) this.rb != (UnityEngine.Object) null && (UnityEngine.Object) this.rb != (UnityEngine.Object) this.parentUnit.rb)
    {
      if ((UnityEngine.Object) this.rb == (UnityEngine.Object) null)
        Debug.LogError((object) $"Couldn't find rb for part {this.gameObject}, attachInfo.parentPart = {this.attachInfo.parentPart}");
      this.rb.mass = this.mass;
    }
    else
    {
      if (this.joints.Length != 0 && (UnityEngine.Object) this.joints[0].connectedPart == (UnityEngine.Object) null)
        this.joints[0].connectedPart = this.attachInfo.parentPart;
      this.xform.SetParent((Transform) null, true);
      this.rb = this.gameObject.AddComponent<Rigidbody>();
      this.rb.mass = this.mass;
      this.rb.drag = 0.0f;
      this.rb.angularDrag = 0.0f;
      this.rb.sleepThreshold = 0.0f;
      this.rb.velocity = velocity;
      this.rb.angularVelocity = this.parentUnit.rb.angularVelocity;
      this.rb.useGravity = true;
      if (position != Vector3.zero)
        this.rb.position = position;
      this.rb.interpolation = RigidbodyInterpolation.Interpolate;
      this.simplePhysics = false;
      if (!((UnityEngine.Object) this.debugArrow != (UnityEngine.Object) null))
        return;
      this.debugArrow.GetComponent<MeshRenderer>().material.color = Color.red;
    }
  }

  public void CreateJoints()
  {
    for (int index = 0; index < this.joints.Length; ++index)
    {
      PartJoint joint = this.joints[index];
      FixedJoint fixedJoint = this.gameObject.AddComponent<FixedJoint>();
      if ((UnityEngine.Object) joint.anchor != (UnityEngine.Object) null)
        fixedJoint.anchor = joint.anchor.position - this.rb.position;
      fixedJoint.connectedBody = joint.connectedPart.rb;
      fixedJoint.enableCollision = false;
      fixedJoint.breakForce = joint.breakForce * 10f;
      fixedJoint.breakTorque = joint.breakTorque * 10f;
      joint.joint = (Joint) fixedJoint;
      if (joint.solverIterations != 6)
        this.rb.solverIterations = joint.solverIterations;
    }
  }

  public void SetHingeJoint(
    int index,
    AeroPart connectedPart,
    float spring,
    float damp,
    float targetPosition,
    float breakStrength,
    float baseAngleOffset)
  {
    if ((UnityEngine.Object) this.rb == (UnityEngine.Object) this.parentUnit.rb)
    {
      this.xform.localEulerAngles = new Vector3(targetPosition + baseAngleOffset, 0.0f, 0.0f);
    }
    else
    {
      if (this.movingJoints.Count <= index)
      {
        PartJoint partJoint = new PartJoint();
        HingeJoint hingeJoint = this.gameObject.AddComponent<HingeJoint>();
        hingeJoint.connectedBody = connectedPart.rb;
        hingeJoint.anchor = Vector3.zero;
        this.rb.solverIterations = 8;
        connectedPart.rb.solverIterations = 8;
        hingeJoint.breakForce = breakStrength;
        hingeJoint.breakTorque = breakStrength;
        hingeJoint.useSpring = true;
        partJoint.joint = (Joint) hingeJoint;
        this.movingJoints.Add(partJoint);
      }
      HingeJoint joint = this.movingJoints[index].joint as HingeJoint;
      if ((UnityEngine.Object) joint == (UnityEngine.Object) null)
        return;
      JointSpring jointSpring = new JointSpring()
      {
        spring = spring,
        damper = damp,
        targetPosition = targetPosition
      };
      joint.spring = jointSpring;
    }
  }

  public void BreakAllJoints()
  {
    for (int index = this.joints.Length - 1; index >= 0; --index)
    {
      if ((UnityEngine.Object) this.joints[index].joint != (UnityEngine.Object) null)
        UnityEngine.Object.Destroy((UnityEngine.Object) this.joints[index].joint);
    }
    this.joints = Array.Empty<PartJoint>();
  }

  ~AeroPart() => AeroPart.DisposeJobFields(ref this.JobFields);

  private static void DisposeJobFields(ref PtrAllocation<AeroPartFields> fields)
  {
    if (fields.IsCreated)
    {
      ref AeroPartFields local = ref fields.Ref();
      if (local.liftTransformIndex.IsCreated)
        local.liftTransformIndex.RemoveRef();
      if (local.otherTransformIndex.IsCreated)
        local.otherTransformIndex.RemoveRef();
    }
    fields.Dispose();
  }

  public bool GetJobTransforms(out Transform liftTransform, out Transform otherTransform)
  {
    if ((double) this.airflowChanneling > 0.0)
    {
      liftTransform = this.liftNormal;
      otherTransform = this.xform;
      return true;
    }
    liftTransform = this.liftNormal;
    otherTransform = (Transform) null;
    return false;
  }

  private Ptr<AeroPartFields> GetOrCreateJobField()
  {
    if (!this.JobFields.IsCreated)
    {
      JobsAllocator<AeroPartFields>.Allocate(ref this.JobFields);
      ref AeroPartFields local = ref this.JobFields.Ref();
      local.centerOfLift = this.centerOfLift;
      local.airfoilID = this.airfoilID;
      local.buoyancy = this.buoyancy;
      local.collisionSize = this.collisionSize;
      local.airflowChanneling = this.airflowChanneling;
      local.lastSplashTime = 0.0f;
    }
    return (Ptr<AeroPartFields>) this.JobFields;
  }

  public void UpdateJobFields()
  {
    ref AeroPartFields local = ref this.JobFields.Ref();
    local.velocity = this.rb.velocity;
    local.mass = this.mass;
    local.wingArea = this.wingArea;
    local.dragArea = this.dragArea;
    local.wingEffectiveness = this.wingEffectiveness;
    local.submergedAmount = this.submergedAmount;
    this.submergedAmount = 0.0f;
  }

  public void MergeWithParent()
  {
    if (this.attachInfo == null || this.attachInfo.detachedFromParentPart)
      return;
    this.xform.SetParent(this.attachInfo.parentPart.xform);
    this.xform.localPosition = this.attachInfo.localPosition;
    this.xform.localRotation = this.attachInfo.localRotation;
    for (int index = 0; index < this.joints.Length; ++index)
    {
      if ((UnityEngine.Object) this.joints[index].joint != (UnityEngine.Object) null)
        UnityEngine.Object.Destroy((UnityEngine.Object) this.joints[index].joint);
    }
    if ((UnityEngine.Object) this.rb != (UnityEngine.Object) this.parentUnit.rb)
      UnityEngine.Object.Destroy((UnityEngine.Object) this.rb);
    this.rb = this.parentUnit.rb;
    this.simplePhysics = true;
    if (!((UnityEngine.Object) this.debugArrow != (UnityEngine.Object) null))
      return;
    this.debugArrow.GetComponent<MeshRenderer>().material.color = Color.yellow;
  }

  public override void ApplyDamage(
    float netPierceDamage,
    float netBlastDamage,
    float netFireDamage,
    float netImpactDamage)
  {
    base.ApplyDamage(netPierceDamage, netBlastDamage, netFireDamage, netImpactDamage);
    this.wingEffectiveness = Mathf.Lerp(0.5f, 1f, this.hitPoints * 0.01f);
    if (!this.parentUnit.LocalSim)
      return;
    if (this.attachInfo != null)
      this.attachInfo.attachmentStrength = 0.0f;
    float num = Mathf.Max((float) (((double) this.hitPoints - (double) this.structuralThreshold) / (100.0 - (double) this.structuralThreshold)), 0.0f);
    foreach (PartJoint joint in this.joints)
    {
      if ((UnityEngine.Object) joint.joint != (UnityEngine.Object) null)
      {
        joint.joint.breakForce = (float) ((double) joint.breakForce * (double) num * 10.0);
        joint.joint.breakTorque = (float) ((double) joint.breakTorque * (double) num * 10.0);
        this.attachInfo.attachmentStrength += joint.breakForce + joint.breakTorque;
      }
    }
    if ((double) this.parentUnit.displayDetail <= 1.0 || (double) netPierceDamage <= 0.0)
      return;
    if ((UnityEngine.Object) this.hitSource == (UnityEngine.Object) null)
    {
      this.hitSource = this.gameObject.AddComponent<AudioSource>();
      this.hitSource.outputAudioMixerGroup = SoundManager.i.EffectsMixer;
      this.hitSource.bypassListenerEffects = true;
      this.hitSource.clip = this.hitSound;
      this.hitSource.pitch = UnityEngine.Random.Range(0.8f, 1.2f);
      this.hitSource.spatialBlend = 1f;
      this.hitSource.dopplerLevel = 0.0f;
      this.hitSource.spread = 5f;
      this.hitSource.maxDistance = 200f;
      this.hitSource.minDistance = 5f;
      this.hitSource.priority = 128 /*0x80*/;
    }
    this.hitSource.Play();
  }

  public override void SpawnFragments()
  {
    base.SpawnFragments();
    for (int index = 0; index < this.joints.Length; ++index)
    {
      if ((UnityEngine.Object) this.joints[index].joint != (UnityEngine.Object) null)
      {
        this.joints[index].joint.breakForce = 0.0f;
        this.joints[index].joint.breakTorque = 0.0f;
      }
    }
  }

  public void ApplyJobFields()
  {
    if (!this.JobFields.IsCreated)
      return;
    ref AeroPartFields local = ref this.JobFields.Ref();
    if (local.splashed)
    {
      Vector3 position = this.xform.position;
      bool flag1 = false;
      bool flag2 = false;
      RaycastHit hitInfo;
      if (Physics.Linecast(position + Vector3.up * 100f, position - Vector3.up * 10f, out hitInfo, 64 /*0x40*/))
      {
        flag2 = (UnityEngine.Object) hitInfo.collider.sharedMaterial == (UnityEngine.Object) GameAssets.i.WaterMaterial;
        flag1 = !flag2 && (double) hitInfo.point.y > (double) Datum.LocalSeaY;
      }
      if (!flag2)
        position.y = Datum.LocalSeaY;
      if (!flag1)
        UnityEngine.Object.Destroy((UnityEngine.Object) UnityEngine.Object.Instantiate<GameObject>(GameAssets.i.splash_large, position, Quaternion.LookRotation(Vector3.up + new Vector3(this.rb.velocity.x, 0.0f, this.rb.velocity.z) * 0.1f)), 20f);
    }
    if (local.angularDragChanged)
      this.rb.angularDrag = local.angularDrag;
    switch (local.hasForce)
    {
      case JobForceType.Force:
        this.rb.AddForce(local.force);
        break;
      case JobForceType.ForceAndTorque:
        this.rb.AddForce(local.force);
        this.rb.AddTorque(local.torque);
        break;
    }
  }

  public override void RemoveFromUnit()
  {
    base.RemoveFromUnit();
    if (!(this.parentUnit is Aircraft parentUnit))
      return;
    parentUnit.DeregisterAeroPart(this);
  }

  protected override void OnDestroy()
  {
    base.OnDestroy();
    JobManager.Remove(ref this.JobPart);
    AeroPart.DisposeJobFields(ref this.JobFields);
  }
}

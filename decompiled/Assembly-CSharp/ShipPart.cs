// Decompiled with JetBrains decompiler
// Type: ShipPart
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.Jobs;
using System;
using UnityEngine;

#nullable disable
public class ShipPart : UnitPart
{
  [SerializeField]
  private float displacement;
  [SerializeField]
  private float height;
  [SerializeField]
  private float leakThreshold;
  [SerializeField]
  private float leakRateMin;
  [SerializeField]
  private float leakRateMax;
  [SerializeField]
  private float sinkThreshold;
  [SerializeField]
  private float breakJointStrength = 1000f;
  [SerializeField]
  private Vector3 directionalDrag;
  [SerializeField]
  private Transform forceTransform;
  private float originalDisplacement;
  private float leakToDisplacement;
  [SerializeField]
  private GameObject sinkEffect;
  [SerializeField]
  private ShipPart[] connectedCompartments;
  [SerializeField]
  private bool compartmentalized;
  [SerializeField]
  private bool debugBreak;
  private bool submerged;
  private float surfaceArea;
  private float leakRate;
  private Ship parentShip;
  public PtrAllocation<ShipPartFields> JobFields;
  public NuclearOption.Jobs.JobPart<ShipPart, ShipPartFields> JobPart;
  private bool DamageControlActive;
  private float DamageControlStart;
  private float DamageControlDelay = 20f;

  public event Action<ShipPart> onDetachFromParent;

  protected override void Awake()
  {
    base.Awake();
    this.enabled = false;
    this.bounds = this.gameObject.GetComponent<Collider>().bounds;
    this.leakRate = 0.0f;
    this.surfaceArea = this.bounds.extents.x * this.bounds.extents.z;
    this.height = this.bounds.extents.y;
    this.rb = this.parentUnit.rb;
    this.originalDisplacement = this.displacement;
    this.leakToDisplacement = this.displacement;
    this.parentShip = this.parentUnit as Ship;
    this.parentShip.parts.Add(this);
    this.DamageControlActive = false;
    this.DamageControlDelay /= Mathf.Clamp(this.parentShip.skill, 0.1f, 1f);
    if ((UnityEngine.Object) this.forceTransform == (UnityEngine.Object) null)
      this.forceTransform = this.xform;
    if (!((UnityEngine.Object) this.xform.parent == (UnityEngine.Object) null))
      return;
    this.parentUnit.rb.mass = this.CalcMassWithChildren();
  }

  public void Flood()
  {
    this.leakToDisplacement = 0.0f;
    this.leakRate = this.leakRateMax;
    this.directionalDrag.z = this.directionalDrag.x;
  }

  private void Leak()
  {
    if ((double) this.transform.GlobalPosition().y - (double) this.height < (double) Datum.SeaLevel.y)
      this.displacement -= this.leakRate * Time.deltaTime;
    if (this.compartmentalized || (double) this.displacement >= (double) this.originalDisplacement * (double) this.parentShip.damageControlDeploymentThreshold)
      return;
    this.parentShip.damageControlAvailable -= this.originalDisplacement;
    this.compartmentalized = true;
    if ((double) this.parentShip.damageControlAvailable > 0.0)
      return;
    foreach (ShipPart connectedCompartment in this.connectedCompartments)
      connectedCompartment.Flood();
  }

  private void DamageControl()
  {
    if (this.detachedFromUnit || this.parentShip.disabled || this.submerged || (double) this.parentShip.damageControlAvailable <= 0.0 || this.compartmentalized)
      return;
    if (!this.DamageControlActive)
    {
      this.DamageControlActive = true;
      this.DamageControlStart = Time.timeSinceLevelLoad;
    }
    else
    {
      if (this.DamageControlActive && (double) Time.timeSinceLevelLoad < (double) this.DamageControlStart + (double) this.DamageControlDelay)
        return;
      float num1 = 0.0f;
      float num2 = 0.0f;
      float num3 = Mathf.Clamp(this.parentShip.skill, 0.1f, 1f);
      if ((double) this.leakRate < 0.10000000149011612)
      {
        this.leakRate = 0.0f;
      }
      else
      {
        num1 = 0.02f * this.leakRateMin * Time.deltaTime;
        this.leakRate -= num1 * num3;
        this.leakRate = Mathf.Max(this.leakRate, 0.0f);
      }
      if ((double) this.displacement > (double) this.originalDisplacement)
      {
        this.displacement = this.originalDisplacement;
      }
      else
      {
        num2 = 1f / 1000f * this.originalDisplacement * Time.deltaTime;
        this.displacement += num2 * num3;
        this.displacement = Mathf.Min(this.displacement, this.originalDisplacement);
      }
      this.parentShip.damageControlAvailable -= (10f * num1 + num2) * Time.deltaTime / num3;
      if ((double) this.parentShip.damageControlAvailable < 0.0)
        this.parentShip.damageControlAvailable = 0.0f;
      if ((double) this.leakRate >= 0.10000000149011612 || (double) this.displacement < (double) this.originalDisplacement)
        return;
      this.leakRate = 0.0f;
      this.displacement = this.originalDisplacement;
      this.leakToDisplacement = this.displacement;
      this.DamageControlActive = false;
    }
  }

  public NuclearOption.Jobs.JobPart<ShipPart, ShipPartFields> SetupJob()
  {
    ColorLog<ShipPart>.InfoAssert(this.JobPart == null, "should not have job part before detached");
    this.JobPart = new NuclearOption.Jobs.JobPart<ShipPart, ShipPartFields>(this, this.GetOrCreateJobField());
    return this.JobPart;
  }

  public Transform GetJobTransforms() => this.forceTransform;

  private static void DisposeJobFields(ref PtrAllocation<ShipPartFields> fields)
  {
    fields.Dispose();
  }

  private Ptr<ShipPartFields> GetOrCreateJobField()
  {
    if (!this.JobFields.IsCreated)
    {
      JobsAllocator<ShipPartFields>.Allocate(ref this.JobFields);
      ref ShipPartFields local = ref this.JobFields.Ref();
      local.directionalDrag = this.directionalDrag;
      local.partHeight = this.height;
    }
    return (Ptr<ShipPartFields>) this.JobFields;
  }

  ~ShipPart() => ShipPart.DisposeJobFields(ref this.JobFields);

  public void UpdateJobFields(ref JobTransformValues.ReadOnly transform)
  {
    ref ShipPartFields local = ref this.JobFields.Ref();
    local.velocity = this.rb.GetPointVelocity(transform.Position);
    local.displacement = this.displacement;
    local.mass = this.mass;
  }

  public void ApplyJobFields()
  {
    if (!this.JobFields.IsCreated)
      return;
    ref ShipPartFields local = ref this.JobFields.Ref();
    if ((double) local.submergedAmount > (double) this.sinkThreshold && !this.submerged && (double) this.displacement < (double) this.originalDisplacement * 0.5)
    {
      this.submerged = true;
      this.Flood();
      if (this.attachInfo != null && !this.attachInfo.detachedFromParentPart)
        (this.attachInfo.parentPart as ShipPart).Flood();
      UnityEngine.Object.Instantiate<GameObject>(this.sinkEffect, this.xform.position with
      {
        y = Datum.LocalSeaY
      }, Quaternion.LookRotation(Vector3.up)).transform.SetParent(this.xform);
    }
    if ((double) this.displacement > (double) this.leakToDisplacement)
      this.Leak();
    if ((double) this.displacement < (double) this.originalDisplacement)
      this.DamageControl();
    if (!this.detachedFromUnit)
      return;
    this.rb.angularDrag = Mathf.Clamp(local.submergedAmount * 2f, 0.1f, 2f);
    this.rb.AddForce(local.force);
  }

  protected override void UnitPart_OnParentDetached(UnitPart parentPart)
  {
    this.rb = parentPart.rb;
    if (this.parentShip.remoteSim && !this.detachedFromUnit)
      JobManager.Add(this.SetupJob());
    base.UnitPart_OnParentDetached((UnitPart) this);
  }

  public override void Detach(Vector3 velocity, Vector3 relativePos)
  {
    Rigidbody rb = this.attachInfo.parentPart.rb;
    this.parentShip.damageControlAvailable -= this.originalDisplacement;
    this.xform.SetParent((Transform) null);
    this.rb = this.gameObject.AddComponent<Rigidbody>();
    this.rb.mass = this.CalcMassWithChildren();
    this.attachInfo.parentPart.rb.mass -= this.rb.mass;
    this.rb.interpolation = RigidbodyInterpolation.Interpolate;
    this.rb.velocity = velocity;
    this.rb.angularVelocity = this.attachInfo.parentPart.rb.angularVelocity;
    this.rb.maxLinearVelocity = 60f;
    if ((double) this.breakJointStrength > 0.0)
    {
      ConfigurableJoint configurableJoint = this.gameObject.AddComponent<ConfigurableJoint>();
      configurableJoint.connectedBody = rb;
      configurableJoint.xMotion = ConfigurableJointMotion.Locked;
      configurableJoint.yMotion = ConfigurableJointMotion.Locked;
      configurableJoint.zMotion = ConfigurableJointMotion.Locked;
      configurableJoint.angularXMotion = ConfigurableJointMotion.Limited;
      configurableJoint.angularYMotion = ConfigurableJointMotion.Limited;
      configurableJoint.angularZMotion = ConfigurableJointMotion.Limited;
      SoftJointLimit softJointLimit = new SoftJointLimit();
      softJointLimit.limit = 20f;
      configurableJoint.highAngularXLimit = softJointLimit;
      configurableJoint.lowAngularXLimit = softJointLimit;
      configurableJoint.angularYLimit = softJointLimit;
      configurableJoint.angularZLimit = softJointLimit;
      configurableJoint.anchor = (this.xform.InverseTransformPoint(this.attachInfo.parentPart.xform.position) + this.attachInfo.localPosition) / 2f;
      configurableJoint.breakForce = this.rb.mass * this.breakJointStrength;
    }
    this.displacement *= 0.0f;
    this.Flood();
    ShipPart parentPart = this.attachInfo.parentPart as ShipPart;
    parentPart.displacement *= 0.5f;
    parentPart.Flood();
    foreach (ShipPart connectedCompartment in this.connectedCompartments)
    {
      if (!((UnityEngine.Object) connectedCompartment == (UnityEngine.Object) parentPart))
      {
        connectedCompartment.displacement *= 0.5f;
        connectedCompartment.Flood();
      }
    }
    base.Detach(Vector3.zero, Vector3.zero);
  }

  public override void ApplyDamage(
    float netPierceDamage,
    float netBlastDamage,
    float netFireDamage,
    float netImpactDamage)
  {
    this.hitPoints -= netPierceDamage + netBlastDamage + netFireDamage + netImpactDamage;
    float t = Mathf.Clamp01((float) (((double) this.hitPoints - (double) this.structuralThreshold) / ((double) this.leakThreshold - (double) this.structuralThreshold)));
    this.leakRate = Mathf.Lerp(this.leakRateMax, this.leakRateMin, t);
    this.leakToDisplacement = Mathf.Clamp(this.originalDisplacement * this.hitPoints / this.leakThreshold, 0.0f, this.originalDisplacement);
    this.directionalDrag.z = Mathf.Lerp(this.directionalDrag.z, this.directionalDrag.x, Mathf.Pow(1f - t, 2f));
    for (int index1 = 0; index1 < this.damageMaterial.renderers.Count; ++index1)
    {
      if (this.damageMaterial.indices.Length != 0)
      {
        for (int index2 = 0; index2 < this.damageMaterial.indices.Length; ++index2)
          this.damageMaterial.renderers[index1].materials[index2].SetFloat("_HitPoints", this.hitPoints);
      }
      else if ((UnityEngine.Object) this.damageMaterial.renderers[index1] != (UnityEngine.Object) null)
        this.damageMaterial.renderers[index1].material.SetFloat("_HitPoints", this.hitPoints);
    }
    for (int index = this.damageEffects.Count - 1; index >= 0; --index)
    {
      DamageEffect damageEffect = this.damageEffects[index];
      if ((double) this.hitPoints < (double) damageEffect.threshold)
      {
        GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(damageEffect.prefab, this.xform.position, this.xform.rotation);
        gameObject.transform.SetParent((UnityEngine.Object) gameObject.GetComponent<DamageParticles>() != (UnityEngine.Object) null ? this.xform : Datum.origin);
        this.damageEffects.RemoveAt(index);
      }
    }
    if ((double) this.hitPoints < (double) this.integrityThreshold)
      this.SpawnFragments();
    this.InvokeDamage(netPierceDamage, netBlastDamage, netFireDamage, netImpactDamage);
  }

  protected override void OnDestroy()
  {
    base.OnDestroy();
    JobManager.Remove(ref this.JobPart);
    ShipPart.DisposeJobFields(ref this.JobFields);
  }

  public bool IsCriticallyDamaged()
  {
    return this.submerged || (double) this.hitPoints <= 0.0 || this.IsDetached();
  }

  public float GetDisplacement() => this.displacement;

  public float GetOriginalDisplacement() => this.originalDisplacement;
}

// Decompiled with JetBrains decompiler
// Type: UnitPart
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.SavedMission;
using System;
using System.Collections.Generic;
using UnityEngine;

#nullable disable
public class UnitPart : MonoBehaviour, IDamageable
{
  [HideInInspector]
  public byte id;
  [Header("Structure")]
  [SerializeField]
  protected bool criticalPart;
  public Unit parentUnit;
  public float mass;
  protected Vector3 collisionSize;
  protected Bounds bounds;
  public Rigidbody rb;
  private Vector3 velocity;
  protected bool fragmented;
  protected bool detachedFromUnit;
  [SerializeField]
  protected Transform centerOfMass;
  [Header("Damage")]
  public float hitPoints = 100f;
  [SerializeField]
  protected ArmorProperties armorProperties;
  [SerializeField]
  protected float structuralThreshold;
  [SerializeField]
  protected float integrityThreshold = float.MinValue;
  [SerializeField]
  protected AudioClip hitSound;
  protected AudioSource hitSource;
  [SerializeField]
  protected ImpactDamage impactDamage;
  [SerializeField]
  protected DamageMaterial damageMaterial;
  [SerializeField]
  protected List<DamageEffect> damageEffects = new List<DamageEffect>();
  protected List<DamageParticles> hostedParticles = new List<DamageParticles>();
  [SerializeField]
  protected GameObject[] disintegrationEffects;
  [SerializeField]
  protected GameObject[] disintegrateObjects;
  protected UnitPart.AttachInfo attachInfo;
  protected float baseMass;
  protected float baseDrag;
  protected float weaponDrag;
  protected float weaponMass;
  protected float fuelMass;

  public Transform xform { get; private set; }

  public bool IsDetached() => this.detachedFromUnit;

  public event Action<UnitPart> onParentDetached;

  public event Action<UnitPart> onPartDetached;

  public event Action<Rigidbody> onParentRBChanged;

  public event Action<UnitPart> onJointBroken;

  public event Action<UnitPart.OnApplyDamage> onApplyDamage;

  protected virtual void Awake()
  {
    this.xform = this.transform;
    UnitPart component1 = (UnityEngine.Object) this.xform.parent == (UnityEngine.Object) null ? (UnitPart) null : this.xform.parent.GetComponent<UnitPart>();
    if ((UnityEngine.Object) this.parentUnit == (UnityEngine.Object) null && (UnityEngine.Object) this.xform.parent != (UnityEngine.Object) null)
    {
      component1 = this.xform.parent.parent.GetComponent<UnitPart>();
      this.parentUnit = component1.parentUnit;
    }
    this.rb = this.parentUnit.rb;
    this.id = this.parentUnit.RegisterDamageable((IDamageable) this);
    this.parentUnit.partLookup.Add(this);
    this.CreateAttachInfo(component1);
    if ((double) this.baseMass == 0.0)
      this.baseMass = this.mass;
    this.hitPoints = 100f;
    if (this.damageMaterial != null)
    {
      Renderer component2 = this.GetComponent<Renderer>();
      if (this.damageMaterial.renderers.Count == 0 && (UnityEngine.Object) component2 != (UnityEngine.Object) null)
        this.damageMaterial.renderers.Add(component2);
    }
    Collider component3 = this.GetComponent<Collider>();
    if (!((UnityEngine.Object) component3 != (UnityEngine.Object) null))
      return;
    this.collisionSize = component3.bounds.extents;
  }

  public void CreateAttachInfo(UnitPart parentPart)
  {
    if (!((UnityEngine.Object) parentPart != (UnityEngine.Object) null))
      return;
    this.attachInfo = new UnitPart.AttachInfo(parentPart, false, parentPart.transform.InverseTransformPoint(this.xform.position), this.xform.localRotation);
    this.attachInfo.parentPart.onParentDetached += new Action<UnitPart>(this.UnitPart_OnParentDetached);
  }

  public virtual void RemoveFromUnit()
  {
    if ((UnityEngine.Object) this.parentUnit == (UnityEngine.Object) null)
      return;
    this.parentUnit.DeregisterDamageable((int) this.id);
    this.parentUnit.partLookup.Remove(this);
  }

  protected virtual void OnDestroy()
  {
    this.onParentDetached = (Action<UnitPart>) null;
    this.onPartDetached = (Action<UnitPart>) null;
    this.onJointBroken = (Action<UnitPart>) null;
    this.onParentRBChanged = (Action<Rigidbody>) null;
    this.onApplyDamage = (Action<UnitPart.OnApplyDamage>) null;
  }

  public Transform GetTransform() => this.xform;

  public float GetMass() => this.mass;

  public virtual void TakeShockwave(Vector3 origin, float overpressure, float blastPower)
  {
    if ((UnityEngine.Object) this.rb == (UnityEngine.Object) null)
      return;
    double num = (double) Mathf.Pow(this.rb.mass * (1f / 1000f), 0.33f);
    float a = (float) ((double) Mathf.Min((float) (num * num), (float) ((double) blastPower * (double) blastPower * 1.0)) * (double) overpressure * 0.029999999329447746);
    this.rb.AddForce(FastMath.NormalizedDirection(origin, this.xform.position) * Mathf.Min(a, 60f * this.rb.mass), ForceMode.Impulse);
  }

  protected virtual void UnitPart_OnParentDetached(UnitPart parentPart)
  {
    this.detachedFromUnit = true;
    Action<UnitPart> onParentDetached = this.onParentDetached;
    if (onParentDetached == null)
      return;
    onParentDetached(this);
  }

  protected void UnitPart_OnParentRBChanged(Rigidbody rb) => this.rb = rb;

  private void OnJointBreak(float breakForce)
  {
    Action<UnitPart> onJointBroken = this.onJointBroken;
    if (onJointBroken == null)
      return;
    onJointBroken(this);
  }

  public void SetLivery(LiveryData livery, MaterialCleanup materialCleanup)
  {
    for (int index = 0; index < this.damageMaterial.renderers.Count; ++index)
    {
      Material material = this.damageMaterial.renderers[index].material;
      materialCleanup.Add(material);
      material.SetTexture("_Livery", (Texture) livery.Texture);
      material.SetFloat("_Glossiness", livery.Glossiness);
    }
  }

  public ArmorProperties GetArmorProperties() => this.armorProperties;

  public float GetStructuralThreshold() => this.structuralThreshold;

  public float CalcMassWithChildren()
  {
    float num = 0.0f;
    foreach (UnitPart componentsInChild in this.gameObject.GetComponentsInChildren<UnitPart>())
      num += componentsInChild.mass;
    return num;
  }

  public void ModifyMass(float amount)
  {
    this.mass += amount;
    this.rb.mass = this.mass;
    if (!((UnityEngine.Object) this.centerOfMass != (UnityEngine.Object) null) || !this.parentUnit.LocalSim)
      return;
    this.rb.centerOfMass = this.centerOfMass.localPosition;
  }

  public virtual void ModifyDrag(float amount)
  {
  }

  public virtual void Repair() => this.hitPoints = 100f;

  public void AddHostedParticles(DamageParticles damageParticles)
  {
    this.hostedParticles.Add(damageParticles);
  }

  public void TakeDamage(
    float pierceDamage,
    float blastDamage,
    float amountAffected,
    float fireDamage,
    float impactDamage,
    PersistentID dealerID)
  {
    if (this.parentUnit.SavedUnit is SavedScenery savedUnit && savedUnit.indestructible)
      return;
    if (!this.parentUnit.IsServer)
    {
      Debug.LogWarning((object) $"TakeDamage called on {this} but it is not spawned on server");
    }
    else
    {
      float pierceDamage1 = Mathf.Max(pierceDamage - this.armorProperties.pierceArmor, 0.0f) / Mathf.Max(this.armorProperties.pierceTolerance, 0.01f);
      float blastDamage1 = blastDamage * amountAffected / Mathf.Max(this.armorProperties.blastTolerance, 0.01f);
      float fireDamage1 = Mathf.Max(fireDamage - this.armorProperties.fireArmor, 0.0f) / Mathf.Max(this.armorProperties.fireTolerance, 0.01f);
      float damageAmount = pierceDamage1 + blastDamage1 + fireDamage1 + impactDamage;
      if ((UnityEngine.Object) this.parentUnit == (UnityEngine.Object) null || (double) damageAmount <= 0.0)
        return;
      if (dealerID.IsValid && dealerID != this.parentUnit.persistentID)
        this.parentUnit.RecordDamage(dealerID, damageAmount);
      if (this.criticalPart && (double) this.hitPoints - (double) damageAmount <= 0.0 && !this.parentUnit.disabled)
      {
        this.parentUnit.Networkdisabled = true;
        if (!(this.parentUnit is Scenery))
          this.parentUnit.ReportKilled();
      }
      this.parentUnit.RpcDamage(this.id, new DamageInfo(pierceDamage1, blastDamage1, fireDamage1, impactDamage));
      if (this.attachInfo == null || this.attachInfo.detachedFromParentPart || (double) this.hitPoints - (double) damageAmount >= (double) this.structuralThreshold || this is AeroPart)
        return;
      this.attachInfo.detachedFromParentPart = true;
      this.parentUnit.DetachPart(this.id, (UnityEngine.Object) this.rb != (UnityEngine.Object) null ? this.rb.GetPointVelocity(this.xform.position) : Vector3.zero, this.xform.position - this.attachInfo.parentPart.transform.position);
    }
  }

  public void InvokeDamage(
    float netPierceDamage,
    float netBlastDamage,
    float netFireDamage,
    float netImpactDamage)
  {
    UnitPart.OnApplyDamage onApplyDamage1 = new UnitPart.OnApplyDamage()
    {
      detached = this.detachedFromUnit,
      hitPoints = this.hitPoints,
      pierceDamage = netPierceDamage,
      blastDamage = netBlastDamage,
      fireDamage = netFireDamage,
      impactDamage = netImpactDamage
    };
    Action<UnitPart.OnApplyDamage> onApplyDamage2 = this.onApplyDamage;
    if (onApplyDamage2 == null)
      return;
    onApplyDamage2(onApplyDamage1);
  }

  public virtual void ApplyDamage(
    float netPierceDamage,
    float netBlastDamage,
    float netFireDamage,
    float netImpactDamage)
  {
    this.hitPoints -= netPierceDamage + netBlastDamage + netFireDamage + netImpactDamage;
    for (int index = 0; index < this.damageMaterial.renderers.Count; ++index)
    {
      if (!((UnityEngine.Object) this.damageMaterial.renderers[index] == (UnityEngine.Object) null))
        this.damageMaterial.renderers[index].material.SetFloat("_HitPoints", this.hitPoints);
    }
    this.InvokeDamage(netPierceDamage, netBlastDamage, netFireDamage, netImpactDamage);
    for (int index = this.damageEffects.Count - 1; index >= 0; --index)
    {
      DamageEffect damageEffect = this.damageEffects[index];
      if ((double) this.hitPoints < (double) damageEffect.threshold)
      {
        GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(damageEffect.prefab, this.xform);
        DamageParticles component;
        if (gameObject.TryGetComponent<DamageParticles>(out component))
          this.parentUnit.spawnedEffects.Add(component);
        else
          gameObject.transform.SetParent(Datum.origin);
        this.damageEffects.RemoveAt(index);
      }
    }
    if ((double) this.hitPoints >= (double) this.integrityThreshold)
      return;
    this.SpawnFragments();
  }

  public virtual void Detach(Vector3 velocity, Vector3 relativePos)
  {
    if (this.attachInfo == null)
    {
      Debug.LogError((object) $"attachInfo null for part {this.name}, id:{this.id}");
    }
    else
    {
      this.attachInfo.parentPart.onParentDetached -= new Action<UnitPart>(this.UnitPart_OnParentDetached);
      this.attachInfo.detachedFromParentPart = true;
      this.detachedFromUnit = true;
      Action<UnitPart> onParentDetached = this.onParentDetached;
      if (onParentDetached != null)
        onParentDetached(this);
      Action<UnitPart> onPartDetached = this.onPartDetached;
      if (onPartDetached == null)
        return;
      onPartDetached(this);
    }
  }

  public void RemovePart() => UnityEngine.Object.Destroy((UnityEngine.Object) this.gameObject);

  public void DetachDamageParticles()
  {
    foreach (DamageParticles hostedParticle in this.hostedParticles)
    {
      if ((UnityEngine.Object) hostedParticle != (UnityEngine.Object) null)
        hostedParticle.ParentObjectCulled();
    }
  }

  public virtual void SpawnFragments()
  {
    Action<UnitPart> onJointBroken = this.onJointBroken;
    if (onJointBroken != null)
      onJointBroken(this);
    if (this.fragmented)
      return;
    foreach (GameObject disintegrateObject in this.disintegrateObjects)
    {
      Renderer component;
      if (disintegrateObject.TryGetComponent<Renderer>(out component))
        component.enabled = false;
      disintegrateObject.layer = 15;
    }
    if ((double) this.xform.position.y > (double) Datum.LocalSeaY)
    {
      foreach (GameObject disintegrationEffect in this.disintegrationEffects)
      {
        GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(disintegrationEffect, this.xform);
        DamageParticles component;
        if (gameObject.TryGetComponent<DamageParticles>(out component))
          this.parentUnit.spawnedEffects.Add(component);
        else
          gameObject.transform.SetParent(Datum.origin);
      }
    }
    else
      UnityEngine.Object.Instantiate<GameObject>(GameAssets.i.splash_large, this.xform.position with
      {
        y = Datum.LocalSeaY
      }, Quaternion.LookRotation(Vector3.up));
    this.fragmented = true;
  }

  public Unit GetUnit() => this.parentUnit;

  protected void ImpactDrag()
  {
  }

  protected class AttachInfo
  {
    public UnitPart parentPart;
    public float attachmentStrength;
    public bool detachedFromParentPart;
    public Vector3 localPosition;
    public Quaternion localRotation;

    public AttachInfo(
      UnitPart parentPart,
      bool detached,
      Vector3 localPosition,
      Quaternion localRotation)
    {
      this.parentPart = parentPart;
      this.detachedFromParentPart = detached;
      this.localPosition = localPosition;
      this.localRotation = localRotation;
    }
  }

  public struct OnApplyDamage
  {
    public bool detached;
    public float hitPoints;
    public float pierceDamage;
    public float blastDamage;
    public float fireDamage;
    public float impactDamage;
  }

  public struct OnCollision
  {
    public float force;
    public float g;
  }
}

// Decompiled with JetBrains decompiler
// Type: FuelTank
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

#nullable disable
public class FuelTank : MonoBehaviour
{
  [SerializeField]
  private float fuelCapacity;
  public float fuelMass;
  [SerializeField]
  private FuelTank[] connectedTanks;
  [SerializeField]
  private float leakThreshold;
  [SerializeField]
  private float leakPerHP;
  [SerializeField]
  private float maxLeakRate;
  [SerializeField]
  private float ruptureGMin = 20f;
  [SerializeField]
  private float ruptureGMax = 100f;
  [SerializeField]
  private float ignitionGMin = 20f;
  [SerializeField]
  private float ignitionGMax = 200f;
  [SerializeField]
  private float ignitionPierceMin;
  [SerializeField]
  private float ignitionPierceMax;
  [SerializeField]
  private float ignitionBlastMin;
  [SerializeField]
  private float ignitionBlastMax;
  [SerializeField]
  private float fireIntensity;
  [SerializeField]
  private GameObject leakEffect;
  [SerializeField]
  private GameObject fireEffect;
  [SerializeField]
  private GameObject fireball;
  private GameObject fireEffectSpawn;
  private GameObject fireballSpawn;
  private DamageParticles fireParticles;
  private ParticleSystem leakSystem;
  private float leakRate;
  private float lastCollision;
  private bool isLeaking;
  private Vector3 velocityPrev;
  [SerializeField]
  private UnitPart part;
  private Aircraft aircraft;
  private bool onFire;
  private bool ruptured;
  private static readonly Collider[] fireColliders = new Collider[100];

  private void Awake()
  {
    this.fuelMass = 0.0f;
    if ((UnityEngine.Object) this.part == (UnityEngine.Object) null)
      this.part = this.gameObject.GetComponent<UnitPart>();
    this.aircraft = this.part.parentUnit as Aircraft;
    this.aircraft.RegisterFuelTank(this);
    this.part.onApplyDamage += new Action<UnitPart.OnApplyDamage>(this.FuelTank_OnPartApplyDamage);
    this.part.onPartDetached += new Action<UnitPart>(this.FuelTank_OnDetach);
    this.aircraft.onInitialize += new Action(this.FuelTank_OnInitialize);
  }

  private void FuelTank_OnInitialize()
  {
    this.aircraft.onInitialize -= new Action(this.FuelTank_OnInitialize);
    this.enabled = this.aircraft.LocalSim;
  }

  public float GetCapacity() => this.fuelCapacity;

  public float GetLevel() => this.fuelMass;

  private void OnCollisionEnter(Collision collision)
  {
    if (!((UnityEngine.Object) collision.body == (UnityEngine.Object) null))
      return;
    this.lastCollision = Time.timeSinceLevelLoad;
  }

  public bool Refuel(float ratio)
  {
    if ((double) this.leakRate > 0.0)
      return false;
    float fuelMass = this.fuelMass;
    this.fuelMass = this.fuelCapacity * ratio;
    this.part.ModifyMass(this.fuelMass - fuelMass);
    return true;
  }

  private void FuelTank_OnDetach(UnitPart part)
  {
    part.onPartDetached -= new Action<UnitPart>(this.FuelTank_OnDetach);
    if (this.aircraft.remoteSim)
      return;
    if ((double) this.leakRate < (double) this.maxLeakRate)
    {
      this.ruptured = true;
      this.PunctureTank(this.maxLeakRate);
      this.aircraft.FuelTankStatus(part.id, true, this.onFire);
    }
    foreach (FuelTank connectedTank in this.connectedTanks)
      connectedTank.PunctureTank(this.maxLeakRate);
  }

  private void FuelTank_OnPartApplyDamage(UnitPart.OnApplyDamage e)
  {
    if (e.detached)
      this.leakRate = this.maxLeakRate;
    else if ((double) e.hitPoints < (double) this.leakThreshold)
      this.PunctureTank((this.leakThreshold - e.hitPoints) * this.leakPerHP);
    if (!this.aircraft.LocalSim)
      return;
    this.CheckDamageStatus(e);
  }

  private void CheckDamageStatus(UnitPart.OnApplyDamage e)
  {
    bool ruptured = false;
    bool onFire = false;
    if ((double) this.leakRate < (double) this.maxLeakRate && e.detached)
    {
      this.leakRate = this.maxLeakRate;
      ruptured = true;
    }
    float num1 = 1f / this.leakRate;
    float num2 = this.leakRate / this.maxLeakRate;
    if ((double) this.leakRate > 0.0 && !this.onFire && ((double) e.fireDamage > (double) num1 || (double) e.pierceDamage * (double) num2 > (double) UnityEngine.Random.Range(this.ignitionPierceMin, this.ignitionPierceMax) || (double) e.blastDamage * (double) num2 > (double) UnityEngine.Random.Range(this.ignitionBlastMin, this.ignitionBlastMax)))
      onFire = true;
    if (!(ruptured | onFire))
      return;
    this.aircraft.FuelTankStatus(this.part.id, ruptured, onFire);
  }

  public void UpdateStatus(bool ruptured, bool onFire)
  {
    if (!this.ruptured & ruptured)
    {
      this.ruptured = true;
      this.RuptureEffects();
    }
    if (!(!this.onFire & onFire))
      return;
    if ((bool) (UnityEngine.Object) this.leakEffect)
      this.onFire = true;
    if (this.aircraft.IsServer)
      this.FuelTankFire().Forget();
    this.IgnitionEffects();
  }

  public void UseFuel(float rate)
  {
    float fuelMass = this.fuelMass;
    this.fuelMass -= rate;
    this.fuelMass = Mathf.Max(this.fuelMass, 0.0f);
    float amount = this.fuelMass - fuelMass;
    if (!this.aircraft.LocalSim)
      return;
    this.part.ModifyMass(amount);
  }

  private void IgnitionEffects()
  {
    if ((double) this.transform.position.y <= (double) Datum.LocalSeaY || !((UnityEngine.Object) this.fireEffectSpawn == (UnityEngine.Object) null))
      return;
    this.fireEffectSpawn = UnityEngine.Object.Instantiate<GameObject>(this.fireEffect, this.transform);
    this.fireEffectSpawn.transform.localPosition = Vector3.zero;
    this.fireParticles = this.fireEffectSpawn.GetComponent<DamageParticles>();
    this.part.AddHostedParticles(this.fireParticles);
    if ((double) this.leakRate == (double) this.maxLeakRate)
      this.Fireball();
    if (!((UnityEngine.Object) this.leakSystem != (UnityEngine.Object) null))
      return;
    this.leakSystem.Stop();
    UnityEngine.Object.Destroy((UnityEngine.Object) this.leakSystem.gameObject, 5f);
  }

  private void RuptureEffects()
  {
    this.leakRate = this.maxLeakRate;
    if (!this.onFire)
      return;
    this.Fireball();
  }

  private void Fireball()
  {
    if (!((UnityEngine.Object) this.fireballSpawn == (UnityEngine.Object) null) || !((UnityEngine.Object) this.fireball != (UnityEngine.Object) null) || (double) this.transform.position.y <= (double) Datum.LocalSeaY)
      return;
    this.fireballSpawn = UnityEngine.Object.Instantiate<GameObject>(this.fireball, this.transform);
    this.part.AddHostedParticles(this.fireballSpawn.GetComponent<DamageParticles>());
    this.fireballSpawn.transform.localPosition = Vector3.zero;
  }

  public void PunctureTank(float leakRate)
  {
    if (!this.isLeaking)
    {
      this.isLeaking = true;
      this.leakSystem = UnityEngine.Object.Instantiate<GameObject>(this.leakEffect, this.transform).GetComponent<ParticleSystem>();
    }
    this.leakRate = Mathf.Clamp(leakRate, this.leakRate, this.maxLeakRate);
  }

  private void FixedUpdate()
  {
    Vector3 velocity = this.part.rb.velocity;
    if ((double) Time.timeSinceLevelLoad - (double) this.lastCollision < 0.25 && !FastMath.InRange(this.velocityPrev, velocity, Mathf.Min(this.ruptureGMin, this.ignitionGMin) * 9.81f * Time.fixedDeltaTime) && (double) this.part.xform.position.y > (double) Datum.LocalSeaY && this.velocityPrev != Vector3.zero)
    {
      double num = (double) (velocity - this.velocityPrev).magnitude / (9.8100004196167 * (double) Time.fixedDeltaTime);
      bool ruptured = false;
      bool onFire = false;
      if (num > (double) UnityEngine.Random.Range(this.ruptureGMin, this.ruptureGMax))
      {
        this.leakRate = this.maxLeakRate;
        ruptured = true;
      }
      if (num > (double) UnityEngine.Random.Range(this.ignitionGMin, this.ignitionGMax))
        onFire = true;
      if (ruptured | onFire)
        this.aircraft.FuelTankStatus(this.part.id, ruptured, onFire);
    }
    if ((double) this.leakRate > 0.0)
      this.LeakFuel();
    this.velocityPrev = velocity;
  }

  private void LeakFuel()
  {
    float fuelMass = this.fuelMass;
    this.fuelMass -= this.leakRate * Time.deltaTime;
    if ((double) this.fuelMass <= 0.0)
    {
      this.fuelMass = 0.0f;
      this.leakRate = 0.0f;
      if ((UnityEngine.Object) this.leakSystem != (UnityEngine.Object) null)
      {
        this.leakSystem.Stop();
        UnityEngine.Object.Destroy((UnityEngine.Object) this.leakSystem.gameObject, 5f);
        this.leakSystem = (ParticleSystem) null;
      }
    }
    this.fuelMass = Mathf.Max(this.fuelMass, 0.0f);
    if (!this.aircraft.LocalSim)
      return;
    this.part.ModifyMass(this.fuelMass - fuelMass);
  }

  private async UniTask FuelTankFire()
  {
    FuelTank fuelTank = this;
    CancellationToken cancel = fuelTank.destroyCancellationToken;
    while (!cancel.IsCancellationRequested)
    {
      int num = Physics.OverlapSphereNonAlloc(fuelTank.transform.position, fuelTank.fireIntensity * 3f, FuelTank.fireColliders);
      if ((double) fuelTank.transform.position.y < (double) Datum.LocalSeaY && (UnityEngine.Object) fuelTank.fireParticles != (UnityEngine.Object) null)
        fuelTank.fireParticles.ParentObjectCulled();
      for (int index = 0; index < num; ++index)
      {
        IDamageable component;
        if (FuelTank.fireColliders[index].TryGetComponent<IDamageable>(out component))
          component.TakeDamage(0.0f, 0.0f, 1f, (float) ((double) fuelTank.fireIntensity * (double) fuelTank.leakRate * 0.05000000074505806), 0.0f, PersistentID.None);
      }
      await UniTask.Delay(1000);
    }
    cancel = new CancellationToken();
  }
}

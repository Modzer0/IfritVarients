// Decompiled with JetBrains decompiler
// Type: Gun
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Cysharp.Threading.Tasks;
using NuclearOption.Networking;
using System;
using System.Threading;
using UnityEngine;

#nullable disable
public class Gun : Weapon
{
  [SerializeField]
  [ColorUsage(false, true)]
  private Color tracerColor = Color.red;
  [SerializeField]
  private MissileDefinition guidedProjectile;
  [SerializeField]
  private int tracerRatio = 6;
  [SerializeField]
  private float tracerSize = 1f;
  [SerializeField]
  private float bulletSelfDestruct = 5f;
  [SerializeField]
  private float bulletSpread = 0.1f;
  [SerializeField]
  private float fireRate;
  [SerializeField]
  private float reloadTime;
  [SerializeField]
  private int magazineCapacity;
  [SerializeField]
  private int magazines;
  [SerializeField]
  private bool proximityTimer;
  [SerializeField]
  private bool startLoaded = true;
  public bool ForceServerAuthority;
  private Unit proximityFuseTarget;
  private BulletSim bulletSim;
  private float queuedBullets;
  private float fireInterval;
  private int bulletsLoaded;
  private int tracerSeed;
  private int maxMagazines;
  private bool reloading;
  private float muzzleVelocity;
  private float timeUntilReload;
  private int ticksSinceTriggerPull;
  public Rigidbody velocityInherit;
  [SerializeField]
  private Transform[] muzzles;
  [SerializeField]
  private AudioClip[] fireSounds;
  [SerializeField]
  private AudioClip fireStart;
  [SerializeField]
  private AudioClip fireSustained;
  [SerializeField]
  private AudioClip fireEnd;
  private AudioSource[] sources;
  [SerializeField]
  private float volume = 1f;
  [SerializeField]
  private float pitch = 1f;
  [SerializeField]
  private bool modifyPitch;
  [SerializeField]
  private float startPitchMultiplier = 1f;
  [SerializeField]
  private float pitchClimbRate = 1f;
  private float startPitch;
  [SerializeField]
  private AudioSource recoilSound;
  [SerializeField]
  private AudioSource reloadSound;
  [SerializeField]
  private Transform recoilTransform;
  [SerializeField]
  private Transform spinTransform;
  [SerializeField]
  private Transform ejectionTransform;
  [SerializeField]
  private GameObject ejectionPrefab;
  [SerializeField]
  private Vector3 ejectionLinearVelocity;
  [SerializeField]
  private Vector3 ejectionRotationalVelocity;
  [SerializeField]
  private float ejectionLifetime;
  [SerializeField]
  private float recoilImpulse;
  [SerializeField]
  private float recoilTravel;
  [SerializeField]
  private float recoilRate;
  [SerializeField]
  private float recoilReturnRate;
  [SerializeField]
  private float spinRate;
  private float recoilEnergy;
  private float recoilPosition;
  private float currentSpinRate;
  [SerializeField]
  private ParticleSystem[] muzzleParticles;
  [SerializeField]
  private GameObject impactEffectGround;
  [SerializeField]
  private GameObject impactEffectArmor;
  [SerializeField]
  private GameObject impactEffectWater;
  [SerializeField]
  private GameObject selfDestructEffect;
  [SerializeField]
  private bool heatEnabled;
  [SerializeField]
  private Gun.Heat heat;

  public GameObject[] ImpactEffectsPrefabs { get; private set; }

  private void Awake()
  {
    if (this.startLoaded)
    {
      this.bulletsLoaded = this.magazineCapacity;
      this.ammo = this.bulletsLoaded + this.magazines * this.magazineCapacity;
    }
    this.maxMagazines = this.magazines;
    this.tracerSeed = UnityEngine.Random.Range(0, 6);
    this.enabled = false;
    this.fireInterval = 60f / this.fireRate;
    this.muzzleVelocity = this.info.muzzleVelocity;
    this.ImpactEffectsPrefabs = new GameObject[4];
    this.ImpactEffectsPrefabs[0] = this.impactEffectGround;
    this.ImpactEffectsPrefabs[1] = this.impactEffectArmor;
    this.ImpactEffectsPrefabs[2] = this.impactEffectWater;
    this.ImpactEffectsPrefabs[3] = this.selfDestructEffect;
    this.sources = this.gameObject.GetComponents<AudioSource>();
    foreach (AudioSource source in this.sources)
    {
      source.volume = this.volume;
      source.pitch = this.pitch;
    }
    if (this.sources.Length > 1)
    {
      this.sources[1].loop = true;
      this.sources[1].clip = this.fireSustained;
      if (this.modifyPitch)
      {
        this.startPitch = this.pitch * this.startPitchMultiplier;
        this.sources[1].pitch = this.startPitch;
      }
    }
    if (!this.heatEnabled)
      return;
    this.heat.Initialize(this);
  }

  public override void AttachToUnit(Unit unit)
  {
    base.AttachToUnit(unit);
    this.velocityInherit = unit.rb;
  }

  public override void AttachToHardpoint(Aircraft aircraft, Hardpoint hardpoint, WeaponMount mount)
  {
    base.AttachToHardpoint(aircraft, hardpoint, mount);
    if ((UnityEngine.Object) mount != (UnityEngine.Object) null && mount.GunAmmo)
      this.LoadAmmunition(mount);
    hardpoint.ModifyMass((float) this.ammo * this.info.massPerRound);
    this.attachedUnit = hardpoint.part.parentUnit;
    foreach (AudioSource source in this.sources)
      aircraft.RegisterDopplerSound(source);
  }

  public void LoadAmmunition(WeaponMount weaponMount)
  {
    if ((UnityEngine.Object) weaponMount == (UnityEngine.Object) null)
    {
      this.magazines = 0;
      if (this.hardpoint != null)
        this.hardpoint.ModifyMass((float) -this.ammo * this.info.massPerRound);
      this.ammo = 0;
    }
    else
    {
      this.info = weaponMount.info;
      this.magazines = this.maxMagazines;
      this.bulletsLoaded = this.magazineCapacity;
      this.ammo = this.bulletsLoaded + this.magazines * this.magazineCapacity;
    }
  }

  public override void Rearm()
  {
    int ammo = this.ammo;
    this.magazines = this.maxMagazines;
    this.bulletsLoaded = this.magazineCapacity;
    this.ammo = this.bulletsLoaded + this.magazines * this.magazineCapacity;
    if (this.hardpoint != null)
      this.hardpoint.ModifyMass(this.info.massPerRound * (float) (this.ammo - ammo));
    this.ReportReloading(false);
  }

  public override int GetAmmoLoaded() => this.bulletsLoaded;

  public override int GetAmmoTotal() => this.ammo;

  public override int GetFullAmmo() => this.magazineCapacity * (1 + this.maxMagazines);

  public override float GetReloadProgress()
  {
    return (double) this.reloadTime <= 0.0 ? 0.0f : this.timeUntilReload / this.reloadTime;
  }

  private void OnDestroy()
  {
    if (this.hardpoint != null)
      this.hardpoint.ModifyMass(-this.info.massPerRound * (float) this.ammo);
    if (!(this.attachedUnit is Aircraft attachedUnit))
      return;
    foreach (AudioSource source in this.sources)
      attachedUnit.DeregisterDopplerSound(source);
  }

  public override void SetTarget(Unit target)
  {
    this.currentTarget = target;
    if (!this.proximityTimer && !(bool) (UnityEngine.Object) this.guidedProjectile)
      return;
    this.proximityFuseTarget = target;
  }

  private void ShotSound()
  {
    if ((double) this.attachedUnit.displayDetail < 1.0)
      return;
    if (this.sources.Length < 2)
    {
      this.sources[0].PlayOneShot(this.fireSounds[UnityEngine.Random.Range(0, this.fireSounds.Length)]);
      if ((UnityEngine.Object) this.recoilSound != (UnityEngine.Object) null)
      {
        if (!this.heatEnabled)
          this.recoilSound.pitch = UnityEngine.Random.Range(0.95f, 1.05f);
        this.recoilSound.Play();
      }
    }
    else if (!this.sources[1].isPlaying)
      this.sources[0].PlayOneShot(this.fireStart);
    if (this.sources.Length <= 1 || this.sources[1].isPlaying || (double) Time.timeSinceLevelLoad - (double) this.lastFired >= (double) this.fireInterval * 1.1000000238418579 + (double) Time.deltaTime)
      return;
    this.sources[1].Play();
    this.sources[1].time = UnityEngine.Random.Range(0.0f, this.sources[1].clip.length);
  }

  private void LoopSounds()
  {
    if (this.sources[1].isPlaying)
    {
      if (this.modifyPitch)
      {
        if ((double) this.sources[1].pitch < (double) this.pitch)
          this.sources[1].pitch += Time.deltaTime * this.pitchClimbRate;
        else
          this.sources[1].pitch = this.pitch;
      }
      if ((double) Time.timeSinceLevelLoad - (double) this.lastFired <= (double) Time.deltaTime + (double) this.fireInterval)
        return;
      this.sources[1].Stop();
      this.sources[0].PlayOneShot(this.fireEnd);
    }
    else
    {
      if (!this.modifyPitch)
        return;
      if ((double) this.sources[1].pitch > (double) this.startPitch)
        this.sources[1].pitch -= Time.deltaTime;
      else
        this.sources[1].pitch = this.startPitch;
    }
  }

  private void SpawnBullet(float timeOffset)
  {
    this.TrackFiringVisibility().Forget();
    this.ShotSound();
    if ((UnityEngine.Object) this.recoilTransform != (UnityEngine.Object) null)
    {
      ++this.recoilEnergy;
      if (this.attachedUnit.LocalSim && (UnityEngine.Object) this.attachedUnit.rb != (UnityEngine.Object) null)
        this.attachedUnit.rb.AddForceAtPosition(-this.transform.forward * this.recoilImpulse, this.recoilTransform.position, ForceMode.Impulse);
    }
    if (this.attachedUnit.LocalSim && (double) this.fireInterval > 0.20000000298023224)
      this.attachedUnit.SingleRemoteFire(this.weaponStation.Number, this.weaponStation.Ammo - 1);
    foreach (ParticleSystem muzzleParticle in this.muzzleParticles)
      muzzleParticle.Play();
    if ((UnityEngine.Object) this.ejectionTransform != (UnityEngine.Object) null)
      this.SpawnEjection().Forget();
    if (this.heatEnabled)
      this.heat.GunFired();
    foreach (Transform muzzle in this.muzzles)
    {
      Vector3 vector3 = (UnityEngine.Object) this.velocityInherit != (UnityEngine.Object) null ? this.velocityInherit.velocity : Vector3.zero;
      ++this.tracerSeed;
      bool tracer = false;
      if (this.tracerSeed > this.tracerRatio)
      {
        this.tracerSeed -= this.tracerRatio;
        tracer = true;
      }
      if (this.hardpoint != null)
        this.hardpoint.ModifyMass(-this.info.massPerRound);
      bool active = NetworkManagerNuclearOption.i.Server.Active;
      if ((UnityEngine.Object) this.guidedProjectile != (UnityEngine.Object) null & active)
      {
        NetworkSceneSingleton<Spawner>.i.SpawnMissile(this.guidedProjectile, muzzle.transform.position, muzzle.transform.rotation, vector3 + muzzle.transform.forward * this.muzzleVelocity, this.proximityFuseTarget, this.attachedUnit);
        break;
      }
      if ((UnityEngine.Object) this.bulletSim == (UnityEngine.Object) null)
        this.bulletSim = BulletSim.Create(this.attachedUnit, this, this.weaponStation.GetTurret());
      this.bulletSim.AddBullet(muzzle.transform, vector3 + muzzle.transform.forward * this.muzzleVelocity, this.bulletSpread, this.bulletSelfDestruct, tracer, this.tracerSize, this.tracerColor, timeOffset, this.proximityFuseTarget);
      if (active && (UnityEngine.Object) this.attachedUnit != (UnityEngine.Object) null && (UnityEngine.Object) this.attachedUnit.NetworkHQ != (UnityEngine.Object) null)
        this.attachedUnit.NetworkHQ.missionStatsTracker.MunitionCost(this.attachedUnit, this.info.costPerRound);
    }
  }

  private async UniTask SpawnEjection()
  {
    await UniTask.Delay(200);
    await UniTask.WaitForFixedUpdate();
    GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.ejectionPrefab, (Transform) null);
    Rigidbody component = gameObject.GetComponent<Rigidbody>();
    component.solverIterations = 1;
    component.Move(this.ejectionTransform.position, this.ejectionTransform.rotation);
    component.transform.position = this.ejectionTransform.position;
    component.velocity = (UnityEngine.Object) this.velocityInherit != (UnityEngine.Object) null ? this.velocityInherit.velocity : Vector3.zero;
    component.AddRelativeForce(this.ejectionLinearVelocity, ForceMode.VelocityChange);
    component.AddRelativeTorque(this.ejectionRotationalVelocity, ForceMode.VelocityChange);
    UnityEngine.Object.Destroy((UnityEngine.Object) gameObject, this.ejectionLifetime);
  }

  public override void RemoteSingleFire(
    Unit firingUnit,
    Unit target,
    Vector3 inheritedVelocity,
    WeaponStation weaponStation,
    GlobalPosition aimpoint)
  {
    if ((double) this.timeUntilReload > 0.0 || this.Safety)
      return;
    if (this.hardpoint != null)
    {
      if (this.hardpoint.part.IsDetached())
        return;
      if (this.info.useWeaponDoors)
        this.hardpoint.SpringOpenBayDoors();
    }
    if (firingUnit is Aircraft aircraft)
      aircraft.RequestRearm();
    this.SpawnBullet(0.0f);
  }

  public override void Fire(
    Unit firingUnit,
    Unit target,
    Vector3 inheritedVelocity,
    WeaponStation weaponStation,
    GlobalPosition aimpoint)
  {
    if ((double) this.timeUntilReload > 0.0 || this.Safety || weaponStation.Ammo <= 0 && this.attachedUnit.LocalSim)
      return;
    if (this.hardpoint != null)
    {
      if (this.hardpoint.part.IsDetached())
        return;
      if (this.info.useWeaponDoors)
        this.hardpoint.SpringOpenBayDoors();
    }
    if (firingUnit is Aircraft aircraft)
      aircraft.RequestRearm();
    this.ticksSinceTriggerPull = 0;
    this.weaponStation = weaponStation;
    this.enabled = true;
  }

  private void Update() => ++this.ticksSinceTriggerPull;

  private async UniTask SpinBarrels()
  {
    Gun gun = this;
    gun.currentSpinRate = 1f;
    CancellationToken cancel = gun.destroyCancellationToken;
    while ((double) gun.currentSpinRate > 0.0)
    {
      if (cancel.IsCancellationRequested)
      {
        cancel = new CancellationToken();
        return;
      }
      gun.spinTransform.Rotate(gun.currentSpinRate * gun.spinRate * Time.deltaTime * Vector3.forward, Space.Self);
      gun.currentSpinRate -= Time.deltaTime;
      await UniTask.Yield();
    }
    cancel = new CancellationToken();
  }

  private void FixedUpdate()
  {
    if ((double) this.queuedBullets < 1.0 && (double) Time.timeSinceLevelLoad - (double) this.lastFired > (double) this.fireInterval)
      this.queuedBullets = 1f;
    this.queuedBullets = this.ticksSinceTriggerPull < 2 ? this.queuedBullets + (float) ((double) Time.fixedDeltaTime * (double) this.fireRate * 0.016669999808073044) : 0.0f;
    this.queuedBullets = Mathf.Min(this.queuedBullets, (float) this.bulletsLoaded);
    int roundsFired = 0;
    while ((double) this.queuedBullets >= 1.0)
    {
      --this.queuedBullets;
      this.SpawnBullet(this.queuedBullets * this.fireInterval);
      ++roundsFired;
      --this.ammo;
      --this.bulletsLoaded;
      this.weaponStation.Updated();
    }
    if (roundsFired > 0)
    {
      this.lastFired = Time.timeSinceLevelLoad;
      this.weaponStation.UpdateLastFired(roundsFired);
      if ((double) this.spinRate > 0.0 && FastMath.InRange(this.spinTransform.position, SceneSingleton<CameraStateManager>.i.transform.position, 100f))
      {
        if ((double) this.currentSpinRate <= 0.0)
          this.SpinBarrels().Forget();
        this.currentSpinRate = 1f;
      }
    }
    if (this.bulletsLoaded == 0 && !this.reloading && this.magazines > 0)
    {
      this.timeUntilReload = this.reloadTime;
      this.reloading = true;
      this.ReportReloading(true);
      --this.magazines;
      if ((UnityEngine.Object) this.reloadSound != (UnityEngine.Object) null)
        this.reloadSound.Play();
    }
    if (this.sources.Length > 1)
      this.LoopSounds();
    if ((double) this.recoilTravel > 0.0 && (UnityEngine.Object) this.recoilTransform != (UnityEngine.Object) null)
    {
      this.recoilPosition += (double) this.recoilEnergy > 0.0 ? this.recoilRate * Time.deltaTime : -this.recoilReturnRate * Time.deltaTime;
      if ((double) this.recoilPosition >= 1.0)
        this.recoilEnergy = 0.0f;
      this.recoilPosition = Mathf.Clamp01(this.recoilPosition);
      this.recoilTransform.localPosition = new Vector3(0.0f, 0.0f, -this.recoilPosition * this.recoilTravel);
    }
    if (this.heatEnabled)
      this.heat.Update(Time.deltaTime);
    if ((double) this.timeUntilReload > 0.0)
    {
      this.timeUntilReload -= Time.fixedDeltaTime;
      if ((double) this.timeUntilReload > 0.0)
        return;
      this.bulletsLoaded = this.magazineCapacity;
      this.reloading = false;
      this.ReportReloading(false);
      this.weaponStation.Updated();
    }
    else
    {
      if ((double) Time.timeSinceLevelLoad - (double) this.lastFired <= 1.0 || this.heatEnabled && (double) this.heat.GetNormalisedHeat() > 0.0)
        return;
      this.enabled = false;
    }
  }

  public int GetCurrentAmmo() => this.bulletsLoaded + this.magazines * this.magazineCapacity;

  [Serializable]
  private class Heat
  {
    [SerializeField]
    private AudioSource barrelSteamSound;
    [SerializeField]
    private ParticleSystem barrelSteamParticles;
    [SerializeField]
    private ParticleSystem breachSteamParticles;
    [SerializeField]
    private AudioSource barrelDamageSound;
    [SerializeField]
    private ParticleSystem barrelDamageParticles;
    private Gun gun;
    private float heat;
    private float overheatFactor;
    [SerializeField]
    private float heatPerShot;
    [SerializeField]
    private float coolingPerSecond;
    [SerializeField]
    private float coolingPer100kph;
    [SerializeField]
    private float maxHeat;
    private float baseFireRate;
    [SerializeField]
    private float firerateDegradation;
    private float baseSpread;
    [SerializeField]
    private float accuracyDegradation;
    [SerializeField]
    private float velocityDegradation;
    private ParticleSystem.MainModule particleMain;

    public void Initialize(Gun gun)
    {
      this.gun = gun;
      this.baseFireRate = gun.fireRate;
      this.baseSpread = gun.bulletSpread;
    }

    public void Update(float time)
    {
      if ((double) this.heat <= 0.0)
        return;
      float num = (this.coolingPerSecond + (float) ((double) this.coolingPer100kph * (double) this.gun.attachedUnit.speed * 0.035999998450279236)) * time;
      if ((double) this.heat > (double) num)
      {
        this.heat -= num;
        this.overheatFactor = Mathf.Clamp((float) ((double) this.heat / (double) this.maxHeat - 1.0), 0.0f, Mathf.Min((float) ((double) this.baseFireRate / (double) this.firerateDegradation * 1.0099999904632568), this.gun.info.muzzleVelocity / this.velocityDegradation));
        if ((double) this.overheatFactor > 0.0)
        {
          this.gun.fireRate = this.baseFireRate - this.firerateDegradation * this.overheatFactor;
          this.gun.fireInterval = 60f / this.gun.fireRate;
          if ((UnityEngine.Object) this.gun.recoilSound != (UnityEngine.Object) null)
            this.gun.recoilSound.pitch = (float) (1.0 - (double) this.overheatFactor * (double) this.firerateDegradation / (double) this.baseFireRate);
          this.gun.bulletSpread = this.baseSpread + this.accuracyDegradation * this.overheatFactor;
          this.gun.muzzleVelocity = this.gun.info.muzzleVelocity - this.velocityDegradation * this.overheatFactor;
          if ((UnityEngine.Object) this.barrelSteamSound != (UnityEngine.Object) null)
          {
            if (!this.barrelSteamSound.isPlaying)
              this.barrelSteamSound.Play();
            this.barrelSteamSound.volume = Mathf.Clamp01(this.overheatFactor);
          }
          if (!((UnityEngine.Object) this.barrelSteamParticles != (UnityEngine.Object) null) || !((UnityEngine.Object) this.breachSteamParticles != (UnityEngine.Object) null))
            return;
          Color color = new Color(1f, 1f, 1f, Mathf.Clamp01(this.overheatFactor));
          if (!this.barrelSteamParticles.isPlaying)
            this.barrelSteamParticles.Play();
          if (!this.breachSteamParticles.isPlaying)
            this.breachSteamParticles.Play();
          this.particleMain = this.barrelSteamParticles.main;
          this.particleMain.startColor = (ParticleSystem.MinMaxGradient) color;
          this.particleMain.startLifetimeMultiplier = Mathf.Lerp(2f, 0.2f, Mathf.Clamp01(this.gun.attachedUnit.speed / 10f));
          this.particleMain = this.breachSteamParticles.main;
          this.particleMain.startColor = (ParticleSystem.MinMaxGradient) color;
        }
        else
        {
          this.overheatFactor = 0.0f;
          this.gun.fireRate = this.baseFireRate;
          this.gun.fireInterval = 60f / this.baseFireRate;
          if ((UnityEngine.Object) this.gun.recoilSound != (UnityEngine.Object) null)
            this.gun.recoilSound.pitch = 1f;
          this.gun.bulletSpread = this.baseSpread;
          this.gun.muzzleVelocity = this.gun.info.muzzleVelocity;
          this.barrelSteamSound.Stop();
          this.barrelSteamParticles.Stop();
          this.breachSteamParticles.Stop();
        }
      }
      else
        this.heat = 0.0f;
    }

    public void GunFired()
    {
      if ((double) this.overheatFactor > 1.0)
      {
        this.heat += this.heatPerShot * (this.overheatFactor - 1f);
        if ((UnityEngine.Object) this.barrelDamageParticles != (UnityEngine.Object) null)
          this.barrelDamageParticles.Play();
        if ((UnityEngine.Object) this.barrelDamageSound != (UnityEngine.Object) null)
        {
          this.barrelDamageSound.pitch = UnityEngine.Random.Range(0.9f, 1.1f);
          this.barrelDamageSound.Play();
        }
      }
      this.heat += this.heatPerShot;
    }

    public float GetNormalisedHeat() => this.heat / this.maxHeat;
  }
}

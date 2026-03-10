// Decompiled with JetBrains decompiler
// Type: Laser
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.Networking;
using System;
using UnityEngine;

#nullable disable
public class Laser : Weapon
{
  [SerializeField]
  private float power;
  [SerializeField]
  private float maxAngle;
  [SerializeField]
  private float trackingRate;
  [SerializeField]
  private AnimationCurve damageAtRange;
  [ColorUsage(true, true)]
  [SerializeField]
  private Color color;
  private ParticleSystem[] hitParticles;
  [SerializeField]
  private ParticleSystem[] muzzleParticles;
  [SerializeField]
  private GameObject hitEffectPrefab;
  [SerializeField]
  private Renderer beamRenderer;
  private GameObject hitEffectSpawn;
  private Transform currentTargetTransform;
  [SerializeField]
  private float blastDamage;
  [SerializeField]
  private float fireDamage;
  [SerializeField]
  private Transform directionTransform;
  [SerializeField]
  private AudioClip fireStart;
  [SerializeField]
  private AudioClip fireSustained;
  [SerializeField]
  private AudioClip fireEnd;
  [SerializeField]
  private bool modifyPitch;
  [SerializeField]
  private float pitchFallRate = 1f;
  [SerializeField]
  private float startPitchMultiplier = 1f;
  private float startPitch;
  private bool fireCommanded;
  private AudioSource[] sources;
  [SerializeField]
  private float volume = 1f;
  [SerializeField]
  private float pitch = 1f;
  private PowerSupply powerSupply;
  [SerializeField]
  private bool vehicularPowerSupply;
  private float lastDamageTick;
  private float fireTime;
  private float laserLastFired;
  private float previousLastFired;
  private float beamScale;

  private void Start()
  {
    this.beamRenderer.enabled = false;
    this.beamScale = this.beamRenderer.transform.localScale.x;
    this.lastDamageTick = Time.timeSinceLevelLoad;
    this.sources = new AudioSource[3];
    for (int index = 0; index < this.sources.Length; ++index)
    {
      AudioSource audioSource = this.gameObject.AddComponent<AudioSource>();
      audioSource.outputAudioMixerGroup = SoundManager.i.EffectsMixer;
      audioSource.spatialBlend = 1f;
      audioSource.spread = 20f;
      audioSource.dopplerLevel = 0.0f;
      audioSource.minDistance = 10f;
      audioSource.maxDistance = 50f;
      this.sources[index] = audioSource;
    }
    this.sources[0].clip = this.fireStart;
    this.sources[1].clip = this.fireSustained;
    this.sources[2].clip = this.fireEnd;
    this.sources[1].loop = true;
    if (!this.modifyPitch)
      return;
    this.startPitch = this.pitch * this.startPitchMultiplier;
    this.sources[1].pitch = this.startPitch;
  }

  public override void AttachToUnit(Unit unit)
  {
    base.AttachToUnit(unit);
    this.powerSupply = unit.GetPowerSupply();
    this.powerSupply.AddUser();
    this.attachedUnit.onDisableUnit += new Action<Unit>(this.AttachedUnit_onDisableUnit);
  }

  private void AttachedUnit_onDisableUnit(Unit obj) => this.beamRenderer.enabled = false;

  public void OnDestroy()
  {
    this.beamRenderer.enabled = false;
    if ((UnityEngine.Object) this.powerSupply != (UnityEngine.Object) null)
      this.powerSupply.RemoveUser();
    this.attachedUnit.onDisableUnit -= new Action<Unit>(this.AttachedUnit_onDisableUnit);
  }

  private void LaserSound()
  {
    if (this.beamRenderer.enabled)
    {
      if ((double) this.fireTime > 0.20000000298023224 && !this.sources[1].isPlaying)
        this.sources[1].Play();
      if ((double) this.fireTime == 0.0 && (double) Time.timeSinceLevelLoad - (double) this.laserLastFired > 0.30000001192092896)
        this.sources[0].PlayOneShot(this.fireStart);
      this.fireTime += Time.deltaTime;
      this.laserLastFired = Time.timeSinceLevelLoad;
    }
    else
    {
      if (this.sources[1].isPlaying)
        this.sources[1].Stop();
      if ((double) this.fireTime > 0.0)
        this.sources[2].Play();
      this.fireTime = 0.0f;
    }
    if (this.sources[1].isPlaying)
    {
      if (!this.modifyPitch)
        return;
      if ((double) this.sources[1].pitch > (double) this.pitch)
        this.sources[1].pitch -= Time.deltaTime * this.pitchFallRate;
      else
        this.sources[1].pitch = this.pitch;
    }
    else
    {
      if (!this.modifyPitch)
        return;
      if ((double) this.sources[1].pitch < (double) this.startPitch)
        this.sources[1].pitch += Time.deltaTime * this.pitchFallRate;
      else
        this.sources[1].pitch = this.startPitch;
    }
  }

  public override void SetTarget(Unit target)
  {
    this.enabled = true;
    this.currentTargetTransform = (UnityEngine.Object) target != (UnityEngine.Object) null ? target.GetRandomPart() : (Transform) null;
    this.currentTarget = target;
  }

  public override void Fire(
    Unit owner,
    Unit target,
    Vector3 inheritedVelocity,
    WeaponStation weaponStation,
    GlobalPosition aimpoint)
  {
    if (!this.enabled)
      this.enabled = true;
    this.fireCommanded = true;
    this.previousLastFired = this.lastFired;
    this.lastFired = Time.timeSinceLevelLoad;
    weaponStation.LastFiredTime = Time.timeSinceLevelLoad;
    if (!((UnityEngine.Object) this.hitEffectSpawn == (UnityEngine.Object) null))
      return;
    this.hitEffectSpawn = UnityEngine.Object.Instantiate<GameObject>(this.hitEffectPrefab, (Transform) null);
    this.hitParticles = this.hitEffectSpawn.GetComponentsInChildren<ParticleSystem>();
  }

  public void LateUpdate()
  {
    if ((double) this.lastFired != (double) this.previousLastFired && (double) Time.timeSinceLevelLoad <= (double) this.lastFired + 0.20000000298023224)
      return;
    this.fireCommanded = false;
    this.beamRenderer.enabled = false;
    this.enabled = false;
    this.LaserSound();
  }

  private void FixedUpdate()
  {
    Vector3 end = (UnityEngine.Object) this.currentTargetTransform != (UnityEngine.Object) null ? this.currentTargetTransform.position : this.transform.position + this.transform.forward * 20000f;
    GlobalPosition knownPosition;
    if ((UnityEngine.Object) this.currentTarget != (UnityEngine.Object) null && !this.attachedUnit.NetworkHQ.IsTargetBeingTracked(this.currentTarget) && this.attachedUnit.NetworkHQ.TryGetKnownPosition(this.currentTarget, out knownPosition))
      end = knownPosition.ToLocalPosition();
    Vector3 vector3 = end - this.transform.position;
    this.directionTransform.rotation = Quaternion.LookRotation(Vector3.RotateTowards(this.transform.forward, vector3, this.maxAngle * ((float) Math.PI / 180f), 0.0f));
    float num1 = Vector3.Angle(this.directionTransform.forward, vector3);
    if ((double) num1 > 1.0)
      end = this.transform.position + this.directionTransform.forward * 20000f;
    if (this.fireCommanded && (double) num1 < 1.0)
    {
      foreach (ParticleSystem muzzleParticle in this.muzzleParticles)
        muzzleParticle.Play();
      float num2 = 1f;
      if (!this.vehicularPowerSupply)
        num2 = Mathf.Clamp01(this.powerSupply.DrawPower((double) this.fireTime < 0.20000000298023224 ? this.power * 0.1f : this.power) / this.power);
      this.beamRenderer.enabled = true;
      this.beamRenderer.material.SetVector("_WorldOffset", (Vector4) Datum.originPosition);
      this.beamRenderer.material.SetColor("_Color", this.color * num2);
      RaycastHit hitInfo;
      if (Physics.Linecast(this.directionTransform.position, end, out hitInfo, -8193))
      {
        this.beamRenderer.transform.localScale = new Vector3(this.beamScale, this.beamScale, hitInfo.distance);
        if ((UnityEngine.Object) this.hitEffectSpawn != (UnityEngine.Object) null)
        {
          this.hitEffectSpawn.transform.SetParent(hitInfo.transform);
          this.hitEffectSpawn.transform.position = hitInfo.point;
        }
        foreach (ParticleSystem hitParticle in this.hitParticles)
        {
          if ((UnityEngine.Object) hitParticle != (UnityEngine.Object) null)
            hitParticle.Play();
        }
        IDamageable component = hitInfo.collider.gameObject.GetComponent<IDamageable>();
        if (NetworkManagerNuclearOption.i.Server.Active && component != null && (double) Time.timeSinceLevelLoad - (double) this.lastDamageTick > 0.20000000298023224)
        {
          this.lastDamageTick = Time.timeSinceLevelLoad;
          float num3 = this.damageAtRange.Evaluate(hitInfo.distance) * num2;
          component.TakeDamage(0.0f, (float) ((double) this.blastDamage * (double) num3 * 0.20000000298023224), 1f, (float) ((double) this.fireDamage * (double) num3 * 0.20000000298023224), 0.0f, this.attachedUnit.persistentID);
        }
      }
    }
    else
      this.beamRenderer.enabled = false;
    this.LaserSound();
  }
}

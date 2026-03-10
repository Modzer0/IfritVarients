// Decompiled with JetBrains decompiler
// Type: JetNozzle
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;

#nullable disable
public class JetNozzle : MonoBehaviour
{
  [SerializeField]
  private UnitPart part;
  [SerializeField]
  private Turbojet turbojet;
  [SerializeField]
  private GameObject failureEffect;
  [SerializeField]
  private Transform thrustTransform;
  [SerializeField]
  private JetNozzle.JetParticleParameters heatHaze;
  [SerializeField]
  private float thrustProportion;
  [SerializeField]
  private float swivelSpeed;
  [SerializeField]
  private float pitchThrust;
  [SerializeField]
  private float rollThrust;
  [SerializeField]
  private float yawSwivel;
  [SerializeField]
  private float angleMax;
  [SerializeField]
  private float angleMin;
  [SerializeField]
  private float thrustMaxVolume;
  [SerializeField]
  private float IRMin;
  [SerializeField]
  private float IRMax;
  [SerializeField]
  private ParticleSystem glow;
  [SerializeField]
  private Transform[] vectorTransforms;
  [SerializeField]
  private JetNozzle.Afterburner[] afterburners;
  [SerializeField]
  private AudioSource thrustAudio;
  [Header("Swivel Audio")]
  [SerializeField]
  private AudioSource swivelAudio;
  [SerializeField]
  private float swivelPitchBase;
  [SerializeField]
  private float swivelPitchFactor;
  [SerializeField]
  [Range(0.0f, 1f)]
  private float volumeMultiplier;
  private float swivelVolume;
  private ParticleSystem.MainModule glowMain;
  private ParticleSystem.MainModule afterburnerMain;
  private Aircraft aircraft;
  private float lastSlowUpdate;
  private float afterburnerOpacity;
  private float totalThrust;
  [HideInInspector]
  public float priority;
  private float swivelAngle;
  private float fuelConsumption;
  private bool outOfSoundCone;
  private IRSource irSource;
  private DamageParticles damageParticles;
  private float camFacing;
  private float camDistance;
  private float smoothVel;

  private void Awake()
  {
    if ((UnityEngine.Object) this.heatHaze.system != (UnityEngine.Object) null)
      this.heatHaze.Initialize();
    this.thrustAudio.time = (float) UnityEngine.Random.Range(0, 2);
    if ((UnityEngine.Object) this.glow != (UnityEngine.Object) null)
      this.glowMain = this.glow.main;
    this.aircraft = this.part.parentUnit as Aircraft;
    this.CreateIRSource();
    if (!((UnityEngine.Object) this.turbojet != (UnityEngine.Object) null))
      return;
    this.turbojet.OnEngineDisable += new Action(this.JetNozzle_OnEngineFail);
  }

  public void CreateIRSource()
  {
    this.irSource = new IRSource(this.thrustTransform, 0.0f, false);
    this.aircraft.AddIRSource(this.irSource);
  }

  public float GetPriority(ControlInputs inputs)
  {
    this.priority = Mathf.Max((float) ((double) this.thrustProportion + (double) this.pitchThrust * (double) inputs.pitch + (double) this.rollThrust * (double) inputs.roll), 0.0f);
    return this.priority;
  }

  public float GetIRMin() => this.IRMin;

  public float GetIRMax()
  {
    float irMax = this.IRMax;
    foreach (JetNozzle.Afterburner afterburner in this.afterburners)
      irMax += afterburner.GetMaxIRIntensity();
    return irMax;
  }

  public float GetMaxThrust()
  {
    float maxThrust = 0.0f;
    foreach (JetNozzle.Afterburner afterburner in this.afterburners)
      maxThrust += afterburner.GetMaxThrust();
    return maxThrust;
  }

  public float GetTotalThrust()
  {
    float totalThrust = 0.0f;
    foreach (JetNozzle.Afterburner afterburner in this.afterburners)
      totalThrust += afterburner.GetThrust();
    return totalThrust;
  }

  public float GetFuelConsumption() => this.fuelConsumption;

  public void SlowUpdate(float rpmRatio, float thrustRatio)
  {
    this.lastSlowUpdate = Time.timeSinceLevelLoad;
    if ((UnityEngine.Object) this.heatHaze.system != (UnityEngine.Object) null)
      this.heatHaze.UpdateParticles(thrustRatio, rpmRatio, this.aircraft.speed);
    Vector3 from = this.thrustTransform.position - Camera.main.transform.position;
    this.camFacing = Vector3.Dot(from.normalized, this.thrustTransform.forward);
    this.camDistance = from.magnitude;
    float speedOfSound = LevelInfo.GetSpeedOfSound(this.aircraft.GlobalPosition().y);
    if ((double) this.aircraft.speed > (double) speedOfSound)
      SonicBoomManager.RegisterAircraft(this.aircraft);
    int num1 = this.outOfSoundCone ? 1 : 0;
    this.outOfSoundCone = (double) this.aircraft.speed > (double) speedOfSound && (double) this.camDistance > 20.0 && (double) Vector3.Angle(from, this.aircraft.rb.velocity) > 45.0 * (double) speedOfSound / (double) this.aircraft.speed;
    int num2 = this.outOfSoundCone ? 1 : 0;
    if (num1 == num2)
      return;
    if (this.outOfSoundCone)
    {
      foreach (JetNozzle.Afterburner afterburner in this.afterburners)
        afterburner.DisableAudio();
      this.thrustAudio.Stop();
      this.thrustAudio.volume = 0.0f;
    }
    else
    {
      foreach (JetNozzle.Afterburner afterburner in this.afterburners)
        afterburner.EnableAudio();
      this.thrustAudio.Play();
    }
  }

  public bool IsOutOfSoundCone() => this.outOfSoundCone;

  public void FastUpdate(float rpmRatio, float thrustRatio)
  {
    this.thrustAudio.volume = thrustRatio * this.thrustMaxVolume * Mathf.Lerp(0.7f, 2f, this.camFacing);
    this.thrustAudio.dopplerLevel = (double) this.camDistance < 20.0 || (double) this.camFacing > 0.0 ? 0.0f : 1f;
  }

  private void JetNozzle_OnEngineFail()
  {
    this.thrustProportion = 0.0f;
    this.FailureEffect();
  }

  public void FailureEffect()
  {
    GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.failureEffect, this.thrustTransform);
    gameObject.transform.SetParent(this.thrustTransform);
    gameObject.transform.localPosition = Vector3.zero;
    this.irSource.transform.SetParent(this.aircraft.transform);
    this.irSource.intensity = 0.2f;
  }

  public void CullDamageParticles()
  {
    if (!((UnityEngine.Object) this.damageParticles != (UnityEngine.Object) null))
      return;
    this.damageParticles.ParentObjectCulled();
  }

  public void Thrust(
    float thrustAmount,
    float rpmRatio,
    float thrustRatio,
    float throttle,
    bool allowAfterburner)
  {
    this.totalThrust = thrustAmount;
    this.irSource.intensity = Mathf.Lerp(this.IRMin, this.IRMax, thrustRatio);
    this.fuelConsumption = 0.0f;
    foreach (JetNozzle.Afterburner afterburner in this.afterburners)
    {
      afterburner.Run(allowAfterburner ? throttle * ((double) rpmRatio > 0.85000002384185791 ? 1f : 0.0f) : 0.0f);
      this.fuelConsumption += afterburner.GetFuelConsumption();
      this.totalThrust += afterburner.GetThrust() * Mathf.Clamp(this.aircraft.airDensity, 0.4f, 1f);
      this.irSource.intensity += afterburner.GetIRIntensity();
    }
    if (this.aircraft.LocalSim)
      this.part.rb.AddForceAtPosition(this.thrustTransform.forward * this.totalThrust, this.thrustTransform.position);
    this.FastUpdate(rpmRatio, thrustRatio);
    if ((double) Time.timeSinceLevelLoad - (double) this.lastSlowUpdate <= 0.10000000149011612)
      return;
    this.SlowUpdate(this.totalThrust, thrustRatio);
  }

  public void Aim(ControlInputs inputs, Vector3 aimDirection)
  {
    aimDirection += -this.part.transform.forward * this.yawSwivel * inputs.yaw;
    float f = Mathf.Clamp(-TargetCalc.GetAngleOnAxis(aimDirection, -this.thrustTransform.forward, this.transform.right), -this.swivelSpeed * Time.deltaTime, this.swivelSpeed * Time.deltaTime);
    if ((UnityEngine.Object) this.swivelAudio != (UnityEngine.Object) null)
    {
      this.swivelVolume = FastMath.SmoothDamp(this.swivelVolume, (double) Mathf.Abs(f) >= (double) this.swivelSpeed * (double) Time.deltaTime * 0.5 ? 1f : 0.0f, ref this.smoothVel, 0.1f);
      this.swivelAudio.volume = this.swivelVolume * this.volumeMultiplier;
      this.swivelAudio.pitch = this.swivelPitchBase + this.swivelVolume * this.swivelPitchFactor;
    }
    this.swivelAngle += f;
    this.swivelAngle = Mathf.Clamp(this.swivelAngle, this.angleMin, this.angleMax);
    this.transform.localEulerAngles = new Vector3(this.swivelAngle, 0.0f, 0.0f);
  }

  [Serializable]
  private class Afterburner
  {
    [SerializeField]
    private Renderer flameRenderer;
    [SerializeField]
    private Renderer nozzleGlowRenderer;
    [SerializeField]
    private Transform thrustDirection;
    [SerializeField]
    private AudioSource source;
    [SerializeField]
    private float smoothing;
    [SerializeField]
    private float throttleStart = 99.8f;
    [SerializeField]
    private float throttleEnd = 100f;
    [SerializeField]
    private float thrust;
    [SerializeField]
    private float fuelConsumption;
    [SerializeField]
    private float flameBrightness;
    [SerializeField]
    private float nozzleGlowBrightness;
    [SerializeField]
    private float IRIntensity = 1f;
    public float afterburnerAmount;

    public void Run(float throttleAmount)
    {
      this.afterburnerAmount = Mathf.Lerp(this.afterburnerAmount, Mathf.Clamp01((float) (((double) throttleAmount - (double) this.throttleStart) / ((double) this.throttleEnd - (double) this.throttleStart))), this.smoothing * Time.deltaTime);
      this.nozzleGlowRenderer.material.SetColor("_EmissionColor", Color.white * this.afterburnerAmount * this.nozzleGlowBrightness);
      if ((double) this.afterburnerAmount < 0.0099999997764825821)
      {
        if (this.flameRenderer.enabled)
          this.flameRenderer.enabled = false;
        this.source.volume = 0.0f;
      }
      else
      {
        if (!this.flameRenderer.enabled)
          this.flameRenderer.enabled = true;
        this.source.volume = this.afterburnerAmount;
        this.flameRenderer.material.SetFloat("_Brightness", Mathf.Lerp(0.0f, this.flameBrightness, this.afterburnerAmount));
        this.flameRenderer.transform.localScale = new Vector3(1f, 1f, (float) (1.0 + (double) Mathf.PerlinNoise1D(Time.timeSinceLevelLoad * 15f) * 0.05000000074505806));
      }
    }

    public float GetIRIntensity() => this.IRIntensity * this.afterburnerAmount;

    public float GetMaxIRIntensity() => this.IRIntensity;

    public void DisableAudio()
    {
      if (!this.source.isPlaying)
        return;
      this.source.Stop();
    }

    public void EnableAudio()
    {
      if (this.source.isPlaying || (double) this.afterburnerAmount <= 0.0099999997764825821)
        return;
      this.source.Play();
    }

    public float GetFuelConsumption() => this.afterburnerAmount * this.fuelConsumption;

    public float GetMaxThrust() => this.thrust;

    public float GetThrust() => this.afterburnerAmount * this.thrust;
  }

  [Serializable]
  private struct JetParticleParameters
  {
    public ParticleSystem system;
    private ParticleSystem.MainModule main;
    private ParticleSystem.EmissionModule emit;
    [SerializeField]
    private float sizeMin;
    [SerializeField]
    private float sizeMax;
    [SerializeField]
    private float speedMin;
    [SerializeField]
    private float speedMax;
    [SerializeField]
    private float opacityMin;
    [SerializeField]
    private float opacityMax;
    [SerializeField]
    private float lifeMin;
    [SerializeField]
    private float lifeMax;
    [SerializeField]
    private float airspeedLifeFactor;
    [SerializeField]
    private float rateMin;
    [SerializeField]
    private float rateMax;

    public void Initialize()
    {
      this.main = this.system.main;
      this.emit = this.system.emission;
    }

    public void UpdateParticles(float thrustRatio, float rpmRatio, float airspeed)
    {
      if ((double) rpmRatio < 0.20000000298023224 && this.system.isPlaying)
        this.system.Stop();
      if ((double) rpmRatio > 0.20000000298023224 && !this.system.isPlaying)
        this.system.Play();
      this.main.startColor = (ParticleSystem.MinMaxGradient) new Color(1f, 1f, 1f, Mathf.Lerp(this.opacityMin, this.opacityMax, thrustRatio));
      this.main.startSize = (ParticleSystem.MinMaxCurve) Mathf.Lerp(this.sizeMin, this.sizeMax, thrustRatio);
      this.main.startLifetime = (ParticleSystem.MinMaxCurve) (Mathf.Lerp(this.lifeMin, this.lifeMax, thrustRatio) / (float) (1.0 + (double) airspeed * (double) this.airspeedLifeFactor));
      this.main.startSpeed = (ParticleSystem.MinMaxCurve) Mathf.Lerp(this.speedMin, this.speedMax, thrustRatio);
      this.emit.rateOverTime = (ParticleSystem.MinMaxCurve) Mathf.Lerp(this.rateMin, this.rateMax, thrustRatio);
    }
  }
}

// Decompiled with JetBrains decompiler
// Type: TurbineEngine
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;

#nullable disable
public class TurbineEngine : MonoBehaviour, IEngine, IPowerSource, IReportDamage
{
  public float maxRPM;
  public float minRPM = 1000f;
  public float maxPower;
  private ControlInputs controlInputs;
  public float spoolUpTime;
  [SerializeField]
  private float startupTime = 10f;
  public float currentPower;
  public float powerRatio;
  public float RPMRatio;
  public float startedAmount;
  private bool operable = true;
  private float currentRPM;
  private float condition = 1f;
  public float throttle;
  private AudioSource turbineAudio;
  [SerializeField]
  private float pitch = 1f;
  [SerializeField]
  private float volume = 1f;
  [SerializeField]
  private AudioSource startupAudio;
  public Aircraft aircraft;
  [SerializeField]
  private ParticleSystem[] heatHazeParticles;
  [SerializeField]
  private float maxFuelConsumption;
  [SerializeField]
  private float IRMin;
  [SerializeField]
  private float IRMax;
  private float lastFuelCheck;
  private bool hasFuel;
  private IRSource heatSource;
  [SerializeField]
  private Transform IRSourceTransform;
  [SerializeField]
  private UnitPart[] criticalParts;
  [SerializeField]
  private float criticalDamageThreshold;
  [SerializeField]
  private GameObject failureEffect;
  [SerializeField]
  private Transform failureTransform;
  [SerializeField]
  private string failureMessage;
  [SerializeField]
  private AudioClip failureMessageAudio;

  public event Action OnEngineDisable;

  public event Action OnEngineDamage;

  public event Action<OnReportDamage> onReportDamage;

  private void OnEnable()
  {
    this.turbineAudio = this.GetComponent<AudioSource>();
    this.turbineAudio.volume = 0.0f;
    this.turbineAudio.loop = true;
    this.turbineAudio.time = UnityEngine.Random.Range(0.0f, this.turbineAudio.clip.length);
    this.controlInputs = this.aircraft.GetInputs();
    this.aircraft.engineStates.Add((IEngine) this);
    this.aircraft.onSpawnedInPosition += new Action(this.TurbineEngine_OnInitialize);
    this.heatSource = new IRSource(new GameObject(this.gameObject.name + "_IRSource").transform, 0.0f, false);
    this.heatSource.transform.SetParent(this.IRSourceTransform);
    this.heatSource.transform.localPosition = Vector3.zero;
    this.aircraft.AddIRSource(this.heatSource);
    foreach (UnitPart criticalPart in this.criticalParts)
    {
      criticalPart.onApplyDamage += new Action<UnitPart.OnApplyDamage>(this.TurbineEngine_OnDamage);
      criticalPart.onPartDetached += new Action<UnitPart>(this.TurbineEngine_OnDetach);
    }
  }

  private void TurbineEngine_OnInitialize()
  {
    if ((double) this.aircraft.radarAlt <= (double) this.aircraft.definition.spawnOffset.y + 1.0)
      return;
    this.hasFuel = true;
    this.startedAmount = 1f;
    this.throttle = 1f;
    this.currentRPM = this.maxRPM;
  }

  public float GetMaxThrust() => 0.0f;

  public float GetThrust() => 0.0f;

  public float GetMaxPower() => this.maxPower;

  public float GetPower() => this.currentPower;

  public float GetRPM() => this.currentRPM;

  public float GetRPMRatio() => this.currentRPM / this.maxRPM;

  public void Throttle(float throttle) => this.throttle = Mathf.Clamp01(throttle);

  public float GetIRMin() => this.IRMin;

  public float GetIRMax() => this.IRMax;

  public void SetInteriorSounds(bool useInteriorSound)
  {
  }

  private void UseFuel(float fuelConsumption)
  {
    this.lastFuelCheck = Time.timeSinceLevelLoad;
    this.hasFuel = this.aircraft.UseFuel(fuelConsumption);
  }

  private void TurbineEngine_OnDamage(UnitPart.OnApplyDamage e)
  {
    if (!this.operable)
      return;
    Action onEngineDamage = this.OnEngineDamage;
    if (onEngineDamage != null)
      onEngineDamage();
    this.condition = Mathf.Clamp((float) (((double) e.hitPoints - (double) this.criticalDamageThreshold) / (100.0 - (double) this.criticalDamageThreshold)), 0.0f, this.condition);
    if (!e.detached && (double) this.condition > 0.0)
      return;
    this.KillEngine();
  }

  private void TurbineEngine_OnDetach(UnitPart e)
  {
    if (!this.operable)
      return;
    Action onEngineDamage = this.OnEngineDamage;
    if (onEngineDamage != null)
      onEngineDamage();
    this.condition = 0.0f;
    this.KillEngine();
  }

  private void KillEngine()
  {
    Action<OnReportDamage> onReportDamage = this.onReportDamage;
    if (onReportDamage != null)
      onReportDamage(new OnReportDamage()
      {
        failureMessage = this.failureMessage,
        audioReport = this.failureMessageAudio
      });
    Action onEngineDisable = this.OnEngineDisable;
    if (onEngineDisable != null)
      onEngineDisable();
    this.heatSource.transform.SetParent(this.aircraft.transform);
    this.operable = false;
    foreach (UnitPart criticalPart in this.criticalParts)
    {
      criticalPart.onApplyDamage -= new Action<UnitPart.OnApplyDamage>(this.TurbineEngine_OnDamage);
      criticalPart.onPartDetached -= new Action<UnitPart>(this.TurbineEngine_OnDetach);
    }
    for (int index = 0; index < this.heatHazeParticles.Length; ++index)
    {
      if (!((UnityEngine.Object) this.heatHazeParticles[index] == (UnityEngine.Object) null) && this.heatHazeParticles[index].isPlaying)
        this.heatHazeParticles[index].Stop();
    }
    if ((UnityEngine.Object) this.failureTransform == (UnityEngine.Object) null || (double) this.transform.position.y < (double) Datum.origin.position.y)
      return;
    GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.failureEffect, this.failureTransform);
    gameObject.transform.localRotation = Quaternion.identity;
    DamageParticles component = gameObject.GetComponent<DamageParticles>();
    if ((UnityEngine.Object) component != (UnityEngine.Object) null)
      this.aircraft.spawnedEffects.Add(component);
    if (!((UnityEngine.Object) this.startupAudio != (UnityEngine.Object) null) || !this.startupAudio.isPlaying)
      return;
    this.startupAudio.Stop();
  }

  public bool IsOperable() => this.operable && this.hasFuel;

  private void Animate(bool running)
  {
    if ((double) this.aircraft.displayDetail < 1.0)
      return;
    this.turbineAudio.pitch = this.RPMRatio - this.powerRatio * this.pitch + this.pitch;
    if (running)
      this.turbineAudio.volume = (float) ((double) this.RPMRatio * 0.25 + (double) this.powerRatio * 0.75) * this.volume;
    else
      this.turbineAudio.volume = this.RPMRatio * 0.5f * this.volume;
  }

  private void Update()
  {
    if ((UnityEngine.Object) this.aircraft == (UnityEngine.Object) null)
      return;
    bool running = this.aircraft.Ignition && this.operable && this.hasFuel && (double) this.aircraft.airDensity > 0.20000000298023224;
    if ((double) this.RPMRatio < 0.30000001192092896 & running && (UnityEngine.Object) this.startupAudio != (UnityEngine.Object) null && !this.startupAudio.isPlaying)
      this.startupAudio.Play();
    if (running && (double) this.transform.position.y < (double) Datum.origin.position.y)
      this.KillEngine();
    this.RPMRatio = this.currentRPM / this.maxRPM;
    if (running)
    {
      this.currentRPM = Mathf.Lerp(this.currentRPM, this.maxRPM * (float) ((double) this.condition * 0.5 + 0.5), Time.deltaTime / ((double) this.currentRPM < (double) this.minRPM ? this.spoolUpTime * 2f : this.spoolUpTime));
      this.currentPower = this.currentRPM / this.maxRPM * this.maxPower * this.throttle;
      this.currentPower *= this.aircraft.airDensity / 1.225f;
      if ((double) this.currentRPM < (double) this.minRPM)
        this.currentPower = 0.0f;
      if ((double) this.startedAmount < 1.0)
      {
        this.startedAmount += Time.deltaTime / this.startupTime;
        this.currentPower *= this.startedAmount;
      }
      this.heatSource.intensity = Mathf.Lerp(this.IRMin, this.IRMax, this.currentPower / this.maxPower);
    }
    else
    {
      this.startedAmount = 0.0f;
      if (this.operable)
        this.currentRPM -= (float) ((double) this.maxRPM / (double) this.spoolUpTime * (double) Time.deltaTime * 0.10000000149011612);
      else
        this.currentRPM -= this.maxRPM / this.spoolUpTime * Time.deltaTime;
      this.currentPower = 0.0f;
      this.heatSource.intensity = this.IRMin;
    }
    if (this.heatHazeParticles != null)
    {
      bool flag = (double) this.aircraft.rb.velocity.sqrMagnitude > 2500.0;
      for (int index = 0; index < this.heatHazeParticles.Length; ++index)
      {
        if (!((UnityEngine.Object) this.heatHazeParticles[index] == (UnityEngine.Object) null))
        {
          if (((flag ? 0 : (!this.heatHazeParticles[index].isPlaying ? 1 : 0)) & (running ? 1 : 0)) != 0)
            this.heatHazeParticles[index].Play();
          if (this.heatHazeParticles[index].isPlaying & flag)
            this.heatHazeParticles[index].Stop();
        }
      }
    }
    this.currentRPM = Mathf.Clamp(this.currentRPM, 0.0f, this.maxRPM);
    this.powerRatio = (float) ((double) this.currentPower * (double) this.currentPower / ((double) this.maxPower * (double) this.maxPower));
    if ((double) Time.timeSinceLevelLoad - (double) this.lastFuelCheck > 1.0)
      this.UseFuel(this.powerRatio * this.maxFuelConsumption);
    this.Animate(running);
  }

  Transform IEngine.get_transform() => this.transform;
}

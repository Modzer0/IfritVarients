// Decompiled with JetBrains decompiler
// Type: DuctedFan
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;

#nullable disable
public class DuctedFan : 
  MonoBehaviour,
  IEngine,
  IThrustSource,
  IReportDamage,
  IPowerOutput,
  IPowerSource
{
  private Aircraft aircraft;
  private ControlInputs controlInputs;
  [SerializeField]
  private Transmission transmission;
  [SerializeField]
  private UnitPart unitPart;
  [SerializeField]
  private float minThrust;
  [SerializeField]
  private float spoolRate;
  [SerializeField]
  private float yawCoef;
  [SerializeField]
  private float bladeDrag;
  [SerializeField]
  private float responseTime;
  [SerializeField]
  private float maxRPM;
  [SerializeField]
  private float gearing;
  [SerializeField]
  private float nominalPower;
  [SerializeField]
  private float maxPower;
  [SerializeField]
  private float area;
  [SerializeField]
  private float idleThresholdPositive;
  [SerializeField]
  private float idleThresholdNegative;
  private float rpm;
  [SerializeField]
  private Transform thrustVector;
  [SerializeField]
  private Transform rotator;
  private GameObject thrustDebug;
  [SerializeField]
  private RotorShaft rotorShaft;
  private float trimThrust;
  private float availableThrust;
  private float maxThrust;
  private float currentThrust;
  private float availablePower;
  private float currentThrustSmoothVel;
  [SerializeField]
  private float IRMin;
  [SerializeField]
  private float IRMax;
  [SerializeField]
  private Transform IRTransform;
  [SerializeField]
  private bool isHeatSource;
  private IRSource heatSource;
  private bool inoperable;
  private bool reverseThrust;
  [SerializeField]
  private UnitPart[] criticalParts;
  [SerializeField]
  private float criticalDamageThreshold;
  [SerializeField]
  private MeshFilter meshFilter;
  [SerializeField]
  private MeshRenderer meshRenderer;
  [SerializeField]
  private Mesh slowMesh;
  [SerializeField]
  private Mesh fastMesh;
  [SerializeField]
  private Material slowMaterial;
  [SerializeField]
  private Material fastMaterial;
  [SerializeField]
  private float rpmThreshold;
  [SerializeField]
  private string failureMessage;
  [SerializeField]
  private AudioClip failureMessageAudio;
  [Header("Audio")]
  [SerializeField]
  private AudioSource fanSource;
  [SerializeField]
  private AudioClip exteriorSound;
  [SerializeField]
  private AudioClip interiorSound;
  [SerializeField]
  private float unthrottledVolume = 0.5f;
  [SerializeField]
  private float throttledVolume = 1f;
  [SerializeField]
  private float pitch = 1f;

  public event Action OnEngineDisable;

  public event Action OnEngineDamage;

  public event Action<OnReportDamage> onReportDamage;

  private void OnEnable()
  {
    this.maxThrust = Mathf.Pow((float) (2.4000000953674316 * (double) this.area * ((double) this.nominalPower * (double) this.nominalPower)), 0.3333f);
    this.aircraft = this.unitPart.parentUnit as Aircraft;
    this.controlInputs = this.aircraft.GetInputs();
    foreach (UnitPart criticalPart in this.criticalParts)
    {
      criticalPart.onApplyDamage += new Action<UnitPart.OnApplyDamage>(this.DuctedFan_OnDamage);
      criticalPart.onPartDetached += new Action<UnitPart>(this.DuctedFan_OnDetached);
    }
    if (PlayerSettings.debugVis)
    {
      this.thrustDebug = UnityEngine.Object.Instantiate<GameObject>(GameAssets.i.debugArrow, this.thrustVector);
      this.thrustDebug.transform.localPosition = Vector3.zero;
    }
    if ((UnityEngine.Object) this.rotorShaft == (UnityEngine.Object) null)
      this.controlInputs.customAxis1 = (float) (((double) this.idleThresholdNegative + (double) this.idleThresholdPositive) * 0.5);
    if (this.isHeatSource)
    {
      this.heatSource = new IRSource(this.IRTransform, 0.0f, false);
      this.aircraft.AddIRSource(this.heatSource);
    }
    this.aircraft.engines.Add((IEngine) this);
  }

  private void Update()
  {
    this.rotator.localEulerAngles += Vector3.forward * this.rpm * 6f * Time.deltaTime;
    if ((double) this.aircraft.displayDetail <= 1.0)
      return;
    this.Animate();
  }

  public void SetDesiredBaseThrust(float thrust) => this.trimThrust = thrust;

  public float GetMaxThrust() => this.maxThrust;

  public float GetThrust() => this.currentThrust;

  public void SendPower(float power) => this.availablePower = Mathf.Min(power, this.maxPower);

  public float GetRPM() => this.rpm;

  public float GetRPMRatio() => this.rpm / this.maxRPM;

  public float GetPower() => this.availablePower;

  public float GetMaxPower() => this.nominalPower;

  public void Throttle(float throttle)
  {
  }

  public void SetInteriorSounds(bool useInteriorSound)
  {
    if (!((UnityEngine.Object) this.fanSource != (UnityEngine.Object) null))
      return;
    if (this.fanSource.isPlaying)
    {
      if (useInteriorSound)
      {
        this.fanSource.Stop();
        this.fanSource.clip = this.interiorSound;
        this.fanSource.time = UnityEngine.Random.Range(0.0f, this.fanSource.clip.length);
        this.fanSource.Play();
      }
      else
      {
        this.fanSource.Stop();
        this.fanSource.clip = this.exteriorSound;
        this.fanSource.time = UnityEngine.Random.Range(0.0f, this.fanSource.clip.length);
        this.fanSource.Play();
      }
    }
    else if (useInteriorSound)
      this.fanSource.clip = this.interiorSound;
    else
      this.fanSource.clip = this.exteriorSound;
  }

  private void DuctedFan_OnDetached(UnitPart unitPart)
  {
    foreach (UnitPart criticalPart in this.criticalParts)
      criticalPart.onPartDetached -= new Action<UnitPart>(this.DuctedFan_OnDetached);
    this.inoperable = true;
    this.rpm *= 0.1f;
  }

  private void DuctedFan_OnDamage(UnitPart.OnApplyDamage e)
  {
    if (this.inoperable)
      return;
    Action onEngineDamage = this.OnEngineDamage;
    if (onEngineDamage != null)
      onEngineDamage();
    if (!e.detached && (double) e.hitPoints >= (double) this.criticalDamageThreshold)
      return;
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
    this.inoperable = true;
    foreach (UnitPart criticalPart in this.criticalParts)
      criticalPart.onApplyDamage -= new Action<UnitPart.OnApplyDamage>(this.DuctedFan_OnDamage);
    this.aircraft.RemoveIRSource(this.heatSource);
  }

  private void Animate()
  {
    this.meshFilter.mesh = (double) this.rpm * (double) Time.timeScale < (double) this.rpmThreshold ? this.slowMesh : this.fastMesh;
    this.meshRenderer.material = (double) this.rpm * (double) Time.timeScale < (double) this.rpmThreshold ? this.slowMaterial : this.fastMaterial;
    if (!((UnityEngine.Object) this.fanSource != (UnityEngine.Object) null))
      return;
    this.fanSource.pitch = this.rpm / this.maxRPM * this.pitch;
    this.fanSource.volume = (float) ((double) this.rpm / (double) this.maxRPM * ((double) this.unthrottledVolume + (double) Mathf.Abs(this.currentThrust) / (double) this.maxThrust * (double) this.throttledVolume));
  }

  private void FixedUpdate()
  {
    if (this.inoperable)
    {
      this.rpm -= this.rpm * this.bladeDrag * Mathf.Abs(this.currentThrust / this.maxThrust) * Time.fixedDeltaTime;
      this.availableThrust = 0.0f;
      this.availablePower = 0.0f;
    }
    else
    {
      this.reverseThrust = false;
      if ((UnityEngine.Object) this.rotorShaft != (UnityEngine.Object) null)
      {
        float num1 = this.trimThrust + this.controlInputs.yaw * this.yawCoef;
        if ((double) num1 < 0.0)
        {
          this.reverseThrust = true;
          num1 = Mathf.Abs(num1);
        }
        float num2 = Mathf.Abs(num1);
        float a = Mathf.Sqrt((float) ((double) num2 * (double) num2 * (double) num2 / (2.0 * (double) this.aircraft.airDensity * (double) this.area)));
        if ((double) a > 0.0)
          this.transmission.RequestPower((IPowerOutput) this, Mathf.Min(a, this.nominalPower));
        this.availableThrust = Mathf.Min(num1, this.maxThrust);
        this.rpm = this.rotorShaft.GetRPM() * this.gearing;
      }
      else
      {
        double num3 = ((double) this.controlInputs.customAxis1 - (double) this.idleThresholdPositive) / (1.0 - (double) this.idleThresholdPositive);
        float num4 = (this.idleThresholdNegative - this.controlInputs.customAxis1) / this.idleThresholdNegative;
        float f = Mathf.Clamp01((float) num3) - Mathf.Clamp01(num4) + this.controlInputs.yaw * this.yawCoef;
        this.transmission.RequestPower((IPowerOutput) this, Mathf.Clamp(this.nominalPower * Mathf.Max(Mathf.Abs(f), 0.025f), 0.0f, this.maxPower));
        if ((double) this.availablePower > (double) this.maxPower * (1.0 / 1000.0))
          this.rpm += this.spoolRate * Time.fixedDeltaTime;
        else
          this.rpm -= (float) (((double) this.rpm + 20.0) * (double) this.bladeDrag * (0.30000001192092896 + (double) Mathf.Abs(this.currentThrust / this.maxThrust))) * Time.fixedDeltaTime;
        this.availableThrust = Mathf.Pow((float) (2.0 * (double) this.aircraft.airDensity * (double) this.area * ((double) this.availablePower * (double) this.availablePower)), 0.3333f);
        if ((double) f < 0.0)
          this.availableThrust *= -1f;
        if ((double) Mathf.Abs(f) < 0.02500000037252903)
          this.availableThrust = 0.0f;
      }
      if (this.reverseThrust)
        this.availableThrust *= -1f;
      this.currentThrust = Mathf.SmoothDamp(this.currentThrust, this.availableThrust, ref this.currentThrustSmoothVel, this.responseTime);
      this.rpm = Mathf.Clamp(this.rpm, 0.0f, this.maxRPM);
      float t = (float) ((double) this.rpm * (double) this.rpm / ((double) this.maxRPM * (double) this.maxRPM));
      if (this.isHeatSource)
        this.heatSource.intensity = Mathf.Lerp(this.IRMin, this.IRMax, t);
      if (this.aircraft.LocalSim)
        this.unitPart.rb.AddForceAtPosition(this.thrustVector.forward * this.currentThrust * t, this.thrustVector.position);
      if (!((UnityEngine.Object) this.thrustDebug != (UnityEngine.Object) null))
        return;
      this.thrustDebug.transform.rotation = Quaternion.LookRotation(this.thrustVector.forward);
      this.thrustDebug.transform.localScale = Vector3.up + Vector3.right + Vector3.forward * this.currentThrust * (1f / 1000f);
    }
  }

  Transform IEngine.get_transform() => this.transform;
}

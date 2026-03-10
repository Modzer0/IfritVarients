// Decompiled with JetBrains decompiler
// Type: Turbojet
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;

#nullable disable
public class Turbojet : MonoBehaviour, IEngine, IThrustSource, IReportDamage
{
  public float maxThrust;
  private Vector3 nozzleAngles;
  public float damageFactor;
  private float thrust;
  private float parasiticThrustLoss;
  public bool engineFire;
  [SerializeField]
  private AudioSource turbineAudio;
  [SerializeField]
  private Vector3 thrustVectoring;
  [SerializeField]
  private Vector3 thrustVectoringGain = new Vector3(1f, 1f, 4f);
  [SerializeField]
  private Vector2 throttleRemap = new Vector2(0.0f, 1f);
  [SerializeField]
  private float thrustVectoringMaxAirspeed;
  [SerializeField]
  private float minDensity;
  [SerializeField]
  private float splitThrustFactor;
  [SerializeField]
  private AnimationCurve altitudeThrust;
  private bool operable = true;
  private float rpm;
  [SerializeField]
  private JetNozzle[] nozzles;
  [SerializeField]
  private Transform[] vectoringTransforms;
  [SerializeField]
  private float turbineMaxPitch;
  [SerializeField]
  private float minRPM;
  [SerializeField]
  private float maxRPM;
  [SerializeField]
  private float maxSpeed = 522f;
  [SerializeField]
  private float spoolRate;
  [SerializeField]
  private float startupRate;
  [SerializeField]
  private float fuelConsumptionMin;
  [SerializeField]
  private float fuelConsumptionMax;
  [SerializeField]
  private float damageThreshold;
  [SerializeField]
  private UnitPart[] criticalParts;
  [SerializeField]
  private string failureMessage;
  [SerializeField]
  private AudioClip failureMessageAudio;
  private ControlInputs controlInputs;
  private float lastFuelCheck;
  private float thrustRatio;
  private bool hasFuel;
  private bool afterburnerOn;
  private Aircraft aircraft;
  private float condition = 1f;
  private bool outOfSoundCone;

  public event Action<OnReportDamage> onReportDamage;

  public event Action OnEngineDisable;

  public event Action OnEngineDamage;

  private void Awake()
  {
    this.aircraft = this.criticalParts[0].parentUnit as Aircraft;
    this.aircraft.engineStates.Add((IEngine) this);
    this.aircraft.onInitialize += new Action(this.Turbojet_OnInitialize);
    this.controlInputs = this.aircraft.GetInputs();
    foreach (UnitPart criticalPart in this.criticalParts)
    {
      criticalPart.onApplyDamage += new Action<UnitPart.OnApplyDamage>(this.Turbojet_OnApplyDamage);
      criticalPart.onParentDetached += new Action<UnitPart>(this.Turbojet_OnPartDetach);
    }
  }

  private void Start() => SonicBoomManager.RegisterAircraft(this.aircraft);

  public float GetSpoolPercentage() => this.thrustRatio;

  public float GetMaxThrust()
  {
    float maxThrust = this.maxThrust;
    foreach (JetNozzle nozzle in this.nozzles)
      maxThrust += nozzle.GetMaxThrust();
    return maxThrust;
  }

  public float GetThrust()
  {
    float thrust = this.thrust;
    foreach (JetNozzle nozzle in this.nozzles)
      thrust += nozzle.GetTotalThrust();
    return thrust;
  }

  public float GetThrustRatio() => this.thrust / this.maxThrust;

  public float GetRPM() => this.rpm;

  public float GetRPMRatio() => this.rpm / this.maxRPM;

  public void SetParasiticLoss(float loss) => this.parasiticThrustLoss = loss;

  public void SetInteriorSounds(bool useInteriorSound)
  {
  }

  private void Turbojet_OnInitialize()
  {
    if ((double) this.aircraft.radarAlt <= (double) this.aircraft.definition.spawnOffset.y + 1.0)
      return;
    this.rpm = this.maxRPM;
  }

  private void Turbojet_OnPartDetach(UnitPart part)
  {
    if (!this.operable)
      return;
    this.KillEngine();
  }

  private void Turbojet_OnApplyDamage(UnitPart.OnApplyDamage e)
  {
    if (!this.operable)
      return;
    Action onEngineDamage = this.OnEngineDamage;
    if (onEngineDamage != null)
      onEngineDamage();
    this.condition = Mathf.Clamp((float) (((double) e.hitPoints - (double) this.damageThreshold) / (100.0 - (double) this.damageThreshold)), 0.0f, this.condition);
    if ((double) this.condition > 0.0)
      return;
    this.KillEngine();
  }

  private void KillEngine()
  {
    this.operable = false;
    foreach (UnitPart criticalPart in this.criticalParts)
    {
      if (!((UnityEngine.Object) criticalPart == (UnityEngine.Object) null))
      {
        criticalPart.onApplyDamage -= new Action<UnitPart.OnApplyDamage>(this.Turbojet_OnApplyDamage);
        criticalPart.onParentDetached -= new Action<UnitPart>(this.Turbojet_OnPartDetach);
      }
    }
    Action onEngineDisable = this.OnEngineDisable;
    if (onEngineDisable != null)
      onEngineDisable();
    Action<OnReportDamage> onReportDamage = this.onReportDamage;
    if (onReportDamage == null)
      return;
    onReportDamage(new OnReportDamage()
    {
      failureMessage = this.failureMessage,
      audioReport = this.failureMessageAudio
    });
  }

  private void Animate()
  {
    if ((double) this.rpm > 0.0 && !this.turbineAudio.isPlaying && !this.outOfSoundCone)
      this.turbineAudio.Play();
    this.turbineAudio.dopplerLevel = 1f;
    this.turbineAudio.pitch = this.rpm / this.maxRPM * this.turbineMaxPitch;
  }

  private void SpeedOfSoundEffects()
  {
    Vector3 from = this.transform.position - Camera.main.transform.position;
    bool flag = (double) from.magnitude < 20.0;
    bool outOfSoundCone = this.outOfSoundCone;
    this.outOfSoundCone = (double) this.aircraft.speed > 340.0 && !flag && (double) Vector3.Angle(from, this.aircraft.rb.velocity) > 15300.0 / (double) this.aircraft.speed;
    if (flag || outOfSoundCone == this.outOfSoundCone)
      return;
    if (this.outOfSoundCone)
      this.turbineAudio.Stop();
    else
      this.turbineAudio.Play();
  }

  private void UseFuel(float fuelConsumption)
  {
    this.lastFuelCheck = Time.timeSinceLevelLoad;
    this.hasFuel = this.aircraft.UseFuel(fuelConsumption);
  }

  private void FixedUpdate()
  {
    this.Animate();
    this.SpeedOfSoundEffects();
    bool flag = this.aircraft.Ignition && this.operable && (double) this.aircraft.airDensity > (double) this.minDensity && this.hasFuel;
    if ((((double) this.transform.position.y >= (double) Datum.LocalSeaY ? 0 : (this.operable ? 1 : 0)) & (flag ? 1 : 0)) != 0)
    {
      this.KillEngine();
      foreach (JetNozzle nozzle in this.nozzles)
        nozzle.CullDamageParticles();
    }
    float num1 = this.controlInputs.throttle;
    float num2 = 0.0f;
    this.thrust = 0.0f;
    float fuelConsumption = 0.0f;
    if ((double) Mathf.Abs(this.splitThrustFactor) > 0.0)
      num1 = Mathf.Clamp01(num1 + this.controlInputs.yaw * this.splitThrustFactor);
    if (flag)
    {
      num2 = Mathf.Lerp(this.minRPM, this.minRPM + (float) (((double) this.maxRPM - (double) this.minRPM) * ((double) this.condition * 0.5 + 0.5)), (float) (((double) num1 - (double) this.throttleRemap.x) / ((double) this.throttleRemap.y - (double) this.throttleRemap.x)));
      fuelConsumption = Mathf.Lerp(this.fuelConsumptionMin, this.fuelConsumptionMax, this.thrustRatio);
    }
    float a = Mathf.Clamp(num2 - this.rpm, -this.spoolRate, this.spoolRate);
    if ((double) this.rpm < (double) this.minRPM)
      a = Mathf.Min(a, this.startupRate);
    this.rpm += a * Time.deltaTime;
    this.thrustRatio = flag ? Mathf.Clamp((float) (((double) this.rpm - (double) this.minRPM * 0.85000002384185791) / ((double) this.maxRPM - (double) this.minRPM)), 0.0f, 1f) : 0.0f;
    this.thrust = Mathf.Max((this.maxThrust - this.parasiticThrustLoss) * this.thrustRatio, 0.0f);
    float num3 = -Mathf.Clamp(this.controlInputs.pitch * this.thrustVectoringGain.x, -1f, 1f) * this.thrustVectoring.x;
    float num4 = Mathf.Clamp(-this.controlInputs.yaw, -1f, 1f) * this.thrustVectoring.y;
    float num5 = num3 - Mathf.Clamp(this.controlInputs.roll * this.thrustVectoringGain.z, -1f, 1f) * this.thrustVectoring.z;
    if ((double) this.aircraft.radarAlt < 3.0)
      num4 = 0.0f;
    this.nozzleAngles.x += Mathf.Clamp(num5 - this.nozzleAngles.x, -70f * Time.deltaTime, 70f * Time.deltaTime);
    this.nozzleAngles.y += Mathf.Clamp(num4 - this.nozzleAngles.y, -70f * Time.deltaTime, 70f * Time.deltaTime);
    if ((double) this.aircraft.speed > (double) this.thrustVectoringMaxAirspeed)
      this.nozzleAngles = Vector3.zero;
    for (int index = 0; index < this.vectoringTransforms.Length; ++index)
      this.vectoringTransforms[index].transform.localEulerAngles = new Vector3(Mathf.Clamp(this.nozzleAngles.x + this.nozzleAngles.z, -20f, 20f), this.nozzleAngles.y, 0.0f);
    this.thrust *= this.altitudeThrust.Evaluate(this.transform.position.y - Datum.LocalSeaY);
    if ((double) this.aircraft.speed > (double) this.maxSpeed)
      this.thrust *= Mathf.Max((float) (1.0 - 5.0 * ((double) this.aircraft.speed - (double) this.maxSpeed) / (double) this.maxSpeed), 0.0f);
    float rpmRatio = this.rpm / this.maxRPM;
    foreach (JetNozzle nozzle in this.nozzles)
    {
      bool allowAfterburner = (double) this.parasiticThrustLoss <= 0.0 || (double) this.controlInputs.customAxis1 > 0.30000001192092896;
      nozzle.Thrust(this.thrust, rpmRatio, this.thrustRatio, this.controlInputs.throttle, allowAfterburner);
      fuelConsumption += nozzle.GetFuelConsumption();
    }
    if ((double) Time.timeSinceLevelLoad - (double) this.lastFuelCheck <= 1.0)
      return;
    this.UseFuel(fuelConsumption);
  }

  Transform IEngine.get_transform() => this.transform;
}

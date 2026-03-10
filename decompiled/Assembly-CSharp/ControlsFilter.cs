// Decompiled with JetBrains decompiler
// Type: ControlsFilter
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;
using UnityEngine.Serialization;

#nullable disable
public class ControlsFilter : MonoBehaviour
{
  [SerializeField]
  private float minSpeed = 25f;
  [SerializeField]
  private float minAlt = 1f;
  [SerializeField]
  protected string flightAssistName;
  [SerializeField]
  protected ControlsFilter.FlyByWire flyByWire;
  [SerializeField]
  protected ControlsFilter.AutoHover autoHover;
  [SerializeField]
  protected ControlsFilter.AimAssist aimAssist;
  [SerializeField]
  private bool flightAssistDefault;
  public bool ReverseThrust;
  protected bool autoHoverActive;
  protected AircraftParameters aircraftParameters;
  protected Aircraft aircraft;
  protected Autopilot autopilot;
  protected float gearDownSmoothed;
  protected float gearDownSmoothingVel;
  protected float landingSpeed;
  private GlobalPosition? hoverTarget;

  public event Action OnSetAutoHover;

  public bool HasAutoHover() => this.autoHover.Enabled;

  public void ToggleAutoHover()
  {
    if (!this.autoHover.Enabled || (double) this.aircraft.radarAlt < 1.0)
      return;
    this.autoHover.Toggle(this.aircraft);
    Action onSetAutoHover = this.OnSetAutoHover;
    if (onSetAutoHover == null)
      return;
    onSetAutoHover();
  }

  public void SetAutoHover(bool enabled)
  {
    if (!this.autoHover.Enabled)
      return;
    if ((double) this.aircraft.radarAlt < 1.0)
      enabled = false;
    this.autoHover.Set(this.aircraft, enabled);
    Action onSetAutoHover = this.OnSetAutoHover;
    if (onSetAutoHover == null)
      return;
    onSetAutoHover();
  }

  public bool IsAutoHoverEnabled() => this.autoHover.Active;

  public (bool, float[]) GetFlyByWireParameters() => this.flyByWire.GetParameters();

  public void SetFlyByWireParameters(bool enabled, float[] parameters)
  {
    this.flyByWire.ApplyParameters(enabled, parameters);
  }

  public void SetFlightAssist(bool enabled, Aircraft aircraft)
  {
    this.aircraft = aircraft;
    if (!this.HasFlightAssist() || !GameManager.IsLocalAircraft(aircraft))
      return;
    string report = enabled ? this.flightAssistName + " <b>Enabled</b>" : this.flightAssistName + " <b>Disabled</b>";
    SceneSingleton<AircraftActionsReport>.i.ReportText(report, 5f);
  }

  public bool HasFlightAssist() => !string.IsNullOrEmpty(this.flightAssistName);

  public bool FlightAssistDefault() => this.flightAssistDefault;

  public void GetAim(Unit target, out GlobalPosition? aimPoint, out GlobalPosition? impactPoint)
  {
    this.aimAssist.GetAim(target, this.aircraft, out aimPoint, out impactPoint);
  }

  public virtual void Filter(
    ControlInputs inputs,
    Vector3 rawInputs,
    Rigidbody rb,
    float gForce,
    bool flightAssist)
  {
    if ((double) this.aircraft.speed < (double) this.minSpeed || (double) this.aircraft.radarAlt < (double) this.minAlt)
      return;
    this.gearDownSmoothed = FastMath.SmoothDamp(this.gearDownSmoothed, !this.aircraft.gearDeployed || (double) this.aircraft.speed >= 140.0 ? 0.0f : 1f, ref this.gearDownSmoothingVel, 1f);
    Vector3 localAngularVelocity = rb.transform.InverseTransformDirection(rb.angularVelocity);
    if (this.autoHover.Enabled && this.autoHover.Active)
      this.autoHover.Hover(inputs, this.aircraft);
    if (this.aimAssist.Enabled & flightAssist)
      this.aimAssist.Assist(this.aircraft);
    if (!this.flyByWire.Enabled)
      return;
    this.flyByWire.Filter(this.aircraft, inputs, localAngularVelocity, flightAssist);
  }

  [Serializable]
  private class AutoTrimmer
  {
    public bool Enabled;
    [SerializeField]
    private float gLimit;
    [SerializeField]
    private float cornerSpeed;
    [SerializeField]
    private float maxRollRate;
    [SerializeField]
    private float maxRollRateSpeed;
    [SerializeField]
    private float aoaLimit;
    [SerializeField]
    private float aoaLimiterStrength;
    [SerializeField]
    private Vector3 correctionStrength;
    [SerializeField]
    private Vector3 trimP;
    [SerializeField]
    private Vector3 trimD;
    [SerializeField]
    private Vector3 trimLimit;
    [SerializeField]
    private LandingGear noseGear;
    private Vector3 trim;
    private Vector3 angularErrorPrev;

    public void Trim(Aircraft aircraft, ControlInputs inputs, Vector3 localAngularVelocity)
    {
      float num1 = 1.225f / aircraft.airDensity;
      float num2 = Mathf.Max(aircraft.speed * num1, 10f);
      inputs.pitch *= Mathf.Min(num2 * num2 / this.cornerSpeed * this.cornerSpeed, 1f);
      Vector3 vector3_1 = Vector3.Scale(new Vector3(this.gLimit * 9.81f / Mathf.Max(aircraft.speed, this.cornerSpeed), 0.0f, this.maxRollRate * Mathf.Clamp(aircraft.speed * num1 / this.maxRollRateSpeed, 0.2f, 1f)), new Vector3(inputs.pitch, inputs.yaw, inputs.roll));
      Vector3 b = localAngularVelocity - vector3_1;
      Vector3 a = (b - this.angularErrorPrev) * (1f / Time.fixedDeltaTime);
      this.angularErrorPrev = b;
      this.trim += Vector3.Scale(-b, this.trimP) - Vector3.Scale(a, this.trimD);
      this.trim = new Vector3(Mathf.Clamp(this.trim.x, -this.trimLimit.x, this.trimLimit.x), 0.0f, Mathf.Clamp(this.trim.z, -this.trimLimit.z, this.trimLimit.z));
      if (this.noseGear.WeightOnWheel(0.05f))
        this.trim = Vector3.zero;
      Vector3 vector3_2 = Vector3.Scale(this.correctionStrength, b) * Mathf.Min(this.cornerSpeed / num2, 1f);
      inputs.pitch += this.trim.x - vector3_2.x;
      inputs.yaw += this.trim.y - vector3_2.y;
      inputs.roll += this.trim.z - vector3_2.z;
    }
  }

  [Serializable]
  protected class FlyByWire
  {
    public bool Enabled;
    [SerializeField]
    private float gLimitPositive = 9f;
    [SerializeField]
    private float opacity = 1f;
    [FormerlySerializedAs("breakawaySpeed")]
    [SerializeField]
    private float cornerSpeed = 170f;
    [SerializeField]
    private float postStallManeuverSpeed = 180f;
    [SerializeField]
    private float maxRollSpeed = 300f;
    [SerializeField]
    private float takeoffSpeed = 40f;
    [SerializeField]
    private float pidTransitionSpeed = 150f;
    [SerializeField]
    private float maxPitchAngularVel = 1f;
    [SerializeField]
    private float maxRollAngularVel = 6f;
    [SerializeField]
    private float alphaLimiter = 25f;
    [SerializeField]
    private float alphaLimiterStrength = 0.05f;
    [SerializeField]
    private float pitchAdjusterLimitSlow = 0.5f;
    [SerializeField]
    private float pitchAdjusterLimitFast = 0.2f;
    [SerializeField]
    private float directControlFactor = 0.5f;
    [SerializeField]
    private float pFactorSlow = 4f;
    [SerializeField]
    private float pFactorFast = 2f;
    [SerializeField]
    private float dFactorSlow = 0.05f;
    [SerializeField]
    private float dFactorFast = 0.02f;
    [SerializeField]
    private float rollTrimRate = 0.1f;
    [SerializeField]
    private float rollTrimLimit = 0.1f;
    [SerializeField]
    private float yawTightness = 1f;
    [SerializeField]
    private float rollTightness = 1f;
    [SerializeField]
    private Vector3 inputSmoothing;
    [SerializeField]
    private LandingGear noseGear;
    private Vector3 inputSmoothingVel;
    private Vector3 inputsSmoothed;
    private float pPrev;
    private float pitchAdjuster;
    private float rollTrim;
    private float limitFactorSmoothed;
    private float timeAirborne;

    public (bool, float[]) GetParameters()
    {
      return (this.Enabled, new float[15]
      {
        this.directControlFactor,
        this.maxPitchAngularVel,
        this.cornerSpeed,
        this.postStallManeuverSpeed,
        this.pidTransitionSpeed,
        this.pitchAdjusterLimitSlow,
        this.pFactorSlow,
        this.dFactorSlow,
        this.pitchAdjusterLimitFast,
        this.pFactorFast,
        this.dFactorFast,
        this.rollTrimRate,
        this.rollTrimLimit,
        this.yawTightness,
        this.rollTightness
      });
    }

    public void ApplyParameters(bool enabled, float[] parameters)
    {
      this.Enabled = enabled;
      this.directControlFactor = parameters[0];
      this.maxPitchAngularVel = parameters[1];
      this.cornerSpeed = parameters[2];
      this.postStallManeuverSpeed = parameters[3];
      this.pidTransitionSpeed = parameters[4];
      this.pitchAdjusterLimitSlow = parameters[5];
      this.pFactorSlow = parameters[6];
      this.dFactorSlow = parameters[7];
      this.pitchAdjusterLimitFast = parameters[8];
      this.pFactorFast = parameters[9];
      this.dFactorFast = parameters[10];
      this.rollTrimRate = parameters[11];
      this.rollTrimLimit = parameters[12];
      this.yawTightness = parameters[13];
      this.rollTightness = parameters[14];
    }

    public void Filter(
      Aircraft aircraft,
      ControlInputs inputs,
      Vector3 localAngularVelocity,
      bool stabilityAssist)
    {
      float a = (float) ((double) aircraft.speed * (double) aircraft.airDensity / 1.2250000238418579);
      this.limitFactorSmoothed = Mathf.Lerp(this.limitFactorSmoothed, stabilityAssist || (double) a > (double) this.postStallManeuverSpeed ? 1f : 0.0f, Time.fixedDeltaTime);
      Vector3 vector3_1 = new Vector3(inputs.pitch, inputs.yaw, inputs.roll);
      if ((double) inputs.pitch > 0.0 && (double) localAngularVelocity.x < -0.10000000149011612)
        inputs.pitch = 0.0f;
      if ((double) inputs.pitch < 0.0 && (double) localAngularVelocity.x > 0.10000000149011612)
        inputs.pitch = 0.0f;
      if (this.inputSmoothing != Vector3.zero)
      {
        this.inputsSmoothed.x = Mathf.SmoothDamp(this.inputsSmoothed.x, inputs.pitch, ref this.inputSmoothingVel.x, this.inputSmoothing.x);
        this.inputsSmoothed.y = Mathf.SmoothDamp(this.inputsSmoothed.y, inputs.yaw, ref this.inputSmoothingVel.y, this.inputSmoothing.y);
        this.inputsSmoothed.z = Mathf.SmoothDamp(this.inputsSmoothed.z, inputs.roll, ref this.inputSmoothingVel.z, this.inputSmoothing.z);
        inputs.pitch = this.inputsSmoothed.x;
        inputs.roll = this.inputsSmoothed.y;
        inputs.roll = this.inputsSmoothed.z;
      }
      float num1 = (float) ((double) inputs.pitch * (double) this.gLimitPositive * 9.8100004196167) / Mathf.Max(a, this.cornerSpeed * 0.75f);
      if ((double) a < (double) this.cornerSpeed * 1.2999999523162842)
      {
        num1 *= Mathf.Clamp(a / this.cornerSpeed, 0.3f, 1f);
        Vector3 vector3_2 = Quaternion.Inverse(aircraft.transform.rotation) * aircraft.rb.velocity;
        float f = Mathf.Atan2(vector3_2.y, vector3_2.z) * 57.29578f;
        if ((double) Mathf.Abs(f) > (double) this.alphaLimiter && (double) Mathf.Sign(f) == (double) Mathf.Sign(num1))
        {
          float num2 = Mathf.Abs(f) - this.alphaLimiter;
          num1 *= (float) (1.0 - (double) Mathf.Clamp(num2, 0.0f, 10f) * (double) this.alphaLimiterStrength);
        }
      }
      float t1 = Mathf.InverseLerp(this.pidTransitionSpeed * 0.8f, this.pidTransitionSpeed * 1.2f, a);
      float num3 = Mathf.Lerp(inputs.pitch * this.maxPitchAngularVel, num1, this.limitFactorSmoothed);
      if ((double) aircraft.radarAlt < 0.20000000298023224)
      {
        if ((double) a < (double) this.takeoffSpeed)
          this.pitchAdjuster = Mathf.Clamp(this.pitchAdjuster, -0.2f, 0.2f);
        if (this.noseGear.WeightOnWheel(0.05f))
          num3 = Mathf.Clamp(num3, -0.2f, localAngularVelocity.x);
      }
      float num4 = Mathf.Lerp(this.pFactorSlow, this.pFactorFast, t1);
      float num5 = Mathf.Lerp(this.dFactorSlow, this.dFactorFast, t1);
      float num6 = localAngularVelocity.x - num3;
      float num7 = (num6 - this.pPrev) / Time.fixedDeltaTime;
      this.pPrev = num6;
      float num8 = (float) -((double) num6 * (double) num4 + (double) num7 * (double) num5);
      float max = Mathf.Lerp(this.pitchAdjusterLimitSlow, this.pitchAdjusterLimitFast, t1);
      this.pitchAdjuster += num8 * Time.fixedDeltaTime;
      this.pitchAdjuster = Mathf.Clamp(this.pitchAdjuster, -max, max);
      float t2 = Mathf.Clamp01((float) ((double) this.cornerSpeed * (double) this.cornerSpeed * 1.2250000238418579) / Mathf.Max(aircraft.airDensity * aircraft.speed * aircraft.speed, 50f));
      inputs.pitch = Mathf.Clamp(-num6 * this.directControlFactor * t2 + this.pitchAdjuster, -1f, 1f);
      if ((double) this.opacity < 1.0)
      {
        vector3_1.x *= t2;
        inputs.pitch = Mathf.Lerp(vector3_1.x, inputs.pitch, this.opacity);
        inputs.yaw = Mathf.Lerp(vector3_1.y, inputs.yaw, this.opacity);
        inputs.roll = Mathf.Lerp(vector3_1.z, inputs.roll, this.opacity);
      }
      float num9 = Mathf.Clamp((float) ((double) this.yawTightness * (double) t2 * ((double) localAngularVelocity.y - (double) inputs.yaw)), -1f, 1f);
      inputs.yaw = -num9;
      float num10 = localAngularVelocity.z - inputs.roll * -this.maxRollAngularVel * Mathf.Clamp(a / this.maxRollSpeed, 0.5f, 1f);
      if ((double) Mathf.Abs(localAngularVelocity.x) < 0.10000000149011612 && (double) aircraft.radarAlt > 0.5)
        this.rollTrim += Mathf.Clamp(Mathf.Clamp(num10, -0.1f, 0.1f), -this.rollTrimRate * Time.fixedDeltaTime, this.rollTrimRate * Time.fixedDeltaTime);
      this.rollTrim = Mathf.Clamp(this.rollTrim, -this.rollTrimLimit, this.rollTrimLimit);
      if ((double) a > (double) this.maxRollSpeed)
        inputs.roll *= (float) ((double) this.maxRollSpeed * (double) this.maxRollSpeed / ((double) a * (double) a));
      inputs.roll = Mathf.Lerp(inputs.roll, num10 * this.rollTightness, t2);
      inputs.roll = Mathf.Clamp(inputs.roll + this.rollTrim * t2, -1f, 1f);
    }
  }

  [Serializable]
  protected class AutoHover
  {
    public bool Enabled;
    public bool Active;
    [SerializeField]
    private bool setFlightAssistOff;
    [SerializeField]
    private float customAxis1Position;
    [SerializeField]
    private float errorGain = 0.1f;
    [SerializeField]
    private float sensitivity;
    [SerializeField]
    private float hoverBaseThrottle = 0.5f;
    [SerializeField]
    private float climbSensitivity = 8f;
    [SerializeField]
    private float customAxisSlowDown;
    [SerializeField]
    private float correctionStrength = 0.5f;
    [SerializeField]
    private float maxSpeed = 30f;
    [SerializeField]
    private PIDFactors attitudePIDFactors;
    [SerializeField]
    private PIDFactors altitudePIDFactors;
    [SerializeField]
    private float lastShipCheck;
    public bool storedFlightAssistState;
    private float throttleOverride;
    private PID pitchPID;
    private PID rollPID;
    private PID altitudePID;
    private AircraftParameters aircraftParameters;
    protected Vector3 surfaceVelocity;

    private void CheckNearbyShip(FactionHQ faction, GlobalPosition position)
    {
      if ((double) Time.timeSinceLevelLoad - (double) this.lastShipCheck < 3.0)
        return;
      this.lastShipCheck = Time.timeSinceLevelLoad;
      Ship nearestShip;
      float nearestDistance;
      if ((UnityEngine.Object) faction != (UnityEngine.Object) null && faction.TryGetNearestShip(position, out nearestShip, out nearestDistance) && (double) nearestDistance < 250000.0)
        this.surfaceVelocity = nearestShip.rb.velocity;
      else
        this.surfaceVelocity = Vector3.zero;
    }

    public void Hover(ControlInputs inputs, Aircraft aircraft)
    {
      if (!this.Enabled || !this.Active)
        return;
      if ((double) aircraft.radarAlt < 0.20000000298023224)
      {
        aircraft.GetControlsFilter().SetAutoHover(false);
      }
      else
      {
        this.CheckNearbyShip(aircraft.NetworkHQ, aircraft.GlobalPosition());
        if (this.pitchPID == null)
        {
          this.aircraftParameters = aircraft.GetAircraftParameters();
          this.pitchPID = new PID(this.attitudePIDFactors);
          this.rollPID = new PID(this.attitudePIDFactors);
          this.altitudePID = new PID(this.altitudePIDFactors);
        }
        Vector3 surfaceVelocity = this.surfaceVelocity with
        {
          y = 0.0f
        };
        Vector3 vector3 = new Vector3(aircraft.rb.velocity.x, 0.0f, aircraft.rb.velocity.z) - surfaceVelocity;
        float num1 = Vector3.Dot(vector3, aircraft.transform.forward);
        Vector3 other = Vector3.up - Vector3.ClampMagnitude(vector3, Mathf.Sqrt(Mathf.Min(vector3.magnitude, this.maxSpeed))) * this.errorGain;
        float angleOnAxis1 = TargetCalc.GetAngleOnAxis(aircraft.transform.up, other, aircraft.transform.right);
        float angleOnAxis2 = TargetCalc.GetAngleOnAxis(aircraft.transform.up, other, -aircraft.transform.forward);
        float num2 = Mathf.Clamp(this.pitchPID.GetOutput(angleOnAxis1 * this.sensitivity, 1f, Time.fixedDeltaTime, new Vector3(this.attitudePIDFactors.P, this.attitudePIDFactors.I, this.attitudePIDFactors.D)), -this.correctionStrength, this.correctionStrength);
        float num3 = Mathf.Clamp(this.rollPID.GetOutput(angleOnAxis2 * this.sensitivity, 1f, Time.fixedDeltaTime, new Vector3(this.attitudePIDFactors.P, this.attitudePIDFactors.I, this.attitudePIDFactors.D)), -this.correctionStrength, this.correctionStrength);
        inputs.pitch = Mathf.Clamp(inputs.pitch + num2, -1f, 1f);
        inputs.roll = Mathf.Clamp(inputs.roll + num3, -1f, 1f);
        inputs.customAxis1 = Mathf.Clamp01(this.customAxis1Position + num1 * this.customAxisSlowDown);
        float num4 = (inputs.throttle - this.hoverBaseThrottle) * this.climbSensitivity;
        if ((double) Mathf.Abs(inputs.throttle - this.hoverBaseThrottle) < 0.10000000149011612)
          num4 = 0.0f;
        this.throttleOverride = Mathf.Clamp01(this.throttleOverride + this.altitudePID.GetOutput(-(aircraft.rb.velocity.y - num4), 2f, Time.fixedDeltaTime, new Vector3(this.altitudePIDFactors.P, this.altitudePIDFactors.I, this.altitudePIDFactors.D)));
        inputs.throttle = this.throttleOverride;
      }
    }

    public void Set(Aircraft aircraft, bool enabled) => this.Active = enabled;

    public void Toggle(Aircraft aircraft)
    {
      this.Active = !this.Active;
      if (this.setFlightAssistOff)
      {
        if (this.Active)
        {
          this.storedFlightAssistState = aircraft.flightAssist;
          aircraft.SetFlightAssist(false);
        }
        else
          aircraft.SetFlightAssist(this.storedFlightAssistState);
      }
      if (this.Active)
        this.throttleOverride = aircraft.GetInputs().throttle;
      if (!GameManager.IsLocalAircraft(aircraft))
        return;
      string report = this.Active ? "Auto Hover <b>Enabled</b>" : "Auto Hover <b>Disabled</b>";
      SceneSingleton<AircraftActionsReport>.i.ReportText(report, 5f);
    }
  }

  [Serializable]
  protected class GLimiter
  {
    public bool Enabled;
    [SerializeField]
    private float gLimit;
    [SerializeField]
    private float limitStrength = 1f;
    [SerializeField]
    private float predictionStrength = 0.5f;
    [SerializeField]
    private float predictionTime = 1f;
    [SerializeField]
    private float smoothing = 0.05f;
    [SerializeField]
    private float rollonRate = 1f;
    [SerializeField]
    private float rolloffRate = 0.1f;
    private float gPrev;
    private float gRateSmoothed;
    private float smoothingVel;
    private float limiterStrength;
    private float inputMagnitudeAtOverG;

    public void LimitG(ControlInputs inputs, Aircraft aircraft, float inverseDynamicPressure)
    {
      if (!this.Enabled)
      {
        this.gPrev = 0.0f;
      }
      else
      {
        double num1 = (double) Mathf.Clamp01(1f - inverseDynamicPressure);
        float f = Vector3.Dot(aircraft.accel + Vector3.up * 1f, aircraft.transform.up);
        this.gRateSmoothed = FastMath.SmoothDamp(this.gRateSmoothed, (f - this.gPrev) / Time.fixedDeltaTime, ref this.smoothingVel, this.smoothing);
        float num2 = Mathf.Abs(f) - this.gLimit;
        float num3 = Mathf.Abs(f + this.gRateSmoothed * this.predictionTime) - this.gLimit;
        if ((double) num2 + (double) num3 > 0.0)
          this.limiterStrength += (float) ((double) num2 * (double) this.limitStrength + (double) num3 * (double) this.predictionStrength) * this.rollonRate * Time.fixedDeltaTime;
        else
          this.limiterStrength -= this.rolloffRate * Time.fixedDeltaTime;
        this.inputMagnitudeAtOverG = (double) num2 <= 0.0 ? Mathf.Lerp(this.inputMagnitudeAtOverG, 1f, 0.5f * Time.fixedDeltaTime) : Mathf.Abs(inputs.pitch);
        this.limiterStrength = Mathf.Clamp01(this.limiterStrength);
        if ((double) this.gPrev != 0.0)
          inputs.pitch *= 1f - this.limiterStrength;
        inputs.pitch = Mathf.Clamp(inputs.pitch, -this.inputMagnitudeAtOverG, this.inputMagnitudeAtOverG);
        this.gPrev = f;
      }
    }
  }

  [Serializable]
  protected class AngularVelocityDamper
  {
    public bool Enabled;
    [SerializeField]
    private float pitchDamping;
    [SerializeField]
    private float pitchDampingLimit;
    [SerializeField]
    private float rollDamping;
    [SerializeField]
    private float rollDampingLimit;
    [SerializeField]
    private float yawDamping;
    [SerializeField]
    private float yawDampingLimit;
    [SerializeField]
    private float yawDampingGround;
    [Range(0.0f, 2f)]
    [SerializeField]
    private float assistOffOpacity;
    [Range(0.0f, 2f)]
    [SerializeField]
    private float assistOnOpacity;
    [Range(1f, 3f)]
    [SerializeField]
    private float gearDownMultiplier;

    public void DampAngularVelocity(
      ControlInputs inputs,
      Vector3 localAngularVel,
      float inverseDynamicPressure,
      bool assist,
      float gearDownFactor,
      bool onGround)
    {
      if (!this.Enabled)
        return;
      float num = (assist ? this.assistOnOpacity : this.assistOffOpacity) * Mathf.Lerp(1f, this.gearDownMultiplier, gearDownFactor);
      inputs.pitch -= this.pitchDamping * localAngularVel.x * Mathf.Min(inverseDynamicPressure, this.pitchDampingLimit) * num;
      inputs.yaw -= (onGround ? this.yawDampingGround : this.yawDamping) * localAngularVel.y * Mathf.Min(inverseDynamicPressure, this.yawDampingLimit);
      inputs.roll -= -this.rollDamping * localAngularVel.z * Mathf.Min(inverseDynamicPressure, this.rollDampingLimit);
    }
  }

  [Serializable]
  protected class SpeedRemap
  {
    public bool Enabled;
    [SerializeField]
    private float referenceSpeed = 160f;
    [SerializeField]
    private float zeroEffectSpeed = 200f;
    [SerializeField]
    private float fullEffectSpeed = 340f;
    [SerializeField]
    private bool remapRoll;

    public void Remap(ControlInputs inputs, float dynamicPressure, float speed)
    {
      float num1 = Mathf.Clamp01((float) ((double) this.referenceSpeed * (double) this.referenceSpeed * 1.2000000476837158) / dynamicPressure);
      float num2 = (float) ((double) this.zeroEffectSpeed * (double) this.zeroEffectSpeed * 1.2000000476837158);
      float num3 = (float) ((double) this.fullEffectSpeed * (double) this.fullEffectSpeed * 1.2000000476837158);
      float t = (float) (((double) dynamicPressure - (double) num2) / ((double) num3 - (double) num2));
      inputs.pitch = Mathf.Lerp(inputs.pitch, inputs.pitch * num1, t);
      if (!this.remapRoll)
        return;
      inputs.roll = Mathf.Lerp(inputs.roll, inputs.roll * num1, t);
    }
  }

  [Serializable]
  protected class ResponseRateLimiter
  {
    public bool Enabled;
    [SerializeField]
    private float pitchRateCenter = 1f;
    [SerializeField]
    private float pitchRateMid = 0.5f;
    [SerializeField]
    private float pitchRateLimits = 0.1f;
    private float pitchPrev;

    public void LimitResponseRate(ControlInputs inputs)
    {
      if (!this.Enabled)
        return;
      float pitchRateCenter = this.pitchRateCenter;
      float num1 = Mathf.Abs(this.pitchPrev);
      float num2 = (double) num1 >= 0.5 ? Mathf.Lerp(this.pitchRateMid, this.pitchRateLimits, (float) ((double) num1 * 2.0 - 1.0)) : Mathf.Lerp(this.pitchRateCenter, this.pitchRateMid, num1 * 2f);
      float num3 = inputs.pitch - this.pitchPrev;
      if ((double) inputs.pitch > 0.0 && (double) num3 > 0.0)
        inputs.pitch = Mathf.Min(inputs.pitch, this.pitchPrev + num2 * Time.fixedDeltaTime);
      if ((double) inputs.pitch < 0.0 && (double) num3 < 0.0)
        inputs.pitch = Mathf.Max(inputs.pitch, this.pitchPrev - num2 * Time.fixedDeltaTime);
      this.pitchPrev = inputs.pitch;
    }
  }

  [Serializable]
  protected class AoALimiter
  {
    public bool Enabled;
    [SerializeField]
    private float maxAlpha = 30f;
    [SerializeField]
    private float pGain = 2f;
    [SerializeField]
    private float dGain = 0.1f;
    [SerializeField]
    private float minSpeed = 60f;
    private float alphaPrev;

    public void LimitAoA(ControlInputs inputs, Aircraft aircraft, bool assist)
    {
      if (!this.Enabled || !assist || (double) aircraft.speed < (double) this.minSpeed)
        return;
      Vector3 vector3 = aircraft.cockpit.transform.InverseTransformDirection(aircraft.cockpit.rb.velocity);
      float num1 = Mathf.Max(Mathf.Atan2(vector3.y, vector3.z) * -57.29578f, 0.0f);
      float num2 = this.pGain * (num1 - this.maxAlpha) / this.maxAlpha;
      if ((double) num2 > 0.0)
        inputs.pitch += Mathf.Clamp01(num2);
      this.alphaPrev = num1;
    }
  }

  [Serializable]
  protected class AimAssist
  {
    public bool Enabled;
    private Vector3 correction;
    private Vector3 smoothedCorrection;
    private Vector3 correctionSmoothingVel;
    private Vector3 lead;
    private Vector3 targetVector;
    private Vector3 ccipVector;
    private Vector3 targetVelPrev;
    private Vector3 targetAccelSmoothed;
    private Vector3 targetAccelSmoothingVel;
    private GlobalPosition? accurateAimpoint;
    private GlobalPosition? accurateImpactPoint;
    private float lastSim;
    private float lastAimRequest;
    private float targetDist;
    private float targetSpeed;
    private Unit currentTarget;
    [SerializeField]
    private PIDFactors pitchPID;
    [SerializeField]
    private PIDFactors yawPID;
    [SerializeField]
    private PIDFactors rollPID;
    private PID xPID;
    private PID yPID;
    private PID zPID;
    private ControlInputs inputs;

    public void GetAim(
      Unit target,
      Aircraft aircraft,
      out GlobalPosition? aimPoint,
      out GlobalPosition? impactPoint)
    {
      if ((UnityEngine.Object) target != (UnityEngine.Object) this.currentTarget)
      {
        this.currentTarget = target;
        if (this.inputs == null)
        {
          this.inputs = aircraft.GetInputs();
          this.xPID = new PID(this.pitchPID);
          this.yPID = new PID(this.yawPID);
          this.zPID = new PID(this.rollPID);
        }
      }
      this.CalcAim(target, aircraft);
      aimPoint = this.accurateAimpoint;
      impactPoint = this.accurateImpactPoint;
    }

    public void CalcAim(Unit target, Aircraft aircraft)
    {
      this.accurateAimpoint = new GlobalPosition?();
      this.accurateImpactPoint = new GlobalPosition?();
      WeaponStation currentWeaponStation = aircraft.weaponManager.currentWeaponStation;
      GlobalPosition knownPosition;
      if ((double) currentWeaponStation.WeaponInfo.muzzleVelocity == 0.0 || currentWeaponStation.HasTurret() || !aircraft.NetworkHQ.TryGetKnownPosition(target, out knownPosition) || FastMath.OutOfRange(knownPosition, aircraft.GlobalPosition(), currentWeaponStation.WeaponInfo.targetRequirements.maxRange))
        return;
      Weapon weapon = currentWeaponStation.Weapons[0];
      if ((double) Vector3.Angle(weapon.transform.forward, knownPosition - aircraft.GlobalPosition()) > 40.0)
        return;
      this.lastAimRequest = Time.timeSinceLevelLoad;
      GlobalPosition globalPosition = weapon.transform.GlobalPosition();
      this.targetDist = FastMath.Distance(knownPosition, globalPosition);
      float num = this.targetDist / weapon.info.muzzleVelocity;
      this.targetSpeed = target.speed;
      Vector3 vector3_1 = (double) this.targetSpeed < 1.0 ? Vector3.zero : target.rb.velocity;
      Vector3 target1 = (vector3_1 - this.targetVelPrev) / Time.fixedDeltaTime;
      this.targetVelPrev = vector3_1;
      this.targetAccelSmoothed = Vector3.SmoothDamp(this.targetAccelSmoothed, target1, ref this.targetAccelSmoothingVel, 0.25f);
      Vector3 vector3_2 = num * vector3_1 + 0.5f * num * num * this.targetAccelSmoothed;
      GlobalPosition targetPos = globalPosition + weapon.transform.forward * this.targetDist;
      if ((double) Time.timeSinceLevelLoad - (double) this.lastSim > 0.10000000149011612)
      {
        this.lastSim = Time.timeSinceLevelLoad;
        Vector3 initialVelocity = aircraft.rb.velocity + weapon.transform.forward * currentWeaponStation.WeaponInfo.muzzleVelocity;
        this.correction = Kinematics.TrajectorySim(currentWeaponStation.WeaponInfo, initialVelocity, globalPosition, targetPos, Vector3.zero, Vector3.zero, 0.1f, out float _);
      }
      this.smoothedCorrection = Vector3.SmoothDamp(this.smoothedCorrection, this.correction, ref this.correctionSmoothingVel, 0.15f);
      this.accurateAimpoint = new GlobalPosition?(knownPosition + vector3_2 - this.smoothedCorrection);
      this.accurateImpactPoint = new GlobalPosition?(targetPos + this.smoothedCorrection - vector3_2);
      this.targetVector = knownPosition - globalPosition;
      this.ccipVector = this.accurateImpactPoint.Value - globalPosition;
    }

    public void Assist(Aircraft aircraft)
    {
      if ((double) Time.timeSinceLevelLoad - (double) this.lastAimRequest > 0.20000000298023224)
      {
        if (!((UnityEngine.Object) this.currentTarget != (UnityEngine.Object) null))
          return;
        this.currentTarget = (Unit) null;
        if (this.inputs == null)
          return;
        this.xPID.Reseti();
        this.yPID.Reseti();
        this.zPID.Reseti();
      }
      else
      {
        float angleOnAxis1 = TargetCalc.GetAngleOnAxis(this.ccipVector, this.targetVector, aircraft.transform.right);
        float angleOnAxis2 = TargetCalc.GetAngleOnAxis(this.ccipVector, this.targetVector, aircraft.transform.up);
        float num = (float) (2.0 - ((double) angleOnAxis1 + (double) angleOnAxis2) * 0.25) * (float) ((double) this.targetDist * (1.0 / 1000.0) - 1.0);
        if ((double) this.targetSpeed >= 40.0 || (double) Mathf.Abs(angleOnAxis1) >= 2.0 || (double) Mathf.Abs(angleOnAxis2) >= 2.0 || (double) this.targetDist <= 500.0)
          return;
        float t = Mathf.Clamp01(num);
        this.inputs.pitch *= Mathf.Lerp(1f, 0.2f, t);
        this.inputs.yaw *= Mathf.Lerp(1f, 0.3f, t);
        Vector3 other = Vector3.up + aircraft.transform.right * angleOnAxis2 * 0.1f;
        float currentError = -TargetCalc.GetAngleOnAxis(aircraft.transform.up, other, aircraft.transform.forward);
        float output1 = this.xPID.GetOutput(angleOnAxis1, Time.fixedDeltaTime);
        float output2 = this.yPID.GetOutput(angleOnAxis2, Time.fixedDeltaTime);
        float output3 = this.zPID.GetOutput(currentError, Time.fixedDeltaTime);
        this.inputs.pitch += Mathf.Clamp(output1 * t, -0.1f, 0.1f);
        this.inputs.yaw += Mathf.Clamp(output2 * t, -0.1f, 0.1f);
        this.inputs.roll += Mathf.Clamp(output3 * t, -0.05f, 0.05f);
      }
    }
  }
}

// Decompiled with JetBrains decompiler
// Type: DuctedThrustSystem
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;

#nullable disable
public class DuctedThrustSystem : MonoBehaviour, INozzleGauge
{
  private DuctedThrustSystem.DuctedThrustMode mode;
  [SerializeField]
  private AnimationCurve thrustAtAirspeed;
  [SerializeField]
  private Aircraft aircraft;
  [SerializeField]
  private JetNozzle[] nozzles;
  [SerializeField]
  private Turbojet[] turbojets;
  [SerializeField]
  private float swivelRate;
  [SerializeField]
  private DuctedThrustSystem.IntakeDoor[] intakeDoors;
  [SerializeField]
  private float intakeDoorSpeedThreshold;
  [HideInInspector]
  public float angle;
  private bool intakeDoorsOpen;
  private bool safeMode;
  private bool auto;
  private float lastAirborne = -100f;
  private float takeoffSpeed;
  private float angleTarget;
  private float thrustRatio;
  private ControlInputs inputs;

  private void Awake()
  {
    this.inputs = this.aircraft.GetInputs();
    this.inputs.customAxis1 = 1f;
    this.aircraft.onSetFlightAssist += new Action<Aircraft.OnFlightAssistToggle>(this.DuctedThrustSystem_OnSetFlightAssist);
    this.takeoffSpeed = this.aircraft.GetAircraftParameters().takeoffSpeed;
  }

  public float GetNozzleAngle() => this.angle;

  private void DuctedThrustSystem_OnEngineDisable() => this.safeMode = true;

  private void DuctedThrustSystem_OnSetFlightAssist(Aircraft.OnFlightAssistToggle e)
  {
    this.auto = e.enabled && (UnityEngine.Object) this.aircraft.Player != (UnityEngine.Object) null;
  }

  public float GetMaxThrust() => this.thrustAtAirspeed.Evaluate(0.0f);

  private void AutoAngle()
  {
    int mode1 = (int) this.mode;
    float num1 = this.aircraft.speed / this.takeoffSpeed;
    float num2 = this.aircraft.radarAlt - this.aircraft.definition.spawnOffset.y;
    if ((double) num2 > 1.0)
      this.lastAirborne = Time.timeSinceLevelLoad;
    if (this.aircraft.gearDeployed)
    {
      if ((double) num1 < 0.10000000149011612 && ((double) num2 < 1.0 || (double) this.inputs.throttle > 0.10000000149011612))
        this.mode = DuctedThrustSystem.DuctedThrustMode.Forward;
      if ((double) num1 > 0.34999999403953552 && (double) this.inputs.throttle > 0.89999997615814209)
        this.mode = DuctedThrustSystem.DuctedThrustMode.Takeoff;
      if ((double) num1 > 0.5 && (double) this.inputs.throttle < 0.89999997615814209)
        this.mode = DuctedThrustSystem.DuctedThrustMode.Reverse;
      if ((double) num1 < 0.5 && (double) Time.timeSinceLevelLoad - (double) this.lastAirborne < 8.0)
        this.mode = DuctedThrustSystem.DuctedThrustMode.Hover;
    }
    else
    {
      if ((double) this.inputs.throttle > 0.5)
        this.mode = DuctedThrustSystem.DuctedThrustMode.Forward;
      if ((double) this.inputs.throttle < 0.5)
        this.mode = DuctedThrustSystem.DuctedThrustMode.Reverse;
    }
    int mode2 = (int) this.mode;
    if (mode1 != mode2 && (UnityEngine.Object) this.aircraft == (UnityEngine.Object) SceneSingleton<CombatHUD>.i.aircraft)
      SceneSingleton<AircraftActionsReport>.i.ReportText($"Vectoring Mode set to {this.mode}", 4f);
    if (this.mode == DuctedThrustSystem.DuctedThrustMode.Forward)
      this.angleTarget = 0.0f;
    if (this.mode == DuctedThrustSystem.DuctedThrustMode.Takeoff)
      this.angleTarget = 45f;
    if (this.mode == DuctedThrustSystem.DuctedThrustMode.Hover)
      this.angleTarget = 95f;
    if (this.mode == DuctedThrustSystem.DuctedThrustMode.Reverse)
      this.angleTarget = 120f;
    this.inputs.customAxis1 = (float) (1.0 - (double) this.angleTarget / 120.0);
  }

  private void Swivel()
  {
    Vector3 forward = this.aircraft.transform.forward with
    {
      y = 0.0f
    };
    float num1 = Vector3.Dot(this.aircraft.rb.velocity, this.aircraft.transform.forward);
    float num2 = Mathf.Lerp(Mathf.Lerp((float) (0.15000000596046448 * -(double) TargetCalc.GetAngleOnAxis(this.aircraft.transform.forward, forward, this.aircraft.transform.right)), (float) (0.20000000298023224 * -(double) TargetCalc.GetAngleOnAxis(this.aircraft.transform.forward, this.aircraft.rb.velocity, this.aircraft.transform.right)), num1 / this.takeoffSpeed) + (float) (((double) this.inputs.throttle - 0.800000011920929) * 0.0099999997764825821) * num1, (double) this.inputs.throttle < 0.5 ? -1f : 2f, Mathf.Clamp01((float) ((double) num1 / (double) this.takeoffSpeed - 0.30000001192092896)));
    float num3 = (double) this.aircraft.radarAlt < (double) this.aircraft.definition.spawnOffset.y + 1.0 ? this.inputs.pitch * 4f : 0.0f;
    double num4 = (double) Mathf.Clamp01((float) ((double) this.aircraft.speed / (double) this.takeoffSpeed * 2.0));
    Vector3 vector3 = Vector3.down - this.aircraft.transform.forward * (num3 + num2);
    if (this.aircraft.LocalSim && this.auto && (UnityEngine.Object) this.aircraft.Player != (UnityEngine.Object) null && !this.aircraft.IsAutoHoverEnabled())
    {
      this.AutoAngle();
      if ((double) this.aircraft.speed > (double) this.takeoffSpeed)
        this.inputs.customAxis1 += this.inputs.pitch * 0.5f;
    }
    this.angleTarget = (float) ((1.0 - (double) this.inputs.customAxis1) * 120.0);
    this.angle += Mathf.Clamp(this.angleTarget - this.angle, -40f * Time.deltaTime, 40f * Time.deltaTime);
    this.angle = Mathf.Clamp(this.angle, 0.0f, 120f);
    Vector3 aimDirection = -this.aircraft.transform.forward * Mathf.Cos(this.angle * ((float) Math.PI / 180f)) + -this.aircraft.transform.up * Mathf.Sin(this.angle * ((float) Math.PI / 180f));
    this.thrustRatio = 0.0f;
    float num5 = 0.0f;
    foreach (Turbojet turbojet in this.turbojets)
    {
      this.thrustRatio += turbojet.GetThrustRatio();
      num5 += turbojet.GetRPMRatio();
    }
    this.thrustRatio /= (float) this.turbojets.Length;
    float rpmRatio = num5 / (float) this.turbojets.Length;
    float num6 = this.thrustRatio * this.thrustAtAirspeed.Evaluate(this.aircraft.speed);
    float num7 = 0.0f;
    foreach (JetNozzle nozzle in this.nozzles)
      num7 += nozzle.GetPriority(this.inputs);
    foreach (JetNozzle nozzle in this.nozzles)
    {
      nozzle.Aim(this.inputs, aimDirection);
      if ((double) num7 != 0.0)
      {
        float thrustAmount = nozzle.priority / num7 * num6;
        nozzle.Thrust(thrustAmount, rpmRatio, this.thrustRatio, this.inputs.throttle, false);
      }
    }
  }

  private void Update()
  {
    if ((double) this.aircraft.speed < (double) this.intakeDoorSpeedThreshold && !this.intakeDoorsOpen)
    {
      this.intakeDoorsOpen = true;
      foreach (DuctedThrustSystem.IntakeDoor intakeDoor in this.intakeDoors)
        intakeDoor.Open();
    }
    if ((double) this.aircraft.speed > (double) this.intakeDoorSpeedThreshold && this.intakeDoorsOpen)
    {
      this.intakeDoorsOpen = false;
      foreach (DuctedThrustSystem.IntakeDoor intakeDoor in this.intakeDoors)
        intakeDoor.Close();
    }
    foreach (DuctedThrustSystem.IntakeDoor intakeDoor in this.intakeDoors)
      intakeDoor.Animate(this.thrustRatio);
  }

  private void FixedUpdate() => this.Swivel();

  private enum DuctedThrustMode
  {
    Forward,
    Takeoff,
    Hover,
    Reverse,
  }

  [Serializable]
  private class IntakeDoor
  {
    private bool opening;
    private bool closing;
    [SerializeField]
    private Transform hinge;
    [SerializeField]
    private float speed;
    [SerializeField]
    private Vector3 openAngle;
    [SerializeField]
    private AudioSource intakeSound;
    [SerializeField]
    private float pitchMin;
    [SerializeField]
    private float pitchMax;
    [SerializeField]
    [Range(0.0f, 2f)]
    private float volumeMultiplier;
    private float openAmount;

    public void Open()
    {
      this.opening = true;
      this.closing = false;
    }

    public void Close()
    {
      this.opening = false;
      this.closing = true;
    }

    public void Animate(float thrustRatio)
    {
      if ((UnityEngine.Object) this.intakeSound != (UnityEngine.Object) null)
      {
        if ((double) this.openAmount > 0.0)
        {
          this.intakeSound.volume = this.openAmount * thrustRatio * this.volumeMultiplier;
          this.intakeSound.pitch = Mathf.Lerp(this.pitchMin, this.pitchMax, this.openAmount * thrustRatio);
          if (!this.intakeSound.isPlaying)
            this.intakeSound.Play();
        }
        else if (this.intakeSound.isPlaying)
          this.intakeSound.Stop();
      }
      if (!this.opening && !this.closing)
        return;
      this.openAmount += this.opening ? this.speed * Time.deltaTime : -this.speed * Time.deltaTime;
      if ((double) this.openAmount > 1.0 || (double) this.openAmount < 0.0)
      {
        this.opening = false;
        this.closing = false;
        this.openAmount = Mathf.Clamp01(this.openAmount);
      }
      this.hinge.localEulerAngles = this.openAngle * this.openAmount;
    }
  }
}

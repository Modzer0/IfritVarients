// Decompiled with JetBrains decompiler
// Type: SwivelDuctSystem
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Rewired;
using System;
using UnityEngine;

#nullable disable
public class SwivelDuctSystem : MonoBehaviour, INozzleGauge
{
  private SwivelDuctSystem.SwivelDuctMode swivelDuctMode;
  [SerializeField]
  private Aircraft aircraft;
  [SerializeField]
  private SwivelDuctSystem.Bearing[] bearings;
  [SerializeField]
  private SwivelDuctSystem.LiftFan[] liftFans;
  [SerializeField]
  private SwivelDuctSystem.MovingPanel[] movingPanels;
  [SerializeField]
  private SwivelDuctSystem.ThrustDoor[] thrustDoors;
  [SerializeField]
  private SwivelDuctSystem.Thruster[] thrusters;
  [SerializeField]
  private float maxSwivelAirspeed = 139f;
  [SerializeField]
  private float swivelSpeed;
  [SerializeField]
  private float thrustDoorSpeed;
  [SerializeField]
  private float shortLandingSpeed;
  [SerializeField]
  private float shortTakeoffSpeed;
  private float swivelPosition;
  private float swivelPositionPrev;
  private float thrustDoorPosition;
  private ControlInputs inputs;
  private float timeOnGround;
  private float timeAirborne;
  private Player player;

  public float GetNozzleAngle() => this.swivelPosition * 90f;

  private void CheckForManualInput(bool autoHoverEnabled)
  {
    if ((UnityEngine.Object) this.aircraft.Player == (UnityEngine.Object) null)
    {
      this.swivelDuctMode = SwivelDuctSystem.SwivelDuctMode.Manual;
    }
    else
    {
      if (autoHoverEnabled || !GameManager.IsLocalAircraft(this.aircraft))
        return;
      float f = Mathf.Clamp(this.player.GetAxisRaw("Custom Axis 1"), -1f, 1f);
      float num1 = Mathf.Clamp(this.player.GetAxisRawPrev("Custom Axis 1"), -1f, 1f);
      float num2 = Mathf.Abs(f - num1);
      bool flag = this.player.GetButton("Axis Modifier") && (double) this.player.GetAxisRaw("Throttle") != 0.0;
      if ((((double) num2 <= 0.0 || (double) num2 >= 0.5 ? ((double) Mathf.Abs(f) > 0.5 ? 1 : 0) : 1) | (flag ? 1 : 0)) == 0)
        return;
      this.swivelDuctMode = SwivelDuctSystem.SwivelDuctMode.Manual;
    }
  }

  private void Awake()
  {
    this.inputs = this.aircraft.GetInputs();
    this.inputs.customAxis1 = 1f;
    this.swivelDuctMode = SwivelDuctSystem.SwivelDuctMode.Forward;
    this.player = ReInput.players.GetPlayer(0);
  }

  private void AutoSwivelMode(bool autoHoverEnabled)
  {
    if ((double) this.timeOnGround > 1.0)
    {
      if (this.swivelDuctMode == SwivelDuctSystem.SwivelDuctMode.Hover)
      {
        if ((double) this.inputs.brake >= 0.5 || (double) this.inputs.throttle >= 0.30000001192092896)
          return;
        this.swivelDuctMode = SwivelDuctSystem.SwivelDuctMode.Forward;
      }
      else if ((double) this.aircraft.speed > (double) this.shortTakeoffSpeed && (double) this.inputs.throttle > 0.89999997615814209)
        this.swivelDuctMode = SwivelDuctSystem.SwivelDuctMode.ShortTakeoff;
      else
        this.swivelDuctMode = SwivelDuctSystem.SwivelDuctMode.Forward;
    }
    else
    {
      if ((double) this.timeAirborne <= 5.0)
        return;
      float num = Vector3.Dot(this.aircraft.transform.forward, this.aircraft.rb.velocity);
      if (this.aircraft.gearDeployed)
      {
        if (this.swivelDuctMode == SwivelDuctSystem.SwivelDuctMode.Forward)
          this.swivelDuctMode = SwivelDuctSystem.SwivelDuctMode.ShortLanding;
        if (this.swivelDuctMode != SwivelDuctSystem.SwivelDuctMode.Hover || (double) this.inputs.throttle <= 0.949999988079071 || (double) num <= 30.0)
          return;
        this.swivelDuctMode = SwivelDuctSystem.SwivelDuctMode.ShortTakeoff;
      }
      else
      {
        if ((double) this.aircraft.speed <= 60.0)
          return;
        this.swivelDuctMode = SwivelDuctSystem.SwivelDuctMode.Forward;
      }
    }
  }

  private void LocalSimFixedUpdate()
  {
    this.timeAirborne = (double) this.aircraft.radarAlt > 0.20000000298023224 ? this.timeAirborne + Time.fixedDeltaTime : 0.0f;
    this.timeOnGround = (double) this.aircraft.radarAlt < 0.20000000298023224 ? this.timeOnGround + Time.fixedDeltaTime : 0.0f;
    SwivelDuctSystem.SwivelDuctMode swivelDuctMode = this.swivelDuctMode;
    bool autoHoverEnabled = this.aircraft.IsAutoHoverEnabled();
    if ((double) this.aircraft.speed > (double) this.maxSwivelAirspeed)
    {
      this.swivelDuctMode = SwivelDuctSystem.SwivelDuctMode.Forward;
    }
    else
    {
      this.CheckForManualInput(autoHoverEnabled);
      if (autoHoverEnabled)
        this.swivelDuctMode = SwivelDuctSystem.SwivelDuctMode.Hover;
      else if (this.swivelDuctMode != SwivelDuctSystem.SwivelDuctMode.Manual && this.aircraft.LocalSim)
        this.AutoSwivelMode(autoHoverEnabled);
    }
    if (this.swivelDuctMode != swivelDuctMode && (UnityEngine.Object) SceneSingleton<CombatHUD>.i?.aircraft == (UnityEngine.Object) this.aircraft)
      SceneSingleton<AircraftActionsReport>.i.ReportText($"Vectoring Mode set to {this.swivelDuctMode}", 4f);
    if (!this.aircraft.LocalSim)
      this.swivelDuctMode = SwivelDuctSystem.SwivelDuctMode.Manual;
    switch (this.swivelDuctMode)
    {
      case SwivelDuctSystem.SwivelDuctMode.Forward:
        this.inputs.customAxis1 = 1f;
        break;
      case SwivelDuctSystem.SwivelDuctMode.ShortTakeoff:
        this.inputs.customAxis1 = 0.5f;
        break;
      case SwivelDuctSystem.SwivelDuctMode.ShortLanding:
        this.inputs.customAxis1 = Mathf.Lerp(0.2f, 0.0f, (float) (((double) this.aircraft.speed - (double) this.shortLandingSpeed * 0.89999997615814209) / ((double) this.shortLandingSpeed * 1.1000000238418579 - (double) this.shortLandingSpeed * 0.89999997615814209)));
        break;
      case SwivelDuctSystem.SwivelDuctMode.Hover:
        this.inputs.customAxis1 = 0.0f;
        break;
    }
  }

  private void FixedUpdate()
  {
    if (this.aircraft.LocalSim)
      this.LocalSimFixedUpdate();
    this.swivelPosition += Mathf.Clamp(1f - this.inputs.customAxis1 - this.swivelPosition, -this.swivelSpeed * Time.fixedDeltaTime, this.swivelSpeed * Time.fixedDeltaTime);
    foreach (SwivelDuctSystem.Bearing bearing in this.bearings)
      bearing.Animate(this.swivelPosition, this.inputs);
    if ((double) this.swivelPosition != (double) this.swivelPositionPrev)
    {
      foreach (SwivelDuctSystem.MovingPanel movingPanel in this.movingPanels)
        movingPanel.Animate(this.swivelPosition);
    }
    if ((double) this.swivelPosition > 0.0)
    {
      foreach (SwivelDuctSystem.MovingPanel movingPanel in this.movingPanels)
        movingPanel.ApplyDrag(this.swivelPosition);
    }
    this.thrustDoorPosition += Mathf.Clamp(((double) this.inputs.customAxis1 < 1.0 ? 1f : 0.0f) - this.thrustDoorPosition, -this.thrustDoorSpeed * Time.fixedDeltaTime, this.thrustDoorSpeed * Time.fixedDeltaTime);
    foreach (SwivelDuctSystem.ThrustDoor thrustDoor in this.thrustDoors)
      thrustDoor.Animate(this.thrustDoorPosition);
    foreach (SwivelDuctSystem.LiftFan liftFan in this.liftFans)
      liftFan.Update(this.inputs.pitch, this.thrustDoorPosition, this.swivelPosition);
    foreach (SwivelDuctSystem.Thruster thruster in this.thrusters)
      thruster.Update(this.inputs, this.swivelPosition, this.aircraft.radarAlt, this.aircraft.speed);
    this.swivelPositionPrev = this.swivelPosition;
  }

  private enum SwivelDuctMode
  {
    Forward,
    ShortTakeoff,
    ShortLanding,
    Hover,
    Manual,
  }

  [Serializable]
  private struct Thruster
  {
    [SerializeField]
    private Turbojet jet;
    [SerializeField]
    private UnitPart part;
    [SerializeField]
    private Transform thrustTransform;
    [SerializeField]
    private float maxThrust;
    [SerializeField]
    private float rollFactor;
    [SerializeField]
    private float minSwivel;
    [SerializeField]
    private float minAlt;
    [SerializeField]
    private float maxSpeed;

    public void Update(ControlInputs inputs, float swivelAmount, float alt, float speed)
    {
      if ((double) swivelAmount < (double) this.minSwivel || (double) speed > (double) this.maxSpeed || (double) alt < (double) this.minAlt)
        return;
      this.part.rb.AddForceAtPosition(Mathf.Max(this.maxThrust * this.jet.GetThrustRatio() * inputs.roll * this.rollFactor, 0.0f) * this.thrustTransform.forward, this.thrustTransform.position);
    }
  }

  [Serializable]
  private struct ThrustDoor
  {
    [SerializeField]
    private Transform transform;
    [SerializeField]
    private Vector3 minAngle;
    [SerializeField]
    private Vector3 maxAngle;

    public void Animate(float amount)
    {
      this.transform.localEulerAngles = Vector3.Lerp(this.minAngle, this.maxAngle, amount);
    }
  }

  [Serializable]
  private struct MovingPanel
  {
    [SerializeField]
    private Transform transform;
    [SerializeField]
    private Vector3 minPosition;
    [SerializeField]
    private Vector3 maxPosition;
    [SerializeField]
    private Vector3 minRotation;
    [SerializeField]
    private Vector3 maxRotation;
    [SerializeField]
    private float extraDrag;
    [SerializeField]
    private UnitPart unitPart;
    private float amountPrev;

    public void Animate(float amount)
    {
      if ((double) amount != (double) this.amountPrev)
      {
        this.transform.localPosition = Vector3.Lerp(this.minPosition, this.maxPosition, amount);
        this.transform.localEulerAngles = Vector3.Lerp(this.minRotation, this.maxRotation, amount);
      }
      this.amountPrev = amount;
    }

    public void ApplyDrag(float amount)
    {
      if ((double) this.extraDrag <= 0.0)
        return;
      Vector3 vector3 = this.unitPart.rb.velocity - this.unitPart.parentUnit.GetWindVelocity();
      this.unitPart.rb.AddForce(amount * this.unitPart.parentUnit.airDensity * vector3.sqrMagnitude * -vector3.normalized);
    }
  }

  [Serializable]
  private struct Bearing
  {
    [SerializeField]
    private Transform transform;
    [SerializeField]
    private Vector3 minAngles;
    [SerializeField]
    private Vector3 maxAngles;
    [SerializeField]
    private Vector3 yawFactor;
    [SerializeField]
    private AnimationCurve adjustmentCurve;
    [SerializeField]
    private bool useCurve;
    private float amountPrev;
    private float yawPrev;

    public void Animate(float amount, ControlInputs inputs)
    {
      if ((double) this.amountPrev == (double) amount && (this.yawFactor == Vector3.zero || (double) this.yawPrev == (double) inputs.yaw))
        return;
      this.amountPrev = amount;
      this.yawPrev = inputs.yaw;
      if (this.useCurve)
        amount = this.adjustmentCurve.Evaluate(amount);
      this.transform.localEulerAngles = Vector3.Lerp(this.minAngles, this.maxAngles, amount) + this.yawFactor * inputs.yaw;
    }
  }

  [Serializable]
  private class LiftFan
  {
    [SerializeField]
    private UnitPart part;
    [SerializeField]
    private Transform thrustTransform;
    [SerializeField]
    private Transform nozzleThrustTransform;
    [SerializeField]
    private Transform bladeTransform;
    [SerializeField]
    private Turbojet jet;
    [SerializeField]
    private float thrustProportion;
    [SerializeField]
    private float pitchFactor;
    [SerializeField]
    private float parasiticLoss;
    [SerializeField]
    private float rotateSpeed;
    private float lastCoMCheck;
    private float lastOffsetCalc;
    private float fanCoMDist;
    private float nozzleCoMDist;
    private Vector3 centerOfMass;

    private void CheckCoM()
    {
      if ((double) Time.timeSinceLevelLoad - (double) this.lastCoMCheck < 10.0)
        return;
      this.lastCoMCheck = Time.timeSinceLevelLoad;
      Vector3 centerOfMass = this.part.parentUnit.GetCenterOfMass();
      this.centerOfMass = this.part.parentUnit.transform.InverseTransformPoint(centerOfMass);
      this.fanCoMDist = Vector3.Dot(this.thrustTransform.position - centerOfMass, this.part.xform.forward);
    }

    private void CalcOffsets()
    {
      if ((double) Time.timeSinceLevelLoad - (double) this.lastOffsetCalc < 1.0)
        return;
      this.lastOffsetCalc = Time.timeSinceLevelLoad;
      this.nozzleCoMDist = Vector3.Dot(this.centerOfMass - this.part.parentUnit.transform.InverseTransformPoint(this.nozzleThrustTransform.position), Vector3.forward);
    }

    public void Update(float pitchInput, float doorsOpenAmount, float swivelAmount)
    {
      if ((double) doorsOpenAmount > 0.0)
      {
        this.CheckCoM();
        this.CalcOffsets();
        this.bladeTransform.Rotate(Vector3.up * this.rotateSpeed * Time.deltaTime);
        Vector3 vector3 = this.part.parentUnit.transform.TransformPoint(this.centerOfMass);
        this.part.rb.AddForceAtPosition(Mathf.Max(-this.part.parentUnit.transform.InverseTransformVector(Vector3.Cross(this.jet.GetThrust() * this.nozzleThrustTransform.forward, -(this.nozzleThrustTransform.position - vector3))).x / this.part.parentUnit.transform.InverseTransformVector(Vector3.Cross(this.thrustTransform.forward, -(this.thrustTransform.position - vector3))).x + doorsOpenAmount * pitchInput * this.pitchFactor * this.jet.GetRPMRatio(), 0.0f) * this.thrustTransform.forward, this.thrustTransform.position);
        this.jet.SetParasiticLoss(this.parasiticLoss * swivelAmount);
      }
      else
        this.jet.SetParasiticLoss(0.0f);
    }
  }
}

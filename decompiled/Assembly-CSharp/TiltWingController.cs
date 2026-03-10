// Decompiled with JetBrains decompiler
// Type: TiltWingController
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;

#nullable disable
public class TiltWingController : MonoBehaviour
{
  [SerializeField]
  private TiltWingController.RotatorInput[] rotatingJoints;
  [SerializeField]
  private Aircraft aircraft;
  [SerializeField]
  private float forwardLockSpeed;
  [SerializeField]
  private float hoverLockSpeed;
  [SerializeField]
  private float tiltTransitionRate;
  [SerializeField]
  private Vector3 autoTiltPIDFactors;
  [SerializeField]
  private AnimationCurve tiltAtSpeed;
  private ControlInputs inputs;
  private AircraftParameters aircraftParameters;
  private float tiltCorrectionPosition;
  private bool autoTilt;
  private PID autoTiltPID;

  public (float, float) GetAngleLimits() => this.rotatingJoints[0].GetAngleLimits();

  public float GetAverageAngle() => this.rotatingJoints[0].GetAngle();

  private void Awake()
  {
    this.autoTiltPID = new PID(this.autoTiltPIDFactors);
    this.inputs = this.aircraft.GetInputs();
    this.aircraftParameters = this.aircraft.GetAircraftParameters();
    this.aircraft.onSetFlightAssist += new Action<Aircraft.OnFlightAssistToggle>(this.TiltWingController_OnSetFlightAssist);
    foreach (TiltWingController.RotatorInput rotatingJoint in this.rotatingJoints)
      rotatingJoint.Setup();
  }

  private void TiltWingController_OnSetFlightAssist(Aircraft.OnFlightAssistToggle e)
  {
    this.autoTilt = e.enabled;
  }

  private void FixedUpdate()
  {
    float speed = Vector3.Dot(this.aircraft.rb.velocity, this.aircraft.transform.forward);
    if (this.aircraft.LocalSim && this.autoTilt || !this.aircraft.networked)
    {
      double num1 = (double) Vector3.Dot(this.aircraft.transform.forward, -Vector3.up) * 2.0 + 0.20000000298023224;
      double pitch = (double) this.inputs.pitch;
      float a1 = 0.18f + Mathf.Clamp(-0.04f * TargetCalc.GetAngleOnAxis(this.aircraft.transform.up, Vector3.up, this.aircraft.transform.right), -0.18f, 0.18f);
      float b = 1f;
      float num2 = this.tiltAtSpeed.Evaluate(this.aircraft.speed);
      float a2 = this.aircraft.speed / this.aircraftParameters.maxSpeed;
      float num3 = Mathf.Clamp(this.inputs.throttle - 0.8f, -0.3f, 0.1f) / Mathf.Max(a2, 0.3f);
      float t = num2 + num3 * 0.5f;
      this.inputs.customAxis1 = Mathf.Lerp(a1, b, t);
      if (!this.aircraft.networked || (double) this.aircraft.radarAlt < 1.0 && (double) Mathf.Abs(this.inputs.pitch) < 0.20000000298023224)
        this.inputs.customAxis1 = Mathf.Lerp(this.inputs.customAxis1, 0.18f, Time.fixedDeltaTime);
    }
    if (this.aircraft.IsAutoHoverEnabled())
      this.inputs.customAxis1 = 0.18f;
    this.tiltCorrectionPosition += Mathf.Clamp(this.inputs.customAxis1 - this.tiltCorrectionPosition, -this.tiltTransitionRate * Time.fixedDeltaTime, this.tiltTransitionRate * Time.fixedDeltaTime);
    foreach (TiltWingController.RotatorInput rotatingJoint in this.rotatingJoints)
      rotatingJoint.Animate(this.inputs, speed, this.tiltCorrectionPosition);
  }

  [Serializable]
  private class RotatorLinkage
  {
    [SerializeField]
    private bool stretch;
    [SerializeField]
    private Transform linkage;
    [SerializeField]
    private Transform stretchTo;

    public void Animate()
    {
      Vector3 forward = this.stretchTo.transform.position - this.linkage.transform.position;
      this.linkage.transform.rotation = Quaternion.LookRotation(forward, this.linkage.transform.up);
      if (!this.stretch)
        return;
      this.linkage.transform.localScale = new Vector3(1f, 1f, forward.magnitude);
    }

    public void Remove() => this.linkage.gameObject.GetComponent<Renderer>().enabled = false;
  }

  [Serializable]
  private class RotatorInput
  {
    [SerializeField]
    private AeroPart part;
    [SerializeField]
    private AeroPart[] connectedParts;
    [SerializeField]
    private TiltWingController.RotatorLinkage[] links;
    [SerializeField]
    private float minAngle;
    [SerializeField]
    private float maxAngle;
    [SerializeField]
    private float minSpeed;
    [SerializeField]
    private float maxSpeed;
    [SerializeField]
    private float rotationSpeed;
    [SerializeField]
    private float yawFactor;
    [SerializeField]
    private float customAxis1Factor;
    [SerializeField]
    private float pitchFactor;
    [SerializeField]
    private float spring;
    [SerializeField]
    private float damp;
    [SerializeField]
    private float breakStrength;
    [SerializeField]
    private float currentAngle = 0.18f;
    private float baseAngle;

    public (float, float) GetAngleLimits() => (this.minAngle, this.maxAngle);

    public float GetAngle() => this.currentAngle;

    public void Setup()
    {
      this.baseAngle = this.part.transform.localEulerAngles.x;
      this.part.onPartDetached += new Action<UnitPart>(this.RotatorInput_OnDetached);
    }

    private void RotatorInput_OnDetached(UnitPart unitPart)
    {
      foreach (TiltWingController.RotatorLinkage link in this.links)
        link.Remove();
      this.part.onPartDetached -= new Action<UnitPart>(this.RotatorInput_OnDetached);
    }

    public void Animate(ControlInputs inputs, float speed, float tiltCorrection)
    {
      float num1 = Mathf.Clamp01((float) (1.0 - (double) this.currentAngle * 0.029999999329447746));
      float num2 = this.currentAngle / this.maxAngle;
      int num3 = (double) num2 <= 0.20000000298023224 || (double) num2 >= 0.699999988079071 ? 0 : 1;
      float f = Mathf.Lerp(this.minAngle, this.maxAngle, (float) ((double) this.yawFactor * (double) inputs.yaw * (double) num1 + (double) this.pitchFactor * (double) num3 * (double) inputs.pitch) + tiltCorrection) - this.currentAngle;
      this.currentAngle += Mathf.Clamp(f, -this.rotationSpeed * Time.deltaTime, this.rotationSpeed * Time.deltaTime);
      this.currentAngle = Mathf.Clamp(this.currentAngle, this.minAngle, this.maxAngle);
      for (int index = 0; index < this.connectedParts.Length; ++index)
        this.part.SetHingeJoint(index, this.connectedParts[index], this.spring, this.damp, this.currentAngle, this.breakStrength, this.baseAngle);
      if ((double) Mathf.Abs(f) <= 0.0099999997764825821)
        return;
      foreach (TiltWingController.RotatorLinkage link in this.links)
        link.Animate();
    }
  }
}

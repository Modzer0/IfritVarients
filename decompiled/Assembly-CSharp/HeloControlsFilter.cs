// Decompiled with JetBrains decompiler
// Type: HeloControlsFilter
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;

#nullable disable
public class HeloControlsFilter : ControlsFilter
{
  [SerializeField]
  private HeloControlsFilter.HeloFlyByWire heloFlyByWire;
  [SerializeField]
  private RotorShaft rotorShaft;
  [SerializeField]
  private DuctedFan tailRotor;
  [SerializeField]
  private float tailRotorDist;

  public override void Filter(
    ControlInputs inputs,
    Vector3 rawInputs,
    Rigidbody rb,
    float gForce,
    bool flightAssist)
  {
    this.gearDownSmoothed = FastMath.SmoothDamp(this.gearDownSmoothed, this.aircraft.gearDeployed ? 1f : 0.0f, ref this.gearDownSmoothingVel, 2f);
    Vector3 localAngularVelocity = rb.transform.InverseTransformDirection(rb.angularVelocity);
    if ((UnityEngine.Object) this.tailRotor != (UnityEngine.Object) null)
      this.tailRotor.SetDesiredBaseThrust(this.rotorShaft.GetTorque() / this.tailRotorDist);
    this.autoHover.Hover(inputs, this.aircraft);
    if (!this.heloFlyByWire.Enabled)
      return;
    this.heloFlyByWire.Filter(this.aircraft, inputs, localAngularVelocity);
  }

  public RotorShaft GetRotorShaft() => this.rotorShaft;

  [Serializable]
  private class HeloFlyByWire
  {
    public bool Enabled;
    [SerializeField]
    private float gLimit = 3f;
    [SerializeField]
    private Vector3 directControlFactor = new Vector3(0.7f, 0.7f, 0.7f);
    [SerializeField]
    private Vector3 maxAngularVel = new Vector3(1f, 2f, 2f);
    [SerializeField]
    private Vector3 pFactor = new Vector3(2f, 2f, 2f);
    [SerializeField]
    private Vector3 dFactor = new Vector3(0.01f, 0.01f, 0.01f);
    [SerializeField]
    private float yawWeathervaneStrength = 0.4f;
    [SerializeField]
    private float yawWeathervaneMinSpeed = 40f;
    [SerializeField]
    private float yawWeathervaneMaxSpeed = 60f;
    private Vector3 pPrev;
    private Vector3 compensator;

    public void Filter(Aircraft aircraft, ControlInputs inputs, Vector3 localAngularVelocity)
    {
      float max = this.gLimit * 9.81f / Mathf.Max(aircraft.speed, 10f);
      Vector3 vector3 = new Vector3(Mathf.Clamp(inputs.pitch * this.maxAngularVel.x, -max, max), inputs.yaw * this.maxAngularVel.y, -inputs.roll * this.maxAngularVel.z);
      Vector3 a1 = localAngularVelocity - vector3;
      Vector3 a2 = (a1 - this.pPrev) / Time.fixedDeltaTime;
      this.pPrev = a1;
      if ((double) this.yawWeathervaneStrength > 0.0 && (double) aircraft.speed > (double) this.yawWeathervaneMinSpeed)
      {
        float num1 = Mathf.Clamp01((float) (((double) aircraft.speed - (double) this.yawWeathervaneMinSpeed) / ((double) this.yawWeathervaneMaxSpeed - (double) this.yawWeathervaneMinSpeed)));
        float num2 = TargetCalc.GetAngleOnAxis(aircraft.rb.velocity, aircraft.cockpit.xform.forward, aircraft.cockpit.xform.up) * 0.1f;
        a1.y += num2 * this.yawWeathervaneStrength * num1;
      }
      this.compensator += -(Vector3.Scale(a1, this.pFactor) + Vector3.Scale(a2, this.dFactor)) * Time.fixedDeltaTime;
      this.compensator.x = Mathf.Clamp(this.compensator.x, -1f, 1f);
      this.compensator.y = Mathf.Clamp(this.compensator.y, -1f, 1f);
      this.compensator.z = Mathf.Clamp(this.compensator.z, -1f, 1f);
      if ((double) aircraft.radarAlt < 0.5)
        this.compensator = Vector3.zero;
      inputs.pitch = Mathf.Clamp(-a1.x * this.directControlFactor.x + this.compensator.x, -1f, 1f);
      inputs.yaw = Mathf.Clamp(-a1.y * this.directControlFactor.y + this.compensator.y, -1f, 1f);
      inputs.roll = -Mathf.Clamp(-a1.z * this.directControlFactor.z + this.compensator.z, -1f, 1f);
    }
  }
}

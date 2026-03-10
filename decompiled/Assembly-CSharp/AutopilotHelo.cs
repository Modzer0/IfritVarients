// Decompiled with JetBrains decompiler
// Type: AutopilotHelo
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.DebugScripts;
using UnityEngine;

#nullable disable
public class AutopilotHelo : Autopilot
{
  private float nominalRPM;
  public RotorShaft[] rotorShafts;
  public DuctedFan tailRotor;
  private PID collectivePIDController;
  private PID2D tiltPID;
  private bool tailRotorFailure;
  private float terrainAvoidanceUrgency;
  [SerializeField]
  private float maxTilt = 0.3f;
  [SerializeField]
  private bool compoundHelo = true;
  private GameObject destinationDebug;

  public override void Awake()
  {
    base.Awake();
    this.collectivePIDController = new PID(1f, 0.0f, 0.1f);
    this.obstacleNormal = Vector3.up;
    this.tiltPID = new PID2D(new PIDFactors(1f / 1000f, 0.0f, 0.01f), 500f, 0.5f);
    if ((double) this.aircraft.radarAlt > (double) this.aircraft.definition.spawnOffset.y + 1.0)
      this.controlInputs.throttle = 0.5f;
    this.nominalRPM = 0.0f;
    foreach (RotorShaft rotorShaft in this.rotorShafts)
      this.nominalRPM += rotorShaft.GetMaxRPM() * 0.92f;
    this.nominalRPM /= (float) this.rotorShafts.Length;
  }

  public override void BoresightAim(GlobalPosition aimPoint, float altitudeHold)
  {
    float num1 = Mathf.Clamp(this.aircraft.radarAlt - altitudeHold, -40f, 20f);
    Vector3 self = aimPoint - this.aircraft.GlobalPosition();
    Vector3 localAngularVelocity = this.aircraft.rb.transform.InverseTransformDirection(this.aircraft.rb.angularVelocity);
    Vector3 vector3_1 = self.normalized * this.aircraft.speed - this.aircraft.rb.velocity;
    Vector3 vector3_2 = self.normalized - this.aircraft.transform.forward;
    float num2 = num1 - vector3_1.y;
    float x = -TargetCalc.GetAngleOnAxis(self, this.aircraft.transform.forward, this.aircraft.transform.right);
    float y = -TargetCalc.GetAngleOnAxis(self, this.aircraft.transform.forward, this.aircraft.transform.up);
    float z = -TargetCalc.GetAngleOnAxis(this.aircraft.transform.up, Vector3.up - this.aircraft.transform.right * Mathf.Clamp(Vector3.Dot(this.aircraft.rb.velocity, new Vector3(this.aircraft.transform.right.x, 0.0f, this.aircraft.transform.right.z).normalized) * 0.1f, -0.25f, 0.25f), this.aircraft.transform.forward);
    float num3 = (float) (0.40000000596046448 - (double) num2 * ((double) num2 < 0.0 ? 1.0 : 0.019999999552965164) - (double) this.aircraft.rb.velocity.y * 0.20000000298023224);
    if (this.rotorShafts.Length != 0)
    {
      float num4 = 0.0f;
      float num5 = 0.0f;
      foreach (RotorShaft rotorShaft in this.rotorShafts)
      {
        num4 += rotorShaft.GetVRSFactor();
        num5 += rotorShaft.GetRPM();
      }
      float num6 = num5 / (float) this.rotorShafts.Length;
      float num7 = num4 / (float) this.rotorShafts.Length;
      float num8 = num6 - this.nominalRPM;
      num3 = num3 + Mathf.Min(num8 * 1.5f, 0.0f) + num7 * 2f;
    }
    if (this.compoundHelo)
    {
      this.controlInputs.customAxis1 = (float) ((double) FastMath.Distance(aimPoint, this.aircraft.GlobalPosition()) * (1.0 / 1000.0) - (double) this.aircraft.speed * 0.004999999888241291);
      if ((double) this.aircraft.speed > 80.0)
        this.controlInputs.throttle = Mathf.Min(this.controlInputs.throttle, 0.5f);
    }
    this.hoverController.ApplyInputs(this.controlInputs, new Vector3(x, y, z), localAngularVelocity);
    this.controlInputs.throttle = Mathf.Clamp01(num3);
    this.aircraft.FilterInputs();
  }

  public override void AutoAim(
    GlobalPosition destination,
    float altitudeHold,
    Vector3 aimDirection,
    Vector3 targetVelocity,
    bool followTerrain)
  {
    float num1 = 0.0f;
    float a1 = 0.0f;
    if (this.rotorShafts.Length != 0)
    {
      foreach (RotorShaft rotorShaft in this.rotorShafts)
      {
        num1 += rotorShaft.GetVRSFactor();
        a1 += Mathf.Min(rotorShaft.GetRPM() - this.nominalRPM, 0.0f);
      }
      num1 /= (float) this.rotorShafts.Length;
      a1 /= (float) this.rotorShafts.Length;
    }
    if ((double) num1 > 0.40000000596046448)
      destination = this.aircraft.GlobalPosition() + new Vector3(this.aircraft.transform.forward.x, 0.0f, this.aircraft.transform.forward.z) * 10000f;
    float t1 = this.aircraft.speed / this.aircraftParameters.maxSpeed;
    Vector3 vector3_1 = destination - this.aircraft.GlobalPosition();
    Vector3 rhs = this.aircraft.rb.velocity - targetVelocity;
    if (this.tailRotorFailure)
    {
      altitudeHold = 0.0f;
      Vector3 vector3_2 = vector3_1.normalized * 20f + this.aircraft.rb.velocity * this.aircraft.radarAlt * 2f;
      vector3_1 = destination - this.aircraft.GlobalPosition();
      if ((double) this.aircraft.radarAlt > 10.0 && this.aircraft.gearState == LandingGear.GearState.LockedRetracted)
        this.aircraft.SetGear(true);
      if ((double) this.aircraft.radarAlt < (double) this.aircraft.definition.spawnOffset.y + 0.20000000298023224)
      {
        this.controlInputs.brake = 1f;
        this.controlInputs.throttle = 0.0f;
        return;
      }
      if ((double) this.aircraft.radarAlt < 20.0 && (double) this.aircraft.speed < 15.0)
      {
        this.controlInputs.throttle = 0.0f;
        this.controlInputs.brake = 1f;
        return;
      }
    }
    float magnitude = vector3_1.magnitude;
    Vector3 vector3_3 = vector3_1 with { y = 0.0f };
    double num2 = (double) Mathf.Clamp01(Vector3.Dot(this.aircraft.transform.forward, rhs));
    float t2 = Mathf.SmoothStep(0.0f, 1f, t1);
    if ((double) Time.timeSinceLevelLoad - (double) this.lastWaypointTime > 1.0)
    {
      this.lastWaypointTime = Time.timeSinceLevelLoad;
      Vector3 direction = Vector3.RotateTowards((rhs + this.aircraft.transform.forward * 20f) with
      {
        y = 0.0f
      }, vector3_3, 0.8f, 0.0f);
      this.terrainAvoidanceUrgency = 7.5f - this.TerrainAvoidanceCheck();
      this.terrainAvoidanceUrgency = Mathf.Max(this.terrainAvoidanceUrgency, 0.0f);
      this.waypoint = this.TerrainWaypoint(direction, altitudeHold + this.terrainAvoidanceUrgency, Mathf.Max(this.aircraft.speed, 100f) * 6f);
      if ((Object) this.tailRotor != (Object) null)
        this.tailRotorFailure = (double) this.tailRotor.GetRPM() < 1000.0;
    }
    this.waypointDelta = this.waypoint - this.aircraft.GlobalPosition();
    this.waypointDelta.y = 0.0f;
    float num3 = followTerrain ? this.aircraft.radarAlt - altitudeHold : -vector3_1.y - altitudeHold;
    Vector3 vector3_4 = destination - this.aircraft.GlobalPosition();
    Vector3 vector3_5 = new Vector3(this.aircraft.transform.right.x, 0.0f, this.aircraft.transform.right.z);
    Vector2 output = this.tiltPID.GetOutput(new Vector2(vector3_4.x, vector3_4.z), Time.fixedDeltaTime);
    Vector3 localAngularVelocity = this.aircraft.rb.transform.InverseTransformDirection(this.aircraft.rb.angularVelocity);
    Vector3 self = Vector3.up + new Vector3(Mathf.Clamp(output.x, -this.maxTilt, this.maxTilt), 0.0f, Mathf.Clamp(output.y, -this.maxTilt, this.maxTilt)) + this.obstacleNormal * this.terrainAvoidanceUrgency * 0.2f;
    float x = Mathf.Lerp(-TargetCalc.GetAngleOnAxis(self, this.aircraft.transform.up, this.aircraft.transform.right), -TargetCalc.GetAngleOnAxis(this.waypointDelta, this.aircraft.rb.velocity + this.aircraft.transform.forward * 20f, this.aircraft.transform.right) * 4f, t2);
    float a2 = -TargetCalc.GetAngleOnAxis(this.waypointDelta, this.aircraft.transform.forward, this.aircraft.transform.up);
    if (aimDirection != Vector3.zero)
      a2 = -TargetCalc.GetAngleOnAxis(aimDirection, this.aircraft.transform.forward, this.aircraft.transform.up);
    float b1 = -TargetCalc.GetAngleOnAxis(this.waypointDelta, this.aircraft.rb.velocity, this.aircraft.transform.up);
    float y = Mathf.Lerp(a2, b1, t2);
    float z = Mathf.Lerp(TargetCalc.GetAngleOnAxis(self, this.aircraft.transform.up, this.aircraft.transform.forward), TargetCalc.GetAngleOnAxis(this.aircraft.transform.up, Vector3.up + Mathf.Clamp(b1 * -0.05f, -2f, 2f) * -vector3_5, -this.aircraft.transform.forward), t2);
    this.hoverController.ApplyInputs(this.controlInputs, new Vector3(x, y, z), localAngularVelocity);
    float b2 = Mathf.Clamp01((float) (0.5 + (double) magnitude * (1.0 / 1000.0) - (double) this.aircraft.speed * 0.019999999552965164));
    float num4 = Mathf.Lerp((float) (0.40000000596046448 - (double) num3 * ((double) num3 < 0.0 ? 1.0 : 0.05000000074505806) - (double) this.aircraft.rb.velocity.y * 0.20000000298023224), b2, t2 * 2f) + 0.5f * this.terrainAvoidanceUrgency + num1 * 2f;
    if ((double) this.aircraft.radarAlt > 7.0)
      num4 += Mathf.Min(a1, 0.0f);
    this.controlInputs.throttle += Mathf.Clamp(Mathf.Clamp01(num4) - this.controlInputs.throttle, -1f * Time.deltaTime, 1f * Time.deltaTime);
    if (this.compoundHelo)
    {
      this.controlInputs.customAxis1 = b2;
      if ((double) a1 < -5.0 || (double) Vector3.Dot(this.aircraft.transform.forward, vector3_3) < 0.0)
        this.controlInputs.customAxis1 = 0.5f;
      if ((double) this.aircraft.speed > 80.0)
        this.controlInputs.throttle = Mathf.Min(this.controlInputs.throttle, 0.5f);
    }
    if (DebugVis.Enabled && (Object) SceneSingleton<CameraStateManager>.i.followingUnit == (Object) this.aircraft)
    {
      DebugVis.Create<GameObject>(ref this.waypointDebug, GameAssets.i.waypointDebug, Datum.origin);
      DebugVis.Create<GameObject>(ref this.aimVectorDebug, GameAssets.i.debugArrowGreen, this.aircraft.cockpit.transform);
      DebugVis.Create<GameObject>(ref this.vectorDebug2, GameAssets.i.debugArrow, Datum.origin);
      this.waypointDebug.transform.localPosition = this.waypoint.AsVector3();
      this.aimVectorDebug.transform.rotation = Quaternion.LookRotation(this.waypoint - this.aircraft.GlobalPosition());
      this.waypointDebug.transform.rotation = this.aimVectorDebug.transform.rotation;
      this.aimVectorDebug.transform.localScale = new Vector3(1f, 1f, FastMath.Distance(this.waypoint, this.aircraft.GlobalPosition()));
    }
    this.aircraft.FilterInputs();
  }
}

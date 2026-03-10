// Decompiled with JetBrains decompiler
// Type: AutopilotTiltwing
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.DebugScripts;
using UnityEngine;

#nullable disable
public class AutopilotTiltwing : Autopilot
{
  private PID collectivePIDController;
  [SerializeField]
  private PIDFactors tiltPIDFactors;
  [SerializeField]
  private PIDFactors collectivePIDFactors;
  [SerializeField]
  private float maxTilt = 0.2f;
  private PID3D tiltPID;
  private Vector3 tiltTarget;
  private Vector3 yawVector;
  private float waypointGradient;
  private GameObject destinationDebug;

  public override void Awake()
  {
    base.Awake();
    this.terrainWarning = new TerrainWarningSystem(this.aircraft);
    this.collectivePIDController = new PID(this.collectivePIDFactors);
    this.tiltPID = new PID3D();
    if ((double) this.aircraft.radarAlt <= (double) this.aircraft.definition.spawnOffset.y + 1.0)
      return;
    this.controlInputs.throttle = 0.5f;
  }

  public override void BoresightAim(GlobalPosition aimPoint, float altitudeHold)
  {
    this.AutoAim(aimPoint, altitudeHold, aimPoint - this.aircraft.GlobalPosition(), Vector3.zero, true);
  }

  public override void AutoAim(
    GlobalPosition destination,
    float altitudeHold,
    Vector3 aimDirection,
    Vector3 targetVelocity,
    bool followTerrain)
  {
    float t = this.aircraft.speed * 2f / this.aircraftParameters.maxSpeed;
    Vector3 avoidanceVector;
    if (this.exclusionZoneChecker.TryGetWarning(out avoidanceVector))
    {
      destination = this.aircraft.GlobalPosition() + avoidanceVector.normalized * 10000f;
      this.controlInputs.throttle = 1f;
    }
    destination.y = Mathf.Max(destination.y, this.aircraft.definition.spawnOffset.y + 2f);
    Vector3 vector3_1 = destination - this.aircraft.GlobalPosition();
    float magnitude = new Vector3(vector3_1.x, 0.0f, vector3_1.z).magnitude;
    altitudeHold *= Mathf.Clamp01((float) ((double) magnitude * 0.004999999888241291 + (double) this.aircraft.speed * 0.0099999997764825821 - 1.0));
    Vector3 vector3_2 = this.aircraft.rb.velocity - targetVelocity;
    Vector3 vector3_3 = vector3_1 with { y = 0.0f };
    Vector3 normalized1 = vector3_3.normalized;
    float num1 = Mathf.SmoothStep(0.0f, 1f, t);
    if ((double) Time.timeSinceLevelLoad - (double) this.lastWaypointTime > 0.5)
    {
      this.lastWaypointTime = Time.timeSinceLevelLoad;
      Vector3 vector3_4 = (vector3_2 + this.aircraft.transform.forward * 20f) with
      {
        y = 0.0f
      };
      Vector3 direction = (double) this.aircraft.speed < 60.0 ? vector3_3 : Vector3.RotateTowards(vector3_4, vector3_3, 0.5f, 0.0f);
      if ((double) Vector3.Angle(vector3_4, vector3_3) > 30.0 && (double) this.aircraft.speed > 30.0)
        altitudeHold += 20f;
      float lookAheadDistance = Mathf.Max(Mathf.Min(Mathf.Max((float) ((double) this.aircraft.speed * (double) this.aircraft.speed * 0.10000000149011612), 200f), magnitude), magnitude * 0.1f);
      this.waypoint = this.TerrainWaypoint(direction, altitudeHold, lookAheadDistance);
      this.waypointGradient = (this.waypoint - this.aircraft.GlobalPosition()).y / lookAheadDistance;
    }
    bool flag = false;
    if ((double) magnitude > 500.0 && (double) this.waypointGradient > 0.10000000149011612 && (double) this.aircraft.speed < 60.0)
    {
      flag = true;
      this.controlInputs.customAxis1 = 0.25f;
    }
    if ((double) this.terrainWarning.urgency > 0.0)
    {
      this.controlInputs.customAxis1 = 0.18f;
      flag = true;
    }
    if (flag)
      this.aircraft.SetFlightAssist(false);
    else
      this.aircraft.SetFlightAssist(true);
    float num2 = followTerrain ? this.aircraft.radarAlt - altitudeHold : -vector3_1.y - altitudeHold;
    this.terrainWarning.CheckTerrain();
    this.waypoint = this.waypoint + (Vector3) (Vector2.up * this.terrainWarning.urgency * 50f);
    this.waypointDelta = this.waypoint - this.aircraft.GlobalPosition();
    float num3 = num1 * Mathf.Clamp01((float) (((double) magnitude - 500.0) * 0.00050000002374872565));
    Vector3 localAngularVelocity = this.aircraft.rb.transform.InverseTransformDirection(this.aircraft.rb.angularVelocity);
    Vector3 normalized2 = new Vector3(this.waypointDelta.x, 0.0f, this.waypointDelta.y).normalized;
    Vector3 normalized3 = this.aircraft.rb.velocity.normalized;
    float num4 = Mathf.Clamp(num2, -40f, 20f);
    Vector3 vector3_5 = this.waypointDelta.normalized * this.aircraft.speed - vector3_2 + Vector3.up * Mathf.Max(-num4, 0.0f) * 0.5f;
    float maxLength = this.maxTilt / (float) (1.0 + (double) this.terrainWarning.urgency * 0.25);
    this.tiltTarget = this.tiltPID.GetOutputSqrt(vector3_3, 50f, Time.fixedDeltaTime, this.tiltPIDFactors);
    if ((double) this.terrainWarning.urgency > 0.0)
      this.tiltTarget -= new Vector3(this.aircraft.rb.velocity.x, 0.0f, this.aircraft.rb.velocity.z);
    Vector3 vector3_6 = Vector3.up + Vector3.ClampMagnitude(this.tiltTarget, maxLength);
    float angleOnAxis1 = TargetCalc.GetAngleOnAxis(this.aircraft.transform.up, vector3_6, this.aircraft.transform.right);
    Vector3 waypointDelta = this.waypointDelta;
    Vector3 right = this.aircraft.transform.right;
    float angleOnAxis2 = TargetCalc.GetAngleOnAxis(normalized3, waypointDelta, right);
    this.yawVector = this.waypointDelta;
    float angleOnAxis3 = TargetCalc.GetAngleOnAxis(this.aircraft.transform.forward, this.yawVector, this.aircraft.transform.up);
    float angleOnAxis4 = TargetCalc.GetAngleOnAxis(this.aircraft.cockpit.xform.forward, this.waypointDelta, this.aircraft.cockpit.xform.up);
    float angleOnAxis5 = TargetCalc.GetAngleOnAxis(this.aircraft.transform.up, vector3_6, -this.aircraft.transform.forward);
    Vector3 vector3_7 = -Vector3.Cross(this.aircraft.cockpit.xform.forward, Vector3.up);
    float angleOnAxis6 = TargetCalc.GetAngleOnAxis(this.aircraft.transform.up, Vector3.up + 0.1f * Mathf.Clamp(angleOnAxis4 * 30f, -3f, 3f) * vector3_7, -this.aircraft.transform.forward);
    this.hoverController.ApplyInputs(this.controlInputs, new Vector3(angleOnAxis1, angleOnAxis3, angleOnAxis5), localAngularVelocity);
    this.forwardFlightController.ApplyInputs(this.controlInputs, this.aircraft.speed, new Vector3(angleOnAxis2, angleOnAxis4, angleOnAxis6), num3);
    float a = 0.5f - (num4 + Mathf.Clamp(vector3_2.y * 3f, -15f, 15f) - vector3_5.y) * 0.15f;
    if (followTerrain)
      a += this.terrainWarning.urgency;
    float b = (float) (0.5 + ((double) (Mathf.Sqrt(magnitude) * 1.7f) - (double) this.aircraft.speed) * 0.10000000149011612);
    this.controlInputs.throttle += this.collectivePIDController.GetOutput(Mathf.Lerp(a, b, num3) - this.controlInputs.throttle, 0.25f, Time.fixedDeltaTime, this.aircraftParameters.collectivePID);
    this.controlInputs.throttle = Mathf.Clamp01(this.controlInputs.throttle);
    if ((double) this.terrainWarning.urgency > 0.0)
      this.controlInputs.throttle = 1f;
    if (DebugVis.Enabled && (Object) SceneSingleton<CameraStateManager>.i.followingUnit == (Object) this.aircraft)
    {
      DebugVis.Create<GameObject>(ref this.waypointDebug, GameAssets.i.waypointDebug, Datum.origin);
      DebugVis.Create<GameObject>(ref this.aimVectorDebug, GameAssets.i.debugArrow, this.aircraft.cockpit.transform);
      DebugVis.Create<GameObject>(ref this.vectorDebug2, GameAssets.i.debugArrow, Datum.origin);
      if (DebugVis.Create<GameObject>(ref this.destinationDebug, GameAssets.i.debugPoint, Datum.origin))
        this.destinationDebug.transform.localScale = Vector3.one * 10f;
      this.aimVectorDebug.transform.position = this.aircraft.transform.position + this.aircraft.transform.up * 2f;
      this.aimVectorDebug.transform.rotation = Quaternion.LookRotation(vector3_6);
      this.aimVectorDebug.transform.localScale = new Vector3(2f, 2f, 2f);
      this.destinationDebug.transform.position = this.waypoint.ToLocalPosition();
      Debug.Log((object) $"destinationDist: {magnitude}, altitudeHold: {altitudeHold}");
    }
    this.aircraft.FilterInputs();
  }
}

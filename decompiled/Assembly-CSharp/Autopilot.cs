// Decompiled with JetBrains decompiler
// Type: Autopilot
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption;
using System;
using UnityEngine;

#nullable disable
public class Autopilot : MonoBehaviour
{
  public Aircraft aircraft;
  protected GlobalPosition waypoint;
  protected GlobalPosition smoothedDestination;
  protected Vector3 waypointDelta;
  protected Vector3 obstacleNormal;
  protected ControlInputs controlInputs;
  protected AircraftParameters aircraftParameters;
  protected float lastWaypointTime;
  protected GameObject waypointDebug;
  protected GameObject aimVectorDebug;
  protected GameObject vectorDebug2;
  protected Autopilot.ExclusionZoneChecker exclusionZoneChecker;
  protected TerrainWarningSystem terrainWarning;
  protected Autopilot.AirspeedChecker airspeedChecker;
  private float verticalVelocitySmoothed;
  private float verticalVelocitySmoothingVel;
  [SerializeField]
  protected Autopilot.ForwardFlightController forwardFlightController;
  [SerializeField]
  protected Autopilot.HoverController hoverController;
  private Vector3 hoverErrorPrev;

  public TerrainWarningSystem GetTerrainWarningSystem() => this.terrainWarning;

  public virtual void Awake()
  {
    this.aircraft.autopilot = this;
    this.controlInputs = this.aircraft.GetInputs();
    this.aircraftParameters = this.aircraft.GetAircraftParameters();
    this.obstacleNormal = Vector3.up;
    this.exclusionZoneChecker = new Autopilot.ExclusionZoneChecker(this.aircraft);
    this.airspeedChecker = new Autopilot.AirspeedChecker(this.aircraft);
    this.forwardFlightController.Initialize();
    this.hoverController.Initialize();
  }

  public GlobalPosition TerrainWaypoint(
    Vector3 direction,
    float altitudeTarget,
    float lookAheadDistance)
  {
    direction.y = 0.0f;
    Vector3 start = (this.aircraft.transform.position + direction.normalized * lookAheadDistance) with
    {
      y = Datum.LocalSeaY
    };
    RaycastHit hitInfo;
    if (Physics.Linecast(start + Vector3.up * 5000f, start - Vector3.up * 5000f, out hitInfo, 8256))
      start.y = Mathf.Max(hitInfo.point.y + 1f, Datum.LocalSeaY);
    int num = 0;
    while (num < 6 && Physics.Linecast(start, this.aircraft.transform.position, 8256))
    {
      ++num;
      start += (float) (30 * num) * Vector3.up;
    }
    return (start + Vector3.up * altitudeTarget).ToGlobalPosition();
  }

  protected virtual float TerrainAvoidanceCheck()
  {
    Vector3 start = this.aircraft.transform.position - Vector3.up * this.aircraft.definition.spawnOffset.y * 2f;
    float a = 10f;
    if ((double) this.aircraft.rb.velocity.y < 0.0)
      a = Mathf.Min(a, this.aircraft.transform.position.GlobalY() / -this.aircraft.rb.velocity.y);
    RaycastHit hitInfo;
    if (Physics.Linecast(start, start + this.aircraft.rb.velocity * 20f, out hitInfo, 8256) && (double) hitInfo.point.y > (double) Datum.LocalSeaY)
    {
      this.obstacleNormal = hitInfo.normal;
      a = Mathf.Min(a, hitInfo.distance / this.aircraft.speed);
    }
    else
    {
      float enter;
      if (Datum.WaterPlane().Raycast(new Ray(this.aircraft.transform.position, this.aircraft.rb.velocity), out enter))
      {
        this.obstacleNormal = Vector3.up;
        a = Mathf.Min(a, enter / this.aircraft.speed);
      }
    }
    return a;
  }

  public virtual void AutoLand(
    GlobalPosition destination,
    float heightAboveGlideslope,
    float touchdownDist,
    float altitudeHold)
  {
  }

  public virtual void Hover(GlobalPosition destination, float altitudeHold, Vector3 aimDirection)
  {
    Vector3 vector = destination - this.aircraft.GlobalPosition();
    float num = vector.y + altitudeHold;
    vector.y = 0.0f;
    Vector3 vector3_1 = (vector - this.hoverErrorPrev) / Time.fixedDeltaTime;
    this.hoverErrorPrev = vector;
    Vector3 vector3_2 = Vector3.up * 20f + Vector3.ClampMagnitude((Vector3.ClampMagnitude(vector, 100f) + vector3_1 * 6f) * 0.5f, 6f);
    float angleOnAxis1 = TargetCalc.GetAngleOnAxis(this.aircraft.transform.up, vector3_2, this.aircraft.transform.right);
    float angleOnAxis2 = TargetCalc.GetAngleOnAxis(vector3_2, this.aircraft.transform.up, this.aircraft.transform.forward);
    float y = -TargetCalc.GetAngleOnAxis(aimDirection, this.aircraft.transform.forward, this.aircraft.transform.up);
    Vector3 localAngularVelocity = this.aircraft.rb.transform.InverseTransformDirection(this.aircraft.rb.angularVelocity);
    this.verticalVelocitySmoothed = Mathf.SmoothDamp(this.verticalVelocitySmoothed, this.aircraft.rb.velocity.y, ref this.verticalVelocitySmoothingVel, 0.5f);
    this.controlInputs.throttle = Mathf.Clamp01((float) (0.5 + (double) Mathf.Clamp(num * 0.25f, -1f, 1f) - (double) this.verticalVelocitySmoothed * 0.25));
    this.hoverController.ApplyInputs(this.controlInputs, new Vector3(angleOnAxis1, y, angleOnAxis2), localAngularVelocity);
    this.controlInputs.pitch *= 2f;
    this.controlInputs.roll *= 2f;
    this.controlInputs.yaw *= 2f;
    this.aircraft.FilterInputs();
  }

  public virtual void AutoAim(
    GlobalPosition destination,
    bool aimVelocity,
    bool ignoreCollisions,
    bool runwayAlign,
    float effort,
    float bankAllowed,
    bool followTerrain,
    float altitudeHold,
    Vector3 targetVelocity)
  {
  }

  public virtual void BoresightAim(GlobalPosition destination, float desiredHeight)
  {
  }

  public virtual void AutoAim(
    GlobalPosition destination,
    float altitudeHold,
    Vector3 aimDirection,
    Vector3 targetVelocity,
    bool followTerrain)
  {
  }

  [Serializable]
  protected class ForwardFlightController
  {
    public bool Enabled;
    [SerializeField]
    private float referenceAirspeed;
    [SerializeField]
    private PIDFactors pitchFlightPID;
    [SerializeField]
    private PIDFactors yawFlightPID;
    [SerializeField]
    private PIDFactors rollFlightPID;
    private AeroPID xPID;
    private AeroPID yPID;
    private AeroPID zPID;

    public void Initialize()
    {
      if (!this.Enabled)
        return;
      this.xPID = new AeroPID(this.pitchFlightPID, this.referenceAirspeed);
      this.yPID = new AeroPID(this.yawFlightPID, this.referenceAirspeed);
      this.zPID = new AeroPID(this.rollFlightPID, this.referenceAirspeed);
    }

    public void ApplyInputs(ControlInputs inputs, float airspeed, Vector3 error)
    {
      inputs.pitch = Mathf.Clamp(this.xPID.GetOutputClampP(error.x, 20f, 5f, airspeed), -1f, 1f);
      inputs.yaw = Mathf.Clamp(this.yPID.GetOutputClampP(error.y, 60f, 5f, airspeed), -1f, 1f);
      inputs.roll = Mathf.Clamp(this.zPID.GetOutputClampP(error.z, 60f, 5f, airspeed), -1f, 1f);
    }

    public void ApplyInputs(ControlInputs inputs, float airspeed, Vector3 error, float opacity)
    {
      inputs.pitch = Mathf.Lerp(inputs.pitch, Mathf.Clamp(this.xPID.GetOutput(error.x, 3f, airspeed), -1f, 1f), opacity);
      inputs.yaw = Mathf.Lerp(inputs.yaw, Mathf.Clamp(this.yPID.GetOutput(error.y, 3f, airspeed), -1f, 1f), opacity);
      inputs.roll = Mathf.Lerp(inputs.roll, Mathf.Clamp(this.zPID.GetOutput(error.z, 3f, airspeed), -1f, 1f), opacity);
    }
  }

  [Serializable]
  protected class HoverController
  {
    public bool Enabled;
    [SerializeField]
    private PIDFactors pitchHoverPID;
    [SerializeField]
    private PIDFactors yawHoverPID;
    [SerializeField]
    private PIDFactors rollHoverPID;
    private PID xPID;
    private PID yPID;
    private PID zPID;

    public void Initialize()
    {
      if (!this.Enabled)
        return;
      this.xPID = new PID(this.pitchHoverPID);
      this.yPID = new PID(this.yawHoverPID);
      this.zPID = new PID(this.rollHoverPID);
    }

    public void ApplyInputs(ControlInputs inputs, Vector3 error, Vector3 localAngularVelocity)
    {
      inputs.pitch = Mathf.Clamp(this.xPID.GetOutput(error.x, -localAngularVelocity.x, 0.0f, Time.fixedDeltaTime), -1f, 1f);
      inputs.yaw = Mathf.Clamp(this.yPID.GetOutput(error.y, -localAngularVelocity.y, 0.0f, Time.fixedDeltaTime), -1f, 1f);
      inputs.roll = Mathf.Clamp(this.zPID.GetOutput(error.z, localAngularVelocity.z, 0.0f, Time.fixedDeltaTime), -1f, 1f);
    }
  }

  protected class AirspeedChecker
  {
    private float airspeed;
    private float lastCheck;
    private Aircraft aircraft;

    public AirspeedChecker(Aircraft aircraft) => this.aircraft = aircraft;

    public float GetAirspeed()
    {
      if ((double) Time.timeSinceLevelLoad - (double) this.lastCheck < 0.5)
        return this.airspeed;
      this.airspeed = Vector3.Dot(this.aircraft.cockpit.xform.forward, this.aircraft.rb.velocity - NetworkSceneSingleton<LevelInfo>.i.GetWind(this.aircraft.GlobalPosition()));
      return this.airspeed;
    }
  }

  public class ExclusionZoneChecker
  {
    private readonly Aircraft aircraft;
    private float lastCheck;
    private Vector3 avoidanceVector;
    private bool warning;

    public ExclusionZoneChecker(Aircraft aircraft) => this.aircraft = aircraft;

    public bool TryGetWarning(out Vector3 avoidanceVector)
    {
      if ((double) Time.timeSinceLevelLoad - (double) this.lastCheck < 1.0 || (UnityEngine.Object) this.aircraft.NetworkHQ == (UnityEngine.Object) null)
      {
        avoidanceVector = this.avoidanceVector;
        return this.warning;
      }
      this.lastCheck = Time.timeSinceLevelLoad;
      this.avoidanceVector = Vector3.zero;
      this.warning = false;
      foreach (ExclusionZone exclusionZone in this.aircraft.NetworkHQ.GetExclusionZones())
      {
        if (FastMath.InRange(this.aircraft.GlobalPosition(), exclusionZone.position, exclusionZone.radius) || FastMath.InRange(this.aircraft.GlobalPosition() + this.aircraft.rb.velocity * 10f, exclusionZone.position, exclusionZone.radius * 1.1f))
        {
          this.avoidanceVector += (this.aircraft.GlobalPosition() - exclusionZone.position).normalized;
          this.warning = true;
        }
      }
      avoidanceVector = this.avoidanceVector;
      return this.warning;
    }
  }
}

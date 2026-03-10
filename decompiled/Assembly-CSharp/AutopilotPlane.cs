// Decompiled with JetBrains decompiler
// Type: AutopilotPlane
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.DebugScripts;
using UnityEngine;

#nullable disable
public class AutopilotPlane : Autopilot
{
  [SerializeField]
  private bool preventInvertedFlight;
  [SerializeField]
  private AoALimiter aoaLimiter;
  private float onTargetSmoothed;
  private Vector3 targetVelPrev;
  private Vector3 targetAccel;
  private GameObject errorVectorDebug;

  public override void Awake()
  {
    base.Awake();
    this.aoaLimiter.Initialize(this.aircraft);
    this.terrainWarning = new TerrainWarningSystem(this.aircraft);
  }

  public override void AutoAim(
    GlobalPosition destination,
    bool aimVelocity,
    bool ignoreCollisions,
    bool runwayAlign,
    float effort,
    float bankAllowed,
    bool followTerrain,
    float altitudeHold,
    Vector3 targetVel)
  {
    Vector3 avoidanceVector;
    if (this.exclusionZoneChecker.TryGetWarning(out avoidanceVector))
    {
      destination = this.aircraft.GlobalPosition() + avoidanceVector.normalized * 10000f;
      this.controlInputs.throttle = 1f;
    }
    Vector3 vector3_1 = !aimVelocity || (double) this.aircraft.radarAlt <= 0.20000000298023224 ? this.aircraft.cockpit.xform.forward : this.aircraft.rb.velocity;
    if (!aimVelocity && this.aircraft.weaponManager.currentWeaponStation != null && this.aircraft.weaponManager.currentWeaponStation.WeaponInfo.gun)
    {
      foreach (Weapon weapon in this.aircraft.weaponManager.currentWeaponStation.Weapons)
        vector3_1 += weapon.transform.forward * 1000f;
    }
    float a1 = Vector3.Angle(vector3_1, destination - this.aircraft.GlobalPosition());
    float landingSpeed = this.aircraftParameters.landingSpeed;
    float airspeed = this.airspeedChecker.GetAirspeed();
    float num1 = FastMath.Distance(destination, this.aircraft.GlobalPosition());
    altitudeHold *= Mathf.Lerp(0.1f, 1f, (airspeed - landingSpeed * 0.9f) / landingSpeed);
    float a2 = (double) effort > 1.0 || (double) this.aircraft.radarAlt < 1.0 ? 1f : Mathf.Clamp01(airspeed / this.aircraftParameters.cornerSpeed);
    if (followTerrain)
    {
      this.waypoint = this.terrainWarning.GetFollowTerrainWaypoint(Vector3.RotateTowards(new Vector3(vector3_1.x, 0.0f, vector3_1.z), FastMath.NormalizedDirection(this.aircraft.GlobalPosition(), destination), 0.9f * a2 * a2, 0.0f), altitudeHold, (Autopilot) this);
      this.waypointDelta = this.waypoint - this.aircraft.GlobalPosition();
    }
    else
    {
      Vector3 vector3_2 = vector3_1.normalized * 1000f;
      this.waypointDelta = Vector3.RotateTowards(new Vector3(vector3_2.x, 0.0f, vector3_2.z), FastMath.NormalizedDirection(this.aircraft.GlobalPosition(), destination), 0.9f * a2 * a2, 0.0f);
    }
    if ((double) a1 > 60.0 && (double) num1 < 2000.0 && (double) this.aircraft.GlobalPosition().y - (double) destination.y < 1000.0)
      this.waypointDelta = this.waypointDelta + 2000f * Mathf.Clamp01((float) ((double) airspeed / (double) this.aircraftParameters.cornerSpeed - 1.0)) * Vector3.up;
    this.terrainWarning.CheckTerrain();
    if (!ignoreCollisions && (double) this.terrainWarning.urgency > 0.0)
    {
      this.waypointDelta = this.waypointDelta + this.terrainWarning.urgency * 100f * Vector3.up;
      targetVel += Vector3.up * 200f;
    }
    Vector3 normalized1 = this.waypointDelta.normalized;
    Vector3 vector3_3 = normalized1;
    bool flag = (double) targetVel.sqrMagnitude > 900.0;
    Vector3 normalized2 = vector3_3.normalized;
    if (!flag)
      normalized2 += Vector3.up / Mathf.Max(a1, 5f);
    Vector3 vector3_4 = normalized2 + (float) (((double) a1 - 10.0) * (1.0 / 1000.0)) * Vector3.up;
    float y = TargetCalc.GetAngleOnAxis(this.aircraft.cockpit.xform.forward, normalized1, this.aircraft.cockpit.xform.up);
    if (runwayAlign || (double) this.aircraft.radarAlt < 0.5)
      y = Mathf.Clamp(y * 3f, -20f, 20f) - 30f * this.aircraft.rb.angularVelocity.y;
    if (!aimVelocity && !flag && (double) a1 < 10.0)
      vector3_4 = Vector3.up + this.aircraft.cockpit.xform.right * Mathf.Clamp(y * 0.25f, -0.3f, 0.3f);
    if (this.preventInvertedFlight)
      bankAllowed = Mathf.Min(bankAllowed, 135f);
    bankAllowed *= Mathf.Clamp((float) ((double) this.aircraft.radarAlt * (3.0 / 1000.0) - 1.0), 0.7f, 1.2f);
    bankAllowed *= Mathf.Clamp((float) (1.0 - (double) normalized1.y * 0.20000000298023224), 0.7f, 1.2f);
    bankAllowed *= Mathf.Max(a2, 0.7f);
    float z = 0.0f;
    if ((double) this.aircraft.radarAlt > 1.0)
      z = (double) bankAllowed >= 180.0 ? TargetCalc.GetAngleOnAxis(this.aircraft.cockpit.xform.up, vector3_4, -vector3_1) : TargetCalc.GetAngleOnAxis(this.aircraft.cockpit.xform.up, Vector3.up, -vector3_1) - Mathf.Clamp(TargetCalc.GetAngleOnAxis(vector3_4, Vector3.up, -vector3_1), -bankAllowed, bankAllowed);
    float num2 = TargetCalc.GetAngleOnAxis(vector3_1, normalized1, this.aircraft.cockpit.xform.right);
    if ((double) Mathf.Abs(TargetCalc.GetAngleOnAxis(vector3_1, normalized1, Vector3.up)) > 20.0)
      num2 = Mathf.Min(num2, 0.0f);
    if ((double) a1 > 20.0 && !runwayAlign && (double) this.aircraft.radarAlt > 0.5)
      y = 0.0f;
    this.forwardFlightController.ApplyInputs(this.controlInputs, airspeed, new Vector3(num2, y, z));
    this.aircraft.FilterInputs();
    if (!DebugVis.Enabled)
      return;
    DebugVis.Create<GameObject>(ref this.waypointDebug, GameAssets.i.waypointDebug, Datum.origin);
    DebugVis.Create<GameObject>(ref this.aimVectorDebug, GameAssets.i.debugArrow, this.aircraft.cockpit.transform);
    DebugVis.Create<GameObject>(ref this.vectorDebug2, GameAssets.i.debugArrow, Datum.origin);
    DebugVis.Create<GameObject>(ref this.errorVectorDebug, GameAssets.i.debugArrow, this.aircraft.transform);
    if ((Object) SceneSingleton<CameraStateManager>.i.followingUnit == (Object) this.aircraft)
    {
      this.errorVectorDebug.transform.position = this.aircraft.transform.position + this.aircraft.transform.forward * 10f;
      this.errorVectorDebug.transform.rotation = Quaternion.LookRotation(vector3_4);
      this.errorVectorDebug.transform.localScale = new Vector3(2f, 2f, 5f);
      this.waypointDebug.SetActive(true);
      this.aimVectorDebug.SetActive(true);
      this.waypointDebug.transform.position = this.aircraft.transform.position + this.waypointDelta;
      this.waypointDebug.transform.rotation = Quaternion.LookRotation(this.waypointDelta);
      this.waypointDebug.transform.localScale = Vector3.one * 2f;
      this.aimVectorDebug.transform.localPosition = Vector3.zero;
      this.aimVectorDebug.transform.rotation = Quaternion.LookRotation(this.waypointDelta);
      this.aimVectorDebug.transform.localScale = new Vector3(2f, 2f, 1000f);
    }
    else
    {
      this.waypointDebug.SetActive(false);
      this.aimVectorDebug.SetActive(false);
    }
  }
}

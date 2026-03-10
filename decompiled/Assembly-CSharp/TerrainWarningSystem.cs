// Decompiled with JetBrains decompiler
// Type: TerrainWarningSystem
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

#nullable disable
public class TerrainWarningSystem
{
  public float urgency;
  private float lastCheck;
  private float lastTerrainWaypoint;
  private Aircraft aircraft;
  private GlobalPosition followTerrainWaypoint;

  public TerrainWarningSystem(Aircraft aircraft) => this.aircraft = aircraft;

  public void CheckTerrain()
  {
    if ((double) Time.timeSinceLevelLoad - (double) this.lastCheck < 0.5)
      return;
    this.lastCheck = Time.timeSinceLevelLoad;
    Vector3 normalized = this.aircraft.rb.velocity.normalized;
    float num1 = this.aircraft.speed * 5f;
    Vector3 vector3 = this.aircraft.cockpit.xform.position - Vector3.up * this.aircraft.maxRadius;
    Vector3 direction = normalized * num1;
    float num2 = float.MaxValue;
    Vector3 rhs = Vector3.up;
    float enter;
    if (Datum.WaterPlane().Raycast(new Ray(vector3, direction), out enter) && (double) enter < (double) num1 && (double) enter > 0.0)
    {
      num2 = enter;
      if ((Object) this.aircraft == (Object) SceneSingleton<CameraStateManager>.i.followingUnit)
        Debug.Log((object) $"Water impact detected at distance {num2}");
    }
    RaycastHit hitInfo;
    if (Physics.Linecast(vector3, vector3 + direction, out hitInfo, 8256) && (double) hitInfo.distance < (double) num2)
    {
      num2 = hitInfo.distance;
      rhs = hitInfo.normal;
    }
    this.urgency = (double) num2 < (double) num1 ? this.urgency + Mathf.Clamp(Vector3.Dot(-normalized, rhs), 0.25f, 1f) * num1 / num2 : 0.0f;
  }

  public GlobalPosition GetFollowTerrainWaypoint(
    Vector3 direction,
    float altitudeTarget,
    Autopilot autopilot)
  {
    if ((double) Time.timeSinceLevelLoad - (double) this.lastTerrainWaypoint > 0.5)
    {
      this.lastTerrainWaypoint = Time.timeSinceLevelLoad;
      this.followTerrainWaypoint = autopilot.TerrainWaypoint(direction, altitudeTarget, (float) ((double) Mathf.Max(this.aircraft.speed, 100f) * 6.0 * (9.0 / (double) this.aircraft.GetAircraftParameters().aircraftGLimit)));
    }
    return this.followTerrainWaypoint;
  }
}

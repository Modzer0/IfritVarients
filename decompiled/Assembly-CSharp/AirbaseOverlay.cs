// Decompiled with JetBrains decompiler
// Type: AirbaseOverlay
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;
using UnityEngine.UI;

#nullable disable
public class AirbaseOverlay : MonoBehaviour
{
  [SerializeField]
  private Image airbaseMarker;
  [SerializeField]
  private Text airbaseLabel;
  private Airbase nearestAirbase;
  private Airbase.Runway.RunwayUsage? runwayUsage;
  private bool landing;
  private bool taxiingToRunway;
  private bool reachedRunway;
  private string runwayName;
  private float takeoffTime;
  private Airbase.Runway landedAtRunway;
  [SerializeField]
  private Image[] runwayBorders;
  [SerializeField]
  private Image glideslope;
  [SerializeField]
  private Image glideslopeAimPoint;
  private Vector3[] runwayCorners;
  private GlobalPosition landingApproach;

  private void Awake()
  {
    this.glideslope.enabled = false;
    this.glideslopeAimPoint.enabled = false;
    foreach (Behaviour runwayBorder in this.runwayBorders)
      runwayBorder.enabled = false;
    this.runwayCorners = new Vector3[4];
  }

  private void OnEnable()
  {
    this.airbaseMarker.enabled = false;
    this.airbaseLabel.enabled = false;
    this.StartSlowUpdateDelayed(2f, new Action(this.UpdateNearestAirbase));
  }

  private void ShowRunwayBorders(bool show)
  {
    foreach (Behaviour runwayBorder in this.runwayBorders)
      runwayBorder.enabled = show;
  }

  private void DrawRunwayBorders(Airbase.Runway runway)
  {
    Camera mainCamera = SceneSingleton<CameraStateManager>.i.mainCamera;
    float width = runway.GetWidth();
    Transform start = runway.Start;
    Transform end = runway.End;
    Vector3 position1 = start.position - 0.5f * width * start.right;
    Vector3 position2 = start.position + 0.5f * width * start.right;
    Vector3 position3 = end.position + 0.5f * width * end.right;
    Vector3 position4 = end.position - 0.5f * width * end.right;
    this.ShowRunwayBorders((double) Vector3.Dot(mainCamera.transform.forward, position1 - mainCamera.transform.position) > 0.0 && (double) Vector3.Dot(mainCamera.transform.forward, position2 - mainCamera.transform.position) > 0.0 && (double) Vector3.Dot(mainCamera.transform.forward, position3 - mainCamera.transform.position) > 0.0 && (double) Vector3.Dot(mainCamera.transform.forward, position4 - mainCamera.transform.position) > 0.0);
    float num = 1080f / (float) Screen.height;
    this.runwayCorners[0] = mainCamera.WorldToScreenPoint(position1);
    this.runwayCorners[1] = mainCamera.WorldToScreenPoint(position2);
    this.runwayCorners[2] = mainCamera.WorldToScreenPoint(position3);
    this.runwayCorners[3] = mainCamera.WorldToScreenPoint(position4);
    this.runwayCorners[0].z = 0.0f;
    this.runwayCorners[1].z = 0.0f;
    this.runwayCorners[2].z = 0.0f;
    this.runwayCorners[3].z = 0.0f;
    this.runwayBorders[0].transform.position = this.runwayCorners[0];
    Vector3 vector3_1 = this.runwayCorners[1] - this.runwayCorners[0];
    this.runwayBorders[0].transform.eulerAngles = new Vector3(0.0f, 0.0f, (float) (-(double) Mathf.Atan2(vector3_1.x, vector3_1.y) * 57.295780181884766));
    this.runwayBorders[0].transform.localScale = Vector3.one + Vector3.up * vector3_1.magnitude * num;
    this.runwayBorders[1].transform.position = this.runwayCorners[1];
    Vector3 vector3_2 = this.runwayCorners[2] - this.runwayCorners[1];
    this.runwayBorders[1].transform.eulerAngles = new Vector3(0.0f, 0.0f, (float) (-(double) Mathf.Atan2(vector3_2.x, vector3_2.y) * 57.295780181884766));
    this.runwayBorders[1].transform.localScale = Vector3.one + Vector3.up * vector3_2.magnitude * num;
    this.runwayBorders[2].transform.position = this.runwayCorners[2];
    Vector3 vector3_3 = this.runwayCorners[3] - this.runwayCorners[2];
    this.runwayBorders[2].transform.eulerAngles = new Vector3(0.0f, 0.0f, (float) (-(double) Mathf.Atan2(vector3_3.x, vector3_3.y) * 57.295780181884766));
    this.runwayBorders[2].transform.localScale = Vector3.one + Vector3.up * vector3_3.magnitude * num;
    this.runwayBorders[3].transform.position = this.runwayCorners[3];
    Vector3 vector3_4 = this.runwayCorners[0] - this.runwayCorners[3];
    this.runwayBorders[3].transform.eulerAngles = new Vector3(0.0f, 0.0f, (float) (-(double) Mathf.Atan2(vector3_4.x, vector3_4.y) * 57.295780181884766));
    this.runwayBorders[3].transform.localScale = Vector3.one + Vector3.up * vector3_4.magnitude * num;
  }

  private bool DrawGlideslope(Aircraft aircraft, Airbase.Runway.RunwayUsage runwayUsage)
  {
    if ((double) aircraft.radarAlt < 1.0)
      return false;
    Camera mainCamera = SceneSingleton<CameraStateManager>.i.mainCamera;
    Vector3 position1 = runwayUsage.GetEnd().position;
    float num1 = FastMath.Distance(aircraft.transform.position, position1);
    Vector3 velocity = runwayUsage.Runway.GetVelocity();
    float num2 = Vector3.Dot(aircraft.rb.velocity - velocity, (position1 - aircraft.transform.position).normalized);
    float num3 = num1 / num2;
    Vector3 glideslopeAimpoint = runwayUsage.Runway.GetGlideslopeAimpoint(aircraft, num1 * 0.9f, runwayUsage.Reverse, num3 * 0.9f);
    Vector3 position2 = position1;
    if ((double) Vector3.Dot(glideslopeAimpoint - mainCamera.transform.position, mainCamera.transform.forward) < 0.0 || (double) Vector3.Dot(position2 - mainCamera.transform.position, mainCamera.transform.forward) < 0.0)
      return false;
    Vector3 screenPoint1 = mainCamera.WorldToScreenPoint(glideslopeAimpoint);
    Vector3 screenPoint2 = mainCamera.WorldToScreenPoint(position2);
    screenPoint1.z = 0.0f;
    screenPoint2.z = 0.0f;
    this.glideslope.transform.position = screenPoint2;
    Vector3 vector3 = screenPoint1 - screenPoint2;
    this.glideslope.transform.eulerAngles = new Vector3(0.0f, 0.0f, (float) (-(double) Mathf.Atan2(vector3.x, vector3.y) * 57.295780181884766));
    this.glideslope.transform.localScale = Vector3.one + Vector3.up * (float) ((double) vector3.magnitude * (1080.0 / (double) Screen.height) - 8.0);
    this.glideslopeAimPoint.transform.position = screenPoint1;
    return true;
  }

  private void UpdateNearestAirbase()
  {
    Aircraft aircraft = SceneSingleton<CombatHUD>.i.aircraft;
    if ((UnityEngine.Object) aircraft == (UnityEngine.Object) null)
    {
      this.landing = false;
      this.taxiingToRunway = false;
      this.reachedRunway = false;
      this.takeoffTime = 0.0f;
    }
    else
    {
      if (this.landedAtRunway != null && !this.landedAtRunway.AircraftOnRunway(aircraft))
      {
        this.landedAtRunway.DeregisterLanding(aircraft);
        this.landedAtRunway = (Airbase.Runway) null;
      }
      AircraftParameters aircraftParameters = aircraft.GetAircraftParameters();
      float takeoffDistance = aircraftParameters.takeoffDistance;
      if ((double) aircraft.radarAlt > 1.0)
      {
        this.takeoffTime += 0.5f;
        if ((double) this.takeoffTime >= 1.0)
          aircraft.pilots[0].flightInfo.HasTakenOff = true;
      }
      if (!aircraft.pilots[0].flightInfo.HasTakenOff)
      {
        RunwayQuery runwayQuery = new RunwayQuery()
        {
          RunwayType = RunwayQueryType.Any,
          MinSize = 10f,
          LandingSpeed = 1f
        };
        if (!aircraft.NetworkHQ.AnyNearAirbase(aircraft.transform.position, out this.nearestAirbase) || (double) takeoffDistance <= 0.0 || this.taxiingToRunway)
          return;
        this.runwayUsage = this.nearestAirbase.GetTakeoffRunway(aircraft, takeoffDistance);
        if (!this.runwayUsage.HasValue)
          return;
        this.runwayName = this.runwayUsage.Value.GetName();
        SceneSingleton<AircraftActionsReport>.i.ReportText("Cleared to taxi to " + this.runwayName, 10f);
        this.nearestAirbase.CmdRegisterUsage(aircraft, true, new byte?());
        this.taxiingToRunway = true;
      }
      else
      {
        float num = Mathf.Sqrt(aircraft.GetMass() / aircraft.definition.aircraftInfo.maxWeight) * aircraftParameters.takeoffSpeed;
        RunwayQuery runwayQuery = new RunwayQuery()
        {
          RunwayType = RunwayQueryType.Any,
          MinSize = takeoffDistance,
          TailHook = aircraft.weaponManager.HasTailHook(),
          LandingSpeed = num
        };
        this.nearestAirbase = aircraft.NetworkHQ.GetNearestAirbase(aircraft.transform.position, runwayQuery);
        if ((UnityEngine.Object) this.nearestAirbase == (UnityEngine.Object) null)
          return;
        if (!aircraftParameters.verticalLanding && FastMath.InRange(this.nearestAirbase.center.position, aircraft.transform.position, this.nearestAirbase.GetRadius() + 5000f))
        {
          this.runwayUsage = this.nearestAirbase.RequestLanding(aircraft, runwayQuery);
          if (!this.landing && !aircraftParameters.verticalLanding && (double) aircraft.radarAlt > 20.0 && aircraft.gearDeployed && this.runwayUsage.HasValue && this.runwayUsage.Value.Runway.AircraftOnApproach(aircraft, 2500f, true) && (double) Mathf.Abs(Vector3.Dot(aircraft.transform.forward, this.runwayUsage.Value.GetDirection())) > 0.800000011920929)
          {
            string name = this.runwayUsage.Value.GetName();
            SceneSingleton<AircraftActionsReport>.i.ReportText("Cleared for landing on " + name, 10f);
            this.nearestAirbase.CmdRegisterUsage(aircraft, true, new byte?(this.runwayUsage.Value.Runway.index));
            this.ShowRunwayBorders(true);
            this.landing = true;
          }
          if (!this.runwayUsage.HasValue || !this.landing)
            return;
          Airbase.Runway runway = this.runwayUsage.Value.Runway;
          if ((double) aircraft.radarAlt < 0.20000000298023224)
          {
            this.landedAtRunway = runway;
            this.landing = false;
            this.runwayUsage = new Airbase.Runway.RunwayUsage?();
            this.ShowRunwayBorders(false);
          }
          else
          {
            if ((double) aircraft.radarAlt <= 10.0 || runway.AircraftOnApproach(aircraft, 2500f, false))
              return;
            SceneSingleton<AircraftActionsReport>.i.ReportText("Aborted Landing", 5f);
            this.ShowRunwayBorders(false);
            runway.DeregisterLanding(aircraft);
            this.landing = false;
            this.runwayUsage = new Airbase.Runway.RunwayUsage?();
          }
        }
        else
          this.runwayUsage = new Airbase.Runway.RunwayUsage?();
      }
    }
  }

  private bool PositionMarkers(Aircraft aircraft)
  {
    if ((UnityEngine.Object) this.nearestAirbase == (UnityEngine.Object) null)
      return false;
    Vector3 position = this.nearestAirbase.center.position;
    if (this.landing || (double) aircraft.radarAlt < 1.0 && !this.taxiingToRunway)
      return false;
    string str = this.nearestAirbase.SavedAirbase.DisplayName;
    if (!aircraft.pilots[0].flightInfo.HasTakenOff)
    {
      if (!this.taxiingToRunway || this.reachedRunway || !this.runwayUsage.HasValue)
        return false;
      Transform end = this.runwayUsage.Value.GetEnd();
      position = end.position;
      str = "Taxi to " + this.runwayName;
      if (FastMath.InRange(end.position, aircraft.transform.position, aircraft.maxRadius + 20f))
        this.reachedRunway = true;
    }
    Vector3 lhs = position - SceneSingleton<CameraStateManager>.i.transform.position;
    float magnitude = lhs.magnitude;
    if ((double) Vector3.Dot(lhs, SceneSingleton<CameraStateManager>.i.transform.forward) < 0.0)
      return false;
    Vector3 rayToScreen;
    if (HUDFunctions.PinToScreenEdge(position, out rayToScreen, out float _))
    {
      this.airbaseMarker.transform.position = rayToScreen;
      this.airbaseLabel.transform.position = rayToScreen - rayToScreen.normalized * 50f;
    }
    else
    {
      this.airbaseMarker.transform.position = rayToScreen;
      this.airbaseLabel.transform.position = rayToScreen - Vector3.up * 20f;
    }
    this.airbaseLabel.text = $"{str} {UnitConverter.DistanceReading(magnitude)}";
    this.airbaseLabel.fontSize = (int) PlayerSettings.overlayTextSize;
    return true;
  }

  private void DisplayMarkers(bool show)
  {
    if (show)
    {
      if (this.airbaseMarker.enabled)
        return;
      this.airbaseMarker.enabled = true;
      this.airbaseLabel.enabled = true;
    }
    else
    {
      if (!this.airbaseMarker.enabled)
        return;
      this.airbaseMarker.enabled = false;
      this.airbaseLabel.enabled = false;
    }
  }

  private void LateUpdate()
  {
    Aircraft aircraft = SceneSingleton<CombatHUD>.i.aircraft;
    if ((UnityEngine.Object) aircraft == (UnityEngine.Object) null)
      return;
    this.DisplayMarkers(this.PositionMarkers(aircraft));
    if (this.landing && this.runwayUsage.HasValue)
      this.DrawRunwayBorders(this.runwayUsage.Value.Runway);
    if (this.runwayUsage.HasValue && this.runwayUsage.Value.Runway != null && aircraft.gearDeployed)
    {
      this.glideslope.enabled = this.DrawGlideslope(aircraft, this.runwayUsage.Value);
      this.glideslopeAimPoint.enabled = this.glideslope.enabled;
    }
    else
    {
      this.glideslope.enabled = false;
      this.glideslopeAimPoint.enabled = false;
    }
  }
}

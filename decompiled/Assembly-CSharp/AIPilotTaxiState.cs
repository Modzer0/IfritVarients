// Decompiled with JetBrains decompiler
// Type: AIPilotTaxiState
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using RoadPathfinding;
using System;
using System.Collections.Generic;
using Unity.Profiling;
using UnityEngine;

#nullable disable
public class AIPilotTaxiState : PilotBaseState
{
  private const int MAX_STUCK_TIME_SPEED = 30;
  private const int MAX_STUCK_TIME_YAW = 60;
  private static readonly ProfilerMarker fixedUpdateStateMarker = new ProfilerMarker("AIPilotTaxiState.FixedUpdateState");
  private RoadNetwork taxiNetwork;
  private Airbase airbase;
  private PathfindingAgent pathfinder;
  private AircraftParameters aircraftParameters;
  private List<Obstacle> obstacles = new List<Obstacle>();
  private float lastObstacleRefresh;
  private float lastObstacleCheck;
  private float lastRunwayCrossingCheck;
  private float lastRunwayClearanceCheck;
  private float brakeUrgency;
  private float lastAirbaseSearch;
  private float lastTaxiClearanceCheck;
  private float startupTime;
  private bool toRunway;
  private bool yielding;
  private bool waitingAtRunwayCrossing;
  private bool waitingForTakeoffClearance;
  private bool taxiClearance;
  private bool takeoffQueued;
  private bool disembarking;
  private bool takeoffCleared;
  private Transform exitTaxiPoint;
  private Transform destinationPoint;
  private UnitPart mainPart;
  private Vector3 obstacleAvoidVector;
  private GlobalPosition yieldPosition;
  private Airbase.Runway takeoffRunway;
  private GlobalPosition stuckRealPosition;
  private float stuckPreviousYawSign;
  private float stuckTimerSpeed;
  private float stuckTimerYaw;

  public float stuckTimerSpeedPercent => this.stuckTimerSpeed / 30f;

  public float stuckTimerYawPercent => this.stuckTimerYaw / 60f;

  public override void EnterState(Pilot pilot)
  {
    this.pilot = pilot;
    this.takeoffRunway = (Airbase.Runway) null;
    this.stateDisplayName = "taxiing";
    this.airbase = (Airbase) null;
    this.aircraft = pilot.aircraft;
    this.lastAirbaseSearch = -3f;
    if (this.pathfinder == null)
      this.pathfinder = new PathfindingAgent((Unit) this.aircraft);
    this.exitTaxiPoint = (Transform) null;
    this.destinationPoint = (Transform) null;
    this.toRunway = false;
    this.takeoffQueued = false;
    this.takeoffCleared = false;
    this.waitingForTakeoffClearance = false;
    this.waitingAtRunwayCrossing = false;
    this.stuckTimerSpeed = 0.0f;
    this.stuckTimerYaw = 0.0f;
    this.obstacles.Clear();
    this.aircraftParameters = this.aircraft.GetAircraftParameters();
    this.mainPart = this.aircraft.gameObject.GetComponent<UnitPart>();
    this.mainPart.onApplyDamage += new Action<UnitPart.OnApplyDamage>(this.Taxi_OnTakeDamage);
    this.controlInputs = this.aircraft.GetInputs();
    this.taxiClearance = pilot.flightInfo.HasTakenOff;
    this.startupTime = pilot.flightInfo.HasTakenOff ? 100f : 0.0f;
  }

  private bool WaitingAtRunwayCrossing(Airbase airbase)
  {
    if ((double) Time.timeSinceLevelLoad - (double) this.lastRunwayCrossingCheck < 1.0)
      return this.waitingAtRunwayCrossing;
    this.lastRunwayCrossingCheck = Time.timeSinceLevelLoad;
    if (airbase.AircraftApproachingRunwayInUse(this.aircraft))
    {
      this.stateDisplayName = "waiting at runway crossing";
      this.waitingAtRunwayCrossing = true;
    }
    else
    {
      this.stateDisplayName = "taxiing to takeoff";
      this.waitingAtRunwayCrossing = false;
    }
    return this.waitingAtRunwayCrossing;
  }

  private bool WaitingForTakeoffClearance(float destinationDist)
  {
    if ((double) Time.timeSinceLevelLoad - (double) this.lastRunwayClearanceCheck < 1.0)
      return this.waitingForTakeoffClearance;
    this.lastRunwayClearanceCheck = Time.timeSinceLevelLoad;
    if ((double) destinationDist > 100.0)
    {
      this.waitingForTakeoffClearance = false;
      return false;
    }
    if ((double) destinationDist < 12.0)
    {
      if (!this.takeoffQueued)
        this.takeoffRunway.QueueTakeoff(this.aircraft);
      this.pilot.AITakeoffState = new AIPilotTakeoffState();
      this.pilot.SwitchState((PilotBaseState) this.pilot.AITakeoffState);
      return false;
    }
    bool flag = this.takeoffRunway.IsAvailableForTakeoff(this.aircraft);
    if (flag && !this.takeoffQueued && (double) this.brakeUrgency < 0.10000000149011612)
    {
      this.takeoffRunway.QueueTakeoff(this.aircraft);
      this.takeoffQueued = true;
    }
    if ((double) this.brakeUrgency > 0.10000000149011612 && this.takeoffQueued)
    {
      this.takeoffRunway.DequeueTakeoff(this.aircraft);
      this.takeoffQueued = false;
      flag = false;
    }
    if (flag)
    {
      this.stateDisplayName = "entering takeoff runway";
      this.waitingForTakeoffClearance = false;
    }
    else
    {
      this.stateDisplayName = "waiting for takeoff clearance";
      this.waitingForTakeoffClearance = true;
    }
    return this.waitingForTakeoffClearance;
  }

  private void Disembark()
  {
    this.controlInputs.brake = 1f;
    this.controlInputs.throttle = 0.0f;
    if ((double) this.aircraft.speed >= 1.0)
      return;
    this.pilot.aircraft.StartEjectionSequence();
    this.pilot.SwitchState((PilotBaseState) this.pilot.parkedState);
  }

  private void SearchForAirbase()
  {
    if ((double) Time.timeSinceLevelLoad - (double) this.lastAirbaseSearch < 3.0)
      return;
    this.lastAirbaseSearch = Time.timeSinceLevelLoad;
    this.stateDisplayName = "orienting to nearest airbase";
    if (this.aircraftParameters.verticalLanding)
    {
      this.aircraft.SetFlightAssist(false);
      this.controlInputs.customAxis1 = 1f;
    }
    if (this.aircraft.NetworkHQ.AnyNearAirbase(this.aircraft.transform.position, out this.airbase))
    {
      this.taxiNetwork = this.airbase.GetTaxiNetwork();
      if (this.pilot.flightInfo.HasTakenOff)
      {
        this.stateDisplayName = "taxiing to resupply";
        Airbase.Runway outRunway;
        if (this.airbase.AircraftIsOnRunway(this.aircraft, true, out outRunway))
        {
          if (!this.taxiNetwork.Exists())
          {
            Transform nearestServicePoint;
            if (this.airbase.TryGetNearestServicePoint(this.aircraft.transform.position, out nearestServicePoint))
            {
              this.destinationPoint = nearestServicePoint;
              this.pathfinder.SetMovingTarget(this.destinationPoint);
            }
            else
              this.disembarking = true;
          }
          else
          {
            Transform exitPoint;
            if (outRunway.TryGetExitTaxiPoint(this.aircraft.transform, 0.0f, out exitPoint))
            {
              this.exitTaxiPoint = exitPoint;
              this.pathfinder.Pathfind((RoadNetwork) null, this.exitTaxiPoint.GlobalPosition(), false, (Transform) null);
            }
            else
              this.disembarking = true;
          }
        }
        else
          this.disembarking = true;
      }
      else
      {
        this.airbase.RpcRegisterUsage(this.aircraft, true, new byte?());
        this.stateDisplayName = "taxiing to runway";
        Airbase.Runway.RunwayUsage? takeoffRunway = this.airbase.GetTakeoffRunway(this.aircraft, 0.0f);
        if (!takeoffRunway.HasValue)
          return;
        this.takeoffRunway = takeoffRunway.Value.Runway;
        this.destinationPoint = takeoffRunway.Value.Reverse ? this.takeoffRunway.End : this.takeoffRunway.Start;
        if (this.taxiNetwork.Exists())
          this.pathfinder.Pathfind(this.taxiNetwork, this.destinationPoint.GlobalPosition(), false, (Transform) null);
        else
          this.pathfinder.SetMovingTarget(this.destinationPoint);
        this.toRunway = true;
      }
    }
    else
      this.stateDisplayName = "continuing to search for airbase";
  }

  private void Taxi_OnTakeDamage(UnitPart.OnApplyDamage _)
  {
    this.disembarking = true;
    this.mainPart.onApplyDamage -= new Action<UnitPart.OnApplyDamage>(this.Taxi_OnTakeDamage);
  }

  private void RefreshObstacles()
  {
    if ((double) Time.timeSinceLevelLoad - (double) this.lastObstacleRefresh < 4.0)
      return;
    this.lastObstacleRefresh = Time.timeSinceLevelLoad;
    this.obstacles.Clear();
    foreach (Aircraft aircraft in this.airbase.ControlledAircraft)
    {
      if (!((UnityEngine.Object) aircraft == (UnityEngine.Object) null) && !((UnityEngine.Object) aircraft == (UnityEngine.Object) this.aircraft) && FastMath.InRange(aircraft.transform.position, aircraft.transform.position, 300f))
        this.obstacles.Add(new Obstacle(aircraft.transform, aircraft.maxRadius, aircraft.obstacleTop));
    }
  }

  private void CheckObstacles()
  {
    if ((double) Time.timeSinceLevelLoad - (double) this.lastObstacleCheck < 1.0)
      return;
    this.brakeUrgency = 0.0f;
    this.lastObstacleCheck = Time.timeSinceLevelLoad;
    this.obstacleAvoidVector = Vector3.zero;
    foreach (Obstacle obstacle in this.obstacles)
    {
      if (!((UnityEngine.Object) obstacle.Transform == (UnityEngine.Object) null))
      {
        Vector3 vector3_1 = obstacle.Transform.position - this.aircraft.transform.position;
        Vector3 normalized = vector3_1.normalized;
        float num1 = Vector3.Dot(normalized, this.aircraft.transform.forward);
        float num2 = Vector3.Dot(-normalized, obstacle.Transform.forward);
        if ((double) Vector3.Dot(normalized, this.aircraft.transform.forward) >= 0.0)
        {
          float num3 = vector3_1.magnitude - (obstacle.Radius + this.aircraft.maxRadius);
          if ((double) num3 < 50.0 && (double) num2 > 0.0)
          {
            if (!this.yielding && this.aircraft.pilots[0].flightInfo.HasTakenOff)
            {
              this.yielding = true;
              Vector3 vector3_2 = Vector3.RotateTowards(this.aircraft.transform.forward, -normalized, 1.57079637f, 0.0f);
              this.yieldPosition = this.aircraft.GlobalPosition() + vector3_2 * 50f;
              break;
            }
            if ((double) num1 <= (double) num2)
              break;
          }
          this.brakeUrgency += (double) this.aircraft.speed > (double) Mathf.Sqrt(Mathf.Max(num3 - 10f, 0.0f)) ? 1f : 0.0f;
        }
      }
    }
  }

  public override void LeaveState()
  {
    this.mainPart.onApplyDamage -= new Action<UnitPart.OnApplyDamage>(this.Taxi_OnTakeDamage);
  }

  private void Wait()
  {
    this.controlInputs.brake = 1f;
    this.controlInputs.throttle = 0.01f;
    this.stuckTimerSpeed = 0.0f;
    this.stuckTimerYaw = 0.0f;
  }

  public override void FixedUpdateState(Pilot pilot)
  {
    using (AIPilotTaxiState.fixedUpdateStateMarker.Auto())
    {
      if ((UnityEngine.Object) pilot.aircraft.rb == (UnityEngine.Object) null)
        return;
      if (this.disembarking || this.IsStuck(pilot.aircraft))
        this.Disembark();
      else if ((UnityEngine.Object) this.airbase == (UnityEngine.Object) null)
      {
        this.SearchForAirbase();
        this.controlInputs.brake = 1f;
        this.controlInputs.throttle = 0.1f;
      }
      else
      {
        if ((UnityEngine.Object) this.exitTaxiPoint != (UnityEngine.Object) null && (double) Vector3.Distance(this.exitTaxiPoint.position, this.aircraft.transform.position) < 20.0)
        {
          this.exitTaxiPoint = (Transform) null;
          if (this.airbase.TryGetNearestServicePoint(this.aircraft.transform.position, out this.destinationPoint))
          {
            RoadNetwork taxiNetwork = this.airbase.GetTaxiNetwork();
            if (taxiNetwork.Exists())
              this.pathfinder.Pathfind(taxiNetwork, this.destinationPoint.GlobalPosition(), false, (Transform) null);
            else
              this.pathfinder.SetMovingTarget(this.destinationPoint);
          }
          else
            this.disembarking = true;
        }
        SteeringInfo? steerpoint = this.pathfinder.GetSteerpoint(pilot.aircraft.GlobalPosition(), pilot.aircraft.transform.forward, this.aircraft.speed * 1.5f, false);
        Vector3 vector3_1;
        float num1;
        if (steerpoint.HasValue)
        {
          SteeringInfo steeringInfo = steerpoint.Value;
          this.controlInputs.throttle = 1f;
          vector3_1 = steeringInfo.steerVector;
          float magnitude = vector3_1.magnitude;
          float num2 = (float) (1.0 + (double) Vector3.Angle(this.aircraft.transform.forward, steeringInfo.steerVector) * 0.019999999552965164);
          float a = ((float) (15.0 / (1.0 + (double) steeringInfo.nextWaypointAngle * 0.019999999552965164)) + Mathf.Min(Mathf.Sqrt(magnitude), 15f)) / num2;
          if (this.takeoffQueued)
            a *= 0.5f;
          num1 = Mathf.Max(a, 4f);
        }
        else
        {
          num1 = 4f;
          this.controlInputs.throttle = 0.0f;
        }
        this.RefreshObstacles();
        this.CheckObstacles();
        this.controlInputs.throttle = Mathf.Clamp((float) (0.5 + ((double) num1 - (double) this.aircraft.speed) * 0.30000001192092896), 0.0f, 0.5f);
        this.controlInputs.brake = (double) this.aircraft.speed > (double) num1 ? 1f : 0.0f;
        this.controlInputs.throttle -= this.brakeUrgency;
        this.controlInputs.brake += this.brakeUrgency;
        if (!steerpoint.HasValue)
          return;
        SteeringInfo steeringInfo1 = steerpoint.Value;
        if ((double) this.controlInputs.throttle < 0.10000000149011612 && (double) this.aircraft.speed < 2.0 || (double) Time.timeSinceLevelLoad - (double) pilot.flightInfo.spawnTime < 8.0)
        {
          this.controlInputs.brake += 0.5f;
          this.stuckTimerSpeed = 0.0f;
          this.stuckTimerYaw = 0.0f;
        }
        this.controlInputs.throttle = Mathf.Max(this.controlInputs.throttle, 0.01f);
        vector3_1 = steeringInfo1.steerVector;
        Vector3 vector3_2 = vector3_1.normalized + this.obstacleAvoidVector;
        if (this.yielding)
        {
          vector3_1 = this.yieldPosition - this.aircraft.GlobalPosition();
          float num3 = (float) (6.0 + (double) Vector3.Dot(vector3_1.normalized, this.aircraft.transform.forward) * 3.0);
          this.controlInputs.throttle = (double) this.aircraft.speed > (double) num3 ? 0.01f : 0.5f;
          this.controlInputs.brake = (double) this.aircraft.speed > (double) num3 ? 0.5f : 0.0f;
          this.aircraft.autopilot.AutoAim(this.yieldPosition, false, true, false, 1f, 0.0f, false, 0.0f, Vector3.zero);
          if (!this.airbase.AttachedAirbase && !FastMath.InRange(this.yieldPosition, this.aircraft.GlobalPosition(), this.aircraft.maxRadius + 20f))
            return;
          this.Disembark();
        }
        else
        {
          this.aircraft.autopilot.AutoAim(this.aircraft.GlobalPosition() + vector3_2 * 20f, false, true, false, 1f, 0.0f, false, 0.0f, Vector3.zero);
          if ((UnityEngine.Object) this.destinationPoint == (UnityEngine.Object) null)
          {
            this.Disembark();
          }
          else
          {
            float destinationDist = Vector3.Distance(this.destinationPoint.position, this.aircraft.transform.position);
            if (this.toRunway)
            {
              if (FastMath.InRange(this.aircraft.startPosition, this.aircraft.GlobalPosition(), 10f))
                this.controlInputs.yaw = 0.0f;
              if (!this.WaitingForTakeoffClearance(destinationDist) && !this.WaitingAtRunwayCrossing(this.airbase))
                return;
              this.Wait();
            }
            else
            {
              if ((double) destinationDist >= (double) this.aircraft.maxRadius * 2.0)
                return;
              this.disembarking = true;
            }
          }
        }
      }
    }
  }

  private bool IsStuck(Aircraft aircraft)
  {
    float deltaTime = Time.deltaTime;
    this.stuckTimerSpeed = this.CheckStuckSpeed(aircraft, deltaTime) ? this.stuckTimerSpeed + deltaTime : 0.0f;
    this.stuckTimerYaw = this.CheckStuckYaw() ? this.stuckTimerYaw + deltaTime : 0.0f;
    if ((double) this.brakeUrgency > 0.10000000149011612)
    {
      this.stuckTimerSpeed -= 0.75f * deltaTime;
      this.stuckTimerYaw -= 0.75f * deltaTime;
      this.stuckTimerSpeed = Mathf.Max(this.stuckTimerSpeed, 0.0f);
      this.stuckTimerYaw = Mathf.Max(this.stuckTimerYaw, 0.0f);
    }
    return (double) this.stuckTimerSpeed > 30.0 || (double) this.stuckTimerYaw > 60.0;
  }

  private bool CheckStuckSpeed(Aircraft aircraft, float deltaTime)
  {
    return (double) aircraft.speed < 1.0 || (double) FastMath.SquareDistance(this.stuckRealPosition, aircraft.GlobalPosition()) / ((double) deltaTime * (double) deltaTime) < 2.25;
  }

  private bool CheckStuckYaw()
  {
    float num = Mathf.Sign(this.controlInputs.yaw);
    bool flag = (double) num == (double) this.stuckPreviousYawSign;
    this.stuckPreviousYawSign = num;
    return (((double) this.controlInputs.brake >= 0.800000011920929 ? 0 : ((double) Mathf.Abs(this.controlInputs.yaw) > 0.05000000074505806 ? 1 : 0)) & (flag ? 1 : 0)) != 0;
  }

  public override void UpdateState(Pilot pilot)
  {
  }
}

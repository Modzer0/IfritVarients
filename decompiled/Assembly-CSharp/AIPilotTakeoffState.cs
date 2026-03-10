// Decompiled with JetBrains decompiler
// Type: AIPilotTakeoffState
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using Unity.Profiling;
using UnityEngine;

#nullable disable
public class AIPilotTakeoffState : PilotBaseState
{
  private static readonly ProfilerMarker fixedUpdateStateMarker = new ProfilerMarker("AIPilotTakeoffState.FixedUpdateState");
  private Airbase airbase;
  private Airbase.Runway.RunwayUsage? runwayUsage;
  private Vector3 runwayVector;
  private AircraftInfo aircraftInfo;
  private AircraftParameters aircraftParameters;
  private float runwayLength;
  private float stuckTimer;
  private float startupTime;
  private GameObject aimPointVis;
  private float pitchAccumulate;
  private UnitPart mainPart;
  private bool startedTakeoffRun;
  private bool takenOff;

  public override void EnterState(Pilot pilot)
  {
    this.pilot = pilot;
    this.stateDisplayName = "taking off";
    if ((UnityEngine.Object) pilot.aircraft.NetworkHQ == (UnityEngine.Object) null)
      return;
    this.startedTakeoffRun = false;
    this.takenOff = false;
    this.aircraft = pilot.aircraft;
    this.mainPart = this.aircraft.gameObject.GetComponent<UnitPart>();
    this.mainPart.onApplyDamage += new Action<UnitPart.OnApplyDamage>(this.Takeoff_OnTakeDamage);
    this.airbase = pilot.aircraft.NetworkHQ.GetNearestAirbase(pilot.transform.position);
    if ((UnityEngine.Object) this.airbase == (UnityEngine.Object) null)
      return;
    this.runwayUsage = this.airbase.GetTakeoffRunway(this.aircraft, 0.0f);
    if (this.runwayUsage.HasValue)
    {
      Airbase.Runway runway = this.runwayUsage.Value.Runway;
      bool reverse = this.runwayUsage.Value.Reverse;
      this.runwayLength = runway.Length;
      runway.RegisterStartTakeoff(this.aircraft);
      this.runwayVector = runway.GetDirection(reverse);
      if (!reverse)
        runway.Start.transform.position.ToGlobalPosition();
      else
        runway.End.transform.position.ToGlobalPosition();
    }
    this.controlInputs = pilot.aircraft.GetInputs();
    this.aircraftInfo = pilot.aircraft.definition.aircraftInfo;
    this.aircraftParameters = pilot.aircraft.definition.aircraftParameters;
    this.aircraft.SetFlightAssist(true);
    if (!PlayerSettings.debugVis)
      return;
    this.aimPointVis = NetworkSceneSingleton<Spawner>.i.SpawnLocal(GameAssets.i.debugPoint, pilot.aircraft.transform);
  }

  private void Takeoff_OnTakeDamage(UnitPart.OnApplyDamage _)
  {
    this.aircraft.StartEjectionSequence();
    this.mainPart.onApplyDamage -= new Action<UnitPart.OnApplyDamage>(this.Takeoff_OnTakeDamage);
  }

  public override void LeaveState()
  {
    this.controlInputs.customAxis1 = 1f;
    this.mainPart.onApplyDamage -= new Action<UnitPart.OnApplyDamage>(this.Takeoff_OnTakeDamage);
  }

  public override void FixedUpdateState(Pilot pilot)
  {
    using (AIPilotTakeoffState.fixedUpdateStateMarker.Auto())
    {
      float magnitude = (pilot.aircraft.rb.velocity - NetworkSceneSingleton<LevelInfo>.i.GetWind(this.aircraft.transform.GlobalPosition())).magnitude;
      this.stuckTimer = (double) this.aircraft.speed < 1.0 ? this.stuckTimer + Time.deltaTime : 0.0f;
      this.controlInputs.customAxis1 = (double) magnitude > (double) this.aircraftParameters.takeoffSpeed * 0.699999988079071 ? 0.5f : 1f;
      if ((double) this.stuckTimer > 12.0 || (UnityEngine.Object) this.airbase == (UnityEngine.Object) null || (UnityEngine.Object) this.airbase.CurrentHQ != (UnityEngine.Object) this.aircraft.NetworkHQ)
      {
        pilot.aircraft.StartEjectionSequence();
        pilot.SwitchState((PilotBaseState) pilot.parkedState);
      }
      else
      {
        if ((double) this.aircraft.radarAlt > 1.0)
        {
          this.controlInputs.customAxis1 = 0.5f;
          if (!this.takenOff)
          {
            this.takenOff = true;
            ref Airbase.Runway.RunwayUsage? local = ref this.runwayUsage;
            if (local.HasValue)
              local.GetValueOrDefault().Runway.RegisterTakeoffLeftRunway(this.aircraft);
          }
          if ((double) this.aircraft.radarAlt > 75.0)
          {
            if (pilot.AICombatState == null)
              pilot.AICombatState = new AIPilotCombatModes(pilot.aircraft);
            this.controlInputs.customAxis1 = 1f;
            pilot.SwitchState((PilotBaseState) pilot.AICombatState);
            return;
          }
        }
        if (!this.runwayUsage.HasValue)
          return;
        Airbase.Runway runway = this.runwayUsage.Value.Runway;
        bool reverse = this.runwayUsage.Value.Reverse;
        Vector3 normalized = runway.GetDirection(reverse).normalized;
        Vector3 position = runway.GetNearestPoint(this.aircraft.transform.position, false) + normalized * (float) (50.0 + (double) this.aircraft.speed * 3.0);
        position.ToGlobalPosition();
        if ((UnityEngine.Object) this.aimPointVis != (UnityEngine.Object) null)
          this.aimPointVis.transform.position = position;
        bool flag1 = runway.AircraftOnRunway(this.aircraft);
        bool flag2 = (double) magnitude > (double) this.aircraftParameters.takeoffSpeed * 0.699999988079071 || !flag1;
        if (runway.SkiJump)
          flag2 = !flag1;
        GlobalPosition destination;
        if (!flag2)
        {
          Vector3 other = position - pilot.aircraft.transform.position;
          if ((double) pilot.aircraft.radarAlt > 3.0)
            other = this.runwayVector;
          double angleOnAxis = (double) TargetCalc.GetAngleOnAxis(pilot.aircraft.rb.velocity + pilot.aircraft.transform.forward * 20f, other, pilot.aircraft.transform.up);
          destination = runway.GetNearestPoint(this.aircraft.transform.position + this.aircraft.transform.forward * 50f, true).ToGlobalPosition();
          if ((double) Vector3.Dot(this.aircraft.transform.forward, normalized) < 0.0)
            destination += normalized.normalized * 100f;
          if ((double) Vector3.Dot(this.aircraft.transform.forward, other.normalized) > 0.949999988079071)
            this.startedTakeoffRun = true;
          if (this.startedTakeoffRun)
          {
            this.controlInputs.throttle = 1f;
            this.controlInputs.brake = 0.0f;
          }
          else
          {
            this.controlInputs.throttle = Mathf.Clamp01((float) (1.0 - (double) this.aircraft.speed * 0.10000000149011612));
            this.controlInputs.brake = Mathf.Clamp01(this.aircraft.speed * 0.1f);
          }
        }
        else
        {
          (reverse ? runway.End.transform.position + this.runwayVector * 5f - this.aircraft.transform.position : runway.Start.transform.position + this.runwayVector * 5f - this.aircraft.transform.position).y = this.runwayLength * 0.5f;
          this.controlInputs.throttle = 1f;
          this.controlInputs.brake = 0.0f;
          int num = (double) this.aircraft.radarAlt < 1.0 ? 50 : 50;
          destination = (this.aircraft.transform.position + normalized * 300f + Vector3.up * (float) num).ToGlobalPosition();
          if (runway.SkiJump)
          {
            Vector3 vector3 = this.aircraft.rb.velocity.normalized * 1000f;
            vector3.y = Mathf.Clamp(vector3.y, 50f, 150f);
            destination = this.aircraft.GlobalPosition() + vector3;
          }
        }
        this.aircraft.autopilot.AutoAim(destination, true, false, (double) this.aircraft.radarAlt < 3.0, 0.95f, 30f, false, 0.0f, Vector3.zero);
      }
    }
  }

  public override void UpdateState(Pilot pilot)
  {
  }
}

// Decompiled with JetBrains decompiler
// Type: AIHeloTakeoffState
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using Unity.Profiling;
using UnityEngine;

#nullable disable
public class AIHeloTakeoffState : PilotBaseState
{
  private static readonly ProfilerMarker fixedUpdateStateMarker = new ProfilerMarker("AIHeloTakeoffState.FixedUpdateState");
  private UnitPart mainPart;
  private RotorShaft rotorShaft;
  private float maxRPM;
  private PID collectivePID;
  private float spawnTime;
  private AutopilotHelo autopilotHelo;

  public override void EnterState(Pilot pilot)
  {
    this.spawnTime = 0.0f;
    this.stateDisplayName = "taking off";
    this.aircraft = pilot.aircraft;
    this.pilot = pilot;
    this.controlInputs = this.aircraft.GetInputs();
    this.mainPart = this.aircraft.gameObject.GetComponent<UnitPart>();
    this.mainPart.onApplyDamage += new Action<UnitPart.OnApplyDamage>(this.Takeoff_OnTakeDamage);
    this.collectivePID = new PID(0.5f, 0.02f, 0.1f);
    this.aircraft.SetFlightAssist(true);
    this.aircraft.GetControlsFilter().SetAutoHover(true);
    foreach (UnitPart unitPart in this.aircraft.partLookup)
    {
      if ((UnityEngine.Object) unitPart != (UnityEngine.Object) null && unitPart.gameObject.GetComponent<IEngine>() is RotorShaft component)
      {
        this.rotorShaft = component;
        this.maxRPM = component.GetMaxRPM();
        break;
      }
    }
  }

  public override void FixedUpdateState(Pilot pilot)
  {
    using (AIHeloTakeoffState.fixedUpdateStateMarker.Auto())
    {
      float num = 0.0f;
      if ((UnityEngine.Object) this.rotorShaft != (UnityEngine.Object) null)
        num = this.rotorShaft.GetRPM() - this.maxRPM * 0.9f;
      this.spawnTime += Time.deltaTime;
      this.controlInputs.brake = 1f;
      if ((double) this.spawnTime > 30.0)
        this.aircraft.StartEjectionSequence();
      else if ((double) this.spawnTime < 2.0 && (double) Mathf.Abs(this.aircraft.rb.velocity.y) > 1.0)
      {
        this.controlInputs.throttle = 0.0f;
      }
      else
      {
        if ((double) num < 0.0)
          this.controlInputs.throttle = 0.05f;
        else
          this.controlInputs.throttle = Mathf.Lerp(this.controlInputs.throttle, (float) (0.5 - (double) (this.aircraft.rb.velocity.y - 6f) * 0.10000000149011612), Time.fixedDeltaTime);
        if ((double) this.aircraft.radarAlt - 20.0 > 0.0)
        {
          pilot.SwitchState((PilotBaseState) pilot.AIHeloCombatState);
        }
        else
        {
          double y = (double) this.aircraft.rb.velocity.y;
          if ((double) this.aircraft.radarAlt <= 1.0)
            return;
          this.aircraft.GetControlsFilter().SetAutoHover(true);
          this.aircraft.SetFlightAssist(false);
          this.aircraft.autopilot.Hover(this.aircraft.GlobalPosition(), 50f, this.aircraft.transform.forward);
        }
      }
    }
  }

  public override void UpdateState(Pilot pilot)
  {
  }

  private void Takeoff_OnTakeDamage(UnitPart.OnApplyDamage _)
  {
    if (this.pilot.flightInfo.HasTakenOff)
      return;
    this.aircraft.StartEjectionSequence();
    this.mainPart.onApplyDamage -= new Action<UnitPart.OnApplyDamage>(this.Takeoff_OnTakeDamage);
  }

  public override void LeaveState()
  {
    this.mainPart.onApplyDamage -= new Action<UnitPart.OnApplyDamage>(this.Takeoff_OnTakeDamage);
    this.aircraft.GetControlsFilter().SetAutoHover(false);
    this.pilot.flightInfo.HasTakenOff = true;
  }
}

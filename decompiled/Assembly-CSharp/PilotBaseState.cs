// Decompiled with JetBrains decompiler
// Type: PilotBaseState
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

#nullable disable
public abstract class PilotBaseState
{
  public string stateDisplayName;
  protected Pilot pilot;
  protected Aircraft aircraft;
  protected ControlInputs controlInputs;
  protected ThreatVector threatVector;
  protected PilotBaseState.FuelChecker fuelChecker;
  protected FriendlyAircraftProximity friendlyAircraftProximity;
  protected MissileWarning missileWarningSystem;
  protected Missile evadingMissile;
  protected Airbase nearestAirbase;
  protected GlobalPosition destination;

  public void Initialize(Pilot pilot)
  {
    this.aircraft = pilot.aircraft;
    this.controlInputs = this.aircraft.GetInputs();
    this.missileWarningSystem = this.aircraft.GetMissileWarningSystem();
    this.fuelChecker = new PilotBaseState.FuelChecker(this.aircraft);
  }

  public abstract void EnterState(Pilot pilot);

  public abstract void UpdateState(Pilot pilot);

  public abstract void FixedUpdateState(Pilot pilot);

  protected void FindNearestAirbase()
  {
    this.nearestAirbase = this.aircraft.NetworkHQ.GetNearestAirbase(this.aircraft.transform.position, new RunwayQuery()
    {
      RunwayType = RunwayQueryType.Vertical,
      MinSize = this.aircraft.maxRadius
    });
  }

  public abstract void LeaveState();

  public string GetCurrentState() => this.stateDisplayName;

  protected class FuelChecker
  {
    private Aircraft aircraft;
    private bool enoughFuel;
    private float lastFuelCheck;

    public FuelChecker(Aircraft aircraft)
    {
      this.aircraft = aircraft;
      this.enoughFuel = true;
    }

    public bool HasEnoughFuel()
    {
      if ((double) Time.timeSinceLevelLoad - (double) this.lastFuelCheck > 5.0)
      {
        this.lastFuelCheck = Time.timeSinceLevelLoad;
        this.enoughFuel = (double) this.aircraft.GetFuelLevel() > 0.20000000298023224;
      }
      return this.enoughFuel;
    }
  }
}

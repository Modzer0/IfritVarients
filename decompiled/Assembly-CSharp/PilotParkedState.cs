// Decompiled with JetBrains decompiler
// Type: PilotParkedState
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

#nullable disable
public class PilotParkedState : PilotBaseState
{
  public override void EnterState(Pilot pilot)
  {
    this.stateDisplayName = "parked";
    this.controlInputs = pilot.aircraft.GetInputs();
    if ((double) pilot.aircraft.radarAlt >= 1.0)
      return;
    this.controlInputs.throttle = 0.0f;
    this.controlInputs.brake = 1f;
  }

  public override void LeaveState()
  {
  }

  public override void UpdateState(Pilot pilot)
  {
  }

  public override void FixedUpdateState(Pilot pilot)
  {
  }
}

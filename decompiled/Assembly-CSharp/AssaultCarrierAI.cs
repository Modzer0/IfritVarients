// Decompiled with JetBrains decompiler
// Type: AssaultCarrierAI
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

#nullable disable
public class AssaultCarrierAI : ShipAI
{
  [SerializeField]
  private UnitStorage unitStorage;
  private bool inDeployRange;

  protected override void Initialize()
  {
    if (!this.ship.LocalSim || GameManager.gameState == GameState.Editor || GameManager.gameState == GameState.Encyclopedia)
      return;
    base.Initialize();
  }

  private void ChooseDestination()
  {
    if ((double) Time.timeSinceLevelLoad - (double) this.lastDestinationSelected < 30.0)
      return;
    this.lastDestinationSelected = Time.timeSinceLevelLoad;
    MissionPosition.PositionResult result;
    if (this.ship.holdPosition || this.commandedDestination || (Object) this.ship.NetworkHQ == (Object) null || !MissionPosition.TryGetClosestObjectivePosition((Unit) this.ship, out result))
      return;
    GlobalPosition position = result.Position;
    if (!FastMath.InRange(this.destination, position, 1000f))
    {
      this.destination = position;
      this.pathfinder.Pathfind(NetworkSceneSingleton<LevelInfo>.i.seaLanes, position, false, this.keel);
      this.state = ShipAI.ShipAIState.attacking;
    }
    this.inDeployRange = FastMath.InRange(this.ship.GlobalPosition(), result.Position, this.standoffDistance + 500f);
  }

  private void DeployCargo()
  {
    if (this.inDeployRange)
    {
      this.unitStorage.DeployUnits();
      this.state = ShipAI.ShipAIState.unloading;
    }
    else
    {
      if (!this.unitStorage.HasFinishedDeploying())
        return;
      this.state = ShipAI.ShipAIState.navigating;
    }
  }

  protected override void Update()
  {
    if (!this.commandedDestination && !this.ship.holdPosition)
    {
      this.ChooseDestination();
      if (this.unitStorage.HasUnits())
        this.DeployCargo();
    }
    this.Steer();
  }
}

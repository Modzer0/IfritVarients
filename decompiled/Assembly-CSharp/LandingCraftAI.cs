// Decompiled with JetBrains decompiler
// Type: LandingCraftAI
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

#nullable disable
public class LandingCraftAI : ShipAI
{
  [SerializeField]
  private AirCushion airCushion;
  [SerializeField]
  private UnitStorage unitStorage;
  [SerializeField]
  private UnitStorage homeDock;
  private bool landed;
  private bool returningToResupply;
  private Vector3 shoreDirection;
  private float launchTime;
  private float unloadingTime;

  protected override void Awake()
  {
    base.Awake();
    this.ship.OnLaunch += new Action(this.LandingCraftAI_OnLaunch);
  }

  protected override void Initialize()
  {
    if (GameManager.gameState == GameState.Editor || GameManager.gameState == GameState.Encyclopedia)
      return;
    base.Initialize();
    this.IdentifyHomeDock().Forget();
  }

  private void LandingCraftAI_OnLaunch()
  {
    this.launchTime = Time.timeSinceLevelLoad;
    this.ship.OnLaunch -= new Action(this.LandingCraftAI_OnLaunch);
    this.state = ShipAI.ShipAIState.launching;
  }

  private async UniTask IdentifyHomeDock()
  {
    LandingCraftAI landingCraftAi = this;
    CancellationToken cancel = landingCraftAi.destroyCancellationToken;
    await UniTask.Delay(5000);
    if (cancel.IsCancellationRequested)
      cancel = new CancellationToken();
    else if (landingCraftAi.ship.disabled)
    {
      cancel = new CancellationToken();
    }
    else
    {
      float nearestDistance;
      if (!landingCraftAi.ship.NetworkHQ.TryGetNearestUnitStorage((Unit) landingCraftAi.ship, false, out landingCraftAi.homeDock, out nearestDistance))
      {
        cancel = new CancellationToken();
      }
      else
      {
        Debug.Log((object) $"[LandingCraftAI] found homeDock attached to {landingCraftAi.homeDock.GetUnit().unitName} at distance of {Mathf.Sqrt(nearestDistance)}");
        cancel = new CancellationToken();
      }
    }
  }

  protected override void ChooseTarget()
  {
    if ((double) Time.timeSinceLevelLoad - (double) this.lastDestinationSelected < 30.0)
      return;
    this.lastDestinationSelected = Time.timeSinceLevelLoad;
    MissionPosition.PositionResult result;
    if (this.ship.holdPosition || this.commandedDestination || this.returningToResupply || (UnityEngine.Object) this.ship.NetworkHQ == (UnityEngine.Object) null || !MissionPosition.TryGetClosestObjectivePosition((Unit) this.ship, out result))
      return;
    GlobalPosition position = result.Position;
    float num = FastMath.Distance(this.ship.GlobalPosition(), this.lastDestination);
    if ((double) FastMath.Distance(this.lastDestination, position) <= (double) num * 0.20000000298023224)
      return;
    this.lastDestination = position;
    this.SetDestination(position);
  }

  protected override void SetDestination(GlobalPosition destination)
  {
    this.destination = destination;
    RaycastHit hitInfo1;
    int num1 = !Physics.Linecast(destination.ToLocalPosition() + Vector3.up * 5000f, destination.ToLocalPosition() - Vector3.up * 5000f, out hitInfo1, 64 /*0x40*/) ? 0 : ((double) hitInfo1.point.y > (double) Datum.LocalSeaY ? 1 : 0);
    double num2 = (double) FastMath.Distance(this.ship.GlobalPosition(), destination);
    if (num1 != 0)
    {
      this.state = ShipAI.ShipAIState.landing;
      this.avoidShore = false;
      GlobalPosition nearestPoint1 = this.ship.GlobalPosition();
      NetworkSceneSingleton<LevelInfo>.i.seaLanes.TryGetNearestPoint(destination, out nearestPoint1);
      GlobalPosition nearestPoint2;
      if (NetworkSceneSingleton<LevelInfo>.i.roadNetwork.TryGetNearestPoint(nearestPoint1, out nearestPoint2))
      {
        destination = nearestPoint2;
        if (PlayerSettings.debugVis && (UnityEngine.Object) SceneSingleton<CameraStateManager>.i.followingUnit == (UnityEngine.Object) this.ship)
        {
          GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(GameAssets.i.debugArrowFlat, Datum.origin);
          gameObject.transform.position = nearestPoint1.ToLocalPosition();
          Vector3 forward = nearestPoint2 - nearestPoint1;
          gameObject.transform.rotation = Quaternion.LookRotation(forward, Vector3.up);
          gameObject.transform.localScale = new Vector3(this.ship.definition.width, 1f, forward.magnitude);
          gameObject.transform.position += Vector3.up * 1f;
          UnityEngine.Object.Destroy((UnityEngine.Object) gameObject, 10f);
        }
      }
      nearestPoint1.y = 1f;
      destination.y = 1f;
      this.shoreDirection = destination - nearestPoint1;
      RaycastHit hitInfo2;
      if (Physics.Linecast(nearestPoint1.ToLocalPosition(), destination.ToLocalPosition(), out hitInfo2, 64 /*0x40*/))
      {
        destination = hitInfo2.point.ToGlobalPosition();
        destination.y = 0.0f;
      }
    }
    else
    {
      this.avoidShore = true;
      if (this.state != ShipAI.ShipAIState.returning)
        this.state = ShipAI.ShipAIState.navigating;
    }
    this.destination = destination;
    this.pathfinder.Pathfind(NetworkSceneSingleton<LevelInfo>.i.seaLanes, destination, false, this.keel);
  }

  protected override void Steer()
  {
    if ((double) Time.timeSinceLevelLoad - (double) this.lastSteeringUpdate < 0.20000000298023224)
      return;
    this.lastSteeringUpdate = Time.timeSinceLevelLoad;
    SteeringInfo? shipSteerpoint = this.pathfinder.GetShipSteerpoint(this.ship.GlobalPosition(), this.transform.forward, this.ship.speed, false, this.keel);
    float num1 = FastMath.Distance(this.ship.GlobalPosition(), this.destination);
    this.inputs.throttle = 0.0f;
    this.inputs.steering = 0.0f;
    this.avoidShips = true;
    if (this.state == ShipAI.ShipAIState.docking)
      this.avoidShips = false;
    float num2 = 1f;
    Vector3 forward = shipSteerpoint.HasValue ? shipSteerpoint.GetValueOrDefault().steerVector.normalized : this.ship.transform.forward;
    if ((double) num1 < (double) this.ship.maxRadius + 100.0)
    {
      this.StartHoldPosition();
      this.inputs.throttle = Mathf.Clamp(Vector3.Dot(this.ship.rb.velocity * 0.1f, -this.ship.transform.forward), -1f, 1f);
    }
    else
    {
      if (this.state == ShipAI.ShipAIState.landing && (double) num1 < 300.0)
      {
        forward = this.shoreDirection.normalized;
        num2 = (double) this.ship.speed > 20.0 ? 0.0f : 0.5f;
        if ((UnityEngine.Object) this.airCushion != (UnityEngine.Object) null && this.airCushion.Landed())
        {
          this.airCushion.Deflate();
          this.WaitDeployUnits().Forget();
          this.state = ShipAI.ShipAIState.unloading;
          return;
        }
      }
      if (this.state == ShipAI.ShipAIState.returning && (UnityEngine.Object) this.airCushion != (UnityEngine.Object) null && this.airCushion.Landed())
        forward = -this.shoreDirection.normalized;
      Vector3 zero = Vector3.zero;
      if (this.state == ShipAI.ShipAIState.unloading)
      {
        forward = this.shoreDirection.normalized;
        num2 = 0.0f;
      }
      foreach (Obstacle obstacle in this.obstacles)
      {
        if (!((UnityEngine.Object) obstacle.Transform == (UnityEngine.Object) null))
        {
          float num3 = (float) ((double) this.ship.maxRadius + (double) obstacle.Radius + 50.0);
          if (FastMath.InRange(obstacle.Transform.position, this.ship.transform.position, num3 * 8f))
          {
            Vector3 rhs = obstacle.Transform.position - this.ship.transform.position;
            double num4 = (double) Vector3.Dot(rhs - this.ship.rb.velocity * 5f, rhs);
            float num5 = (float) (1.0 / ((double) rhs.sqrMagnitude / ((double) num3 * (double) num3)));
            zero -= rhs.normalized * num5;
          }
        }
      }
      if (PlayerSettings.debugVis)
      {
        GameObject gameObject1 = UnityEngine.Object.Instantiate<GameObject>(GameAssets.i.debugArrowGreen, this.ship.transform);
        gameObject1.transform.position = this.ship.transform.position + Vector3.up * 3f;
        gameObject1.transform.rotation = Quaternion.LookRotation(forward);
        gameObject1.transform.localScale = new Vector3(5f, 5f, forward.magnitude * 30f);
        UnityEngine.Object.Destroy((UnityEngine.Object) gameObject1, 0.2f);
        GameObject gameObject2 = UnityEngine.Object.Instantiate<GameObject>(GameAssets.i.debugArrowFlat, this.ship.transform);
        Vector3 vector3 = this.destination - this.ship.transform.GlobalPosition();
        gameObject2.transform.position = this.ship.transform.position + Vector3.up * 3f;
        gameObject2.transform.rotation = Quaternion.LookRotation(forward);
        gameObject2.transform.localScale = new Vector3(5f, 5f, vector3.magnitude);
        UnityEngine.Object.Destroy((UnityEngine.Object) gameObject2, 0.2f);
        if (zero != Vector3.zero)
        {
          GameObject gameObject3 = UnityEngine.Object.Instantiate<GameObject>(GameAssets.i.debugArrow, this.ship.transform);
          gameObject3.transform.position = gameObject1.transform.position + gameObject1.transform.forward * gameObject1.transform.localScale.z;
          gameObject3.transform.rotation = Quaternion.LookRotation(zero);
          gameObject3.transform.localScale = new Vector3(5f, 5f, zero.magnitude * 30f);
          UnityEngine.Object.Destroy((UnityEngine.Object) gameObject3, 0.2f);
        }
      }
      Vector3 normalized = (zero + forward).normalized;
      if (this.state == ShipAI.ShipAIState.holding)
        return;
      this.inputs.throttle = num2 * Mathf.Clamp(Vector3.Dot(normalized, this.ship.transform.forward), -1f, 1f);
      this.inputs.steering = Mathf.Clamp(TargetCalc.GetAngleOnAxis(this.ship.transform.forward, normalized, this.ship.transform.up) * -0.1f, -1f, 1f);
    }
  }

  private void ReturnToShip()
  {
    if ((double) Time.timeSinceLevelLoad - (double) this.lastDestinationSelected < 10.0)
      return;
    this.lastDestinationSelected = Time.timeSinceLevelLoad;
    if ((UnityEngine.Object) this.homeDock == (UnityEngine.Object) null && !this.ship.NetworkHQ.TryGetNearestUnitStorage((Unit) this.ship, false, out this.homeDock, out float _))
    {
      Debug.Log((object) "[LandingCraftAI] Couldn't find ship to return to");
      this.state = ShipAI.ShipAIState.holding;
    }
    else
    {
      this.airCushion.Inflate();
      Debug.Log((object) ("[LandingCraftAI] Returning to home dock " + this.homeDock.GetUnit().unitName));
      this.destination = this.homeDock.GetUnit().GlobalPosition();
      this.pathfinder.Pathfind(NetworkSceneSingleton<LevelInfo>.i.seaLanes, this.destination, false, this.keel);
      if (!FastMath.InRange(this.destination, this.ship.GlobalPosition(), 1000f))
        return;
      this.homeDock.RegisterIncoming((Unit) this.ship);
      this.state = ShipAI.ShipAIState.docking;
    }
  }

  private void Dock()
  {
    if ((double) Time.timeSinceLevelLoad - (double) this.lastSteeringUpdate < 0.20000000298023224)
      return;
    if ((UnityEngine.Object) this.homeDock == (UnityEngine.Object) null && this.homeDock.GetUnit().disabled)
    {
      this.state = ShipAI.ShipAIState.holding;
      this.homeDock = (UnitStorage) null;
    }
    else
    {
      Vector3 other = this.homeDock.GetApproachTarget((Unit) this.ship) - this.ship.GlobalPosition();
      this.inputs.steering = Mathf.Clamp(TargetCalc.GetAngleOnAxis(this.ship.transform.forward, other, this.ship.transform.up) * -0.1f, -1f, 1f);
      this.inputs.throttle = Vector3.Dot(this.ship.transform.forward, other.normalized);
      if (FastMath.InRange(this.homeDock.GetDoorTransform().GlobalPosition(), this.ship.GlobalPosition(), 150f))
      {
        this.homeDock.OpenDoors();
        this.inputs.throttle *= 0.5f;
      }
      if ((double) other.sqrMagnitude >= (double) this.ship.maxRadius * (double) this.ship.maxRadius && (double) this.ship.speed >= 1.0)
        return;
      this.homeDock.Store((Unit) this.ship);
      this.homeDock.Transfer(this.unitStorage);
      this.state = ShipAI.ShipAIState.docked;
    }
  }

  private void Launch()
  {
    if ((double) Time.timeSinceLevelLoad - (double) this.lastSteeringUpdate < 0.20000000298023224)
      return;
    this.inputs.steering = 0.0f;
    this.inputs.throttle = 0.5f;
    if ((double) Time.timeSinceLevelLoad - (double) this.launchTime <= 10.0)
      return;
    this.state = ShipAI.ShipAIState.navigating;
  }

  private void NavigateToLand()
  {
  }

  private async UniTask WaitDeployUnits()
  {
    LandingCraftAI landingCraftAi = this;
    CancellationToken cancel = landingCraftAi.destroyCancellationToken;
    while ((double) landingCraftAi.ship.speed > 1.0)
    {
      await UniTask.Delay(1000);
      if (cancel.IsCancellationRequested)
      {
        cancel = new CancellationToken();
        return;
      }
      if (landingCraftAi.ship.disabled)
      {
        cancel = new CancellationToken();
        return;
      }
    }
    landingCraftAi.homeDock = (UnitStorage) null;
    landingCraftAi.unitStorage.DeployUnits();
    while (!landingCraftAi.unitStorage.HasFinishedDeploying())
    {
      await UniTask.Delay(1000);
      if (cancel.IsCancellationRequested)
      {
        cancel = new CancellationToken();
        return;
      }
      if (landingCraftAi.ship.disabled)
      {
        cancel = new CancellationToken();
        return;
      }
    }
    while (((UnityEngine.Object) landingCraftAi.homeDock == (UnityEngine.Object) null || !landingCraftAi.homeDock.HasUnits()) && !landingCraftAi.ship.NetworkHQ.TryGetNearestUnitStorage((Unit) landingCraftAi.ship, true, out landingCraftAi.homeDock, out float _))
    {
      await UniTask.Delay(10000);
      if (cancel.IsCancellationRequested)
      {
        cancel = new CancellationToken();
        return;
      }
      if (landingCraftAi.ship.disabled)
      {
        cancel = new CancellationToken();
        return;
      }
    }
    await UniTask.Delay(2000);
    if (cancel.IsCancellationRequested)
      cancel = new CancellationToken();
    else if (landingCraftAi.ship.disabled)
    {
      cancel = new CancellationToken();
    }
    else
    {
      landingCraftAi.state = ShipAI.ShipAIState.returning;
      landingCraftAi.lastDestinationSelected = -100f;
      cancel = new CancellationToken();
    }
  }

  protected override void Update()
  {
    if (this.state == ShipAI.ShipAIState.docked)
      return;
    if (this.state == ShipAI.ShipAIState.docking)
      this.Dock();
    else if (this.state == ShipAI.ShipAIState.unloading)
    {
      this.unloadingTime += Time.deltaTime;
      if ((double) this.unloadingTime > 60.0)
      {
        this.state = ShipAI.ShipAIState.returning;
        this.lastDestinationSelected = -100f;
        this.unloadingTime = 0.0f;
      }
      this.Steer();
    }
    else
    {
      this.unloadingTime = 0.0f;
      if (this.state == ShipAI.ShipAIState.launching)
        this.Launch();
      else if (this.state == ShipAI.ShipAIState.returning)
      {
        this.ReturnToShip();
        this.Steer();
      }
      else
      {
        if (!this.commandedDestination && !this.ship.holdPosition)
          this.ChooseTarget();
        this.Steer();
      }
    }
  }
}

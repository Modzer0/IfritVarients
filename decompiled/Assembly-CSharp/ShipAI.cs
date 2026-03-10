// Decompiled with JetBrains decompiler
// Type: ShipAI
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using Cysharp.Threading.Tasks;
using NuclearOption.Networking;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

#nullable disable
public class ShipAI : MonoBehaviour
{
  public ShipAI.ShipAIState state;
  protected float lastDestinationSelected;
  protected GlobalPosition lastDestination;
  protected PathfindingAgent pathfinder;
  protected List<GlobalPosition> waypoints = new List<GlobalPosition>();
  protected bool commandedDestination;
  [SerializeField]
  protected Ship ship;
  [SerializeField]
  protected float standoffDistance;
  protected ShipInputs inputs;
  protected float lastTargetAssessTime;
  protected float currentTargetPriority;
  protected float lastSteeringUpdate;
  protected Unit currentTarget;
  protected GlobalPosition destination;
  protected List<Unit> unitsInObstacleRange = new List<Unit>();
  protected List<Obstacle> obstacles = new List<Obstacle>();
  protected Obstacle shoreObstacle;
  protected bool avoidShips;
  protected bool avoidShore;
  protected Transform avoidTransform;
  protected float holdTime = 120f;
  [SerializeField]
  protected Transform keel;

  protected virtual void Awake()
  {
    if (!NetworkManagerNuclearOption.i.Server.Active || GameManager.gameState == GameState.Encyclopedia)
    {
      this.enabled = false;
    }
    else
    {
      this.state = ShipAI.ShipAIState.holding;
      this.avoidShips = true;
      this.avoidShore = true;
      this.pathfinder = new PathfindingAgent((Unit) this.ship);
      Transform transform = new GameObject("shoreObstacle").transform;
      transform.SetParent(Datum.origin);
      transform.position = Vector3.forward * 200000f;
      this.shoreObstacle = new Obstacle(transform, 400f, float.MaxValue);
      this.inputs = this.ship.GetInputs();
      this.ship.onInitialize += new Action(this.Initialize);
      this.ship.UnitCommand.ProcessSetDestination += new UnitCommand.ProcessCommand(this.ShipAI_ProcessSetDestination);
      this.lastDestinationSelected = Time.timeSinceLevelLoad - 25f;
      this.StartSlowUpdateDelayed(5f, new Action(this.CheckObstacles));
    }
  }

  protected virtual void Initialize()
  {
    this.destination = this.ship.GlobalPosition();
    this.lastDestination = this.destination;
    this.ship.onInitialize -= new Action(this.Initialize);
  }

  protected virtual void ChooseTarget()
  {
    if ((double) Time.timeSinceLevelLoad - (double) this.lastDestinationSelected < 30.0)
      return;
    this.lastDestinationSelected = Time.timeSinceLevelLoad;
    if (this.ship.holdPosition || this.commandedDestination || (UnityEngine.Object) this.ship.NetworkHQ == (UnityEngine.Object) null)
      return;
    this.AssessHQTargets();
    GlobalPosition globalPosition = this.destination;
    float num = float.MaxValue;
    bool flag = false;
    GlobalPosition knownPosition;
    if ((UnityEngine.Object) this.currentTarget != (UnityEngine.Object) null && this.ship.NetworkHQ.TryGetKnownPosition(this.currentTarget, out knownPosition))
    {
      globalPosition = knownPosition;
      num = FastMath.Distance(this.ship.GlobalPosition(), globalPosition);
    }
    MissionPosition.PositionResult result;
    if (MissionPosition.TryGetClosestObjectivePosition((Unit) this.ship, out result) && (double) FastMath.Distance(this.ship.GlobalPosition(), result.Position) < (double) num - (double) this.standoffDistance)
    {
      globalPosition = result.Position;
      flag = true;
    }
    if (FastMath.InRange(this.destination, globalPosition, 1000f))
      return;
    this.SetDestination(globalPosition);
    this.state = flag ? ShipAI.ShipAIState.navigating : ShipAI.ShipAIState.attacking;
  }

  public void ArriveAtCommandedDestination()
  {
    this.state = ShipAI.ShipAIState.holding;
    this.ship.holdPosition = true;
    this.WaitHoldPosition(this.holdTime).Forget();
  }

  protected void StartHoldPosition() => this.WaitHoldPosition(this.holdTime).Forget();

  protected async UniTask WaitHoldPosition(float holdTime)
  {
    ShipAI shipAi = this;
    CancellationToken cancel = shipAi.destroyCancellationToken;
    shipAi.state = ShipAI.ShipAIState.holding;
    await UniTask.Delay((int) ((double) holdTime * 1000.0));
    if (cancel.IsCancellationRequested)
    {
      cancel = new CancellationToken();
    }
    else
    {
      shipAi.commandedDestination = false;
      cancel = new CancellationToken();
    }
  }

  protected void HoldPosition()
  {
    this.inputs.throttle = Vector3.Dot(this.ship.rb.velocity.normalized, -this.ship.transform.forward);
    this.inputs.steering = 0.0f;
  }

  protected void ShipAI_ProcessSetDestination(ref UnitCommand.Command command)
  {
    this.commandedDestination = true;
    this.state = ShipAI.ShipAIState.navigating;
    this.SetDestination(command.position);
  }

  protected virtual void SetDestination(GlobalPosition newDestination)
  {
    this.lastDestination = this.destination;
    this.pathfinder.Pathfind(NetworkSceneSingleton<LevelInfo>.i.seaLanes, newDestination, false, this.keel);
    this.destination = newDestination;
    this.lastDestinationSelected = Time.timeSinceLevelLoad;
  }

  protected void CheckObstacles()
  {
    if ((double) this.ship.speed <= 1.0)
      return;
    this.UpdateObstacles();
  }

  protected void UpdateObstacles()
  {
    this.obstacles.Clear();
    if (!BattlefieldGrid.TryGetGridSquare(this.transform.GlobalPosition(), out GridSquare _))
      return;
    if (this.avoidShips)
    {
      foreach (Unit unitsInRange in BattlefieldGrid.GetUnitsInRangeEnumerable(this.ship.GlobalPosition(), 1000f))
      {
        if ((UnityEngine.Object) unitsInRange != (UnityEngine.Object) null && (UnityEngine.Object) unitsInRange != (UnityEngine.Object) this.ship && (double) unitsInRange.radarAlt < 10.0 && ((UnityEngine.Object) unitsInRange.rb == (UnityEngine.Object) null || (double) unitsInRange.rb.mass > (double) this.ship.rb.mass * 0.5))
          this.obstacles.Add(new Obstacle(unitsInRange.transform, unitsInRange.maxRadius, float.MaxValue));
      }
    }
    Vector3 forward = this.ship.transform.forward with
    {
      y = 0.0f
    };
    Vector3 velocity = this.ship.rb.velocity with
    {
      y = 0.0f
    };
    if (!this.avoidShore)
      return;
    RaycastHit hitInfo;
    if (Physics.Linecast(this.keel.position, this.keel.position + forward * 400f + velocity * 5f, out hitInfo, 64 /*0x40*/))
    {
      GlobalPosition fromPosition = this.ship.GlobalPosition();
      GlobalPosition nearestPoint1;
      NetworkSceneSingleton<LevelInfo>.i.roadNetwork.TryGetNearestPoint(fromPosition, out nearestPoint1);
      GlobalPosition nearestPoint2;
      NetworkSceneSingleton<LevelInfo>.i.seaLanes.TryGetNearestPoint(fromPosition, out nearestPoint2);
      Vector3 normalized = Vector3.Lerp(nearestPoint2 - fromPosition, -(nearestPoint1 - fromPosition), 0.5f).normalized with
      {
        y = 0.0f
      };
      this.shoreObstacle.Transform.position = hitInfo.point - normalized * hitInfo.distance * 0.5f;
      if (PlayerSettings.debugVis && (UnityEngine.Object) SceneSingleton<CameraStateManager>.i.followingUnit == (UnityEngine.Object) this.ship)
      {
        GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(GameAssets.i.debugArrowFlat, Datum.origin);
        gameObject.transform.position = hitInfo.point + Vector3.up * 10f;
        gameObject.transform.rotation = Quaternion.LookRotation(normalized, Vector3.up);
        gameObject.transform.localScale = new Vector3(this.ship.definition.width, 1f, 200f);
        UnityEngine.Object.Destroy((UnityEngine.Object) gameObject, 10f);
      }
    }
    this.obstacles.Add(this.shoreObstacle);
  }

  protected virtual void Steer()
  {
    if ((double) Time.timeSinceLevelLoad - (double) this.lastSteeringUpdate < 0.20000000298023224)
      return;
    this.lastSteeringUpdate = Time.timeSinceLevelLoad;
    this.inputs.throttle = 0.0f;
    this.inputs.steering = 0.0f;
    SteeringInfo? shipSteerpoint = this.pathfinder.GetShipSteerpoint(this.ship.GlobalPosition(), this.transform.forward, this.ship.speed, false, this.keel);
    float num1 = FastMath.Distance(this.ship.GlobalPosition(), this.destination);
    if (this.state == ShipAI.ShipAIState.attacking && (double) num1 < (double) this.standoffDistance)
      this.StartHoldPosition();
    else if ((double) num1 < (double) this.ship.maxRadius + 100.0)
    {
      this.StartHoldPosition();
      this.inputs.throttle = Mathf.Clamp(Vector3.Dot(this.ship.rb.velocity * 0.1f, -this.ship.transform.forward), -1f, 1f);
    }
    else
    {
      Vector3 vector3_1 = shipSteerpoint.HasValue ? shipSteerpoint.GetValueOrDefault().steerVector.normalized : this.ship.transform.forward;
      Vector3 zero = Vector3.zero;
      foreach (Obstacle obstacle in this.obstacles)
      {
        if (!((UnityEngine.Object) obstacle.Transform == (UnityEngine.Object) null))
        {
          float num2 = (float) ((double) this.ship.maxRadius + (double) obstacle.Radius + 50.0);
          if (FastMath.InRange(obstacle.Transform.position, this.ship.transform.position, num2 * 8f))
          {
            Vector3 rhs = obstacle.Transform.position - this.ship.transform.position;
            double num3 = (double) Vector3.Dot(rhs - this.ship.rb.velocity * 5f, rhs);
            float num4 = (float) (1.0 / ((double) rhs.sqrMagnitude / ((double) num2 * (double) num2)));
            zero -= rhs.normalized * num4;
          }
        }
      }
      Vector3 normalized = (zero + vector3_1).normalized;
      if (PlayerSettings.debugVis)
      {
        GameObject gameObject1 = UnityEngine.Object.Instantiate<GameObject>(GameAssets.i.debugArrowGreen, this.ship.transform);
        gameObject1.transform.position = this.ship.transform.position + Vector3.up * 3f;
        gameObject1.transform.rotation = Quaternion.LookRotation(normalized);
        gameObject1.transform.localScale = new Vector3(5f, 5f, normalized.magnitude * 30f);
        UnityEngine.Object.Destroy((UnityEngine.Object) gameObject1, 0.2f);
        GameObject gameObject2 = UnityEngine.Object.Instantiate<GameObject>(GameAssets.i.debugArrowFlat, this.ship.transform);
        Vector3 vector3_2 = this.destination - this.ship.transform.GlobalPosition();
        gameObject2.transform.position = this.ship.transform.position + Vector3.up * 3f;
        gameObject2.transform.rotation = Quaternion.LookRotation(normalized);
        gameObject2.transform.localScale = new Vector3(5f, 5f, vector3_2.magnitude);
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
      if (this.state == ShipAI.ShipAIState.holding)
        return;
      this.inputs.throttle = Vector3.Dot(normalized, this.ship.transform.forward);
      if ((double) this.inputs.throttle < 0.0 && (double) Vector3.Dot(this.ship.transform.forward, this.ship.rb.velocity) < 0.0)
        this.inputs.throttle = 0.0f;
      this.inputs.steering = Mathf.Clamp(TargetCalc.GetAngleOnAxis(this.ship.transform.forward, normalized, this.ship.transform.up) * -0.1f, -1f, 1f);
    }
  }

  protected void AssessHQTargets()
  {
    this.currentTargetPriority = 0.0f;
    this.currentTarget = (Unit) null;
    foreach (KeyValuePair<PersistentID, TrackingInfo> keyValuePair in this.ship.NetworkHQ.trackingDatabase)
    {
      TrackingInfo trackingInfo = keyValuePair.Value;
      Unit unit;
      if (trackingInfo.TryGetUnit(out unit) && !unit.disabled && !((UnityEngine.Object) unit.NetworkHQ == (UnityEngine.Object) null) && !((UnityEngine.Object) unit.NetworkHQ == (UnityEngine.Object) this.ship.NetworkHQ) && (double) unit.radarAlt <= 10.0 && (double) unit.speed <= 100.0)
      {
        float num1 = FastMath.SquareDistance(this.ship.GlobalPosition(), trackingInfo.GetPosition());
        foreach (WeaponStation weaponStation in this.ship.weaponStations)
        {
          if (weaponStation.Ammo > 0)
          {
            OpportunityThreat opportunityThreat = CombatAI.AnalyzeTarget(weaponStation, (Unit) this.ship, trackingInfo, mobile: true);
            float num2 = (opportunityThreat.opportunity + opportunityThreat.threat) / num1 * (weaponStation.WeaponInfo.overHorizon ? 100f : 1f);
            if ((double) num2 > (double) this.currentTargetPriority)
            {
              this.currentTarget = unit;
              this.currentTargetPriority = num2;
            }
          }
        }
      }
    }
  }

  protected void OnDestroy()
  {
    if (!((UnityEngine.Object) this.avoidTransform != (UnityEngine.Object) null))
      return;
    UnityEngine.Object.Destroy((UnityEngine.Object) this.avoidTransform.gameObject);
  }

  protected virtual void Update()
  {
    if (!this.commandedDestination && !this.ship.holdPosition)
      this.ChooseTarget();
    this.Steer();
  }

  public enum ShipAIState
  {
    launching,
    holding,
    navigating,
    attacking,
    landing,
    unloading,
    returning,
    docking,
    docked,
  }
}
